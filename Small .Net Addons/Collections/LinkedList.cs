﻿using System;
using System.Collections;
using System.Collections.Generic;
namespace SmallDotNetAddons.Collections;

public class LinkedList<T> : ICollection<T>, IDisposable, ICloneable
{
	private int _count = 0;
	private Node<T>? _head;
	private Node<T>? _tail;
	public int Count => _count;
	bool ICollection<T>.IsReadOnly => false;
	/// <summary>
	/// Don't use to iterate, because the access time is O((N-2)(N+1)/2).
	/// Use foreach instead.
	/// </summary>
	public T this[int index]
	{
		get => ElementAt(index).Item;
		set => ElementAt(index).Item = value;
	}

	public LinkedList() { }
	public LinkedList(IEnumerable<T> items)
	{
		Insert(0, items);
	}

	void ICollection<T>.Add(T item)
	{
		AddLast(item);
	}
	public void AddFirst(T item)
	{
		Insert(0, item);
	}
	public void AddFirst(IEnumerable<T> items)
	{
		Insert(0, items);
	}
	public void AddLast(T item)
	{
		Insert(_count, item);
	}
	public void AddLast(IEnumerable<T> items)
	{
		Insert(_count, items);
	}
	public void Clear()
	{
		_head = null;
		_tail = null;
		_count = 0;
	}
	object ICloneable.Clone()
	{
		return Clone();
	}
	public LinkedList<T> Clone()
	{
		return new(this);
	}
	public bool Contains(T item)
	{
		foreach (T toFind in this)
			if (toFind!.Equals(item))
				return true;
		return false;
	}
	public void CopyTo(T[] array, int arrayIndex)
	{
		ArgumentNullException.ThrowIfNull(array, "Array can't be null.");
		ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex, "Array index must be positive.");
		if (array.Length - arrayIndex < _count)
			throw new ArgumentException("Array too small for copy all elements.");
		foreach (T item in this)
		{
			array[arrayIndex] = item;
			arrayIndex++;
		}
	}
	public LinkedList<T> DeepClone(Func<T, T>? CustomClone = null, bool passUnclonable = false)
	{
		LinkedList<T> list = new();

		foreach (T item in this)
		{
			if (item == null)
			{
				list.AddLast(item: default);
				continue;
			}
			if (CustomClone != null)
			{
				list.AddLast(CustomClone(item));
			}
			else if (item is ICloneable cloneable)
			{
				object obj = cloneable.Clone();
				list.AddLast(obj is T clonedItem 
					? clonedItem 
					: throw new InvalidCastException($"Cloned item of type {obj.GetType()} can't be casted to {typeof(T)} type"));
			}
			else if (passUnclonable)
			{
				list.AddLast(item);
			}
			else
			{
				throw new InvalidOperationException($"Item of type {item.GetType()} cannot be cloned. Use custom cloning or allow unclonable items.");
			}
		}

		return list;
	}
	public void Dispose()
	{
		Clear();
		GC.SuppressFinalize(this);
	}
	private Node<T> ElementAt(int index)
	{
		if (index < 0 || index >= _count)
			throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.");
		Node<T> current = _head!;
		for (int i = 0; i < index; i++)
			current = current.Next!;
		return current;
	}
	public IEnumerator<T> GetEnumerator()
	{
		return new Enumerator(_head);
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
	/// <summary>
	/// Inserts an item at the specified index. If index = <see cref="Count"/>, the item is inserted at the end of the collection.
	/// </summary>
	public void Insert(int index, T item)
	{
		Node<T> node = new(item);
		Insert(index, new NodeSequence(node, node, 1));
	}
	/// <summary>
	/// Inserts items at the specified index. If index = <see cref="Count"/>, items is inserted at the end of the collection.
	/// </summary>
	public void Insert(int index, IEnumerable<T> items)
	{
		EmptyCollectionException.ThrowIfNullOrEmpty(items);
		Insert(index, NodeSequence.Create(items));
	}
	private void Insert(int index, NodeSequence sequence)
	{
		if (index < 0 || index > _count)
			throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and not bigger than the size of the collection.");

		if (index == 0)
		{
			sequence.Tail.Next = _head;
			_head = sequence.Head;
			if (_count == 0)
				_tail = sequence.Tail;
		}
		else if (index == _count)
		{
			_tail!.Next = sequence.Head;
			_tail = sequence.Tail;
		}
		else
		{
			Node<T> current = _head!;
			for (int i = 0; i < index - 1; i++)
				current = current.Next!;
			sequence.Tail.Next = current.Next;
			current.Next = sequence.Head;
		}

		_count += sequence.Count;
	}
	public bool Remove(T item)
	{
		if (_count == 0 || item == null)
		{
			return false;
		}
		else if (_count == 1)
		{
			if (_head!.AreEqual(item))
			{
				Clear();
				return true;
			}
			return false;
		}
		else
		{
			Node<T>? previous = null;
			Node<T> current = _head!;
			for (int i = 0; i < _count; i++)
			{
				if (current.AreEqual(item))
				{
					RemoveNode(current, previous);
					_count--;
					return true;
				}
				previous = current;
				current = current.Next!;
			}
		}
		return false;
	}
	public void RemoveAt(int index)
	{
		if (index < 0 || index >= _count)
			throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.");
		if (_count == 1)
		{
			Clear();
		}
		else if (index == 0)
		{
			_head = _head?.Next;
		}
		else
		{
			Node<T> previous = ElementAt(index - 1);
			RemoveNode(previous.Next!, previous);
		}
		_count--;
	}
	private void RemoveNode(Node<T> removable, Node<T>? previous)
	{
		if (removable.Equals(_head))
		{
			_head = _head.Next;
		}
		else if (removable.Equals(_tail))
		{
			previous!.Next = null;
			_tail = previous;
		}
		else
		{
			previous!.Next = removable.Next;
			removable.Next = null;
		}
	}

	internal class Node<I>(I item)
	{
		public I Item = item;
		public Node<I>? Next;
		internal bool AreEqual(I otherItem)
		{
			return EqualityComparer<I>.Default.Equals(Item, otherItem);
		}
	}
	private record NodeSequence(Node<T> Head, Node<T> Tail, int Count)
	{
		public static NodeSequence Create(IEnumerable<T> items)
		{
			EmptyCollectionException.ThrowIfNullOrEmpty(items);
			using LinkedList<T> list = new();
			foreach (T item in items) list.AddLast(item);
			return new NodeSequence(list._head!, list._tail!, list._count);
		}
	}
	public struct Enumerator : IEnumerator<T>
	{
		private Node<T>? _current;
		private Node<T>? _head;
		public T Current => _current.Item;
		object IEnumerator.Current => _current.Item;

		internal Enumerator(Node<T> head)
		{
			_head = head;
		}
		public void Dispose()
		{
			_head = null;
			_current = null;
			GC.SuppressFinalize(this);
		}
		public bool MoveNext()
		{
			if (_current == null)
			{
				_current = _head;
				return _head != null;
			}
			if (_current.Next != null)
			{
				_current = _current.Next;
				return true;
			}
			return false;
		}
		public void Reset()
		{
			_current = null;
		}
	}
}
