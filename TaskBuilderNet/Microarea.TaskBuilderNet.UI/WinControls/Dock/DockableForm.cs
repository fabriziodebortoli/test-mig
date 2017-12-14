using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microarea.TaskBuilderNet.UI.WinControls.Dock.Win32;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	//=============================================================================
	// Each DockableForm instance represents a single dockable form within
	// the MDI frame. 
	// To create DockableForm in your application, simply add a form to 
	// your project, and change this form’s base class to DockableForm class.
	// DockableForm class is derived from System.Windows.Forms.Form class, 
	// plus some docking properties, methods and events. 																																																																																		
	//=============================================================================
	public partial class DockableForm : System.Windows.Forms.Form
	{
		#region States Enum and StatesEditor Class
		
		//===========================================================================
		[Flags]
		[Serializable]
		[Editor(typeof(DockableForm.StatesEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public enum States
		{
			Float		= 0x0001,
			DockLeft	= 0x0002,
			DockRight	= 0x0004,
			DockTop		= 0x0008,
			DockBottom	= 0x0010,
			Document	= 0x0020
		}

		//===========================================================================
		internal class StatesEditor : UITypeEditor
		{
			private StatesEditorControl statesEditorControl = null;

			#region StatesEditorControl Class

			//===========================================================================
			/// <summary>
			/// Summary description for StatesEditorControl.
			/// </summary>
			private class StatesEditorControl : System.Windows.Forms.UserControl
			{
				private IWindowsFormsEditorService	editorService;
				private States						oldStates;
				private CheckBox					FloatCheckBox;
				private CheckBox					DockLeftCheckBox;
				private CheckBox					DockRightCheckBox;
				private CheckBox					DockTopCheckBox;
				private CheckBox					DockBottomCheckBox;
				private CheckBox					DockFillCheckBox;

				//---------------------------------------------------------------------------
				public States States
				{
					get
					{
						States states = 0;
						if (FloatCheckBox.Checked)
							states |= States.Float;
						if (DockLeftCheckBox.Checked)
							states |= States.DockLeft;
						if (DockRightCheckBox.Checked)
							states |= States.DockRight;
						if (DockTopCheckBox.Checked)
							states |= States.DockTop;
						if (DockBottomCheckBox.Checked)
							states |= States.DockBottom;
						if (DockFillCheckBox.Checked)
							states |= States.Document;

						if (states == 0)
							return oldStates;
						else
							return states;
					}
				}

				//---------------------------------------------------------------------------
				public StatesEditorControl()
				{
					FloatCheckBox = new CheckBox();
					DockLeftCheckBox = new CheckBox();
					DockRightCheckBox = new CheckBox();
					DockTopCheckBox = new CheckBox();
					DockBottomCheckBox = new CheckBox();
					DockFillCheckBox = new CheckBox();

					SuspendLayout();

					FloatCheckBox.Appearance = Appearance.Button;
					FloatCheckBox.Dock = DockStyle.Top;
					FloatCheckBox.Height = 24;
					FloatCheckBox.Text = "(Float)";
					FloatCheckBox.TextAlign = ContentAlignment.MiddleCenter;
					FloatCheckBox.FlatStyle = FlatStyle.System;
			
					DockLeftCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
					DockLeftCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
					DockLeftCheckBox.Width = 24;
					DockLeftCheckBox.FlatStyle = FlatStyle.System;

					DockRightCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
					DockRightCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
					DockRightCheckBox.Width = 24;
					DockRightCheckBox.FlatStyle = FlatStyle.System;

					DockTopCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
					DockTopCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
					DockTopCheckBox.Height = 24;
					DockTopCheckBox.FlatStyle = FlatStyle.System;

					DockBottomCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
					DockBottomCheckBox.Dock = System.Windows.Forms.DockStyle.Bottom;
					DockBottomCheckBox.Height = 24;
					DockBottomCheckBox.FlatStyle = FlatStyle.System;
			
					DockFillCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
					DockFillCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
					DockFillCheckBox.FlatStyle = FlatStyle.System;

					this.Controls.AddRange(new Control[] {
															 DockFillCheckBox,
															 DockBottomCheckBox,
															 DockTopCheckBox,
															 DockRightCheckBox,
															 DockLeftCheckBox,
															 FloatCheckBox});

					Size = new System.Drawing.Size(160, 144);
					BackColor = SystemColors.Control;
					ResumeLayout(false);
				}

				//---------------------------------------------------------------------------
				public void SetStates(IWindowsFormsEditorService aEditorService, States statesFlags)
				{
					editorService = aEditorService;

					oldStates = statesFlags;
					
					if ((statesFlags & States.DockLeft) != 0)
						DockLeftCheckBox.Checked = true;
					if ((statesFlags & States.DockRight) != 0)
						DockRightCheckBox.Checked = true;
					if ((statesFlags & States.DockTop) != 0)
						DockTopCheckBox.Checked = true;
					if ((statesFlags & States.DockTop) != 0)
						DockTopCheckBox.Checked = true;
					if ((statesFlags & States.DockBottom) != 0)
						DockBottomCheckBox.Checked = true;
					if ((statesFlags & States.Document) != 0)
						DockFillCheckBox.Checked = true;
					if ((statesFlags & States.Float) != 0)
						FloatCheckBox.Checked = true;
				}
			}
			
			#endregion

			//---------------------------------------------------------------------------
			public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
			{
				return UITypeEditorEditStyle.DropDown;
			}

			//---------------------------------------------------------------------------
			public override object EditValue(ITypeDescriptorContext context, IServiceProvider sp, object value)
			{
				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)sp.GetService(typeof(IWindowsFormsEditorService));

				if (statesEditorControl == null)
					statesEditorControl = new StatesEditor.StatesEditorControl();

				statesEditorControl.SetStates(edSvc, (States)value);
				edSvc.DropDownControl(statesEditorControl);

				return statesEditorControl.States;
			}
		}

		#endregion

		#region DockableForm fields

		private States					allowedStates = States.DockLeft | States.DockRight | States.DockTop | States.DockBottom | States.Document | States.Float;
		private bool					redockingAllowed = true;
		private bool					enableCloseButton = true;
		private bool					isDummy = false;
		private double					autoHiddenFactor = 0.25;
		private DockableFormsContainer	container = null;
		private DockManager				dockManager = null;
		private DockState				dockState = DockState.Unknown;
		private HiddenMdiChild			hiddenMdiChild = null;
		private bool					hidden = false;
		private bool					hideOnClose = false;
		private DockState				showHint = DockState.Unknown;
		private System.Drawing.Size		floatingFormDefaultSize = FloatingForm.DefaultSize;

		private static readonly object DockStateChangedEvent = new object();
		
		// Tab width and X position used by DockableFormsContainer and DockManager class
		internal int TabWidth = 0;
		internal int TabX = 0;

		#endregion

		#region DockableForm Constructors and Dispose method
		
		//---------------------------------------------------------------------------
		public DockableForm(bool dummyForm)
		{
			isDummy = dummyForm;

			RefreshMdiIntegration();
		}

		//---------------------------------------------------------------------------
		public DockableForm() : this(false)
		{
		}


	
		#endregion

		#region DockableForm internal properties

		//---------------------------------------------------------------------------
		internal string PersistString
		{
			get	{ return GetPersistString(); }
		}

		//---------------------------------------------------------------------------
		internal TabbedPanel TabbedPanel
		{
			get	{ return (FormContainer != null) ? FormContainer.TabbedPanel : null; }
		}

		//---------------------------------------------------------------------------
		internal HiddenMdiChild HiddenMdiChild
		{
			get	{ return hiddenMdiChild; }
		}

		#endregion

		#region DockableForm private properties

		//---------------------------------------------------------------------------
		private DockState DefaultShowState
		{
			get
			{
				if (ShowHint != DockState.Unknown)
					return ShowHint;

				if ((AllowedStates & States.Document) != 0)
					return DockState.Document;
				if ((AllowedStates & States.DockRight) != 0)
					return DockState.DockRight;
				if ((AllowedStates & States.DockLeft) != 0)
					return DockState.DockLeft;
				if ((AllowedStates & States.DockBottom) != 0)
					return DockState.DockBottom;
				if ((AllowedStates & States.DockTop) != 0)
					return DockState.DockTop;
				if ((AllowedStates & States.Float) != 0)
					return DockState.Float;

				return DockState.Unknown;
			}
		}
		
		#endregion

		#region DockableForm public properties

		//---------------------------------------------------------------------------
		public	bool IsDummy { get { return isDummy;} }

		//---------------------------------------------------------------------------
		[DefaultValue(true)]
		public bool IsRedockingAllowed
		{
			get	{	return redockingAllowed;	}
			set	{	redockingAllowed = value;	}
		}

		//---------------------------------------------------------------------------
		[DefaultValue(0.25)]
		public double AutoHiddenFactor
		{
			get	{	return autoHiddenFactor;	}
			set
			{
				if (value <= 0 || value > 1)
                    throw new ArgumentOutOfRangeException();

				if (autoHiddenFactor == value)
					return;

				autoHiddenFactor = value;

				if (DockManager == null)
					return;

				if (DockManager.ActiveAutoHideForm == this)
					DockManager.PerformLayout();
			}
		}
		
		//---------------------------------------------------------------------------
		[DefaultValue(States.DockLeft|States.DockRight|States.DockTop|States.DockBottom|States.Document|States.Float)]
		public States AllowedStates
		{
			get	{	return allowedStates;	}
			set
			{
				if (allowedStates == value)
					return;

				if (!DockManager.IsDockStateValid(DockState, value))
                    throw new InvalidOperationException();

				allowedStates = value;

				if (!DockManager.IsDockStateValid(ShowHint, allowedStates))
					ShowHint = DockState.Unknown;
			}
		}

		//---------------------------------------------------------------------------
		[DefaultValue(true)]
		public bool EnableCloseButton
		{
			get	{	return enableCloseButton;	}
			set
			{
				if (enableCloseButton == value)
					return;

				enableCloseButton = value;
				if (FormContainer != null)
					if (FormContainer.ActiveForm == this)
						FormContainer.Refresh();
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public DockableFormsContainer FormContainer
		{
			get { return container; }
			set
			{
				if (container == value)
					return;

				DockableFormsContainer oldValue = container;
				container = value;

				if (oldValue != null)
					oldValue.RemoveForm(this);

				if (value != null)
					value.AddForm(this);
				
				SetDockState();
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public DockManager DockManager
		{
			get { return dockManager; }
			set
			{
				if (dockManager == value)
					return;

				FormContainer = null;

				if (dockManager != null)
					dockManager.RemoveForm(this);

				dockManager = value;

				if (dockManager != null)
				{
					dockManager.AddForm(this);
					TopLevel = false;
					FormBorderStyle = FormBorderStyle.None;
					ShowInTaskbar = false;
					Visible = true;
				}

				RefreshMdiIntegration();
			}
		}
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public DockState DockState
		{
			get	{	return dockState;	}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsActivated
		{
			get
			{
				return (FormContainer != null && FormContainer.ActiveForm == this) ? FormContainer.IsActivated : false;
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public System.Drawing.Font TabsFont
		{
			get 
			{
				if (FormContainer != null)
					return FormContainer.TabsFont;
				
				if (DockManager != null)
					return DockManager.DefaultTabsFont; 

				return SystemInformation.MenuFont;
			}
			set 
			{
				if (FormContainer == null) 
					return;
				FormContainer.TabsFont = value;
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public System.Drawing.Color TabsActiveTabTextColor
		{
			get 
			{
				if (FormContainer != null)
					return FormContainer.TabsActiveTabTextColor;
				
				if (DockManager != null)
					return DockManager.DefaultActiveTabTextColor; 

				return System.Drawing.SystemColors.ControlText;
			}
			set 
			{
				if (FormContainer == null) 
					return;
				FormContainer.TabsActiveTabTextColor = value;
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public System.Drawing.Color TabsInactiveTabTextColor
		{
			get 
			{
				if (FormContainer != null)
					return FormContainer.TabsInactiveTabTextColor;
				
				if (DockManager != null)
					return DockManager.DefaultInactiveTabTextColor; 

				return System.Drawing.SystemColors.GrayText;
			}
			set 
			{
				if (FormContainer == null) 
					return;
				FormContainer.TabsInactiveTabTextColor = value;
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsHidden
		{
			get	{ return hidden; }
			set
			{
				if (hidden == value)
					return;

				hidden = value;

				if (hidden)
					base.Hide();
				else
					base.Show();

				if (hidden && this.FormContainer != null && this.FormContainer.VisibleFormsCount == 0)
					this.FormContainer.Hide();
			}
		}

		//---------------------------------------------------------------------------
		[DefaultValue(false)]
		public bool HideOnClose
		{
			get	{	return hideOnClose;	}
			set	{	hideOnClose = value;	}
		}

		//---------------------------------------------------------------------------
		[DefaultValue(DockState.Unknown)]
		public DockState ShowHint
		{
			get	{ return showHint; }
			set
			{	
				if (showHint == value)
					return;

				if (DockManager.IsDockStateValid(value, AllowedStates))
					showHint = value;
				else
					throw (new InvalidOperationException("Desired state not allowed, check AllowedStates property."));
			}
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Size FloatingFormDefaultSize 
		{
			get { return floatingFormDefaultSize;} 
			set 
			{
				if (this.MinimumSize == Size.Empty)
				{
					floatingFormDefaultSize = value; 
					return;
				}

				floatingFormDefaultSize = new Size (Math.Max(floatingFormDefaultSize.Width, this.MinimumSize.Width), Math.Max(floatingFormDefaultSize.Height, this.MinimumSize.Height));
			}
		}

		#endregion

		#region DockableForm internal methods

		//---------------------------------------------------------------------------
		internal void SetDockState()
		{
			DockState dockStateToSet = GetDockState();
			if (DockState == dockStateToSet)
				return;

			dockState = dockStateToSet;
			RefreshMdiIntegration();
			OnDockStateChanged(EventArgs.Empty);
		}

		//---------------------------------------------------------------------------
		internal void SetParent(Control value)
		{
			if (Parent == value)
				return;

			Parent = value;
		}

		//---------------------------------------------------------------------------
		internal void RefreshMdiIntegration()
		{
			Form mdiParent = GetMdiParentForm();

			if (mdiParent != null)
			{
				if (hiddenMdiChild != null)
					hiddenMdiChild.SetMdiParent(mdiParent);
				else
					hiddenMdiChild = new HiddenMdiChild(this);
			}
			else if (hiddenMdiChild != null)
			{
				hiddenMdiChild.Close();
				hiddenMdiChild = null;
			}
		}
		
		#endregion

		#region DockableForm private methods

		//---------------------------------------------------------------------------
		private DockState GetDockState()
		{
			if (FormContainer == null)
				return DockState.Unknown;
			else if (IsHidden)
				return DockState.Hidden;
			else
				return FormContainer.DockState;
		}
		
		//---------------------------------------------------------------------------
		private Form GetMdiParentForm()
		{
			if (DockManager == null)
				return null;

			if (!DockManager.MdiIntegration)
				return null;

			if (DockState != DockState.Document)
				return null;

			Form parentMdi = DockManager.FindForm();
			if (parentMdi != null)
				if (!parentMdi.IsMdiContainer)
					parentMdi = null;

			return parentMdi;
		}
		
		#endregion

		#region DockableForm protected overridable methods

		//---------------------------------------------------------------------------
		protected virtual string GetPersistString()
		{
			return GetType().ToString();
		}

		#endregion

		#region DockableForm protected overridden methods

		//---------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);

			if (this.MinimumSize != Size.Empty && this.MinimumSize.Width > 0 && this.MinimumSize.Height > 0)
				floatingFormDefaultSize = this.MinimumSize;
		}
		
		//---------------------------------------------------------------------------
		protected override void OnVisibleChanged(EventArgs e)
		{
			if (FormContainer != null)
			{
				FormContainer.SetDockState();
				FormContainer.ValidateActiveForm();
			}
			
			if (HiddenMdiChild != null)
				HiddenMdiChild.Visible = (!hidden);

			base.OnVisibleChanged(e);
		}

		//---------------------------------------------------------------------------
		protected override void OnTextChanged(EventArgs e)
		{
			if (hiddenMdiChild != null)
				hiddenMdiChild.Text = this.Text;

			if (FormContainer != null)
			{
				if (FormContainer.FloatingForm != null)
					FormContainer.FloatingForm.SetText();
				FormContainer.Refresh();
			}

			base.OnTextChanged(e);
		}

		//---------------------------------------------------------------------------
		protected override void OnMinimumSizeChanged(EventArgs e)
		{
			base.OnMinimumSizeChanged (e);

			floatingFormDefaultSize = new Size (Math.Max(floatingFormDefaultSize.Width, this.MinimumSize.Width), Math.Max(floatingFormDefaultSize.Height, this.MinimumSize.Height));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (this.FormContainer != null)
				this.FormContainer.RemoveForm(this);
		}
		
		#endregion

		#region DockableForm public methods

		//---------------------------------------------------------------------------
		public bool IsDockStateValid(DockState dockState)
		{
			return DockManager.IsDockStateValid(dockState, AllowedStates);
		}

		//---------------------------------------------------------------------------
		public void SetForegroundWindow()
		{
			if (FormContainer != null)
			{
				if (!FormContainer.IsActivated)
					FormContainer.Activate();

				FormContainer.ActiveForm = this;

				if (this.DockState == DockState.Float)
				{
					User32.SetForegroundWindow(FormContainer.FloatingForm.Handle);
					return;
				}

				TabbedPanel currentTabbedPanel = FormContainer.TabbedPanel;
				
				if (currentTabbedPanel != null && currentTabbedPanel.Parent != null && currentTabbedPanel.Parent.Handle != IntPtr.Zero)
					User32.SetForegroundWindow(currentTabbedPanel.Parent.Handle);
			}
		}

		//---------------------------------------------------------------------------
		public new void Activate()
		{
			if (DockManager == null)
			{
				base.Activate();
				return;
			}
			
			if (FormContainer == null)
			{
				Show(DockManager);
				return;
			}
			
			IsHidden = false;

			FormContainer.ActiveForm = this;
			FormContainer.Activate();

			if (this.DockState == DockState.Float)
			{
				this.Size = this.FloatingFormDefaultSize;
				SetForegroundWindow();
			}

			this.Focus();
		}

		//---------------------------------------------------------------------------
		public new void Hide()
		{
			IsHidden = true;
		}

		//---------------------------------------------------------------------------
		public new void Show()
		{
			if (DockManager == null)
			{
				base.Show();
				
				OnShowed(new DockableFormEventArgs(this));

				return;
			}

			Show(DockManager);
		}

		//---------------------------------------------------------------------------
		public void Show(DockManager dockManager)
		{
			if (dockManager == null)
                throw new ArgumentNullException();

			if (DockState == DockState.Unknown)
				Show(dockManager, DefaultShowState);
			else			
				Activate();
		}

		//---------------------------------------------------------------------------
		public void Show(DockManager dockManager, DockState dockState)
		{
			if (dockManager == null)
                throw new ArgumentNullException();

			if (dockState == DockState.Unknown || dockState == DockState.Hidden)
                throw new ArgumentException();

			DockManager = dockManager;

			if (FormContainer == null)
			{
				DockableFormsContainer existingContainer = null;
				foreach (DockableFormsContainer aContainer in DockManager.Containers)
					if (aContainer.VisibleState == dockState)
					{
						existingContainer = aContainer;
						break;
					}

				if (existingContainer == null || dockState == DockState.Float)
					this.FormContainer = new DockableFormsContainer(this, dockState);
				else
					this.FormContainer = existingContainer;
			}

			Activate();
	
			OnShowed(new DockableFormEventArgs(this));
		}

		#endregion

		#region DockableForm Events

		//---------------------------------------------------------------------------
		public event EventHandler DockStateChanged
		{
			add	{ Events.AddHandler(DockStateChangedEvent, value); }
			remove { Events.RemoveHandler(DockStateChangedEvent, value); }
		}
		
		//---------------------------------------------------------------------------
		protected virtual void OnDockStateChanged(EventArgs e)
		{
			EventHandler handler = (EventHandler)Events[DockStateChangedEvent];
			if (handler != null)
				handler(this, e);
		}

		//---------------------------------------------------------------------------
		public delegate void DockableFormShowEventHandler(object sender, DockableFormEventArgs e);
		private static readonly object ShowedEvent = new object();
		public event DockableFormShowEventHandler Showed
		{
			add	{ Events.AddHandler(ShowedEvent, value); }
			remove { Events.RemoveHandler(ShowedEvent, value); }
		}
		//---------------------------------------------------------------------------
		protected virtual void OnShowed(DockableFormEventArgs e)
		{
			DockableFormShowEventHandler handler = (DockableFormShowEventHandler)Events[ShowedEvent];
			if (handler != null)
				handler(this, e);
		}

		#endregion
	}

	#region DockableFormsCollection Class
	//===========================================================================
	/// <summary>
	/// Summary description for DockableFormsCollection.
	/// </summary>
	public class DockableFormsCollection : ReadOnlyCollectionBase
	{
		//---------------------------------------------------------------------------
		internal DockableFormsCollection()
		{
		}

		//---------------------------------------------------------------------------
		internal void Dispose()
		{
			for (int i=Count - 1; i>=0; i--)
				this[i].Dispose();
		}

		//---------------------------------------------------------------------------
		public DockableForm this[int index]
		{
			get {  return InnerList[index] as DockableForm;  }
		}

		//---------------------------------------------------------------------------
		internal int Add(DockableForm aFormToAdd)
		{
			if (Contains(aFormToAdd))
				return IndexOf(aFormToAdd);

			return InnerList.Add(aFormToAdd);
		}

		//---------------------------------------------------------------------------
		internal void Insert(int index, DockableForm aFormToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;

			if (Contains(aFormToInsert))
				return;

			InnerList.Insert(index, aFormToInsert);
		}

		//---------------------------------------------------------------------------
		internal void Insert(DockableForm beforeForm, DockableForm aFormToInsert)
		{
			if (beforeForm == null)
				Add(aFormToInsert);

			if (!Contains(beforeForm))
				return;

			if (Contains(aFormToInsert))
				return;

			Insert(IndexOf(beforeForm), aFormToInsert);
		}

		//---------------------------------------------------------------------------
		internal void Remove(DockableForm aFormToRemove)
		{
			if (!Contains(aFormToRemove))
				return;

			InnerList.Remove(aFormToRemove);
		}

		//---------------------------------------------------------------------------
		internal void Clear()
		{
			InnerList.Clear();
		}

		//---------------------------------------------------------------------------
		public bool Contains(DockableForm aFormToSearch)
		{
			return InnerList.Contains(aFormToSearch);
		}

		//---------------------------------------------------------------------------
		public int IndexOf(DockableForm aFormToSearch)
		{
			if (!Contains(aFormToSearch))
				return -1;
			else
				return InnerList.IndexOf(aFormToSearch);
		}

		//---------------------------------------------------------------------------
		public DockableForm[] Select(DockableForm.States stateFilter)
		{
			int count = 0;
			
			foreach (DockableForm aForm in this)
				if (DockManager.IsDockStateValid(aForm.DockState, stateFilter))
					count ++;

			DockableForm[] forms = new DockableForm[count];

			count = 0;
			foreach (DockableForm aForm in this)
				if (DockManager.IsDockStateValid(aForm.DockState, stateFilter))
					forms[count++] = aForm;

			return forms;
		}
	}
	#endregion

	#region HiddenMdiChild Class

	//===========================================================================
	// La classe HiddenMdiChild serve per poter simulare all'interno della finestra
	// principale dell'applicazione il comportamento usuale delle finestre di tipo
	// MDI Child anche per i documenti aperti in finestre di classe DockableForm: 
	// il titolo del documento viene, grazie a questo stratagemma, mostrato nel 
	// menù delle finestre aperte della form principale e può essere così attivato
	// direttamente cliccando su tale voce.
	//===========================================================================
	/// <summary>
	/// Summary description for HiddenMdiChild.
	/// </summary>
	internal class HiddenMdiChild : Form
	{
		private DockableForm form = null;

		//---------------------------------------------------------------------------
		internal HiddenMdiChild(DockableForm aDockableForm) : base()
		{
			if (aDockableForm == null)
				throw(new ArgumentNullException());

			// Control.FindForm retrieves the form that the control is on.
			Form parentMdiForm = (aDockableForm.DockManager != null) ? aDockableForm.DockManager.FindForm() : null;
			if (parentMdiForm != null)
				if (!parentMdiForm.IsMdiContainer)
					parentMdiForm = null;

			if (parentMdiForm == null)
				throw(new InvalidOperationException());

			form = aDockableForm;
			Menu = aDockableForm.Menu;
			FormBorderStyle = FormBorderStyle.None;
			Text = form.Text;
			Size = new Size(0, 0);
			SetMdiParent(parentMdiForm);
		}

		//---------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				form = null;
				Menu = null;
			}
			base.Dispose(disposing);
		}

		//---------------------------------------------------------------------------
		public void SetMdiParent(Form parentMdiForm)
		{
			if (parentMdiForm == null)
				throw(new ArgumentNullException());

			MdiParent = parentMdiForm;
			Show();
			Size = new Size(0, 0);
		}

		//---------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			if (form != null && m.Msg == (int)Win32.Msgs.WM_MDIACTIVATE && m.HWnd == m.LParam)
				form.Show();

			base.WndProc(ref m);
		}
	}
	#endregion

    //===========================================================================
    public enum DockState
    {
        Unknown = 0,
        Float = 1,
        DockTopAutoHide = 2,
        DockLeftAutoHide = 3,
        DockBottomAutoHide = 4,
        DockRightAutoHide = 5,
        Document = 6,
        DockTop = 7,
        DockLeft = 8,
        DockBottom = 9,
        DockRight = 10,
        Hidden = 11
    }
}
