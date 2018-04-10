
#include "StdAfx.h"

#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\TBExplorerInterface.h>
#include <TbGenlib\generic.hjson> //JSON AUTOMATIC UPDATE
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbWoormViewer\woormdoc.h>
#include <TbWoormViewer\woormfrm.h>
#include <TbWoormViewer\woormvw.h>
#include <TbWoormViewer\WrmBlockModifier.h>

#include <TbWoormViewer\ListDlg.h>

#include ".\soapfunctions.h"

//----------------------------------------------------------------------------
///<summary>
///Pass-Key login
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool PassKeyLogin()
{
	if (!IsUserReportsDeveloper())
	{
		CSetPassKeyDlg dialog;
		if (dialog.DoModal() != IDOK)
		{
			if (!AfxGetBaseApp()->CanUseReportEditor())
				AfxMessageBox(_TB("This functionality is not licensed"));
			else
				AfxMessageBox(_TB("Warning, this option is available only for Administrators, Easybuilder Developers or specific users authorized by Security Module"));
			return FALSE;
		}
	}
	return TRUE;
}

//----------------------------------------------------------------------------
///<summary>
///Allow to generate a new report
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool ExecNewReport (DataLng& docHandle)
{
	if (!IsUserReportsDeveloper())
	{
		CSetPassKeyDlg dialog;
		if (dialog.DoModal() != IDOK)
		{
			if (!AfxGetBaseApp()->CanUseReportEditor())
				AfxMessageBox(_TB("This functionality is not licensed"));
			else
				AfxMessageBox(_TB("Warning, this option is available only for Administrators, Easybuilder Developers or specific users authorized by Security Module"));
			return FALSE;
		}
	}

	CBaseDocument* pDoc = AfxGetTbCmdManager()->RunDocument(szWoormNamespace, szDefaultViewMode, TRUE);
	ASSERT(pDoc);
	if (pDoc)
		docHandle = (long)pDoc;

	return pDoc != NULL;
}

//-----------------------------------------------------------------------------
///<summary>
///Allow to open an existing report
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool ExecOpenReport (DataStr/*[ciString]*/ reportNameSpace, DataLng& docHandle)
{
	if (!IsUserReportsDeveloper())
	{
		CSetPassKeyDlg dialog;
		if (dialog.DoModal() != IDOK)
		{
			if (!AfxGetBaseApp()->CanUseReportEditor())
				AfxMessageBox(_TB("This functionality is not licensed"));
			else
				AfxMessageBox(_TB("Warning, this option is available only for Administrators, Easybuilder Developers or specific users authorized by Security Module"));
			return FALSE;
		}
	}
	
	//------------------------------------
	CWoormDoc* pWDoc = NULL;
	ITBExplorer* pExplorer = NULL;
	
	if (reportNameSpace.IsEmpty())
	{
		CTBNamespace aNameSpace;

		pExplorer = AfxCreateTBExplorer(ITBExplorer::OPEN, aNameSpace);

		if (!pExplorer->Open())
		{
			delete pExplorer;
			return FALSE;
		}
		
		CStringArray aSelectedPaths;
		pExplorer->GetSelPathElements(&aSelectedPaths);

		// non è gestita la multiapertura dei report, quindi verrà aperto solo il primo!
		reportNameSpace = aSelectedPaths.GetAt(0);
	}

	if (reportNameSpace.IsEmpty())
		return FALSE;

	pWDoc = AfxGetTbCmdManager()->RunWoormReport(reportNameSpace.GetString(), NULL, FALSE, FALSE);
	
	docHandle = (long)pWDoc;

	if (pExplorer)
		delete pExplorer;


	return pWDoc != NULL;
}

//-----------------------------------------------------------------------------
///<summary>
///Allow to check syntax reports
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool ExecUpgradeReport ()
{
	if (!AfxGetLoginInfos()->m_bAdmin)
		return FALSE;
	CWrmBlockModifierDlg dlg;
	dlg.DoModal();
	return TRUE;
}

