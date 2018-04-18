using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using System.IO;

namespace Microarea.EasyBuilder.UI
{
	/// <remarks/>
	public enum SaveWindowButtons
	{
		/// <remarks/>
		YesNoCancel,
		/// <remarks/>
		YesCancel,
		/// <remarks/>
		YesNo
	}

	//================================================================================
	/// <remarks/>
	public partial class SaveCustomization : ThemedForm
	{
		private FormEditor editor;
		bool existingCustomization;
			
		private SaveWindowButtons buttons = SaveWindowButtons.YesCancel;
	
		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);
			FormEditor.ShowHelp();
			hevent.Handled = true;
		}
		//--------------------------------------------------------------------------------
		private SaveCustomization(
            FormEditor editor,
			string windowTitle,
			string message,
			bool existingCustomization,
            bool saveForWeb,
            SaveWindowButtons buttons = SaveWindowButtons.YesNoCancel
            )
		{
			InitializeComponent();
            chkSaveForWeb.Visible = true;
            chkSaveForWeb.Checked = true;
            this.editor = editor;
			this.buttons = buttons;
			this.Text = windowTitle;
			this.existingCustomization = existingCustomization;

			lblText.Text = message;
			
			if (existingCustomization)
				txtCustomizationName.Enabled = false;
			
			InitializeButtons();
		}

		///<remarks/>
		//--------------------------------------------------------------------------------
		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);

			chkPublish.Enabled = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationType == ApplicationType.Customization;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
        public static DialogResult SaveNewCustomization(FormEditor editor, 
			string windowTitle,
			string message,
			ref NameSpace ns,
			ref bool publish,
			out bool isDefault,
            ref bool saveForWeb,
            SaveWindowButtons buttons = SaveWindowButtons.YesNoCancel
            )
		{
			SaveCustomization saveWindow = new SaveCustomization(editor, windowTitle, message, false, saveForWeb, buttons);
			saveWindow.chkPublish.Checked = publish;

			IUIService service = editor.GetService(typeof(IUIService)) as IUIService;

			DialogResult result = service.ShowDialog(saveWindow);
			
			//le standardizzazioni sono pubblicate di default.
			//Se è un nuovo documento allora è pubblicato di default.
			publish = saveWindow.chkPublish.Checked;
            saveForWeb = saveWindow.chkSaveForWeb.Checked;
            //Se è salvato come un nuovo documento allora è marchiato come customizzazione di default per quel documento nel corrente contesto di customizzazione.
            isDefault = false;

			if (result == DialogResult.Yes)
				ns = saveWindow.CustomizationNamespace;
			return result;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
        public static DialogResult SaveExistingCustomization(FormEditor editor,
			string windowTitle,
			string message,
			ref NameSpace ns,
			ref bool publish,
			out bool isDefault,
            ref bool saveForWeb,
            SaveWindowButtons buttons = SaveWindowButtons.YesNoCancel
            )
		{
            SaveCustomization saveWindow = new SaveCustomization(editor, windowTitle, message, true, saveForWeb, buttons);
			saveWindow.chkPublish.Checked = publish;

			IUIService service = editor.GetService(typeof(IUIService)) as IUIService;
			DialogResult result = service.ShowDialog(saveWindow);
			publish = saveWindow.chkPublish.Checked;
            saveForWeb = saveWindow.chkSaveForWeb.Checked;
            isDefault = false;
			if (result == DialogResult.Yes)
				ns = saveWindow.CustomizationNamespace;
			return result;
		}

		//--------------------------------------------------------------------------------
		private void InitializeButtons()
		{
			switch (buttons)
			{
				case SaveWindowButtons.YesNoCancel:
					btnNo.Visible = true;
					btnYes.Visible = true;
					btnCancel.Visible = true;
					break;
				case SaveWindowButtons.YesCancel:
					btnNo.Visible = false;
					btnYes.Visible = true;
					btnCancel.Visible = true;
					break;
				case SaveWindowButtons.YesNo:
					btnNo.Visible = true;
					btnYes.Visible = true;
					btnCancel.Visible = false;
					break;
				default:
					break;
			}

			//devo controllare se e' la prima personalizzazione, per rendere visibile o meno il flag "imposta come personalizzazione di default"
			List<string> paths = editor.Controller == null ? null : BasePathFinder.BasePathFinderInstance.GetEasyBuilderAppAssembliesPaths(editor.Sources.Namespace, CUtility.GetUser(), BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications);
			bool isFirstEasyBuilderApp =
				BaseCustomizationContext.CustomizationContextInstance.IsCurrentEasyBuilderAppAStandardization 
					? !BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.EasyBuilderAppFileListManager.ContainsOtherControllerDll(editor.Sources.Namespace)
					: (paths == null || paths.Count == 0);
			
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (DialogResult != System.Windows.Forms.DialogResult.Yes)
				return;

			if (txtCustomizationName.Enabled && !BaseCustomizationContext.CustomizationContextInstance.IsValidName(txtCustomizationName.Text))
			{
				MessageBox.Show(this, Resources.ChooseValidName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				txtCustomizationName.Focus();
				e.Cancel = true;
				return;
			}

            var outputFileFullName = FormEditor.CalculateOutputFileFullName(this.CustomizationNamespace);

            if (outputFileFullName.Length > 254)//260-4 dove 260 e` il limite di lunghezza di path di Windows, 4 e` la lunghezza dell'estensione ".dll"
            {
                //Se gia` il nome della cartella eccede il limite di 248 imposto da Windows per le cartelle non andiamo da nessuna parte.
                //Non posso usare i metodi della classe di sistema Path per ottenere il nome della cartella perche` con un path piu` lungo di 260 ritornano eccezione.
                var directory = outputFileFullName.Substring(0, outputFileFullName.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                System.Diagnostics.Debug.Assert(directory.Length > 248);

                var fileNameWOExt = System.IO.Path.GetFileNameWithoutExtension(outputFileFullName);
                fileNameWOExt = fileNameWOExt.Substring(0, 254 - directory.Length);
                MessageBox.Show(this, string.Format(System.Globalization.CultureInfo.InvariantCulture, Resources.NameTooLong, fileNameWOExt), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCustomizationName.Focus();
                e.Cancel = true;
                return;
            }

            //se sto salvando una nuova customizzazione e questa esiste già, si tratta di un conflitto
            var app = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp;
            if (
                !existingCustomization &&
                (File.Exists(BaseCustomizationContext.CustomizationContextInstance.GetEasyBuilderAppAssemblyFullName(this.CustomizationNamespace, null, app)) ||
                File.Exists(BaseCustomizationContext.CustomizationContextInstance.GetEasyBuilderAppAssemblyFullName(this.CustomizationNamespace, CUtility.GetUser(), app)))
                )
			{
				MessageBox.Show(this, Resources.CustomizationAlreadyExisting, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				{
					txtCustomizationName.Focus();
					e.Cancel = true;
					return;
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void SaveWindow_Load(object sender, System.EventArgs e)
		{
			NameSpace ns = new NameSpace(ControllerSources.GetSafeSerializedNamespace(editor.Sources.Namespace.FullNameSpace));
			txtCustomizationName.Text = ns.Leaf;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public NameSpace CustomizationNamespace
		{
			get
			{
				NameSpace ns;
								
				//se salvo roba già esistente, non devo calcolare dinamicamente il namespace
				if (existingCustomization)
					return editor.Sources.Namespace;

				ns = new NameSpace(editor.Sources.Namespace);
				ns.Leaf = txtCustomizationName.Text;
				return ns;
			}
		}

		//--------------------------------------------------------------------------------
		private void txtCustomizationName_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar != (char)Keys.Space)
				return;

			e.Handled = true;
		}

        //--------------------------------------------------------------------------------
        private void chkSaveForWeb_CheckedChanged(object sender, System.EventArgs e)
        {

        }
    }
}
