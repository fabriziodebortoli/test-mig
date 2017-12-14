#include "stdafx.h"
#ifdef new
#undef new
#endif

#include <stdio.h>
#include <string.h>
#include <direct.h>
#include <locale.h> 
#include <atlenc.h> 

#include <TbNameSolver\chars.h>

#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\Array.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\LineFile.h>
#include <TbGenlib\OslInfo.h>
#include <TbGenlib\OslBaseInterface.h>
#include <TbGenlib\Messages.h>
#include <TbGenlib\SettingsTableManager.h>

#include <TbGenlibUI\FontsDialog.h>
#include <TbGenlibManaged\PostaLiteNet.h>

#include <TbClientCore\ClientObjects.h>

#include <TbOleDb\sqltable.h>

//#include <TBApplicationWrapper\Utility.h>

#include "CMailConnector.h"

#include "PostaLiteSettings.h"
#include "PostaLiteSubscribeDlg.h"
#include "PostaLiteTables.h"
#include "SendPostaLiteDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW

#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//=============================================================================
BOOL CMailConnector::IsPostaLiteEnabled()
{ 
	if (!AfxIsActivated(TBEXT_APP, POSTALITE_ACT))
		return FALSE;

	if (AfxGetLoginInfos()->m_strDatabaseType.Left(6).CompareNoCase(L"ORACLE") == 0)
		return FALSE;

	PostaLiteSettings ps;

	return ps.m_Enabled; 
}

//-----------------------------------------------------------------------------
BOOL CMailConnector::RotateLandscape()
{ 
	PostaLiteSettings ps;

	return ps.m_RotateLandscape; 
}

//-----------------------------------------------------------------------------
BOOL CMailConnector::PostaLitePdfMargins(CRect& rect)
{
	PostaLiteSettings ps;

	rect.left	= ps.m_MarginLeft;
	rect.right	= ps.m_MarginRight;
	rect.top	= ps.m_MarginTop;
	rect.bottom = ps.m_MarginBottom;

	return TRUE; 
}

//-----------------------------------------------------------------------------
DataEnum CMailConnector::GetPostaLiteDefaultDeliveryType ()
{ 
	PostaLiteSettings ps;

	return DataEnum(E_POSTALITE_DELIVERY_TYPE_DEFAULT); 
}

DataEnum CMailConnector::GetPostaLiteDefaultPrintType () 
{ 
	PostaLiteSettings ps;

	return DataEnum(E_POSTALITE_PRINT_TYPE_DEFAULT); 
}

//-----------------------------------------------------------------------------
BOOL CMailConnector::ShowSendPostaLiteDlg	
		(
			CDocument* pCallerDoc,
			CMapiMessage& msg
		)
{
	CSendPostaLiteDlg dlg
		(
			pCallerDoc,
			msg
		);

	return dlg.DoModal() == IDOK;
}

//-----------------------------------------------------------------------------
#define BT_ERROR 3
#define BT_WARNING 2
#define BT_SUCCESS 1



CString EscapeXML(CString s)
{
/*
	int EscapeXML( 
	   const wchar_t * szIn, 
	   int nSrcLen, 
	   wchar_t * szEsc, 
	   int nDestLen, 
	   DWORD dwFlags = ATL_ESC_FLAG_NONE  
	)
*/
	DWORD lenMax = s.GetLength() * 2 + 1;
	TCHAR* sz = new TCHAR[lenMax];

	int len = ATL::EscapeXML((LPCTSTR) s, s.GetLength(), sz, lenMax);
	if (len > 0)
		return CString(sz).Left(len);

	return s;
}

