using System;
using System.IO;
using NUnit.Framework;

namespace fNbt.Test {
    public static class TestFiles {
        public const string DirName = "TestFiles";
        public static readonly string Small = Path.Combine(DirName, "test.nbt");
        public static readonly string SmallGZip = Path.Combine(DirName, "test.nbt.gz");
        public static readonly string SmallZLib = Path.Combine(DirName, "test.nbt.z");
        public static readonly string Big = Path.Combine(DirName, "bigtest.nbt");
        public static readonly string BigGZip = Path.Combine(DirName, "bigtest.nbt.gz");
        public static readonly string BigZLib = Path.Combine(DirName, "bigtest.nbt.z");


        // creates a compound containing lists of every kind of tag
        public static NbtCompound MakeListTest() {
            return new NbtCompound("Root") {
                new NbtList("ByteList") {
                    new NbtByte(100),
                    new NbtByte(20),
                    new NbtByte(3)
                },
                new NbtList("DoubleList") {
                    new NbtDouble(1d),
                    new NbtDouble(2000d),
                    new NbtDouble(-3000000d)
                },
                new NbtList("FloatList") {
                    new NbtFloat(1f),
                    new NbtFloat(2000f),
                    new NbtFloat(-3000000f)
                },
                new NbtList("IntList") {
                    new NbtInt(1),
                    new NbtInt(2000),
                    new NbtInt(-3000000)
                },
                new NbtList("LongList") {
                    new NbtLong(1L),
                    new NbtLong(2000L),
                    new NbtLong(-3000000L)
                },
                new NbtList("ShortList") {
                    new NbtShort(1),
                    new NbtShort(200),
                    new NbtShort(-30000)
                },
                new NbtList("StringList") {
                    new NbtString("one"),
                    new NbtString("two thousand"),
                    new NbtString("negative three million")
                },
                new NbtList("CompoundList") {
                    new NbtCompound(),
                    new NbtCompound(),
                    new NbtCompound()
                },
                new NbtList("ListList") {
                    new NbtList(NbtTagType.List),
                    new NbtList(NbtTagType.List),
                    new NbtList(NbtTagType.List)
                },
                new NbtList("ByteArrayList") {
                    new NbtByteArray(new byte[] {
                        1, 2, 3
                    }),
                    new NbtByteArray(new byte[] {
                        11, 12, 13
                    }),
                    new NbtByteArray(new byte[] {
                        21, 22, 23
                    })
                },
                new NbtList("IntArrayList") {
                    new NbtIntArray(new[] {
                        1, -2, 3
                    }),
                    new NbtIntArray(new[] {
                        1000, -2000, 3000
                    }),
                    new NbtIntArray(new[] {
                        1000000, -2000000, 3000000
                    })
                }
            };
        }


        // creates a file with lots of compounds and lists, used to test NbtReader compliance
        public static Stream MakeReaderTest() {
            var root = new NbtCompound("root") {
                new NbtInt("first"),
                new NbtInt("second"),
                new NbtCompound("third-comp") {
                    new NbtInt("inComp1"),
                    new NbtInt("inComp2"),
                    new NbtInt("inComp3")
                },
                new NbtList("fourth-list") {
                    new NbtList {
                        new NbtCompound {
                            new NbtCompound("inList1")
                        }
                    },
                    new NbtList {
                        new NbtCompound {
                            new NbtCompound("inList2")
                        }
                    },
                    new NbtList {
                        new NbtCompound {
                            new NbtCompound("inList3")
                        }
                    }
                },
                new NbtInt("fifth"),
                new NbtByteArray("hugeArray", new byte[1024*1024])
            };
            byte[] testData = new NbtFile(root).SaveToBuffer(NbtCompression.None);
            return new MemoryStream(testData);
        }


        // creates an NbtFile with contents identical to "test.nbt"
        public static NbtFile MakeSmallFile() {
            return new NbtFile(new NbtCompound("hello world") {
                new NbtString("name", "Bananrama")
            });
        }


        public static void AssertNbtSmallFile(NbtFile file) {
            // See TestFiles/test.nbt.txt to see the expected format
            Assert.IsInstanceOf<NbtCompound>(file.RootTag);

            NbtCompound root = file.RootTag;
            Assert.AreEqual("hello world", root.Name);
            Assert.AreEqual(1, root.Count);

            Assert.IsInstanceOf<NbtString>(root["name"]);

            var node = (NbtString)root["name"];
            Assert.AreEqual("name", node.Name);
            Assert.AreEqual("Bananrama", node.Value);
        }


