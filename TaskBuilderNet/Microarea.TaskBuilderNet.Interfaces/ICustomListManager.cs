using System.Collections.Generic;
using System.IO;


namespace Microarea.TaskBuilderNet.Interfaces
{
	//=========================================================================
	/// <remarks />
	public interface ICustomListManager
	{
		//---------------------------------------------------------------------
		/// <remarks />
		ICustomList CustomList { get; }
		/// <remarks />
		string CustomListFullPath { get; set; }
		/// <remarks />
		bool IsEnabled { get; set; }

		//---------------------------------------------------------------------
		/// <remarks />
		bool AddReadOnlyServerDocumentPartToCustomList(string filePath, bool save = true);
		/// <remarks />
		bool AddToCustomList(string filePath, bool save = true, bool isActiveDocument = false, string publishedUser = "", string documentNamespace = "");
		/// <remarks />
		void RemoveFromCustomListAndFromFileSystem(string filePath);

		//---------------------------------------------------------------------
		/// <remarks />
		Stream GetStreamFromCustomList(ICustomList list);

		//---------------------------------------------------------------------
		/// <remarks />
		void LoadCustomList();
		/// <remarks />
		void LoadCustomList(Stream stream);
		/// <remarks />
		void SaveCustomList();
		/// <remarks />
		void PurgeCustomListFromNonExistingFiles();
		/// <remarks />
		void RenameCompanyPathsInCustomList(IList<IRenamedCompany> renamedCompanies);

		//---------------------------------------------------------------------
		/// <remarks />
		bool ContainsControllerDll(INameSpace controllerNamespace);
		/// <remarks />
		bool ContainsOtherControllerDll(INameSpace controllerNamespace);

		/// <remarks />
		string[] GetAllReadOnlyServerDocPart(string fullPath);
	}
}
