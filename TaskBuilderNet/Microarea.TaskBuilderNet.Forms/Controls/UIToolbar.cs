using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Forms.Properties;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
 	//================================================================================================================
	public class UIToolbarManager : FlowLayoutPanel
	{
		UIPrimaryToolbar primaryToolbar;
		UIAuxiliaryToolbar auxiliaryToolbar;

		Dictionary<Keys, RadItem> shortcuts = new Dictionary<Keys, RadItem>();

		public IUIToolbar PrimaryToolbar { get { return primaryToolbar; } set { primaryToolbar = value as UIPrimaryToolbar; } }
		public IUIToolbar AuxiliaryToolbar { get { return auxiliaryToolbar; } set { auxiliaryToolbar = value as UIAuxiliaryToolbar; } }

		//-------------------------------------------------------------------------
		public PrimaryToolbarStyle PrimaryToolbarStyle
		{
			set
			{
				primaryToolbar.Initialize(value);
				InternalUpdateStatus();
			}
		}
		//-------------------------------------------------------------------------
		public AuxiliaryToolbarStyle AuxiliaryToolbarStyle
		{
			set
			{ 
				auxiliaryToolbar.Initialize(value);
				InternalUpdateStatus(); 
			}
		}

		//-------------------------------------------------------------------------
		public UIToolbarManager()
		{
			primaryToolbar = new UIPrimaryToolbar();
			primaryToolbar.ShortcutAdded += new EventHandler<ShortcutAddedEventArgs>(ShortcutAdded);
			primaryToolbar.UpdateStatus += new EventHandler(UpdateStatus);
			this.Controls.Add(primaryToolbar);
		
			auxiliaryToolbar = new UIAuxiliaryToolbar();
			auxiliaryToolbar.ShortcutAdded += new EventHandler<ShortcutAddedEventArgs>(ShortcutAdded);
			auxiliaryToolbar.UpdateStatus += new EventHandler(UpdateStatus);
			this.Controls.Add(auxiliaryToolbar);
		}

		//-------------------------------------------------------------------------
		public bool ProcessShortcut(Keys keys)
		{
			RadItem item = null;
			
			shortcuts.TryGetValue(keys, out item);
			if (item == null)
				return false;

			item.PerformClick();
			return true;
		}

		//-------------------------------------------------------------------------
		void UpdateStatus(object sender, EventArgs e)
		{
			InternalUpdateStatus();
		}

		//-------------------------------------------------------------------------
		void ShortcutAdded(object sender, ShortcutAddedEventArgs e)
		{
			if (!shortcuts.ContainsKey(e.Keys))
				shortcuts.Add(e.Keys, e.Owner);
		}

		//-------------------------------------------------------------------------
		public int ToolbarHeight { get { return primaryToolbar.ToolbarHeight + auxiliaryToolbar.ToolbarHeight; } }

		//-------------------------------------------------------------------------
		public int ToolbarWidth 
		{
			get { return (primaryToolbar as UIPrimaryToolbar).Width;} 
			set
			{ 
				primaryToolbar.Width = value;
				auxiliaryToolbar.Width = value;
			} 
		}

		//-------------------------------------------------------------------------
		protected virtual void InternalUpdateStatus()
		{
            primaryToolbar.ApplyEnableConditions();
            auxiliaryToolbar.ApplyEnableConditions();
		}
	}

	//================================================================================================================
	public class UIToolbar : RadCommandBar, IUIContainer, IUIToolbar
	{
        TBCUI cui;
        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }


        private Dictionary<RadItem, ToolBarItemEnableCondition> enableConditions = new Dictionary<RadItem, ToolBarItemEnableCondition>();

		public event EventHandler UpdateStatus;
		public event EventHandler<ShortcutAddedEventArgs> ShortcutAdded;
        public event EventHandler ToolbarCreated;
		
		protected CommandBarRowElement commandBarRow;
		protected CommandBarStripElement commandBarStrip;

		static bool useLargeButtons = InitButtons();

		//-------------------------------------------------------------------------
		public UIToolbar()
		{

			cui = new TBCUI(this, NameSpaceObjectType.Toolbar);
            ThemeClassName = typeof(RadCommandBar).ToString();

			//PrimaryToolbar
			commandBarRow = new CommandBarRowElement();
			Rows.Add(commandBarRow);

			commandBarStrip = new CommandBarStripElement();
            commandBarStrip.OverflowButton.Visibility = ElementVisibility.Hidden;
			commandBarStrip.ItemsChanged += new RadCommandBarBaseItemCollectionItemChangedDelegate(commandBarStrip_ItemsChanged);

			commandBarRow.Strips.Add(commandBarStrip);

			this.Margin = new Padding(0, 0, 0, 0);
 		}

		//-------------------------------------------------------------------------
		private static bool InitButtons()
		{
			SettingItem si = null;
			if (BasePathFinder.IsInitialized)
				si = BasePathFinder.BasePathFinderInstance.GetSettingItem("Framework", "TBGenLib", "Preference", "UseLargeToolBarButtons");
			return (si == null) ? true : si.Values[0].Equals("1");
		}

        //-------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (cui != null)
            {
                cui.Dispose();
                cui = null;
            }
        }

        //---------------------------------------------------------------------------
        protected void OnToolbarCreated()
        {
            if (ToolbarCreated != null)
                ToolbarCreated(this, EventArgs.Empty);
        }

		//-------------------------------------------------------------------------
		public void OnUpdateStatus()
		{
            if (CUI != null && CUI.Document != null && CUI.Document.IsAlive && UpdateStatus != null)
		        UpdateStatus(this, EventArgs.Empty);
		}

        //-------------------------------------------------------------------------
		public void ApplyEnableConditions()
		{
            foreach (KeyValuePair<RadItem, ToolBarItemEnableCondition> item in EnableConditions)
                item.Key.Enabled = item.Value.Invoke();
		}

		//-------------------------------------------------------------------------
        internal Dictionary<RadItem, ToolBarItemEnableCondition> EnableConditions
		{
			get { return enableConditions; }
		}

   		//-------------------------------------------------------------------------
		protected void OnShortcutAdded(Keys keys, RadItem owner)
		{
			if (ShortcutAdded != null)
				ShortcutAdded(this, new ShortcutAddedEventArgs(owner, keys));
		}

		//-------------------------------------------------------------------------
		public int ToolbarHeight { get { return Visible ? this.Height : 0; } }

		//-------------------------------------------------------------------------
		public void AddSeparator()
		{
			CommandBarSeparator separator = new CommandBarSeparator();

			separator.Margin = new System.Windows.Forms.Padding(3);
				
			if (!useLargeButtons)
			{
				separator.AutoSize = false;
				//button.ImageLayout = ImageLayout.Stretch;
				separator.Size = new Size(2, 20);
			}

			commandBarStrip.Items.Add(separator);
			this.Visible = true;
		}

		//-------------------------------------------------------------------------
		public void ChangeToolBarItemDetails(string buttonName, string newText, Image newImage)
		{
			RadCommandBarBaseItem button = commandBarStrip.Items[buttonName] as RadCommandBarBaseItem;
			if (button == null)
				return;

			button.Text = newText;
			button.Image = newImage;
		}

		//-------------------------------------------------------------------------
		public void AddButton
			(
			string name,
			string text,
			EventHandler action,
			ToolBarItemEnableCondition enableCondition,
			INameSpace imageNamespace,
			Keys keys = Keys.None
			)
		{
			AddButton(name, text, action, enableCondition, ImageLoader.GetImageFromNamespace(imageNamespace), keys);
		}

		//-------------------------------------------------------------------------
		public virtual void AddButton
			(
			string name,
			string text,
			EventHandler action,
			ToolBarItemEnableCondition enableCondition,
			Image image = null,
			Keys keys = Keys.None
			)
		{
			UIToolbarButton button = CreateCommandButton(name, text, action, enableCondition, image);
			commandBarStrip.Items.Add(button);
		
			if (keys != Keys.None)
				OnShortcutAdded(keys, button);

			this.Visible = true;
		}

		//-------------------------------------------------------------------------
		void commandBarStrip_ItemsChanged(RadCommandBarBaseItemCollection changed, RadCommandBarBaseItem target, ItemsChangeOperation operation)
		{
			IUIToolbarItem toolbarItem = target as IUIToolbarItem;
			if (toolbarItem != null)
				toolbarItem.OnParentChanged(this);
		}

		//-------------------------------------------------------------------------
		public void AddSplitButton(
			string name,
			string text,
			EventHandler defaultClick,
			ToolBarItemEnableCondition enableCondition,
			List<IMenuItemGeneric> items,
			INameSpace imageNamespace,
			Keys keys = Keys.None)
		{
			AddSplitButton(name, text, defaultClick, enableCondition, items, ImageLoader.GetImageFromNamespace(imageNamespace), keys);
		}

		//-------------------------------------------------------------------------
		public virtual void AddSplitButton(
			string name,
			string text,
			EventHandler defaultClick,
			ToolBarItemEnableCondition enableCondition,
			List<IMenuItemGeneric> items, 
			Image image = null,
			Keys keys = Keys.None)
		{
			UIToolbarSplitButton button = CreateCommandSplitButton(name, text, defaultClick, enableCondition, items, image);
			commandBarStrip.Items.Add(button);
		  
			this.Visible = true;
		}

		//-------------------------------------------------------------------------
		protected UIToolbarButton CreateCommandButton(
			string name,
			string text,
			EventHandler action,
			ToolBarItemEnableCondition enableCondition,
			Image image = null
			)
		{
			UIToolbarButton button = new UIToolbarButton();
			button.Name = name;
			button.Text =  button.ToolTipText = text;

			if (!useLargeButtons)
			{
				button.AutoSize = false;
				button.ImageLayout = ImageLayout.Stretch;
				button.Size = new Size(20, 20);
			}

			button.TextImageRelation = TextImageRelation.ImageBeforeText;
			button.ImageTransparentColor = Color.Magenta;
			button.Margin = new System.Windows.Forms.Padding(3);
		
			button.Click += action; //questa è l'azione dell'utente
			button.Click += new EventHandler(button_Click); //questa è internal per l'aggiornamento della toolbar

			//button.FitToSizeMode = RadFitToSizeMode.FitToParentPadding;
			button.Image = image;
			if (image == null)
			{
				button.DrawText = true;
			}

			if (enableCondition != null)
				EnableConditions.Add(button, enableCondition);

			return button;
		}

		//-------------------------------------------------------------------------
		private UIToolbarSplitButton CreateCommandSplitButton
			(
			string name,
			string text,
			EventHandler defaultClick,
			ToolBarItemEnableCondition enableCondition,
			List<IMenuItemGeneric> items = null,
			Image image = null
			)

		{
			UIToolbarSplitButton button = new UIToolbarSplitButton();
			if (image != null)
				button.Image = image;
			
			button.Name = name;
			button.Text = text;
			button.Click += new EventHandler(defaultClick);
			button.Margin = new System.Windows.Forms.Padding(3);
			EnableConditions.Add(button, enableCondition);

			if (!useLargeButtons)
			{
				button.AutoSize = false;
				button.ImageLayout = ImageLayout.Stretch;
				button.Size = new Size(32, 20);
				
				button.ArrowPart.AutoSize = false;
				button.ArrowPart.Size = new Size(10, 18);
			}

			if (items != null)
			{
				foreach (IMenuItemGeneric current in items)
				{
					IMenuItemClickable item = current as IMenuItemClickable;
					if (item != null)
					{
						UIMenuItem menuItem = new UIMenuItem();
						menuItem.Name = menuItem.Text = item.Text;
						menuItem.Click += new EventHandler(item.OnClick);

						if (item.EnableDelegate != null)
							menuItem.Enabled = item.EnableDelegate.Invoke();

						if (item.CheckedDelegate != null)
							menuItem.IsChecked = item.CheckedDelegate.Invoke();

						button.Items.Add(menuItem);

						EnableConditions.Add(menuItem, item.EnableDelegate);

						OnShortcutAdded(item.Keys, menuItem);
					}

					IMenuItemSeparator separator = current as IMenuItemSeparator;
					if (separator != null)
					{
						RadMenuSeparatorItem menuItem = new RadMenuSeparatorItem();
						menuItem.Name = separator.Name;
						button.Items.Add(menuItem);
					}
				}
			}
			return button;
		}

		//-------------------------------------------------------------------------
		void button_Click(object sender, EventArgs e)
		{
			OnUpdateStatus();
		}

        //-------------------------------------------------------------------------
        public System.Collections.IList ChildControls
        {
            get { 
                   List<IComponent> items = new List<IComponent>();
                   foreach (RadItem item in commandBarStrip.Items)
                        items.Add(item);
                    return items; 
                }
        }

        //-------------------------------------------------------------------------
        private IUIToolbarItem GetItem(IUIContainer container, string name)
        {
            foreach (IComponent component in container.ChildControls)
            {
                IUIComponent item = component as IUIComponent;

				IUIToolbarItem toolbarItem = item as IUIToolbarItem;
				if (toolbarItem != null && string.Compare(item.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
					return toolbarItem;

                IUIContainer cnt = item as IUIContainer;
                if (cnt != null)
                    return GetItem(cnt, name);
            }
            
            return null;
        }

        //-------------------------------------------------------------------------
        public IUIToolbarItem GetItem(string name)
        {
            return GetItem(this, name);
        }

        //-------------------------------------------------------------------------
        public void ChangeEnableCondition(IUIToolbarItem item, ToolBarItemEnableCondition enableCondition)
        {
            EnableConditions[item as RadItem] = enableCondition;
        }

    }

	//================================================================================================================
	/// <summary>
	///  primary toolbar
	/// </summary>
	[ToolboxItem(false)]
	internal partial class UIPrimaryToolbar : UIToolbar
	{
		PrimaryToolbarStyle toolbarStyle;

        MAbstractFormDoc Document { get { return CUI.Document as MAbstractFormDoc; } }
		/// <summary>
		///   Constructor
		/// </summary>
		//-------------------------------------------------------------------------
		public UIPrimaryToolbar()
		{
			ThemeClassName = GetType().BaseType.BaseType.ToString();
			this.Visible = false;
        }

        //-------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (CUI.Document != null)
                CUI.Document.FormModeChanged -= new EventHandler<EventArgs>(Document_FormModeChanged);

            base.Dispose(disposing);      
        }

        //-------------------------------------------------------------------------
        void Document_FormModeChanged(object sender, EventArgs e)
        {
            OnUpdateStatus();
        }

		//---------------------------------------------------------------------------
		internal void Initialize(PrimaryToolbarStyle toolbarStyle)
		{
			this.toolbarStyle = toolbarStyle;

            if (CUI.Document != null)
                CUI.Document.FormModeChanged += new EventHandler<EventArgs>(Document_FormModeChanged);

			switch (toolbarStyle)
			{
				case PrimaryToolbarStyle.Standard:
					CreateStandardToolbar();
                    OnToolbarCreated();
					break;
				case PrimaryToolbarStyle.Batch:
					CreateBatchToolbar();
                    OnToolbarCreated();
					break;
				case PrimaryToolbarStyle.RowView:
					CreateRowViewToolbar();
                    OnToolbarCreated();
					break;
				case PrimaryToolbarStyle.SlaveView:
                    OnToolbarCreated();
					break;
				case PrimaryToolbarStyle.None:
				default:
					return;
			}
		}

		//---------------------------------------------------------------------------
		public override void AddButton(
			string name,
			string text,
			EventHandler action,
			ToolBarItemEnableCondition enableCondition,
			Image image = null,
			Keys keys = Keys.None
			)
		{
			if (toolbarStyle == PrimaryToolbarStyle.None)
				return;

			base.AddButton(name, text, action, enableCondition, image, keys);
		}

		//---------------------------------------------------------------------------
		public override void AddSplitButton(
			string name,
			string text,
			EventHandler defaultClick,
			ToolBarItemEnableCondition enableCondition,
			List<IMenuItemGeneric> items,
			Image image = null,
			Keys keys = Keys.None
			)
		{
			if (toolbarStyle == PrimaryToolbarStyle.None)
				return;

			base.AddSplitButton(name, text, defaultClick, enableCondition, items, image, keys);
		}


		//---------------------------------------------------------------------------
		private void CreateStandardToolbar()
		{
			AddButton
				(
				"New",
				Resources.ToolbarStrings_New,
                (sender, args) => { Document.EnterInNewRecord(); },
                () => { return Document.CanDoNewRecord(); },
                Resources.Toolbar_New,
				Keys.F2
				);

			AddButton
				(
				"Edit",
				Resources.ToolbarStrings_Edit,
				(sender, args) => { Document.EditCurrentRecord(); },
				() => { return Document.CanDoEditRecord(); },
				Resources.Toolbar_Edit,
				Keys.Enter | Keys.Control
				);

			AddButton
				(
				"Save",
				Resources.ToolbarStrings_Save,
				(sender, args) => { Document.SaveCurrentRecord(); },
				() => { return Document.CanDoSaveRecord(); },
				Resources.Toolbar_Save,
				Keys.F10
				);

			AddSeparator();

			AddSplitButton
				(
				"Report",
				Resources.ToolbarStrings_Report,
				(sender, args) => { Document.FormReport(); },
				() => { return true; },
				GenerateReportDataBinding(),
				Resources.Toolbar_Report,
				Keys.F6
				);

			AddSplitButton
				(
				"Radar",
				Resources.ToolbarStrings_Radar,
				(sender, args) => { Document.Radar(); },
				() => { return true; },
				GenerateRadarDataBinding(),
				Resources.Toolbar_Radar,
				Keys.F7
				);

			AddButton
				(
				"Customize",
				Resources.ToolbarStrings_Customize,
				(sender, args) => { Document.Customize(); },
				() => { return Document.CanDoCustomize(); },
				Resources.Toolbar_Customize,
				Keys.F8 | Keys.Control
				);

			AddSeparator();

			AddButton
				(
				"SearchSpecific",
				Resources.ToolbarStrings_SearchSpecific,
				(sender, args) => { Document.GoInFindMode(); },
				() => { return Document.CanDoFindRecord(); },
				Resources.Toolbar_SearchSpecific,
				Keys.F3
				);

			AddButton
				(
				"SearchGeneric",
				Resources.ToolbarStrings_SearchGeneric,
				(sender, args) => { Document.Query(); },
				() => { return true; }, //impreciso il true
				Resources.Toolbar_SearchGeneric,
				Keys.F4
				);

			AddSeparator();

			AddButton
				(
				"First",
				Resources.ToolbarStrings_First,
				(sender, args) => { Document.MoveFirst(); },
				() => { return Document.CanDoFirstRecord(); },
				Resources.Toolbar_First,
				Keys.I | Keys.Control
				);
			AddButton
				(
				"Prev",
				Resources.ToolbarStrings_Prev,
				(sender, args) => { Document.MovePrev(); },
				() => { return Document.CanDoPrevRecord(); },
				Resources.Toolbar_Prev,
				Keys.P | Keys.Control
				);
			AddButton
				(
				"Next",
				Resources.ToolbarStrings_Next,
				(sender, args) => { Document.MoveNext(); },
				() => { return Document.CanDoNextRecord(); },
				Resources.Toolbar_Next,
				Keys.S | Keys.Control
				);
			AddButton
				(
				"Last",
				Resources.ToolbarStrings_Last,
				(sender, args) => { Document.MoveLast(); },
				() => { return Document.CanDoLastRecord(); },
				Resources.Toolbar_Last,
				Keys.F | Keys.Control
				);

			AddButton
				(
				"RefreshData",
				Resources.ToolbarStrings_RefreshData,
				(sender, args) => { Document.RefreshRowset(); },
				() => { return Document.CanDoRefreshRowset(); },
				Resources.Toolbar_RefreshData,
				Keys.R | Keys.Control
				);

			AddSplitButton
				(
				"ExecuteQuery",
				Resources.ToolbarStrings_ExecuteQuery,
				(sender, args) =>
				{
					Document.ExecQuery(); 
					OnUpdateStatus();
				},
				() => { return true; }, 
				GenerateExecuteQueryDataBinding(),
				Resources.Toolbar_ExecuteQuery,
				Keys.Q | Keys.Control
				);

			AddSeparator();

			AddButton
				(
				"Delete",
				Resources.ToolbarStrings_Delete,
				(sender, args) => { Document.DeleteCurrentRecord(); },
				() => { return Document.CanDoDeleteRecord(); },
				Resources.Toolbar_Delete,
				Keys.F5
				);

			AddButton
				(
				"Undo",
				Resources.ToolbarStrings_UndoChanges,
				(sender, args) => { Document.UndoChanges(); },
				() => { return true; },
				Resources.Toolbar_UndoChanges,
				Keys.Escape
				);


			AddButton
				(
				"Finish",
				Resources.ToolbarStrings_Finish,
				(sender, args) =>
				{
					if (Document.SaveModified)
						Document.Close();
				},
				() => { return true; },
				Resources.Toolbar_Finish,
				Keys.F4 | Keys.Control
				);

			AddSeparator();

			AddButton
				(
				"Help",
				Resources.ToolbarStrings_Help,
				(sender, args) => { Document.FormHelp(); },
				() => { return true; },
				Resources.Toolbar_HelpOnLine,
				Keys.F1
				);

			AddSeparator();
		}

		//---------------------------------------------------------------------------
		private void CreateBatchToolbar()
		{
			AddSplitButton
				(
				"Report",
				Resources.ToolbarStrings_Report,
				(sender, args) => { Document.FormReport(); },
				() => { return true; }, 
				GenerateReportDataBinding(),
				Resources.Toolbar_Report
				);

			AddButton
				(
				"Customize",
				Resources.ToolbarStrings_Customize,
				(sender, args) => { Document.Customize(); },
				() => { return Document.CanDoCustomize(); },
				Resources.Toolbar_Customize
				);

			AddSeparator();

			Document.BatchExecuting += new EventHandler<CancelEventArgs>(Document_BatchExecuting);
			Document.BatchExecuted += new EventHandler<EventArgs>(Document_BatchExecuted);
			AddButton
				(
				"StartStop",
				Resources.ToolbarStrings_StartStop,
				(sender, args) => { Document.ExecuteBatch(); },
				() => { return true; },
				Resources.Toolbar_RunGreen
				);

			AddButton
				(
				"PauseResume",
				Resources.ToolbarStrings_PauseResume,
				(sender, args) => { Document.Customize(); },
				() => { return Document.CanDoCustomize(); },
				Resources.Toolbar_Hourglass
				);

			AddSeparator();

			AddButton
				(
				"Finish",
				Resources.ToolbarStrings_Finish,
				(sender, args) =>
				{
					if (Document.SaveModified)
						Document.Close();
				},
				() => { return true; },
				Resources.Toolbar_Finish
				);

			AddSeparator();

			AddButton
				(
				"Help",
				Resources.ToolbarStrings_Help,
				(sender, args) => { Document.FormHelp(); },
				() => { return true; },
				Resources.Toolbar_HelpOnLine
				);
		}

		//---------------------------------------------------------------------------
		private void CreateRowViewToolbar()
		{
			AddButton
				   (
				   "MainWindow",
				   Resources.ToolbarStrings_GoToMaster,
				   (sender, args) => {  },
				   () => { return true; },
				    Resources.Toolbar_GoToMaster,
				   Keys.F2
				   );

			AddSeparator();

			AddButton
				(
				"New",
				Resources.ToolbarStrings_New,
				(sender, args) => { Document.EnterInNewRecord(); },
				() => { return Document.CanDoNewRecord(); },
				Resources.Toolbar_New,
				Keys.F2
				);

			AddButton
				(
				"Edit",
				Resources.ToolbarStrings_Edit,
				(sender, args) => { Document.EditCurrentRecord(); },
				() => { return Document.CanDoEditRecord(); },
				Resources.Toolbar_Edit,
				Keys.Enter | Keys.Control
				);

			AddButton
				(
				"Save",
				Resources.ToolbarStrings_Save,
				(sender, args) => { Document.SaveCurrentRecord(); },
				() => { return Document.CanDoSaveRecord(); },
				Resources.Toolbar_Save,
				Keys.F10
				);

			AddSeparator();

			AddButton
				(
				"First",
				Resources.ToolbarStrings_First,
				(sender, args) => { Document.MoveFirst(); },
				() => { return Document.CanDoFirstRecord(); },
				Resources.Toolbar_First,
				Keys.I | Keys.Control
				);
			AddButton
				(
				"Prev",
				Resources.ToolbarStrings_Prev,
				(sender, args) => { Document.MovePrev(); },
				() => { return Document.CanDoPrevRecord(); },
				Resources.Toolbar_Prev,
				Keys.P | Keys.Control
				);
			AddButton
				(
				"Next",
				Resources.ToolbarStrings_Next,
				(sender, args) => { Document.MoveNext(); },
				() => { return Document.CanDoNextRecord(); },
				Resources.Toolbar_Next,
				Keys.S | Keys.Control
				);
			AddButton
				(
				"Last",
				Resources.ToolbarStrings_Last,
				(sender, args) => { Document.MoveLast(); },
				() => { return Document.CanDoLastRecord(); },
				Resources.Toolbar_Last,
				Keys.F | Keys.Control
				);

			AddButton
				(
				"RefreshData",
				Resources.ToolbarStrings_RefreshData,
				(sender, args) => { Document.RefreshRowset(); },
				() => { return Document.CanDoRefreshRowset(); },
				Resources.Toolbar_RefreshData,
				Keys.R | Keys.Control
				);

			AddSeparator();

			//qui servono quelli per muovere la riga
			AddButton
				(
				"FirstRow",
				Resources.ToolbarStrings_FirstRow,
				(sender, args) => {  },
				null,
				Resources.Toolbar_FirstRow,
				Keys.Home | Keys.Control
				);
			AddButton
				(
				"PrevRow",
				Resources.ToolbarStrings_PrevRow,
				(sender, args) => {  },
				null,
				Resources.Toolbar_PrevRow,
				Keys.Up | Keys.Control
				);
			AddButton
				(
				"NextRow",
				Resources.ToolbarStrings_NextRow,
				(sender, args) => { },
                null,
				Resources.Toolbar_NextRow,
				Keys.Down| Keys.Control
				);
			AddButton
				(
				"LastRow",
				Resources.ToolbarStrings_LastRow,
				(sender, args) => { },
                null,
				Resources.Toolbar_LastRow,
				Keys.End | Keys.Control
				);

			AddButton
				(
				"Undo",
				Resources.ToolbarStrings_UndoChanges,
				(sender, args) => { Document.UndoChanges(); },
				() => { return true; },
				Resources.Toolbar_UndoChanges,
				Keys.Escape
				);


			AddButton
				(
				"Finish",
				Resources.ToolbarStrings_Finish,
				(sender, args) =>
				{
					if (Document.SaveModified)
						Document.Close();
				},
				() => { return true; },
				Resources.Toolbar_Finish,
				Keys.F4 | Keys.Control
				);

			AddSeparator();

			AddButton
				(
				"Help",
				Resources.ToolbarStrings_Help,
				(sender, args) => { Document.FormHelp(); },
				() => { return true; },
				Resources.Toolbar_HelpOnLine,
				Keys.F1
				);

			AddSeparator();

		}

		//-------------------------------------------------------------------------
		void Document_BatchExecuted(object sender, EventArgs e)
		{
			ChangeToolBarItemDetails("StartStop", "Start", Resources.Toolbar_RunGreen);

			OnUpdateStatus();
		}

		//-------------------------------------------------------------------------
		void Document_BatchExecuting(object sender, CancelEventArgs e)
		{
			ChangeToolBarItemDetails("StartStop", "Stop", Resources.Toolbar_RunRed);

			OnUpdateStatus();
		}

		//-------------------------------------------------------------------------
		private List<IMenuItemGeneric> GenerateRadarDataBinding()
		{
			List<IMenuItemGeneric> splitItems = new List<IMenuItemGeneric>();

			MenuItemClickable nrtDefaultNrt = new MenuItemClickable
						(
						Resources.ToolbarStrings_DefaultNRT,
						"Radar",
						(sender, args) =>
						{
							Document.NewWrmRadar();
							OnUpdateStatus();
						},
						() => { return true; },
						() => { return true; },
						Keys.F7
						);
			splitItems.Add(nrtDefaultNrt);

			MenuItemClickable AltRadar = new MenuItemClickable
						(
						Resources.ToolbarStrings_Radar,
						"AltRadar",
						(sender, args) =>
						{
							Document.AltRadar(); 
							OnUpdateStatus();
						},
						() => { return true; },
						() => { return true; }, 
						Keys.F7 | Keys.Control
						);
			splitItems.Add(AltRadar);

			return splitItems;
		}

		//-------------------------------------------------------------------------
		private List<IMenuItemGeneric> GenerateExecuteQueryDataBinding()
		{
			List<IMenuItemGeneric> splitItems = new List<IMenuItemGeneric>();
			
			List<string> queries = Document.GetAttachedQueries();
			int maxNumberOfQueries = 17;
			for (int i = 0; i < queries.Count; i++)
			{
				//faccio vedere solo 17 query al massimo (vecchio limite control c++, trasferito pari pari)
				if (i >= maxNumberOfQueries)
				{
					MenuItemClickable otherItem = new MenuItemClickable
						(
						"...",
						"MoreQueries",
						(sender, args) => { Document.OtherQuery(); },
						() => { return true; },
						() => { return true; }
						);
					splitItems.Add(otherItem);
					break;
				}

				MenuItemClickable currentItem = new MenuItemClickable
						(
						queries[i],
						"Query"+i,
						(sender, args) => 
						{
							RadItem tempItem = sender as RadItem;
							Document.ExecSelQuery(tempItem.Text);
						},
						() => { return true; },
						() => { return true; }
						);
				
				splitItems.Add(currentItem);
			}

			if (Document.CanShowQueryManager())
			{
				MenuItemClickable queryManagerItem = new MenuItemClickable
						(
						Resources.QueryManagerString,
						"QueryManager",
						(sender, args) =>
						{
							Document.EditQuery();
						},
						() => { return true; },
						() => { return true; }
						);

				splitItems.Add(queryManagerItem);
			}

			return splitItems;
		}

		//-------------------------------------------------------------------------
		private List<IMenuItemGeneric> GenerateReportDataBinding()
		{
			int defaultSelection = -1;
			List<string> reports = Document.GetAttachedReports(ref defaultSelection);
			List<IMenuItemGeneric> splitItems = new List<IMenuItemGeneric>();
			for (int i = 0; i < reports.Count; i++)
			{
				splitItems.Add
					(
					new MenuItemClickable
						(reports[i], 
						"Report"+i,
						(sender, args) =>
						{
							RadItem radItem = sender as RadItem;
							Document.SelReport(radItem.Text);
						},
						() => { return true; },
						() => { return i == defaultSelection; }
						)
					);
			}
			return splitItems;
		}
	}


	//================================================================================================================
	/// <summary>
	///  secondary toolbar
	/// </summary>
	[ToolboxItem(false)]
	internal partial class UIAuxiliaryToolbar : UIToolbar
	{
		//---------------------------------------------------------------------------
		public UIAuxiliaryToolbar()
		{
			ThemeClassName = GetType().BaseType.BaseType.ToString();
			this.Visible = false;
		}

		//---------------------------------------------------------------------------
		internal void Initialize(AuxiliaryToolbarStyle toolbarStyle)
		{
   			switch (toolbarStyle)
			{
				case AuxiliaryToolbarStyle.Standard:
					OnToolbarCreated();
					break;
				case AuxiliaryToolbarStyle.None:
				default:
					return;
			}
		}
	}
}