using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace fNbt.Serialization.Compiled {
    internal class SerializeCodeEmitter : CodeEmitter {
        // NbtCompound.Add(NbtTag)
        static readonly MethodInfo NbtCompoundAddMethod =
            typeof(NbtCompound).GetMethod("Add", new[] { typeof(NbtTag) });

        // NbtList.Add(NbtTag)
        static readonly MethodInfo NbtListAddMethod =
            typeof(NbtList).GetMethod("Add", new[] { typeof(NbtTag) });

        // new NbtCompound(string)
        static readonly ConstructorInfo NbtCompoundCtor =
            typeof(NbtCompound).GetConstructor(new[] { typeof(string) });

        // NbtFile.RootTag
        static readonly PropertyInfo NbtFileRootTagProperty = typeof(NbtFile).GetProperty("RootTag");


        readonly ParameterExpression varRootTag;
        readonly ParameterExpression argTagName;
        readonly ParameterExpression argValue;
        readonly NbtCompiler.CallResolver callResolver;


        public SerializeCodeEmitter([NotNull] ParameterExpression argTagName, [NotNull] ParameterExpression argValue,
                                    [NotNull] NbtCompiler.CallResolver callResolver) {
            if (argTagName == null) throw new ArgumentNullException("argTagName");
            if (argValue == null) throw new ArgumentNullException("argValue");
            if (callResolver == null) throw new ArgumentNullException("callResolver");
            varRootTag = Expression.Parameter(typeof(NbtCompound), "rootTag");
            this.argTagName = argTagName;
            this.argValue = argValue;
            this.callResolver = callResolver;
        }


        #region CodeEmitter implementation

        public override ParameterExpression ReturnValue {
            get { return varRootTag; }
        }

        public override Expression GetPreamble() {
            // varRootTag = new NbtCompound(argTagName);
            return Expression.Assign(varRootTag, Expression.New(NbtCompoundCtor, argTagName));
        }


        public override Expression HandlePrimitiveOrEnum(string tagName, PropertyInfo property) {
            // Find a mapping from PropertyType to closest NBT equivalent
            Type convertedType = SerializationUtil.GetConvertedType(property.PropertyType);

            // property getter
            Expression getPropertyExpr = Expression.MakeMemberAccess(argValue, property);

            // create a new instance of the appropriate tag
            Expression newTagExpr = MakeNbtTagCtor(convertedType,
                                                   Expression.Constant(tagName, typeof(string)),
                                                   getPropertyExpr);

            return Expression.Call(varRootTag, NbtCompoundAddMethod, newTagExpr);
        }


        // Generates an expression that creates an NbtTag for given property of a directly-mappable types.
        // Directly-mappable types are: primitives, enums, byte[], int[], and string.
        // HandlePrimitiveOrEnum is actually more efficient (and preferred by NbtCompiler) for primitives and enums,
        // because it skips boxing and NullPolicy checks. This one is pretty much only used for byte[]/int[]/string.
        public override Expression HandleDirectlyMappedType(string tagName, PropertyInfo property, NullPolicy selfPolicy) {
            // declare a local var, which will hold the property's value
            ParameterExpression varValue = Expression.Parameter(property.PropertyType);

            // Fallback path, in case value is null and NullPolicy is InsertDefaults
            Expression defaultVal = Expression.Constant(SerializationUtil.GetDefaultValue(property.PropertyType));
            // varRootTag.Add( new NbtTag(tagName, <defaultVal>) );
            Expression defaultValExpr =
                Expression.Call(varRootTag,
                                NbtCompoundAddMethod,
                                MakeNbtTagCtor(property.PropertyType,
                                               Expression.Constant(tagName, typeof(string)),
                                               defaultVal));

            // varRootTag.Add( new NbtTag(tagName, <varValue>) );
            Expression makeTagExpr =
                Expression.Call(varRootTag,
                                NbtCompoundAddMethod,
                                MakeNbtTagCtor(property.PropertyType,
                                               Expression.Constant(tagName, typeof(string)),
                                               varValue));

            // Getter for the property value
            Expression getPropertyExpr = Expression.MakeMemberAccess(argValue, property);

            // generate the appropriate enclosing expressions, depending on NullPolicy
            return NbtCompiler.MakeNullHandler(varValue, getPropertyExpr, selfPolicy,
                                               makeTagExpr, defaultValExpr, MakePropertyNullMessage(property));
        }


        public override Expression HandleINbtSerializable(string tagName, PropertyInfo property, NullPolicy selfPolicy) {
            MethodInfo serializeMethod = property.PropertyType.GetMethod("Serialize", new[] { typeof(string) });
            ParameterExpression varValue = Expression.Parameter(property.PropertyType, "value");
            string nullMsg = MakePropertyNullMessage(property);

            // rootTag.Add( value.Serialize() )
            Expression serializeExpr = Expression.Call(
                varRootTag, NbtCompoundAddMethod,
                Expression.Call(varValue, serializeMethod, Expression.Constant(tagName)));
            
            // Fallback path, in case value is null and NullPolicy is InsertDefaults
            Expression defaultExpr = Expression.New(NbtCompoundCtor, Expression.Constant(tagName));

            // Getter for the property value
            Expression propValue = Expression.MakeMemberAccess(argValue, property);

            return NbtCompiler.MakeNullHandler(varValue, propValue, selfPolicy, serializeExpr, defaultExpr, nullMsg);
        }


        public override Expression HandleIList(string tagName, PropertyInfo property, Type iListImpl,
                                               NullPolicy selfPolicy, NullPolicy elementPolicy) {
            // Getter for the property value
            Expression getPropertyExpr = Expression.MakeMemberAccess(argValue, property);
            
            // Find the element type (the 'T' in IList<T>)
            Type elementType = iListImpl.GetGenericArguments()[0];

            Expression tagNameExpr = Expression.Constant(tagName, typeof(string));
            string selfNullMsg = MakePropertyNullMessage(property);
            string elementNullMsg = MakeElementNullMessage(property);

            return MakeIListHandler(getPropertyExpr, elementType, tagNameExpr,
                                    selfPolicy, elementPolicy, selfNullMsg, elementNullMsg,
                                    expr => Expression.Call(varRootTag, NbtCompoundAddMethod, expr));
        }


        public override Expression HandleNbtTag(string tagName, PropertyInfo property, NullPolicy selfPolicy) {
            // Add tag directly to the list
            return MakeNbtTagPropertyHandler(property, tagName, property.PropertyType, selfPolicy, expr => expr);
        }


        public override Expression HandleNbtFile(string tagName, PropertyInfo property, NullPolicy selfPolicy) {
            // Add NbtFile's root tag directly to the list
            return MakeNbtTagPropertyHandler(
                property, tagName, typeof(NbtCompound), selfPolicy,
                expr => Expression.MakeMemberAccess(expr, NbtFileRootTagProperty));
        }


        public override Expression HandleStringIDictionary(string tagName, PropertyInfo property, Type iDictImpl,
                                                           NullPolicy selfPolicy, NullPolicy elementPolicy) {
            Expression getIDictExpr = Expression.MakeMemberAccess(argValue, property);
            string nullElementMessage = MakeElementNullMessage(property);
            string nullMessage = MakePropertyNullMessage(property);

            return MakeStringIDictionaryHandler(
                Expression.Constant(tagName, typeof(string)), getIDictExpr, iDictImpl,
                selfPolicy, elementPolicy, nullMessage, nullElementMessage,
                expr => Expression.Call(varRootTag, NbtCompoundAddMethod, expr));
        }


        public override Expression HandleCompoundObject(string tagName, PropertyInfo property, NullPolicy selfPolicy) {
            return MakeNbtTagPropertyHandler(
                property, tagName, typeof(NbtCompound), selfPolicy,
                expr => callResolver.MakeCall(property.PropertyType,
                                              Expression.Constant(tagName, typeof(string)), expr));
        }

        #endregion


        // Creates serialization code for arrays and implementations of IList<?>.
        // For byte[] and int[], we use SerializePropertyDirectly(...) instead -- it's more efficient.
        [NotNull]
        Expression MakeIListHandler([NotNull] Expression getIListExpr, [NotNull] Type elementType,
                                    [NotNull] Expression tagNameExpr,
                                    NullPolicy selfPolicy, NullPolicy elementPolicy, [NotNull] string selfNullMsg,
                                    [NotNull] string elementNullMsg,
                                    [NotNull] Func<Expression, Expression> processTagExpr) {
            Type listType = getIListExpr.Type;

            // Declare locals
            ParameterExpression varIList = Expression.Parameter(listType, "iList");
            ParameterExpression varListTag = Expression.Parameter(typeof(NbtList), "listTag");
            ParameterExpression varLength = Expression.Parameter(typeof(int), "length");
            ParameterExpression varIndex = Expression.Parameter(typeof(int), "i");

            // Find getters for this IList
            MethodInfo countGetterImpl, itemGetterImpl;
            if (listType.IsArray) {
                // Although Array claims to implement IList<>, there is no way to retrieve
                // the interface implementation: it's handled in an unusual way by the runtime.
                // So we have to resort to getting Length/GetValue instead of Count/Item
                countGetterImpl = listType.GetProperty("Length").GetGetMethod();
                itemGetterImpl = listType.GetMethod("GetValue", new[] { typeof(int) });
            } else {
                // For non-array IList<> types, grab this.Count getter (which maps to get_Count())
                countGetterImpl = SerializationUtil.GetGenericInterfaceMethodImpl(
                    listType, typeof(ICollection<>), "get_Count", Type.EmptyTypes);
                // ...and the getter for indexer this[int], which maps to get_Item(int)
                itemGetterImpl = SerializationUtil.GetGenericInterfaceMethodImpl(
                    listType, typeof(IList<>), "get_Item", new[] { typeof(int) });
            }

            // Create handler for a single element
            Expression getElementExpr = Expression.Call(varIList, itemGetterImpl, varIndex);
            Expression getCountExpr = Expression.Call(varIList, countGetterImpl);
            Expression handleOneElementExpr = MakeElementHandler(
                elementType, NbtCompiler.NullStringExpr, getElementExpr, elementPolicy, elementNullMsg,
                tag => Expression.Call(varListTag, NbtListAddMethod, tag));

            // Arrange tag construction in a loop
            LabelTarget loopBreak = Expression.Label(typeof(void));
            Expression mainLoop = 
                // while (true)
                Expression.Loop(
                    Expression.Block(
                        // if (i >= length) break;
                        Expression.IfThen(
                            Expression.GreaterThanOrEqual(varIndex, varLength),
                            Expression.Break(loopBreak)),

                        // <process and add one element to the list>
                        handleOneElementExpr,

                        // ++i;
                        Expression.PreIncrementAssign(varIndex)),
                    loopBreak);

            // new NbtList(tagName, NbtTagType.*)
            ConstructorInfo listTagCtor =
                typeof(NbtList).GetConstructor(new[] { typeof(string), typeof(NbtTagType) });
            Expression makeListTagExpr =
                Expression.New(listTagCtor, tagNameExpr, Expression.Constant(GetNbtTagType(elementType)));

            // Fallback path, in case value our IList null and NullPolicy is InsertDefaults:
            // Add an empty list to root.
            Expression defaultValExpr = processTagExpr(makeListTagExpr);

            // Primary path, in case our IList is not null:
            // Package the list-building loop into a neat block, with locals
            Expression makeTagExpr = Expression.Block(
                new[] { varListTag, varIndex, varLength },

                // NbtList listTag = new NbtList(tagName, NbtTagType.*);
                Expression.Assign(varListTag, makeListTagExpr),

                // int length = iList.Count;
                Expression.Assign(varLength, getCountExpr),

                // int i=0;
                Expression.Assign(varIndex, Expression.Constant(0)),

                // (fill the list tag)
                mainLoop,

                // rootTag.Add( listTag );
                processTagExpr(varListTag));

            // Generate the appropriate enclosing expressions, which choose path depending on NullPolicy
            return NbtCompiler.MakeNullHandler(varIList, getIListExpr, selfPolicy,
                                               makeTagExpr, defaultValExpr, selfNullMsg);
        }


        // Creates serialization code for values of type IDictionary<string,?>
        [NotNull]
        Expression MakeStringIDictionaryHandler([NotNull] Expression tagNameExpr, [NotNull] Expression getIDictExpr,
                                                [NotNull] Type iDictImpl,
                                                NullPolicy selfPolicy, NullPolicy elementPolicy,
                                                [NotNull] string selfNullMsg, [NotNull] string elementNullMsg,
                                                [NotNull] Func<Expression, Expression> processTagExpr) {
            Type elementType = iDictImpl.GetGenericArguments()[1];

            // find type of KeyValuePair<string,?> that the enumerator will return
            Type kvpType = typeof(KeyValuePair<,>).MakeGenericType(iDictImpl.GetGenericArguments());
            PropertyInfo keyProp = kvpType.GetProperty("Key");
            PropertyInfo valueProp = kvpType.GetProperty("Value");

            // locate IDictionary.GetEnumerator()
            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(kvpType);
            Type enumeratorType = typeof(IEnumerator<>).MakeGenericType(kvpType);
            MethodInfo getEnumeratorImpl =
                SerializationUtil.GetGenericInterfaceMethodImpl(
                    iDictImpl, enumerableType, "GetEnumerator", Type.EmptyTypes);

            // locate IEnumerator.MoveNext()
            MethodInfo moveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
            MethodInfo disposeMethod = typeof(IDisposable).GetMethod("Dispose");
            PropertyInfo currentProp = enumeratorType.GetProperty("Current");

            ParameterExpression varIDict = Expression.Parameter(iDictImpl, "iDict");
            ParameterExpression varCompTag = Expression.Parameter(typeof(NbtCompound), "compTag");
            ParameterExpression varEnumerator = Expression.Parameter(enumeratorType, "enumerator");
            ParameterExpression varKvp = Expression.Parameter(kvpType, "element");

            // generate handlers for individual elements
            Expression getNameExpr = Expression.MakeMemberAccess(varKvp, keyProp);
            Expression getValueExpr = Expression.MakeMemberAccess(varKvp, valueProp);
            Expression handleOneElementExpr = MakeElementHandler(
                elementType, getNameExpr, getValueExpr, elementPolicy, elementNullMsg,
                tag => Expression.Call(varCompTag, NbtCompoundAddMethod, tag));

            // Make glue code to hold everything together
            LabelTarget loopBreak = Expression.Label(typeof(void));

            Expression loopBody = Expression.Block(
                new[] { varKvp },
                Expression.Assign(varKvp, Expression.MakeMemberAccess(varEnumerator, currentProp)),
                handleOneElementExpr);

            Expression makeIDictTagExpr =
                Expression.Block(
                    new[] { varCompTag, varEnumerator },

                    // varCompTag = new NbtCompound(tagName)
                    Expression.Assign(varCompTag, Expression.New(NbtCompoundCtor, tagNameExpr)),
                    // varEnumerator = iDict.GetEnumerator()
                    Expression.Assign(varEnumerator, Expression.Call(varIDict, getEnumeratorImpl)),

                    // try {
                    Expression.MakeTry(
                        typeof(void),
                        // while (enumerator.MoveNext()) <loopBody>;
                        Expression.Loop(
                            Expression.IfThenElse(Expression.Call(varEnumerator, moveNextMethod),
                                                  loopBody,
                                                  Expression.Break(loopBreak)),
                            loopBreak),

                        // } finally { enumerator.Dispose(); }
                        Expression.Call(varEnumerator, disposeMethod),
                        null, null),

                    processTagExpr(varCompTag));

            // default value (in case selfPolicy is InsertDefault): new NbtCompound(tagName)
            Expression defaultValExpr = Expression.New(NbtCompoundCtor, tagNameExpr);

            return NbtCompiler.MakeNullHandler(varIDict, getIDictExpr, selfPolicy,
                                               makeIDictTagExpr, defaultValExpr, selfNullMsg);
        }


        // Creates code for handling a single element of array, IList<?>, or IDictionary<string,?>.
        [NotNull]
        Expression MakeElementHandler([NotNull] Type elementType, [NotNull] Expression tagNameExpr,
                                      [NotNull] Expression tagValueExpr,
                                      NullPolicy elementPolicy, [NotNull] string nullElementMsg,
                                      [NotNull] Func<Expression, Expression> addTagExprFunc) {
            if (tagValueExpr.Type != elementType) {
                // In case value getter's return type is different from value return type (e.g. Array.GetValue(int))
                tagValueExpr = Expression.Convert(tagValueExpr, elementType);
            }

            if (elementType.IsPrimitive || elementType.IsEnum) {
                //=== Serializing primitives and enums ===
                // tag.Add( new NbtTag(kvp.Key, kvp.Value) );
                return addTagExprFunc(MakeNbtTagCtor(elementType, tagNameExpr, tagValueExpr));

            } else if (SerializationUtil.IsDirectlyMappedType(elementType)) {
                //=== Serializing directly-mapped reference types (byte[], int[], string) ===
                // declare a local var, which will hold the property's value
                ParameterExpression varElementValue = Expression.Parameter(elementType, "elementValue");

                // Primary path, in case element value is not null:
                // iDictTag.Add(new NbtTag(kvp.Key, <getValueExpr>));
                Expression addElementExpr =
                    addTagExprFunc(MakeNbtTagCtor(elementType, tagNameExpr, tagValueExpr));

                // Fallback path, in case element value is null and elementPolicy is InsertDefaults:
                // Add a default-value tag to the list: listTag.Add(new NbtTag(null, <default>))
                Expression defaultValExpr = Expression.Constant(SerializationUtil.GetDefaultValue(elementType));
                Expression defaultElementExpr =
                    addTagExprFunc(MakeNbtTagCtor(elementType, tagNameExpr, defaultValExpr));

                // generate the appropriate enclosing expressions, depending on NullPolicy
                return NbtCompiler.MakeNullHandler(varElementValue, tagValueExpr, elementPolicy,
                                                   addElementExpr, defaultElementExpr, nullElementMsg);
            } else {
                //=== Serializing everything else ===
                // Check if this is an IList-of-ILists
                Type iListImpl = SerializationUtil.GetGenericInterfaceImpl(elementType, typeof(IList<>));
                Type iDictImpl = SerializationUtil.GetStringIDictionaryImpl(elementType);

                // check if this type can handle its own serialization
                if (typeof(INbtSerializable).IsAssignableFrom(elementType)) {
                    // element is INbtSerializable
                    MethodInfo serializeMethod = elementType.GetMethod("Serialize", new[] { typeof(string) });
                    Expression newTagExpr = Expression.Call(tagValueExpr, serializeMethod, tagNameExpr);
                    return addTagExprFunc(newTagExpr);

                } else if (typeof(NbtTag).IsAssignableFrom(elementType)) {
                    // element is NbtTag
                    return MakeNbtTagHandler(
                        elementType, elementType, tagNameExpr, tagValueExpr, elementPolicy, nullElementMsg,
                        expr => expr, addTagExprFunc);

                } else if (typeof(NbtFile).IsAssignableFrom(elementType)) {
                    // element is NbtFile
                    return MakeNbtTagHandler(
                        typeof(NbtCompound), typeof(NbtFile), tagNameExpr, tagValueExpr, elementPolicy, nullElementMsg,
                        expr => Expression.MakeMemberAccess(expr, NbtFileRootTagProperty), addTagExprFunc);

                } else if (iListImpl != null) {
                    // element is IList<?>
                    Type subElementType = iListImpl.GetGenericArguments()[0];
                    return MakeIListHandler(tagValueExpr, subElementType, tagNameExpr, elementPolicy,
                                            elementPolicy, nullElementMsg, nullElementMsg, addTagExprFunc);

                } else if (iDictImpl != null) {
                    // element is IDictionary<string,?>
                    return MakeStringIDictionaryHandler(tagNameExpr, tagValueExpr, iDictImpl, elementPolicy,
                                                        elementPolicy, nullElementMsg, nullElementMsg, addTagExprFunc);

                } else {
                    // Get NbtSerialize<T> method for elementType
                    Expression makeElementTagExpr = callResolver.MakeCall(elementType, tagNameExpr, tagValueExpr);

                    // declare a local var, which will hold the element's value
                    ParameterExpression varElementValue = Expression.Parameter(elementType, "elementValue");

                    // Primary path, adds the newly-made Compound tag to our list
                    Expression addSerializedCompoundExpr = addTagExprFunc(makeElementTagExpr);

                    // Fallback path, in case element's value is null and NullPolicy is InsertDefaults
                    Expression addEmptyCompoundExpr =
                        addTagExprFunc(Expression.New(NbtCompoundCtor, tagNameExpr));

                    // Generate the appropriate enclosing expressions, which choose path depending on NullPolicy
                    return NbtCompiler.MakeNullHandler(varElementValue, tagValueExpr, elementPolicy,
                                                       addSerializedCompoundExpr, addEmptyCompoundExpr, nullElementMsg);
                }
            }
        }
        

        // Creates expression that handles a property that's directly convertible to an NbtTag object.
        // Wrapper for the more-general MakeNbtTagHandler(...) function.
        // Value of <property> should be convertible by <conversionFunc> to an expression that evaluates to an NbtTag object.
        // At run time, if the value is not null, the NbtTag is added to <varRootTag> (which is an NbtCompound).
        [NotNull]
        Expression MakeNbtTagPropertyHandler([NotNull] PropertyInfo property, [NotNull] string tagName,
                                             [NotNull] Type tagType, NullPolicy selfPolicy,
                                             [NotNull] Func<ParameterExpression, Expression> conversionFunc) {
            // Getter for the property value
            Expression getPropertyExpr = Expression.MakeMemberAccess(argValue, property);
            string nullMsg = MakePropertyNullMessage(property);
            Expression tagNameExpr = Expression.Constant(tagName, typeof(string));

            // Generate the appropriate enclosing expressions, which choose path depending on NullPolicy
            return MakeNbtTagHandler(tagType, property.PropertyType, tagNameExpr, getPropertyExpr, selfPolicy, nullMsg,
                                     conversionFunc,
                                     tagExpr => Expression.Call(varRootTag, NbtCompoundAddMethod, tagExpr));
        }


        // Creates an expression that handles an expression that's directly convertible to an NbtTag object.
        // Value of <property> should be convertible by <conversionFunc> to an expression that evaluates to an NbtTag object.
        // At run time, if the value is not null, the NbtTag is added to <varRootTag> (which is an NbtCompound).
        // Otherwise if value is null,
        // a) If NullPolicy=Error, a NullReferenceException is thrown
        // b) If NullPolicy=Ignore, nothing happens
        // c) If NullPolicy=InsertDefault, an empty NbtCompound tag is added to <varRootTag>
        [NotNull]
        Expression MakeNbtTagHandler([NotNull] Type tagType, [NotNull] Type valueType, [NotNull] Expression tagNameExpr,
                                     [NotNull] Expression getPropertyExpr, NullPolicy selfPolicy,
                                     [NotNull] String nullMsg,
                                     [NotNull] Func<ParameterExpression, Expression> conversionFunc,
                                     [NotNull] Func<Expression, Expression> processTagExpr) {
            // declare a local var, which will hold the property's value
            ParameterExpression varValue = Expression.Parameter(valueType, "value");

            // Primary path, adds the root tag of the NbtFile
            Expression makeTagExpr = processTagExpr(conversionFunc(varValue));

            // Fallback path, in case value is null and NullPolicy is InsertDefaults
            ConstructorInfo tagCtor;
            if (tagType == typeof(NbtTag)) {
                tagCtor = NbtCompoundCtor;
            } else {
                tagCtor = tagType.GetConstructor(new[] { typeof(string) });
            }
            Expression defaultVal = Expression.New(tagCtor, tagNameExpr);
            Expression defaultValExpr = processTagExpr(defaultVal);

            // Generate the appropriate enclosing expressions, which choose path depending on NullPolicy
            return NbtCompiler.MakeNullHandler(varValue, getPropertyExpr, selfPolicy,
                                               makeTagExpr, defaultValExpr, nullMsg);
        }


        // Creates an NbtTag constructor for given tag name and value expressions.
        // valueType must be a primitive or an enum. Casting and conversion are added as needed.
        [NotNull]
        static NewExpression MakeNbtTagCtor([NotNull] Type valueType, [NotNull] Expression tagNameExpr,
                                            [NotNull] Expression rawValueExpr) {
            if (!SerializationUtil.IsDirectlyMappedType(valueType)) {
                // Shouldn't trigger unless this method is internally misused
                throw new ArgumentException("Given type must be primitive, enum, string, byte[], or int[]", "valueType");
            }

            // Add conversion logic, if needed
            Expression tagValueExpr = MakeConversionToDirectType(valueType, rawValueExpr);

            // Find an NbtTag subtype for given type. Given type must be primitive or enum.
            // For example: byte -> NbtByte; int[] -> NbtIntArray, etc 
            Type elementTagType = SerializationUtil.TypeToTagMap[tagValueExpr.Type];

            // Find appropriate constructor
            ConstructorInfo tagCtor = elementTagType.GetConstructor(new[] { typeof(string), tagValueExpr.Type });
            // ReSharper disable once AssignNullToNotNullAttribute -- constructor will never be null
            return Expression.New(tagCtor, tagNameExpr, tagValueExpr);
        }


        // Perform any necessary conversion to go from tagValueExpr to closest directly-mapped value type
        [NotNull]
        static Expression MakeConversionToDirectType([NotNull] Type valueType, [NotNull] Expression tagValueExpr) {
            // Add casting/conversion, if needed
            Type convertedType = SerializationUtil.GetConvertedType(valueType);

            // boxed values returned by Array.GetValue() needs to be cast to bool first
            if (valueType != tagValueExpr.Type) {
                tagValueExpr = Expression.Convert(tagValueExpr, valueType);
            }

            if (valueType == typeof(bool)) {
                // Special handling for booleans: (<tagValueExpr> ? (byte)1 : (byte)0)
                return Expression.Condition(tagValueExpr,
                                            Expression.Constant((byte)1),
                                            Expression.Constant((byte)0));
            } else if (valueType != convertedType) {
                // special handling (casting) for enums and sbyte/ushort/char/uint/ulong/decimal
                return Expression.Convert(tagValueExpr, convertedType);
            } else {
                return tagValueExpr;
            }
        }


        // Finds a NbtTagType for given value type.
        // NbtTagType.Compound is returned for any value type that is not a primitive/enum/array/IList<T>
        // For example: int -> NbtTagType.Int; List<string> -> NbtTagType.List; etc
        static NbtTagType GetNbtTagType([NotNull] Type rawValueType) {
            Type convertedType = SerializationUtil.GetConvertedType(rawValueType);

            Type directTagType;
            if (SerializationUtil.TypeToTagMap.TryGetValue(convertedType, out directTagType)) {
                return SerializationUtil.FindTagTypeEnum(directTagType);
            }

            Type iListImpl = SerializationUtil.GetGenericInterfaceImpl(rawValueType, typeof(IList<>));
            if (iListImpl != null) {
                return NbtTagType.List;
            } else {
                return NbtTagType.Compound;
            }
        }


        // Generate a message for a NullReferenceException to be thrown if given property's value is null
        [NotNull]
        static string MakePropertyNullMessage([NotNull] PropertyInfo property) {
            return string.Format("Property {0}.{1} may not be null.",
                                 // ReSharper disable once PossibleNullReferenceException
                                 property.DeclaringType.Name, property.Name);
        }


        // Generate a message for a NullReferenceException to be thrown if given property's element is null
        [NotNull]
        static string MakeElementNullMessage([NotNull] PropertyInfo property) {
            return string.Format("Null elements not allowed inside {0}.{1}",
                                 // ReSharper disable once PossibleNullReferenceException
                                 property.DeclaringType.Name, property.Name);
        }
    }
}
