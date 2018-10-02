// Uncomment this define to have serializers print debug information to stdout
#define DEBUG_NBTSERIALIZE_COMPILER

// Hiding erroneous warnings -- see http://youtrack.jetbrains.com/issue/RSRP-333085
// ReSharper disable PossiblyMistakenUseOfParamsMethod
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

#if DEBUG_NBTSERIALIZE_COMPILER
using System.Diagnostics;
#endif

namespace fNbt.Serialization.Compiled {
    delegate NbtCompound NbtSerialize(string tagName, object value);


    delegate object NbtDeserialize(object baseObject, NbtCompound tag);


    static class NbtCompiler {
        // new ArgumentNullException(string)
        static readonly ConstructorInfo ArgumentNullExceptionCtor =
            typeof(ArgumentNullException).GetConstructor(new[] { typeof(string) });

        // new NullReferenceException(string)
        static readonly ConstructorInfo NullReferenceExceptionCtor =
            typeof(NullReferenceException).GetConstructor(new[] { typeof(string) });

        // (string)null -- used to select appropriate constructor/method overloads when creating unnamed tags
        internal static readonly Expression NullStringExpr = Expression.Constant(null, typeof(string));

#if DEBUG_NBTSERIALIZE_COMPILER
        // Debug.WriteLine(string)
        static readonly MethodInfo DebugWriteLineMethod =
            typeof(Debug).GetMethod("WriteLine", new[] { typeof(string) });


        // Used for debugging
        [NotNull]
        static Expression MarkLineExpr([NotNull] string message) {
            if (message == null) throw new ArgumentNullException("message");
            var stackTrace = new StackTrace();
            StackFrame caller = stackTrace.GetFrame(1);
            string line = message + " @ " + caller.GetMethod().Name;
            return Expression.Call(DebugWriteLineMethod, Expression.Constant(line));
        }
#endif

        // Global serializer/deserializer cache
        static readonly object SerializerLock = new object();
        static readonly object DeserializerLock = new object();
        static readonly Dictionary<Type, NbtSerialize> SerializerCache = new Dictionary<Type, NbtSerialize>();
        static readonly Dictionary<Type, NbtDeserialize> DeserializerCache = new Dictionary<Type, NbtDeserialize>();

        // Used to track self-referencing and cross-referencing types that require
        // dynamic invocation by CallResolver. It's safe to have this field be static because
        // creation of new serializers is synchronized by a lock (SerializerLock)
        static readonly Dictionary<Type, Expression> ParentSerializers = new Dictionary<Type, Expression>();


        [NotNull]
        public static NbtSerialize GetSerializer([NotNull] Type t) {
            if (t == null) throw new ArgumentNullException("t");
            lock (SerializerLock) {
                NbtSerialize result;
                if (!SerializerCache.TryGetValue(t, out result)) {
                    result = CreateSerializer(t);
                }
                return result;
            }
        }


        [NotNull]
        public static NbtDeserialize GetDeserializer([NotNull] Type t) {
            if (t == null) throw new ArgumentNullException("t");
            lock (DeserializerLock) {
                NbtDeserialize result;
                if (!DeserializerCache.TryGetValue(t, out result)) {
                    result = CreateDeserializer(t);
                }
                return result;
            }
        }


        // create and invoke a variant of CreateDeserializer<T> for given type
        [NotNull]
        static NbtDeserialize CreateDeserializer( [NotNull] Type t ) {
            throw new NotImplementedException(); // TODO
        }


