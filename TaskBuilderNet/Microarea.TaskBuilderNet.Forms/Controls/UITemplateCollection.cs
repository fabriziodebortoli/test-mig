using System;
using System.Collections;
using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.Forms.Controls
{

	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	//=============================================================================================
	public class TemplateItemCollection<TOriginalCollection, TDerivedCollectionItem> : IList<TDerivedCollectionItem>, IList where TOriginalCollection : IList where TDerivedCollectionItem : class
	{
		TOriginalCollection originalCollection;

		//-----------------------------------------------------------------------------------------
		public TemplateItemCollection(TOriginalCollection originalCollection)
		{
			this.originalCollection = originalCollection;
		}

		//-----------------------------------------------------------------------------------------
		public int IndexOf(TDerivedCollectionItem item)
		{
			return originalCollection.IndexOf(item);
		}

		//-----------------------------------------------------------------------------------------
		public void Insert(int index, TDerivedCollectionItem item)
		{
			originalCollection.Insert(index, item);
		}

		//-----------------------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			originalCollection.RemoveAt(index);
		}

		//-----------------------------------------------------------------------------------------
		public TDerivedCollectionItem this[int index]
		{
			get
			{
				return (TDerivedCollectionItem)originalCollection[index] ;
			}
			set
			{
				originalCollection[index] = value;
			}
		}

		//-----------------------------------------------------------------------------------------
		public void Add(TDerivedCollectionItem item)
		{
			originalCollection.Add(item);
		}

		//-----------------------------------------------------------------------------------------
		public void Clear()
		{
			originalCollection.Clear();
		}

		//-----------------------------------------------------------------------------------------
		public bool Contains(TDerivedCollectionItem item)
		{
			return originalCollection.Contains(item);
		}

		//-----------------------------------------------------------------------------------------
		public void CopyTo(TDerivedCollectionItem[] array, int arrayIndex)
		{
		}

		//-----------------------------------------------------------------------------------------
		public int Count
		{
			get { return originalCollection.Count; }
		}

		//-----------------------------------------------------------------------------------------
		public bool IsReadOnly
		{
			get { return originalCollection.IsReadOnly; }
		}

		//-----------------------------------------------------------------------------------------
		public bool Remove(TDerivedCollectionItem item)
		{
			originalCollection.Remove(item);
			return true;
		}

		//-----------------------------------------------------------------------------------------
		public IEnumerator<TDerivedCollectionItem> GetEnumerator()
		{
			return originalCollection.GetEnumerator() as IEnumerator<TDerivedCollectionItem>;
		}

		//-----------------------------------------------------------------------------------------
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return originalCollection.GetEnumerator();
		}

		//-----------------------------------------------------------------------------------------
		public int Add(object value)
		{
			TDerivedCollectionItem itemToAdd = value as TDerivedCollectionItem;
			if (itemToAdd != null)
			{
				originalCollection.Add(itemToAdd);
				return originalCollection.IndexOf(itemToAdd);
			}
			return -1;
		}

		//-----------------------------------------------------------------------------------------
		public bool Contains(object value)
		{
			TDerivedCollectionItem item = value as TDerivedCollectionItem;
			if (item != null)
				return originalCollection.Contains(item);
			else return false;
		}

		//-----------------------------------------------------------------------------------------
		public int IndexOf(object value)
		{
			TDerivedCollectionItem item = value as TDerivedCollectionItem;
			if (item != null)
				return originalCollection.IndexOf(item);
			else return -1;
		}

		//-----------------------------------------------------------------------------------------
		public void Insert(int index, object value)
		{
			TDerivedCollectionItem item = value as TDerivedCollectionItem;
			if (item != null)
			originalCollection.Insert(index, item);
		}

		//-----------------------------------------------------------------------------------------
		public bool IsFixedSize
		{
			get { return false; }
		}

		//-----------------------------------------------------------------------------------------
		public void Remove(object value)
		{
			TDerivedCollectionItem item = value as TDerivedCollectionItem;
			if (item != null)
			originalCollection.Remove(item);
		}

		//-----------------------------------------------------------------------------------------
		object IList.this[int index]
		{
			get
			{
				return originalCollection[index];
			}
			set
			{
				originalCollection[index] = value as TDerivedCollectionItem;
			}
		}

		//-----------------------------------------------------------------------------------------
		public void CopyTo(Array array, int index)
		{
		}

		//-----------------------------------------------------------------------------------------
		public bool IsSynchronized
		{
			get { return false; }
		}

		//-----------------------------------------------------------------------------------------
		public object SyncRoot
		{
			get { return null; }
		}
	}
}