void CMailConnector::PostaLiteSentNotify (CMapiMessage* pMsg, CPostaLiteAddress* pAddr, CString sDescription, int eIcon)
{
	DataDate dtExpire;
	dtExpire.SetTodayDate();
	dtExpire += 1;

	int closingTimer = eIcon != BT_SUCCESS ? 20000 : 5000;
	BOOL bHistoricize = eIcon != BT_SUCCESS;

	if (eIcon == BT_ERROR)
	{
		sDescription = _TB("The document cannot be send") + ':' + sDescription;
		dtExpire += 14;
	}
	else if (eIcon == BT_WARNING)
		dtExpire += 2;

	const CLoginInfos* infos = AfxGetLoginContext()->GetLoginInfos();
	CStringArray arRecipients; arRecipients.Add(infos->m_strUserName);

	CString sBody (
			CString(L"<html><body>") + 
			L"<a>" + (eIcon == BT_ERROR ? L"<b>" : L"") + _TB("The document") + L" <i>" + EscapeXML('\"' + pMsg->m_sSubject + '\"') + L"</i>" + + (eIcon == BT_ERROR ? L"</b>" : L"") + L"</a>" +
			L"<br><a>" + EscapeXML(sDescription) + L"</a>"
		);
		
		if (
			!pMsg->m_PostaLiteMsg.m_sDocNamespace.IsEmpty() &&
			!pMsg->m_PostaLiteMsg.m_sDocPrimaryKey.IsEmpty() &&
			pMsg->m_PostaLiteMsg.m_sDocPrimaryKey.Find('\\') < 0
			)
		{
			sBody +=  L"<br><a href=\"";
			sBody +=  GetTBNavigateUrl(pMsg->m_PostaLiteMsg.m_sDocNamespace, pMsg->m_PostaLiteMsg.m_sDocPrimaryKey);
			sBody +=  L"\">" + _TB("document") + L"</a>";
		}

		if (
			!pMsg->m_PostaLiteMsg.m_sAddresseeNamespace.IsEmpty() &&
			!pMsg->m_PostaLiteMsg.m_sAddresseePrimaryKey.IsEmpty() &&
			pMsg->m_PostaLiteMsg.m_sAddresseePrimaryKey.Find('\\') < 0
			)
		{
			sBody += L"<br><a href=\"";
			sBody +=  GetTBNavigateUrl(pMsg->m_PostaLiteMsg.m_sAddresseeNamespace, pMsg->m_PostaLiteMsg.m_sAddresseePrimaryKey);
			sBody += L"\">" + _TB("addressee") + L"</a>";
		}
		
		sBody += L"</body></html>";
	//----

	AfxGetLoginManager()->AdvancedSendBalloon
		(
			sBody,	
			dtExpire,
			CLoginManagerInterface::bt_PostaLite,
			arRecipients,
			(
				eIcon == BT_SUCCESS ? 
				CLoginManagerInterface::bs_Information : 
				(eIcon == BT_WARNING ? CLoginManagerInterface::bs_Warning : CLoginManagerInterface::bs_Error)),
			bHistoricize,
			TRUE,
			closingTimer
		);

	::PostMessage(AfxGetMenuWindowHandle(), UM_IMMEDIATE_BALLOON, NULL, NULL); //CUtility::ShowImmediateBalloon(); non compila
}

