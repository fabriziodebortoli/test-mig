
#include "stdafx.h"

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\minmax.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\LocalizableObjs.h>
#include <TbGeneric\ParametersSections.h>
#include <TBGeneric\WndObjDescription.h>

#include <TbGenlib\reswalk.h>
#include <TbGenlib\parsobj.h>
#include <TbGenlib\TBBreadCrumb.h>
#include <TbGenlib\TBPropertyGrid.h>
#include <TbGenlib\messages.h>
#include <TbGenlib\generic.h>

#include <TbGenlib\BaseFrm.hjson> //JSON AUTOMATIC UPDATE
#include <TbGenlib\PARSLBX.H>
#include <TbGenlib\PARSEDT.H>
#include <TbGenlib\AddressEdit.h>

#include <TbGenlibManaged\Main.h>

#include <TbGenlib\SettingsTableManager.h>

#include <TbGes\JsonFrame.h>

#include <TbOledb\oledbmng.h>
#include <TbOledb\sqlrec.h>
#include <TbOledb\sqltable.h>			
#include <TbOledb\sqlcatalog.h>	

#include <TbWoormEngine\report.h>

#include "browser.h"
#include "bodyedit.h"
#include "hotlink.h"
#include "dbt.h"
#include "barquery.h"
#include "extdoc.h"
#include "tabber.h"
#include "formmng.h"
#include "SlaveViewContainer.h"
#include "TreeEdit.h"
#include "ParsedPanel.h"
#include "TileDialog.h"
#include "TileManager.h"
#include "TBGridControl.h"
#include "ExtDocView.h"
#include "JsonFormEngineEx.h"
#include "HeaderStrip.h"
#include "CAddressDlg.h"

//resources
#include "extdoc.hjson" //JSON AUTOMATIC UPDATE
#include "barquery.hjson" //JSON AUTOMATIC UPDATE
#include "bodyedit.hjson" //JSON AUTOMATIC UPDATE
#include "EmptyView.hjson"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define VERIFY_DBT(pDBT) ASSERT(NULL == pDBT || NULL == pDBT->GetMainPrototype() || pDBT == pDBT->GetMainPrototype());


//==============================================================================
//	local DataToCtrlLink class
//==============================================================================
class DataToCtrlLink : public CObject
{
public:
	int	m_nDataInfoIdx;
	int	m_nControlLinkIdx;
	int m_nItemCheckLBIdx;

	DataToCtrlLink(int nDataInfoIdx, int nControlLinkIdx, int nItemCheckLBIdx)
		:
		m_nDataInfoIdx		(nDataInfoIdx),
		m_nControlLinkIdx	(nControlLinkIdx),
		m_nItemCheckLBIdx	(nItemCheckLBIdx)
	{}

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext& dc) const	{ ASSERT_VALID(this); AFX_DUMP0(dc, " DataToCtrlLink\n");}
	void AssertValid() const			{ CObject::AssertValid();}
#endif // _DEBUG
};

//==============================================================================
//	local CAnchorCtrl class
//	classe prototipale di ausilio per ancorare la pulsantiera della WizardView al bottom della form
//  TODO non è ancora generica
//==============================================================================
class /*TB_EXPORT*/ CAnchorCtrl : public CObject
{
public:
	enum AnchorDirection { AC_BOTTOM }; // AC_RIGHT, ...

	UINT				m_nControlID;
	AnchorDirection		m_eDirection;
	CPoint				m_cpOrigin;
/*
	CAnchorCtrl () 
		: m_nControlID(0), m_eDirection(AC_BOTTOM), m_cpOrigin(0,0) 
		{}

	CAnchorCtrl (UINT nControlID, AnchorDirection eDirection = AC_BOTTOM, CPoint cpOrigin = CPoint(0,0))
		: 
			m_nControlID	(nControlID), 
			m_eDirection	(eDirection), 
			m_cpOrigin		(cpOrigin) 
		{}
*/
	CAnchorCtrl (UINT nControlID, CWnd* pParent, AnchorDirection eDirection = AC_BOTTOM);

	void Move		(CWnd* pParent);
};

//------------------------------------------------------------------------------
CAnchorCtrl::CAnchorCtrl (UINT nControlID, CWnd* pParent, AnchorDirection eDirection /*= AC_BOTTOM*/)
	: 
		m_nControlID	(nControlID), 
		m_eDirection	(eDirection)
{
	CWnd* pCtrl  = pParent->GetDlgItem(nControlID);
	if (pCtrl == NULL)
	{
		TRACE(_T("CAnchorCtrl::CAnchorCtrl: anchor control with %d doesn't exist\n"), nControlID);
		ASSERT(FALSE);
		m_nControlID = 0;
		return;
	}
	CRect rectCtrl, rectW;
	pParent->GetWindowRect(rectW);
	pParent->GetDlgItem(nControlID)->GetWindowRect(rectCtrl);

	m_cpOrigin.x = rectCtrl.left - rectW.left; //LEFT anchor
	//RIGHT m_cpOrigin.x = rectW.right - rectCtrl.left - 2; 
	m_cpOrigin.y = rectW.bottom - rectCtrl.top + 10; //posto per la scrollbar orizzontale
}

//------------------------------------------------------------------------------
void CAnchorCtrl::Move(CWnd* pParent) 
{
	CWnd* pC = pParent->GetDlgItem(m_nControlID);
	if (pC == NULL) return;

	CRect rectW;
	pParent->GetWindowRect(rectW);

	if (m_eDirection == CAnchorCtrl::AC_BOTTOM)
	{
		CPoint ptOrigin(rectW.left + m_cpOrigin.x, rectW.bottom - m_cpOrigin.y); //LEFT anchor 
		//CPoint ptOrigin(rectW.right - m_cpOrigin.x, rectW.bottom - m_cpOrigin.y); //RIGHT anchor 
		pParent->ScreenToClient(&ptOrigin);
		pC->SetWindowPos(NULL, ptOrigin.x, ptOrigin.y, 0,0, SWP_NOSIZE);
	}
}

////////////////////////////////////////////////////////////////////////////////
//				funzioni di uso generale
////////////////////////////////////////////////////////////////////////////////
//

CBodyEdit* GetBodyEdits(ControlLinks* pControlLinks, int* pnStartIdx /*= NULL*/)
{   
	ASSERT_VALID(pControlLinks); 
	if (!pControlLinks) 
		return NULL;

	return (CBodyEdit*) pControlLinks->GetBodyEdits(pnStartIdx);
}

CBodyEdit* GetBodyEdits(ControlLinks* pControlLinks, const CTBNamespace& aNS)
{
	ASSERT_VALID(pControlLinks); 
	if (!pControlLinks) 
		return NULL;

	return (CBodyEdit*) pControlLinks->GetBodyEdits(aNS);
}

//-----------------------------------------------------------------------------
CAbstractFormDoc* GetDocument(CWnd* pWnd)
{
	ASSERT_VALID(pWnd);
	CAbstractFormView* pView = dynamic_cast<CAbstractFormView*> (pWnd);
	if (pView)
		return pView->GetDocument();

	CTabDialog* pDialog = dynamic_cast<CTabDialog*> (pWnd);
	if (pDialog)
		return (CAbstractFormDoc*)pDialog->GetDocument();
	
	CParsedPanel* pPanel = dynamic_cast<CParsedPanel*> (pWnd);
	if (pPanel)
		return (CAbstractFormDoc*)pPanel->GetDocument();

	CTileDialog* pTileDialog = dynamic_cast<CTileDialog*> (pWnd);
	if (pTileDialog)
		return (CAbstractFormDoc*)pTileDialog->GetDocument();

	CAbstractFormFrame* pFrame = dynamic_cast<CAbstractFormFrame*> (pWnd);
	if (pFrame)
		return (CAbstractFormDoc*)pFrame->GetDocument();

	return NULL;
}

//-----------------------------------------------------------------------------
void EnableControlLinks (ControlLinks* pControlLinks, BOOL bEnable, BOOL bMustSetOSLReadOnly)
{
	ASSERT_VALID(pControlLinks); 
	if (!pControlLinks) 
		return;

	for (int i = 0; i <= pControlLinks->GetUpperBound(); i++)
	{
		CWnd* pWnd = pControlLinks->GetAt(i);
		if (!pWnd)
			continue;

		CParsedCtrl* pCtrl = GetParsedCtrl(pWnd);

		// Try for Parsed Control			
		if (pCtrl)
		{
			if	(
					!bEnable && 
					pCtrl->GetButton() &&
					(pCtrl->GetDataType() == DataType::Date || pCtrl->GetDataType() == DataType::DateTime)
				)
				pCtrl->GetButton()->PostMessage(UM_DESTROY_CALENDAR);

			pCtrl->SetDataReadOnly(!bEnable);
			pCtrl->SetDataOSLReadOnly(bMustSetOSLReadOnly);

			if (pCtrl->GetDocument() &&
				(
					(pCtrl->GetDocument()->GetFormMode() == CBaseDocument::NEW && OSL_CAN_DO(pCtrl->GetInfoOSL(), OSL_GRANT_NEW) == 0) || 
					(pCtrl->GetDocument()->GetFormMode() == CBaseDocument::EDIT && OSL_CAN_DO(pCtrl->GetInfoOSL(), OSL_GRANT_EDIT) == 0)
				)
				)
				pCtrl->SetDataOSLReadOnly(TRUE);

			if (OSL_CAN_DO(pCtrl->GetInfoOSL(), OSL_GRANT_EXECUTE) == 0)
			{
				pCtrl->SetDataOSLReadOnly(TRUE);
				pCtrl->SetDataOSLHide(TRUE);
			}
			continue;
		}

		// Try for CBodyEdit
		if (pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)))
		{
			DBTSlaveBuffered* pDBT = ((CBodyEdit*)pWnd)->GetDBT();
			if (pDBT)
			{
				pDBT->SetReadOnly(!bEnable);
				//il dbt prototipo deve seguire le sorti di quello corrente
				//perché sono i suoi dataobj ad essere addlinkati nelle colonne
				DBTSlaveBuffered* pDBTPrototype = (DBTSlaveBuffered*)pDBT->GetMainPrototype();
				if (pDBTPrototype)
					pDBTPrototype->SetReadOnly(!bEnable);
			}
			continue;
		}

		// Try for CTBPropertyGrid
		if (pWnd->IsKindOf(RUNTIME_CLASS(CTBPropertyGrid)))
		{
			((CTBPropertyGrid*)pWnd)->EnableControlLinks(bEnable, bMustSetOSLReadOnly);
			continue;
		}

		if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedPanel)))
		{
			((CParsedPanel*)pWnd)->EnableControlLinks(bEnable, bMustSetOSLReadOnly);
			continue;
		}
	}
}

//-----------------------------------------------------------------------------
void SetOSLReadOnlyOnControlLinks (ControlLinks* pControlLinks, const Array& aDataToCtrlMap)
{
	ASSERT_VALID(pControlLinks); 
	if (!pControlLinks) 
		return;

	for (int i = 0; i <= pControlLinks->GetUpperBound(); i++)
	{
		CWnd* pWnd = pControlLinks->GetAt(i);
		CParsedCtrl* pControl = GetParsedCtrl(pWnd);

		BOOL bRecordData = FALSE;

		// cerca l'eventuale corrispondenza di questo control nella mappa che collega
		// l'indice delle colonna del record con l'indice del vettore dei ControlLinks
		for (int j = 0; j <= aDataToCtrlMap.GetUpperBound(); j++)
		{
			DataToCtrlLink*	pDataToCtrlLink	= (DataToCtrlLink*) aDataToCtrlMap[j];
			if (i == pDataToCtrlLink->m_nControlLinkIdx)
			{
				bRecordData = TRUE;
				break;
			}
		}

		if (pControl && !bRecordData)
			pControl->SetDataOSLReadOnly(TRUE);
	}
}


//---------------------------------------------------------------------------
// Control Created internally (useful for derived control)
// This function as to prefered from the function with already created control
//
//-----------------------------------------------------------------------------
CParsedCtrl* AddLink
	(
		const CString&	sName,
		CWnd*			pWnd,
		ControlLinks*	pControlLinks,
		UINT			nIDC, 
		SqlRecord*		pRecord, 
		DataObj*		pDataObj, 
		CRuntimeClass*	pParsedCtrlClass,
		HotKeyLink*		pHotKeyLink,			/* = NULL */
		UINT			nBtnID					/* = BTN_DEFAULT */
	)
{
	//Se il control non e` uno static allora controllo che il nome non sia gia` stato AddLink-ato.
	//Non voglio confrontare RUNTIME_CLASS perche` dovrei farlo per ogni possibile classe figlia di
	//CStatic e non voglio cablare qui questa conoscenza: il giorno in cui nascera` una nuova
	//classe static qui mi dimenticherei di aggiungerla al controllo.
	//Non posso usare IsKindOf perche` CParsedCtrl non e` DECLARE_DYNAMIC.
	//Non voglio rimuovere la chiamata a CHECK_UNIQUE_NAME perche` quando chi l'ha inserita e` stato
	//valutato potesse essere utile.
	CParsedCtrl* pOriginalControl = ::GetParsedCtrl(pParsedCtrlClass->CreateObject());
	CStatic* pCtrlStatic = dynamic_cast<CStatic*>(pOriginalControl);
	if (!pCtrlStatic)
		CHECK_UNIQUE_NAME(pControlLinks, sName);

	CAbstractFormDoc* pDoc = ::GetDocument(pWnd);
	CParsedCtrl* pControl = NULL;

	pControl = pDoc ? pDoc->DispatchOnCreateParsedCtrl(nIDC, pParsedCtrlClass) : NULL;

	if (pControl == NULL)
		pControl = pOriginalControl;
	else if (!pControl->GetCtrlCWnd()->IsKindOf(pParsedCtrlClass))
	{
		TRACE(_T("AddLink %s - IDC %d subclassed from runtime class %s to %s\n"), sName, nIDC, pParsedCtrlClass->m_lpszClassName, pControl->GetCtrlCWnd()->GetRuntimeClass()->m_lpszClassName);
		ASSERT(FALSE);
	}

	ASSERT(pControl);
	pControl->Attach(nBtnID);
	pControl->AttachDocument(pDoc);
	if (pDataObj)
		pControl->Attach(pDataObj);

    if (pHotKeyLink)
    {
		pHotKeyLink->AttachDocument(::GetDocument(pWnd));
	   	pControl->AttachHotKeyLink(pHotKeyLink);
 	}

	// pRecord can be NULL for DataObj not conneted to any SqlRecord
	// use default len and precision
	//
	if (pRecord)
	{
		long nLength = 0; 
		TRY
		{
			nLength = pRecord->GetColumnLength(pDataObj); //potrebbe essere anche un parametro
		}
		
		CATCH(SqlException, e)
		{
			delete pControl;
			ASSERT(FALSE);
			THROW_LAST();
		}
		END_CATCH

		pControl->SetCtrlMaxLen(nLength, FALSE);
		pControl->AttachRecord(pRecord);
	}

	if (!pControl->SubclassEdit(nIDC, pWnd))
	{
		delete pControl;

		ASSERT(FALSE);
		return NULL;
	}

	if (pHotKeyLink)
    {
		pHotKeyLink->DoOnCreatedOwnerCtrl();
	}

	pControlLinks->Add(pControl->GetCtrlCWnd());

	CAddressEdit* pAddressEdit = dynamic_cast<CAddressEdit*>(pControl->GetCtrlCWnd());
	if (pAddressEdit)
	{
		pAddressEdit->SetAddressDlgClass(RUNTIME_CLASS(CAddressDlg));
		pAddressEdit->SetSelectAddressDlgClass(RUNTIME_CLASS(CSelectAddressDlg));
	}

	return pControl;
} 

//-----------------------------------------------------------------------------
CParsedCtrl* AddLinkAndCreateControl
	(
		const CString&	sName,
		DWORD			dwStyle, 
		const CRect&	rect,
		CWnd*			pWnd,
		ControlLinks*	pControlLinks,
		UINT			nIDC, 
		SqlRecord*		pRecord, 
		DataObj*		pDataObj, 
		CRuntimeClass*	pParsedCtrlClass,
		void*			pHotKeyLink,			/* = NULL */
		BOOL			bIsARuntimeClass,		/* = FALSE */
		UINT			nBtnID					/* = BTN_DEFAULT */
	)
{
	return ::CreateControl
	(
		sName,
		dwStyle, 
		rect,
		pWnd,
		::GetDocument(pWnd),
		pControlLinks,
		nIDC, 
		pRecord, 
		pDataObj, 
		pParsedCtrlClass,
		pHotKeyLink,
		bIsARuntimeClass,
		nBtnID
	);
}

//-----------------------------------------------------------------------------

CBodyEdit* AddLinkAndCreateBodyEdit
	(
		CRect				rect,
		CParsedForm*		pParentForm,
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pBodyEditClass,
		CRuntimeClass*		pRowFormViewClass /*=NULL*/,
		CString				strRowFormViewTitle, /*=T("")*/
		CString				sName /*=T("")*/,
		CString				sRowViewName  /*_T("")*/
	)
{   
	VERIFY_DBT(pDBT)

	CHECK_UNIQUE_NAME(pControlLinks, sName);

	CBodyEdit* pBodyEdit = (CBodyEdit*) pBodyEditClass->CreateObject();
	ASSERT(pBodyEdit);

	pBodyEdit->SetName(sName);
	pBodyEdit->SetParentForm(pParentForm);
	pBodyEdit->SetBodyEditID(nIDC);

	pBodyEdit->Attach(pDBT, pRowFormViewClass, strRowFormViewTitle, sRowViewName);

	BOOL bCreated = pBodyEdit->Create
		(
			WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_TABSTOP | BS_OWNERDRAW,
			rect,
			pParentWnd,
			nIDC
		);

	pControlLinks->Add(pBodyEdit);
	return pBodyEdit;
}

// Serve per poter aggiungere bottoni che cambiano stato sulla base del valore del
// dataobj passato. Nel caso che non si passa un DataObj il bottone non cambia
// stato ma si comporta come un normale bottone sensibile pero` al panning della
// formview
//
//-----------------------------------------------------------------------------
CExtButton* AddLink
	(
		const CString&	sName,
		CExtButtonExtendedInfo*	pExtInfo,
		ControlLinks*	pControlLinks,
		UINT			nIDC, 
		SqlRecord*		pRecord		/* = NULL */, 
		DataObj*		pDataObj	/* = NULL */ 
	)
{
	CHECK_UNIQUE_NAME(pControlLinks, sName);

	if (pDataObj)
	{
		if (pDataObj->GetDataType() != DATA_BOOL_TYPE)
		{
			TRACE0("CExtButton* AddLink: the DataObj should be a DataBool\n");
			ASSERT(FALSE);
			return NULL;
		}

		CPushButton* pButton = new CPushButton;
		CParsedCtrl* pControl = (CParsedCtrl*) pButton;

		pControl->Attach(pDataObj);
		pControl->AttachDocument(::GetDocument(pExtInfo->GetParentWnd()));

		// pRecord can be NULL for DataObj not conneted to any SqlRecord
		if (pRecord)
		{
			pControl->AttachRecord(pRecord);
		}

		if (!pButton->SubclassEdit(nIDC, pExtInfo->GetParentWnd()))
		{
			delete pButton;

			ASSERT(FALSE);
			return NULL;
		}

		pControlLinks->Add(pButton);
		return pButton;
	}

	// istanzia un normale CExtButton per gestire il panning della view
	CExtButton* pButton = new CExtButton;
		
	pButton->SetExtInfo (pExtInfo);
	pButton->GetExtInfo ()->SetNamespace (sName);

	if (!pButton->SubclassDlgItem(nIDC, pExtInfo->GetParentWnd()))
	{
		delete pButton;
		
		ASSERT(FALSE);
		return NULL;
	}

	pControlLinks->Add(pButton);
	return pButton;
}

// CBodyEdit Created internally (useful for derived CBodyEdit)
// This function as to prefered from the function with already created CBodyEdit
//-----------------------------------------------------------------------------
CBodyEdit* AddLink
	(
		CParsedForm*		pParentForm,
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pParsedCtrlClass,
		CRuntimeClass*		pRowFormViewClass /*=NULL*/,
		CString				strRowFormViewTitle, /*=_T("")*/
		CString				sBodyName,				/*= _T("")*/
		CString				sRowViewName
	)
{
	VERIFY_DBT(pDBT)
	// bypassa il subclassing automatico dei BCG nel caso l'AddLink venga
	// chiamata dopo il meccanismo della BuildDataCtrlLinks
	CWnd* pWnd = pParentWnd->GetDlgItem(nIDC);
	if (!pWnd)
		return NULL;
	
	if (pWnd->IsKindOf(RUNTIME_CLASS(CBCGPButton)))
		pWnd->UnsubclassWindow();


	CBodyEdit* pBodyEdit = (CBodyEdit*) pParsedCtrlClass->CreateObject();
	ASSERT_VALID(pBodyEdit);
	if (!pBodyEdit)
		return NULL;
	
	if (!sBodyName.IsEmpty())
		pBodyEdit->SetName(sBodyName);

	CHECK_UNIQUE_NAME(pControlLinks, pBodyEdit->GetBEName());

	pBodyEdit->SetParentForm(pParentForm);

	if (!strRowFormViewTitle.IsEmpty() || !sRowViewName.IsEmpty())
		pBodyEdit->Attach(pDBT, pRowFormViewClass, strRowFormViewTitle, sRowViewName);
	else
		pBodyEdit->Attach(pDBT, pRowFormViewClass);

	if (!pBodyEdit->SubclassDlgItem(nIDC, pParentWnd))
	{
		TRACE("CBodyEdit* AddLink: bodyedit subclassing control %s failed", pBodyEdit->GetRuntimeClass()->m_lpszClassName);
		ASSERT(FALSE);
		
		delete pBodyEdit;

		return NULL;
	}

	pControlLinks->Add(pBodyEdit);
	return pBodyEdit;
}