//-----------------------------------------------------------------------------
///<summary>
///Allow user to develop reports
///</summary>
//[TBWebMethod(woorm_method=false)]
DataBool IsUserReportsDeveloper ()
{
	if (AfxGetLoginContext()->IsLocked())
		return FALSE;

	if (AfxGetApplicationContext()->IsPassKeyActive())
		return TRUE;

	if (!AfxGetBaseApp()->CanUseReportEditor())
		return FALSE;

	//if (!AfxIsCalAvailable(_T("Framework"), _NS_ACT("ReportEditor")))
	//	return FALSE;

	if (AfxGetLoginInfos()->m_bAdmin || AfxGetLoginInfos()->m_bEasyBuilderDeveloper)
	{
		return AfxIsCalAvailable(TBNET_APP, REPORTEDITOR_ACT);
	}

	if (!AfxGetSecurityInterface()->IsSecurityEnabled())
		return FALSE;

	BOOL b = FALSE;

	CTBNamespace ns(CTBNamespace::FUNCTION, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.IsUserReportsDeveloper"));
	CInfoOSL infoOSL(ns, OSLType_Function);
	AfxGetSecurityInterface()->GetObjectGrant (&infoOSL);
	if (OSL_IS_PROTECTED(&infoOSL) ? OSL_CAN_DO(&infoOSL, OSL_GRANT_EXECUTE) : FALSE)
		b = TRUE;

	CTBNamespace ns2(CTBNamespace::FUNCTION, _NS_WEB("Framework.TbWoormViewer.TbWoormViewer.ExecNewReport"));
	CInfoOSL infoOSL2(ns2, OSLType_Function);
	AfxGetSecurityInterface()->GetObjectGrant (&infoOSL2);
	if (OSL_IS_PROTECTED(&infoOSL2) ? OSL_CAN_DO(&infoOSL2, OSL_GRANT_EXECUTE) : FALSE)
		b = TRUE;

	if (b)
		return AfxIsCalAvailable(TBNET_APP, REPORTEDITOR_ACT);

	return FALSE;
}

//-----------------------------------------------------------------------------
///<summary>
///Get  Company Info by Tag
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=true)]
DataStr GetCompanyInfo(DataStr/*[ciString]*/ tagProperty)
{
	DataStr ds = AfxGetLoginContext()->GetCompanyInfo(tagProperty);
	return ds;
}

//-----------------------------------------------------------------------------    

