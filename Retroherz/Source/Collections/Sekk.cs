using System;
using System.Collections.Generic;
namespace Retroherz.Collections;
// TODO: Find better name!
// MemoryBag, Sack, Kit, Pouch, Pocket, Pack, Satchel, Tote, Poke, Sac, Sekk, Case, Duffel

// Sekk & Stakk ??
// Sack & Stack ??
// Pouch

/// <summary>
///	Represents a generic collection of unmanaged types.
/// Powered by Span and Memory.
/// </summary>
public class Sekk<T>: IEquatable<Sekk<T>> where T: unmanaged
{
	private Memory<T> _data;
	private int _count;

	/// <summary>
	///	Returns the current maximum capacity of the collection.
	/// </summary>
	///	<returns>
	///	The the current maximum capacity of the collection.
	///	</returns>
	public int Capacity => _data.Length;

	/// <summary>
	///	Get the number of elements contained in the collection.
	/// </summary>
	///	<returns>
	///	The number of elements in the current collection.
	///	</returns>
	public int Count => _data.Slice(0, _count).Length;

	///	<summary>
	///	Construct a collection with a given capacity.
	///	</summary>
	public Sekk(int size = 16)
	{
		_data = new T[size];
		_count = 0;
	}

	/// <summary>
	///	Indexer of the collection.
	/// </summary>
	public T this[int i]
	{
		get => (i < _count) ? _data.Span[i] : throw new IndexOutOfRangeException();
		set => _data.Span[i] = (i < _count) ? value : throw new IndexOutOfRangeException();
	}

	/// <summary>
	///	Add an element to the collection.
	/// </summary>
	public void Add(T element)
	{
		// Resize collection if needed.
		if (_data.Length < _count + 1)
		{
			int size = _data.Length * 2;
			Span<T> temp = stackalloc T[size];
			_data.Span.CopyTo(temp);
			_data = new T[size];
			Console.WriteLine($"Resized [{size}]");
		}

		// Add the element.
		_data.Span[_count] = element;

		// Increment the counter.
		_count++;
	}

	/// <summary>
	///	Searches an entire sorted collection for a specified value using the specified TComparer generic type.
	///	<summary>
	public int BinarySearch(T value, IComparer<T> comparer) => (
		_data.Slice(0, _count).Span.BinarySearch(value, comparer)
	);

	/// <summary>
	///	Searches an entire sorted collection for a specified value using the specified IComparable generic interface.
	///	<summary>
	public int BinarySearch(IComparable<T> comparable) => (
		_data.Slice(0, _count).Span.BinarySearch(comparable)
	);

	///	<summary>
	///	Searches an entire sorted Scollection for a value using the specified TComparable  generic type.
	/// </summary>
	public int BinarySearch(T value, IComparable<T> comparable) => (
		_data.Slice(0, _count).Span.BinarySearch<T, IComparable<T>>(comparable)
	);

	/// <summary>
	///	Searches an entire one-dimensional sorted array for a specific element,
	/// using the IComparable generic interface implemented by each element of the Array and by the specified object.
	/// </summary>
	///	<remarks>
	///	Remarks< Because a call to the ToArray method performs a heap allocation, it should generally be avoided.
	///	</remarks>
	public int BinarySearch(T value) => (
		Array.BinarySearch<T>(_data.Slice(0, _count).ToArray(), value)
	);

	/// <summary>
	/// Clears the content of this collection.
	/// </summary>
	public void Clear()
	{
		_data.Span.Clear();	// Not sure if needed?
		_count = 0;
	}

	///	<summary>
	///	Returns true if the collecton includes the specified element.
	///	</summary>
	public bool Contains(T element) => this.Find(element) < 0 ? false : true;

	///	<summary>
	///	Copies the contents of this collection into a destination Memory.
	///	</summary>
	public void CopyTo(Memory<T> destination) => _data.Slice(0, _count).CopyTo(destination);

	///	<summary>
	///	Copies the contents of this collection into a destination Span.
	///	</summary>
	public void CopyTo(Span<T> destination) => _data.Slice(0, _count).Span.CopyTo(destination);

	///	<summary>
	///	Copies the contents of this collection into a destination collection.
	///	</summary>
	public void CopyTo(Sekk<T> destination) => _data.Slice(0, _count).CopyTo(destination._data);

	/// <summary>
	///	Fills the elements of this collection with a specified value.
	///	</summary>
	public void Fill(T value) => _data.Span.Fill(value);

