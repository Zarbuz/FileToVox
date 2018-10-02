using System;
using System.Linq;

namespace fNbt.Serialization {
    public class ConversionOptions : IEquatable<ConversionOptions> {
        public static ConversionOptions Defaults { get; private set; }
        static ConversionOptions() {
            Defaults = new ConversionOptions();
        }
        
        public NullPolicy SelfNullPolicy { get; set; }
        public NullPolicy DefaultNullPolicy { get; set; }
        public NullPolicy DefaultElementNullPolicy { get; set; }
        public MissingPolicy DefaultMissingPolicy { get; set; }
        public bool IgnoreISerializable { get; set; }
        public string[] IgnoredProperties { get; set; }


        public bool Equals(ConversionOptions other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SelfNullPolicy == other.SelfNullPolicy
                && DefaultNullPolicy == other.DefaultNullPolicy
                && DefaultElementNullPolicy == other.DefaultElementNullPolicy
                && DefaultMissingPolicy == other.DefaultMissingPolicy
                && IgnoreISerializable == other.IgnoreISerializable
                && Equals(IgnoredProperties, other.IgnoredProperties);
        }


        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ConversionOptions)obj);
        }


        public override int GetHashCode() {
            unchecked {
                var hashCode = (int)SelfNullPolicy;
                hashCode = (hashCode*397) ^ (int)DefaultNullPolicy;
                hashCode = (hashCode*397) ^ (int)DefaultElementNullPolicy;
                hashCode = (hashCode*397) ^ (int)DefaultMissingPolicy;
                hashCode = (hashCode*397) ^ IgnoreISerializable.GetHashCode();
                hashCode = (hashCode*397) ^ (IgnoredProperties != null ? IgnoredProperties.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}