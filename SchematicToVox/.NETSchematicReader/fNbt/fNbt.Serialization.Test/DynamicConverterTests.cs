using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace fNbt.Serialization.Test {
    [TestFixture]
    public class DynamicConverterTests {

        [Test]
        public void PrimitiveTest() {
            NbtTag intTag = NbtConvert.MakeTag("derp", 1);
            Assert.IsInstanceOf<NbtInt>(intTag);
            Assert.Equals(intTag.IntValue, 1);
        }
    }
}