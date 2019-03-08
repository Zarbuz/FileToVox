using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;

namespace fNbt {
    /// <summary> An efficient writer for writing NBT data directly to streams.
    /// Each instance of NbtWriter writes one complete file. 
    /// NbtWriter enforces all constraints of the NBT file format
    /// EXCEPT checking for duplicate tag names within a compound. </summary>
    public sealed class NbtWriter {
        const int MaxStreamCopyBufferSize = 8*1024;

        readonly NbtBinaryWriter writer;
        NbtTagType listType;
        NbtTagType parentType;
        int listIndex;
        int listSize;
        Stack<NbtWriterNode> nodes;


        /// <summary> Initializes a new instance of the NbtWriter class. </summary>
        /// <param name="stream"> Stream to write to. </param>
        /// <param name="rootTagName"> Name to give to the root tag (written immediately). </param>
        /// <remarks> Assumes that data in the stream should be Big-Endian encoded. </remarks>
        /// <exception cref="ArgumentNullException"> <paramref name="stream"/> or <paramref name="rootTagName"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> <paramref name="stream"/> is not writable. </exception>
        public NbtWriter([NotNull] Stream stream, [NotNull] String rootTagName)
            : this(stream, rootTagName, true) {}


        /// <summary> Initializes a new instance of the NbtWriter class. </summary>
        /// <param name="stream"> Stream to write to. </param>
        /// <param name="rootTagName"> Name to give to the root tag (written immediately). </param>
        /// <param name="bigEndian"> Whether NBT data should be in Big-Endian encoding. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="stream"/> or <paramref name="rootTagName"/> is <c>null</c>. </exception>
        /// <exception cref="ArgumentException"> <paramref name="stream"/> is not writable. </exception>
        public NbtWriter([NotNull] Stream stream, [NotNull] String rootTagName, bool bigEndian) {
            if (rootTagName == null) throw new ArgumentNullException("rootTagName");
            writer = new NbtBinaryWriter(stream, bigEndian);
            writer.Write((byte)NbtTagType.Compound);
            writer.Write(rootTagName);
            parentType = NbtTagType.Compound;
        }


        /// <summary> Gets whether the root tag has been closed.
        /// No more tags may be written after the root tag has been closed. </summary>
        public bool IsDone { get; private set; }

        /// <summary> Gets the underlying stream of the NbtWriter. </summary>
        [NotNull]
        public Stream BaseStream {
            get { return writer.BaseStream; }
        }


        #region Compounds and Lists

        /// <summary> Begins an unnamed compound tag. </summary>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named compound tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        public void BeginCompound() {
            EnforceConstraints(null, NbtTagType.Compound);
            GoDown(NbtTagType.Compound);
        }


        /// <summary> Begins a named compound tag. </summary>
        /// <param name="tagName"> Name to give to this compound tag. May not be null. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed compound tag was expected -OR- a tag of a different type was expected. </exception>
        public void BeginCompound([NotNull] String tagName) {
            EnforceConstraints(tagName, NbtTagType.Compound);
            GoDown(NbtTagType.Compound);

            writer.Write((byte)NbtTagType.Compound);
            writer.Write(tagName);
        }


        /// <summary> Ends a compound tag. </summary>
        /// <exception cref="NbtFormatException"> Not currently in a compound. </exception>
        public void EndCompound() {
            if (IsDone || parentType != NbtTagType.Compound) {
                throw new NbtFormatException("Not currently in a compound.");
            }
            GoUp();
            writer.Write(NbtTagType.End);
        }


        /// <summary> Begins an unnamed list tag. </summary>
        /// <param name="elementType"> Type of elements of this list. </param>
        /// <param name="size"> Number of elements in this list. Must not be negative. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named list tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="size"/> is negative -OR-
        /// <paramref name="elementType"/> is not a valid NbtTagType. </exception>
        public void BeginList(NbtTagType elementType, int size) {
            if (size < 0) {
                throw new ArgumentOutOfRangeException("size", "List size may not be negative.");
            }
            if (elementType < NbtTagType.Byte || elementType > NbtTagType.IntArray) {
                throw new ArgumentOutOfRangeException("elementType");
            }
            EnforceConstraints(null, NbtTagType.List);
            GoDown(NbtTagType.List);
            listType = elementType;
            listSize = size;

            writer.Write((byte)elementType);
            writer.Write(size);
        }


