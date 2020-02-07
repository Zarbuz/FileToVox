//#define Exclude_Check_For_Set_Modifications_In_Enumerator
//#define Exclude_Check_For_Is_Disposed_In_Enumerator
//#define Exclude_No_Hash_Array_Implementation
//#define Exclude_Cache_Optimize_Resize

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Motvin.Collections
{
    // didn't implement ISerializable and IDeserializationCallback
    // these are implemented in the .NET HashSet
    // the 7th HashSet constructor has params for serialization -implement that if serialization is implemented
    // also add using System.Runtime.Serialization;

    public class FastHashSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ISet<T>
    {
        private const int MaxSlotsArraySize = int.MaxValue - 2;

        // this is the size of the non-hash array used to make small counts of items faster
        private const int InitialArraySize = 8;

        // this is the # of initial nodes for the slots array after going into hashing after using the noHashArray
        // this is 16 + 1; the + 1 is for the first node (node at index 0) which doesn't get used because 0 is the NullIndex
        private const int InitialSlotsArraySize = 17;

        // this indicates end of chain if the nextIndex of a node has this value and also indicates no chain if a buckets array element has this value
        private const int NullIndex = 0;

        // if a node's nextIndex = this value, then it is a blank node - this isn't a valid nextIndex when unmarked and also when marked (because we don't allow int.MaxValue items)
        private const int BlankNextIndexIndicator = int.MaxValue;

        // use this instead of the negate negative logic when getting hashindex - this saves an if (hashindex < 0) which can be the source of bad branch prediction
        private const int HighBitNotSet = unchecked((int)0b0111_1111_1111_1111_1111_1111_1111_1111);

        // The Mark... constants below are for marking, unmarking, and checking if an item is marked.
        // This is usefull for some set operations.

        // doing an | (bitwise or) with this and the nextIndex marks the node, setting the bit back will give the original nextIndex value
        private const int MarkNextIndexBitMask = unchecked((int)0b1000_0000_0000_0000_0000_0000_0000_0000);

        // doing an & (bitwise and) with this and the nextIndex sets it back to the original value (unmarks it)
        private const int MarkNextIndexBitMaskInverted = ~MarkNextIndexBitMask;

        // FastHashSet doesn't allow using an item/node index as high as int.MaxValue.
        // There are 2 reasons for this: The first is that int.MaxValue is used as a special indicator
        private const int LargestPrimeLessThanMaxInt = 2147483629;

        // these are primes above the .75 loadfactor of the power of 2 except from 30,000 through 80,000, where we conserve space to help with cache space
        private static readonly int[] bucketsSizeArray = { 11, 23, 47, 89, 173, 347, 691, 1367, 2741, 5471, 10_937, 19_841/*16_411/*21_851*/, 40_241/*32_771/*43_711*/, 84_463/*65_537/*87_383*/, /*131_101*/174_767,
			/*262_147*/349_529, 699_053, 1_398_107, 2_796_221, 5_592_407, 11_184_829, 22_369_661, 44_739_259, 89_478_503, 17_8956_983, 35_7913_951, 715_827_947, 143_1655_777, LargestPrimeLessThanMaxInt};

        // the buckets array can be pre-allocated to a large size, but it's not good to use that entire size for hashing because of cache locality
        // instead do at most 3 size steps (for 3 levels of cache) before using its actual allocated size

        // when an initial capacity is selected in the constructor or later, allocate the required space for the buckets array, but only use a subset of this space until the load factor is met
        // limit the # of used elements to optimize for cpu caches
        private static readonly int[] bucketsSizeArrayForCacheOptimization = { 3_371, 62_851, 701_819 };

        private const double LoadFactorConst = .75;

        private int currentIndexIntoBucketsSizeArray;

        private int bucketsModSize;

#if !Exclude_Check_For_Set_Modifications_In_Enumerator
        private int incrementForEverySetModification;
#endif

        // resize the buckets array when the count reaches this value
        private int resizeBucketsCountThreshold;

        private int count;

        private int nextBlankIndex;

        // this is needed because if items are removed, they get added into the blank list starting at nextBlankIndex, but we may want to TrimExcess capacity, so this is a quick way to see what the ExcessCapacity is
        private int firstBlankAtEndIndex;

        private IEqualityComparer<T> comparer;

        // make the buckets size a primary number to make the mod function less predictable
        private int[] buckets;

        private TNode[] slots;

#if !Exclude_No_Hash_Array_Implementation
        // used for small sets - when the count of items is small, it is usually faster to just use an array of the items and not do hashing at all (this can also use slightly less memory)
        // There may be some cases where the sets can be very small, but there can be very many of these sets.  This can be good for these cases.
        private T[] noHashArray;
#endif

        internal enum FoundType
        {
            FoundFirstTime,
            FoundNotFirstTime,
            NotFound
        }

        internal struct TNode
        {
            // the cached hash code of the item - this is so we don't have to call GetHashCode multiple times, also doubles as a nextIndex for blanks, since blank nodes don't need a hash code
            public int hashOrNextIndexForBlanks;

            public int nextIndex;

            public T item;

            public TNode(T elem, int nextIndex, int hash)
            {
                this.item = elem;

                this.nextIndex = nextIndex;

                this.hashOrNextIndexForBlanks = hash;
            }
        }

        // 1 - same constructor params as HashSet
        /// <summary>Initializes a new instance of the FastHashSet&lt;<typeparamref name="T"/>&gt;.</summary>
        /// <typeparam name="T">The element type of the FastHashSet.</typeparam>
        public FastHashSet()
        {
            comparer = EqualityComparer<T>.Default;
            SetInitialCapacity(InitialArraySize);
        }

        // 2 - same constructor params as HashSet
        /// <summary>Initializes a new instance of the FastHashSet&lt;<typeparamref name="T"/>&gt;.</summary>
        /// <typeparam name="T">The element type of the FastHashSet.</typeparam>
        /// <param name="collection">The collection to initially add to the FastHashSet.</param>
        public FastHashSet(IEnumerable<T> collection)
        {
            comparer = EqualityComparer<T>.Default;
            AddInitialEnumerable(collection);
        }

        // 3 - same constructor params as HashSet
        /// <summary>Initializes a new instance of the FastHashSet&lt;<typeparamref name="T"/>&gt;.</summary>
        /// <typeparam name="T">The element type of the FastHashSet.</typeparam>
        /// <param name="comparer">The IEqualityComparer to use for determining equality of elements in the FastHashSet.</param>
        public FastHashSet(IEqualityComparer<T> comparer)
        {
            this.comparer = comparer ?? EqualityComparer<T>.Default;
            SetInitialCapacity(InitialArraySize);
        }

        // 4 - same constructor params as HashSet
        /// <summary>Initializes a new instance of the FastHashSet&lt;<typeparamref name="T"/>&gt;.</summary>
        /// <typeparam name="T">The element type of the FastHashSet.</typeparam>
        /// <param name="capacity">The initial capacity of the FastHashSet.</param>
        public FastHashSet(int capacity)
        {
            comparer = EqualityComparer<T>.Default;
            SetInitialCapacity(capacity);
        }

        // 5 - same constructor params as HashSet
        /// <summary>Initializes a new instance of the FastHashSet&lt;<typeparamref name="T"/>&gt;.</summary>
        /// <typeparam name="T">The element type of the FastHashSet</typeparam>
        /// <param name="collection">The collection to initially add to the FastHashSet.</param>
        /// <param name="comparer">The IEqualityComparer to use for determining equality of elements in the FastHashSet.</param>
        public FastHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            this.comparer = comparer ?? EqualityComparer<T>.Default;
            AddInitialEnumerable(collection);
        }

        // 6 - same constructor params as HashSet
        /// <summary>Initializes a new instance of the FastHashSet&lt;<typeparamref name="T"/>&gt;.</summary>
        /// <typeparam name="T">The element type of the set</typeparam>
        /// <param name="capacity">The initial capacity of the FastHashSet.</param>
        /// <param name="comparer">The IEqualityComparer to use for determining equality of elements in the FastHashSet.</param>
        public FastHashSet(int capacity, IEqualityComparer<T> comparer)
        {
            this.comparer = comparer ?? EqualityComparer<T>.Default;
            SetInitialCapacity(capacity);
        }

        /// <summary>Initializes a new instance of the FastHashSet&lt;<typeparamref name="T"/>&gt;.</summary>
        /// <typeparam name="T">The element type of the FastHashSet</typeparam>
        /// <param name="collection">The collection to initially add to the FastHashSet.</param>
        /// <param name="areAllCollectionItemsDefinitelyUnique">True if the collection items are all unique.  The collection items can be added more quickly if they are known to be unique.</param>
        /// <param name="capacity">The initial capacity of the FastHashSet.</param>
        /// <param name="comparer">The IEqualityComparer to use for determining equality of elements in the FastHashSet.</param>
#if false // removed for now because it's probably not that useful and needs some changes to be correct
		public FastHashSet(IEnumerable<T> collection, bool areAllCollectionItemsDefinitelyUnique, int capacity, IEqualityComparer<T> comparer = null)
		{
			this.comparer = comparer ?? EqualityComparer<T>.Default;
			SetInitialCapacity(capacity);

			if (areAllCollectionItemsDefinitelyUnique)
			{
				// this and the call below must deal correctly with an initial capacity already set
				AddInitialUniqueValuesEnumerable(collection);
			}
			else
			{
				AddInitialEnumerable(collection);
			}
		}
#endif

        private void AddInitialUniqueValuesEnumerable(IEnumerable<T> collection)
        {
            int itemsCount = 0;
#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                nextBlankIndex = 1;
                foreach (T item in collection)
                {
                    int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                    int hashIndex = hash % bucketsModSize;

                    int index = buckets[hashIndex];
                    buckets[hashIndex] = nextBlankIndex;

                    ref TNode t = ref slots[nextBlankIndex];

                    t.hashOrNextIndexForBlanks = hash;
                    t.nextIndex = index;
                    t.item = item;

                    nextBlankIndex++;
                    itemsCount++;
                }
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                foreach (T item in collection)
                {
                    noHashArray[itemsCount++] = item;
                }
            }
#endif
            count = itemsCount;
            firstBlankAtEndIndex = nextBlankIndex;
        }

        private void AddInitialEnumerableWithEnoughCapacity(IEnumerable<T> collection)
        {
            // this assumes we are hashing
            foreach (T item in collection)
            {
                int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                int hashIndex = hash % bucketsModSize;

                for (int index = buckets[hashIndex]; index != NullIndex;)
                {
                    ref TNode t = ref slots[index];

                    if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                    {
                        goto Found; // item was found
                    }

                    index = t.nextIndex;
                }

                ref TNode tBlank = ref slots[nextBlankIndex];

                tBlank.hashOrNextIndexForBlanks = hash;
                tBlank.nextIndex = buckets[hashIndex];
                tBlank.item = item;

                buckets[hashIndex] = nextBlankIndex;

                nextBlankIndex++;

#if !Exclude_Cache_Optimize_Resize
                count++;

                if (count >= resizeBucketsCountThreshold)
                {
                    ResizeBucketsArrayForward(GetNewBucketsArraySize());
                }
#endif
            Found:;
            }
            firstBlankAtEndIndex = nextBlankIndex;
#if Exclude_Cache_Optimize_Resize
			count = nextBlankIndex - 1;
#endif
        }

        private void AddInitialEnumerable(IEnumerable<T> collection)
        {
            FastHashSet<T> fhset = collection as FastHashSet<T>;
            if (fhset != null && Equals(fhset.Comparer, Comparer))
            {
                // a set with the same item comparer must have all items unique
                // so Count will be the exact Count of the items added
                // also don't have to check for equals of items
                // and a FastHashSet has the additional advantage of not having to call GetHashCode() if it is hashing
                // and it has access to the internal slots array so we don't have to use the foreach/enumerator

                int count = fhset.Count;
                SetInitialCapacity(count);

#if !Exclude_No_Hash_Array_Implementation
                if (IsHashing)
                {
                    if (fhset.IsHashing)
                    {
#endif
                        // this FastHashSet is hashing and collection is a FastHashSet (with equal comparer) and it is also hashing

                        nextBlankIndex = 1;
                        int maxNodeIndex = fhset.slots.Length - 1;
                        if (fhset.firstBlankAtEndIndex <= maxNodeIndex)
                        {
                            maxNodeIndex = fhset.firstBlankAtEndIndex - 1;
                        }

                        for (int i = 1; i <= maxNodeIndex; i++)
                        {
                            ref TNode t2 = ref fhset.slots[i];
                            if (t2.nextIndex != BlankNextIndexIndicator)
                            {
                                int hash = t2.hashOrNextIndexForBlanks;
                                int hashIndex = hash % bucketsModSize;

                                ref TNode t = ref slots[nextBlankIndex];

                                t.hashOrNextIndexForBlanks = hash;
                                t.nextIndex = buckets[hashIndex]; ;
                                t.item = t2.item;

                                buckets[hashIndex] = nextBlankIndex;

                                nextBlankIndex++;
                            }
                        }
                        this.count = count;
                        firstBlankAtEndIndex = nextBlankIndex;
#if !Exclude_No_Hash_Array_Implementation
                    }
                    else
                    {
                        // this FastHashSet is hashing and collection is a FastHashSet (with equal comparer) and it is NOT hashing

                        nextBlankIndex = 1;
                        for (int i = 0; i < fhset.count; i++)
                        {
                            ref T item = ref noHashArray[i];

                            int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                            int hashIndex = hash % bucketsModSize;

                            ref TNode t = ref slots[nextBlankIndex];

                            t.hashOrNextIndexForBlanks = hash;
                            t.nextIndex = buckets[hashIndex];
                            t.item = item;

                            buckets[hashIndex] = nextBlankIndex;

                            nextBlankIndex++;
                        }
                    }
                }
                else
                {
                    // this FastHashSet is not hashing

                    AddInitialUniqueValuesEnumerable(collection);
                }
#endif
            }
            else
            {
                // collection is not a FastHashSet with equal comparer

                HashSet<T> hset = collection as HashSet<T>;
                if (hset != null && Equals(hset.Comparer, Comparer))
                {
                    // a set with the same item comparer must have all items unique
                    // so Count will be the exact Count of the items added
                    // also don't have to check for equals of items

                    int usedCount = hset.Count;
                    SetInitialCapacity(usedCount);

                    AddInitialUniqueValuesEnumerable(collection);
                }
                else
                {
                    ICollection<T> coll = collection as ICollection<T>;
                    if (coll != null)
                    {
                        SetInitialCapacity(coll.Count);
#if !Exclude_No_Hash_Array_Implementation
                        if (IsHashing)
                        {
#endif
                            // call SetInitialCapacity and then set the capacity back to get rid of the excess?

                            AddInitialEnumerableWithEnoughCapacity(collection);

                            TrimExcess();
#if !Exclude_No_Hash_Array_Implementation
                        }
                        else
                        {
                            foreach (T item in collection)
                            {
                                Add(item);
                            }
                        }
#endif
                    }
                    else
                    {
                        SetInitialCapacity(InitialArraySize);

                        foreach (T item in collection)
                        {
                            Add(in item);
                        }
                    }
                }
            }
        }

        private void SetInitialCapacity(int capacity)
        {
#if !Exclude_No_Hash_Array_Implementation
            if (capacity > InitialArraySize)
            {
#endif
                // skip using the array and go right into hashing
                InitHashing(capacity);
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                CreateNoHashArray(); // don't set the capacity/size of the noHashArray
            }
#endif
        }

