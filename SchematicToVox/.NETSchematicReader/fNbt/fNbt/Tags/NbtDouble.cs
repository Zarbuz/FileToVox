using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt {
    /// <summary> A tag containing a double-precision floating point number. </summary>
    public sealed class NbtDouble : NbtTag {
        /// <summary> Type of this tag (Double). </summary>
        public override NbtTagType TagType {
            get { return NbtTagType.Double; }
        }

        /// <summary> Value/payload of this tag (a double-precision floating point number). </summary>
        public double Value { get; set; }


        /// <summary> Creates an unnamed NbtDouble tag with the default value of 0. </summary>
        public NbtDouble() {}


        /// <summary> Creates an unnamed NbtDouble tag with the given value. </summary>
        /// <param name="value"> Value to assign to this tag. </param>
        public NbtDouble(double value)
            : this(null, value) {}


        /// <summary> Creates an NbtDouble tag with the given name and the default value of 0. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        public NbtDouble([CanBeNull] string tagName)
            : this(tagName, 0) {}


        /// <summary> Creates an NbtDouble tag with the given name and value. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        /// <param name="value"> Value to assign to this tag. </param>
        public NbtDouble([CanBeNull] string tagName, double value) {
            name = tagName;
            Value = value;
        }


        /// <summary> Creates a copy of given NbtDouble tag. </summary>
        /// <param name="other"> Tag to copy. May not be <c>null</c>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="other"/> is <c>null</c>. </exception>
        public NbtDouble([NotNull] NbtDouble other) {
            if (other == null) throw new ArgumentNullException("other");
            name = other.name;
            Value = other.Value;
        }


        internal override bool ReadTag(NbtBinaryReader readStream) {
            if (readStream.Selector != null && !readStream.Selector(this)) {
                readStream.ReadDouble();
                return false;
            }
            Value = readStream.ReadDouble();
            return true;
        }


        internal override void SkipTag(NbtBinaryReader readStream) {
            readStream.ReadDouble();
        }


        internal override void WriteTag(NbtBinaryWriter writeStream) {
            writeStream.Write(NbtTagType.Double);
            if (Name == null) throw new NbtFormatException("Name is null");
            writeStream.Write(Name);
            writeStream.Write(Value);
        }


        internal override void WriteData(NbtBinaryWriter writeStream) {
            writeStream.Write(Value);
        }


        public override object Clone() {
            return new NbtDouble(this);
        }


        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel) {
            for (int i = 0; i < indentLevel; i++) {
                sb.Append(indentString);
            }
            sb.Append("TAG_Double");
            if (!String.IsNullOrEmpty(Name)) {
                sb.AppendFormat("(\"{0}\")", Name);
            }
            sb.Append(": ");
            sb.Append(Value);
        }
    }
}
