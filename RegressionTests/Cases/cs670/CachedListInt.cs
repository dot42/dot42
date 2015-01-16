using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Luxmate.MMT.Utilities
{
	public interface ICachedListIntItem
	{
		int CacheID { get; }
	}

	public class CachedListInt<T> : SortableList<T> where T : class, ICachedListIntItem
	{
		public CachedListInt()
		{
		}

		public IEnumerable<T> Items { get { return this; } }

		//public override string ToString()
		//{
		//    return _Items.ToString();
		//}

		new public T this[int itemID]
		{
			get { return _GetItemFromCache(itemID); }
		}

		public virtual T GetByID(int itemID)
		{
			return _GetItemFromCache(itemID);
		}

		public bool ContainsID(int itemID)
		{
			return (_GetItemFromCache(itemID) != null);
			//return base.ContainsKey(itemID);
		}

		public T GetByIndex(int index)
		{
			if (index < 0 || index >= Count)
				return default(T);
			return base._GetAtIndex(index);
		}

		//public void EnsureIsFirstItem(T item)
		//{
		//    if (item == null) return;

		//    int index = _Items.IndexOf(item);
		//    if (index == 0) return;

		//    if (index < 0)
		//    {
		//        Insert(0, item);
		//    }
		//    else
		//    {
		//        _Items.Remove(item);
		//        _Items.Insert(0, item);
		//    }
		//}


		public override void Add(T item)
		{
			if (item == null) return;
			if (Contains(item)) return;
			if (ContainsID(item.CacheID)) return;
			base.Add(item);
		}


		//public virtual void AddRange(IEnumerable<T> collection)
		//{
		//    if (collection == null) return;
		//    foreach (T item in collection)
		//    {
		//        Add(item);
		//    }
		//}

		//public virtual void AddOnce(T item)
		//{
		//    if (item == null) return;
		//    if (Contains(item)) return;
		//    Add(item);
		//}

		//public virtual void AddRangeOnce(IEnumerable<T> collection)
		//{
		//    if (collection == null) return;
		//    foreach (T item in collection)
		//    {
		//        AddOnce(item);
		//    }
		//}

		////public void OnIDChanged(T item)
		////{
		////    if (item == null) return;
		////    if (_Items.Contains(item) == false) return;

		////    _RemoveItemFromCacheByValue(item);
		////    _AddItemToCache(item);
		////}

		//public void Clear()
		//{
		//    _Items.Clear();
		//    _ItemsByID.Clear();
		//}

		//public bool Contains(T item)
		//{
		//    if (item == null) return false;
		//    //return (_Items.Contains(item) || ContainsID(item.CacheID));
		//    return base.ContainsValue(item);
		//}

		//public void CopyTo(T[] array, int arrayIndex)
		//{
		//    _Items.CopyTo(array, arrayIndex);
		//}

		//public int Count
		//{
		//    get { return base.Size(); }
		//}

		//bool ICollection<T>.IsReadOnly
		//{
		//    get { return false; }
		//}

		//public bool Remove(T item)
		//{
		//    if (item == null) return false;
		//    if (base.Size() == 0) return false;
		//    var values = base.Values();
		//    for (var it = values.Iterator(); it.HasNext(); )
		//    {
		//        var element = it.Next();
		//        if (element == item)
		//        {
		//            it.Remove();
		//            return true;
		//        }
		//    }
		//    return false;
		//}

		public bool RemoveByID(int id)
		{
			//if (id == null) return false;
			if (Count == 0) return false;
			T item = _GetItemFromCache(id);
			if (item != null)
			{
				base.Remove(item);
				return true;
			}
			return false;
		}

		//public bool Remove(T item)
		//{
		//    throw new NotImplementedException();
		//    //if (item == null) return false;
		//    //if (_Items.Contains(item) == false) return false;
		//    //_RemoveItemFromCache(item);
		//    //return _Items.Remove(item);
		//}

		//public IEnumerator<T> GetEnumerator()
		//{
		//    return _Items.GetEnumerator();
		//}

		//System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		//{
		//    return ((System.Collections.IEnumerable)_Items).GetEnumerator();
		//}

		//public T[] ToArray()
		//{
		//    return _Items.ToArray();
		//}

		////public void Sort(Comparison<T> comparision)
		////{
		////    _Items.Sort(comparision);
		////}


		////public void ForEach(Action<T> action)
		////{
		////    _Items.ForEach(action);
		////}

		//#endregion

		//protected T _GetItemFromCache(TCacheID id)
		//{
		//    T item = default(T);
		//    if (id != null)
		//    {
		//        _ItemsByID.TryGetValue(id, out item);
		//    }
		//    return item;
		//}
		protected T _GetItemFromCache(int id)
		{
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				T item = base._GetAtIndex(i);
				if (item.CacheID == id)
					return item;
			}
			//foreach (T item in this)
			//{
			//    if (item.CacheID == id)
			//        return item;
			//}
			return default(T);
			////if (id != null && base.ContainsKey(id))
			//if (base.ContainsKey(id))
			//{
			//    return base.Get(id);
			//}
			//return item;
		}

		//protected void _AddItemToCache(T item)
		//{
		//    if (item == null) return;

		//    TCacheID id = item.CacheID;
		//    if (id == null) return;

		//    if (_ItemsByID.ContainsKey(id) == false)
		//    {
		//        _ItemsByID.Add(id, item);
		//    }
		//}

		//protected void _RemoveItemFromCache(T item)
		//{
		//    if (item == null) return;

		//    TCacheID id = item.CacheID;
		//    if (id == null) return;

		//    if (_ItemsByID.ContainsKey(id))
		//    {
		//        _ItemsByID.Remove(id);
		//        //_ItemsByID.KeySet().Remove(id);
		//    }
		//}

		//protected void _RemoveItemFromCacheByValue(T item)
		//{
		//    if (item == null) return;

		//    //foreach (KeyValuePair<TCacheID, T> kv in _ItemsByID)
		//    using (var wrap = new Dot42.Collections.IterableWrapper<Java.Util.IMap_IEntry<TCacheID, T>>(_ItemsByID.EntrySet()).GetEnumerator())
		//    {
		//        while(wrap.MoveNext())
		//        {
		//            var kv = wrap.Current;
		//            if (kv.GetValue() == item)
		//            {
		//                _ItemsByID.Remove(kv.GetKey());
		//                break;
		//            }
		//        }
		//    }
		//}

	}

