using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Dot42;

namespace Luxmate.MMT.Utilities
{
	public class SortableList<T> : IList<T>, IList, ICollection<T>, ICollection, IEnumerable<T>, IEnumerable
	{
		// Fields
		private readonly Java.Util.ArrayList<T> list;

		// Methods
		public SortableList()
		{
			this.list = new Java.Util.ArrayList<T>();
		}

		public SortableList(Java.Lang.IIterable<T> source)
		{
			Java.Util.ICollection<T> capacity = source as Java.Util.ICollection<T>;
			if (capacity != null)
			{
				this.list = new Java.Util.ArrayList<T>(capacity);
			}
			else
			{
				this.list = new Java.Util.ArrayList<T>();
				foreach (T local in source.AsEnumerable<T>())
				{
					this.list.Add(local);
				}
			}
		}

		public SortableList(IEnumerable<T> source)
		{
			Java.Util.ICollection<T> capacity = source as Java.Util.ICollection<T>;
			if (capacity != null)
			{
				this.list = new Java.Util.ArrayList<T>(capacity);
			}
			else
			{
				this.list = new Java.Util.ArrayList<T>();
				foreach (T local in source)
				{
					this.list.Add(local);
				}
			}
		}

		public SortableList(int capacity)
		{
			this.list = new Java.Util.ArrayList<T>(capacity);
		}

		public virtual void Add(T item)
		{
			this.list.Add(item);
		}

		public int Add(object element)
		{
			int num = this.list.Size();
			this.list.Add((T)element);
			return num;
		}

		public void AddRange(IEnumerable<T> collection)
		{
			if (collection != null)
			{
				//this.list.AddAll(collection);
				foreach (var element in collection)
				{
					Add(element);
				}
			}
		}

		public void Clear()
		{
			this.list.Clear();
		}

		public bool Contains(T element)
		{
			return this.list.Contains(element);
		}

		public bool Contains(object element)
		{
			return this.list.Contains(element);
		}

		private static class CollectionsHelper
		{
			// Methods
			public static void CopyTo<E>(Java.Util.ICollection<E> collection, E[] array, int index)
			{
				if (index == 0)
				{
					collection.ToArray<E>(array);
				}
				else
				{
					E[] localArray = collection.ToArray<E>(new E[0]);
					for (int i = 0; i < localArray.Length; i = (int)(i + 1))
					{
						array[index + i] = localArray[i];
					}
				}
			}

			public static void CopyTo<E>(Java.Util.ICollection<E> collection, Array array, int index)
			{
				throw new NotImplementedException();
			}
		}

		public void CopyTo(Array array, int index)
		{
			CollectionsHelper.CopyTo<T>(this.list, array, index);
		}

		public void CopyTo(T[] array, int index)
		{
			CollectionsHelper.CopyTo<T>(this.list, array, index);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this.list);
		}

		public int IndexOf(T element)
		{
			return this.list.IndexOf(element);
		}

		public int IndexOf(object element)
		{
			return this.list.IndexOf(element);
		}

		public void Insert(int index, T element)
		{
			this.list.Add(index, element);
		}

		public void Insert(int index, object element)
		{
			this.list.Add(index, (T)element);
		}

		public bool Remove(T element)
		{
			return this.list.Remove(element);
		}

		public void Remove(object element)
		{
			this.list.Remove(element);
		}

		public void RemoveAt(int index)
		{
			this.list.Remove(this.list.Get(index));
		}