//-----------------------------------------------------------------------------
CDBTTreeEdit* AddLink
	(
		CParsedForm*		pParentForm,
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CString				sName,
		CRuntimeClass*		pTreeClass /*=NULL*/
	)
{
	CHECK_UNIQUE_NAME(pControlLinks, sName);

	ASSERT(!pTreeClass || pTreeClass->IsDerivedFrom(RUNTIME_CLASS(CDBTTreeEdit)));

	CDBTTreeEdit* pEdit = pTreeClass 
		? (CDBTTreeEdit*)pTreeClass->CreateObject() 
		: new CDBTTreeEdit();

	pEdit->Attach(pDBT);
	
	if (!pEdit->SubclassEdit(nIDC, pParentWnd)) 
	{
		TRACE("CDBTTreeEdit* AddLink: dbttreeedit subclassing control %s failed", pEdit->GetRuntimeClass()->m_lpszClassName);
		ASSERT(FALSE);
		
		delete pEdit;
		return NULL;
	}

	pControlLinks->Add(pEdit);
	return pEdit;
}

//-----------------------------------------------------------------------------
CTBGridControl*	AddLinkGridInternal
	(
		CParsedForm*		pParentForm,	//CAbstractFormView/TabDialog
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pGridRuntimeClass/*	= NULL*/,
		CString				sGridName/*			= _T("")*/	// bodyedit name for dynamic binding
	)
{
	CTBGridControl* pGridControl =
		(pGridRuntimeClass == NULL) ?
		new CTBGridControl() :
		(CTBGridControl*)pGridRuntimeClass->CreateObject();

	pGridControl->SetName(sGridName);
	pGridControl->SetParentForm(pParentForm);
	
	// Create grid control:
	CRect rect;
	pParentWnd->GetWindowRect(rect);
	pParentWnd->ScreenToClient (rect);
	pGridControl->Create (WS_CHILD | WS_VISIBLE, rect, pParentWnd, nIDC);

	if (pDBT)
		pGridControl->SetDataSource(pDBT);

	pControlLinks->Add(pGridControl);
	return pGridControl;
}

//-----------------------------------------------------------------------------
CTBGridControl*	AddLinkGridInternal
(
CParsedForm*		pParentForm,	//CAbstractFormView/TabDialog
CWnd*				pParentWnd,
ControlLinks*		pControlLinks,
UINT				nIDC,
SqlTable*			sqlTbl,
CRuntimeClass*		pGridRuntimeClass/*	= NULL*/,
CString				sGridName/*			= _T("")*/	// bodyedit name for dynamic binding
)
{
	CTBGridControl* pGridControl =
		(pGridRuntimeClass == NULL) ?
		new CTBGridControl() :
		(CTBGridControl*)pGridRuntimeClass->CreateObject();

	pGridControl->SetName(sGridName);
	pGridControl->SetParentForm(pParentForm);

	// Create grid control:
	CRect rect;
	pParentWnd->GetWindowRect(rect);
	pParentWnd->ScreenToClient(rect);
	pGridControl->Create(WS_CHILD | WS_VISIBLE, rect, pParentWnd, nIDC);

	pGridControl->SetDataSource(sqlTbl);

	pControlLinks->Add(pGridControl);
	return pGridControl;
}


//-----------------------------------------------------------------------------
int AddDataBoolToCheckLB
	(
		CBoolCheckListBox*	pBCLB,
		LPCTSTR				lpszAssoc,
		SqlRecord*, 
		DataObj*			pDataObj
	)
{
	ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)));

	if (!pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)))
		return -1;

	return pBCLB->AddDataBool(lpszAssoc, (DataBool&)*pDataObj);
}

//-----------------------------------------------------------------------------
CLabelStatic* AddLabelLink (CWnd* pParentWnd, UINT nIDC)
{
	CAbstractFormDoc* pDoc = ::GetDocument(pParentWnd);

	CLabelStatic* pLabel = new CLabelStatic();

	pLabel->AttachDocument(pDoc);

	if (!pLabel->SubclassDlgItem(nIDC, pParentWnd))
	{
		TRACE(_T("AddLabelLink with IDC %d fails to subclass resource control\n"), nIDC);
		ASSERT(FALSE);
		delete pLabel;
		return NULL;
	}

	//serve ad inizializzare il dataobj!
	CString sText;
	pLabel->GetWindowText(sText);
	pLabel->SetValue((LPCTSTR)sText);
	//----

	pLabel->SetZOrderInnerControls(pParentWnd);

	return pLabel;
} 

//-----------------------------------------------------------------------------
CLabelStatic* AddLabelLinkWithLine(CWnd* pParentWnd, UINT nIDC, COLORREF titleColor, int nSizePen /*= 1*/, /*ELinePos*/int pos /*= CLabelStatic::LP_TOP*/)
{
	CLabelStatic* pLabel = AddLabelLink(pParentWnd, nIDC);
	pLabel->SetOwnFont(AfxGetThemeManager()->GetStaticWithLineFont(), FALSE);
	pLabel->ShowTextWithLine(titleColor, 1, CLabelStatic::LP_TOP);
	pLabel->SetTextColor(titleColor);
	return pLabel;
}

//-----------------------------------------------------------------------------
CLabelStatic* AddSeparatorLink (CWnd* pParentWnd, UINT nIDC, COLORREF crBorder, int nSizePen, BOOL  bVertical, CLabelStatic::ELinePos pos/* = CLabelStatic::LP_VCENTER*/)
{
	ASSERT_VALID(pParentWnd);
	if (!pParentWnd)
		return NULL;

	CAbstractFormDoc* pDoc = ::GetDocument(pParentWnd);
	//ASSERT_VALID(pDoc);
	//if (!pDoc)
	//	return NULL;

	CLabelStatic* pLabel = new CLabelStatic();

	pLabel->AttachDocument(pDoc);

	if (!pLabel->SubclassDlgItem(nIDC, pParentWnd))
	{
		TRACE(_T("AddLabelLink with IDC %d fails to subclass resource control\n"), nIDC);
		ASSERT(FALSE);
		delete pLabel;
		return NULL;
	}

#ifdef _DEBUG
	CString sText;
	pLabel->GetWindowText(sText);
#endif
	pLabel->SetValue(L"");	//ELIMINA IL TESTO
	
	CRect rcClient;
	pLabel->GetWindowRect(&rcClient);
	pParentWnd->ScreenToClient(rcClient);
	
	if (bVertical)
	{
		int x;
		switch(pos)
		{
			case CLabelStatic::LP_LEFT:
			case CLabelStatic::LP_TOP:
				x = rcClient.left;
				break;
			case CLabelStatic::LP_HCENTER:
			case CLabelStatic::LP_VCENTER:
				x = rcClient.left + rcClient.Width() / 2 - nSizePen / 2;
				break;
			case CLabelStatic::LP_RIGHT:
			case CLabelStatic::LP_BOTTOM:
				x = rcClient.right - nSizePen;
				break;
			default:
				ASSERT(FALSE);
		}

		VERIFY(pLabel->SetWindowPos(NULL,	x,				rcClient.top,	nSizePen,			rcClient.Height(), 
			SWP_NOZORDER|SWP_SHOWWINDOW));	

		pos = CLabelStatic::LP_HCENTER;
	}
	else
	{
		int y;
		switch(pos)
		{
			case CLabelStatic::LP_TOP:
				y = rcClient.top;
				break;
			case CLabelStatic::LP_VCENTER:
				y = rcClient.top + rcClient.Height() / 2 - nSizePen / 2;
				break;
			case CLabelStatic::LP_BOTTOM:
				y = rcClient.bottom - nSizePen;
				break;
			default:
				ASSERT(FALSE);
		}

		VERIFY(pLabel->SetWindowPos(NULL,	rcClient.left,	y,				rcClient.Width(),	nSizePen, 
			SWP_NOZORDER|SWP_SHOWWINDOW));

		pos = CLabelStatic::LP_VCENTER;
	}

	pLabel->ShowSeparator(crBorder, nSizePen, bVertical, pos);

	pLabel->SetZOrderInnerControls(pParentWnd);

	return pLabel;
} 

//-----------------------------------------------------------------------------
CGroupBoxBtn* AddGroupBoxLink (CWnd* pParentWnd, UINT nIDC)
{
	CGroupBoxBtn* pGrp = new CGroupBoxBtn();

	if (!pGrp->SubclassDlgItem(nIDC, pParentWnd))
	{
		TRACE(_T("AddGroupBoxLink with IDC %d fails to subclass resource control\n"), nIDC);
		ASSERT(FALSE);
		delete pGrp;
		return NULL;
	}

	pGrp->SetZOrderInnerControls(pParentWnd);

	return pGrp;
} 

//-----------------------------------------------------------------------------
CParsedCtrl*  ReplaceAddLink
	(
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC, 
		CRuntimeClass*		pParsedCtrlClass,
		HotKeyLink*			pHotKeyLink			 /*= NULL */,
		UINT				nBtnID				/* = BTN_DEFAULT */
	)
{ 
	int idx = pControlLinks->FindIndexByIDC (nIDC);
	if (idx == -1) 
		return NULL;

	CWnd* pWnd = pControlLinks->GetAt(idx);
	ASSERT_VALID(pWnd);
	
	CParsedCtrl* pParCtrl = ::GetParsedCtrl(pWnd);
	if (!pParCtrl)
	{
		ASSERT_TRACE1(FALSE, "ReplaceAddLink failed on ctrl with ID: %d\n", nIDC);
		return NULL;
	}
	
	CString sName = pParCtrl->GetNamespace().GetObjectName();
	ASSERT(!sName.IsEmpty());

	DataObj* pObj = pParCtrl->GetCtrlData();
	ASSERT_VALID(pObj);

	SqlRecord* pRec = (SqlRecord*)pParCtrl->m_pSqlRecord;
	
	DWORD dwStyle = 0;
	DWORD dwExStyle = 0;
	HWND hw = NULL;
	CRect rect;

	if (pWnd)
		pControlLinks->Remove(pWnd);

	pWnd->GetWindowRect(rect);
	pParentWnd->ScreenToClient(rect);

	// aggiunge la larghezza degli eventuali bottoni
	CRect btnRect;
	if (pParCtrl->GetButton())
	{
		pParCtrl->GetButton()->GetWindowRect(btnRect);
		rect.right += (BTN_OFFSET + btnRect.Width());
	}
	for (int i = 0; i <= pParCtrl->GetStateCtrlsArray().GetUpperBound(); i++)
	{
		CStateCtrlObj* pStateCtrl = (CStateCtrlObj*) pParCtrl->GetStateCtrlsArray().GetAt(i);
		CStateButton* pStateButton = pStateCtrl->GetButton();
		pStateButton->GetWindowRect(btnRect);
		rect.right += (BTN_OFFSET + btnRect.Width());
	}

	// prende gli stili della finestra
	dwStyle = pWnd->GetStyle();
	dwExStyle = pWnd->GetExStyle();

	SAFE_DELETE(pWnd); pParCtrl = NULL;
	//----

	pParCtrl = ::AddLinkAndCreateControl
					(
						sName,
						dwStyle | dwExStyle, 
						rect,
						pParentWnd,
						pControlLinks,
						nIDC, 
						pRec,
						pObj,
						pParsedCtrlClass,
						pHotKeyLink,		
						FALSE,
						nBtnID				
					);
	ASSERT(pParCtrl);
	if (!pParCtrl) 
		return NULL;

	//reinserisco il nuovo controllo nella posizione originaria
	pWnd = pParCtrl->GetCtrlCWnd ();
	pControlLinks->SetAt(idx, pWnd);
	pControlLinks->SetAt(pControlLinks->GetUpperBound(), NULL);
	pControlLinks->SetSize(pControlLinks->GetCount() - 1);

	return pParCtrl;
}

//-----------------------------------------------------------------------------
CParsedPanel* AddLink
		(
				CWnd*			pParentWnd,
				ControlLinks*	pControlLinks,
				UINT			nIDC, 
				CRuntimeClass*	pParsedPanelClass, 
				CObject*		pPanelOwner,
		const	CString&		sName, 
		const	CString&		sCaption, 
				BOOL			bCallOnInitialUpdate /*TRUE*/
		)
{
	CString strName = sName;
	strName.Trim();
	if (strName.IsEmpty())
	{
		TRACE(_T("Empty name not allowed %s\n"), pParsedPanelClass->m_lpszClassName);
		ASSERT(FALSE);
		strName = pParsedPanelClass->m_lpszClassName;
	}

	CAbstractFormDoc* pDocument = GetDocument(pParentWnd);

	CParsedPanel* pPanel = (CParsedPanel*)(pParsedPanelClass->CreateObject());
	pPanel->AttachDocument(pDocument);
	pPanel->AttachOwner(pPanelOwner);

	pPanel->Create(nIDC, sCaption, pParentWnd);

	pPanel->ShowWindow(SW_SHOW);
	pControlLinks->Add(pPanel);

	return pPanel;
}

//-----------------------------------------------------------------------------
CTBPropertyGrid* AddLinkPropertyGrid
(
	CParsedForm*		pParentForm,
	CWnd*				pParentWnd,
	ControlLinks*		pControlLinks,
	UINT				nIDC,
	CString				sName,
	CRuntimeClass*		pRuntimeClass, /*NULL*/
	CRect				rect /*= CRect(0, 0, 0, 0)*/
	)
{
	CTBPropertyGrid* pControl = (pRuntimeClass == NULL) ? new CTBPropertyGrid() : (CTBPropertyGrid*)pRuntimeClass->CreateObject();
	return AddLinkPropertyGrid(pControl, pParentForm, pParentWnd, pControlLinks, nIDC, sName, rect);
}
//-----------------------------------------------------------------------------
CTBPropertyGrid* AddLinkPropertyGrid
	(
		CTBPropertyGrid*	pControl,
		CParsedForm*		pParentForm,
		CWnd*				pParentWnd,
		ControlLinks*		pControlLinks,
		UINT				nIDC,
		CString				sName,
		CRect				rect /*= CRect(0, 0, 0, 0)*/
	)
{
	pControl->SetName(sName);
	pControl->SetParentForm(pParentForm);

	CWnd* pPlaceHolder = pParentWnd->GetDlgItem(nIDC);
	if (pPlaceHolder)
	{
		pPlaceHolder->GetWindowRect(rect);
		
		VERIFY(DestroyWindow(pPlaceHolder->UnsubclassWindow()));
		pParentWnd->ScreenToClient(rect);
	}
	else if (rect.IsRectNull())
	{
		pParentWnd->GetWindowRect(rect);
		pParentWnd->ScreenToClient(rect);
	}

	if (!pControl->Create(WS_CHILD | WS_VISIBLE | WS_BORDER | WS_TABSTOP, rect, pParentWnd, nIDC))
	{
		ASSERT(FALSE);
		delete pControl;
		return NULL;
	}

	if (pControl)
		pControlLinks->Add(pControl);
		
	return pControl;
}

//-----------------------------------------------------------------------------
BOOL SubclassParsedControl
	(
		CWnd*			pParent,
		UINT			nIDC,
		CWnd*			pControl,
		DataObj*		pDataObj,
		const CString&	sName,
		const CString&	sNsHotKeyLink /*= L""*/,
		UINT			nBtnID /*= BTN_DEFAULT*/
	)
{
	ASSERT_VALID(pControl);
	CParsedCtrl* pCtrl = dynamic_cast<CParsedCtrl*>(pControl);
	if (!pCtrl)
		return FALSE;

	CAbstractFormDoc* pDoc = ::GetDocument(pParent);
	if (pDoc) 
		pCtrl->AttachDocument(pDoc);
	pCtrl->Attach(pDataObj);

	pCtrl->Attach(nBtnID);

	CParsedForm* pForm = dynamic_cast<CParsedForm*>(pParent);
	if (pForm) 
		pForm->SetChildControlNamespace(sName, pCtrl);

	if (!sNsHotKeyLink.IsEmpty())
	{
		CTBNamespace nsHKL(CTBNamespace::HOTLINK, sNsHotKeyLink);
		if (nsHKL.IsValid())
		{
			HotKeyLink*		pHotKeyLink = (HotKeyLink*)AfxGetTbCmdManager()->RunHotlink(nsHKL);
			if (pHotKeyLink)
			{
				if (pDoc) 
					pHotKeyLink->AttachDocument(pDoc);
				pCtrl->AttachHotKeyLink(pHotKeyLink, TRUE);

				pHotKeyLink->DoOnCreatedOwnerCtrl();
			}
		}
	}

	if (!pCtrl->SubclassEdit(nIDC, pParent, sName))
		return FALSE;

	return TRUE;
}
//===========================================================================
// CAbstractFormView
//===========================================================================

IMPLEMENT_DYNAMIC(CAbstractFormView, CBaseFormView)

BEGIN_MESSAGE_MAP(CAbstractFormView, CBaseFormView)
	
	ON_WM_HSCROLL		()
	ON_WM_VSCROLL		()

	ON_COMMAND (ID_EXTDOC_UP, 		OnScrollUp		)
	ON_COMMAND (ID_EXTDOC_DOWN,	 	OnScrollDown	)
	ON_COMMAND (ID_EXTDOC_LEFT,	 	OnScrollLeft	)
	ON_COMMAND (ID_EXTDOC_RIGHT,	OnScrollRight	)
	ON_COMMAND (ID_EXTDOC_HOME, 	OnScrollTop		)
	ON_COMMAND (ID_EXTDOC_END,		OnScrollBotton	)

	ON_MESSAGE (UM_VALUE_CHANGED,	OnValueChanged)
	ON_MESSAGE (UM_RUN_BATCH,		OnRunBatch)
	ON_MESSAGE (UM_GET_CONTROL_DESCRIPTION,OnGetControlDescription)
	ON_MESSAGE(UM_GET_LOCALIZER_INFO,			OnGetLocalizerInfo)
	
	ON_WM_SIZE			()
	ON_WM_RBUTTONDOWN	()
	ON_WM_ERASEBKGND	()
	ON_WM_DESTROY		()
	ON_WM_PAINT			()
	ON_WM_MOUSEWHEEL	()

END_MESSAGE_MAP()


#pragma warning(disable:4355) // disabilita la warning sull'uso del this del parent
//---------------------------------------------------------------------------
CAbstractFormView::CAbstractFormView (const CString& sName, UINT nIDTemplate)
	:
	CBaseFormView				(sName, nIDTemplate),
	m_strStop 					(_TB("&Interrupt")),
	m_strStart					(_TB("&Start")),
	m_strResume					(_TB("&Continue")),
	m_strPause					(_TB("&Pause")),
	m_bInitialUpdateDone		(FALSE),
	m_bTemporaryHidden			(FALSE)
{
	m_pTabManagers	= new TabManagers();
	m_pTileGroups = new TileGroups();
	m_mwTop			= 0;
}

#pragma warning(default:4355)
//-----------------------------------------------------------------------------
CAbstractFormView::~CAbstractFormView()
{
	SAFE_DELETE(m_pTabManagers);
	SAFE_DELETE(m_pTileGroups);
}

CAbstractFormDoc*	CAbstractFormView::GetDocument() const
{ 
	if (!(CView::m_pDocument) || !(CView::m_pDocument)->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		return NULL;

	return (CAbstractFormDoc*) CView::m_pDocument; 
}

//-----------------------------------------------------------------------------
void CAbstractFormView::OnDestroy() 
{
	SyncExternalControllerInfo(TRUE);
	
	__super::OnDestroy();
}

// Questo metodo permette di risalire alla CWnd di un control IDC
//-----------------------------------------------------------------------------
CWnd* CAbstractFormView::GetWndLinkedCtrl(UINT nIDC)
{   
	CWnd* pWnd = ::GetWndLinkedCtrl(m_pControlLinks, nIDC);
	if (pWnd) return pWnd;
			
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CWnd* pTabManager = m_pTabManagers->GetAt(i);
		if (pTabManager->GetDlgCtrlID() == nIDC)
			return pTabManager;

		pWnd = m_pTabManagers->GetActiveDlg(i)->GetWndLinkedCtrl(nIDC);
		if (pWnd) return pWnd;
	}

	// ruota l'azione alle eventuali TileDialog presenti
	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CBaseTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		if (pTileGroup->GetDlgCtrlID() == nIDC)
			return pTileGroup;

		for (int j = 0; j <= pTileGroup->GetTileDialogs()->GetUpperBound(); j++)
		{
			pWnd = pTileGroup->GetTileDialogs()->GetAt(j)->GetWndLinkedCtrl(nIDC);
			if (pWnd) 
				return pWnd;
		}
	}
	
	return NULL;
}
//-----------------------------------------------------------------------------
void CAbstractFormView::OnPaint()
{
	//a volte la CreateDlgIndirect manda un WM_PAINT prima di avere il tempo di chiamare SetScrollSizes,
	//e CScrollView solleva un fastidioso ASSERT
	//succede nei panel di woorm ad esempio, che sono view create a posteriori
#ifdef DEBUG
	if (m_nMapMode == 0)
	{
		CPaintDC dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente
		return;
	}
#endif
	__super::OnPaint();
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormView::OnEraseBkgnd(CDC* pDC)
{
	if (DoEraseBkgnd(pDC))
		return TRUE;

	if (GetDocument() && !GetDocument()->IsInStaticDesignMode())
	{
		CWnd* pCtrl = this->GetWindow(GW_CHILD);
		for (; pCtrl; pCtrl = pCtrl->GetNextWindow())
		{
			CRect screen;
			pCtrl->GetWindowRect(&screen);
			this->ScreenToClient(&screen);
			pDC->ExcludeClipRect(&screen);
		}
	}
	
	CRect rclientRect;
	this->GetClientRect(rclientRect);
	pDC->FillRect(&rclientRect, &GetDlgBackBrush());
	return TRUE;
}

//-----------------------------------------------------------------------------
CWnd* CAbstractFormView::GetWndLinkedCtrl(const CTBNamespace& aNS)
{   
	CWnd* pWnd = ::GetWndLinkedCtrl(m_pControlLinks, aNS);
	if (pWnd) 
		return pWnd;
			
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabManager* pTabber = m_pTabManagers->GetAt(i);
		pWnd = pTabber->GetWndLinkedCtrl(aNS);
		if (pWnd)
			return pWnd;
	}

	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CBaseTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pWnd = pTileGroup->GetWndLinkedCtrl(aNS);
		if (pWnd)
			return pWnd;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
CBodyEdit* CAbstractFormView::GetBodyEdits(int* pnStartIdx/* = NULL*/)
{
	CBodyEdit* pWnd = (CBodyEdit*) m_pControlLinks->GetBodyEdits(pnStartIdx);
	if (pWnd) 
		return pWnd;
			
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		pWnd = m_pTabManagers->GetActiveDlg(i)->GetBodyEdits(pnStartIdx);
		if (pWnd) 
			return pWnd;
	}
	
	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pWnd = pTileGroup->GetBodyEdits(pnStartIdx);
		if (pWnd) 
			return pWnd;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CBodyEdit* CAbstractFormView::GetBodyEdits(const CTBNamespace& aNS)
{
	CBodyEdit* pWnd = (CBodyEdit*) m_pControlLinks->GetBodyEdits(aNS);
	if (pWnd) return pWnd;
			
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		pWnd = m_pTabManagers->GetActiveDlg(i)->GetBodyEdits(aNS);
		if (pWnd) return pWnd;
	}
	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pWnd = pTileGroup->GetBodyEdits(aNS);
		if (pWnd) 
			return pWnd;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CAbstractFormView::GetLinkedParsedCtrl(UINT nIDC)
{ 
	CParsedCtrl* pControl = ::GetLinkedParsedCtrl(m_pControlLinks, nIDC);
	if (pControl) return pControl;
			
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		pControl = m_pTabManagers->GetActiveDlg(i)->GetLinkedParsedCtrl(nIDC);
		if (pControl) return pControl;
	}
	
	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pControl = pTileGroup->GetLinkedParsedCtrl(nIDC);
		if (pControl) 
			return pControl;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CAbstractFormView::GetLinkedParsedCtrl (DataObj* pDataObj)
{ 
	CParsedCtrl* pControl = ::GetLinkedParsedCtrl(m_pControlLinks, pDataObj);
	if (pControl) return pControl;
			
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		pControl = m_pTabManagers->GetActiveDlg(i)->GetLinkedParsedCtrl(pDataObj);
		if (pControl) return pControl;
	}

	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pControl = pTileGroup->GetLinkedParsedCtrl(pDataObj);
		if (pControl) 
			return pControl;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CAbstractFormView::GetLinkedParsedCtrl(const CTBNamespace& aNS)
{ 
	CParsedCtrl* pControl = ::GetLinkedParsedCtrl(m_pControlLinks, aNS);
	if (pControl) return pControl;
			
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		if (m_pTabManagers->GetActiveDlg(i))
			pControl = m_pTabManagers->GetActiveDlg(i)->GetLinkedParsedCtrl(aNS);
		if (pControl) return pControl;
	}

	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pControl = pTileGroup->GetLinkedParsedCtrl(aNS);
		if (pControl) 
			return pControl;
	}

	return NULL;
}


