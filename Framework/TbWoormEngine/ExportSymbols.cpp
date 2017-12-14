
#include "stdafx.h"
#include <stdio.h>

#include <TbXMLCore\XMLDocObj.h>
#include <TbNameSolver\TbNamespaces.h>
#include <TbNameSolver\ThreadContext.h>

#include "ExportSymbols.h"

IMPLEMENT_DYNCREATE(CExportSymbols, CObject)

//----------------------------------------------------------------------------
CExportSymbols::CExportSymbols()
{
	m_bIsActivated = FALSE;
}

//----------------------------------------------------------------------------
CExportSymbols* AfxGetExportSymbols()
{
	return AfxGetLoginContext()->GetObject<CExportSymbols>();
}

//-----------------------------------------------------------------------------
void CExportSymbols::Activate() 
{
	AfxGetExportSymbols()->m_bIsActivated = TRUE;
}
//-----------------------------------------------------------------------------
BOOL CExportSymbols::IsActivated() 
{
	return AfxGetExportSymbols()->m_bIsActivated; 
}
//-----------------------------------------------------------------------------
void CExportSymbols::DeActivate() 
{
	AfxGetLoginContext()->GetObject<CExportSymbols>()->m_bIsActivated = FALSE;
	m_arFiles.RemoveAll();
}

