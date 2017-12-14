using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	#region ApplicationsComboBox class

	//============================================================================
	public partial class ApplicationsComboBox : System.Windows.Forms.ComboBox
	{
		private IPathFinder	pathFinder = null;
		ApplicationsComboBoxItemsCollection listItems = null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public ApplicationsComboBox(IPathFinder aPathFinder)
		{
            InitializeComponent();
            
            listItems = new ApplicationsComboBoxItemsCollection(this);

			Init(aPathFinder);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public ApplicationsComboBox() : this(null)
		{
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void Init(IPathFinder aPathFinder)
		{
			pathFinder = aPathFinder;

		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public IPathFinder PathFinder 
		{ 
			get { return pathFinder;} 
			set
			{
				if (pathFinder == value)
					return;

				pathFinder = value;
			
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public IEnumerator Enumerator
		{
			get { return base.Items.GetEnumerator(); }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int ItemsCount
		{
			get { return base.Items.Count; }
		}

		/// <summary>
		/// The items in the list box
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		new public ApplicationsComboBoxItemsCollection Items
		{
			get { return listItems; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetApplicationInfoAt(int index, ApplicationInfo aApplicationInfo)
		{
			SetElementAt(index, new ApplicationsComboBoxItem(aApplicationInfo));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetElementAt(int index, ApplicationsComboBoxItem item)
		{
			if (index < 0 || index >= ItemsCount)
				return;

			base.Items[index] = item;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public IApplicationInfo GetApplicationInfoAt(int index)
		{
			ApplicationsComboBoxItem item = GetElementAt(index);
			if (item == null)
				return null;

			return item.ApplicationInfo;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public ApplicationsComboBoxItem GetElementAt(int index)
		{
			if (index < 0 || index >= ItemsCount)
			{
				Debug.Fail("Error in ApplicationsComboBox.GetElementAt");
				return null;
			}

			return (ApplicationsComboBoxItem)base.Items[index];
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetApplicationNameAt(int index)
		{
			IApplicationInfo appInfo = GetApplicationInfoAt(index);
			if (appInfo == null)
				return String.Empty;

			return appInfo.Name;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertApplicationInfoAt(int index, IApplicationInfo aApplicationInfo)
		{
			if (index < 0)
				return -1;

			return InsertItemAt(index, new ApplicationsComboBoxItem(aApplicationInfo));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertItemAt(int index, ApplicationsComboBoxItem item)
		{
			item.itemIndex = index;
			base.Items.Insert(index, item);
			return index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddApplicationInfo(IApplicationInfo aApplicationInfo)
		{
			return AddItem(new ApplicationsComboBoxItem(aApplicationInfo));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddItem(ApplicationsComboBoxItem item)
		{
			return base.Items.Add(item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool RemoveApplicationInfo(IApplicationInfo aApplicationInfo)
		{
			if (aApplicationInfo == null)
				return false;

			int index = FindApplicationIndex(aApplicationInfo.Name);
			while (index >= 0)
			{
				RemoveItemAt(index);

				index = FindApplicationIndex(aApplicationInfo.Name, index);
			}
			return false;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveItemAt(int index)
		{
			base.Items.RemoveAt(index);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			base.Items.Clear();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Fill(bool skipTB = true)
		{
			Clear();

			if (pathFinder == null || pathFinder.ApplicationInfos == null)
				return;

			foreach(IApplicationInfo appInfo in pathFinder.ApplicationInfos)
			{
				if (skipTB && appInfo.ApplicationType != ApplicationType.TaskBuilderApplication)
					continue;
				
				AddApplicationInfo(appInfo);
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public int FindApplicationIndex(string aApplicationName)
		{
			return FindApplicationIndex(aApplicationName, -1);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int FindApplicationIndex(string aApplicationName, int startIndex)
		{
			if (aApplicationName == null || aApplicationName.Length == 0)
				return -1;

			if (startIndex == -1)
				startIndex = 0;
			
			if (startIndex < 0 || startIndex >= ItemsCount)
			{
				Debug.Fail("Error in ApplicationsComboBox.FindApplicationIndex: wrong start index.");
				return -1;
			}

			for(int i = startIndex; i < ItemsCount; i++)
			{
				IApplicationInfo appInfo = GetApplicationInfoAt(i);
				if (appInfo != null && String.Compare(aApplicationName, appInfo.Name, true, CultureInfo.InvariantCulture) == 0)
					return i;
			}

			return -1;
		}
	}

	#endregion

	#region ApplicationsComboBoxItem class

	//============================================================================
	public class ApplicationsComboBoxItem : Component, ICloneable
	{
		private IApplicationInfo applicationInfo = null;

		public int itemIndex = -1;
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public ApplicationsComboBoxItem(IApplicationInfo aApplicationInfo)
		{
			applicationInfo = aApplicationInfo;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public IApplicationInfo ApplicationInfo { get { return applicationInfo;} }

		//--------------------------------------------------------------------------------------------------------------------------------
		private string GetApplicationBrandedTitle()
		{
			if (applicationInfo == null || applicationInfo.Name.IsNullOrEmpty())
				return String.Empty;

            string appBrandMenuTitle = InstallationData.BrandLoader.GetApplicationBrandMenuTitle(applicationInfo.Name);  
            return appBrandMenuTitle.IsNullOrEmpty() ? applicationInfo.Name: appBrandMenuTitle;
		}

		/// <summary>
		/// Item index. Used by collection editor
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public int Index
		{
			get { return itemIndex; }
		}

		/// <summary>
		/// The item's text
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public string Text
		{
			get { return GetApplicationBrandedTitle(); }
		}
        
		//--------------------------------------------------------------------------------------------------------------------------------
		public object Clone() 
		{
			return new ApplicationsComboBoxItem(applicationInfo);
		}

		/// <summary>
		/// Converts the item to string representation. Needed for property editor
		/// </summary>
		/// <returns>String representation of the item</returns>
		//--------------------------------------------------------------------------------------------------------------------------------
		public override string ToString()
		{
			return Text;
		}
	}

	#endregion

	#region ApplicationsComboBoxItemsCollection class

	/// <summary>
	/// The combobox's items collection class
	/// </summary>
	//============================================================================
	public class ApplicationsComboBoxItemsCollection : IList, ICollection, IEnumerable
	{
		ApplicationsComboBox owner = null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public ApplicationsComboBoxItemsCollection(ApplicationsComboBox aOwner)
		{
			owner = aOwner;
		}

        
		#region IList implemented members...

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set { this[index] = (ApplicationsComboBoxItem)value; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((ApplicationsComboBoxItem)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			Insert(index, (ApplicationsComboBoxItem)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		#region ICollection implemented members...

		//--------------------------------------------------------------------------------------------------------------------------------
		void ICollection.CopyTo(Array array, int index) 
		{
			for (IEnumerator e = GetEnumerator(); e.MoveNext();)
				array.SetValue(e.Current, index++);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool ICollection.IsSynchronized 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		object ICollection.SyncRoot 
		{
			get { return this; }
		}

		#endregion

		//--------------------------------------------------------------------------------------------------------------------------------
		public int Count 
		{
			get { return owner.ItemsCount; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsReadOnly 
		{
			get { return false; }
		}
			
		//--------------------------------------------------------------------------------------------------------------------------------
		public ApplicationsComboBoxItem this[int index]
		{
			get { return owner.GetElementAt(index); }
			set { owner.SetElementAt(index, value); }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public IEnumerator GetEnumerator() 
		{
			return owner.Enumerator;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool Contains(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int IndexOf(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Remove(ApplicationsComboBoxItem item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Insert(int index, ApplicationsComboBoxItem item)
		{
			owner.InsertItemAt(index, item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int Add(ApplicationsComboBoxItem item)
		{
			return owner.InsertItemAt(Count, item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AddRange(ApplicationsComboBoxItem[] items)
		{
			for(IEnumerator e = items.GetEnumerator(); e.MoveNext();)
				owner.InsertItemAt(Count, (ApplicationsComboBoxItem)e.Current);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			owner.Clear();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			owner.RemoveItemAt(index);
		}
	}
	#endregion

}
