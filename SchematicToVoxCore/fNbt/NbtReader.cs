using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace fNbt {
    /// <summary> Represents a reader that provides fast, non-cached, forward-only access to NBT data.
    /// Each instance of NbtReader reads one complete file. </summary>
    public class NbtReader {
        NbtParseState state = NbtParseState.AtStreamBeginning;
        readonly NbtBinaryReader reader;
        Stack<NbtReaderNode> nodes;
        readonly long streamStartOffset;
        bool atValue;
        object valueCache;
        readonly bool canSeekStream;


        /// <summary> Initializes a new instance of the NbtReader class. </summary>
        /// <param name="stream"> Stream to read from. </param>
        /// <remarks> Assumes that data in the stream is Big-Endian encoded. </remarks>
        /// <exception cref="ArgumentNullException"> <paramref name="stream"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> <paramref name="stream"/> is not readable. </exception>
        public NbtReader([NotNull] Stream stream)
            : this(stream, true) {}


        /// <summary> Initializes a new instance of the NbtReader class. </summary>
        /// <param name="stream"> Stream to read from. </param>
        /// <param name="bigEndian"> Whether NBT data is in Big-Endian encoding. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="stream"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> <paramref name="stream"/> is not readable. </exception>
        public NbtReader([NotNull] Stream stream, bool bigEndian) {
            if (stream == null) throw new ArgumentNullException("stream");
            SkipEndTags = true;
            CacheTagValues = false;
            ParentTagType = NbtTagType.Unknown;
            TagType = NbtTagType.Unknown;

            canSeekStream = stream.CanSeek;
            if (canSeekStream) {
                streamStartOffset = stream.Position;
            }

            reader = new NbtBinaryReader(stream, bigEndian);
        }


        /// <summary> Gets the name of the root tag of this NBT stream. </summary>
        [CanBeNull]
        public string RootName { get; private set; }

        /// <summary> Gets the name of the parent tag. May be null (for root tags and descendants of list elements). </summary>
        [CanBeNull]
        public string ParentName { get; private set; }

        /// <summary> Gets the name of the current tag. May be null (for list elements and end tags). </summary>
        [CanBeNull]
        public string TagName { get; private set; }

        /// <summary> Gets the type of the parent tag. Returns TagType.Unknown if there is no parent tag. </summary>
        public NbtTagType ParentTagType { get; private set; }

        /// <summary> Gets the type of the current tag. </summary>
        public NbtTagType TagType { get; private set; }

        /// <summary> Whether tag that we are currently on is a list element. </summary>
        public bool IsListElement {
            get { return (ParentTagType == NbtTagType.List); }
        }

        /// <summary> Whether current tag has a value to read. </summary>
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

        /// <summary> Whether current tag has a name. </summary>
        public bool HasName {
            get { return (TagName != null); }
        }

        /// <summary> Whether this reader has reached the end of stream. </summary>
        public bool IsAtStreamEnd {
            get { return state == NbtParseState.AtStreamEnd; }
        }

        /// <summary> Whether the current tag is a Compound. </summary>
        public bool IsCompound {
            get { return (TagType == NbtTagType.Compound); }
        }

        /// <summary> Whether the current tag is a List. </summary>
        public bool IsList {
            get { return (TagType == NbtTagType.List); }
        }

        /// <summary> Whether the current tag has length (Lists, ByteArrays, and IntArrays have length).
        /// Compound tags also have length, technically, but it is not known until all child tags are read. </summary>
        public bool HasLength {
            get {
                switch (TagType) {
                    case NbtTagType.List:
                    case NbtTagType.ByteArray:
                    case NbtTagType.IntArray:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary> Gets the Stream from which data is being read. </summary>
        [NotNull]
        public Stream BaseStream {
            get { return reader.BaseStream; }
        }

        /// <summary> Gets the number of bytes from the beginning of the stream to the beginning of this tag.
        /// If the stream is not seekable, this value will always be 0. </summary>
        public int TagStartOffset { get; private set; }

        /// <summary> Gets the number of tags read from the stream so far
        /// (including the current tag and all skipped tags). 
        /// If <c>SkipEndTags</c> is <c>false</c>, all end tags are also counted. </summary>
        public int TagsRead { get; private set; }

        /// <summary> Gets the depth of the current tag in the hierarchy.
        /// <c>RootTag</c> is at depth 1, its descendant tags are 2, etc. </summary>
        public int Depth { get; private set; }

        /// <summary> If the current tag is TAG_List, returns type of the list elements. </summary>
        public NbtTagType ListType { get; private set; }

        /// <summary> If the current tag is TAG_List, TAG_Byte_Array, or TAG_Int_Array, returns the number of elements. </summary>
        public int TagLength { get; private set; }

        /// <summary> If the parent tag is TAG_List, returns the number of elements. </summary>
        public int ParentTagLength { get; private set; }

        /// <summary> If the parent tag is TAG_List, returns index of the current tag. </summary>
        public int ListIndex { get; private set; }

        /// <summary> Gets whether this NbtReader instance is in state of error.
        /// No further reading can be done from this instance if a parse error occurred. </summary>
        public bool IsInErrorState {
            get { return (state == NbtParseState.Error); }
        }


        /// <summary> Reads the next tag from the stream. </summary>
        /// <returns> true if the next tag was read successfully; false if there are no more tags to read. </returns>
        /// <exception cref="NbtFormatException"> If an error occurred while parsing data in NBT format. </exception>
        /// <exception cref="InvalidReaderStateException"> If NbtReader cannot recover from a previous parsing error. </exception>
        public bool ReadToFollowing() {
            switch (state) {
                case NbtParseState.AtStreamBeginning:
                    // set state to error in case reader.ReadTagType throws.
                    state = NbtParseState.Error;
                    // read first tag, make sure it's a compound
                    if (reader.ReadTagType() != NbtTagType.Compound) {
                        state = NbtParseState.Error;
                        throw new NbtFormatException("Given NBT stream does not start with a TAG_Compound");
                    }
                    Depth = 1;
                    TagType = NbtTagType.Compound;
                    // Read root name. Advance to the first inside tag.
                    ReadTagHeader(true);
                    RootName = TagName;
                    return true;

                case NbtParseState.AtCompoundBeginning:
                    GoDown();
                    state = NbtParseState.InCompound;
                    goto case NbtParseState.InCompound;

                case NbtParseState.InCompound:
                    if (atValue) {
                        SkipValue();
                    }
                    // Read next tag, check if we've hit the end
                    if (canSeekStream) {
                        TagStartOffset = (int)(reader.BaseStream.Position - streamStartOffset);
                    }

                    NbtParseState oldState = state;
                    // set state to error in case reader.ReadTagType throws.
                    state = NbtParseState.Error;
                    TagType = reader.ReadTagType();
                    state = oldState;

                    if (TagType == NbtTagType.End) {
                        TagName = null;
                        TagsRead++;
                        state = NbtParseState.AtCompoundEnd;
                        if (SkipEndTags) {
                            TagsRead--;
                            goto case NbtParseState.AtCompoundEnd;
                        } else {
                            return true;
                        }
                    } else {
                        ReadTagHeader(true);
                        return true;
                    }

                case NbtParseState.AtListBeginning:
                    GoDown();
                    ListIndex = -1;
                    TagType = ListType;
                    state = NbtParseState.InList;
                    goto case NbtParseState.InList;

                case NbtParseState.InList:
                    if (atValue) {
                        SkipValue();
                    }
                    ListIndex++;
                    if (ListIndex >= ParentTagLength) {
                        GoUp();
                        if (ParentTagType == NbtTagType.List) {
                            state = NbtParseState.InList;
                            TagType = NbtTagType.List;
                            goto case NbtParseState.InList;
                        } else if (ParentTagType == NbtTagType.Compound) {
                            state = NbtParseState.InCompound;
                            goto case NbtParseState.InCompound;
                        } else {
                            // This should not happen unless NbtReader is bugged
                            state = NbtParseState.Error;
                            throw new NbtFormatException(InvalidParentTagError);
                        }
                    } else {
                        if (canSeekStream) {
                            TagStartOffset = (int)(reader.BaseStream.Position - streamStartOffset);
                        }
                        ReadTagHeader(false);
                    }
                    return true;

                case NbtParseState.AtCompoundEnd:
                    GoUp();
                    if (ParentTagType == NbtTagType.List) {
                        state = NbtParseState.InList;
                        TagType = NbtTagType.Compound;
                        goto case NbtParseState.InList;
                    } else if (ParentTagType == NbtTagType.Compound) {
                        state = NbtParseState.InCompound;
                        goto case NbtParseState.InCompound;
                    } else if (ParentTagType == NbtTagType.Unknown) {
                        state = NbtParseState.AtStreamEnd;
                        return false;
                    } else {
                        // This should not happen unless NbtReader is bugged
                        state = NbtParseState.Error;
                        throw new NbtFormatException(InvalidParentTagError);
                    }

                case NbtParseState.AtStreamEnd:
                    // nothing left to read!
                    return false;

                default:
                    // Parsing error, or unexpected state.
                    throw new InvalidReaderStateException(ErroneousStateError);
            }
        }


        void ReadTagHeader(bool readName) {
            TagsRead++;
            TagName = (readName ? reader.ReadString() : null);

            valueCache = null;
            TagLength = 0;
            atValue = false;
            ListType = NbtTagType.Unknown;

            switch (TagType) {
                case NbtTagType.Byte:
                case NbtTagType.Short:
                case NbtTagType.Int:
                case NbtTagType.Long:
                case NbtTagType.Float:
                case NbtTagType.Double:
                case NbtTagType.String:
                    atValue = true;
                    break;

                case NbtTagType.IntArray:
                case NbtTagType.ByteArray:
                    TagLength = reader.ReadInt32();
                    atValue = true;
                    break;

                case NbtTagType.List:
                    // Setting state to error in case reader throws
                    state = NbtParseState.Error;
                    ListType = reader.ReadTagType();
                    TagLength = reader.ReadInt32();
                    if (TagLength < 0) {
                        throw new NbtFormatException("Negative tag length given: " + TagLength);
                    }
                    state = NbtParseState.AtListBeginning;
                    break;

                case NbtTagType.Compound:
                    state = NbtParseState.AtCompoundBeginning;
                    break;

                default:
                    state = NbtParseState.Error;
                    throw new NbtFormatException("Trying to read tag of unknown type.");
            }
        }


        // Goes one step down the NBT file's hierarchy, preserving current state
        void GoDown() {
            if (nodes == null) nodes = new Stack<NbtReaderNode>();
            var newNode = new NbtReaderNode {
                ListIndex = ListIndex,
                ParentTagLength = ParentTagLength,
                ParentName = ParentName,
                ParentTagType = ParentTagType,
                ListType = ListType
            };
            nodes.Push(newNode);

            ParentName = TagName;
            ParentTagType = TagType;
            ParentTagLength = TagLength;
            ListIndex = 0;
            TagLength = 0;

            Depth++;
        }


        // Goes one step up the NBT file's hierarchy, restoring previous state
        void GoUp() {
            NbtReaderNode oldNode = nodes.Pop();

            ParentName = oldNode.ParentName;
            ParentTagType = oldNode.ParentTagType;
            ParentTagLength = oldNode.ParentTagLength;
            ListIndex = oldNode.ListIndex;
            ListType = oldNode.ListType;
            TagLength = 0;

            Depth--;
        }


        void SkipValue() {
            // Make sure to check for "atValue" before calling this method
            switch (TagType) {
                case NbtTagType.Byte:
                    reader.ReadByte();
                    break;

                case NbtTagType.Short:
                    reader.ReadInt16();
                    break;

                case NbtTagType.Float:
                case NbtTagType.Int:
                    reader.ReadInt32();
                    break;

                case NbtTagType.Double:
                case NbtTagType.Long:
                    reader.ReadInt64();
                    break;

                case NbtTagType.ByteArray:
                    reader.Skip(TagLength);
                    break;

                case NbtTagType.IntArray:
                    reader.Skip(sizeof(int)*TagLength);
                    break;

                case NbtTagType.String:
                    reader.SkipString();
                    break;

                default:
                    throw new InvalidOperationException(NonValueTagError);
            }
            atValue = false;
            valueCache = null;
        }


        /// <summary> Reads until a tag with the specified name is found. 
        /// Returns false if are no more tags to read (end of stream is reached). </summary>
        /// <param name="tagName"> Name of the tag. May be null (to look for next unnamed tag). </param>
        /// <returns> <c>true</c> if a matching tag is found; otherwise <c>false</c>. </returns>
        /// <exception cref="NbtFormatException"> If an error occurred while parsing data in NBT format. </exception>
        /// <exception cref="InvalidOperationException"> If NbtReader cannot recover from a previous parsing error. </exception>
        public bool ReadToFollowing([CanBeNull] string tagName) {
            while (ReadToFollowing()) {
                if (TagName == tagName) {
                    return true;
                }
            }
            return false;
        }


        /// <summary> Advances the NbtReader to the next descendant tag with the specified name.
        /// If a matching child tag is not found, the NbtReader is positioned on the end tag. </summary>
        /// <param name="tagName"> Name of the tag you wish to move to. May be null (to look for next unnamed tag). </param>
        /// <returns> <c>true</c> if a matching descendant tag is found; otherwise <c>false</c>. </returns>
        /// <exception cref="NbtFormatException"> If an error occurred while parsing data in NBT format. </exception>
        /// <exception cref="InvalidReaderStateException"> If NbtReader cannot recover from a previous parsing error. </exception>
        public bool ReadToDescendant([CanBeNull] string tagName) {
            if (state == NbtParseState.Error) {
                throw new InvalidReaderStateException(ErroneousStateError);
            } else if (state == NbtParseState.AtStreamEnd) {
                return false;
            }
            int currentDepth = Depth;
            while (ReadToFollowing()) {
                if (Depth <= currentDepth) {
                    return false;
                } else if (TagName == tagName) {
                    return true;
                }
            }
            return false;
        }


        /// <summary> Advances the NbtReader to the next sibling tag, skipping any child tags.
        /// If there are no more siblings, NbtReader is positioned on the tag following the last of this tag's descendants. </summary>
        /// <returns> <c>true</c> if a sibling element is found; otherwise <c>false</c>. </returns>
        /// <exception cref="NbtFormatException"> If an error occurred while parsing data in NBT format. </exception>
        /// <exception cref="InvalidReaderStateException"> If NbtReader cannot recover from a previous parsing error. </exception>
        public bool ReadToNextSibling() {
            if (state == NbtParseState.Error) {
                throw new InvalidReaderStateException(ErroneousStateError);
            } else if (state == NbtParseState.AtStreamEnd) {
                return false;
            }
            int currentDepth = Depth;
            while (ReadToFollowing()) {
                if (Depth == currentDepth) {
                    return true;
                } else if (Depth < currentDepth) {
                    return false;
                }
            }
            return false;
        }


        /// <summary> Advances the NbtReader to the next sibling tag with the specified name.
        /// If a matching sibling tag is not found, NbtReader is positioned on the tag following the last siblings. </summary>
        /// <param name="tagName"> The name of the sibling tag you wish to move to. </param>
        /// <returns> <c>true</c> if a matching sibling element is found; otherwise <c>false</c>. </returns>
        /// <exception cref="NbtFormatException"> If an error occurred while parsing data in NBT format. </exception>
        /// <exception cref="InvalidOperationException"> If NbtReader cannot recover from a previous parsing error. </exception>
        public bool ReadToNextSibling([CanBeNull] string tagName) {
            while (ReadToNextSibling()) {
                if (TagName == tagName) {
                    return true;
                }
            }
            return false;
        }


        /// <summary> Skips current tag, its value/descendants, and any following siblings.
        /// In other words, reads until parent tag's sibling. </summary>
        /// <returns> Total number of tags that were skipped. Returns 0 if end of the stream is reached. </returns>
        /// <exception cref="NbtFormatException"> If an error occurred while parsing data in NBT format. </exception>
        /// <exception cref="InvalidReaderStateException"> If NbtReader cannot recover from a previous parsing error. </exception>
        public int Skip() {
            if (state == NbtParseState.Error) {
                throw new InvalidReaderStateException(ErroneousStateError);
            } else if (state == NbtParseState.AtStreamEnd) {
                return 0;
            }
            int startDepth = Depth;
            int skipped = 0;
            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (ReadToFollowing() && Depth >= startDepth) {
                skipped++;
            }
            return skipped;
        }


        /// <summary> Reads the entirety of the current tag, including any descendants,
        /// and constructs an NbtTag object of the appropriate type. </summary>
        /// <returns> Constructed NbtTag object;
        /// <c>null</c> if <c>SkipEndTags</c> is <c>true</c> and trying to read an End tag. </returns>
        /// <exception cref="NbtFormatException"> If an error occurred while parsing data in NBT format. </exception>
        /// <exception cref="InvalidReaderStateException"> If NbtReader cannot recover from a previous parsing error. </exception>
        /// <exception cref="EndOfStreamException"> End of stream has been reached (no more tags can be read). </exception>
        /// <exception cref="InvalidOperationException"> Tag value has already been read, and CacheTagValues is false. </exception>
        [NotNull]
        public NbtTag ReadAsTag() {
            switch (state) {
                case NbtParseState.Error:
                    throw new InvalidReaderStateException(ErroneousStateError);

                case NbtParseState.AtStreamEnd:
                    throw new EndOfStreamException();

                case NbtParseState.AtStreamBeginning:
                case NbtParseState.AtCompoundEnd:
                    ReadToFollowing();
                    break;
            }

            // get this tag
            NbtTag parent;
            if (TagType == NbtTagType.Compound) {
                parent = new NbtCompound(TagName);
            } else if (TagType == NbtTagType.List) {
                parent = new NbtList(TagName, ListType);
            } else if (atValue) {
                NbtTag result = ReadValueAsTag();
                ReadToFollowing();
                // if we're at a value tag, there are no child tags to read
                return result;
            } else {
                // end tags cannot be read-as-tags (there is no corresponding NbtTag object)
                throw new InvalidOperationException(NoValueToReadError);
            }

            int startingDepth = Depth;
            int parentDepth = Depth;

            do {
                ReadToFollowing();
                // Going up the file tree, or end of document: wrap up
                while (Depth <= parentDepth && parent.Parent != null) {
                    parent = parent.Parent;
                    parentDepth--;
                }
                if (Depth <= startingDepth) break;

                NbtTag thisTag;
                if (TagType == NbtTagType.Compound) {
                    thisTag = new NbtCompound(TagName);
                    AddToParent(thisTag, parent);
                    parent = thisTag;
                    parentDepth = Depth;
                } else if (TagType == NbtTagType.List) {
                    thisTag = new NbtList(TagName, ListType);
                    AddToParent(thisTag, parent);
                    parent = thisTag;
                    parentDepth = Depth;
                } else if (TagType != NbtTagType.End) {
                    thisTag = ReadValueAsTag();
                    AddToParent(thisTag, parent);
                }
            } while (true);

            return parent;
        }


        void AddToParent([NotNull] NbtTag thisTag, [NotNull] NbtTag parent) {
            var parentAsList = parent as NbtList;
            if (parentAsList != null) {
                parentAsList.Add(thisTag);
            } else {
                var parentAsCompound = parent as NbtCompound;
                if (parentAsCompound != null) {
                    parentAsCompound.Add(thisTag);
                } else {
                    // cannot happen unless NbtReader is bugged
                    throw new NbtFormatException(InvalidParentTagError);
                }
            }
        }


        [NotNull]
        NbtTag ReadValueAsTag() {
            if (!atValue) {
                // Should never happen
                throw new InvalidOperationException(NoValueToReadError);
            }
            atValue = false;
            switch (TagType) {
                case NbtTagType.Byte:
                    return new NbtByte(TagName, reader.ReadByte());

                case NbtTagType.Short:
                    return new NbtShort(TagName, reader.ReadInt16());

                case NbtTagType.Int:
                    return new NbtInt(TagName, reader.ReadInt32());

                case NbtTagType.Long:
                    return new NbtLong(TagName, reader.ReadInt64());

                case NbtTagType.Float:
                    return new NbtFloat(TagName, reader.ReadSingle());

                case NbtTagType.Double:
                    return new NbtDouble(TagName, reader.ReadDouble());

                case NbtTagType.String:
                    return new NbtString(TagName, reader.ReadString());

                case NbtTagType.ByteArray:
                    byte[] value = reader.ReadBytes(TagLength);
                    if (value.Length < TagLength) {
                        throw new EndOfStreamException();
                    }
                    return new NbtByteArray(TagName, value);

                case NbtTagType.IntArray:
                    var ints = new int[TagLength];
                    for (int i = 0; i < TagLength; i++) {
                        ints[i] = reader.ReadInt32();
                    }
                    return new NbtIntArray(TagName, ints);

                default:
                    throw new InvalidOperationException(NonValueTagError);
            }
        }


        /// <summary> Reads the value as an object of the type specified. </summary>
        /// <typeparam name="T"> The type of the value to be returned.
        /// Tag value should be convertible to this type. </typeparam>
        /// <returns> Tag value converted to the requested type. </returns>
        /// <exception cref="EndOfStreamException"> End of stream has been reached (no more tags can be read). </exception>
        /// <exception cref="NbtFormatException"> If an error occurred while parsing data in NBT format. </exception>
        /// <exception cref="InvalidOperationException"> Value has already been read, or there is no value to read. </exception>
        /// <exception cref="InvalidReaderStateException"> If NbtReader cannot recover from a previous parsing error. </exception>
        /// <exception cref="InvalidCastException"> Tag value cannot be converted to the requested type. </exception>
        public T ReadValueAs<T>() {
            return (T)ReadValue();
        }


        /// <summary> Reads the value as an object of the correct type, boxed.
        /// Cannot be called for tags that do not have a single-object value (compound, list, and end tags). </summary>
        /// <returns> Tag value converted to the requested type. </returns>
        /// <exception cref="EndOfStreamException"> End of stream has been reached (no more tags can be read). </exception>
        /// <exception cref="NbtFormatException"> If an error occurred while parsing data in NBT format. </exception>
        /// <exception cref="InvalidOperationException"> Value has already been read, or there is no value to read. </exception>
        /// <exception cref="InvalidReaderStateException"> If NbtReader cannot recover from a previous parsing error. </exception>
        [NotNull]
        public object ReadValue() {
            if (state == NbtParseState.AtStreamEnd) {
                throw new EndOfStreamException();
            }
            if (!atValue) {
                if (cacheTagValues) {
                    if (valueCache == null) {
                        throw new InvalidOperationException("No value to read.");
                    } else {
                        return valueCache;
                    }
                } else {
                    throw new InvalidOperationException(NoValueToReadError);
                }
            }
            valueCache = null;
            atValue = false;
            object value;
            switch (TagType) {
                case NbtTagType.Byte:
                    value = reader.ReadByte();
                    break;

                case NbtTagType.Short:
                    value = reader.ReadInt16();
                    break;

                case NbtTagType.Float:
                    value = reader.ReadSingle();
                    break;

                case NbtTagType.Int:
                    value = reader.ReadInt32();
                    break;

                case NbtTagType.Double:
                    value = reader.ReadDouble();
                    break;

                case NbtTagType.Long:
                    value = reader.ReadInt64();
                    break;

                case NbtTagType.ByteArray:
                    byte[] valueArr = reader.ReadBytes(TagLength);
                    if (valueArr.Length < TagLength) {
                        throw new EndOfStreamException();
                    }
                    value = valueArr;
                    break;

                case NbtTagType.IntArray:
                    var intValue = new int[TagLength];
                    for (int i = 0; i < TagLength; i++) {
                        intValue[i] = reader.ReadInt32();
                    }
                    value = intValue;
                    break;

                case NbtTagType.String:
                    value = reader.ReadString();
                    break;

                default:
                    throw new InvalidOperationException(NonValueTagError);
            }
            valueCache = cacheTagValues ? value : null;
            return value;
        }


        /// <summary> If the current tag is a List, reads all elements of this list as an array.
        /// If any tags/values have already been read from this list, only reads the remaining unread tags/values.
        /// ListType must be a value type (byte, short, int, long, float, double, or string).
        /// Stops reading after the last list element. </summary>
        /// <typeparam name="T"> Element type of the array to be returned.
        /// Tag contents should be convertible to this type. </typeparam>
        /// <returns> List contents converted to an array of the requested type. </returns>
        /// <exception cref="EndOfStreamException"> End of stream has been reached (no more tags can be read). </exception>
        /// <exception cref="InvalidOperationException"> Current tag is not of type List. </exception>
        /// <exception cref="InvalidReaderStateException"> If NbtReader cannot recover from a previous parsing error. </exception>
        /// <exception cref="NbtFormatException"> If an error occurred while parsing data in NBT format. </exception>
        [NotNull]
        public T[] ReadListAsArray<T>() {
            switch (state) {
                case NbtParseState.AtStreamEnd:
                    throw new EndOfStreamException();
                case NbtParseState.Error:
                    throw new InvalidReaderStateException(ErroneousStateError);
                case NbtParseState.AtListBeginning:
                    GoDown();
                    ListIndex = 0;
                    TagType = ListType;
                    state = NbtParseState.InList;
                    break;
                case NbtParseState.InList:
                    break;
                default:
                    throw new InvalidOperationException("ReadListAsArray may only be used on List tags.");
            }

            int elementsToRead = ParentTagLength - ListIndex;

            // special handling for reading byte arrays (as byte arrays)
            if (ListType == NbtTagType.Byte && typeof(T) == typeof(byte)) {
                TagsRead += elementsToRead;
                ListIndex = ParentTagLength - 1;
                T[] val = (T[])(object)reader.ReadBytes(elementsToRead);
                if (val.Length < elementsToRead) {
                    throw new EndOfStreamException();
                }
                return val;
            }

            // for everything else, gotta read elements one-by-one
            var result = new T[elementsToRead];
            switch (ListType) {
                case NbtTagType.Byte:
                    for (int i = 0; i < elementsToRead; i++) {
                        result[i] = (T)Convert.ChangeType(reader.ReadByte(), typeof(T));
                    }
                    break;

                case NbtTagType.Short:
                    for (int i = 0; i < elementsToRead; i++) {
                        result[i] = (T)Convert.ChangeType(reader.ReadInt16(), typeof(T));
                    }
                    break;

                case NbtTagType.Int:
                    for (int i = 0; i < elementsToRead; i++) {
                        result[i] = (T)Convert.ChangeType(reader.ReadInt32(), typeof(T));
                    }
                    break;

                case NbtTagType.Long:
                    for (int i = 0; i < elementsToRead; i++) {
                        result[i] = (T)Convert.ChangeType(reader.ReadInt64(), typeof(T));
                    }
                    break;

                case NbtTagType.Float:
                    for (int i = 0; i < elementsToRead; i++) {
                        result[i] = (T)Convert.ChangeType(reader.ReadSingle(), typeof(T));
                    }
                    break;

                case NbtTagType.Double:
                    for (int i = 0; i < elementsToRead; i++) {
                        result[i] = (T)Convert.ChangeType(reader.ReadDouble(), typeof(T));
                    }
                    break;

                case NbtTagType.String:
                    for (int i = 0; i < elementsToRead; i++) {
                        result[i] = (T)Convert.ChangeType(reader.ReadString(), typeof(T));
                    }
                    break;

                default:
                    throw new InvalidOperationException("ReadListAsArray may only be used on lists of value types.");
            }
            TagsRead += elementsToRead;
            ListIndex = ParentTagLength - 1;
            return result;
        }


        /// <summary> Parsing option: Whether NbtReader should skip End tags in ReadToFollowing() automatically while parsing.
        /// Default is <c>true</c>. </summary>
        public bool SkipEndTags { get; set; }

        /// <summary> Parsing option: Whether NbtReader should save a copy of the most recently read tag's value.
        /// Unless CacheTagValues is <c>true</c>, tag values can only be read once. Default is <c>false</c>. </summary>
        public bool CacheTagValues {
            get { return cacheTagValues; }
            set {
                cacheTagValues = value;
                if (!cacheTagValues) {
                    valueCache = null;
                }
            }
        }

        bool cacheTagValues;


        /// <summary> Returns a String that represents the tag currently being read by this NbtReader instance.
        /// Prints current tag's depth, ordinal number, type, name, and size (for arrays and lists). Does not print value.
        /// Indents the tag according default indentation (NbtTag.DefaultIndentString). </summary>
        public override string ToString() {
            return ToString(false, NbtTag.DefaultIndentString);
        }


        /// <summary> Returns a String that represents the tag currently being read by this NbtReader instance.
        /// Prints current tag's depth, ordinal number, type, name, size (for arrays and lists), and optionally value.
        /// Indents the tag according default indentation (NbtTag.DefaultIndentString). </summary>
        /// <param name="includeValue"> If set to <c>true</c>, also reads and prints the current tag's value. 
        /// Note that unless CacheTagValues is set to <c>true</c>, you can only read every tag's value ONCE. </param>
        [NotNull]
        public string ToString(bool includeValue) {
            return ToString(includeValue, NbtTag.DefaultIndentString);
        }


        /// <summary> Returns a String that represents the current NbtReader object.
        /// Prints current tag's depth, ordinal number, type, name, size (for arrays and lists), and optionally value. </summary>
        /// <param name="indentString"> String to be used for indentation. May be empty string, but may not be <c>null</c>. </param>
        /// <param name="includeValue"> If set to <c>true</c>, also reads and prints the current tag's value. </param>
        [NotNull]
        public string ToString(bool includeValue, [NotNull] string indentString) {
            if (indentString == null) throw new ArgumentNullException("indentString");
            var sb = new StringBuilder();
            for (int i = 0; i < Depth; i++) {
                sb.Append(indentString);
            }
            sb.Append('#').Append(TagsRead).Append(". ").Append(TagType);
            if (IsList) {
                sb.Append('<').Append(ListType).Append('>');
            }
            if (HasLength) {
                sb.Append('[').Append(TagLength).Append(']');
            }
            sb.Append(' ').Append(TagName);
            if (includeValue && (atValue || HasValue && cacheTagValues) && TagType != NbtTagType.IntArray &&
                TagType != NbtTagType.ByteArray) {
                sb.Append(" = ").Append(ReadValue());
            }
            return sb.ToString();
        }


        const string NoValueToReadError = "Value aready read, or no value to read.",
            NonValueTagError = "Trying to read value of a non-value tag.",
            InvalidParentTagError = "Parent tag is neither a Compound nor a List.",
            ErroneousStateError = "NbtReader is in an erroneous state!";
    }
}