// Questo metodo permette di risalire alla CWnd di un control IDC
//-----------------------------------------------------------------------------
CWnd* CAbstractFormView::GetWndCtrl(UINT nIDC)
{   
	CWnd* pWnd = GetDlgItem(nIDC);
	if (pWnd) 
		return pWnd;
	
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		if (m_pTabManagers->GetActiveDlg(i))
		{
			pWnd = m_pTabManagers->GetActiveDlg(i)->GetWndCtrl(nIDC);
			if (pWnd) return pWnd;
		}
	}

	// ruota l'azione alle eventuali TileDialog presenti
	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		CWnd* pWnd = pTileGroup->GetWndCtrl(nIDC);
		if (pWnd)
			return pWnd;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CBaseTabManager* CAbstractFormView::GetTabber (UINT nIDC)
{
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i < m_pTabManagers->GetSize(); i++)
	{
		CTabManager* pTabManager = m_pTabManagers->GetAt(i);
		if (pTabManager->GetDlgCtrlID() == nIDC)
			return pTabManager;

		CTabDialog* pDialog = pTabManager->GetActiveDlg();
		CBaseTabManager* pChildTab = pDialog->GetTabber(nIDC);
		if (pChildTab)
			return pChildTab;
	}

	return NULL;
}
	
//-----------------------------------------------------------------------------
BOOL CAbstractFormView::TabDialogActivate(const CString& sNsTab)
{
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		if (m_pTabManagers->GetAt(i)->TabDialogActivate(sNsTab))
			return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
int CAbstractFormView::TabDialogActivate(UINT nTabIDC, UINT nIDD)
{
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		int nRet = m_pTabManagers->GetAt(i)->TabDialogActivate(nTabIDC, nIDD);
		if (nRet > 0) return nRet;
	}
	
	return 0;
}

//-----------------------------------------------------------------------------
int CAbstractFormView::TabDialogShow(UINT nTabIDC, UINT nIDD, BOOL bShow)
{
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		int nRet = m_pTabManagers->GetAt(i)->TabDialogShow(nTabIDC, nIDD, bShow);
		if (nRet > 0) return nRet;
	}
	
	return 0;
}

//-----------------------------------------------------------------------------
int CAbstractFormView::TileGroupShow(UINT nTileManagerIDC, UINT nIDDTileGroup, BOOL bShow)
{
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabManager* pTabManager = m_pTabManagers->GetAt(i);
		if (pTabManager->GetDlgCtrlID() == nTileManagerIDC)
		{ 
			UINT id = pTabManager->GetTabDialogID(nIDDTileGroup);
			return pTabManager->TabDialogShow(nTileManagerIDC, id, bShow);
		}
	}

	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		if (pTileGroup->GetDlgCtrlID() == nIDDTileGroup)
		{
			pTileGroup->Show(bShow);
		}
		
	}
	return 0;
}

//-----------------------------------------------------------------------------
int CAbstractFormView::TileGroupEnable(UINT nTileManagerIDC, UINT nIDDTileGroup, BOOL bEnable /* = TRUE */)
{
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabManager* pTabManager = m_pTabManagers->GetAt(i);
		if (pTabManager->GetDlgCtrlID() == nTileManagerIDC)
		{ 
			UINT id = pTabManager->GetTabDialogID(nIDDTileGroup);
			return pTabManager->TabDialogEnable(nTileManagerIDC, id, bEnable);		
		}
	}

	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		if (pTileGroup->GetDlgCtrlID() == nIDDTileGroup)
		{
			pTileGroup->Enable(bEnable);
		}	
	}
	return 0;
}

//-----------------------------------------------------------------------------
int	 CAbstractFormView::TileGroupActivate(UINT nTileManagerIDC, UINT nTileGroupIDC)
{
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabManager* pTabManager = m_pTabManagers->GetAt(i);
		if (pTabManager->GetDlgCtrlID() == nTileManagerIDC)
		{
			UINT id = pTabManager->GetTabDialogID(nTileGroupIDC);
			return pTabManager->TabDialogActivate(nTileManagerIDC, nTileGroupIDC);
		}
	}
	return 0;
}

//-----------------------------------------------------------------------------
int	CAbstractFormView::TileDialogEnable(UINT nIDDTileGroup, UINT nIDD, BOOL bEnable)
{
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabManager* pTabManager = m_pTabManagers->GetAt(i);
		UINT id = pTabManager->GetTabDialogID(nIDDTileGroup);
		CTileGroup* pGroup = id != 0 ? pTabManager->GetActiveTileGroup() : NULL;
		if (pGroup)
		{
			pGroup->EnableTile(nIDD, bEnable);
			break;
		}
	}

	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		if (pTileGroup && pTileGroup->GetDlgCtrlID() == nIDDTileGroup)
		{
			pTileGroup->EnableTile(nIDD, bEnable);
			break;
		}
	}
	return 0;
}

//-----------------------------------------------------------------------------
int	CAbstractFormView::TilePanelEnable(UINT nIDDTileGroup, UINT nIDD, BOOL bEnable)
{	
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabManager* pTabManager = m_pTabManagers->GetAt(i);
		UINT id = pTabManager->GetTabDialogID(nIDDTileGroup);
		CTileGroup* pGroup = id != 0 ? pTabManager->GetActiveTileGroup() : NULL;
		if (pGroup)
		{
			pGroup->EnableTilePanel(nIDD, bEnable);
			break;
		}
	}

	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		if (pTileGroup && pTileGroup->GetDlgCtrlID() == nIDDTileGroup)
		{
			pTileGroup->EnableTilePanel(nIDD, bEnable);
			break;
		}
	}
	return 0;
}

//-----------------------------------------------------------------------------
int CAbstractFormView::TabDialogEnable(UINT nTabIDC, UINT nIDD, BOOL bEnable /* = TRUE */)
{
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		int nRet = m_pTabManagers->GetAt(i)->TabDialogEnable(nTabIDC, nIDD, bEnable);
		if (nRet > 0) return nRet;
	}
	
	return 0;
}

//-----------------------------------------------------------------------------
void CAbstractFormView::PrepareTabDialogNamespaces()
{
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		m_pTabManagers->GetAt(i)->PrepareTabDialogNamespaces();
	}
}

//-----------------------------------------------------------------------------
CString CAbstractFormView::GetCaption()
{
	CString sCaption;
	CAbstractFormDoc* pDocument = GetDocument();

	if (pDocument)
	{
		sCaption = pDocument->m_pClientDocs->OnGetCaption(this);
		if (sCaption.IsEmpty())
			sCaption = pDocument->OnGetCaption(this);
	}
	 
	if (!sCaption.IsEmpty())
		return sCaption;

	if (pDocument)
		return pDocument->GetDefaultMenuDescription();

	return OnGetCaption(this);
}

//-----------------------------------------------------------------------------
void CAbstractFormView::SetDefaultFocus() 
{
	if (AfxIsRemoteInterface())
		return;
	
	if (m_pControlLinks->GetSize() > 0)
	{
		m_pControlLinks->SetDefaultFocus(this, m_phLastCtrlFocused);
	}
	else
	{
		for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
		{
			CBaseTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
			if (pTileGroup && pTileGroup->IsWindowEnabled())
			{
				if (pTileGroup->SetDefaultFocus())
					return;
			}
		}

		for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
		{
			CTabManager* pTabManager = m_pTabManagers->GetAt(i);
			if (pTabManager->IsWindowEnabled() && pTabManager->GetActiveDlg())
			{
				pTabManager->SetDefaultFocus();
				return;
			}
		}
	}
}

// Attenzione : chiama la EnableViewControlLinks della view 
// per gestire solo i dataobj di pertinenza della view
//----------------------------------------------------------------------------
void CAbstractFormView::EnableViewControls()
{
	if (BatchEnableViewControls())
		return;
		
	// normal processing in interctive mode
	// Non deve essere chiamata la DisableControlsAlways perchè viene
	// dato per scontato che basta la chiamata fatta dal documento al suo
	// cambio di stato
	switch (GetDocument()->GetFormMode())
	{
		case CBaseDocument::BROWSE	: 
			EnableViewControlLinks (FALSE);
			break;
			
		case CBaseDocument::NEW:
			EnableViewControlLinks	();
			GetDocument()->DispatchDisableControlsForAddNew();
			break;

		case CBaseDocument::EDIT:
			EnableViewControlLinks	();
			GetDocument()->DispatchDisableControlsForEdit ();
			break;

		case CBaseDocument::FIND:
			EnableViewControlLinks	(FALSE);
			GetDocument()->DispatchEnableControlsForFind ();
			break;			
	}
}

// Attenzione : chiama la EnableViewControlLinks della view 
// per gestire solo i dataobj di pertinenza della view ma chiama DisableControlsForBatch
// del documento per gestire bene le disabilitazioni che il programmatore DEVE
// gestire in modo congruente basandosi sullo stato corrente dei DataObj
// del documento (anche alla partenza).
//-----------------------------------------------------------------------------
BOOL CAbstractFormView::BatchEnableViewControls()
{
	if (GetDocument()->GetType() != VMT_BATCH)
		return FALSE;
		
	EnableViewControlLinks (!GetDocument()->m_bBatchRunning);
	if (!GetDocument()->m_bBatchRunning)
	{
		GetDocument()->DispatchDisableControlsForBatch();
	}

	SetBatchViewButtonState();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CAbstractFormView::SetBatchViewButtonState()
{
	if	(
			GetDocument()->IsEditingParamsFromExternalController() &&
			!GetDocument()->m_bBatchRunning
		)
	{
		m_strStart = _TB("Save");
		m_strPause = _TB("Cancel");
	}
	else
		OnSetBatchButtonIDS();

	CWnd* pStartStop = GetWndCtrl(IDC_EXTDOC_BATCH_START_STOP);
	if (pStartStop)	
		pStartStop->SetWindowText(GetDocument()->m_bBatchRunning ? m_strStop : m_strStart);
		
	CWnd* pPauseResume = GetWndCtrl(IDC_EXTDOC_BATCH_PAUSE_RESUME);
	if (pPauseResume)	
	{
		pPauseResume->EnableWindow
		(
			GetDocument()->IsEditingParamsFromExternalController() ||
			GetDocument()->m_bBatchRunning
		);
		pPauseResume->SetWindowText
		(
			GetDocument()->GetBatchScheduler ().IsPaused() 
			?m_strResume	
			:m_strPause
		);
	}
}

//------------------------------------------------------------------------------
BOOL CAbstractFormView::DispatchPrepareAuxData()
{
	m_pControlLinks->OnPrepareAuxData();
	if (!OnPrepareAuxData())
		return FALSE;
	
	GetDocument()->OnPrepareAuxData(this);

	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabManager* pTabManager =  m_pTabManagers->GetAt(i);
		if (!pTabManager->PrepareAuxData())
			return FALSE;
	}	

	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pTileGroup->PrepareAuxData();
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CAbstractFormView::EnableViewControlLinks	(BOOL bEnable /* = TRUE*/, BOOL bMustSetOSLReadOnly /*= FALSE*/)
{
	::EnableControlLinks (m_pControlLinks, bEnable, bMustSetOSLReadOnly);

	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabDialog* pTab = NULL;
		if (m_pTabManagers->GetActiveDlg(i))
			VERIFY( pTab = m_pTabManagers->GetActiveDlg(i) );
		if (pTab && pTab->m_hWnd)
		{
			pTab->EnableTabDialogControlLinks(bEnable, bMustSetOSLReadOnly);
		}
	}

	// ruota l'azione alle eventuali TileDialog presenti
	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pTileGroup->EnableViewControlLinks(bEnable, bMustSetOSLReadOnly); 
	}
}
//------------------------------------------------------------------------------
void CAbstractFormView::OnFindHotLinks()
{
	::OnFindHotLinks(m_pControlLinks);
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabDialog* pTab = NULL;
		if (m_pTabManagers->GetActiveDlg(i))
			VERIFY((pTab = m_pTabManagers->GetActiveDlg(i)) != NULL);
		if (pTab && pTab->m_hWnd)
			pTab->OnFindHotLinks();
	}

	// ruota l'azione alle eventuali TileDialog presenti
	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pTileGroup->OnFindHotLinks();
	}
}
//------------------------------------------------------------------------------
void CAbstractFormView::OnUpdateControls(BOOL bParentIsVisible)
{
	if (!::IsWindow(this->m_hWnd))
	{
		ASSERT_TRACE1(FALSE, "CAbstractFormView::OnUpdateControls() called on null window handle of class %s\n", CString(this->GetRuntimeClass()->m_lpszClassName));
		return;
	}
	::OnUpdateControls(m_pControlLinks, bParentIsVisible);
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabDialog* pTab = NULL;
		if (m_pTabManagers->GetActiveDlg(i))
			VERIFY( (pTab = m_pTabManagers->GetActiveDlg(i)) != NULL );
		if (pTab && pTab->m_hWnd)
			pTab->OnUpdateControls(bParentIsVisible);
	}

	// ruota l'azione alle eventuali TileDialog presenti
	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pTileGroup->OnUpdateControls(bParentIsVisible);
	}
}

//------------------------------------------------------------------------------
CTileGroup*	CAbstractFormView::GetTileGroup(UINT nIDC)
{
	for (int i = m_pTileGroups->GetUpperBound(); i >= 0; i--)
	{
		CTileGroup* pGroup = m_pTileGroups->GetAt(i);
		if (pGroup && pGroup->GetDlgCtrlID() == nIDC)
		{
			return pGroup;
			break;
		}
	}
	return NULL;
}

//------------------------------------------------------------------------------
CBaseTileDialog* CAbstractFormView::GetTileDialog(UINT nIDD)
{
	CBaseTileDialog* pTile = NULL;
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabDialog* pTab = NULL;
		if (m_pTabManagers->GetActiveDlg(i))
			VERIFY(pTab = m_pTabManagers->GetActiveDlg(i));
		if (pTab && pTab->m_hWnd)
		{
			pTile = pTab->GetTileDialog(nIDD);
			if (pTile)
				return pTile;
		}
	}

	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pTile = pTileGroup->GetTileDialog(nIDD);
		if (pTile)
			return pTile;
	}
	
	return NULL;
}

//------------------------------------------------------------------------------
void CAbstractFormView::RemoveTileGroup(UINT nIDC)
{
	for (int i = m_pTileGroups->GetUpperBound(); i >= 0; i--)
	{
		CTileGroup* pGroup = m_pTileGroups->GetAt(i);

		if (pGroup && pGroup->GetDlgCtrlID() == nIDC)
		{
			if (m_pLayoutContainer)
				m_pLayoutContainer->RemoveChildElement(pGroup);
		
			m_pTileGroups->RemoveAt(i);
			break;
		}
	}
}

//------------------------------------------------------------------------------------
void CAbstractFormView::MoveTileGroup(CBaseTileGroup* pTileGroup, int indexNew)
{
	BOOL bOldOwns = m_pTileGroups->IsOwnsElements();
	m_pTileGroups->SetOwns(FALSE);
	RemoveTileGroup(pTileGroup->GetDlgCtrlID());
	m_pTileGroups->SetOwns(bOldOwns);
	
	if (indexNew > m_pTileGroups->GetSize())
		m_pTileGroups->Add(pTileGroup);
	else
		m_pTileGroups->InsertAt(indexNew, pTileGroup);

	if (m_pLayoutContainer)
		m_pLayoutContainer->InsertChildElement(pTileGroup, indexNew);
}

//------------------------------------------------------------------------------
void CAbstractFormView::OnResetDataObjs()
{
	::OnResetDataObjs(m_pControlLinks);
	
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabDialog* pTab = NULL;
		if (m_pTabManagers->GetActiveDlg(i))
			VERIFY( pTab = m_pTabManagers->GetActiveDlg(i) );
		if (pTab && pTab->m_hWnd)
			pTab->OnResetDataObjs();
	}

	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pTileGroup->OnResetDataObjs();
	}
}