        public static void AssertNbtBigFile(NbtFile file) {
            // See TestFiles/bigtest.nbt.txt to see the expected format
            Assert.IsInstanceOf<NbtCompound>(file.RootTag);

            NbtCompound root = file.RootTag;
            Assert.AreEqual("Level", root.Name);
            Assert.AreEqual(12, root.Count);

            Assert.IsInstanceOf<NbtLong>(root["longTest"]);
            NbtTag node = root["longTest"];
            Assert.AreEqual("longTest", node.Name);
            Assert.AreEqual(9223372036854775807, ((NbtLong)node).Value);

            Assert.IsInstanceOf<NbtShort>(root["shortTest"]);
            node = root["shortTest"];
            Assert.AreEqual("shortTest", node.Name);
            Assert.AreEqual(32767, ((NbtShort)node).Value);

            Assert.IsInstanceOf<NbtString>(root["stringTest"]);
            node = root["stringTest"];
            Assert.AreEqual("stringTest", node.Name);
            Assert.AreEqual("HELLO WORLD THIS IS A TEST STRING ÅÄÖ!", ((NbtString)node).Value);

            Assert.IsInstanceOf<NbtFloat>(root["floatTest"]);
            node = root["floatTest"];
            Assert.AreEqual("floatTest", node.Name);
            Assert.AreEqual(0.49823147f, ((NbtFloat)node).Value);

            Assert.IsInstanceOf<NbtInt>(root["intTest"]);
            node = root["intTest"];
            Assert.AreEqual("intTest", node.Name);
            Assert.AreEqual(2147483647, ((NbtInt)node).Value);

            Assert.IsInstanceOf<NbtCompound>(root["nested compound test"]);
            node = root["nested compound test"];
            Assert.AreEqual("nested compound test", node.Name);
            Assert.AreEqual(2, ((NbtCompound)node).Count);

            // First nested test
            Assert.IsInstanceOf<NbtCompound>(node["ham"]);
            var subNode = (NbtCompound)node["ham"];
            Assert.AreEqual("ham", subNode.Name);
            Assert.AreEqual(2, subNode.Count);

            // Checking sub node values
            Assert.IsInstanceOf<NbtString>(subNode["name"]);
            Assert.AreEqual("name", subNode["name"].Name);
            Assert.AreEqual("Hampus", ((NbtString)subNode["name"]).Value);

            Assert.IsInstanceOf<NbtFloat>(subNode["value"]);
            Assert.AreEqual("value", subNode["value"].Name);
            Assert.AreEqual(0.75, ((NbtFloat)subNode["value"]).Value);
            // End sub node

            // Second nested test
            Assert.IsInstanceOf<NbtCompound>(node["egg"]);
            subNode = (NbtCompound)node["egg"];
            Assert.AreEqual("egg", subNode.Name);
            Assert.AreEqual(2, subNode.Count);

            // Checking sub node values
            Assert.IsInstanceOf<NbtString>(subNode["name"]);
            Assert.AreEqual("name", subNode["name"].Name);
            Assert.AreEqual("Eggbert", ((NbtString)subNode["name"]).Value);

            Assert.IsInstanceOf<NbtFloat>(subNode["value"]);
            Assert.AreEqual("value", subNode["value"].Name);
            Assert.AreEqual(0.5, ((NbtFloat)subNode["value"]).Value);
            // End sub node

            Assert.IsInstanceOf<NbtList>(root["listTest (long)"]);
            node = root["listTest (long)"];
            Assert.AreEqual("listTest (long)", node.Name);
            Assert.AreEqual(5, ((NbtList)node).Count);

            // The values should be: 11, 12, 13, 14, 15
            for (int nodeIndex = 0; nodeIndex < ((NbtList)node).Count; nodeIndex++) {
                Assert.IsInstanceOf<NbtLong>(node[nodeIndex]);
                Assert.AreEqual(null, node[nodeIndex].Name);
                Assert.AreEqual(nodeIndex + 11, ((NbtLong)node[nodeIndex]).Value);
            }

            Assert.IsInstanceOf<NbtList>(root["listTest (compound)"]);
            node = root["listTest (compound)"];
            Assert.AreEqual("listTest (compound)", node.Name);
            Assert.AreEqual(2, ((NbtList)node).Count);

            // First Sub Node
            Assert.IsInstanceOf<NbtCompound>(node[0]);
            subNode = (NbtCompound)node[0];

            // First node in sub node
            Assert.IsInstanceOf<NbtString>(subNode["name"]);
            Assert.AreEqual("name", subNode["name"].Name);
            Assert.AreEqual("Compound tag #0", ((NbtString)subNode["name"]).Value);

            // Second node in sub node
            Assert.IsInstanceOf<NbtLong>(subNode["created-on"]);
            Assert.AreEqual("created-on", subNode["created-on"].Name);
            Assert.AreEqual(1264099775885, ((NbtLong)subNode["created-on"]).Value);

            // Second Sub Node
            Assert.IsInstanceOf<NbtCompound>(node[1]);
            subNode = (NbtCompound)node[1];

            // First node in sub node
            Assert.IsInstanceOf<NbtString>(subNode["name"]);
            Assert.AreEqual("name", subNode["name"].Name);
            Assert.AreEqual("Compound tag #1", ((NbtString)subNode["name"]).Value);

            // Second node in sub node
            Assert.IsInstanceOf<NbtLong>(subNode["created-on"]);
            Assert.AreEqual("created-on", subNode["created-on"].Name);
            Assert.AreEqual(1264099775885, ((NbtLong)subNode["created-on"]).Value);

            Assert.IsInstanceOf<NbtByte>(root["byteTest"]);
            node = root["byteTest"];
            Assert.AreEqual("byteTest", node.Name);
            Assert.AreEqual(127, ((NbtByte)node).Value);

            const string byteArrayName =
                "byteArrayTest (the first 1000 values of (n*n*255+n*7)%100, starting with n=0 (0, 62, 34, 16, 8, ...))";
            Assert.IsInstanceOf<NbtByteArray>(root[byteArrayName]);
            node = root[byteArrayName];
            Assert.AreEqual(byteArrayName, node.Name);
            Assert.AreEqual(1000, ((NbtByteArray)node).Value.Length);

            // Values are: the first 1000 values of (n*n*255+n*7)%100, starting with n=0 (0, 62, 34, 16, 8, ...)
            for (int n = 0; n < 1000; n++) {
                Assert.AreEqual((n*n*255 + n*7)%100, ((NbtByteArray)node)[n]);
            }

            Assert.IsInstanceOf<NbtDouble>(root["doubleTest"]);
            node = root["doubleTest"];
            Assert.AreEqual("doubleTest", node.Name);
            Assert.AreEqual(0.4931287132182315, ((NbtDouble)node).Value);

            Assert.IsInstanceOf<NbtIntArray>(root["intArrayTest"]);
            var intArrayTag = root.Get<NbtIntArray>("intArrayTest");
            Assert.IsNotNull(intArrayTag);
            var rand = new Random(0);
            for (int i = 0; i < 10; i++) {
                Assert.AreEqual(rand.Next(), intArrayTag.Value[i]);
            }
        }


