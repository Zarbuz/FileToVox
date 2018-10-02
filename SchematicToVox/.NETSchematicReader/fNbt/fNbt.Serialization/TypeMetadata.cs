using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace fNbt.Serialization {
    internal class TypeMetadata {
        public readonly TypeCategory Category;
        public readonly PropertyInfo[] Properties;
        public readonly Dictionary<PropertyInfo, string> PropertyTagNames;
        public readonly Dictionary<PropertyInfo, NullPolicy> NullPolicies;
        public readonly Dictionary<PropertyInfo, NullPolicy> ElementNullPolicies;
        public readonly HashSet<PropertyInfo> IgnoredProperties;
        

        static readonly ConcurrentDictionary<Type, TypeMetadata> TypeMetadataCache =
            new ConcurrentDictionary<Type, TypeMetadata>();


        // Read and store metadata about given type, for non-compiled serialization/deserialization
        // This only needs to be called once, on the very first serialization/deserialization call.
        public static TypeMetadata ReadTypeMetadata([NotNull] Type type) {
            if (type == null) throw new ArgumentNullException("type");
            TypeMetadata typeMeta;
            if (!TypeMetadataCache.TryGetValue(type, out typeMeta)) {
                // If meta cache does not contain this type yet, lock and double-check
                lock (TypeMetadataCache) {
                    if (!TypeMetadataCache.TryGetValue(type, out typeMeta)) {
                        // If meta cache still does not contain this type, fetch info and store it in cache
                        typeMeta = new TypeMetadata(type);
                        TypeMetadataCache.TryAdd(type, typeMeta);
                    }
                }
            }
            return typeMeta;
        }


        TypeMetadata([NotNull] Type contractType) {
            Category = SerializationUtil.CategorizeType(contractType);
            if (Category == TypeCategory.ConvertibleByProperties) {
                Properties =
                    contractType.GetProperties()
                                .Where(p => !Attribute.GetCustomAttributes(p, typeof(NbtIgnoreAttribute)).Any())
                                .ToArray();
                PropertyTagNames = new Dictionary<PropertyInfo, string>();

                foreach (PropertyInfo property in Properties) {
                    // read [TagName] attributes
                    Attribute[] nameAttributes = Attribute.GetCustomAttributes(property, typeof(TagNameAttribute));
                    string tagName;
                    if (nameAttributes.Length != 0) {
                        tagName = ((TagNameAttribute)nameAttributes[0]).Name;
                    } else {
                        tagName = property.Name;
                    }
                    PropertyTagNames.Add(property, tagName);

                    // read [NullPolicy] attributes
                    var nullPolicyAttr =
                        Attribute.GetCustomAttribute(property, typeof(NullPolicyAttribute)) as NullPolicyAttribute;
                    if (nullPolicyAttr != null) {
                        if (nullPolicyAttr.SelfPolicy != NullPolicy.Default) {
                            if (NullPolicies == null) {
                                NullPolicies = new Dictionary<PropertyInfo, NullPolicy>();
                            }
                            NullPolicies.Add(property, nullPolicyAttr.SelfPolicy);
                        }
                        if (nullPolicyAttr.ElementPolicy != NullPolicy.Default) {
                            if (ElementNullPolicies == null) {
                                ElementNullPolicies = new Dictionary<PropertyInfo, NullPolicy>();
                            }
                            ElementNullPolicies.Add(property, nullPolicyAttr.ElementPolicy);
                        }
                    }

                    // check for presence of [NonSerialized] and [NbtIgnore] attributes
                    if (Attribute.IsDefined(property, typeof(NonSerializedAttribute)) ||
                        Attribute.IsDefined(property, typeof(NbtIgnoreAttribute))) {
                        if (IgnoredProperties == null) {
                            IgnoredProperties = new HashSet<PropertyInfo>();
                        }
                        IgnoredProperties.Add(property);
                    }
                }
            }
        }
    }
}
