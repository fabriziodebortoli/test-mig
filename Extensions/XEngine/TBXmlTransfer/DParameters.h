#pragma once 

#include <TBGENERIC\dataobj.h>
#include <TBGES\extdoc.h>
#include <TBOLEDB\sqlrec.h>
#include <TBGES\dbt.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class TXEParameters;
class DXEParameters;

//////////////////////////////////////////////////////////////////////////////
//         class DBTXEParameters definition 
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
class TB_EXPORT DBTXEParameters : public DBTMaster
{ 
	DECLARE_DYNAMIC(DBTXEParameters)

public:
	DBTXEParameters(CRuntimeClass*, CAbstractFormDoc*);

public:
	TXEParameters* GetXEParameters	() const { return (TXEParameters*) GetRecord(); }

protected: // Gestione delle query
	DXEParameters*	GetDocument		() { return (DXEParameters*)m_pDocument; }

	virtual void	OnEnableControlsForFind	() {}
	virtual	void	OnDefineQuery			();
	virtual	void	OnPrepareQuery			();
	virtual	void	OnPrepareBrowser		(SqlTable* pTable);
	virtual	BOOL	OnCheckPrimaryKey		();
	virtual	void	OnPreparePrimaryKey		();
	virtual void	OnDisableControlsForEdit();
};

//////////////////////////////////////////////////////////////////////////////
//     class DXEParameters definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DXEParameters : public CAbstractFormDoc
{
	DECLARE_DYNCREATE(DXEParameters)

public:
	DXEParameters() : m_pMaxDoc(NULL), m_pMaxKByte(NULL), m_pPaddingNum(NULL), m_pDBTXEParameters(NULL) {}

public: 
	DBTXEParameters*	m_pDBTXEParameters;

public:
	TXEParameters*	GetXEParameters() const;

public:
	CParsedCtrl *m_pMaxDoc;
	CParsedCtrl *m_pMaxKByte;
	CParsedCtrl *m_pPaddingNum;

	DataBool	m_bUTF16;
	DataStr		m_strServerName;

private:
	void	SaveXEParameters	();
	void	CheckIsLocalPath	(const CString& strPath);
	CString OnSelPath			(const CString& strPath);
	BOOL	SetEmptyPath		();

protected:
	virtual BOOL	OnAttachData 		();

	virtual BOOL	CanDoEditRecord		();
	virtual BOOL	CanDoNewRecord		();

	virtual BOOL 	OnEditTransaction	()	{ SaveXEParameters(); return TRUE; }
	virtual BOOL 	OnNewTransaction	()	{ SaveXEParameters(); return TRUE; }

	virtual BOOL	OnOkTransaction		();
	virtual	BOOL	OnPrepareAuxData	();

	// servono?
	virtual BOOL	CanDoDeleteRecord	()	{ return FALSE; }
	virtual BOOL	CanDoFindRecord		()	{ return FALSE; }
	virtual BOOL	CanDoQuery			()	{ return FALSE; }
	virtual BOOL	CanDoRadar			()	{ return FALSE; }
	virtual BOOL	CanDoProperties		()	{ return FALSE; }

protected:	
	// Generated message map functions
	//{{AFX_MSG(DXEParameters)
	afx_msg void OnSiteChanged();
	afx_msg void OnExportPathChanged();
	afx_msg void OnImportPathChanged();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"