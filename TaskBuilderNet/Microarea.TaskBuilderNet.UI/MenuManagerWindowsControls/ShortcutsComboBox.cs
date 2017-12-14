using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	public partial class MenuShortcutsCompositeComboBox : System.Windows.Forms.UserControl
	{
		private MenuMngWinCtrl menuMngWinCtrl = null;

		private MenuShortcutsComboBox ShortcutsComboBox;
		private MenuShortcutsRunButton RunShortcutButton;
		private System.Windows.Forms.ToolTip ShortcutToolTip;
		private System.Windows.Forms.Panel ComboPanel;
		private System.Windows.Forms.ContextMenu ShortcutContextMenu;
		
		public event MenuMngCtrlEventHandler RunShortcut;
		public event MenuMngCtrlEventHandler DisplayShortcutContextMenu;
		public event MenuMngCtrlEventHandler DisplayStartupShortcutContextMenu;
		
		//---------------------------------------------------------------------------
		public MenuShortcutsCompositeComboBox(MenuMngWinCtrl aMenuMngWinCtrl)
		{
			menuMngWinCtrl = aMenuMngWinCtrl;

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			ShortcutsComboBox.MenuMngWinCtrl = menuMngWinCtrl;
			ShortcutsComboBox.ItemHeight = RunShortcutButton.Height - 4;
		}
		
		//---------------------------------------------------------------------------
		public MenuShortcutsCompositeComboBox() : this(null)
		{
		}
				
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetShortcutAt(int index, MenuXmlNode shortcut)
		{
			if (index < 0 || (shortcut != null && !shortcut.IsShortcut))
				return;

			if (ShortcutsComboBox != null)
				ShortcutsComboBox.SetShortcutAt(index, shortcut);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuXmlNode GetShortcutAt(int index)
		{
			return (ShortcutsComboBox != null) ? ShortcutsComboBox.GetShortcutAt(index) : null;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuXmlNode GetCurrentSelectedShortcut()
		{
			if (ShortcutsComboBox == null || ShortcutsComboBox.SelectedIndex == -1)
				return null;

			return GetShortcutAt(ShortcutsComboBox.SelectedIndex);
		}
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SelectShortcutAt(int index)
		{
			if (ShortcutsComboBox == null || index < 0 || index >= ShortcutsComboBox.ItemsCount)
				return;

			ShortcutsComboBox.SelectedIndex = index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertShortcutAt(int index, MenuXmlNode shortcut)
		{
			return (ShortcutsComboBox != null) ? ShortcutsComboBox.InsertShortcutAt(index, shortcut) : -1;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddShortcut(MenuXmlNode shortcutToAdd)
		{
			return (ShortcutsComboBox != null) ? ShortcutsComboBox.AddShortcut(shortcutToAdd) : -1;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool RemoveShortcut(MenuXmlNode shortcutToRemove)
		{
			return (ShortcutsComboBox != null) ? ShortcutsComboBox.RemoveShortcut(shortcutToRemove) : false;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveItemAt(int index)
		{
			if (ShortcutsComboBox != null)
				ShortcutsComboBox.RemoveItemAt(index);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			if (ShortcutsComboBox != null)
				ShortcutsComboBox.Clear();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetComboBoxTooltipText(string text)
		{
			if (ShortcutToolTip != null && ShortcutsComboBox != null)
				ShortcutToolTip.SetToolTip(ShortcutsComboBox, text);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SetCurrentShortcutTooltipText()
		{
			MenuXmlNode selShortcut = GetCurrentSelectedShortcut();

			SetComboBoxTooltipText((selShortcut != null) ? selShortcut.GetShortcutDescription() : String.Empty);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ShortcutsComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			SetCurrentShortcutTooltipText();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RunShortcutButton_Click(object sender, System.EventArgs e)
		{
			MenuXmlNode selShortcut = GetCurrentSelectedShortcut();

			if (selShortcut == null)
				return;

			if (RunShortcut != null)
				RunShortcut(this, new MenuMngCtrlEventArgs(selShortcut));
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void RunShortcutButton_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				RunShortcutButton_Click(sender, (System.EventArgs)e);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ShortcutContextMenu_Popup(object sender, System.EventArgs e)
		{
			if (((ContextMenu)sender).SourceControl == ShortcutsComboBox)
			{
				MenuXmlNode selShortcut = GetCurrentSelectedShortcut();

				if (selShortcut != null && DisplayShortcutContextMenu != null && !selShortcut.IsStartupShortcut)
					DisplayShortcutContextMenu(this, new MenuMngCtrlEventArgs(selShortcut));

				if (selShortcut != null && DisplayStartupShortcutContextMenu != null && selShortcut.IsStartupShortcut)
					DisplayStartupShortcutContextMenu(this, new MenuMngCtrlEventArgs(selShortcut));
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuShortcutsComboBox ComboBox
		{
			get { return ShortcutsComboBox; }
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public int ItemsCount
		{
			get { return ShortcutsComboBox.ItemsCount; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuShortcutsRunButton RunButton
		{
			get { return RunShortcutButton; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public new System.Windows.Forms.ContextMenu ContextMenu
		{
			get { return ShortcutContextMenu; }
		}
		/// <summary>
		/// The ImageList control from which this combobox takes the images
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public ImageList ImageList
		{
			get { return (ShortcutsComboBox != null) ? ShortcutsComboBox.ImageList : null; }
			set { if (ShortcutsComboBox != null) ShortcutsComboBox.ImageList = value; }
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public int PreferredHeight 
		{
			get { return (ShortcutsComboBox != null) ? (ShortcutsComboBox.PreferredHeight + 3) : 0; }
		}
	}	
	/// <summary>
	/// Combo box with images that supports design-time editing
	/// </summary>
	//============================================================================
	public partial class MenuShortcutsComboBox : System.Windows.Forms.ComboBox
	{
		private MenuMngWinCtrl menuMngWinCtrl = null;

		private ImageList imageList = null;
		MenuShortcutsComboBoxItemsCollection listItems = null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuShortcutsComboBox() : this(null)
		{
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuShortcutsComboBox(MenuMngWinCtrl aMenuMngWinCtrl)
		{
			// Set owner draw mode
			base.DrawMode = DrawMode.OwnerDrawFixed;
			listItems = new MenuShortcutsComboBoxItemsCollection(this);

			MenuMngWinCtrl = aMenuMngWinCtrl;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public MenuMngWinCtrl MenuMngWinCtrl 
		{ 
			get { return menuMngWinCtrl; } 
			set 
			{ 
				menuMngWinCtrl = value; 
				if (menuMngWinCtrl != null)
					imageList = menuMngWinCtrl.CommandsImageList;
			} 
		}
		
		/// <summary>
		/// Hides the parent DrawMode property from property browser
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public new DrawMode DrawMode
		{
			get { return base.DrawMode; }
			set { }
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
		/// The ImageList control from which this combobox takes the images
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public ImageList ImageList
		{
			get { return imageList; }
			set 
			{ 
				imageList = value; 

				Invalidate();
			}
		}

		/// <summary>
		/// The items in the list box
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		new public MenuShortcutsComboBoxItemsCollection Items
		{
			get { return listItems; }
		}

		/// <summary>
		/// Overrides parent OnDrawItem method to perform custom painting
		/// </summary>
		/// <param name="pe"></param>
		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
		{
			e.DrawBackground();
			e.DrawFocusRectangle();
			Rectangle bounds = e.Bounds;
			// Check whether the index is valid
			if(e.Index >= 0 && e.Index < base.Items.Count) 
			{
				MenuShortcutsComboBoxItem item = (MenuShortcutsComboBoxItem)base.Items[e.Index];

				// If the image list is present and the image index is set, draw the image
				if(imageList != null)
				{
					if (item.ImageIndex != -1 && item.ImageIndex < imageList.Images.Count) 
					{
						int imageYPosition = bounds.Top;
						if (imageList.ImageSize.Height < bounds.Height)
							imageYPosition += (bounds.Height - imageList.ImageSize.Height)/2;
						imageList.Draw(e.Graphics, bounds.Left, imageYPosition, item.ImageIndex); 
					}
					bounds.Location = new Point(bounds.Location.X + imageList.ImageSize.Width, bounds.Location.Y);
					bounds.Width -= imageList.ImageSize.Width;
				}
				StringFormat stringFormat = new StringFormat();
				stringFormat.LineAlignment = StringAlignment.Center;
				stringFormat.Trimming = StringTrimming.EllipsisWord;
				stringFormat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;

				// Draw item text
				System.Drawing.SolidBrush textBrush = new SolidBrush(e.ForeColor);
				e.Graphics.DrawString(item.Text, e.Font, textBrush, bounds, stringFormat);
				textBrush.Dispose();
			}
			base.OnDrawItem(e);
		}
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetShortcutAt(int index, MenuXmlNode shortcut)
		{
			if (shortcut != null && !shortcut.IsShortcut)
				return;

			SetElementAt(index, new MenuShortcutsComboBoxItem(menuMngWinCtrl, shortcut));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetElementAt(int index, MenuShortcutsComboBoxItem item)
		{
			if (index < 0 || index >= ItemsCount)
				return;

			base.Items[index] = item;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuXmlNode GetShortcutAt(int index)
		{
			MenuShortcutsComboBoxItem item = GetElementAt(index);
			if (item == null)
				return null;

			return item.Shortcut;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuShortcutsComboBoxItem GetElementAt(int index)
		{
			if (index < 0 || index >= ItemsCount)
			{
				Debug.Fail("Error in MenuShortcutsComboBox.GetElementAt");
				return null;
			}

			return (MenuShortcutsComboBoxItem)base.Items[index];
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertShortcutAt(int index, MenuXmlNode shortcut)
		{
			if (index < 0 || (shortcut != null && !shortcut.IsShortcut))
				return -1;

			return InsertItemAt(index, new MenuShortcutsComboBoxItem(menuMngWinCtrl, shortcut));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertItemAt(int index, MenuShortcutsComboBoxItem item)
		{
			item.itemIndex = index;
			base.Items.Insert(index, item);
			return index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddShortcut(MenuXmlNode shortcut)
		{
			return AddItem(new MenuShortcutsComboBoxItem(menuMngWinCtrl, shortcut));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddItem(MenuShortcutsComboBoxItem item)
		{
			return base.Items.Add(item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool RemoveShortcut(MenuXmlNode shortcutToRemove)
		{
			if (shortcutToRemove == null || !shortcutToRemove.IsShortcut)
				return false;

			int index = -1;
			index = FindStringExact(shortcutToRemove.GetShortcutName(), index);
			while (index >= 0)
			{
				MenuXmlNode shortcut = GetShortcutAt(index);

				if (shortcut != null && shortcut.IsSameShortcutAs(shortcutToRemove))
				{
					RemoveItemAt(index);
					return true;
				}
				index = FindStringExact(shortcutToRemove.GetShortcutName(), index);
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
	}
	
	//============================================================================
	public class MenuShortcutsComboBoxItem : Component, ICloneable
	{
		private MenuMngWinCtrl menuMngWinCtrl = null;
		private MenuXmlNode shortcut;
		private int imageIndex;

		public int itemIndex = -1;

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuShortcutsComboBoxItem(MenuMngWinCtrl aMenuMngWinCtrl, MenuXmlNode aShortcut, int aImageIndex)
		{
			menuMngWinCtrl = aMenuMngWinCtrl;
			Shortcut = aShortcut;
			if (aImageIndex != -1)
				imageIndex = aImageIndex;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuShortcutsComboBoxItem(MenuMngWinCtrl aMenuMngWinCtrl, MenuXmlNode aShortcut) : this(aMenuMngWinCtrl, aShortcut, -1)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuShortcutsComboBoxItem(MenuMngWinCtrl aMenuMngWinCtrl) : this(aMenuMngWinCtrl, null)
		{
		}

		/// <summary>
		/// Item index. Used by collection editor
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public int Index
		{
			get { return itemIndex; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuXmlNode Shortcut
		{
			get { return shortcut; }
			set 
			{
				if (value != null && value.IsShortcut)
				{
					shortcut = value;
					
					ImageIndex = (menuMngWinCtrl != null) ? menuMngWinCtrl.GetMenuTreeImageIdx(shortcut) : -1;
				}
				else
				{
					Debug.Assert(value == null, "Set MenuShortcutsComboBoxItem.Shortcut property error: invalid node type");
					shortcut = null;
				}
			}
		}

		/// <summary>
		/// The item's text
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public string Text
		{
			get { return (shortcut != null) ? shortcut.GetShortcutName() : String.Empty; }
		}
        
		/// <summary>
		/// The item's image index
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public int ImageIndex
		{
			get { return imageIndex; }
			set { imageIndex = value; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public object Clone() 
		{
			return new MenuShortcutsComboBoxItem(menuMngWinCtrl, shortcut, imageIndex);
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

	/// <summary>
	/// The combobox's items collection class
	/// </summary>
	//============================================================================
	public class MenuShortcutsComboBoxItemsCollection : IList, ICollection, IEnumerable
	{
		MenuShortcutsComboBox owner = null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuShortcutsComboBoxItemsCollection(MenuShortcutsComboBox aOwner)
		{
			owner = aOwner;
		}

        
		#region IList implemented members...

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set { this[index] = (MenuShortcutsComboBoxItem)value; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((MenuShortcutsComboBoxItem)item);
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
			Insert(index, (MenuShortcutsComboBoxItem)item);
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
		public MenuShortcutsComboBoxItem this[int index]
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
		public void Remove(MenuShortcutsComboBoxItem item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Insert(int index, MenuShortcutsComboBoxItem item)
		{
			owner.InsertItemAt(index, item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int Add(MenuShortcutsComboBoxItem item)
		{
			return owner.InsertItemAt(Count, item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AddRange(MenuShortcutsComboBoxItem[] items)
		{
			for(IEnumerator e = items.GetEnumerator(); e.MoveNext();)
				owner.InsertItemAt(Count, (MenuShortcutsComboBoxItem)e.Current);
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

	//============================================================================
	public class MenuShortcutsRunButton : System.Windows.Forms.Button
	{
		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuShortcutsRunButton()
		{
			Stream bitmapStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.RunShortcut.bmp");
			if (bitmapStream != null)
				Image = Image.FromStream(bitmapStream);
		}
	}
}
