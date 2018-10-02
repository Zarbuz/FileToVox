using System;
using System.Text;
using JetBrains.Annotations;

namespace fNbt {
    /// <summary> A tag containing a single signed 32-bit integer. </summary>
    public sealed class NbtInt : NbtTag {
        /// <summary> Type of this tag (Int). </summary>
        public override NbtTagType TagType {
            get { return NbtTagType.Int; }
        }

        /// <summary> Value/payload of this tag (a single signed 32-bit integer). </summary>
        public int Value { get; set; }


        /// <summary> Creates an unnamed NbtInt tag with the default value of 0. </summary>
        public NbtInt() {}


        /// <summary> Creates an unnamed NbtInt tag with the given value. </summary>
        /// <param name="value"> Value to assign to this tag. </param>
        public NbtInt(int value)
            : this(null, value) {}


        /// <summary> Creates an NbtInt tag with the given name and the default value of 0. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        public NbtInt([CanBeNull] string tagName)
            : this(tagName, 0) {}


        /// <summary> Creates an NbtInt tag with the given name and value. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        /// <param name="value"> Value to assign to this tag. </param>
        public NbtInt([CanBeNull] string tagName, int value) {
            name = tagName;
            Value = value;
        }


        /// <summary> Creates a copy of given NbtInt tag. </summary>
        /// <param name="other"> Tag to copy. May not be <c>null</c>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="other"/> is <c>null</c>. </exception>
        public NbtInt([NotNull] NbtInt other) {
            if (other == null) throw new ArgumentNullException("other");
            name = other.name;
            Value = other.Value;
        }


        internal override bool ReadTag(NbtBinaryReader readStream) {
            if (readStream.Selector != null && !readStream.Selector(this)) {
                readStream.ReadInt32();
                return false;
            }
            Value = readStream.ReadInt32();
            return true;
        }


        internal override void SkipTag(NbtBinaryReader readStream) {
            readStream.ReadInt32();
        }


        internal override void WriteTag(NbtBinaryWriter writeStream) {
            writeStream.Write(NbtTagType.Int);
            if (Name == null) throw new NbtFormatException("Name is null");
            writeStream.Write(Name);
            writeStream.Write(Value);
        }


        internal override void WriteData(NbtBinaryWriter writeStream) {
            writeStream.Write(Value);
        }


        public override object Clone() {
            return new NbtInt(this);
        }


        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel) {
            for (int i = 0; i < indentLevel; i++) {
                sb.Append(indentString);
            }
            sb.Append("TAG_Int");
            if (!String.IsNullOrEmpty(Name)) {
                sb.AppendFormat("(\"{0}\")", Name);
            }
            sb.Append(": ");
            sb.Append(Value);
        }
    }
}
