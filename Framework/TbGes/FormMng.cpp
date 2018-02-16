
#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\globals.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\DataObjDescription.h>
#include <TbGeneric\FunctionObjectsInfo.h>
#include <TbGeneric\ReportObjectsInfo.h>
#include <TbGeneric\FormNSChanges.h>

#include <TbParser\Parser.h>
#include <TbParser\XmlFunctionObjectsParser.h>
#include <TbParser\XmlReportObjectsParser.h>
#include <TbParser\FormNSChangesParser.h>

#include <TbGenlib\messages.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\Const.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\SettingsTableManager.h>
#include <TbGenlib\TbTreeCtrl.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include "extdoc.h"

#include "formmng.h"
#include "formmngdlg.h"

#include "XMLDocGenerator.h"

//................................. resources
#include "formmng.hjson"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


/*
	Sintassi per il file di form (form.tbf)

Relase 1
Title "Anagrafica Articoli"

Querys
Begin
End

Objects
Begin
	Body "namespaces be1" "BodyTitle1"
	Begin
		Column "nsc1" "ColTitle1" Type Monetario 
		Column "nsc2" "ColTitle2" Type Quantity Status 2
		Column "nsc5" "ColTitle5" Type Quantity Status 1 Size 10 Order 3
	End
	Body "namespaces be2", "BodyTitle2"
	Begin
		Column "nsc7" "ColTitle7" Type Monetario 
		Column "nsc9" "ColTitle9" Type Quantity Status 2 Size 20
		Column "nsc11" "ColTitle11" Type Quantity Status 2 Order 2
	End

End

Grammatica del .tbf
===================

<FormObjectArray>	::= Objects Begin {<BodyEditInfo>} End
<BodyEditInfo>		::= Body <Namespace> {<Title>} Begin <BodyColumn> End
<BodyEditColumn>	::= Column <Namespace> {<Title>} Type <DataType> {<Status>} {<Size>} {<Order>}
<Status>			::= Status {Hidden|Grayed}
<Size>				::= Size 10
<Order>				::= Order 1
<DataType>			::= Quantity|Money etc.... (vedi Woorm)
<Title>				::= <anystring>
<Namespace>			::= <a valid namespace>


*/

// Ritorna il nome del report predefinito
CString GetPredefinedRadar() { static CString strPredefined(_TB("Default")); return strPredefined; }
CString GetAliasRadar() { static CString strAlias(_TB("New")); return strAlias; }
#define COLUMN_IMAGE_OFFSET 2

//=============================================================================

/////////////////////////////////////////////////////////////////////////////
//							CReportManager
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------

CReportManager::CReportManager (BOOL bIsRadars/*=FALSE*/,BOOL bIsBarcode/*=FALSE*/ )
	: 
	m_bIsRadars		(bIsRadars),
	m_bIsBarcode	(bIsBarcode),
	m_bModUser		(FALSE),
	m_bModAllUsers	(FALSE)
{
}

//------------------------------------------------------------------------------
CReportManager::CReportManager (const CReportManager& source)
{
	*this = source;
}

//-----------------------------------------------------------------------------
CReportManager& CReportManager ::operator= (const CReportManager& rm)
{ 
	m_arAllReports		= rm.m_arAllReports; 
	m_arShowReports		= rm.m_arShowReports;
	m_arStandardReports = rm.m_arStandardReports;
	m_arAllUsersReports = rm.m_arAllUsersReports;
	m_arUserReports		= rm.m_arUserReports;
	m_bModUser			= rm.m_bModUser;
	m_bModAllUsers		= rm.m_bModAllUsers;
	m_NsAllUsrs			= rm.m_NsAllUsrs;
	m_NsUsr				= rm.m_NsUsr;
	m_NsStd				= rm.m_NsStd;
	m_NsCurrDefault		= rm.m_NsCurrDefault;
	m_bIsRadars			= rm.m_bIsRadars;	
	m_bIsBarcode		= rm.m_bIsBarcode;	

	m_NsSpecificReportForPrint		= rm.m_NsSpecificReportForPrint;	
	m_NsSpecificReportForEmail		= rm.m_NsSpecificReportForEmail;	

	return *this; 
}

//-----------------------------------------------------------------------------
BOOL CReportManager::Parse (const CTBNamespace& nsDoc, CString sFileName, CReportObjectsDescription& arReports, CTBNamespace& nsDefault, const CString& sUsr)
{
	CXMLReportObjectsParser aParser; 

	CLocalizableXMLDocument aXMLDocStd(nsDoc, AfxGetPathFinder());
	aXMLDocStd.EnableMsgMode(FALSE);
	if (ExistFile(sFileName))
	{
		if (!aXMLDocStd.LoadXMLFile(sFileName))
		{
			ASSERT(FALSE);
			TRACE("CReportManager::Parse: failed to load the file %s of the document %s.", sFileName, nsDoc.ToString()); 
			return FALSE;
		}
		aParser.Parse(&aXMLDocStd, &arReports, nsDoc, nsDefault);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CReportManager::Parse (const CTBNamespace& nsDoc, BOOL bAdmin /*FALSE*/ )
{
	m_NsAllUsrs.Clear ();
	m_NsCurrDefault.Clear ();
	m_NsUsr.Clear ();
	m_NsStd.Clear ();

	// TODO anche il parse per i reports dei ClientDoc
	m_arStandardReports.RemoveAll();
	m_arAllUsersReports.RemoveAll();
	m_arUserReports.RemoveAll();

	CString sFileName = _T("");

	if (m_bIsRadars)	
		sFileName = AfxGetPathFinder()->GetDocumentRadarsFile(nsDoc, CPathFinder::STANDARD);
	else
		if (m_bIsBarcode)
			sFileName = AfxGetPathFinder()->GetDocumentBarcodeFile(nsDoc, CPathFinder::STANDARD);
		else
			sFileName = AfxGetPathFinder()->GetDocumentReportsFile(nsDoc, CPathFinder::STANDARD);

	if (ExistFile(sFileName))
	{
		Parse(nsDoc, sFileName, m_arStandardReports, m_NsStd,  _T(""));
		SetFrom(m_arStandardReports, CBaseDescription::XML_STANDARD);
	}
	// carico i report dei client doc, ed imposto come NS di default per std l'ultimo caricato dai clientdoc se 
	// esistente, altrimenti rimane quello del docuement
	CObArray arClientDocs;
	AfxGetClientDocs(nsDoc, arClientDocs);
	
	for (int i=0; i <= arClientDocs.GetUpperBound(); i++)
	{
		CBaseDescription* pDescri = (CBaseDescription*) arClientDocs.GetAt(i);

		if (m_bIsRadars)	
			sFileName = AfxGetPathFinder()->GetDocumentRadarsFile(pDescri->GetNamespace(), CPathFinder::STANDARD);
		else
			if (m_bIsBarcode)
				sFileName = AfxGetPathFinder()->GetDocumentBarcodeFile(pDescri->GetNamespace(), CPathFinder::STANDARD);
			else
				sFileName = AfxGetPathFinder()->GetDocumentReportsFile(pDescri->GetNamespace(), CPathFinder::STANDARD);

		if (ExistFile(sFileName) && AfxIsActivated(pDescri->GetNamespace().GetApplicationName(), pDescri->GetNamespace().GetModuleName()))
		{
			CTBNamespace NsLocal; 
			Parse(nsDoc, sFileName, m_arStandardReports, NsLocal, _T(""));
			if (!NsLocal.ToString().IsEmpty())
				m_NsStd.SetNamespace(NsLocal);
			SetFrom(m_arStandardReports, CBaseDescription::XML_STANDARD);
		}
	}

	if (m_bIsRadars)	
		sFileName = AfxGetPathFinder()->GetDocumentRadarsFile(nsDoc, CPathFinder::ALL_USERS);
	else
		if (m_bIsBarcode)
			sFileName = AfxGetPathFinder()->GetDocumentBarcodeFile(nsDoc, CPathFinder::ALL_USERS);
		else
			sFileName = AfxGetPathFinder()->GetDocumentReportsFile(nsDoc, CPathFinder::ALL_USERS);

	if (ExistFile(sFileName))
	{
		Parse(nsDoc, sFileName, m_arAllUsersReports, m_NsAllUsrs, _T(""));
		SetFrom(m_arAllUsersReports, CBaseDescription::XML_ALLUSERS);
	}

	if (m_bIsRadars)	
		sFileName = AfxGetPathFinder()->GetDocumentRadarsFile(nsDoc, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName);
	else
		if (m_bIsBarcode)
			sFileName = AfxGetPathFinder()->GetDocumentBarcodeFile(nsDoc, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName);
		else
			sFileName = AfxGetPathFinder()->GetDocumentReportsFile(nsDoc, CPathFinder::USERS, AfxGetLoginInfos()->m_strUserName);

	if (ExistFile(sFileName))
	{
		Parse(nsDoc, sFileName, m_arUserReports, m_NsUsr, AfxGetLoginInfos()->m_strUserName);
		SetFrom(m_arUserReports, CBaseDescription::XML_USER);
	}

	if (bAdmin)
		AddDefaultReport();
	else
		MakeGeneralArrayReport();
    
	return TRUE;
}

// aggiunge, nel caso di amministratore, il report di default se appartenente a lista divera
//-----------------------------------------------------------------------------
BOOL CReportManager::AddDefaultReport()
{
	if (!m_NsUsr.ToString().IsEmpty())	
	{
		CDocumentReportDescription* pDefUsr = (CDocumentReportDescription*) m_arUserReports.GetReportInfo(m_NsUsr);
		if (pDefUsr)
			pDefUsr->SetDefault(TRUE);
		else
		{
			CDocumentReportDescription* pDefAllUsrs = (CDocumentReportDescription*) m_arAllUsersReports.GetReportInfo(m_NsUsr);
			if (pDefAllUsrs)
			{
				if (!pDefAllUsrs->IsDefault())
				{
					CDocumentReportDescription* pAddDef = new CDocumentReportDescription(*pDefAllUsrs);
					pAddDef->SetDefault(TRUE);
					pAddDef->m_XMLFrom = CDocumentReportDescription::XML_USER;
					m_arUserReports.AddReport(pAddDef);
				}
			}
			else
			{
				CDocumentReportDescription* pDefStd = (CDocumentReportDescription*) m_arStandardReports.GetReportInfo(m_NsUsr);
				if (pDefStd)
				{
					CDocumentReportDescription* pAddDef = new CDocumentReportDescription(*pDefStd);
					pAddDef->SetDefault(TRUE);
					pAddDef->m_XMLFrom = CDocumentReportDescription::XML_USER;
					m_arUserReports.AddReport(pAddDef);
				}
			}
		}
	}

	// gestione AllUsers
	if (!m_NsAllUsrs.ToString().IsEmpty())	
	{
		CDocumentReportDescription* pDefAllUsers = (CDocumentReportDescription*) m_arAllUsersReports.GetReportInfo(m_NsAllUsrs);
		if (pDefAllUsers)
			pDefAllUsers->SetDefault(TRUE);
		else
		{		
			CDocumentReportDescription* pDefStd = (CDocumentReportDescription*) m_arStandardReports.GetReportInfo(m_NsAllUsrs);
			if (pDefStd)
			{
				CDocumentReportDescription* pAddDef = new CDocumentReportDescription(*pDefStd);
				pAddDef->SetDefault(TRUE);
				pAddDef->m_XMLFrom = CDocumentReportDescription::XML_ALLUSERS;
				m_arAllUsersReports.AddReport(pAddDef);
			}
		}
	}

	// gestione Stdandard
	if (!m_NsStd.ToString().IsEmpty())	
	{
		CDocumentReportDescription* pDefStd = (CDocumentReportDescription*) m_arStandardReports.GetReportInfo(m_NsStd);
		if (pDefStd)
			pDefStd->SetDefault(TRUE);		
	}

	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL CReportManager::ParseUser (const CTBNamespace& nsDoc, const CString& sUsr)
{
	m_arUserReports.RemoveAll();
	m_NsUsr = nsDoc;
	CString sFileName = _T("");
	CTBNamespace ns;
	ns.SetNamespace(_T("document.testapplication.tadataentry.taordini.aree"));

	if (m_bIsRadars)
		sFileName = AfxGetPathFinder()->GetDocumentRadarsFile(nsDoc, CPathFinder::USERS, sUsr);
	else
		if (m_bIsBarcode)
			sFileName = AfxGetPathFinder()->GetDocumentBarcodeFile(nsDoc, CPathFinder::USERS, sUsr);
		else
			sFileName = AfxGetPathFinder()->GetDocumentReportsFile(nsDoc, CPathFinder::USERS, sUsr);

	if (ExistFile(sFileName))
	{
		Parse(nsDoc, sFileName, m_arUserReports, m_NsUsr, sUsr);
		SetFrom(m_arUserReports, CBaseDescription::XML_USER);
	}
	else
		m_NsUsr.SetNamespace(_T(""));

	AddDefaultReport();			
	return TRUE;
}

//-----------------------------------------------------------------------------
void CReportManager::SetFrom (CReportObjectsDescription& arReports, CBaseDescription::XMLFrom From)
{
	for (int i = 0; i <= arReports.GetReports().GetUpperBound(); i++)
	{
		CBaseDescription* pBaseDescription = (CBaseDescription*) arReports.GetReports().GetAt(i);	
		pBaseDescription->m_XMLFrom = From;
	}
}

//----------------------------------------------------------------------------
void CReportManager::MakeGeneralArrayReport()
{
	m_arAllReports.RemoveAll();
	CDocumentReportDescription* pNewDescription;

	for (int i = 0; i <= m_arUserReports.GetReports().GetUpperBound(); i++)
	{
		pNewDescription = new CDocumentReportDescription(*((CDocumentReportDescription*) m_arUserReports.GetReports().GetAt(i)));
		m_arAllReports.AddReport(pNewDescription);
	}

	for (int n = 0; n <= m_arAllUsersReports.GetReports().GetUpperBound(); n++)
	{
		pNewDescription = new CDocumentReportDescription(*((CDocumentReportDescription*) m_arAllUsersReports.GetReports().GetAt(n))); 
		m_arAllReports.AddReport(pNewDescription);
	}

	for (int s = 0; s <= m_arStandardReports.GetReports().GetUpperBound(); s++)
	{
		pNewDescription = new CDocumentReportDescription(*((CDocumentReportDescription*) m_arStandardReports.GetReports().GetAt(s))); 
		m_arAllReports.AddReport(pNewDescription);
	}
	// devo riportare i default, anche se esterni
	SetDetaultInGeneralArray();
	MakeShowReportArray();
}

//-----------------------------------------------------------------------
void CReportManager::SetDetaultInGeneralArray()
{
	// gestione usr
	if (!m_NsUsr.ToString().IsEmpty())	
	{
		CDocumentReportDescription* pDefUsr = (CDocumentReportDescription*) m_arUserReports.GetReportInfo(m_NsUsr);
		if (pDefUsr)
			pDefUsr->SetDefault(TRUE);
		else
		{
			CDocumentReportDescription* pDefAllUsrs = (CDocumentReportDescription*) m_arAllUsersReports.GetReportInfo(m_NsUsr);
			if (pDefAllUsrs)
			{
				if (!pDefAllUsrs->IsDefault())
				{
					CDocumentReportDescription* pAddDef = new CDocumentReportDescription(*pDefAllUsrs);
					pAddDef->SetDefault(TRUE);
					pAddDef->m_XMLFrom = CDocumentReportDescription::XML_USER;
					m_arAllReports.AddReport(pAddDef);
				}
			}
			else
			{
				CDocumentReportDescription* pDefStd = (CDocumentReportDescription*) m_arStandardReports.GetReportInfo(m_NsUsr);
				if (pDefStd)
				{
					CDocumentReportDescription* pAddDef = new CDocumentReportDescription(*pDefStd);
					pAddDef->SetDefault(TRUE);
					pAddDef->m_XMLFrom = CDocumentReportDescription::XML_USER;
					m_arAllReports.AddReport(pAddDef);
				}
			}
		}
	}

	// gestione AllUsers
	if (!m_NsAllUsrs.ToString().IsEmpty())	
	{
		CDocumentReportDescription* pDefAllUsers = (CDocumentReportDescription*) m_arAllUsersReports.GetReportInfo(m_NsAllUsrs);
		if (pDefAllUsers)
			pDefAllUsers->SetDefault(TRUE);
		else
		{		
			CDocumentReportDescription* pDefStd = (CDocumentReportDescription*) m_arStandardReports.GetReportInfo(m_NsAllUsrs);
			if (pDefStd)
			{
				CDocumentReportDescription* pAddDef = new CDocumentReportDescription(*pDefStd);
				pAddDef->SetDefault(TRUE);
				pAddDef->m_XMLFrom = CDocumentReportDescription::XML_ALLUSERS;
				m_arAllReports.AddReport(pAddDef);
			}
		}
	}

	// gestione Stdandard
	if (!m_NsStd.ToString().IsEmpty())	
	{
		CDocumentReportDescription* pDefStd = (CDocumentReportDescription*) m_arStandardReports.GetReportInfo(m_NsStd);
		if (pDefStd)
			pDefStd->SetDefault(TRUE);		
	}
}

//-----------------------------------------------------------------------
void CReportManager::MakeShowReportArray()
{
	m_arShowReports.RemoveAll();
	for (int i = 0; i <= m_arAllReports.GetReports().GetUpperBound(); i++)
	{
		CDocumentReportDescription* pReport = (CDocumentReportDescription*) m_arAllReports.GetReports().GetAt(i);
		if (ExistElementInShowArray(pReport) || pReport->m_XMLFrom == CDocumentReportDescription::XML_DELETED)
			continue;
		CleanShowArray(pReport);
		m_arShowReports.AddReport(new CDocumentReportDescription(*pReport));
	}
	// manca l'impostazione del default!!!!
	SetDefaultReportDescriptionInShowRep();
}

//-----------------------------------------------------------------------
BOOL CReportManager::ExistElementInShowArray(CDocumentReportDescription* pReportInfo) 
{
	CDocumentReportDescription* pShowReport = NULL;
	// caso esistenza ma nn deve essere inserito
	for (int i = 0; i <= m_arShowReports.GetReports().GetUpperBound(); i++)
	{
		pShowReport = (CDocumentReportDescription*) m_arShowReports.GetReports().GetAt(i);
		if (pShowReport->GetNamespace() == pReportInfo->GetNamespace())
			switch (pReportInfo->m_XMLFrom)
			{
				case CDocumentReportDescription::XML_STANDARD: // controlla se esite allusr e usr
					if (
							pShowReport->m_XMLFrom == CDocumentReportDescription::XML_ALLUSERS	||
							pShowReport->m_XMLFrom == CDocumentReportDescription::XML_USER		||
							pShowReport->m_XMLFrom == CDocumentReportDescription::XML_MODIFIED
						)
						return TRUE;
				case CDocumentReportDescription::XML_ALLUSERS:
					if (
						pShowReport->m_XMLFrom == CDocumentReportDescription::XML_USER ||
						pShowReport->m_XMLFrom == CDocumentReportDescription::XML_MODIFIED
						)
						return TRUE;				
				// case CDocumentReportDescription::XML_MODIFIED:   ??????????
			}	
	}

	return FALSE;
}

//-----------------------------------------------------------------------
void CReportManager::CleanShowArray(CDocumentReportDescription* pReportInfo)
{
	for (int i = 0; i <= m_arShowReports.GetReports().GetUpperBound(); i++)
	{
		CDocumentReportDescription* pReport = (CDocumentReportDescription*) m_arShowReports.GetReports().GetAt(i);
		ASSERT(pReport);
		if (pReport->GetNamespace() == pReportInfo->GetNamespace())
		{
			m_arShowReports.RemoveReport(pReport);
			return;
		}	
	}	
}
//----------------------------------------------------------------------------
void CReportManager::SetDefaultReportDescriptionInShowRep()
{	
	//if (m_NsUsr.ToString().IsEmpty())	// non esiste report di default definito qui	
	BOOL bExistDefUsr		= (!m_NsUsr.ToString().IsEmpty());
	BOOL bExistDefAllUsrs	= (!m_NsAllUsrs.ToString().IsEmpty());
	BOOL bExistDefStd		= (!m_NsStd.ToString().IsEmpty());

	if (bExistDefUsr)
	{
		CDocumentReportDescription* pReportUsr = m_arShowReports.GetReportInfo(m_NsUsr);
		if (pReportUsr)
			pReportUsr->SetDefault(TRUE);

		CDocumentReportDescription* pReportAllUsrs = m_arShowReports.GetReportInfo(m_NsAllUsrs);
		if (m_NsUsr != m_NsAllUsrs)
			if (pReportAllUsrs)
				pReportAllUsrs->SetDefault(FALSE);

		CDocumentReportDescription* pReportStd = m_arShowReports.GetReportInfo(m_NsStd);
		if (m_NsUsr != m_NsStd)
			if (pReportStd)
				pReportStd->SetDefault(FALSE);		
	}
	else 
	{
		if (bExistDefAllUsrs)
		{
			CDocumentReportDescription* pReportAllUsrs = m_arShowReports.GetReportInfo(m_NsAllUsrs);
			if (pReportAllUsrs)
				pReportAllUsrs->SetDefault(TRUE);        

			CDocumentReportDescription* pReportStd = m_arShowReports.GetReportInfo(m_NsStd);
			if (pReportStd)
				if (m_NsStd != m_NsAllUsrs)
					pReportStd->SetDefault(FALSE);
		}
		else
			if (bExistDefStd)
			{
				CDocumentReportDescription* pReportStd = m_arShowReports.GetReportInfo(m_NsStd);
				if (pReportStd)
					pReportStd->SetDefault(TRUE);
			}
	}
}

//----------------------------------------------------------------------------
CDocumentReportDescription* CReportManager::GetDefaultReportDescriptionInShowRep()
{
	return m_arShowReports.GetDefault();
}

//----------------------------------------------------------------------------
CDocumentReportDescription* CReportManager::GetDefaultReportDescription()
{
	if (m_NsCurrDefault.ToString().IsEmpty())
		return m_arShowReports.GetDefault();

	return m_arShowReports.GetReportInfo(m_NsCurrDefault);
}

//----------------------------------------------------------------------------
CDocumentReportDescription* CReportManager::GetReportDescription(const CString& sReport)
{
	for (int i = 0; i <= m_arShowReports.GetReports().GetUpperBound(); i++)
	{
		CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) (m_arShowReports.GetReports().GetAt(i));
		ASSERT(pReportInfo);

		if (pReportInfo->GetNamespace().ToString().CompareNoCase(sReport) == 0)
			return pReportInfo;
	}
	return NULL;
}

//----------------------------------------------------------------------------
CDocumentReportDescription* CReportManager::GetReportDescription (int nIdx)
{
	if (nIdx < 0 || nIdx > m_arShowReports.GetReports().GetUpperBound())
		return NULL;

	CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) (m_arShowReports.GetReports().GetAt(nIdx));
	ASSERT(pReportInfo);

	return pReportInfo;
}

//----------------------------------------------------------------------------
void CReportManager::SetCurrentDefaultReport(const CString& sReport)
{
	CTBNamespace nsCurrent(CTBNamespace::REPORT, sReport);
	CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) m_arShowReports.GetReportInfo(nsCurrent);

	if (pReportInfo)
		m_NsCurrDefault = nsCurrent;
}