#if !Exclude_No_Hash_Array_Implementation
        // this function can be called to switch from using the noHashArray and start using the hashing arrays (slots and buckets)
        // this function can also be called before noHashArray is even allocated in order to skip using the array and go right into hashing
        private void SwitchToHashing(int capacityIncrease = -1)
        {
            InitHashing(capacityIncrease);

            if (noHashArray != null)
            {
                // i is the index into noHashArray
                for (int i = 0; i < count; i++)
                {
                    ref T item = ref noHashArray[i];

                    int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                    int hashIndex = hash % bucketsModSize;

                    ref TNode t = ref slots[nextBlankIndex];

                    t.hashOrNextIndexForBlanks = hash;
                    t.nextIndex = buckets[hashIndex];
                    t.item = item;

                    buckets[hashIndex] = nextBlankIndex;

                    nextBlankIndex++;
                }
                noHashArray = null; // this array can now be garbage collected because it is no longer referenced
            }

            firstBlankAtEndIndex = nextBlankIndex;
        }
#endif

        private void InitHashing(int capacity = -1)
        {
            int newSlotsArraySize;
            int newBucketsArraySize;
            int newBucketsArrayModSize;

            bool setThresh = false;
            if (capacity == -1)
            {
                newSlotsArraySize = InitialSlotsArraySize;

                newBucketsArraySize = bucketsSizeArray[0];
                if (newBucketsArraySize < newSlotsArraySize)
                {
                    for (currentIndexIntoBucketsSizeArray = 1; currentIndexIntoBucketsSizeArray < bucketsSizeArray.Length; currentIndexIntoBucketsSizeArray++)
                    {
                        newBucketsArraySize = bucketsSizeArray[currentIndexIntoBucketsSizeArray];
                        if (newBucketsArraySize >= newSlotsArraySize)
                        {
                            break;
                        }
                    }
                }
                newBucketsArrayModSize = newBucketsArraySize;
            }
            else
            {
                newSlotsArraySize = capacity + 1; // add 1 to accomodate blank first node (node at 0 index)

                newBucketsArraySize = FastHashSetUtil.GetEqualOrClosestHigherPrime((int)(newSlotsArraySize / LoadFactorConst));

#if !Exclude_Cache_Optimize_Resize
                if (newBucketsArraySize > bucketsSizeArrayForCacheOptimization[0])
                {
                    newBucketsArrayModSize = bucketsSizeArrayForCacheOptimization[0];
                    setThresh = true;
                }
                else
#endif
                {
                    newBucketsArrayModSize = newBucketsArraySize;
                }
            }

            if (newSlotsArraySize == 0)
            {
                // this is an error, the int.MaxValue has been used for capacity and we require more - throw an Exception for this
                // could try this with HashSet and see what exception it throws?
                throw new InvalidOperationException("Exceeded maximum number of items allowed for this container.");
            }

            slots = new TNode[newSlotsArraySize]; // the slots array has an extra item as it's first item (0 index) that is for available items - the memory is wasted, but it simplifies things
            buckets = new int[newBucketsArraySize]; // these will be initially set to 0, so make 0 the blank(available) value and reduce all indices by one to get to the actual index into the slots array
            bucketsModSize = newBucketsArrayModSize;

            if (setThresh)
            {
                resizeBucketsCountThreshold = (int)(newBucketsArrayModSize * LoadFactorConst);
            }
            else
            {
                CalcUsedItemsLoadFactorThreshold();
            }

            nextBlankIndex = 1; // start at 1 because 0 is the blank item

            firstBlankAtEndIndex = nextBlankIndex;
        }

#if !Exclude_No_Hash_Array_Implementation
        private void CreateNoHashArray()
        {
            noHashArray = new T[InitialArraySize];
        }
#endif

        private void CalcUsedItemsLoadFactorThreshold()
        {
            if (buckets != null)
            {
                if (buckets.Length == bucketsModSize)
                {
                    resizeBucketsCountThreshold = slots.Length; // with this value, the buckets array should always resize after the slots array (in the same public function call)
                }
                else
                {
                    // when buckets.Length > bucketsModSize, this means we want to more slowly increase the bucketsModSize to keep things in the L1-3 caches
                    resizeBucketsCountThreshold = (int)(bucketsModSize * LoadFactorConst);
                }
            }
        }

        /// <summary>True if the FastHashSet if read-only.  This is always false.  This is only present to implement ICollection<T>, it has no real value otherwise.</summary>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>Copies all elements of the FastHashSet&lt;<typeparamref name="T"/>&gt; into an array starting at arrayIndex.  This implements ICollection<T>.CopyTo(T[], Int32).</summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The starting array index to copy elements to.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex, count);
        }

        /// <summary>Copies all elements of the FastHashSet&lt;<typeparamref name="T"/>&gt; into an array starting at the first array index.</summary>
        /// <param name="array">The destination array.</param>
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0, count);
        }

        // not really sure how this can be useful because you never know exactly what elements you will get copied (unless you copy them all)
        // it could easily vary for different implementations or if items were added in different order or if items were added removed and then added, instead of just added 
        /// <summary>Copies count number of elements of the FastHashSet&lt;<typeparamref name="T"/>&gt; into an array starting at arrayIndex.</summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The starting array index to copy elements to.</param>
        /// <param name="count">The number of elements to copy.</param>
        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array), "Value cannot be null.");
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Non negative number is required.");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Non negative number is required.");
            }

            if (arrayIndex + count > array.Length)
            {
                throw new ArgumentException("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
            }

            if (count == 0)
            {
                return;
            }

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int pastNodeIndex = slots.Length;
                if (firstBlankAtEndIndex < pastNodeIndex)
                {
                    pastNodeIndex = firstBlankAtEndIndex;
                }

                int cnt = 0;
                for (int i = 1; i < pastNodeIndex; i++)
                {
                    if (slots[i].nextIndex != BlankNextIndexIndicator)
                    {
                        array[arrayIndex++] = slots[i].item;
                        if (++cnt == count)
                        {
                            break;
                        }
                    }
                }
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                int cnt = this.count;
                if (cnt > count)
                {
                    cnt = count;
                }

                // for small arrays, I think the for loop below will actually be faster than Array.Copy because of the overhead of that function - could test this
                //Array.Copy(noHashArray, 0, array, arrayIndex, cnt);

                for (int i = 0; i < cnt; i++)
                {
                    array[arrayIndex++] = noHashArray[i];
                }
            }
#endif
        }

        /// <summary>
        /// Gets the IEqualityComparer used to determine equality for items of this FastHashSet.
        /// </summary>
        public IEqualityComparer<T> Comparer
        {
            get
            {
                // if not set, return the default - this is what HashSet does
                // even if it is set to null explicitly, it will still return the default
                // this behavior is implmented in the constructor
                return comparer;
            }
        }

        /// <summary>
        /// >Gets the number of items in this FastHashSet.
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
        }

        // this is the percent of used items to all items (used + blank/available)
        // at which point any additional added items will
        // first resize the buckets array to the next prime to avoid too many collisions and chains becoming too large
        /// <summary>
        /// Gets the fraction of 'used items count' divided by 'used items plus available/blank items count'.
        /// The buckets array is resized when adding items and this fraction is reached, so this is the minimum LoadFactor for the buckets array.
        /// </summary>
        public double LoadFactor
        {
            get
            {
                return LoadFactorConst;
            }
        }

        // this is the capacity that can be trimmed with TrimExcessCapacity
        // items that were removed from the hash arrays can't be trimmed by calling TrimExcessCapacity, only the blank items at the end
        // items that were removed from the noHashArray can be trimmed by calling TrimExcessCapacity because the items after are moved to fill the blank space
        /// <summary>
        /// Gets the capacity that can be trimmed with TrimExcessCapacity.
        /// </summary>
        public int ExcessCapacity
        {
            get
            {
                int excessCapacity;
#if !Exclude_No_Hash_Array_Implementation
                if (IsHashing)
                {
#endif
                    excessCapacity = slots.Length - firstBlankAtEndIndex;
#if !Exclude_No_Hash_Array_Implementation
                }
                else
                {
                    excessCapacity = noHashArray.Length - count;
                }
#endif
                return excessCapacity;
            }
        }

        /// <summary>
        /// Gets the capacity of the FastHashSet, which is the number of elements that can be contained without resizing.
        /// </summary>
        public int Capacity
        {
            get
            {
#if !Exclude_No_Hash_Array_Implementation
                if (IsHashing)
                {
#endif
                    return slots.Length - 1; // subtract 1 for blank node at 0 index
#if !Exclude_No_Hash_Array_Implementation
                }
                else
                {
                    return noHashArray.Length;
                }
#endif
            }
        }

        /// <summary>
        /// Gets the size of the next capacity increase of the FastHashSet.
        /// </summary>
        public int NextCapacityIncreaseSize
        {
            get
            {
                return GetNewSlotsArraySizeIncrease(out int oldSlotsArraySize);
            }
        }

        /// <summary>
        /// Gets the count of items when the next capacity increase (resize) of the FastHashSet will happen.
        /// </summary>
        public int NextCapacityIncreaseAtCount
        {
            get
            {
                return resizeBucketsCountThreshold;
            }
        }

        public bool IsHashing
        {
            get => noHashArray == null;
        }

        // the actual capacity at the end of this function may be more than specified
        // (in the case when it was more before this function was called - nothing is trimmed by this function, or in the case that slighly more capacity was allocated by this function)
        /// <summary>
        /// Allocate enough space (or make sure existing space is enough) for capacity number of items to be stored in the FastHashSet without any further allocations.
        /// </summary>
        /// <param name="capacity">The capacity to ensure.</param>
        /// <returns>The actual capacity at the end of this function.</returns>
        public int EnsureCapacity(int capacity)
        {
            // this function is only in .net core for HashSet as of 4/15/2019
#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

            int currentCapacity;

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                currentCapacity = slots.Length - count;
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                currentCapacity = noHashArray.Length - count;
            }