//------------------------------------------------------------------------------
BOOL CAbstractFormView::SetControlAutomaticExpression(UINT nID, const CString& strExp)
{
	for (int i = 0; i < m_pControlLinks->GetSize(); i++)
	{
		CWnd* pWnd = m_pControlLinks->GetAt(i);
		ASSERT(pWnd);

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);

		// i body edit sono ignorati
		if (!pControl || pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)) || !pControl->GetCtrlData())
			continue;

		if ((UINT)pWnd->GetDlgCtrlID() == nID)
			return pControl->SetAutomaticExpression(strExp);
	}

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormView::SetControlAutomaticExpression(DataObj* pDataObj, const CString& strExp)
{
	for (int i = 0; i < m_pControlLinks->GetSize(); i++)
	{
		CWnd* pWnd = m_pControlLinks->GetAt(i);
		ASSERT(pWnd);

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);

		// i body edit sono ignorati
		if (!pControl || pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)) || !pControl->GetCtrlData())
			continue;

		if (pControl->GetCtrlData() == pDataObj)
			return pControl->SetAutomaticExpression(strExp);
	}

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CAbstractFormView::GetControlAutomaticExpression(UINT& nID, CString& strExp)
{
	static int nPos = 0; 
	if (nID == 0) nPos = 0;

	if (m_pControlLinks->GetSize() == 0)
		return FALSE;

	for (;nPos < m_pControlLinks->GetSize(); nPos++)
	{
		CWnd* pWnd = m_pControlLinks->GetAt(nPos);
		ASSERT(pWnd);

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);

		// i body edit sono ignorati
		if (!pControl || pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)) || !pControl->GetCtrlData())
			continue;

		nID = pWnd->GetDlgCtrlID();
		strExp = pControl->GetAutomaticExpression();
		nPos++;

		return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CAbstractFormView::AddLink
	(
		UINT			nIDC, 
		const CString&	sName,
		SqlRecord*		pRecord, 
		DataObj*		pDataObj, 
		CRuntimeClass*	pParsedCtrlClass,
		CString			sNsHotKeyLink,
		UINT			nBtnID				/* = BTN_DEFAULT */
	)
{
	CTBNamespace nsHKL(CTBNamespace::HOTLINK, sNsHotKeyLink); 
	CRuntimeClass* pControlClass = NULL;
	HotKeyLink*		pHotKeyLink = !sNsHotKeyLink.IsEmpty() ?
									(HotKeyLink*) AfxGetTbCmdManager()->RunHotlink(nsHKL, NULL, &pControlClass) :
									NULL;

	CRuntimeClass* pPCClass = pParsedCtrlClass;
	if (pControlClass == NULL) 
		pPCClass = pParsedCtrlClass;
	else if (pParsedCtrlClass == NULL) 
		pPCClass = pControlClass;
	else if (pControlClass->IsDerivedFrom(pParsedCtrlClass))
		pPCClass = pControlClass;
	else
	{
		TRACE(_T("Incompatible runtime class for parsed control: hotlink has rtc named %s but AddLink has rtc named %s\n"), pControlClass->m_lpszClassName, pParsedCtrlClass->m_lpszClassName);
		//ASSERT(FALSE);
	}

	CParsedCtrl* pCtrl = AddLink(nIDC, sName, pRecord, pDataObj, pPCClass, pHotKeyLink, nBtnID);
	if (pCtrl && pHotKeyLink)
	{
		if (pHotKeyLink != pCtrl->GetHotLink())
			delete pHotKeyLink;
		else
			pCtrl->SetOwnHotKeyLink(TRUE);
	}
	return pCtrl;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CAbstractFormView::AddLink
	(
		UINT			nIDC, 
		const CString&	sName,
		SqlRecord*		pRecord, 
		DataObj*		pDataObj, 
		CRuntimeClass*	pParsedCtrlClass,
		HotKeyLink*		pHotKeyLink,		/* = NULL */
		UINT			nBtnID				/* = BTN_DEFAULT */
	)
{
	if (sName.IsEmpty())
	{
		TRACE(_T("CAbstractFormView::AddLink: the control idc=%d has empty name\n"), nIDC);
		ASSERT(FALSE);
	}

	CParsedCtrl* pCtrl =  ::AddLink
		(
			sName,
			this, 
			m_pControlLinks, 
			nIDC, 
			pRecord, 
			pDataObj, 
			pParsedCtrlClass,
			pHotKeyLink, 
			nBtnID
		);

	SetChildControlNamespace(sName, pCtrl);
	return pCtrl;
}
//-----------------------------------------------------------------------------
CDBTTreeEdit* CAbstractFormView::AddLink
		(
			UINT				nIDC, 
			DBTSlaveBuffered*	pDBT, 
			CString				sName,
			CRuntimeClass*		pTreeClass /*= NULL*/ 
		)
{
	return  ::AddLink
							(	
								this, 
								this,
								m_pControlLinks, 
								nIDC, 
								pDBT, 
								sName,
								pTreeClass
							);
}
//-----------------------------------------------------------------------------
CExtButton* CAbstractFormView::AddLink
	(
		UINT			nIDC, 
		const CString&	sName,
		SqlRecord*		pRecord		/* = NULL */, 
		DataObj*		pDataObj	/* = NULL */ 
	)
{
	if (sName.IsEmpty())
	{
		TRACE(_T("CAbstractFormView::AddLink: the control idc=%d has empty name\n"), nIDC);
		ASSERT(FALSE);
	}

	CExtButtonExtendedInfo* pExtInfo = new CExtButtonExtendedInfo(this, GetInfoOSL());

	CExtButton* pBtn = ::AddLink (sName, pExtInfo, m_pControlLinks, nIDC, pRecord, pDataObj);

	if (pBtn && pBtn->IsKindOf(RUNTIME_CLASS(CParsedButton)) && pDataObj)
	{
		CParsedButton* pCtrl = (CParsedButton*) pBtn;

		SetChildControlNamespace(sName, pCtrl);

		delete pExtInfo;
	}
	return pBtn;
}

//-----------------------------------------------------------------------------
CTBGridControl* CAbstractFormView::AddLinkGrid
	(
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pGridControlClass,
		CString				sName
		
	)
{
	CTBGridControl* grid = ::AddLinkGridInternal
		(
			this,
			this, 
			m_pControlLinks, 
			nIDC,
			pDBT,
			pGridControlClass,
			sName
		);

	return grid;
}

CTBGridControl* CAbstractFormView::AddLinkGrid
(
UINT				nIDC,
SqlTable*			sqlTbl,
CRuntimeClass*		pGridControlClass,
CString				sName

)
{
	CTBGridControl* grid = ::AddLinkGridInternal
		(
		this,
		this,
		m_pControlLinks,
		nIDC,
		sqlTbl,
		pGridControlClass,
		sName
		);

	return grid;
}


//-----------------------------------------------------------------------------
CBodyEdit* CAbstractFormView::AddLink
	(
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pParsedCtrlClass,
		CRuntimeClass*		pRowFormViewClass/* = NULL*/,
		CString				strRowFormViewTitle, /* = ""*/
		CString				sName
	)
{
	CBodyEdit* pBody = ::AddLink
		(
			this, this, 
			m_pControlLinks, 
			nIDC, pDBT, pParsedCtrlClass, 
			pRowFormViewClass, strRowFormViewTitle, sName
		);

	if (pBody)
	{
		ASSERT(pBody->GetInfoOSL()->m_pParent == GetInfoOSL());

		pBody->GetInfoOSL()->m_pParent = GetInfoOSL();
	}

	return pBody;
}

//-----------------------------------------------------------------------------
CParsedPanel* CAbstractFormView::AddLink
	(
				UINT			nIDC, 
				CRuntimeClass*	pParsedPanelClass, 
				CObject*		pPanelOwner,
		const	CString&		sName, 
		const	CString&		sCaption, 
				BOOL			bCallOnInitialUpdate /*TRUE*/
	)
{
	return ::AddLink
		(
			this,
			m_pControlLinks,
			nIDC, 
			pParsedPanelClass, 
			pPanelOwner,
			sName, 
			sCaption, 
			bCallOnInitialUpdate
	);
}

//-----------------------------------------------------------------------------
CParsedCtrl* CAbstractFormView::AddLinkAndCreateControl
		(
			const CString&	sName,
			DWORD			dwStyle, 
			const CRect&	rect,
			UINT			nIDC, 
			SqlRecord*		pRecord, 
			DataObj*		pDataObj, 
			CRuntimeClass*	pParsedCtrlClass,
			HotKeyLink*		pHotKeyLink			/*= NULL*/,
			UINT			nBtnID				/*= BTN_DEFAULT*/
		)
{
	CParsedCtrl* pCtrl = ::AddLinkAndCreateControl
		(
			sName,
			dwStyle, 
			rect,
			this, //parent
            this->GetControlLinks(),
			nIDC, 
			pRecord, 
			pDataObj, 
			pParsedCtrlClass,
			pHotKeyLink,
			FALSE,
			nBtnID
		);

	SetChildControlNamespace(sName, pCtrl);
	return pCtrl;
}

//-----------------------------------------------------------------------------
CBodyEdit* CAbstractFormView::AddLinkAndCreateBodyEdit
	(
		CRect				rect,
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pBodyEditClass,
		CRuntimeClass*		pRowFormViewClass /*= NULL*/,
		CString				strRowFormViewTitle/* = _T("")*/,
		CString				sBodyName  /*= _T("")*/,
		CString				sRowViewName/*  = _T("")*/
	)
{
	CBodyEdit* pBody = ::AddLinkAndCreateBodyEdit
		(
			rect,
			this,
			this,
			this->GetControlLinks(),
			nIDC, 
			pDBT, 
			pBodyEditClass,
			pRowFormViewClass,
			strRowFormViewTitle,
			sBodyName,
			sRowViewName
		);
		
	//if (pBody)
	//{
	//	ASSERT(pBody->GetInfoOSL()->m_pParent == GetInfoOSL());

	//	pBody->GetInfoOSL()->m_pParent = GetInfoOSL();
	//}

	return pBody;
}

//-----------------------------------------------------------------------------
CLabelStatic* CAbstractFormView::AddLabelLink (UINT nIDC)
{
	CLabelStatic* p = ::AddLabelLink(this, nIDC);
	m_pControlLinks->Add(p);
	return p;
}

//-----------------------------------------------------------------------------
CLabelStatic* CAbstractFormView::AddLabelLinkWithLine(UINT nIDC, int nSizePen /*= 1*/,int pos /*= CLabelStatic::LP_TOP*/)
{
	CLabelStatic* p = ::AddLabelLinkWithLine(this, nIDC, AfxGetThemeManager()->GetStaticWithLineLineForeColor(), nSizePen, pos);
	m_pControlLinks->Add(p);
	return p;
}

//-----------------------------------------------------------------------------
CLabelStatic* CAbstractFormView::AddSeparatorLink (UINT nIDC, COLORREF crBorder, int nSizePen/* = 1*/, BOOL  bVertical/* = FALSE*/, CLabelStatic::ELinePos pos/* = CLabelStatic::LP_VCENTER*/)
{
	CLabelStatic* p = ::AddSeparatorLink(this, nIDC, crBorder, nSizePen, bVertical, pos);
	m_pControlLinks->Add(p);
	return p;
}

//-----------------------------------------------------------------------------
CGroupBoxBtn* CAbstractFormView::AddGroupBoxLink (UINT nIDC)
{
	CGroupBoxBtn* p = ::AddGroupBoxLink(this, nIDC);
	m_pControlLinks->Add(p);
	return p;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CAbstractFormView::ReplaceAddLink
	(
		UINT			nIDC, 
		CRuntimeClass*	pParsedCtrlClass,
		HotKeyLink*		pHotKeyLink			/* = NULL */,
		UINT			nBtnID				/* = BTN_DEFAULT */
	)
{
	return ::ReplaceAddLink
	(
		this,
		this->m_pControlLinks,
		nIDC, 
		pParsedCtrlClass,
		pHotKeyLink,			
		nBtnID/*,				
		bReCreateCtrl*/
	);
}


//-----------------------------------------------------------------------------
CTBPropertyGrid* CAbstractFormView::AddLinkPropertyGrid
(
	UINT				nIDC,
	CString				sName,
	CRuntimeClass*		pRuntimeClass /*NULL*/
)
{
	return ::AddLinkPropertyGrid(::GetParsedForm(this), this, m_pControlLinks, nIDC, sName, pRuntimeClass);
}

//----------------------------------------------------------------------------- 
CTileGroup* CAbstractFormView::AddTileGroup(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate /*TRUE*/, CRect rectWnd /*= CRect(0, 0, 0, 0)*/)
{
	if (!m_pLayoutContainer)
		EnableLayout();
	CTileGroup* pTileGroup = (CTileGroup*)__super::AddBaseTileGroup(nIDC, pClass, sName, bCallOnInitialUpdate, NULL, rectWnd);
	pTileGroup->m_pDlgInfoItem = new TileGroupInfoItem(pClass, nIDC, -1, nIDC);
	m_DlgInfos.Add(pTileGroup->m_pDlgInfoItem);

	//check unique name for all groups
	for (int i = 0; i < m_pTileGroups->GetCount(); i++)
	{
		CBaseTileGroup* pTileGrp = m_pTileGroups->GetAt(i);
		if (!pTileGrp)
			continue;
		CString sTileGrpNS = pTileGrp->GetNamespace().GetObjectName();
		int res = sTileGrpNS.CompareNoCase(sName);
		ASSERT(res != 0);
		TRACE(_T("A tile group with the same name ") + sName + _T(" already exists! Please verify the uniqueness for all groups names"));
	}

	m_pTileGroups->Add(pTileGroup);

	return pTileGroup;
}

//-----------------------------------------------------------------------------
CTileManager* CAbstractFormView::AddTileManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate /*TRUE*/)
{
	if (!pClass->IsDerivedFrom(RUNTIME_CLASS(CTileManager)))
	{
		ASSERT_TRACE1(FALSE, "Runtime class parameter %s must be a CTabDialog!\n", (LPCTSTR)CString(pClass->m_lpszClassName));
		return NULL;
	}

	if (!m_pLayoutContainer)
		EnableLayout();
	return (CTileManager*)AddTabManager(nIDC, pClass, sName, bCallOnInitialUpdate);
}

//-----------------------------------------------------------------------------
CHeaderStrip* CAbstractFormView::AddHeaderStrip(UINT nIDC, const CString& strDefaultCaption, BOOL bCallInitialUpdate /*= TRUE*/, CRect rectWnd /*= CRect(0, 0, 0, 0)*/, CRuntimeClass* pClass /*=NULL*/)
{
	if (!m_pLayoutContainer)
		EnableLayout();
	CHeaderStrip* pStrip = CHeaderStrip::AddHeaderStrip(this, nIDC, strDefaultCaption, bCallInitialUpdate, rectWnd, pClass);
	return pStrip;
}

//-----------------------------------------------------------------------------
CTabManager* CAbstractFormView::AddTabManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate /*TRUE*/)
{
	if (!pClass->IsDerivedFrom(RUNTIME_CLASS(CTabManager)))
	{
		ASSERT_TRACE1(FALSE, "Runtime class parameter %s must be a CTabDialog!\n", (LPCTSTR)CString(pClass->m_lpszClassName));
		return NULL;
	}

	CTabManager* pTabManager = NULL;

	// already addlinked
	for (int i=0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		pTabManager = m_pTabManagers->GetAt(i);
		if (pTabManager && pTabManager->GetDlgCtrlID() == nIDC)
			return pTabManager;
	}

	pTabManager = (CTabManager*) __super::AddBaseTabManager(nIDC, pClass, sName, bCallOnInitialUpdate);

	if (!m_pTabManagers)
		m_pTabManagers = new TabManagers;

	for (int i = 0; i < m_pTabManagers->GetCount(); i++)
	{
		CTabManager* pTabMng = m_pTabManagers->GetAt(i);
		if (!pTabMng)
			continue;
		CString sTabMngNS = pTabMng->GetNamespace().GetObjectName();
		int res = sTabMngNS.CompareNoCase(sName);
		ASSERT(res != 0);
		TRACE(_T("A tile manager with the same name ") + sName + _T(" already exists! Please verify the uniqueness for all tile managers! "));
	}

	m_pTabManagers->Add(pTabManager);
	

	return pTabManager;
}

