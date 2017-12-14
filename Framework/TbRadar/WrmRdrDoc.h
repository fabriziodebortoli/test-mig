
#pragma once

#include <TbWoormViewer\woormdoc.h>

#include <TbGes\extdoc.h>
#include <TbGes\TbRadarInterface.h>

//includere alla fine degli include del .H
#include "beginh.dex"


// Classi definite nel WrmRadar
//=============================================================================
class CWrmRadarFrame;
class CWrmRadarView;
class CWrmRadarDoc;
class Table;
//	Definizione di CWrmRadarDoc
//=============================================================================
class TB_EXPORT CWrmRadarDoc : public CWoormDocMng, public ITBRadar
{
	DECLARE_DYNCREATE(CWrmRadarDoc)
protected:
	BOOL m_bTemporary;

public:
	CWrmRadarDoc	();
	~CWrmRadarDoc	();

public:
	virtual BOOL    OnOpenDocument	(LPCTSTR pszPathName);

	// Ritornano la view e la frame associate al documento
	CWrmRadarView*	GetWrmRadarView		();
	CWrmRadarFrame*	GetWrmRadarFrame	();

	BOOL ChangeStayAlive	();

	void Attach	(CAbstractFormDoc* pCallerDoc, BOOL bTemporary);
	void Attach (HotKeyLink*, SqlTable*, SqlRecord*);
	void Detach	();

	BOOL OnWrmRadarRecordSelected	(BOOL bActivateDoc = TRUE);

	BOOL	RecordSelected			(Table* pTable, int nRow, BOOL bLinked, BOOL bActivateDoc);
	BOOL	CanDoExit			(); 
	Table*	GetConnectedTable	();
	void	VKUpDown			(BOOL bUp = TRUE);

	void	SetTemporary		(BOOL bSet)		{ m_bTemporary = bSet; }
	BOOL	IsTemporary			() { return m_bTemporary;}

	void	ActivateAndShowRadar(BOOL = TRUE)	{ Activate(); }
	void	CloseRadar			();

	CBaseDocument*		GetDocument			() { return this; }

	virtual void		Run					(const CString& sAuxWhereClause);
	virtual CString		UpdateWhereClause	(SqlTable* pSqlTable);
	virtual BOOL		Customize			(HotKeyLinkObj::SelectionType, CString& sAuxQuery);

	BOOL				SaveModified	()	{ return CWoormDoc::SaveModified(); }

    afx_msg	void OnVKUp				();
    afx_msg	void OnVKDown			();

protected:
	BOOL	GetDataOfRecordSelected	(Table* pTable, int nRow);

	virtual BOOL CanDoExportData	(UINT nID);
	virtual BOOL CanDoSendMail		();
	virtual BOOL CanPushToClient()		{return FALSE;}

	// Generated message map functions
	//{{AFX_MSG(CWrmRadarDoc)
	afx_msg void OnFileSave			();
	afx_msg void OnFileSaveAs		();
    afx_msg	void OnVKReturn			();
    afx_msg	void OnEscape			();
    afx_msg	void OnAllowEditing		();
	
	afx_msg void OnUpdateMinusMultiLines();
	afx_msg void OnUpdatePlusMultiLines();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()

// Diagnostics
#ifdef _DEBUG
public:
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const { CWoormDoc::AssertValid(); }
#endif // _DEBUG
};
//=============================================================================

#include "endh.dex"
