using System;
using System.IO;
using System.Windows.Forms;
using Microarea.EasyBuilder.MenuEditor;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.UI
{
	//================================================================================
	public partial class NewDocument : ThemedForm
	{
		
		private MDocument document;
		private bool userInsertedDocumentTitle;
			
		//--------------------------------------------------------------------------------
		//private 
        /// <summary>
        /// 
        /// </summary>
        public NewDocument(MDocument document)
		{
			InitializeComponent();
			this.document = document;
		}

		
		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);
			FormEditor.ShowHelp();
			hevent.Handled = true;
		}

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		public static bool SaveNewDocument(
			IWin32Window windowOwner,
			MDocument document,
			NameSpace originalNamespace,
			bool fromExistingDocument,
			bool isBatch,
			out string title,
			Microarea.EasyBuilder.MVC.DocumentControllers controllers = null
			)
		{
			title = String.Empty;
			if (
				(BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationType == ApplicationType.Standardization) &&
				BaseCustomizationContext.CustomizationContextInstance.NotAlone(Resources.AddNewDocumentCaption, 1, fromExistingDocument ? 1 : 0)
				//Se il nuovo documento è creato a partire da un altro documento allora mi aspetto che ci possa essere un solo documento aperto ("Save as new document") che è quello da cui sto salvando il presente,
				//se invece è creato da zero alora mi aspetto che non ci siano documenti aperti ("New Document" da menù File.).
				)
				return false;

			NewDocument saveWindow = new NewDocument(document);
			title = "";

			using (SafeThreadCallContext context = new SafeThreadCallContext())
			{
				if (DialogResult.OK != saveWindow.ShowDialog(windowOwner))
					return false;
			}

			title = saveWindow.txtDocumentTitle.Text;
			
			//aggiorno il namespace di documento con le informazioni inserite dll'utente
			string savingNamespaceSafe = ControllerSources.GetSafeSerializedNamespace(saveWindow.DocumentNamespace as INameSpace);
			document.Namespace = new NameSpace(savingNamespaceSafe);
			
			//aggiungo la dichiarazione del nuovo documento nei module objects in memoria e su file system
			AddToModuleObjects(
				document.Namespace, 
				saveWindow.txtDocumentTitle.Text, 
				originalNamespace,
				isBatch
				);

			MoveEasyBuilderAppToNewDocumentFolder(originalNamespace, document.Namespace, controllers);

			MenuEditorEngine.CreateMenuFileIfNecessaryAndAddToCurrentCustomizationContext(
				title,
				saveWindow.DocumentNamespace.GetNameSpaceWithoutType(),
				CUtility.GetUser(),
				isBatch
				);
			
			return true;
		}

		//--------------------------------------------------------------------------------
		private static void MoveEasyBuilderAppToNewDocumentFolder(
			INameSpace originalNamespace,
			INameSpace newDocNamespace,
			Microarea.EasyBuilder.MVC.DocumentControllers controllers
			)
		{
			if (originalNamespace == null)
				return;

			string sourceFolder = BasePathFinder.BasePathFinderInstance.GetDocumentPath(originalNamespace);
			if (!Directory.Exists(sourceFolder))
				return;

			string destinationFolder =  BasePathFinder.BasePathFinderInstance.GetDocumentPath(newDocNamespace);
			if (!Directory.Exists(destinationFolder))
			{
				Directory.CreateDirectory(destinationFolder);
			}

			DirectoryInfo di = new DirectoryInfo(sourceFolder);
			bool found = false;
			foreach (FileInfo fi in di.GetFiles(NameSolverStrings.DllSearchCriteria))
			{
				foreach (var controller in controllers)
				{
					if (!controller.GetType().Assembly.FullName.CompareNoCase(AssembliesLoader.Load(fi.FullName).FullName))
						continue;
					found = true;
					break;
				}

				if (!found)
					continue;
				
				string fileDestinationPath = Path.Combine(destinationFolder, fi.Name);
				fi.CopyTo(fileDestinationPath);

				BaseCustomizationContext
					.CustomizationContextInstance
					.CurrentEasyBuilderApp
					.EasyBuilderAppFileListManager
					.AddReadOnlyServerDocumentPartToCustomList(fileDestinationPath);

				found = false;
			}
		}
	
		/// <summary>
		/// Aggiunge una entry nel documentobjects 
		/// </summary>
		/// <param name="documentNamespace"></param>
		/// <param name="title"></param>
		/// <param name="originalDocumentNamespace"></param>
		/// <param name="isBatch"></param>
		//--------------------------------------------------------------------------------
		internal static void AddToModuleObjects(INameSpace documentNamespace, string title, NameSpace originalDocumentNamespace, bool isBatch)
		{
			IBaseModuleInfo mi = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleInfo;
			DocumentsObjectInfo targetDocInfo = (DocumentsObjectInfo)mi.DocumentObjectsInfo;

			NameSpace namespaceToSave = null;
			if (originalDocumentNamespace != null)
			{
				IDocumentsObjectInfo sourceDocInfo = BasePathFinder.BasePathFinderInstance.GetModuleInfoByName(originalDocumentNamespace.Application, originalDocumentNamespace.Module).DocumentObjectsInfo;
				namespaceToSave = CalculateOriginalTemplateNamespace(sourceDocInfo, originalDocumentNamespace);
			}
			//aggiungo il nuovo documento
			DocumentInfo di = new DocumentInfo(mi, documentNamespace, title, "", "", "");
			//se non ho un namespace del documento originale, o il documento originale era gia' un documento dinamico
			if (namespaceToSave == null)
			{
				di.IsDynamic = true; //è un documento dinamico
			}
			else
			{
				di.TemplateNamespace = namespaceToSave; //è una customizzazione di un documento esistente
			}
			
			if (isBatch)
			{
				di.IsBatch = true;
				di.AddViewMode(new ViewMode("Default", "", NameSolverXmlStrings.Batch, true));
			}
			else
			{
				di.IsDataEntry = true;
				di.AddViewMode(new ViewMode("Default", "", NameSolverXmlStrings.DataEntry, false));
			}
			targetDocInfo.Documents.Add(di);
			//risalvo il file aggiornato
			string documentsObjectPath = mi.GetDocumentObjectsPath();
			targetDocInfo.UnParse(documentsObjectPath);

			//Aggiungo il file alla customizzazione corrente
			BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(documentsObjectPath);

			//aggiorno le informazioni nella struttura C++
			MDocument.AddDynamicDocumentObject(documentNamespace, namespaceToSave, title, isBatch);
		}

		//--------------------------------------------------------------------------------
		private static NameSpace CalculateOriginalTemplateNamespace(IDocumentsObjectInfo info, NameSpace originalDocumentNamespace)
		{
			if (originalDocumentNamespace == null)
				return null;

			if (String.Compare(originalDocumentNamespace.Library, NameSolverStrings.DynamicLibraryName) != 0)
				return originalDocumentNamespace;

			if (info == null)
				return null;

			foreach (DocumentInfo documentInfo in info.Documents)
			{
				if (documentInfo.NameSpace.Equals(originalDocumentNamespace))
				{
					IDocumentsObjectInfo sourceDocInfo = BasePathFinder.BasePathFinderInstance.GetModuleInfoByName(documentInfo.NameSpace.Application, documentInfo.NameSpace.Module).DocumentObjectsInfo;
					return CalculateOriginalTemplateNamespace(sourceDocInfo, documentInfo.TemplateNamespace as NameSpace);
				}
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (DialogResult != System.Windows.Forms.DialogResult.OK)
				return;

			if (!BaseCustomizationContext.CustomizationContextInstance.IsValidName(txtDocumentName.Text))
			{
				MessageBox.Show(this, Resources.InvalidDocumentName, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				txtDocumentName.Focus();
				e.Cancel = true;
				return;
			}
			if (string.IsNullOrEmpty(txtDocumentTitle.Text))
			{
				MessageBox.Show(this, Resources.InvalidDocumentTitle, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				txtDocumentTitle.Focus();
				e.Cancel = true;
				return;
			}
			
			//se sto salvando una nuova customizzazione e questa esiste già, si tratta di un conflitto
			if (ExistDocument(DocumentNamespace))
			{
				MessageBox.Show(this, Resources.DocumentAlreadyExisting, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				{
					txtDocumentName.Focus();
					e.Cancel = true;
					return;
				}
			}
		}
		//--------------------------------------------------------------------------------
		internal bool ExistDocument(INameSpace documentNamespace)
		{
			return BasePathFinder.BasePathFinderInstance.GetDocumentInfo(documentNamespace) != null;
		}

		/// <summary>
		/// 
		/// </summary>
		//--------------------------------------------------------------------------------
		public INameSpace DocumentNamespace
		{
			get
			{
				return BaseCustomizationContext.CustomizationContextInstance.FormatDynamicNamespaceDocument(txtContextApp.Text, txtContextMod.Text,txtDocumentName.Text);
			}
		}

		///<remarks />
		//--------------------------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			InitAppInfo();
		}

		///<remarks />
		//--------------------------------------------------------------------------------
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			this.Focus();
			txtDocumentName.Focus();
		}

		//--------------------------------------------------------------------------------
		private void InitAppInfo()
		{
			txtContextApp.Text = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName;
			txtContextMod.Text = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName;
		}

		//--------------------------------------------------------------------------------
		private void btnChange_Click(object sender, System.EventArgs e)
		{
			if (ChooseCustomizationContext.Choose())
			{
				InitAppInfo();
			}
		}

		//--------------------------------------------------------------------------------
		private void txtDocumentName_TextChanged(object sender, EventArgs e)
		{
			if (!userInsertedDocumentTitle)
			{
				txtDocumentTitle.Text = txtDocumentName.Text;
			}
		}

		//--------------------------------------------------------------------------------
		private void txtDocumentTitle_KeyPress(object sender, KeyPressEventArgs e)
		{
			userInsertedDocumentTitle = !txtDocumentTitle.Text.IsNullOrEmpty();
		}
	}
}
