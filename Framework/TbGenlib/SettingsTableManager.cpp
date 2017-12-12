#include "stdafx.h"

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\SettingsTable.h>

#include <TbParser\XmlSettingsParser.h>

#include "ParsObj.h"
#include "ParsBtn.h"
#include "Baseapp.h"
#include "Messages.h"

#include "SettingsTableManager.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif
//============================================================================
//					CCustomSaveInterface
//============================================================================

//-----------------------------------------------------------------------------
CCustomSaveInterface::CCustomSaveInterface()
	:
	m_bSaveAllFile(FALSE),
	m_bSaveAllUsers(FALSE),
	m_eSaveMode(COMPANY_USERS)
{
	m_aUsers.RemoveAll();
	m_aUsers.Add(AfxGetLoginInfos()->m_strUserName);
}


//============================================================================
//								CParameterInfo								 //
//============================================================================

IMPLEMENT_DYNAMIC(CParameterInfo, CObject)
//-----------------------------------------------------------------------------
CParameterInfo::CParameterInfo	(
									const CTBNamespace& aOwner, 
									const CString&		sFileName, 
									const CString&		sSection
								)
{
	m_bWrite	= FALSE;
	m_sFileName	= sFileName;
	m_Owner		= aOwner;
	m_sSection	= sSection;
}

//-----------------------------------------------------------------------------
BOOL CParameterInfo::WriteParameters()
{
	m_bWrite = TRUE;

	if (!m_Owner.IsValid() || m_sFileName.IsEmpty() || m_sSection.IsEmpty())
		return FALSE;

	AfxCreateSettingsSection (m_Owner, m_sFileName, m_sSection);
	BindParameters();
		
	return AfxSaveSettings (m_Owner, m_sFileName, m_sSection);
}

//-----------------------------------------------------------------------------
void CParameterInfo::ReadParameters()
{
	m_bWrite = FALSE;

	if (m_Owner.IsValid() && !m_sFileName.IsEmpty() && !m_sSection.IsEmpty())
		BindParameters();
}

//-----------------------------------------------------------------------------
void CParameterInfo::BindParam (const CString& sSetting, DataObj& aValue)
{
	ASSERT(!sSetting.IsEmpty() || !m_sSection.IsEmpty() || m_Owner.IsValid() || !m_sFileName.IsEmpty());

	if (m_bWrite)
		AfxSetSettingValue	(m_Owner, m_sSection, sSetting, aValue, m_sFileName);
	else
		aValue.Assign(*AfxGetSettingValue (m_Owner, m_sSection, sSetting, aValue, m_sFileName));
}

//-----------------------------------------------------------------------------
DataObj* CParameterInfo::GetSettingValue
	(
		const CString& sSetting,
		const DataObj& aDefault
	)
{
	return AfxGetSettingValue(m_Owner, m_sSection, sSetting, aDefault, m_sFileName);
}

//-----------------------------------------------------------------------------
void CParameterInfo::SetSettingValue
	(
		const CString& sSetting,
		const DataObj& aValue
	)
{
	AfxSetSettingValue	(m_Owner, m_sSection,  sSetting, aValue, m_sFileName); 
	AfxSaveSettings		(m_Owner, m_sFileName, m_sSection);
}

//============================================================================
//	General Functions
//============================================================================
//----------------------------------------------------------------------------------------------
BOOL SaveSettings	(
						const CTBNamespace&		aModule, 
						const CString&			sFileName, 
						const CString&			sSection, 
						const int&				nRelease, 
						CCustomSaveInterface*	pSaveInterface
					)

