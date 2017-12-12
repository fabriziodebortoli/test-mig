using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	//===========================================================================
	// DockableFormsContainer class works as container of DockableForm class. 
	// It displays a collection of DockableForm with a tab strip.
	// The DockableFormsContainer.Forms property and DockableForm.FormContainer 
	// property specify the parent-child relationship between DockableFormsContainer
	// instance and DockableForm instance. 
	// The ActiveForm property determines which DockableForm is currently
	// being displayed in the DockableFormsContainer instance.
	//===========================================================================
	
	/// <summary>
	/// Description of class DockableFormsContainer
	/// </summary>
	public class DockableFormsContainer : IDisposable
	{
		//===========================================================================
		public enum LayoutStyles
		{
			None,
			Vertical,
			Horizontal
		}
		
		#region DockableFormsContainer private fields
		
		private DockableForm				activeForm = null;
		private DockableFormsCollection		forms = null;
		private DockManager					dockManager = null;
		private DockState					dockState = DockState.Unknown;
		private bool						redockingAllowed = true;
		private bool						disposed = false;
		private EventHandlerList			events;
		private FloatingForm				floatingForm = null;
		private bool						activated = false;
		private bool						hidden = false;
		private LayoutStyles				dockLayoutStyle = LayoutStyles.Vertical;
		private LayoutStyles				documentLayoutStyle = LayoutStyles.Vertical;
		private LayoutStyles				floatingLayoutStyle = LayoutStyles.Horizontal;
		private DockState					restoreState = DockState.Unknown;
		private double						dockSize = 0.5;
		private double						documentSize = 0.5;
		private double						floatingFormSize = 0.5;
		private DockableTabbedPanel			dockTabbedPanel = null;
		private DockableDocumentPanel		documentTabbedPanel = null;
		private DockableTabbedPanel			floatingTabbedPanel = null;
		private DockState					visibleState;
		private System.Drawing.Font			tabsFont = SystemInformation.MenuFont;
		private System.Drawing.Color		tabsActiveTabTextColor = SystemColors.ControlText;
		private System.Drawing.Color		tabsInactiveTabTextColor = SystemColors.GrayText;
		private System.Drawing.Color		dockActiveCaptionColor = SystemColors.ActiveCaption;
		private System.Drawing.Color		dockActiveCaptionTextColor = SystemColors.ActiveCaptionText;
		private System.Drawing.Color		dockInactiveCaptionColor = SystemColors.InactiveCaption;
		private System.Drawing.Color		dockInactiveCaptionTextColor = SystemColors.InactiveCaptionText;
		private bool						showTabsAlways = true;

		#endregion

		//---------------------------------------------------------------------------
		public DockableFormsContainer(DockableForm aForm, DockState dockState)
		{
			Initialize(aForm, dockState, null, false, Rectangle.Empty);
		}

		//---------------------------------------------------------------------------
		public DockableFormsContainer(DockableForm aForm, DockState dockState, FloatingForm aFloatingForm)
		{
			Initialize(aForm, dockState, aFloatingForm, false, Rectangle.Empty);
		}

		//---------------------------------------------------------------------------
		public DockableFormsContainer(DockableForm aForm, Rectangle floatingFormBounds)
		{
			Initialize(aForm, DockState.Float, null, true, floatingFormBounds);
		}

		//---------------------------------------------------------------------------
		~DockableFormsContainer()
		{
			Dispose(false);
		}

		//---------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//---------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			disposed = true;

			if (disposing)
			{
				Forms.Dispose();
				if (dockTabbedPanel != null)
				{
					dockTabbedPanel.Dispose();
					dockTabbedPanel = null;
				}
				if (floatingTabbedPanel != null)
				{
					floatingTabbedPanel.Dispose();
					floatingTabbedPanel = null;
				}
				if (documentTabbedPanel != null)
				{
					documentTabbedPanel.Dispose();
					documentTabbedPanel = null;
				}
				Events.Dispose();

				if (dockManager != null)
				{
					dockManager.RemoveContainer(this);
					dockManager = null;
				}
			}
		}
		
		#region DockableFormsContainer public properties
		
		//---------------------------------------------------------------------------
		public DockableFormsCollection Forms
		{
			get	{ return forms; }
		}

		//---------------------------------------------------------------------------
		public int VisibleFormsCount
		{
			get
			{
				int count = 0;
				foreach (DockableForm aForm in Forms)
				{
					if (!aForm.IsHidden)
						count ++;
				}
				return count;
			}
		}

		//---------------------------------------------------------------------------
		public DockManager DockManager { get { return dockManager; } }

		//---------------------------------------------------------------------------
		public string Text { get { return (ActiveForm != null) ? ActiveForm.Text : String.Empty; } }

		//---------------------------------------------------------------------------
		public DockState VisibleState
		{
			get	{ return visibleState; }
			set
			{
				if (value == DockState.Unknown || value == DockState.Hidden || !IsDockStateValid(value))
					throw new InvalidOperationException();

				visibleState = value;
				SetDockState();
			}
		}

		//---------------------------------------------------------------------------
		public bool IsDisposed { get { return disposed; } }
		
		//---------------------------------------------------------------------------
		public DockState DockState { get { return dockState; } }
		
		//---------------------------------------------------------------------------
		public bool IsRedockingAllowed { get { return redockingAllowed; } set { redockingAllowed = value; } }
		
		//---------------------------------------------------------------------------
		public bool IsAutoHide { get { return DockManager.IsDockStateAutoHide(DockState); } }

		//---------------------------------------------------------------------------
		public DockableForm ActiveForm
		{
			get	{ return activeForm; }
			set
			{
				if (activeForm == value)
					return;

				if (value != null)
				{
					if (GetVisibleFormIndex(value) == -1)
						throw new InvalidOperationException(Strings.ActiveDockableFormInvalidValue);
				}
				else
				{
					if (this.VisibleFormsCount != 0)
						throw new InvalidOperationException(Strings.ActiveDockableFormInvalidValue);
				}

				DockableForm oldValue = activeForm;

				activeForm = value;
				if (activeForm != null)
				{
					TabbedPanel tabbedPanel = GetTabbedPanel(DockState);
					if (tabbedPanel != null)
						tabbedPanel.EnsureTabVisible(activeForm);
				}

				if (DockManager.ActiveDocumentsContainer == this)
					DockManager.SetActiveDocumentForm(activeForm);

				if (FloatingForm != null)
					FloatingForm.SetText();

				Refresh();
			}
		}
		
		//---------------------------------------------------------------------------
		private bool ContainsFocus
		{
			get
			{
				if (DockManager.Containers != null && DockManager.Containers.Count > 0)
				{
					foreach (DockableFormsContainer aContainer in DockManager.Containers)
					{
						if (aContainer.TabbedPanel == null)
							continue;

						if (aContainer.TabbedPanel.ContainsFocus && Contains(aContainer))
							return false;
					}
				}

				if (TabbedPanel == null)
					return false;
				else
					return TabbedPanel.ContainsFocus;
			}
		}

		//---------------------------------------------------------------------------
		public FloatingForm FloatingForm
		{
			get	{ return floatingForm; }
			set
			{
				if (floatingForm == value)
					return;

				if (FloatingTabbedPanel == null)
					floatingTabbedPanel = new DockableTabbedPanel(this, true);

				FloatingForm oldValue = floatingForm;

				floatingForm = value;

				if (oldValue != null)
					oldValue.RemoveContainer(this);
				if (value != null)
					value.AddContainer(this);
			}
		}

		//---------------------------------------------------------------------------
		public bool IsActivated
		{
			get	{ return activated; }
			set
			{
				//if (activated == value)
				//	return;

				if (value == false && DockManager.ActiveDocumentsContainer == this)
					return;

				if (value == true)
					Show();

				if (value && TabbedPanel == null)
					return;
				
				activated = value;
				
				if (!activated && ContainsFocus)
					DockManager.AutoHideControl.Focus();
				else if (activated)
				{
					DockableFormsContainer previousContainer = this.Previous;
					while (previousContainer != null)
					{
						previousContainer.IsActivated = false;
						previousContainer = previousContainer.Previous;
					}
					if (DockState == DockState.Float)
					{
						foreach(DockableFormsContainer aDockContainer in DockManager.Containers)
						{
							aDockContainer.IsActivated = false;
						}						
					}


					if (IsAutoHide)
						DockManager.ActiveAutoHideForm = ActiveForm;
					if (DockState == DockState.Document)
						DockManager.SetActiveDocument(this);

					TabbedPanel.Focus();

					if (DockState == DockState.Float)
						FloatingForm.Activate();
				}

				Refresh();
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsHidden
		{
			get	{	return hidden;	}
			set
			{
				hidden = value;
				SetDockState();
			}
		}

		//---------------------------------------------------------------------------
		public LayoutStyles DockLayoutStyle
		{
			get	{ return dockLayoutStyle; }
			set
			{
				if (dockLayoutStyle == value)
					return;

				dockLayoutStyle = value;
				if (DockManager.IsDockStateDocked(DockState))
					PerformLayout();
			}
		}

		//---------------------------------------------------------------------------
		public LayoutStyles DocumentLayoutStyle
		{
			get	{ return documentLayoutStyle; }
			set
			{
				if (documentLayoutStyle == value)
					return;

				documentLayoutStyle = value;
				if (DockState == DockState.Document)
					PerformLayout();
			}
		}
		
		//---------------------------------------------------------------------------
		public LayoutStyles FloatingLayoutStyle
		{
			get	{ return floatingLayoutStyle;	}
			set
			{
				if (floatingLayoutStyle == value)
					return;

				floatingLayoutStyle = value;
				if (DockState == DockState.Float)
					PerformLayout();
			}
		}
		
		//---------------------------------------------------------------------------
		public DockableFormsContainer First
		{
			get
			{
				for (int i=0; i < DockManager.Containers.Count; i++)
				{
					if (DockManager.Containers[i].DockState == DockState)
						return DockManager.Containers[i];
				}

				return null;
			}
		}

		//---------------------------------------------------------------------------
		public DockableFormsContainer Last
		{
			get
			{
				for (int i = DockManager.Containers.Count-1; i >= 0; i--)
				{
					if (DockManager.Containers[i].DockState == DockState)
						return DockManager.Containers[i];
				}

				return null;
			}
		}

		//---------------------------------------------------------------------------
		public DockableFormsContainer Next
		{
			get
			{
				DockableFormsContainersCollection containers = null;

				if (DockState == DockState.Float)
				{
					if (this.FloatingForm == null)
						return null;

					containers = this.FloatingForm.Containers;
				}
				else
				{
					if (this.DockManager == null)
						return null;
					containers = this.DockManager.Containers;
				}

				if (containers == null)
					return null;

				for (int i = containers.IndexOf(this)+1; i < containers.Count; i++)
				{
					if (containers[i].DockState == DockState)
						return containers[i];
				}

				return null;
			}
		}

		//---------------------------------------------------------------------------
		public DockableFormsContainer Previous
		{
			get
			{
				DockableFormsContainersCollection containers;

				if (DockState == DockState.Float)
					containers = FloatingForm.Containers;
				else
					containers = DockManager.Containers;

				for (int i = containers.IndexOf(this)-1; i >= 0; i--)
				{
					if (containers[i].DockState == DockState)
						return containers[i];
				}

				return null;
			}
		}

		//---------------------------------------------------------------------------
		public DockState RestoreState
		{
			get	{	return restoreState;	}
			set
			{
				if (!DockManager.IsValidRestoreState(value))
                    throw new InvalidOperationException();

				restoreState = value;
			}
		}

		//---------------------------------------------------------------------------
		public double DockSize
		{
			get	{	return dockSize;	}
			set
			{
				if (value <= 0 || value >= 1)
					throw(new ArgumentOutOfRangeException());

				if (dockSize == value)
					return;

				dockSize = value;
				if (DockManager.IsDockStateDocked(DockState))
					PerformLayout();
			}
		}

		//---------------------------------------------------------------------------
		public double DocumentSize
		{
			get	{ return documentSize; }
			set
			{
				if (value <= 0 || value >= 1)
					throw(new ArgumentOutOfRangeException());

				if (documentSize == value)
					return;

				documentSize = value;
				if (DockState == DockState.Document)
					PerformLayout();
			}
		}

		//---------------------------------------------------------------------------
		public double FloatingSize
		{
			get	{ return floatingFormSize; }
			set
			{
				if (value <= 0 || value >= 1)
					throw(new ArgumentOutOfRangeException());

				if (floatingFormSize == value)
					return;

				floatingFormSize = value;
				if (DockState == DockState.Float)
					PerformLayout();
			}
		}

		//---------------------------------------------------------------------------
		public double LayoutSize
		{
			get
			{
				if (DockState == DockState.Float)
					return FloatingSize;
				else if (DockState == DockState.Document)
					return DocumentSize;
				else
					return DockSize;
			}
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Font TabsFont
		{
			get { return tabsFont; } 
			set 
			{
				if (tabsFont.Equals(value))
					return;

				tabsFont = value;

				if (TabbedPanel != null)
					TabbedPanel.TabsFont = tabsFont;

			}
		}

		//---------------------------------------------------------------------------
		public System.Drawing.Color TabsActiveTabTextColor
		{
			get { return tabsActiveTabTextColor; } 
			set 
			{
				if (tabsActiveTabTextColor.Equals(value))
					return;

				tabsActiveTabTextColor = value;

				if (TabbedPanel != null)
					TabbedPanel.TabsActiveTabTextColor = tabsActiveTabTextColor;
			}
		}
		
		//---------------------------------------------------------------------------
		public System.Drawing.Color TabsInactiveTabTextColor
		{
			get { return tabsInactiveTabTextColor; } 
			set
			{
				if (tabsInactiveTabTextColor.Equals(value))
					return;

				tabsInactiveTabTextColor = value; 

				if (TabbedPanel != null)
					TabbedPanel.TabsInactiveTabTextColor = tabsInactiveTabTextColor;
			}
		}
	
		//---------------------------------------------------------------------------
		public bool ShowTabsAlways
		{
			get { return showTabsAlways; }
		
			set 
			{
				if (showTabsAlways == value)
					return;
				
				showTabsAlways = value;

				Refresh();
			}
		}

		#endregion

		#region DockableFormsContainer protected properties
		
		//---------------------------------------------------------------------------
		protected EventHandlerList Events { get { return events; } }

		#endregion

		#region DockableFormsContainer private properties
		
		//---------------------------------------------------------------------------
		private System.Drawing.Color DockActiveCaptionColor
		{
			get { return dockActiveCaptionColor; } 
			set
			{
				if (dockActiveCaptionColor.Equals(value))
					return;

				dockActiveCaptionColor = value; 

				if (dockTabbedPanel != null)
					dockTabbedPanel.ActiveCaptionColor = dockActiveCaptionColor; 
			}
		}

		//---------------------------------------------------------------------------
		private System.Drawing.Color DockActiveCaptionTextColor
		{
			get { return dockActiveCaptionTextColor; } 
			set
			{
				if (dockActiveCaptionTextColor.Equals(value))
					return;

				dockActiveCaptionTextColor = value; 

				if (dockTabbedPanel != null)
					dockTabbedPanel.ActiveCaptionTextColor = dockActiveCaptionTextColor; 
			}
		}

		//---------------------------------------------------------------------------
		private System.Drawing.Color DockInactiveCaptionColor
		{
			get { return dockInactiveCaptionColor; } 
			set
			{
				if (dockInactiveCaptionColor.Equals(value))
					return;

				dockInactiveCaptionColor = value; 

				if (dockTabbedPanel != null)
					dockTabbedPanel.InactiveCaptionColor = dockInactiveCaptionColor; 
			}
		}
		
		//---------------------------------------------------------------------------
		private System.Drawing.Color DockInactiveCaptionTextColor
		{
			get { return dockInactiveCaptionTextColor; } 
			set
			{
				if (dockInactiveCaptionTextColor.Equals(value))
					return;

				dockInactiveCaptionTextColor = value; 

				if (dockTabbedPanel != null)
					dockTabbedPanel.InactiveCaptionTextColor = dockInactiveCaptionTextColor; 
			}
		}

		#endregion

		#region DockableFormsContainer internal properties

		//---------------------------------------------------------------------------
		internal TabbedPanel TabbedPanel { get { return GetTabbedPanel(DockState); } }

		//---------------------------------------------------------------------------
		internal DockableTabbedPanel DockTabbedPanel { get { return dockTabbedPanel; } }

		//---------------------------------------------------------------------------
		internal DockableDocumentPanel DocumentTabbedPanel { get { return documentTabbedPanel; } }

		//---------------------------------------------------------------------------
		internal DockableTabbedPanel FloatingTabbedPanel { get	{ return floatingTabbedPanel; } }

		//---------------------------------------------------------------------------
		internal Size FloatingFormDefaultSize { get { return (activeForm != null) ? activeForm.FloatingFormDefaultSize : FloatingForm.DefaultSize; } }
		
		#endregion

		#region DockableFormsContainer private methods

		//---------------------------------------------------------------------------
		private void Initialize(DockableForm aForm, DockState dockState, FloatingForm aFloatingForm, bool flagBounds, Rectangle floatingFormBounds)
		{
			if (aForm == null)
                throw new ArgumentNullException();

			if (dockState == DockState.Unknown || dockState == DockState.Hidden)
                throw new ArgumentException();

			if (aForm.DockManager == null)
                throw new ArgumentException();

			events = new EventHandlerList();
			forms = new DockableFormsCollection();

			dockManager = aForm.DockManager;
			showTabsAlways = dockManager.ShowTabsAlways;

			dockManager.AddContainer(this);

			if (documentTabbedPanel != null)
				documentTabbedPanel.BackColor= dockManager.BackColor; 
			if (dockTabbedPanel != null)
				dockTabbedPanel.BackColor= dockManager.BackColor; 

			if (aFloatingForm != null)
				FloatingForm = aFloatingForm;
			else if (flagBounds)
				FloatingForm = new FloatingForm(DockManager, this, floatingFormBounds);

			aForm.FormContainer = this;

			VisibleState = dockState;
			
			TabsFont = new System.Drawing.Font(dockManager.DefaultTabsFont, System.Drawing.FontStyle.Regular);
			TabsActiveTabTextColor = dockManager.DefaultActiveTabTextColor;
			TabsInactiveTabTextColor = dockManager.DefaultInactiveTabTextColor;

			this.DockActiveCaptionColor			= dockManager.DockPanelActiveCaptionColor;
			this.DockActiveCaptionTextColor		= dockManager.DockPanelActiveCaptionTextColor;
			this.DockInactiveCaptionColor		= dockManager.DockPanelInactiveCaptionColor;
			this.DockInactiveCaptionTextColor	= dockManager.DockPanelInactiveCaptionTextColor;
		}

		//---------------------------------------------------------------------------
		private TabbedPanel GetTabbedPanel(DockState state)
		{
			if (state == DockState.Document)
				return DocumentTabbedPanel;
			else if (state == DockState.Float)
				return FloatingTabbedPanel;
			else if (state != DockState.Unknown && state != DockState.Hidden)
				return DockTabbedPanel;
			else
				return null;
		}

		#endregion
		
		#region DockableFormsContainer public methods

		//---------------------------------------------------------------------------
		public void Activate()
		{
			IsActivated = true;
		}

		//---------------------------------------------------------------------------
		public void SetDockState()
		{
			DockState aDockStateToSet;

			if (this.Forms == null || this.Forms.Count == 0)
				aDockStateToSet = DockState.Unknown;
			else if (IsHidden)
				aDockStateToSet = DockState.Hidden;
			else
				aDockStateToSet = VisibleState;

			if (dockState == aDockStateToSet)
				return;

			if (DockManager.ActiveAutoHideForm == ActiveForm && !DockManager.IsDockStateAutoHide(aDockStateToSet))
			{
				bool oldAnimateAutoHide = DockManager.AnimateAutoHide;
				DockManager.AnimateAutoHide = false;
				DockManager.ActiveAutoHideForm = null;
				DockManager.AnimateAutoHide = oldAnimateAutoHide;
			}

			DockState oldDockState = dockState;

			dockState = aDockStateToSet;

			if (DockManager.ActiveDocumentsContainer == this && aDockStateToSet != DockState.Document)
				DockManager.SetActiveDocument(DockManager.GetContainer(DockState.Document));

			if (DockState == DockState.Float)
			{
				if (FloatingForm == null)
					floatingForm = new FloatingForm(DockManager, this, (Forms.Count == 1) ? Forms[0].Bounds : Rectangle.Empty);
			}
			else if (DockState == DockState.Document)
			{
				if (DocumentTabbedPanel == null)
					documentTabbedPanel = new DockableDocumentPanel(this);
				if (dockManager != null)
					documentTabbedPanel.BackColor= dockManager.BackColor; 
			}
			else if (DockState != DockState.Hidden && DockState != DockState.Unknown)
			{
				if (DockTabbedPanel == null)
					dockTabbedPanel = new DockableTabbedPanel(this);
				if (dockManager != null)
					dockTabbedPanel.BackColor= dockManager.BackColor; 
				
				DockTabbedPanel.AutoHide = DockManager.IsDockStateAutoHide(DockState);
				DockTabbedPanel.TabsFont = TabsFont;
				DockTabbedPanel.TabsActiveTabTextColor = TabsActiveTabTextColor;
				DockTabbedPanel.TabsInactiveTabTextColor = TabsInactiveTabTextColor;
				DockTabbedPanel.ActiveCaptionColor = DockActiveCaptionColor;
				DockTabbedPanel.ActiveCaptionTextColor = DockActiveCaptionTextColor;
				DockTabbedPanel.InactiveCaptionColor = DockInactiveCaptionColor;
				DockTabbedPanel.InactiveCaptionTextColor = DockInactiveCaptionTextColor;
			}

			// Temporarily make DockManager invisible to avoid screen flicks
			//bool dockManagerVisible = DockManager.Visible;
			//DockManager.Hide();
			//SetParent(null);

			if (oldDockState == DockState.Float)
				FloatingForm.RefreshContainers();
			else
				DockManager.RefreshContainers(oldDockState);

			if (DockState == DockState.Float)
				FloatingForm.RefreshContainers();
			else
				DockManager.RefreshContainers(DockState);

			//DockManager.Visible = dockManagerVisible;
			//Refresh();

			foreach (DockableForm aForm in Forms)
				aForm.SetDockState();

			if (DockManager.IsValidRestoreState(DockState))
				RestoreState = DockState;

			OnDockStateChanged(EventArgs.Empty);
		}

		//---------------------------------------------------------------------------
		public bool IsDockStateValid(DockState dockState)
		{
			foreach (DockableForm aForm in Forms)
				if (!aForm.IsDockStateValid(dockState))
					return false;

			return true;
		}

		//---------------------------------------------------------------------------
		public void AddForm(DockableForm aFormToAdd)
		{
			if (Forms.Contains(aFormToAdd))
				return;

			Forms.Add(aFormToAdd);

			SetDockState();
			
			if (!aFormToAdd.IsHidden && VisibleFormsCount == 1)
			{
				ActiveForm = aFormToAdd;
				Refresh();
			}
		}

		//---------------------------------------------------------------------------
		public void RemoveForm(DockableForm aFormToRemove)
		{
			if (!Forms.Contains(aFormToRemove))
				return;
			
			int index = GetVisibleFormIndex(aFormToRemove);

			Forms.Remove(aFormToRemove);
			if (TabbedPanel != null)
				if (TabbedPanel.Contains(aFormToRemove))
					aFormToRemove.SetParent(null);

			SetDockState();

			ValidateActiveForm();

			if (Forms.Count == 0)
				Close();

			if (index != -1)
				Refresh();
		}

		//---------------------------------------------------------------------------
		public void SetFormIndex(DockableForm aForm, int index)
		{
			int oldIndex = Forms.IndexOf(aForm);
			if (oldIndex == -1)
                throw new ArgumentException();

			if (index < 0 || index > Forms.Count - 1)
				if (index != -1)
                    throw new ArgumentOutOfRangeException();
				
			if (oldIndex == index)
				return;
			if (oldIndex == Forms.Count - 1 && index == -1)
				return;

			Forms.Remove(aForm);
			if (index == -1)
				Forms.Add(aForm);
			else if (oldIndex < index)
				Forms.Insert(index - 1, aForm);
			else
				Forms.Insert(index, aForm);

			Refresh();
		}

		//---------------------------------------------------------------------------
		public void CloseForm(DockableForm aFormToClose)
		{
			if (aFormToClose == null || aFormToClose.FormContainer != this)
				return;

			if (aFormToClose.HideOnClose)
				aFormToClose.Hide();
			else
				aFormToClose.Close();
		}

		//---------------------------------------------------------------------------
		public void CloseActiveForm()
		{
			CloseForm(ActiveForm);
		}

		//---------------------------------------------------------------------------
		public bool Contains(DockableFormsContainer aContainer)
		{
			if (aContainer.DockState != DockState ||
				DockState == DockState.Hidden || DockState == DockState.Unknown ||
				DockManager.IsDockStateAutoHide(DockState))
				return false;

			if (DockState == DockState.Float)
			{
				if (FloatingForm != aContainer.FloatingForm)
					return false;
				else
					return (FloatingForm.Containers.IndexOf(aContainer) > FloatingForm.Containers.IndexOf(this));
			}
			else
				return (DockManager.Containers.IndexOf(aContainer) > DockManager.Containers.IndexOf(this));
		}

		//---------------------------------------------------------------------------
		public int GetVisibleFormIndex(DockableForm container)
		{
			if (container == null)
				return -1;

			if (container.IsHidden)
				return -1;

			int index = -1;
			foreach (DockableForm aForm in Forms)
			{
				if (!aForm.IsHidden)
					index++;

				if (aForm == container)
					return index;
			}
			return -1;
		}

		//---------------------------------------------------------------------------
		public DockableForm GetVisibleForm(int index)
		{
			int currentIndex = -1;
			foreach (DockableForm aForm in Forms)
			{
				if (!aForm.IsHidden)
					currentIndex ++;

				if (currentIndex == index)
					return aForm;
			}
			throw(new ArgumentOutOfRangeException());
		}

		//---------------------------------------------------------------------------
		public void SetParent(Control control)
		{
			TabbedPanel aTabbedPanel = TabbedPanel;
			if (DockTabbedPanel != null)
				DockTabbedPanel.SetParent(null);

			if (DocumentTabbedPanel != null)
				DocumentTabbedPanel.SetParent(null);

			if (FloatingTabbedPanel != null)
				FloatingTabbedPanel.SetParent(null);

			if (aTabbedPanel != null)
				aTabbedPanel.SetParent(control);
		}

		//---------------------------------------------------------------------------
		public void Show()
		{
			IsHidden = false;
		}

		//---------------------------------------------------------------------------
		public void Hide()
		{
			IsHidden = true;
		}

		//---------------------------------------------------------------------------
		public void PerformLayout()
		{
			if (TabbedPanel != null)
				TabbedPanel.PerformLayout();
		}

		//---------------------------------------------------------------------------
		public void Refresh()
		{
			if (DockState == DockState.Unknown || DockState == DockState.Hidden)
				return;

			if (IsAutoHide)
				DockManager.Invalidate();

			if (TabbedPanel != null)
			{
				TabbedPanel.Invalidate();
				
				PerformLayout();

				if (TabbedPanel is DockableTabbedPanel)
					((DockableTabbedPanel)TabbedPanel).OnAutoHide(DockManager.ActiveAutoHideForm == ActiveForm);
			}
		}

		//---------------------------------------------------------------------------
		public void Close()
		{
			Dispose();
		}

		#endregion
		
		#region DockableFormsContainer internal methods

		//---------------------------------------------------------------------------
		internal void ValidateActiveForm()
		{
			if (ActiveForm == null)
				return;

			if (GetVisibleFormIndex(ActiveForm) >= 0)
				return;

			DockableForm prevVisible = null;
			for (int i = Forms.IndexOf(ActiveForm) - 1; i >= 0; i--)
				if (!Forms[i].IsHidden)
				{
					prevVisible = Forms[i];
					break;
				}

			DockableForm nextVisible = null;
			for (int i = Forms.IndexOf(ActiveForm) + 1; i < Forms.Count; i++)
				if (!Forms[i].IsHidden)
				{
					nextVisible = Forms[i];
					break;
				}

			if (prevVisible != null)
				ActiveForm = prevVisible;
			else if (nextVisible != null)
				ActiveForm = nextVisible;
			else
				ActiveForm = null;
		}

		//---------------------------------------------------------------------------
		internal void CloseAllForms()
		{
			if (Forms != null && Forms.Count > 0)
			{
				for (int i = Forms.Count -1; i >= 0; i--)
				{
					DockableForm formToClose = Forms[i]; 
					if (formToClose == null)
						continue;

					RemoveForm(formToClose);
					formToClose.Close();
				}
			}
		}
		
		#endregion
		
		#region DockableFormsContainer events
		
		private static readonly object DockStateChangedEvent = new object();
		public event EventHandler DockStateChanged
		{
			add	{	Events.AddHandler(DockStateChangedEvent, value);	}
			remove	{	Events.RemoveHandler(DockStateChangedEvent, value);	}
		}
		protected virtual void OnDockStateChanged(EventArgs e)
		{
			EventHandler handler = (EventHandler)Events[DockStateChangedEvent];
			if (handler != null)
				handler(this, e);
		}
		
		#endregion
		
	}

	#region DockableFormsContainersCollection Class
	//==============================================================================
	/// <summary>
	/// Summary description for DockableFormsCollection.
	/// </summary>
	public class DockableFormsContainersCollection : ReadOnlyCollectionBase
	{
		//---------------------------------------------------------------------------
		internal DockableFormsContainersCollection()
		{
		}

		//---------------------------------------------------------------------------
		public DockableFormsContainer this[int index]
		{
			get {  return InnerList[index] as DockableFormsContainer;  }
		}

		//---------------------------------------------------------------------------
		internal void Dispose()
		{
			for (int i = Count - 1; i >= 0; i--)
				this[i].Close();
		}

		//---------------------------------------------------------------------------
		internal int Add(DockableFormsContainer aContainerToAdd)
		{
			if (InnerList.Contains(aContainerToAdd))
				return InnerList.IndexOf(aContainerToAdd);

			return InnerList.Add(aContainerToAdd);
		}

		//---------------------------------------------------------------------------
		internal void Insert(int index, DockableFormsContainer aContainerToInsert)
		{
			if (index < 0 || index > InnerList.Count - 1)
				return;
			
			if (Contains(aContainerToInsert))
				return;

			InnerList.Insert(index, aContainerToInsert);
		}

		//---------------------------------------------------------------------------
		internal void Add(DockableFormsContainer beforeContainer, DockableFormsContainer aContainerToAdd)
		{
			if (beforeContainer == null)
				Add(aContainerToAdd);

			if (!Contains(beforeContainer))
				return;

			if (Contains(aContainerToAdd))
				return;

			InnerList.Insert(IndexOf(beforeContainer), aContainerToAdd);
		}

		//---------------------------------------------------------------------------
		public bool Contains(DockableFormsContainer aContainerToSearch)
		{
			return InnerList.Contains(aContainerToSearch);
		}

		//---------------------------------------------------------------------------
		public int IndexOf(DockableFormsContainer aContainerToSearch)
		{
			return InnerList.IndexOf(aContainerToSearch);
		}

		//---------------------------------------------------------------------------
		internal void Remove(DockableFormsContainer aContainerToRemove)
		{
			InnerList.Remove(aContainerToRemove);
		}

		//---------------------------------------------------------------------------
		internal void RemoveAll()
		{
			InnerList.Clear();
		}
	}

	#endregion

}
