using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace fNbt.Serialization {
    /// <summary> Contains shared code and data used by various serialization-related classes. </summary>
    internal static class SerializationUtil {
        // Gets default value for directly-mapped reference types, to substitute a null
        public static object GetDefaultValue(Type type) {
            if (type == typeof(string)) {
                return String.Empty;
            } else if (type == typeof(int[])) {
                return new int[0];
            } else if (type == typeof(byte[])) {
                return new byte[0];
            } else if (type.IsArray) {
                return Activator.CreateInstance(type);
            } else {
                throw new ArgumentException();
            }
        }


        public static TypeCategory CategorizeType(Type type) {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                // Nullable<T> is not supported
                return TypeCategory.NotSupported;
            }else if (type.IsEnum) {
                return TypeCategory.Enum;
            } else if (PrimitiveConversionMap.ContainsKey(type)) {
                return TypeCategory.ConvertiblePrimitive;
            } else if (type.IsPrimitive) {
                return TypeCategory.Primitive;
            } else if (type == typeof(string)) {
                return TypeCategory.String;
            } else if (type.IsArray) {
                if (type.GetArrayRank() > 1) {
                    // Serialization of multi-dimensional arrays is not supported at this time.
                    return TypeCategory.NotSupported;
                }
                Type elementType = type.GetElementType();
                if (IsSafelyConvertibleToByte(elementType)) {
                    return TypeCategory.ByteArray;
                } else if (IsSafelyConvertibleToInt(elementType)) {
                    return TypeCategory.IntArray;
                } else {
                    return TypeCategory.Array;
                }
            } else if (GetStringIDictionaryImpl(type) != null) {
                return TypeCategory.IDictionary;
            } else if (GetGenericInterfaceImpl(type, typeof(IList<>)) != null) {
                return TypeCategory.IList;
            } else if (typeof(NbtTag).IsAssignableFrom(type)) {
                return TypeCategory.NbtTag;
            } else if (typeof(INbtSerializable).IsAssignableFrom(type)) {
                return TypeCategory.INbtSerializable;
            } else {
                return TypeCategory.ConvertibleByProperties;
            }
        }


        public static bool IsDirectlyMappedType(Type type) {
            return !type.IsGenericType && // to catch Nullable<T>
                   (type.IsPrimitive || type.IsEnum ||
                    type == typeof(byte[]) ||
                    type == typeof(int[]) ||
                    type == typeof(string));
        }


        // Returns type of an appropriate NbtTag subclass, or null if no direct mapping could be found
        public static Type FindTagType(Type valueType) {
            Type tagType;
            TypeToTagMap.TryGetValue(valueType, out tagType);
            return tagType;
        }


        // mapping of directly-usable types to their NbtTag subtypes
        internal static readonly Dictionary<Type, Type> TypeToTagMap = new Dictionary<Type, Type> {
            { typeof(byte), typeof(NbtByte) },
            { typeof(short), typeof(NbtShort) },
            { typeof(int), typeof(NbtInt) },
            { typeof(long), typeof(NbtLong) },
            { typeof(float), typeof(NbtFloat) },
            { typeof(double), typeof(NbtDouble) },
            { typeof(byte[]), typeof(NbtByteArray) },
            { typeof(int[]), typeof(NbtIntArray) },
            { typeof(string), typeof(NbtString) }
        };


        static readonly Dictionary<Type, NbtTagType> TypeToTagTypeEnum = new Dictionary<Type, NbtTagType> {
            { typeof(NbtByte), NbtTagType.Byte },
            { typeof(NbtByteArray), NbtTagType.ByteArray },
            { typeof(NbtDouble), NbtTagType.Double },
            { typeof(NbtFloat), NbtTagType.Float },
            { typeof(NbtInt), NbtTagType.Int },
            { typeof(NbtIntArray), NbtTagType.IntArray },
            { typeof(NbtLong), NbtTagType.Long },
            { typeof(NbtShort), NbtTagType.Short },
            { typeof(NbtString), NbtTagType.String },
            { typeof(NbtCompound), NbtTagType.Compound },
            { typeof(NbtList), NbtTagType.List }
        };


        public static NbtTagType FindTagTypeEnum(Type tagType) {
            NbtTagType result;
            if (TypeToTagTypeEnum.TryGetValue(tagType, out result)) {
                return result;
            } else {
                return NbtTagType.Unknown;
            }
        }


        // Mapping of convertible value types to directly-usable primitive types
        static readonly Dictionary<Type, Type> PrimitiveConversionMap = new Dictionary<Type, Type> {
            { typeof(bool), typeof(byte) },
            { typeof(sbyte), typeof(byte) },
            { typeof(ushort), typeof(short) },
            { typeof(char), typeof(short) },
            { typeof(uint), typeof(int) },
            { typeof(ulong), typeof(long) },
            { typeof(decimal), typeof(double) }
        };


        // Finds an NBT primitive that is closest to the given type.
        // If given type primitive or enum, then the original type is returned.
        // For example: bool -> byte; char -> short, etc
        [NotNull]
        public static Type GetConvertedType([NotNull] Type rawType) {
            if (rawType == null) throw new ArgumentNullException("rawType");
            if (rawType.IsEnum) {
                rawType = Enum.GetUnderlyingType(rawType);
            }

            Type convertedType;
            if (!PrimitiveConversionMap.TryGetValue(rawType, out convertedType)) {
                convertedType = rawType;
            }

            return convertedType;
        }


        [CanBeNull]
        public static Type GetGenericInterfaceImpl(Type concreteType, Type genericInterface) {
            if (genericInterface.IsGenericTypeDefinition) {
                if (concreteType.IsGenericType && concreteType.GetGenericTypeDefinition() == genericInterface) {
                    // concreteType itself is the desired generic interface
                    return concreteType;
                } else {
                    // Check if concreteType implements the desired generic interface ONCE
                    // Double implementations (e.g. Foo : Bar<T1>, Bar<T2>) are not acceptable.
                    return concreteType.GetInterfaces()
                                       .SingleOrDefault(x => x.IsGenericType &&
                                                             x.GetGenericTypeDefinition() == genericInterface);
                }
            } else {
                return genericInterface;
            }
        }


        public static Type GetStringIDictionaryImpl(Type concreteType) {
            return concreteType.GetInterfaces().FirstOrDefault(
                iFace => iFace.IsGenericType &&
                         iFace.GetGenericTypeDefinition() == typeof(IDictionary<,>) &&
                         iFace.GetGenericArguments()[0] == typeof(string));
        }


        [NotNull]
        public static MethodInfo GetGenericInterfaceMethodImpl(Type concreteType, Type genericInterface,
                                                               string methodName, Type[] methodParams) {
            // Find a specific generic implementation of the interface
            Type impl = GetGenericInterfaceImpl(concreteType, genericInterface);
            if (impl == null) {
                throw new ArgumentException(concreteType + " does not implement " + genericInterface);
            }

            MethodInfo interfaceMethod = impl.GetMethod(methodName, methodParams);
            if (interfaceMethod == null) {
                throw new ArgumentException(genericInterface + " does not contain method " + methodName);
            }

            if (impl.IsInterface) {
                // if concreteType is itself an interface (e.g. IList<> implements ICollection<>),
                // We don't need to look up the interface implementation map. We can just return
                // the interface's method directly.
                return interfaceMethod;
            } else {
                // If concreteType is a class, we need to get a MethodInfo for its specific implementation.
                // We cannot just call "GetMethod()" on the concreteType, because explicit implementations
                // may cause ambiguity.
                InterfaceMapping implMap = concreteType.GetInterfaceMap(impl);

                int methodIndex = Array.IndexOf(implMap.InterfaceMethods, interfaceMethod);
                MethodInfo concreteMethod = implMap.TargetMethods[methodIndex];
                return concreteMethod;
            }
        }


        static readonly Dictionary<Type, Func<NbtTag>> TagConstructors = new Dictionary<Type, Func<NbtTag>> {
            { typeof(NbtByte), () => new NbtByte() },
            { typeof(NbtByteArray), () => new NbtByteArray() },
            { typeof(NbtCompound), () => new NbtCompound() },
            { typeof(NbtDouble), () => new NbtDouble() },
            { typeof(NbtFloat), () => new NbtFloat() },
            { typeof(NbtInt), () => new NbtInt() },
            { typeof(NbtIntArray), () => new NbtIntArray() },
            { typeof(NbtList), () => new NbtList() },
            { typeof(NbtLong), () => new NbtLong() },
            { typeof(NbtShort), () => new NbtShort() },
            { typeof(NbtString), () => new NbtString() }
        };


        public static Type FindTagTypeForValue(Type valueType) {
            if (valueType == null) throw new ArgumentNullException("valueType");

            Type convertedValueType = GetConvertedType(valueType);
            if (IsDirectlyMappedType(convertedValueType)) {
                // value can be mapped directly
                return FindTagType(valueType);
            } else if (typeof(NbtTag).IsAssignableFrom(valueType)) {
                // value is already an NbtTag -- nothing to construct!
                return null;
            }

            // Check if value is Nullable<T>
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                throw new NotSupportedException("Serialization of nullable types is not supported at this time.");
            }

            // Check if value is an array
            if (valueType.IsArray) {
                if (valueType.GetArrayRank() > 1) {
                    throw new NotSupportedException(
                        "Serialization of multi-dimensional arrays is not supported at this time.");
                }
                Type elementType = valueType.GetElementType();
                if (IsDirectlyMappedType(elementType)) {
                    // Array that might be convertible to byte[] or int[]
                    if (IsSafelyConvertibleToByte(elementType)) {
                        return typeof(NbtByteArray);
                    } else if (IsSafelyConvertibleToInt(elementType)) {
                        return typeof(NbtIntArray);
                    }
                }
                // Arrays cannot fit into byte[]/int[] -- use a list instead.
                return typeof(NbtList);
            }

            // Check if value is a list
            Type iListImpl = GetGenericInterfaceImpl(valueType, typeof(IList<>));
            if (iListImpl != null) {
                // Lists and arrays
                return typeof(NbtList);
            }
            // INbtSerializable, IDictionary, and everything else
            return typeof(NbtCompound);
        }

        
        // Checks if values of given type can be cast to byte without data loss
        static bool IsSafelyConvertibleToByte([NotNull] Type valueType) {
            if (valueType == null) throw new ArgumentNullException("valueType");
            return valueType == typeof(bool) ||
                   valueType == typeof(byte) || valueType == typeof(sbyte);
        }


        // Checks if values of given type can be cast to int without data loss
        static bool IsSafelyConvertibleToInt([NotNull] Type valueType) {
            if (valueType == null) throw new ArgumentNullException("valueType");
            return valueType == typeof(bool) ||
                   valueType == typeof(byte) || valueType == typeof(sbyte) ||
                   valueType == typeof(short) || valueType == typeof(ushort) ||
                   valueType == typeof(int) || valueType == typeof(uint);
        }


        /// <summary> Creates a blank tag of a type appropriate for serializing values of given type. </summary>
        /// <returns> A blank NbtTag -OR- null if given valueType already derives from NbtTag. </returns>
        [CanBeNull]
        public static NbtTag ConstructTag([NotNull] Type valueType) {
            Type tagType = FindTagTypeForValue(valueType);
            if (tagType != null) {
                return TagConstructors[tagType]();
            } else {
                return null;
            }
        }
    }
}
