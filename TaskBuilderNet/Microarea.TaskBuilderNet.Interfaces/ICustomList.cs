using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.Interfaces
{
	public interface ICustomList : IList<ICustomListItem>
	{
		void Add(string file);
		bool ContainsNoCase(string stringToSearch);
		ICustomListItem FindItem(string fileFullPath);
		IList<ICustomListItem> FindItemByPathPart(string pathPart);
		void Remove(string file);
		void SetActiveDocument(string currentFile);
	}
}