		bool ICollection<T>.Remove(T item)
		{
			return this.list.Remove(item);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		int IList<T>.Add(T element)
		{
			int num = this.list.Size();
			this.list.Add(element);
			return num;
		}

		void IList<T>.Remove(T element)
		{
			this.list.Remove(element);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public T[] ToArray()
		{
			T[] array = new T[this.list.Count];
			return this.list.ToArray<T>(array);
		}

		protected T _GetAtIndex(int index)
		{
			return this.list.Get(index);
		}

		// Properties
		public int Count
		{
			get
			{
				//return this.list.Size();
				return this.list.Count;
			}
		}

		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public T this[int index]
		{
			get
			{
				return this.list.Get(index);
			}
			set
			{
				this.list.Set(index, value);
			}
		}

		public object SyncRoot
		{
			get
			{
				return this.list;
			}
		}

		object IList.this[int index]
		{
			get
			{
				return this.list.Get(index);
			}
			set
			{
				this.list.Set(index, (T)value);
			}
		}

		// Nested Types
		public class Enumerator : Dot42.Collections.IteratorWrapper<T>
		{
			// Methods
            public Enumerator(Java.Lang.IIterable<T> iterable)
                : base(iterable)
			{
			}
		}


		public virtual void Sort(Func<T, T, int> comparer)
		{
			Java.Util.Collections.Sort<T>(this.list, new MyComparator(comparer));
		}

		private class MyComparator : Java.Util.IComparator<T>
		{
			public readonly Func<T, T, int> ValueComparator;

			public MyComparator(Func<T, T, int> valueComparator)
			{
				ValueComparator = valueComparator;
			}

			public int Compare(T a, T b)
			{
				return ValueComparator(a, b);
			}
		}

		//public void ForEach(Action<T> action)
		//{
		//    for (int i = 0; i < this.list.Count; i++)
		//    {
		//        T item = this.list.Get(i);
		//        action(item);
		//    }
		//}
	}

#if DEBUG
	public static class SortableListTest
	{
		public class Elem
		{
			public int ID { get; set; }
			public string Value { get; set; }
		}

		public class ElemList : SortableList<Elem>
		{
		}

		public static void Test()
		{
			var list = new ElemList();
			Elem el1 = new Elem() { ID = 1, Value = "1" };
			list.Add(el1);
			Elem el2 = new Elem() { ID = 2, Value = "2" };
			list.Add(el2);
			Elem el3 = new Elem() { ID = 3, Value = "3" };
			list.Add(el3);
			Elem el4 = new Elem() { ID = 4, Value = "4" };
			list.Add(el4);
			NUnit.Framework.Assert.AreEqual(4, list.Count);
			//list.Add(el1);
			//NUnit.Framework.Assert.AreEqual(4, list.Count);

			Elem el5 = new Elem() { ID = 5, Value = "5" };

		//    NUnit.Framework.Assert.IsTrue(list.GetByID(1) == el1);
		//    NUnit.Framework.Assert.IsTrue(list.GetByID(2) == el2);
		//    NUnit.Framework.Assert.IsTrue(list.GetByID(3) == el3);
		//    NUnit.Framework.Assert.IsTrue(list.GetByID(4) == el4);
		//    NUnit.Framework.Assert.IsTrue(list.GetByID(5) == null);

		//    NUnit.Framework.Assert.IsTrue(list[1] == el1);
		//    NUnit.Framework.Assert.IsTrue(list[2] == el2);
		//    NUnit.Framework.Assert.IsTrue(list[3] == el3);
		//    NUnit.Framework.Assert.IsTrue(list[4] == el4);
		//    NUnit.Framework.Assert.IsTrue(list[5] == null);

		//    NUnit.Framework.Assert.IsTrue(list.GetByIndex(0) == el1);
		//    NUnit.Framework.Assert.IsTrue(list.GetByIndex(1) == el2);
		//    NUnit.Framework.Assert.IsTrue(list.GetByIndex(2) == el3);
		//    NUnit.Framework.Assert.IsTrue(list.GetByIndex(3) == el4);
		//    NUnit.Framework.Assert.IsTrue(list.GetByIndex(4) == null);

		//    NUnit.Framework.Assert.IsTrue(list.ContainsID(1));
		//    NUnit.Framework.Assert.IsTrue(list.ContainsID(2));
		//    NUnit.Framework.Assert.IsTrue(list.ContainsID(3));
		//    NUnit.Framework.Assert.IsTrue(list.ContainsID(4));
		//    NUnit.Framework.Assert.IsFalse(list.ContainsID(5));

		//    NUnit.Framework.Assert.IsTrue(list.Contains(el1));
		//    NUnit.Framework.Assert.IsTrue(list.Contains(el2));
		//    NUnit.Framework.Assert.IsTrue(list.Contains(el3));
		//    NUnit.Framework.Assert.IsTrue(list.Contains(el4));
		//    NUnit.Framework.Assert.IsFalse(list.Contains(el5));

		//    NUnit.Framework.Assert.IsTrue(list.Remove(el2));
		//    NUnit.Framework.Assert.AreEqual(3, list.Count);
		//    NUnit.Framework.Assert.IsFalse(list.ContainsID(2));
		//    NUnit.Framework.Assert.IsFalse(list.Contains(el2));

		//    NUnit.Framework.Assert.IsFalse(list.Remove(el2));
		//    NUnit.Framework.Assert.IsFalse(list.Remove(null));

		//    list.Add(el2.ID, el2);

		//    NUnit.Framework.Assert.IsTrue(list.GetByID(1) == el1);
		//    NUnit.Framework.Assert.IsTrue(list.GetByID(2) == el2);
		//    NUnit.Framework.Assert.IsTrue(list.GetByID(3) == el3);
		//    NUnit.Framework.Assert.IsTrue(list.GetByID(4) == el4);
		//    NUnit.Framework.Assert.IsTrue(list.GetByID(5) == null);

		//    NUnit.Framework.Assert.IsTrue(list[1] == el1);
		//    NUnit.Framework.Assert.IsTrue(list[2] == el2);
		//    NUnit.Framework.Assert.IsTrue(list[3] == el3);
		//    NUnit.Framework.Assert.IsTrue(list[4] == el4);
		//    NUnit.Framework.Assert.IsTrue(list[5] == null);

		//    NUnit.Framework.Assert.IsTrue(list.GetByIndex(0) == el1);
		//    NUnit.Framework.Assert.IsTrue(list.GetByIndex(3) == el2);
		//    NUnit.Framework.Assert.IsTrue(list.GetByIndex(1) == el3);
		//    NUnit.Framework.Assert.IsTrue(list.GetByIndex(2) == el4);
		//    NUnit.Framework.Assert.IsTrue(list.GetByIndex(4) == null);

			list.Sort(delegate(Elem a, Elem b) { return 0 - a.Value.CompareTo(b.Value); });

			NUnit.Framework.Assert.IsTrue(list[3] == el1);
			NUnit.Framework.Assert.IsTrue(list[2] == el2);
			NUnit.Framework.Assert.IsTrue(list[1] == el3);
			NUnit.Framework.Assert.IsTrue(list[0] == el4);
		}


	}
#endif
}
