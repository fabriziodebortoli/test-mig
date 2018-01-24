

namespace TaskBuilderNetCore.Interfaces
{
	//=========================================================================
	public interface ModuleInfo //: ModuleInfo
	{
		string CustomPath { get; }
		System.Collections.ArrayList GetConfigFileArray();
		string GetCustomAllCompaniesAllUsersSettingsFullFilename(string settings);
		string GetCustomAllCompaniesAllUsersSettingsPath();
		string GetCustomAllCompaniesUserSettingsFullFilename(string settings);
		string GetCustomAllCompaniesUserSettingsPath();
		string GetCustomCompanyAllUserSettingsPath();
		string GetCustomCompanyAllUserSettingsPathFullFilename(string settings);
		string GetCustomCompanyUserSettingsPath();
		string GetCustomCompanyUserSettingsPathFullFilename(string settings);
		string GetCustomDocumentPath(string documentName);
		string GetCustomDocumentSchemaFilesPath(string documentName, string userName);
		string GetCustomDocumentSchemaFilesPath(string documentName);
		string GetCustomDocumentSchemaFullFilename(string documentName, string schemaName, string user);
		string GetCustomDocumentSchemaFullFilename(string documentName, string schemaName);
		string GetCustomExcelDocument2007FullFilename(string document, string user);
		string GetCustomExcelDocumentFullFilename(string document);
		string GetCustomExcelDocumentFullFilename(string document, string user);
		string GetCustomExcelFilesPath(string userName);
		string GetCustomExcelFilesPath();
		string GetCustomExcelTemplate2007FullFilename(string template, string user);
		string GetCustomExcelTemplateFullFilename(string template, string user);
		string GetCustomExcelTemplateFullFilename(string template);
		string GetCustomFileFullFilename(string text);
		string GetCustomFileFullFilename(string text, string user);
		string GetCustomFilePath();
		string GetCustomFilePath(string userName);
		string GetCustomImageFullFilename(string image, string user);
		string GetCustomImageFullFilename(string image);
		string GetCustomImagePath();
		string GetCustomImagePath(string userName);
		string GetCustomReportFullFilename(string report, string user);
		string GetCustomReportFullFilename(string report);
		string GetCustomReportPath(string userName);
		string GetCustomReportPath();
		string GetCustomReportSchemaFilesPath();
		string GetCustomReportSchemaFilesPath(string userName);
		string GetCustomReportSchemaFullFilename(string report);
		string GetCustomReportSchemaFullFilename(string report, string user);
		string GetCustomTextFullFilename(string text, string user);
		string GetCustomTextFullFilename(string text);
		string GetCustomTextPath(string userName);
		string GetCustomTextPath();
		string GetCustomWordDocument2007FullFilename(string document, string user);
		string GetCustomWordDocumentFullFilename(string document);
		string GetCustomWordDocumentFullFilename(string document, string user);
		string GetCustomWordFilesPath();
		string GetCustomWordFilesPath(string userName);
		string GetCustomWordTemplate2007FullFilename(string template, string user);
		string GetCustomWordTemplateFullFilename(string template, string user);
		string GetCustomWordTemplateFullFilename(string template);
	}
}
