using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Microarea.TaskBuilderNet.Data.DatabaseWinControls
{
	/// <summary>
	/// OpenFolder
	/// CAUTION: The FolderNameEditor.FolderBrowser type supports the .NET Framework 
	/// infrastructure and is not intended to be used directly from your code.
	/// </summary>
	//=========================================================================
	public class OpenFolder : System.Windows.Forms.Design.FolderNameEditor
	{
		private FolderNameEditor.FolderBrowser folderDialog;

		private string path = string.Empty;
		private string description = string.Empty;
		private string serverToBrowser = string.Empty;

		//Properties
		//---------------------------------------------------------------------
		public string Path { get { return path; } set { path = value; } }
		public string Description { get { return description; } set { description = value; } }
		public string ServerToBrowser { get { return serverToBrowser; } set { serverToBrowser = value; } }

		/// <summary>
		/// OpenFolder(Costruttore)
		/// </summary>
		//---------------------------------------------------------------------
		public OpenFolder()
		{
			folderDialog = new FolderNameEditor.FolderBrowser();
		}

		/// <summary>
		/// ShowDialog - Mostra la Dialog per il browsing
		/// </summary>
		//---------------------------------------------------------------------
		public DialogResult ShowDialog()
		{
			DialogResult result;
			folderDialog.Description = this.Description;
			folderDialog.Style = FolderNameEditor.FolderBrowserStyles.RestrictToDomain | FolderNameEditor.FolderBrowserStyles.ShowTextBox;

			//	scegliendo una StartLocation tra gli enumerativi disponibili, poi non si può ampliare la ricerca.
			folderDialog.StartLocation = FolderBrowserFolder.MyComputer;

			result = folderDialog.ShowDialog(null);
			Path = folderDialog.DirectoryPath;

			return result;
		}
	}
}
