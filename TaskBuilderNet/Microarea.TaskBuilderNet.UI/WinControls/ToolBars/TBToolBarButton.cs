using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	public delegate void TBToolBarButtonEventHandler(TBToolBarButton aButton);

	//===========================================================================
	/// <summary>
	/// Summary description for TBToolBarButton.
	/// </summary>
	[DefaultProperty("Text"), DesignerAttribute(typeof(TBToolBarButtonDesigner)), DesignTimeVisible(false)]
	public class TBToolBarButton : System.ComponentModel.Component
	{
		#region Events
		
		public event TBToolBarButtonEventHandler InvalidateButton;
		public event TBToolBarButtonEventHandler RefreshToolBar;
        public event TBToolBarButtonEventHandler DropDownComboSelectedIndexChanged;
        public event TBToolBarButtonEventHandler DropDownComboDropDown;
		
		#endregion
		
		public const int DropDownXOffset = 4;

		#region TBToolBarButton private data members
		
		private bool enabled = true;
		private bool pushed = false;
		private ToolBarButtonStyle style = ToolBarButtonStyle.PushButton;
		private string text = String.Empty;
        private string fixedText = String.Empty;
        private Font fixedTextFont = null;
        private Color fixedTextColor = Color.Empty;
        private bool visible = true;
		private Icon icon = null;
		private int dropDownWidth = 0;        
		private ComboBox dropDownCombo = null;
        private ContextMenuStrip dropDownMenu = null; 
		private TBToolBarDropDownButton dropDownMenuButton = null; 

		#endregion

//		[StructLayout(LayoutKind.Sequential)]
//			internal struct RECT
//		{
//			public int left;
//			public int top;
//			public int right;
//			public int bottom;
//		}
//		[DllImport("user32.dll", CharSet=CharSet.Auto)]
//		static internal extern bool GetMenuItemRect(IntPtr hParentWnd, IntPtr hMenu, int Item, ref RECT rc);

		#region TBToolBarButton constructor

		//---------------------------------------------------------------------------
		public TBToolBarButton()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		#endregion

		#region TBToolBarButton public properties


        //---------------------------------------------------------------------------
		[System.ComponentModel.Category("Behavior")]
		public bool Enabled 
		{
			get { return enabled; }

			set 
			{
				if (enabled == value)
					return; 
				enabled = value;
				
				RedrawToolBarButton();
			}
		}
		
		//---------------------------------------------------------------------------
		[System.ComponentModel.Category("Behavior")]
		public bool Pushed 
		{
			get 
			{
				return (style == ToolBarButtonStyle.ToggleButton) ? pushed : false; 
			}

			set 
			{
				if (pushed == value)
					return; 
				pushed = value;
				
				if (style == ToolBarButtonStyle.ToggleButton)
					RedrawToolBarButton();
			}
		}

		//---------------------------------------------------------------------------
		[System.ComponentModel.Category("Behavior")]
		public ToolBarButtonStyle Style 
		{
			get { return style; }

			set 
			{
				if (style == value)
					return; 
				style = value;

				if (style != ToolBarButtonStyle.ToggleButton)
					pushed = false;

				if (style == ToolBarButtonStyle.DropDownButton)
				{
					if (dropDownWidth == 0)
						dropDownWidth = SystemInformation.VerticalScrollBarWidth*3;
					
					if (dropDownCombo == null)
					{
						dropDownCombo = new ComboBox();
						dropDownCombo.Anchor = AnchorStyles.Left | AnchorStyles.Top;
						dropDownCombo.Dock = DockStyle.None;
						dropDownCombo.Enabled = true;
						dropDownCombo.DropDownStyle = ComboBoxStyle.DropDownList;
						dropDownCombo.Size = new Size(dropDownWidth, dropDownCombo.Height);
						dropDownCombo.Visible = true;

                        dropDownCombo.SelectedIndexChanged += new EventHandler(DropDownCombo_SelectedIndexChanged);
                        dropDownCombo.DropDown += new EventHandler(DropDownCombo_DropDown);
					}
				}
				else if (dropDownCombo != null)
				{
					dropDownCombo.Dispose();
					dropDownCombo = null;
				}
                RedrawToolBarButton();

				UpdateToolBarLayout();
			}
		}
		
		//---------------------------------------------------------------------------
        [Bindable(true)]
        [Localizable(true)]
		public string Text 
		{
			get { return text; }

			set 
			{
				if (String.Compare(text, value) == 0)
					return; 
				text = value;
				
				RedrawToolBarButton();
			}
		}

        //---------------------------------------------------------------------------
        [Bindable(true)]
        [Localizable(true)]
        [DefaultValue("")]
        [Category("Appearance")]
        public string FixedText
        {
            get { return fixedText; }

            set
            {
                if (String.Compare(fixedText, value) == 0)
                    return;

                fixedText = value;

                UpdateToolBarLayout();
                
                RedrawToolBarButton();
            }
        }

        //---------------------------------------------------------------------------
        public Font FixedTextFont
        {
            get { return fixedTextFont; }

            set
            {
                if (Font.Equals(fixedTextFont, value))
                    return;

                fixedTextFont = value;

                UpdateToolBarLayout();
                
                RedrawToolBarButton();
            }
        }

        //---------------------------------------------------------------------------
        public Color FixedTextColor
        {
            get { return fixedTextColor; }

            set
            {
                if (Color.Equals(fixedTextColor, value))
                    return;

                fixedTextColor = value;

                RedrawToolBarButton();
            }
        }
        
        //---------------------------------------------------------------------------
		[System.ComponentModel.Category("Behavior")]
		public bool Visible 
		{
			get { return visible; }

			set 
			{
				if (visible == value)
					return; 
				visible = value;
				
				RedrawToolBarButton();
			}
		}

		//---------------------------------------------------------------------------
		[System.ComponentModel.Category("Appearance")]
		public Icon Icon
		{
			get { return icon; }
			set 
			{
				icon = value; 
			
				RedrawToolBarButton();
			}
		}

		//---------------------------------------------------------------------------
		public int DropDownWidth 
		{
			get 
			{
				if (style != ToolBarButtonStyle.DropDownButton)
					return 0;
					
				return dropDownWidth; 
			}

			set 
			{
				if (dropDownWidth == value)
					return;

				dropDownWidth = value;

				if (style == ToolBarButtonStyle.DropDownButton)
				{
					if (dropDownCombo != null)
						dropDownCombo.Width = dropDownWidth;

					UpdateToolBarLayout();
				}
			}
		}
	
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public ComboBox DropDownCombo
		{
			get 
			{
				if (style != ToolBarButtonStyle.DropDownButton)
					return null;
					
				return dropDownCombo; 
			}
		}

		//---------------------------------------------------------------------------
        public ContextMenuStrip DropDownMenu
		{
			get 
			{
				return dropDownMenu; 
			}
			set 
			{
				if (dropDownMenu == value)
					return; 

				dropDownMenu = value;

                dropDownMenu.Closed += new ToolStripDropDownClosedEventHandler(DropDownMenu_Closed);

				if (dropDownMenu != null)
				{
					if (dropDownMenuButton == null)
					{
						dropDownMenuButton = new TBToolBarDropDownButton();
						dropDownMenuButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
						dropDownMenuButton.Visible = true;
                        dropDownMenuButton.PopUp += new EventHandler(DropDownMenuButton_PopUp);
					}
				}
				else if (dropDownMenuButton != null)
				{
					dropDownMenuButton.Dispose();
					dropDownMenuButton = null;
				}

				UpdateToolBarLayout();
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		internal TBToolBarDropDownButton DropDownMenuButton
		{
			get 
			{
				if (dropDownMenu == null)
					return null;
					
				return dropDownMenuButton; 
			}
		}

		#endregion

		#region TBToolBarButton private members

		//---------------------------------------------------------------------------
		public void RedrawToolBarButton()
		{
			if (InvalidateButton != null)
				InvalidateButton(this);
		}

		//---------------------------------------------------------------------------
		public void UpdateToolBarLayout()
		{
			if (RefreshToolBar != null)
				RefreshToolBar(this);
		}

		#endregion

		//---------------------------------------------------------------------------
		public int GetAdditionalWidth(bool addDropDownWith)
		{
			if (style == ToolBarButtonStyle.Separator)
				return 0;

			int additionalWidth = 0;
	
			if (dropDownMenu != null)
				additionalWidth += dropDownMenuButton.Width;

			if (addDropDownWith && style == ToolBarButtonStyle.DropDownButton)
				additionalWidth += dropDownWidth + DropDownXOffset;

			return additionalWidth;
		}

		//---------------------------------------------------------------------------
		public int GetAdditionalWidth()
		{
			return GetAdditionalWidth(true);
		}

        private bool isDropDownMenuOpen = false;
        //---------------------------------------------------------------------------
        private void DropDownMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (!dropDownMenuButton.RectangleToScreen(dropDownMenuButton.ClientRectangle).Contains(Control.MousePosition))
                isDropDownMenuOpen = false;
        }
		
		//---------------------------------------------------------------------------
		private void DropDownMenuButton_PopUp(object sender, System.EventArgs e)
		{
			if (dropDownMenu == null || sender == null || !(sender is TBToolBarDropDownButton))
				return;

            if (!isDropDownMenuOpen)
            {
                // Devo forzare un PerformLayout del menù a tendina affinché venga allineato correttamente la prima volta
                dropDownMenu.PerformLayout();

                dropDownMenu.Show((TBToolBarDropDownButton)sender, new Point(((TBToolBarDropDownButton)sender).Width - dropDownMenu.Width, ((TBToolBarDropDownButton)sender).Height + 1));
                
                isDropDownMenuOpen = true;
            }
            else 
            {
                if (dropDownMenu.Visible)
                    dropDownMenu.Close();
                
                isDropDownMenuOpen = false;
            }

			((TBToolBarDropDownButton)sender).Pressed = false;
		}

        //---------------------------------------------------------------------------
        private void DropDownCombo_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (style != ToolBarButtonStyle.DropDownButton || dropDownCombo == null || sender != dropDownCombo)
                return;

            if (DropDownComboSelectedIndexChanged != null)
                DropDownComboSelectedIndexChanged(this);
        }

        //---------------------------------------------------------------------------
        private void DropDownCombo_DropDown(object sender, System.EventArgs e)
        {
            if (style != ToolBarButtonStyle.DropDownButton || dropDownCombo == null || sender != dropDownCombo)
                return;

            if (DropDownComboDropDown != null)
                DropDownComboDropDown(this);

        }
    }

	#region TBToolBarButtonCollection Class

	//===========================================================================
	/// <summary>
	/// Summary description for TBToolBarButtonCollection.
	/// </summary>
	public class TBToolBarButtonCollection : ReadOnlyCollectionBase, IList
	{
		#region Events
		
		public event TBToolBarButtonEventHandler ButtonAdded;
		
		#endregion

		//---------------------------------------------------------------------------
		public TBToolBarButtonCollection()
		{
		}

		#region IList implemented members

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (value != null && !(value is TBToolBarButton))
					throw new NotSupportedException();

				this[index] = (TBToolBarButton)value; 
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is TBToolBarButton))
				throw new NotSupportedException();

			return this.Contains((TBToolBarButton)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((TBToolBarButton)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsReadOnly
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is TBToolBarButton))
				throw new NotSupportedException();

			return this.IndexOf((TBToolBarButton)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			if (item == null)
				throw new ArgumentNullException();
				
			if (!(item is TBToolBarButton))
				throw new NotSupportedException();

			Insert(index, (TBToolBarButton)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;

			if (!(item is TBToolBarButton))
				throw new NotSupportedException();

			Remove((TBToolBarButton)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		//---------------------------------------------------------------------------
		public TBToolBarButton this[int index]
		{
			get {  return (TBToolBarButton)InnerList[index];  }
			set 
			{ 
				InnerList[index] = (TBToolBarButton)value; 
			}
		}

		//---------------------------------------------------------------------------
		public TBToolBarButton[] ToArray()
		{
			return (TBToolBarButton[])InnerList.ToArray(typeof(TBToolBarButton));
		}

		//---------------------------------------------------------------------------
		public int Add(TBToolBarButton aButtonToAdd)
		{
			if (Contains(aButtonToAdd))
				return IndexOf(aButtonToAdd);

			int buttonIndex = InnerList.Add(aButtonToAdd);

			if (buttonIndex >= 0 && ButtonAdded != null)
				ButtonAdded(aButtonToAdd);

			return buttonIndex;
		}

		//---------------------------------------------------------------------------
		public void AddRange(TBToolBarButtonCollection aButtonsCollectionToAdd)
		{
			if (aButtonsCollectionToAdd == null || aButtonsCollectionToAdd.Count == 0)
				return;

			foreach (TBToolBarButton aButtonToAdd in aButtonsCollectionToAdd)
				Add(aButtonToAdd);
		}

		//---------------------------------------------------------------------------
		public void AddRange(TBToolBarButton[] aButtonsArrayToAdd)
		{
			if (aButtonsArrayToAdd == null || aButtonsArrayToAdd.Length == 0)
				return;

			foreach (TBToolBarButton aButtonToAdd in aButtonsArrayToAdd)
				Add(aButtonToAdd);
		}

		//---------------------------------------------------------------------------
		public void Insert(int index, TBToolBarButton aButtonToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aButtonToInsert))
				return;

			InnerList.Insert(index, aButtonToInsert);
	
			if (ButtonAdded != null)
				ButtonAdded(aButtonToInsert);
		}

		//---------------------------------------------------------------------------
		public void Insert(TBToolBarButton beforeButton, TBToolBarButton aButtonToInsert)
		{
			if (beforeButton == null)
				Add(aButtonToInsert);

			if (!Contains(beforeButton))
				return;

			if (Contains(aButtonToInsert))
				return;

			Insert(IndexOf(beforeButton), aButtonToInsert);
		}

		//---------------------------------------------------------------------------
		public void Remove(TBToolBarButton aButtonToRemove)
		{
			if (!Contains(aButtonToRemove))
				return;

			InnerList.Remove(aButtonToRemove);
		}

		//---------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			InnerList.RemoveAt(index);
		}

		//---------------------------------------------------------------------------
		public void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(TBToolBarButton aButtonToSearch)
		{
			foreach (object aItem in InnerList)
			{
				if (aItem == aButtonToSearch)
					return true;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public int IndexOf(TBToolBarButton aButtonToSearch)
		{
			if (!Contains(aButtonToSearch))
				return -1;
			
			return InnerList.IndexOf(aButtonToSearch);
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is TBToolBarButtonCollection))
				return false;

			if (obj == this)
				return true;

			if (((TBToolBarButtonCollection)obj).Count != this.Count)
				return false;

			if (this.Count == 0)
				return true;

			for (int i = 0; i < this.Count; i++)
			{
				if (!TBToolBarButton.Equals(this[i], ((TBToolBarButtonCollection)obj)[i]))
					return false;
			}

			return true;
		}

		// if I override Equals, I must override GetHashCode as well... 
		//---------------------------------------------------------------------------
		public override int GetHashCode() 
		{
			return base.GetHashCode();
		}

		//---------------------------------------------------------------------------
		internal bool HasDropDownMenus()
		{
			foreach (TBToolBarButton aButton in this)
			{
				if (aButton.DropDownMenuButton != null)
					return true;
			}
			return false;
		}
	}

	#endregion

	//====================================================================================
	internal class TBToolBarButtonDesigner : System.ComponentModel.Design.ComponentDesigner
	{
        //---------------------------------------------------------------------------
		public bool Enabled 
		{
			get 
			{
				return (bool)ShadowProperties["Enabled"];
			}
			set 
			{
				this.ShadowProperties["Enabled"] = value;
			}
		}

		//---------------------------------------------------------------------------
		public bool Pushed 
		{
			get 
			{
				return (bool)ShadowProperties["Pushed"];
			}
			set 
			{
				this.ShadowProperties["Pushed"] = value;
			}
		}

		//---------------------------------------------------------------------------
		public ToolBarButtonStyle Style 
		{
			get 
			{
				return (ToolBarButtonStyle)ShadowProperties["Style"];
			}
			set 
			{
				this.ShadowProperties["Style"] = value;
			}
		}

		//---------------------------------------------------------------------------
		public bool Text 
		{
			get 
			{
				return (bool)ShadowProperties["Text"];
			}
			set 
			{
				this.ShadowProperties["Text"] = value;
			}
		}
		//---------------------------------------------------------------------------
		public bool Visible 
		{
			get 
			{
				return (bool)ShadowProperties["Visible"];
			}
			set 
			{
				this.ShadowProperties["Visible"] = value;
			}
		}
		
		//---------------------------------------------------------------------------
		public int SmallImageIndex 
		{ 
			get 
			{
				return (int)ShadowProperties["SmallImageIndex"];
			}
			set 
			{
				this.ShadowProperties["SmallImageIndex"] = value;
			}
		}
		
		//---------------------------------------------------------------------------
		public int LargeImageIndex
		{
			get 
			{
				return (int)ShadowProperties["LargeImageIndex"];
			}
			set 
			{
				this.ShadowProperties["LargeImageIndex"] = value;
			}
		}

		//---------------------------------------------------------------------------
		public int DropDownWidth
		{
			get 
			{
				return (int)ShadowProperties["DropDownWidth"];
			}
			set 
			{
				this.ShadowProperties["DropDownWidth"] = value;
			}
		}
		
		//---------------------------------------------------------------------------
		public ContextMenu DropDownMenu
		{
			get 
			{
				return (ContextMenu)ShadowProperties["DropDownMenu"];
			}
			set 
			{
				this.ShadowProperties["DropDownMenu"] = value;
			}
		}
	}
}
