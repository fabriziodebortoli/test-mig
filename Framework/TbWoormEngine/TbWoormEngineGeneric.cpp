
#include "StdAfx.h"
#include "generic.h"
#include <tbgenlib\tbstrings.h>

//----------------------------------------------------------------------------
BOOL OnToolHitTest(const CWnd* pParentWnd, const CMultiListBox& list, CPoint point, TOOLINFO* pTI, const CString& strTableName)
{
	CRect rect;
	list.GetWindowRect(&rect);
	pParentWnd->ScreenToClient(&rect);
	
	if (rect.PtInRect(point)) 
	{
		BOOL bOutside;
		pParentWnd->MapWindowPoints((CWnd*)&list, &point, 1);
		UINT idx = list.ItemFromPoint(point, bOutside);

		CString strBaseText = list.GetString1(idx);
		CString strText = AfxLoadDatabaseString(strBaseText, strTableName.IsEmpty()? strBaseText : strTableName);
		
		TCHAR* s = new TCHAR[200]; 
		TB_TCSCPY(s, strText);
		pTI->lpszText = s;
		
		pTI->hwnd = pParentWnd->m_hWnd;
		pTI->uId = (UINT)list.m_hWnd;
		pTI->uFlags |= (TTF_IDISHWND | TTF_SUBCLASS);

		return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------
BOOL OnToolHitTestCriterion(const CWnd* pParentWnd, const CEdit& edit, CPoint point, TOOLINFO* pTI, SqlConnection* conn)
{ 
	CString baseQualifiedName, targetQualifiedName;
	CRect rect;
	edit.GetWindowRect(&rect);
	pParentWnd->ScreenToClient(&rect);
	int start, end;
	edit.GetSel(start, end);
    CString selectedColumn = _T("");
    CString selectedTable = _T("");
	if (end != 0) //exists a selected string 
	{
		CString extendedSelection = GetExtendedSelectedString(edit, start, end);
				int curPos = 0; 
		selectedTable = extendedSelection.Tokenize(_TB("."), curPos);
		if (curPos != -1)
			selectedColumn = extendedSelection.Tokenize(_TB("."), curPos);
	}
	if (rect.PtInRect(point)) 
	{   //valid selection
		if ( conn->ExistTable(selectedTable) && conn->GetTableInfo(selectedTable)->ExistColumn(selectedColumn))
		{     
				CString targetQualifiedName = AfxLoadDatabaseString(selectedTable, selectedTable) + _TB(".") + AfxLoadDatabaseString(selectedColumn, selectedTable);
	 
				TCHAR* s = new TCHAR[200]; 
				TB_TCSCPY(s, targetQualifiedName);
				pTI->lpszText = s;
				pTI->hwnd = pParentWnd->m_hWnd;
				pTI->uId = (UINT)edit.m_hWnd;
				pTI->uFlags |= (TTF_IDISHWND | TTF_SUBCLASS);
				return TRUE;
		}
		else  //retrieve information from pointed char
		{
				CPoint newP = point;
				pParentWnd->ClientToScreen(&newP);
				edit.ScreenToClient(&newP);
				int pos = edit.CharFromPos(newP);
				baseQualifiedName = GetStringFromPos(edit, pos);
		       
				if (baseQualifiedName.Find(_T(".")) >= 0)
				{  
					targetQualifiedName = GetTargetQualifiedName(baseQualifiedName);
					TCHAR* s = new TCHAR[200]; 
					TB_TCSCPY(s, targetQualifiedName);
					pTI->lpszText = s;
					pTI->hwnd = pParentWnd->m_hWnd;
					pTI->uId = (UINT)edit.m_hWnd;
					pTI->uFlags |= (TTF_IDISHWND | TTF_SUBCLASS);

					return TRUE;
				}
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
CString GetStringFromPos (const CEdit& edit, int pos)
{
	if (pos == -1)
	{
	    //TRACE("NN\n");	
		return _T("");
	}
    int nCharIndex = LOWORD(pos);
	int startBase = nCharIndex;
	int endBase = nCharIndex;
	
//	TRACE("index char %d\n",nCharIndex);
	CString wclause;
	edit.GetWindowText(wclause);
	while ( 
		   ( _istalnum(wclause[startBase]) || wclause[startBase] == '.' || wclause[startBase] == '_' ) 
            &&
			startBase > 0
		   )
			startBase--;
	while ( 
		    ( _istalnum(wclause[endBase]) || wclause[endBase] == '.' || wclause[endBase] == '_' )
			&&
			endBase < wclause.GetLength()
		   )
			endBase++;
   
    if (startBase !=0 ) 
		  startBase++;
    
	CString ret = wclause.Mid(startBase, endBase - startBase);
	//TRACE(ret);
	return (ret);
}

//----------------------------------------------------------------------------
CString GetTargetQualifiedName( CString baseQualifiedName)
{
	if (baseQualifiedName.Find(_T(".")) >= 0)
		{
		 int curPos = 0; 
		 CString tableName = baseQualifiedName.Tokenize(_TB("."), curPos);
	     CString columnName = baseQualifiedName.Tokenize(_TB("."), curPos);
	     CString strText = AfxLoadDatabaseString(tableName, tableName) + _TB(".") + AfxLoadDatabaseString(columnName, tableName);
	     return strText; 
		}
	return CString("");
}
//----------------------------------------------------------------------------
CString GetExtendedSelectedString ( const CEdit& edit, int startSelection, int endSelection )
{  //returns a string with format table.column  if selection was a subpart of it 
  CString selectedString, editText, selectedColumn, selectedTable;
  edit.GetWindowText(editText);
  //da come inizializzo start & end dipende comportamento
  int startBase = startSelection; 
  int endBase = endSelection;  

  while ( 
		   ( _istalnum(editText[startBase]) || editText[startBase] == '.' || editText[startBase] == '_' ) 
            &&
			startBase > 0
		   )
			startBase--;
  while ( 
		    ( _istalnum(editText[endBase]) || editText[endBase] == '.' || editText[endBase] == '_' )
			&&
			endBase < editText.GetLength()
		   )
			endBase++;
   
  if (startBase !=0 ) 
		  startBase++;
    
  CString ret = editText.Mid(startBase, endBase - startBase);
  if ( ret.Find(_T('.')) >= 0 )	  
	 return ret;
  
  return CString("");
}

//---------------------------------------------------------------------------
BOOL OnToolHitTestTableName(const CWnd* pParentWnd,const CEdit& edit, CPoint point, TOOLINFO* pTI)
{ 
	CRect rect;
	edit.GetWindowRect(&rect);
	pParentWnd->ScreenToClient(&rect);
	
	if (rect.PtInRect(point)) 
	{
		pParentWnd->MapWindowPoints((CWnd*)&edit, &point, 1);

		CString strBaseText;
		edit.GetWindowText(strBaseText); 
//----
		if ( strBaseText.GetAt(0)== CString("["))
		        strBaseText = strBaseText.Mid(5);
		TCHAR* nextToken;
		TCHAR*	pszList = strBaseText.GetBuffer();
		LPCTSTR  pTableName = _tcstok_s( pszList, _T(","), &nextToken );
		CString strText;
		while( pTableName != NULL )
		{   
			CString strTableName = pTableName;
			strTableName.Trim();	
			strText = strText +  AfxLoadDatabaseString(strTableName, strTableName) + CString(", "); 
			/* Get next token: */
			pTableName = _tcstok_s( NULL,  _T(","), &nextToken);
		}
		strBaseText.ReleaseBuffer();
        strText = strText.Left(strText.GetLength() - 2);
		
		TCHAR* s = new TCHAR[200]; 
		TB_TCSCPY(s, strText);
		pTI->lpszText = s;
		
		pTI->hwnd = pParentWnd->m_hWnd;
		pTI->uId = (UINT)edit.m_hWnd;
		pTI->uFlags |= (TTF_IDISHWND | TTF_SUBCLASS);

		return TRUE;
	}

	return FALSE;
}