        #region Value test

        // creates an NbtCompound with one of tag of each value-type
        public static NbtCompound MakeValueTest() {
            return new NbtCompound("root") {
                new NbtByte("byte", 1),
                new NbtShort("short", 2),
                new NbtInt("int", 3),
                new NbtLong("long", 4L),
                new NbtFloat("float", 5f),
                new NbtDouble("double", 6d),
                new NbtByteArray("byteArray", new byte[] { 10, 11, 12 }),
                new NbtIntArray("intArray", new[] { 20, 21, 22 }),
                new NbtString("string", "123")
            };
        }


        public static void AssertValueTest(NbtFile file) {
            Assert.IsInstanceOf<NbtCompound>(file.RootTag);

            NbtCompound root = file.RootTag;
            Assert.AreEqual("root", root.Name);
            Assert.AreEqual(9, root.Count);

            Assert.IsInstanceOf<NbtByte>(root["byte"]);
            NbtTag node = root["byte"];
            Assert.AreEqual("byte", node.Name);
            Assert.AreEqual(1, node.ByteValue);

            Assert.IsInstanceOf<NbtShort>(root["short"]);
            node = root["short"];
            Assert.AreEqual("short", node.Name);
            Assert.AreEqual(2, node.ShortValue);

            Assert.IsInstanceOf<NbtInt>(root["int"]);
            node = root["int"];
            Assert.AreEqual("int", node.Name);
            Assert.AreEqual(3, node.IntValue);

            Assert.IsInstanceOf<NbtLong>(root["long"]);
            node = root["long"];
            Assert.AreEqual("long", node.Name);
            Assert.AreEqual(4L, node.LongValue);

            Assert.IsInstanceOf<NbtFloat>(root["float"]);
            node = root["float"];
            Assert.AreEqual("float", node.Name);
            Assert.AreEqual(5f, node.FloatValue);

            Assert.IsInstanceOf<NbtDouble>(root["double"]);
            node = root["double"];
            Assert.AreEqual("double", node.Name);
            Assert.AreEqual(6d, node.DoubleValue);

            Assert.IsInstanceOf<NbtByteArray>(root["byteArray"]);
            node = root["byteArray"];
            Assert.AreEqual("byteArray", node.Name);
            CollectionAssert.AreEqual(new byte[] { 10, 11, 12 }, node.ByteArrayValue);

            Assert.IsInstanceOf<NbtIntArray>(root["intArray"]);
            node = root["intArray"];
            Assert.AreEqual("intArray", node.Name);
            CollectionAssert.AreEqual(new[] { 20, 21, 22 }, node.IntArrayValue);

            Assert.IsInstanceOf<NbtString>(root["string"]);
            node = root["string"];
            Assert.AreEqual("string", node.Name);
            Assert.AreEqual("123", node.StringValue);
        }

        #endregion
    }
}
