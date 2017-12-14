#pragma once

#include <afxdtctl.h>
#include <afxtempl.h>

#include "BarQyDlg.h"
#include "xmlgesinfo.h"
#include "xmlControls.h"


//includere alla fine degli include del .H
#include "beginh.dex"

class CAbstractFormDoc;
class CXMLDocObjectInfo;
class CDocumentObjectsDescription;
class CXMLDBTInfo;
class CXMLXRefInfo;
class CXMLSegmentInfo;

//----------------------------------------------------------------------------------
// CXMLDocGenerator dialog
//----------------------------------------------------------------------------------
class TB_EXPORT CXMLDocGenerator
{
private:
	CXMLDocObjectInfo*	MakeInfo(CAbstractFormDoc*);

public:
	CXMLDocObjectInfo*	GenerateNewXMLDocInfo(CAbstractFormDoc*, CXMLDocObjectInfo* pOldDocObjInfo); //data la descrizione in xml e il documento in memoria mette a posto le informazioni	

};

//----------------------------------------------------------------------------------
// CDocDescrMngPage dialog
//----------------------------------------------------------------------------------
class TB_EXPORT CDocDescrMngPage : public CParsedDialogWithTiles
{
	DECLARE_DYNAMIC(CDocDescrMngPage)
private:
	CTBNamespace 			m_nsDoc;
	CDocDescrTreeCtrl		m_TreeCtrl;
	int						m_nProfileVersion;
	CBCGPComboBox			m_cbEnvelopeClass; 
	CStrEdit				m_DataUrlEdit;
	CXMLDocObjectInfo*		m_pOldDocObjInfo;
	CXMLDocObjectInfo*		m_pNewDocObjInfo;
	CStrEdit				m_DocNsEdit;
	Array*					m_pAllDocuments; 

	int						m_oldModuleItemIdx;
	int						m_oldDocumentItemIdx;

public:
	BOOL					m_bModified;

public:
	CDocDescrMngPage	(CWnd* pParent = NULL);   // standard constructor
	~CDocDescrMngPage ();

private:
	void	EnableControls				(BOOL bEnable);
	void	FillModsCombo();
	void	FillHeader					();
	void	FillEnvClassCombo			(const CString&); 
	void	UpdateDataValue				(CXMLDocObjectInfo*);
	BOOL	SaveModified();
	BOOL	IsDifferentDescription();
	void	ReturnToOldSelection();
	virtual void OnCustomizeToolbar();

public:
	virtual void OnCancel				();

protected:
	virtual BOOL OnInitDialog				();

protected:
	// Generated message map functions
	//{{AFX_MSG(CDocDescrMngPage)
	afx_msg void OnLoadDescription			();
	afx_msg void OnSaveDescription			();	
	afx_msg void OnContextMenu(CWnd*, CPoint);
	afx_msg	void OnComboModsChanged();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};



#include "endh.dex"

