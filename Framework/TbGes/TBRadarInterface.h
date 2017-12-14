
#pragma once

#include <TbNameSolver\TBNameSpaces.h>
#include <TbGenlib\HLinkObj.h>
#include "extdoc.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class SqlRecord;
class SqlTable;
class HotKeyLink;
class CBaseDocument;
class CAbstractFormDoc;

//--------------------------------------------------------------------------
class TB_EXPORT ITBRadar
{
protected:
	// Flag per la gestione dell'opzione 'stay alive'
	BOOL m_bEnableStayAlive;
	BOOL m_bStayAlive;
	BOOL m_bIsWaiting;
	BOOL m_bDocumentCalled;
	BOOL m_bCanBeParentWindow; //indica se il radar puo' essere parent per altre finestre: in chiusura questo non deve avvenire!

	// Tipo di Sqlrecord (usato come buffer di scambio) sul quale il WrmRadar deve operare.
	SqlRecord*			m_pCallerSqlRecord;
	SqlRecord*			m_pDynamicSqlRecord;	// SqlRecord a cui il WrmRadar è collegato
	CAbstractFormDoc*	m_pCallerDoc;			// Documento al quale il WrmRadar è collegato
	HotKeyLink*			m_pCallerHotLink;		// Htlink al quale il WrmRadar è collegato

public:

	ITBRadar() 
		:
		m_bEnableStayAlive	(FALSE),
		m_bStayAlive		(FALSE),
		m_bIsWaiting		(FALSE),
		m_bDocumentCalled	(FALSE),
		m_pCallerDoc		(NULL),
		m_pCallerHotLink	(NULL),
		m_pCallerSqlRecord	(NULL),
		m_pDynamicSqlRecord	(NULL),
		m_bCanBeParentWindow(TRUE)
		{}

	virtual ~ITBRadar() {}

	virtual void Attach (HotKeyLink*, SqlTable*, SqlRecord*) = 0;
	virtual void Attach (CAbstractFormDoc*, BOOL) = 0;

	virtual void Detach() = 0;
	virtual void CloseRadar() = 0; 
	virtual CBaseDocument* GetDocument() = 0;
	virtual void ActivateAndShowRadar(BOOL  = TRUE) = 0; 
	virtual BOOL SaveModified() = 0;

	virtual void		Run(const CString&)									{}
	virtual CString		UpdateWhereClause(SqlTable*)						{ return _T(""); }
	virtual BOOL		Customize(HotKeyLinkObj::SelectionType, CString&)	{ return FALSE; }

	virtual void		SetDefaultSelect(const CString& sQualifiedName) {}

	virtual void		StartReady() {}

	void EnableStayAlive(BOOL bEnable = TRUE, BOOL bSet = FALSE)	
	{ 
		m_bEnableStayAlive = bEnable;
		m_bStayAlive = bSet; 
	}

	BOOL				IsEnableStayAlive	() { return m_bEnableStayAlive; }
	BOOL				IsStayAlive			() { return m_bEnableStayAlive && m_bStayAlive;}

	void				SetWaiting			(BOOL bWaiting) { m_bIsWaiting = bWaiting;}
	BOOL				IsWaiting			() { return m_bIsWaiting;}
	
	HotKeyLink*			GetCallerHotLink	() { return m_pCallerHotLink; }
	CAbstractFormDoc*	GetCallerDoc		() { return m_pCallerDoc; }
	void				SetCanBeParentWindow(BOOL bSet) { m_bCanBeParentWindow = bSet; }
	BOOL				CanBeParentWindow() { return m_bCanBeParentWindow; }

	void OnRadarDied()
				{
					// Notifica al documento (chiamata da DBT)
					if (m_pCallerDoc)
					{
						CAbstractFormDoc* pDoc = m_pCallerDoc;
						m_pCallerDoc = NULL;

						pDoc->OnRadarDied(this);
					}
					else if (m_pCallerHotLink)// Notifica all'HotLink.
					{
						ASSERT (m_pCallerDoc == NULL);

						HotKeyLink* pHKL = m_pCallerHotLink;
						m_pCallerHotLink = NULL;

						pHKL->OnRadarDied(this);
					}
				}

	virtual	int		GetRadarColumnIndex		(const CString& sColumnName)	{ return -1; }
	virtual	BOOL	MoveRadarColumn			(int nFromIdxCol, int nToIdxCol){ return FALSE; }
	virtual BOOL	SetColumnTitle			(int nIdxCol, LPCTSTR szTitle)	{ return FALSE; }
	virtual void	SetColumnScreenSize		(int nIdxCol, int width)		{}
	virtual	BOOL	RemoveRadarColumn		(int nIdxCol)					{ return FALSE; }
	virtual BOOL	SetColumnAlign			(int nIdxCol, UINT align)		{ return FALSE; }	//DT_LEFT, DT_CENTER, DT_RIGHT
};

//-----------------------------------------------------------------------------

class TB_EXPORT CTBRadarFactory : public CObject
{
	DECLARE_DYNAMIC(CTBRadarFactory)
public:
	CTBRadarFactory() {}

	virtual ITBRadar* CreateInstance(HotKeyLink*, SqlTable*, SqlRecord*, HotKeyLinkObj::SelectionType);
	virtual ITBRadar* CreateInstance(CAbstractFormDoc*) ;
	virtual ITBRadar* CreateInstance(CAbstractFormDoc*, const CString&, BOOL = FALSE);

	virtual CString BuildWoormRadar(CAbstractFormDoc*) { return _T(""); }
};


TB_EXPORT CTBRadarFactory* AfxGetTBRadarFactory();
TB_EXPORT ITBRadar* AfxCreateTBRadar(HotKeyLink*, SqlTable*, SqlRecord*, HotKeyLinkObj::SelectionType);
TB_EXPORT ITBRadar* AfxCreateTBRadar(CAbstractFormDoc*);
TB_EXPORT ITBRadar* AfxCreateTBRadar(CAbstractFormDoc*, const CString&, BOOL = FALSE);

//-----------------------------------------------------------------------------
#include "endh.dex"

