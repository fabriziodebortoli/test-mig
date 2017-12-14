using System.Collections.Generic;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.Packager
{
	/// <summary>
	/// Classe derivata da list di CustomListItem
	/// Accetta e restituisce dei fullpath ma internamente memorizza path relativi all'interno della custom
	/// </summary>
	//=========================================================================
	public class CustomList : List<ICustomListItem>, ICustomList
	{
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public CustomList()
			: base()
		{

		}
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public CustomList(IEnumerable<ICustomListItem> collection)
			: base(collection)
		{
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public CustomList(int capacity)
			: base(capacity)
		{
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public new void Add(ICustomListItem clItem)
		{
			base.Add(clItem);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public void Add(string file)
		{
			this.Add(new CustomListItem(file, false, string.Empty, CustomListManager.CalculateItemSource(file), string.Empty));
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public void Remove(string file)
		{
			ICustomListItem tempItem = null;
			foreach (ICustomListItem item in this)
			{
				if (item.FilePath.CompareNoCase(file))
				{
					tempItem = item;
					break;
				}
			}
			
			if (tempItem != null)
				base.Remove(tempItem);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public void SetActiveDocument(string currentFile)
		{
			//Disattivo tutti gli active documents legati al documento che sto analizzando
			DeactiveDocuments(currentFile);

			//Cerco nella custom list l'item corrente...
			ICustomListItem item = FindItem(currentFile);
			if (item == null)
				return;

			//...e lo attivo
			item.IsActiveDocument = true;
		}

		/// <summary>
		/// Cerca tutti i documenti nella custom list che hanno come parent namespace quello legato al file corrente e 
		/// li disattiva tutti
		/// </summary>
		/// <param name="currentFile"></param>
		//-----------------------------------------------------------------------------
		private void DeactiveDocuments(string currentFile)
		{
			//recupero il namespace del documento papa
			string docNamespace = BaseCustomizationContext.CustomizationContextInstance.GetParentPseudoNamespaceFromFullPath(currentFile);

			//rimetto a false gli active documents che matchano il namespace
			foreach (ICustomListItem currentItem in this)
			{
				string tempItemNamespace = BaseCustomizationContext.CustomizationContextInstance.GetParentPseudoNamespaceFromFullPath(currentItem.FilePath);
				if (tempItemNamespace.CompareNoCase(docNamespace))
					currentItem.IsActiveDocument = false;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public ICustomListItem FindItem(string fileFullPath)
		{
			foreach (ICustomListItem item in this)
			{
				if (item.FilePath.CompareNoCase(fileFullPath))
					return item;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public bool ContainsNoCase(string stringToSearch)
		{
			IList<ICustomListItem> items = FindItemByPathPart(stringToSearch);
			return items != null && items.Count > 0;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public IList<ICustomListItem> FindItemByPathPart(string pathPart)
		{
			List<ICustomListItem> items = new List<ICustomListItem>();
			foreach (ICustomListItem item in this)
			{
				if (item.FilePath.ContainsNoCase(pathPart))
					items.Add(item);
			}
			return items;
		}
	}
}
