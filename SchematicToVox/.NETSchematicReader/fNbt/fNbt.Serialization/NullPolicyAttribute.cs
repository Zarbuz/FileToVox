using System;

namespace fNbt.Serialization {
    public class NullPolicyAttribute : Attribute {
        public readonly NullPolicy SelfPolicy;
        public readonly NullPolicy ElementPolicy;


        public NullPolicyAttribute(NullPolicy selfPolicy) {
            SelfPolicy = selfPolicy;
            ElementPolicy = NullPolicy.Default;
        }


        public NullPolicyAttribute(NullPolicy selfPolicy, NullPolicy elementPolicy) {
            SelfPolicy = selfPolicy;
            ElementPolicy = elementPolicy;
        }
    }
}
