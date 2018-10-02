using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace fNbt {
    /// <summary> Base class for different kinds of named binary tags. </summary>
    public abstract class NbtTag : ICloneable {
        /// <summary> Parent compound tag, either NbtList or NbtCompound, if any.
        /// May be <c>null</c> for detached tags. </summary>
        [CanBeNull]
        public NbtTag Parent { get; internal set; }

        /// <summary> Type of this tag. </summary>
        public abstract NbtTagType TagType { get; }

        /// <summary> Returns true if tags of this type have a value attached.
        /// All tags except Compound, List, and End have values. </summary>
        public bool HasValue {
            get {
                switch (TagType) {
                    case NbtTagType.Compound:
                    case NbtTagType.End:
                    case NbtTagType.List:
                    case NbtTagType.Unknown:
                        return false;
                    default:
                        return true;
                }
            }
        }

        /// <summary> Name of this tag. Immutable, and set by the constructor. May be <c>null</c>. </summary>
        /// <exception cref="ArgumentNullException"> If <paramref name="value"/> is <c>null</c>, and <c>Parent</c> tag is an NbtCompound.
        /// Name of tags inside an <c>NbtCompound</c> may not be null. </exception>
        /// <exception cref="ArgumentException"> If this tag resides in an <c>NbtCompound</c>, and a sibling tag with the name already exists. </exception>
        [CanBeNull]
        public string Name {
            get { return name; }
            set {
                if (name == value) {
                    return;
                }

                var parentAsCompound = Parent as NbtCompound;
                if (parentAsCompound != null) {
                    if (value == null) {
                        throw new ArgumentNullException("value",
                                                        "Name of tags inside an NbtCompound may not be null.");
                    } else if (name != null) {
                        parentAsCompound.RenameTag(name, value);
                    }
                }

                name = value;
            }
        }

        protected string name;

        /// <summary> Gets the full name of this tag, including all parent tag names, separated by dots. 
        /// Unnamed tags show up as empty strings. </summary>
        [NotNull]
        public string Path {
            get {
                if (Parent == null) {
                    return Name ?? "";
                }
                var parentAsList = Parent as NbtList;
                if (parentAsList != null) {
                    return parentAsList.Path + '[' + parentAsList.IndexOf(this) + ']';
                } else {
                    return Parent.Path + '.' + Name;
                }
            }
        }

        internal abstract bool ReadTag([NotNull] NbtBinaryReader readStream);

        internal abstract void SkipTag([NotNull] NbtBinaryReader readStream);

        internal abstract void WriteTag([NotNull] NbtBinaryWriter writeReader);

        // WriteData does not write the tag's ID byte or the name
        internal abstract void WriteData([NotNull] NbtBinaryWriter writeReader);


        #region Shortcuts

        /// <summary> Gets or sets the tag with the specified name. May return <c>null</c>. </summary>
        /// <returns> The tag with the specified key. Null if tag with the given name was not found. </returns>
        /// <param name="tagName"> The name of the tag to get or set. Must match tag's actual name. </param>
        /// <exception cref="InvalidOperationException"> If used on a tag that is not NbtCompound. </exception>
        /// <remarks> ONLY APPLICABLE TO NbtCompound OBJECTS!
        /// Included in NbtTag base class for programmers' convenience, to avoid extra type casts. </remarks>
        public virtual NbtTag this[string tagName] {
            get { throw new InvalidOperationException("String indexers only work on NbtCompound tags."); }
            set { throw new InvalidOperationException("String indexers only work on NbtCompound tags."); }
        }

        /// <summary> Gets or sets the tag at the specified index. </summary>
        /// <returns> The tag at the specified index. </returns>
        /// <param name="tagIndex"> The zero-based index of the tag to get or set. </param>
        /// <exception cref="ArgumentOutOfRangeException"> tagIndex is not a valid index in this tag. </exception>
        /// <exception cref="ArgumentNullException"> Given tag is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> Given tag's type does not match ListType. </exception>
        /// <exception cref="InvalidOperationException"> If used on a tag that is not NbtList, NbtByteArray, or NbtIntArray. </exception>
        /// <remarks> ONLY APPLICABLE TO NbtList, NbtByteArray, and NbtIntArray OBJECTS!
        /// Included in NbtTag base class for programmers' convenience, to avoid extra type casts. </remarks>
        public virtual NbtTag this[int tagIndex] {
            get { throw new InvalidOperationException("Integer indexers only work on NbtList tags."); }
            set { throw new InvalidOperationException("Integer indexers only work on NbtList tags."); }
        }

        /// <summary> Returns the value of this tag, cast as a byte.
        /// Only supported by NbtByte tags. </summary>
        /// <exception cref="InvalidCastException"> When used on a tag other than NbtByte. </exception>
        public byte ByteValue {
            get {
                if (TagType == NbtTagType.Byte) {
                    return ((NbtByte)this).Value;
                } else {
                    throw new InvalidCastException("Cannot get ByteValue from " + GetCanonicalTagName(TagType));
                }
            }
        }

        /// <summary> Returns the value of this tag, cast as a short (16-bit signed integer).
        /// Only supported by NbtByte and NbtShort. </summary>
        /// <exception cref="InvalidCastException"> When used on an unsupported tag. </exception>
        public short ShortValue {
            get {
                switch (TagType) {
                    case NbtTagType.Byte:
                        return ((NbtByte)this).Value;
                    case NbtTagType.Short:
                        return ((NbtShort)this).Value;
                    default:
                        throw new InvalidCastException("Cannot get ShortValue from " + GetCanonicalTagName(TagType));
                }
            }
        }

        /// <summary> Returns the value of this tag, cast as an int (32-bit signed integer).
        /// Only supported by NbtByte, NbtShort, and NbtInt. </summary>
        /// <exception cref="InvalidCastException"> When used on an unsupported tag. </exception>
        public int IntValue {
            get {
                switch (TagType) {
                    case NbtTagType.Byte:
                        return ((NbtByte)this).Value;
                    case NbtTagType.Short:
                        return ((NbtShort)this).Value;
                    case NbtTagType.Int:
                        return ((NbtInt)this).Value;
                    default:
                        throw new InvalidCastException("Cannot get IntValue from " + GetCanonicalTagName(TagType));
                }
            }
        }

        /// <summary> Returns the value of this tag, cast as a long (64-bit signed integer).
        /// Only supported by NbtByte, NbtShort, NbtInt, and NbtLong. </summary>
        /// <exception cref="InvalidCastException"> When used on an unsupported tag. </exception>
        public long LongValue {
            get {
                switch (TagType) {
                    case NbtTagType.Byte:
                        return ((NbtByte)this).Value;
                    case NbtTagType.Short:
                        return ((NbtShort)this).Value;
                    case NbtTagType.Int:
                        return ((NbtInt)this).Value;
                    case NbtTagType.Long:
                        return ((NbtLong)this).Value;
                    default:
                        throw new InvalidCastException("Cannot get LongValue from " + GetCanonicalTagName(TagType));
                }
            }
        }

        /// <summary> Returns the value of this tag, cast as a long (64-bit signed integer).
        /// Only supported by NbtFloat and, with loss of precision, by NbtDouble, NbtByte, NbtShort, NbtInt, and NbtLong. </summary>
        /// <exception cref="InvalidCastException"> When used on an unsupported tag. </exception>
        public float FloatValue {
            get {
                switch (TagType) {
                    case NbtTagType.Byte:
                        return ((NbtByte)this).Value;
                    case NbtTagType.Short:
                        return ((NbtShort)this).Value;
                    case NbtTagType.Int:
                        return ((NbtInt)this).Value;
                    case NbtTagType.Long:
                        return ((NbtLong)this).Value;
                    case NbtTagType.Float:
                        return ((NbtFloat)this).Value;
                    case NbtTagType.Double:
                        return (float)((NbtDouble)this).Value;
                    default:
                        throw new InvalidCastException("Cannot get FloatValue from " + GetCanonicalTagName(TagType));
                }
            }
        }

        /// <summary> Returns the value of this tag, cast as a long (64-bit signed integer).
        /// Only supported by NbtFloat, NbtDouble, and, with loss of precision, by NbtByte, NbtShort, NbtInt, and NbtLong. </summary>
        /// <exception cref="InvalidCastException"> When used on an unsupported tag. </exception>
        public double DoubleValue {
            get {
                switch (TagType) {
                    case NbtTagType.Byte:
                        return ((NbtByte)this).Value;
                    case NbtTagType.Short:
                        return ((NbtShort)this).Value;
                    case NbtTagType.Int:
                        return ((NbtInt)this).Value;
                    case NbtTagType.Long:
                        return ((NbtLong)this).Value;
                    case NbtTagType.Float:
                        return ((NbtFloat)this).Value;
                    case NbtTagType.Double:
                        return ((NbtDouble)this).Value;
                    default:
                        throw new InvalidCastException("Cannot get DoubleValue from " + GetCanonicalTagName(TagType));
                }
            }
        }

        /// <summary> Returns the value of this tag, cast as a byte array.
        /// Only supported by NbtByteArray tags. </summary>
        /// <exception cref="InvalidCastException"> When used on a tag other than NbtByteArray. </exception>
        public byte[] ByteArrayValue {
            get {
                if (TagType == NbtTagType.ByteArray) {
                    return ((NbtByteArray)this).Value;
                } else {
                    throw new InvalidCastException("Cannot get ByteArrayValue from " + GetCanonicalTagName(TagType));
                }
            }
        }

        /// <summary> Returns the value of this tag, cast as an int array.
        /// Only supported by NbtIntArray tags. </summary>
        /// <exception cref="InvalidCastException"> When used on a tag other than NbtIntArray. </exception>
        public int[] IntArrayValue {
            get {
                if (TagType == NbtTagType.IntArray) {
                    return ((NbtIntArray)this).Value;
                } else {
                    throw new InvalidCastException("Cannot get IntArrayValue from " + GetCanonicalTagName(TagType));
                }
            }
        }

        /// <summary> Returns the value of this tag, cast as a string.
        /// Returns exact value for NbtString, and stringified (using InvariantCulture) value for NbtByte, NbtDouble, NbtFloat, NbtInt, NbtLong, and NbtShort.
        /// Not supported by NbtCompound, NbtList, NbtByteArray, or NbtIntArray. </summary>
        /// <exception cref="InvalidCastException"> When used on an unsupported tag. </exception>
        public string StringValue {
            get {
                switch (TagType) {
                    case NbtTagType.String:
                        return ((NbtString)this).Value;
                    case NbtTagType.Byte:
                        return ((NbtByte)this).Value.ToString(CultureInfo.InvariantCulture);
                    case NbtTagType.Double:
                        return ((NbtDouble)this).Value.ToString(CultureInfo.InvariantCulture);
                    case NbtTagType.Float:
                        return ((NbtFloat)this).Value.ToString(CultureInfo.InvariantCulture);
                    case NbtTagType.Int:
                        return ((NbtInt)this).Value.ToString(CultureInfo.InvariantCulture);
                    case NbtTagType.Long:
                        return ((NbtLong)this).Value.ToString(CultureInfo.InvariantCulture);
                    case NbtTagType.Short:
                        return ((NbtShort)this).Value.ToString(CultureInfo.InvariantCulture);
                    default:
                        throw new InvalidCastException("Cannot get StringValue from " + GetCanonicalTagName(TagType));
                }
            }
        }

        #endregion


        /// <summary> Returns a canonical (Notchy) name for the given NbtTagType,
        /// e.g. "TAG_Byte_Array" for NbtTagType.ByteArray </summary>
        /// <param name="type"> NbtTagType to name. </param>
        /// <returns> String representing the canonical name of a tag,
        /// or null of given TagType does not have a canonical name (e.g. Unknown). </returns>
        [CanBeNull]
        public static string GetCanonicalTagName(NbtTagType type) {
            switch (type) {
                case NbtTagType.Byte:
                    return "TAG_Byte";
                case NbtTagType.ByteArray:
                    return "TAG_Byte_Array";
                case NbtTagType.Compound:
                    return "TAG_Compound";
                case NbtTagType.Double:
                    return "TAG_Double";
                case NbtTagType.End:
                    return "TAG_End";
                case NbtTagType.Float:
                    return "TAG_Float";
                case NbtTagType.Int:
                    return "TAG_Int";
                case NbtTagType.IntArray:
                    return "TAG_Int_Array";
                case NbtTagType.List:
                    return "TAG_List";
                case NbtTagType.Long:
                    return "TAG_Long";
                case NbtTagType.Short:
                    return "TAG_Short";
                case NbtTagType.String:
                    return "TAG_String";
                default:
                    return null;
            }
        }


        /// <summary> Prints contents of this tag, and any child tags, to a string.
        /// Indents the string using multiples of the given indentation string. </summary>
        /// <returns> A string representing contents of this tag, and all child tags (if any). </returns>
        public override string ToString() {
            return ToString(DefaultIndentString);
        }


        /// <summary> Creates a deep copy of this tag. </summary>
        /// <returns> A new NbtTag object that is a deep copy of this instance. </returns>
        public abstract object Clone();


        /// <summary> Prints contents of this tag, and any child tags, to a string.
        /// Indents the string using multiples of the given indentation string. </summary>
        /// <param name="indentString"> String to be used for indentation. </param>
        /// <returns> A string representing contents of this tag, and all child tags (if any). </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="indentString"/> is <c>null</c>. </exception>
        [NotNull]
        public string ToString([NotNull] string indentString) {
            if (indentString == null) throw new ArgumentNullException("indentString");
            var sb = new StringBuilder();
            PrettyPrint(sb, indentString, 0);
            return sb.ToString();
        }


        internal abstract void PrettyPrint([NotNull] StringBuilder sb, [NotNull] string indentString, int indentLevel);

        /// <summary> String to use for indentation in NbtTag's and NbtFile's ToString() methods by default. </summary>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is <c>null</c>. </exception>
        [NotNull]
        public static string DefaultIndentString {
            get { return defaultIndentString; }
            set {
                if (value == null) throw new ArgumentNullException("value");
                defaultIndentString = value;
            }
        }

        static string defaultIndentString = "  ";
    }
}
