using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AL_Common;

public static partial class Common
{
    #region Classes

    [PublicAPI]
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ArrayWithLength<T>
    {
        public readonly T[] Array;
        public readonly int Length;

        public ArrayWithLength()
        {
            Array = System.Array.Empty<T>();
            Length = 0;
        }

        public ArrayWithLength(T[] array)
        {
            Array = array;
            Length = array.Length;
        }

        public ArrayWithLength(T[] array, int length)
        {
            Array = array;
            Length = length;
        }

        // This MUST be a method (not a static field) to maintain performance!
        public static ArrayWithLength<T> Empty() => new();

        /// <summary>
        /// Manually bounds-checked past <see cref="T:Length"/>.
        /// If you don't need bounds-checking past <see cref="T:Length"/>, access <see cref="T:Array"/> directly.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // Very unfortunately, we have to manually bounds-check here, because our array could be longer
                // than Length (such as when it comes from a pool).
                if (index > Length - 1) ThrowHelper.IndexOutOfRange();
                return Array[index];
            }
        }
    }

    // How many times have you thought, "Gee, I wish I could just reach in and grab that backing array from
    // that List, instead of taking the senseless performance hit of having it copied to a newly allocated
    // array all the time in a ToArray() call"? Hooray!
    /// <summary>
    /// Because this list exposes its internal array and also doesn't clear said array on <see cref="ClearFast"/>,
    /// it must be used with care.
    /// <para>
    /// -Only use this with value types. Reference types will be left hanging around in the array.
    /// </para>
    /// <para>
    /// -The internal array is there so you can get at it without incurring an allocation+copy.
    ///  It can very easily become desynced with the <see cref="ListFast{T}"/> if you modify it.
    /// </para>
    /// <para>
    /// -Only use the internal array in conjunction with the <see cref="Count"/> property.
    ///  Using the <see cref="ItemsArray"/>.Length value will get the array's actual length, when what you
    ///  wanted was the list's "virtual" length. This is the same as a normal List except with a normal List
    ///  the array is private so you can't have that problem.
    /// </para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicAPI]
    public sealed class ListFast<T> : IEnumerable<T>
    {
        public T[] ItemsArray;
        private int _itemsArrayLength;

        /// <summary>
        /// Properties are slow. You can set this from outside if you know what you're doing.
        /// </summary>
        public int Count;

        /// <summary>
        /// No bounds checking, so use caution!
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ItemsArray[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => ItemsArray[index] = value;
        }

        public ListFast(int capacity)
        {
            ItemsArray = new T[capacity];
            _itemsArrayLength = capacity;
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)Count)
            {
                ThrowHelper.ArgumentOutOfRange(nameof(index), "Out of range");
            }
            Count--;
            if (index < Count)
            {
                Array.Copy(ItemsArray, index + 1, ItemsArray, index, Count - index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sort(IComparer<T> comparer) => Array.Sort(ItemsArray, 0, Count, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (Count == _itemsArrayLength) EnsureCapacity(Count + 1);
            ItemsArray[Count++] = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ListFast<T> items)
        {
            EnsureCapacity(Count + items.Count);
            // We usually add small enough arrays that a loop is faster
            for (int i = 0; i < items.Count; i++)
            {
                ItemsArray[Count + i] = items[i];
            }
            Count += items.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ListFast<T> items, int count)
        {
            EnsureCapacity(Count + count);
            // We usually add small enough arrays that a loop is faster
            for (int i = 0; i < count; i++)
            {
                ItemsArray[Count + i] = items[i];
            }
            Count += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange_Large(T[] items)
        {
            int length = items.Length;
            EnsureCapacity(Count + length);
            Array.Copy(items, 0, ItemsArray, Count, length);
            Count += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange_Large(ListFast<T> items)
        {
            int length = items.Count;
            EnsureCapacity(Count + length);
            Array.Copy(items.ItemsArray, 0, ItemsArray, Count, length);
            Count += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddFast(T item) => ItemsArray[Count++] = item;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearFullAndAdd(T[] items)
        {
            ClearFull();
            AddRange_Large(items);
        }

        public void InsertAtZeroFast(T item)
        {
            Array.Copy(ItemsArray, 0, ItemsArray, 1, Count);
            ItemsArray[0] = item;
            Count++;
        }

        /*
        Honestly, for fixed-size value types, doing an Array.Clear() is completely unnecessary. For reference
        types, you definitely want to clear it to get rid of all the references, but for ints or chars etc.,
        all a clear does is set a bunch of fixed-width values to other fixed-width values. You don't save
        space and you don't get rid of loose references, all you do is waste an alarming amount of time. We
        drop fully 200ms from the Unicode parser just by using the fast clear!
        */
        /// <summary>
        /// Just sets <see cref="Count"/> to 0. Doesn't zero out the array or do anything else whatsoever.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearFast() => Count = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearFull()
        {
            if (Count > 0)
            {
                Array.Clear(ItemsArray, 0, Count);
                Count = 0;
            }
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _itemsArrayLength;
            set
            {
                if (value == _itemsArrayLength) return;
                if (value > 0)
                {
                    T[] objArray = new T[value];
                    if (Count > 0) Array.Copy(ItemsArray, 0, objArray, 0, Count);
                    ItemsArray = objArray;
                    _itemsArrayLength = value;
                    if (_itemsArrayLength < Count) Count = _itemsArrayLength;
                }
                else
                {
                    ItemsArray = Array.Empty<T>();
                    _itemsArrayLength = 0;
                    Count = 0;
                }
            }
        }

        public void HardReset(int capacity)
        {
            ClearFast();
            Capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRecycleState(int count, int maxEntriesBeforeCapacityReset = 25_000)
        {
            if (_itemsArrayLength > maxEntriesBeforeCapacityReset)
            {
                Capacity = 0;
            }

            Count = count;

            if (_itemsArrayLength < Count)
            {
                T[] objArray = new T[Count];
                if (Count > 0) Array.Copy(ItemsArray, 0, objArray, 0, _itemsArrayLength);
                ItemsArray = objArray;
                _itemsArrayLength = Count;
                if (_itemsArrayLength < Count) Count = _itemsArrayLength;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int min)
        {
            if (_itemsArrayLength >= min) return;
            int newCapacity = _itemsArrayLength == 0 ? 4 : _itemsArrayLength * 2;
            if ((uint)newCapacity > 2146435071U) newCapacity = 2146435071;
            if (newCapacity < min) newCapacity = min;
            Capacity = newCapacity;
        }

        public ListFast<T> ClearedAndWithCapacityAtLeast(int capacity)
        {
            EnsureCapacity(capacity);
            ClearFast();
            return this;
        }

        public void ClearFastAndEnsureCapacity(int capacity)
        {
            EnsureCapacity(capacity);
            ClearFast();
        }

        private Enumerator? _enumerator;

        public IEnumerator<T> GetEnumerator() => _enumerator ??= new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            private readonly ListFast<T> _list;
            private int _index;
            private T _current;

            internal Enumerator(ListFast<T> list)
            {
                _list = list;
                _index = 0;
                _current = default!;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                ListFast<T> localList = _list;

                if (((uint)_index < (uint)localList.Count))
                {
                    _current = localList.ItemsArray[_index];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                _index = _list.Count + 1;
                _current = default!;
                return false;
            }

            public readonly T Current => _current;

            readonly object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _list.Count + 1)
                    {
                        ThrowHelper.ThrowInvalidOperationException(SR.InvalidOperation_EnumOpCantHappen);
                    }
                    return _current!;
                }
            }

            void IEnumerator.Reset()
            {
                _index = 0;
                _current = default!;
            }
        }
    }

    #endregion

    #region Methods

    #region Array initialization

    /// <summary>
    /// Returns an array of type <typeparamref name="T"/> with all elements initialized to non-null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="length"></param>
    public static T[] InitializedArray<T>(int length) where T : new()
    {
        T[] ret = new T[length];
        for (int i = 0; i < length; i++)
        {
            ret[i] = new T();
        }
        return ret;
    }

    /// <summary>
    /// Returns an array of type <typeparamref name="T"/> with all elements initialized to <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="length"></param>
    /// <param name="value">The value to initialize all elements with.</param>
    public static T[] InitializedArray<T>(int length, T value) where T : new()
    {
        T[] ret = new T[length];
        for (int i = 0; i < length; i++)
        {
            ret[i] = value;
        }
        return ret;
    }

    /// <summary>
    /// Returns two arrays of type <typeparamref name="T1"/> and <typeparamref name="T2"/> respectively,
    /// with all elements initialized to non-null. Uses a single assignment loop for performance.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="length"></param>
    /// <param name="array1"></param>
    /// <param name="array2"></param>
    public static void InitializeArrays<T1, T2>(int length,
        out T1[] array1,
        out T2[] array2)
        where T1 : new()
        where T2 : new()
    {
        array1 = new T1[length];
        array2 = new T2[length];
        for (int i = 0; i < length; i++)
        {
            array1[i] = new T1();
            array2[i] = new T2();
        }
    }

    #endregion

    public static T[] CombineArrays<T>(params T[][] arrays)
    {
        int totalLen = 0;
        foreach (T[] array in arrays)
        {
            totalLen += array.Length;
        }

        T[] ret = new T[totalLen];

        int pos = 0;
        foreach (T[] array in arrays)
        {
            int arrayLen = array.Length;

            Array.Copy(array, 0, ret, pos, arrayLen);

            pos += arrayLen;
        }

        return ret;
    }

    /// <summary>
    /// Clears <paramref name="array"/> and returns it back.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">The array to clear.</param>
    /// <returns>A cleared version of <paramref name="array"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Cleared<T>(this T[] array)
    {
        Array.Clear(array, 0, array.Length);
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear<T>(this T[] array) => Array.Clear(array, 0, array.Length);

    public static bool StartsWith(this byte[] first, byte[] second)
    {
        if (first.Length < second.Length) return false;

        for (int i = 0; i < second.Length; i++)
        {
            if (first[i] != second[i]) return false;
        }
        return true;
    }

    public static void AddRange_Small<T>(this List<T> list, List<T> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            list.Add(items[i]);
        }
    }

    public static void AddRange_Small<T>(this List<T> list, T[] items)
    {
        foreach (T item in items)
        {
            list.Add(item);
        }
    }

    #region Clear and add

    public static void ClearAndAdd_Single<T>(this List<T> list, T item)
    {
        list.Clear();
        list.Add(item);
    }

    public static void ClearAndAdd_Small<T>(this List<T> list, List<T> items)
    {
        list.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            list.Add(items[i]);
        }
    }

    public static void ClearAndAdd_Small<T>(this List<T> list, T[] items)
    {
        list.Clear();
        foreach (T item in items)
        {
            list.Add(item);
        }
    }

    /// <summary>
    /// Uses AddRange(), which incurs an extra copy of the passed array for no conceivable reason.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="items"></param>
    public static void ClearAndAdd_Large<T>(this List<T> list, T[] items)
    {
        list.Clear();
        /*
        @MEM(List.AddRange): This thing allocates a new array every time! WTF?!

        T[] array = new T[count];
        objs.CopyTo(array, 0);
        array.CopyTo((Array) this._items, index);

        @NET5(List.AddRange): They fixed this in .NET 7 at least.
        c.CopyTo(_items, index);
        */
        list.AddRange(items);
    }

    public static void ClearAndEnsureCapacity<T>(this List<T> list, int capacity)
    {
        list.Clear();
        if (list.Capacity < capacity) list.Capacity = capacity;
    }

    #endregion

    #region Dispose and clear

    public static void DisposeAll<T>(this T[] array) where T : IDisposable?
    {
        foreach (T item in array)
        {
            item?.Dispose();
        }
    }

    public static void DisposeRange<T>(this T[] array, int start, int end) where T : IDisposable?
    {
        for (int i = start; i < end; i++)
        {
            array[i]?.Dispose();
        }
    }

    #endregion

    public static HashSetI ToHashSetI(this IEnumerable<string> source) => new(source);

    public static HashSetPathI ToHashSetPathI(this IEnumerable<string> source) => new(source);

    #endregion
}
