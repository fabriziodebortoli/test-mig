using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.WizardDialogLib
{
	/// <summary>
	/// Contenitore della form e relative pagine del wizard
	/// </summary>
	//=========================================================================
	public class WizardManager
	{
        private List<WizardPage> pages = null;
        private Form wizardForm;

		protected string wizardTitle = String.Empty;
		protected Icon wizardFormIcon = null;

		// per sapere dall'esterno se l'utente ha cliccato sul pulsante Fine del wizard
		// può essere utile se si desidera in uscita agganciare delle nuove finestre evitando
		// l'istanziazione di un thread separato
		protected bool finishButtonClicked = false; 

		public event WizardDialogLib.PageActivationEventHandler PageActivating;
		public event WizardDialogLib.PageActivationEventHandler PageActivated;
	
		//Evento per richimare l'help della pagina del wizard
		public delegate void HelpFromWizardPageEventHandler(object sender, string nameSpace, string searchParameter);
		public event HelpFromWizardPageEventHandler OnHelpFromWizardPage = null;
		
		public event EventHandler Finished;
		public event EventHandler Cancelled;

        //--------------------------------------------------------------------
        public WizardManager(Type wizardFormType)
        {
            if 
                (
                wizardFormType == null || 
                !wizardFormType.IsSubclassOf(typeof(Form)) ||
                wizardFormType.GetInterface(typeof(IWizardForm).FullName) == null
                )
                throw new Exception("Invalid Wizard Form Type.");

            wizardForm = wizardFormType.Assembly.CreateInstance(wizardFormType.FullName) as Form;
            IWizardForm wizardFormInterface = wizardForm as IWizardForm;
            wizardFormInterface.WizardManager = this;

            ComposeWizard();
        }

        //--------------------------------------------------------------------
        public WizardManager() : this(typeof(WizardForm))
        {
        }

        //--------------------------------------------------------------------
        public Form WizardForm { get { return wizardForm; } }
		//--------------------------------------------------------------------
		public string FormTitle { get { return wizardTitle; } set { wizardTitle = value; } }
		//--------------------------------------------------------------------
		public bool FinishButtonClicked { get { return finishButtonClicked; } }
		//--------------------------------------------------------------------
		public Icon FormIcon { get { return wizardFormIcon; } set { wizardFormIcon = value; } }
		//--------------------------------------------------------------------
		public bool ShowInTaskbar { get { return wizardForm.ShowInTaskbar; } set { wizardForm.ShowInTaskbar = value; } }

		//--------------------------------------------------------------------
		protected virtual void ComposeWizard()
		{}
		
		//--------------------------------------------------------------------
		public void AddWizardPage(WizardPage aWizardPage)
		{
			if (aWizardPage == null)
				return;

			if (pages == null)
                pages = new List<WizardPage>();

            // manage help requestes via F1 key
            aWizardPage.HelpRequested += new HelpEventHandler(WizardPage_HelpRequested);
			
            pages.Add(aWizardPage);
		}

		//--------------------------------------------------------------------
		public DialogResult Run(IWin32Window owner)
		{
            if (wizardForm == null || pages == null || pages.Count == 0)
				return DialogResult.None;

			if (!string.IsNullOrWhiteSpace(wizardTitle))
				wizardForm.Text = wizardTitle;

			if (wizardFormIcon != null)
				wizardForm.Icon = wizardFormIcon;
			
			wizardForm.Load += new EventHandler(WizardForm_Load);
            
            IWizardForm wizardFormInterface = wizardForm as IWizardForm;
            wizardFormInterface.PageActivating += new PageActivationEventHandler(WizardForm_PageActivating);
            wizardFormInterface.PageActivated += new PageActivationEventHandler(WizardForm_PageActivated);

			AutoSizeToMaxDims();
			
			wizardForm.Controls.AddRange((Control[])pages.ToArray());
			
			DialogResult result = (owner != null) ? wizardForm.ShowDialog(owner) : wizardForm.ShowDialog();

			if (result == DialogResult.OK)
			{
				finishButtonClicked = true;

				OnFinished();

				if (Finished != null)
					Finished(this, new System.EventArgs());
			}
			else if (result == DialogResult.Cancel)
			{
				OnCancelled();

				if (Cancelled != null)
					Cancelled(this, new System.EventArgs());
			}

			return result;
		}

		/// <summary>
		/// Returns the size calculated on the max height and width of the pages
		/// </summary>
		//--------------------------------------------------------------------
		public void AutoSizeToMaxDims()
		{
			int h = 0;
			int w = 0;
			foreach (Control c in pages)
			{
				h = Math.Max(h, c.Size.Height);
				w = Math.Max(w, c.Size.Width);
			}

            IWizardForm wizardFormInterface = wizardForm as IWizardForm;

            wizardForm.Size = new Size(w, h + wizardFormInterface.ButtonsBarHeight);
		}

		//--------------------------------------------------------------------
		public DialogResult Run()
		{
			return Run(null);
		}

		#region Help
		//---------------------------------------------------------------------
		void WizardPage_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            // if help hanlder was provided, use it
            if (OnHelpFromWizardPage != null)
                OnHelpFromWizardPage(sender, string.Empty, string.Empty);
            // otherwise, use standard handler
            else
                HelpManager.HelpRequested(sender, hlpevent);
        }

        //---------------------------------------------------------------------
		public void HelpFromWizardPage(object sender, string nameSpace, string searchParameter)
		{
            if (nameSpace == null || nameSpace.Length == 0)
                nameSpace = GetDefaultNameSpace();

            if (searchParameter == null || searchParameter.Length == 0)
                searchParameter = GetDefaultSearchParameters();

            // if help hanlder was provided, use it
            if (OnHelpFromWizardPage != null)
                OnHelpFromWizardPage(sender, nameSpace, searchParameter);
            // otherwise, use standard handler
            else
                HelpManager.OnHelpFromDialog(sender, nameSpace, searchParameter);
		}

		//---------------------------------------------------------------------
		public virtual string GetDefaultNameSpace()
		{
			return string.Empty;
		}

		//---------------------------------------------------------------------
		public virtual string GetDefaultSearchParameters()
		{
			return string.Empty;
		}
		#endregion

		//---------------------------------------------------------------------
		private void WizardForm_Load(object sender, System.EventArgs e)
		{
			if (sender != null && (sender is Form))
				OnWizardFormLoad((Form)sender);
		}
		
		//---------------------------------------------------------------------
		protected virtual void OnWizardFormLoad(Form wizardForm)
		{
		}

		//---------------------------------------------------------------------
		private void WizardForm_PageActivating(object sender, int pageIndex, WizardPage page)
		{
            if (wizardForm == null)
                return;
            IWizardForm wizardFormInterface = wizardForm as IWizardForm;

			if (pageIndex == 0)
                wizardFormInterface.SetWizardButtons(WizardButton.Next);
			else if (pageIndex == (pages.Count - 1))
                wizardFormInterface.SetWizardButtons(WizardButton.Back | WizardButton.Finish);
			else
                wizardFormInterface.SetWizardButtons(WizardButton.Back | WizardButton.Next);

			OnPageActivating(pageIndex, page);
			
			if (PageActivating != null)
				PageActivating(this, pageIndex, page);
		}
		
		//---------------------------------------------------------------------
		protected virtual void OnPageActivating(int pageIndex, WizardPage page)
		{
		}
		
		//---------------------------------------------------------------------
		private void WizardForm_PageActivated(object sender, int pageIndex, WizardPage page)
		{
			OnPageActivated(pageIndex, page);

			if (PageActivated != null)
				PageActivated(this, pageIndex, page);
		}

		//---------------------------------------------------------------------
		protected virtual void OnPageActivated(int pageIndex, WizardPage page)
		{
		}

		//---------------------------------------------------------------------
		protected virtual void OnCancelled()
		{
		}
	
		//---------------------------------------------------------------------
		protected virtual void OnFinished()
		{
		}
	}
}