//-----------------------------------------------------------------------------
void CAbstractFormView::ActivateTabDialogs(TabManagers* pTabManagers) 
{
	if (pTabManagers == NULL) return ;

	for (int i = 0; i < pTabManagers->GetSize(); i++)
	{
		CTabManager *pTab = pTabManagers->GetAt(i);
		DlgInfoArray *pInfoArray = pTab->GetDlgInfoArray();
		
		//tengo traccia della tab corrente
		CTabDialog *pDialog = pTab->GetActiveDlg();
		UINT nOriginalActiveDialogID = pDialog ? pDialog->GetDialogID() : 0;

		for (int j = 0; j < pInfoArray->GetSize(); j++)
		{
			DlgInfoItem* pDlgItem = pInfoArray->GetAt(j);
			pTab->TabDialogActivate(pTab->GetDlgCtrlID(), pDlgItem->GetDialogID());
			
			pDialog = pTab->GetActiveDlg();

			if (pDialog)
				ActivateTabDialogs(pDialog->GetChildTabManagers());
		}
		// riattivo la tab attiva in origine 
		if (nOriginalActiveDialogID)
			pTab->TabDialogActivate(pTab->GetDlgCtrlID(), nOriginalActiveDialogID);	
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormView::PerformBatchOperations()
{
	ActivateTabDialogs(m_pTabManagers);
	GetDocument()->BatchStart();
}
//-----------------------------------------------------------------------------
BOOL CAbstractFormView::ReCreateControls()
{
	//distruggo tutte le finestre
	CWnd* pCtrl = GetWindow(GW_CHILD);
	CWnd* pPrevCtrl = NULL;
	for (; pCtrl; pCtrl = pCtrl->GetNextWindow())
	{
		//non distruggo la corrente, altrimenti non funziona la GetNextWindow
		if (pPrevCtrl)
			pPrevCtrl->DestroyWindow();
		pPrevCtrl = pCtrl;
	}
	if (pPrevCtrl)
		pPrevCtrl->DestroyWindow();
	
	//pulisco  i tab managers ed i tile managers
	m_pTabManagers->RemoveAll();
	m_pTileGroups->RemoveAll();
	
	//pulisco gli elementi di layout
	m_pLayoutContainer->ClearChildElements();
	
	m_HWNDPositionsMap.RemoveAll();
	m_Panel.RemoveAll();
	m_Excluded.RemoveAll();
	
	m_pControlLinks->RemoveAll();

	if (m_pJsonContext && m_pJsonContext->m_pDescription)
	{
		m_pJsonContext->m_pDescription->EvaluateExpressions(m_pJsonContext);
		//ricreo le finestre
		CJsonFormEngineObj::GetInstance()->CreateChilds(m_pJsonContext, this);
	}
	// rifaccio le addlink
	OnInitialUpdate();

	//Prj. 6709 - in questo caso di ricreazione della finestra non la nascondo
	ShowWindow(SW_SHOW);
	m_bTemporaryHidden = FALSE;
	if (m_pLayoutContainer)
	{
		CRect rcView;
		GetClientRect(&rcView);
		Relayout(rcView);
	}
	return TRUE;
}
//----------------------------------------------
CTabManager* CAbstractFormView::CreateTabManager(UINT nIDC, CRuntimeClass* pClass, UINT nExpandHeight, const CString& sName)
{
	//---- disabilito momentaneamente il resize automatico dei control dei control che lo prevedono
	int i = 0;
	for (i = 0 ; i < m_pControlLinks->GetSize(); i++)
	{
		ResizableCtrl* pwndChild = dynamic_cast<ResizableCtrl*>(m_pControlLinks->GetAt(i));
		if (pwndChild)
			pwndChild->InitSizeInfo(NULL);
	}
	
	//---- aumento l'altezza della finestra frame (che poi pilota la view)
	CRect rectView;
	GetClientRect(rectView);
	int nHView = rectView.Height();
	int nWView = rectView.Width();

	CAbstractFormFrame* pFrame = GetFrame();
	if (!pFrame)
	{
		ASSERT(FALSE);
		return NULL;
	}
	CRect rectFrame;
	pFrame->GetWindowRect(rectFrame);
	int nHFrame = rectFrame.Height();
	int nWFrame = rectFrame.Width();
	
	CRect rectMsgBar;
	if (pFrame->GetMsgBar())
		pFrame->GetMsgBar()->GetWindowRect(rectMsgBar);

	int nHeightToAdd = nExpandHeight + rectMsgBar.Height();
	pFrame->SetWindowPos (NULL, 0,0, nWFrame, nHFrame + nHeightToAdd, SWP_NOMOVE|SWP_NOZORDER);
	
	//---- Creo dinamicamente il pulsante segnaposto
	CRect rectPB (2, nHView, nWView -4, nHView + nHeightToAdd -2);
	CButton pb;
	pb.Create
			( 
				_T("tabmanager"),
				WS_VISIBLE | WS_TABSTOP,
				rectPB, 
				this, 
				nIDC
			);

	//---- Riabilito ridemensionamento automatico dei control che lo prevedono
	for (i=0 ; i < m_pControlLinks->GetSize(); i++)
	{
		ResizableCtrl* pwndChild = dynamic_cast<ResizableCtrl*>(m_pControlLinks->GetAt(i));
		if (pwndChild)
			pwndChild->InitSizeInfo(m_pControlLinks->GetAt(i));
	}

	//---- Subclasso il pulsante e creo il nuovo TabManager
	return AddTabManager(nIDC, pClass, sName);
}

//-------------------------------------------------------------------------------------------
void CAbstractFormView::ShiftControl (UINT nTop, UINT nExpandHeight)
{
	//---- disabilito momentaneamente il resize automatico dei control dei control che lo prevedono
	int i = 0;
	for (i = 0 ; i < m_pControlLinks->GetSize(); i++)
	{
		ResizableCtrl* pwndChild = dynamic_cast<ResizableCtrl*>(m_pControlLinks->GetAt(i));
		if (pwndChild)
			pwndChild->InitSizeInfo(NULL);
	}
	
	CAbstractFormFrame* pFrame = GetFrame();
	if (!pFrame)
	{
		ASSERT(FALSE);
		return;
	}

	//---- aumento l'altezza della finestra frame (che poi pilota la view)
	CRect rectFrame;
	pFrame->GetWindowRect(rectFrame);
	int nHFrame = rectFrame.Height();
	int nWFrame = rectFrame.Width();
	
	CRect rectMsgBar;
	if (pFrame->GetMsgBar())
		pFrame->GetMsgBar()->GetWindowRect(rectMsgBar);

	int nHeightToAdd = nExpandHeight + rectMsgBar.Height();

	pFrame->SetWindowPos (NULL, 0,0, nWFrame, nHFrame + nHeightToAdd, SWP_NOMOVE|SWP_NOZORDER|SWP_SHOWWINDOW);
	pFrame->RecalcLayout();
	
	//---- 
	CParsedForm::ShiftControl (nTop, nExpandHeight);

	//---- Riabilito ridemensionamento automatico dei control che lo prevedono
	for (i=0 ; i < m_pControlLinks->GetSize(); i++)
	{
		ResizableCtrl* pwndChild = dynamic_cast<ResizableCtrl*>(m_pControlLinks->GetAt(i));
		if (pwndChild)
			pwndChild->InitSizeInfo(m_pControlLinks->GetAt(i));
	}
	//pFrame->RecalcLayout();
}

//-----------------------------------------------------------------------------
void CAbstractFormView::MoveControls(CSize offset)
{
	::MoveControls(this, offset, m_pControlLinks);
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormView::SetControlValue(UINT nIDC, const DataObj& val)
{
	return m_pControlLinks->SetControlValue(nIDC, val);
}

//----------------------------------------------------------------------------
CAbstractFormFrame* CAbstractFormView::GetFrame() const
{
	CFrameWnd* tempWnd = GetParentFrame(); 
	if (tempWnd)
		return dynamic_cast<CAbstractFormFrame*>(tempWnd);

	return NULL;
}

//----------------------------------------------------------------------------
void CAbstractFormView::CalculateOSLInfo()
{
	CInfoOSL* pParent = NULL;
	if (GetParent() && GetParent()->IsKindOf(RUNTIME_CLASS(CTaskBuilderDockPane)))
	{
		pParent = ((CTaskBuilderDockPane*) GetParent())->GetInfoOSL();
		GetInfoOSL()->SetType(OSLType_SlaveTemplate);
	}
	else
	{
		if (GetInfoOSL()->GetType() == OSLType_Null)
			GetInfoOSL()->SetType(OSLType_SlaveTemplate);

		if (GetDocument())
			pParent = GetDocument()->GetInfoOSL();
	}

	GetInfoOSL()->m_pParent = pParent;
	if (pParent)
		GetNamespace().SetChildNamespace(CTBNamespace::FORM, m_sName, pParent->m_Namespace);
}

//----------------------------------------------------------------------------
void CAbstractFormView::OnInitialUpdate()
{
	CBaseFormView::OnInitialUpdate();

	//ASSERT(GetDocument() != NULL);

//	CalculateOSLInfo	(); TODO LARA
	 
	CAbstractFormFrame* pFrame  = GetFrame();
	if (pFrame && GetParent() == pFrame)
	{
		// resize view to dialog size
		pFrame->SetFrameSize(GetDialogSize());
		//perché la view dovrebbe modificare l'id della frame?
		//pFrame->SetIDHelp	(m_nID);
	}
	
	CAbstractFormDoc* pDoc = GetDocument();

	if (AfxIsRemoteInterface())
	{
		if (m_pJsonContext)
			CJsonFormEngineObj::GetInstance()->BuildWebControlLinks(this, m_pJsonContext);
	}
	else
	{
		//Prj. 6709 - tentativo di risolvere il flickering
		if (m_pJsonContext)
		{
			CWndObjDescription* pWndDescription = dynamic_cast<CWndObjDescription*>(m_pJsonContext->m_pDescription);
			
			CTaskBuilderDockPane* pDockPane = dynamic_cast<CTaskBuilderDockPane*>(GetParent());
			CTaskBuilderDockPaneTabs* pDockPaneTabs = dynamic_cast<CTaskBuilderDockPaneTabs*>(GetParent());

			if (pWndDescription && !pDockPane && !pDockPaneTabs)
			{
				//memorizzo lo stato iniziale della finestra
				SetNativeWindowVisible(pWndDescription->m_bVisible);
				//provo a vedere se il frame e modal
				CJsonFrame* pOwnFrame = dynamic_cast<CJsonFrame*>(GetFrame());
				BOOL bIsModalFrame = FALSE;
				if (pOwnFrame)
				{
					CWndFrameDescription* pFrameDesc = pOwnFrame->GetFrameDescription();
					if (pFrameDesc)
						bIsModalFrame = pFrameDesc->m_bModalFrame;
				}
				//se view "modale" (vedi ExtDoc.cpp - CreateSlaveView) oppure frame "modale", NON DEVO fare HIDE/SHOW
				SetModal(IsModal() || bIsModalFrame);
				if (!IsModal())
				{
					ShowWindow(SW_HIDE);
					m_bTemporaryHidden = TRUE;
				}

	
			}
		}
		//END Prj. 6709 - tentativo di risolvere il flickering


		// Collega i dataObjs agli specifici controls nella dialog
		if (m_pJsonContext)
			m_pJsonContext->BuildDataControlLinks();
		BuildDataControlLinks();
	}
	// give the client docs the opportunity to add tiles in the view's tilegroups
	if (m_pTileGroups) 
		for (int g = 0; g <= m_pTileGroups->GetUpperBound(); g++)
		{
			CTileGroup* pTileGroup = m_pTileGroups->GetAt(g);
			if (pDoc) pDoc->AddClientDocTileDialog(pTileGroup);
		}
	// possibilità ai clientdoc di intervenire sulla BuildDataControlLinks
	if (pDoc)
		pDoc->OnBuildDataControlLinks(this);

	ApplyTBVisualManager();
	
	if (!m_pJsonContext)//se sono json, il font letto dai settings è già impostato
		SetDefaultFont();
	
	

	SendMessageToDescendants(UM_INITIALIZE_TILE_LAYOUT, (WPARAM)NULL, (LPARAM)this);
	//RequestRelayout();
	NotifyLayoutChanged();


	//TODO le CRowFormView DEVONO allineare anche m_DataToCtrlMap valorizzato nelle AddLink
	//if (!IsKindOf(RUNTIME_CLASS(CRowFormView)))
	//	m_pControlLinks->AlignToTabOrder (this);	

	// chiama quella locale alla view per dare la possibilita' locale
	// di chiamare hotlink e tblreader non aggiornati quando la view non
	// e' aperta (ad esempio slaveview)	
	OnPrepareAuxData();
	//do la possibilita' di dire la loro anche ai clientdoc
	if (GetDocument()) GetDocument()->OnPrepareAuxData(this);

	// Abilita i controls sulla base dello stato del documento
	if (GetDocument()) EnableViewControls();

	OnModifyDataObjs(m_pControlLinks, TRUE);//in fase di visualizzazione iniziale, tutti i controlli sono da considerare modificati
	if (GetDocument()) GetDocument()->UpdateDataView();
	SetDefaultFocus();

	//se il documento e' predisposto per le autoexpression inizializzo i controlli
	//che hanno un/espressione associata
	if (GetDocument() && 
		GetDocument()->m_pAutoExpressionMng		&&
		GetDocument()->m_pVariablesArray)
	{
		for (int i= 0; i < GetDocument()->m_pAutoExpressionMng->GetSize(); i++)
		{
			for(int n = 0 ; n < GetDocument()->m_pVariablesArray->GetSize() ; n++)
			{
				if(GetDocument()->m_pVariablesArray->GetAt(n)->GetName().CompareNoCase(GetDocument()->m_pAutoExpressionMng->GetAt(i)->GetVarName()) == 0)
					SetControlAutomaticExpression(
						GetDocument()->m_pVariablesArray->GetAt(n)->GetDataObj(), 
						GetDocument()->m_pAutoExpressionMng->GetAt(i)->GetExpression()
						);
			}
		}
	}
    //utilizzato per centratura controlli
	m_bInitialUpdateDone =  TRUE;

	//se e' stata personalizzata la centratura dei controlli in via programmativa non leggo il setting globale
	if (!IsCenterControlsCustomized())
		SetCenterControls(AfxCenterControlsEnabled());
	
	if (GetCenterControls() && (!m_pTabManagers || m_pTabManagers->GetSize() == 0))
	{
		CRect clienRect;
		GetClientRect(clienRect);
		CenterControls(this, clienRect.Width(), clienRect.Height());
	}

	if (GetDocument() && GetDocument()->IsExternalControlled())
		SyncExternalControllerInfo(FALSE);
}

//-----------------------------------------------------------------------------
BOOL CAbstractFormView::PreTranslateMessage(MSG* pMsg)
{
#ifndef _OLD_PTM

	if (!GetDocument())
		return __super::PreTranslateMessage(pMsg);

	if	(
			(!GetDocument()->m_bForwardingSysKeydownToChild && !GetDocument()->m_bForwardingSysKeydownToParent ||
			GetDocument()->m_bForwardingSysKeydownToParent)
		)
	{
		// since next statement will eat frame window accelerators,
		//   we call the ParsedForm::PreProcessMessage first
		if (CParsedForm::PreProcessMessage(pMsg))
			return TRUE;

		BOOL bHoldForwardingSysKeydownToChild = GetDocument()->m_bForwardingSysKeydownToChild;
		GetDocument()->m_bForwardingSysKeydownToChild = TRUE;

		BOOL bOk = FALSE;
		for (int i = 0; i <= m_pTabManagers->GetUpperBound() && !bOk; i++)
		{
			CTabManager* pTabber = m_pTabManagers->GetAt(i);
			if (!pTabber)
				continue;

			if (GetDocument()->m_bForwardingSysKeydownToParent && (LPARAM)pTabber->GetSafeHwnd() == pMsg->lParam)
				continue;

			bOk = pTabber->PreTranslateMessage(pMsg);
		}

		GetDocument()->m_bForwardingSysKeydownToChild = bHoldForwardingSysKeydownToChild;

		if (bOk)
			return TRUE;

		GetDocument()->m_bForwardingSysKeydownToChild = TRUE;

		// ruota l'azione alle eventuali TileDialog presenti
		for (int i = 0; i <= m_pTileGroups->GetUpperBound() && !bOk; i++)
		{
			CBaseTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
			if (GetDocument() && GetDocument()->m_bForwardingSysKeydownToParent && pTileGroup && (LPARAM)pTileGroup->GetSafeHwnd() == pMsg->lParam)
				continue;

			bOk = pTileGroup && pTileGroup->PreTranslateMessage(pMsg);
		}

		GetDocument()->m_bForwardingSysKeydownToChild = FALSE;

		if (bOk)
			return TRUE;
	}

	if (GetDocument()->m_bForwardingSysKeydownToChild)
		return FALSE;

	return CTaskBuilderTabWnd::PreProcessSysKeyMessage(pMsg, GetDocument(), this) || __super::PreTranslateMessage(pMsg);

#else

	//in design mode solo il form editor gestisce i messaggi
	if (GetDocument() && GetDocument()->IsInDesignMode() && pMsg->message >= WM_KEYFIRST && pMsg->message <= WM_KEYLAST)
		return FALSE;

	// since next statement will eat frame window accelerators,
	//   we call the ParsedForm::PreProcessMessage first
	if (!m_pTabManagers || CParsedForm::PreProcessMessage(pMsg))
		return TRUE;

	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CTabManager* pTabber = m_pTabManagers->GetAt(i);
		if (pTabber && pTabber->PreTranslateMessage(pMsg))
			return TRUE;
	}

	// ruota l'azione alle eventuali TileDialog presenti
	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CBaseTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		for (int j = 0; j <= pTileGroup->GetTileDialogs()->GetUpperBound(); j++)
		{
			CWnd* pTileDialog = pTileGroup->GetTileDialogs()->GetAt(j);
			if (pTileDialog && pTileDialog->PreTranslateMessage(pMsg)) 
				return TRUE;
		}
	}

	return __super::PreTranslateMessage(pMsg);

#endif
}

// Standard beahaviour for value changed message
//--------------------------------------------------------------------------
LRESULT CAbstractFormView::OnValueChanged(WPARAM wParam, LPARAM lParam)
{
	if (GetDocument() && GetDocument()->OnValueChanged(wParam, lParam) == 0)
	{
		GetDocument()->UpdateDataView ();
		return 0L;
	}

	return 1L;
}

//-----------------------------------------------------------------------------
LRESULT CAbstractFormView::OnRunBatch(WPARAM wParam, LPARAM)
{
	if (GetDocument()->GetType() != VMT_BATCH) return 0;
    
    // Se sta runnando il processo batch lo ferma altrimenti lo lancia
	if ((BOOL)wParam) 
		GetDocument()->BatchStart();
	else
		GetDocument()->BatchStop();
	return 0;
}

//------------------------------------------------------------------------------
void CAbstractFormView::SetCustomMoveTopWindows(int top)
{
	if (top == 0)		
		m_mwTop = 0;
	if (m_mwTop < top)	
		m_mwTop = top;
}

//------------------------------------------------------------------------------
void CAbstractFormView::OnSize(UINT nType, int cx, int cy) 
{	
	if (!IsWindow(this->m_hWnd))
		return;
	//Prj. 6709 - tentativo fi risolvere il flickering
	//tardare la SHOW della finestra più possibile, cioè fino alla resume della toolbar/tabbedtoolbar
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	//con questa modifica le finestre modali sono escluse dal algoritmo di flickering
	if (pFrame && pFrame->IsLayoutSuspended(!IsModal()))
		return;
	//end flickerig

	__super::OnSize(nType, cx, cy);

	if (pFrame)
	{
		CView* pView = pFrame->GetActiveView();
		if (pView && m_mwTop != 0)
			pView->SetWindowPos(NULL, 0, m_mwTop - 3, cx, cy + m_mwTop, SWP_NOSIZE | SWP_NOZORDER);
	}

	if (!GetCenterControls() || (m_pTabManagers && m_pTabManagers->GetSize()))
	{
		//visualizza la finestra se è stata nascosta e reset flag m_bTemporaryHidden
		if (GetNativeWindowVisible() && m_bTemporaryHidden && GetDocument()->IsInDesignMode() != CBaseDocument::DesignMode::DM_RUNTIME)
		{
			
			ShowWindow(SW_SHOW);
			m_bTemporaryHidden = FALSE;
		}
		
		return;
	}

	if (m_bInitialUpdateDone)
		CenterControls(this, cx, cy);

	//visualizza la finestra se è stata nascosta e reset flag m_bTemporaryHidden
	if (GetNativeWindowVisible() && m_bTemporaryHidden && GetDocument()->IsInDesignMode() != CBaseDocument::DesignMode::DM_RUNTIME)
	{
		ShowWindow(SW_SHOW);
		GetDocument()->UpdateFrameCounts();
		m_bTemporaryHidden = FALSE;
	}
}

//-----------------------------------------------------------------------------
void CAbstractFormView::OnRButtonDown(UINT nFlag, CPoint mousePos)
{
	CBaseFormView::OnRButtonDown(nFlag, mousePos);
	CWnd* pWnd = ChildWindowFromPoint(mousePos);
	if (pWnd)
	{
		CParsedCtrl* pParsed = GetParsedCtrl(pWnd);
		if (GetDocument() && pParsed)
		{
			CMenu   menu;
			menu.CreatePopupMenu();

			if (
					GetDocument()->ShowingPopupMenu(pParsed->GetCtrlID(), &menu) &&
					menu.GetMenuItemCount() > 0
				)	
			{
				CRect ItemRect;
				GetWindowRect(ItemRect);
				CPoint point = ItemRect.TopLeft();
				point += mousePos;
				menu.TrackPopupMenu (TPM_LEFTBUTTON, point.x, point.y, this);
			}
		}
	}
}

//-----------------------------------------------------------------------------
LRESULT CAbstractFormView::OnGetLocalizerInfo(WPARAM wParam, LPARAM lParam)
{
	return GetLocalizerInfo(wParam, lParam);
}
//-----------------------------------------------------------------------------
LRESULT CAbstractFormView::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*) wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit). 
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	/*
	CString strId = (LPCTSTR)lParam;
	CWndImageDescription* pDesc = (CWndImageDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndImageDescription), strId);
	pDesc->UpdateAttributes(this);
	pDesc->m_Type = CWndObjDescription::View;

	AddBkgndImageDescription(pDesc);

	pDesc->AddChildWindows(this);*/
	CWndObjDescription *pDesc = m_pJsonContext->m_pDescription;
	if (pContainer->IndexOf(pDesc) == -1)
	{
		m_pJsonContext->m_bOwnDescription = false;
		pContainer->Add(pDesc);
		pDesc->SetParent(pContainer->GetParent());
		pDesc->SetAdded(true);
	}
	//m_pJsonContext->m_pDescription->SetUnchanged(true);
	return (LRESULT)pDesc;
}

//-----------------------------------------------------------------------------
void CAbstractFormView::SyncExternalControllerInfo(BOOL bSave)
{
	if (!GetDocument())
		return;

	if (bSave)
		GetDocument()->RetrieveControlData(m_pControlLinks);
	else
		GetDocument()->ValorizeControlData(m_pControlLinks, FALSE);

	if (this->m_pTileGroups == NULL)
		return;

	for (int i = 0; i < this->m_pTileGroups->GetSize(); i++)
	{
		CTileGroup* tileGroup = m_pTileGroups->GetAt(i);
		ASSERT_VALID(tileGroup);

		for (int ii = 0; ii < tileGroup->GetTileDialogs()->GetSize(); ii++)
		{

			CTileDialog* dialog = dynamic_cast<CTileDialog*>(tileGroup->GetTileDialogs()->GetAt(ii));
			ASSERT_VALID(dialog);

			if (bSave)
				dialog->GetDocument()->RetrieveControlData(dialog->GetControlLinks());
			else
				dialog->GetDocument()->ValorizeControlData(dialog->GetControlLinks(), FALSE);

		}

		for (int j = 0; j < tileGroup->GetTilePanels()->GetSize(); j++)
		{
			CTilePanel* tilePanel = dynamic_cast<CTilePanel*>(tileGroup->GetTilePanels()->GetAt(j));
			ASSERT(tilePanel);
		}

	}
}

//-----------------------------------------------------------------------------

/////////////////////////////////////////////////////////////////////////////
// CAbstractFormView Scroll support



//------------------------------------------------------------------------------
void CAbstractFormView::OnScrollUp		() { OnVScroll (SB_PAGEUP,		0, GetScrollBarCtrl(SB_VERT)); }
void CAbstractFormView::OnScrollDown	() { OnVScroll (SB_PAGEDOWN,	0, GetScrollBarCtrl(SB_VERT)); }
void CAbstractFormView::OnScrollLeft	() { OnHScroll (SB_PAGELEFT,	0, GetScrollBarCtrl(SB_HORZ)); }
void CAbstractFormView::OnScrollRight	() { OnHScroll (SB_PAGERIGHT,	0, GetScrollBarCtrl(SB_HORZ)); }

//------------------------------------------------------------------------------
void CAbstractFormView::OnScrollTop	() 
{
	OnVScroll (SB_TOP,	0, GetScrollBarCtrl(SB_VERT));
	OnHScroll (SB_LEFT,	0, GetScrollBarCtrl(SB_VERT));
}

//------------------------------------------------------------------------------
void CAbstractFormView::OnScrollBotton	()
{
	OnVScroll (SB_BOTTOM,	0, GetScrollBarCtrl(SB_VERT));
	OnHScroll (SB_LEFT,		0, GetScrollBarCtrl(SB_VERT));
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CAbstractFormView::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CAbstractFormView\n");
}

void CAbstractFormView::AssertValid() const
{
	__super::AssertValid();
}
#endif //_DEBUG


//==============================================================================
//	CMasterFormView
//==============================================================================

IMPLEMENT_DYNAMIC(CMasterFormView, CAbstractFormView)

BEGIN_MESSAGE_MAP(CMasterFormView, CAbstractFormView)
	//{{AFX_MSG_MAP(CMasterFormView)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CMasterFormView::CMasterFormView(const CString& sName, UINT nIDTemplate)
	:
	CAbstractFormView (sName, nIDTemplate)
{
	GetInfoOSL()->SetType(OSLType_Template);
}

//-----------------------------------------------------------------------------
CMasterFrame* CMasterFormView::GetFrame() const
{ 
	CMasterFrame* pFrame = dynamic_cast<CMasterFrame*>(GetParentFrame());
	ASSERT(pFrame);
	return pFrame;
}
//-----------------------------------------------------------------------------
void CMasterFormView::OnInitialUpdate()
{
	if (m_pJsonContext && m_pJsonContext->m_pDescription && !m_pJsonContext->m_pDescription->m_strText.IsEmpty())
	{
		CFrameWnd* pWnd = GetParentFrame();
		if (GetDocument()->GetFirstView() == this)
		{
			//valorizzo eventuali variabili calcolate come espressione prima di utilizzare il testo per il titolo
			m_pJsonContext->m_pDescription->EvaluateExpressions(m_pJsonContext);
		
			CString sTitle = AfxLoadJsonString(m_pJsonContext->m_pDescription->m_strText, m_pJsonContext->m_pDescription);
			GetDocument()->SetFormTitle(sTitle);
		}
	}
	__super::OnInitialUpdate();
	
}
/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CMasterFormView::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CMasterFormView\n");
	CAbstractFormView::Dump(dc);
}

void CMasterFormView::AssertValid() const
{
	CAbstractFormView::AssertValid();
}
#endif //_DEBUG

//==============================================================================
//	CSlaveFormView
//==============================================================================

IMPLEMENT_DYNAMIC(CSlaveFormView, CAbstractFormView)

BEGIN_MESSAGE_MAP(CSlaveFormView, CAbstractFormView)
	//{{AFX_MSG_MAP(CSlaveFormView)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CSlaveFormView::CSlaveFormView(const CString& sName, UINT nIDTemplate)
	:
	CAbstractFormView	(sName, nIDTemplate)
{
	GetInfoOSL()->SetType(OSLType_SlaveTemplate);
}

//---------------------------------------------------------------------------
void CSlaveFormView::EnableViewControlLinks	(BOOL bEnable/* = TRUE*/, BOOL bMustSetOSLReadOnly/*=FALSE*/)
{
	switch (GetDocument()->GetFormMode())
	{
			//case CBaseDocument::BROWSE:
			//case CBaseDocument::FIND:
							
			case CBaseDocument::NEW:
				bMustSetOSLReadOnly = bMustSetOSLReadOnly || (OSL_CAN_DO( GetDocument()->GetInfoOSL(), OSL_GRANT_NEW) == 0);
				break;

			case CBaseDocument::EDIT:
				bMustSetOSLReadOnly = bMustSetOSLReadOnly || (OSL_CAN_DO( GetDocument()->GetInfoOSL(), OSL_GRANT_EDIT) == 0);
				break;
	}

	CAbstractFormView::EnableViewControlLinks (bEnable, bMustSetOSLReadOnly);
}
//-----------------------------------------------------------------------------
void CSlaveFormView::SetDefaultFocus()
{
	ASSERT(GetParentFrame());
	ASSERT_KINDOF(CSlaveFrame, GetParentFrame());
	CSlaveFrame* pFrame = (CSlaveFrame*)GetParentFrame();
	if (pFrame->IsPopup())
		__super::SetDefaultFocus();//le frame embedded non devono impostare il defaul focus
}

//-----------------------------------------------------------------------------
void CSlaveFormView::OnActivateView(BOOL bActive, CView* pActivateView, CView* pDeactiveView)
{
	ASSERT(GetParentFrame());
	ASSERT_KINDOF(CSlaveFrame, GetParentFrame());
	CSlaveFrame* pFrame = (CSlaveFrame*)GetParentFrame();
	if (pFrame->IsPopup())
	{
		__super::OnActivateView(bActive, pActivateView, pDeactiveView);
	}
}
//-----------------------------------------------------------------------------

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CSlaveFormView::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CSlaveFormView\n");
	CAbstractFormView::Dump(dc);
}

void CSlaveFormView::AssertValid() const
{
	CAbstractFormView::AssertValid();
}
#endif //_DEBUG

//==============================================================================
//	CJsonSlaveFormView
//==============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonSlaveFormView, CSlaveFormView)
//-----------------------------------------------------------------------------
CJsonSlaveFormView::CJsonSlaveFormView()
	:
	CSlaveFormView(_T(""), 0)
{
}
//-----------------------------------------------------------------------------
CJsonSlaveFormView::CJsonSlaveFormView(UINT nIDTemplate)
	:
	CSlaveFormView(_T(""), nIDTemplate)
{
}

//==============================================================================
//	CSlaveFixedSizeView
//==============================================================================

IMPLEMENT_DYNAMIC(CSlaveFixedSizeView, CSlaveFormView)

CSlaveFixedSizeView::CSlaveFixedSizeView(const CString& sName, UINT nIDTemplate)
	:
	CSlaveFormView(sName, nIDTemplate)
{
	SetCenterControls(FALSE); 
}

//-----------------------------------------------------------------------------
BOOL CSlaveFixedSizeView::PreCreateWindow(CREATESTRUCT& cs) 
{
	BOOL bOk = __super::PreCreateWindow(cs);

	cs.style		= cs.style		& ~(WS_BORDER|WS_DLGFRAME|WS_THICKFRAME);
	cs.dwExStyle	= cs.dwExStyle	& ~(WS_EX_STATICEDGE|WS_EX_CLIENTEDGE|WS_EX_WINDOWEDGE);
	return bOk;
}

//==============================================================================
//	CRowFormView
//==============================================================================

IMPLEMENT_DYNAMIC(CRowFormView, CAbstractFormView)

