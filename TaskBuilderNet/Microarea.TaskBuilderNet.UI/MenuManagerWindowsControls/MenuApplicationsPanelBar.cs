using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;


namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	public delegate void PanelStateChangedEventHandler(object sender, MenuApplicationPanelEventArgs e);

	#region MenuApplicationsPanelBar class

	/// <summary>
	/// An ExplorerBar-type extended Panel for containing MenuApplicationPanel objects.
	/// </summary>
	//============================================================================
	public partial class MenuApplicationsPanelBar : System.Windows.Forms.Panel, System.ComponentModel.ISupportInitialize
	{
		private MenuApplicationPanelCollection panels = new MenuApplicationPanelCollection();
		private MenuApplicationPanel currentSelectedApplicationPanel = null;
		private int ySpacing = 8;
		private int xSpacing = 8;
		private bool initialising = false;

		public event MenuMngCtrlEventHandler SelectedApplicationChanged;
		public event MenuMngCtrlEventHandler SelectedGroupChanging;
		public event MenuMngCtrlEventHandler SelectedGroupChanged;

        private Image brandedMenuLogoImage = null;
		
		//---------------------------------------------------------------------------
		public MenuApplicationsPanelBar()
		{
			InitializeComponent();

			BackColor = Color.CornflowerBlue;

			Stream brandedMenuLogoImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.MenuLogo.png");
			if (brandedMenuLogoImageStream != null)
				this.BrandedMenuLogoImage = Image.FromStream(brandedMenuLogoImageStream);
		}

		#region MenuApplicationsPanelBar public properties

        //---------------------------------------------------------------------------
        public Image BrandedMenuLogoImage
        {
            get { return brandedMenuLogoImage; }
            set
            {
                brandedMenuLogoImage = value;

                this.BrandedMenuLogoPictureBox.Image = brandedMenuLogoImage;

                if (brandedMenuLogoImage != null)
                {
                    this.BrandedMenuLogoPictureBox.Size = brandedMenuLogoImage.Size;
                    this.BrandedMenuLogoPictureBox.Location = new Point(this.Width - this.BrandedMenuLogoPictureBox.Width, this.Height - this.BrandedMenuLogoPictureBox.Height);
                    this.BrandedMenuLogoPictureBox.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                    this.BrandedMenuLogoPictureBox.SendToBack();
                }

            }
        }

		//---------------------------------------------------------------------------
		public MenuApplicationPanelCollection Panels
		{
			get
			{
				return panels;
			}
		}

		//---------------------------------------------------------------------------
		public MenuApplicationPanel SelectedApplicationPanel
		{
			get
			{
				return currentSelectedApplicationPanel;
			}
		}

		//---------------------------------------------------------------------------
		public int YSpacing
		{
			get
			{
				return ySpacing;
			}
			set
			{
				ySpacing = value;
				UpdateAllPanelsPositions();
			}
		}

		/// <summary>
		/// Gets/sets the vertical spacing between adjacent panels.
		/// </summary>
		//---------------------------------------------------------------------------
		public int XSpacing
		{
			get
			{
				return xSpacing;
			}
			set
			{
				xSpacing = value;
				UpdateAllPanelsPositions();
			}
		}

		#endregion

		/// <summary>
		/// Signals the object that initialization is starting.
		/// </summary>
		//---------------------------------------------------------------------------
		public void BeginInit()
		{
			initialising = true;
		}

		/// <summary>
		/// Signals the object that initialization is complete.
		/// </summary>
		//---------------------------------------------------------------------------
		public void EndInit()
		{
			initialising = false;
		}
		
		//---------------------------------------------------------------------------
        public void ClearControls()
        {
            if (this.Controls != null && this.Controls.Count > 0)
            {
                for (int controlIdx = this.Controls.Count - 1; controlIdx >= 0; controlIdx--)
                {
                    if (this.Controls[controlIdx]== this.BrandedMenuLogoPictureBox)
                        continue;

                    this.Controls.RemoveAt(controlIdx);
                }
            }
        }

		//---------------------------------------------------------------------------
		public void UpdateAllPanelsPositions()
		{
			if (panels == null || panels.Count < 1)
				return;

			UpdatePositions(panels.Count - 1);

            this.BrandedMenuLogoPictureBox.SendToBack();
		}

		//---------------------------------------------------------------------------
		private void UpdatePositions(int index)
		{
			if (initialising || this.DisplayRectangle.Width == 0 || panels == null || index < 0 || index >= panels.Count)
				return;

			for (int i = index; i >= 0; i--)
			{
				int panelTop = 0;
				// Update the panel locations.
				if(i == panels.Count - 1)
				{
					// Top panel.
					panelTop = this.DisplayRectangle.Top + ySpacing;
				}
				else
				{
					panelTop = panels[i+1].Bottom + ySpacing;
				}

				panels[i].Location = new Point(xSpacing, panelTop);
				panels[i].Size =  new Size (this.DisplayRectangle.Width - (2 * xSpacing), panels[i].Height);
			}

			if (this.Visible)
				this.UpdateBounds();

			this.PerformLayout();
		}

		//---------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			//AutoScrollPosition = new System.Drawing.Point(0,0);

			UpdateAllPanelsPositions();
		}

		/// <summary>
		/// Event handler for the <see cref="Control.ControlAdded">ControlAdded</see> event.
		/// </summary>
		/// <param name="e">A <see cref="System.Windows.Forms.ControlEventArgs">ControlEventArgs</see> that contains the event data.</param>
		//---------------------------------------------------------------------------
		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);

			if(e.Control is MenuApplicationPanel)
			{
				// Adjust the docking property to Left | Right | Top
				e.Control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
				((MenuApplicationPanel)e.Control).PanelStateChanged +=	new PanelStateChangedEventHandler(MenuApplicationsPanel_PanelStateChanged);
				((MenuApplicationPanel)e.Control).SelectedApplicationChanged += new MenuMngCtrlEventHandler(MenuApplicationsPanel_SelectedApplicationChanged);
				((MenuApplicationPanel)e.Control).SelectedGroupChanging += new MenuMngCtrlEventHandler(MenuApplicationsPanel_SelectedGroupChanging);
				((MenuApplicationPanel)e.Control).SelectedGroupChanged += new MenuMngCtrlEventHandler(MenuApplicationsPanel_SelectedGroupChanged);

				if(initialising)
				{
					// In the middle of InitializeComponent call.
					// Generated code adds panels in reverse order, so add to end
					panels.Add((MenuApplicationPanel)e.Control);
				}
				else
				{
					// Add the panel to the beginning of the internal collection.
					panels.Insert(0, (MenuApplicationPanel)e.Control);
				}
				// Update the size and position of the panels
				UpdateAllPanelsPositions();
			}
		}

		/// <summary>
		/// Event handler for the ControlRemoved event.
		/// </summary>
		/// <param name="e">A ControlEventArgs that contains the event data.</param>
		//---------------------------------------------------------------------------
		protected override void OnControlRemoved(ControlEventArgs e)
		{
			base.OnControlRemoved(e);

			if(e.Control is MenuApplicationPanel)
			{
				// Get the index of the panel within the collection.
				int index = panels.IndexOf((MenuApplicationPanel)e.Control);
				if (index != -1)
				{
					// Remove this panel from the collection.
					panels.Remove(index);
					// Update the position of any remaining panels.
					UpdateAllPanelsPositions();
				}
			}
		}

		//---------------------------------------------------------------------------
		private void MenuApplicationsPanel_PanelStateChanged(object sender, MenuApplicationPanelEventArgs e)
		{
			if (panels == null)
				return;

			// Get the index of the control that just changed state.
			int index = panels.IndexOf(e.MenuApplicationPanel);
			if (index > 0 && index < panels.Count)
			{
				// Now update the position of all subsequent panels.
				UpdatePositions(--index);
			}
		}
		
		//---------------------------------------------------------------------------
		private void MenuApplicationsPanel_SelectedApplicationChanged(object sender, MenuMngCtrlEventArgs e)
		{
			currentSelectedApplicationPanel = (sender != null && (sender is MenuApplicationPanel)) ? (MenuApplicationPanel)sender : null;
			
			if (SelectedApplicationChanged != null)
				SelectedApplicationChanged(this, e);
		}

		//---------------------------------------------------------------------------
		private void MenuApplicationsPanel_SelectedGroupChanging(object sender, MenuMngCtrlEventArgs e)
		{
			if (SelectedGroupChanging != null)
				SelectedGroupChanging(this, e);
		}

		//---------------------------------------------------------------------------
		private void MenuApplicationsPanel_SelectedGroupChanged(object sender, MenuMngCtrlEventArgs e)
		{
			if (SelectedGroupChanged != null)
				SelectedGroupChanged(this, e);
		}

	}

	#endregion
}