        // Generates specialized methods for serializing objects of given Type to NBT
        [NotNull]
        static NbtSerialize CreateSerializer(Type type) {
            // This allows our function to call itself, while it is still being built up.
            NbtSerialize placeholderDelegate = null;
            // A closure is modified intentionally, at the end of this method:
            // placeholderDelegate will be replaced with reference to the compiled function.
            // ReSharper disable once AccessToModifiedClosure
            Expression<Func<NbtSerialize>> placeholderExpr = () => placeholderDelegate;
            ParentSerializers.Add(type, placeholderExpr);
            try {
                // Define function arguments
                ParameterExpression argTagName = Expression.Parameter(typeof(string), "tagName");
                ParameterExpression argValue = Expression.Parameter(type, "value");

                // Create our resolver and emitter
                var callResolver = new CallResolver();
                CodeEmitter codeEmitter = new SerializeCodeEmitter(argTagName, argValue, callResolver);

                // Define return label
                LabelTarget returnTarget = Expression.Label(codeEmitter.ReturnValue.Type);

                // Build up the type-specific list of expressions that perform serialization
                List<Expression> propSerializersList =
                    MakePropertySerializers(codeEmitter, type);

                if (callResolver.HasParameters) {
                    propSerializersList.InsertRange(0, callResolver.GetParameterAssignmentList());
                }

                Expression serializersExpr;
                if (propSerializersList.Count == 0) {
                    serializersExpr = Expression.Empty();
                } else if (propSerializersList.Count == 1) {
                    serializersExpr = propSerializersList[0];
                } else {
                    serializersExpr = Expression.Block(propSerializersList);
                }

                // Create function-wide variables -- includes root tag and serializer delegates
                var vars = new List<ParameterExpression> {
                    codeEmitter.ReturnValue
                };
                if (callResolver.HasParameters) {
                    vars.AddRange(callResolver.GetParameterList());
                }

                // Construct the method body
                BlockExpression method = Expression.Block(
                    vars,

                    // if( argValue == null )
                    Expression.IfThen(
                        Expression.ReferenceEqual(argValue, Expression.Constant(null)),
                        //  throw new ArgumentNullException("value");
                        Expression.Throw(Expression.New(ArgumentNullExceptionCtor, Expression.Constant("value")))),
                    codeEmitter.GetPreamble(),

                    // (run the generated serializing code)
                    serializersExpr,

                    // return varRootTag;
                    Expression.Return(returnTarget, codeEmitter.ReturnValue, codeEmitter.ReturnValue.Type),
                    Expression.Label(returnTarget, Expression.Constant(null, typeof(NbtCompound))));

                // compile
                Expression<NbtSerialize> methodLambda =
                    Expression.Lambda<NbtSerialize>(method, argTagName, argValue);

#if DEBUG_NBTSERIALIZE_COMPILER
                // When in debug mode, print the expression tree to stdout.
                PropertyInfo propInfo =
                    typeof(Expression)
                        .GetProperty("DebugView",
                                     BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance);

                var debugView = (string)propInfo.GetValue(methodLambda, null);
                Debug.WriteLine(debugView);
#endif

                NbtSerialize compiledMethod = methodLambda.Compile();

                // modify the closure created earlier, to allow recursive calls
                placeholderDelegate = compiledMethod;

                SerializerCache.Add(type, compiledMethod);
                return compiledMethod;
            } finally {
                ParentSerializers.Remove(type);
            }
        }


        // Produces a list of expressions that, together, do the job of
        // producing all necessary NbtTags and adding them to the "varRootTag" compound tag.
        [NotNull]
        static List<Expression> MakePropertySerializers([NotNull] CodeEmitter codeEmitter, [NotNull] Type type) {
            var expressions = new List<Expression>();

            foreach (PropertyInfo property in GetSerializableProperties(type)) {
                Type propType = property.PropertyType;

#if DEBUG_NBTSERIALIZE_COMPILER
                expressions.Add(MarkLineExpr("Serializing " + property));
#endif

                // read tag name
                Attribute nameAttribute = Attribute.GetCustomAttribute(property, typeof(TagNameAttribute));
                string tagName;
                if (nameAttribute != null) {
                    tagName = ((TagNameAttribute)nameAttribute).Name;
                } else {
                    tagName = property.Name;
                }

                // read NullPolicy attribute
                var nullPolicyAttr =
                    (NullPolicyAttribute)Attribute.GetCustomAttribute(property, typeof(NullPolicyAttribute));
                var selfPolicy = NullPolicy.Default;
                var elementPolicy = NullPolicy.Default;
                if (nullPolicyAttr != null) {
                    selfPolicy = nullPolicyAttr.SelfPolicy;
                    elementPolicy = nullPolicyAttr.ElementPolicy;
                }

                // simple serialization for primitive types
                if (propType.IsPrimitive || propType.IsEnum) {
                    expressions.Add(codeEmitter.HandlePrimitiveOrEnum(tagName, property));
                    continue;
                }

                // serialize reference types that map directly to NBT tag types
                if (SerializationUtil.IsDirectlyMappedType(propType)) {
                    expressions.Add(codeEmitter.HandleDirectlyMappedType(tagName, property, selfPolicy));
                    continue;
                }

                // check if this type can handle its own serialization
                if (typeof(INbtSerializable).IsAssignableFrom(propType)) {
                    expressions.Add(codeEmitter.HandleINbtSerializable(tagName, property, selfPolicy));
                    continue;
                }

                // Skip serializing NbtTag properties
                if (typeof(NbtTag).IsAssignableFrom(propType)) {
                    expressions.Add(codeEmitter.HandleNbtTag(tagName, property, selfPolicy));
                    continue;
                }

                // serialize something that implements IDictionary<string,?>
                Type iDictImpl = SerializationUtil.GetStringIDictionaryImpl(propType);
                if (iDictImpl != null) {
                    expressions.Add(codeEmitter.HandleStringIDictionary(tagName, property, iDictImpl, selfPolicy,
                                                                        elementPolicy));
                    continue;
                }

                // serialize something that implements IList<?>
                Type iListImpl = SerializationUtil.GetGenericInterfaceImpl(propType, typeof(IList<>));
                if (iListImpl != null) {
                    expressions.Add(codeEmitter.HandleIList(tagName, property, iListImpl, selfPolicy, elementPolicy));
                    continue;
                }

                // Skip serializing NbtFile properties
                if (propType == typeof(NbtFile)) {
                    expressions.Add(codeEmitter.HandleNbtFile(tagName, property, selfPolicy));
                    continue;
                }

                // Compound expressions
                expressions.Add(codeEmitter.HandleCompoundObject(tagName, property, selfPolicy));
            }
            return expressions;
        }


