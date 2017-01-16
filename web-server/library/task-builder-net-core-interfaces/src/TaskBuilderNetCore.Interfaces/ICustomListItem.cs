namespace TaskBuilderNetCore.Interfaces
{
	public interface ICustomListItem
	{
		string FilePath { get; set; }
		bool IsActiveDocument { get; set; }
		bool IsReadOnlyServerDocumentPart { get; set; }
		string PublishedUser { get; set; }
		ItemSource ItemSource { get; }
	}


	//=========================================================================
	/// <remarks/>
	public enum ItemSource
	{
		/// <remarks/>
		Custom,
		/// <remarks/>
		Standard
	}
}