BEGIN_MESSAGE_MAP(CRowFormView, CAbstractFormView)
	//{{AFX_MSG_MAP(CRowFormView)
	ON_WM_SETFOCUS()

	ON_MESSAGE (UM_VALUE_CHANGED,	OnValueChanged)

	ON_COMMAND(ID_EXTDOC_PREV_ROW,	OnMoveToPrevRow)
	ON_COMMAND(ID_EXTDOC_NEXT_ROW,	OnMoveToNextRow)
	ON_COMMAND(ID_EXTDOC_FIRST_ROW,	OnMoveToFirstRow)
	ON_COMMAND(ID_EXTDOC_LAST_ROW,	OnMoveToLastRow)
	ON_COMMAND(ID_EXTDOC_GOTO_MASTER, OnGotoMaster)

	ON_UPDATE_COMMAND_UI(ID_EXTDOC_PREV_ROW,	OnUpdateMoveToPrevRow)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_NEXT_ROW,	OnUpdateMoveToNextRow)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_FIRST_ROW,	OnUpdateMoveToFirstRow)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_LAST_ROW,	OnUpdateMoveToLastRow)

	ON_COMMAND(ID_EXTDOC_DELETE_ROW,			OnDeleteRow)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_DELETE_ROW,	OnUpdateDeleteRow)

	ON_COMMAND(ID_EXTDOC_INSERT_ROW,			OnInsertRow)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_INSERT_ROW,	OnUpdateInsertRow)

	ON_UPDATE_COMMAND_UI(ID_EXTDOC_TOTAL_RECORDS_INDICATORS, 	OnUpdateTotalRecordsIndicator)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CRowFormView::CRowFormView(const CString& sName, UINT nIDTemplate)
	:
	CAbstractFormView		(sName, nIDTemplate),
	m_pBodyEdit				(NULL),
	m_pTemplateRecordClass	(NULL)
{
	GetInfoOSL()->SetType(OSLType_RowSlaveTemplate);
}

//-----------------------------------------------------------------------------
CRowFormView::~CRowFormView()
{
	// signal only if attacched to body edit
	if (m_pBodyEdit)
		m_pBodyEdit->OnRowFormViewDied(this);
}

//----------------------------------------------------------------------------
void CRowFormView::OnInitialUpdate()
{
	if (GetDocument() && GetDocument()->GetFirstView())
	{
		CBaseFormView* pView = dynamic_cast<CBaseFormView*>(GetDocument()->GetFirstView());
		
		// if the main document view has a layout, the row view has also a layout
		if (pView && pView->GetLayoutContainer())
			EnableLayout(TRUE);
	}
	__super::OnInitialUpdate();
}

//-----------------------------------------------------------------------------
void CRowFormView::SetDefaultFocus()
{
	ASSERT(GetParentFrame());
	ASSERT_KINDOF(CRowFormFrame, GetParentFrame());
	CRowFormFrame* pFrame = (CRowFormFrame*)GetParentFrame();
	if (pFrame->IsPopup())
		__super::SetDefaultFocus();//le frame embedded non devono impostare il defaul focus
}

//-----------------------------------------------------------------------------
void CRowFormView::Attach(CBodyEdit* pBodyEdit)
{
	ASSERT(m_pBodyEdit == NULL);
	ASSERT_VALID(pBodyEdit);

	m_pBodyEdit = pBodyEdit;
	m_pBodyEdit->m_strRowFormViewName = m_sName;

	DBTSlaveBuffered* pDBT = GetDBT();
	if (!pDBT)
	{
		ASSERT_TRACE(FALSE,"CRowFormView::Attach: the DBTSlaveBuffered is null!\n");
		return;
	}
	ASSERT_VALID(pDBT);

	if (!m_pTemplateRecordClass)
	{
		m_pTemplateRecordClass = pDBT->GetRecord()->GetRuntimeClass();
	}
	else if (!pDBT->GetRecord()->GetRuntimeClass()->IsDerivedFrom(m_pTemplateRecordClass))
	{
		ASSERT_TRACE3(FALSE,
			"CRowFormView::Attach: the AddLink of fields of the records don't belong to DBTSlaveBuffered should be done as last ones!\nRowView class: %s, DBT record class: %s, template record class: %s\n",
			GetRuntimeClass()->m_lpszClassName,
			pDBT->GetRecord()->GetRuntimeClass()->m_lpszClassName,
			(m_pTemplateRecordClass ? m_pTemplateRecordClass->m_lpszClassName : (LPCSTR)_T(""))
		);
		m_pTemplateRecordClass = pDBT->GetRecord()->GetRuntimeClass();
	}
}

//-----------------------------------------------------------------------------
void CRowFormView::Detach()
{
	if (m_pBodyEdit)
		m_pBodyEdit = NULL;
}

//-----------------------------------------------------------------------------
DBTSlaveBuffered* CRowFormView::GetDBT () const	
{
	if (m_pBodyEdit)
	{
		ASSERT_VALID(m_pBodyEdit);
		return m_pBodyEdit->GetDBT();
	}
	return  NULL; 
} 

//-----------------------------------------------------------------------------
CBodyEdit* CRowFormView::GetBodyEdit () const	
{
	ASSERT_VALID(m_pBodyEdit);
	return  m_pBodyEdit; 
} 

//-----------------------------------------------------------------------------
void CRowFormView::EnableMappedControlLinks (ControlLinks* pControlLinks, const Array& aDataToCtrlMap, BOOL bEnable, BOOL bMustSetOSLReadOnly)
{
	for (int nCLIdx = 0; nCLIdx < pControlLinks->GetSize(); nCLIdx++)
	{
		CWnd* pWnd = pControlLinks->GetAt(nCLIdx);
		ASSERT(pWnd);

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);

		if (pControl)
		{
			BOOL bRecordData = FALSE;

			if	(
					!bEnable && 
					pControl->GetButton() &&
					(pControl->GetDataType() == DataType::Date || pControl->GetDataType() == DataType::DateTime)
				)
				pControl->GetButton()->PostMessage(UM_DESTROY_CALENDAR);

			 //cerca l'eventuale corrispondenza di questo control nella mappa che collega
			 //l'indice delle colonna del record con l'indice del vettore dei ControlLinks
			for (int j = 0; j <= aDataToCtrlMap.GetUpperBound(); j++)
			{
				DataToCtrlLink*	pDataToCtrlLink	= (DataToCtrlLink*) aDataToCtrlMap[j];
				if (nCLIdx == pDataToCtrlLink->m_nControlLinkIdx)
				{
					bRecordData = TRUE;
					break;
				}
			}

			if (pControl && !bRecordData)
			{	
				pControl->SetDataReadOnly(!bEnable);

				if (bMustSetOSLReadOnly)
					pControl->SetDataOSLReadOnly(TRUE);
			}
		}
	}
}

//------------------------------------------------------------------------------
void CRowFormView::OnUpdateMappedControls(ControlLinks* pControlLinks, const Array& aDataToCtrlMap)
{
	DBTSlaveBuffered* pDbt = GetDBT();
	for (int nCLIdx = 0; nCLIdx < pControlLinks->GetSize(); nCLIdx++)
	{
		CWnd* pWnd = pControlLinks->GetAt(nCLIdx);
		ASSERT(pWnd);

		CParsedCtrl* pControl = GetParsedCtrl(pWnd);

		if (pControl)
		{
			if ( (!pDbt || pDbt->GetCurrentRowIdx() == -1) && !pWnd->IsKindOf(RUNTIME_CLASS(CParsedStatic)))
			{
				pControl->EnableCtrl(FALSE); 
				continue;
			}
			
			BOOL bRecordData = FALSE;

			/*Cerca l'eventuale corrispondenza di questo control nella mappa che collega
			 l'indice delle colonna del record con l'indice del vettore dei ControlLinks*/
			for (int j = 0; j <= aDataToCtrlMap.GetUpperBound(); j++)
			{
				DataToCtrlLink*	pDataToCtrlLink	= (DataToCtrlLink*) aDataToCtrlMap[j];
				if (nCLIdx == pDataToCtrlLink->m_nControlLinkIdx)
				{
					bRecordData = TRUE;
					break;
				}
			}
			
			if	(!bRecordData)
			{
				if	(
						pControl->IsDataModified() ||
						pControl->ForceUpdateCtrlView(pDbt ? pDbt->GetCurrentRowIdx() : -1)
					)
				{
					pControl->UpdateCtrlStatus();
					pControl->UpdateCtrlView();
				}
				else 
					if (pWnd->IsKindOf(RUNTIME_CLASS(CExtButton)) && ((CExtButton*)pWnd)->ForceUpdateCtrlView())
				{
					pControl->UpdateCtrlStatus();
					pControl->UpdateCtrlView();

					pWnd->RedrawWindow(NULL, NULL, RDW_INVALIDATE|RDW_UPDATENOW);
				}
			}
		}
		else if (pWnd->IsKindOf(RUNTIME_CLASS(CExtButton)))
		{
			CExtButton* pB = (CExtButton*) pWnd; 
			if (pB->ForceUpdateCtrlView())
			{
				pB->RedrawWindow(NULL, NULL, RDW_INVALIDATE|RDW_UPDATENOW);
			}
		}	
		else if (pWnd->IsKindOf(RUNTIME_CLASS(CBodyEdit)))
		{
			CBodyEdit* pBodyEdit = (CBodyEdit*)pWnd;
			DBTSlaveBuffered* pDBT = pBodyEdit->GetDBT();

			if (pDBT && pDBT->IsModified())
			{
				pBodyEdit->UpdateBodyStatus();
				pBodyEdit->UpdateCtrlBody();
			}
		}
	}
}

//-----------------------------------------------------------------------------
void CRowFormView::EnableViewControlLinks	(BOOL bEnable/* = TRUE*/, BOOL bMustSetOSLReadOnly/*=FALSE*/)
{
	switch (GetDocument()->GetFormMode())
	{
			//case CBaseDocument::BROWSE:
			//case CBaseDocument::FIND:
							
			case CBaseDocument::NEW:
				bMustSetOSLReadOnly = bMustSetOSLReadOnly || (OSL_CAN_DO( GetDocument()->GetInfoOSL(), OSL_GRANT_NEW) == 0);
				break;

			case CBaseDocument::EDIT:
				bMustSetOSLReadOnly = bMustSetOSLReadOnly || (OSL_CAN_DO( GetDocument()->GetInfoOSL(), OSL_GRANT_EDIT) == 0);
				break;
	}

	EnableMappedControlLinks (m_pControlLinks, m_DataToCtrlMap, bEnable, bMustSetOSLReadOnly);

	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
	{
		CRowTabDialog* pTab = NULL;
		VERIFY( pTab = (CRowTabDialog*) m_pTabManagers->GetActiveDlg(i) );
		ASSERT( pTab->IsKindOf(RUNTIME_CLASS(CRowTabDialog)) );
		
		if(pTab && pTab->m_hWnd)
			pTab->EnableTabDialogControlLinks(bEnable, bMustSetOSLReadOnly);
	}
	// ruota l'azione alle eventuali TileDialog presenti
	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pTileGroup->EnableViewControlLinks(bEnable, bMustSetOSLReadOnly);
	}
}

//------------------------------------------------------------------------------
void CRowFormView::OnUpdateControls(BOOL bParentIsVisible)
{
	OnUpdateMappedControls(m_pControlLinks, m_DataToCtrlMap);
	
	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
		m_pTabManagers->GetActiveDlg(i)->OnUpdateControls(bParentIsVisible);

	// ruota l'azione alle eventuali TileDialog presenti
	for (int i = 0; i <= m_pTileGroups->GetUpperBound(); i++)
	{
		CTileGroup* pTileGroup = m_pTileGroups->GetAt(i);
		pTileGroup->OnUpdateControls(bParentIsVisible);
	}
}

//------------------------------------------------------------------------------
#define ROWVIEW_RML(a)	if (pControl->m_p##a && pControl->m_nDataIdx##a > -1) \
	{ \
		pControl->m_p##a = (DataStr*)pRecord->GetDataObjAt(pControl->m_nDataIdx##a); \
	}

void RebuildMappedLinks(SqlRecord* pRecord, CBaseAddressEdit* pControl)
{
	ROWVIEW_RML(City);
	ROWVIEW_RML(County);
	ROWVIEW_RML(Country);
	ROWVIEW_RML(Zip);
	ROWVIEW_RML(Region);
	ROWVIEW_RML(Latitude);
	ROWVIEW_RML(Longitude);

	ROWVIEW_RML(Address);
	ROWVIEW_RML(District);
	ROWVIEW_RML(FederalState);
	ROWVIEW_RML(ISOCode);
}

void CRowFormView::RebuildMappedLinks(SqlRecord* pRecord, ControlLinks* pControlLinks, const Array& aDataToCtrlMap)
{

	DBTSlaveBuffered* pDBT = GetDBT();

	for (int i = 0; i <= aDataToCtrlMap.GetUpperBound(); i++)
	{
		DataToCtrlLink*	pDataToCtrlLink	= (DataToCtrlLink*) aDataToCtrlMap[i];

		DataObj* pData = pRecord->GetDataObjAt(pDataToCtrlLink->m_nDataInfoIdx); //GetDataObjAtEx
		BOOL bColReadOnly = pDBT ? pDBT->GetRecord()->GetDataObjAt(pDataToCtrlLink->m_nDataInfoIdx)->IsReadOnly() : TRUE;

		int	nControlLinkIdx = pDataToCtrlLink->m_nControlLinkIdx;

		CWnd* pWnd = pControlLinks->GetAt(nControlLinkIdx);
		CParsedCtrl* pControl = GetParsedCtrl(pWnd);

		if (pWnd->IsKindOf(RUNTIME_CLASS(CBoolCheckListBox)))
		{
			while(TRUE) 
			{
				ASSERT(pData->IsKindOf(RUNTIME_CLASS(DataBool)));

				((CBoolCheckListBox*) pWnd)->SetDataBoolAt(pDataToCtrlLink->m_nItemCheckLBIdx, (DataBool&) *pData);
				((CBoolCheckListBox*) pWnd)->Enable(pDataToCtrlLink->m_nItemCheckLBIdx, !bColReadOnly && !pData->IsReadOnly());

				if	(
						i == aDataToCtrlMap.GetUpperBound() ||
						((DataToCtrlLink*) aDataToCtrlMap[i + 1])->m_nControlLinkIdx != nControlLinkIdx
					)
					break;

				// Item successivo nella CheckListBox
				pDataToCtrlLink	= (DataToCtrlLink*) aDataToCtrlMap[ ++i ];
				pData = pRecord->GetDataObjAt(pDataToCtrlLink->m_nDataInfoIdx);
			}

			pControl->ForceUpdateCtrlView(pDBT ? pDBT->GetCurrentRowIdx() : -1);
			pControl->UpdateCtrlView();
		}
		else
		{
			pControl->Attach(pData);
			
			//INTERAZIONE CON BODYEDIT
			ColumnInfo* pLinkedColInfo = GetLinkedColumnInfo (pDataToCtrlLink->m_nDataInfoIdx);
			if (pLinkedColInfo)
			{
				// Gestione degli stati
				pLinkedColInfo->AttachCtrlStateData(pRecord, pControl);
				//----

				// Gestione decimali dinamici
				if (
						pLinkedColInfo->IsDynamicDecimalFormatter() 
						&&
						(
							pControl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CDoubleEdit))
							||
							pControl->GetCtrlCWnd()->IsKindOf(RUNTIME_CLASS(CDoubleStatic))
						)
					)
				{
					pLinkedColInfo->SetDynamicDecimalFormatter(pRecord);
				}
				else if (pLinkedColInfo->IsAddress())
				{
					pLinkedColInfo->SetAddressComponents(pRecord);

					ASSERT_KINDOF(CBaseAddressEdit, pControl->GetCtrlCWnd());
					::RebuildMappedLinks(pRecord, (CBaseAddressEdit*) pControl->GetCtrlCWnd());
				}
				else if (pLinkedColInfo->HasSingleComponent())
				{
					pLinkedColInfo->SetComponent(pRecord);

					if (pControl->GetParamDataIdx() > -1)
					{
						pControl->BindParam(pRecord->GetDataObjAt(pControl->GetParamDataIdx()));
					}
				}

				//----
				if (
						pData->IsOSLReadOnly() &&
						GetDocument()->GetFormMode() == CBaseDocument::EDIT &&
						OSL_CAN_DO(m_pBodyEdit->GetInfoOSL(), OSL_GRANT_BE_ADDROW) &&
						OSL_CAN_DO(const_cast<ColumnInfo*>(pLinkedColInfo)->GetInfoOSL(), OSL_GRANT_NEW) &&
						pRecord->IsInsertedByUI()
					)
				{
					pData->SetOSLReadOnly(FALSE);
				}
				//----

				pControl->UpdateCtrlView();
			}
			else if (pControl)
				pControl->AttachRecordToStateData(pRecord);

			pControl->ForceUpdateCtrlView(pDBT ? pDBT->GetCurrentRowIdx() : -1);
			pControl->UpdateCtrlStatus();
			pControl->UpdateCtrlView();

			if (!pData->IsReadOnly() &&	bColReadOnly)
				pControl->EnableCtrl(FALSE);
		}
	}
}

// si occupa di cercare nel BodyEdit la ColInfo legata a quel particolare dato
//-----------------------------------------------------------------------------
ColumnInfo* CRowFormView::GetLinkedColumnInfo (int nDataInfoIdx)
{
	return m_pBodyEdit ? m_pBodyEdit->GetColumnFromDataIdx(nDataInfoIdx) : NULL;
}

// Viene usata anche dalle eventuali tabdialog figlie
//-----------------------------------------------------------------------------
void CRowFormView::BuildMappedDataToCtrlLink
	(
		SqlRecord* pRecord,
		DataObj* pDataObj,
		Array& aDataToCtrlMap,
		int nControlLinksIdx, 
		int nItemCheckLBIdx /* =-1 */
	)
{
	if (pRecord == NULL)
		return;

	// All dataobj MUST belong from same record
	if (!m_pTemplateRecordClass)
		m_pTemplateRecordClass = pRecord->GetRuntimeClass();
	else
		if (pRecord->GetRuntimeClass() != m_pTemplateRecordClass)
			return;

	int nDataInfoIdx = pRecord->Lookup(pDataObj);
	ASSERT(nDataInfoIdx != -1);
		
	if (nItemCheckLBIdx >= 0)
	{
		// E` necessario inoltre ricalcolare gli indici gia` inseriti con chiamate precdenti
		// per tener conto del fatto che la AddDataBool ritorni un indice gia usato per effetto
		// del sorting degli item nel caso la listbox abbia l'attributo di sort
		//
		for (int i = 0; i <= aDataToCtrlMap.GetUpperBound(); i++)
		{
			DataToCtrlLink*	pDataToCtrlLink	= (DataToCtrlLink*) aDataToCtrlMap[i];
			if	(
					pDataToCtrlLink->m_nControlLinkIdx == nControlLinksIdx &&
					pDataToCtrlLink->m_nItemCheckLBIdx >= nItemCheckLBIdx
				)
				pDataToCtrlLink->m_nItemCheckLBIdx++;
		}
	}
    // Save current record dataobj position. Useful for RebuildLinks (see above)
	aDataToCtrlMap.Add(new DataToCtrlLink(nDataInfoIdx, nControlLinksIdx, nItemCheckLBIdx));
}

//-----------------------------------------------------------------------------
void CRowFormView::RebuildLinks (SqlRecord* pRecord)
{
	DBTSlaveBuffered* pDBT = GetDBT();
	if (!pDBT) 
		return;

	ASSERT_VALID(pRecord);
	if (!pRecord) 
		return;

	ASSERT(m_pTemplateRecordClass);
	ASSERT(pRecord->GetRuntimeClass() == m_pTemplateRecordClass);

	RebuildMappedLinks (pRecord, m_pControlLinks, m_DataToCtrlMap);

	// ruota l'azione alle eventuali TabDialog presenti
	for (int i = 0; i <= m_pTabManagers->GetUpperBound(); i++)
		((CRowTabDialog*)m_pTabManagers->GetActiveDlg(i))->RebuildLinks(pRecord);

	// ruota l'azione alle eventuali TabDialog presenti
	for (int t = 0; t <= m_pTileGroups->GetUpperBound(); t++)
	{
		CTileGroup* pGroup = m_pTileGroups->GetAt(t);
		pGroup->RebuildLinks(pRecord);
	}

	BOOL bMustSetOSLReadOnly = FALSE;
	switch (GetDocument()->GetFormMode())
	{
			//case CBaseDocument::BROWSE:
			//case CBaseDocument::FIND:
							
			case CBaseDocument::NEW:
				bMustSetOSLReadOnly = (OSL_CAN_DO( GetDocument()->GetInfoOSL(), OSL_GRANT_NEW) == 0);
				break;

			case CBaseDocument::EDIT:
				bMustSetOSLReadOnly = (OSL_CAN_DO( GetDocument()->GetInfoOSL(), OSL_GRANT_EDIT) == 0);
				break;
	}
	if (bMustSetOSLReadOnly)
		SetOSLReadOnlyOnControlLinks(m_pControlLinks, m_DataToCtrlMap);
	//----

	OnUpdateControls();
}

// Control Created internally (useful for derived control)
// This function as to prefered from the function with already created control
//-----------------------------------------------------------------------------
CParsedCtrl* CRowFormView::AddLink
	(
		UINT			nIDC, 
		const CString&	sName,
		SqlRecord*		pRecord, 
		DataObj*		pDataObj, 
		CRuntimeClass*	pParsedCtrlClass,
		HotKeyLink*		pHotKeyLink,		/* = NULL */
		UINT			nBtnID				/* = BTN_DEFAULT */
	)
{
	if (!pParsedCtrlClass->IsDerivedFrom(RUNTIME_CLASS(CBoolCheckListBox)))
		BuildMappedDataToCtrlLink
			(
				pRecord,
				pDataObj,
				m_DataToCtrlMap,
				m_pControlLinks->GetSize()
			);
	// Standard behaviour
	return CAbstractFormView::AddLink 
		(
			nIDC, 
			sName,
			pRecord, 
			pDataObj, 
			pParsedCtrlClass,
			pHotKeyLink,
			nBtnID
		);
}


//Control Created internally (useful for derived control)
// This function as to prefered from the function with already created control
//-----------------------------------------------------------------------------
CParsedCtrl* CRowFormView:: AddLinkAndCreateControl
	(
		const CString&	sName,
		DWORD			dwStyle, 
		const CRect&	rect,
		UINT			nIDC, 
		SqlRecord*		pRecord, 
		DataObj*		pDataObj, 
		CRuntimeClass*	pParsedCtrlClass,
		void*			pHotKeyLink			/*= NULL*/,
		BOOL			bIsARuntimeClass	/*= FALSE*/,
		UINT			nBtnID				/*= BTN_DEFAULT*/
	)
{
	BuildMappedDataToCtrlLink
		(
			pRecord,
			pDataObj,
			m_DataToCtrlMap,
			m_pControlLinks->GetSize()
		);

	return ::AddLinkAndCreateControl(sName, dwStyle, rect, this, m_pControlLinks, nIDC, pRecord, pDataObj, pParsedCtrlClass, pHotKeyLink, bIsARuntimeClass, nBtnID);
}

