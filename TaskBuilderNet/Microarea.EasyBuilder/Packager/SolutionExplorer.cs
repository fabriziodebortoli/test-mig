using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microarea.EasyBuilder.BackendCommunication;
using Microarea.EasyBuilder.Properties;
using Microarea.EasyBuilder.UI;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using WaitingWnd = Microarea.TaskBuilderNet.UI.WinControls.WaitingWindow;

namespace Microarea.EasyBuilder.Packager
{
	/// <summary>
	/// Using this form the user can manage all customization packages installed,
	/// he can install new packages as well.
	/// </summary>
	//===================================================================================
	public partial class SolutionExplorer : ThemedForm
	{
        private const string DocumentsString = "Documents";
        private const string ReportsString = "Reports";
        private const string DbScriptString = "DbScript";
        private const string OtherFilesString = "OtherFiles";
        private const string ReferenceAssembliesString = "ReferenceAssemblies";
        private const string MenusString = "Menu";
        private const string ModuleObjectsString = "ModuleObjects";
        private const string ReferenceObjectsString = "ReferenceObjects";

        private bool isEasyBuilderDeveloper;
        private string currentApplicationName = string.Empty;
        private LoginManager loginManager;

        /// <summary>
        /// Initializes a new instance of the SolutionExplorer.
        /// </summary>
        //-----------------------------------------------------------------------------
        public SolutionExplorer(LoginManager loginManager)
        {
            InitializeComponent();
            InitializeViewer();

            this.loginManager = loginManager;

            isEasyBuilderDeveloper = loginManager.IsEasyBuilderDeveloper(loginManager.AuthenticationToken);

            FillApplicationList();
        }

        //-----------------------------------------------------------------------------
        void CustomizationContext_AddedItem(object sender, CustomListItemAddedEventArgs e)
        {
            //se il file non è stato aggiunto all'applicazione correntemente selezionata non faccio niente
            if (!currentApplicationName.CompareNoCase(e.Customization.ApplicationName))
                return;

            UpdateTreeView(e);
        }

        //-----------------------------------------------------------------------------
        private void UpdateTreeView(CustomListItemAddedEventArgs e)
        {
            if (IsDisposed)
                return;

            if (InvokeRequired)
            {
                this.Invoke((Action)delegate { UpdateTreeView(e); });
                return;
            }

            FillModulesTreeView();
        }

        /// <summary>
        /// Raises the Load event and populates the application list.
        /// </summary>
        //-----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            this.TopMost = true;
            base.OnLoad(e);

            //Imposto il modulo di attivo di default basandomi su quello che ho memorizzato nei setting
            //se i settings non matchano con niente nella lista, viene scelto la prima applicazione/modulo trovata
            SetDefaultActiveModule();

			(BaseCustomizationContext.CustomizationContextInstance as CustomizationContext).AddedItem += new EventHandler<CustomListItemAddedEventArgs>(CustomizationContext_AddedItem);
            this.TopMost = false;
        }

