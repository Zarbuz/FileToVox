using System;
using System.CodeDom;
using System.IO;
using NUnit.Framework;

namespace fNbt.Test {
    [TestFixture]
    public sealed class NbtReaderTests {
        [Test]
        public void PrintBigFileUncompressed() {
            using (FileStream fs = File.OpenRead("TestFiles/bigtest.nbt")) {
                var reader = new NbtReader(fs);
                Assert.AreEqual(fs, reader.BaseStream);
                while (reader.ReadToFollowing()) {
                    Console.Write("@" + reader.TagStartOffset + " ");
                    Console.WriteLine(reader.ToString());
                }
                Assert.AreEqual("Level", reader.RootName);
            }
        }


        [Test]
        public void PrintBigFileUncompressedNoSkip() {
            using (FileStream fs = File.OpenRead("TestFiles/bigtest.nbt")) {
                var reader = new NbtReader(fs) {
                    SkipEndTags = false
                };
                Assert.AreEqual(fs, reader.BaseStream);
                while (reader.ReadToFollowing()) {
                    Console.Write("@" + reader.TagStartOffset + " ");
                    Console.WriteLine(reader.ToString());
                }
                Assert.AreEqual("Level", reader.RootName);
            }
        }


        [Test]
        public void CacheTagValuesTest() {
            byte[] testData = new NbtFile(TestFiles.MakeValueTest()).SaveToBuffer(NbtCompression.None);
            var reader = new NbtReader(new MemoryStream(testData));
            Assert.IsFalse(reader.CacheTagValues);
            reader.CacheTagValues = true;
            Assert.IsTrue(reader.ReadToFollowing()); // root

            Assert.IsTrue(reader.ReadToFollowing()); // byte
            Assert.AreEqual(1, reader.ReadValue());
            Assert.AreEqual(1, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // short
            Assert.AreEqual(2, reader.ReadValue());
            Assert.AreEqual(2, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // int
            Assert.AreEqual(3, reader.ReadValue());
            Assert.AreEqual(3, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // long
            Assert.AreEqual(4L, reader.ReadValue());
            Assert.AreEqual(4L, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // float
            Assert.AreEqual(5f, reader.ReadValue());
            Assert.AreEqual(5f, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // double
            Assert.AreEqual(6d, reader.ReadValue());
            Assert.AreEqual(6d, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // byteArray
            CollectionAssert.AreEqual(new byte[] { 10, 11, 12 }, (byte[])reader.ReadValue());
            CollectionAssert.AreEqual(new byte[] { 10, 11, 12 }, (byte[])reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // intArray
            CollectionAssert.AreEqual(new[] { 20, 21, 22 }, (int[])reader.ReadValue());
            CollectionAssert.AreEqual(new[] { 20, 21, 22 }, (int[])reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // string
            Assert.AreEqual("123", reader.ReadValue());
            Assert.AreEqual("123", reader.ReadValue());
        }


        [Test]
        public void NestedListTest() {
            var root = new NbtCompound("root") {
                new NbtList("OuterList") {
                    new NbtList {
                        new NbtByte()
                    },
                    new NbtList {
                        new NbtShort()
                    },
                    new NbtList {
                        new NbtInt()
                    }
                }
            };
            byte[] testData = new NbtFile(root).SaveToBuffer(NbtCompression.None);
            using (var ms = new MemoryStream(testData)) {
                var reader = new NbtReader(ms);
                while (reader.ReadToFollowing()) {
                    Console.WriteLine(reader.ToString(true));
                }
            }
        }


        [Test]
        public void PropertiesTest() {
            var reader = new NbtReader(TestFiles.MakeReaderTest());
            Assert.AreEqual(0, reader.Depth);
            Assert.AreEqual(0, reader.TagsRead);

            Assert.IsTrue(reader.ReadToFollowing());
            Assert.AreEqual("root", reader.TagName);
            Assert.AreEqual(NbtTagType.Compound, reader.TagType);
            Assert.AreEqual(NbtTagType.Unknown, reader.ListType);
            Assert.IsFalse(reader.HasValue);
            Assert.IsTrue(reader.IsCompound);
            Assert.IsFalse(reader.IsList);
            Assert.IsFalse(reader.IsListElement);
            Assert.IsFalse(reader.HasLength);
            Assert.AreEqual(0, reader.ListIndex);
            Assert.AreEqual(1, reader.Depth);
            Assert.AreEqual(null, reader.ParentName);
            Assert.AreEqual(NbtTagType.Unknown, reader.ParentTagType);
            Assert.AreEqual(0, reader.ParentTagLength);
            Assert.AreEqual(0, reader.TagLength);
            Assert.AreEqual(1, reader.TagsRead);

            Assert.IsTrue(reader.ReadToFollowing());
            Assert.AreEqual("first", reader.TagName);
            Assert.AreEqual(NbtTagType.Int, reader.TagType);
            Assert.AreEqual(NbtTagType.Unknown, reader.ListType);
            Assert.IsTrue(reader.HasValue);
            Assert.IsFalse(reader.IsCompound);
            Assert.IsFalse(reader.IsList);
            Assert.IsFalse(reader.IsListElement);
            Assert.IsFalse(reader.HasLength);
            Assert.AreEqual(0, reader.ListIndex);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("root", reader.ParentName);
            Assert.AreEqual(NbtTagType.Compound, reader.ParentTagType);
            Assert.AreEqual(0, reader.ParentTagLength);
            Assert.AreEqual(0, reader.TagLength);
            Assert.AreEqual(2, reader.TagsRead);

            Assert.IsTrue(reader.ReadToFollowing("fourth-list"));
            Assert.AreEqual("fourth-list", reader.TagName);
            Assert.AreEqual(NbtTagType.List, reader.TagType);
            Assert.AreEqual(NbtTagType.List, reader.ListType);
            Assert.IsFalse(reader.HasValue);
            Assert.IsFalse(reader.IsCompound);
            Assert.IsTrue(reader.IsList);
            Assert.IsFalse(reader.IsListElement);
            Assert.IsTrue(reader.HasLength);
            Assert.AreEqual(0, reader.ListIndex);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("root", reader.ParentName);
            Assert.AreEqual(NbtTagType.Compound, reader.ParentTagType);
            Assert.AreEqual(0, reader.ParentTagLength);
            Assert.AreEqual(3, reader.TagLength);
            Assert.AreEqual(8, reader.TagsRead);

            Assert.IsTrue(reader.ReadToFollowing()); // first list element, itself a list
            Assert.AreEqual(null, reader.TagName);
            Assert.AreEqual(NbtTagType.List, reader.TagType);
            Assert.AreEqual(NbtTagType.Compound, reader.ListType);
            Assert.IsFalse(reader.HasValue);
            Assert.IsFalse(reader.IsCompound);
            Assert.IsTrue(reader.IsList);
            Assert.IsTrue(reader.IsListElement);
            Assert.IsTrue(reader.HasLength);
            Assert.AreEqual(0, reader.ListIndex);
            Assert.AreEqual(3, reader.Depth);
            Assert.AreEqual("fourth-list", reader.ParentName);
            Assert.AreEqual(NbtTagType.List, reader.ParentTagType);
            Assert.AreEqual(3, reader.ParentTagLength);
            Assert.AreEqual(1, reader.TagLength);
            Assert.AreEqual(9, reader.TagsRead);

            Assert.IsTrue(reader.ReadToFollowing()); // first nested list element, compound
            Assert.AreEqual(null, reader.TagName);
            Assert.AreEqual(NbtTagType.Compound, reader.TagType);
            Assert.AreEqual(NbtTagType.Unknown, reader.ListType);
            Assert.IsFalse(reader.HasValue);
            Assert.IsTrue(reader.IsCompound);
            Assert.IsFalse(reader.IsList);
            Assert.IsTrue(reader.IsListElement);
            Assert.IsFalse(reader.HasLength);
            Assert.AreEqual(0, reader.ListIndex);
            Assert.AreEqual(4, reader.Depth);
            Assert.AreEqual(null, reader.ParentName);
            Assert.AreEqual(NbtTagType.List, reader.ParentTagType);
            Assert.AreEqual(1, reader.ParentTagLength);
            Assert.AreEqual(0, reader.TagLength);
            Assert.AreEqual(10, reader.TagsRead);

            Assert.IsTrue(reader.ReadToFollowing("fifth"));
            Assert.AreEqual("fifth", reader.TagName);
            Assert.AreEqual(NbtTagType.Int, reader.TagType);
            Assert.AreEqual(NbtTagType.Unknown, reader.ListType);
            Assert.IsTrue(reader.HasValue);
            Assert.IsFalse(reader.IsCompound);
            Assert.IsFalse(reader.IsList);
            Assert.IsFalse(reader.IsListElement);
            Assert.IsFalse(reader.HasLength);
            Assert.AreEqual(0, reader.ListIndex);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("root", reader.ParentName);
            Assert.AreEqual(NbtTagType.Compound, reader.ParentTagType);
            Assert.AreEqual(0, reader.ParentTagLength);
            Assert.AreEqual(0, reader.TagLength);
            Assert.AreEqual(18, reader.TagsRead);

            Assert.IsTrue(reader.ReadToFollowing());
            Assert.AreEqual("hugeArray", reader.TagName);
            Assert.AreEqual(NbtTagType.ByteArray, reader.TagType);
            Assert.AreEqual(NbtTagType.Unknown, reader.ListType);
            Assert.IsTrue(reader.HasValue);
            Assert.IsFalse(reader.IsCompound);
            Assert.IsFalse(reader.IsList);
            Assert.IsFalse(reader.IsListElement);
            Assert.IsTrue(reader.HasLength);
            Assert.AreEqual(0, reader.ListIndex);
            Assert.AreEqual(2, reader.Depth);
            Assert.AreEqual("root", reader.ParentName);
            Assert.AreEqual(NbtTagType.Compound, reader.ParentTagType);
            Assert.AreEqual(0, reader.ParentTagLength);
            Assert.AreEqual(1024*1024, reader.TagLength);
            Assert.AreEqual(19, reader.TagsRead);
        }


        [Test]
        public void ReadToSiblingTest() {
            var reader = new NbtReader(TestFiles.MakeReaderTest());
            Assert.IsTrue(reader.ReadToFollowing());
            Assert.AreEqual("root", reader.TagName);
            Assert.IsTrue(reader.ReadToFollowing());
            Assert.AreEqual("first", reader.TagName);
            Assert.IsTrue(reader.ReadToNextSibling("third-comp"));
            Assert.AreEqual("third-comp", reader.TagName);
            Assert.IsTrue(reader.ReadToNextSibling());
            Assert.AreEqual("fourth-list", reader.TagName);
            Assert.IsTrue(reader.ReadToNextSibling());
            Assert.AreEqual("fifth", reader.TagName);
            Assert.IsTrue(reader.ReadToNextSibling());
            Assert.AreEqual("hugeArray", reader.TagName);
            Assert.IsFalse(reader.ReadToNextSibling());
            // Test twice, since we hit different paths through the code
            Assert.IsFalse(reader.ReadToNextSibling());
        }


        [Test]
        public void ReadToDescendantTest() {
            var reader = new NbtReader(TestFiles.MakeReaderTest());
            Assert.IsTrue(reader.ReadToDescendant("third-comp"));
            Assert.AreEqual("third-comp", reader.TagName);
            Assert.IsTrue(reader.ReadToDescendant("inComp2"));
            Assert.AreEqual("inComp2", reader.TagName);
            Assert.IsFalse(reader.ReadToDescendant("derp"));
            Assert.AreEqual("inComp3", reader.TagName);
            reader.ReadToFollowing(); // at fourth-list
            Assert.IsTrue(reader.ReadToDescendant("inList2"));
            Assert.AreEqual("inList2", reader.TagName);

            // Ensure ReadToDescendant returns false when at end-of-stream
            while (reader.ReadToFollowing()) {}
            Assert.IsFalse(reader.ReadToDescendant("*"));
        }


        [Test]
        public void SkipTest() {
            var reader = new NbtReader(TestFiles.MakeReaderTest());
            reader.ReadToFollowing(); // at root
            reader.ReadToFollowing(); // at first
            reader.ReadToFollowing(); // at second
            reader.ReadToFollowing(); // at third-comp
            reader.ReadToFollowing(); // at inComp1
            Assert.AreEqual("inComp1", reader.TagName);
            Assert.AreEqual(2, reader.Skip());
            Assert.AreEqual("fourth-list", reader.TagName);
            Assert.AreEqual(11, reader.Skip());
            Assert.IsFalse(reader.ReadToFollowing());
            Assert.AreEqual(0, reader.Skip());
        }


        [Test]
        public void ReadAsTagTest1() {
            // read various lists/compounds as tags
            var reader = new NbtReader(TestFiles.MakeReaderTest());
            reader.ReadToFollowing(); // skip root
            while (!reader.IsAtStreamEnd) {
                reader.ReadAsTag();
            }
            Assert.Throws<EndOfStreamException>(() => reader.ReadAsTag());
        }


        [Test]
        public void ReadAsTagTest2() {
            // read the whole thing as one tag
            byte[] testData = new NbtFile(TestFiles.MakeValueTest()).SaveToBuffer(NbtCompression.None);
            {
                var reader = new NbtReader(new MemoryStream(testData));
                var root = (NbtCompound)reader.ReadAsTag();
                TestFiles.AssertValueTest(new NbtFile(root));
            }
            {
                // Try the same thing but with end tag skipping disabled
                var reader = new NbtReader(new MemoryStream(testData)) {
                    SkipEndTags = false
                };
                var root = (NbtCompound)reader.ReadAsTag();
                TestFiles.AssertValueTest(new NbtFile(root));
            }
        }


        [Test]
        public void ReadAsTagTest3() {
            // read values as tags
            byte[] testData = new NbtFile(TestFiles.MakeValueTest()).SaveToBuffer(NbtCompression.None);
            var reader = new NbtReader(new MemoryStream(testData));
            var root = new NbtCompound("root");

            // skip root
            reader.ReadToFollowing();
            reader.ReadToFollowing();

            while (!reader.IsAtStreamEnd) {
                root.Add(reader.ReadAsTag());
            }

            TestFiles.AssertValueTest(new NbtFile(root));
        }


        [Test]
        public void ReadAsTagTest4() {
            // read a bunch of lists as tags
            byte[] testData = new NbtFile(TestFiles.MakeListTest()).SaveToBuffer(NbtCompression.None);

            // first, read everything all-at-once
            {
                var reader = new NbtReader(new MemoryStream(testData));
                while (!reader.IsAtStreamEnd) {
                    Console.WriteLine(reader.ReadAsTag());
                }
            }

            // next, read each list individually
            {
                var reader = new NbtReader(new MemoryStream(testData));
                reader.ReadToFollowing(); // read to root
                reader.ReadToFollowing(); // read to first list tag
                while (!reader.IsAtStreamEnd) {
                    Console.WriteLine(reader.ReadAsTag());
                }
            }
        }


        [Test]
        public void ReadListAsArray() {
            NbtCompound intList = TestFiles.MakeListTest();

            var ms = new MemoryStream();
            new NbtFile(intList).SaveToStream(ms, NbtCompression.None);
            ms.Seek(0, SeekOrigin.Begin);
            var reader = new NbtReader(ms);

            // attempt to read value before we're in a list
            Assert.Throws<InvalidOperationException>(() => reader.ReadListAsArray<int>());

            // test byte values
            reader.ReadToFollowing("ByteList");
            byte[] bytes = reader.ReadListAsArray<byte>();
            CollectionAssert.AreEqual(new byte[] { 100, 20, 3 }, bytes);

            // test double values
            reader.ReadToFollowing("DoubleList");
            double[] doubles = reader.ReadListAsArray<double>();
            CollectionAssert.AreEqual(new[] { 1d, 2000d, -3000000d }, doubles);

            // test float values
            reader.ReadToFollowing("FloatList");
            float[] floats = reader.ReadListAsArray<float>();
            CollectionAssert.AreEqual(new[] { 1f, 2000f, -3000000f }, floats);

            // test int values
            reader.ReadToFollowing("IntList");
            int[] ints = reader.ReadListAsArray<int>();
            CollectionAssert.AreEqual(new[] { 1, 2000, -3000000 }, ints);

            // test long values
            reader.ReadToFollowing("LongList");
            long[] longs = reader.ReadListAsArray<long>();
            CollectionAssert.AreEqual(new[] { 1L, 2000L, -3000000L }, longs);

            // test short values
            reader.ReadToFollowing("ShortList");
            short[] shorts = reader.ReadListAsArray<short>();
            CollectionAssert.AreEqual(new short[] { 1, 200, -30000 }, shorts);

            // test short values
            reader.ReadToFollowing("StringList");
            string[] strings = reader.ReadListAsArray<string>();
            CollectionAssert.AreEqual(new[] { "one", "two thousand", "negative three million" }, strings);

            // try reading list of compounds (should fail)
            reader.ReadToFollowing("CompoundList");
            Assert.Throws<InvalidOperationException>(() => reader.ReadListAsArray<NbtCompound>());

            // skip to the end of the stream
            while (reader.ReadToFollowing()) {}
            Assert.Throws<EndOfStreamException>(() => reader.ReadListAsArray<int>());
        }


        [Test]
        public void ReadListAsArrayRecast() {
            NbtCompound intList = TestFiles.MakeListTest();

            var ms = new MemoryStream();
            new NbtFile(intList).SaveToStream(ms, NbtCompression.None);
            ms.Seek(0, SeekOrigin.Begin);
            var reader = new NbtReader(ms);

            // test bytes as shorts
            reader.ReadToFollowing("ByteList");
            short[] bytes = reader.ReadListAsArray<short>();
            CollectionAssert.AreEqual(bytes,
                                      new short[] {
                                          100, 20, 3
                                      });
        }


        [Test]
        public void ReadValueTest() {
            byte[] testData = new NbtFile(TestFiles.MakeValueTest()).SaveToBuffer(NbtCompression.None);
            var reader = new NbtReader(new MemoryStream(testData));

            Assert.IsTrue(reader.ReadToFollowing()); // root

            Assert.IsTrue(reader.ReadToFollowing()); // byte
            Assert.AreEqual(1, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // short
            Assert.AreEqual(2, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // int
            Assert.AreEqual(3, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // long
            Assert.AreEqual(4L, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // float
            Assert.AreEqual(5f, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // double
            Assert.AreEqual(6d, reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // byteArray
            CollectionAssert.AreEqual(new byte[] { 10, 11, 12 }, (byte[])reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // intArray
            CollectionAssert.AreEqual(new[] { 20, 21, 22 }, (int[])reader.ReadValue());
            Assert.IsTrue(reader.ReadToFollowing()); // string
            Assert.AreEqual("123", reader.ReadValue());

            // Skip to the very end and make sure that we can't read any more values
            reader.ReadToFollowing();
            Assert.Throws<EndOfStreamException>(() => reader.ReadValue());
        }


        [Test]
        public void ReadValueAsTest() {
            byte[] testData = new NbtFile(TestFiles.MakeValueTest()).SaveToBuffer(NbtCompression.None);
            var reader = new NbtReader(new MemoryStream(testData));

            Assert.IsTrue(reader.ReadToFollowing()); // root

            Assert.IsTrue(reader.ReadToFollowing()); // byte
            Assert.AreEqual(1, reader.ReadValueAs<byte>());
            Assert.IsTrue(reader.ReadToFollowing()); // short
            Assert.AreEqual(2, reader.ReadValueAs<short>());
            Assert.IsTrue(reader.ReadToFollowing()); // int
            Assert.AreEqual(3, reader.ReadValueAs<int>());
            Assert.IsTrue(reader.ReadToFollowing()); // long
            Assert.AreEqual(4L, reader.ReadValueAs<long>());
            Assert.IsTrue(reader.ReadToFollowing()); // float
            Assert.AreEqual(5f, reader.ReadValueAs<float>());
            Assert.IsTrue(reader.ReadToFollowing()); // double
            Assert.AreEqual(6d, reader.ReadValueAs<double>());
            Assert.IsTrue(reader.ReadToFollowing()); // byteArray
            CollectionAssert.AreEqual(new byte[] { 10, 11, 12 }, reader.ReadValueAs<byte[]>());
            Assert.IsTrue(reader.ReadToFollowing()); // intArray
            CollectionAssert.AreEqual(new[] { 20, 21, 22 }, reader.ReadValueAs<int[]>());
            Assert.IsTrue(reader.ReadToFollowing()); // string
            Assert.AreEqual("123", reader.ReadValueAs<string>());
        }


        [Test]
        public void ErrorTest() {
            var root = new NbtCompound("root");
            byte[] testData = new NbtFile(root).SaveToBuffer(NbtCompression.None);

            // creating NbtReader without a stream, or with a non-readable stream
            Assert.Throws<ArgumentNullException>(() => new NbtReader(null));
            Assert.Throws<ArgumentException>(() => new NbtReader(new NonReadableStream()));

            // corrupt the data
            testData[0] = 123;
            var reader = new NbtReader(new MemoryStream(testData));

            // attempt to use ReadValue when not at value
            Assert.Throws<InvalidOperationException>(() => reader.ReadValue());
            reader.CacheTagValues = true;
            Assert.Throws<InvalidOperationException>(() => reader.ReadValue());

            // attempt to read a corrupt stream
            Assert.Throws<NbtFormatException>(() => reader.ReadToFollowing());

            // make sure we've properly entered the error state
            Assert.IsTrue(reader.IsInErrorState);
            Assert.IsFalse(reader.HasName);
            Assert.Throws<InvalidReaderStateException>(() => reader.ReadToFollowing());
            Assert.Throws<InvalidReaderStateException>(() => reader.ReadListAsArray<int>());
            Assert.Throws<InvalidReaderStateException>(() => reader.ReadToNextSibling());
            Assert.Throws<InvalidReaderStateException>(() => reader.ReadToDescendant("derp"));
            Assert.Throws<InvalidReaderStateException>(() => reader.ReadAsTag());
            Assert.Throws<InvalidReaderStateException>(() => reader.Skip());
        }


        [Test]
        public void NonSeekableStreamSkip1() {
            byte[] fileBytes = File.ReadAllBytes("TestFiles/bigtest.nbt");
            using (var ms = new MemoryStream(fileBytes)) {
                using (var nss = new NonSeekableStream(ms)) {
                    var reader = new NbtReader(nss);
                    reader.ReadToFollowing();
                    reader.Skip();
                }
            }
        }


        [Test]
        public void NonSeekableStreamSkip2() {
            using (var ms = TestFiles.MakeReaderTest()) {
                using (var nss = new NonSeekableStream(ms)) {
                    var reader = new NbtReader(nss);
                    reader.ReadToFollowing();
                    reader.Skip();
                }
            }
        }


        [Test]
        public void CorruptFileRead() {
            byte[] emptyFile = new byte[0];
            Assert.Throws<EndOfStreamException>(() => TryReadBadFile(emptyFile));
            Assert.Throws<EndOfStreamException>(
                () => new NbtFile().LoadFromBuffer(emptyFile, 0, emptyFile.Length, NbtCompression.None));
            Assert.Throws<EndOfStreamException>(
                () => NbtFile.ReadRootTagName(new MemoryStream(emptyFile), NbtCompression.AutoDetect, true, 0));
            Assert.Throws<EndOfStreamException>(
                () => NbtFile.ReadRootTagName(new MemoryStream(emptyFile), NbtCompression.None, true, 0));

            byte[] badHeader = {
                0x02, // TAG_Short ID (instead of TAG_Compound ID)
                0x00, 0x01, 0x66, // Root name: 'f'
                0x00 // end tag
            };
            Assert.Throws<NbtFormatException>(() => TryReadBadFile(badHeader));
            Assert.Throws<NbtFormatException>(
                () => new NbtFile().LoadFromBuffer(badHeader, 0, badHeader.Length, NbtCompression.None));
            Assert.Throws<NbtFormatException>(
                () => NbtFile.ReadRootTagName(new MemoryStream(badHeader), NbtCompression.None, true, 0));

            byte[] badStringLength = {
                0x0A, // Compound tag
                0xFF, 0xFF, 0x66, // Root name 'f' (with string length given as "-1")
                0x00 // end tag
            };
            Assert.Throws<NbtFormatException>(() => TryReadBadFile(badStringLength));
            Assert.Throws<NbtFormatException>(
                () => new NbtFile().LoadFromBuffer(badStringLength, 0, badStringLength.Length, NbtCompression.None));
            Assert.Throws<NbtFormatException>(
                () => NbtFile.ReadRootTagName(new MemoryStream(badStringLength), NbtCompression.None, true, 0));

            byte[] abruptStringEnd = {
                0x0A, // Compound tag
                0x00, 0xFF, 0x66, // Root name 'f' (string length given as 5)
                0x00 // premature end tag
            };
            Assert.Throws<EndOfStreamException>(() => TryReadBadFile(abruptStringEnd));
            Assert.Throws<EndOfStreamException>(
                () => new NbtFile().LoadFromBuffer(abruptStringEnd, 0, abruptStringEnd.Length, NbtCompression.None));
            Assert.Throws<EndOfStreamException>(
                () => NbtFile.ReadRootTagName(new MemoryStream(abruptStringEnd), NbtCompression.None, true, 0));

            byte[] badSecondTag = {
                0x0A, // Compound tag
                0x00, 0x01, 0x66, // Root name: 'f'
                0xFF, 0x01, 0x4E, 0x7F, 0xFF, // Short tag named 'N' with invalid tag ID (0xFF instead of 0x02)
                0x00 // end tag
            };
            Assert.Throws<NbtFormatException>(() => TryReadBadFile(badSecondTag));
            Assert.Throws<NbtFormatException>(
                () => new NbtFile().LoadFromBuffer(badSecondTag, 0, badSecondTag.Length, NbtCompression.None));

            byte[] badListType = {
                0x0A, // Compound tag
                0x00, 0x01, 0x66, // Root name: 'f'
                0x09, // List tag
                0x00, 0x01, 0x66, // List tag name: 'g'
                0xFF // invalid list tag type (-1)
            };
            Assert.Throws<NbtFormatException>(() => TryReadBadFile(badListType));
            Assert.Throws<NbtFormatException>(
                () => new NbtFile().LoadFromBuffer(badListType, 0, badListType.Length, NbtCompression.None));

            byte[] badListSize = {
                0x0A, // Compound tag
                0x00, 0x01, 0x66, // Root name: 'f'
                0x09, // List tag
                0x00, 0x01, 0x66, // List tag name: 'g'
                0x01, // List type: Byte
                0xFF, 0xFF, 0xFF, 0xFF, // List size: -1
            };
            Assert.Throws<NbtFormatException>(() => TryReadBadFile(badListSize));
            Assert.Throws<NbtFormatException>(
                () => new NbtFile().LoadFromBuffer(badListSize, 0, badListSize.Length, NbtCompression.None));
        }


        [Test]
        public void PartialReadTest() {
            // read the whole thing as one tag
            TestFiles.AssertValueTest(PartialReadTestInternal(new NbtFile(TestFiles.MakeValueTest())));
            TestFiles.AssertNbtSmallFile(PartialReadTestInternal(TestFiles.MakeSmallFile()));
            TestFiles.AssertNbtBigFile(PartialReadTestInternal(new NbtFile(TestFiles.Big)));
        }


        static NbtFile PartialReadTestInternal(NbtFile comp) {
            byte[] testData = comp.SaveToBuffer(NbtCompression.None);
            var reader = new NbtReader(new PartialReadStream(new MemoryStream(testData)));
            var root = (NbtCompound)reader.ReadAsTag();
            return new NbtFile(root);
        }


        void TryReadBadFile(byte[] data) {
            using (MemoryStream ms = new MemoryStream(data)) {
                NbtReader reader = new NbtReader(ms);
                try {
                    while (reader.ReadToFollowing()) {}
                } catch (Exception) {
                    Assert.IsTrue(reader.IsInErrorState);
                    throw;
                }
            }
        }


        class NonReadableStream : MemoryStream {
            public override bool CanRead {
                get { return false; }
            }


            public override int ReadByte() {
                throw new NotSupportedException();
            }


            public override int Read(byte[] buffer, int offset, int count) {
                throw new NotSupportedException();
            }
        }
    }
}