// Serve per poter aggiungere bottoni che cambiano stato sulla base del valore del
// dataobj passato. Nel caso che non si passa un DataObj il bottone non cambia
// stato ma si comporta come un normale bottone sensibile pero` al panning della
// formview
//-----------------------------------------------------------------------------
CExtButton* CRowFormView::AddLink
	(
		UINT			nIDC, 
		const CString&	sName,
		SqlRecord*		pRecord,	/* = NULL */
		DataObj*		pDataObj	/* = NULL */ 
	)
{
	BuildMappedDataToCtrlLink
		(
			pRecord,
			pDataObj,
			m_DataToCtrlMap,
			m_pControlLinks->GetSize()
		);

	// Standard behaviour
	return CAbstractFormView::AddLink (nIDC, sName, pRecord, pDataObj);
}

//-----------------------------------------------------------------------------
CBodyEdit* CRowFormView::AddLink
	(
		UINT				nIDC, 
		DBTSlaveBuffered*	pDBT, 
		CRuntimeClass*		pParsedCtrlClass,
		CRuntimeClass*		pRowFormViewClass /*= NULL*/,
		CString				strRowFormViewTitle,  /*=""*/
		CString				sName /*= _T("")*/

	)
{
	//if (pDBT->m_pDBTMaster)
	//{
	//	ASSERT_TRACE(FALSE, "CRowFormView::AddLink: cannot allow one to many to many relation on row view");
	//	return NULL;
	//}
	
	return __super::AddLink
						(
							nIDC, 
							pDBT, 
							pParsedCtrlClass,
							pRowFormViewClass,
							strRowFormViewTitle,
							sName
						);
}

//-----------------------------------------------------------------------------
// Server per aggiungere item ad un CheckListBox
int CRowFormView::AddDataBoolToCheckLB
	(
		CBoolCheckListBox*	pBCLB,
		LPCTSTR				lpszAssoc,
		SqlRecord*			pRecord, 
		DataObj*			pDataObj
	)
{
	ASSERT(pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)));

	if (!pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)))
		return -1;

	int nIdxLB = pBCLB->AddDataBool(lpszAssoc, (DataBool&)*pDataObj);
	BuildMappedDataToCtrlLink
		(
			pRecord,
			pDataObj,
			m_DataToCtrlMap,
			m_pControlLinks->GetUpperBound(),
			nIdxLB
		);

	return nIdxLB;
}

//-----------------------------------------------------------------------------
void CRowFormView::OnActivateView(BOOL bActive, CView* pActivateView, CView* pDeactiveView)
{
	ASSERT(GetParentFrame());
	ASSERT_KINDOF(CRowFormFrame, GetParentFrame());
	CRowFormFrame* pFrame = (CRowFormFrame*)GetParentFrame();
	if (pFrame->IsPopup())
	{
		if (bActive && m_pBodyEdit)
			m_pBodyEdit->ActivateBody(SET_BODY_CURR_ROW);

		CAbstractFormView::OnActivateView(bActive, pActivateView, pDeactiveView);
	}
}

// Standard beahaviour for value changed message
//--------------------------------------------------------------------------
LRESULT CRowFormView::OnValueChanged(WPARAM wParam, LPARAM lParam)
{
	DBTSlaveBuffered* pDBT = GetDBT();
	if (pDBT)
	{
		SqlRecord* pRec = pDBT->GetCurrentRow();
		if (pRec && !pRec->IsNeverStorable())
			pRec->SetStorable();
	}

	return CAbstractFormView::OnValueChanged(wParam, lParam);
}

// To manage data validation on view focusing
//-----------------------------------------------------------------------------
void CRowFormView::OnSetFocus(CWnd* pOldWnd)
{
	if (m_pBodyEdit)
		m_pBodyEdit->ActivateBody(SET_BODY_CURR_ROW);

	CAbstractFormView::OnSetFocus(pOldWnd);
}

//-----------------------------------------------------------------------------
void CRowFormView::OnUpdateTotalRecordsIndicator(CCmdUI* pCmdUI)
{
	DBTSlaveBuffered* pDBT = GetDBT();
	if (!pDBT)
		return;

	long lCurrentRecord	= pDBT->GetCurrentRowIdx() + 1;
	long lTotalRecords	= pDBT->GetSize();

	pCmdUI->Enable();
	pCmdUI->SetText(cwsprintf(_T("%05ld/%05ld"), lCurrentRecord, lTotalRecords));
}

//-----------------------------------------------------------------------------
int CRowFormView::GetDataIdxMappedToCtrl(int nCtrlIdx)
{
	return ((DataToCtrlLink*)m_DataToCtrlMap[nCtrlIdx])->m_nDataInfoIdx;
}

//-----------------------------------------------------------------------------
void CRowFormView::OnDeleteRow	() 
{
	ASSERT_VALID(m_pBodyEdit);
	m_pBodyEdit->DeleteRecord();
	//m_pBodyEdit->PostMessageW(WM_COMMAND, IDC_BE_DELETE, (LPARAM) m_pBodyEdit->m_hWnd);	
}

//-----------------------------------------------------------------------------
void CRowFormView::OnInsertRow	() 
{
	ASSERT_VALID(m_pBodyEdit);
	m_pBodyEdit->DoInsertRecord();
	//m_pBodyEdit->PostMessageW(WM_COMMAND, IDC_BE_INSERT, (LPARAM) m_pBodyEdit->m_hWnd);	
}

//-----------------------------------------------------------------------------
void CRowFormView::OnUpdateDeleteRow (CCmdUI* pCmdUI) 
{ 
	BOOL b =  CanDoDeleteRow ();
	//TRACE("%s(%d) CRowFormView::OnUpdateDeleteRow: %d\n",  (LPCTSTR)m_sName, this->m_nID, b);
	pCmdUI->Enable( b); 
}

//-----------------------------------------------------------------------------
void CRowFormView::OnUpdateInsertRow (CCmdUI* pCmdUI) 
{ 
	BOOL b = CanDoInsertRow ();
	//TRACE(L"%s(%d) CRowFormView::OnUpdateInsertRow: %d\n", (LPCTSTR)m_sName, this->m_nID, b);
	pCmdUI->Enable( b); 
}

//------------------------------------------------------------------------------
BOOL CRowFormView::CanDoDeleteRow	()
{
	return m_pBodyEdit ? m_pBodyEdit->CanDeleteRowSimple() : FALSE;
}

//------------------------------------------------------------------------------
BOOL CRowFormView::CanDoInsertRow	()
{
	return m_pBodyEdit ? m_pBodyEdit->CanInsertRowSimple() : FALSE;
}