//----------------------------------------------------------------------------
CDocumentReportDescription* CReportManager::GetCurrentDefaultReport()
{
	if (!m_NsCurrDefault.IsValid())
		return GetDefaultReportDescription();
	
	return m_arShowReports.GetReportInfo(m_NsCurrDefault);
}

//----------------------------------------------------------------------------
void CReportManager::RedistribuitionReportArray()
{
	m_arAllUsersReports.RemoveAll();
	m_arUserReports.RemoveAll();
	m_arStandardReports.RemoveAll();

	for (int i = 0; i <= m_arAllReports.GetReports().GetUpperBound(); i++)
	{
		CDocumentReportDescription* pReportInfoNew = new CDocumentReportDescription();
		CDocumentReportDescription* pReportInfoOld = (CDocumentReportDescription*) m_arAllReports.GetReports().GetAt(i);

		pReportInfoNew->SetNamespace(pReportInfoOld->GetNamespace());		
			
		CString strPath = _T("");
		
		strPath = AfxGetPathFinder()->GetFileNameFromNamespace(pReportInfoNew->GetNamespace(), AfxGetLoginInfos()->m_strUserName);
		if (AfxGetPathFinder()->IsStandardPath(strPath))
			m_arStandardReports.AddReport(pReportInfoNew);
		else 
		{
			CString strUser = AfxGetPathFinder()->GetUserNameFromPath(strPath);
			if (AfxGetPathFinder()->GetPosTypeFromPath(strPath) == CPathFinder::ALL_USERS)
				m_arAllUsersReports.AddReport(pReportInfoNew);
			else
				m_arUserReports.AddReport(pReportInfoNew);
		}
	}
}

//----------------------------------------------------------------------------
void CReportManager::CleanFromStd()
{
	for (int i = 0; i <= m_arStandardReports.GetReports().GetUpperBound(); i++)
	{
		CDocumentReportDescription* pReport = (CDocumentReportDescription*) m_arStandardReports.GetReports().GetAt(i);
		RemoveFromGeneralReportArray(pReport);
	}	
	RedistribuitionReportArray();
}

//----------------------------------------------------------------------------
void CReportManager::CleanFromAllUsrs()
{
	for (int i = 0; i <= m_arAllUsersReports.GetReports().GetUpperBound(); i++)
	{
		CDocumentReportDescription* pReport = (CDocumentReportDescription*) m_arAllUsersReports.GetReports().GetAt(i);
		RemoveFromGeneralReportArray(pReport);		
	}
	RedistribuitionReportArray();
}

//----------------------------------------------------------------------------
void CReportManager::RemoveFromGeneralReportArray(CDocumentReportDescription* pReport)
{
	m_arAllReports.RemoveReport(pReport);
}

/////////////////////////////////////////////////////////////////////////////
//							CFormPropertiesDlg
///////////////////////////////////////////////////////////////////////////////
//
class CFormPropertiesDlg : public CLocalizablePropertyPage
{
	DECLARE_DYNAMIC(CFormPropertiesDlg)
	friend CFormSheet;

protected:
	CFormSheet*		m_pSheet;
	FormProperties&	m_FormProperties;

public:
	// Dialog Data
	CButton		m_ctrlFormMngPropertiesFindable;
	CButton		m_ctrlFormMngPropertiesDescending;
	CButton		m_ctrlFormMngPropertiesFindSlave;

public:
	CFormPropertiesDlg(CFormSheet* pSheet, FormProperties& aFormProperties);

protected:
	void SetModified	(BOOL bModified = TRUE);
	void DoSave			(BOOL bFromApply);

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CFormPropertiesDlg)
	virtual BOOL OnInitDialog	();
	virtual void OnOK			();
	virtual BOOL OnApply		();

	afx_msg void OnChangePropertiesFindable		();
	afx_msg void OnChangePropertiesDescending	();
	afx_msg void OnChangePropertiesFindSlave	();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

IMPLEMENT_DYNAMIC(CFormPropertiesDlg, CLocalizablePropertyPage)
/////////////////////////////////////////////////////////////////////////////
//							CFormBodyDlg
///////////////////////////////////////////////////////////////////////////////
//
class CFormBodyDlg : public CLocalizablePropertyPage
{
	DECLARE_DYNAMIC(CFormBodyDlg)
	friend CFormSheet;

protected:
	CFormSheet*		m_pSheet;
	FormBodyEdits&	m_FormBodyEdits;
	HTREEITEM		m_hCurrentItem;
	LPARAM			m_lParamCurrentItem;
	BOOL			b_bSuspendTree;
public:
	// Dialog Data
	CTBTreeCtrl	m_ctrlTree;

	CBCGPEdit	m_ctrlFormMngBodyName;
	CBCGPEdit	m_ctrlFormMngColumnName;
	CButton		m_ctrlFormMngVisible;
	CButton		m_ctrlFormMngEditable;
	CBCGPEdit	m_ctrlFormMngOrder;
	CSpinButtonCtrl m_ctrlFormMngSpinOrder;
	CBCGPEdit	m_ctrlFormMngWidth;

	// per visualizzare i valori di default dello stato
	CButton		m_ctrlFormMngVisible2;
	CButton		m_ctrlFormMngEditable2;
	CStatic		m_ctrlFormMngDefaultColumnName;
	CButton		m_ctrlFormMngResetDefault;
	CImageList	m_imaSmall;
	CStatic		m_ctrlFormMngDefaultWidth;
	CStatic		m_ctrlFormMngDefaultOrder;

// Construction
public:
	CFormBodyDlg(CFormSheet* pSheet, FormBodyEdits& aFormBodyEdits);   // standard constructor

protected:
	void FillAllTree();
	HTREEITEM  FillBodyTree(BodyEditInfo*);
	HTREEITEM InsertSibling(HTREEITEM htmParent, BodyEditColumn* pBodyEditColumn, HTREEITEM htmAfter = TVI_LAST);

	void SetModifedColumn	();
	void SetModifedBody		();
	void DoSave				(BOOL bFromApply);

	int	GetImage (BodyEditColumn* pBodyEditColumn);
// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CFormBodyDlg)
	virtual BOOL OnInitDialog		();
	virtual void OnOK				();
	virtual BOOL OnApply			();

	afx_msg void OnChangeBodyName	();
	afx_msg void OnChangeColumnName ();

	afx_msg void OnSelchangedTree	(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnSelchangingTree	(NMHDR* pNMHDR, LRESULT* pResult);

	afx_msg void OnKillfocusBodyName	();
	afx_msg void OnKillfocusColumnName	();
	afx_msg void OnStatusChanged		();
	afx_msg void OnClickResetDefault	();
	afx_msg void OnDeltaposSpinOrder(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnKillfocusColumnWidth	();

	afx_msg void	OnLButtonDblClk(UINT nFlags, CPoint point);

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

IMPLEMENT_DYNAMIC(CFormBodyDlg, CLocalizablePropertyPage)

/////////////////////////////////////////////////////////////////////////////
//							BodyEditColumn
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(BodyEditColumn, CObject)

//------------------------------------------------------------------------------
BodyEditColumn::BodyEditColumn()
	:
	m_wDefaultStatus		(0),
	m_wStatus				(0),
	m_nColScreenWidth		(0),
	m_nDefaultColScreenWidth(0),
	m_bModified				(FALSE),
	m_nColPos				(-1),
	m_nDefaultColPos		(-1),
	m_bStatusChanged		(FALSE)
{
	m_UICulture = AfxGetCulture();
}

//------------------------------------------------------------------------------
BodyEditColumn::BodyEditColumn
	(
		CTBNamespace&	aNamespace,

		LPCTSTR		pszColumnTitle,
		LPCTSTR		pszDefaultColumnTitle,
		DataType	ColumnDataType,
		WORD		wDefaultStatus /*= 0*/,
		WORD		wStatus /*= 0*/,
		int			nColPos /*= -1*/,
		int			nDefaultColPos /*= -1*/,
		int			nColScreenWidth /*= 0*/,
		int			nDefaultColScreenWidth /*= 0*/
	)
	:
	m_strColumnTitle		(pszColumnTitle),
	m_strDefaultColumnTitle	(pszDefaultColumnTitle),
	m_ColumnDataType		(ColumnDataType),
	m_wDefaultStatus		(wDefaultStatus),
	m_wStatus				(wStatus),
	m_nColPos				(nColPos),
	m_nDefaultColPos		(nDefaultColPos),
	m_nColScreenWidth		(nColScreenWidth),
	m_nDefaultColScreenWidth(nDefaultColScreenWidth),

	m_bModified				(FALSE),
	m_bStatusChanged		(FALSE)
{
	m_Namespace.SetNamespace(aNamespace);
	m_UICulture = AfxGetCulture();
}

//------------------------------------------------------------------------------
BodyEditColumn::BodyEditColumn (const BodyEditColumn& source)
{
	*this = source;
}

//------------------------------------------------------------------------------
BodyEditColumn& BodyEditColumn::operator=(const BodyEditColumn& source)
{
	m_Namespace				= source.m_Namespace;
	m_strColumnTitle		= source.m_strColumnTitle;
	m_strDefaultColumnTitle	= source.m_strDefaultColumnTitle;
	m_ColumnDataType		= source.m_ColumnDataType;
	m_wDefaultStatus		= source.m_wDefaultStatus;
	m_wStatus				= source.m_wStatus;
	m_nColScreenWidth		= source.m_nColScreenWidth;
	m_nDefaultColScreenWidth= source.m_nDefaultColScreenWidth;
	m_nColPos				= source.m_nColPos;
	m_nDefaultColPos		= source.m_nDefaultColPos;
	m_bModified				= source.m_bModified;
	m_UICulture				= source.m_UICulture; 
	m_bStatusChanged		= source.m_bStatusChanged;
	return *this;
}


/////////////////////////////////////////////////////////////////////////////
//							BodyEditInfo
///////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(BodyEditInfo, Array);

//------------------------------------------------------------------------------
BodyEditInfo::BodyEditInfo ()
	:
	m_bModified	(FALSE),
	m_bValid	(FALSE),		// validato solo dal bodyedit istanziato
	m_pRowFormViewClass (NULL)
{
	GetInfoOSL()->SetType(OSLType_BodyEdit);
}


//------------------------------------------------------------------------------
BodyEditInfo::BodyEditInfo (const CTBNamespace& ns, LPCTSTR pszTitle /*= NULL */)
	:
	m_strBodyTitle	(pszTitle),
	m_bModified		(FALSE),
	m_bValid		(FALSE),
	m_pRowFormViewClass (NULL)
{
	GetInfoOSL()->SetType(OSLType_BodyEdit);
	GetInfoOSL()->m_Namespace = ns;
}


//------------------------------------------------------------------------------
BodyEditInfo::BodyEditInfo (const BodyEditInfo& source)
{
	*this = source;
}

//------------------------------------------------------------------------------
BodyEditInfo& BodyEditInfo::operator= (const BodyEditInfo& source)
{
	if (this == &source) return *this;

	*GetInfoOSL()	= *(const_cast<BodyEditInfo&>(source).GetInfoOSL());
	m_strBodyTitle	= source.m_strBodyTitle;
	m_bModified		= source.m_bModified;
	m_bValid		= source.m_bValid;
	m_pRowFormViewClass = source.m_pRowFormViewClass;

	// svuoto nell'eventualita` che ci sia gia` qualche cosa
	RemoveAll();

	//copia tutti gli elementi
	for (int j = 0; j <= source.GetUpperBound(); j++)
	{
		BodyEditColumn* pBodyEditColumn = source.GetAt(j);
		ASSERT(pBodyEditColumn);

		if (pBodyEditColumn) 
			Add(new BodyEditColumn(*pBodyEditColumn));
	}

	return *this;
}

//------------------------------------------------------------------------------
BodyEditColumn* BodyEditInfo::GetColumnObject(const CTBNamespace& ns)const
{
	for (int j = 0; j <= GetUpperBound(); j++)
	{
		BodyEditColumn* pBodyEditColumn = GetAt(j);
		ASSERT(pBodyEditColumn);
		if (pBodyEditColumn->m_Namespace == ns)
			return pBodyEditColumn;
		ASSERT (ns.ToString().CompareNoCase(pBodyEditColumn->m_Namespace.ToString()));
	}
	return NULL;
}

//------------------------------------------------------------------------------
BodyEditColumn* BodyEditInfo::GetColumnObject(const CString& sNs)const
{
	for (int j = 0; j <= GetUpperBound(); j++)
	{
		BodyEditColumn* pBodyEditColumn = GetAt(j);
		ASSERT(pBodyEditColumn);
		if (sNs == pBodyEditColumn->m_Namespace.ToString())
			return pBodyEditColumn;
		ASSERT (sNs.CompareNoCase(pBodyEditColumn->m_Namespace.ToString()));
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void BodyEditInfo::FixLineFeeds()
{
	//aggiusto i caratteri di a capo messi dall'utente (\n) mettendo il carattere
	//effettivo di a capo
	for (int j = 0; j <= GetUpperBound(); j++)
	{
		BodyEditColumn* pBodyEditColumn = GetAt(j);
		ASSERT(pBodyEditColumn);
		pBodyEditColumn->m_strColumnTitle.Replace(_T("\\n"), _T("\n"));
	}
}

//-----------------------------------------------------------------------------
BOOL BodyEditInfo::IsDefaultBody() const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		BodyEditColumn* pBodyEditColumn = GetAt(i);
		ASSERT(pBodyEditColumn);
		if 
			(
				pBodyEditColumn->m_wDefaultStatus != pBodyEditColumn->m_wStatus ||
				pBodyEditColumn->m_strDefaultColumnTitle != pBodyEditColumn->m_strColumnTitle ||
				pBodyEditColumn->m_nDefaultColScreenWidth != pBodyEditColumn->m_nColScreenWidth ||
				pBodyEditColumn->m_nDefaultColPos != pBodyEditColumn->m_nColPos
			)
			return FALSE;
	}
	// bodyedit could have changed main attributes
	return !m_bModified;
}

//-----------------------------------------------------------------------------

#define RESET_STATUS(a, b)\
		if (a != b)\
		{\
			a = b;\
			pBodyEditColumn->m_bModified = TRUE;\
			m_bModified = TRUE;\
		}

void BodyEditInfo::ResetToDefault()
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		BodyEditColumn* pBodyEditColumn = GetAt(i);
		ASSERT(pBodyEditColumn);
		
		RESET_STATUS (pBodyEditColumn->m_wStatus, pBodyEditColumn->m_wDefaultStatus)
		RESET_STATUS (pBodyEditColumn->m_strColumnTitle, pBodyEditColumn->m_strDefaultColumnTitle)
		RESET_STATUS (pBodyEditColumn->m_nColScreenWidth, pBodyEditColumn->m_nDefaultColScreenWidth)
		RESET_STATUS (pBodyEditColumn->m_nColPos, pBodyEditColumn->m_nDefaultColPos)
	}
}

//------------------------------------------------------------------------------
BOOL BodyEditInfo::LessThen( CObject* pO1, CObject* pO2) const
{
	return ((BodyEditColumn*)pO1)->m_nColPos < ((BodyEditColumn*)pO2)->m_nColPos;
}

//------------------------------------------------------------------------------
int	BodyEditInfo::Compare(CObject* pO1, CObject* pO2) const
{
	return ((BodyEditColumn*)pO1)->m_nColPos < ((BodyEditColumn*)pO2)->m_nColPos ?
			-1 :
			(((BodyEditColumn*)pO1)->m_nColPos > ((BodyEditColumn*)pO2)->m_nColPos ?
			1 :
			0);
}

/////////////////////////////////////////////////////////////////////////////
//							FormObjects
///////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
FormBodyEdits::FormBodyEdits(const FormBodyEdits& source)
{
	*this = source;
}

// modificare quando gli oggetti saranno di tipo diverso
//------------------------------------------------------------------------------
FormBodyEdits& FormBodyEdits::operator= (const FormBodyEdits& source)
{
	// svuoto nell'eventualita` che ci sia gia` qualche cosa
	RemoveAll();

	//copia tutti gli elementi 
	for (int j = 0; j <= source.GetUpperBound(); j++)
	{
		BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)source.GetAt(j);
		ASSERT(pBodyEditInfo);

		if (pBodyEditInfo) 
			Add(new BodyEditInfo(*pBodyEditInfo));
	}
	
