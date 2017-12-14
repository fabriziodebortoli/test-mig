using System;
using System.Drawing;
using Microarea.TaskBuilderNet.Forms.Properties;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;
using Telerik.WinControls;
using Telerik.WinControls.Layouts;
using Telerik.WinControls.Primitives;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	public class UIGridToolbarContainer : GridVisualElement, IGridView
	{
		RadGridViewElement      gridElement;
		GridViewInfo            viewInfo;
        UIGrid                  ownerGrid;
   	
        RadCommandBar			bar;				//toolbar
		CommandBarRowElement	commandBarRow;
		CommandBarStripElement	commandBarStrip;

        CommandBarTextBox       searchTextBox;
        //lista dei bottoni della toolbar della grid
        RadCommandBarBaseItemCollection childrenElements = new RadCommandBarBaseItemCollection();

        public RadCommandBarBaseItemCollection ChildrenElements
        {
            get { return childrenElements; } 
        }

        //---------------------------------------------------------------------
        public CommandBarTextBox SearchTextBox { get { return searchTextBox; } }

        //---------------------------------------------------------------------
        public UIGridToolbarContainer(UIGrid grid)
        {
            ownerGrid = grid;
        }

		//---------------------------------------------------------------------
		protected override void InitializeFields()
		{
			base.InitializeFields();

			this.Padding = new System.Windows.Forms.Padding(10);
			this.StretchHorizontally = true;
			this.MinSize = new Size(0, 30);
			this.MaxSize = new Size(0, 30);
			this.DrawFill = true;
			this.Class = "RowFill";
		}



		//---------------------------------------------------------------------
		protected override void CreateChildElements()
		{
			base.CreateChildElements();

			bar = new RadCommandBar();
            commandBarRow = new CommandBarRowElement();
			bar.Rows.Add(commandBarRow);
			
			commandBarStrip  =  new CommandBarStripElement();
            commandBarStrip.OverflowButton.Visibility = ElementVisibility.Hidden;
            commandBarRow.Strips.Add(commandBarStrip);
			this.Children.Add(bar.CommandBarElement);
		}
		//---------------------------------------------------------------------
		public void AddToolbarToggleButton(string name, string tooltip, EventHandler action, Image image = null, bool formModeChangeSensitive = false)
		{
            UICommandBarToggleButton button = new UICommandBarToggleButton();
            button.FormModeChangedSensitive = formModeChangeSensitive;
			button.ToolTipText = tooltip;
            button.Name = name;
			button.Click += action;
            
            if (image != null)
                button.Image = image;
            
            AddToCommandStrip(button);
		}

        public void AddToCommandStrip(RadCommandBarBaseItem button)
        {
            if (commandBarStrip.Items.Count > 0 && commandBarStrip.Items[commandBarStrip.Items.Count - 1] is CommandBarTextBox)
                commandBarStrip.Items.Insert(commandBarStrip.Items.Count - 1, button);
            else
                commandBarStrip.Items.Add(button);
            childrenElements.Add(button);
        }

		//---------------------------------------------------------------------
		public void AddToolbarButton(string name, string tooltip, EventHandler action, Image image = null, bool formModeChangeSensitive = false)
		{
            UICommandBarButton button = new UICommandBarButton(ownerGrid);
            button.FormModeChangedSensitive = formModeChangeSensitive;
			button.ToolTipText = tooltip;
            button.Name = name;
			button.Click += action;

			if (image != null)
				button.Image = image;

            AddToCommandStrip(button);
		}

		//---------------------------------------------------------------------
		public void AddFinderSection()
		{
            searchTextBox = new CommandBarTextBox();
            searchTextBox.MinSize = new Size(200, 0);
            
            AddIconToTextBoxElement( searchTextBox.TextBoxElement, Resources.ToolbarGrid_Search, Dock.Left);
            searchTextBox.NullText = Resources.Search;
            
        	commandBarStrip.Items.Add(searchTextBox);
        }

        //---------------------------------------------------------------------
        static void AddIconToTextBoxElement(RadTextBoxElement textElement, Image iconImage, Dock dock)
        {
            ImagePrimitive icon = new ImagePrimitive();
            icon.Image = iconImage;

            RadTextBoxItem item = textElement.TextBoxItem;

            textElement.Children.Remove(item);

            icon.SetValue(DockLayoutPanel.DockProperty, dock);

            DockLayoutPanel dockPanel = new DockLayoutPanel();
            dockPanel.LastChildFill = true;

            dockPanel.Children.Add(icon);
            dockPanel.Children.Add(item);

            textElement.Children.Add(dockPanel);
        }
        
		//---------------------------------------------------------------------
		protected override Type ThemeEffectiveType
		{
			get
			{
				return typeof(GridTableHeaderRowElement);
			}
		}

		//---------------------------------------------------------------------
		public void Initialize(RadGridViewElement gridElement, GridViewInfo viewInfo)
		{
			this.gridElement = gridElement;
			this.viewInfo = viewInfo;

			this.BackColor = Color.Magenta;
		}

		//---------------------------------------------------------------------
		public void Detach()
		{
			this.gridElement = null;
			this.viewInfo = null;
		}

		//---------------------------------------------------------------------
		public void UpdateView()
		{
		}

		//---------------------------------------------------------------------
		public RadGridViewElement GridViewElement
		{
			get { return this.gridElement; }
		}

		//---------------------------------------------------------------------
		public GridViewInfo ViewInfo
		{
			get { return this.viewInfo; }
		}

        
        internal void DocumentFormModeChanged(IMAbstractFormDoc document)
        {
            foreach (IUIGridToolbarItem button in ChildrenElements)
            {
                if (button.FormModeChangedSensitive && document != null)
                    button.Enabled = document.FormMode == FormModeType.New || document.FormMode == FormModeType.Edit;
            }
        }

        //---------------------------------------------------------------------
        internal void ShowToolbarButton(string name, bool visible)
        {
            foreach (IUIComponent btn in ChildrenElements)
            {
                if (btn.Name == name)
                {
                    btn.Visible = visible;
                }
            }
        }

        //---------------------------------------------------------------------
        internal void EnableToolbarButton(string name, bool enable)
        {
            foreach (IUIComponent btn in ChildrenElements)
            {
                if (btn.Name == name)
                {
                    btn.Enabled = enable;
                }
            }
        }

        //---------------------------------------------------------------------
        internal void Clear()
        {
            commandBarStrip.Items.Clear();
            childrenElements.Clear();
        }
    }


    //=========================================================================
    public class UICommandBarButton : CommandBarButton, IUIComponent, IUIToolbarItem, IUIGridToolbarItem
    {
        TBCUI cui;
        bool formModeChangedSensitive;
        UIGrid ownerGrid;

        //---------------------------------------------------------------------
        public UIGrid OwnerGrid
        {
            get { return ownerGrid; }
        }

        //---------------------------------------------------------------------
        public UICommandBarButton(UIGrid grid)
        {
            cui = new TBCUI(this, Interfaces.NameSpaceObjectType.ToolbarButton);
            ownerGrid = grid;
        }

        //---------------------------------------------------------------------
        public bool FormModeChangedSensitive
        {
            get { return formModeChangedSensitive; }
            set { formModeChangedSensitive = value; }
        }

        //---------------------------------------------------------------------
        public ITBCUI CUI
        {
            get { return cui; }
        }

        public event EventHandler ParentChanged;

        //---------------------------------------------------------------------
        public bool Visible
        {
            get
            {
                return base.Visibility == ElementVisibility.Visible;
            }
            set
            {
                base.Visibility = value ? ElementVisibility.Visible : ElementVisibility.Hidden;
            }
        }


        //-------------------------------------------------------------------------
        public void OnParentChanged(object owner)
        {
            if (ParentChanged != null)
                ParentChanged(owner, EventArgs.Empty);
        }
    }

    //=========================================================================
    internal class UICommandBarToggleButton : CommandBarToggleButton, IUIComponent, IUIToolbarItem, IUIGridToolbarItem
    {
        TBCUI cui;
        bool formModeChangedSensitive;
        //---------------------------------------------------------------------
        public UICommandBarToggleButton()
        {
            cui = new TBCUI(this, Interfaces.NameSpaceObjectType.ToolbarButton);
        }

        //---------------------------------------------------------------------
        public bool FormModeChangedSensitive
        {
            get { return formModeChangedSensitive; }
            set { formModeChangedSensitive = value; }
        }

        //---------------------------------------------------------------------
        public ITBCUI CUI
        {
            get { return cui; }
        }

        public event EventHandler ParentChanged;

        //---------------------------------------------------------------------
        public bool Visible
        {
            get
            {
                return base.Visibility == ElementVisibility.Visible;
            }
            set
            {
                base.Visibility = value ? ElementVisibility.Visible : ElementVisibility.Hidden;
            }
        }


        //-------------------------------------------------------------------------
        public void OnParentChanged(object owner)
        {
            if (ParentChanged != null)
                ParentChanged(owner, EventArgs.Empty);
        }
    }
}