///<summary>
///It retrives ownerid's namespace web-methods list
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool GetDocumentMethods(DataLng handleDoc, DataArray& /*string*/ arMethods)
{
	if (handleDoc == 0)
		return DataBool(FALSE);
	CBaseDocument* pDoc = (CBaseDocument*)(long)handleDoc;
	if (!pDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
		return DataBool(FALSE);

	return DataBool(pDoc->GetMethods(arMethods));
}

//-----------------------------------------------------------------------------    
///<summary>
///It retrives PostaLite enable state
///</summary>
//[TBWebMethod(securityhidden=true)]
DataBool IsPostaLiteEnabled ()
{
	return AfxGetIMailConnector()->IsPostaLiteEnabled ();
}

//-----------------------------------------------------------------------------
///<summary>
/// Decode PostaLite DeliveryType
///</summary>
//[TBWebMethod(securityhidden=true)]
DataStr PostaLiteDecodeDeliveryType(DataInt deliveryType)
{
	CPostaLiteAddress::Delivery eDT = (CPostaLiteAddress::Delivery) (int) deliveryType;
	switch (eDT)
	{
		case CPostaLiteAddress::PostaMassiva : 
			return AfxGetEnumsTable()->GetEnumItemTitle(E_POSTALITE_DELIVERY_TYPE_POSTA_MASSIVA);
			//return POSTALITE_ENUM_FIELD(CONTEXT_MENU_MESSAGE_DELIVERY_TYPE_MASSIVA);
		case CPostaLiteAddress::PostaPrioritaria : 
			return AfxGetEnumsTable()->GetEnumItemTitle(E_POSTALITE_DELIVERY_TYPE_POSTA_PRIORITARIA);
			//return POSTALITE_ENUM_FIELD(CONTEXT_MENU_MESSAGE_DELIVERY_TYPE_PRIORITARIA);
		case CPostaLiteAddress::PostaRaccomandata : 
			return AfxGetEnumsTable()->GetEnumItemTitle(E_POSTALITE_DELIVERY_TYPE_RACCOMANDATA);
			//return POSTALITE_ENUM_FIELD(CONTEXT_MENU_MESSAGE_DELIVERY_TYPE_RACCOMANDATA);
		case CPostaLiteAddress::PostaRaccomandataAR :    
			return AfxGetEnumsTable()->GetEnumItemTitle(E_POSTALITE_DELIVERY_TYPE_RACCOMANDATA_AR);
			//return POSTALITE_ENUM_FIELD(CONTEXT_MENU_MESSAGE_DELIVERY_TYPE_RACCOMANDATA_AR);
		//case CPostaLiteAddress::Fax :    
		//	return AfxGetEnumsTable()->GetEnumItemTitle(E_POSTALITE_DELIVERY_TYPE_FAX);
			//return POSTALITE_ENUM_FIELD(CONTEXT_MENU_MESSAGE_DELIVERY_TYPE_FAX);
		case 4:
			return _TB("Delivery by FAX was not supported");
	}
	return DataStr();
}

//-----------------------------------------------------------------------------
///<summary>
/// Decode PostaLite Print Type
///</summary>
//[TBWebMethod(securityhidden=true)]
DataStr PostaLiteDecodePrintType(DataInt printType)
{
	CPostaLiteAddress::Print ePT = (CPostaLiteAddress::Print) (int) printType;
	switch(ePT)
	{
		case CPostaLiteAddress::Front_BlackWhite : 
			return AfxGetEnumsTable()->GetEnumItemTitle(E_POSTALITE_PRINT_TYPE_FRONT_BW);
				//POSTALITE_ENUM_FIELD(CONTEXT_MENU_MESSAGE_PRINT_TYPE_BIANCONEROFRONTE);
		case CPostaLiteAddress::FrontBack_BlackWhite : 
			return AfxGetEnumsTable()->GetEnumItemTitle(E_POSTALITE_PRINT_TYPE_FRONTBACK_BW);
			//return POSTALITE_ENUM_FIELD(CONTEXT_MENU_MESSAGE_PRINT_TYPE_BIANCONEROFRONTERETRO);
		case CPostaLiteAddress::Front_Color : 
			return AfxGetEnumsTable()->GetEnumItemTitle(E_POSTALITE_PRINT_TYPE_FRONT_COLOR);
			//return POSTALITE_ENUM_FIELD(CONTEXT_MENU_MESSAGE_PRINT_TYPE_COLOREFRONTE);
		case CPostaLiteAddress::FrontBack_Color : 
			return AfxGetEnumsTable()->GetEnumItemTitle(E_POSTALITE_PRINT_TYPE_FRONTBACK_COLOR);
			//return POSTALITE_ENUM_FIELD(CONTEXT_MENU_MESSAGE_PRINT_TYPE_COLOREFRONTERETRO);
	}
	return  DataStr();
}

//-----------------------------------------------------------------------------
///<summary>
/// Decode PostaLite envelope status
///</summary>
//[TBWebMethod(securityhidden=true)]
DataStr PostaLiteDecodeStatus(DataInt status)
{
	switch((int)status)
	{
		case 0: 
			return DataStr(_TB("Waiting for upload"));
		case 1: 
			return DataStr(_TB("Envelope uploaded to PostaLite"));
		case 2: 
			return DataStr(_TB("Closed Envelope"));
		case 3: 
			return DataStr(_TB("Uploading envelope to PostaLite"));
		case 4: 
			return DataStr(_TB("Invalid"));
		default: 
			return DataStr(_TB("Unknown"));
	}
	return  DataStr();
}

//-----------------------------------------------------------------------------
///<summary>
/// Decode PostaLite user status
///</summary>
//[TBWebMethod(securityhidden=true)]
DataStr PostaLiteDecodeCodeState(DataInt codeState)
{
	switch((int)codeState)
	{
		case 1: 
			return DataStr(_TB("Enabled"));
		case 2: 
			return DataStr(_TB("Awaiting activation"));
		case -21: 
			return DataStr(_TB("Disabled"));
		default:
			return DataStr(_TB("Unknown"));
	}
}

//-----------------------------------------------------------------------------
///<summary>
/// Decode PostaLite document external status
///</summary>
//[TBWebMethod(securityhidden=true)]
DataStr PostaLiteDecodeStatusExt(DataLng status)
{
	switch((int)status)
	{
		case CPostaLiteAddress::PresoInCarico: //1
			return DataStr(_TB("Accepted: the envelope has been successfully uploaded to PostaLite"));
		case CPostaLiteAddress::Annullato: //2   
			return DataStr(_TB("Canceled: the envelope has been canceled by the user")); 
		case CPostaLiteAddress::InElaborazione: //3 
			return DataStr(_TB("In processing: the envelope has been delivered to the addressee")); 
		case CPostaLiteAddress::Errato: //4 
			return DataStr(_TB("Wrong: the envelope has not been sent"));
		case CPostaLiteAddress::Spedito: //5
			return DataStr(_TB("Sent: the envelope has been delivered to Postel"));
		case CPostaLiteAddress::SpeditoConInesitati: //6
			return DataStr(_TB("Sent with errors: the envelope has been partially delivered"));
		case CPostaLiteAddress::InStampa: //9
			return DataStr(_TB("Printing: the envelope has been validated by Postel and sent to print"));
		case CPostaLiteAddress::Sospeso: //11
			return DataStr(_TB("Suspended: the envelope has not been accepted yet due to a insufficient credit"));
		case CPostaLiteAddress::AnnullatoParzialmente: //12
			return DataStr(_TB("Partially Cancelled: the envelope has been canceled by the user after sending to the supplier"));
		case CPostaLiteAddress::None:
			break;
		default:
			return DataStr(_TB("Unknown code") + cwsprintf(_T(": %d"), (int)status)) ;
	}
	return  DataStr();
}

//-----------------------------------------------------------------------------
///<summary>
/// Decode PostaLite document's external error
///</summary>
//[TBWebMethod(securityhidden=true)]
DataStr PostaLiteDecodeErrorExt(DataLng error)
{
	switch((int)error)
	{
		case -1: 
			return DataStr(_TB("Generic Error")); //Errore generico
		case -2: 
			return DataStr(_TB("Invalid surname or company name")); //Cognome o Ragione sociale non valide
		case -3: 
			return DataStr(_TB("Invalid name")); //Nome non valido
		case -4: 
			return DataStr(_TB("Invalid address")); //Indirizzo non valido
		case -5: 
			return DataStr(_TB("Invalid city")); //Città non valida
		case -6: 
			return DataStr(_TB("Invalid zip code")); //Cap non valido
		case -7: 
			return DataStr(_TB("Invalid county")); //Provincia non valida
		case -8: 
			return DataStr(_TB("Invalid country"));//Nazione non valida
		case -9: 
			return DataStr(_TB("Invalid phone number")); //Telefono non valido
		case -10: 
			return DataStr(_TB("Invalid fax number")); //Fax non valido
		case -11: 
			return DataStr(_TB("Invalid fiscal code")); //Codice fiscale non valido
		case -12: 
			return DataStr(_TB("Invalid tax id number")); //Partita Iva non valida
		case -13: 
			return DataStr(_TB("Invalid gender")); //Sesso non valido
		case -14: 
			return DataStr(_TB("Invalid e-mail address")); //Email non valida
		case -15: 
			return DataStr(_TB("Invalid notes")); //Note non valide
		case -16: 
			return DataStr(_TB("Invalid company name Id")); // Identificativo azienda non valida
		case -17: 
			return DataStr(_TB("Error during envelope creation")); //Errore nella creazione del lotto
		case -18: 
			return DataStr(_TB("Error assigning authentication token")); //Errore nell'assegnazione del Token di autenticazione
		case -19: 
			return DataStr(_TB("Invalid authentication code")); //Codice di autenticazione non valido
		case -20: 
			return DataStr(_TB("Expired user")); //Utente scaduto
		case -21: 
			return DataStr(_TB("User not active")); //Utente non attivo
		case -22: 
			return DataStr(_TB("User not authorized")); //Utente non abilitato al servizio
		case -23: 
			return DataStr(_TB("Invalid file name")); //Filename non valido
		case -24: 
			return DataStr(_TB("Invalid file type")); //Tipo di file non valido
		case -25: 
			return DataStr(_TB("Invalid file content")); //Il contenuto del file non è valido
		case -26: 
			return DataStr(_TB("Invalid delivery type")); //Il tipo di spedizione non è valido
		case -27: 
			return DataStr(_TB("Invalid print type")); //Il tipo di stampa non è valido
		case -28: 
			return DataStr(_TB("Invalid total number of pages")); //Numero totale delle pagine non valido
		case -29: 
			return DataStr(_TB("Invalid country"));//Nazione non valida
		case -30: 
			return DataStr(_TB("Invalid zip code")); //Cap non valido
		case -31: 
			return DataStr(_TB("Invalid city"));//Città non valida
		case -32: 
			return DataStr(_TB("Invalid county"));//Nazione non valida
		case -33: 
			return DataStr(_TB("Invalid Fax")); //Fax non valido
		case -34: 
			return DataStr(_TB("Invalid description")); //Descrizione non valida
		case -35: 
			return DataStr(_TB("Invalid print type for the choosen delivery type")); //Tipo di stampa non valida per il tipo di spedizione scelto
		case -36: 
			return DataStr(_TB("Suspended user")); //Utente sospeso
		case -37: 
			return DataStr(_TB("Certified delivery: sending zone must be Italy")); //Raccomandata o RaccomandataAR la zona di invio deve essere Italia
		case -38: 
			return DataStr(_TB("Unable to determine che destionation zone: please check the zip code")); // Impossibile calcolare la zona di destinazione. Controllare il cap
		case -39: 
			return DataStr(_TB("Unable to determine che destionation zone: please check the fax number")); //Impossibile calcolare la zona di destinazione. Controllare il fax
		case -40: 
			return DataStr(_TB("Unable to determine the print cost")); //Impossibile calcolare il costo della stampa
		case -41: 
			return DataStr(_TB("Unrecognize destionation country")); //La nazione indicata non è stata riconosciuta
		case -42: 
			return DataStr(_TB("Invalid sender zip code")); //Mittente_Cap non valido
		case -43: 
			return DataStr(_TB("Invalid sender e-mail")); //Mittente_Email non valida
		case -44: 
			return DataStr(_TB("Invalid sender address")); //Mittente_Indirizzo non valido
		case -45: 
			return DataStr(_TB("Invalid sender city")); //Mittente_Località non valida
		case -46: 
			return DataStr(_TB("Invalid sender company name")); //Mittente_NomeCognomeRagioneSociale non valido
		case -47: 
			return DataStr(_TB("Invalid sender county")); //Mittente_Provincia non valida
		case -55: 
			return DataStr(_TB("Invalid envelope id")); //Identificativo del lotto non valido
		case -56: 
			return DataStr(_TB("Invalid document")); //Documento non valido
		case -57: 
			return DataStr(_TB("Invalid user and/or password")); //Login e/o password non validi
		case -58: 
			return DataStr(_TB("No user found for the current login")); //Nessun utente trovato per login e password specificati
		case -59: 
			return DataStr(_TB("Invalid sender phone number")); //Mittente_Telefono non valido
		case -60: 
			return DataStr(_TB("Invalid addresse cover ")); //Indirizzo del destinatario_cover non valido
		case -61: 
			return DataStr(_TB("Invalid addresse surname or company name"));
		case -62: 
			return DataStr(_TB("Invalid legal code "));
		case -63: 
			return DataStr(_TB("Invalid activity code"));
		case -64: 
			return DataStr(_TB("Invalid area code"));
		case -65: 
			return DataStr(_TB("Invalid birth date"));
		case -66: 
			return DataStr(_TB("Invalid birth location"));
		case -67: 
			return DataStr(_TB("Invalid birth location"));
		case -68: 
			return DataStr(_TB("Invalid contact reference"));
		case -69: 
			return DataStr(_TB("Invalid private entity option"));
		case -70: 
			return DataStr(_TB("Invalid private entity company name"));
		case -71: 
			return DataStr(_TB("Legal code Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -72: 
			return DataStr(_TB("Activity code Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -73: 
			return DataStr(_TB("Location Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -74: 
			return DataStr(_TB("ZIP code Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -75: 
			return DataStr(_TB("County Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -76: 
			return DataStr(_TB("Address Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -77: 
			return DataStr(_TB("Telephone number Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -78: 
			return DataStr(_TB("Fax number Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -79: 
			return DataStr(_TB("Fiscal code Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -80: 
			return DataStr(_TB("VAT number Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -81: 
			return DataStr(_TB("Fiscal code and/or VAT number not specified"));
		case -82: 
			return DataStr(_TB("E-mail Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -83: 
			return DataStr(_TB("Phone area code Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -84: 
			return DataStr(_TB("Fax area code Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -85: 
			return DataStr(_TB("County Public Entity (option 2b Law August 13 N° 136) invalid"));
		case -86: 
			return DataStr(_TB("Cadastral code not found for specified county"));
		case -87: 
			return DataStr(_TB("User is already active, cannot perform a new activation"));
		case -88: 
			return DataStr(_TB("User is Suspended, cannot top up credit"));
		case -89: 
			return DataStr(_TB("Invalid top up amount"));
		case -90: 
			return DataStr(_TB("Invalid VAT value"));
		case -91: 
			return DataStr(_TB("Invalid total amount"));
		case -92: 
			return DataStr(_TB("Calculated total amount is invalid"));
		case -93: 
			return DataStr(_TB("Insufficient credit to complete the desired upload"));
		case -94: 
			return DataStr(_TB("PostaLite service is temporarily suspended for the desired upload"));
		case 0:
			break;
		default:
			return DataStr(_TB("Unknown error") + cwsprintf(_T(": %d"), (int)error)) ;
	}
	return  DataStr();
}

//=============================================================================