	return *this;
}

//-----------------------------------------------------------------------------
void FormBodyEdits::FixLineFeeds()
{
	for (int i = 0; i < GetCount(); i++)
	{
		BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)GetAt(i);
		ASSERT(pBodyEditInfo);

		pBodyEditInfo->FixLineFeeds();
	}
}

/////////////////////////////////////////////////////////////////////////////
//							FormTiles
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(TileInfo, CObject);

//------------------------------------------------------------------------------
FormTiles::FormTiles(const FormTiles& source)
{
	*this = source;
}

// modificare quando gli oggetti saranno di tipo diverso
//------------------------------------------------------------------------------
FormTiles& FormTiles::operator= (const FormTiles& source)
{
	// svuoto nell'eventualita` che ci sia gia` qualche cosa
	RemoveAll();

	//copia tutti gli elementi 
	for (int j = 0; j <= source.GetUpperBound(); j++)
	{
		TileInfo* pTileInfo = (TileInfo*)source.GetAt(j);
		ASSERT(pTileInfo);

		if (pTileInfo)
			Add(new TileInfo(*pTileInfo));
	}

	return *this;
}

/////////////////////////////////////////////////////////////////////////////
//							FormProperties
///////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
FormProperties::FormProperties ()
	:
	m_OrderByType	(BROWSER_QUERY),
	m_bDescending	(FALSE),
	m_bFindSlave	(TRUE)
{
}

//------------------------------------------------------------------------------
FormProperties::FormProperties (const FormProperties& source)
{
	*this = source;
}

//------------------------------------------------------------------------------
FormProperties& FormProperties::operator= (const FormProperties& source)
{
	m_OrderByType	= source.m_OrderByType;
	m_bDescending	= source.m_bDescending;
	m_bFindSlave	= source.m_bFindSlave;

	return *this;
}

///////////////////////////////////////////////////////////////////////////////
//								CFormManager
///////////////////////////////////////////////////////////////////////////////

#define FORM_RELEASE	1
#define MODIFIED_COLLAPSED 2
#define MODIFIED_PINUNPIN 4
//-----------------------------------------------------------------------------
CFormManager::CFormManager(CAbstractFormDoc* pDocument)
	:
	m_nRelease				(FORM_RELEASE),
	m_nFormRelease			(FORM_RELEASE),
	m_bParseError			(FALSE),
	m_bTBFModified			(FALSE),
	m_Radars				(TRUE),
	m_Barcode				(FALSE,TRUE),
	m_pFormNSChangeArray	(NULL),
	m_bInternalModified		(FALSE),
	m_nNsRelease			(1),
	m_bUpdateTBFFiles		(TRUE),
	m_nDialogsModified		(FALSE),
	m_bDialogsSaveStateEnabled(FALSE)
{
	ASSERT_VALID(pDocument);
	m_pDocument = pDocument;
	m_Ns = pDocument->GetNamespace();
	// inserisce il woorm-radar predefinito
	//AddRadarObject(GetPredefinedRadar(), TRUE);
}


//------------------------------------------------------------------------------
CFormManager::CFormManager(const CFormManager& source)
	:
	m_pFormNSChangeArray	(NULL),
	m_bInternalModified		(FALSE),
	m_nNsRelease			(1),
	m_bUpdateTBFFiles		(TRUE),
	m_nDialogsModified		(FALSE),
	m_bDialogsSaveStateEnabled(FALSE)
{
	*this = source;
}

//-----------------------------------------------------------------------------
CFormManager::~CFormManager()
{
	SAFE_DELETE(m_pFormNSChangeArray);
}

//-----------------------------------------------------------------------------
BOOL CFormManager::ActivableForm ()
{
	return !m_pDocument->GetFormName().IsEmpty();
}

//------------------------------------------------------------------------------
BOOL CFormManager::HasPinUnPinModified() const
{
	return (m_nDialogsModified & MODIFIED_PINUNPIN) == MODIFIED_PINUNPIN;
}


