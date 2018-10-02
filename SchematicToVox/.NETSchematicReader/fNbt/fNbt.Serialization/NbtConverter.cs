using System;
using fNbt.Serialization.Compiled;
using JetBrains.Annotations;

namespace fNbt.Serialization {
    public class NbtConverter {
        readonly Type contractType;
        readonly NbtSerialize compiledSerializeDelegate;
        readonly NbtDeserialize compiledDeserializeDelegate;

        public Type Type {
            get { return contractType; }
        }


        internal NbtConverter([NotNull] Type contractType) {
            if (contractType == null) throw new ArgumentNullException("contractType");
            this.contractType = contractType;
            compiledSerializeDelegate = NbtCompiler.GetSerializer(contractType);
            compiledDeserializeDelegate = NbtCompiler.GetDeserializer(contractType);
        }


        public NbtTag MakeTag(string tagName, object obj) {
            if (!contractType.IsInstanceOfType(obj)) {
                throw new ArgumentException("Invalid type! Expected an object of type " + contractType);
            }
            return compiledSerializeDelegate(tagName, obj);
        }


        public NbtTag FillTag(object obj, NbtTag tag) {
            if (!contractType.IsInstanceOfType(obj)) {
                throw new ArgumentException("Invalid type! Expected an object of type " + contractType);
            }
            throw new NotImplementedException();
        }


        public object MakeObject(NbtTag tag) {
            throw new NotImplementedException();
        }


        public object FillObject(object obj, NbtTag tag) {
            throw new NotImplementedException();
        }
    }


    // Convenience class for working with strongly-typed NbtSerializers. Handy if type is known at compile time.
    public class NbtConverter<T> : NbtConverter {
        internal NbtConverter()
            : base(typeof(T)) {}


        public NbtTag MakeTag(string tagName, T obj) {
            return base.MakeTag(tagName, obj);
        }


        new public T MakeObject(NbtTag tag) {
            return (T)base.MakeObject(tag);
        }
    }
}