#endif

            if (currentCapacity < capacity)
            {
                IncreaseCapacity(capacity - currentCapacity);
            }

            // this should be the number where the next lowest number would force a resize of buckets array with the current loadfactor and the entire slots array is full
            int calcedNewBucketsArraySize = (int)(slots.Length / LoadFactorConst) + 1;

            if (calcedNewBucketsArraySize < 0 && calcedNewBucketsArraySize > LargestPrimeLessThanMaxInt)
            {
                calcedNewBucketsArraySize = LargestPrimeLessThanMaxInt;
            }
            else
            {
                calcedNewBucketsArraySize = FastHashSetUtil.GetEqualOrClosestHigherPrime(calcedNewBucketsArraySize);
            }

            if (buckets.Length < calcedNewBucketsArraySize)
            {
                // -1 means stop trying to increase the size based on the array of primes
                // instead calc based on 2 * existing length and then get the next higher prime
                currentIndexIntoBucketsSizeArray = -1;

                ResizeBucketsArrayForward(calcedNewBucketsArraySize);
            }

            return slots.Length - count;
        }

        // return true if bucketsModSize was set, false otherwise
        private bool CheckForModSizeIncrease()
        {
            if (bucketsModSize < buckets.Length)
            {
                // instead of array, just have 3 constants
                int partLength = (int)(buckets.Length * .75);

                int size0 = bucketsSizeArrayForCacheOptimization[0];
                int size1 = bucketsSizeArrayForCacheOptimization[1];
                if (bucketsModSize == size0)
                {
                    if (size1 <= partLength)
                    {
                        bucketsModSize = size1;
                        return true;
                    }
                    else
                    {
                        bucketsModSize = buckets.Length;
                        return true;
                    }
                }
                else
                {
                    int size2 = bucketsSizeArrayForCacheOptimization[2];
                    if (bucketsModSize == size1)
                    {
                        if (size2 <= partLength)
                        {
                            bucketsModSize = size2;
                            return true;
                        }
                        else
                        {
                            bucketsModSize = buckets.Length;
                            return true;
                        }
                    }
                    else if (bucketsModSize == size2)
                    {
                        bucketsModSize = buckets.Length;
                        return true;
                    }
                }
            }
            return false;
        }

        private int GetNewSlotsArraySizeIncrease(out int oldArraySize)
        {
            if (slots != null)
            {
                oldArraySize = slots.Length;
            }
            else
            {
                oldArraySize = InitialSlotsArraySize; // this isn't the old array size, but it is the initial size we should start at
            }

            int increaseInSize;

            if (oldArraySize == 1)
            {
                increaseInSize = InitialSlotsArraySize - 1;
            }
            else
            {
                increaseInSize = oldArraySize - 1;
            }

            int maxIncreaseInSize = MaxSlotsArraySize - oldArraySize;

            if (increaseInSize > maxIncreaseInSize)
            {
                increaseInSize = maxIncreaseInSize;
            }
            return increaseInSize;
        }

        // if the value returned gets used and that value is different than the current buckets.Length, then the calling code should increment currentIndexIntoSizeArray because this would now be the current
        private int GetNewBucketsArraySize()
        {
            int newArraySize;

            if (currentIndexIntoBucketsSizeArray >= 0)
            {
                if (currentIndexIntoBucketsSizeArray + 1 < bucketsSizeArray.Length)
                {
                    newArraySize = bucketsSizeArray[currentIndexIntoBucketsSizeArray + 1];
                }
                else
                {
                    newArraySize = buckets.Length;
                }
            }
            else
            {
                // -1 means stop trying to increase the size based on the array of primes
                // instead calc based on 2 * existing length and then get the next higher prime
                newArraySize = buckets.Length;
                if (newArraySize < int.MaxValue / 2)
                {
                    newArraySize = FastHashSetUtil.GetEqualOrClosestHigherPrime(newArraySize + newArraySize);
                }
                else
                {
                    newArraySize = LargestPrimeLessThanMaxInt;
                }
            }

            return newArraySize;
        }

        // if hashing, increase the size of the slots array
        // if not yet hashing, switch to hashing
        private void IncreaseCapacity(int capacityIncrease = -1)
        {
            // this function might be a fair bit over overhead for resizing at small sizes (like 33 and 65)
            // could try to reduce the overhead - there could just be a nextSlotsArraySize (don't need increase?), or nextSlotsArraySizeIncrease?
            // then we don't have to call GetNewSlotsArraySizeIncrease at all?
            // could test the overhead by just replacing all of the code with 
#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int newSlotsArraySizeIncrease;
                int oldSlotsArraySize;

                if (capacityIncrease == -1)
                {
                    newSlotsArraySizeIncrease = GetNewSlotsArraySizeIncrease(out oldSlotsArraySize);
                }
                else
                {
                    newSlotsArraySizeIncrease = capacityIncrease;
                    oldSlotsArraySize = slots.Length;
                }

                if (newSlotsArraySizeIncrease <= 0)
                {
                    throw new InvalidOperationException("Exceeded maximum number of items allowed for this container.");
                }

                int newSlotsArraySize = oldSlotsArraySize + newSlotsArraySizeIncrease;

                TNode[] newSlotsArray = new TNode[newSlotsArraySize];
                Array.Copy(slots, 0, newSlotsArray, 0, slots.Length); // check the IL, I think Array.Resize and Array.Copy without the start param calls this, so avoid the overhead by calling directly
                slots = newSlotsArray;

#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                SwitchToHashing(capacityIncrease);
            }
#endif
        }

        private TNode[] IncreaseCapacityNoCopy(int capacityIncrease = -1)
        {
#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int newSlotsrraySizeIncrease;
                int oldSlotsArraySize;

                if (capacityIncrease == -1)
                {
                    newSlotsrraySizeIncrease = GetNewSlotsArraySizeIncrease(out oldSlotsArraySize);
                }
                else
                {
                    newSlotsrraySizeIncrease = capacityIncrease;
                    oldSlotsArraySize = slots.Length;
                }

                if (newSlotsrraySizeIncrease <= 0)
                {
                    throw new InvalidOperationException("Exceeded maximum number of items allowed for this container.");
                }

                int newSlotsArraySize = oldSlotsArraySize + newSlotsrraySizeIncrease;

                TNode[] newSlotsArray = new TNode[newSlotsArraySize];
                return newSlotsArray;
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                SwitchToHashing(capacityIncrease);
                return null;
            }
