
#pragma once

#include <TbGeneric\Array.h>
#include <TbParser\Parser.h>
#include <TbGenlib\basedoc.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class CWoormDocMng;
class Parser;
class Unparser;


TB_EXPORT CString MakeDosName (const CString& strName);


// proprietà del report
///////////////////////////////////////////////////////////////////////////////
//	class CDocProperty
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CDocProperties: public CObject
{
public:
	CWoormDocMng*		m_pWoormDoc;
	CString			m_strTitle;
	CString			m_strSubject;
	CString			m_strAuthor;
	CString			m_strCompany;
	CString			m_strComments;
	CString			m_strDefaultSecurityRoles;
	BOOL			m_bModifyFlag;
	// NOTA le informazioni legati alla dimensione, posizione, date (creazione,ultima modifica,ultimo accesso)
	// li chiedo ogni volta al sistema. Non li memorizzo!!!

public:
	CDocProperties(CWoormDocMng*);
	
public:
	BOOL Parse				(Parser&);	
	void Unparse			(Unparser&);				
	void SetModifyFlag		(BOOL bModify = TRUE)	{ m_bModifyFlag = bModify; }
	BOOL IsModified			()						{ return m_bModifyFlag; }
	BOOL IsEmpty();
};



///////////////////////////////////////////////////////////////////////////////
//					class CGeneralPropPage
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CGeneralPropPage : public CLocalizablePropertyPage
{
	DECLARE_DYNAMIC(CGeneralPropPage)
private:
	CString m_strFileName;

public:
	CGeneralPropPage(const CString&);

public:
	virtual BOOL OnInitDialog	();

};

///////////////////////////////////////////////////////////////////////////////
//					class CSummaryPage
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CSummaryPage : public CLocalizablePropertyPage
{
	DECLARE_DYNAMIC(CSummaryPage)
private:
	CDocProperties* m_pDocProperties;
	CString			m_strTitle;
	CString			m_strSubject;

public:
	CSummaryPage(CDocProperties*);

public:
	virtual BOOL OnInitDialog	();
	virtual void DoDataExchange	(CDataExchange* pDX);    // DDX/DDV support
	virtual void OnOK();
};


///////////////////////////////////////////////////////////////////////////////
//					class CDocPropertiesSheet
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CDocPropertiesSheet : public CLocalizablePropertySheet
{
	DECLARE_DYNAMIC(CDocPropertiesSheet)
public:
	CDocProperties*	m_pDocProperties;
	CGeneralPropPage*	m_pGeneralPage;
	CSummaryPage*		m_pSummaryPage;
	//CHistoryPage* m_pHistoryPage;

public:
	CDocPropertiesSheet(CDocProperties*);
	~CDocPropertiesSheet ();

// Diagnostics
#ifdef _DEBUG
	void Dump(CDumpContext& dc) const
		{ ASSERT_VALID(this); AFX_DUMP0(dc, " CDocPropertiesSheet\n"); CLocalizablePropertySheet::Dump(dc); }
#endif // _DEBUG
};


//===========================================================================
TB_EXPORT void DoPropertiesSheet(CWoormDocMng*);

#include "endh.dex"
