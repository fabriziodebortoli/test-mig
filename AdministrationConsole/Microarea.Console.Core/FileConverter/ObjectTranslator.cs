using System.Collections;

namespace Microarea.Console.Core.FileConverter
{
	//--------------------------------------------------------------------------------
	public interface ITableTranslator
	{
		string TranslateTable(string tableOldName);
		string TranslateColumn(string tableOldName, string columnOldName);
		bool ExistColumn(string tableOldName, string columnOldName);
		string OwnerColumn(string tableOldName, string columnOldName);
		bool Splitted(string tableOldName, out string allSplittedTables);
		string ReverseTranslateTable(string tableName);
	}

	//--------------------------------------------------------------------------------
	public interface IFontTranslator
	{
		string TranslateFont(string fontName);
		bool IsPublicFont(string fontName);

	}

	//--------------------------------------------------------------------------------
	public interface IFormatterTranslator
	{
		string TranslateFormatter(string formatterName);
		bool IsPublicFormatter(string formatterName);
	}

	//--------------------------------------------------------------------------------
	public interface IDocumentsTranslator
	{
		string TranslateDocument(string documentName);
		bool ExistDocument(string documentName);
	}

	//--------------------------------------------------------------------------------
	public interface IReportsTranslator
	{
		string TranslateReport(string reportName);
		bool ExistReport(string reportName);
	}

	//--------------------------------------------------------------------------------
	public interface IWoormInfoVariablesTranslator
	{
		string TranslateWoormInfoVariable(string name);
		bool ExistWoormInfoVariable(string name);
	}

	//--------------------------------------------------------------------------------
	public interface ILibrariesTranslator
	{
		string TranslateApplication(string appName);
		string TranslateModule(string ns);
		string TranslateLibrary(string ns);
	}

	//--------------------------------------------------------------------------------
	public interface IActivationsTranslator
	{
		string TranslateActivation(string app);
		string TranslateActivation(string app, string mod);
	}

	//--------------------------------------------------------------------------------
	public interface IFunctionTranslator
	{
		string TranslateFunction(string functionName, string targetModule);

		string GetDescription(string functionName);
		
		bool IsUnsupportedFunction(string functionName);
		bool IsDocumentFunction(string functionName);
		bool IsFunctionWithContext(string functionName);

		bool IsOverloadedDocumentFunction(string functionName);

		bool IsWoorminfoFunction(string functionName);
		bool IsWoorminfoReturnValueFunction(string functionName);

		bool RetrieveSubstituteVariables(string functionName, out string [] variables, out string [] variableTypes);
		bool RetrieveSubstituteReturnValue(string functionName, out string variable, out string type);
		string GetReturnValueName(string functionName);
		bool NeedOtherParameters(string functionName);
	}

	//--------------------------------------------------------------------------------
	public interface IHotLinkTranslator
	{
		string TranslateHotLink(string hotLinkName);
		string GetDescription(string hotLinkName);
		bool WithContext(string hotLinkName);
	}

	//--------------------------------------------------------------------------------
	public interface ILinkTranslator
	{
		string TranslateLink(string linkName);
		string GetFormMasterTableName(string linkFormName);
		string GetGuid(string linkName);
	}

	//--------------------------------------------------------------------------------
	public interface IEnumTranslator
	{
		string TranslateEnumType(string enumName);
		string TranslateEnumValue(string enumName, string enumValue);
		string GetTagValue(string tagName);
		string GetItemValue(string tagName, string itemName);
	}

	//--------------------------------------------------------------------------------
	public interface IUnknownObjectTranslator
	{
		string TranslateObject(string message, string currentLine, string objectName);
	}

	//--------------------------------------------------------------------------------
	public interface IFileObjectTranslator
	{
		string TranslateFile(string fileName, bool isText, string reportNameSource, ArrayList reportExternalPaths, DestinationReportsData reportsData);
	}
}
