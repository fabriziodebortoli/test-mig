#include "stdafx.h"
#include <TbGeneric\DataObj.h>
#include <TbGeneric\DataFileInfo.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGenlib\PARSOBJ.H>
#include <TbGenlib\TBCommandInterface.h>

#include "ItemSource.h"

//-------------------------------------------------------------------------------------------
//			IItemSource
//-------------------------------------------------------------------------------------------
IItemSource::IItemSource()
{
	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szFormsSection, szMaxComboBoxItems, DataLng(m_nMaxItemsNo), szTbDefaultSettingFileName);
	m_nMaxItemsNo = pSetting ? (pSetting->IsKindOf(RUNTIME_CLASS(DataInt)) ? (long)*((DataInt*)pSetting) : (long)*((DataLng*)pSetting)) : m_nMaxItemsNo;
}



IMPLEMENT_DYNCREATE(CItemSource, CObject)
//-----------------------------------------------------------------------------
CItemSource::CItemSource()
{
}

//-----------------------------------------------------------------------------
CItemSource::~CItemSource()
{
}

//-----------------------------------------------------------------------------
CAbstractFormDoc* CItemSource::GetDocument()
{
	return m_pDocument;
}

//-----------------------------------------------------------------------------
void CItemSource::SetDocument(CAbstractFormDoc* pDoc)
{
	m_pDocument = pDoc;
}

//-----------------------------------------------------------------------------
CString CItemSource::GetDescription(const DataObj* pValue)
{
	//m_bShowDescription se è true, per omogeneità di comportamento bisogna overridare anche la GetDescription()
	ASSERT(!m_bShowDescription);
	return m_pControl ? m_pControl->FormatData(pValue) : pValue ? pValue->Str() : _T("");
}

//-----------------------------------------------------------------------------
void CItemSource::SetControl(CParsedCtrl* pControl) {
	m_pControl = pControl;
	if (m_pControl)
	{
		//se c'è, rimuovo uno stile che mi formatta in modo diverso dal pregresso il testo, perché ho
		//cambiato il timing della chiamata (anche se in teoria sarebbe più corretto così)
		bool bHadStyle = (m_pControl->GetCtrlStyle() & NUM_STYLE_SHOW_ZERO) == NUM_STYLE_SHOW_ZERO;
		if (bHadStyle)
			m_pControl->SetCtrlStyle(m_pControl->GetCtrlStyle() & ~NUM_STYLE_SHOW_ZERO);

		OnControlAttached();
		//ripristino lo stile
		if (bHadStyle)
			m_pControl->SetCtrlStyle(m_pControl->GetCtrlStyle() | NUM_STYLE_SHOW_ZERO);
	}
}

//-----------------------------------------------------------------------------
CJsonSerializer CItemSource::GetJson(const CString& cmpId) 
{
	CJsonSerializer resp;
	resp.WriteString(_T("cmd"), _T("ItemSourceExtended"));
	resp.OpenObject(_T("args"));
	resp.WriteString(_T("cmpId"), cmpId);

	OnGetJson(resp);
	
	resp.CloseObject();
	return resp;
};

//-------------------------------------------------------------------------------------------
//			CItemSourceXml
//-------------------------------------------------------------------------------------------

IMPLEMENT_DYNCREATE(CItemSourceXml, CItemSource)
//-----------------------------------------------------------------------------
CItemSourceXml::CItemSourceXml()
	: CItemSource()
{

}

//-----------------------------------------------------------------------------
CItemSourceXml::~CItemSourceXml()
{
	//SAFE_DELETE(m_pDfi);  //memory leak?
}

//-----------------------------------------------------------------------------
void CItemSourceXml::GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue)
{
	if (!m_pDfi || m_pDfi->m_arElements.IsEmpty())
		return;

	CString sPrefix;

	if (m_pDfi->m_bFilterLike)
	{
		//TODOLUCA
		//sPrefix = CStrCombo::GetValue();
		sPrefix = strCurrentValue;
	}

	long idxCount = 0;
	for (long i = 0; i <= m_pDfi->m_arElements.GetUpperBound(); i++)
	{
		//TODOLUCA
		//if (idxCount >  m_nMaxItemsNo)
		//{
		//	//AddAssociation(cwsprintf(FormatMessage(MAX_ITEM_REACHED), m_nMaxItemsNo), _T(""));
		//	values.Add(DataStr().Clone());
		//	descriptions.Add(cwsprintf(FormatMessage(MAX_ITEM_REACHED), m_nMaxItemsNo));
		//	break;
		//}

		CString sVal = m_pDfi->GetValue(i);

		if (!sPrefix.IsEmpty())
		{
			//TRACE(m_pDfi->GetValue(i));TRACE(_T("\n"));

			if (::FindNoCase(sVal, sPrefix) != 0)
				continue;;
		}

		values.Add(DataStr(sVal).Clone());
		descriptions.Add(m_pDfi->GetDescription(i));
		idxCount++;
	}
}

//-----------------------------------------------------------------------------
void CItemSourceXml::SetKey(const CString& strFieldName)
{
	// controllo l'esistenza del DataFileInfo
	if (!m_pDfi /*|| !GetItemSource() || !GetItemSource()->GetDataFileInfo()*/)
		return;

	m_pDfi->ChangeKey(strFieldName);
}

//-----------------------------------------------------------------------------
void CItemSourceXml::SetHidden(const CString& strFieldName, BOOL bHidden /*= TRUE*/)
{
	// controllo l'esistenza del DataFileInfo
	if (!m_pDfi)
		return;

	m_pDfi->SetHidden(strFieldName, bHidden);
}

//-----------------------------------------------------------------------------
void CItemSourceXml::Initialize(const CString& strParameter, bool allowChanges /*false*/, bool bUseProductLanguage /* false*/)
{
	m_strParameter = strParameter;
	m_pDfi = AfxGetTbCmdManager()->GetDataFileInfo(strParameter, allowChanges, bUseProductLanguage);
}