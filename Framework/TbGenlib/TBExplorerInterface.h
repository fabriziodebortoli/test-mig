
#pragma once


#include <TbNameSolver\TBNameSpaces.h>
#include "parsobj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//--------------------------------------------------------------------------
class TB_EXPORT ITBExplorer : public CObject
{
	DECLARE_DYNAMIC(ITBExplorer)

public:
	enum TBExplorerType { EXPLORE, OPEN, SAVE };

public:
	virtual ~ITBExplorer() {}

public:
	virtual BOOL	Open					() = 0;													// apertura dell'oggetto
	virtual void	SetMultiOpen			() = 0;								// consente la multiselezione degli oggetti
	virtual int		GetSavePath				(CStringArray&) = 0;				// restituzione array di stringhe delle path in cui salvare

	virtual void	GetSelPathElement		(CString& StrSelectedPath) = 0;		// restituzione stringa del path dell'oggetto selezionato
	virtual void	GetSelPathElements		(CStringArray* aSelectedPaths) = 0; // restituzione array di stringhe dei path degli oggetti selezionati

	virtual void	GetSelNameSpace			(CTBNamespace& Ns) = 0;				// restituzione NameSpace dell'oggetto selezionato
	virtual void	GetSelArrayNameSpace	(CTBNamespaceArray& aNs) = 0;		// restituzione array di NameSpace degli oggetti selezionati
	virtual void	SetCanLink				() = 0;								// consente di impostare la futura possibilità di poter importate o linkare	(per Woorm)
};

//-----------------------------------------------------------------------------
class TB_EXPORT CBaseDocumentExplorerDlg : public CParsedDialog
{
	DECLARE_DYNAMIC(CBaseDocumentExplorerDlg)

public:	
	CString		m_FullNameSpace;
	BOOL		m_bFilterXmlDescrition;

	CBaseDocumentExplorerDlg(UINT idd) : CParsedDialog(idd, NULL), m_bFilterXmlDescrition (TRUE) {}
};

//--------------------------------------------------------------------------
class TB_EXPORT CTBExplorerFactory : public CObject
{
	DECLARE_DYNCREATE(CTBExplorerFactory)

public:
	CRuntimeClass*	m_rtcDocumentExplorer;

public:
	CTBExplorerFactory() : m_rtcDocumentExplorer(NULL) {}
	//deve avere gli stessi parametri del costruttore
	virtual ITBExplorer* CreateInstance(ITBExplorer::TBExplorerType aType, const CTBNamespace& aNameSpace, BOOL bIsNew = FALSE, BOOL bOnlyStdAndAllusr = FALSE, BOOL bIsUsr = FALSE, BOOL bSaveForAdmin = FALSE);

	CBaseDocumentExplorerDlg* CreateDocumentExplorerDlg() { return (CBaseDocumentExplorerDlg*)(m_rtcDocumentExplorer ? m_rtcDocumentExplorer->CreateObject() : NULL); }
};

TB_EXPORT CTBExplorerFactory* AfxGetTBExplorerFactory();

TB_EXPORT ITBExplorer* AfxCreateTBExplorer(ITBExplorer::TBExplorerType aType, const CTBNamespace& aNameSpace, BOOL bIsNew = FALSE, BOOL bOnlyStdAndAllusr = FALSE, BOOL bIsUsr = FALSE, BOOL bSaveForAdmin = FALSE);
//-----------------------------------------------------------------------------
#include "endh.dex"

