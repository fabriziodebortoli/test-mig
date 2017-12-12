using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.Packager
{
	//=========================================================================
	/// <remarks/>
	public class CustomListItem : ICustomListItem
	{
		private string filePath = string.Empty;
		private bool isActiveDocument;
		private string publishedUser = string.Empty;
		private bool isReadOnlyServerDocumentPart;
		private ItemSource itemSource;
		private string documentNamespace;

		/// <remarks/>
		public string FilePath { get { return filePath; } set { filePath = value; } }

		/// <remarks/>
		public bool IsActiveDocument
		{
			get { return isActiveDocument; }
			set
			{
				isActiveDocument = value;
				BaseCustomizationContext.CustomizationContextInstance.UpdateActiveDocuments(isActiveDocument, filePath, publishedUser);
			}
		}
		/// <remarks/>
		public string PublishedUser { get { return publishedUser; } set { publishedUser = value; } }
		/// <remarks/>
		public bool IsReadOnlyServerDocumentPart
		{
			get { return isReadOnlyServerDocumentPart; }
			set
			{
				isReadOnlyServerDocumentPart = value;
			}
		}

		/// <remarks/>
		//-----------------------------------------------------------------------------
		public ItemSource ItemSource
		{
			get { return itemSource; }
		}

		/// <remarks/>
		//-----------------------------------------------------------------------------
		public string DocumentNamespace
		{
			get { return documentNamespace; }
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public CustomListItem(string filePath, bool isActiveDocument, string publishedUser, ItemSource itemSource, string documentNamespace)
		{
			this.filePath = filePath;
			this.isActiveDocument = isActiveDocument;
			this.publishedUser = publishedUser;
			this.itemSource = itemSource;
			this.documentNamespace = documentNamespace;

		}
	}

}