	///	<summary>
	///	Search for an element in the collection.
	///	</summary>
	///	<returns>
	///	Index of specified element.
	/// </returns>
	public int Find(T element)
	{
		for (int i = 0; i < this.Count; i++)
			if (element.Equals(_data.Span[i]))
				return i;

		return -1;
	}

	/// <summary>
	///	Returns an enumerator for this collection.
	/// </summary>
	///	<returns>
	///	An enumerator for this collecction.
	///	</returns>
	public Span<T>.Enumerator GetEnumerator() => _data.Span.Slice(0, _count).GetEnumerator();

	/// <summary>
	///	Sorts the elements in the entire collection using the specified comparison.
	///	</summary>
	public void Sort(Comparison<T> comparison) => _data.Slice(0, _count).Span.Sort(comparison);

	/// <summary>
	///	Removes the element at the specified index from the collection.
	/// </sumary>
	/// <returns>
	///	The removed element.
	/// </returns>
	public T Remove(int index)
	{
		T removed = _data.Span[index];

		// Last element gets freed position.
		_data.Span[index] = _data.Span[_count - 1];

		_count--;

		return removed;
	}

	/// <summary>
	///	Removes the specified element from the collection.
	/// </sumary>
	/// <returns>
	///	The removed element.
	///	</returns>
	public T Remove(T element)
	{
		int index = this.Find(element);

		T removed = _data.Span[index];

		_data.Span[index] = _data.Span[_count - 1];

		_count--;

		return removed;
	}

	///	<summary>
	///	Returns a value that indicates whether the current collection is empty.
	///	</summary>
	public bool IsEmpty() => _data.IsEmpty;

	/// <summary>
	///	Removes duplicate elements from collection.
	/// </summary>
	///	<returns>
	///	Returns a Span of non duplicate elements.
	///	</returns>
	public Span<T> Unique()
	{
		int k = 0;
		for (int i = 0; i < this.Count; i++)
		{
			bool isDuplicate = false;
			for (int j = i + 1; j < this.Count; j++)
				if (_data.Span[i].Equals(_data.Span[j]))
				{
					isDuplicate = true;
					break;
				}

			if (!isDuplicate)
			{
				_data.Span[k] = _data.Span[i];
				k++;
			}
		}

		_count = k;

		return _data.Slice(0, _count).Span;
	}

	/// <summary>
	/// Returns a new copy of the collection without duplicates.
	/// </summary>
	///	<returns>
	///	A copy of the current collection with no duplicates.
	///	</returns>
	public Sekk<T> UniqueCopy()
	{
		Sekk<T> copy = new Sekk<T>(this.Count);
		
		for (int i = 0; i < this.Count; i++)
		{
			bool isDuplicate = false;
			for (int j = i + 1; j < this.Count; j++)
				if (_data.Span[i].Equals(_data.Span[j]))
				{
					isDuplicate = true;
					break;
				}

			if (!isDuplicate)
				copy.Add(_data.Span[i]);
		}

		return copy;
	}

	public static implicit operator Span<T>(Sekk<T> spanBag) => spanBag._data.Slice(0, spanBag._count).Span;
	public static implicit operator ReadOnlySpan<T>(Sekk<T> spanBag) => spanBag._data.Slice(0, spanBag._count).Span;
	public static implicit operator Memory<T>(Sekk<T> spanBag) => spanBag._data.Slice(0, spanBag._count);
	public static implicit operator ReadOnlyMemory<T>(Sekk<T> spanBag) => spanBag._data.Slice(0, spanBag._count);

	/// <summary>
	/// Returns true if the collection equals the other collection.
	/// </summary>
	public bool Equals(Sekk<T> other) => (
		(Capacity == other.Capacity) &&
		(Count == other.Count)
	);

	// Overrides
	/// <summary>
	/// Returns true if the collection equals the object.
	/// </summary>
	public override bool Equals(object obj) => (
		(obj is Sekk<T> other) &&
		(other.Capacity, other.Count).Equals((Capacity, Count))
	);

	/// <summary>
	///	Returns the hash code for this instance.
	/// </summary>
	/// <returns>
	///	A 32-bit signed integer that is the hash code for this instance.
	///	</returns>
	public override int GetHashCode() => (Capacity, Count).GetHashCode();

	public override string ToString() => "{" + $"Capacity:{Capacity} Count:{Count}" + "}";
}