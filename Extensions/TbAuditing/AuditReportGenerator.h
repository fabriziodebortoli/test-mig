
#pragma once
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbGeneric\ReferenceObjectsInfo.h>
#include <TbGeneric\LineFile.h>
#include <TbGes\hotlink.h>
#include "beginh.dex"

class CTBNamespace;
class AuditingManager;
class SqlColumnInfo;
class CXMLFixedKeyArray;
class CLocalizableXMLDocument;


//////////////////////////////////////////////////////////////////////////////
//             					ReportGenerator
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT ReportGenerator
{
private:
	CTBNamespace*		m_pNamespace;
	AuditingManager*	m_pAuditMng;
	CXMLFixedKeyArray*	m_pFixedKey;
	CString				m_strPkColumns;
	CLocalizableXMLDocument* m_pXMLTitleDoc;

public:
	ReportGenerator(CTBNamespace*, AuditingManager*, CXMLFixedKeyArray*);

private:
	CString GetLocalizedTitle	(LPCTSTR szTag);
	void	Localize			(CString& strBuffer);

	int	 GetColumnWidth(const SqlColumnInfo* pItem);
	CString GetTypeWRM(const DataType& aDataType);
	void InsertColumns(CLineFile& fOutputFile);
	void InsertVariables(CLineFile& fOutputFile);
	void InsertTableFields(CLineFile& fOutputFile);
	void ModifyWhereClause(CLineFile& fOutputFile, CString& strBuffer);
	void WriteReport(CLineFile& fOutputFile, CString& strBuffer);
	CString GetReportName(const CString& strReportPath);

public:
	CTBNamespace* ReportGenerator::CreateReportFromTemplate(BOOL bAllUsers, const CString&);
};

#include "endh.dex"