        // Generates and returns "glue" (enclosing expression) that combines the given expressions together based on given NullPolicy.
        // The returned expression does this:
        // 1) Evaluates <getPropertyExpr> and assigns it to <varValue>
        // 2) If value is non-null, evaluates <nonNullExpr>
        // 3) If value is null:
        //    a) if NullPolicy=Error, throws a NullReferenceException with given <exceptionMessage>
        //    b) if NullPolicy=Ignore, does nothing
        //    c) if NullPolicy=InsertDefault, evaluates <defaultValExpr>
        [NotNull]
        internal static Expression MakeNullHandler([NotNull] ParameterExpression varValue,
                                                   [NotNull] Expression getPropertyExpr,
                                                   NullPolicy policy, [NotNull] Expression nonNullExpr,
                                                   [NotNull] Expression defaultValExpr,
                                                   [NotNull] string exceptionMessage) {
            // locate the getter for this property
            Expression ifExpr;

            switch (policy) {
                case NullPolicy.Error:
                    ifExpr = Expression.IfThenElse(
                        // if (value==null) throw new NullReferenceException(exceptionMessage)
                        Expression.ReferenceEqual(varValue, Expression.Constant(null)),
                        Expression.Throw(
                            Expression.New(NullReferenceExceptionCtor, Expression.Constant(exceptionMessage))),
                        // else <nonNullExpr>
                        nonNullExpr);
                    break;
                case NullPolicy.Ignore:
                    ifExpr = Expression.IfThen(
                        // if (value!=null) <nonNullExpr>
                        Expression.Not(Expression.ReferenceEqual(varValue, Expression.Constant(null))),
                        nonNullExpr);
                    break;
                case NullPolicy.InsertDefault:
                    ifExpr = Expression.IfThenElse(
                        // if (value==null) <defaultValExpr>
                        Expression.ReferenceEqual(varValue, Expression.Constant(null)),
                        defaultValExpr,
                        // else <nonNullExpr>
                        nonNullExpr);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unrecognized value for NullPolicy: " + policy);
            }

            return Expression.Block(
                // var varValue = value.ThisProperty;
                new[] { varValue },
                Expression.Assign(varValue, getPropertyExpr),
                // (check if value is null, and do something)
                ifExpr);
        }


        // Get a list of all serializable (readable, non-ignored, instance) properties for given type
        [NotNull]
        static IEnumerable<PropertyInfo> GetSerializableProperties([NotNull] Type type) {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                       .Where(p => Attribute.GetCustomAttribute(p, typeof(NbtIgnoreAttribute)) == null)
                       .Where(p => p.CanRead)
                       .ToArray();
        }


        internal class CallResolver {
            readonly Dictionary<Type, ParameterExpression> parameters = new Dictionary<Type, ParameterExpression>();


            public bool HasParameters {
                get { return parameters.Count > 0; }
            }


            public IEnumerable<ParameterExpression> GetParameterList() {
                return parameters.Values;
            }


            public IEnumerable<Expression> GetParameterAssignmentList() {
                var assignmentExprs = new List<Expression>();
                foreach (KeyValuePair<Type, ParameterExpression> param in parameters) {
                    Expression val = Expression.Invoke(ParentSerializers[param.Key]);
                    assignmentExprs.Add(Expression.Assign(param.Value, val));
                }
                return assignmentExprs;
            }


            [NotNull]
            public Expression MakeCall([NotNull] Type type, [NotNull] Expression tagNameExpr,
                                       [NotNull] Expression objectExpr) {
                Expression serializerExpr;
                if (ParentSerializers.TryGetValue(type, out serializerExpr)) {
                    // Dynamically resolved invoke -- for self-referencing/cross-referencing types
                    // We delay resolving the correct NbtSerialize delegate for type, since that delegate is still
                    // being created at this time. The resulting serialization code is a bit less efficient, but at least
                    // it has the correct behavior.
                    ParameterExpression paramExpr;
                    if (!parameters.TryGetValue(type, out paramExpr)) {
                        paramExpr = Expression.Parameter(typeof(NbtSerialize), "serializerFor" + type.Name);
                        parameters.Add(type, paramExpr);
                    }

                    return Expression.Invoke(paramExpr, tagNameExpr,
                                             objectExpr);
                } else {
                    // Statically resolved invoke
                    Delegate compoundSerializer = GetSerializer(type);
                    MethodInfo invokeMethodInfo = compoundSerializer.GetType().GetMethod("Invoke");
                    return Expression.Call(Expression.Constant(compoundSerializer), invokeMethodInfo,
                                           tagNameExpr, objectExpr);
                }
            }
        }
    }
}
