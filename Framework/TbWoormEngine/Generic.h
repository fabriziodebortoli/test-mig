
#pragma once

#include <TbGeneric\mlistbox.h>
#include <TbOleDb\sqlconnect.h>
#include <TbOleDb\sqlcatalog.h>

BOOL		OnToolHitTest				(const CWnd* pParentWnd, const CMultiListBox &list, CPoint point, TOOLINFO* pTI, const CString& strTableName);
BOOL		OnToolHitTestCriterion		(const CWnd* pParentWnd, const CEdit& edit, CPoint point, TOOLINFO* pTI, SqlConnection* conn);
CString		GetStringFromPos			(const CEdit& edit, int pos);
CString		GetTargetQualifiedName		(CString baseQualifiedName);
BOOL		OnToolHitTestTableName		(const CWnd* pParentWnd, const CEdit& edit, CPoint point, TOOLINFO* pTI);
CString		GetExtendedSelectedString	(const CEdit& edit, int startSelection, int endSelection );