//------------------------------------------------------------------------------
BOOL CFormManager::HasDialogCustomized(CTBNamespace aNs, BOOL& bPinned, BOOL& bCollapsed)
{
	TileInfo* pTI = GetTileInfo(aNs);
	if (pTI)
	{
		bPinned = pTI->m_bIsPinned;
		bCollapsed = pTI->m_bIsCollapsed;
		return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------
void CFormManager::SetDialogsPinState(CTBNamespace aNs, BOOL bPinned)
{
	if (!m_bDialogsSaveStateEnabled)
		return;

	TileInfo* pTI = GetTileInfo(aNs);
	if (pTI)
		pTI->m_bIsPinned = bPinned;
	else
		AddTileInfo(new TileInfo(aNs.ToString(), bPinned));

	m_nDialogsModified |= MODIFIED_PINUNPIN;
}

//------------------------------------------------------------------------------
void CFormManager::SetDialogsCollapsedState(CTBNamespace aNs, BOOL bCollapsed)
{
	if (!m_bDialogsSaveStateEnabled)
		return;

	TileInfo* pTI = GetTileInfo(aNs);
	if (pTI)
		pTI->m_bIsCollapsed = bCollapsed;
	else
		AddTileInfo(new TileInfo(aNs.ToString(),FALSE, bCollapsed));

	m_nDialogsModified |= MODIFIED_COLLAPSED;
}

//------------------------------------------------------------------------------
void CFormManager::EnableDialogStateSave(BOOL bEnable)
{
	m_bDialogsSaveStateEnabled = bEnable;
}

//------------------------------------------------------------------------------
BOOL CFormManager::IsTBFInAutoSave() const
{ 
	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szShowAdminCustomSaveDialog, DataBool(FALSE));
	BOOL bAdminAsk = pSetting ? *((DataBool*) pSetting) : FALSE;

	// utente oppure parametro a false oppure e' cambiato il solo collapse
	return !AfxGetLoginInfos()->m_bAdmin || !bAdminAsk || (!m_bTBFModified && !HasPinUnPinModified() && m_nDialogsModified);
}

//------------------------------------------------------------------------------
CFormManager& CFormManager::operator= (const CFormManager& source)
{
	m_pDocument			= source.m_pDocument;
	m_nRelease			= source.m_nRelease;
	m_nFormRelease		= source.m_nFormRelease;
	m_BodyEditInfos			= source.m_BodyEditInfos;
	m_Properties		= source.m_Properties;
	m_bTBFModified		= source.m_bTBFModified;
	m_Reports			= source.m_Reports;
	m_Radars			= source.m_Radars;
	m_Barcode			= source.m_Barcode;
	m_Ns				= source.m_Ns;
	m_nDialogsModified = source.m_nDialogsModified;
	m_bDialogsSaveStateEnabled = source.m_bDialogsSaveStateEnabled;
	//TODO: mancano dei field: sarà voluto ?
	return *this;
}

//-----------------------------------------------------------------------------
void CFormManager::CopyTBFDataTo (CFormManager* pTo)
{
	 pTo->m_nRelease		=	m_nRelease;		
	 pTo->m_nFormRelease	=	m_nFormRelease;	
	 pTo->m_BodyEditInfos	=	m_BodyEditInfos;		
	 pTo->m_Properties		=	m_Properties;	
	 pTo->m_bTBFModified	=	m_bTBFModified;  
	 pTo->m_nDialogsModified = m_nDialogsModified;
	 pTo->m_bDialogsSaveStateEnabled = m_bDialogsSaveStateEnabled;
}

//-----------------------------------------------------------------------------
void CFormManager::CopyRadarsDataTo	(CFormManager* pTo)
{
	pTo->m_Radars = m_Radars;
}

//-----------------------------------------------------------------------------
void CFormManager::CopyReportsDataTo (CFormManager* pTo)
{
	pTo->m_Reports = m_Reports;
}

//-----------------------------------------------------------------------------
void CFormManager::CopyBarcodeDataTo (CFormManager* pTo)
{
	pTo->m_Barcode = m_Barcode;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::SaveModified()
{
	if (!m_bParseError && ActivableForm())
	{
		//In unattended mode se il documento e' stato modificato non devo permetterne la chiusura
		if (AfxIsInUnattendedMode() && !IsTBFInAutoSave())
		{
			BOOL bXMLRadarModified, bXMLReportModified, bTBFModified, bXMLBarcodeModified;
			return !IsModified(bXMLRadarModified, bXMLReportModified, bTBFModified, bXMLBarcodeModified);
		}
		else
			//non devo tornare il valore per non impedire la chiusura del documento in interattivo
			SaveSheetModified(_T(""), NULL, TRUE);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::IsModified (BOOL& bXMLRadarModified, BOOL& bXMLReportModified, BOOL& bTBFModified,BOOL& bXMLBarcodeModified)
{
	bXMLRadarModified = m_Radars.IsAllUsersModified() || m_Radars.IsUserModified();
	bXMLReportModified = m_Reports.IsAllUsersModified() || m_Reports.IsUserModified();
	bXMLBarcodeModified = m_Barcode.IsAllUsersModified() || m_Barcode.IsUserModified();

	bTBFModified = m_bTBFModified || m_nDialogsModified;

	return bXMLRadarModified || bXMLReportModified || bTBFModified || bXMLBarcodeModified;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::SaveSheetModified(const CString& sUserForSave, CFormManager* pFormMngToUpdate /*NULL*/, BOOL bFromDoc /*FALSE*/)
{
	if (!ActivableForm())
		return TRUE;

	BOOL bXMLRadarSaved	 = TRUE;
	BOOL bXMLReportSaved = TRUE;
	BOOL bXMLBarcodeSaved = TRUE;
	BOOL bTBFSaved		 = TRUE;

	BOOL bXMLRadarModified, bXMLReportModified, bXMLBarcodeModified, bTBFModified;
	if (!IsModified(bXMLRadarModified, bXMLReportModified, bTBFModified, bXMLBarcodeModified))
		return TRUE;

	// se è modificato qualcosa fa la richiesta
	if (!sUserForSave.IsEmpty() && (bXMLReportModified || bXMLRadarModified || bXMLBarcodeModified))
	{
		CString strMsgRep		= _TB("Are you sure to save changes for document report?");
		CString strMsg			= _TB("Are you sure to save changes for document radar?");
		CString strMsgBarcode	= _TB("Are you sure to save changes for Barcode report?");

		if (bXMLReportModified)
			if (AfxMessageBox(strMsgRep, MB_YESNO) == IDYES)
			{
				UnparseReportXML(sUserForSave);
				if (pFormMngToUpdate)
				{
					if (m_pDocument)
						pFormMngToUpdate->m_Reports.Parse(m_pDocument->GetNamespace());
					else
						CopyReportsDataTo(pFormMngToUpdate);

					pFormMngToUpdate->m_Reports.MakeShowReportArray();
				}
			}
			else
				bXMLReportSaved = FALSE;
				
		if (bXMLRadarModified)
			if (AfxMessageBox(strMsg, MB_YESNO) == IDYES)
			{
				UnparseRadarXML(sUserForSave);
				if (pFormMngToUpdate)
				{
					if (m_pDocument)
						pFormMngToUpdate->m_Radars.Parse(m_pDocument->GetNamespace());
					else
						CopyRadarsDataTo(pFormMngToUpdate);

					pFormMngToUpdate->m_Radars.MakeShowReportArray();
				}
			}	
			else
				bXMLRadarSaved = FALSE;

		if (bXMLBarcodeModified)
			if (AfxMessageBox(strMsgBarcode, MB_YESNO) == IDYES)
			{
				UnparseBarcodeXML(sUserForSave);
				if (pFormMngToUpdate)
				{
					if (m_pDocument)
						pFormMngToUpdate->m_Barcode.Parse(m_pDocument->GetNamespace());
					else
						CopyBarcodeDataTo(pFormMngToUpdate);

					pFormMngToUpdate->m_Barcode.MakeShowReportArray();
				}
			}	
			else
				bXMLBarcodeSaved = FALSE;
	}

	if (bTBFModified)
	{
		if (bFromDoc)
		{
			if	(
					IsTBFInAutoSave() ||
					((m_bTBFModified || HasPinUnPinModified() ) && AfxMessageBox(_TB("Do you want to save the changes to document interface (grid and tiles)?"), MB_YESNO) == IDYES)
				)
			{
				m_bTBFModified = FALSE;
				if (!UnparseTBF())
					bTBFSaved = FALSE;	
			}
			else
				bTBFSaved = FALSE;	
		}
		else
			AfxMessageBox(_TB("Warning, to document interface (grid and tiles) can be saved on exit of document!"));

		if (pFormMngToUpdate)
		{
			//copio di dati di colonna in quelli di dcumento
			CopyTBFDataTo (pFormMngToUpdate);		
			
			//aggiusto i caratteri di a capo messi dall'utente (\n) mettendo il carattere
			//effettivo di a capo (an. #13260)
			pFormMngToUpdate->FixLineFeeds();
		}
	}

	return (bTBFSaved && bXMLReportSaved && bXMLRadarSaved && bXMLBarcodeSaved) ;
}

//-----------------------------------------------------------------------------
int CFormManager::EditForm(BOOL bExecEnabled, LPCTSTR pszNewFile /*= NULL*/)
{
	if (!ActivableForm())
		return IDCANCEL;

	// su errore non permetto di modificare perche` la struttura in memoria
	// potrebbe essere incoerente.
	//
	if (m_bParseError)
	{
		AfxMessageBox(cwsprintf(_TB("Unable to customize the active object due to syntax errors in the archive {0-%s}"), (LPCTSTR)m_pDocument->GetFormName()));
		return IDCANCEL;
	}

	CString strTitle = m_pDocument->GetTitle();
	CFormSheet aFormSheet(*this, strTitle, bExecEnabled);

	if (pszNewFile)
	{
		ASSERT(aFormSheet.m_pFormRadarDlg);
		aFormSheet.m_pFormRadarDlg->m_sNewReport = pszNewFile;
		aFormSheet.SetActivePage(aFormSheet.m_pFormRadarDlg);
	}
	return aFormSheet.DoModal();
}

//-----------------------------------------------------------------------
CString CFormManager::GetReportName()
{
	CFunctionDescription* pReportInfo = m_Reports.GetCurrentDefaultReport();//GetDefaultReportDescription();
	if (pReportInfo == NULL)
		return _T("");
	return pReportInfo->GetNamespace().ToString();
}

//-----------------------------------------------------------------------
CFunctionDescription* CFormManager::GetReportDescription()
{
	return m_Reports.GetDefaultReportDescription();
}

//-----------------------------------------------------------------------
CString CFormManager::GetBarcodeName()
{
	CFunctionDescription* pReportInfo = m_Barcode.GetCurrentDefaultReport();//GetDefaultReportDescription();
	if (pReportInfo == NULL)
		return _T("");
	return pReportInfo->GetNamespace().ToString();
}

//-----------------------------------------------------------------------
CFunctionDescription* CFormManager::GetBarcodeDescription()
{
	return m_Barcode.GetDefaultReportDescription();
}
// nel caso di report corrente attivo deve funzionare solo una volta
// a causa del bottone di EXEC ma non deve modificare altro.
//-----------------------------------------------------------------------
CString CFormManager::GetRadarName()
{
	CFunctionDescription* pReportInfo = m_Radars.GetCurrentDefaultReport();//GetDefaultReportDescription();
	if (pReportInfo == NULL)
		return _T("");
	return pReportInfo->GetNamespace().ToString();
}

//----------------------------------------------------------------------------
CString CFormManager::GetReportName (int nIdx)
{
	CFunctionDescription* pReportInfo = GetReportDescription(nIdx);
	if (pReportInfo == NULL)
		return _T("");
	return pReportInfo->GetNamespace().ToString();
}

//----------------------------------------------------------------------------
CFunctionDescription* CFormManager::GetReportDescription (int nIdx)
{
	return m_Reports.GetReportDescription (nIdx);
}

//----------------------------------------------------------------------------
CString CFormManager::GetBarcodeName (int nIdx)
{
	CFunctionDescription* pReportInfo = GetBarcodeDescription(nIdx);
	if (pReportInfo == NULL)
		return _T("");
	return pReportInfo->GetNamespace().ToString();
}

//----------------------------------------------------------------------------
CFunctionDescription* CFormManager::GetBarcodeDescription (int nIdx)
{
	return m_Barcode.GetReportDescription (nIdx);
}
//----------------------------------------------------------------------------
CString CFormManager::GetRadarName (int nIdx)
{
	if (nIdx < 0 || nIdx > m_Radars.m_arShowReports.GetReports().GetUpperBound())
		return _T("");

	CFunctionDescription* pReportInfo = (CFunctionDescription*) (m_Radars.m_arShowReports.GetReports().GetAt(nIdx));
	ASSERT(pReportInfo);

	if (pReportInfo->GetName().CompareNoCase(GetPredefinedRadar()) == 0) 
		return _T("");

	return pReportInfo->GetNamespace().ToString();
}

//-----------------------------------------------------------------------------
int CFormManager::GetIndexBarcodeDefault()
{
	CDocumentReportDescription* pReportDef = NULL;

	if (m_Barcode.m_NsCurrDefault.IsValid())
		pReportDef = (CDocumentReportDescription*) m_Barcode.m_arShowReports.GetReportInfo(m_Reports.m_NsCurrDefault);
	else
		pReportDef = (CDocumentReportDescription*) m_Barcode.m_arShowReports.GetDefault();

	for (int i = 0; i <= m_Barcode.m_arShowReports.GetReports().GetUpperBound(); i++)
	{
		CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) (m_Barcode.m_arShowReports.GetReports().GetAt(i));
		ASSERT(pReportInfo);

		if (pReportInfo == pReportDef)
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
int CFormManager::GetIndexReportDefault()
{
	CDocumentReportDescription* pReportDef = NULL;

	if (m_Reports.m_NsCurrDefault.IsValid())
		pReportDef = (CDocumentReportDescription*) m_Reports.m_arShowReports.GetReportInfo(m_Reports.m_NsCurrDefault);
	else
		pReportDef = (CDocumentReportDescription*) m_Reports.m_arShowReports.GetDefault();

	for (int i = 0; i <= m_Reports.m_arShowReports.GetReports().GetUpperBound()/*m_Reports.m_arReports.GetReports().GetUpperBound()*/; i++)
	{
		CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) (m_Reports.m_arShowReports.GetReports().GetAt(i));//  (m_Reports.m_arReports.GetReports().GetAt(i));
		ASSERT(pReportInfo);

		if (pReportInfo == pReportDef)
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
int CFormManager::GetIndexRadarDefault	()
{
	CDocumentReportDescription* pReportDef = NULL;

	if (m_Radars.m_NsCurrDefault.IsValid())
		pReportDef = (CDocumentReportDescription*) m_Radars.m_arShowReports.GetReportInfo(m_Radars.m_NsCurrDefault);
	else
		pReportDef = (CDocumentReportDescription*) m_Radars.m_arShowReports.GetDefault();

	for (int i = 0; i <= m_Radars.m_arShowReports.GetReports().GetUpperBound()/*m_Reports.m_arReports.GetReports().GetUpperBound()*/; i++)
	{
		CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) (m_Radars.m_arShowReports.GetReports().GetAt(i));//  (m_Reports.m_arReports.GetReports().GetAt(i));
		ASSERT(pReportInfo);

		if (pReportInfo == pReportDef)
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
int CFormManager::GetIndexRadarPredefinito	()
{
	for (int i = 0; i <= m_Radars.m_arShowReports.GetReports().GetUpperBound(); i++)
	{
		CFunctionDescription* pReportInfo = (CFunctionDescription*) (m_Radars.m_arShowReports.GetReports().GetAt(i));
		ASSERT(pReportInfo);

		if (pReportInfo->GetName().CompareNoCase(GetPredefinedRadar()) == 0)
			return i;
	}
	return -1;
}

//----------------------------------------------------------------------------
void CFormManager::EnumRadarAlias(CStringArray& arReportName)
{	
	arReportName.RemoveAll();

	CStringArray arNamespaces;
	for (int i = 0; i <= m_Radars.m_arShowReports.GetReports().GetUpperBound(); i++)
	{
		CFunctionDescription* pReportInfo = (CFunctionDescription*) (m_Radars.m_arShowReports.GetReports().GetAt(i));
		ASSERT(pReportInfo);

		if (pReportInfo->GetName().CompareNoCase(GetPredefinedRadar()) == 0) 
			continue;

		BOOL bFound = FALSE;
		for (int n=0; n <= arNamespaces.GetUpperBound(); n++)
		{
			if (pReportInfo->GetNamespace().ToString().CompareNoCase(arNamespaces.GetAt(n)) == 0)
			{
				bFound = TRUE;
				break;
			}
		}
		if (bFound)
			continue;

		arNamespaces.Add (pReportInfo->GetNamespace().ToString());

		CString sTitle = AfxLoadXMLString (
								pReportInfo->GetNotLocalizedTitle(), 
								szBarcodeXML, 
								AfxGetDictionaryPathFromNamespace(m_pDocument->GetNamespace(), TRUE)
							);

		arReportName.Add(pReportInfo->GetTitle());
	}
}
//----------------------------------------------------------------------------
int CFormManager::AddTileInfo(TileInfo* aObj) 
{ 
	return m_TileInfos.Add(aObj); 
}

//----------------------------------------------------------------------------
void CFormManager::EnumBarcodeAlias(CStringArray& arReportName)
{	
	arReportName.RemoveAll();
	
	CStringArray arNamespaces;
	for (int i = 0; i <= m_Barcode.m_arShowReports.GetReports().GetUpperBound(); i++)
	{
		CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) (m_Barcode.m_arShowReports.GetReports().GetAt(i));
		ASSERT(pReportInfo);

		BOOL bFound = FALSE;
		for (int n=0; n <= arNamespaces.GetUpperBound(); n++)
		{
			if (pReportInfo->GetNamespace().ToString().CompareNoCase(arNamespaces.GetAt(n)) == 0)
			{
				bFound = TRUE;
				break;
			}
		}
		if (bFound)
			continue;

		arNamespaces.Add (pReportInfo->GetNamespace().ToString());

		CString sTitle = AfxLoadXMLString (
								pReportInfo->GetNotLocalizedTitle(), 
								szBarcodeXML, 
								AfxGetDictionaryPathFromNamespace(m_pDocument->GetNamespace(), TRUE)
							);
		arReportName.Add(sTitle);
	}
}

//----------------------------------------------------------------------------
void CFormManager::EnumReportAlias(CReportMenuNode* pReportRootNode)
{	
	pReportRootNode->RemoveAllSons();
	
	CStringArray arNamespaces;
	BOOL bGroupSpecified(FALSE);
	for (int i = 0; i <= m_Reports.m_arShowReports.GetReports().GetUpperBound(); i++)
	{
		CDocumentReportDescription* pReportInfo = (CDocumentReportDescription*) (m_Reports.m_arShowReports.GetReports().GetAt(i));
		ASSERT(pReportInfo);

		//---- verifica di non averlo già processato
		BOOL bFound = FALSE;
		for (int n=0; n <= arNamespaces.GetUpperBound(); n++)
		{
			if (pReportInfo->GetNamespace().ToString().CompareNoCase(arNamespaces.GetAt(n)) == 0)
			{
				bFound = TRUE;
				break;
			}
		}
		if (bFound)
			continue;
		//----

		arNamespaces.Add (pReportInfo->GetNamespace().ToString());

		CString sTitle = AfxLoadXMLString (
								pReportInfo->GetNotLocalizedTitle(), 
								szReportsXML, 
								AfxGetDictionaryPathFromNamespace(m_pDocument->GetNamespace(), TRUE)
							);

		if (pReportInfo->GetGroupDescription().GetId() > 0 || bGroupSpecified/*sto mischiando una gestione a gruppi dei report con report che non speficano gruppo*/)
		{
			// gestione gruppi
			if (!bGroupSpecified)
				bGroupSpecified = TRUE;
			CReportMenuNode* pReportNode = new CReportMenuNode;
			pReportNode->SetNodeTag(sTitle);

			CReportMenuNode* pAlreadyPresentGroupNode(NULL);
			CReportMenuNode* pCurrGroupNode(NULL);
			for (int j = 0; j <= pReportRootNode->GetSonsUpperBound(); j++)
			{
				pCurrGroupNode = pReportRootNode->GetSonAt(j);
				if (pCurrGroupNode->GetNodeTag().CompareNoCase(pReportInfo->GetGroupDescription().GetLocalize()) == 0)
				{
					pAlreadyPresentGroupNode = pCurrGroupNode;
					break;
				}
			}
			if (!pAlreadyPresentGroupNode)
			{
				CReportMenuNode* pGroupNode = new CReportMenuNode;
				pGroupNode->SetNodeTag(pReportInfo->GetGroupDescription().GetLocalize());
				pGroupNode->AddSon(pReportNode);
				pGroupNode->SetUseSubMenu(pReportInfo->GetGroupDescription().GetUseSubMenu());

				pReportRootNode->AddSon(pGroupNode);
			}
			else
			{
				pAlreadyPresentGroupNode->AddSon(pReportNode);
			}
		}
		else
		{
			// gestione priva di gruppi
			CReportMenuNode* pSon = new CReportMenuNode;
			pSon->SetNodeTag(sTitle);
			pSon->SetUseSubMenu(FALSE);
			pReportRootNode->AddSon(pSon);
		}
	}
}

//-----------------------------------------------------------------------------
BodyEditInfo*  CFormManager::GetBodyEditInfo(const CTBNamespace& ns) const
{
	for (int i = 0; i <= m_BodyEditInfos.GetUpperBound(); i++)
	{
		BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)m_BodyEditInfos.GetAt(i);
		if (pBodyEditInfo->GetInfoOSL()->m_Namespace == ns) 
			return pBodyEditInfo;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
BodyEditInfo*  CFormManager::GetBodyEditInfo(const CString& sNs) const
{
	for (int i = 0; i <= m_BodyEditInfos.GetUpperBound(); i++)
	{
		BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)m_BodyEditInfos.GetAt(i);
		if (sNs.CompareNoCase(pBodyEditInfo->GetInfoOSL()->m_Namespace.ToString()) == 0)
			return pBodyEditInfo;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
TileInfo*  CFormManager::GetTileInfo(const CTBNamespace& ns) const
{
	CString sNs = ns.ToString();
	return GetTileInfo(sNs);
}

//-----------------------------------------------------------------------------
TileInfo*  CFormManager::GetTileInfo(const CString& sNs) const
{
	for (int i = 0; i <= m_TileInfos.GetUpperBound(); i++)
	{
		TileInfo* pTileInfo = (TileInfo*)m_TileInfos.GetAt(i);
		if (pTileInfo->m_sNamespace.CompareNoCase(sNs) == 0)
			return pTileInfo;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CFormManager::ParseFormNSChanges()
{
	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szFormsSection, szUpdateTBFFiles, DataBool(m_bUpdateTBFFiles), szTbDefaultSettingFileName);
	m_bUpdateTBFFiles = pSetting ? *((DataBool*) pSetting) : m_bUpdateTBFFiles;

	if (!m_bUpdateTBFFiles)
		return;

	m_pFormNSChangeArray = new CFormNSChangeArray();
	CFormNSChangeParser aFormNSChangeParser;
	if (aFormNSChangeParser.LoadFormNSChange(m_pFormNSChangeArray, m_nNsRelease, m_pDocument->GetNamespace()))
	{
		m_nNsRelease = max(m_nNsRelease, m_pFormNSChangeArray->m_NsRelease);
		m_bInternalModified = FALSE;
	}
	else
		SAFE_DELETE(m_pFormNSChangeArray);
}

//-----------------------------------------------------------------------------
BOOL CFormManager::Parse()
{
	// time consuming funtion. Use waiting cursor
	m_pDocument->BeginWaitCursor();

	// leggo sempre entrambi
	m_bParseError = !ParseXML();

	if (ParseTBF())
		m_bTBFModified = FALSE;
	else
		m_bParseError = TRUE;

	m_pDocument->EndWaitCursor();

	return !m_bParseError;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::ParseXML()
{
	return m_Reports.Parse(m_Ns) && m_Radars.Parse(/*m_Ns*/m_pDocument->GetNamespace()) && m_Barcode.Parse(/*m_Ns*/m_pDocument->GetNamespace());
}

//-----------------------------------------------------------------------------
BOOL CFormManager::ParseTBF()
{
	CString sFormName = SLASH_CHAR + m_pDocument->GetFormName();

	// prima cerco in poi nell'utente dell'azienda corrente
	CString sFormPath = AfxGetPathFinder()->GetDocumentDescriptionPath
							(
								m_pDocument->GetNamespace(), 
								CPathFinder::USERS,
								AfxGetLoginInfos()->m_strUserName, 
								FALSE
							) + sFormName;

	// poi in AllUsers dell'azienda corrente
	if (!ExistFile(sFormPath))
		sFormPath = AfxGetPathFinder()->GetDocumentDescriptionPath
							(
								m_pDocument->GetNamespace(), 
								CPathFinder::ALL_USERS,
								_T(""),
								FALSE
							) + sFormName;

	
	// non esiste proprio
	if (!ExistFile(sFormPath))
		return TRUE;

	Parser* pLex = new Parser();
	BOOL bOk = pLex->Open(sFormPath) && ParseForm(*pLex);
	
	SAFE_DELETE(pLex);
	if (bOk && m_bInternalModified)
	{
		UnparseTBFSilent();
		m_bInternalModified = FALSE;
	}
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::ParseForm(Parser& lex)
{
	BOOL bOk =
			lex.ParseTag (T_RELEASE) &&
			lex.ParseInt (m_nFormRelease);

	if (!bOk)
		return FALSE;

	// break if wrong m_nRelease definition strFormFile
	if (m_nFormRelease > m_nRelease)
	{
		lex.SetError(_TB("Wrong release!"));
		return FALSE;
	}

	if (lex.Matched (T_COMMA))
		bOk = lex.ParseInt (m_nNsRelease);
	
	if (!bOk)
		return FALSE;

	ParseFormNSChanges();

	while (TRUE)
	{
		switch (lex.LookAhead())
		{
			case T_OBJECTS:
			{
				if (!ParseObjects(lex))
					return FALSE;
				break;
			}
			case T_PROPERTIES:
			{
				if (!ParseProperties(lex))
					return FALSE;
				break;
			}

			default:
				return TRUE;
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::ParseObjects(Parser& lex)
{
	if (!lex.Match(T_OBJECTS) || !lex.ParseBegin())
		return FALSE;

	BOOL bOk = TRUE;
	while (bOk)
	{
		switch (lex.LookAhead())
		{
			case T_BODY    : 
			{
				bOk = ParseBodyInfo(lex);
				break;
			}
			case T_DIALOG:
			{
				bOk = ParseTileInfo(lex);
				break;
			}

			case T_END  : 
				return lex.ParseEnd();

			case T_EOF  : return FALSE;
			default     : return lex.SetError (_TB("Wrong body syntax"));
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::ParseBodyInfo(Parser& lex)
{
	CString	strBodyTitle, strNamespace;

	BOOL bOk =
		lex.ParseTag		(T_BODY)		&&
		lex.ParseString		(strNamespace)	&&
		lex.ParseString		(strBodyTitle);

	if (!bOk) return FALSE;

	if (m_pFormNSChangeArray)
	{
		CString strNewNamespace = m_pFormNSChangeArray->GetNewNS(strNamespace);
		if (strNewNamespace.CompareNoCase(strNamespace) != 0)
		{
			strNamespace = strNewNamespace;
			m_bInternalModified = TRUE;
		}
	}

	BOOL bNew = FALSE;
	BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)GetBodyEditInfo(strNamespace);
	if (!pBodyEditInfo)
	{
		bNew = TRUE;
		pBodyEditInfo = new BodyEditInfo;
		pBodyEditInfo->m_strBodyTitle = strBodyTitle;
		pBodyEditInfo->GetInfoOSL()->m_Namespace.SetNamespace(strNamespace);
		//@@TODO: non ho eventuali parent diretti! (tipo TabDialog, SlaveView)
		//pBodyEditInfo->GetInfoOSL()->m_pParent = &((CSingleExtDocTemplate*)(m_pDocument->GetDocTemplate()))->GetInfoOSL();
	}
	else { ASSERT (pBodyEditInfo->IsKindOf(RUNTIME_CLASS(BodyEditInfo))); }

	if	(
			bOk									&&
			lex.ParseBegin	()					&&
			ParseAllColumns(lex, pBodyEditInfo)	&&
			lex.ParseEnd	()
		)
	{
		if (bNew) m_BodyEditInfos.Add(pBodyEditInfo);

		pBodyEditInfo->m_bModified = TRUE;

		return TRUE;
	}

	if (bNew) delete pBodyEditInfo;

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::ParseAllColumns(Parser& lex, BodyEditInfo* pBodyEditInfo)
{
	BOOL bOk = TRUE;
	if (lex.LookAhead(T_COLUMN))
		bOk = ParseColumnInfo(lex, pBodyEditInfo);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::ParseColumnInfo(Parser& lex, BodyEditInfo* pBodyEditInfo)
{
	BOOL bOk = TRUE;
	while (lex.LookAhead(T_COLUMN))
	{
		CString     strColumnCulture = AfxGetCulture();
		CString		strColumnNamespace;
		CString		strColumnTitle;
		DataType	dtColumnDataType;

		BOOL bColPos	= FALSE;
		int	 nColPos	= -1;
		BOOL bColScreenWidth	= FALSE;
		int	 nColScreenWidth	= 0;
		BOOL bStatus	= FALSE;
		WORD wStatus	= 0;

		bOk =
			lex.ParseTag		(T_COLUMN)			&&
			lex.ParseString		(strColumnNamespace) &&
			lex.ParseString		(strColumnTitle)	&&
			lex.ParseTag		(T_TYPE)			&&
			lex.ParseDataType	(dtColumnDataType);
		if (!bOk) 
			return FALSE;

		if (m_pFormNSChangeArray)
		{
			CString strNewNamespace = m_pFormNSChangeArray->GetNewNS(strColumnNamespace);
			if (strNewNamespace.CompareNoCase(strColumnNamespace) != 0)
			{
				strColumnNamespace = strNewNamespace;
				m_bInternalModified = TRUE;
			}
		}

		strColumnTitle.Replace(_T("\\n"), _T("\n")); //TODO puzza: verificare a cosa serve

		if (lex.Matched(T_STATUS))
		{
			bStatus = TRUE;
			bOk = lex.ParseWord(wStatus); 
		}
		if (lex.Matched(T_SIZE))
		{
			bColScreenWidth = TRUE; 
			bOk = lex.ParseInt(nColScreenWidth); 
		}
		if (lex.Matched(T_ORDER))
		{
			bColPos = TRUE; 
			bOk = lex.ParseInt(nColPos); 
		}
        if (lex.Matched(T_FLOCALIZE))
		{
            bOk = lex.ParseString(strColumnCulture); 
 		}

		BodyEditColumn* pBodyEditColumn = (BodyEditColumn*) pBodyEditInfo->GetColumnObject(strColumnNamespace);
		if (!pBodyEditColumn)
		{
			pBodyEditColumn = new BodyEditColumn;
			pBodyEditInfo->Add(pBodyEditColumn);

			pBodyEditColumn->m_Namespace.SetNamespace(strColumnNamespace);
		}
		else
		{
			TRACE(_T("tbf parse existed column %s"), strColumnNamespace);
			ASSERT(FALSE);
		}

		pBodyEditColumn->m_strColumnTitle = strColumnTitle;
		pBodyEditColumn->m_UICulture = strColumnCulture;

		pBodyEditColumn->m_ColumnDataType = dtColumnDataType;

		if (bStatus)
		{
			pBodyEditColumn->m_bStatusChanged = TRUE;
			pBodyEditColumn->m_wStatus = wStatus;
			WORD w = (STATUS_GRAYED|STATUS_NOCHANGE_GRAYED);
			if ((wStatus & w) == w)
				pBodyEditColumn->m_wDefaultStatus |= w;
		}

		if (bColScreenWidth)
			pBodyEditColumn->m_nColScreenWidth = nColScreenWidth;
		else
			pBodyEditColumn->m_nColScreenWidth = pBodyEditColumn->m_nDefaultColScreenWidth;

		if (bColPos)
			pBodyEditColumn->m_nColPos = nColPos;
		else
			pBodyEditColumn->m_nColPos = pBodyEditColumn->m_nDefaultColPos;

		pBodyEditColumn->m_bModified = TRUE;
	}
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::ParseTileInfo(Parser& lex)
{
	CString	strNamespace;
	WORD wPinned = 0;

	BOOL bOk =
		lex.ParseTag(T_DIALOG) &&
		lex.ParseString(strNamespace) &&
		lex.ParseTag(T_STATUS) &&
		lex.ParseWord(wPinned);

	if (bOk)
	{
		BOOL bPinned = wPinned == 1;
		WORD wCollapsed = 0;
		BOOL bCollapsed = lex.ParseComma() && lex.ParseWord(wCollapsed) && wCollapsed == 1;
		TileInfo* pInfo = new TileInfo(strNamespace, bPinned, bCollapsed);
		m_TileInfos.Add(pInfo);
	}

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::ParseProperties(Parser& lex)
{
	lex.SkipToken();

	BOOL bOrderByType;
	BOOL bDescending;

	if
		(
			lex.ParseBegin()							&&
			lex.ParseTag		(T_ORDER_FIND_FIELD)	&&
			lex.ParseBool		(bOrderByType)			&&
			lex.ParseTag		(T_DESCENDING)			&&
			lex.ParseBool		(bDescending)		
		)
	{	
		if (bOrderByType)
			m_Properties.m_OrderByType = FormProperties::FINDABLE_QUERY;
		m_Properties.m_bDescending = bDescending;

		if (lex.Matched (T_FIND_SLAVE_FIELD) && !lex.ParseBool (m_Properties.m_bFindSlave) )
			return FALSE;

		return lex.ParseEnd();
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::UnparseTBF()
{
	CStringArray arUsers;
	BOOL bForStandard		= FALSE;

	// finestra di richiesta salvataggio
	CCustomSaveInterface aSaveInterface;
	if (IsTBFInAutoSave())
	{
		aSaveInterface.m_eSaveMode = CCustomSaveInterface::COMPANY_USERS;
		aSaveInterface.m_aUsers.Add(AfxGetLoginInfos()->m_strUserName);
		aSaveInterface.m_bSaveAllUsers = FALSE;
	}
	else
	{
		CCustomSaveDialog aDlg(&aSaveInterface, AfxGetMainWnd());
		aDlg.EnableAllCompanies(TRUE);

		if (aDlg.DoModal() == IDCANCEL)
			return FALSE;
	}
	bForStandard	 = aSaveInterface.m_eSaveMode == CCustomSaveInterface::STANDARD;

	// per lo standard lo faccio secco
	CString sFileName;
	if (bForStandard)
	{
		sFileName = AfxGetPathFinder()->GetDocumentDescriptionPath(m_pDocument->GetNamespace(), CPathFinder::STANDARD) + SLASH_CHAR + m_pDocument->GetFormName();
		Unparser ofile		(sFileName);
		ofile.SetFormat		(CLineFile::UTF8);

		ofile.UnparseTag	(T_RELEASE,	FALSE);
		ofile.UnparseInt	(m_nRelease);

		UnparseForm			(ofile);
		return TRUE;
	}

	// travaso le richieste
	for (int i=0; i <= aSaveInterface.m_aUsers.GetUpperBound(); i++)
		arUsers.Add(aSaveInterface.m_aUsers.GetAt(i));

	for (int i=0; i <= arUsers.GetUpperBound(); i++)
	{
		// file da salvare
		sFileName = AfxGetPathFinder()->GetDocumentDescriptionPath
					(
						m_pDocument->GetNamespace(), 
						aSaveInterface.m_bSaveAllUsers ? CPathFinder::ALL_USERS : CPathFinder::USERS, 
						arUsers.GetAt(i), 
						TRUE, 
						CPathFinder::CURRENT
					)
					+ SLASH_CHAR + m_pDocument->GetFormName();

		Unparser ofile		(sFileName);
		ofile.SetFormat		(CLineFile::UTF8);

		ofile.UnparseTag	(T_RELEASE,	FALSE);
		ofile.UnparseInt	(m_nRelease);

		UnparseForm			(ofile);
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFormManager::UnparseTBFSilent()
{
	CStringArray arUsers;

	// finestra di richiesta salvataggio
	CString sFileName;

	sFileName = AfxGetPathFinder()->GetDocumentDescriptionPath
					(
						m_pDocument->GetNamespace(), 
						m_ForCurrentUser ? CPathFinder::USERS : CPathFinder::ALL_USERS, 
						AfxGetLoginInfos()->m_strUserName, 
						TRUE, 
						CPathFinder::CURRENT
					)
					+ SLASH_CHAR + m_pDocument->GetFormName();

	Unparser ofile		(sFileName);
	ofile.SetFormat		(CLineFile::UTF8);

	ofile.UnparseTag	(T_RELEASE,	FALSE);
	ofile.UnparseInt	(m_nRelease, FALSE);
	ofile.UnparseTag	(T_COMMA, FALSE);
	ofile.UnparseInt	(m_nNsRelease);

	UnparseForm(ofile);
	
	return TRUE;
}
//-----------------------------------------------------------------------------
void CFormManager::UnparseBarcodeXML(const CString& sUserForSave /*_T("")*/)
{
	if (m_Barcode.IsAllUsersModified())
	{
		UnparseAllUsersReportsXML();
		m_Barcode.SetAllUsersModified(FALSE);
	}	

	if (m_Barcode.IsUserModified())
	{
		UnparseUserReportsXML(sUserForSave,FALSE,TRUE);
		m_Barcode.SetUserModified(FALSE);
	}	
}
//-----------------------------------------------------------------------------
void CFormManager::UnparseReportXML(const CString& sUserForSave /*_T("")*/)
{
	if (m_Reports.IsAllUsersModified())
	{
		UnparseAllUsersReportsXML();
		m_Reports.SetAllUsersModified(FALSE);
	}	

	if (m_Reports.IsUserModified())
	{
		UnparseUserReportsXML(sUserForSave);
		m_Reports.SetUserModified(FALSE);
	}	
}

//-----------------------------------------------------------------------------
void CFormManager::UnparseRadarXML(const CString& sUserForSave /*_T("")*/)
{
	if (m_Radars.IsUserModified() && !m_Radars.IsAllUsersModified())
	{
		UnparseUserReportsXML(sUserForSave, TRUE);
		m_Radars.SetUserModified(FALSE);
	}

	if (m_Radars.IsAllUsersModified())
	{
		UnparseUserReportsXML(sUserForSave, TRUE, FALSE, TRUE);
		//		UnparseAllUsersReportsXML(TRUE);
		m_Radars.SetAllUsersModified(TRUE);
	}
}

//-----------------------------------------------------------------------------
BOOL CFormManager::UnparseStandardReportsXML()
{
	CXMLReportObjectsParser aParser; 
	CXMLDocumentObject		aXMLDocStd;
	CBaseDescriptionArray	aReport;

	for (int z = 0; z <= m_Reports.m_arAllReports.GetReports().GetUpperBound(); z++)
	{
		CDocumentReportDescription* pRep		 = (CDocumentReportDescription*) m_Reports.m_arAllReports.GetReports().GetAt(z);
		CString						sStdFilePath = AfxGetPathFinder()->GetReportFullNameIn(pRep->GetNamespace(), CPathFinder::STANDARD);
		
		switch (pRep->m_XMLFrom)
		{
			case (CDocumentReportDescription::XML_MODIFIED):
			{
				for (int g = 0; g <= m_Reports.m_arStandardReports.GetReports().GetUpperBound(); g++)
				{
					CDocumentReportDescription* pRepMod = (CDocumentReportDescription*) m_Reports.m_arStandardReports.GetReports().GetAt(g);
					if (pRep->GetNamespace() == pRepMod->GetNamespace())
						m_Reports.m_arStandardReports.RemoveReportAt(g);
				}
				CDocumentReportDescription* pRepNew = new CDocumentReportDescription(*pRep);
				pRepNew->m_XMLFrom = CDocumentReportDescription::XML_STANDARD;
				m_Reports.m_arStandardReports.AddReport(pRepNew);
				break;
			}

			case (CDocumentReportDescription::XML_ADD):
			{
				if (!ExistFile(sStdFilePath))
				{
					AfxMessageBox(cwsprintf(_TB("Report '{0-%s}' cannot be save because it isn't in the same location of xml file!!!"), pRep->GetName()));
					break;					
				}

				CDocumentReportDescription* pRepAdd = new CDocumentReportDescription(*pRep);
				pRepAdd->m_XMLFrom = CDocumentReportDescription::XML_STANDARD;
				m_Reports.m_arStandardReports.AddReport(pRepAdd);
				break;
			}

			case (CDocumentReportDescription::XML_DELETED):
				m_Reports.m_arStandardReports.RemoveReport(pRep);
				break;
		}
	}	

	CString sFileName = _T("");
	if (TRUE /*m_bIsRadars*/)
		AfxGetPathFinder()->GetDocumentReportsFile(m_Ns, CPathFinder::STANDARD);
	else
		AfxGetPathFinder()->GetDocumentRadarsFile(m_Ns, CPathFinder::STANDARD);

	m_Reports.MakeGeneralArrayReport();

	if (!m_Reports.m_arStandardReports.GetReports().IsEmpty())
	{
		aParser.Unparse(&aXMLDocStd, &m_Reports.m_arStandardReports);
		return aXMLDocStd.SaveXMLFile(sFileName, TRUE);
	}
	else
		return DeleteFile(sFileName);
}

//-----------------------------------------------------------------------------
BOOL CFormManager::UnparseAllUsersReportsXML(BOOL bIsRadars /*FALSE*/)
{
	CXMLReportObjectsParser aParser; 	
	CXMLDocumentObject		aXMLDocAllUsrs;
	CBaseDescriptionArray	aReport;

	for (int z = 0; z <= m_Reports.m_arAllReports.GetReports().GetUpperBound(); z++)
	{
		CDocumentReportDescription* pRep			= (CDocumentReportDescription*) m_Reports.m_arAllReports.GetReports().GetAt(z);
		CString						sFileAllUsr		= AfxGetPathFinder()->GetReportFullNameIn(pRep->GetNamespace(), CPathFinder::ALL_USERS);
		CString						sStdFilePath	= AfxGetPathFinder()->GetReportFullNameIn(pRep->GetNamespace(), CPathFinder::STANDARD);
		switch (pRep->m_XMLFrom)
		{
			case (CDocumentReportDescription::XML_MODIFIED):
			{
				for (int g = 0; g <= m_Reports.m_arAllUsersReports.GetReports().GetUpperBound(); g++)
				{
					CDocumentReportDescription* pRepMod = (CDocumentReportDescription*)m_Reports.m_arAllUsersReports.GetReports().GetAt(g);
					if (pRep->GetNamespace() == pRepMod->GetNamespace())
						m_Reports.m_arAllUsersReports.RemoveReportAt(g);
				}

				CDocumentReportDescription* pRepNew = new CDocumentReportDescription(*pRep);
				pRepNew->m_XMLFrom = CDocumentReportDescription::XML_ALLUSERS;
				m_Reports.m_arAllUsersReports.AddReport(pRepNew);
				break;
			}

			case (CDocumentReportDescription::XML_ADD):
			{
				if (!ExistFile(sFileAllUsr) && !ExistFile(sStdFilePath))
				{
					AfxMessageBox(cwsprintf(_TB("Report '{0-%s}' cannot be save because it isn't in the same location of xml file!!!"), pRep->GetName()));
                    break;
				}

				CDocumentReportDescription* pRepAdd = new CDocumentReportDescription(*pRep);
				pRepAdd->m_XMLFrom = CDocumentReportDescription::XML_ALLUSERS;
				m_Reports.m_arAllUsersReports.AddReport(pRepAdd);
				break;
			}

			case (CDocumentReportDescription::XML_DELETED):
			{
				m_Reports.m_arAllUsersReports.RemoveReport(pRep);
				break;
			}
		}
	}	

	m_Reports.MakeGeneralArrayReport();
	
	CString sFileName = _T("");
	if (TRUE /*m_bIsRadars*/)
		sFileName = AfxGetPathFinder()->GetDocumentRadarsFile(m_Ns, CPathFinder::ALL_USERS);
	else
		sFileName = AfxGetPathFinder()->GetDocumentReportsFile(m_Ns, CPathFinder::ALL_USERS);

	if (!m_Reports.m_arAllUsersReports.GetReports().IsEmpty())
	{
		aParser.Unparse(&aXMLDocAllUsrs, &m_Reports.m_arAllUsersReports);
		return aXMLDocAllUsrs.SaveXMLFile(sFileName, TRUE);
	}
	else
		return DeleteFile(sFileName);
}

//-----------------------------------------------------------------------------
BOOL CFormManager::UnparseUserReportsXML(const CString& sUsr/*CCustomSaveInterface* pSaveInterface*/, BOOL bIsRadars /*= FALSE*/, BOOL bIsBarcode /*= FALSE*/, BOOL bIsAllUser /*= FALSE*/)
{
	// so già che ci sono delle modifiche quindi riscrivo ex novo il file xml
	CXMLReportObjectsParser		aParserUsr; 
	CXMLDocumentObject			aXMLDocUsr;
	BOOL						bExistInOtherXML	= FALSE;
	CString						sFileName			= _T("");
	CDocumentReportDescription* pDefUsr				= NULL; 
	CDocumentReportDescription* pDefault			= NULL;
	CDocumentReportDescription* pDefDefined 		= NULL;		
	
	if (bIsRadars)		
	{
		pDefUsr		= m_Radars.m_arShowReports.GetReportInfo(m_Reports.m_NsUsr);
		pDefault	= m_Radars.m_arShowReports.GetDefault();
		if (pDefUsr)
		{
			CDocumentReportDescription* pRepAllUsr = m_Radars.m_arAllUsersReports.GetReportInfo(pDefUsr->GetNamespace());
			if (pRepAllUsr)
			{
				if (!pRepAllUsr->IsDefault())
					bExistInOtherXML = TRUE;
			}
			else
			{
				CDocumentReportDescription* pRepStd	= m_Radars.m_arStandardReports.GetReportInfo(pDefUsr->GetNamespace());
				if (pRepStd)
					bExistInOtherXML = TRUE;
			}
		}

		if (bIsAllUser)
			sFileName = AfxGetPathFinder()->GetDocumentRadarsFile(m_Ns, CPathFinder::ALL_USERS);
		else
			sFileName = AfxGetPathFinder()->GetDocumentRadarsFile(m_Ns, CPathFinder::USERS, sUsr);

		pDefDefined = m_Radars.m_arAllUsersReports.GetDefault();	
		
		if (pDefDefined != NULL && pDefDefined->GetNamespace() == pDefault->GetNamespace())//( || pDefDefined->GetNamespace() == pDefault->GetNamespace())
			pDefault = NULL;
		if (!m_Radars.m_arUserReports.GetReports().IsEmpty() || pDefault)
		{
			aParserUsr.Unparse(&aXMLDocUsr, &m_Radars.m_arUserReports/*, bExistInOtherXML*/, pDefault);
			aXMLDocUsr.SaveXMLFile(sFileName, TRUE);
		}
		else
			return DeleteFile(sFileName);

		return TRUE;
	}
	if (bIsBarcode)
	{
		pDefUsr		= m_Barcode.m_arShowReports.GetReportInfo(m_Reports.m_NsUsr);
		pDefault	= m_Barcode.m_arShowReports.GetDefault();
		if (pDefUsr)
		{
			CDocumentReportDescription* pRepAllUsr = m_Barcode.m_arAllUsersReports.GetReportInfo(pDefUsr->GetNamespace());
			if (pRepAllUsr)
			{
				if (!pRepAllUsr->IsDefault())
					bExistInOtherXML = TRUE;
			}
			else
			{
				CDocumentReportDescription* pRepStd	= m_Barcode.m_arStandardReports.GetReportInfo(pDefUsr->GetNamespace());
				if (pRepStd)
					bExistInOtherXML = TRUE;
			}
		}

		pDefDefined = m_Barcode.m_arAllUsersReports.GetDefault();	
		
		if (pDefDefined != NULL && pDefDefined->GetNamespace() == pDefault->GetNamespace())//( || pDefDefined->GetNamespace() == pDefault->GetNamespace())
			pDefault = NULL;
		if (!m_Barcode.m_arUserReports.GetReports().IsEmpty() || pDefault)
		{
			aParserUsr.Unparse(&aXMLDocUsr, &m_Barcode.m_arUserReports/*, bExistInOtherXML*/, pDefault);
			aXMLDocUsr.SaveXMLFile(sFileName, TRUE);
		}
		else
			return DeleteFile(sFileName);

		return TRUE;
	}
	else
	{
		pDefUsr		= m_Reports.m_arShowReports.GetReportInfo(m_Reports.m_NsUsr);
		pDefault	= m_Reports.m_arShowReports.GetDefault();
		// se definito default di usr controllo che il report non appartenga già alla lista std e allusrs
		if (pDefUsr)
		{
			CDocumentReportDescription* pRepAllUsr = m_Reports.m_arAllUsersReports.GetReportInfo(pDefUsr->GetNamespace());
			if (pRepAllUsr)
			{
				if (!pRepAllUsr->IsDefault())
					bExistInOtherXML = TRUE;
			}
			else
			{
				CDocumentReportDescription* pRepStd	= m_Reports.m_arStandardReports.GetReportInfo(pDefUsr->GetNamespace());
				if (pRepStd)
					bExistInOtherXML = TRUE;
			}
		}
	//	else
	//		pDefault = NULL;

		sFileName	= AfxGetPathFinder()->GetDocumentReportsFile(m_Ns, CPathFinder::USERS, sUsr);
//		pDefDefined = m_Reports.m_arAllUsersReports.GetDefault();
		if (!pDefault || pDefault->m_XMLFrom != CBaseDescription::XML_USER)
	        pDefault = NULL;

		if (!m_Reports.m_arUserReports.GetReports().IsEmpty() || pDefault)
		{
			aParserUsr.Unparse(&aXMLDocUsr, &m_Reports.m_arUserReports/*, bExistInOtherXML*/, pDefault);
			aXMLDocUsr.SaveXMLFile(sFileName, TRUE);
		}
		else
			return DeleteFile(sFileName);

		return TRUE;
	}
}

//-----------------------------------------------------------------------------
void CFormManager::UnparseForm(Unparser& ofile)
{
	UnparseProperties(ofile);
	UnparseObjects(ofile);
}

//-----------------------------------------------------------------------------
void CFormManager::UnparseObjects(Unparser& ofile)
{
	ofile.UnparseTag    (T_OBJECTS);
	ofile.UnparseBegin	();
		UnparseBodyEditInfos(ofile);
		UnparseTileInfos(ofile);
	ofile.UnparseEnd();
}

//-----------------------------------------------------------------------------
void CFormManager::UnparseBodyEditInfos(Unparser& ofile)
{
	for (int i = 0; i <= m_BodyEditInfos.GetUpperBound(); i++)
	{
		BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)m_BodyEditInfos.GetAt(i);
		ASSERT(pBodyEditInfo);

		if	(
				!pBodyEditInfo || 
				!pBodyEditInfo->m_bModified || 
				pBodyEditInfo->IsDefaultBody()
			) 
			continue;

		ofile.UnparseTag    (T_BODY, FALSE);
		ofile.UnparseString	(pBodyEditInfo->GetInfoOSL()->m_Namespace.ToString(), FALSE);
		ofile.UnparseString	(pBodyEditInfo->m_strBodyTitle);

		ofile.UnparseBegin	();
		UnparseBodyEditInfo	(ofile, pBodyEditInfo);
		ofile.UnparseEnd	();
	}
}

//-----------------------------------------------------------------------------
void CFormManager::UnparseBodyEditInfo(Unparser& ofile, BodyEditInfo* pBodyEditInfo)
{
	for (int i = 0; i <= pBodyEditInfo->GetUpperBound(); i++)
	{
		BodyEditColumn* pBodyEditColumn = pBodyEditInfo->GetAt(i);
		ASSERT(pBodyEditColumn);

		if	(
				!pBodyEditColumn ||
				//!pBodyEditColumn->m_bModified ||
					(
						pBodyEditColumn->m_wDefaultStatus == pBodyEditColumn->m_wStatus &&
						pBodyEditColumn->m_strDefaultColumnTitle == pBodyEditColumn->m_strColumnTitle &&
						(pBodyEditColumn->m_nColScreenWidth == 0 || pBodyEditColumn->m_nDefaultColScreenWidth == pBodyEditColumn->m_nColScreenWidth) &&
						pBodyEditColumn->m_nDefaultColPos == pBodyEditColumn->m_nColPos
					)
			)
			continue;

		ofile.UnparseTag		(T_COLUMN, FALSE);
		ofile.UnparseString		(pBodyEditColumn->m_Namespace.ToString(), FALSE);
		CString strColumnTitle = pBodyEditColumn->m_strColumnTitle;
		strColumnTitle.Replace(_T("\n"), _T("\\n"));
		ofile.UnparseString		(strColumnTitle, FALSE);

		ofile.UnparseTag		(T_TYPE,								FALSE);
		ofile.UnparseDataType	(pBodyEditColumn->m_ColumnDataType,	DataType::Null,	FALSE);

		WORD st = pBodyEditColumn->m_wStatus  & ~(STATUS_NOCHANGE_GRAYED|STATUS_NOCHANGE_HIDDEN);
		if (pBodyEditColumn->m_wDefaultStatus != st)
		{
			ofile.UnparseTag		(T_STATUS,							FALSE);
			ofile.UnparseWord		(st,								FALSE);
		}

		if (pBodyEditColumn->m_nColScreenWidth != 0 && pBodyEditColumn->m_nDefaultColScreenWidth != pBodyEditColumn->m_nColScreenWidth)
		{
			ofile.UnparseBlank	(										FALSE);
			ofile.UnparseTag	(T_SIZE,								FALSE);
			ofile.UnparseInt	(pBodyEditColumn->m_nColScreenWidth,	FALSE);
		}

		if (pBodyEditColumn->m_nColPos != -1 && pBodyEditColumn->m_nDefaultColPos != pBodyEditColumn->m_nColPos)
		{
			ofile.UnparseBlank	(										FALSE);
			ofile.UnparseTag	(T_ORDER,								FALSE);
			ofile.UnparseInt	(pBodyEditColumn->m_nColPos,			FALSE);
		}

		ofile.UnparseBlank      (										FALSE);  
		ofile.UnparseTag		(T_FLOCALIZE,							FALSE);
		ofile.UnparseString     (AfxGetCulture(),						FALSE);

		ofile.UnparseCrLf		();
	}
}

//-----------------------------------------------------------------------------
void CFormManager::UnparseTileInfos(Unparser& ofile)
{
	for (int i = 0; i <= m_TileInfos.GetUpperBound(); i++)
	{
		TileInfo* pInfo = (TileInfo*)m_TileInfos.GetAt(i);
		ASSERT(pInfo);

		ofile.UnparseTag(T_DIALOG, FALSE);
		ofile.UnparseString(pInfo->m_sNamespace, FALSE);

		ofile.UnparseTag(T_STATUS, FALSE);
		ofile.UnparseWord(pInfo->m_bIsPinned ? 1 : 0);
		ofile.UnparseComma();
		ofile.UnparseWord(pInfo->m_bIsCollapsed ? 1 : 0);
	}
}

//-----------------------------------------------------------------------------
void CFormManager::UnparseProperties(Unparser& ofile)
{
	ofile.UnparseTag    (T_PROPERTIES);
	ofile.UnparseBegin	();
	ofile.UnparseTag	(T_ORDER_FIND_FIELD, FALSE);
	ofile.UnparseBool	(m_Properties.m_OrderByType == FormProperties::FINDABLE_QUERY);
	ofile.UnparseTag	(T_DESCENDING, FALSE);
	ofile.UnparseBool	(m_Properties.m_bDescending);
	if (!m_Properties.m_bFindSlave)
	{
		ofile.UnparseTag	(T_FIND_SLAVE_FIELD, FALSE);
		ofile.UnparseBool	(m_Properties.m_bFindSlave);
	}
	ofile.UnparseEnd	();		
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CFormManager::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CFormManager\n");
}

void CFormManager::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG


/////////////////////////////////////////////////////////////////////////////
//						CFormPropertiesDlg
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CFormPropertiesDlg::CFormPropertiesDlg(CFormSheet* pSheet, FormProperties& aFormProperties)
	: 
	CLocalizablePropertyPage (IDD_FORMMNG_PROPERTIES),
	m_pSheet			(pSheet),
	m_FormProperties	(aFormProperties)
{
}

/////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CFormPropertiesDlg, CLocalizablePropertyPage)
	//{{AFX_MSG_MAP(CFormPropertiesDlg)
	ON_BN_CLICKED	(IDC_FORMMNG_PROPERTIES_FINDABLE,	OnChangePropertiesFindable)
	ON_BN_CLICKED	(IDC_FORMMNG_PROPERTIES_DESCENDING,	OnChangePropertiesDescending)
	ON_BN_CLICKED	(IDC_FORMMNG_PROPERTIES_FIND_SLAVE,	OnChangePropertiesFindSlave)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CFormPropertiesDlg::OnInitDialog() 
{
	CLocalizablePropertyPage::OnInitDialog();

	m_ctrlFormMngPropertiesFindable.	SubclassDlgItem(IDC_FORMMNG_PROPERTIES_FINDABLE,	this);
	m_ctrlFormMngPropertiesDescending.	SubclassDlgItem(IDC_FORMMNG_PROPERTIES_DESCENDING,	this);

	m_ctrlFormMngPropertiesFindable.	SetCheck(m_FormProperties.m_OrderByType == FormProperties::FINDABLE_QUERY);
	m_ctrlFormMngPropertiesDescending.	SetCheck(m_FormProperties.m_bDescending);

	m_ctrlFormMngPropertiesFindSlave.	SubclassDlgItem(IDC_FORMMNG_PROPERTIES_FIND_SLAVE,	this);
	m_ctrlFormMngPropertiesFindSlave.	SetCheck(m_FormProperties.m_bFindSlave);
	DataObj* pB = AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szEnableFindOnSlaveFields, DataBool(TRUE), szTbDefaultSettingFileName);
	BOOL bEnableFindSlave = pB ? *((DataBool*) pB) : TRUE;
	m_ctrlFormMngPropertiesFindSlave.EnableWindow(bEnableFindSlave);
	//if (!bEnableFindSlave)
	//	m_ctrlFormMngPropertiesFindSlave.SetCheck(2);
	//----
	CAbstractFormDoc* pDocument = m_pSheet->GetFormManager()->GetDocument();
	if (pDocument)
	{
		CWnd* pWnd = GetDlgItem(IDC_FORMMNG_DOCNS);
		if (pWnd)
			pWnd->SetWindowText(pDocument->GetNamespace().ToString());

		pWnd = GetDlgItem(IDC_FORMMNG_VIEWNS);
		CMasterFormView* pView = (CMasterFormView*) pDocument->GetFirstView();
		if (pWnd && pView)
			pWnd->SetWindowText(pView->GetNamespace().ToString());
	}

/* TODO AUDITING
	if 
		( 
			AfxGetActivityMonitorInterface()->MayTraceOperation() &&
			( 
				AfxGetActivityMonitorInterface()->IsTraceReporterEnabled () ||
				AfxGetActivityMonitorInterface()->IsOSLAdmin()
			)
		)
		GetDlgItem(IDC_FORMMNG_OSL_REPORTER)->EnableWindow(TRUE);
*/

/*
	// se l'utente ha abilitato la gestione di performance e se è l'unico documento attivo 
	// allora abilito il pulsante di apertura della dialog
	DataObj* pSetting = AfxGetSettingValuue(snsTbGenlib, szPerformanceAnalizer, szAnalizeDocPerformance, szTbDefaultSettingFileName);
	BOOL bDocPerfomance = pSetting && pSetting->GetDataType() == DataType::Bool ? *((DataBool*) pSetting): FALSE;
	
	if (bDocPerfomance	&& AfxGetBaseApp()->GetNrOpenDocuments() == 1)
		GetDlgItem(IDC_FORMMNG_DOC_PERFORMANCE)->EnableWindow(TRUE);	
*/
	return TRUE;
}

//-----------------------------------------------------------------------
void CFormPropertiesDlg::OnChangePropertiesFindable()
{
	m_FormProperties.m_OrderByType = 
		m_ctrlFormMngPropertiesFindable.GetCheck()
			? FormProperties::FINDABLE_QUERY
			: m_FormProperties.m_OrderByType = FormProperties::BROWSER_QUERY;

	SetModified();
}

//-----------------------------------------------------------------------
void CFormPropertiesDlg::OnChangePropertiesDescending()
{
	m_FormProperties.m_bDescending = m_ctrlFormMngPropertiesDescending.GetCheck();
	SetModified();
}

//-----------------------------------------------------------------------
void CFormPropertiesDlg::OnChangePropertiesFindSlave()
{
	m_FormProperties.m_bFindSlave = m_ctrlFormMngPropertiesFindSlave.GetCheck();
	SetModified();
}

//-----------------------------------------------------------------------
void CFormPropertiesDlg::DoSave(BOOL bFromApply)
{
	ASSERT(m_pSheet);

	if (m_pSheet->m_bTBFModified)
		m_pSheet->SaveSheet(_T(""));
}

//-----------------------------------------------------------------------
void CFormPropertiesDlg::OnOK( )
{
	ASSERT_VALID(this);
	DoSave (FALSE);
}

//-----------------------------------------------------------------------
BOOL CFormPropertiesDlg::OnApply()
{
	ASSERT_VALID(this);
	DoSave (TRUE);
	
	return TRUE;
}

//-----------------------------------------------------------------------
void CFormPropertiesDlg::SetModified(BOOL bModified /*=TRUE*/)
{
	if (bModified)
		m_pSheet->m_bTBFModified = bModified;

	CLocalizablePropertyPage::SetModified(bModified);

//@@TODO  abiitare dopo aver fattop parse/unparse
//	ASSERT( GetParent() && GetParent()->IsKindOf(RUNTIME_CLASS(CFormSheet)) );
//	((CFormSheet*)GetParent())->SetUpdateable();
}

/////////////////////////////////////////////////////////////////////////////
//						CFormBodyDlg
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CFormBodyDlg::CFormBodyDlg(CFormSheet* pSheet, FormBodyEdits& aFormBodyEdits)
	:	 
	CLocalizablePropertyPage (IDD_FORMMNG_BODY),
	m_pSheet			(pSheet),
	m_FormBodyEdits		(aFormBodyEdits),
	m_hCurrentItem		(NULL),
	m_lParamCurrentItem	(LPARAM(0L)),
	b_bSuspendTree		(FALSE)
{
}

/////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CFormBodyDlg, CLocalizablePropertyPage)
	//{{AFX_MSG_MAP(CFormBodyDlg)
	ON_NOTIFY(TVN_SELCHANGED,	IDC_FORMMNG_TREE, OnSelchangedTree)
	ON_NOTIFY(TVN_SELCHANGING,	IDC_FORMMNG_TREE, OnSelchangingTree)

	ON_EN_KILLFOCUS	(IDC_FORMMNG_BODY_NAME,		OnKillfocusBodyName)

	ON_EN_KILLFOCUS	(IDC_FORMMNG_COL_NAME,	OnKillfocusColumnName)
	ON_EN_KILLFOCUS	(IDC_FORMMNG_COL_WIDTH,	OnKillfocusColumnWidth)

	ON_BN_CLICKED	(IDC_FORMMNG_COL_VISIBLE,		OnStatusChanged)
	ON_BN_CLICKED	(IDC_FORMMNG_COL_EDITABLE,		OnStatusChanged)

	ON_BN_CLICKED	(IDC_FORMMNG_RESETDEFAULT,	OnClickResetDefault)

	ON_NOTIFY(UDN_DELTAPOS, IDC_FORMMNG_COL_SPINORDER, OnDeltaposSpinOrder)

	ON_WM_LBUTTONDBLCLK()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
BOOL CFormBodyDlg::OnInitDialog() 
{
	CLocalizablePropertyPage::OnInitDialog();

	m_ctrlTree.				SubclassDlgItem (IDC_FORMMNG_TREE,		this);

	m_ctrlFormMngBodyName.	SubclassDlgItem (IDC_FORMMNG_BODY_NAME,	this);
	m_ctrlFormMngResetDefault.SubclassDlgItem (IDC_FORMMNG_RESETDEFAULT,	this);

	m_ctrlFormMngColumnName.SubclassDlgItem (IDC_FORMMNG_COL_NAME,this);
	m_ctrlFormMngVisible.	SubclassDlgItem (IDC_FORMMNG_COL_VISIBLE,	this);
	m_ctrlFormMngEditable.	SubclassDlgItem (IDC_FORMMNG_COL_EDITABLE,	this);
	m_ctrlFormMngWidth.		SubclassDlgItem (IDC_FORMMNG_COL_WIDTH, 	this);
	m_ctrlFormMngOrder.		SubclassDlgItem (IDC_FORMMNG_COL_ORDER,		this);
	m_ctrlFormMngSpinOrder.	SubclassDlgItem (IDC_FORMMNG_COL_SPINORDER,	this);

	m_ctrlFormMngVisible2.	SubclassDlgItem (IDC_FORMMNG_COL_VISIBLE2,	this);
	m_ctrlFormMngEditable2.	SubclassDlgItem (IDC_FORMMNG_COL_EDITABLE2,	this);
	m_ctrlFormMngDefaultColumnName.SubclassDlgItem (IDC_FORMMNG_COL_DEFAULTNAME,this);
	m_ctrlFormMngDefaultOrder.	SubclassDlgItem (IDC_FORMMNG_COL_DEFAULTORDER,	this);
	m_ctrlFormMngDefaultWidth.	SubclassDlgItem (IDC_FORMMNG_COL_DEFAULTWIDTH,	this);

	// valori di default non si possono cambiare
	m_ctrlFormMngVisible2.EnableWindow(FALSE);
	m_ctrlFormMngEditable2.EnableWindow(FALSE);

	CString asPaths[8];
	asPaths[0] = TBGlyph(szGlyphTable);//TBGlyph(szIconFormMngGrid);
	asPaths[1] = TBIcon(szIconDocument, CONTROL); //TBGlyph(szIconFormMngDocument);
	asPaths[2] = TBIcon(szIconEditFilled, CONTROL);//TBIcon(szIconFormMngVisibleEdit);
	asPaths[3] = TBIcon(szIconEditFilled, CONTROL);//TBGlyph(szIconFormMngEdit);
	asPaths[4] = TBIcon(szIconVisible, CONTROL);//TBGlyph(szIconFormMngVisible);
	asPaths[5] = TBIcon(szIconInvisible, CONTROL);//TBGlyph(szIconFormMngHidden);
	asPaths[6] = TBIcon(szIconStop, CONTROL);//TBGlyph(szIconFormMngStop);
	asPaths[7] = TBIcon(szIconInfo, CONTROL);//TBGlyph(szIconFormMngInfo);

	for (size_t i = 0; i < 7; i++)
	{
		HICON hIcon = TBLoadImage(asPaths[i]);
		if (hIcon == NULL)
		{
			continue;
		}
		if (i == 0)
		{
			CSize iconSize = GetHiconSize(hIcon);
			m_imaSmall.Create(iconSize.cx, iconSize.cy, ILC_COLOR32, 20, 20);
			m_imaSmall.SetBkColor(AfxGetThemeManager()->GetTransBmpTransparentDefaultColor());
		}

		m_imaSmall.Add(hIcon);
	}


	m_ctrlTree.SetImageList(&m_imaSmall, TVSIL_NORMAL);

	// inizialmente tutto disabilitato perche` potrebbero non esserci body
	m_ctrlFormMngBodyName.	EnableWindow(FALSE);
	m_ctrlFormMngColumnName.EnableWindow(FALSE);
	m_ctrlFormMngVisible.	EnableWindow(FALSE);
	m_ctrlFormMngEditable.	EnableWindow(FALSE);
	m_ctrlFormMngSpinOrder.	EnableWindow(FALSE);
	m_ctrlFormMngWidth.		EnableWindow(FALSE);


	FillAllTree();
	return TRUE;
}

//-----------------------------------------------------------------------
int	CFormBodyDlg::GetImage (BodyEditColumn* pBodyEditColumn)
{
	int nImage = COLUMN_IMAGE_OFFSET;

	if (pBodyEditColumn->m_wStatus & (STATUS_GRAYED | STATUS_NOCHANGE_GRAYED))
		nImage = COLUMN_IMAGE_OFFSET + 2;
	if (pBodyEditColumn->m_wStatus & (STATUS_HIDDEN | STATUS_NOCHANGE_HIDDEN))
		nImage = COLUMN_IMAGE_OFFSET + 3;

	return nImage;
}

//-----------------------------------------------------------------------
void CFormBodyDlg::OnLButtonDblClk(UINT , CPoint )
{
	CString sClipboard;

	for (int i = 0; i <= m_FormBodyEdits.GetUpperBound(); i++)
	{
		BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)(m_FormBodyEdits.GetAt(i));
		ASSERT_VALID(pBodyEditInfo);
		if (!pBodyEditInfo->m_bValid)
			continue;
		if (!pBodyEditInfo->GetSize())
			continue;

		CString sNamespace = pBodyEditInfo ->GetInfoOSL()->m_Namespace.ToUnparsedString();
		sClipboard += sNamespace + ' ' + '\"' + pBodyEditInfo->m_strBodyTitle + '\"' + '\n';
	}

	if (sClipboard.IsEmpty())
		return;

	::CopyToClipboard(sClipboard);
}

//-----------------------------------------------------------------------
void CFormBodyDlg::FillAllTree() 
{
	for (int i = 0; i <= m_FormBodyEdits.GetUpperBound(); i++)
	{
		BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)(m_FormBodyEdits.GetAt(i));

		FillBodyTree(pBodyEditInfo);
	}
}

//-----------------------------------------------------------------------
HTREEITEM CFormBodyDlg::FillBodyTree(BodyEditInfo* pBodyEditInfo)
{
	ASSERT_VALID(pBodyEditInfo);
	ASSERT_KINDOF(BodyEditInfo, pBodyEditInfo);	
	
	// se le informazioni sono incomplete (non validate dall'istanza effettiva del
	// bodyedit non lo inserisco  per non confondere le idee
	if (!pBodyEditInfo->m_bValid)
		return NULL;
	if (!pBodyEditInfo->GetSize())
		return NULL;

	// aggiunge il singolo bodyedit
	HTREEITEM htmB = m_ctrlTree.InsertItem(pBodyEditInfo->m_strBodyTitle, 0,0, TVI_ROOT, TVI_LAST );
	m_ctrlTree.SetItemData(htmB,(DWORD)pBodyEditInfo); 

	for (int j = 0; j <= pBodyEditInfo->GetUpperBound(); j++)
	{
		BodyEditColumn* pBodyEditColumn = pBodyEditInfo->GetAt(j);
		ASSERT(pBodyEditColumn);

		InsertSibling(htmB, pBodyEditColumn);
	}
	return htmB;
}

//-----------------------------------------------------------------------
HTREEITEM CFormBodyDlg::InsertSibling(HTREEITEM htmParent, BodyEditColumn* pBodyEditColumn, HTREEITEM htmAfter /*= TVI_LAST*/)
{
	ASSERT_VALID(pBodyEditColumn);
	ASSERT_KINDOF(BodyEditColumn, pBodyEditColumn);	

	CString strTitle = pBodyEditColumn->m_strColumnTitle;
	if ( strTitle.IsEmpty() )
		strTitle.Format(_T("[%s]"), pBodyEditColumn->m_strDefaultColumnTitle);
	
	strTitle.Replace(_T("\n"),_T("\\n"));
	
	HTREEITEM htmC = m_ctrlTree.InsertItem(strTitle, 1,1, htmParent, htmAfter);
	m_ctrlTree.SetItemData(htmC,(DWORD)pBodyEditColumn); 

	// rinfresco anche l'immagine nel tree
	int nImage = GetImage(pBodyEditColumn);//->m_wStatus + COLUMN_IMAGE_OFFSET;
	m_ctrlTree.SetItemImage(htmC, nImage, nImage); 

	return htmC;
}

//-----------------------------------------------------------------------
void CFormBodyDlg::SetModifedColumn()
{
	BodyEditColumn* pBodyEditColumn = (BodyEditColumn*)m_lParamCurrentItem;
	ASSERT_VALID(pBodyEditColumn);
	ASSERT_KINDOF(BodyEditColumn, pBodyEditColumn);	

	// gestisce i modificati
	pBodyEditColumn->m_bModified = TRUE;

	SetModifedBody();
}

//-----------------------------------------------------------------------
void CFormBodyDlg::SetModifedBody()
{
	HTREEITEM hParent = m_ctrlTree.GetParentItem (m_hCurrentItem);
	if (hParent != NULL)
	{
		BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)m_ctrlTree.GetItemData(hParent); 
		ASSERT_VALID(pBodyEditInfo);
		ASSERT_KINDOF(BodyEditInfo, pBodyEditInfo);	

		pBodyEditInfo->m_bModified = TRUE;
	}
	m_pSheet->m_bTBFModified = TRUE;
	SetModified(TRUE);
}

//-----------------------------------------------------------------------
void CFormBodyDlg::DoSave(BOOL bFromApply)
{
	ASSERT(m_pSheet);

	if (m_pSheet->m_bTBFModified)
		m_pSheet->SaveSheet(_T(""));
}

//-----------------------------------------------------------------------
void CFormBodyDlg::OnOK()
{
	ASSERT_VALID(this);
	DoSave (FALSE);
}

//-----------------------------------------------------------------------
BOOL CFormBodyDlg::OnApply()
{
	ASSERT_VALID(this);
	DoSave (TRUE);
	
	return TRUE;
}

//-----------------------------------------------------------------------
void CFormBodyDlg::OnKillfocusBodyName()
{
	// non sono un nodo di body
	if ( m_ctrlTree.GetChildItem (m_hCurrentItem) == NULL)
		return;

	BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)m_lParamCurrentItem;
	ASSERT_VALID(pBodyEditInfo);
	ASSERT_KINDOF(BodyEditInfo, pBodyEditInfo);	

	m_ctrlFormMngBodyName.GetWindowText(pBodyEditInfo->m_strBodyTitle);
	m_ctrlTree.SetItemText(m_hCurrentItem, pBodyEditInfo->m_strBodyTitle);

	pBodyEditInfo->m_bModified = TRUE;
	SetModifedBody();
}

//-----------------------------------------------------------------------
void CFormBodyDlg::OnDeltaposSpinOrder(NMHDR* pNMHDR, LRESULT* pResult) 
{
	NM_UPDOWN* pNMUpDown = (NM_UPDOWN*)pNMHDR;
	// TODO: Add your control notification handler code here
	CString strOrder;

	// non sono una colonna di Body
	if ( m_ctrlTree.GetChildItem (m_hCurrentItem) != NULL)
		return;

	BodyEditColumn* pBodyEditColumn = (BodyEditColumn*)m_lParamCurrentItem;
	ASSERT_VALID(pBodyEditColumn);
	ASSERT_KINDOF(BodyEditColumn, pBodyEditColumn);	
	
	HTREEITEM hSwapSibling;
	if ( pNMUpDown->iDelta > 0 )
		hSwapSibling = m_ctrlTree.GetNextSiblingItem( m_hCurrentItem );
	else
		hSwapSibling = m_ctrlTree.GetPrevSiblingItem( m_hCurrentItem );

	if (hSwapSibling != NULL)
	{
		int nOldPos;
		m_ctrlFormMngOrder.GetWindowText(strOrder);
		nOldPos = _tstoi(strOrder);

		int nNewPos = nOldPos + (pNMUpDown->iDelta > 0 ? 1 : -1);//una sola posizione alla volta

		int nLower,nUpper;
		m_ctrlFormMngSpinOrder.GetRange(nLower,nUpper);
		if (nNewPos > nLower || nNewPos < nUpper)
		{
			*pResult = 1; //FAIL: do NOT update 
			return;
		}

		BodyEditColumn* pSwapSibling = (BodyEditColumn*) m_ctrlTree.GetItemData (hSwapSibling);
		ASSERT_VALID(pSwapSibling);
		ASSERT_KINDOF(BodyEditColumn, pSwapSibling);	
		
		int nSelImg;
		int nImageSwapSibling;
		int nImageCurrent;

		m_ctrlTree.GetItemImage(hSwapSibling, nImageSwapSibling, nSelImg);
		m_ctrlTree.GetItemImage(m_hCurrentItem, nImageCurrent, nSelImg);

		m_ctrlTree.SetItemText(hSwapSibling, pBodyEditColumn->m_strColumnTitle);
		m_ctrlTree.SetItemText(m_hCurrentItem, pSwapSibling->m_strColumnTitle);
		m_ctrlTree.SetItemData(hSwapSibling, (DWORD)pBodyEditColumn);
		m_ctrlTree.SetItemData(m_hCurrentItem, (DWORD)pSwapSibling);
		m_ctrlTree.SetItemImage(m_hCurrentItem, nImageSwapSibling, nImageSwapSibling);
		m_ctrlTree.SetItemImage(hSwapSibling, nImageCurrent, nImageCurrent);

		pBodyEditColumn->m_nColPos = nNewPos;
		pBodyEditColumn->m_bModified = TRUE;

		pSwapSibling->m_nColPos = nOldPos;
		pSwapSibling->m_bModified = TRUE;

		//----
		SetModifedBody();

		m_ctrlTree.Select (hSwapSibling, TVGN_CARET);//visualizzera la nuova posizione
	}
	
	*pResult = 1; //Segnalo di non aggiornare automaticamente per evitare incrementi multipli
}

//-----------------------------------------------------------------------
void CFormBodyDlg::OnKillfocusColumnName()
{
	// non sono una colonna di Body
	if ( m_ctrlTree.GetChildItem (m_hCurrentItem) != NULL)
		return;

	BodyEditColumn* pBodyEditColumn = (BodyEditColumn*)m_lParamCurrentItem;
	ASSERT(pBodyEditColumn);

	m_ctrlFormMngColumnName.GetWindowText(pBodyEditColumn->m_strColumnTitle);
	m_ctrlTree.SetItemText(m_hCurrentItem, pBodyEditColumn->m_strColumnTitle);

	// gestisce i modificati
	SetModifedColumn();
}
//-----------------------------------------------------------------------
void CFormBodyDlg::OnKillfocusColumnWidth()
{
	// non sono una colonna di Body
	if ( m_ctrlTree.GetChildItem (m_hCurrentItem) != NULL)
		return;

	BodyEditColumn* pBodyEditColumn = (BodyEditColumn*)m_lParamCurrentItem;
	ASSERT_VALID(pBodyEditColumn);
	ASSERT_KINDOF(BodyEditColumn, pBodyEditColumn);	

	CString strTmp;
	m_ctrlFormMngWidth.GetWindowText(strTmp);

	if( pBodyEditColumn->m_nColScreenWidth != min(_tstoi(strTmp), 0x7fff) )
	{
		pBodyEditColumn->m_nColScreenWidth = min(_tstoi(strTmp), 0x7fff);

		// gestisce i modificati
		SetModifedColumn();
	}
}

//-----------------------------------------------------------------------
void CFormBodyDlg::OnStatusChanged()
{
	// non sono una colonna di Body
	if ( m_ctrlTree.GetChildItem (m_hCurrentItem) != NULL)
		return;

	BodyEditColumn* pBodyEditColumn = (BodyEditColumn*)m_lParamCurrentItem;
	ASSERT_VALID(pBodyEditColumn);
	ASSERT_KINDOF(BodyEditColumn, pBodyEditColumn);	

	pBodyEditColumn->m_wStatus &= ~(STATUS_HIDDEN | STATUS_GRAYED);
	pBodyEditColumn->m_wStatus |= m_ctrlFormMngVisible.GetCheck()	? 0	: STATUS_HIDDEN;
	pBodyEditColumn->m_wStatus |= m_ctrlFormMngEditable.GetCheck()	? 0 : STATUS_GRAYED;
	pBodyEditColumn->m_bStatusChanged = TRUE;

	//if (m_ctrlFormMngVisible.GetCheck() == 0)
	//	m_ctrlFormMngEditable.SetCheck(0);

	// rinfresco anche l'immagine nel tree
	int nImage = GetImage(pBodyEditColumn); 
	m_ctrlTree.SetItemImage(m_hCurrentItem, nImage, nImage); 

	// gestisce i modificati
	SetModifedColumn();
}


//-----------------------------------------------------------------------
void CFormBodyDlg::OnSelchangedTree(NMHDR* pNMHDR, LRESULT* pResult) 
{
	if (b_bSuspendTree) return;

	NMTREEVIEW* pNMTreeView = (NMTREEVIEW*)pNMHDR;
	
	// occorre mantenere info dell'elemento del tree correntemente attivo
	m_hCurrentItem = pNMTreeView->itemNew.hItem;
	m_lParamCurrentItem = pNMTreeView->itemNew.lParam;
	if (!m_lParamCurrentItem)
		return;
	if ( m_ctrlTree.GetChildItem (m_hCurrentItem) != NULL) //sono un nodo intermedio
	{
		BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)m_lParamCurrentItem;
		ASSERT_VALID(pBodyEditInfo);
		ASSERT_KINDOF(BodyEditInfo, pBodyEditInfo);	

		// pulisco e disabilito i control "Proprietà colonna"
		m_ctrlFormMngColumnName.EnableWindow(FALSE);
		m_ctrlFormMngColumnName.SetWindowText(_T(""));
		m_ctrlFormMngDefaultColumnName.SetWindowText(_T(""));
		m_ctrlFormMngVisible.	EnableWindow(FALSE);
		m_ctrlFormMngEditable.	EnableWindow(FALSE);
		m_ctrlFormMngSpinOrder.	EnableWindow(FALSE);
		m_ctrlFormMngWidth.		EnableWindow(FALSE);
		m_ctrlFormMngOrder.		SetWindowText(_T(""));
		m_ctrlFormMngWidth.		SetWindowText(_T(""));
		m_ctrlFormMngDefaultOrder. SetWindowText(_T(""));
		m_ctrlFormMngDefaultWidth. SetWindowText(_T(""));

		m_ctrlFormMngBodyName.EnableWindow(TRUE);
		m_ctrlFormMngResetDefault.EnableWindow(TRUE);
		m_ctrlFormMngBodyName.SetWindowText(pBodyEditInfo->m_strBodyTitle);
		return;
	}

	// sono in una foglia, pulisco e disabilito i control del body
	// ed abilito i control "Proprietà colonne"
	m_ctrlFormMngBodyName.EnableWindow(FALSE);
	m_ctrlFormMngResetDefault.EnableWindow(FALSE);
	
	BodyEditColumn* pBodyEditColumn = (BodyEditColumn*)m_lParamCurrentItem;
	ASSERT_VALID(pBodyEditColumn);
	ASSERT_KINDOF(BodyEditColumn, pBodyEditColumn);
	if (!pBodyEditColumn->IsKindOf(RUNTIME_CLASS(BodyEditColumn)))
		return;
	// se ho il GUID nullo non abilito i control "Proprietà colonna"
	BOOL bEnable = TRUE;		 

	m_ctrlFormMngColumnName.EnableWindow(bEnable);
	m_ctrlFormMngVisible.EnableWindow 
		(
			bEnable && 
			((pBodyEditColumn->m_wDefaultStatus & STATUS_NOCHANGE_HIDDEN) == 0)
		);
	m_ctrlFormMngEditable.EnableWindow
		(
			bEnable && 
			((pBodyEditColumn->m_wDefaultStatus & STATUS_NOCHANGE_GRAYED) == 0)
		);
	m_ctrlFormMngSpinOrder.EnableWindow (bEnable);
	m_ctrlFormMngWidth.EnableWindow (bEnable);	//@@TODO bEnable //NON funziona ancora in tutti i casi
	m_ctrlFormMngOrder.EnableWindow (bEnable);
	
	pBodyEditColumn->m_strColumnTitle.Replace(_T("\n"),_T("\\n"));

	CString strDefaultCT = pBodyEditColumn->m_strDefaultColumnTitle;
	strDefaultCT.Replace(_T("\n"),_T("\r\n"));

	m_ctrlFormMngColumnName.SetWindowText(pBodyEditColumn->m_strColumnTitle);
	m_ctrlFormMngDefaultColumnName.SetWindowText(strDefaultCT);
	m_ctrlFormMngVisible.SetCheck((pBodyEditColumn->m_wStatus & STATUS_HIDDEN) != STATUS_HIDDEN);
	m_ctrlFormMngEditable.SetCheck((pBodyEditColumn->m_wStatus & STATUS_GRAYED) != STATUS_GRAYED);

	HTREEITEM hParent = m_ctrlTree.GetParentItem (m_hCurrentItem);
	if (hParent != NULL)
	{
		BodyEditInfo* pBodyEditInfo = (BodyEditInfo*) m_ctrlTree.GetItemData (hParent); 
		ASSERT_VALID(pBodyEditInfo);
		ASSERT_KINDOF(BodyEditInfo, pBodyEditInfo);	

		m_ctrlFormMngSpinOrder.SetRange (pBodyEditInfo->GetUpperBound(), 0); //up decrementa pos., down incrementa
	}
	CString strTmp;
	strTmp.Format (_T("%d"), pBodyEditColumn->m_nColPos);
	m_ctrlFormMngOrder.SetWindowText (strTmp);

	if(pBodyEditColumn->m_nColScreenWidth == 0)
		pBodyEditColumn->m_nColScreenWidth = pBodyEditColumn->m_nDefaultColScreenWidth;
	strTmp.Format (_T("%d"), pBodyEditColumn->m_nColScreenWidth);
	m_ctrlFormMngWidth.SetWindowText (strTmp);

	strTmp.Format (_T("%d"), pBodyEditColumn->m_nDefaultColScreenWidth);
	m_ctrlFormMngDefaultWidth.SetWindowText (strTmp);
	strTmp.Format (_T("%d"), pBodyEditColumn->m_nDefaultColPos);
	m_ctrlFormMngDefaultOrder.SetWindowText (strTmp);

	// visualizza i valori di default
	m_ctrlFormMngVisible2.SetCheck((pBodyEditColumn->m_wDefaultStatus & STATUS_HIDDEN) != STATUS_HIDDEN);
	m_ctrlFormMngEditable2.SetCheck((pBodyEditColumn->m_wDefaultStatus & STATUS_GRAYED) != STATUS_GRAYED);

	*pResult = 0;
}


// Mi serve per aggiornare il dato all'interno del bodyeditinfo perche`
// il killfocus arriva dopo la selezione del tree
//-----------------------------------------------------------------------
void CFormBodyDlg::OnSelchangingTree(NMHDR* pNMHDR, LRESULT* pResult) 
{
	if (b_bSuspendTree) return;

	// all'inizio non ho nessun corrente selezionato
	if (m_hCurrentItem == NULL)
		return;

	ASSERT(m_hCurrentItem);
	ASSERT(m_lParamCurrentItem);

	if ( m_ctrlTree.GetChildItem (m_hCurrentItem) != NULL) //sono un nodo intermedio
	{
		BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)m_lParamCurrentItem;
		ASSERT_VALID(pBodyEditInfo);
		ASSERT_KINDOF(BodyEditInfo, pBodyEditInfo);	

		// aggiorno  sia il buffer che il tree (il tree no se e` vuoto)
		CString strCurrentTitle = pBodyEditInfo->m_strBodyTitle;
		m_ctrlFormMngBodyName.GetWindowText(pBodyEditInfo->m_strBodyTitle);

		// solo se e` diverso
		if (strCurrentTitle != pBodyEditInfo->m_strBodyTitle)
		{
			m_ctrlTree.SetItemText(m_hCurrentItem, pBodyEditInfo->m_strBodyTitle);
			SetModifedBody();
		}
		return;
	}

	BodyEditColumn* pBodyEditColumn = dynamic_cast<BodyEditColumn*>((CObject*)m_lParamCurrentItem);
	if (pBodyEditColumn)
	{
		ASSERT_VALID(pBodyEditColumn);
		ASSERT_KINDOF(BodyEditColumn, pBodyEditColumn);

		// aggiorno  sia il buffer che il tree
		CString strCurrentTitle = pBodyEditColumn->m_strColumnTitle;
		m_ctrlFormMngColumnName.GetWindowText(pBodyEditColumn->m_strColumnTitle);
		if (strCurrentTitle != pBodyEditColumn->m_strColumnTitle)
		{
			m_ctrlTree.SetItemText(m_hCurrentItem, pBodyEditColumn->m_strColumnTitle);
			SetModifedColumn();
		}
		CString strTmp;
		m_ctrlFormMngWidth.GetWindowText(strTmp);

		if( pBodyEditColumn->m_nColScreenWidth != min( _tstoi(strTmp), 0x7fff ) )
		{
			pBodyEditColumn->m_nColScreenWidth = min( _tstoi(strTmp), 0x7fff );
			SetModifedColumn();
		}
	}
	// autorizzo la prosecuzione della selezione
	*pResult = FALSE;
}

//---------------------------------------------------------------------------
void CFormBodyDlg::OnClickResetDefault()
{
	// non sono un nodo di body
	if ( m_ctrlTree.GetChildItem (m_hCurrentItem) == NULL)
		return;

	BodyEditInfo* pBodyEditInfo = (BodyEditInfo*)m_lParamCurrentItem;
	ASSERT_VALID(pBodyEditInfo);
	ASSERT_KINDOF(BodyEditInfo, pBodyEditInfo);	

	b_bSuspendTree = TRUE;

	pBodyEditInfo->ResetToDefault();
	m_pSheet->m_bTBFModified = TRUE;
	SetModified(TRUE);

	m_ctrlTree.DeleteItem(m_hCurrentItem);
	m_hCurrentItem = NULL;
	HTREEITEM ht = FillBodyTree(pBodyEditInfo);

	b_bSuspendTree = FALSE;

	m_ctrlTree.SelectItem(ht);
}

/////////////////////////////////////////////////////////////////////////////
//							CFormSheet
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC (CFormSheet, CLocalizablePropertySheet)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFormSheet, CLocalizablePropertySheet)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CFormSheet::CFormSheet (CFormManager& aFormManager, const CString& strTitle, BOOL bExecEnabled)
	:
	CLocalizablePropertySheet(strTitle),
	m_pFormBodyDlg			(NULL),
	m_pFormPropertiesDlg	(NULL),
	m_pFormRadarDlg			(NULL),
	m_pFormReportDlg		(NULL),
	m_pFormBarcodeDlg		(NULL),
	m_pSecurityAdmin		(NULL), 
	m_pProfilesWizPropPage	(NULL),
	m_FormManagerSource		(aFormManager),
	m_FormManager			(aFormManager), // qua effettua la copia locale
	m_bExecEnabled			(bExecEnabled),
	m_sCaption				(_TB("Document radar")),
	m_sCaptionBarcode		(_TB("Barcode")),
	m_bTBFModified			(FALSE),
	m_bXMLModified			(FALSE)
{
	// deve usare la copia locale per tutte le Tab
	m_pFormBodyDlg			= new CFormBodyDlg		(this, m_FormManager.m_BodyEditInfos);
	m_pFormPropertiesDlg	= new CFormPropertiesDlg(this, m_FormManager.m_Properties);
	m_pFormReportDlg		= new CFormReportDlg	(this, m_FormManager.m_Reports);
	m_pFormRadarDlg			= new CFormReportDlg	(this, m_FormManager.m_Radars);
	       
	AddPage(m_pFormPropertiesDlg);	
	AddPage(m_pFormBodyDlg);
	AddPage(m_pFormReportDlg);
	AddPage(m_pFormRadarDlg);
	m_pFormReportDlg->SetCaption(m_sCaption);
	

	CAbstractFormDoc *pDoc = aFormManager.GetDocument();

	if (pDoc->m_bEnableReportBarcodeWMS)
	{
		m_pFormBarcodeDlg		= new CFormReportDlg	(this, m_FormManager.m_Barcode);
		AddPage(m_pFormBarcodeDlg);
		m_pFormBarcodeDlg->SetCaption(m_sCaptionBarcode);
	}
	
	if (
			AfxIsActivated(TBEXT_APP, INTERACTIVE_FUNCTIONALITY) &&
			pDoc->GetXMLDataManager() && 
			pDoc->CanLoadXMLDescription() &&
			pDoc->GetType() != VMT_BATCH
		)
	{
		CXMLDataManagerObj* pXMLDataManagerObj = pDoc->GetXMLDataManager();
		
		if(pXMLDataManagerObj && !pDoc->GetXmlDescription()->IsTransferDisabled())
		{
			m_pProfilesWizPropPage = pXMLDataManagerObj->CreateProfilesWizardPropPage(pDoc->GetNamespace());
			if(m_pProfilesWizPropPage)
				AddPage(m_pProfilesWizPropPage);		
		}
	}

	if (AfxGetSecurityInterface()->IsSecurityEnabled() && AfxGetLoginInfos()->m_bAdmin)
	{
		m_pSecurityAdmin = AfxGetSecurityInterface()->OpenOslAdminDlgProtectDoc(this, pDoc);
		AddPage(m_pSecurityAdmin);
	}
}

//-----------------------------------------------------------------------------
CFormSheet::~CFormSheet()
{
	SAFE_DELETE(m_pFormReportDlg);
	SAFE_DELETE(m_pFormRadarDlg);
	SAFE_DELETE(m_pFormBarcodeDlg);
	SAFE_DELETE(m_pFormPropertiesDlg);
	SAFE_DELETE(m_pFormBodyDlg);
	SAFE_DELETE(m_pSecurityAdmin);
	SAFE_DELETE(m_pProfilesWizPropPage);
}

//-----------------------------------------------------------------------------
BOOL CFormSheet::OnInitDialog () 
{
    BOOL bResult = __super::OnInitDialog();
    
    // Hide Apply and Help buttons
    CWnd* pWnd = GetDlgItem (IDHELP);
    pWnd->ShowWindow (SW_HIDE);

	pWnd = GetDlgItem (ID_APPLY_NOW);
	pWnd->SetWindowText(_TB("Apply"));

	pWnd = GetDlgItem (IDCANCEL);
	pWnd->SetWindowText(_TB("Cancel"));
    
    return bResult;
}

//-----------------------------------------------------------------------
BOOL CFormSheet::SaveSheet(const CString& sUserForSave)
{
	m_FormManager.m_bTBFModified = m_bTBFModified;
	
	BOOL bSave = m_FormManager.SaveSheetModified(sUserForSave, &m_FormManagerSource);
	if (bSave)
	{
		m_bTBFModified = FALSE;
		m_bXMLModified = FALSE;
	}
	// dopo aver salvato riporto la modifica nella FormReportDialog
	//if (m_pFormReportDlg->m_FormReports.m_arReports.GetReports().GetSize() > m_FormManagerSource.m_Reports.m_arReports.GetReports().GetSize())
	//	m_pFormReportDlg->m_FormReports = m_FormManagerSource.m_Reports;

	return bSave;
}

//-----------------------------------------------------------------------

/////////////////////////////////////////////////////////////////////////////
CTBNamespace AfxGetDocumentDefaultReport(const CString& sDocumentNamespace)
{
	CTBNamespace ns(CTBNamespace::DOCUMENT, sDocumentNamespace );

	CReportManager aReportManager;
	aReportManager.Parse(ns);

	CFunctionDescription* pRep = aReportManager.GetDefaultReportDescription();
	return pRep ? pRep->GetNamespace() : CTBNamespace();
}

/////////////////////////////////////////////////////////////////////////////
CString AfxGetDocumentDefaultTitleReport(const CString& sDocumentNamespace)
{
	CTBNamespace ns(CTBNamespace::DOCUMENT, sDocumentNamespace);

	CReportManager aReportManager;
	aReportManager.Parse(ns);

	CFunctionDescription* pRep = aReportManager.GetDefaultReportDescription();
	return pRep ? pRep->GetTitle() : _T("");
}


/////////////////////////////////////////////////////////////////////////////
//							ReportMngDlg
///////////////////////////////////////////////////////////////////////////////
//

IMPLEMENT_DYNAMIC (ReportMngDlg, CLocalizablePropertySheet)
//-----------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(ReportMngDlg, CLocalizablePropertySheet)
	//{{AFX_MSG_MAP( ReportMngDlg )
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
ReportMngDlg::ReportMngDlg	(CString& strReportName, const CString& sDocumentNamespace)
	:
	CLocalizablePropertySheet	(_TB("Document report")),
	m_sReportName				(strReportName),
	m_pFormReportDlg			(NULL),
	m_RepManagerSource			(m_RepManager),
	m_bUpdated					(TRUE)
{
	m_Namespace.AutoCompleteNamespace(CTBNamespace::DOCUMENT, sDocumentNamespace, CTBNamespace());
	m_RepManager.Parse(m_Namespace);

	m_pFormReportDlg = new CFormReportDlg(this, m_RepManager);
	AddPage(m_pFormReportDlg);
}

//-----------------------------------------------------------------------------
ReportMngDlg::ReportMngDlg	(CString& strReportName, CAbstractFormDoc* pDoc) 
	:
	CLocalizablePropertySheet	(_TB("Document report")),
	m_sReportName				(strReportName),
	m_pFormReportDlg			(NULL),
	m_RepManagerSource			(pDoc->GetReportManager()),
	m_Namespace					(pDoc->GetNamespace()),
	m_bUpdated					(TRUE)
{
	m_RepManager = m_RepManagerSource;
    m_pFormReportDlg = new CFormReportDlg(this, /*m_RepManagerSourc*/m_RepManager);
	AddPage(m_pFormReportDlg);
}

//-----------------------------------------------------------------------------
BOOL ReportMngDlg::OnInitDialog () 
{
    BOOL bResult = __super::OnInitDialog();
    
    // Hide Apply and Help buttons
    CWnd* pWnd = GetDlgItem (IDHELP);
    pWnd->ShowWindow (SW_HIDE);

	pWnd = GetDlgItem (ID_APPLY_NOW);
	pWnd->SetWindowText(_TB("Apply"));

	pWnd = GetDlgItem (IDCANCEL);
	pWnd->SetWindowText(_TB("Cancel"));

	AddOnModule* pAddOnMod = AfxGetAddOnModule(m_Namespace);
    ASSERT(pAddOnMod);
	const CDocumentDescription* pDoc = AfxGetDocumentDescription(m_Namespace);
	if (!pDoc)
		return FALSE;

	CString sCaption = _TB("Document report") + pDoc->GetTitle();
	this->SetTitle(sCaption);
    
    return bResult;
}

//-----------------------------------------------------------------------------
ReportMngDlg::~ReportMngDlg()
{
	if (m_pFormReportDlg) delete m_pFormReportDlg;
}

//-----------------------------------------------------------------------------
BOOL ReportMngDlg::SaveSheet(const CString& sUserForSave, CReportManager* pRepMng)
{
	// faccio la stessa cosa di CFormManager::Unparse()	
	if (pRepMng->IsUserModified())
	{
		CXMLReportObjectsParser aParserUsr; 
		CXMLDocumentObject		aXMLDocUsr;

		BOOL bExistInOtherXML = FALSE;
		CDocumentReportDescription* pDefUsr = pRepMng->m_arShowReports.GetReportInfo(pRepMng->m_NsUsr);//m_Reports.m_arUserReports.GetDefault();
		CDocumentReportDescription* pDefault = pRepMng->m_arShowReports.GetDefault();
		if (pDefUsr)
		{
			CDocumentReportDescription* pRepAllUsr = pRepMng->m_arAllUsersReports.GetReportInfo(pDefUsr->GetNamespace());
			if (pRepAllUsr)
			{
				if (!pRepAllUsr->IsDefault())
					bExistInOtherXML = TRUE;
			}
			else
			{
				CDocumentReportDescription* pRepStd	= pRepMng->m_arStandardReports.GetReportInfo(pDefUsr->GetNamespace());
				if (pRepStd)
					bExistInOtherXML = TRUE;
			}
		}

		CString sFileName	= AfxGetPathFinder()->GetDocumentReportsFile(m_Namespace, CPathFinder::USERS, sUserForSave);
		CDocumentReportDescription* pDefDefined = pRepMng->m_arAllUsersReports.GetDefault();

		if (pDefDefined != NULL && pDefDefined->GetNamespace() == pDefault->GetNamespace())
			pDefault = NULL;
		if (!pRepMng->m_arUserReports.GetReports().IsEmpty() || pDefault)
		{
			aParserUsr.Unparse(&aXMLDocUsr, &pRepMng->m_arUserReports, pDefault);
			aXMLDocUsr.SaveXMLFile(sFileName, TRUE);
		}
		else
			DeleteFile(sFileName);	
		
		return TRUE;
	}
	return FALSE;
}
//=============================================================================
