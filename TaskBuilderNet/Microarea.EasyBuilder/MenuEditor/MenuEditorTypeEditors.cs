using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.MenuEditor
{
	//=========================================================================
	internal static class GenericMenuEditorTypeEditorFunctions
	{
		//--------------------------------------------------------------------------------
		internal static string GetNamespaceFromNode(TreeNode tn)
		{
			StringBuilder sb = new StringBuilder();

			while (tn.Parent != null)
			{
				string s = string.Empty;
				s = (tn.Parent.Parent != null)
					? string.Concat(".", tn.Name)
					: tn.Name;

				sb.Insert(0, s);
				tn = tn.Parent;
			}

			return sb.ToString();
		}

		//--------------------------------------------------------------------------------
		internal static void Add(TreeNode parentNode, string ns)
		{
			if (ns.IsNullOrEmpty())
				return;

			int index = ns.IndexOf(".");
			string currentToken = string.Empty;
			string restOfNamespace = string.Empty;
			if (index >= 0)
			{
				currentToken = ns.Substring(0, index);
				restOfNamespace = ns.Substring(index + 1);
			}
			else
				currentToken = ns;

			TreeNode tn;

			TreeNode[] nodes = parentNode.Nodes.Find(currentToken, false);
			if (nodes == null || nodes.Count() == 0)
			{
				tn = new TreeNode(currentToken);
				tn.Name = currentToken;
				parentNode.Nodes.Add(tn);
			}
			else
				tn = nodes[0];

			Add(tn, restOfNamespace);
		}

		//--------------------------------------------------------------------------------
		internal static TreeNode GetNodeByFullPath(TreeNode root, string fullPath)
		{
			TreeNode selectedNode = root;

			string[] tokens = fullPath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string token in tokens)
			{
				TreeNode[] foundNodes = selectedNode.Nodes.Find(token, false);
				if (foundNodes == null || foundNodes.Length == 0)
					return null;

				selectedNode = foundNodes[0];
			}

			return selectedNode;
		}
	}

	//=========================================================================
	internal class DocumentBatchMenuCommandObjectTypeEditor : UITypeEditor
	{
		TreeView tv;
		private string currentValue;

		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		//--------------------------------------------------------------------------------
		public override bool IsDropDownResizable { get { return true; } }

		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			//LUCA, DECLINO OGNI RESPONSABILITA' per questo if , sono stato costretto da Matteo
			if (context.Instance.GetType() == typeof(DocumentMenuCommand))
			{
				Predicate<DocumentInfo> predicate = new Predicate<DocumentInfo>((DocumentInfo di) => { return di.IsDataEntry; });
				tv = CreateTreeView(Resources.Documents, predicate);
			}
			else
			{
				Predicate<DocumentInfo> predicate = new Predicate<DocumentInfo>((DocumentInfo di) => { return di.IsBatch; });
				tv = CreateTreeView(Resources.Batch, predicate);
			}
			tv.Tag = service;

			tv.VisibleChanged += new EventHandler(Tv_VisibleChanged);
			tv.MouseDoubleClick += new MouseEventHandler(Tv_MouseDoubleClick);
			currentValue = value as string;

			service.DropDownControl(tv);

			if (tv.SelectedNode == null)
				return base.EditValue(context, provider, value);

			return GenericMenuEditorTypeEditorFunctions.GetNamespaceFromNode(tv.SelectedNode);
		}

		//--------------------------------------------------------------------------------
		private static TreeView CreateTreeView(string rootText, Predicate<DocumentInfo> predicate)
		{
			TreeNode root = new TreeNode(rootText);
			root.Expand();
			root.Name = rootText;
			TreeView tv = new TreeView();
			tv.Nodes.Add(root);
			tv.Width = 300;
			tv.Height = 300;

			foreach (BaseApplicationInfo bai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
			{
				foreach (BaseModuleInfo bmi in bai.Modules)
				{
					if (bmi.Documents == null)
						continue;

					foreach (DocumentInfo di in bmi.Documents)
					{
						if (!predicate(di))
							continue;

						GenericMenuEditorTypeEditorFunctions.Add(tv.Nodes[0], di.NameSpace.GetNameSpaceWithoutType());
					}
				}
			}

			return tv;
		}
		//--------------------------------------------------------------------------------
		void Tv_VisibleChanged(object sender, EventArgs e)
		{
			if (!tv.Visible || tv.IsDisposed || currentValue.IsNullOrEmpty())
				return;

			tv.SelectedNode = GenericMenuEditorTypeEditorFunctions.GetNodeByFullPath(tv.Nodes[0], currentValue);
		}

		//--------------------------------------------------------------------------------
		void Tv_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (tv.SelectedNode == null || tv.SelectedNode.Nodes.Count > 0)
				return;

			IWindowsFormsEditorService service = tv.Tag as IWindowsFormsEditorService;
			tv.MouseDoubleClick -= new MouseEventHandler(Tv_MouseDoubleClick);

			currentValue = GenericMenuEditorTypeEditorFunctions.GetNamespaceFromNode(tv.SelectedNode);
			
			service.CloseDropDown();
			tv.Dispose();
		}
	}

	//=========================================================================
	internal class FileMenuCommandObjectTypeEditor : UITypeEditor
	{
		OpenFileDialog ofd;

		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			DialogResult res = DialogResult.None;
			using (ofd = new OpenFileDialog())
			{
				ofd.Multiselect = false;

				//LUCA, DECLINO OGNI RESPONSABILITA' per questo if , sono stato costretto da Matteo
				if (context.Instance.GetType() == typeof(ExeMenuCommand))
				{
					ofd.Title = Resources.MenuEditorSelectExecutable;
					ofd.Filter = Resources.MenuEditorFilterExecutable;
				}
				else if (context.Instance.GetType() == typeof(TextMenuCommand))
				{
					ofd.Title = Resources.MenuEditorSelectText;
					ofd.Filter = Resources.MenuEditorFilterText;
				}
				else
					return base.EditValue(provider, value);

				res = ofd.ShowDialog();
			}
			if (res != DialogResult.OK)
				return value;

			return ofd.FileName;
		}
	}

	//=========================================================================
	internal class OfficeItemMenuCommandObjectTypeEditor : UITypeEditor
	{
		OpenFileDialog ofd;

		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			OfficeItemMenuCommand officeMenuItem = context.Instance as OfficeItemMenuCommand;
			if (officeMenuItem == null)
				return base.EditValue(provider, value);

			string newValue = value as string;
			if (newValue.IsNullOrEmpty() && !officeMenuItem.CommandObject.IsNullOrEmpty())
			{
				officeMenuItem.CommandObject = String.Empty;
				officeMenuItem.Application = String.Empty;
				officeMenuItem.SubType = String.Empty;

				return String.Empty;
			}
			DialogResult res = DialogResult.None;
			using (ofd = new OpenFileDialog())
			{
				ofd.Multiselect = false;

				ofd.Title = Resources.MenuEditorSelectOffice;
				ofd.Filter = Resources.MenuEditorFilterOffice;

				res = ofd.ShowDialog();
			}
			if (res != DialogResult.OK)
				return value;

			//Capire dall'estensione se è Word oppuer Excel e importare il documento creando una cartella sorella della
			//cartella 'Menu' che si chiami 'Word' oppure 'Excel' rispettivamente.
			string officeFileToBeImportedFullName = ofd.FileName;
			string moduleFullPath = Path.Combine
					(
					BasePathFinder.BasePathFinderInstance.GetCustomApplicationsPath(),
					BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName,
					BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName
					);
			if (IsExcel(officeFileToBeImportedFullName))
			{
				moduleFullPath = Path.Combine(moduleFullPath, NameSolverStrings.Excel);

				officeMenuItem.Application = NameSolverStrings.Excel;
			}
			else
			{
				moduleFullPath = Path.Combine(moduleFullPath, NameSolverStrings.Word);

				officeMenuItem.Application = NameSolverStrings.Word;
			}

			if (!Directory.Exists(moduleFullPath))
				Directory.CreateDirectory(moduleFullPath);

			string safeFileNameWithoutExtension = Path.GetFileNameWithoutExtension(officeFileToBeImportedFullName).ReplaceNoCase(" ", "_");
			string safeFullName = Path.GetFileName(officeFileToBeImportedFullName).ReplaceNoCase(" ", "_");

			string destinationPath = Path.Combine(moduleFullPath, safeFullName);

			//Copiare il file selezionato nella cartela appena creata.
			File.Copy(officeFileToBeImportedFullName, destinationPath, true);
			BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(destinationPath);

			SetSubType(officeMenuItem, destinationPath);

			return String.Format(
				"{0}.{1}.{2}",
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ApplicationName,
				BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp.ModuleName,
				safeFileNameWithoutExtension
				);
		}

		//--------------------------------------------------------------------------------
		private static bool IsExcel(string officeFileToBeImportedFullName)
		{
			switch (Path.GetExtension(officeFileToBeImportedFullName))
			{
				case ".xls":
				case ".xlsx":
				case ".xlt":
				case ".xltx":
					return true;
				default:
					return false;
			};
		}

		//--------------------------------------------------------------------------------
		private static void SetSubType(OfficeItemMenuCommand officeMenuItem, string officeFileToBeImportedFullName)
		{
			switch (Path.GetExtension(officeFileToBeImportedFullName))
			{
				case ".doc":
				case ".xls":
					officeMenuItem.SubType = MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT;
					break;
				case ".docx":
				case ".xlsx":
					officeMenuItem.SubType = MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_DOCUMENT_2007;
					break;
				case ".dot":
				case ".xlt":
				officeMenuItem.SubType = MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE;
					break;
				case ".dotx":
				case ".xltx":
					officeMenuItem.SubType = MenuXmlNode.MenuXmlNodeCommandSubType.XML_OFFICE_ITEM_SUBTYPE_TEMPLATE_2007;
					break;
				default:
					break;
			}
		}
	}

	//=========================================================================
	internal class FunctionMenuCommandObjectTypeEditor : UITypeEditor
	{
		TreeView tv;
		private string currentValue;

		//--------------------------------------------------------------------------------
		public override bool IsDropDownResizable { get { return true; } }

		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			if (context.Instance.GetType() != typeof(FunctionMenuCommand))
				return base.EditValue(context, provider, value);

			tv = CreateTreeView(Resources.Functions);

			tv.Tag = service;

			tv.VisibleChanged += new EventHandler(Tv_VisibleChanged);
			tv.MouseDoubleClick += new MouseEventHandler(Tv_MouseDoubleClick);
			currentValue = value as string;

			service.DropDownControl(tv);

			if (tv.SelectedNode == null)
				return base.EditValue(context, provider, value);

			return GenericMenuEditorTypeEditorFunctions.GetNamespaceFromNode(tv.SelectedNode);
		}

		//--------------------------------------------------------------------------------
		private static TreeView CreateTreeView(string rootText)
		{
			TreeNode root = new TreeNode(rootText);
			root.Expand();
			root.Name = rootText;
			TreeView tv = new TreeView();
			tv.Nodes.Add(root);
			tv.Width = 300;
			tv.Height = 300;

			foreach (BaseApplicationInfo bai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
			{
				foreach (BaseModuleInfo bmi in bai.Modules)
				{
                    if (bmi == null || bmi.WebMethods == null || bmi.WebMethods.Count == 0)
						continue;

                    foreach (FunctionPrototype fi in bmi.WebMethods)
					{
						if (fi.Parameters.Count == 0)
							GenericMenuEditorTypeEditorFunctions.Add(tv.Nodes[0], fi.NameSpace.GetNameSpaceWithoutType());
					}
				}
			}
			return tv;
		}

		//--------------------------------------------------------------------------------
		void Tv_VisibleChanged(object sender, EventArgs e)
		{
			if (!tv.Visible || tv.IsDisposed || currentValue.IsNullOrEmpty())
				return;

			tv.SelectedNode = GenericMenuEditorTypeEditorFunctions.GetNodeByFullPath(tv.Nodes[0], currentValue);
		}

		//--------------------------------------------------------------------------------
		void Tv_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (tv.SelectedNode == null || tv.SelectedNode.Nodes.Count > 0)
				return;

			IWindowsFormsEditorService service = tv.Tag as IWindowsFormsEditorService;
			tv.MouseDoubleClick -= new MouseEventHandler(Tv_MouseDoubleClick);

			currentValue = GenericMenuEditorTypeEditorFunctions.GetNamespaceFromNode(tv.SelectedNode);

			service.CloseDropDown();
			tv.Dispose();
		}
	}

	//=========================================================================
	internal class ReportMenuCommandObjectTypeEditor : UITypeEditor
	{
		TreeView tv;
		string currentValue;

		TreeNode root;
		//TreeNode newReport;
		
		//--------------------------------------------------------------------------------
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		//--------------------------------------------------------------------------------
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

			if (context.Instance.GetType() != typeof(ReportMenuCommand))
				return base.EditValue(context, provider, value);

			root = new TreeNode(Resources.Functions);
			root.Name = Resources.Functions;
			//newReport = new TreeNode(MenuEditorStrings.AddNewReport);
			//newReport.Name = MenuEditorStrings.AddNewReport;

			tv = CreateTreeView();

			tv.Tag = service;

			tv.VisibleChanged += new EventHandler(tv_VisibleChanged);
			tv.MouseDoubleClick += new MouseEventHandler(tv_MouseDoubleClick);
			currentValue = value as string;

			service.DropDownControl(tv);

			if (tv.SelectedNode == null)
				return base.EditValue(context, provider, value);

			return GenericMenuEditorTypeEditorFunctions.GetNamespaceFromNode(tv.SelectedNode);
		}

		//--------------------------------------------------------------------------------
		void tv_VisibleChanged(object sender, EventArgs e)
		{
			if (!tv.Visible || tv.IsDisposed || currentValue.IsNullOrEmpty())
				return;

			tv.SelectedNode = GenericMenuEditorTypeEditorFunctions.GetNodeByFullPath(tv.Nodes[0], currentValue);
		}

		//--------------------------------------------------------------------------------
		void tv_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (tv.SelectedNode == null || tv.SelectedNode.Nodes.Count > 0)
				return;

			IWindowsFormsEditorService service = tv.Tag as IWindowsFormsEditorService;
			tv.MouseDoubleClick -= new MouseEventHandler(tv_MouseDoubleClick);

			//if (tv.SelectedNode == newReport)
			//{
			//    OpenFileDialog ofd = new OpenFileDialog();
			//    ofd.Multiselect = false;
			//    ofd.Title = Resources.MenuEditorSelectReport;
			//    ofd.Filter = Resources.MenuEditorFilterReport;
			//    DialogResult res = ofd.ShowDialog();
			//    if (res != DialogResult.OK)
			//        return;

			//    string reportToBeImportedFullName = ofd.FileName;
			//    string moduleFullPath = Path.Combine
			//            (
			//            BasePathFinder.BasePathFinderInstance.GetCustomApplicationsPath(),
			//            BaseCustomizationContext.CustomizationContextInstance.CurrentCustomization.ApplicationName,
			//            BaseCustomizationContext.CustomizationContextInstance.CurrentCustomization.ModuleName
			//            );
				
			//    moduleFullPath = Path.Combine(moduleFullPath, NameSolverStrings.Report);
				
			//    if (!Directory.Exists(moduleFullPath))
			//        Directory.CreateDirectory(moduleFullPath);

			//    string safeFileNameWithoutExtension = Path.GetFileNameWithoutExtension(reportToBeImportedFullName).ReplaceNoCase(" ", "_");
			//    string safeFullName = Path.GetFileName(reportToBeImportedFullName).ReplaceNoCase(" ", "_");

			//    string destinationPath = Path.Combine(moduleFullPath, safeFullName);

			//    //Copiare il file selezionato nella cartela appena creata. 
			//    //chiedere se overridare?
			//    File.Copy(reportToBeImportedFullName, destinationPath, true);
			//    BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(destinationPath);

			//    string reportNs = string.Format("{0}.{1}.{2}",
			//        BaseCustomizationContext.CustomizationContextInstance.CurrentCustomization.ApplicationName,
			//        BaseCustomizationContext.CustomizationContextInstance.CurrentCustomization.ModuleName,
			//        Path.GetFileNameWithoutExtension(destinationPath));

			//    currentValue = new NameSpace(reportNs, NameSpaceObjectType.Report);

			//}
			//else
			//{
			//    currentValue = GenericMenuEditorTypeEditorFunctions.GetNamespaceFromNode(tv.SelectedNode);
			//}

			currentValue = GenericMenuEditorTypeEditorFunctions.GetNamespaceFromNode(tv.SelectedNode);
			service.CloseDropDown();
			tv.Dispose();
		}

		//--------------------------------------------------------------------------------
		internal TreeView CreateTreeView()
		{
			root.Expand();
			TreeView tv = new TreeView();
			tv.Nodes.Add(root);
			//tv.Nodes.Add(newReport);
			tv.Width = 300;
			tv.Height = 300;

			foreach (BaseApplicationInfo bai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
			{
				foreach (BaseModuleInfo bmi in bai.Modules)
				{
					string reportStandard = bmi.GetStandardReportPath();

					if (!Directory.Exists(reportStandard))
						continue;

					string[] reports = Directory.GetFiles(reportStandard, NameSolverStrings.WrmExtensionSearchCriteria);

					foreach (string report in reports)
					{
						string reportNs = string.Format("{0}.{1}.{2}", bai.Name, bmi.Name, Path.GetFileNameWithoutExtension(report));
						GenericMenuEditorTypeEditorFunctions.Add(tv.Nodes[0], reportNs);
					}
				}
			}

			return tv;
		}
	}
}
