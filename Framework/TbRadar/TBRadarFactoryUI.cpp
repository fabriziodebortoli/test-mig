#include "StdAfx.h"

//#include <TBNameSolver\ApplicationContext.h>

#include <TbGeneric\DataObj.h>
//#include <TbGeneric\globals.h>
//#include <TbGeneric\ParametersSections.h>

//#include <TbGenlib\parsobj.h>
//#include <TbGenlib\baseapp.h>
//#include <TbGenlib\oslbaseinterface.h>
//#include <TbGenlib\TbCommandInterface.h>

//#include <TbWebServicesWrappers\LoginManagerInterface.h>
//
//#include <TbGenlibUI\SettingsTableManager.h>

//#include <TbOledb\oledbmng.h>
//#include <TbOledb\sqlaccessor.h>				
//#include <TbOledb\sqlrec.h>
//#include <TbOledb\sqltable.h>		

#include <TbGeneric\ReferenceObjectsInfo.h>

//#include <TbWoormEngine\QueryObject.h>	
//#include <TbWoormEngine\inputmng.h>		

#include <TbGes\TbRadarInterface.h>
#include <TbGes\hotlink.h>
//#include <TbGes\extdoc.h>

#include "TBRadarFactoryUI.h"


//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBRadarFactoryUI, CTBRadarFactory)

//-----------------------------------------------------------------------------
ITBRadar* CTBRadarFactoryUI::CreateInstance(HotKeyLink* pHkl, SqlTable* pT, SqlRecord* pR, HotKeyLinkObj::SelectionType nQuerySelection)
{
	ITBRadar* pIRadarDoc = NULL;
	// build a new radar object and attach related data
	
	if (pHkl->IsKindOf(RUNTIME_CLASS(DynamicHotKeyLink)))
	{
		CHotlinkDescription::CSelectionMode* pMode = pHkl->GetHotlinkDescription()->GetSelectionMode((CHotlinkDescription::ESelectionType) nQuerySelection, pHkl->GetCustomSearch());
		if (!pMode)
		{
			ASSERT(FALSE);
			return NULL;
		}
		if (pMode->m_eMode == CHotlinkDescription::REPORT)
		{
			CWrmRadarDoc* pRadarDoc = (CWrmRadarDoc*) AfxGetTbCmdManager()->RunWoormReport(pMode->m_sBody, NULL, TRUE, FALSE);
			pIRadarDoc = (ITBRadar*) pRadarDoc;
			if (pIRadarDoc)
			{
				pRadarDoc->Attach(pHkl, pT, pR);

				CString sAuxQuery;
				pRadarDoc->Customize(nQuerySelection, sAuxQuery);
				pRadarDoc->Run (sAuxQuery);
			}
			else 
			{
				ASSERT(FALSE);
				return NULL;
			}
			return pIRadarDoc;
		}
	}

	const CSingleExtDocTemplate* pRadarTemplate = AfxGetTemplate(RUNTIME_CLASS(CRadarView), 0);
	if (pRadarTemplate && pT)
	{
		pIRadarDoc = (CRadarDoc*) AfxOpenDocumentOnCurrentThread(pRadarTemplate, NULL);
		if (pIRadarDoc)
			pIRadarDoc->Attach(pHkl, pT, pR);
	}
	return pIRadarDoc;
}

//-----------------------------------------------------------------------------
ITBRadar* CTBRadarFactoryUI::CreateInstance(CAbstractFormDoc* pDoc)
{
	CRadarDoc* pRadarDoc = NULL;
	// build a new radar object and attach related data
	const CSingleExtDocTemplate* pRadarTemplate = AfxGetTemplate(RUNTIME_CLASS(CRadarView), 0);
	if (pRadarTemplate)
	{
		pRadarDoc = (CRadarDoc*)  AfxOpenDocumentOnCurrentThread(pRadarTemplate, NULL);
		if (pRadarDoc)
		{
			pRadarDoc->Attach (pDoc);
		}
	}
	return pRadarDoc;
}
//-----------------------------------------------------------------------------
ITBRadar* CTBRadarFactoryUI::CreateInstance(CAbstractFormDoc* pDoc, const CString& sReportName, BOOL bTemporary)
{
	CWrmRadarDoc* pRadar = NULL;
	if (bTemporary)
	{
		CWoormInfo*	pWoormInfo = new CWoormInfo(DataStr(sReportName));
		pWoormInfo->m_bOwnedByReport = TRUE;	
		pWoormInfo->m_bIsReportString = TRUE;	

		pRadar = (CWrmRadarDoc*) AfxGetTbCmdManager()->RunWoormReport(pWoormInfo, pDoc, NULL, TRUE);
	}
	else
		pRadar = (CWrmRadarDoc*) AfxGetTbCmdManager()->RunWoormReport(sReportName, pDoc, TRUE);

	if (pRadar)
	{
		// associa il documento chiamante per l'eventuale risoluzione di funzioni
		// di documento
		pRadar->Attach(pDoc, bTemporary);
	}
	return pRadar;
}
//-----------------------------------------------------------------------------
CString CTBRadarFactoryUI::BuildWoormRadar(CAbstractFormDoc* pDoc) 
{
	CWrmMaker wrmMaker(pDoc);

	if (wrmMaker.BuildWoorm(TRUE)) 
		return wrmMaker.m_strReportName;
	return _T("");
}