//-----------------------------------------------------------------------------
BOOL CMailConnector::PostaLiteCheckCountry(CMapiMessage* pMsg, CPostaLiteAddress* pAddrs)
{
	PostaLiteSettings ps;

	if (!pAddrs->m_sISOCode.IsEmpty())
	{
		pAddrs->m_sCountry = CPostaLiteNet::GetPostaliteCountryFromIsoCode(pAddrs->m_sISOCode);
	}

	if (pAddrs->m_sCountry.IsEmpty())
	{
		pAddrs->m_sCountry = ps.m_DefaultCountry.IsEmpty() ? L"Italia" : ps.m_DefaultCountry;

		PostaLiteSentNotify(pMsg, pAddrs, _TB("Empty country is not allowed: default value from setting will be used"), BT_WARNING);
	}

	if (
		!pAddrs->m_sFax.IsEmpty() &&
		!pAddrs->m_sISOCode.IsEmpty() &&
		pAddrs->m_sFax[0] != '+' && pAddrs->m_sFax.Left(2) != L"00" &&
		pAddrs->m_sCountry.CompareNoCase(L"Italia")
		)
	{
		FailedInvokeCode aFailedCode;
		CTBNamespace ns(_NS_HKL("HotKeyLink.Erp.Company.Dbl.ISOCountryCodes"));
		HotKeyLinkObj* pHKL = AfxGetTbCmdManager()->RunHotlink (ns, &aFailedCode);
		if (pHKL)
		{
			DataStr dsISO (pAddrs->m_sISOCode);
			if (pHKL->DoFindRecord(&dsISO))
			{
				DataObj* pTelephonePrefix = pHKL->GetField(_NS_FLD("TelephonePrefix"));
				if (pTelephonePrefix)
				{
					CString s = ((DataStr*)pTelephonePrefix)->GetString();
					if (!s.IsEmpty())
					{
						if (s[0] != '+' && s.Left(2) != L"00")
							s = '+' + s;
						pAddrs->m_sFax =  s + pAddrs->m_sFax;
					}
				}
			}
			delete pHKL;
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CMailConnector::PostaLiteEnqueueMsg(CMapiMessage* pMsg, SqlSession* pSql, CDiagnostic* pDiagnostic /*= NULL*/)
{ 
	ASSERT(pSql);

	if (!AfxIsActivated(TBEXT_APP,  MAILCONNECTOR_ACT))
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(_TB("MailConnector-PostaLite is not activated")));						
		return FALSE;
	}
	if (!IsPostaLiteEnabled())
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(_TB("PostaLite is not enabled")));						
		return FALSE;
	}
	if (pMsg->m_PostaLiteMsg.GetSize() < 1)
	{
		if (pDiagnostic)
			pDiagnostic->Add(CString(_TB("There are not PostaLite messages")));						

		return FALSE;
	}
	if (pMsg->m_PostaLiteMsg.m_nFilePages == 0)
	{
		PostaLiteSentNotify(pMsg, (CPostaLiteAddress*) pMsg->m_PostaLiteMsg.GetAt(0), _TB("Wrong pdf"), BT_ERROR);
		return FALSE;	 							
	}

	//----
	CString sFile = pMsg->m_PostaLiteMsg.m_sFileName;
	CString sName = pMsg->m_sAttachmentReportName;
	if (sName.IsEmpty())
	{
		sName =	 ::GetNameWithExtension (sFile);
		int nIdx = sName.Find(_T("##"));
		if (nIdx > 0)
			sName = sName.Left(nIdx) + '.' + FileExtension::PDF_EXT();
	}
	
	int nDocSent = 0;
	TMsgQueue	aRec;
	SqlTable	aTbl(&aRec, pSql);
	
	for (int i = 0; i < pMsg->m_PostaLiteMsg.GetSize(); i++)
	{
		CPostaLiteAddress* pAddr = pMsg->m_PostaLiteMsg[i];
		
		//---- Validazioni
		if (pAddr->m_sAddressee.IsEmpty())
		{
			PostaLiteSentNotify(pMsg, pAddr, _TB("Addressee is empty"), BT_ERROR);
			continue;	 
		}

		if (pAddr->m_deDeliveryType == E_POSTALITE_DELIVERY_TYPE_FAX)
		{
			if (pAddr->m_sFax.IsEmpty())
			{
				PostaLiteSentNotify(pMsg, pAddr, _TB("Fax number is empty"), BT_ERROR);
				continue;	 
			}
		}
		else
		{					
			if (
				pAddr->m_sAddress.IsEmpty() ||
				pAddr->m_sCity.IsEmpty() ||
				pAddr->m_sCounty.IsEmpty() ||
				//pAddr->m_sCountry.IsEmpty() ||
				pAddr->m_sZipCode.IsEmpty() 
			)
			{
				PostaLiteSentNotify(pMsg, pAddr, _TB("Address is incomplete"), BT_ERROR);
				continue;	 
			}
		}

		if (pAddr->m_deDeliveryType == E_POSTALITE_DELIVERY_TYPE_FAX)
		{
			if (pMsg->m_PostaLiteMsg.m_nFilePages > 49)
			{
				PostaLiteSentNotify(pMsg, pAddr, _TB("Too many pages"), BT_ERROR);
				continue;	 
			}
		}

		if (pAddr->m_deDeliveryType != E_POSTALITE_DELIVERY_TYPE_FAX)
		{
			if (pMsg->m_PostaLiteMsg.m_nFilePages > 98)
			{
				PostaLiteSentNotify(pMsg, pAddr, _TB("Too many pages"), BT_ERROR);
				continue;	 
			}

			if (
					pMsg->m_PostaLiteMsg.m_nFilePages > 49 &&
					(
						pAddr->m_dePrintType == E_POSTALITE_PRINT_TYPE_FRONT_BW 
						||
						pAddr->m_dePrintType == E_POSTALITE_PRINT_TYPE_FRONT_COLOR 
					)
				)
			{
				if (pMsg->m_PostaLiteMsg.m_nFilePages < 99)
				{
					if (pAddr->m_dePrintType == E_POSTALITE_PRINT_TYPE_FRONT_BW)
						pAddr->m_dePrintType = E_POSTALITE_PRINT_TYPE_FRONTBACK_BW;
					else if (pAddr->m_dePrintType == E_POSTALITE_PRINT_TYPE_FRONT_COLOR)
						pAddr->m_dePrintType = E_POSTALITE_PRINT_TYPE_FRONTBACK_COLOR;

					PostaLiteSentNotify(pMsg, pAddr, _TB("Too many pages: print type was chanded to front-back"), BT_WARNING);
				}
				else
				{
					PostaLiteSentNotify(pMsg, pAddr, _TB("Too many pages"), BT_ERROR);
					continue;	 
				}
			}
		}		

		if (!PostaLiteCheckCountry(pMsg, pAddr))
		{
			PostaLiteSentNotify(pMsg, pAddr, _TB("Invalid country"), BT_ERROR);
			continue;	 				
		}

		if (
				pAddr->m_deDeliveryType == E_POSTALITE_DELIVERY_TYPE_POSTA_MASSIVA
				&&
				pAddr->m_sCountry.CompareNoCase(_T("Italia"))
			)
		{
			pAddr->m_deDeliveryType == E_POSTALITE_DELIVERY_TYPE_POSTA_PRIORITARIA;

			PostaLiteSentNotify(pMsg, pAddr, _TB("'Posta Massiva' delivery type was not allowed outside Italy: delivery type was changed to 'Posta Prioritaria'"), BT_WARNING);
		}

		if (pAddr->m_deDeliveryType == E_POSTALITE_DELIVERY_TYPE_FAX)
		{
			if (pAddr->m_sFax[0] == '+')
				pAddr->m_sFax = _T("00") + pAddr->m_sFax.Mid(1);

			pAddr->m_dePrintType = E_POSTALITE_PRINT_TYPE_FRONT_BW;

			if (pAddr->m_sCountry.CompareNoCase(_T("Italia")) && pAddr->m_sFax.Left(2) != _T("00"))
			{
				PostaLiteSentNotify(pMsg, pAddr, _TB("Outside Italy Fax number should be prefixed"), BT_ERROR);
				continue;	 
			}
		}

		TRY
		{
			aRec.f_MsgID = 0;

			aTbl.Open(TRUE);	
			aTbl.SetAutocommit();
			aTbl.SelectAll();
			aTbl.Query();

			aTbl.AddNew();
			//--------------

				aRec.f_Fax			= pAddr->m_sFax;
				aRec.f_Addressee	= pAddr->m_sAddressee;
				aRec.f_Address		= pAddr->m_sAddress;
				aRec.f_ZipCode		= pAddr->m_sZipCode;
				aRec.f_City			= pAddr->m_sCity;
				aRec.f_County		= pAddr->m_sCounty;
				aRec.f_Country		= pAddr->m_sCountry;
				
				aRec.f_DeliveryType	= CPostaLiteAddress::ConvertDeliveryType(pAddr->m_deDeliveryType);
				if (pAddr->m_deDeliveryType == E_POSTALITE_DELIVERY_TYPE_FAX)
					aRec.f_PrintType	= CPostaLiteAddress::Front_BlackWhite;
				else
					aRec.f_PrintType	= CPostaLiteAddress::ConvertPrintType(pAddr->m_dePrintType);

				if (pMsg->m_sSubject.IsEmpty())
					aRec.f_Subject		= sName;
				else 
					aRec.f_Subject		= pMsg->m_sSubject;
				
				aRec.f_DocNamespace			= pMsg->m_PostaLiteMsg.m_sDocNamespace;
				aRec.f_DocPrimaryKey		= pMsg->m_PostaLiteMsg.m_sDocPrimaryKey;

				aRec.f_AddresseeNamespace	= pMsg->m_PostaLiteMsg.m_sAddresseeNamespace;
				aRec.f_AddresseePrimaryKey	= pMsg->m_PostaLiteMsg.m_sAddresseePrimaryKey;
				if (!pAddr->m_sAuxKey.IsEmpty()) 
					aRec.f_AddresseePrimaryKey	+= pAddr->m_sAuxKey;

				aRec.f_DocFileName			= sName;
				aRec.f_DocPages				= pMsg->m_PostaLiteMsg.m_nFilePages;
				aRec.f_DocSize				= pMsg->m_PostaLiteMsg.m_nFileSize;		

			//-------------
			aTbl.Update();
			aTbl.Close();
			
			nDocSent++;

			PostaLiteSentNotify(pMsg, pAddr, _TB("The document was prepared for the send"), BT_SUCCESS);
		}
		CATCH(SqlException, e)
		{           
			TraceError	(
							cwsprintf	(
											_TB("Update table %s: %s\n"),
											(LPCTSTR)TMsgQueue::GetStaticName(),
											(LPCTSTR)e->m_strError
										)
						);
			if (aTbl.IsOpen()) 
				aTbl.Close();

			PostaLiteSentNotify(pMsg, pAddr, _TB("Error on prepare the document"), BT_ERROR);

			return FALSE;
		}
		END_CATCH

		if (aRec.f_MsgID == 0)
			return FALSE;	//non posso salvare il pdf

		BOOL bOk = CPostaLiteNet::SavePdfBlob(AfxGetLoginInfos()->m_strNonProviderCompanyConnectionString, sFile, (long)aRec.f_MsgID, pDiagnostic);
		if (!bOk)
		{
			//if (pDiagnostic)
			//	pDiagnostic->Add(CString(_TB("PostaLite ...")));	

			PostaLiteSentNotify(pMsg, pAddr, _TB("Error on attach the document"), BT_ERROR);
			return FALSE;
		}
	}
	return nDocSent > 0; 
}

