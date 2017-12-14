
namespace Microarea.TaskBuilderNet.Interfaces
{
	[System.ComponentModel.TypeConverter(typeof(System.ComponentModel.TypeConverter))]
	//=========================================================================
	public interface INameSpace
	{
        INameSpaceType NameSpaceType { get; }
		void CreateNameSpace(string fullNameSpace);
        bool IsValid();
 		string FullNameSpace { get; }

        string GetTokenValue(NameSpaceObjectType tokenType);
        string GetTokenValue(string tokenType);

        string GetEndPointNameSpace();
        string GetNameSpaceWithoutType();
        string GetNameSpaceWithoutLastToken();
        //------------------------------
		string Application { get; }
		string Command { get; }
		string Dbt { get; }
		string Document { get; }
		string Leaf { get;}
		string DocumentSchema { get; }
		string ExcelDocument { get; }
		string ExcelDocument2007 { get; }
		string ExcelTemplate { get; }
		string ExcelTemplate2007 { get; }
		string File { get; }
		string Function { get; }
		string GetExcel2007DocumentFileName();
		string GetExcel2007TemplateFileName();
		string GetExcelDocumentFileName();
		string GetExcelTemplateFileName();
		string GetReportFileName();
		string GetWord2007DocumentFileName();
		string GetWord2007TemplateFileName();
		string GetWordDocumentFileName();
		string GetWordTemplateFileName();
		string Hotlink { get; }
        string HotKeyLink { get; }
        string Image { get; }
		string Library { get; }
		string Module { get; }
		string Report { get; }
		string ReportSchema { get; }
		string Setting { get; }
		string Table { get; }
		string Text { get; }
		string ToString();
		string View { get; }
		string WordDocument { get; }
		string WordDocument2007 { get; }
		string WordTemplate { get; }
		string WordTemplate2007 { get; }
	}
}