#endif
        }

        private void ResizeBucketsArrayForward(int newBucketsArraySize)
        {
            if (newBucketsArraySize == buckets.Length)
            {
                // this will still work if no increase in size - it just might be slower than if you could increase the buckets array size
            }
            else
            {
                if (!CheckForModSizeIncrease()) //??? clean this up, it isn't really good to do it this way - no need to call GetNewBucketsArraySize before calling this function
                {
                    buckets = new int[newBucketsArraySize];
                    bucketsModSize = newBucketsArraySize;

                    if (currentIndexIntoBucketsSizeArray >= 0)
                    {
                        currentIndexIntoBucketsSizeArray++; // when the newBucketsArraySize gets used in the above code, point to the next avaialble size - ??? not sure this is the best place to increment this
                    }
                }
                else
                {
                    Array.Clear(buckets, 0, bucketsModSize);
                }

                CalcUsedItemsLoadFactorThreshold();

                int bucketsArrayLength = buckets.Length;

                int pastNodeIndex = slots.Length;
                if (firstBlankAtEndIndex < pastNodeIndex)
                {
                    pastNodeIndex = firstBlankAtEndIndex;
                }

                //??? for a loop where the end is array.Length, the compiler can skip any array bounds checking - can it do it for this code - it should be able to because pastIndex is no more than buckets.Length
                if (firstBlankAtEndIndex == count + 1)
                {
                    // this means there aren't any blank nodes
                    for (int i = 1; i < pastNodeIndex; i++)
                    {
                        ref TNode t = ref slots[i];

                        int hashIndex = t.hashOrNextIndexForBlanks % bucketsArrayLength;
                        t.nextIndex = buckets[hashIndex];

                        buckets[hashIndex] = i;
                    }
                }
                else
                {
                    // this means there are some blank nodes
                    for (int i = 1; i < pastNodeIndex; i++)
                    {
                        ref TNode t = ref slots[i];
                        if (t.nextIndex != BlankNextIndexIndicator) // skip blank nodes
                        {
                            int hashIndex = t.hashOrNextIndexForBlanks % bucketsArrayLength;
                            t.nextIndex = buckets[hashIndex];

                            buckets[hashIndex] = i;
                        }
                    }
                }
            }
        }

        private void ResizeBucketsArrayForwardKeepMarks(int newBucketsArraySize)
        {
            if (newBucketsArraySize == buckets.Length)
            {
                // this will still work if no increase in size - it just might be slower than if you could increase the buckets array size
            }
            else
            {
                //??? what if there is a high percent of blank/unused items in the slots array before the firstBlankAtEndIndex (mabye because of lots of removes)?
                // It would probably be faster to loop through the buckets array and then do chaining to find the used nodes - one problem with this is that you would have to find blank nodes - but they would be chained
                // this probably isn't a very likely scenario

                if (!CheckForModSizeIncrease()) //??? clean this up, it isn't really good to do it this way - no need to call GetNewBucketsArraySize before calling this function
                {
                    buckets = new int[newBucketsArraySize];
                    bucketsModSize = newBucketsArraySize;

                    if (currentIndexIntoBucketsSizeArray >= 0)
                    {
                        currentIndexIntoBucketsSizeArray++; // when the newBucketsArraySize gets used in the above code, point to the next avaialble size - ??? not sure this is the best place to increment this
                    }
                }

                CalcUsedItemsLoadFactorThreshold();

                int bucketsArrayLength = buckets.Length;

                int pastNodeIndex = slots.Length;
                if (firstBlankAtEndIndex < pastNodeIndex)
                {
                    pastNodeIndex = firstBlankAtEndIndex;
                }

                //??? for a loop where the end is array.Length, the compiler can skip any array bounds checking - can it do it for this code - it should be able to because pastIndex is no more than buckets.Length
                if (firstBlankAtEndIndex == count + 1)
                {
                    // this means there aren't any blank nodes
                    for (int i = 1; i < pastNodeIndex; i++)
                    {
                        ref TNode t = ref slots[i];

                        int hashIndex = t.hashOrNextIndexForBlanks % bucketsArrayLength;
                        t.nextIndex = buckets[hashIndex] | (t.nextIndex & MarkNextIndexBitMask);

                        buckets[hashIndex] = i;
                    }
                }
                else
                {
                    // this means there are some blank nodes
                    for (int i = 1; i < pastNodeIndex; i++)
                    {
                        ref TNode t = ref slots[i];
                        if (t.nextIndex != BlankNextIndexIndicator) // skip blank nodes
                        {
                            int hashIndex = t.hashOrNextIndexForBlanks % bucketsArrayLength;
                            t.nextIndex = buckets[hashIndex] | (t.nextIndex & MarkNextIndexBitMask);

                            buckets[hashIndex] = i;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes all items from the FastHashSet, but does not do any trimming of the resulting unused memory.
        /// To trim the unused memory, call TrimExcess.
        /// </summary>
        public void Clear()
        {
#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
#endif
            {
                firstBlankAtEndIndex = 1;
                nextBlankIndex = 1;
                Array.Clear(buckets, 0, buckets.Length);
            }

            count = 0;
        }

        // documentation states:
        // You can use the TrimExcess method to minimize a HashSet<T> object's memory overhead once it is known that no new elements will be added
        // To completely clear a HashSet<T> object and release all memory referenced by it, call this method after calling the Clear method.
        /// <summary>
        /// Trims excess capacity to minimize the FastHashSet's memory overhead.
        /// </summary>
        public void TrimExcess()
        {
#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                if (slots.Length > firstBlankAtEndIndex && firstBlankAtEndIndex > 0)
                {
                    Array.Resize(ref slots, firstBlankAtEndIndex);
                    // when firstBlankAtEndIndex == slots.Length, that means there are no blank at end items
                }
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                if (noHashArray != null && noHashArray.Length > count && count > 0)
                {
                    Array.Resize(ref noHashArray, count);
                }
            }
#endif
        }

        // this is only present to implement ICollection<T> - it has no real value otherwise because the Add method with bool return value already does this
        /// <summary>
        /// Implements the ICollection&lt;T&gt; Add method.  If possible, use the FastHashSet Add method instead to avoid any slight overhead and return a bool that indicates if the item was added.
        /// </summary>
        /// <param name="item">The item to add.</param>
        void ICollection<T>.Add(T item)
        {
            Add(in item);
        }

        // we need 2 versions of Add, one with 'in' and one without 'in' because the one without 'in' is needed to implement the ISet Add method
        // always keep the code for these 2 Add methods exactly the same
        /// <summary>
        /// Add an item to the FastHashSet using a read-only reference (in) parameter.  Use this version of the Add method when item is a large value type to avoid copying large objects.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>True if the item was added, or false if the FastHashSet already contains the item.</returns>
        public bool Add(in T item)
        {
#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif

                int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                int hashIndex = hash % bucketsModSize;

                for (int index = buckets[hashIndex]; index != NullIndex;)
                {
                    ref TNode t = ref slots[index];

                    if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                    {
                        return false; // item was found, so return false to indicate it was not added
                    }

                    index = t.nextIndex;
                }

                if (nextBlankIndex >= slots.Length)
                {
                    // there aren't any more blank nodes to add items, so we need to increase capacity
                    IncreaseCapacity();
                }

                int firstIndex = buckets[hashIndex];
                buckets[hashIndex] = nextBlankIndex;

                ref TNode tBlank = ref slots[nextBlankIndex];
                if (nextBlankIndex >= firstBlankAtEndIndex)
                {
                    // the blank nodes starting at firstBlankAtEndIndex aren't chained
                    nextBlankIndex = ++firstBlankAtEndIndex;
                }
                else
                {
                    // the blank nodes before firstBlankAtEndIndex are chained (the hashOrNextIndexForBlanks points to the next blank node)
                    nextBlankIndex = tBlank.hashOrNextIndexForBlanks;
                }

                tBlank.hashOrNextIndexForBlanks = hash;
                tBlank.nextIndex = firstIndex;
                tBlank.item = item;

                count++;

                if (count >= resizeBucketsCountThreshold)
                {
                    ResizeBucketsArrayForward(GetNewBucketsArraySize());
                }

                return true;
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                int i;
                for (i = 0; i < count; i++)
                {
                    if (comparer.Equals(item, noHashArray[i]))
                    {
                        return false;
                    }
                }

                if (i == noHashArray.Length)
                {
                    SwitchToHashing();

                    int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                    int hashIndex = hash % bucketsModSize;

                    ref TNode tBlank = ref slots[nextBlankIndex];

                    tBlank.hashOrNextIndexForBlanks = hash;
                    tBlank.nextIndex = buckets[hashIndex];
                    tBlank.item = item;

                    buckets[hashIndex] = nextBlankIndex;

                    nextBlankIndex = ++firstBlankAtEndIndex;

                    count++;

                    return true;
                }
                else
                {
                    // add to noHashArray
                    noHashArray[i] = item;
                    count++;
                    return true;
                }
            }
#endif
        }

        /// <summary>
        /// Add an item to the FastHashSet.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>True if the item was added, or false if the FastHashSet already contains the item.</returns>
        public bool Add(T item)
        {
#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif

                int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                int hashIndex = hash % bucketsModSize;

                for (int index = buckets[hashIndex]; index != NullIndex;)
                {
                    ref TNode t = ref slots[index];

                    if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                    {
                        return false; // item was found, so return false to indicate it was not added
                    }

                    index = t.nextIndex;
                }

                if (nextBlankIndex >= slots.Length)
                {
                    // there aren't any more blank nodes to add items, so we need to increase capacity
                    IncreaseCapacity();
                }

                int firstIndex = buckets[hashIndex];
                buckets[hashIndex] = nextBlankIndex;

                ref TNode tBlank = ref slots[nextBlankIndex];
                if (nextBlankIndex >= firstBlankAtEndIndex)
                {
                    // the blank nodes starting at firstBlankAtEndIndex aren't chained
                    nextBlankIndex = ++firstBlankAtEndIndex;
                }
                else
                {
                    // the blank nodes before firstBlankAtEndIndex are chained (the hashOrNextIndexForBlanks points to the next blank node)
                    nextBlankIndex = tBlank.hashOrNextIndexForBlanks;
                }

                tBlank.hashOrNextIndexForBlanks = hash;
                tBlank.nextIndex = firstIndex;
                tBlank.item = item;

                count++;

                if (count >= resizeBucketsCountThreshold)
                {
                    ResizeBucketsArrayForward(GetNewBucketsArraySize());
                }

                return true;
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                int i;
                for (i = 0; i < count; i++)
                {
                    if (comparer.Equals(item, noHashArray[i]))
                    {
                        return false;
                    }
                }

                if (i == noHashArray.Length)
                {
                    SwitchToHashing();

                    int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                    int hashIndex = hash % bucketsModSize;

                    ref TNode tBlank = ref slots[nextBlankIndex];

                    tBlank.hashOrNextIndexForBlanks = hash;
                    tBlank.nextIndex = buckets[hashIndex];
                    tBlank.item = item;

                    buckets[hashIndex] = nextBlankIndex;

                    nextBlankIndex = ++firstBlankAtEndIndex;

                    count++;

                    return true;
                }
                else
                {
                    // add to noHashArray
                    noHashArray[i] = item;
                    count++;
                    return true;
                }
            }
#endif
        }

        // return the index in the slots array of the item that was added or found
        private int AddToHashSetIfNotFound(in T item, int hash, out bool isFound)
        {
            // this assmes we are hashing

            int hashIndex = hash % bucketsModSize;

            for (int index = buckets[hashIndex]; index != NullIndex;)
            {
                ref TNode t = ref slots[index];

                if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                {
                    isFound = true;
                    return index; // item was found, so return the index of the found item
                }

                index = t.nextIndex;
            }

            if (nextBlankIndex >= slots.Length)
            {
                // there aren't any more blank nodes to add items, so we need to increase capacity
                IncreaseCapacity();
                ResizeBucketsArrayForward(GetNewBucketsArraySize());

                // fix things messed up by buckets array resize
                hashIndex = hash % bucketsModSize;
            }

            int firstIndex = buckets[hashIndex];
            buckets[hashIndex] = nextBlankIndex;

            int addedNodeIndex = nextBlankIndex;
            ref TNode tBlank = ref slots[nextBlankIndex];
            if (nextBlankIndex >= firstBlankAtEndIndex)
            {
                // the blank nodes starting at firstBlankAtEndIndex aren't chained
                nextBlankIndex = ++firstBlankAtEndIndex;
            }
            else
            {
                // the blank nodes before firstBlankAtEndIndex are chained (the hashOrNextIndexForBlanks points to the next blank node)
                nextBlankIndex = tBlank.hashOrNextIndexForBlanks;
            }

            tBlank.hashOrNextIndexForBlanks = hash;
            tBlank.nextIndex = firstIndex;
            tBlank.item = item;

            count++;

            isFound = false;
            return addedNodeIndex; // item was not found, so return the index of the added item
        }

        // return the node index that was added, or NullIndex if item was found
        private int AddToHashSetIfNotFoundAndMark(in T item, int hash)
        {
            // this assumes we are hashing

            int hashIndex = hash % bucketsModSize;

            for (int index = buckets[hashIndex]; index != NullIndex;)
            {
                ref TNode t = ref slots[index];

                if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                {
                    return NullIndex; // item was found, so return NullIndex to indicate it was not added
                }

                index = t.nextIndex & MarkNextIndexBitMaskInverted; ;
            }

            if (nextBlankIndex >= slots.Length)
            {
                // there aren't any more blank nodes to add items, so we need to increase capacity
                IncreaseCapacity();
                ResizeBucketsArrayForwardKeepMarks(GetNewBucketsArraySize());

                // fix things messed up by buckets array resize
                hashIndex = hash % bucketsModSize;
            }

            int firstIndex = buckets[hashIndex];
            buckets[hashIndex] = nextBlankIndex;

            int addedNodeIndex = nextBlankIndex;
            ref TNode tBlank = ref slots[nextBlankIndex];
            if (nextBlankIndex >= firstBlankAtEndIndex)
            {
                // the blank nodes starting at firstBlankAtEndIndex aren't chained
                nextBlankIndex = ++firstBlankAtEndIndex;
            }
            else
            {
                // the blank nodes before firstBlankAtEndIndex are chained (the hashOrNextIndexForBlanks points to the next blank node)
                nextBlankIndex = tBlank.hashOrNextIndexForBlanks;
            }

            tBlank.hashOrNextIndexForBlanks = hash;
            tBlank.nextIndex = firstIndex | MarkNextIndexBitMask;
            tBlank.item = item;

            count++;

            return addedNodeIndex; // item was not found, so return the index of the added item
        }

        // we need 2 versions of Contains, one with 'in' and one without 'in' because the one without 'in' is needed to implement the ICollection Contains method
        // always keep the code for these 2 Contains methods exactly the same
        /// <summary>
        /// Return true if the item is contained in the FastHashSet, otherwise return false.  Use this version of the Contains method when item is a large value type to avoid copying large objects.
        /// </summary>
        /// <param name="item">The item to search for in the FastHashSet.</param>
        /// <returns>True if found, false if not found.</returns>
        public bool Contains(in T item)
        {
#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                int hashIndex = hash % bucketsModSize;

                for (int index = buckets[hashIndex]; index != NullIndex;)
                {
                    ref TNode t = ref slots[index];

                    if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                    {
                        return true; // item was found, so return true
                    }

                    index = t.nextIndex;
                }
                return false;
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (comparer.Equals(item, noHashArray[i]))
                    {
                        return true; // item was found, so return true
                    }
                }
                return false;
            }
#endif
        }

        // this implements Contains for ICollection<T>
        /// <summary>
        /// Return true if the item is contained in the FastHashSet, otherwise return false.
        /// </summary>
        /// <param name="item">The item to search for in the FastHashSet.</param>
        /// <returns>True if found, false if not found.</returns>
        public bool Contains(T item)
        {
#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                int hashIndex = hash % bucketsModSize;

                for (int index = buckets[hashIndex]; index != NullIndex;)
                {
                    ref TNode t = ref slots[index];

                    if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                    {
                        return true; // item was found, so return true
                    }

                    index = t.nextIndex;
                }
                return false;
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (comparer.Equals(item, noHashArray[i]))
                    {
                        return true; // item was found, so return true
                    }
                }
                return false;
            }
#endif
        }

        /// <summary>
        /// Removes the item from the FastHashSet if found and returns true if the item was found and removed.
        /// </summary>
        /// <param name="item">The item value to remove.</param>
        /// <returns>True if the item was removed, or false if the item was not contained in the FastHashSet.</returns>
        public bool Remove(T item)
        {
#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                int hashIndex = hash % bucketsModSize;

                int priorIndex = NullIndex;

                for (int index = buckets[hashIndex]; index != NullIndex;)
                {
                    ref TNode t = ref slots[index];

                    if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                    {
                        // item was found, so remove it

                        if (priorIndex == NullIndex)
                        {
                            buckets[hashIndex] = t.nextIndex;
                        }
                        else
                        {
                            slots[priorIndex].nextIndex = t.nextIndex;
                        }

                        // add node to blank chain or to the blanks at the end (if possible)
                        if (index == firstBlankAtEndIndex - 1)
                        {
                            if (nextBlankIndex == firstBlankAtEndIndex)
                            {
                                nextBlankIndex--;
                            }
                            firstBlankAtEndIndex--;
                        }
                        else
                        {
                            t.hashOrNextIndexForBlanks = nextBlankIndex;
                            nextBlankIndex = index;
                        }

                        t.nextIndex = BlankNextIndexIndicator;

                        count--;

                        return true;
                    }

                    priorIndex = index;

                    index = t.nextIndex;
                }
                return false; // item not found
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (comparer.Equals(item, noHashArray[i]))
                    {
                        // remove the item by moving all remaining items to fill over this one - this is probably faster than Array.CopyTo
                        for (int j = i + 1; j < count; j++, i++)
                        {
                            noHashArray[i] = noHashArray[j];
                        }
                        count--;
                        return true;
                    }
                }
                return false;
            }