        /// <summary> Begins an unnamed list tag. </summary>
        /// <param name="tagName"> Name to give to this compound tag. May not be null. </param>
        /// <param name="elementType"> Type of elements of this list. </param>
        /// <param name="size"> Number of elements in this list. Must not be negative. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed list tag was expected -OR- a tag of a different type was expected. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="size"/> is negative -OR-
        /// <paramref name="elementType"/> is not a valid NbtTagType. </exception>
        public void BeginList([NotNull] String tagName, NbtTagType elementType, int size) {
            if (size < 0) {
                throw new ArgumentOutOfRangeException("size", "List size may not be negative.");
            }
            if (elementType < NbtTagType.Byte || elementType > NbtTagType.IntArray) {
                throw new ArgumentOutOfRangeException("elementType");
            }
            EnforceConstraints(tagName, NbtTagType.List);
            GoDown(NbtTagType.List);
            listType = elementType;
            listSize = size;

            writer.Write((byte)NbtTagType.List);
            writer.Write(tagName);
            writer.Write((byte)elementType);
            writer.Write(size);
        }


        /// <summary> Ends a list tag. </summary>
        /// <exception cref="NbtFormatException"> Not currently in a list -OR-
        /// not all list elements have been written yet. </exception>
        public void EndList() {
            if (parentType != NbtTagType.List || IsDone) {
                throw new NbtFormatException("Not currently in a list.");
            } else if (listIndex < listSize) {
                throw new NbtFormatException("Cannot end list: not all list elements have been written yet. " +
                                             "Expected: " + listSize + ", written: " + listIndex);
            }
            GoUp();
        }

        #endregion


        #region Value Tags

