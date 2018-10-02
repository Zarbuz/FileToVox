using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace fNbt {
    /// <summary> A tag containing a list of unnamed tags, all of the same kind. </summary>
    public sealed class NbtList : NbtTag, IList<NbtTag>, IList {
        /// <summary> Type of this tag (List). </summary>
        public override NbtTagType TagType {
            get { return NbtTagType.List; }
        }

        [NotNull]
        readonly List<NbtTag> tags = new List<NbtTag>();

        /// <summary> Gets or sets the tag type of this list. All tags in this NbtTag must be of the same type. </summary>
        /// <exception cref="ArgumentException"> If the given NbtTagType does not match the type of existing list items (for non-empty lists). </exception>
        /// <exception cref="ArgumentOutOfRangeException"> If the given NbtTagType is a recognized tag type. </exception>
        public NbtTagType ListType {
            get { return listType; }
            set {
                if (value == NbtTagType.End) {
                    // Empty lists may have type "End", see: https://github.com/fragmer/fNbt/issues/12
                    if (tags.Count > 0) {
                        throw new ArgumentException("Only empty list tags may have TagType of End.");
                    }
                }else if (value < NbtTagType.Byte || (value > NbtTagType.IntArray && value != NbtTagType.Unknown)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (tags.Count > 0) {
                    NbtTagType actualType = tags[0].TagType;
                    // We can safely assume that ALL tags have the same TagType as the first tag.
                    if (actualType != value) {
                        string msg = String.Format("Given NbtTagType ({0}) does not match actual element type ({1})",
                                                   value, actualType);
                        throw new ArgumentException(msg);
                    }
                }
                listType = value;
            }
        }

        NbtTagType listType;
        

        /// <summary> Creates an unnamed NbtList with empty contents and undefined ListType. </summary>
        public NbtList()
            : this(null, null, NbtTagType.Unknown) {}


        /// <summary> Creates an NbtList with given name, empty contents, and undefined ListType. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        public NbtList([CanBeNull] string tagName)
            : this(tagName, null, NbtTagType.Unknown) {}


        /// <summary> Creates an unnamed NbtList with the given contents, and inferred ListType. 
        /// If given tag array is empty, NbtTagType remains Unknown. </summary>
        /// <param name="tags"> Collection of tags to insert into the list. All tags are expected to be of the same type.
        /// ListType is inferred from the first tag. List may be empty, but may not be <c>null</c>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="tags"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> If given tags are of mixed types. </exception>
        public NbtList([NotNull] IEnumerable<NbtTag> tags)
            : this(null, tags, NbtTagType.Unknown) {
            // the base constructor will allow null "tags," but we don't want that in this constructor
            if (tags == null) throw new ArgumentNullException("tags");
        }


        /// <summary> Creates an unnamed NbtList with empty contents and an explicitly specified ListType.
        /// If ListType is Unknown, it will be inferred from the type of the first added tag.
        /// Otherwise, all tags added to this list are expected to be of the given type. </summary>
        /// <param name="givenListType"> Name to assign to this tag. May be Unknown. </param>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="givenListType"/> is not a recognized tag type. </exception>
        public NbtList(NbtTagType givenListType)
            : this(null, null, givenListType) {}


        /// <summary> Creates an NbtList with the given name and contents, and inferred ListType. 
        /// If given tag array is empty, NbtTagType remains Unknown. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        /// <param name="tags"> Collection of tags to insert into the list. All tags are expected to be of the same type.
        /// ListType is inferred from the first tag. List may be empty, but may not be <c>null</c>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="tags"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> If given tags are of mixed types. </exception>
        public NbtList([CanBeNull] string tagName, [NotNull] IEnumerable<NbtTag> tags)
            : this(tagName, tags, NbtTagType.Unknown) {
            // the base constructor will allow null "tags," but we don't want that in this constructor
            if (tags == null) throw new ArgumentNullException("tags");
        }


        /// <summary> Creates an unnamed NbtList with the given contents, and an explicitly specified ListType. </summary>
        /// <param name="tags"> Collection of tags to insert into the list.
        /// All tags are expected to be of the same type (matching givenListType).
        /// List may be empty, but may not be <c>null</c>. </param>
        /// <param name="givenListType"> Name to assign to this tag. May be Unknown (to infer type from the first element of tags). </param>
        /// <exception cref="ArgumentNullException"> <paramref name="tags"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="givenListType"/> is not a valid tag type. </exception>
        /// <exception cref="ArgumentException"> If given tags do not match <paramref name="givenListType"/>, or are of mixed types. </exception>
        public NbtList([NotNull] IEnumerable<NbtTag> tags, NbtTagType givenListType)
            : this(null, tags, givenListType) {
            // the base constructor will allow null "tags," but we don't want that in this constructor
            if (tags == null) throw new ArgumentNullException("tags");
        }


        /// <summary> Creates an NbtList with the given name, empty contents, and an explicitly specified ListType. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        /// <param name="givenListType"> Name to assign to this tag.
        /// If givenListType is Unknown, ListType will be inferred from the first tag added to this NbtList. </param>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="givenListType"/> is not a valid tag type. </exception>
        public NbtList([CanBeNull] string tagName, NbtTagType givenListType)
            : this(tagName, null, givenListType) {}


        /// <summary> Creates an NbtList with the given name and contents, and an explicitly specified ListType. </summary>
        /// <param name="tagName"> Name to assign to this tag. May be <c>null</c>. </param>
        /// <param name="tags"> Collection of tags to insert into the list.
        /// All tags are expected to be of the same type (matching givenListType). May be empty or <c>null</c>. </param>
        /// <param name="givenListType"> Name to assign to this tag. May be Unknown (to infer type from the first element of tags). </param>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="givenListType"/> is not a valid tag type. </exception>
        /// <exception cref="ArgumentException"> If given tags do not match <paramref name="givenListType"/>, or are of mixed types. </exception>
        public NbtList([CanBeNull] string tagName, [CanBeNull] IEnumerable<NbtTag> tags, NbtTagType givenListType) {
            name = tagName;
            ListType = givenListType;

            if (tags == null) return;
            foreach (NbtTag tag in tags) {
                Add(tag);
            }
        }


        /// <summary> Creates a deep copy of given NbtList. </summary>
        /// <param name="other"> An existing NbtList to copy. May not be <c>null</c>. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="other"/> is <c>null</c>. </exception>
        public NbtList([NotNull] NbtList other) {
            if (other == null) throw new ArgumentNullException("other");
            name = other.name;
            listType = other.listType;
            foreach (NbtTag tag in other.tags) {
                tags.Add((NbtTag)tag.Clone());
            }
        }


        /// <summary> Gets or sets the tag at the specified index. </summary>
        /// <returns> The tag at the specified index. </returns>
        /// <param name="tagIndex"> The zero-based index of the tag to get or set. </param>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="tagIndex"/> is not a valid index in the NbtList. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> Given tag's type does not match ListType. </exception>
        [NotNull]
        public override NbtTag this[int tagIndex] {
            get { return tags[tagIndex]; }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                } else if (value.Parent != null) {
                    throw new ArgumentException("A tag may only be added to one compound/list at a time.");
                } else if (value == this || value == Parent) {
                    throw new ArgumentException("A list tag may not be added to itself or to its child tag.");
                } else if (value.Name != null) {
                    throw new ArgumentException("Named tag given. A list may only contain unnamed tags.");
                }
                if (listType != NbtTagType.Unknown && value.TagType != listType) {
                    throw new ArgumentException("Items must be of type " + listType);
                }
                tags[tagIndex] = value;
                value.Parent = this;
            }
        }


        /// <summary> Gets or sets the tag with the specified name. </summary>
        /// <param name="tagIndex"> The zero-based index of the tag to get or set. </param>
        /// <typeparam name="T"> Type to cast the result to. Must derive from NbtTag. </typeparam>
        /// <returns> The tag with the specified key. </returns>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="tagIndex"/> is not a valid index in the NbtList. </exception>
        /// <exception cref="InvalidCastException"> If tag could not be cast to the desired tag. </exception>
        [NotNull]
        [Pure]
        public T Get<T>(int tagIndex) where T : NbtTag {
            return (T)tags[tagIndex];
        }


        /// <summary> Adds all tags from the specified collection to the end of this NbtList. </summary>
        /// <param name="newTags"> The collection whose elements should be added to this NbtList. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="newTags"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> If given tags do not match ListType, or are of mixed types. </exception>
        public void AddRange([NotNull] IEnumerable<NbtTag> newTags) {
            if (newTags == null) throw new ArgumentNullException("newTags");
            foreach (NbtTag tag in newTags) {
                Add(tag);
            }
        }


        /// <summary> Copies all tags in this NbtList to an array. </summary>
        /// <returns> Array of NbtTags. </returns>
        [NotNull]
        [Pure]
        // ReSharper disable ReturnTypeCanBeEnumerable.Global
        public NbtTag[] ToArray() {
            // ReSharper restore ReturnTypeCanBeEnumerable.Global
            return tags.ToArray();
        }


        /// <summary> Copies all tags in this NbtList to an array, and casts it to the desired type. </summary>
        /// <typeparam name="T"> Type to cast every member of NbtList to. Must derive from NbtTag. </typeparam>
        /// <returns> Array of NbtTags cast to the desired type. </returns>
        /// <exception cref="InvalidCastException"> If contents of this list cannot be cast to the given type. </exception>
        [NotNull]
        [Pure]
        public T[] ToArray<T>() where T : NbtTag {
            var result = new T[tags.Count];
            for (int i = 0; i < result.Length; i++) {
                result[i] = (T)tags[i];
            }
            return result;
        }


        #region Reading / Writing

        internal override bool ReadTag(NbtBinaryReader readStream) {
            if (readStream.Selector != null && !readStream.Selector(this)) {
                SkipTag(readStream);
                return false;
            }

            ListType = readStream.ReadTagType();

            int length = readStream.ReadInt32();
            if (length < 0) {
                throw new NbtFormatException("Negative list size given.");
            }

            for (int i = 0; i < length; i++) {
                NbtTag newTag;
                switch (ListType) {
                    case NbtTagType.Byte:
                        newTag = new NbtByte();
                        break;
                    case NbtTagType.Short:
                        newTag = new NbtShort();
                        break;
                    case NbtTagType.Int:
                        newTag = new NbtInt();
                        break;
                    case NbtTagType.Long:
                        newTag = new NbtLong();
                        break;
                    case NbtTagType.Float:
                        newTag = new NbtFloat();
                        break;
                    case NbtTagType.Double:
                        newTag = new NbtDouble();
                        break;
                    case NbtTagType.ByteArray:
                        newTag = new NbtByteArray();
                        break;
                    case NbtTagType.String:
                        newTag = new NbtString();
                        break;
                    case NbtTagType.List:
                        newTag = new NbtList();
                        break;
                    case NbtTagType.Compound:
                        newTag = new NbtCompound();
                        break;
                    case NbtTagType.IntArray:
                        newTag = new NbtIntArray();
                        break;
                    default:
                        // should never happen, since ListType is checked beforehand
                        throw new NbtFormatException("Unsupported tag type found in a list: " + ListType);
                }
                newTag.Parent = this;
                if (newTag.ReadTag(readStream)) {
                    tags.Add(newTag);
                }
            }
            return true;
        }


        internal override void SkipTag(NbtBinaryReader readStream) {
            // read list type, and make sure it's defined
            ListType = readStream.ReadTagType();

            int length = readStream.ReadInt32();
            if (length < 0) {
                throw new NbtFormatException("Negative list size given.");
            }

            switch (ListType) {
                case NbtTagType.Byte:
                    readStream.Skip(length);
                    break;
                case NbtTagType.Short:
                    readStream.Skip(length*sizeof(short));
                    break;
                case NbtTagType.Int:
                    readStream.Skip(length*sizeof(int));
                    break;
                case NbtTagType.Long:
                    readStream.Skip(length*sizeof(long));
                    break;
                case NbtTagType.Float:
                    readStream.Skip(length*sizeof(float));
                    break;
                case NbtTagType.Double:
                    readStream.Skip(length*sizeof(double));
                    break;
                default:
                    for (int i = 0; i < length; i++) {
                        switch (listType) {
                            case NbtTagType.ByteArray:
                                new NbtByteArray().SkipTag(readStream);
                                break;
                            case NbtTagType.String:
                                readStream.SkipString();
                                break;
                            case NbtTagType.List:
                                new NbtList().SkipTag(readStream);
                                break;
                            case NbtTagType.Compound:
                                new NbtCompound().SkipTag(readStream);
                                break;
                            case NbtTagType.IntArray:
                                new NbtIntArray().SkipTag(readStream);
                                break;
                        }
                    }
                    break;
            }
        }


        internal override void WriteTag(NbtBinaryWriter writeStream) {
            writeStream.Write(NbtTagType.List);
            if (Name == null) throw new NbtFormatException("Name is null");
            writeStream.Write(Name);
            WriteData(writeStream);
        }


        internal override void WriteData(NbtBinaryWriter writeStream) {
            if (ListType == NbtTagType.Unknown) {
                throw new NbtFormatException("NbtList had no elements and an Unknown ListType");
            }
            writeStream.Write(ListType);
            writeStream.Write(tags.Count);
            foreach (NbtTag tag in tags) {
                tag.WriteData(writeStream);
            }
        }

        #endregion


        #region Implementation of IEnumerable<NBtTag> and IEnumerable

        /// <summary> Returns an enumerator that iterates through all tags in this NbtList. </summary>
        /// <returns> An IEnumerator&gt;NbtTag&lt; that can be used to iterate through the list. </returns>
        public IEnumerator<NbtTag> GetEnumerator() {
            return tags.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator() {
            return tags.GetEnumerator();
        }

        #endregion


        #region Implementation of IList<NbtTag> and ICollection<NbtTag>

        /// <summary> Determines the index of a specific tag in this NbtList </summary>
        /// <returns> The index of tag if found in the list; otherwise, -1. </returns>
        /// <param name="tag"> The tag to locate in this NbtList. </param>
        public int IndexOf([CanBeNull] NbtTag tag) {
            if (tag == null) return -1;
            return tags.IndexOf(tag);
        }


        /// <summary> Inserts an item to this NbtList at the specified index. </summary>
        /// <param name="tagIndex"> The zero-based index at which newTag should be inserted. </param>
        /// <param name="newTag"> The tag to insert into this NbtList. </param>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="tagIndex"/> is not a valid index in this NbtList. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="newTag"/> is <c>null</c>. </exception>
        public void Insert(int tagIndex, [NotNull] NbtTag newTag) {
            if (newTag == null) {
                throw new ArgumentNullException("newTag");
            }
            if (listType != NbtTagType.Unknown && newTag.TagType != listType) {
                throw new ArgumentException("Items must be of type " + listType);
            } else if (newTag.Parent != null) {
                throw new ArgumentException("A tag may only be added to one compound/list at a time.");
            }
            tags.Insert(tagIndex, newTag);
            if (listType == NbtTagType.Unknown) {
                listType = newTag.TagType;
            }
            newTag.Parent = this;
        }


        /// <summary> Removes a tag at the specified index from this NbtList. </summary>
        /// <param name="index"> The zero-based index of the item to remove. </param>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="index"/> is not a valid index in the NbtList. </exception>
        public void RemoveAt(int index) {
            NbtTag tag = this[index];
            tags.RemoveAt(index);
            tag.Parent = null;
        }


        /// <summary> Adds a tag to this NbtList. </summary>
        /// <param name="newTag"> The tag to add to this NbtList. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="newTag"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> If <paramref name="newTag"/> does not match ListType. </exception>
        public void Add([NotNull] NbtTag newTag) {
            if (newTag == null) {
                throw new ArgumentNullException("newTag");
            } else if (newTag.Parent != null) {
                throw new ArgumentException("A tag may only be added to one compound/list at a time.");
            } else if (newTag == this || newTag == Parent) {
                throw new ArgumentException("A list tag may not be added to itself or to its child tag.");
            } else if (newTag.Name != null) {
                throw new ArgumentException("Named tag given. A list may only contain unnamed tags.");
            }
            if (listType != NbtTagType.Unknown && newTag.TagType != listType) {
                throw new ArgumentException("Items in this list must be of type " + listType + ". Given type: " +
                                            newTag.TagType);
            }
            tags.Add(newTag);
            newTag.Parent = this;
            if (listType == NbtTagType.Unknown) {
                listType = newTag.TagType;
            }
        }


        /// <summary> Removes all tags from this NbtList. </summary>
        public void Clear() {
            for (int i = 0; i < tags.Count; i++) {
                tags[i].Parent = null;
            }
            tags.Clear();
        }


        /// <summary> Determines whether this NbtList contains a specific tag. </summary>
        /// <returns> true if given tag is found in this NbtList; otherwise, false. </returns>
        /// <param name="item"> The tag to locate in this NbtList. </param>
        public bool Contains([NotNull] NbtTag item) {
            if (item == null) throw new ArgumentNullException("item");
            return tags.Contains(item);
        }


        /// <summary> Copies the tags of this NbtList to an array, starting at a particular array index. </summary>
        /// <param name="array"> The one-dimensional array that is the destination of the tag copied from NbtList.
        /// The array must have zero-based indexing. </param>
        /// <param name="arrayIndex"> The zero-based index in array at which copying begins. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="array"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> arrayIndex is less than 0. </exception>
        /// <exception cref="ArgumentException"> Given array is multidimensional; arrayIndex is equal to or greater than the length of array;
        /// the number of tags in this NbtList is greater than the available space from arrayIndex to the end of the destination array;
        /// or type NbtTag cannot be cast automatically to the type of the destination array. </exception>
        public void CopyTo(NbtTag[] array, int arrayIndex) {
            tags.CopyTo(array, arrayIndex);
        }


        /// <summary> Removes the first occurrence of a specific NbtTag from the NbtCompound.
        /// Looks for exact object matches, not name matches. </summary>
        /// <returns> true if tag was successfully removed from this NbtList; otherwise, false.
        /// This method also returns false if tag is not found. </returns>
        /// <param name="tag"> The tag to remove from this NbtList. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="tag"/> is <c>null</c>. </exception>
        public bool Remove([NotNull] NbtTag tag) {
            if (tag == null) throw new ArgumentNullException("tag");
            if (!tags.Remove(tag)) {
                return false;
            }
            tag.Parent = null;
            return true;
        }


        /// <summary> Gets the number of tags contained in the NbtList. </summary>
        /// <returns> The number of tags contained in the NbtList. </returns>
        public int Count {
            get { return tags.Count; }
        }

        bool ICollection<NbtTag>.IsReadOnly {
            get { return false; }
        }

        #endregion


        #region Implementation of IList and ICollection

        void IList.Remove([NotNull] object value) {
            Remove((NbtTag)value);
        }


        [NotNull]
        object IList.this[int tagIndex] {
            get { return tags[tagIndex]; }
            set { this[tagIndex] = (NbtTag)value; }
        }


        int IList.Add([NotNull] object value) {
            Add((NbtTag)value);
            return (tags.Count - 1);
        }


        bool IList.Contains([NotNull] object value) {
            return tags.Contains((NbtTag)value);
        }


        int IList.IndexOf([NotNull] object value) {
            return tags.IndexOf((NbtTag)value);
        }


        void IList.Insert(int index, [NotNull] object value) {
            Insert(index, (NbtTag)value);
        }


        bool IList.IsFixedSize {
            get { return false; }
        }


        void ICollection.CopyTo(Array array, int index) {
            CopyTo((NbtTag[])array, index);
        }


        object ICollection.SyncRoot {
            get { return (tags as ICollection).SyncRoot; }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        bool IList.IsReadOnly {
            get { return false; }
        }

        #endregion


        public override object Clone() {
            return new NbtList(this);
        }


        internal override void PrettyPrint(StringBuilder sb, string indentString, int indentLevel) {
            for (int i = 0; i < indentLevel; i++) {
                sb.Append(indentString);
            }
            sb.Append("TAG_List");
            if (!String.IsNullOrEmpty(Name)) {
                sb.AppendFormat("(\"{0}\")", Name);
            }
            sb.AppendFormat(": {0} entries {{", tags.Count);

            if (Count > 0) {
                sb.Append('\n');
                foreach (NbtTag tag in tags) {
                    tag.PrettyPrint(sb, indentString, indentLevel + 1);
                    sb.Append('\n');
                }
                for (int i = 0; i < indentLevel; i++) {
                    sb.Append(indentString);
                }
            }
            sb.Append('}');
        }
    }
}