//-----------------------------------------------------------------------------
void CRowFormView::OnMoveToPrevRow	()
{ 
	if (!CheckForm(TRUE)) 
		return;

	if (!CanDoMoveToPrevRow()) 
		return;

	if (m_pBodyEdit)
	{
		m_pBodyEdit->DoMoveToPrevRow ();
		return;
	} 

	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void CRowFormView::OnMoveToNextRow	() 
{ 
	if (!CheckForm(TRUE)) 
		return;

	if (!CanDoMoveToNextRow()) 
		return;

	if (m_pBodyEdit)
	{
		m_pBodyEdit->DoMoveToNextRow ();
		return;
	} 
	
	ASSERT(FALSE);
}
//-----------------------------------------------------------------------------
void CRowFormView::OnMoveToFirstRow	() 
{ 
	if (!CheckForm(TRUE)) 
		return;

	if (!CanDoMoveToFirstRow()) 
		return;

	if (m_pBodyEdit)
	{
		m_pBodyEdit->DoMoveToFirstRow ();
		return;
	} 
	
	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void CRowFormView::OnGotoMaster()
{
	if (GetFrame()) {
		GetFrame()->OnGotoMaster();
	}
}

//-----------------------------------------------------------------------------
void CRowFormView::OnMoveToLastRow	() 
{ 
	if (!CheckForm(TRUE)) 
		return;

	if (!CanDoMoveToLastRow()) 
		return;

	if (m_pBodyEdit)
	{
		m_pBodyEdit->DoMoveToLastRow ();
		return;
	} 

	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void CRowFormView::OnUpdateMoveToPrevRow	(CCmdUI* pCmdUI) { pCmdUI->Enable( CanDoMoveToPrevRow ()); }
void CRowFormView::OnUpdateMoveToNextRow	(CCmdUI* pCmdUI) { pCmdUI->Enable( CanDoMoveToNextRow ()); }
void CRowFormView::OnUpdateMoveToFirstRow	(CCmdUI* pCmdUI) { pCmdUI->Enable( CanDoMoveToFirstRow ()); }
void CRowFormView::OnUpdateMoveToLastRow	(CCmdUI* pCmdUI) { pCmdUI->Enable( CanDoMoveToLastRow ()); }

//------------------------------------------------------------------------------
BOOL CRowFormView::CanDoMoveToPrevRow	()
{
	if (m_pBodyEdit) 
		return m_pBodyEdit->CanDoMoveToPrevRow	();

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CRowFormView::CanDoMoveToNextRow	()
{
	if (m_pBodyEdit) 
		return m_pBodyEdit->CanDoMoveToNextRow	();

	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CRowFormView::CanDoMoveToFirstRow	()
{
	if (m_pBodyEdit) 
		return m_pBodyEdit->CanDoMoveToFirstRow	();

	return FALSE;
}


//------------------------------------------------------------------------------
BOOL CRowFormView::CanDoMoveToLastRow	()
{
	if (m_pBodyEdit) 
		return m_pBodyEdit->CanDoMoveToLastRow	();
	
	return FALSE;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CRowFormView::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CRowFormView\n");
	CAbstractFormView::Dump(dc);
}

void CRowFormView::AssertValid() const
{
	CAbstractFormView::AssertValid();
}

#endif //_DEBUG

//==============================================================================
//	CDynamicFormView
//==============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDynamicFormView, CMasterFormView)
	
CDynamicFormView::CDynamicFormView(const CString& sName /*= _T("Dynamic")*/)
	:
	CMasterFormView(sName, IDD_EMPTY_VIEW)
{
	SetResourceModule(GetDllInstance(RUNTIME_CLASS(CDynamicFormView)));
}


//==============================================================================
//	CJsonFormView
//==============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonFormView, CMasterFormView)

//-----------------------------------------------------------------------------
CJsonFormView::CJsonFormView()
	:
	CMasterFormView(_T(""), 0)
{
}
//-----------------------------------------------------------------------------
CJsonFormView::CJsonFormView(UINT nIDTemplate)
	:
	CMasterFormView(_T(""), nIDTemplate)
{
}

//-----------------------------------------------------------------------------
void CJsonFormView::AssignJsonContext(CJsonContext* pContext)
{
	m_pJsonContext = pContext;
}

//==============================================================================
//	CWizardFormView
//==============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CWizardFormView, CMasterFormView)

BEGIN_MESSAGE_MAP(CWizardFormView, CMasterFormView)
	ON_WM_VSCROLL()

	ON_COMMAND	(IDC_WIZARD_NEXT,	OnWizardNext)
	ON_COMMAND	(IDC_WIZARD_BACK,	OnWizardBack)
	ON_COMMAND	(IDC_WIZARD_FINISH,	OnWizardFinish)
	ON_COMMAND	(IDCANCEL,			OnWizardCancel)
	ON_COMMAND	(ID_WIZARD_RESTART,	OnWizardRestart)
	ON_MESSAGE	(UM_EXTDOC_BATCH_COMPLETED,			OnBatchCompleted)
	//ON_COMMAND(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, OnBatchStartStop)

	ON_WM_SIZE()
END_MESSAGE_MAP() 

//------------------------------------------------------------------------------
void CWizardFormView::OnVScroll (UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{
	__super::OnVScroll(nSBCode, nPos, pScrollBar);
	
	DoAnchorages ();
}

//-----------------------------------------------------------------------------
BOOL CWizardFormView::IsToEnableWizardNext	()
{
	CTabManager* pTab = GetTabManager();
	CTabDialog* pActiveDialog = pTab != NULL ? pTab->GetActiveDlg() : NULL;
	DlgInfoItem* pLastInfo = GetLastEnabledItem();

	return !pActiveDialog || (!m_bWizardFinished && pActiveDialog->GetDlgInfoItem() != pLastInfo);
}
//-----------------------------------------------------------------------------
BOOL CWizardFormView::IsToEnableWizardBack	() 
{ 
	CTabManager* pTab = GetTabManager();
	CTabDialog* pActiveDialog  = pTab != NULL ? pTab->GetActiveDlg() : NULL;
	DlgInfoItem* pFirstInfo = pTab->GetDlgInfoArray()->GetAt(0);
	
	return !pActiveDialog || (!m_bWizardFinished && pActiveDialog->GetDlgInfoItem() != pFirstInfo);
}

//-----------------------------------------------------------------------------
BOOL CWizardFormView::IsToEnableWizardFinish	() 
{
	CTabManager* pTab = GetTabManager();
	CTabDialog* pActiveDialog = pTab != NULL ? pTab->GetActiveDlg() : NULL;
	DlgInfoItem* pLastInfo = GetLastEnabledItem();

	return !m_bWizardFinished && pActiveDialog && pActiveDialog->GetDlgInfoItem() == pLastInfo;
}
//-----------------------------------------------------------------------------
BOOL CWizardFormView::IsToEnableWizardRestart	() { return m_bWizardFinished; }
//-----------------------------------------------------------------------------
CTabWizard* CWizardFormView::GetTabManager() 
{ 
	if (m_pTabManagers->GetSize() > 0)
		return (CTabWizard*)m_pTabManagers->GetAt(0);
	return NULL;
}

//-----------------------------------------------------------------------------
CWizardFormView::CWizardFormView()
	:
	CMasterFormView (_T(""), IDD_WIZARD_FORM_VIEW),
	m_IDCBitmap							(IDC_WIZARDBMP), 
	m_bIsDirectCallToWizardFinishCall	(FALSE),
	m_bWizardFinished					(FALSE),
	m_bUseOldButtonStyle				(FALSE),
	m_bReExecutable						(TRUE)
{
	GetInfoOSL()->SetType(OSLType_BatchTemplate);
	SetResourceModule(GetDllInstance(RUNTIME_CLASS(CWizardFormView)));//per trovare la risorsa IDD_WIZARD_FORM_VIEW
}

//-----------------------------------------------------------------------------
CWizardFormView::CWizardFormView(const CString& sName, UINT nIDTemplate)
	:
	CMasterFormView						(sName, nIDTemplate),
	m_IDCBitmap							(IDC_WIZARDBMP), 
	m_bIsDirectCallToWizardFinishCall	(FALSE),
	m_bWizardFinished					(FALSE),
	m_bUseOldButtonStyle				(FALSE),
	m_bReExecutable						(TRUE)
{
	GetInfoOSL()->SetType(OSLType_BatchTemplate);
}

//-----------------------------------------------------------------------------
CWizardFormView::~CWizardFormView ()
{
}

//-----------------------------------------------------------------------------
afx_msg	LRESULT	CWizardFormView::OnBatchCompleted	(WPARAM wParam, LPARAM lParam)
{
	m_bWizardFinished = TRUE;
	SetWizardButtons(0);
	return 0L;
}


//-----------------------------------------------------------------------------
void CWizardFormView::BuildDataControlLinks()
{	
	OnBuildDataControlLinks();

	if (m_pTabManagers == NULL || m_pTabManagers->GetSize() == 0)
		AddTabManager(IDC_WIZARD_TAB, RUNTIME_CLASS(CTabWizard),_T("WizardTabber"));
	
	//BugFix #21431 
	GetTabManager()->SetDefaultSequence(FALSE);

	CWizardTabDialog* pDialog = (CWizardTabDialog*)GetTabManager()->GetActiveDlg();
	if(pDialog)
	{
		pDialog->SetFocus();
		pDialog->OnUpdateWizardButtons();
		((CWizardFormDoc*)GetDocument())->DispatchUpdateWizardButtons(pDialog->GetDialogID());
	}

	if (m_bUseOldButtonStyle)
	{
		//anchor to bottom wizard's buttons 
		m_arAnchorControls.Add(new CAnchorCtrl(IDC_WIZARD_BACK, this));
		m_arAnchorControls.Add(new CAnchorCtrl(IDC_WIZARD_NEXT, this));
		m_arAnchorControls.Add(new CAnchorCtrl(IDCANCEL, this));
		m_arAnchorControls.Add(new CAnchorCtrl(IDC_WIZARD_FINISH, this));
	}
	else
	{
	CWnd* pWnd = GetDlgItem(IDC_WIZARD_BACK);
	if (pWnd)
		pWnd->DestroyWindow();
	pWnd = GetDlgItem(IDC_WIZARD_NEXT);
	if (pWnd)
		pWnd->DestroyWindow();
	pWnd = GetDlgItem(IDCANCEL);
	if (pWnd)
		pWnd->DestroyWindow();
	pWnd = GetDlgItem(IDC_WIZARD_FINISH);
	if (pWnd)
		pWnd->DestroyWindow();
	}

	//riabilita ridimensionamento verticale
	if (!m_pLayoutContainer)
	{
		GetTabManager()->SetAutoSizeCtrl(3);
		//preserva la pulsantiera del wizard dal ridimensionamento verticale del tabmanager
		//int nMargin  = ((CAnchorCtrl*) m_arAnchorControls.GetAt(0))->m_cpOrigin.y + 5;
		GetTabManager()->SetAutoSizeMargin(0, 10);

	}
	UpdateStepper();
}

//----------------------------------------------------------------------------
void CWizardFormView::OnWizardStart()
{
	if (GetDocument()->GetNotValidView(TRUE))
		return;

	m_bWizardFinished = FALSE;
	
	
	CTabDialog *pDlg = NULL;
	UINT nActiveDlgIDD = 0;
	do
	{
		pDlg = GetTabManager()->GetActiveDlg();
		ASSERT(pDlg);
		nActiveDlgIDD = pDlg->GetDialogID();

		OnWizardBack();
	}
	while (nActiveDlgIDD != GetTabManager()->GetActiveDlg()->GetDialogID());

	SetWizardButtons(0);

	OnWizardInit();
}

//----------------------------------------------------------------------------
void CWizardFormView::OnWizardNext()
{
	if (GetDocument()->GetNotValidView(TRUE))
		return;

	CTabDialog *pDlg = GetTabManager()->GetActiveDlg();
	ASSERT_KINDOF(CWizardTabDialog, pDlg);
	
	if(!pDlg)
	{
		ASSERT(FALSE);
		return;
	}

	UINT currentIDD = pDlg->GetDlgInfoItem()->GetDialogID();
	int pos = GetTabManager()->GetTabDialogPos (currentIDD);

	UINT nextIDD = WIZARD_DEFAULT_TAB;
	// prima interrogo i clientdocument
	if (GetDocument())
		nextIDD = ((CWizardFormDoc*)GetDocument())->DispatchWizardNext(currentIDD);

	// poi interrogo la tab
	if (nextIDD == WIZARD_DEFAULT_TAB) 
		nextIDD = ((CWizardTabDialog*)pDlg)->OnWizardNext();

	//infine la view
	if (nextIDD == WIZARD_DEFAULT_TAB) 
		nextIDD = OnWizardNext(currentIDD);

	//se nessuno risponde, pesco nell'array
	if (nextIDD == WIZARD_DEFAULT_TAB)
	{
		pos = GetTabManager()->GetDlgItemNextPos(pos, TRUE);
		if (pos >= 0)	
			nextIDD = GetTabManager()->GetDlgInfoArray()->GetAt(pos)->GetDialogID ();
	}
	if (nextIDD != WIZARD_SAME_TAB 
		&& nextIDD > 0 
		&& currentIDD != nextIDD)
	{
		((CWizardTabDialog*)pDlg)->Deactivate();

		TabDialogShow(IDC_WIZARD_TAB, currentIDD, FALSE);
		int nOK = TabDialogShow(IDC_WIZARD_TAB, nextIDD, TRUE);
		if (nOK && TabDialogActivate(IDC_WIZARD_TAB, nextIDD))
		{
			EnableButtons(GetTabManager()->GetTabDialogPos(nextIDD));
			//tengo traccia della storia delle tab su cui sono passato
			m_PreviousTabs.Add (currentIDD);
		}
		else
			VERIFY(TabDialogShow(IDC_WIZARD_TAB, currentIDD, TRUE));
	}

	if (/*TODO RIMUOVERE TEST SU BATCH FRAME*/TRUE || GetDocument()->GetMasterFrame()->IsKindOf(RUNTIME_CLASS(CWizardBatchFrame)))
		if (nextIDD == 0 && currentIDD != nextIDD) 
		{
			//La dispatch è già nella OnWizardFinish, in questo modo non viene dispatchata due volte
			//((CWizardFormDoc*)GetDocument())->DispatchWizardFinish(currentIDD);

			//altera il booleano che indica se la OnWizardFinish proviene da WizardNext o è diretta
			m_bIsDirectCallToWizardFinishCall = FALSE;
			OnWizardFinish();
			m_bIsDirectCallToWizardFinishCall = TRUE;
		}

	UpdateStepper(nextIDD != currentIDD && nextIDD != WIZARD_SAME_TAB);
}


// A differenza della OnWizardNext() questo metodo skippa i controlli del programmatore
// per effettuare solamente il cambio di tab
//-------------------------------------------------------------------------------------
void CWizardFormView::DoWizardNext()
{
	if (GetDocument()->GetNotValidView(TRUE))
		return;

	CTabDialog *pDlg = GetTabManager()->GetActiveDlg();
	ASSERT_KINDOF(CWizardTabDialog, pDlg);
	
	if(!pDlg)
	{
		ASSERT(FALSE);
		return;
	}

	UINT currentIDD = pDlg->GetDlgInfoItem()->GetDialogID();
	int pos = GetTabManager()->GetTabDialogPos(currentIDD);

	UINT nextIDD = 0;
	pos = GetTabManager()->GetDlgItemNextPos(pos, TRUE);
	if (pos < 0)	
		return;

	nextIDD = GetTabManager()->GetDlgInfoArray()->GetAt(pos)->GetDialogID();

	((CWizardTabDialog*)pDlg)->Deactivate();

	TabDialogShow(IDC_WIZARD_TAB, currentIDD, FALSE);
	int nOK = TabDialogShow(IDC_WIZARD_TAB, nextIDD, TRUE);
	if (nOK && TabDialogActivate(IDC_WIZARD_TAB, nextIDD))
	{
		//tengo traccia della storia delle tab su cui sono passato
		m_PreviousTabs.Add (currentIDD);
	}
	else
		VERIFY(TabDialogShow(IDC_WIZARD_TAB, currentIDD, TRUE));

	// Disable wizard back
	if (GetFrame()->GetTabbedToolBar())
	{
		CTBToolBar* pToolbar = GetFrame()->GetTabbedToolBar()->FindToolBar(szToolbarNameMain);
		if (pToolbar)
		{
			pToolbar->SuspendLayout();
			pToolbar->HideButton(IDC_WIZARD_BACK, TRUE);
			if (/*TODO RIMUOVERE TEST SU BATCH FRAME*/TRUE || GetDocument()->GetMasterFrame()->IsKindOf(RUNTIME_CLASS(CWizardBatchFrame)))
			{
				pToolbar->SetButtonInfo(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN, TBBS_BUTTON, TBIcon(szIconPause, TOOLBAR), _TB("Pause"));
				pToolbar->SetButtonInfo(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN,   TBBS_BUTTON, TBIcon(szIconStop, TOOLBAR),  _TB("Stop"));
				pToolbar->HideButton(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN, FALSE);
				pToolbar->HideButton(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, FALSE);
			}
			else
			{
				pToolbar->HideButton(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, TRUE);
			}

			pToolbar->ResumeLayout();
			pToolbar->AdjustLayout();
			pToolbar->RepositionRightButtons();
		}
	}

	UpdateStepper(nextIDD != currentIDD && nextIDD != WIZARD_SAME_TAB);
}

//----------------------------------------------------------------------------
void CWizardFormView::OnWizardBack()
{
	CTabDialog *pDlg = GetTabManager()->GetActiveDlg();
	ASSERT_KINDOF(CWizardTabDialog, pDlg);

	if (!pDlg)
	{
		ASSERT(FALSE);
		return;
	}

	if (GetDocument()->GetNotValidView(TRUE))
		return;

	m_bWizardFinished = FALSE;

	UINT currentIDD = pDlg->GetDlgInfoItem()->GetDialogID();
	int pos = GetTabManager()->GetTabDialogPos (currentIDD);

	UINT prevIDD = WIZARD_DEFAULT_TAB;
	// prima interrogo i clientdocument
	if (GetDocument())
		prevIDD = ((CWizardFormDoc*)GetDocument())->DispatchWizardBack(currentIDD);

	// poi interrogo la tab
	if (prevIDD == WIZARD_DEFAULT_TAB) 
		prevIDD = ((CWizardTabDialog*)pDlg)->OnWizardBack();

	//poi la view
	if (prevIDD == WIZARD_DEFAULT_TAB) 
		prevIDD = OnWizardBack(currentIDD);

	//se nessuno risponde, pesco nell'array
	if (prevIDD == WIZARD_DEFAULT_TAB)
	{
		pos = GetTabManager()->GetDlgItemPrevPos(pos, TRUE);
		if (pos >= 0)	
			prevIDD = GetTabManager()->GetDlgInfoArray()->GetAt(pos)->GetDialogID ();
	}
	if (prevIDD > 0 && prevIDD != currentIDD)
	{
		((CWizardTabDialog*)pDlg)->Deactivate();

		TabDialogShow(IDC_WIZARD_TAB, currentIDD, FALSE);
		int nOK = TabDialogShow(IDC_WIZARD_TAB, prevIDD, TRUE);
		if (nOK && TabDialogActivate(IDC_WIZARD_TAB, prevIDD))
		{		
			EnableButtons (GetTabManager()->GetTabDialogPos(prevIDD));

			// pesco l'ultima tab della lista; se questa e' quella che sto attivando,
			// significa che sto seguendo la stessa sequenza (all'indietro) con cui
			// ho attivato le tab (in avanti).
			// se non e' la stessa, risalgo all'indietro fino a che non trovo la tab che sto attivando,
			// rimuovendo le tab intermedie dalla lista
			UINT pos = m_PreviousTabs.GetSize ();
			BOOL bFound;
			while (pos)
			{
				bFound = prevIDD == m_PreviousTabs.GetAt(pos-1);
				m_PreviousTabs.RemoveAt (pos-1);
				pos--;
				if (bFound) break;
			}
		}
		else
			VERIFY(TabDialogShow(IDC_WIZARD_TAB, currentIDD, TRUE));
	}

	UpdateStepper(prevIDD == currentIDD || prevIDD == WIZARD_SAME_TAB ? 0 : -1);
}

//-----------------------------------------------------------------------------
const BOOL& CWizardFormView::IsReExcecutable() const
{
	return m_bReExecutable;
}

//-----------------------------------------------------------------------------
void CWizardFormView::SetReExcecutable(BOOL bValue)
{
	m_bReExecutable = bValue;
}

//----------------------------------------------------------------------------
LRESULT CWizardFormView::OnWizardBack(UINT /*IDD*/)	
{
	UINT pos = m_PreviousTabs.GetSize ();
	if(pos)
		return m_PreviousTabs.GetAt (--pos);
	else 
		return WIZARD_DEFAULT_TAB;
}

//---------------------------------------------------------------------------
//void CWizardFormView::OnBatchStartStop()
//{
//	CAbstractFormDoc* pDoc = dynamic_cast<CAbstractFormDoc*>(GetDocument());
//	if (pDoc && !pDoc->m_bBatchRunning)
//		OnWizardNext();
//}
//----------------------------------------------------------------------------
CTabManager* CWizardFormView::AddTabManager(UINT nIDC, CRuntimeClass* pClass, const CString& sName, BOOL bCallOnInitialUpdate)
{
	if (!m_pLayoutContainer && !m_bUseOldButtonStyle)
		EnableLayout();

	return __super::AddTabManager(nIDC, pClass, sName, bCallOnInitialUpdate);
}

//----------------------------------------------------------------------------
void CWizardFormView::EnableButtons(int position)
{
	DWORD flags=0;

	//il pulsante finish e abilitato se la corrente e' l'ultima tab abilitata 
	//il pulsante next e abilitato se dopo c'e' almeno una tab NON disabilitata
	BOOL bFinish = position == GetTabManager()->GetDlgInfoArray()->GetUpperBound();

	if (!bFinish)
	{
		CTabDialog* pDialog = GetTabManager()->GetActiveDlg();
		if (pDialog->IsKindOf(RUNTIME_CLASS(CWizardTabDialog)))
		{
			bFinish = ((CWizardTabDialog *) pDialog)->IsLast();
		}
	}

	if (!bFinish)
	{
		bFinish = TRUE;
		for (int i = position + 1; i < GetTabManager()->GetDlgInfoArray()->GetSize (); i++)
			if (GetTabManager()->GetDlgInfoArray()->GetAt(i)->IsEnabled())
			{
				bFinish = FALSE;
				break;
			}
	}

	if (!m_bUseOldButtonStyle && (/*TODO RIMUOVERE TEST SU BATCH FRAME*/TRUE || GetDocument()->GetMasterFrame()->IsKindOf(RUNTIME_CLASS(CWizardBatchFrame))))
	{
		flags |= PSWIZB_NEXT; 
		if (bFinish)
		{
			CWnd* pWnd = GetDlgItem(IDC_WIZARD_NEXT);
			if (pWnd)
				pWnd->SetWindowText(GetDocument()->IsEditingParamsFromExternalController() ? _TB("&Save") : _TB("&Execute"));
		}
		else
		{
			CWnd* pWnd = GetDlgItem(IDC_WIZARD_NEXT);
			if (pWnd)
				pWnd->SetWindowText(_TB("&Next"));
		}
	}
	else
	{
		if (bFinish)
			flags |= PSWIZB_FINISH; 
		else
			flags |= PSWIZB_NEXT; 
	}

	//il pulsante back e abilitato se prima c'e' almeno una tab NON disabilitata
	BOOL bBack = FALSE;
	for (int i = position - 1; i >= 0; i--)
		if (GetTabManager()->GetDlgInfoArray()->GetAt(i)->IsEnabled())
		{
			bBack = TRUE;
			break;
		}
	if (bBack)
		flags |= PSWIZB_BACK; 

	SetWizardButtons (flags);
}

//----------------------------------------------------------------------------
void CWizardFormView::OnWizardFinish()
{
	//il metodo deve essere chiamato solo una volta
	ASSERT(!m_bWizardFinished);

	CTabDialog *pDlg = GetTabManager()->GetActiveDlg();
	ASSERT_KINDOF(CWizardTabDialog, pDlg);
	if(!pDlg)
	{
		ASSERT(FALSE);
		return;
	}

	if (GetDocument()->GetNotValidView(TRUE))
		return;

	((CWizardTabDialog*)pDlg)->Deactivate();

	OnWizardEnd();

	//spostata la dispatch in modo tale che arrivi il finish al test manager prima delle finestre di Errore
	//questa dispatch NON VA REGISTRATA se la chiamata OnWizardFinish è originata dall'ultima OnWizardNext
	CWizardTabDialog* pTabDlg = ((CWizardTabDialog*)GetTabManager()->GetActiveDlg());

	//se è una chiamata diretta al metodo OnWizardFinish (Non proveniente da wizardNext) funziona normalmente
	if (m_bIsDirectCallToWizardFinishCall)
		((CWizardFormDoc*)GetDocument())->DispatchOnBeforeWizardFinish(pTabDlg->GetDialogID());
	else
	{
		//se proviene dal wizardNext fa la dispatch ma non la registra sul TestManager
		CApplicationContext::MacroRecorderStatus localStatus = AfxGetApplicationContext()->m_MacroRecorderStatus;
		AfxGetApplicationContext()->m_MacroRecorderStatus = CApplicationContext::IDLE;

		((CWizardFormDoc*)GetDocument())->DispatchOnBeforeWizardFinish(pTabDlg->GetDialogID());

		//ripristina lo stato del macrorecorder
		AfxGetApplicationContext()->m_MacroRecorderStatus = localStatus;
	}
	// Se documento Wizard avvio la batch, disabilito il tasto Indietro 
	// e ripristino la caption del tasto Annulla
	if (/*TODO RIMUOVERE TEST SU BATCH FRAME*/TRUE || GetDocument()->GetMasterFrame()->IsKindOf(RUNTIME_CLASS(CWizardBatchFrame)))
	{
		CWnd* pWnd = GetDlgItem(IDC_WIZARD_BACK);
		if (pWnd)
			pWnd->EnableWindow(FALSE);

		GetDocument()->OnBatchStartStop();
	}
	
	pTabDlg = ((CWizardTabDialog*)GetTabManager()->GetActiveDlg());

	((CWizardFormDoc*)GetDocument())->DispatchWizardFinish(pTabDlg->GetDialogID());
	pTabDlg->OnWizardFinish();

	ASSERT_VALID(this);
	m_bWizardFinished = TRUE;
}

//----------------------------------------------------------------------------
void CWizardFormView::OnWizardRestart ()
{
	((CWizardFormDoc*)GetDocument())->OnWizardRestart();
	OnWizardStart();
}

//----------------------------------------------------------------------------
void CWizardFormView::OnWizardCancel()
{
	CTabDialog *pDlg = GetTabManager()->GetActiveDlg();
	ASSERT_KINDOF(CWizardTabDialog, pDlg);
	if(!pDlg)
	{
		ASSERT(FALSE);
		return;
	}

	if (/*TODO RIMUOVERE TEST SU BATCH FRAME*/TRUE || GetDocument()->GetMasterFrame()->IsKindOf(RUNTIME_CLASS(CWizardBatchFrame)))
	{
		if (GetDocument()->m_bBatchRunning)
		{
			GetDocument()->OnBatchPauseResume();
			CWnd* pWnd = GetDlgItem(IDCANCEL);
			ASSERT(pWnd);
			if (pWnd)
			{
				if (GetDocument()->m_BatchScheduler.IsPaused())
					pWnd->SetWindowText(_TB("&Continue"));
				else
					pWnd->SetWindowText(_TB("&Pause"));
			}
		}
		else
		{
			((CWizardTabDialog*)pDlg)->Deactivate();
			OnWizardAbort();
			((CWizardFormDoc*)GetDocument())->DispatchWizardCancel(pDlg->GetDialogID());
			((CWizardTabDialog*)pDlg)->OnWizardCancel();
		}
	}
	else
	{
		((CWizardTabDialog*)pDlg)->Deactivate();
		OnWizardAbort();
		((CWizardFormDoc*)GetDocument())->DispatchWizardCancel(pDlg->GetDialogID());
		((CWizardTabDialog*)pDlg)->OnWizardCancel();
	}
}
//-----------------------------------------------------------------------------
DlgInfoItem* CWizardFormView::GetLastEnabledItem()
{
	DlgInfoItem* pLastItem = NULL;
	DlgInfoArray* pAr = GetTabManager()->GetDlgInfoArray();
	for (int t = pAr->GetUpperBound(); t >= 0; t--)
		if (pAr->GetAt(t)->IsEnabled())
		{
			pLastItem = pAr->GetAt(t);
			break;
		}
	return pLastItem;
}

//-----------------------------------------------------------------------------
void CWizardFormView::SetWizardButtons(DWORD dwFlags)
{
	if (m_bUseOldButtonStyle)
	{
		CWnd* pWnd = GetDlgItem(IDC_WIZARD_BACK);
		ASSERT(pWnd);
		if (pWnd)
			pWnd->EnableWindow(dwFlags & PSWIZB_BACK);

		pWnd = GetDlgItem(IDC_WIZARD_NEXT);
		ASSERT(pWnd);
		if (pWnd)
			pWnd->EnableWindow(dwFlags & PSWIZB_NEXT);

		pWnd = GetDlgItem(IDC_WIZARD_FINISH);
		ASSERT(pWnd);
		if (pWnd)
		{
			pWnd->EnableWindow(dwFlags & PSWIZB_FINISH);
			/*Carloz per Scheduler*/
			if (GetDocument()->IsEditingParamsFromExternalController())
				pWnd->SetWindowText(_TB("&Save"));
		}
	}
	else
	{
		// 1 - BACK, NEXT
		// 2 - BACK, ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN
		// 3 - ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN
		// 4 - ID_WIZARD_RESTART, BACK

		CAbstractFormFrame* pFrame = GetFrame();
		if (!pFrame || !pFrame->HasToolbar())
			return;
		CTBTabbedToolbar* pTabbedToolbar = pFrame->GetTabbedToolBar();
		if (!pTabbedToolbar)
			return;

		// tabbed toolbar
		CTBToolBar* pToolbar = pTabbedToolbar->FindToolBar(szToolbarNameMain);
		if (!pToolbar)
			return;

		DlgInfoItem* pLastItem = GetLastEnabledItem();

		CTabDialog* pDialog = GetTabManager()->GetActiveDlg();
		BOOL bOnLastTab = pLastItem == pDialog->GetDlgInfoItem();

		if (!bOnLastTab && pDialog->IsKindOf(RUNTIME_CLASS(CWizardTabDialog)))
		{
			bOnLastTab = ((CWizardTabDialog *)pDialog)->IsLast();
		}

		if (bOnLastTab)
		{
			// la CanDoBatchExecute del documento va a decidere se la batch può essere fatta partire o meno

			if (GetDocument()->m_bBatchRunning & !m_bWizardFinished)
			{
				// punto 3
				pToolbar->HideButton(IDC_WIZARD_BACK, TRUE);
				pToolbar->HideButton(IDC_WIZARD_NEXT, TRUE);
				pToolbar->HideButton(ID_WIZARD_RESTART, TRUE);
				pToolbar->SetButtonInfo(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN, TBBS_BUTTON, TBIcon(szIconPause, TOOLBAR), _TB("Pause"));
				pToolbar->SetButtonInfo(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, TBBS_BUTTON, TBIcon(szIconStop, TOOLBAR), _TB("Stop"));
				pToolbar->HideButton(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, FALSE);
				pToolbar->HideButton(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN, FALSE);
			}
			else if (m_bWizardFinished)
			{
				// punto 4
				pToolbar->HideButton(IDC_WIZARD_BACK, TRUE);
				pToolbar->HideButton(IDC_WIZARD_NEXT, TRUE);
				pToolbar->HideButton(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN, TRUE);

				CWizardFormDoc* pDoc = dynamic_cast<CWizardFormDoc*>(GetDocument());
				ASSERT(pDoc);
				pToolbar->HideButton(ID_WIZARD_RESTART, FALSE);
				pToolbar->HideButton(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, !pDoc->IsReExecutable());
			}
			else
			{
				// punto 2
				pToolbar->HideButton(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, FALSE);
				pToolbar->SetButtonInfo(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, TBBS_BUTTON, TBIcon(szIconStart, TOOLBAR), _TB("Start"));
				pToolbar->SetButtonInfo(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN, TBBS_BUTTON, TBIcon(szIconPause, TOOLBAR), _TB("Pause"));
				pToolbar->HideButton(IDC_WIZARD_BACK, FALSE);
				pToolbar->HideButton(IDC_WIZARD_NEXT, TRUE);
				pToolbar->EnableButton(IDC_WIZARD_BACK, TRUE);
				pToolbar->HideButton(ID_WIZARD_RESTART, TRUE);
			}
		}
		else
		{
			// punto 1
			DlgInfoItem* pFirstItem = GetTabManager()->GetDlgInfoArray()->GetAt(0);
			BOOL bOnFirstTab = pFirstItem == pDialog->GetDlgInfoItem();
			pToolbar->HideButton(IDC_WIZARD_BACK, bOnFirstTab);
			pToolbar->HideButton(IDC_WIZARD_NEXT, FALSE);
			pToolbar->HideButton(ID_WIZARD_RESTART, TRUE);
			pToolbar->SetButtonInfo(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, TBBS_BUTTON, TBIcon(szIconStart, TOOLBAR), _TB("Start"));
			pToolbar->HideButton(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN, TRUE);
			pToolbar->HideButton(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN, TRUE);
		}

		pTabbedToolbar->UpdateTabWnd();
	}
}

//-----------------------------------------------------------------------------
void CWizardFormView::PerformBatchOperations()
{
	//Si tratta di un Wizard: ha uno ed un solo solo TabManager !
	ASSERT(m_pTabManagers->GetSize() == 1);
	CTabWizard* pTab = (CTabWizard*) m_pTabManagers->GetAt(0);
	ASSERT(pTab->IsKindOf(RUNTIME_CLASS(CTabWizard)));

	CWizardTabDialog *pLocalDialog = NULL;
	UINT nActiveDlgIDD = 0;

	OnWizardStart();
	do 
	{
		pLocalDialog = (CWizardTabDialog *) pTab->GetActiveDlg();
		ASSERT(pLocalDialog);
		nActiveDlgIDD = pLocalDialog->GetDialogID();
		OnWizardNext();
	}
	while (nActiveDlgIDD != pTab->GetActiveDlg()->GetDialogID());

	//gia chiamata nell'ultimo step della OnWizardNext?
	if (!m_bWizardFinished)
		OnWizardFinish();

}

//-----------------------------------------------------------------------------
void CWizardFormView::SetWizardBitmap(UINT nIDRes)
{
	CWnd* pWnd = GetDlgItem(m_IDCBitmap);
	ASSERT(pWnd);
	if (pWnd)
	{
		HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(nIDRes), RT_BITMAP);
		HBITMAP hBmp = ::LoadBitmap (hInst, MAKEINTRESOURCE (nIDRes));
		((CStatic*)pWnd)->SetBitmap(hBmp);
	}
}

//------------------------------------------------------------------------------
void CWizardFormView::OnSize (UINT nType, int cx, int cy)
{
	__super::OnSize (nType, cx, cy);	

	if (cy)
		DoAnchorages ();
}

//------------------------------------------------------------------------------
void CWizardFormView::DoAnchorages	()
{
	for (int i=0; i < m_arAnchorControls.GetSize(); i++)
	{
		CAnchorCtrl* pAc = (CAnchorCtrl*) m_arAnchorControls.GetAt(i);
		pAc->Move(this);
	}
}

//-----------------------------------------------------------------------------
void CWizardFormView::SyncExternalControllerInfo(BOOL bSave)
{
	if (bSave)
		GetDocument()->RetrieveControlData(m_pControlLinks);
	else
		GetDocument()->ValorizeControlData(m_pControlLinks, TRUE);

	for (int i= 0; i < this->m_pTileGroups->GetSize(); i++)
	{
		CTileGroup* tileGroup = m_pTileGroups->GetAt(i);
		ASSERT_VALID(tileGroup);

		for (int ii = 0; ii < tileGroup->GetTileDialogs()->GetSize(); ii++)
		{
			 
			CTileDialog* dialog = dynamic_cast<CTileDialog*>(tileGroup->GetTileDialogs()->GetAt(ii));
			ASSERT_VALID(dialog);

			if (bSave)
				dialog->GetDocument()->RetrieveControlData(dialog->GetControlLinks());
			else
				dialog->GetDocument()->ValorizeControlData(dialog->GetControlLinks(), FALSE);

		}

		for (int j = 0; j < tileGroup->GetTilePanels()->GetSize(); j++)
		{
			CTilePanel* tilePanel = dynamic_cast<CTilePanel*>(tileGroup->GetTilePanels()->GetAt(j));
			ASSERT(tilePanel);
		}
	}

}

//-----------------------------------------------------------------------------
void CWizardFormView::UpdateStepper(int nDirection /* 1*/)
{
	CFrameStepper* pStepper = GetStepper ();
	if (!pStepper)
		return;

	pStepper->UpdateStepper(GetStepperRootDescription(), GetTabManager(), nDirection);
}

//-----------------------------------------------------------------------------
CFrameStepper* CWizardFormView::GetStepper	()
{
	CMasterFrame* pFrame = GetFrame();
	
	return pFrame ? pFrame->GetFrameStepper() : NULL;
}

//-----------------------------------------------------------------------------
CString	CWizardFormView::GetStepperRootDescription	()
{
	return cwsprintf( _TB("%s"), GetDocument()->GetTitle());
}


/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CWizardFormView::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " CWizardFormView\n");
	CMasterFormView::Dump(dc);
}

void CWizardFormView::AssertValid() const
{
	CMasterFormView::AssertValid();
}

#endif //_DEBUG
//==============================================================================
//	CJsonFormView
//==============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonWizardFormView, CWizardFormView)

//-----------------------------------------------------------------------------
void CJsonWizardFormView::AssignJsonContext(CJsonContext* pContext)
{
	m_pJsonContext = pContext;
}