#endif
        }

        // this is a new public method not in HashSet
        /// <summary>
        /// <para>Removes the item from the FastHashSet if found and also if the predicate param evaluates to true on the found item.</para>
        /// <para>This is useful if there is something about the found item other than its equality value that can be used to determine if it should be removed.</para>
        /// </summary>
        /// <param name="item">The item value to remove.</param>
        /// <param name="removeIfPredIsTrue">The predicate to evaluate on the found item.</param>
        /// <returns>True if the item was removed, or false if the item was not removed.</returns>
        public bool RemoveIf(in T item, Predicate<T> removeIfPredIsTrue)
        {
            if (removeIfPredIsTrue == null)
            {
                throw new ArgumentNullException(nameof(removeIfPredIsTrue), "Value cannot be null.");
            }

            // the following code is almost the same as the Remove(item) function except that it additionally invokes the removeIfPredIsTrue param to see if the item should be removed

#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int hash = (comparer.GetHashCode(item) & HighBitNotSet);
                int hashIndex = hash % bucketsModSize;

                int priorIndex = NullIndex;

                for (int index = buckets[hashIndex]; index != NullIndex;)
                {
                    ref TNode t = ref slots[index];

                    if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                    {
                        if (removeIfPredIsTrue.Invoke(t.item))
                        {
                            // item was found and predicate was true, so remove it

                            if (priorIndex == NullIndex)
                            {
                                buckets[hashIndex] = t.nextIndex;
                            }
                            else
                            {
                                slots[priorIndex].nextIndex = t.nextIndex;
                            }

                            // add node to blank chain or to the blanks at the end (if possible)
                            if (index == firstBlankAtEndIndex - 1)
                            {
                                if (nextBlankIndex == firstBlankAtEndIndex)
                                {
                                    nextBlankIndex--;
                                }
                                firstBlankAtEndIndex--;
                            }
                            else
                            {
                                t.hashOrNextIndexForBlanks = nextBlankIndex;
                                nextBlankIndex = index;
                            }

                            t.nextIndex = BlankNextIndexIndicator;

                            count--;

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    priorIndex = index;

                    index = t.nextIndex;
                }
                return false; // item not found
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    if (comparer.Equals(item, noHashArray[i]))
                    {
                        if (removeIfPredIsTrue.Invoke(noHashArray[i]))
                        {
                            // remove the item by moving all remaining items to fill over this one - this is probably faster than Array.CopyTo
                            for (int j = i + 1; j < count; j++, i++)
                            {
                                noHashArray[i] = noHashArray[j];
                            }
                            count--;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            }
#endif
        }

        // this is a new public method not in HashSet
        /// <summary>
        /// <para>Returns a ref to the element in the FastHashSet if found, or adds the item if not present in the FastHashSet and returns a ref to the added element.</para>
        /// <para>The returned element reference should only be changed in ways that does not effect its GetHashCode value.</para>
        /// <para>The returned element reference should only be used before any modifications to the FastHashSet (like Add or Remove) which may invalidate it.</para>
        /// </summary>
        /// <param name="item">The item to be added or found.</param>
        /// <param name="isFound">Set to true if the item is found, or false if the added was not found and added.</param>
        /// <returns>Returns a ref to the found item or to the added item.</returns>
        public ref T FindOrAdd(in T item, out bool isFound)
        {
#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

            isFound = false;
#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int addedOrFoundItemIndex = AddToHashSetIfNotFound(in item, (comparer.GetHashCode(item) & HighBitNotSet), out isFound);
                return ref slots[addedOrFoundItemIndex].item;
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                int i;
                for (i = 0; i < count; i++)
                {
                    if (comparer.Equals(item, noHashArray[i]))
                    {
                        isFound = true;
                        return ref noHashArray[i];
                    }
                }

                if (i == noHashArray.Length)
                {
                    SwitchToHashing();
                    return ref FindOrAdd(in item, out isFound);
                }
                else
                {
                    // add to noHashArray and keep isAdded true
                    noHashArray[i] = item;
                    count++;
                    return ref noHashArray[i];
                }
            }
#endif
        }

        // this is a new public method not in HashSet
        /// <summary>
        /// <para>Tries to find the element with the same value as item in the FastHashSet and, if found, it returns a ref to this found element.</para>
        /// <para>This is similar to TryGetValue except it returns a ref to the actual element rather than creating copy of the element with an out parameter.</para>
        /// <para>This allows the actual element to be changed if it is a mutable value type.</para>
        /// <para>The returned element reference should only be changed in ways that does not effect its GetHashCode value.</para>
        /// <para>The returned element reference should only be used before any modifications to the FastHashSet (like Add or Remove) which may invalidate it.</para>
        /// </summary>
        /// <param name="item">The item to be found.</param>
        /// <param name="isFound">Set to true if the item is found, or false if not found.</param>
        /// <returns>Returns a ref to the element if it is found and sets the isFound out parameter to true.  If not found, it returns a ref to the first element available and sets the isFound out parameter to false.</returns>
        public ref T Find(in T item, out bool isFound)
        {
            isFound = false;
#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                FindInSlotsArray(item, out int foundNodeIndex, out int priorNodeIndex, out int bucketsIndex);
                if (foundNodeIndex != NullIndex)
                {
                    isFound = true;
                }

                return ref slots[foundNodeIndex].item;
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                int i;
                for (i = 0; i < count; i++)
                {
                    if (comparer.Equals(item, noHashArray[i]))
                    {
                        isFound = true;
                        return ref noHashArray[i];
                    }
                }

                // if item was not found, still need to return a ref to something, so return a ref to the first item in the array
                return ref noHashArray[0];
            }
#endif
        }

        // this is a new public method not in HashSet
        /// <summary>
        /// <para>Tries to find the element with the same value as item in the FastHashSet and, if found,it returns a ref to this found element, except if it is also removed (which is determined by the removeIfPredIsTrue parameter).</para>
        /// <para>The returned element reference should only be changed in ways that does not effect its GetHashCode value.</para>
        /// <para>The returned element reference should only be used before any modifications to the FastHashSet (like Add or Remove) which may invalidate it.</para>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="removeIfPredIsTrue">The predicate to evaluate on the found item.</param>
        /// <param name="isFound">Set to true if the item is found, or false if not found.</param>
        /// <param name="isRemoved">Set to true if the item is found and then removed, or false if not removed.</param>
        /// <returns>Returns a ref to the element if it is found (and not removed) and sets the isFound out parameter to true and the isRemoved out parameter to false.  If removed, it returns a reference to the first available element.</returns>
        public ref T FindAndRemoveIf(in T item, Predicate<T> removeIfPredIsTrue, out bool isFound, out bool isRemoved)
        {
            if (removeIfPredIsTrue == null)
            {
                throw new ArgumentNullException(nameof(removeIfPredIsTrue), "Value cannot be null.");
            }

#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

            isFound = false;
            isRemoved = false;

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                FindInSlotsArray(item, out int foundNodeIndex, out int priorNodeIndex, out int bucketsIndex);
                if (foundNodeIndex != NullIndex)
                {
                    isFound = true;
                    ref TNode t = ref slots[foundNodeIndex];
                    if (removeIfPredIsTrue.Invoke(t.item))
                    {
                        if (priorNodeIndex == NullIndex)
                        {
                            buckets[bucketsIndex] = t.nextIndex;
                        }
                        else
                        {
                            slots[priorNodeIndex].nextIndex = t.nextIndex;
                        }

                        // add node to blank chain or to the blanks at the end (if possible)
                        if (foundNodeIndex == firstBlankAtEndIndex - 1)
                        {
                            if (nextBlankIndex == firstBlankAtEndIndex)
                            {
                                nextBlankIndex--;
                            }
                            firstBlankAtEndIndex--;
                        }
                        else
                        {
                            t.hashOrNextIndexForBlanks = nextBlankIndex;
                            nextBlankIndex = foundNodeIndex;
                        }

                        t.nextIndex = BlankNextIndexIndicator;

                        count--;

                        isRemoved = true;

                        foundNodeIndex = NullIndex;
                    }
                }

                return ref slots[foundNodeIndex].item;
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                int i;
                for (i = 0; i < count; i++)
                {
                    if (comparer.Equals(item, noHashArray[i]))
                    {
                        isFound = true;
                        if (removeIfPredIsTrue.Invoke(noHashArray[i]))
                        {
                            // remove the item by moving all remaining items to fill over this one - this is probably faster than Array.CopyTo
                            for (int j = i + 1; j < count; j++, i++)
                            {
                                noHashArray[i] = noHashArray[j];
                            }
                            count--;

                            isRemoved = true;
                            return ref noHashArray[0];
                        }
                        else
                        {
                            return ref noHashArray[i];
                        }
                    }
                }

                // if item was not found, still need to return a ref to something, so return a ref to the first item in the array
                return ref noHashArray[0];
            }
#endif
        }

        // return index into slots array or 0 if not found
        //??? to make things faster, could have a FindInSlotsArray that just returns foundNodeIndex and another version called FindWithPriorInSlotsArray that has the 3 out params
        // first test to make sure this works as is
        private void FindInSlotsArray(in T item, out int foundNodeIndex, out int priorNodeIndex, out int bucketsIndex)
        {
            foundNodeIndex = NullIndex;
            priorNodeIndex = NullIndex;

            int hash = (comparer.GetHashCode(item) & HighBitNotSet);
            int hashIndex = hash % bucketsModSize;

            bucketsIndex = hashIndex;

            int priorIndex = NullIndex;

            for (int index = buckets[hashIndex]; index != NullIndex;)
            {
                ref TNode t = ref slots[index];

                if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                {
                    foundNodeIndex = index;
                    priorNodeIndex = priorIndex;
                    return; // item was found
                }

                priorIndex = index;

                index = t.nextIndex;
            }
            return; // item not found
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool FindInSlotsArray(in T item, int hash)
        {
            int hashIndex = hash % bucketsModSize;

            for (int index = buckets[hashIndex]; index != NullIndex;)
            {
                ref TNode t = ref slots[index];

                if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                {
                    return true; // item was found, so return true
                }

                index = t.nextIndex; ;
            }
            return false;
        }

#if !Exclude_No_Hash_Array_Implementation
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool FindInNoHashArray(in T item)
        {
            for (int i = 0; i < count; i++)
            {
                if (comparer.Equals(item, noHashArray[i]))
                {
                    return true; // item was found, so return true
                }
            }
            return false;
        }
#endif

        private void UnmarkAllNextIndexValues(int maxNodeIndex)
        {
            // must be hashing to be here
            for (int i = 1; i <= maxNodeIndex; i++)
            {
                slots[i].nextIndex &= MarkNextIndexBitMaskInverted;
            }
        }

        // removeMarked = true, means remove the marked items and keep the unmarked items
        // removeMarked = false, means remove the unmarked items and keep the marked items
        private void UnmarkAllNextIndexValuesAndRemoveAnyMarkedOrUnmarked(bool removeMarked)
        {
            // must be hashing to be here

            // must traverse all of the chains instead of just looping through the slots array because going through the chains is the only way to set
            // nodes within a chain to blank and still be able to remove the blank node from the chain

            int index;
            int nextIndex;
            int priorIndex;
            int lastNonBlankIndex = firstBlankAtEndIndex - 1;
            for (int i = 0; i < buckets.Length; i++)
            {
                priorIndex = NullIndex; // 0 means use buckets array
                index = buckets[i];

                while (index != NullIndex)
                {
                    ref TNode t = ref slots[index];
                    nextIndex = t.nextIndex;
                    bool isMarked = (nextIndex & MarkNextIndexBitMask) != 0;
                    if (isMarked)
                    {
                        // this node is marked, so unmark it
                        nextIndex &= MarkNextIndexBitMaskInverted;
                        t.nextIndex = nextIndex;
                    }

                    if (removeMarked == isMarked)
                    {
                        // set this node to blank

                        count--;

                        // first try to set it to blank by adding it to the blank at end group
                        if (index == lastNonBlankIndex)
                        {
                            //??? does it make sense to attempt this because any already blank items before this will not get added
                            lastNonBlankIndex--;
                            if (nextBlankIndex == firstBlankAtEndIndex)
                            {
                                nextBlankIndex--;
                            }
                            firstBlankAtEndIndex--;
                        }
                        else
                        {
                            // add to the blank group

                            t.nextIndex = BlankNextIndexIndicator;

                            t.hashOrNextIndexForBlanks = nextBlankIndex;
                            nextBlankIndex = index;
                        }

                        if (priorIndex == NullIndex)
                        {
                            buckets[i] = nextIndex;
                        }
                        else
                        {
                            slots[priorIndex].nextIndex = nextIndex;
                        }

                        // keep priorIndex the same because we removed the node in the chain, so the priorIndex is still the same value
                    }
                    else
                    {
                        priorIndex = index; // node was not removed from the chain, so the priorIndex now points to the node that was not removed
                    }

                    index = nextIndex;
                }
            }
        }

        private FoundType FindInSlotsArrayAndMark(in T item, out int foundNodeIndex)
        {
            int hash = (comparer.GetHashCode(item) & HighBitNotSet); ;
            int hashIndex = hash % bucketsModSize;

            int index = buckets[hashIndex];

            if (index == NullIndex)
            {
                foundNodeIndex = NullIndex;
                return FoundType.NotFound;
            }
            else
            {
                // item with same hashIndex already exists, so need to look in the chained list for an equal item (using Equals)

                int nextIndex;
                while (true)
                {
                    ref TNode t = ref slots[index];
                    nextIndex = t.nextIndex;

                    // check if hash codes are equal before calling Equals (which may take longer) items that are Equals must have the same hash code
                    if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                    {
                        foundNodeIndex = index;
                        if ((nextIndex & MarkNextIndexBitMask) == 0)
                        {
                            // not marked, so mark it
                            t.nextIndex |= MarkNextIndexBitMask;

                            return FoundType.FoundFirstTime;
                        }
                        return FoundType.FoundNotFirstTime;
                    }

                    nextIndex &= MarkNextIndexBitMaskInverted;
                    if (nextIndex == NullIndex)
                    {
                        foundNodeIndex = NullIndex;
                        return FoundType.NotFound; // not found
                    }
                    else
                    {
                        index = nextIndex;
                    }
                }
            }
        }

        // this is a new public method not in HashSet
        /// <summary>
        /// <para>Get the information about the size of chains in the FastHashSet.</para>
        /// <para>The size of chains should be small to reduce traversing and comparing items.</para>
        /// <para>This can indicate the effectiveness of the hash code creation method.</para>
        /// </summary>
        /// <param name="avgNodeVisitsPerChain">Outputs the average node visits per chain.  This is a single number that summarizes the average length of chains in terms of the average number of compares until an equal value is found (when the item is present).</param>
        /// <returns>A List of LevelAndCount items that gives the length of each chain in the FastHashSet.</returns>
        public List<ChainLevelAndCount> GetChainLevelsCounts(out double avgNodeVisitsPerChain)
        {
            Dictionary<int, int> itemsInChainToCountDict = new Dictionary<int, int>();

            // this function only makes sense when hashing
            int chainCount = 0;
            if (buckets != null)
            {
                for (int i = 0; i < buckets.Length; i++)
                {
                    int index = buckets[i];
                    if (index != NullIndex)
                    {
                        chainCount++;
                        int itemsInChain = 1;

                        while (slots[index].nextIndex != NullIndex)
                        {
                            index = slots[index].nextIndex;
                            itemsInChain++;
                        }

                        itemsInChainToCountDict.TryGetValue(itemsInChain, out int cnt);
                        cnt++;
                        itemsInChainToCountDict[itemsInChain] = cnt;
                    }
                }
            }

            double totalAvgNodeVisitsIfVisitingAllChains = 0;
            List<ChainLevelAndCount> lst = new List<ChainLevelAndCount>(itemsInChainToCountDict.Count);
            foreach (KeyValuePair<int, int> keyVal in itemsInChainToCountDict)
            {
                lst.Add(new ChainLevelAndCount(keyVal.Key, keyVal.Value));
                if (keyVal.Key == 1)
                {
                    totalAvgNodeVisitsIfVisitingAllChains += keyVal.Value;
                }
                else
                {
                    totalAvgNodeVisitsIfVisitingAllChains += keyVal.Value * (keyVal.Key + 1.0) / 2.0;
                }
            }

            if (chainCount == 0)
            {
                avgNodeVisitsPerChain = 0;
            }
            else
            {
                avgNodeVisitsPerChain = totalAvgNodeVisitsIfVisitingAllChains / chainCount;
            }

            lst.Sort();

            return lst;
        }

        // this is a new public method not in HashSet
        /// <summary>
        /// <para>Reorders items in the same hash chain (items that have the same hash code or mod to the same index), so that they are adjacent in memory.</para>
        /// <para>This gives better locality of reference for larger count of items, which can result in fewer cache misses.</para>
        /// </summary>
        public void ReorderChainedNodesToBeAdjacent()
        {
            if (slots != null)
            {
                TNode[] newSlotsArray = new TNode[slots.Length];

                // copy elements using the buckets array chains so there is better locality in the chains
                int index;
                int newIndex = 1;
                for (int i = 0; i < buckets.Length; i++)
                {
                    index = buckets[i];
                    if (index != NullIndex)
                    {
                        buckets[i] = newIndex;
                        while (true)
                        {
                            ref TNode t = ref slots[index];
                            ref TNode tNew = ref newSlotsArray[newIndex];
                            index = t.nextIndex;
                            newIndex++;

                            // copy
                            tNew.hashOrNextIndexForBlanks = t.hashOrNextIndexForBlanks;
                            tNew.item = t.item;
                            if (index == NullIndex)
                            {
                                tNew.nextIndex = NullIndex;
                                break;
                            }
                            tNew.nextIndex = newIndex;
                        }
                    }
                }

                newIndex++;
                nextBlankIndex = newIndex;
                firstBlankAtEndIndex = newIndex;
                slots = newSlotsArray;
            }
        }

        /// <summary>
        /// Looks for equalValue and if found, returns a copy of the found value in actualValue and returns true.
        /// </summary>
        /// <param name="equalValue">The item to look for.</param>
        /// <param name="actualValue">The copy of the found value, if found, or the default value of the same type if not found.</param>
        /// <returns>True if equalValue is found, or false if not found.</returns>
        public bool TryGetValue(T equalValue, out T actualValue)
        {
#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                FindInSlotsArray(equalValue, out int foundNodeIndex, out int priorNodeIndex, out int bucketsIndex);
                if (foundNodeIndex > 0)
                {
                    actualValue = slots[foundNodeIndex].item;
                    return true;
                }

                actualValue = default;
                return false;
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                int i;
                for (i = 0; i < count; i++)
                {
                    if (comparer.Equals(equalValue, noHashArray[i]))
                    {
                        actualValue = noHashArray[i];
                        return true;
                    }
                }

                actualValue = default;
                return false;
            }
#endif
        }

        /// <summary>
        /// Adds all items in <paramref name="other"/> into this FastHashSet.  This is similar to AddRange for other types of collections, but it is called UnionWith for ISets.
        /// </summary>
        /// <param name="other">The enumerable items to add (cannot be null).</param>
        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Value cannot be null.");
            }

            // Note: HashSet doesn't seem to increment this unless it really changes something - like doing an Add(3) when 3 is already in the hashset doesn't increment, same as doing a UnionWith with an empty set as the param.
