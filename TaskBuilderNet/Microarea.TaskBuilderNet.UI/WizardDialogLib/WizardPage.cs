using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WizardDialogLib
{
	/// <summary>
	/// Summary description for WizardPage.
	/// </summary>
	//=========================================================================
	[Designer(typeof(WizardDialogLib.WizardPageDesigner))]
	public partial class WizardPage : System.Windows.Forms.UserControl
	{
		public bool ToSkip = false;
		private System.Drawing.Font font = null;

		//--------------------------------------------------------------------
		public WizardPage()
		{
			InitializeComponent();
	
            if (this.WizardForm != null && this.WizardForm.DefaultPageFont != null)
			    this.Font = this.WizardForm.DefaultPageFont;
		}

		//--------------------------------------------------------------------
		protected IWizardForm WizardForm
		{
			get
			{
				Form parentForm = this.ParentForm;
				if (parentForm == null || !(parentForm is IWizardForm))
					return null;

				// Return the parent WizardForm
				return (IWizardForm)parentForm;
			}
		}

		//--------------------------------------------------------------------
		protected WizardManager WizardManager
		{
			get
			{
				IWizardForm parentWizardForm = this.WizardForm;
				if (parentWizardForm == null)
					return null;

				// Return the parent WizardForm
				return parentWizardForm.WizardManager;
			}
		}

		/// <summary>
		/// Deactivate if validation successful
		/// </summary>
		//--------------------------------------------------------------------
		public virtual bool OnKillActive()
		{
			return Validate();
		}

		/// <summary>
		/// Activate the page
		/// </summary>
		//--------------------------------------------------------------------
		public virtual bool OnSetActive()
		{
			return true;
		}

		/// <summary>
		/// Move to the default previous page in the wizard
		/// </summary>
		//--------------------------------------------------------------------
		public virtual string OnWizardBack()
		{
			return this.WizardForm.NextPage;
		}

		/// <summary>
		/// Finish the wizard
		/// </summary>
		//--------------------------------------------------------------------
        public virtual bool OnWizardFinish()
		{ 
			return true;
		}

		/// <summary>
		/// Move to the default next page in the wizard
		/// </summary>
		//--------------------------------------------------------------------
        public virtual string OnWizardNext()
		{
			return this.WizardForm.NextPage;
		}

		/// <summary>
		/// OnWizardHelp
		/// Premuto il bottone di Help
		/// </summary>
		/// <returns></returns>
		//--------------------------------------------------------------------
        public virtual bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, string.Empty, string.Empty);
			return true;
		}

		//--------------------------------------------------------------------
		private void UpdateFont()
		{
			base.Font = this.Font;
		}
		
		//--------------------------------------------------------------------
		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			UpdateFont();
		}
		
		//--------------------------------------------------------------------
		[Browsable(true), ReadOnly(false), Localizable(true)]  
		public override System.Drawing.Font Font 
		{
			get 
			{
				if (font == null) 
				{
					if (Parent != null) 
						font = Parent.Font;
					
					if (font == null) 
						font = WizardForm.DefaultPageFont;
				}
				return font;
			}

			set
			{
				if (value != null)
					font = value;
				else if (this.WizardForm != null && this.WizardForm.DefaultPageFont != null)
					font = this.WizardForm.DefaultPageFont;

                if (font != null)
				    base.Font = font;
			}
		}
	}

	//----------------------------------------------------------------------------
	public class WizardPageDesigner : System.Windows.Forms.Design.ControlDesigner 
	{
		// this is the "shadow" property
		[Localizable(true)]  
		public System.Drawing.Font Font
		{
			get 
			{
				return Control.Font;
			}
			set 
			{
				Control.Font = value;
			}
		}

		//----------------------------------------------------------------------------
		protected override void PreFilterProperties( IDictionary properties )
		{
			base.PreFilterProperties(properties);

			// shadow the Font property so we can intercept the set.
			properties["Font"] = 
				TypeDescriptor.CreateProperty
				(
				this.GetType(),
				"Font", 
				typeof(System.Drawing.Font), 
				CategoryAttribute.Appearance,
				LocalizableAttribute.Yes,
				new DefaultValueAttribute(WizardForm.DefaultPageFont)
				);
		}
	}
}
