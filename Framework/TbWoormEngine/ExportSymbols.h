#pragma once

#include <TbGeneric\Globals.h>
#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>

//includere alla fine degli include del .H
#include "beginh.dex"


class TB_EXPORT CExportSymbols : public CObject
{
	DECLARE_DYNCREATE(CExportSymbols)

private:
	class _CExportSymbolsIdent : public CObject
	{
	public:
		CString m_strName;
		CString m_strType;
		CString m_strColumn;
		DataType m_DataType;

		_CExportSymbolsIdent(const CString& strName, const CString& strType, const DataType aDataType = DataType::Null) 
			: 
			m_strName(strName), 
			m_strType(strType),
			m_DataType(aDataType)
			{}
	};

	class _CExportSymbolsFile : public CObject
	{
	public:
		CString m_strName;
		CString m_strType;

		Array m_arIdents;

		_CExportSymbolsFile(const CString& strName, const CString& strType) 
			: 
			m_strName(strName),
			m_strType(strType) 
			{}

	};

	Array m_arFiles;

private:
	BOOL m_bIsActivated;

protected:
	CExportSymbols();

public:
	BOOL AddFile(const CString& strFileName, const CString& strType);
	BOOL AddItemOnLastFile(const CString& strName, const CString& strType, const DataType& m_DataType = DataType::Null);
	BOOL ChangeItemTypeOnLastFile(const CString& strName, const CString& strOldType, const CString& strNewType);
	//void AddItemOnFile(const CString& strFileName, const CString& strName, const CString& strType);
	BOOL AddColumnNameToItemOnLastFile(const CString& strName, const CString& strColumn);
	BOOL GetItemsFileFilteredByType(const CString& strFileName, const CString& strType, DataTypeNamedArray& arItems);

	void SaveCSV(LPCTSTR);
	void SaveXML(LPCTSTR);
	void SaveCSV2(LPCTSTR);

	void Activate();
	BOOL IsActivated();
	void DeActivate();

	
};

TB_EXPORT CExportSymbols * AfxGetExportSymbols();

#define EXP_SYMB_REPORT _T("Report")

#define EXP_SYMB_VAR			_T("Var")
#define EXP_SYMB_VAR_INPUT		_T("Var-Input")
#define EXP_SYMB_VAR_ASK		_T("Var-Ask")
#define EXP_SYMB_VAR_COLUMN		_T("Var-Column")

#define EXP_SYMB_PROCEDURE _T("Procedure")
#define EXP_SYMB_EVENT _T("Event")
#define EXP_SYMB_DIALOG _T("Dialog")
#define EXP_SYMB_FONT _T("Font")
#define EXP_SYMB_FORMATTER _T("Formatter")

#include "endh.dex"