#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

            if (other == this)
            {
                return;
            }

            //??? maybe there is a faster way to add a bunch at one time - I copied the Add code below to make this faster
            //foreach (T item in range)
            //{
            //	Add(item);
            //}

            // do this with more code because it might get called in some high performance situations

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                foreach (T item in other)
                {
                    AddToHashSetIfNotFound(in item, (comparer.GetHashCode(item) & HighBitNotSet), out bool isFound);
                }
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                int i;

                foreach (T item in other)
                {
                    //??? if it's easier for the jit compiler or il compiler to remove the array bounds checking then
                    // have i < noHashArray.Length and do the check for count within the loop with a break statement
                    for (i = 0; i < count; i++)
                    {
                        if (comparer.Equals(item, noHashArray[i]))
                        {
                            goto found; // break out of inner for loop
                        }
                    }

                    // if here then item was not found
                    if (i == noHashArray.Length)
                    {
                        SwitchToHashing();
                        AddToHashSetIfNotFound(in item, (comparer.GetHashCode(item) & HighBitNotSet), out bool isFound);
                    }
                    else
                    {
                        // add to noHashArray
                        noHashArray[i] = item;
                        count++;
                    }

                found:;
                }
            }
#endif
        }

        /// <summary>
        /// Removes all items in <paramref name="other"/> from the FastHashSet.
        /// </summary>
        /// <param name="other">The enumerable items (cannot be null).</param>
        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Value cannot be null.");
            }

#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif
            if (other == this)
            {
                Clear();
            }
            else
            {
                foreach (T item in other)
                {
                    Remove(item);
                }
            }
        }

        /// <summary>
        /// Removes items from the FastHashSet so that the only remaining items are those contained in <paramref name="other"/> that also match an item in the FastHashSet.
        /// </summary>
        /// <param name="other">The enumerable items (cannot be null).</param>
        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Value cannot be null.");
            }

            if (other == this)
            {
                return;
            }

#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

            // if hashing, find each item in the slots array and mark anything found, but remove from being found again

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int foundItemCount = 0; // the count of found items in the hash - without double counting
                foreach (T item in other)
                {
                    FoundType foundType = FindInSlotsArrayAndMark(in item, out int foundIndex);
                    if (foundType == FoundType.FoundFirstTime)
                    {
                        foundItemCount++;

                        if (foundItemCount == count)
                        {
                            break;
                        }
                    }
                }

                if (foundItemCount == 0)
                {
                    Clear();
                }
                else
                {
                    UnmarkAllNextIndexValuesAndRemoveAnyMarkedOrUnmarked(false);
                }
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                // Note: we could actually do this faster by moving any found items to the front and keeping track of the found items
                // with a single int index
                // the problem with this method is it reorders items and even though that shouldn't matter in a set
                // it might cause issues with code that incorrectly assumes order stays the same for operations like this

                // possibly a faster implementation would be to use the method above, but keep track of original order with an int array of the size of count (ex. item at 0 was originally 5, and also item at 5 was originally 0)

                // set the corresponding bit in this int if an item was found
                // using a uint means the no hashing array cannot be more than 32 items
                uint foundItemBits = 0;

                int i;

                int foundItemCount = 0; // the count of found items in the hash - without double counting
                foreach (T item in other)
                {
                    for (i = 0; i < count; i++)
                    {
                        if (comparer.Equals(item, noHashArray[i]))
                        {
                            uint mask = (1u << i);
                            if ((foundItemBits & mask) == 0)
                            {
                                foundItemBits |= mask;
                                foundItemCount++;
                            }
                            goto found; // break out of inner for loop
                        }
                    }

                found:
                    if (foundItemCount == count)
                    {
                        // all items in the set were found, so there is nothing to remove - the set isn't changed
                        return;
                    }
                }

                if (foundItemCount == 0)
                {
                    count = 0; // this is the equivalent of calling Clear
                }
                else
                {
                    // remove any items that are unmarked (unfound)
                    // go backwards because this can be faster
                    for (i = count - 1; i >= 0; i--)
                    {
                        uint mask = (1u << i);
                        if ((foundItemBits & mask) == 0)
                        {
                            if (i < count - 1)
                            {
                                // a faster method if there are multiple unfound items in a row is to find the first used item (make i go backwards until the item is used and then increment i by 1)
                                // if there aren't multiple unused in a row, then this is a bit of a waste

                                int j = i + 1; // j now points to the next item after the unfound one that we want to keep

                                i--;
                                while (i >= 0)
                                {
                                    uint mask2 = (1u << i);
                                    if ((foundItemBits & mask2) != 0)
                                    {
                                        break;
                                    }
                                    i--;
                                }
                                i++;

                                int k = i;
                                for (; j < count; j++, k++)
                                {
                                    noHashArray[k] = noHashArray[j];
                                }
                            }

                            count--;
                        }
                    }
                }
            }
#endif
        }

        // An empty set is a proper subset of any other collection. Therefore, this method returns true if the collection represented by the current HashSet<T> object
        // is empty unless the other parameter is also an empty set.
        // This method always returns false if Count is greater than or equal to the number of elements in other.
        // If the collection represented by other is a HashSet<T> collection with the same equality comparer as the current HashSet<T> object,
        // then this method is an O(n) operation. Otherwise, this method is an O(n + m) operation, where n is Count and m is the number of elements in other.

        /// <summary>
        /// Returns true if this FastHashSet is a proper subset of <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The enumerable items (cannot be null).</param>
        /// <returns>True if a proper subset of <paramref name="other"/>.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Value cannot be null.");
            }

            if (other == this)
            {
                return false;
            }

            ICollection<T> collection = other as ICollection<T>;
            if (collection != null)
            {
                if (count == 0 && collection.Count > 0)
                {
                    return true; // by definition, an empty set is a proper subset of any non-empty collection
                }

                if (count >= collection.Count)
                {
                    return false;
                }
            }
            else
            {
                if (count == 0)
                {
                    foreach (T item in other)
                    {
                        return true;
                    }
                    return false;
                }
            }

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int foundItemCount = 0; // the count of found items in the hash - without double counting
                int maxFoundIndex = 0;
                bool notFoundAtLeastOne = false;
                foreach (T item in other)
                {
                    FoundType foundType = FindInSlotsArrayAndMark(in item, out int foundIndex);
                    if (foundType == FoundType.FoundFirstTime)
                    {
                        foundItemCount++;
                        if (maxFoundIndex < foundIndex)
                        {
                            maxFoundIndex = foundIndex;
                        }
                    }
                    else if (foundType == FoundType.NotFound)
                    {
                        notFoundAtLeastOne = true;
                    }

                    if (notFoundAtLeastOne && foundItemCount == count)
                    {
                        // true means all of the items in the set were found in other and at least one item in other was not found in the set
                        break; // will return true below after unmarking
                    }
                }

                UnmarkAllNextIndexValues(maxFoundIndex);

                return notFoundAtLeastOne && foundItemCount == count; // true if all of the items in the set were found in other and at least one item in other was not found in the set
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                uint foundItemBits = 0;

                int foundItemCount = 0; // the count of found items in the hash - without double counting
                bool notFoundAtLeastOne = false;
                foreach (T item in other)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (comparer.Equals(item, noHashArray[i]))
                        {
                            uint mask = (1u << i);
                            if ((foundItemBits & mask) == 0)
                            {
                                foundItemBits |= mask;
                                foundItemCount++;
                            }
                            goto found; // break out of inner for loop
                        }
                    }

                    // if here then item was not found
                    notFoundAtLeastOne = true;

                found:
                    if (notFoundAtLeastOne && foundItemCount == count)
                    {
                        // true means all of the items in the set were found in other and at least one item in other was not found in the set
                        return true;
                    }
                }

                return false;
            }
