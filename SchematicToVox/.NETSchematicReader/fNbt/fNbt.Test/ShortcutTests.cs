using System;
using System.Globalization;
using NUnit.Framework;

namespace fNbt.Test {
    [TestFixture]
    public class ShortcutTests {
        [Test]
        public void NbtByteTest() {
            object dummy;
            NbtTag test = new NbtByte(250);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteArrayValue);
            Assert.AreEqual(250, test.ByteValue);
            Assert.AreEqual((double)250, test.DoubleValue);
            Assert.AreEqual((float)250, test.FloatValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntArrayValue);
            Assert.AreEqual(250, test.IntValue);
            Assert.AreEqual(250L, test.LongValue);
            Assert.AreEqual(250, test.ShortValue);
            Assert.AreEqual("250", test.StringValue);
            Assert.IsTrue(test.HasValue);
        }


        [Test]
        public void NbtByteArrayTest() {
            object dummy;
            byte[] bytes = { 1, 2, 3, 4, 5 };
            NbtTag test = new NbtByteArray(bytes);
            CollectionAssert.AreEqual(bytes, test.ByteArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.DoubleValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.FloatValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.LongValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ShortValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.StringValue);
            Assert.IsTrue(test.HasValue);
        }


        [Test]
        public void NbtCompoundTest() {
            object dummy;
            NbtTag test = new NbtCompound("Derp");
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.DoubleValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.FloatValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.LongValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ShortValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.StringValue);
            Assert.IsFalse(test.HasValue);
        }


        [Test]
        public void NbtDoubleTest() {
            object dummy;
            NbtTag test = new NbtDouble(0.4931287132182315);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteValue);
            Assert.AreEqual(0.4931287132182315, test.DoubleValue);
            Assert.AreEqual((float)0.4931287132182315, test.FloatValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.LongValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ShortValue);
            Assert.AreEqual((0.4931287132182315).ToString(CultureInfo.InvariantCulture), test.StringValue);
            Assert.IsTrue(test.HasValue);
        }


        [Test]
        public void NbtFloatTest() {
            object dummy;
            NbtTag test = new NbtFloat(0.49823147f);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteValue);
            Assert.AreEqual((double)0.49823147f, test.DoubleValue);
            Assert.AreEqual(0.49823147f, test.FloatValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.LongValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ShortValue);
            Assert.AreEqual((0.49823147f).ToString(CultureInfo.InvariantCulture), test.StringValue);
            Assert.IsTrue(test.HasValue);
        }


        [Test]
        public void NbtIntTest() {
            object dummy;
            NbtTag test = new NbtInt(2147483647);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteValue);
            Assert.AreEqual((double)2147483647, test.DoubleValue);
            Assert.AreEqual((float)2147483647, test.FloatValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntArrayValue);
            Assert.AreEqual(2147483647, test.IntValue);
            Assert.AreEqual(2147483647L, test.LongValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ShortValue);
            Assert.AreEqual("2147483647", test.StringValue);
            Assert.IsTrue(test.HasValue);
        }


        [Test]
        public void NbtIntArrayTest() {
            object dummy;
            int[] ints = { 1111, 2222, 3333, 4444, 5555 };
            NbtTag test = new NbtIntArray(ints);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.DoubleValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.FloatValue);
            CollectionAssert.AreEqual(ints, test.IntArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.LongValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ShortValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.StringValue);
            Assert.IsTrue(test.HasValue);
        }


        [Test]
        public void NbtListTest() {
            object dummy;
            NbtTag test = new NbtList("Derp");
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.DoubleValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.FloatValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.LongValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ShortValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.StringValue);
            Assert.IsFalse(test.HasValue);
        }


        [Test]
        public void NbtLongTest() {
            object dummy;
            NbtTag test = new NbtLong(9223372036854775807);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteValue);
            Assert.AreEqual((double)9223372036854775807, test.DoubleValue);
            Assert.AreEqual((float)9223372036854775807, test.FloatValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntValue);
            Assert.AreEqual(9223372036854775807, test.LongValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ShortValue);
            Assert.AreEqual("9223372036854775807", test.StringValue);
            Assert.IsTrue(test.HasValue);
        }


        [Test]
        public void NbtShortTest() {
            object dummy;
            NbtTag test = new NbtShort(32767);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteValue);
            Assert.AreEqual((double)32767, test.DoubleValue);
            Assert.AreEqual((float)32767, test.FloatValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntArrayValue);
            Assert.AreEqual(32767, test.IntValue);
            Assert.AreEqual(32767L, test.LongValue);
            Assert.AreEqual(32767, test.ShortValue);
            Assert.AreEqual("32767", test.StringValue);
            Assert.IsTrue(test.HasValue);
        }


        [Test]
        public void NbtStringTest() {
            object dummy;
            NbtTag test = new NbtString("HELLO WORLD THIS IS A TEST STRING ÅÄÖ!");
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ByteValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.DoubleValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.FloatValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntArrayValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.IntValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.LongValue);
            Assert.Throws<InvalidCastException>(() => dummy = test.ShortValue);
            Assert.AreEqual("HELLO WORLD THIS IS A TEST STRING ÅÄÖ!", test.StringValue);
            Assert.IsTrue(test.HasValue);
        }
    }
}
