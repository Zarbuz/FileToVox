using System;

namespace fNbt.Serialization {
    /// <summary> Decorates the given property or field with the specified NBT tag name. </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public class TagNameAttribute : Attribute {
        public string Name { get; set; }


        public TagNameAttribute(string name) {
            Name = name;
        }
    }
}