#endif
        }

        /// <summary>
        /// Returns true if this FastHashSet is a subset of <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The enumerable items (cannot be null).</param>
        /// <returns>True if a subset of <paramref name="other"/>.</returns>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Value cannot be null.");
            }

            if (other == this)
            {
                return true;
            }

            if (count == 0)
            {
                return true; // by definition, an empty set is a subset of any collection
            }

            ICollection<T> collection = other as ICollection<T>;
            if (collection != null)
            {
                if (count > collection.Count)
                {
                    return false;
                }
            }

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int foundItemCount = 0; // the count of found items in the hash - without double counting
                int maxFoundIndex = 0;
                foreach (T item in other)
                {
                    FoundType foundType = FindInSlotsArrayAndMark(in item, out int foundIndex);
                    if (foundType == FoundType.FoundFirstTime)
                    {
                        foundItemCount++;
                        if (maxFoundIndex < foundIndex)
                        {
                            maxFoundIndex = foundIndex;
                        }

                        if (foundItemCount == count)
                        {
                            break;
                        }
                    }
                }

                UnmarkAllNextIndexValues(maxFoundIndex);

                return foundItemCount == count; // true if all of the items in the set were found in other
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                uint foundItemBits = 0;

                int foundItemCount = 0; // the count of found items in the hash - without double counting
                foreach (T item in other)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (comparer.Equals(item, noHashArray[i]))
                        {
                            uint mask = (1u << i);
                            if ((foundItemBits & mask) == 0)
                            {
                                foundItemBits |= mask;
                                foundItemCount++;
                            }
                            goto found; // break out of inner for loop
                        }
                    }

                found:
                    if (foundItemCount == count)
                    {
                        break;
                    }
                }

                return foundItemCount == count; // true if all of the items in the set were found in other
            }
#endif
        }

        /// <summary>
        /// Returns true if this FastHashSet is a proper superset of <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The enumerable items (cannot be null).</param>
        /// <returns>True if a proper superset of <paramref name="other"/>.</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Value cannot be null.");
            }

            if (other == this)
            {
                return false;
            }

            if (count == 0)
            {
                return false; // an empty set can never be a proper superset of anything (not even an empty collection)
            }

            ICollection<T> collection = other as ICollection<T>;
            if (collection != null)
            {
                if (collection.Count == 0)
                {
                    return true; // by definition, an empty other means the set is a proper superset of it if the set has at least one value
                }
            }
            else
            {
                foreach (T item in other)
                {
                    goto someItemsInOther;
                }
                return true;
            }

        someItemsInOther:

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int foundItemCount = 0; // the count of found items in the hash - without double counting
                int maxFoundIndex = 0;
                foreach (T item in other)
                {
                    FoundType foundType = FindInSlotsArrayAndMark(in item, out int foundIndex);
                    if (foundType == FoundType.FoundFirstTime)
                    {
                        foundItemCount++;
                        if (maxFoundIndex < foundIndex)
                        {
                            maxFoundIndex = foundIndex;
                        }

                        if (foundItemCount == count)
                        {
                            break;
                        }
                    }
                    else if (foundType == FoundType.NotFound)
                    {
                        // any unfound item means this can't be a proper superset of
                        UnmarkAllNextIndexValues(maxFoundIndex);
                        return false;
                    }
                }

                UnmarkAllNextIndexValues(maxFoundIndex);

                return foundItemCount < count; // true if all of the items in other were found in set and at least one item in set was not found in other
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                uint foundItemBits = 0;

                int foundItemCount = 0; // the count of found items in the hash - without double counting
                foreach (T item in other)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (comparer.Equals(item, noHashArray[i]))
                        {
                            uint mask = (1u << i);
                            if ((foundItemBits & mask) == 0)
                            {
                                foundItemBits |= mask;
                                foundItemCount++;
                            }
                            goto found; // break out of inner for loop
                        }
                    }

                    // if here then item was not found
                    return false;

                found:
                    if (foundItemCount == count)
                    {
                        break;
                    }
                }

                return foundItemCount < count; // true if all of the items in other were found in set and at least one item in set was not found in other
            }
#endif
        }

        /// <summary>
        /// Returns true if this FastHashSet is a superset of <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The enumerable items (cannot be null).</param>
        /// <returns>True if a superset of <paramref name="other"/>.</returns>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Value cannot be null.");
            }

            if (other == this)
            {
                return true;
            }

            ICollection<T> collection = other as ICollection<T>;
            if (collection != null)
            {
                if (collection.Count == 0)
                {
                    return true; // by definition, an empty other means the set is a superset of it
                }
            }
            else
            {
                foreach (T item in other)
                {
                    goto someItemsInOther;
                }
                return true;
            }

        someItemsInOther:

            if (count == 0)
            {
                return false; // an empty set can never be a proper superset of anything (except an empty collection - but an empty collection returns true above)
            }

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                foreach (T item in other)
                {
                    if (!FindInSlotsArray(in item, (comparer.GetHashCode(item) & HighBitNotSet)))
                    {
                        return false;
                    }
                }

                return true; // true if all of the items in other were found in the set, false if at least one item in other was not found in the set
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                int i;

                foreach (T item in other)
                {
                    for (i = 0; i < count; i++)
                    {
                        if (comparer.Equals(item, noHashArray[i]))
                        {
                            goto found; // break out of inner for loop
                        }
                    }

                    // if here then item was not found
                    return false;

                found:;

                }

                return true; // true if all of the items in other were found in the set, false if at least one item in other was not found in the set
            }
#endif
        }

        /// <summary>
        /// Returns true if this FastHashSet contains any items in <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The enumerable items (cannot be null).</param>
        /// <returns>True if contains any items in <paramref name="other"/>.</returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Value cannot be null.");
            }

            if (other == this)
            {
                return count > 0; // return false if there are no items when both sets are the same, otherwise return true when both sets are the same
            }

            foreach (T item in other)
            {
                if (Contains(in item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if this FastHashSet contains exactly the same elements as <paramref name="other"/>.
        /// </summary>
        /// <param name="other">The enumerable items (cannot be null).</param>
        /// <returns>True if contains the same elements as <paramref name="other"/>.</returns>
        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Value cannot be null.");
            }

            if (other == this)
            {
                return true;
            }

            // if other is ICollection, then it has count

            ICollection c = other as ICollection;

            if (c != null)
            {
                if (c.Count < count)
                {
                    return false;
                }

                HashSet<T> hset = other as HashSet<T>;
                if (hset != null && Equals(hset.Comparer, Comparer))
                {
                    if (hset.Count != count)
                    {
                        return false;
                    }

                    foreach (T item in other)
                    {
                        if (!Contains(in item))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                FastHashSet<T> fhset = other as FastHashSet<T>;
                if (fhset != null && Equals(fhset.Comparer, Comparer))
                {
                    if (fhset.Count != count)
                    {
                        return false;
                    }

#if !Exclude_No_Hash_Array_Implementation
                    if (IsHashing)
                    {
#endif
                        int pastNodeIndex = slots.Length;
                        if (firstBlankAtEndIndex < pastNodeIndex)
                        {
                            pastNodeIndex = firstBlankAtEndIndex;
                        }

#if !Exclude_No_Hash_Array_Implementation
                        if (fhset.IsHashing)
                        {
#endif
                            for (int i = 1; i < pastNodeIndex; i++)
                            {
                                // could not do the blank check if we know there aren't any blanks - below code and in the loop in the else
                                // could do the check to see if there are any blanks first and then have 2 versions of this code, one with the check for blank and the other without it
                                if (slots[i].nextIndex != BlankNextIndexIndicator) // skip any blank nodes
                                {
                                    if (!fhset.FindInSlotsArray(in slots[i].item, slots[i].hashOrNextIndexForBlanks))
                                    {
                                        return false;
                                    }
                                }
                            }
#if !Exclude_No_Hash_Array_Implementation
                        }
                        else
                        {
                            for (int i = 1; i < pastNodeIndex; i++)
                            {
                                if (slots[i].nextIndex != BlankNextIndexIndicator) // skip any blank nodes
                                {
                                    if (!fhset.FindInNoHashArray(in slots[i].item))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (T item in other)
                        {
                            if (!FindInNoHashArray(in item))
                            {
                                return false;
                            }
                        }
                    }
                    return true;
#endif
                }

            }


#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                int foundItemCount = 0; // the count of found items in the hash - without double counting
                int maxFoundIndex = 0;
                foreach (T item in other)
                {
                    FoundType foundType = FindInSlotsArrayAndMark(in item, out int foundIndex);
                    if (foundType == FoundType.FoundFirstTime)
                    {
                        foundItemCount++;
                        if (maxFoundIndex < foundIndex)
                        {
                            maxFoundIndex = foundIndex;
                        }
                    }
                    else if (foundType == FoundType.NotFound)
                    {
                        UnmarkAllNextIndexValues(maxFoundIndex);
                        return false;
                    }
                }

                UnmarkAllNextIndexValues(maxFoundIndex);

                return foundItemCount == count;
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                uint foundItemBits = 0;

                int foundItemCount = 0; // the count of found items in the hash - without double counting
                foreach (T item in other)
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (comparer.Equals(item, noHashArray[i]))
                        {
                            uint mask = (1u << i);
                            if ((foundItemBits & mask) == 0)
                            {
                                foundItemBits |= mask;
                                foundItemCount++;
                            }
                            goto found; // break out of inner for loop
                        }
                    }
                    // if here then item was not found
                    return false;
                found:;
                }

                return foundItemCount == count;
            }
#endif
        }

        // From the online document: Modifies the current HashSet<T> object to contain only elements that are present either in that object or in the specified collection, but not both.
        /// <summary>
        /// Modifies the FastHashSet so that it contains only items in the FashHashSet or <paramref name="other"/>, but not both.
        /// So items in <paramref name="other"/> that are also in the FastHashSet are removed, and items in <paramref name="other"/> that are not in the FastHashSet are added.
        /// </summary>
        /// <param name="other">The enumerable items (cannot be null).</param>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other), "Value cannot be null.");
            }

            if (other == this)
            {
                Clear();
            }

#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

#if !Exclude_No_Hash_Array_Implementation
            if (!IsHashing)
            {
                // to make things easier for now, just switch to hashing if calling this function and deal with only one set of code
                SwitchToHashing();
            }
#endif

            // for the first loop through other, add any unfound items and mark
            int addedNodeIndex;
            int maxAddedNodeIndex = NullIndex;
            foreach (T item in other)
            {
                addedNodeIndex = AddToHashSetIfNotFoundAndMark(in item, (comparer.GetHashCode(item) & HighBitNotSet));
                if (addedNodeIndex > maxAddedNodeIndex)
                {
                    maxAddedNodeIndex = addedNodeIndex;
                }
            }

            foreach (T item in other)
            {
                RemoveIfNotMarked(in item);
            }

            UnmarkAllNextIndexValues(maxAddedNodeIndex);
        }

        private void RemoveIfNotMarked(in T item)
        {
            // calling this function assumes we are hashing
            int hash = (comparer.GetHashCode(item) & HighBitNotSet);
            int hashIndex = hash % bucketsModSize;

            int priorIndex = NullIndex;

            for (int index = buckets[hashIndex]; index != NullIndex;)
            {
                ref TNode t = ref slots[index];

                if (t.hashOrNextIndexForBlanks == hash && comparer.Equals(t.item, item))
                {
                    // item was found, so remove it if not marked
                    if ((t.nextIndex & MarkNextIndexBitMask) == 0)
                    {
                        if (priorIndex == NullIndex)
                        {
                            buckets[hashIndex] = t.nextIndex;
                        }
                        else
                        {
                            // if slots[priorIndex].nextIndex was marked, then keep it marked
                            // already know that t.nextIndex is not marked
                            slots[priorIndex].nextIndex = t.nextIndex | (slots[priorIndex].nextIndex & MarkNextIndexBitMask);
                        }

                        // add node to blank chain or to the blanks at the end (if possible)
                        if (index == firstBlankAtEndIndex - 1)
                        {
                            if (nextBlankIndex == firstBlankAtEndIndex)
                            {
                                nextBlankIndex--;
                            }
                            firstBlankAtEndIndex--;
                        }
                        else
                        {
                            t.hashOrNextIndexForBlanks = nextBlankIndex;
                            nextBlankIndex = index;
                        }

                        t.nextIndex = BlankNextIndexIndicator;

                        count--;

                        return;
                    }
                }

                priorIndex = index;

                index = t.nextIndex & MarkNextIndexBitMaskInverted;
            }
            return; // item not found
        }

        /// <summary>
        /// Removes any items in the FastHashSet where the <paramref name="match"/> predicate is true for that item.
        /// </summary>
        /// <param name="match">The match predicate (cannot be null).</param>
        /// <returns>The number of items removed.</returns>
        public int RemoveWhere(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match), "Value cannot be null.");
            }

#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            incrementForEverySetModification++;
#endif

            int removeCount = 0;

