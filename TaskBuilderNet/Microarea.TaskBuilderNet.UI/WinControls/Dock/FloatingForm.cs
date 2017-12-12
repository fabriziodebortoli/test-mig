using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.UI.WinControls.Dock.Win32;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock
{
	//===========================================================================
	// FloatingForm class works as container of DockableFormsContainer class.
	// It maintains a collection of DockableFormsContainer instances through its 
	// Containers property.
	// All FloatingForm instances are contained in DockManager instance. 
	// The DockManager.FloatingForms property and FloatingForm.DockManager property
	// specify the parent-child relationship between DockManager instance and 
	// FloatingForm instance.
	//===========================================================================
	
	/// <summary>
	/// Descripiton of class FloatingForm
	/// </summary>
	public partial class FloatingForm : Form
	{
		#region FloatingForm private fields
		
		private DockableFormsContainersCollection	containers = null;
		private bool								redockingAllowed = true;
		private DockManager							dockManager;

		#endregion

		#region FloatingForm constructors
		
		//---------------------------------------------------------------------------
		public FloatingForm(DockManager aDockManager, DockableFormsContainer aContainer, Rectangle bounds)
		{
			Initialize(aDockManager, aContainer, bounds);
		}

		//---------------------------------------------------------------------------
		public FloatingForm(DockManager aDockManager, DockableFormsContainer aContainer) : this(aDockManager, aContainer, Rectangle.Empty)
		{
		}

		//---------------------------------------------------------------------------
		public FloatingForm(DockManager aDockManager, Rectangle bounds) : this(aDockManager, null, bounds)
		{
		}

		//---------------------------------------------------------------------------
		public FloatingForm(DockManager aDockManager, DockableFormsContainer aContainer, Point location)
		{
			Rectangle bounds = new Rectangle(location, DefaultSize);
			Initialize(aDockManager, aContainer, bounds);
		}

		#endregion

		#region FloatingForm private properties
		
		//---------------------------------------------------------------------------
		private int VisibleContainersCount
		{
			get
			{
				int count = 0;
				foreach (DockableFormsContainer cw in Containers)
					if (cw.DockState == DockState.Float)
						count ++;

				return count;
			}
		}

		#endregion
		
		#region FloatingForm internal properties
		
		//---------------------------------------------------------------------------
		internal Control DummyControl { get { 	return dummyControl; } }

		#endregion
		
		#region FloatingForm public properties
		
		//---------------------------------------------------------------------------
		public static new Size DefaultSize { get { return new Size(300, 300); } }

		//---------------------------------------------------------------------------
		public DockableFormsContainersCollection Containers { get { return containers; } }
				
		//---------------------------------------------------------------------------
		public bool IsRedockingAllowed { get { return redockingAllowed; } set { redockingAllowed = value; } }

		//---------------------------------------------------------------------------
		public DockManager DockManager { get { return dockManager; } }

		#endregion

		#region FloatingForm private methods

		//---------------------------------------------------------------------------
		private void Initialize(DockManager aDockManager, DockableFormsContainer aContainer, Rectangle bounds)
		{
			if (aDockManager == null)
                throw new ArgumentNullException();

			containers = new DockableFormsContainersCollection();

			FormBorderStyle = FormBorderStyle.SizableToolWindow;
			ShowInTaskbar = false;

			if (bounds != Rectangle.Empty)
			{
				StartPosition = FormStartPosition.Manual;
				Bounds = bounds;
			}
			else
				StartPosition = FormStartPosition.WindowsDefaultLocation;

			dummyControl = new Control();
			dummyControl.Bounds = Rectangle.Empty;

			Controls.Add(dummyControl);

			dockManager = aDockManager;
			Owner = DockManager.FindForm();
			DockManager.AddFloatingForm(this);
			
			if (aContainer != null)
				aContainer.FloatingForm = this;
		}

		#endregion
		
		#region FloatingForm public methods

		//---------------------------------------------------------------------------
		public void SetText()
		{
			DockableFormsContainer firstFloatingContainer = null;
			int floatingContainersCount = 0;
			
			foreach (DockableFormsContainer aContainer in Containers)
			{
				if (aContainer.DockState != DockState.Float)
					continue;

				if (firstFloatingContainer == null)
					firstFloatingContainer = aContainer;

				floatingContainersCount ++;
			}

			if (floatingContainersCount == 1)
				Text = (firstFloatingContainer.ActiveForm != null) ? firstFloatingContainer.ActiveForm.Text : String.Empty;
			else
				Text = String.Empty;
		}

		//---------------------------------------------------------------------------
		public void RefreshContainers()
		{
			if (VisibleContainersCount == 0)
			{
				if (Owner != null)
					Owner.Activate();
				Hide();
				return;
			}

			DockableFormsContainer first = null;
			DockableFormsContainer prev = null;
			foreach (DockableFormsContainer aContainer in Containers)
			{
				if (aContainer.DockState != DockState.Float)
				{
					if (aContainer.FloatingTabbedPanel != null)
						aContainer.FloatingTabbedPanel.SetParent(null);
					continue;
				}

				if (first == null)
				{
					first = aContainer;
					aContainer.SetParent(this);
					aContainer.TabbedPanel.Dock = DockStyle.Fill;
				}
				else
				{
					aContainer.SetParent(prev.TabbedPanel);
					aContainer.TabbedPanel.Dock = DockStyle.None;
				}
				((DockableTabbedPanel)aContainer.TabbedPanel).HasCaption = (VisibleContainersCount > 1);
				aContainer.Refresh();
				prev = aContainer;
			}

			SetText();
			Show();
		}

		//---------------------------------------------------------------------------
		public void AddContainer(DockableFormsContainer aContainerToAdd)
		{
			containers.Add(aContainerToAdd);
			RefreshContainers();
		}

		//---------------------------------------------------------------------------
		public void RemoveContainer(DockableFormsContainer aContainerToRemove)
		{
			containers.Remove(aContainerToRemove);
			if (Contains(aContainerToRemove.TabbedPanel))
				aContainerToRemove.SetParent(null);

			if (Containers.Count == 0)
			{
				if (Owner != null)
					Owner.Activate();
				Close();
			}
			else
				RefreshContainers();
		}

		//---------------------------------------------------------------------------
		public bool IsDockStateValid(DockState dockState)
		{
			foreach (DockableFormsContainer container in Containers)
				foreach (DockableForm form in container.Forms)
					if (!DockManager.IsDockStateValid(dockState, form.AllowedStates))
						return false;

			return true;
		}

		//---------------------------------------------------------------------------
		public void SetContainerIndex(DockableFormsContainer aContainer, int index)
		{
			int oldIndex = Containers.IndexOf(aContainer);
			if (oldIndex == -1)
                throw new ArgumentException();

			if (index < 0 || index > Containers.Count - 1)
				if (index != -1)
                    throw new ArgumentOutOfRangeException();
				
			if (oldIndex == index)
				return;
			if (oldIndex == Containers.Count - 1 && index == -1)
				return;

			Containers.Remove(aContainer);
			if (index == -1)
				Containers.Add(aContainer);
			else if (oldIndex < index)
				Containers.Insert(index - 1, aContainer);
			else
				Containers.Insert(index, aContainer);

			RefreshContainers();
		}

		#endregion
		
		#region FloatingForm protected overridden methods
		
		//---------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == (int)Win32.Msgs.WM_NCLBUTTONDOWN)
			{
				if (IsDisposed)
					return;

				uint result = User32.SendMessage(this.Handle, (int)Win32.Msgs.WM_NCHITTEST, 0, (uint)m.LParam);
				if (result == 2 && DockManager.IsRedockingAllowed && this.redockingAllowed)	// HITTEST_CAPTION
				{
					foreach(DockableFormsContainer aDockContainer in DockManager.Containers)
						aDockContainer.IsActivated = false;

					Activate();

					dockManager.BeginDragFloatingForm(this);
				}
				else
					base.WndProc(ref m);

				return;
			}
			else if (m.Msg == (int)Win32.Msgs.WM_CLOSE)
			{
				if (Containers.Count == 0)
				{
					base.WndProc(ref m);
					return;
				}
				
				for (int i = Containers.Count - 1; i >= 0; i--)
				{
					if (Containers[i].DockState != DockState.Float)
						continue;

					DockableFormsCollection forms = Containers[i].Forms;
					for (int j = forms.Count - 1; j >= 0; j--)
					{
						DockableForm aForm = forms[j];
						if (aForm.HideOnClose)
							aForm.Hide();
						else
							aForm.Close();
					}
				}
				
				return;
			}
			else if (m.Msg == (int)Win32.Msgs.WM_NCLBUTTONDBLCLK)
			{
				uint result = User32.SendMessage(this.Handle, (int)Win32.Msgs.WM_NCHITTEST, 0, (uint)m.LParam);
				if (result != 2)	// HITTEST_CAPTION
				{
					base.WndProc(ref m);
					return;
				}
				
				// Show as RestoreState
				foreach(DockableFormsContainer aContainer in Containers)
				{
					if (aContainer.DockState != DockState.Float)
						continue;

					if (aContainer.RestoreState != DockState.Unknown)
					{
						aContainer.VisibleState = aContainer.RestoreState;
						aContainer.Activate();
					}
				}

				return;
			}

			base.WndProc(ref m);
		}

		#endregion

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FloatingForm));
            this.SuspendLayout();
            // 
            // FloatingForm
            // 
            resources.ApplyResources(this, "$this");
            this.Name = "FloatingForm";
            this.ResumeLayout(false);

        }
		
	}

	#region FloatingFormsCollection Class
		
	//===========================================================================
	public class FloatingFormsCollection : ReadOnlyCollectionBase
	{
		//---------------------------------------------------------------------------
		public FloatingFormsCollection()
		{
		}

		//---------------------------------------------------------------------------
		public FloatingForm this[int index]
		{
			get {  return InnerList[index] as FloatingForm;  }
		}

		//---------------------------------------------------------------------------
		public int Add(FloatingForm fw)
		{
			if (InnerList.Contains(fw))
				return InnerList.IndexOf(fw);

			return InnerList.Add(fw);
		}

		//---------------------------------------------------------------------------
		public bool Contains(FloatingForm fw)
		{
			return InnerList.Contains(fw);
		}

		//---------------------------------------------------------------------------
		public void Dispose()
		{
			for (int i=Count - 1; i>=0; i--)
				this[i].Close();
		}

		//---------------------------------------------------------------------------
		public int IndexOf(FloatingForm fw)
		{
			return InnerList.IndexOf(fw);
		}

		//---------------------------------------------------------------------------
		public void Remove(FloatingForm fw)
		{
			InnerList.Remove(fw);
		}
	}

	#endregion
}
