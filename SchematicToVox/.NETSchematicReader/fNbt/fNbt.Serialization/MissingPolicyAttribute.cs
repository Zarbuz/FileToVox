using System;

namespace fNbt.Serialization {
    public class MissingPolicyAttribute : Attribute {
        public readonly MissingPolicy Policy;

        public MissingPolicyAttribute(MissingPolicy policy) {
            Policy = policy;
        }
    }
}