#if !Exclude_No_Hash_Array_Implementation
            if (IsHashing)
            {
#endif
                // must traverse all of the chains instead of just looping through the slots array because going through the chains is the only way to set
                // nodes within a chain to blank and still be able to remove the blank node from the chain

                int priorIndex;
                int nextIndex;
                for (int i = 0; i < buckets.Length; i++)
                {
                    priorIndex = NullIndex; // 0 means use buckets array

                    for (int index = buckets[i]; index != NullIndex;)
                    {
                        ref TNode t = ref slots[index];

                        nextIndex = t.nextIndex;
                        if (match.Invoke(t.item))
                        {
                            // item was matched, so remove it

                            if (priorIndex == NullIndex)
                            {
                                buckets[i] = nextIndex;
                            }
                            else
                            {
                                slots[priorIndex].nextIndex = nextIndex;
                            }

                            // add node to blank chain or to the blanks at the end (if possible)
                            if (index == firstBlankAtEndIndex - 1)
                            {
                                if (nextBlankIndex == firstBlankAtEndIndex)
                                {
                                    nextBlankIndex--;
                                }
                                firstBlankAtEndIndex--;
                            }
                            else
                            {
                                t.hashOrNextIndexForBlanks = nextBlankIndex;
                                nextBlankIndex = index;
                            }

                            t.nextIndex = BlankNextIndexIndicator;

                            count--;
                            removeCount++;
                        }

                        priorIndex = index;

                        index = nextIndex;
                    }
                }
#if !Exclude_No_Hash_Array_Implementation
            }
            else
            {
                int i;
                for (i = count - 1; i >= 0; i--)
                {
                    if (match.Invoke(noHashArray[i]))
                    {
                        removeCount++;

                        if (i < count - 1)
                        {
                            int j = i + 1;
                            int k = i;
                            for (; j < count; j++, k++)
                            {
                                noHashArray[k] = noHashArray[j];
                            }
                        }

                        count--;
                    }
                }
            }
#endif

            return removeCount;
        }

        private class FastHashSetEqualityComparer : IEqualityComparer<FastHashSet<T>>
        {
            public bool Equals(FastHashSet<T> x, FastHashSet<T> y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (y == null)
                {
                    return false;
                }

                if (x != null)
                {
                    return x.SetEquals(y);
                }
                else
                {
                    return false;
                }
            }

            public int GetHashCode(FastHashSet<T> set)
            {
                if (set == null)
                {
                    // oddly the documentation for the IEqualityComparer.GetHashCode function says it will throw an ArgumentNullException if the param is null
                    return 0; // 0 seems to be what .NET framework uses when passing in null, so return the same thing to be consistent
                }
                else
                {
                    unchecked
                    {
                        int hashCode = 0;
#if !Exclude_No_Hash_Array_Implementation
                        if (set.IsHashing)
                        {
#endif
                            int pastNodeIndex = set.slots.Length;
                            if (set.firstBlankAtEndIndex < pastNodeIndex)
                            {
                                pastNodeIndex = set.firstBlankAtEndIndex;
                            }

                            for (int i = 1; i < pastNodeIndex; i++)
                            {
                                if (set.slots[i].nextIndex != 0) // nextIndex == 0 indicates a blank/available node
                                {
                                    // maybe do ^= instead of add? - will this produce the same thing regardless of order? - if ^= maybe we don't need unchecked
                                    // sum up the individual item hash codes - this way it won't matter what order the items are in, the same resulting hash code will be produced
                                    hashCode += set.slots[i].hashOrNextIndexForBlanks;
                                }
                            }
#if !Exclude_No_Hash_Array_Implementation
                        }
                        else
                        {
                            for (int i = 0; i < set.count; i++)
                            {
                                // sum up the individual item hash codes - this way it won't matter what order the items are in, the same resulting hash code will be produced
                                hashCode += set.noHashArray[i].GetHashCode();
                            }
                        }
#endif
                        return hashCode;
                    }
                }
            }
        }

        /// <summary>
        /// Creates and returns the IEqualityComparer for a FastHashSet which can be used to compare two FastHashSets based on their items being equal.
        /// </summary>
        /// <returns>An IEqualityComparer for a FastHashSet.</returns>
        public static IEqualityComparer<FastHashSet<T>> CreateSetComparer()
        {
            return new FastHashSetEqualityComparer();
        }

        /// <summary>
        /// Allows enumerating through items in the FastHashSet.  Order is not guaranteed.
        /// </summary>
        /// <returns>The IEnumerator<T> for the FastHashSet.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new FastHashSetEnumerator<T>(this);
        }

        /// <summary>
        /// Allows enumerating through items in the FastHashSet.  Order is not guaranteed.
        /// </summary>
        /// <returns>The IEnumerator for the FastHashSet.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new FastHashSetEnumerator<T>(this);
        }

        private class FastHashSetEnumerator<T2> : IEnumerator<T2>
        {
            private FastHashSet<T2> set;
            private int currentIndex = -1;

#if !Exclude_Check_For_Is_Disposed_In_Enumerator
            private bool isDisposed;
#endif

#if !Exclude_Check_For_Set_Modifications_In_Enumerator
            private int incrementForEverySetModification;
#endif

            /// <summary>
            /// Constructor for the FastHashSetEnumerator that takes a FastHashSet as a parameter.
            /// </summary>
            /// <param name="set">The FastHashSet to enumerate through.</param>
            public FastHashSetEnumerator(FastHashSet<T2> set)
            {
                this.set = set;
#if !Exclude_No_Hash_Array_Implementation
                if (set.IsHashing)
                {
#endif
                    currentIndex = NullIndex; // 0 is the index before the first possible node (0 is the blank node)
#if !Exclude_No_Hash_Array_Implementation
                }
                else
                {
                    currentIndex = -1;
                }
#endif

#if !Exclude_Check_For_Set_Modifications_In_Enumerator
                incrementForEverySetModification = set.incrementForEverySetModification;
#endif
            }

            /// <summary>
            /// Moves to the next item for the FastHashSet enumerator.
            /// </summary>
            /// <returns>True if there was a next item, otherwise false.</returns>
            public bool MoveNext()
            {
#if !Exclude_Check_For_Is_Disposed_In_Enumerator
                if (isDisposed)
                {
                    // the only reason this code returns false when Disposed is called is to be compatable with HashSet<T>
                    // if this level of compatibility isn't needed, then #define Exclude_Check_For_Is_Disposed_In_Enumerator to remove this check and makes the code slightly faster
                    return false;
                }
#endif

#if !Exclude_Check_For_Set_Modifications_In_Enumerator
                if (incrementForEverySetModification != set.incrementForEverySetModification)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
#endif

#if !Exclude_No_Hash_Array_Implementation
                if (set.IsHashing)
                {
#endif
                    // it's easiest to just loop through the node array and skip any nodes that are blank
                    // rather than looping through the buckets array and following the nextIndex to the end of each bucket

                    while (true)
                    {
                        currentIndex++;
                        if (currentIndex < set.firstBlankAtEndIndex)
                        {
                            if (set.slots[currentIndex].nextIndex != BlankNextIndexIndicator)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            currentIndex = set.firstBlankAtEndIndex;
                            return false;
                        }
                    }
#if !Exclude_No_Hash_Array_Implementation
                }
                else
                {
                    currentIndex++;
                    if (currentIndex < set.count)
                    {
                        return true;
                    }
                    else
                    {
                        currentIndex--;
                        return false;
                    }
                }
#endif
            }

            /// <summary>
            /// Resets the FastHashSet enumerator.
            /// </summary>
            public void Reset()
            {
#if !Exclude_Check_For_Set_Modifications_In_Enumerator
                if (incrementForEverySetModification != set.incrementForEverySetModification)
                {
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                }
#endif

#if !Exclude_No_Hash_Array_Implementation
                if (set.IsHashing)
                {
#endif
                    currentIndex = NullIndex; // 0 is the index before the first possible node (0 is the blank node)
#if !Exclude_No_Hash_Array_Implementation
                }
                else
                {
                    currentIndex = -1;
                }
#endif
            }

            /// <summary>
            /// Implements the IDisposable.Dispose method for the FastHashSet enumerator.
            /// </summary>
            void IDisposable.Dispose()
            {
#if !Exclude_Check_For_Is_Disposed_In_Enumerator
                isDisposed = true;
#endif
            }

            /// <summary>
            ///  Gets the current item for the FastHashSet enumerator.
            /// </summary>
            public T2 Current
            {
                get
                {
#if !Exclude_No_Hash_Array_Implementation
                    if (set.IsHashing)
                    {
#endif
                        // it's easiest to just loop through the node array and skip any nodes with nextIndex = 0
                        // rather than looping through the buckets array and following the nextIndex to the end of each bucket

                        if (currentIndex > NullIndex && currentIndex < set.firstBlankAtEndIndex)
                        {
                            return set.slots[currentIndex].item;
                        }
#if !Exclude_No_Hash_Array_Implementation
                    }
                    else
                    {
                        if (currentIndex >= 0 && currentIndex < set.count)
                        {
                            return set.noHashArray[currentIndex];
                        }
                    }
#endif
                    return default;
                }
            }

            /// <summary>
            ///  Gets a reference to the current item for the FastHashSet enumerator.
            /// </summary>
            public ref T2 CurrentRef
            {
                get
                {
#if !Exclude_No_Hash_Array_Implementation
                    if (set.IsHashing)
                    {
#endif
                        // it's easiest to just loop through the node array and skip any nodes with nextIndex = 0
                        // rather than looping through the buckets array and following the nextIndex to the end of each bucket

                        if (currentIndex > NullIndex && currentIndex < set.firstBlankAtEndIndex)
                        {
                            return ref set.slots[currentIndex].item;
                        }
                        else
                        {
                            // we can just return a ref to the 0 node's item instead of throwing an exception? - this should have a default item value
                            return ref set.slots[0].item;
                        }
#if !Exclude_No_Hash_Array_Implementation
                    }
                    else
                    {
                        if (currentIndex >= 0 && currentIndex < set.count)
                        {
                            return ref set.noHashArray[currentIndex];
                        }
                        else
                        {
                            // we can just return a ref to the 0 node's item instead of throwing an exception?
                            return ref set.noHashArray[0];
                        }
                    }
#endif
                }
            }

            /// <summary>
            ///  True if the current item is valid for the FastHashSet enumerator, otherwise false.
            /// </summary>
            public bool IsCurrentValid
            {
                get
                {
#if !Exclude_No_Hash_Array_Implementation
                    if (set.IsHashing)
                    {
#endif
                        // it's easiest to just loop through the node array and skip any nodes with nextIndex = 0
                        // rather than looping through the buckets array and following the nextIndex to the end of each bucket

                        if (currentIndex > NullIndex && currentIndex < set.firstBlankAtEndIndex)
                        {
                            return true;
                        }
#if !Exclude_No_Hash_Array_Implementation
                    }
                    else
                    {
                        if (currentIndex >= 0 && currentIndex < set.count)
                        {
                            return true;
                        }
                    }
#endif
                    return false;
                }
            }

            /// <summary>
            /// Gets the Current item for the FastHashSet enumerator.
            /// </summary>
            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        public static class FastHashSetUtil
        {
            /// <summary>
            ///  Return the prime number that is equal to n (if n is a prime number) or the closest prime number greather than n.
            /// </summary>
            /// <param name="n">The lowest number to start looking for a prime.</param>
            /// <returns>The passed in n parameter value (if it is prime), or the next highest prime greater than n.</returns>
            public static int GetEqualOrClosestHigherPrime(int n)
            {
                if (n >= LargestPrimeLessThanMaxInt)
                {
                    // the next prime above this number is int.MaxValue, which we don't want to return that value because some indices increment one or two ints past this number and we don't want them to overflow
                    return LargestPrimeLessThanMaxInt;
                }

                if ((n & 1) == 0)
                {
                    n++; // make n odd
                }

                bool found;

                do
                {
                    found = true;

                    int sqrt = (int)Math.Sqrt(n);
                    for (int i = 3; i <= sqrt; i += 2)
                    {
                        int div = n / i;
                        if (div * i == n) // dividing and multiplying might be faster than a single % (n % i) == 0
                        {
                            found = false;
                            n += 2;
                            break;
                        }
                    }
                } while (!found);

                return n;
            }
        }
    }

    public struct ChainLevelAndCount : IComparable<ChainLevelAndCount>
    {
        public ChainLevelAndCount(int level, int count)
        {
            this.Level = level;
            this.Count = count;
        }

        public int Level;
        public int Count;

        public int CompareTo(ChainLevelAndCount other)
        {
            return Level.CompareTo(other.Level);
        }
    }

#if DEBUG
    public static class DebugOutput
    {
        public static void OutputEnumerableItems<T2>(IEnumerable<T2> e, string enumerableName)
        {
            System.Diagnostics.Debug.WriteLine("---start items: " + enumerableName + "---");
            int count = 0;
            foreach (T2 item in e)
            {
                System.Diagnostics.Debug.WriteLine(item.ToString());
                count++;
            }
            System.Diagnostics.Debug.WriteLine("---end items: " + enumerableName + "; count = " + count.ToString("N0") + "---");
        }

        public static void OutputSortedEnumerableItems<T2>(IEnumerable<T2> e, string enumerableName)
        {
            List<T2> lst = new List<T2>(e);
            lst.Sort();
            System.Diagnostics.Debug.WriteLine("---start items (sorted): " + enumerableName + "---");
            int count = 0;
            foreach (T2 item in lst)
            {
                System.Diagnostics.Debug.WriteLine(item.ToString());
                count++;
            }
            System.Diagnostics.Debug.WriteLine("---end items: " + enumerableName + "; count = " + count.ToString("N0") + "---");
        }
    }
#endif
}