        /// <summary>
        /// Raises the Closed event
        /// </summary>
        //-----------------------------------------------------------------------------
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
			(BaseCustomizationContext.CustomizationContextInstance as CustomizationContext).AddedItem -= new EventHandler<CustomListItemAddedEventArgs>(CustomizationContext_AddedItem);

		}

		//-----------------------------------------------------------------------------
		private void InitializeViewer()
        {
            ofdAddFile.InitialDirectory = BasePathFinder.BasePathFinderInstance.GetCustomPath();
            tvFiles.ImageList = ImageLists.TreeViewFilesImageList;
            tvFiles.Font = new System.Drawing.Font(tvFiles.Font, FontStyle.Bold);
            tvFiles.StateImageList = ImageLists.StatusImageList;
            lvApplications.SmallImageList = ImageLists.SmallImageList;
            lvApplications.StateImageList = ImageLists.StatusImageList;

            BaseCustomizationContext.CustomizationContextInstance.Diagnostic.AddedDiagnostic += new EventHandler(Diagnostic_AddedDiagnostic);
        }

        //-----------------------------------------------------------------------------
        private void FillApplicationList()
        {
            lvApplications.BeginUpdate();
            lvApplications.Items.Clear();

            //Carica tutte le customizzazioni nella list view, da notare che la vera customizzazione è il modulo
            foreach (Customization customization in BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications)
            {
                //Se ho già aggiunto l'applicazione non faccio niente
                if (lvApplications.Items.ContainsKey(customization.ApplicationName))
                    continue;

                //Aggiunto l'applicazione alla lista, e se è quella attiva la spunto e seleziono
                ApplicationListViewItem lvItem = new ApplicationListViewItem(customization.ApplicationName);
                //lvItem.IsActiveApplication = BaseCustomizationContext.CustomizationContextInstance.IsActiveApplication(customization.ApplicationName);
                //if (lvItem.IsActiveApplication)
                //    lvItem.Selected = true;
                if (BaseCustomizationContext.CustomizationContextInstance.IsActiveApplication(customization.ApplicationName))
                    currentApplicationName = customization.ApplicationName;

                //icona per differenziare verticali da custumizzazioni presa dall'imagelist
                lvItem.IsStandardApplication = customization.ApplicationType == ApplicationType.Standardization;

                //Mettiamo nel tag il tipo di applicazoine EasyBuilder,
                //ci viene comodo quando aggiungiamo un modulo ad un'applicazione che non ha moduli.
                lvItem.Tag = customization.ApplicationType;

                lvApplications.Items.Add(lvItem);
            }

            if (currentApplicationName.IsNullOrWhiteSpace() && lvApplications.Items.Count > 0)
                currentApplicationName = lvApplications.Items[0].Name;

            FillModulesTreeView();

            UpdateButtons();

            lvApplications.EndUpdate();
        }

        //-----------------------------------------------------------------------------
        void Diagnostic_AddedDiagnostic(object sender, EventArgs e)
        {
            if (!IsDisposed)
            {
                Invoke(new Action(
                    ()
                    =>
                    tsShowDiagnostic.Enabled = true
                    ));
            }
        }

        //-----------------------------------------------------------------------------------------
        private void FillModulesTreeView()
        {
            if (currentApplicationName == null || currentApplicationName.Trim().Length == 0)
                return;

            tvFiles.BeginUpdate();

            tvFiles.Nodes.Clear();

            List<string> customListFiles = BaseCustomizationContext.CustomizationContextInstance.GetAllEasyBuilderAppsFileListPath(currentApplicationName);
            List<string> loadedModules = new List<string>();
            foreach (string customFile in customListFiles)
            {
                IEasyBuilderApp cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(customFile);
                if (cust == null)
                    continue;

                loadedModules.Add(cust.ModuleName);
                TreeViewProjectFilesNode tvNode = PrepareModuleTree(cust.ModuleName);

                foreach (CustomListItem item in cust.EasyBuilderAppFileListManager.CustomList)
                    AddFileToTree(cust, item);

                SetEnableDisableStatus(tvNode, cust);
            }

            if (BaseCustomizationContext.CustomizationContextInstance.ShouldStandardizationsBeAvailable())
            {
                foreach (IBaseModuleInfo module in BasePathFinder.BasePathFinderInstance.GetModulesList(currentApplicationName))
                {
                    if (loadedModules.ContainsNoCase(module.Name))
                        continue;

                    IEasyBuilderApp cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(currentApplicationName, module.Name);
                    if (cust == null)
                        continue;

                    TreeViewProjectFilesNode tvNode = PrepareModuleTree(cust.ModuleName);

                    foreach (CustomListItem item in cust.EasyBuilderAppFileListManager.CustomList)
                        AddFileToTree(cust, item);

                    SetEnableDisableStatus(tvNode, cust);
                }
            }

            UpdateButtons();

            tvFiles.EndUpdate();
        }

        //-----------------------------------------------------------------------------------------
        private TreeViewProjectFilesNode PrepareModuleTree(string moduleName)
        {
            TreeViewProjectFilesNode moduleNode = new TreeViewProjectFilesNode(moduleName, string.Empty, TreeViewProjectFileItemType.Module);
            tvFiles.Nodes.Add(moduleNode);

            TreeViewProjectFilesNode documents = new TreeViewProjectFilesNode(DocumentsString, TreeViewProjectFileItemType.Documents);
            moduleNode.Nodes.Add(documents);
            documents.Expand();

            TreeViewProjectFilesNode reports = new TreeViewProjectFilesNode(ReportsString, TreeViewProjectFileItemType.Reports);
            moduleNode.Nodes.Add(reports);
            reports.Expand();

            TreeViewProjectFilesNode dbScript = new TreeViewProjectFilesNode(DbScriptString, TreeViewProjectFileItemType.DbScript);
            moduleNode.Nodes.Add(dbScript);

            TreeViewProjectFilesNode moduleObjects = new TreeViewProjectFilesNode(ModuleObjectsString, TreeViewProjectFileItemType.ModuleObjects);
            moduleNode.Nodes.Add(moduleObjects);
            moduleObjects.Expand();

            TreeViewProjectFilesNode referenceObjects = new TreeViewProjectFilesNode(ReferenceObjectsString, TreeViewProjectFileItemType.ReferenceObjects);
            moduleNode.Nodes.Add(referenceObjects);
            referenceObjects.Expand();

            TreeViewProjectFilesNode referenceAssemblies = new TreeViewProjectFilesNode(ReferenceAssembliesString, TreeViewProjectFileItemType.ReferenceAssemblies);
            moduleNode.Nodes.Add(referenceAssemblies);

            TreeViewProjectFilesNode menus = new TreeViewProjectFilesNode(MenusString, TreeViewProjectFileItemType.Menus);
            moduleNode.Nodes.Add(menus);

            TreeViewProjectFilesNode otherFiles = new TreeViewProjectFilesNode(OtherFilesString, TreeViewProjectFileItemType.OtherFiles);
            moduleNode.Nodes.Add(otherFiles);

            //espando il nodo del modulo
            moduleNode.Expand();

            return moduleNode;
        }

        //-----------------------------------------------------------------------------
        private void AddFileToTree(IEasyBuilderApp cust, CustomListItem customListItem)
        {
            if (String.IsNullOrWhiteSpace(cust.ModuleName) || customListItem == null)
                return;

            string file = customListItem.FilePath;
            string publishedUser = customListItem.PublishedUser;
            IBaseModuleInfo module = BasePathFinder.BasePathFinderInstance.GetModuleInfoByName(cust.ApplicationName, cust.ModuleName);

            FileInfo fi = new FileInfo(file);

            string referencedAssembliesPath = PathFinderWrapper.GetEasyStudioReferenceAssembliesPath();
            if (fi.FullName.IndexOfNoCase(string.Format("\\{0}\\", referencedAssembliesPath)) > 0)
            {
                TreeViewProjectFilesNode referenceAssemblies = GetNodeByType(cust.ModuleName, TreeViewProjectFileItemType.ReferenceAssemblies);
                if (referenceAssemblies == null)
                    return;

                TreeViewProjectFilesNode referenceFile = new TreeViewProjectFilesNode(file, file, TreeViewProjectFileItemType.ReferenceAssembly);
                referenceAssemblies.Nodes.Add(referenceFile);
                return;
            }

            if (fi.FullName.IndexOfNoCase(string.Format("\\{0}\\", NameSolverStrings.Menu)) > 0)
            {
                TreeViewProjectFilesNode menus = GetNodeByType(cust.ModuleName, TreeViewProjectFileItemType.Menus);
                if (menus == null)
                    return;

                TreeViewProjectFilesNode menuFile = new TreeViewProjectFilesNode(Path.GetFileName(file), file, TreeViewProjectFileItemType.Menu, publishedUser);
                menus.Nodes.Add(menuFile);
                return;
            }

            if (fi.FullName.IndexOfNoCase(string.Format("{0}\\{1}\\", cust.ModuleName, NameSolverStrings.ModuleObjects)) > 0)
            {
                TreeViewProjectFilesNode moduleObjects = GetNodeByType(cust.ModuleName, TreeViewProjectFileItemType.ModuleObjects);
                //Se è una DLL devo discriminare se si tratti della DLL di modulo o di un nuovo documento.
                if (String.Compare(fi.Extension, ".dll", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (String.Compare(fi.DirectoryName, module.GetModuleObjectPath(), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        //La DLL di modulo va gestita nel nodo moduleObjects, i documenti vanno nel nodo documents
                        //gestito più sotto.
                        TreeViewProjectFilesNode moduleObjectFile = new TreeViewProjectFilesNode(Path.GetFileName(file), file, TreeViewProjectFileItemType.ModuleObject, publishedUser);
                        moduleObjects.Nodes.Add(moduleObjectFile);
                        return;
                    }
                }
                else//Se non è una DLL segue il flusso normale.
                {
                    TreeViewProjectFilesNode moduleObjectFile = new TreeViewProjectFilesNode(Path.GetFileName(file), file, TreeViewProjectFileItemType.ModuleObject, publishedUser);
                    moduleObjects.Nodes.Add(moduleObjectFile);
                    return;
                }
            }

            if (fi.FullName.IndexOfNoCase(string.Format("{0}\\{1}\\", cust.ModuleName, NameSolverStrings.ReferenceObjects)) > 0)
            {
                TreeViewProjectFilesNode referenceObjects = GetNodeByType(cust.ModuleName, TreeViewProjectFileItemType.ReferenceObjects);
                if (referenceObjects == null)
                    return;

                TreeViewProjectFilesNode referenceObjectFile = new TreeViewProjectFilesNode(Path.GetFileName(file), file, TreeViewProjectFileItemType.ReferenceObject, publishedUser);
                referenceObjects.Nodes.Add(referenceObjectFile);
                return;
            }

            if (fi.Extension.CompareNoCase(NameSolverStrings.DllExtension) || fi.Extension.CompareNoCase(NameSolverStrings.EbLinkExtension))
            {
                //Non mostro le DLL che sono state portate dietro dai vari documenti con il processo di "Save as new document"
                if (customListItem.IsReadOnlyServerDocumentPart)
                    return;

                TreeViewProjectFilesNode documents = GetNodeByType(cust.ModuleName, TreeViewProjectFileItemType.Documents);
                if (documents == null)
                    return;

                AddFileToFolder(documents, fi, TreeViewProjectFileItemType.DocumentCustomization, publishedUser);
                return;
            }

            if (fi.Extension.CompareNoCase(NameSolverStrings.WrmExtension))
            {
                TreeViewProjectFilesNode reports = GetNodeByType(cust.ModuleName, TreeViewProjectFileItemType.Reports);
                if (reports == null)
                    return;

                AddFileToFolder(reports, fi, TreeViewProjectFileItemType.ReportCustomization);
                return;
            }

            if (fi.FullName.IndexOfNoCase(string.Format("\\{0}\\", NameSolverStrings.DatabaseScript)) > 0)
            {
                TreeViewProjectFilesNode dbScript = GetNodeByType(cust.ModuleName, TreeViewProjectFileItemType.DbScript);
                if (dbScript == null)
                    return;

                TreeViewProjectFilesNode scriptFile = new TreeViewProjectFilesNode(file, file, TreeViewProjectFileItemType.OtherFile);
                dbScript.Nodes.Add(scriptFile);
                return;
            }

            TreeViewProjectFilesNode otherFiles = GetNodeByType(cust.ModuleName, TreeViewProjectFileItemType.OtherFiles);
            if (otherFiles == null)
                return;

            //escludo dalla addfiles application.config, module.config e customlist
            if (
                file.ContainsNoCase(NameSolverStrings.Application + NameSolverStrings.ConfigExtension) ||
                file.ContainsNoCase(NameSolverStrings.Module + NameSolverStrings.ConfigExtension) ||
                file.ContainsNoCase(NameSolverStrings.CustomListFileExtension) ||
                file.ContainsNoCase(NameSolverStrings.DocumentObjectsXml) ||
                file.ContainsNoCase(NameSolverStrings.DatabaseObjectsXml)
                )
                return;

            TreeViewProjectFilesNode files = new TreeViewProjectFilesNode(file, file, TreeViewProjectFileItemType.OtherFile);
            otherFiles.Nodes.Add(files);
        }

        //-----------------------------------------------------------------------------
        private void SetModuleAsActive()
        {
            if (tvFiles.SelectedNode == null || tvFiles.SelectedNode as TreeViewProjectFilesNode == null)
                return;

            TreeViewProjectFilesNode currentNode = tvFiles.SelectedNode as TreeViewProjectFilesNode;
            if (currentNode == null || currentNode.NodeType != TreeViewProjectFileItemType.Module)
                return;

            //Imposto la nuova customizzazione corrente
            BaseCustomizationContext.CustomizationContextInstance.ChangeEasyBuilderApp(currentApplicationName, currentNode.Name);

            try
            {
                //Imposto l'applicazione attiva
                ApplicationListViewItem tempItem = lvApplications.Items[currentApplicationName] as ApplicationListViewItem;
                if (tempItem == null)
                    return;

                lvApplications.BeginUpdate();
                //metto a false il flag IsDefaultCustomization per tutte le customizzazioni
                foreach (ApplicationListViewItem item in lvApplications.Items)
                    item.IsActiveApplication = false;

                tempItem.IsActiveApplication = true;

                lvApplications.EndUpdate();

                tvFiles.BeginUpdate();

                //Levo l'attivazione del modulo sia dal treeview...
                foreach (TreeViewProjectFilesNode item in tvFiles.Nodes)
                    item.IsActiveModule = false;

                TreeViewProjectFilesNode node = FindModuleInTreeView(currentNode.Name);
                if (node != null)
                    node.IsActiveModule = true;

                tvFiles.EndUpdate();

				BaseCustomizationContext.CustomizationContextInstance.ChangeEasyBuilderApp(currentApplicationName, currentNode.Name);
			}
            catch { }
        }

        //-----------------------------------------------------------------------------
        private void SetDefaultActiveModule()
        {
            string application = BaseCustomizationContext.CustomizationContextInstance.CurrentApplication;
            string module = BaseCustomizationContext.CustomizationContextInstance.CurrentModule;

            //cerco l'applicazione di default tra quelle della list view
            ApplicationListViewItem tempItem = FindApplicationInListView(application);
            if (tempItem == null)
            {
                //Se non la trovo, imposto modulo e applicazione di default
                application = SelectFirstApplicationInListView();
                module = SelectFirstModuleOfTreeView();
                BaseCustomizationContext.CustomizationContextInstance.ChangeEasyBuilderApp(application, module);
                return;
            }

            //se invece l'applicazione è stata trovata, procedo con l'attivazione dell'applicazione stessa
            tempItem.IsActiveApplication = true;

            if (!application.CompareNoCase(currentApplicationName))
                return;

            TreeViewProjectFilesNode moduleNode = FindModuleInTreeView(module);
            if (moduleNode == null)
            {
                module = SelectFirstModuleOfTreeView();
				BaseCustomizationContext.CustomizationContextInstance.ChangeEasyBuilderApp(application, module);
                return;
            }

            //Se invece l'ho trovato, lo imposto a true
            moduleNode.IsActiveModule = true;

            //Se uno dei due tra applicazione e modulo è nullo o vuoto non faccio altro
            if (application.IsNullOrEmpty() || module.IsNullOrEmpty())
                return;

            //Imposto la nuova customizzazione corrente
            BaseCustomizationContext.CustomizationContextInstance.ChangeEasyBuilderApp(application, module);
        }

        //-----------------------------------------------------------------------------
        private string SelectFirstApplicationInListView()
        {
            //Se non ho trovato l'applicazione corrente, uso quella selezionata attualmente
            if (lvApplications.SelectedItems.Count <= 0)
                return string.Empty;

            ApplicationListViewItem tempItem = lvApplications.SelectedItems[0] as ApplicationListViewItem;
            if (tempItem == null)
                return string.Empty;

            //metto a false il flag IsActiveApplication per tutte le customizzazioni
            foreach (ApplicationListViewItem item in lvApplications.Items)
                item.IsActiveApplication = false;

            //seleziono solo quella corrente
            tempItem.IsActiveApplication = true;
            return tempItem.Text;
        }

        //-----------------------------------------------------------------------------
        private string SelectFirstModuleOfTreeView()
        {
            if (tvFiles.Nodes.Count <= 0)
                return string.Empty;

            TreeViewProjectFilesNode node = tvFiles.Nodes[0] as TreeViewProjectFilesNode;
            if (node == null)
                return string.Empty;

            //passo all'attivazione del modulo, usando il primo che trovo
            //Levo l'attivazione del modulo sia dal treeview...
            foreach (TreeViewProjectFilesNode item in tvFiles.Nodes)
                item.IsActiveModule = false;

            node.IsActiveModule = true;
            return node.Text;
        }

        //-----------------------------------------------------------------------------
        private ApplicationListViewItem FindApplicationInListView(string application)
        {
            foreach (ApplicationListViewItem item in lvApplications.Items)
            {
                if (item.Text.CompareNoCase(application))
                    return item;
            }
            return null;
        }

        //-----------------------------------------------------------------------------
        private TreeViewProjectFilesNode FindModuleInTreeView(string module)
        {
            foreach (TreeViewProjectFilesNode node in tvFiles.Nodes)
            {
                if (node.Tag.ToString().CompareNoCase(module))
                    return node;
            }
            return null;
        }

        //-----------------------------------------------------------------------------
        private void EnableUI(bool enable)
        {
            //disabilita tutta la form
            Enabled = enable;
        }

        //-----------------------------------------------------------------------------
        private void AddModule()
        {
            try
            {
                //disabilita tutta la form per evitare interazione dell'utente durante le operazioni di background
                EnableUI(false);

                IList<IEasyBuilderApp> apps = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(currentApplicationName);
                ApplicationType easyBuilderAppType = ApplicationType.Undefined;

                if ((apps == null || apps.Count == 0) && (lvApplications.SelectedItems == null || lvApplications.SelectedItems.Count == 0))
                    return;

                easyBuilderAppType =
                    (apps == null || apps.Count == 0) ?
                    (ApplicationType)lvApplications.SelectedItems[0].Tag :
                    apps[0].ApplicationType;

                if (easyBuilderAppType == ApplicationType.Standardization)
                {
                    if (!TestCredentialsForSalesModulesCrypting())
                        return;

                    if (BaseCustomizationContext.CustomizationContextInstance.NotAlone(Resources.AddModuleCaption, 1, 0, this))
                        return;
                }

                AddRenameCustomization ac = new AddRenameCustomization(CustomizationStatus.AddModule, GetAlreadyUsedModuleNames(), currentApplicationName);
                DialogResult result = ac.ShowDialog(this);
                if (result != System.Windows.Forms.DialogResult.OK)
                    return;

                string newModuleName = ac.ModuleName;
                if (string.IsNullOrEmpty(newModuleName))
                    return;

                ShowWaitingWndWhileExecutingAction(
                    (Action)delegate
                    {
                        BaseCustomizationContext.CustomizationContextInstance.AddNewEasyBuilderApp(currentApplicationName, newModuleName, easyBuilderAppType);
                    },
                    String.Format(Resources.WaitingMessageCreation, newModuleName)
                );

                //dopodiché riforzo il caricamento della finestra del packager
                FillModulesTreeView();
                SetTreeViewModuleNodeAsActive(newModuleName);
            }
            finally
            {
                EnableUI(true);
            }
        }

        //-----------------------------------------------------------------------------
        private void SetTreeViewModuleNodeAsActive(string newModuleName)
        {
            foreach (TreeViewProjectFilesNode node in tvFiles.Nodes)
            {
                if (node.NodeType == TreeViewProjectFileItemType.Module && String.Compare(node.Name, newModuleName) == 0)
                {
                    tvFiles.SelectedNode = node;
                    break;
                }
            }
            SetModuleAsActive();
        }

        //-----------------------------------------------------------------------------
        private static bool TestCredentialsForSalesModulesCrypting()
        {
            bool credentialsOk = true;
            if (!CrypterWrapper.TestCredentials(Settings.Default.Username, Crypto.Decrypt(Settings.Default.Password)))
            {
                credentialsOk = false;
                Credentials cred = new Credentials();
                cred.Header = Resources.WrongCredentials1;
                if (cred.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    credentialsOk = true;
            }
            return credentialsOk;
        }

        //-----------------------------------------------------------------------------
        private List<string> GetAlreadyUsedModuleNames()
        {
            List<string> listModules = new List<string>();
            for (int i = 0; i < tvFiles.Nodes.Count; i++)
            {
                listModules.Add(tvFiles.Nodes[i].Text);
            }
            return listModules;
        }

        //-----------------------------------------------------------------------------
        private void DeleteModule()
        {
            try
            {
                bool reloadLvApplications = false;
                bool askForDeletingApplication = true;
                if (tvFiles.Nodes.Count == 1)
                {
                    //Si sta rimuovendo l'unico nodo dell'applicazione, quindi si avverte che verra rimossa tutta l'applicaizone
                    if (MessageBox.Show(this, Resources.ConfirmDeleteModule, Resources.DeleteModuleCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                        return;

                    askForDeletingApplication = false;
                    reloadLvApplications = true;
                }

                //disabilita tutta la form per evitare interazione dell'utente durante le operazioni di background
                EnableUI(false);
                if (tvFiles.SelectedNode == null)
                    return;

                TreeViewProjectFilesNode currentNode = tvFiles.SelectedNode as TreeViewProjectFilesNode;
                if (currentNode == null)
                    return;

                if (currentNode.NodeType != TreeViewProjectFileItemType.Module)
                    return;

                if (askForDeletingApplication)
                {
                    DialogResult res = MessageBox.Show(
                        this,
                        Resources.ConfirmDeleteCustomization,
                        Resources.DeleteCustomizationCaption,
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question
                        );

                    if (res != System.Windows.Forms.DialogResult.OK)
                        return;
                }

                //rimuovo l'elemento dalla listview
                tvFiles.Nodes.Remove(currentNode);

                //rimuovo anche il nodo papa, se non c'è niente dentro
                IDisposable disposable = null;
                if (currentNode.Parent != null && currentNode.Parent.Nodes.Count == 0)
                {
                    tvFiles.Nodes.Remove(currentNode.Parent);
                    disposable = currentNode.Parent as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }

                string applicationName = lvApplications.SelectedItems != null && lvApplications.SelectedItems.Count > 0 ? lvApplications.SelectedItems[0].Text : String.Empty;
                if (String.IsNullOrWhiteSpace(applicationName))
                {
                    MessageBox.Show(this, Resources.UnableToDeleteAModule,
                        Resources.DeleteModuleError,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );
                    return;
                }

                ShowWaitingWndWhileExecutingAction(
                    (Action)delegate
                    {
                        BaseCustomizationContext.CustomizationContextInstance.DeleteEasyBuilderApp(applicationName, currentNode.Name);
                    },
                    String.Format(Resources.WaitingMessageDelete, currentNode.Name)
                );

                //Se la customizzazione cancellata era di default e ne è rimasta almeno una, allora quella diventa di default
                if (lvApplications.Items.Count > 0 && BaseCustomizationContext.CustomizationContextInstance.CurrentModule == currentNode.Tag.ToString() && tvFiles.Nodes.Count > 0)
                {
					BaseCustomizationContext.CustomizationContextInstance.ChangeEasyBuilderApp(BaseCustomizationContext.CustomizationContextInstance.CurrentApplication, tvFiles.Nodes[0].Tag.ToString());
                    ((TreeViewProjectFilesNode)tvFiles.Nodes[0]).IsActiveModule = true;
                }

                //Disposo il nodo cancellato.
                disposable = currentNode as IDisposable;
                if (disposable != null)
                    disposable.Dispose();

                CUtility.RefreshMenuDocument();
                tvFiles.Refresh();
                if (reloadLvApplications)
                    FillApplicationList();
            }
            finally
            {
                EnableUI(true);
            }
        }

        //-----------------------------------------------------------------------------
        private void RenameModule()
        {
            try
            {
                //disabilita tutta la form per evitare interazione dell'utente durante le operazioni di background
                EnableUI(false);
                TreeViewProjectFilesNode currentNode = tvFiles.SelectedNode as TreeViewProjectFilesNode;
                if (currentNode.NodeType != TreeViewProjectFileItemType.Module)
                    return;

                string oldModuleName = currentNode.Tag.ToString();
                IEasyBuilderApp cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(currentApplicationName, oldModuleName);

                if (
                    cust != null &&
                    (cust.ApplicationType == ApplicationType.Standardization) &&
                    !TestCredentialsForSalesModulesCrypting()
                    )
                    return;

                //Se sono già stati applicati scatti di release al modulo, non posso rinominare la customizzaione
                if (!CanRenameModule(oldModuleName))
                {
                    MessageBox.Show(this, Resources.UnableToRenameModuleForScript,
                        Resources.RenameCustomization,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );
                    return;
                }

                if (IsModuleContainsScripts(oldModuleName))
                {
                    DialogResult res = MessageBox.Show(this, Resources.DoYouWantToRenameModuleAnyway,
                        Resources.RenameCustomization,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                        );

                    if (res == System.Windows.Forms.DialogResult.No)
                        return;
                }

                AddRenameCustomization ac = new AddRenameCustomization(CustomizationStatus.RenameModule, GetAlreadyUsedModuleNames(), currentApplicationName, oldModuleName);
                DialogResult result = ac.ShowDialog(this);
                if (result != System.Windows.Forms.DialogResult.OK)
                    return;

                string newName = ac.ModuleName;
                if (string.IsNullOrEmpty(newName))
                    return;

                ShowWaitingWndWhileExecutingAction(
                    (Action)delegate
                    {
                        cust.RenameEasyBuilderApp(newName);
                    },
                    String.Format(Resources.WaitingMessageRename, oldModuleName, newName)
                );

                //Se la customizzazione rinominata era di default, aggiorno il nome anche nei settings
                if (BaseCustomizationContext.CustomizationContextInstance.CurrentModule == oldModuleName)
                {
					BaseCustomizationContext.CustomizationContextInstance.CurrentModule = newName;
                    BaseCustomizationContext.CustomizationContextInstance.SaveSettings();
                }

                FillModulesTreeView();
            }
            finally
            {
                EnableUI(true);
            }
        }

        //-----------------------------------------------------------------------------
        private static void AddFileToFolder(TreeViewProjectFilesNode parentNode, FileInfo fi, TreeViewProjectFileItemType leafType, string publishedUser = "")
        {
            string ns = BaseCustomizationContext.CustomizationContextInstance.GetParentPseudoNamespaceFromFullPath(fi.FullName);
            if (ns.IsNullOrEmpty())
                return;

            //dll e report sono aggiunti ad un sottonodo chiamato con il namespace del documento o del report
            //prima di aggiungere il file, cerco il nodo in questione...
            TreeViewProjectFilesNode documentNode = null;
            TreeNode[] nodes = parentNode.Nodes.Find(ns, false);
            if (nodes.Length == 1)
                documentNode = nodes[0] as TreeViewProjectFilesNode;
            else
            {
                //...o lo creo se non esiste
                documentNode = new TreeViewProjectFilesNode(ns, TreeViewProjectFileItemType.DocumentReport);
                parentNode.Nodes.Add(documentNode);
            }

            TreeViewProjectFilesNode file = new TreeViewProjectFilesNode(fi.Name, fi.FullName, leafType, publishedUser);
            //prima di aggiungere dll o report, verifico se è già presente
            TreeNode[] subNodes = documentNode.Nodes.Find(file.Text, false);
            if (subNodes == null || subNodes.Length <= 0)
                documentNode.Nodes.Add(file);
        }

        //-----------------------------------------------------------------------------
        private TreeViewProjectFilesNode GetNodeByType(string moduleName, TreeViewProjectFileItemType type)
        {
            //i child nodes del tree sono i moduli, cerco quello che mi interessa
            foreach (TreeViewProjectFilesNode module in tvFiles.Nodes)
            {
                if (!module.Tag.ToString().CompareNoCase(moduleName))
                    continue;

                //tra i child del modulo ci sono le folder che mi interessano
                foreach (TreeViewProjectFilesNode folder in module.Nodes)
                {
                    if (folder.NodeType == type)
                        return folder;
                }
            }
            return null;
        }

        /// <summary>
        /// Sul changed del file di customizzazione vengono caricati i file referenziati nella custom list
        /// </summary>
        //-----------------------------------------------------------------------------
        private void lvCustomizations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvApplications.SelectedItems.Count <= 0)
                return;

            if (currentApplicationName == lvApplications.SelectedItems[0].Name)
                return;

            currentApplicationName = lvApplications.SelectedItems[0].Name;
            FillModulesTreeView();
        }

        //-----------------------------------------------------------------------------
        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            chApplications.Width = splitContainer1.Panel1.Width - 1;
        }

        //-----------------------------------------------------------------------------
        private TreeViewProjectFilesNode FindModuleNode(TreeViewProjectFilesNode currentNode)
        {
            if (currentNode == null)
                return null;

            if (currentNode.NodeType == TreeViewProjectFileItemType.Module)
                return currentNode;

            TreeViewProjectFilesNode parent = tvFiles.SelectedNode.Parent as TreeViewProjectFilesNode;
            while (parent != null)
            {
                if (parent.Parent == null || parent.NodeType == TreeViewProjectFileItemType.Module)
                    return parent;

                parent = parent.Parent as TreeViewProjectFilesNode;
            }
            return null;
        }

        //-----------------------------------------------------------------------------
        private void deleteItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteItem();
        }

        /// <summary>
        /// Dato il nodo corrente, riempie la lista pathsToRemove con tutti i sottopath figli del nodo stesso
        /// (Metodo ricorsivo)
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="currentNode"></param>
        //-----------------------------------------------------------------------------
        private void FindSubNodesFilePaths(TreeViewProjectFilesNode currentNode, ref List<string> filePaths)
        {
            if (!string.IsNullOrEmpty(currentNode.FileFullPath))
                filePaths.Add(currentNode.FileFullPath);

            foreach (TreeViewProjectFilesNode item in currentNode.Nodes)
                FindSubNodesFilePaths(item, ref filePaths);
        }

        //-----------------------------------------------------------------------------
        private void CustomListViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Serializzo le customizzazioni che sono state modificate
            foreach (Customization current in BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications)
            {
                //Salvo la custom list
                if (current != null)
                    current.EasyBuilderAppFileListManager.SaveCustomList();
            }
        }

        //-----------------------------------------------------------------------------
        private void cmsFilesManagement_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Point mousePos = tvFiles.PointToClient(MousePosition);

            TreeViewProjectFilesNode node = tvFiles.GetNodeAt(mousePos.X, mousePos.Y) as TreeViewProjectFilesNode;
            tvFiles.SelectedNode = node;

            UpdateTreeViewMenus(node);
        }

        //-----------------------------------------------------------------------------
        private void cmsFilesManagement_Opened(object sender, EventArgs e)
        {
            //Se neanche un menu item è visibile allora non mostro il menu
            bool shouldMenuBeVisible = false;
            foreach (ToolStripMenuItem menuItem in cmsFilesManagement.Items)
            {
                if (menuItem.Visible)
                {
                    shouldMenuBeVisible = true;
                    break;
                }
            }
            cmsFilesManagement.Visible = !shouldMenuBeVisible;
        }

        //-----------------------------------------------------------------------------
        private void tsExportPackage_Click(object sender, EventArgs e)
        {
            ExportPackage();
        }

        //-----------------------------------------------------------------------------
        private void tsImportPackage_Click(object sender, EventArgs e)
        {
            ImportPackage();
        }

        //-----------------------------------------------------------------------------
        private void ExportPackage()
        {
            if (!EBLicenseManager.CanPack)
            {
                MessageBox.Show(
                        this,
                        Resources.FunctionalityNotAllowedWithCurrentActivation,
                        NameSolverStrings.EasyStudioDesigner,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation
                        );
                return;
            }

            bool existsCustomizationNotPublished = false;
            //Prima di esportare il package effettuo il salvataggio
            foreach (IEasyBuilderApp easyBuilderApp in BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(currentApplicationName))
            {
                easyBuilderApp.EasyBuilderAppFileListManager.SaveCustomList();
                if (easyBuilderApp.ApplicationType == ApplicationType.Customization && !existsCustomizationNotPublished)
                {
                    foreach (CustomListItem item in easyBuilderApp.EasyBuilderAppFileListManager.CustomList)
                    {
                        if (!String.IsNullOrWhiteSpace(item.PublishedUser))
                        {
                            existsCustomizationNotPublished = true;
                            break;
                        }
                    }
                }
            }

            //Avverto l'utente se ci sono customizzazioni non pubblicate (che quindi non finiscono nel pacchetto)
            if (existsCustomizationNotPublished)
            {
                if (MessageBox.Show(this, Resources.UnpublishedCustomization, Resources.ExportPackage, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }
            //Chiedo il filepath del package da esportare
            ImportExportPackage iep = new ImportExportPackage(ImportExportPackageType.Export, currentApplicationName);
            DialogResult result = iep.ShowDialog(this);
            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            //Se il path selezionato è vuoto non faccio niente
            if (iep.FullFileName.IsNullOrEmpty())
                return;

            //altrimenti esporto il package con il nome selezionato (su un altro thread)
            new Thread(() => { CompressCustomization(iep.FullFileName); }).Start();
        }

        //-----------------------------------------------------------------------------
        private void ImportPackage()
        {
            bool deleteBeforeImporting = false;
            //Chiedo il filepath del package da importare
            ImportExportPackage iep = new ImportExportPackage(ImportExportPackageType.Import, currentApplicationName);
            DialogResult result = iep.ShowDialog(this);
            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            //Se il path selezionato è vuoto non faccio niente
            if (iep.FullFileName.IsNullOrEmpty())
                return;

            EbpInfo ebpInfo = EbpInfo.InspectEbp(iep.FullFileName);

            //Se l'applicazione esiste già, chiedo cosa si vuole fare, 
            if (BaseCustomizationContext.CustomizationContextInstance.IsApplicationAlreadyExisting(ebpInfo.ApplicationName))
            {
                DialogResult res = MessageBox.Show
                    (
                    string.Format(Resources.ExistingApplicationDelete, ebpInfo.ApplicationName),
                    Resources.ImportPackage,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2
                    );

                if (res == System.Windows.Forms.DialogResult.Cancel)
                    return;

                //se si risponde "Si" viene cancellata la customizzazione prima di importare
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    DialogResult deletionRes = MessageBox.Show(
                                    this,
                                    Resources.ConfirmDeleteCustomization,
                                    Resources.DeleteCustomizationCaption,
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Question
                                    );

                    if (deletionRes != System.Windows.Forms.DialogResult.OK)
                        return;

                    deleteBeforeImporting = true;
                }

                //altrimenti rimane tutto inalterato, cadaveri compresi
            }

            //Controlli preliminari prima di installare il pacchetto.
            List<string> messages = new List<string>();
            PreliminaryChecks(ebpInfo, messages);

            if (messages.Count > 0)
            {
                MessageBox.Show(
                    this,
                    String.Join(Environment.NewLine, messages),
                    Resources.ImportPackage,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                    );
                return;
            }
            //Fine controlli preliminari
            lvApplications.Enabled = false;
            tvFiles.Enabled = false;

            //altrimenti importo il package con il nome selezionato (su un altro thread)
            new Thread(() => { UncompressCustomization(ebpInfo, deleteBeforeImporting); }).Start();

            //Invalido la cache per far si che vengano ricaricati i menu e rigenerata la dll degli enumerativi.
            BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
        }

        //---------------------------------------------------------------------
        private static void PreliminaryChecks(
            EbpInfo ebpInfo,
            IList<string> messages
            )
        {
            List<EnumItem> warningItems = new List<EnumItem>();
            //Se il pacchetto contiene enumerativi allora controllo
            //che non ci siano conflitti con enumerativi già installati.
            if (ebpInfo.ContainsEnums)
            {
                //Carico gli enumerativi installati
                Enums enums = new Enums();
                enums.LoadXml(false);

                EnumTag currentTag = null;
                EnumItem currentItemByName = null;
                String ownerModuleName = null;
                String ownerApplicationName = null;
                //Ciclo sugli enumerativi apportati dal pacchetto
                foreach (EnumTag enumTag in ebpInfo.EnumTags)
                {
                    //Se il pacchetto apporta un tag che non è installato => ok
                    currentTag = enums.Tags.GetTag(enumTag.Name);
                    if (currentTag == null)
                        continue;

                    //Altrimenti guardo se ci siano conflitti di item
                    foreach (EnumItem item in enumTag.EnumItems)
                    {
                        //Se esiste un item con lo stesso nome...
                        currentItemByName = currentTag.GetItemByName(item.Name);
                        if (currentItemByName != null)
                        {
                            ownerModuleName = currentItemByName.OwnerModule.Name;
                            ownerApplicationName = currentItemByName.OwnerModule.ParentApplicationInfo.Name;

                            //...che non è apportato dalla stessa <applicazione, modulo> => warning
                            if (ebpInfo.ApplicationName != ownerApplicationName || !ebpInfo.ModulesName.Contains(ownerModuleName))
                            {
                                warningItems.Add(currentItemByName);
                                continue;
                            }
                        }

                        //Se esiste un item con lo stesso valore => warning
                        if (currentTag.ExistItem(item.Value))
                        {
                            if (ebpInfo.ApplicationName != ownerApplicationName || !ebpInfo.ModulesName.Contains(ownerModuleName))
                            {
                                warningItems.Add(item);
                            }
                        }
                    }
                }
            }

            if (warningItems.Count > 0)
                messages.Add(Resources.EnumConflict);
        }

        //-----------------------------------------------------------------------------
        private void SetStatusBarInfo(int maxProgressBarElement, string statusBarText)
        {
            BeginInvoke(new MethodInvoker(() =>
            {
                tsStatus.Text = statusBarText;
                tsProgressBar.Visible = true;
                tsProgressBar.Value = 0;
                tsProgressBar.Maximum = maxProgressBarElement;
                statusStrip1.Refresh();
            }));
        }

        //-----------------------------------------------------------------------------
        private void SetStatusBarText(string statusBarText)
        {
            Invoke(new MethodInvoker(() =>
            {
                tsStatus.Text = statusBarText;
                statusStrip1.Refresh();
            }));
        }

        //-----------------------------------------------------------------------------
        void cf_CompressedFileClose(object sender, EventArgs e)
        {
            ResetProgressBar(Resources.PackingCompleted);
        }

        //-----------------------------------------------------------------------------
        void cf_BeginCompressFile(object sender, CompressEventArgs arg)
        {
            BeginInvoke
            (
                new MethodInvoker(() =>
                {
                    if (tsProgressBar.Value < tsProgressBar.Maximum)
                        tsProgressBar.Value++;

                    tsCurrentFile.Text = arg.CurrentProcessingFileName;
                    statusStrip1.Refresh();
                })
            );
        }

        //-----------------------------------------------------------------------------
        private void UncompressCustomization(EbpInfo ebpInfo, bool deleteBeforeImporting)
        {
            try
            {
                //Se devo cancellare l'applicazione esistente prima di importare la nuova perche' ha la stesso nome,
                //cancello tutti i file dell'applicazione su file system per evitare "cadaveri" della vecchia applicazione
                if (deleteBeforeImporting)
                {
                    BaseCustomizationContext.CustomizationContextInstance.DeleteApplication(ebpInfo.ApplicationName);
                }

                ebpInfo.BeginUncompressFile += new EventHandler<CompressEventArgs>(EbpInfo_BeginUncompressFile);
                ebpInfo.CompanyRelatedFileDetected += new EventHandler<CompanyRelatedFileDetectedEventArgs>(EbpInfo_CompanyRelatedFileDetected);
                ebpInfo.FileAlreadyExists += new EventHandler<FileAlreadyExistsEventArgs>(EbpInfo_FileAlreadyExists);
                ebpInfo.UnpackStarted += new EventHandler<UnpackStartedEventArgs>(EbpInfo_UnpackStarted);
                ebpInfo.UnpackEnded += new EventHandler<EventArgs>(EbpInfo_UnpackEnded);

                List<IRenamedCompany> renamedCompanies = new List<IRenamedCompany>();
                OverwriteResult overWriteResult = OverwriteResult.None;
                //Estraggo i file del package
                ebpInfo.ExtractFiles(renamedCompanies, ref overWriteResult);

                //applico eventuali modifiche a file di azienda o altro ai file appena unzippati
                ApplyUncompressedCustomization(
                    ebpInfo.ApplicationName,
                    renamedCompanies,
                    ebpInfo.EbpFiles,
                    overWriteResult,
                    ebpInfo.ApplicationType
                    );

                ebpInfo.BeginUncompressFile -= new EventHandler<CompressEventArgs>(EbpInfo_BeginUncompressFile);
                ebpInfo.CompanyRelatedFileDetected -= new EventHandler<CompanyRelatedFileDetectedEventArgs>(EbpInfo_CompanyRelatedFileDetected);
                ebpInfo.FileAlreadyExists -= new EventHandler<FileAlreadyExistsEventArgs>(EbpInfo_FileAlreadyExists);
                ebpInfo.UnpackStarted -= new EventHandler<UnpackStartedEventArgs>(EbpInfo_UnpackStarted);
                ebpInfo.UnpackEnded -= new EventHandler<EventArgs>(EbpInfo_UnpackEnded);
            }
            catch (Exception exc)
            {
                BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(exc.ToString());
            }

        }

        //-----------------------------------------------------------------------------
        void EbpInfo_UnpackStarted(object sender, UnpackStartedEventArgs e)
        {
            SetStatusBarInfo(e.FilesNumber, Resources.StartUnpacking);
        }

        //-----------------------------------------------------------------------------
        void EbpInfo_UnpackEnded(object sender, EventArgs e)
        {
            SetStatusBarText(Resources.UnpackingCompleted);
        }

        //-----------------------------------------------------------------------------
        void EbpInfo_FileAlreadyExists(object sender, FileAlreadyExistsEventArgs e)
        {
            Invoke
            (
                new MethodInvoker(() =>
                {
                    OverwriteFileWindow or = new OverwriteFileWindow(e.FullFilePath);
                    or.ShowDialog(this);
                    e.OverwriteResult = or.Result;
                }
            )
            );
        }

        //-----------------------------------------------------------------------------
        void EbpInfo_CompressedFileClose(object sender, EventArgs e)
        {
            ResetProgressBar(Resources.UnpackingCompleted);
            BeginInvoke
            (
                new MethodInvoker(() =>
                {
                    FillApplicationList();
                })
            );
        }

        //-----------------------------------------------------------------------------
        void EbpInfo_CompanyRelatedFileDetected(object sender, CompanyRelatedFileDetectedEventArgs e)
        {
            Invoke
            (
                new MethodInvoker(() =>
                {
                    RenameCompanyFolders rcf = new RenameCompanyFolders(e.OldCompanyName);
                    rcf.ShowDialog(this);

                    e.NewCompanyName = rcf.NewCompanyName;
                }
            )
            );
        }

        //-----------------------------------------------------------------------------
        void EbpInfo_BeginUncompressFile(object sender, CompressEventArgs e)
        {
            BeginInvoke
            (
                new MethodInvoker(() =>
                {
                    tsProgressBar.Value++;
                    tsCurrentFile.Text = e.CurrentProcessingFileName;
                    statusStrip1.Refresh();
                })
            );
        }

        //-----------------------------------------------------------------------------
        private void ApplyUncompressedCustomization(
            string applicationName,
            List<IRenamedCompany> renamedCompanies,
            IList<string> customFiles,
            OverwriteResult overWriteResult,
            ApplicationType applicationType
            )
        {
            SetStatusBarInfo(10, Resources.Loading);

            //Mostra una progress bar che si riempie mano a mano che vengono caricati i file della standardizzazione.
            bool loading = true;
            Task.Factory.StartNew(
                new Action(
                    () =>
                    {
                        while (loading)
                        {
                            Thread.Sleep(100);
                            BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        if (tsProgressBar.Value == tsProgressBar.Maximum)
                                            tsProgressBar.Value = 0;
                                        tsProgressBar.Value++;

                                        statusStrip1.Refresh();
                                    }));
                        }
                    }));


            //Rinomino i path nella temp al loro naturale posto nella custom
            RenameCompanyFolders(renamedCompanies, overWriteResult);

            //stabilisco in base al file di custom list che sto caricando qual'è l'applicazione e il modulo
            foreach (string currentCustomFile in customFiles)
            {
                //se la customizzazione non esiste ancora, la creo, assieme alle sue application infos
                string application, module = string.Empty;
                BaseCustomizationContext.CustomizationContextInstance.GetApplicationAndModuleFromCustomFile(currentCustomFile, out application, out module);

                //Non uso la AddNewCustomization perchè nel caso stia importando la customizzazione su 
                //una esistente la addnew mi tornerebbe quella esistente, invece io devo caricare quella 
                //appena importata
                IEasyBuilderApp easyBuilderApp = BaseCustomizationContext.CustomizationContextInstance.Import(application, module, applicationType);

                //Rimuovo la vecchia customizzazione dall'array
                if (BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications.Contains(easyBuilderApp))
                    BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications.Remove(easyBuilderApp);

                //e aggiungo la nuova
                BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications.Add(easyBuilderApp);

                //Frugo nella custom list e rinomino anche i path interni
                easyBuilderApp.EasyBuilderAppFileListManager.RenameCompanyPathsInCustomList(renamedCompanies);
                easyBuilderApp.EasyBuilderAppFileListManager.PurgeCustomListFromNonExistingFiles();
            }

            BasePathFinder.BasePathFinderInstance.RefreshEasyBuilderApps(applicationType);

            loading = false;
            ResetProgressBar(String.Empty);

            Invoke(new MethodInvoker(() =>
            {
                lvApplications.Enabled = true;
                tvFiles.Enabled = true;

                ResetProgressBar(Resources.UnpackingCompleted);
                //Ricarico tutto il tree
                FillApplicationList();

                UpdateButtons();

                //Se tra le customizzazioni importate ci sono moduli con script allegati segnalo che
                //sarebbe meglio ricaricare mago dopo aver applicato gli script dalla console
                if (IsApplicationContainsScripts(applicationName))
                    MessageBox.Show(Resources.YouShoudRestartAfterUnpack, Resources.UnpackingCompleted, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                //infine seleziono la customizzazione appena importata
                if (lvApplications.Items[applicationName] != null)
                    lvApplications.Items[applicationName].Selected = true;

                CUtility.ReloadAllMenus();
            }));

            CUtility.ReloadApplication(applicationName);
            //Se si e' importato un verticale bisogna reinizializzare login manager perche veda i file di solution e module
            if (applicationType == ApplicationType.Standardization)
            {
                loginManager.ReloadConfiguration();
            }
        }

        //-----------------------------------------------------------------------------
        private void ResetProgressBar(string text)
        {
            BeginInvoke
                     (
                         new MethodInvoker(() =>
                         {
                             tsProgressBar.Value = tsProgressBar.Maximum;
                             tsProgressBar.Visible = false;
                             tsStatus.Text = text;
                             tsCurrentFile.Text = String.Empty;
                             statusStrip1.Refresh();
                         })
                     );
        }

        /// <summary>
        /// Rinomino i folder delle aziende presenti nel file zip con i nuovi nome scelti dall'utente
        /// </summary>
        //-----------------------------------------------------------------------------
        private void RenameCompanyFolders(List<IRenamedCompany> renamedCompanies, OverwriteResult overWriteResult)
        {
            foreach (RenamedCompany item in renamedCompanies)
            {
                string sourceDir = Path.Combine(Path.GetTempPath() + NameSolverStrings.Subscription, item.OldCompanyName);
                string destDir = Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomCompaniesPath(), item.NewCompanyName);

                DirectoryInfo source = new DirectoryInfo(sourceDir);
                DirectoryInfo target = new DirectoryInfo(destDir);

                try
                {
                    if (Directory.Exists(sourceDir))
                        CopyAll(source, target, overWriteResult);
                }
                catch (Exception exc)
                {
                    string message = string.Format(Resources.UnableToMoveFile, source.Name, target.Name, exc.Message);
                    BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(message);
                }

                try
                {
                    Directory.Delete(sourceDir, true);
                }
                catch (Exception)
                {
                }
            }
        }

        //-----------------------------------------------------------------------------
        internal void CopyAll(DirectoryInfo source, DirectoryInfo target, OverwriteResult overWriteResult)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
                Directory.CreateDirectory(target.FullName);

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                string file = Path.Combine(target.ToString(), fi.Name);
                if (SkipExistingFileForOverwrite(file, ref overWriteResult))
                    continue;

                fi.CopyTo(file, true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = !Directory.Exists(diSourceSubDir.Name)
                        ? target.CreateSubdirectory(diSourceSubDir.Name)
                        : new DirectoryInfo(diSourceSubDir.Name);

                CopyAll(diSourceSubDir, nextTargetSubDir, overWriteResult);
            }
        }

        /// <summary>
        /// Se il file esiste già in base al valore del parametro overWriteResult fa apparire una finestra di 
        /// messaggio che chiede cosa fare per la sovrascrittura (yes, no, yestoall, notoall) e memorizza questa
        /// risposta per i file successivi se necessario
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <param name="overWriteResult"></param>
        /// <returns></returns>
        //-----------------------------------------------------------------------------
        private bool SkipExistingFileForOverwrite(string fileFullName, ref OverwriteResult overWriteResult)
        {
            if (File.Exists(fileFullName) &&
                (
                overWriteResult == OverwriteResult.None ||
                overWriteResult == OverwriteResult.Yes ||
                overWriteResult == OverwriteResult.No
                )
                )
            {
                OverwriteFileWindow or = new OverwriteFileWindow(fileFullName);
                this.Invoke(new Action(
                    ()
                    =>
                    {
                        or.ShowDialog(this);
                    }
                ));
                overWriteResult = or.Result;
            }

            if (
                File.Exists(fileFullName) &&
                (overWriteResult == OverwriteResult.No || overWriteResult == OverwriteResult.NoToAll)
                )
                return true;

            return false;
        }

        //-----------------------------------------------------------------------------
        private void CompressCustomization(string outputFileName)
        {
            EbpInfo ebpInfo = EbpInfo.CreateEbp(outputFileName);

            ebpInfo.BeginCompressFile += new EventHandler<CompressEventArgs>(cf_BeginCompressFile);
            ebpInfo.EndCompressFile += new EventHandler<CompressEventArgs>(cf_CompressedFileClose);

            ebpInfo.CompressFiles(BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(currentApplicationName));

            ebpInfo.BeginCompressFile -= new EventHandler<CompressEventArgs>(cf_BeginCompressFile);
            ebpInfo.EndCompressFile -= new EventHandler<CompressEventArgs>(cf_CompressedFileClose);
        }

        //-----------------------------------------------------------------------------
        private void UpdateButtons()
        {
            try
            {
                tsShowDiagnostic.Enabled =
                    BaseCustomizationContext.CustomizationContextInstance.Diagnostic.TotalErrors > 0 ||
                    BaseCustomizationContext.CustomizationContextInstance.Diagnostic.TotalWarnings > 0 ||
                    BaseCustomizationContext.CustomizationContextInstance.Diagnostic.TotalInformations > 0;

                int selectedItemsCount = lvApplications.SelectedItems == null ? 0 : lvApplications.SelectedItems.Count;

                ApplicationType appType = ApplicationType.Undefined;
                if (selectedItemsCount > 0)
                    appType = ((ApplicationType)lvApplications.SelectedItems[0].Tag);

                tsmiDeleteApplication.Visible = true;
                tsmiDeleteApplication.Enabled = selectedItemsCount > 0 && appType != ApplicationType.Undefined;
                tsmiRenameApplication.Visible = EBLicenseManager.CanDesign && isEasyBuilderDeveloper;

                //disabilito il rename application se sono una standardizzazione oppure se non ci sono applicazioni selezionate
                tsmiRenameApplication.Enabled = selectedItemsCount > 0 && !IsStandardization();

                tsExportApplication.Visible = EBLicenseManager.CanPack && isEasyBuilderDeveloper;

                tsOptions.Visible = EBLicenseManager.CanDesign;

                UpdateTreeViewMenus();
            }
            catch (Exception)
            {
            }
        }

        //-----------------------------------------------------------------------------
        private bool IsStandardization()
        {
            IList<IEasyBuilderApp> appList = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(lvApplications.SelectedItems[0].Text);

            return appList.Count > 0 && appList[0].ApplicationType == ApplicationType.Standardization;
        }

        //-----------------------------------------------------------------------------
        private void UpdateTreeViewMenus(TreeViewProjectFilesNode tvNode = null)
        {
            IEasyBuilderApp cust = null;
            if (tvNode != null && tvNode.NodeType == TreeViewProjectFileItemType.Module)
                cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(currentApplicationName, tvNode.Tag.ToString());

            if (tvNode == null && tvFiles.SelectedNode != null)
                tvNode = tvFiles.SelectedNode as TreeViewProjectFilesNode;

            AddApplicationToolstripButton.Visible = EBLicenseManager.CanDesign && isEasyBuilderDeveloper;

            tsmiDeleteItem.Visible = EBLicenseManager.CanDesign && isEasyBuilderDeveloper;
            tsmiDeleteItem.Enabled =
                tvNode != null && tvNode.IsActivated && !string.IsNullOrEmpty(tvNode.FileFullPath);

            tsmiDeleteModule.Visible = isEasyBuilderDeveloper;
            tsmiDeleteModule.Enabled =
                tvNode != null && tvNode.IsActivated && tvNode.NodeType == TreeViewProjectFileItemType.Module;

            tsmiRenameModule.Visible = EBLicenseManager.CanDesign && isEasyBuilderDeveloper;
            tsmiRenameModule.Enabled =
                tvNode != null && tvNode.IsActivated && tvNode.NodeType == TreeViewProjectFileItemType.Module;

            tsmiSetActiveDocument.Visible = EBLicenseManager.CanDesign && isEasyBuilderDeveloper;
            tsmiSetActiveDocument.Enabled =
                tvNode != null && tvNode != null && tvNode.NodeType == TreeViewProjectFileItemType.DocumentCustomization && tvNode.IsActivated && !tvNode.IsActiveDocument;

            packModuleAssembliesToolStripMenuItem.Visible = EBLicenseManager.CanDesign && isEasyBuilderDeveloper && EBLicenseManager.CanPack && cust != null && (cust.ApplicationType == ApplicationType.Standardization);
            packModuleAssembliesToolStripMenuItem.Enabled =
                tvNode != null && tvNode != null && tvNode.NodeType == TreeViewProjectFileItemType.Module;

            AddModuleToolStripButton.Visible = addModuleToolStripMenuItem.Visible =
                EBLicenseManager.CanDesign &&
                (
                lvApplications.SelectedItems != null && lvApplications.SelectedItems.Count > 0
                ) &&
                isEasyBuilderDeveloper;
            AddModuleToolStripButton.Enabled = addModuleToolStripMenuItem.Enabled =
                lvApplications.SelectedItems != null && lvApplications.SelectedItems.Count > 0;

            addModuleToolStripMenuItem.Visible = isEasyBuilderDeveloper;

            //e` possibile browse-are su un file anche se il relativo modulo non e` attivo.
            tsmiOpenFileLocation.Visible =
                (
                tvNode != null &&
                !tvNode.FileFullPath.IsNullOrEmpty()
                );

            tsmiPublish.Visible =
                EBLicenseManager.CanDesign &&
                tvNode != null &&
                BaseCustomizationContext.CustomizationContextInstance.IsSubjectedToPublication(tvNode.FileFullPath) &&
                !tvNode.PublishedUser.IsNullOrEmpty() &&
                isEasyBuilderDeveloper;

            tsmiPublish.Enabled =
                tvNode != null && tvNode.IsActivated;

            tsmiSetAsActiveModule.Visible =
                EBLicenseManager.CanDesign && tvNode != null &&
                tvNode.NodeType == TreeViewProjectFileItemType.Module &&
                cust != null &&
                cust.IsEnabled &&
                !(
				BaseCustomizationContext.CustomizationContextInstance.CurrentApplication.CompareNoCase(currentApplicationName) &&
				BaseCustomizationContext.CustomizationContextInstance.CurrentModule.CompareNoCase(tvNode.Tag.ToString())
                ) &&
                isEasyBuilderDeveloper;

            tsmiSetAsActiveModule.Enabled =
                tvNode != null && tvNode.IsActivated;

            tsmiDisableModule.Enabled = tvNode != null && tvNode.IsActivated;

            SetEnableDisableStatus(tvNode, cust);
        }

        //-----------------------------------------------------------------------------
        private void SetEnableDisableStatus(TreeViewProjectFilesNode tvNode, IEasyBuilderApp easyBuilderApp)
        {
            tsmiDisableModule.Visible =
                tvNode != null && tvNode.NodeType == TreeViewProjectFileItemType.Module &&
                !(
				BaseCustomizationContext.CustomizationContextInstance.CurrentApplication.CompareNoCase(currentApplicationName) &&
				BaseCustomizationContext.CustomizationContextInstance.CurrentModule.CompareNoCase(tvNode.Tag.ToString()) &&
                tvNode.IsEnabled
                ) &&
                isEasyBuilderDeveloper
                ;

            tsmiDisableModule.Text = (tvNode != null && tvNode.IsEnabled == true)
                ? Resources.DisableModule
                : Resources.EnableModule;

            tsmiDisableModule.Image = (tvNode != null && tvNode.IsEnabled == true)
                ? Resources.Off :
                Resources.On;

            if (tvNode != null && tvNode.NodeType == TreeViewProjectFileItemType.Module)
            {
                foreach (TreeViewProjectFilesNode node in tvFiles.Nodes)
                {
                    if (!node.Tag.ToString().CompareNoCase(tvNode.Tag.ToString()))
                        continue;

                    IEasyBuilderApp cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(currentApplicationName, tvNode.Tag.ToString());
                    if (cust != null)
                        node.IsEnabled = cust.IsEnabled;
                }
            }

            if (easyBuilderApp != null && (easyBuilderApp.ApplicationType == ApplicationType.Standardization))
            {
                //se il modulo e' disabilitato, e' inutile che controllo l'attivazione
                if (!tvNode.IsEnabled)
                    return;

                tvNode.IsActivated = CUtility.IsActivated(easyBuilderApp.ApplicationName, easyBuilderApp.ModuleName);

                if (!tvNode.IsActivated)
                {
                    if (!tvNode.Text.ContainsNoCase(" not activated"))
                        tvNode.Text = String.Concat(tvNode.Text, " not activated");
                }
                else
                    tvNode.Text = tvNode.Text.Replace(" not activated", String.Empty);
            }
        }

        //-----------------------------------------------------------------------------
        private void AddApplication()
        {
            try
            {
                //disabilita tutta la form per evitare interazione dell'utente durante le operazioni di background
                EnableUI(false);

                AddRenameCustomization ac = new AddRenameCustomization(CustomizationStatus.AddApplication);

                DialogResult result = ac.ShowDialog(this);
                if (result != System.Windows.Forms.DialogResult.OK)
                    return;

                if (
                    ac.CreateApplicationInStandardFolder &&
                    BaseCustomizationContext.CustomizationContextInstance.NotAlone(Resources.AddApplicationCaption, 1, 0, this)// &&
                    //!TestCredentialsForSalesModulesCrypting() Non testate le credenziali perchè già testate da AddRenameCustomization
                    )
                    return;
                bool ret = false;

                ShowWaitingWndWhileExecutingAction(
                    (Action)delegate
                    {
                        ApplicationType appType = ApplicationType.Customization;
                        if (ac.CreateApplicationInStandardFolder)
                            appType = ApplicationType.Standardization;

                        ret = !BaseCustomizationContext.CustomizationContextInstance.AddNewEasyBuilderApp(ac.ApplicationName, ac.ModuleName, appType);
                    },
                    String.Format(Resources.WaitingMessageCreation, ac.ApplicationName)
                );

                if (ret)
                    return;

                //Se ho già aggiunto l'applicazione non faccio niente
                if (!lvApplications.Items.ContainsKey(ac.ApplicationName))
                {
                    //e la aggiungo alla listview
                    ApplicationListViewItem newItem = new ApplicationListViewItem(ac.ApplicationName);

                    //icona per differenziare verticali da custumizzazioni presa dall'imagelist
                    IList<IEasyBuilderApp> custs = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(ac.ApplicationName);
                    newItem.IsStandardApplication = custs[0].ApplicationType == ApplicationType.Standardization;

                    //Mettiamo nel tag il tipo di applicazione EasyBuilder,
                    //ci viene comodo quando aggiungiamo un modulo ad un'applicazione che non ha moduli.
                    newItem.Tag = custs[0].ApplicationType;

                    lvApplications.Items.Add(newItem);
                    newItem.Selected = true;
                }

                //dopodichà riforzo il caricamento della finestra del packager
                FillModulesTreeView();
                //imposta l'applicazione appena aggiunta come attiva
                SetTreeViewModuleNodeAsActive(ac.ModuleName);
            }
            finally
            {
                EnableUI(true);
            }
        }

        //-----------------------------------------------------------------------------
        private bool CanRenameApplication(string applicationName)
        {
            //Se almeno un modulo ha degli script già applicati, non posso rinominare l'applicazione
            IList<IEasyBuilderApp> custs = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(applicationName);
            foreach (Customization cust in custs)
            {

                if (!cust.IsEnabled)
                {
                    MessageBox.Show(this, Resources.UnableToRenameApplicationDisabledModule,
                    Resources.RenameCustomization,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                    return false;
                }

                if (cust.ModuleInfo != null && cust.ModuleInfo.CurrentDBRelease > 0)
                {
                    MessageBox.Show(this, Resources.UnableToRenameApplication,
                    Resources.RenameCustomization,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                    return false;
                }
            }

            return true;
        }

        //-----------------------------------------------------------------------------
        private bool CanRenameModule(string moduleName)
        {
            //Se almeno un modulo ha degli script già applicati, non posso rinominare l'applicazione
            IEasyBuilderApp cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(currentApplicationName, moduleName);
            if (cust == null || cust.ModuleInfo == null || cust.ModuleInfo.CurrentDBRelease > 0)
                return false;

            return true;
        }

        //-----------------------------------------------------------------------------
        private static bool IsApplicationContainsScripts(string applicationName)
        {
            //Se l'applicazione contiene moduli che hanno script inclusi lo segnalo all'utente, poi
            //sono affari suoi
            IList<IEasyBuilderApp> custs = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderAppsByApplicationName(applicationName);
            foreach (IEasyBuilderApp cust in custs)
            {
                if (cust.EasyBuilderAppFileListManager.CustomList.ContainsNoCase(string.Format("\\{0}\\", NameSolverStrings.DatabaseScript)))
                    return true;
            }

            return false;
        }

        //-----------------------------------------------------------------------------
        private bool IsModuleContainsScripts(string module)
        {
            //Se l'applicazione contiene moduli che hanno script inclusi lo segnalo all'utente, poi
            //sono affari suoi
            IEasyBuilderApp cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(currentApplicationName, module);
            if (cust.EasyBuilderAppFileListManager.CustomList.ContainsNoCase(string.Format("\\{0}\\", NameSolverStrings.DatabaseScript)))
                return true;

            return false;
        }

        //-----------------------------------------------------------------------------
        private void RenameApplication()
        {
            try
            {
                //disabilita tutta la form per evitare interazione dell'utente durante le operazioni di background
                EnableUI(false);
                //Se sono già stati applicati scatti di release al modulo, non posso rinominare la customizzaione
                if (!CanRenameApplication(currentApplicationName))
                {
                    return;
                }

                if (IsApplicationContainsScripts(currentApplicationName))
                {
                    DialogResult res = MessageBox.Show(this, Resources.DoYouWantToRenameApplicationAnyway,
                        Resources.RenameCustomization,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                        );

                    if (res == System.Windows.Forms.DialogResult.No)
                        return;
                }

                string oldApplicationName = currentApplicationName;
                AddRenameCustomization ac = new AddRenameCustomization(CustomizationStatus.RenameApplication, null, currentApplicationName);
                DialogResult result = ac.ShowDialog(this);
                if (result != System.Windows.Forms.DialogResult.OK)
                    return;

                string newName = currentApplicationName = ac.ApplicationName;
                if (string.IsNullOrEmpty(newName))
                    return;

                ListViewItem item = lvApplications.SelectedItems[0] as ListViewItem;
                string oldName = item.Text;
                item.Text = item.Name = newName;

                ShowWaitingWndWhileExecutingAction(
                    (Action)delegate
                    {
                        BaseCustomizationContext.CustomizationContextInstance.RenameApplication(oldApplicationName, newName);
                    },
                    String.Format(Resources.WaitingMessageRename, oldApplicationName, newName)
                );

                //Se la customizzazione rinominata era di default, aggiorno il nome anche nei settings
				
                if (BaseCustomizationContext.CustomizationContextInstance.CurrentApplication == oldName)
                {
					BaseCustomizationContext.CustomizationContextInstance.CurrentApplication = newName;
					BaseCustomizationContext.CustomizationContextInstance.SaveSettings();
                }

                FillModulesTreeView();
            }
            finally
            {
                EnableUI(true);
            }
        }

        //Metodo che mostra una finestra di attesa per dare feedback all'utente e lancia l'action time consuming
        //in un altro thread
        //-----------------------------------------------------------------------------
        private static void ShowWaitingWndWhileExecutingAction(Action tsAction, string message)
        {
            WaitingWnd waitWnd = new WaitingWnd(message, 40);
            waitWnd.Show();
            Functions.DoParallelProcedure(tsAction);
            waitWnd.Close();
            waitWnd.Dispose();
        }

        //-----------------------------------------------------------------------------
        private void DeleteApplication()
        {
            try
            {
                //disabilita tutta la form per evitare interazione dell'utente durante le operazioni di background
                EnableUI(false);
                if (lvApplications.SelectedItems.Count == 0)
                    return;

                if (BaseCustomizationContext.CustomizationContextInstance.NotAlone(Resources.DeleteApplication, 1, 0, this))
                    return;

                DialogResult deletionRes = MessageBox.Show(
                                                    this,
                                                    Resources.ConfirmDeleteCustomization,
                                                    Resources.DeleteCustomizationCaption,
                                                    MessageBoxButtons.OKCancel,
                                                    MessageBoxIcon.Question
                                                    );

                if (deletionRes != System.Windows.Forms.DialogResult.OK)
                    return;

                //rimuovo l'elemento dalla listview
                ApplicationListViewItem removedItem = lvApplications.SelectedItems[0] as ApplicationListViewItem;
                lvApplications.Items.Remove(removedItem);
                tvFiles.Nodes.Clear();

                ShowWaitingWndWhileExecutingAction(
                    (Action)delegate
                    {
                        BaseCustomizationContext.CustomizationContextInstance.DeleteApplication(removedItem.Text);
                    },
                    String.Format(Resources.WaitingMessageDelete, removedItem.Text)
                );

                //Selezione la prima customizzazione della lista
                ApplicationListViewItem tempItem = null;
                if (lvApplications.Items.Count > 0)
                {
                    tempItem = lvApplications.Items[0] as ApplicationListViewItem;
                    BaseCustomizationContext.CustomizationContextInstance.ChangeEasyBuilderApp(tempItem.Name);
                    currentApplicationName = tempItem.Name;
                    tempItem.Selected = true;

                    //se l'applicazione cancellata era quella di attiva, imposto la prima della lista come attiva
                    //(perche deve esserci sempre un'applicazione attiva)
                    tempItem.IsActiveApplication = removedItem.IsActiveApplication;

                    //e se l'applicazione corrente era di default, cambio anche quella
                    if (BaseCustomizationContext.CustomizationContextInstance.CurrentApplication == removedItem.Text)
                    {
						BaseCustomizationContext.CustomizationContextInstance.CurrentApplication = tempItem.Text;
						BaseCustomizationContext.CustomizationContextInstance.SaveSettings();
                    }
                }
                else
                {
                    BaseCustomizationContext.CustomizationContextInstance.ChangeEasyBuilderApp(null);
                    currentApplicationName = null;
                }

                FillModulesTreeView();

                CUtility.RefreshMenuDocument();
            }
            finally
            {
                EnableUI(true);
                UpdateButtons();
            }
        }

        //-----------------------------------------------------------------------------
        private void DeleteItem()
        {
            if (tvFiles == null)
                return;

            //Se il nodo corrente non è un TreeViewProjectFilesNode
            //oppure se non riporta un path non posso lavorare.
            TreeViewProjectFilesNode currentNode = tvFiles.SelectedNode as TreeViewProjectFilesNode;
            if (currentNode == null || currentNode.FileFullPath.IsNullOrEmpty())
                return;

            //Se non recupero il nodo di modulo non posso lavorare eprchè mi serve il suo nome.
            TreeViewProjectFilesNode moduleNode = FindModuleNode(tvFiles.SelectedNode as TreeViewProjectFilesNode);
            if (moduleNode == null)
                return;

            if (MessageBox.Show(this, Resources.DeleteItemConfirm, Resources.DeleteItemCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.OK)
                return;

            IEasyBuilderApp cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(currentApplicationName, moduleNode.Tag.ToString());

            switch (currentNode.NodeType)
            {
                case TreeViewProjectFileItemType.DocumentCustomization:
                    BaseCustomizationContext.CustomizationContextInstance.DeleteDocumentCustomization(cust, currentNode.FileFullPath, currentNode.PublishedUser);
                    break;
                case TreeViewProjectFileItemType.Menu:
                    BaseCustomizationContext.CustomizationContextInstance.DeleteMenu(cust, currentNode.FileFullPath);
                    break;
                default:
                    cust.EasyBuilderAppFileListManager.RemoveFromCustomListAndFromFileSystem(currentNode.FileFullPath);
                    break;
            }

            //Mi tengo da parte il nodo parent prima di cancellare il current dal tree
            TreeViewProjectFilesNode parentNode = currentNode.Parent as TreeViewProjectFilesNode;

            //devo rimuovere il nodo corrente e tutti i suoi sotto nodi, e togliere dalla custom list i file figli
            //rimuovo il file dalla lista
            tvFiles.Nodes.Remove(currentNode);

            IDisposable disposable = currentNode as IDisposable;
            if (disposable != null)
                disposable.Dispose();

            //Se il nodo padre dell'elemento che sto cancellando non ha più figli, cancello anche lui
            if (parentNode != null && parentNode.Nodes.Count <= 0)
            {
                tvFiles.Nodes.Remove(parentNode);

                disposable = parentNode as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            UpdateButtons();
        }

        //-----------------------------------------------------------------------------
        private void PublishDocument(TreeViewProjectFilesNode documentNode)
        {
            PublishDocument(documentNode, true);
        }

        //-----------------------------------------------------------------------------
        private void PublishDocument(TreeViewProjectFilesNode documentNode, bool refreshModulesTree)
        {
            if (documentNode == null || documentNode.NodeType != TreeViewProjectFileItemType.DocumentCustomization)
                return;

            if (documentNode.FileFullPath.IsNullOrEmpty())
                return;

            BaseCustomizationContext.CustomizationContextInstance.PublishDocument(Path.GetDirectoryName(documentNode.FileFullPath), Path.GetFileNameWithoutExtension(documentNode.FileFullPath), documentNode.PublishedUser, documentNode.IsActiveDocument);

            if (refreshModulesTree)
                FillModulesTreeView();
        }

        //-----------------------------------------------------------------------------
        private void OpenFileLocation()
        {
            if (tvFiles.SelectedNode == null || tvFiles.SelectedNode as TreeViewProjectFilesNode == null)
                return;

            TreeViewProjectFilesNode currentNode = tvFiles.SelectedNode as TreeViewProjectFilesNode;
            if (currentNode.FileFullPath.IsNullOrEmpty())
                return;

            try
            {
                FileInfo fi = new FileInfo(currentNode.FileFullPath);
                Process.Start("Explorer.exe", fi.DirectoryName);
            }
            catch (Exception)
            {
            }
        }

        //-----------------------------------------------------------------------------
        private void OpenPackingOptions()
        {
            PackingOptions p = new PackingOptions(EBLicenseManager.CanDesign);
            DialogResult result = p.ShowDialog(this);

            //Se le opzioni sono state modificate, ricarico le customizzazioni
            if (result == System.Windows.Forms.DialogResult.OK)
                FillModulesTreeView();
        }

        //-----------------------------------------------------------------------------
        private void tvFiles_MouseUp(object sender, MouseEventArgs e)
        {
            TreeNode node = tvFiles.GetNodeAt(e.X, e.Y);
            tvFiles.SelectedNode = node;
        }

        //-----------------------------------------------------------------------------
        private void renameCustomizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameApplication();
        }

        //-----------------------------------------------------------------------------
        private void tvFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateButtons();
        }

        //-----------------------------------------------------------------------------
        private void deleteCustomizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteApplication();
        }

        //-----------------------------------------------------------------------------
        private void tsOptions_Click(object sender, EventArgs e)
        {
            OpenPackingOptions();
        }

        //-----------------------------------------------------------------------------
        private void tsShowDiagnostic_Click(object sender, EventArgs e)
        {
            if (BaseCustomizationContext.CustomizationContextInstance.Diagnostic.Elements.Count == 0)
                return;

            DiagnosticViewer.ShowDiagnostic(BaseCustomizationContext.CustomizationContextInstance.Diagnostic);
        }

        //-----------------------------------------------------------------------------
        private void ofdAddFile_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (string item in ofdAddFile.FileNames)
            {
                //Se il path fa parte della "Custom" allora tutto ok
                if (item.ContainsNoCase(BasePathFinder.BasePathFinderInstance.GetCustomPath()))
                    continue;

                //altrimenti impedisco la scelta del file e la chiusura della finestra
                MessageBox.Show(this,
                    Resources.UnableToAddFileNotInCustom,
                    Resources.ErrorAddingFile,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
                e.Cancel = true;
                return;
            }
        }

        //-----------------------------------------------------------------------------
        private void cmsApplications_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UpdateButtons();
        }

        //-----------------------------------------------------------------------------
        private void setAsActiveModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetModuleAsActive();
        }

        //-----------------------------------------------------------------------------
        private void deleteModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteModule();
        }

        //-----------------------------------------------------------------------------
        private void renameModuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameModule();
        }

        //-----------------------------------------------------------------------------
        private void setActiveDocumentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetActiveDoc();
        }

        //-----------------------------------------------------------------------------
        private void SetActiveDoc()
        {
            if (tvFiles.SelectedNode == null || tvFiles.SelectedNode as TreeViewProjectFilesNode == null)
                return;

            TreeViewProjectFilesNode currentNode = tvFiles.SelectedNode as TreeViewProjectFilesNode;
            if (currentNode.NodeType != TreeViewProjectFileItemType.DocumentCustomization || currentNode.IsActiveDocument)
                return;

            TreeViewProjectFilesNode moduleNode = FindModuleNode(currentNode);
            if (moduleNode == null)
                return;

            //Cerco la custom list su cui fare le modifiche
            IEasyBuilderApp cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(currentApplicationName, moduleNode.Tag.ToString());
            if (cust == null)
                return;

            cust.SaveActiveDocument(currentNode.FileFullPath, currentNode.PublishedUser);

            //azzero l'active document dello stesso nodo e tolgo isActiveDocument da tutti i nodi fratelli
            TreeViewProjectFilesNode parentNode = currentNode.Parent as TreeViewProjectFilesNode;
            foreach (TreeViewProjectFilesNode item in parentNode.Nodes)
                item.IsActiveDocument = false;

            //e imposto a true quello che mi interessa
            currentNode.IsActiveDocument = true;

            UpdateButtons();
        }

        //-----------------------------------------------------------------------------
        private void tsDisableModule_Click(object sender, EventArgs e)
        {
            EnableDisableCustomization();
        }

        //-----------------------------------------------------------------------------
        private void tsmiDisableModule_Click(object sender, EventArgs e)
        {
            EnableDisableCustomization();
        }

        //-----------------------------------------------------------------------------
        private void EnableDisableCustomization()
        {
            if
                (
                tvFiles.SelectedNode == null ||
                (tvFiles.SelectedNode as TreeViewProjectFilesNode) == null ||
                (tvFiles.SelectedNode as TreeViewProjectFilesNode).NodeType != TreeViewProjectFileItemType.Module
                )
                return;

            string moduleName = (tvFiles.SelectedNode as TreeViewProjectFilesNode).Tag.ToString();
            IEasyBuilderApp cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(currentApplicationName, moduleName);

            if (cust == null)
            {
                BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(Resources.EasyBuilderAppNotFound);
                return;
            }
            if (cust.IsEnabled)
            {
                (tvFiles.SelectedNode as TreeViewProjectFilesNode).IsEnabled = false;
                cust.DisableModule();
            }
            else
            {
                (tvFiles.SelectedNode as TreeViewProjectFilesNode).IsEnabled = true;
                cust.EnableModule();
            }

            //Sull'abilitazione/disabilitazione del modulo deve allineare la struttura in memoria delle applicazioni
            BasePathFinder.BasePathFinderInstance.RefreshEasyBuilderApps(cust.ApplicationType);

            //Invalido la cache del menu affinchè venga ricaricato presentando nascondendo il menu apportato dal modulo.
            BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
            CUtility.ReloadAllMenus();


            UpdateButtons();
        }

        //-----------------------------------------------------------------------------
        private void tsmExport_Click(object sender, EventArgs e)
        {
            ExportPackage();
        }

        //-----------------------------------------------------------------------------
        private void tsmImport_Click(object sender, EventArgs e)
        {
            ImportPackage();
        }

        //-----------------------------------------------------------------------------
        private void tsmOption_Click(object sender, EventArgs e)
        {
            OpenPackingOptions();
        }

        //-----------------------------------------------------------------------------
        private void tsmShowDiagnostic_Click(object sender, EventArgs e)
        {
            if (BaseCustomizationContext.CustomizationContextInstance.Diagnostic.Elements.Count == 0)
                return;

            DiagnosticViewer.ShowDiagnostic(BaseCustomizationContext.CustomizationContextInstance.Diagnostic);
        }

        //-----------------------------------------------------------------------------
        private void tsmExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //-----------------------------------------------------------------------------
        private void tsRefresh_Click(object sender, EventArgs e)
        {
            FillApplicationList();
        }

        //-----------------------------------------------------------------------------
        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileLocation();
        }

        //-----------------------------------------------------------------------------
        private void tsmiPublishCustomization_Click(object sender, EventArgs e)
        {
            PublishItem(tvFiles.SelectedNode as TreeViewProjectFilesNode);
        }

        //-----------------------------------------------------------------------------
        private void PublishItem(TreeViewProjectFilesNode itemNode)
        {
            PublishItem(itemNode, true);
        }

        //-----------------------------------------------------------------------------
        private void PublishItem(TreeViewProjectFilesNode itemNode, bool refreshModulesTree)
        {
            if (itemNode == null)
                return;

            switch (itemNode.NodeType)
            {
                case TreeViewProjectFileItemType.DocumentCustomization:
                    PublishDocument(itemNode, refreshModulesTree);
                    break;
                case TreeViewProjectFileItemType.Menu:
                    PublishMenu(itemNode, refreshModulesTree);
                    break;
                default:
                    break;
            }
        }

        //-----------------------------------------------------------------------------
        private void PublishMenu(TreeViewProjectFilesNode menuNode)
        {
            PublishMenu(menuNode, true);
        }

        //-----------------------------------------------------------------------------
        private void PublishMenu(TreeViewProjectFilesNode menuNode, bool refreshModulesTree)
        {
            if (menuNode == null || menuNode.NodeType != TreeViewProjectFileItemType.Menu)
                return;

            BaseCustomizationContext.CustomizationContextInstance.PublishMenu(menuNode.FileFullPath);

            if (refreshModulesTree)
                FillModulesTreeView();
        }

        //-----------------------------------------------------------------------------
        private void tsmiExportApplication_Click(object sender, EventArgs e)
        {
            ExportPackage();
        }

        //-----------------------------------------------------------------------------
        private void tsmiImportApplication_Click(object sender, EventArgs e)
        {
            ImportPackage();
        }

        //-----------------------------------------------------------------------------
        private void AddApplicationToolstripButton_Click(object sender, EventArgs e)
        {
            AddApplication();
        }

        //-----------------------------------------------------------------------------
        private void AddModuleToolStripButton_Click(object sender, EventArgs e)
        {
            AddModule();
        }

        //-----------------------------------------------------------------------------
        private void addModuleToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            AddModule();
        }

        //-----------------------------------------------------------------------------
        private void PublishAllCustomizations(object sender, EventArgs e)
        {
            foreach (TreeNode treeNode in tvFiles.Nodes)
                PublishAllCustomizationsInternal(treeNode as TreeViewProjectFilesNode);

            FillModulesTreeView();
        }

        //-----------------------------------------------------------------------------
        private void PublishAllCustomizationsInternal(TreeViewProjectFilesNode treeViewProjectFilesNode)
        {
            if (treeViewProjectFilesNode == null)
                return;

            //Pubblico eventuali item di questo nodo...
            PublishItem(treeViewProjectFilesNode, false);

            //...e se ci sono nodi figli vado in ricorsione.
            if (treeViewProjectFilesNode.Nodes != null && treeViewProjectFilesNode.Nodes.Count > 0)
                foreach (TreeNode treeNode in treeViewProjectFilesNode.Nodes)
                    PublishAllCustomizationsInternal(treeNode as TreeViewProjectFilesNode);
        }

        //-----------------------------------------------------------------------------
        private void GroupModuleAssemblies(object sender, EventArgs e)
        {
			/*
				string applicationName = lvApplications.SelectedItems != null && lvApplications.SelectedItems.Count > 0 ? lvApplications.SelectedItems[0].Text : String.Empty;
				if (String.IsNullOrWhiteSpace(applicationName))
					return;
				string moduleName = currentNode.Tag.ToString();
				IEasyBuilderApp cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(applicationName, moduleName);
				//TODO MAtteo che percorso passare?
				AssemblyPackager.Build(AssemblyPackager.ProcedureBehaviour.AttendedLeaveDialog, null, cust);
			 */



			try
			{
				if (tvFiles.Nodes.Count == 1)
				{
					//Si sta rimuovendo l'unico nodo dell'applicazione, quindi si avverte che verra rimossa tutta l'applicaizone
					if (MessageBox.Show(this, Resources.DoYouWantToGroup, "EasyStudio", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
						return;

				}

				//disabilita tutta la form per evitare interazione dell'utente durante le operazioni di background
				EnableUI(false);
				if (tvFiles.SelectedNode == null)
					return;

				TreeViewProjectFilesNode currentNode = tvFiles.SelectedNode as TreeViewProjectFilesNode;
				if (currentNode == null)
					return;

				if (currentNode.NodeType != TreeViewProjectFileItemType.Module)
					return;
				string applicationName = lvApplications.SelectedItems != null && lvApplications.SelectedItems.Count > 0 ? lvApplications.SelectedItems[0].Text : String.Empty;
				if (String.IsNullOrWhiteSpace(applicationName))
					return;
				string moduleName = currentNode.Tag.ToString();
				IEasyBuilderApp cust = BaseCustomizationContext.CustomizationContextInstance.FindEasyBuilderApp(applicationName, moduleName);
				//TODO MAtteo che percorso passare?
				AssemblyPackager.Build(AssemblyPackager.ProcedureBehaviour.AttendedLeaveDialog, null, cust);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message);
			}
            finally
            {
                EnableUI(true);
            }
        }
    }

    //=============================================================================
    internal enum TreeViewProjectFileItemType
    {
        Module,
        Documents,
        DocumentReport,
        DocumentCustomization,
        Reports,
        ReportCustomization,
        DbScript,
        OtherFiles,
        OtherFile,
        ReferenceAssemblies,
        ReferenceAssembly,
        Menus,
        Menu,
        ModuleObjects,
        ModuleObject,
        ReferenceObjects,
        ReferenceObject
    }

    //=============================================================================
    internal class TreeViewProjectFilesNode : TreeNode, IDisposable
    {
        private Font normalFont = new Font(new FontFamily("Verdana"), 9);
        private Font boldFont = new Font(new FontFamily("Verdana"), 9, FontStyle.Bold);
        private bool isActiveDocument;
        private bool isActiveModule;
        private bool isEnabled;
        private bool isActivated = true;
        private string publishedUser = string.Empty;

        private TreeViewProjectFileItemType nodeType = TreeViewProjectFileItemType.DocumentReport;
        private string fileFullPath = string.Empty;

        internal TreeViewProjectFileItemType NodeType { get { return nodeType; } }
        internal string FileFullPath { get { return fileFullPath; } set { fileFullPath = value; } }
        //-----------------------------------------------------------------------------
        /// <remarks/>
        public bool IsActiveDocument
        {
            get { return isActiveDocument; }
			set
			{
				isActiveDocument = value;

				if (nodeType != TreeViewProjectFileItemType.DocumentCustomization)
					return;
			}
		}

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                if (isActiveModule)
                    return;

                this.StateImageIndex = !isEnabled ? (int)ImageLists.StatusImageListImages.Disabled : -1;
            }
        }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public bool IsActivated
        {
            get { return isActivated; }
            set
            {
                isActivated = value;

                if (!isActivated)
                    ForeColor = Color.Red;
                else
                    ForeColor = Color.Black;

                //this.StateImageIndex = !isActivated ? (int)ImageLists.StatusImageListImages.Disabled : -1;
                foreach (TreeViewProjectFilesNode node in this.Nodes)
                    node.SetIsActivateOnChildren(value);
            }
        }

        //-----------------------------------------------------------------------------
        private void SetIsActivateOnChildren(bool value)
        {
            IsActivated = value;
            foreach (TreeViewProjectFilesNode node in Nodes)
                node.SetIsActivateOnChildren(value);
        }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public bool IsActiveModule
        {
            get { return isActiveModule; }
            set
            {
                isActiveModule = value;
                if (this.nodeType != TreeViewProjectFileItemType.Module)
                    return;

                this.NodeFont = (isActiveModule)
                    ? boldFont
                    : normalFont;
                this.Text += " ";//corregge un bug del treenode che taglia alcuni caratteri con font bold
                this.ImageIndex = this.SelectedImageIndex = (isActiveModule)
                    ? (int)ImageLists.TreeViewFilesImages.Active
                    : (int)ImageLists.TreeViewFilesImages.Root;
                this.ToolTipText = isActiveModule ? Resources.ActiveModule : string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        //-----------------------------------------------------------------------------
        public string PublishedUser
        {
            get { return publishedUser; }
            set
            {
                publishedUser = value;

                this.Text = this.Name = (!publishedUser.IsNullOrEmpty() && (NodeType == TreeViewProjectFileItemType.DocumentCustomization || NodeType == TreeViewProjectFileItemType.Menu))
                    ? string.Format(Resources.FileNotPublishedForUser, this.Name, publishedUser)
                    : this.Name;
            }
        }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public TreeViewProjectFilesNode(string nodeName, TreeViewProjectFileItemType nodeType, string publishedUser = "")
            : this(nodeName, string.Empty, nodeType, publishedUser)
        {

        }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public TreeViewProjectFilesNode(string nodeName, string fileFullPath, TreeViewProjectFileItemType nodeType, string publishedUser = "")
            : base(nodeName)
        {
            this.Name = this.Text = nodeName;
            this.Tag = nodeName;
            this.nodeType = nodeType;
            this.fileFullPath = fileFullPath;
            this.NodeFont = normalFont;
            this.Text += " ";//corregge un bug del treenode che taglia alcuni caratteri con font bold

            switch (nodeType)
            {
                case TreeViewProjectFileItemType.Module:
                    this.ImageIndex = this.SelectedImageIndex = (int)ImageLists.TreeViewFilesImages.Root;
                    break;
                case TreeViewProjectFileItemType.DocumentReport:
                    this.ImageIndex = this.SelectedImageIndex = (int)ImageLists.TreeViewFilesImages.FolderClosed;
                    break;

                case TreeViewProjectFileItemType.Documents:
                case TreeViewProjectFileItemType.OtherFiles:
                case TreeViewProjectFileItemType.DbScript:
                case TreeViewProjectFileItemType.Reports:
                case TreeViewProjectFileItemType.ReferenceAssemblies:
                case TreeViewProjectFileItemType.Menus:
                case TreeViewProjectFileItemType.ModuleObjects:
                case TreeViewProjectFileItemType.ReferenceObjects:
                    this.ImageIndex = this.SelectedImageIndex = (int)ImageLists.TreeViewFilesImages.Folder;
                    break;

                case TreeViewProjectFileItemType.OtherFile:
                case TreeViewProjectFileItemType.DocumentCustomization:
                case TreeViewProjectFileItemType.ReferenceAssembly:
                case TreeViewProjectFileItemType.Menu:
                case TreeViewProjectFileItemType.ModuleObject:
                case TreeViewProjectFileItemType.ReferenceObject:
                case TreeViewProjectFileItemType.ReportCustomization:
                    this.ImageIndex = this.SelectedImageIndex = GetImageIndexFromFileType();
                    PublishedUser = publishedUser;

                    //se il file è un default per la customizzazione allora lo segno
                    foreach (string item in BaseCustomizationContext.CustomizationContextInstance.ActiveDocuments)
                    {
                        string custNs = BaseCustomizationContext.CustomizationContextInstance.GetPseudoNamespaceFromFullPath(fileFullPath, publishedUser);

                        if (!item.CompareNoCase(custNs))
                            continue;

                        IsActiveDocument = true;
                        break;
                    }
                    break;
                default:
                    break;
            }
        }

        //-----------------------------------------------------------------------------
        private int GetImageIndexFromFileType()
        {
            if (fileFullPath.IsNullOrEmpty())
                return -1;

            FileInfo fi = new FileInfo(fileFullPath);
            if (fi.Extension.CompareNoCase(NameSolverStrings.XmlExtension))
                return (int)ImageLists.TreeViewFilesImages.Xml;
            else if (fi.Extension.CompareNoCase(NameSolverStrings.ConfigExtension))
                return (int)ImageLists.TreeViewFilesImages.Config;
            else if (fi.Extension.CompareNoCase(NameSolverStrings.DllExtension))
                return (int)ImageLists.TreeViewFilesImages.Dll;
            else if (fi.Extension.CompareNoCase(NameSolverStrings.WrmExtension))
                return (int)ImageLists.TreeViewFilesImages.Report;
            else if (fi.Extension.CompareNoCase(NameSolverStrings.SqlExtension))
                return (int)ImageLists.TreeViewFilesImages.SqlFile;

            return -1;
        }

        //---------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //---------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (normalFont != null)
            {
                normalFont.Dispose();
                normalFont = null;
            }
            if (boldFont != null)
            {
                boldFont.Dispose();
                boldFont = null;
            }
        }
    }

    //===================================================================================
    internal class ApplicationListViewItem : ListViewItem
    {
        private bool isActiveApplication = false;
        private bool isStandardApplication = false;

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public bool IsActiveApplication
        {
            get { return isActiveApplication; }
            set
            {
                isActiveApplication = value;
                this.StateImageIndex = (value) ? (int)ImageLists.StatusImageListImages.ActiveApplication : -1;
                this.ToolTipText = isActiveApplication ? Resources.ActiveApplication : string.Empty;
            }
        }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public bool IsStandardApplication
        {
            get { return isStandardApplication; }
            set
            {
                isStandardApplication = value;
                this.ImageIndex = (value) ? (int)ImageLists.SmallImageListImages.StandardApplication : (int)ImageLists.SmallImageListImages.CustomApplication;
            }
        }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public ApplicationListViewItem(string text)
            : base(text)
        {
            this.Name = text;
        }
    }

    //=============================================================================
    internal class RenamedCompany : IRenamedCompany
    {
        string oldCompanyName;
        string newCompanyName;
        /// <remarks/>
        public string OldCompanyName { get { return oldCompanyName; } }
        /// <remarks/>
        public string NewCompanyName { get { return newCompanyName; } }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public RenamedCompany(string oldCompanyName, string newCompanyName)
        {
            this.oldCompanyName = oldCompanyName;
            this.newCompanyName = newCompanyName;
        }
        //-----------------------------------------------------------------------------
        /// <remarks/>
        public RenamedCompany(string oldCompanyName)
        {
            this.oldCompanyName = oldCompanyName;
            this.newCompanyName = string.Empty;
        }
        //-----------------------------------------------------------------------------
        /// <remarks/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public override bool Equals(object obj)
        {
            return oldCompanyName.CompareNoCase((obj as RenamedCompany).oldCompanyName);
        }
    }

    //=============================================================================
    internal class CompanyRelatedUnzipFiles
    {
        string fileFullPath;
        string oldCompanyName;
        string newCompanyName;

        /// <remarks/>
        public string FileFullPath { get { return fileFullPath; } set { fileFullPath = value; } }
        /// <remarks/>
        public string OldCompanyName { get { return oldCompanyName; } set { oldCompanyName = value; } }
        /// <remarks/>
        public string NewCompanyName { get { return newCompanyName; } set { newCompanyName = value; } }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public CompanyRelatedUnzipFiles(string fileFullPath)
        {
            this.fileFullPath = fileFullPath;
            this.oldCompanyName = string.Empty;
            this.newCompanyName = string.Empty;
        }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public CompanyRelatedUnzipFiles(string fileFullPath, string oldCompanyName, string newCompanyName)
        {
            this.fileFullPath = fileFullPath;
            this.oldCompanyName = oldCompanyName;
            this.newCompanyName = newCompanyName;
        }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //-----------------------------------------------------------------------------
        /// <remarks/>
        public override bool Equals(object obj)
        {
            return fileFullPath.CompareNoCase((obj as CompanyRelatedUnzipFiles).fileFullPath);
        }
    }
}