#if DEBUG
	public static class CachedListIntTest
	{
		public class Elem : ICachedListIntItem
		{
			public int ID { get; set; }
			public string Value { get; set; }
			public int Value2 { get; set; }

			//int ICachedListIntItem<int>.CacheID { get { return ID; } }
			public int CacheID { get { return ID; } }
		}

		public class ElemList : CachedListInt<Elem>
		{
		}

		public static void Test()
		{
			var list = new ElemList();
			NUnit.Framework.Assert.AreEqual(0, list.Count);

			Elem el1 = new Elem() { ID = 1, Value = "1", Value2 = 11 };
			list.Add(el1);
			NUnit.Framework.Assert.AreEqual(1, list.Count);
			Elem el2 = new Elem() { ID = 2, Value = "2", Value2 = 22 };
			list.Add(el2);
			Elem el3 = new Elem() { ID = 3, Value = "3", Value2 = 33 };
			list.Add(el3);
			Elem el4 = new Elem() { ID = 4, Value = "4", Value2 = 44 };
			list.Add(el4);
			NUnit.Framework.Assert.AreEqual(4, list.Count);
			list.Add(el1);
			NUnit.Framework.Assert.AreEqual(4, list.Count);

			Elem el5 = new Elem() { ID = 5, Value = "5", Value2 = 55 };

			NUnit.Framework.Assert.IsTrue(list.GetByID(1) == el1);
			NUnit.Framework.Assert.IsTrue(list.GetByID(2) == el2);
			NUnit.Framework.Assert.IsTrue(list.GetByID(3) == el3);
			NUnit.Framework.Assert.IsTrue(list.GetByID(4) == el4);
			NUnit.Framework.Assert.IsTrue(list.GetByID(5) == null);

			NUnit.Framework.Assert.IsTrue(list[1] == el1);
			NUnit.Framework.Assert.IsTrue(list[2] == el2);
			NUnit.Framework.Assert.IsTrue(list[3] == el3);
			NUnit.Framework.Assert.IsTrue(list[4] == el4);
			NUnit.Framework.Assert.IsTrue(list[5] == null);

			NUnit.Framework.Assert.IsTrue(list.GetByIndex(0) == el1);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(1) == el2);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(2) == el3);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(3) == el4);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(4) == null);

			NUnit.Framework.Assert.IsTrue(list.ContainsID(1));
			NUnit.Framework.Assert.IsTrue(list.ContainsID(2));
			NUnit.Framework.Assert.IsTrue(list.ContainsID(3));
			NUnit.Framework.Assert.IsTrue(list.ContainsID(4));
			NUnit.Framework.Assert.IsFalse(list.ContainsID(5));

			NUnit.Framework.Assert.IsTrue(list.Contains(el1));
			NUnit.Framework.Assert.IsTrue(list.Contains(el2));
			NUnit.Framework.Assert.IsTrue(list.Contains(el3));
			NUnit.Framework.Assert.IsTrue(list.Contains(el4));
			NUnit.Framework.Assert.IsFalse(list.Contains(el5));

			NUnit.Framework.Assert.IsTrue(list.Remove(el2));
			NUnit.Framework.Assert.AreEqual(3, list.Count);
			NUnit.Framework.Assert.IsFalse(list.ContainsID(2));
			NUnit.Framework.Assert.IsFalse(list.Contains(el2));

			NUnit.Framework.Assert.IsFalse(list.Remove(el2));
			NUnit.Framework.Assert.IsFalse(list.Remove(null));

			list.Add(el2);

			NUnit.Framework.Assert.IsTrue(list.GetByID(1) == el1);
			NUnit.Framework.Assert.IsTrue(list.GetByID(2) == el2);
			NUnit.Framework.Assert.IsTrue(list.GetByID(3) == el3);
			NUnit.Framework.Assert.IsTrue(list.GetByID(4) == el4);
			NUnit.Framework.Assert.IsTrue(list.GetByID(5) == null);

			NUnit.Framework.Assert.IsTrue(list[1] == el1);
			NUnit.Framework.Assert.IsTrue(list[2] == el2);
			NUnit.Framework.Assert.IsTrue(list[3] == el3);
			NUnit.Framework.Assert.IsTrue(list[4] == el4);
			NUnit.Framework.Assert.IsTrue(list[5] == null);

			NUnit.Framework.Assert.IsTrue(list.GetByIndex(0) == el1);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(3) == el2);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(1) == el3);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(2) == el4);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(4) == null);

			list.Sort(delegate(Elem a, Elem b) { return a.Value.CompareTo(b.Value); });
			list.Sort((a, b) => a.Value.CompareTo(b.Value));

			NUnit.Framework.Assert.IsTrue(list.GetByIndex(0) == el1);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(1) == el2);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(2) == el3);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(3) == el4);

			list.Sort(delegate(Elem a, Elem b) { return 0 - a.Value.CompareTo(b.Value); });

			NUnit.Framework.Assert.IsTrue(list.GetByIndex(3) == el1);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(2) == el2);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(1) == el3);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(0) == el4);

			//list.Sort(delegate(Elem a, Elem b) { return 0 - a.Value2.CompareTo(b.Value2); });
			/*list.Sort(delegate(Elem a, Elem b) { return  CompareHelper.CompareTo(a.Value2, b.Value2); });

			NUnit.Framework.Assert.IsTrue(list.GetByIndex(0) == el1);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(1) == el2);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(2) == el3);
			NUnit.Framework.Assert.IsTrue(list.GetByIndex(3) == el4);*/
		}


	}
#endif
}