        /// <summary> Writes an unnamed byte tag. </summary>
        /// <param name="value"> The unsigned byte to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named byte tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        public void WriteByte(byte value) {
            EnforceConstraints(null, NbtTagType.Byte);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed byte tag. </summary>
        /// <param name="tagName"> Name to give to this compound tag. May not be null. </param>
        /// <param name="value"> The unsigned byte to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed byte tag was expected -OR- a tag of a different type was expected. </exception>
        public void WriteByte([NotNull] String tagName, byte value) {
            EnforceConstraints(tagName, NbtTagType.Byte);
            writer.Write((byte)NbtTagType.Byte);
            writer.Write(tagName);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed double tag. </summary>
        /// <param name="value"> The eight-byte floating-point value to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named double tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        public void WriteDouble(double value) {
            EnforceConstraints(null, NbtTagType.Double);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed byte tag. </summary>
        /// <param name="tagName"> Name to give to this compound tag. May not be null. </param>
        /// <param name="value"> The unsigned byte to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed byte tag was expected -OR- a tag of a different type was expected. </exception>
        public void WriteDouble([NotNull] String tagName, double value) {
            EnforceConstraints(tagName, NbtTagType.Double);
            writer.Write((byte)NbtTagType.Double);
            writer.Write(tagName);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed float tag. </summary>
        /// <param name="value"> The four-byte floating-point value to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named float tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        public void WriteFloat(float value) {
            EnforceConstraints(null, NbtTagType.Float);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed float tag. </summary>
        /// <param name="tagName"> Name to give to this compound tag. May not be null. </param>
        /// <param name="value"> The four-byte floating-point value to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed float tag was expected -OR- a tag of a different type was expected. </exception>
        public void WriteFloat([NotNull] String tagName, float value) {
            EnforceConstraints(tagName, NbtTagType.Float);
            writer.Write((byte)NbtTagType.Float);
            writer.Write(tagName);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed int tag. </summary>
        /// <param name="value"> The four-byte signed integer to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named int tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        public void WriteInt(int value) {
            EnforceConstraints(null, NbtTagType.Int);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed int tag. </summary>
        /// <param name="tagName"> Name to give to this compound tag. May not be null. </param>
        /// <param name="value"> The four-byte signed integer to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed int tag was expected -OR- a tag of a different type was expected. </exception>
        public void WriteInt([NotNull] String tagName, int value) {
            EnforceConstraints(tagName, NbtTagType.Int);
            writer.Write((byte)NbtTagType.Int);
            writer.Write(tagName);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed long tag. </summary>
        /// <param name="value"> The eight-byte signed integer to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named long tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        public void WriteLong(long value) {
            EnforceConstraints(null, NbtTagType.Long);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed long tag. </summary>
        /// <param name="tagName"> Name to give to this compound tag. May not be null. </param>
        /// <param name="value"> The eight-byte signed integer to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed long tag was expected -OR- a tag of a different type was expected. </exception>
        public void WriteLong([NotNull] String tagName, long value) {
            EnforceConstraints(tagName, NbtTagType.Long);
            writer.Write((byte)NbtTagType.Long);
            writer.Write(tagName);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed short tag. </summary>
        /// <param name="value"> The two-byte signed integer to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named short tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        public void WriteShort(short value) {
            EnforceConstraints(null, NbtTagType.Short);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed short tag. </summary>
        /// <param name="tagName"> Name to give to this compound tag. May not be null. </param>
        /// <param name="value"> The two-byte signed integer to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed short tag was expected -OR- a tag of a different type was expected. </exception>
        public void WriteShort([NotNull] String tagName, short value) {
            EnforceConstraints(tagName, NbtTagType.Short);
            writer.Write((byte)NbtTagType.Short);
            writer.Write(tagName);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed string tag. </summary>
        /// <param name="value"> The string to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named string tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        public void WriteString([NotNull] String value) {
            if (value == null) throw new ArgumentNullException("value");
            EnforceConstraints(null, NbtTagType.String);
            writer.Write(value);
        }


        /// <summary> Writes an unnamed string tag. </summary>
        /// <param name="tagName"> Name to give to this compound tag. May not be null. </param>
        /// <param name="value"> The string to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed string tag was expected -OR- a tag of a different type was expected. </exception>
        public void WriteString([NotNull] String tagName, [NotNull] String value) {
            if (value == null) throw new ArgumentNullException("value");
            EnforceConstraints(tagName, NbtTagType.String);
            writer.Write((byte)NbtTagType.String);
            writer.Write(tagName);
            writer.Write(value);
        }

        #endregion


        #region ByteArray and IntArray

        /// <summary> Writes an unnamed byte array tag, copying data from an array. </summary>
        /// <param name="data"> A byte array containing the data to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named byte array tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="data"/> is null </exception>
        public void WriteByteArray([NotNull] byte[] data) {
            if (data == null) throw new ArgumentNullException("data");
            WriteByteArray(data, 0, data.Length);
        }


        /// <summary> Writes an unnamed byte array tag, copying data from an array. </summary>
        /// <param name="data"> A byte array containing the data to write. </param>
        /// <param name="offset"> The starting point in <paramref name="data"/> at which to begin writing. Must not be negative. </param>
        /// <param name="count"> The number of bytes to write. Must not be negative. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named byte array tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="offset"/> or
        /// <paramref name="count"/> is negative. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="data"/> is null </exception>
        /// <exception cref="ArgumentException"> <paramref name="count"/> is greater than
        /// <paramref name="offset"/> subtracted from the array length. </exception>
        public void WriteByteArray([NotNull] byte[] data, int offset, int count) {
            CheckArray(data, offset, count);
            EnforceConstraints(null, NbtTagType.ByteArray);
            writer.Write(count);
            writer.Write(data, offset, count);
        }


        /// <summary> Writes a named byte array tag, copying data from an array. </summary>
        /// <param name="tagName"> Name to give to this byte array tag. May not be null. </param>
        /// <param name="data"> A byte array containing the data to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed byte array tag was expected -OR- a tag of a different type was expected. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="tagName"/> or
        /// <paramref name="data"/> is null </exception>
        public void WriteByteArray([NotNull] String tagName, [NotNull] byte[] data) {
            if (data == null) throw new ArgumentNullException("data");
            WriteByteArray(tagName, data, 0, data.Length);
        }


        /// <summary> Writes a named byte array tag, copying data from an array. </summary>
        /// <param name="tagName"> Name to give to this byte array tag. May not be null. </param>
        /// <param name="data"> A byte array containing the data to write. </param>
        /// <param name="offset"> The starting point in <paramref name="data"/> at which to begin writing. Must not be negative. </param>
        /// <param name="count"> The number of bytes to write. Must not be negative. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed byte array tag was expected -OR- a tag of a different type was expected. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="offset"/> or
        /// <paramref name="count"/> is negative. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="tagName"/> or
        /// <paramref name="data"/> is null </exception>
        /// <exception cref="ArgumentException"> <paramref name="count"/> is greater than
        /// <paramref name="offset"/> subtracted from the array length. </exception>
        public void WriteByteArray([NotNull] String tagName, [NotNull] byte[] data, int offset, int count) {
            CheckArray(data, offset, count);
            EnforceConstraints(tagName, NbtTagType.ByteArray);
            writer.Write((byte)NbtTagType.ByteArray);
            writer.Write(tagName);
            writer.Write(count);
            writer.Write(data, offset, count);
        }


        /// <summary> Writes an unnamed byte array tag, copying data from a stream. </summary>
        /// <remarks> A temporary buffer will be allocated, of size up to 8192 bytes.
        /// To manually specify a buffer, use one of the other WriteByteArray() overloads. </remarks>
        /// <param name="dataSource"> A Stream from which data will be copied. </param>
        /// <param name="count"> The number of bytes to write. Must not be negative. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named byte array tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="count"/> is negative. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="dataSource"/> is null. </exception>
        /// <exception cref="ArgumentException"> Given stream does not support reading. </exception>
        public void WriteByteArray([NotNull] Stream dataSource, int count) {
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if (!dataSource.CanRead) {
                throw new ArgumentException("Given stream does not support reading.", "dataSource");
            } else if (count < 0) {
                throw new ArgumentOutOfRangeException("count", "count may not be negative");
            }
            int bufferSize = Math.Min(count, MaxStreamCopyBufferSize);
            var streamCopyBuffer = new byte[bufferSize];
            WriteByteArray(dataSource, count, streamCopyBuffer);
        }


        /// <summary> Writes an unnamed byte array tag, copying data from a stream. </summary>
        /// <param name="dataSource"> A Stream from which data will be copied. </param>
        /// <param name="count"> The number of bytes to write. Must not be negative. </param>
        /// <param name="buffer"> Buffer to use for copying. Size must be greater than 0. Must not be null. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named byte array tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="count"/> is negative. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="dataSource"/> is null. </exception>
        /// <exception cref="ArgumentException"> Given stream does not support reading -OR-
        /// <paramref name="buffer"/> size is 0. </exception>
        public void WriteByteArray([NotNull] Stream dataSource, int count, [NotNull] byte[] buffer) {
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (!dataSource.CanRead) {
                throw new ArgumentException("Given stream does not support reading.", "dataSource");
            } else if (count < 0) {
                throw new ArgumentOutOfRangeException("count", "count may not be negative");
            } else if (buffer.Length == 0 && count > 0) {
                throw new ArgumentException("buffer size must be greater than 0 when count is greater than 0", "buffer");
            }
            EnforceConstraints(null, NbtTagType.ByteArray);
            WriteByteArrayFromStreamImpl(dataSource, count, buffer);
        }


        /// <summary> Writes a named byte array tag, copying data from a stream. </summary>
        /// <remarks> A temporary buffer will be allocated, of size up to 8192 bytes.
        /// To manually specify a buffer, use one of the other WriteByteArray() overloads. </remarks>
        /// <param name="tagName"> Name to give to this byte array tag. May not be null. </param>
        /// <param name="dataSource"> A Stream from which data will be copied. </param>
        /// <param name="count"> The number of bytes to write. Must not be negative. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed byte array tag was expected -OR- a tag of a different type was expected. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="count"/> is negative. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="dataSource"/> is null. </exception>
        /// <exception cref="ArgumentException"> Given stream does not support reading. </exception>
        public void WriteByteArray([NotNull] String tagName, [NotNull] Stream dataSource, int count) {
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count", "count may not be negative");
            }
            int bufferSize = Math.Min(count, MaxStreamCopyBufferSize);
            var streamCopyBuffer = new byte[bufferSize];
            WriteByteArray(tagName, dataSource, count, streamCopyBuffer);
        }


        /// <summary> Writes an unnamed byte array tag, copying data from another stream. </summary>
        /// <param name="tagName"> Name to give to this byte array tag. May not be null. </param>
        /// <param name="dataSource"> A Stream from which data will be copied. </param>
        /// <param name="count"> The number of bytes to write. Must not be negative. </param>
        /// <param name="buffer"> Buffer to use for copying. Size must be greater than 0. Must not be null. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed byte array tag was expected -OR- a tag of a different type was expected. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="count"/> is negative. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="dataSource"/> is null. </exception>
        /// <exception cref="ArgumentException"> Given stream does not support reading -OR-
        /// <paramref name="buffer"/> size is 0. </exception>
        public void WriteByteArray([NotNull] String tagName, [NotNull] Stream dataSource, int count,
                                   [NotNull] byte[] buffer) {
            if (dataSource == null) throw new ArgumentNullException("dataSource");
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (!dataSource.CanRead) {
                throw new ArgumentException("Given stream does not support reading.", "dataSource");
            } else if (count < 0) {
                throw new ArgumentOutOfRangeException("count", "count may not be negative");
            } else if (buffer.Length == 0 && count > 0) {
                throw new ArgumentException("buffer size must be greater than 0 when count is greater than 0", "buffer");
            }
            EnforceConstraints(tagName, NbtTagType.ByteArray);
            writer.Write((byte)NbtTagType.ByteArray);
            writer.Write(tagName);
            WriteByteArrayFromStreamImpl(dataSource, count, buffer);
        }


        /// <summary> Writes an unnamed int array tag, copying data from an array. </summary>
        /// <param name="data"> An int array containing the data to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named int array tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="data"/> is null </exception>
        public void WriteIntArray([NotNull] int[] data) {
            if (data == null) throw new ArgumentNullException("data");
            WriteIntArray(data, 0, data.Length);
        }


        /// <summary> Writes an unnamed int array tag, copying data from an array. </summary>
        /// <param name="data"> An int array containing the data to write. </param>
        /// <param name="offset"> The starting point in <paramref name="data"/> at which to begin writing. Must not be negative. </param>
        /// <param name="count"> The number of elements to write. Must not be negative. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// a named int array tag was expected -OR- a tag of a different type was expected -OR-
        /// the size of a parent list has been exceeded. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="offset"/> or
        /// <paramref name="count"/> is negative. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="data"/> is null </exception>
        /// <exception cref="ArgumentException"> <paramref name="count"/> is greater than
        /// <paramref name="offset"/> subtracted from the array length. </exception>
        public void WriteIntArray([NotNull] int[] data, int offset, int count) {
            CheckArray(data, offset, count);
            EnforceConstraints(null, NbtTagType.IntArray);
            writer.Write(count);
            for (int i = offset; i < count; i++) {
                writer.Write(data[i]);
            }
        }


        /// <summary> Writes a named int array tag, copying data from an array. </summary>
        /// <param name="tagName"> Name to give to this int array tag. May not be null. </param>
        /// <param name="data"> An int array containing the data to write. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed int array tag was expected -OR- a tag of a different type was expected. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="tagName"/> or
        /// <paramref name="data"/> is null </exception>
        public void WriteIntArray([NotNull] String tagName, [NotNull] int[] data) {
            if (data == null) throw new ArgumentNullException("data");
            WriteIntArray(tagName, data, 0, data.Length);
        }


        /// <summary> Writes a named int array tag, copying data from an array. </summary>
        /// <param name="tagName"> Name to give to this int array tag. May not be null. </param>
        /// <param name="data"> An int array containing the data to write. </param>
        /// <param name="offset"> The starting point in <paramref name="data"/> at which to begin writing. Must not be negative. </param>
        /// <param name="count"> The number of elements to write. Must not be negative. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR-
        /// an unnamed int array tag was expected -OR- a tag of a different type was expected. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="offset"/> or
        /// <paramref name="count"/> is negative. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="tagName"/> or
        /// <paramref name="data"/> is null </exception>
        /// <exception cref="ArgumentException"> <paramref name="count"/> is greater than
        /// <paramref name="offset"/> subtracted from the array length. </exception>
        public void WriteIntArray([NotNull] String tagName, [NotNull] int[] data, int offset, int count) {
            CheckArray(data, offset, count);
            EnforceConstraints(tagName, NbtTagType.IntArray);
            writer.Write((byte)NbtTagType.IntArray);
            writer.Write(tagName);
            writer.Write(count);
            for (int i = offset; i < count; i++) {
                writer.Write(data[i]);
            }
        }

        #endregion


        /// <summary> Writes a NbtTag object, and all of its child tags, to stream.
        /// Use this method sparingly with NbtWriter -- constructing NbtTag objects defeats the purpose of this class.
        /// If you already have lots of NbtTag objects, you might as well use NbtFile to write them all at once. </summary>
        /// <param name="tag"> Tag to write. Must not be null. </param>
        /// <exception cref="NbtFormatException"> No more tags can be written -OR- given tag is unacceptable at this time. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="tag"/> is null </exception>
        public void WriteTag([NotNull] NbtTag tag) {
            if (tag == null) throw new ArgumentNullException("tag");
            EnforceConstraints(tag.Name, tag.TagType);
            if (tag.Name != null) {
                tag.WriteTag(writer);
            } else {
                tag.WriteData(writer);
            }
        }


        /// <summary> Ensures that file has been written in its entirety, with no tags left open.
        /// This method is for verification only, and does not actually write any data. 
        /// Calling this method is optional (but probably a good idea, to catch any usage errors). </summary>
        /// <exception cref="NbtFormatException"> Not all tags have been closed yet. </exception>
        public void Finish() {
            if (!IsDone) {
                throw new NbtFormatException("Cannot finish: not all tags have been closed yet.");
            }
        }


        void GoDown(NbtTagType thisType) {
            if (nodes == null) {
                nodes = new Stack<NbtWriterNode>();
            }
            var newNode = new NbtWriterNode {
                ParentType = parentType,
                ListType = listType,
                ListSize = listSize,
                ListIndex = listIndex
            };
            nodes.Push(newNode);

            parentType = thisType;
            listType = NbtTagType.Unknown;
            listSize = 0;
            listIndex = 0;
        }


        void GoUp() {
            if (nodes == null || nodes.Count == 0) {
                IsDone = true;
            } else {
                NbtWriterNode oldNode = nodes.Pop();
                parentType = oldNode.ParentType;
                listType = oldNode.ListType;
                listSize = oldNode.ListSize;
                listIndex = oldNode.ListIndex;
            }
        }


        void EnforceConstraints([CanBeNull] String name, NbtTagType desiredType) {
            if (IsDone) {
                throw new NbtFormatException("Cannot write any more tags: root tag has been closed.");
            }
            if (parentType == NbtTagType.List) {
                if (name != null) {
                    throw new NbtFormatException("Expecting an unnamed tag.");
                } else if (listType != desiredType) {
                    throw new NbtFormatException("Unexpected tag type (expected: " + listType + ", given: " +
                                                 desiredType);
                } else if (listIndex >= listSize) {
                    throw new NbtFormatException("Given list size exceeded.");
                }
                listIndex++;
            } else if (name == null) {
                throw new NbtFormatException("Expecting a named tag.");
            }
        }


        static void CheckArray([NotNull] Array data, int offset, int count) {
            if (data == null) {
                throw new ArgumentNullException("data");
            } else if (offset < 0) {
                throw new ArgumentOutOfRangeException("offset", "offset may not be negative.");
            } else if (count < 0) {
                throw new ArgumentOutOfRangeException("count", "count may not be negative.");
            } else if ((data.Length - offset) < count) {
                throw new ArgumentException("count may not be greater than offset subtracted from the array length.");
            }
        }


        void WriteByteArrayFromStreamImpl([NotNull] Stream dataSource, int count, [NotNull] byte[] buffer) {
            Debug.Assert(dataSource != null);
            Debug.Assert(buffer != null);
            writer.Write(count);
            int maxBytesToWrite = Math.Min(buffer.Length, NbtBinaryWriter.MaxWriteChunk);
            int bytesWritten = 0;
            while (bytesWritten < count) {
                int bytesToRead = Math.Min(count - bytesWritten, maxBytesToWrite);
                int bytesRead = dataSource.Read(buffer, 0, bytesToRead);
                writer.Write(buffer, 0, bytesRead);
                bytesWritten += bytesRead;
            }
        }
    }
}
