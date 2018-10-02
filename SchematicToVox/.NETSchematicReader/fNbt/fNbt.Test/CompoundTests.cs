using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace fNbt.Test {
    [TestFixture]
    public sealed class CompoundTests {
        [Test]
        public void InitializingCompoundFromCollectionTest() {
            NbtTag[] allNamed = {
                new NbtShort("allNamed1", 1),
                new NbtLong("allNamed2", 2),
                new NbtInt("allNamed3", 3)
            };

            NbtTag[] someUnnamed = {
                new NbtInt("someUnnamed1", 1),
                new NbtInt(2),
                new NbtInt("someUnnamed3", 3)
            };

            NbtTag[] someNull = {
                new NbtInt("someNull1", 1),
                null,
                new NbtInt("someNull3", 3)
            };

            NbtTag[] dupeNames = {
                new NbtInt("dupeNames1", 1),
                new NbtInt("dupeNames2", 2),
                new NbtInt("dupeNames1", 3)
            };

            // null collection, should throw
            Assert.Throws<ArgumentNullException>(() => new NbtCompound("nullTest", null));

            // proper initialization
            NbtCompound allNamedTest = null;
            Assert.DoesNotThrow(() => allNamedTest = new NbtCompound("allNamedTest", allNamed));
            CollectionAssert.AreEquivalent(allNamed, allNamedTest);

            // some tags are unnamed, should throw
            Assert.Throws<ArgumentException>(() => new NbtCompound("someUnnamedTest", someUnnamed));

            // some tags are null, should throw
            Assert.Throws<ArgumentNullException>(() => new NbtCompound("someNullTest", someNull));

            // some tags have same names, should throw
            Assert.Throws<ArgumentException>(() => new NbtCompound("dupeNamesTest", dupeNames));
        }


        [Test]
        public void GettersAndSetters() {
            // construct a document for us to test.
            var nestedChild = new NbtCompound("NestedChild");
            var nestedInt = new NbtInt(1);
            var nestedChildList = new NbtList("NestedChildList") {
                nestedInt
            };
            var child = new NbtCompound("Child") {
                nestedChild,
                nestedChildList
            };
            var childList = new NbtList("ChildList") {
                new NbtInt(1)
            };
            var parent = new NbtCompound("Parent") {
                child,
                childList
            };

            // Accessing nested compound tags using indexers
            Assert.AreEqual(nestedChild, parent["Child"]["NestedChild"]);
            Assert.AreEqual(nestedChildList, parent["Child"]["NestedChildList"]);
            Assert.AreEqual(nestedInt, parent["Child"]["NestedChildList"][0]);

            // Accessing nested compound tags using Get and Get<T>
            Assert.Throws<ArgumentNullException>(() => parent.Get<NbtCompound>(null));
            Assert.IsNull(parent.Get<NbtCompound>("NonExistingChild"));
            Assert.AreEqual(nestedChild, parent.Get<NbtCompound>("Child").Get<NbtCompound>("NestedChild"));
            Assert.AreEqual(nestedChildList, parent.Get<NbtCompound>("Child").Get<NbtList>("NestedChildList"));
            Assert.AreEqual(nestedInt, parent.Get<NbtCompound>("Child").Get<NbtList>("NestedChildList")[0]);
            Assert.Throws<ArgumentNullException>(() => parent.Get(null));
            Assert.IsNull(parent.Get("NonExistingChild"));
            Assert.AreEqual(nestedChild, (parent.Get("Child") as NbtCompound).Get("NestedChild"));
            Assert.AreEqual(nestedChildList, (parent.Get("Child") as NbtCompound).Get("NestedChildList"));
            Assert.AreEqual(nestedInt, (parent.Get("Child") as NbtCompound).Get("NestedChildList")[0]);

            // Accessing with Get<T> and an invalid given type
            Assert.Throws<InvalidCastException>(() => parent.Get<NbtInt>("Child"));

            // Using TryGet and TryGet<T>
            NbtTag dummyTag;
            Assert.Throws<ArgumentNullException>(() => parent.TryGet(null, out dummyTag));
            Assert.IsFalse(parent.TryGet("NonExistingChild", out dummyTag));
            Assert.IsTrue(parent.TryGet("Child", out dummyTag));
            NbtCompound dummyCompoundTag;
            Assert.Throws<ArgumentNullException>(() => parent.TryGet(null, out dummyCompoundTag));
            Assert.IsFalse(parent.TryGet("NonExistingChild", out dummyCompoundTag));
            Assert.IsTrue(parent.TryGet("Child", out dummyCompoundTag));

            // Trying to use integer indexers on non-NbtList tags
            Assert.Throws<InvalidOperationException>(() => parent[0] = nestedInt);
            Assert.Throws<InvalidOperationException>(() => nestedInt[0] = nestedInt);

            // Trying to use string indexers on non-NbtCompound tags
            Assert.Throws<InvalidOperationException>(() => childList["test"] = nestedInt);
            Assert.Throws<InvalidOperationException>(() => nestedInt["test"] = nestedInt);

            // Trying to get a non-existent element by name
            Assert.IsNull(parent.Get<NbtTag>("NonExistentTag"));
            Assert.IsNull(parent["NonExistentTag"]);

            // Null indices on NbtCompound
            Assert.Throws<ArgumentNullException>(() => parent.Get<NbtTag>(null));
            Assert.Throws<ArgumentNullException>(() => parent[null] = new NbtInt(1));
            Assert.Throws<ArgumentNullException>(() => nestedInt = (NbtInt)parent[null]);

            // Out-of-range indices on NbtList
            Assert.Throws<ArgumentOutOfRangeException>(() => nestedInt = (NbtInt)childList[-1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => childList[-1] = new NbtInt(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => nestedInt = childList.Get<NbtInt>(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => nestedInt = (NbtInt)childList[childList.Count]);
            Assert.Throws<ArgumentOutOfRangeException>(() => nestedInt = childList.Get<NbtInt>(childList.Count));

            // Using setter correctly
            parent["NewChild"] = new NbtByte("NewChild");

            // Using setter incorrectly
            object dummyObject;
            Assert.Throws<ArgumentNullException>(() => parent["Child"] = null);
            Assert.NotNull(parent["Child"]);
            Assert.Throws<ArgumentException>(() => parent["Child"] = new NbtByte("NotChild"));
            Assert.Throws<InvalidOperationException>(() => dummyObject = parent[0]);
            Assert.Throws<InvalidOperationException>(() => parent[0] = new NbtByte("NewerChild"));

            // Try adding tag to self
            var selfTest = new NbtCompound("SelfTest");
            Assert.Throws<ArgumentException>(() => selfTest["SelfTest"] = selfTest);

            // Try adding a tag that already has a parent
            Assert.Throws<ArgumentException>(() => selfTest[child.Name] = child);
        }


        [Test]
        public void Renaming() {
            var tagToRename = new NbtInt("DifferentName", 1);
            var compound = new NbtCompound {
                new NbtInt("SameName", 1),
                tagToRename
            };

            // proper renaming, should not throw
            tagToRename.Name = "SomeOtherName";

            // attempting to use a duplicate name
            Assert.Throws<ArgumentException>(() => tagToRename.Name = "SameName");

            // assigning a null name to a tag inside a compound; should throw
            Assert.Throws<ArgumentNullException>(() => tagToRename.Name = null);

            // assigning a null name to a tag that's been removed; should not throw
            compound.Remove(tagToRename);
            tagToRename.Name = null;
        }


        [Test]
        public void AddingAndRemoving() {
            var foo = new NbtInt("Foo");
            var test = new NbtCompound {
                foo
            };

            // adding duplicate object
            Assert.Throws<ArgumentException>(() => test.Add(foo));

            // adding duplicate name
            Assert.Throws<ArgumentException>(() => test.Add(new NbtByte("Foo")));

            // adding unnamed tag
            Assert.Throws<ArgumentException>(() => test.Add(new NbtInt()));

            // adding null
            Assert.Throws<ArgumentNullException>(() => test.Add(null));

            // adding tag to self
            Assert.Throws<ArgumentException>(() => test.Add(test));

            // contains existing name/object
            Assert.IsTrue(test.Contains("Foo"));
            Assert.IsTrue(test.Contains(foo));
            Assert.Throws<ArgumentNullException>(() => test.Contains((string)null));
            Assert.Throws<ArgumentNullException>(() => test.Contains((NbtTag)null));

            // contains non-existent name
            Assert.IsFalse(test.Contains("Bar"));

            // contains existing name / different object
            Assert.IsFalse(test.Contains(new NbtInt("Foo")));

            // removing non-existent name
            Assert.Throws<ArgumentNullException>(() => test.Remove((string)null));
            Assert.IsFalse(test.Remove("Bar"));

            // removing existing name
            Assert.IsTrue(test.Remove("Foo"));

            // removing non-existent name
            Assert.IsFalse(test.Remove("Foo"));

            // re-adding object
            test.Add(foo);

            // removing existing object
            Assert.Throws<ArgumentNullException>(() => test.Remove((NbtTag)null));
            Assert.IsTrue(test.Remove(foo));
            Assert.IsFalse(test.Remove(foo));

            // clearing an empty NbtCompound
            Assert.AreEqual(0, test.Count);
            test.Clear();

            // re-adding after clearing
            test.Add(foo);
            Assert.AreEqual(1, test.Count);

            // clearing a non-empty NbtCompound
            test.Clear();
            Assert.AreEqual(0, test.Count);
        }


        [Test]
        public void UtilityMethods() {
            NbtTag[] testThings = {
                new NbtShort("Name1", 1),
                new NbtInt("Name2", 2),
                new NbtLong("Name3", 3)
            };
            var compound = new NbtCompound();

            // add range
            compound.AddRange(testThings);

            // add range with duplicates
            Assert.Throws<ArgumentException>(() => compound.AddRange(testThings));
        }


        [Test]
        public void InterfaceImplementations() {
            NbtTag[] tagList = {
                new NbtByte("First", 1), new NbtShort("Second", 2), new NbtInt("Third", 3),
                new NbtLong("Fourth", 4L)
            };

            // test NbtCompound(IEnumerable<NbtTag>) constructor
            var comp = new NbtCompound(tagList);

            // test .Names and .Tags collections
            CollectionAssert.AreEquivalent(new[] {
                "First", "Second", "Third", "Fourth"
            }, comp.Names);
            CollectionAssert.AreEquivalent(tagList, comp.Tags);

            // test ICollection and ICollection<NbtTag> boilerplate properties
            ICollection<NbtTag> iGenCollection = comp;
            Assert.IsFalse(iGenCollection.IsReadOnly);
            ICollection iCollection = comp;
            Assert.NotNull(iCollection.SyncRoot);
            Assert.IsFalse(iCollection.IsSynchronized);

            // test CopyTo()
            var tags = new NbtTag[iCollection.Count];
            iCollection.CopyTo(tags, 0);
            CollectionAssert.AreEquivalent(comp, tags);

            // test non-generic GetEnumerator()
            var enumeratedTags = comp.ToList();
            CollectionAssert.AreEquivalent(tagList, enumeratedTags);

            // test generic GetEnumerator()
            List<NbtTag> enumeratedTags2 = new List<NbtTag>();
            var enumerator = comp.GetEnumerator();
            while (enumerator.MoveNext()) {
                enumeratedTags2.Add(enumerator.Current);
            }
            CollectionAssert.AreEquivalent(tagList, enumeratedTags2);
        }
    }
}