{
	if (!AfxGetPathFinder() || !pSaveInterface || !aModule.IsValid() || sFileName.IsEmpty())
		return FALSE;

	AfxGetApp()->BeginWaitCursor();

	// istanzio il parser e lo setto secondo le richieste
	XMLSettingsParser aParser;
	BOOL bAllUsers = pSaveInterface->m_bSaveAllUsers;

	switch (pSaveInterface->m_eSaveMode)
	{
	case CCustomSaveInterface::STANDARD:	
		aParser.m_Source = SettingObject::FROM_STANDARD; 
		break;
	case CCustomSaveInterface::ALLCOMPANY_USERS:
		if (!pSaveInterface->m_aUsers.GetSize())
			return FALSE;
		aParser.m_Source = bAllUsers ? SettingObject::FROM_ALLCOMPANYUSERS : SettingObject::FROM_ALLCOMPANYUSER;
		break;
	case CCustomSaveInterface::COMPANY_USERS:
		if (!pSaveInterface->m_aUsers.GetSize())
			return FALSE;
		aParser.m_Source = bAllUsers ? SettingObject::FROM_COMPANYUSERS : SettingObject::FROM_COMPANYUSER;
		break;
	}

	BOOL bOk = TRUE;
	CString sUser;
	CString aMyUser = AfxGetLoginInfos()->m_strUserName;

	// per ogni utente utilizzo una tabella parziale parallela 
	// che è rigenerata dai files aggiornati su disco dell'utente.
	SettingsTable aRefreshedFilesTable;

	for (int i=0; i <= pSaveInterface->m_aUsers.GetUpperBound(); i++)
	{
		sUser = pSaveInterface->m_aUsers.GetAt(i);

		// pulisce i soli settaggi per utente
		aRefreshedFilesTable.ClearSettingsOf (aModule, sFileName, sSection, FALSE, TRUE);

		// legge i files aggiornati (è ottimizzato per non rileggere quelli comuni)
		// se sto facendo il refresh di tutti devo comunque comprendere il mio utente corrente!
		if (!aParser.RefreshTable (AfxGetPathFinder(), aModule, sFileName, bAllUsers ? aMyUser : sUser, SettingsTablePtr(&aRefreshedFilesTable, FALSE)))
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Impossible read new version of parameters {0-%s} for user {1-%s}. "), aModule.ToUnparsedString() + _T(" : ") + sFileName, (LPCTSTR) sUser));
			continue;				
		}

		// copia i valori correnti ed i defaults
		aRefreshedFilesTable.CopyCurrentValuesFrom (AfxGetSettingsTable(), aModule, sFileName, sSection);

		// l'onere di verificare e ottimizzare la scrittura 
		// dei soli modificati è dell'unparser
		if (bOk && !aParser.Unparse
				(
					AfxGetPathFinder(), 
					aModule, sFileName,
					(pSaveInterface->m_bSaveAllFile ? _T("") : sSection), 
					nRelease, 
					sUser, 
					&aRefreshedFilesTable
				)
		   )
			bOk = FALSE;
	}
	
	// adesso faccio il refresh della mia tabella generale con i nuovi files
	AfxGetSettingsTable()->ClearSettingsOf (aModule, sFileName, sSection, TRUE, TRUE);

	if (!aParser.RefreshTable (AfxGetPathFinder(), aModule, sFileName, aMyUser, AfxGetSettingsTable(), FALSE))
		AfxGetDiagnostic()->Add(cwsprintf(_TB("Impossible update in memory version updated of Parameters {0-%s} for current user. Save failed."), aModule.ToUnparsedString() + _T(" : ") + sFileName));
	
	AfxGetApp()->EndWaitCursor();

	return bOk;
}


// consente di salvare una sezione di settaggi specifica
//-----------------------------------------------------------------------------
TB_EXPORT BOOL AFXAPI AfxSaveSettings	(
											const CTBNamespace& aNamespace, 
											const CString& sFileName,
											const CString& sSection, 
											const int& nRelease, /*0*/
											const BOOL bAskOnSave /*FALSE*/,
											CCustomSaveInterface* pInterface /*NULL*/
										)
{
	if (!aNamespace.IsValid() || sFileName.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BOOL bOk = TRUE;

	// se non mi viene passata ne genero una con i default
	CCustomSaveInterface* pSaveInterface = pInterface;
	if (!pSaveInterface)
		pSaveInterface = new CCustomSaveInterface();

	// finestra di richiesta salvataggio
	if (bAskOnSave && AfxGetApplicationContext()->GetCustomSaveDialogClass())
	{
		CObject* pObject = AfxGetApplicationContext()->GetCustomSaveDialogClass()->CreateObject();
		CCustomSaveDialogObj* pCustomSaveDialog = dynamic_cast<CCustomSaveDialogObj*>(pObject);
		if (pCustomSaveDialog)
		{
			pCustomSaveDialog->SetInterface(pSaveInterface, AfxGetMainWnd());
			pCustomSaveDialog->EnableAllCompanies(TRUE);

			if (pCustomSaveDialog->ShowDialog() == IDCANCEL)
				bOk = FALSE;

			delete pObject;
		}
		else
			ASSERT_TRACE(FALSE, "Cannot instantiate save setting dialog!! Cannot save settings!");
	}

	if (bOk && !SaveSettings (aNamespace, sFileName, sSection, nRelease, pSaveInterface))
	{
		// diagnostica di errore
		AfxGetDiagnostic()->Add(_TB("The previous errors occurred when saving settings."), CDiagnostic::Error);
		AfxGetDiagnostic()->Show();
		bOk = FALSE;
	}

	if (!pInterface && pSaveInterface)
		delete pSaveInterface;

	return bOk;
}

// consente di salvare un' intero files di settaggi
//-----------------------------------------------------------------------------
TB_EXPORT BOOL AfxSaveSettingsFile	(TbBaseSettings* pSettings,	const BOOL bAskOnSave)
{
	return AfxSaveSettingsFile(pSettings->GetOwner(), pSettings->GetFileName(), bAskOnSave);
}

// consente di salvare un' intero files di settaggi
//-----------------------------------------------------------------------------
TB_EXPORT BOOL AfxSaveSettingsFile	(
										const CTBNamespace& aNamespace, 
										const CString& sFileName,	
										const BOOL bAskOnSave,
										CCustomSaveInterface* pInterface /*NULL*/
									)
{
	if (!aNamespace.IsValid() || sFileName.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	// se non mi viene passata ne genero una con i default
	CCustomSaveInterface* pSaveInterface = pInterface;
	if (!pSaveInterface)
		pSaveInterface = new CCustomSaveInterface();

	pSaveInterface->m_bSaveAllFile = TRUE; 

	BOOL bOk = AfxSaveSettings (aNamespace, sFileName, _T(""), 0, bAskOnSave, pSaveInterface);
	
	if (!pInterface && pSaveInterface)
		delete pSaveInterface;

	return bOk;
}
