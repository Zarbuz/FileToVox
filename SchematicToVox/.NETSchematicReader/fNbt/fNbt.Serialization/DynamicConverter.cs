using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace fNbt.Serialization {
    internal struct DynamicConverter {
        static readonly object[] EmptyParams = new object[0];

        readonly Type type;
        readonly ConversionOptions options;
        readonly TypeMetadata typeMetadata;


        public DynamicConverter([NotNull] Type type, [NotNull] ConversionOptions options) {
            if (type == null) throw new ArgumentNullException("type");
            if (options == null) throw new ArgumentNullException("options");
            this.type = type;
            this.options = options;
            typeMetadata = TypeMetadata.ReadTypeMetadata(type);
            if (typeMetadata.Category == TypeCategory.NotSupported) {
                throw new NotSupportedException("Conversion of objects of type " + type + " is not supported.");
            }
        }


        public NbtTag MakeTag([CanBeNull] string tagName, [CanBeNull] object value) {
            if (TypeCategory.MappedToPrimitive.HasFlag(typeMetadata.Category)) {
                // primitives, convertible-to-primitives, and enums
                return HandlePrimitiveOrEnum(tagName, value, type);
            } else {
            throw new NotImplementedException();
            }
        }


        public void FillObject([NotNull] NbtTag tag, [NotNull] object obj) {
            if (tag == null) throw new ArgumentNullException("tag");
            if (obj == null) throw new ArgumentNullException("obj");
            throw new NotImplementedException();
        }


        static NbtTag HandlePrimitiveOrEnum(string tagName, object value, Type valueType) {
            if (valueType.IsEnum) {
                // TODO: see if secondary conversion is necessary
                valueType = Enum.GetUnderlyingType(valueType);
            }

            // Native NBT types
            if (valueType == typeof(int)) {
                return new NbtInt(tagName, (int)value);
            } else if (valueType == typeof(byte)) {
                return new NbtByte(tagName, (byte)value);
            } else if (valueType == typeof(short)) {
                return new NbtShort(tagName, (short)value);
            } else if (valueType == typeof(long)) {
                return new NbtLong(tagName, (long)value);
            } else if (valueType == typeof(float)) {
                return new NbtFloat(tagName, (float)value);
            } else if (valueType == typeof(double)) {
                return new NbtDouble(tagName, (double)value);

            } else {
                // Other types convertible to native NBT types
                if (valueType == typeof(bool)) {
                    byte byteVal = (byte)((bool)value ? 1 : 0);
                    return new NbtByte(tagName, byteVal);
                } else if (valueType == typeof(sbyte)) {
                    return new NbtByte(tagName, (byte)(sbyte)value);
                } else if (valueType == typeof(char)) {
                    return new NbtShort(tagName, (short)(char)value);
                } else if (valueType == typeof(ushort)) {
                    return new NbtShort(tagName, (short)(ushort)value);
                } else if (valueType == typeof(uint)) {
                    return new NbtInt(tagName, (int)(uint)value);
                } else if (valueType == typeof(ulong)) {
                    return new NbtLong(tagName, (long)(ulong)value);
                } else if (valueType == typeof(decimal)) {
                    return new NbtDouble(tagName, (double)(decimal)value);
                } else {
                    throw new ArgumentException("Given valueType cannot be mapped to native NBT types.");
                }
            }
        }


        // Takes care of serializing string, byte[], and int[].
        // Fetches NullPolicy and throws SerializationException in case of prohibited null value.
        // Returns null if this property should be skipped/ignored.
        [CanBeNull]
        NbtTag HandleDirectlyMappedType([CanBeNull] string tagName, [CanBeNull] object value, [NotNull] PropertyInfo pinfo,
                                        NullPolicy nullPolicy, [NotNull] Type propType) {
            if (propType == null) throw new ArgumentNullException("propType");
            if (value == null) {
                switch (nullPolicy) {
                    case NullPolicy.InsertDefault:
                        value = SerializationUtil.GetDefaultValue(propType);
                        break;
                    case NullPolicy.Ignore:
                        return null;
                    default: // Default and Error
                        throw MakeNullException(pinfo);
                }
            }
            if (propType == typeof(string)) {
                return new NbtString(tagName,(string)value);
            }else if (propType == typeof(byte[])) {
                return new NbtByteArray(tagName,(byte[])value);
            }else if (propType == typeof(int[])) {
                return new NbtIntArray(tagName, (int[])value);
            } else {
                throw new ArgumentException("Invalid property valueType given to DynamicConverter.HandleDirectlyMappedType: expected string, byte[], or int[]");
            }
        }


        // Handles INbtConvertible, NbtTag, and NbtFile
        [CanBeNull]
        NbtTag HandleNbtConvertible<T>([CanBeNull] string tagName, [CanBeNull] T value, [NotNull] PropertyInfo pinfo,
                               Func<string,T,NbtTag> conversionFunc, Func<string,NbtTag> defaultValueFunc ) {
            if (pinfo == null) throw new ArgumentNullException("pinfo");
            if (value == null) {
                NullPolicy np = GetNullPolicy(pinfo);
                switch (np) {
                    case NullPolicy.InsertDefault:
                        return defaultValueFunc(tagName);
                    case NullPolicy.Ignore:
                        return null;
                    default: // Default and Error
                        throw MakeNullException(pinfo);
                }
            }
            return conversionFunc(tagName,value);
        }


        NbtTag HandleArray([CanBeNull] string tagName, [CanBeNull] Array value, [NotNull] PropertyInfo pinfo) {
            if (pinfo == null) throw new ArgumentNullException("pinfo");

            if (value == null) {
                NullPolicy np = GetNullPolicy(pinfo);
                switch (np) {
                    case NullPolicy.Ignore:
                        return null;
                    case NullPolicy.InsertDefault:
                        return MakeEmptyList(tagName, pinfo.PropertyType.GetElementType());
                    default:
                        throw MakeNullException(pinfo);
                }
            }
            
            if (value.Length == 0) {
                return MakeEmptyList(tagName, pinfo.PropertyType.GetElementType());
            }

            NullPolicy elementNullPolicy = GetElementNullPolicy(pinfo);
            NbtList newList = new NbtList(tagName);
            for (int i = 0; i < value.Length; i++) {
                newList.Add(HandleElement(null, value.GetValue(i), elementNullPolicy));
            }
            return newList;
        }


        NbtTag HandleIList([CanBeNull] string tagName, object value, PropertyInfo pinfo) {
            return HandleIListInner(tagName, pinfo, (dynamic)value);
        }
        
        
        NbtTag HandleIListInner<T>([CanBeNull] string tagName, PropertyInfo pinfo, List<T> value ) {
            Type elementType = typeof(T);
            if (value == null) {
                NullPolicy np = GetNullPolicy(pinfo);
                switch (np) {
                    case NullPolicy.Ignore:
                        return null;
                    case NullPolicy.InsertDefault:
                        return MakeEmptyList(tagName, elementType);
                    default:
                        throw MakeNullException(pinfo);
                }
            }

            // Get list length
            int count = value.Count;
            if (count == 0) {
                return MakeEmptyList(tagName, elementType);
            }
            
            NullPolicy elementNullPolicy = GetElementNullPolicy(pinfo);
            NbtList newList = new NbtList(tagName);
            foreach (T element in value) {
                newList.Add(HandleElement(null, element, elementNullPolicy));
            }
            return newList;
        }


        static NbtList MakeEmptyList(string tagName, Type rawElementType) {
            Type elTagTypeHandle = SerializationUtil.FindTagTypeForValue(rawElementType);
            NbtTagType listType = SerializationUtil.FindTagTypeEnum(elTagTypeHandle);
            return new NbtList(tagName, listType);
        }


        NbtTag HandleElement(string tagName, object value, NullPolicy nullPolicy) {
            throw new NotImplementedException();
        }


        static SerializationException MakeNullException(PropertyInfo pinfo) {
            string errorMsg = String.Format(
                    "Cannot serialize property {0} of given {1} object: Value is null, and NullPolicy is set to Error.",
                    pinfo.Name, pinfo.DeclaringType);
            return new SerializationException(errorMsg);
        }


        bool IsIgnored(PropertyInfo prop) {
            if ((options.IgnoredProperties != null) &&
                options.IgnoredProperties.Contains(prop.Name)) {
                // ignored by options
                return true;
            } else {
                // ignored by valueType attributes
                return (typeMetadata.IgnoredProperties != null) &&
                       typeMetadata.IgnoredProperties.Contains(prop);
            }
        }


        NullPolicy GetNullPolicy([NotNull] PropertyInfo prop) {
            if (prop == null) throw new ArgumentNullException("prop");
            if (typeMetadata.NullPolicies != null) {
                NullPolicy result;
                if (typeMetadata.NullPolicies.TryGetValue(prop, out result)) {
                    return result;
                }
            }
            return options.DefaultNullPolicy;
        }


        NullPolicy GetElementNullPolicy([NotNull] PropertyInfo prop) {
            if (prop == null) throw new ArgumentNullException("prop");
            if (typeMetadata.ElementNullPolicies != null) {
                NullPolicy result;
                if (typeMetadata.ElementNullPolicies.TryGetValue(prop, out result)) {
                    return result;
                }
            }
            return options.DefaultElementNullPolicy;
        }
    }
}