//-----------------------------------------------------------------------------
BOOL CExportSymbols::AddFile(const CString& strFileName, const CString& strType)
{
	for (int i=0; i < m_arFiles.GetSize(); i++)
	{
		_CExportSymbolsFile* f = (_CExportSymbolsFile*) m_arFiles.GetAt(i);
		if (strFileName.CompareNoCase(f->m_strName) == 0)
			return FALSE;
	}
	m_arFiles.Add(new _CExportSymbolsFile(strFileName, strType));
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CExportSymbols::AddItemOnLastFile(const CString& strName, const CString& strType, const DataType& dt)
{
	if (m_arFiles.GetSize() == 0)
	{
		TRACE(_T("File non specificato\n"));
		ASSERT(FALSE);
		return FALSE;
	}

	_CExportSymbolsFile* f = (_CExportSymbolsFile*) m_arFiles.GetAt(m_arFiles.GetUpperBound());

	for (int i=0; i < f->m_arIdents.GetSize(); i++)
	{
		_CExportSymbolsIdent* id = (_CExportSymbolsIdent*) f->m_arIdents.GetAt(i);
		if (strName.CompareNoCase(id->m_strName) == 0 && strType.CompareNoCase(id->m_strType) == 0)
		{
			TRACE(_T("Identificatore duplicato %s nel file %s\n"), strName, f->m_strName);
			ASSERT(FALSE);
			return FALSE;
		}
	}

	f->m_arIdents.Add(new _CExportSymbolsIdent(strName, strType, dt));
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CExportSymbols::ChangeItemTypeOnLastFile(const CString& strName, const CString& strOldType, const CString& strNewType)
{
	if (m_arFiles.GetSize() == 0)
	{
		TRACE(_T("File non specificato\n"));
		ASSERT(FALSE);
		return FALSE;
	}

	_CExportSymbolsFile* f = (_CExportSymbolsFile*) m_arFiles.GetAt(m_arFiles.GetUpperBound());

	for (int i=0; i < f->m_arIdents.GetSize(); i++)
	{
		_CExportSymbolsIdent* id = (_CExportSymbolsIdent*) f->m_arIdents.GetAt(i);
		if (strName.CompareNoCase(id->m_strName) == 0)
		{ 
			if (id->m_strType.CompareNoCase(strNewType) == 0) 
				return TRUE;
			if (id->m_strType.CompareNoCase(strOldType) == 0) 
			{
				id->m_strType = strNewType;
				return TRUE;
			}
		}
	}
	TRACE(_T("Ident %s not found in file %s\n"), strName, f->m_strName);
	ASSERT(FALSE);
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CExportSymbols::GetItemsFileFilteredByType(const CString& strFileName, const CString& strType, DataTypeNamedArray& arItems)
{
	if (m_arFiles.GetSize() == 0)
	{
		TRACE(_T("File non specificato\n"));
		ASSERT(FALSE);
		return FALSE;
	}

	_CExportSymbolsFile* f = (_CExportSymbolsFile*) m_arFiles.GetAt(m_arFiles.GetUpperBound());
	int i = 0;
	for (i = 0; i < m_arFiles.GetSize(); i++)
	{
		f = (_CExportSymbolsFile*) m_arFiles.GetAt(i);
		if (strFileName.CompareNoCase(f->m_strName) == 0)
			break;
	}
	if (i > m_arFiles.GetSize())
		return FALSE;

	for (i = 0; i < f->m_arIdents.GetSize(); i++)
	{
		_CExportSymbolsIdent* id = (_CExportSymbolsIdent*) f->m_arIdents.GetAt(i);
		if (id->m_strType.CompareNoCase(strType) == 0)
			arItems.Add(id->m_strName, id->m_DataType);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CExportSymbols::AddColumnNameToItemOnLastFile(const CString& strName, const CString& strColumn)
{
	if (m_arFiles.GetSize() == 0)
	{
		TRACE(_T("File non found\n"));
		ASSERT(FALSE);
		return FALSE;
	}

	_CExportSymbolsFile* f = (_CExportSymbolsFile*) m_arFiles.GetAt(m_arFiles.GetUpperBound());

	int i = 0;
	for (i = 0 ; i < f->m_arIdents.GetSize() ; i++)
	{
		_CExportSymbolsIdent* id = (_CExportSymbolsIdent*) f->m_arIdents.GetAt(i);
		if (strName.CompareNoCase(id->m_strName) == 0 && id->m_strType.Find(EXP_SYMB_VAR) == 0)
			break;
	}
	if (i == f->m_arIdents.GetSize())
	{
		TRACE(_T("file %s:\nIdent %s not found\n"), f->m_strName, strName);
		ASSERT(FALSE);
		return FALSE;
	}

	_CExportSymbolsIdent* id = (_CExportSymbolsIdent*) f->m_arIdents.GetAt(i);

	id->m_strColumn = strColumn;
	//if (!strColumn.IsEmpty())  //NO! var-Column è per le colonne delle tabelle
	//	id->m_strType = EXP_SYMB_VAR_COLUMN;
	return TRUE;
}

//-----------------------------------------------------------------------------
void CExportSymbols::SaveCSV(LPCTSTR szFileName)
{
	int nR = 0;
	FILE* fp;
	_tfopen_s(&fp, szFileName, _T("w"));
	//_ftprintf(fp, _T("Report;Identificatore;Tipo;N.Riga;Colonna\n"));

	for (int i=0; i < m_arFiles.GetSize(); i++)
	{
		_CExportSymbolsFile* f = (_CExportSymbolsFile*) m_arFiles.GetAt(i);

		for (int j=0; j < f->m_arIdents.GetSize(); j++)
		{
			_CExportSymbolsIdent* id = (_CExportSymbolsIdent*) f->m_arIdents.GetAt(j);

			_ftprintf(fp, _T("%s;%s;%s;%d;%s\n"), (LPCTSTR)f->m_strName, (LPCTSTR)id->m_strName, (LPCTSTR)id->m_strType, ++nR, (LPCTSTR)id->m_strColumn);
		}
	}
	fclose(fp);
}

//-----------------------------------------------------------------------------
void CExportSymbols::SaveCSV2(LPCTSTR szFileName)
{
	FILE* fp;
	_tfopen_s(&fp, szFileName, _T("w"));
	//_ftprintf(fp, _T("oldId,newId,type\n"));

	for (int i=0; i < m_arFiles.GetSize(); i++)
	{
		_CExportSymbolsFile* f = (_CExportSymbolsFile*) m_arFiles.GetAt(i);

		for (int j=0; j < f->m_arIdents.GetSize(); j++)
		{
			_CExportSymbolsIdent* id = (_CExportSymbolsIdent*) f->m_arIdents.GetAt(j);

			CString sType;
			CString sColumnName(_T(' '));

			if (id->m_strType.CompareNoCase(EXP_SYMB_PROCEDURE) == 0)
				sType = _T("O");
			else  if (id->m_strType.CompareNoCase(EXP_SYMB_EVENT) == 0)
				sType = _T("E");
			else  if (id->m_strType.CompareNoCase(EXP_SYMB_DIALOG) == 0)
				sType = _T("A");
			else if (id->m_strType.Find(EXP_SYMB_VAR) == 0)
			{
				sType = _T("V");
				if (!id->m_strColumn.IsEmpty())
				{
					int pos = id->m_strColumn.Find(_T('.'));
					if (pos > 0)
						sColumnName = id->m_strColumn.Mid(pos+1);
				}
			}
			else
				continue;
				//#define EXP_SYMB_FONT _T("Font")
				//#define EXP_SYMB_FORMATTER _T("Formatter")

			_ftprintf(fp, _T("%s,%s,%s\n"), (LPCTSTR)(id->m_strName), (LPCTSTR)sColumnName, (LPCTSTR)sType);
		}
	}
	fclose(fp);
}

//-----------------------------------------------------------------------------
void CExportSymbols::SaveXML(LPCTSTR szFileName)
{
	CXMLDocumentObject oXMLDoc(TRUE);
	CXMLNode* pRootNode = oXMLDoc.CreateRoot(_T("Application"));
	if(!pRootNode)
	{
		ASSERT(FALSE);
		return;
	}
	pRootNode->SetAttribute(_T("source"), _T("TbAppManager"));

	CXMLNode* pModuleNode = NULL;
	CString strModuleName;

	for (int i=0; i < m_arFiles.GetSize(); i++)
	{
		_CExportSymbolsFile* f = (_CExportSymbolsFile*) m_arFiles.GetAt(i);

		CTBNamespace nsReport (_T("Report.") + f->m_strName);

		if (strModuleName.CompareNoCase(nsReport.GetModuleName()))
		{
			pModuleNode = pRootNode->CreateNewChild(_T("Module"));
			if (!pModuleNode)
			{
				ASSERT(FALSE);
				return;
			}	
			strModuleName = nsReport.GetModuleName();
			pModuleNode->SetAttribute(_T("source"), strModuleName);
		}
		
		CXMLNode* pReportNode = pModuleNode->CreateNewChild(_T("Report"));
		if (!pReportNode)
		{
			ASSERT(FALSE);
			return;
		}

		CString sName = nsReport.GetObjectName();
		if (sName.Find('.') > 0) sName = sName.Left(sName.Find('.'));

		pReportNode->SetAttribute(_T("source"), sName);
		pReportNode->SetAttribute(_T("target"), _T(""));
		//pReportNode->SetAttribute(_T("Description"), _T(""));

		for (int j=0; j < f->m_arIdents.GetSize(); j++)
		{
			_CExportSymbolsIdent* id = (_CExportSymbolsIdent*) f->m_arIdents.GetAt(j);

			CXMLNode* pIdentNode = pReportNode->CreateNewChild(id->m_strType);
			if (!pIdentNode)
			{
				ASSERT(FALSE);
				return;
			}

			pIdentNode->SetAttribute(_T("source"), id->m_strName);
			pIdentNode->SetAttribute(_T("target"), _T(""));
			if (!id->m_strColumn.IsEmpty())
				pIdentNode->SetAttribute(_T("Column"), id->m_strColumn);
		}
	}
	oXMLDoc.SaveXMLFile(szFileName, TRUE);
}

//-----------------------------------------------------------------------------

	
