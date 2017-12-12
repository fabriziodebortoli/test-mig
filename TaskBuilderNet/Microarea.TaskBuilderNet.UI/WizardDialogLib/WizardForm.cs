using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WizardDialogLib
{
	/// <summary>
	/// WizardForm
	/// </summary>
	//=========================================================================
    public partial class WizardForm : System.Windows.Forms.Form, IWizardForm
	{
		public const string NextPage = "";
		public const string NoPageChange = null;

        private List<WizardPage> pages = null;

        private int selectedIndex = -1;
		
        private WizardManager wizardManager = null;
	
		private static Font defaultPageFont = new Font("Verdana", 9.75F, FontStyle.Regular, GraphicsUnit.Point);

		#region Constructors
		//--------------------------------------------------------------------
		public WizardForm(WizardManager aWizardManager)
		{
			InitializeComponent();

            FinishWizardButton.Location = NextPageButton.Location;
			wizardManager = aWizardManager;
		}

		//--------------------------------------------------------------------
		public WizardForm() : this(null)
		{
		}
		#endregion

		#region public properties
		//--------------------------------------------------------------------
		public WizardManager WizardManager { get { return wizardManager; } }

		//--------------------------------------------------------------------
		[Browsable(false), ReadOnly(true), Localizable(true)]  
		public static System.Drawing.Font DefaultPageFont { get { return defaultPageFont; } }

		///<summary>
		/// ritorna il nome della pagina chiamante la pagina corrente
		/// nel caso in cui volessi "discriminare" determinati comportamenti nella pagina
		///</summary>
		//--------------------------------------------------------------------
		public string PreviousPageName
		{
			get 
			{
				if (selectedIndex == -1)
					return string.Empty;
				return this.pages[selectedIndex].Name; 
			}
		}
		#endregion

		//--------------------------------------------------------------------
		public void SetFinishButtonText(string text)
		{
			// Set the Finish button text
			FinishWizardButton.Text = text;
        }

		//--------------------------------------------------------------------
		public void SetCancelButtonText(string text)
		{
			// Set the Cancel button text
			CancelWizardButton.Text = text;
		}

        #region IWizardForm implemented members...
        //--------------------------------------------------------------------
        WizardManager IWizardForm.WizardManager { get { return wizardManager; } set { wizardManager = value; } }
        //--------------------------------------------------------------------
        List<WizardPage> IWizardForm.Pages { get { return pages; } }
        //--------------------------------------------------------------------
        string IWizardForm.NextPage { get { return WizardForm.NextPage; } }
        //--------------------------------------------------------------------
        string IWizardForm.NoPageChange { get { return WizardForm.NoPageChange; } }
        //--------------------------------------------------------------------
        System.Drawing.Font IWizardForm.DefaultPageFont { get { return WizardForm.DefaultPageFont; } }
        //--------------------------------------------------------------------
        //Height of the button bar at the bottom of the form, considering also an offset.
        //--------------------------------------------------------------------
        int IWizardForm.ButtonsBarHeight { get { return Size.Height - PageSeparator.Location.Y; } }
        //--------------------------------------------------------------------
        void IWizardForm.SetWizardButtons(WizardButton flags)
        {
            SetWizardButtons(flags);
        }

        public event PageActivationEventHandler PageActivating;
        //---------------------------------------------------------------------------
        event PageActivationEventHandler IWizardForm.PageActivating
        {
            add { PageActivating += value; }
            remove { PageActivating -= value; }
        }

        public event PageActivationEventHandler PageActivated;
        //---------------------------------------------------------------------------
        event PageActivationEventHandler IWizardForm.PageActivated
        {
            add { PageActivated += value; }
            remove { PageActivated -= value; }
        }
        #endregion //IWizardForm implemented members...

        //---------------------------------------------------------------------------
        public void SetWizardButtons(WizardButton flags)
		{
            // Enable/disable and show/hide buttons appropriately
			if ((flags & WizardButton.DisableAll) == WizardButton.DisableAll)
			{
				PreviousPageButton.Enabled = false;
				NextPageButton.Enabled = false;
				FinishWizardButton.Enabled = false;
			}

			PreviousPageButton.Enabled = (flags & WizardButton.Back) == WizardButton.Back;
			NextPageButton.Enabled = (flags & WizardButton.Next) == WizardButton.Next;
			NextPageButton.Visible = (flags & WizardButton.Finish) == 0 && (flags & WizardButton.DisabledFinish) == 0;
			FinishWizardButton.Enabled = (flags & WizardButton.DisabledFinish) == 0;
			FinishWizardButton.Visible = (flags & WizardButton.Finish) == WizardButton.Finish ||
										(flags & WizardButton.DisabledFinish) == WizardButton.DisabledFinish;

			// Set the AcceptButton depending on whether or not the Finish
			// button is visible or not
			AcceptButton = FinishWizardButton.Visible ? FinishWizardButton : NextPageButton;
		}

        //--------------------------------------------------------------------
		public void SetCurrentCursor(Cursor cursor)
		{
			this.Cursor = cursor;
		}

		//--------------------------------------------------------------------
		public void SetDefaultCursor()
		{
			this.Cursor = Cursors.Default;
		}

        //--------------------------------------------------------------------
        int GetIndexByName(string pageName)
        {
            if (pages == null || pages.Count == 0)
                return -1;

            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i].Name == pageName)
                    return i;
            }

            return -1;
        }

        //--------------------------------------------------------------------
        public void ActivatePage(string pageName)
        {
            ActivatePage(GetIndexByName(pageName));
        }

		//--------------------------------------------------------------------
		private void ActivatePage(int newIndex)
		{
			// Ensure the index is valid
			if (newIndex < 0 || newIndex >= pages.Count)
				throw new ArgumentOutOfRangeException();

			// Deactivate the current page if applicable
			WizardPage currentPage = null;
			if (selectedIndex != -1)
			{
				if (selectedIndex == newIndex)
					return;

				currentPage = (WizardPage)pages[selectedIndex];
				if (!currentPage.OnKillActive())
					return;
			}

			// Activate the new page
			WizardPage newPage = (WizardPage)pages[newIndex];
            if (newPage == null)
                return;
	
			if (PageActivating != null)
				PageActivating(this, newIndex, newPage);
			
			if (!newPage.OnSetActive())
				return;

			if (newPage.ToSkip)
			{
				int goTo = (newIndex > selectedIndex) ? GetNextIndex(newIndex) : GetPreviousIndex(newIndex);
				if (goTo != -1)
					ActivatePage(goTo);
				return;
			}

			// Update state
			selectedIndex = newIndex;
			if (currentPage != null)
				currentPage.Visible = false;
			
            newPage.Visible = true;
			newPage.Focus();

			if (PageActivated != null)
				PageActivated(this, newIndex, newPage);
		}

		//--------------------------------------------------------------------
		private void OnClickBack(object sender, System.EventArgs e)
		{
			if (PreviousPageButton.Enabled)
			{
				// Ensure a page is currently selected
				if (selectedIndex == -1)
					return;
				int i = GetPreviousIndex(selectedIndex);
				if (i == -1) 
					return;
				ActivatePage(i);
			}
		}

		//--------------------------------------------------------------------
		private void OnClickNext(object sender, System.EventArgs e)
		{
			if (this.NextPageButton.Enabled)
			{
				// Ensure a page is currently selected
				if (selectedIndex == -1)
					return;
				int i = GetNextIndex(selectedIndex);
				if (i == -1) 
					return;
				ActivatePage(i);
			}
		}

		//--------------------------------------------------------------------
		private int GetNextIndex(int currentIndex)
		{
			// Inform selected page that the Next button was clicked
			string pageName = ((WizardPage)pages[currentIndex]).OnWizardNext();
		
			switch(pageName)
			{
					// Do nothing
				case NoPageChange:
					return -1;

					// Activate the next appropriate page
				case NextPage:
					if (currentIndex + 1 < pages.Count)
						return (currentIndex + 1);
					break;

					// Activate the specified page if it exists
				default:
					foreach(WizardPage page in pages)
					{
						if (page.Name == pageName)
							return (pages.IndexOf(page));
					}
					break;
			}
			return -1;
		}

		//--------------------------------------------------------------------
		private int GetPreviousIndex(int currentIndex)
		{
			// Inform selected page that the Back button was clicked
			string pageName = ((WizardPage)pages[currentIndex]).OnWizardBack();

			switch(pageName)
			{
					// Do nothing
				case NoPageChange:
					return -1;
                        
					// Activate the next appropriate page
				case NextPage:
					if(selectedIndex - 1 >= 0)
						return(currentIndex - 1);
					break;

					// Activate the specified page if it exists
				default:
					foreach (WizardPage page in pages)
					{
						if (page.Name == pageName)
							return (pages.IndexOf(page));
					}
					break;
			}
			return -1;
		}

		//--------------------------------------------------------------------
		private void OnClickCancel(object sender, System.EventArgs e)
		{
			// Close wizard
			if (this.CancelWizardButton.Enabled)
				DialogResult = DialogResult.Cancel;
		}

		//--------------------------------------------------------------------
		private void OnClickFinish(object sender, System.EventArgs e)
		{
			if (this.FinishWizardButton.Enabled)
			{
				// Ensure a page is currently selected
				if (selectedIndex != -1)
				{
					// Inform selected page that the Finish button was clicked
					WizardPage page = (WizardPage)pages[selectedIndex];
				
					if (page.OnWizardFinish())
					{
						// Deactivate page and close wizard
						if (page.OnKillActive())
							DialogResult = DialogResult.OK;
					}
				}
			}
		}

		//--------------------------------------------------------------------
		protected override void OnControlAdded(ControlEventArgs e)
		{
			// Invoke base class implementation
			base.OnControlAdded(e);
            
			// Set default properties for all WizardPage instances added to this form
			WizardPage page = e.Control as WizardPage;
			
			if (page != null)
			{
				page.Visible = false;
				page.Location = new Point(0, 0);
				page.Size = new Size(Width, PageSeparator.Location.Y);

                if (pages == null)
                    pages = new List<WizardPage>();
                
                pages.Add(page);
			}
		}

		//--------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			// Invoke base class implementation
			base.OnLoad(e);
            
			// Activate the first page in the wizard
            if (pages != null && pages.Count > 0)
				ActivatePage(0);
		}

		//--------------------------------------------------------------------
		private void OnClickHelp(object sender, System.EventArgs e)
		{
			if (this.PageHelpButton.Enabled)
			{
				// Ensure a page is currently selected
				if (selectedIndex != -1)
				{
					// Inform selected page that the Finish button was clicked
					WizardPage page = (WizardPage)pages[selectedIndex];
					if (page.OnWizardHelp())
						DialogResult = DialogResult.None;
				}
			}
		}
	}
}