//=============================================================================

///<summary>
/// Subscribe PostaLite services
///</summary>
//[TBWebMethod(woorm_method=false, defaultsecurityroles="aMailConnector Parameter Manager")]
DataBool  PostaLiteSubscribe()
{
	if (!IsFunctionAllowed(_NS_WEB("Function.Extensions.TbMailer.TbMailer.PostaLiteSubscribe")))
	{
		return FALSE;
	}

	CPostaLiteSettingsDialog dlg;
	dlg.DoModal();

	return TRUE;
}

//----------------------------------------------------------------------------

///<summary>
/// Get PostaLite msg's document
///</summary>
//[TBWebMethod(defaultsecurityroles="aMailConnector Parameter Manager")]
DataBool  GetPostaLiteDocument(DataLng msgID, DataStr& tempFileName)
{
	CString sFile = ::GetTempName() + _T(".pdf");

	BOOL bOk = CPostaLiteNet::ReadPdfBlob(AfxGetLoginInfos()->m_strNonProviderCompanyConnectionString, sFile, (long)msgID);
	if (!bOk)
	{
		//if (pDiagnostic)
		//	pDiagnostic->Add(CString(_TB("PostaLite is not activated")));						

		return FALSE;
	}
	tempFileName = sFile;

	return TRUE;
}

//=============================================================================
