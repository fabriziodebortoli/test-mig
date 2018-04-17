#include "stdafx.h"

#include <TbGenlib\PARSOBJ.H>
#include <TbGenlib\TBLinearGauge.h>
#include <TbGenlib\TBSplitterWnd.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "EXTDOC.H"
#include "Dbt.h"
#include "BODYEDIT.H"
#include "JsonFormEngineEx.h"
#include "HeaderStrip.h"
#include "FormMng.h"
#include "Aliases.h"
#include "StatusTile.h"
#include "UnpinnedTilesPane.h"
#include "HotFilterManager.h"

CJsonFormEngine g_Engine;

#define SET_BE_PROP(var, style)\
 if (var != B_UNDEFINED)\
	BESetExStyle(style, var == B_TRUE);

#define DEFINED_FUNCTION _T("Defined")
#define GET_PARENT_NAME_FUNCTION _T("GetParentName")
#define STATIC_AREA_WIDTH 100
#define LEFT_SECOND_STATIC_AREA 327
#define LEFT_COL_1_STD 101
#define LEFT_COL_1_MINI 4
#define LEFT_COL_2 428

//-----------------------------------------------------------------------------
void FillMissingColumnProps(CWndObjDescription* pRowViewDesc, CWndBodyColumnDescription* pColDesc)
{
	if (!pRowViewDesc)
		return;
	CWndObjDescription* pOther = pRowViewDesc->Find(pColDesc->GetID());
	if (!pOther)
		return;
	if (pColDesc->m_strActivation.IsEmpty())
	{
		pColDesc->m_strActivation = pOther->m_strActivation;
	}
	if (pColDesc->m_strName.IsEmpty())
		pColDesc->m_strName = pOther->m_strName;
	if (pColDesc->m_strControlClass.IsEmpty())
		pColDesc->m_strControlClass = pOther->m_strControlClass;
	if (pColDesc->m_strText.IsEmpty())
	{
		if (!pOther->m_strControlCaption.IsEmpty())
			pColDesc->m_strText = AfxLoadJsonString(pOther->m_strControlCaption, pOther);
		else if (!pOther->m_strText.IsEmpty())
			pColDesc->m_strText = AfxLoadJsonString(pOther->m_strText, pOther);
	}
	if (pColDesc->m_nChars == -1)
		pColDesc->m_nChars = pOther->m_nChars;
	if (pColDesc->m_nRows == 1)
		pColDesc->m_nRows = pOther->m_nRows;
	if (pColDesc->m_nTextLimit == 0)
		pColDesc->m_nTextLimit = pOther->m_nTextLimit;
	if (pColDesc->m_sMinValue.IsEmpty())
		pColDesc->m_sMinValue = pOther->m_sMinValue;
	if (pColDesc->m_sMaxValue.IsEmpty())
		pColDesc->m_sMaxValue = pOther->m_sMaxValue;

	if (pColDesc->m_nNumberDecimal == -1 && pOther->m_nNumberDecimal != -1)
		pColDesc->m_nNumberDecimal = pOther->m_nNumberDecimal;

	if (pOther->m_pControlBehaviourDescription && !pColDesc->m_pControlBehaviourDescription)
	{
		pColDesc->m_pControlBehaviourDescription = pOther->m_pControlBehaviourDescription->Clone();
	}
	if (pOther->m_pMenu && !pColDesc->m_pMenu)
	{
		pColDesc->m_pMenu = (CMenuDescription*)pOther->m_pMenu->DeepClone();
		pColDesc->m_pMenu->SetParent(pColDesc);
	}
	pColDesc->m_Expressions.Assign(&pOther->m_Expressions, pOther, pColDesc);
	CItemSourceDescription* pItemSource = NULL;
	//il default per sort e' false; se nella rowview ho specificato true, lo faccio prevalere su quello della colonna, ha senso che siano omogenei
	if (pOther->IsKindOf(RUNTIME_CLASS(CComboDescription)))
	{
		pColDesc->m_bSort = ((CComboDescription*)pOther)->m_bSort;
		pItemSource = ((CComboDescription*)pOther)->m_pItemSourceDescri;
	}
	else if (pOther->IsKindOf(RUNTIME_CLASS(CListDescription)))
	{
		pColDesc->m_bSort = ((CListDescription*)pOther)->m_bSort;
		pItemSource = ((CComboDescription*)pOther)->m_pItemSourceDescri;
	}

	if (pItemSource && !pColDesc->m_pItemSourceDescri)
		pColDesc->m_pItemSourceDescri = pItemSource->Clone();


	if (pOther->IsKindOf(RUNTIME_CLASS(CTextObjDescription)))
	{
		CTextObjDescription* textDesc = ((CTextObjDescription*)pOther);
		for (int i = 0; i < textDesc->m_arValidators.GetSize(); i++)
		{
			CValidatorDescription* pValidator = textDesc->m_arValidators[i]->Clone();
			pColDesc->m_arValidators.Add(pValidator);
		}
	}

	if (pOther->m_pBindings)
	{
		if (!pColDesc->m_pBindings)
		{
			pColDesc->m_pBindings = new BindingInfo();
		}
		pColDesc->m_pBindings->AssignDefaults(pOther->m_pBindings);
	}

	if (pOther->m_pStateData)
	{
		if (!pColDesc->m_pStateData)
		{
			pColDesc->m_pStateData = new StateData();
		}
		pColDesc->m_pStateData->AssignDefaults(pOther->m_pStateData);
	}
}

//-----------------------------------------------------------------------------
unsigned int CountLines(CString str) 
{

	int curPos = 0, counter = 1;
	curPos = str.Find(_T("\n"), curPos);
	while (curPos != -1)
	{
		counter++;
		curPos = str.Find(_T("\n"), curPos + 1);
	};
	return counter;
}

//-----------------------------------------------------------------------------
CAbstractFormView* GetParentView(CWnd* pWnd)
{
	while (pWnd && !pWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		pWnd = pWnd->GetParent();
	return (CAbstractFormView*)pWnd;
}
void AssignProps(CParsedCtrl* pParsedCtrl, DataObj* pDataObj, CWndObjDescription* pDesc)
{
	if (!pDesc->m_sMinValue.IsEmpty())
	{
		::DataObj* pMinValue = pDataObj ?				//clono o creo il dataobj a partire dal tipo di dato
			pDataObj->Clone() : ::DataObj::DataObjCreate(pParsedCtrl->GetDataType());
		pMinValue->AssignFromXMLString(pDesc->m_sMinValue);		//assegno il tipo di dato in formato stringa, il dataobj sa convertire nel proprio tipo
		pParsedCtrl->SetMinValue(*pMinValue);			//assegno il min value al parsed control
		delete pMinValue;
	}

	if (!pDesc->m_sMaxValue.IsEmpty())
	{
		::DataObj* pMaxValue = pDataObj ?				//clono o creo il dataobj a partire dal tipo di dato
			pDataObj->Clone() : ::DataObj::DataObjCreate(pParsedCtrl->GetDataType());
		pMaxValue->Assign(pDesc->m_sMaxValue);
		pParsedCtrl->SetMaxValue(*pMaxValue);
		delete pMaxValue;
	}

	if (pDesc->m_ControlStyle != CS_NONE)
	{
		if ((pDesc->m_ControlStyle & CS_RESET_DEFAULTS) == CS_RESET_DEFAULTS)
		{
			DWORD s = pDesc->m_ControlStyle &~CS_RESET_DEFAULTS;
			pParsedCtrl->SetCtrlStyle(s);
		}
		else
		{
			pParsedCtrl->SetCtrlStyle(pParsedCtrl->GetCtrlStyle() | pDesc->m_ControlStyle);
		}

	}

}

//-----------------------------------------------------------------------------
class ActivationDynamicSymTable : public SymTable
{
	CJsonContext* m_pContext;
public:
	ActivationDynamicSymTable(CJsonContext* pContext) : m_pContext(pContext)
	{
		SetOwns(TRUE);
	}

	virtual SymField* GetField(LPCTSTR pszName) const
	{
		SymField* pField = __super::GetField(pszName);
		if (!pField)
		{
			DataBool bVariable(m_pContext->CheckActivation(pszName));
			pField = new SymField(pszName, DataType::Bool, 0, &bVariable);
			((SymTable*)this)->Add(pField);
		}
		return pField;
	}
	BOOL ResolveCallMethod(CString sFuncName, CFunctionDescription& aMethod, CString& sHandleName) const
	{
		if (sFuncName.CompareNoCase(DEFINED_FUNCTION) == 0)
		{
			aMethod.SetName(sFuncName);
			aMethod.SetNamespace(CTBNamespace(_T("Function.Framework.TbGes.TbGes.") + sFuncName));
			aMethod.AddStrParam(_T("varName"), _T(""));
			aMethod.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

			return TRUE;
		}
		CBaseDocument* pDoc = GetDocument();
		if (!pDoc || !pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
			return FALSE;
		CAbstractFormDoc* pExtDoc = (CAbstractFormDoc*)pDoc;
		if (pExtDoc->ExistAction(sFuncName))
		{
			aMethod.SetNamespace(CTBNamespace(_T("Function.Framework.TbGes.TbGes.") + sFuncName));
			aMethod.SetName(sFuncName);
			aMethod.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
			return TRUE;
		}
		return __super::ResolveCallMethod(sFuncName, aMethod, sHandleName);
	}

	virtual BOOL DispatchFunctionCall(CFunctionDescription* pFunc)
	{
		CBaseDocument* pDoc = GetDocument();
		if (!pDoc || !pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
			return FALSE;

		CAbstractFormDoc* pExtDoc = (CAbstractFormDoc*)pDoc;
		if (pFunc->GetName().CompareNoCase(DEFINED_FUNCTION) == 0)
		{
			DataObj* pDataObj = pFunc->GetParamValue(_T("varName"));
			if (!pDataObj->IsKindOf(RUNTIME_CLASS(DataStr)))
			{
				ASSERT(FALSE);
				return FALSE;
			}
			bool exist = NULL != pExtDoc->GetVariable(pDataObj->Str());
			pFunc->SetReturnValue(DataBool(exist));
			return TRUE;
		}
		return pDoc->DispatchFunctionCall(pFunc);
	}
};


//-----------------------------------------------------------------------------
void CJsonContext::GetBindingInfo(CString sId, CString sName, BindingInfo* pBindingInfo, DBTObject*& pDBT, SqlRecord*& pRecord, DataObj*& pDataObj, CString& sBindingName)
{
	sBindingName = sName;

	if (!pBindingInfo)
	{
		if (sBindingName.IsEmpty())
			sBindingName = sId;
		return;
	}

	if (m_pDoc)
	{
		CString sDataSource = pBindingInfo->m_strDataSource;
		m_pDoc->GetBindingInfo(sDataSource, sId, pDBT, pRecord, pDataObj, sBindingName, TRUE);
	}
}
//-----------------------------------------------------------------------------
void CJsonContext::Associate(CWnd* pWnd)
{
	m_pWnd = pWnd;
	m_pFont = pWnd->GetFont();
	m_pDoc = GetDocument(pWnd);
	m_pForm = GetParsedForm(pWnd);

	if (m_pForm)
	{
		if (!m_pDoc)
		{
			CBaseDocument* pBaseDoc = m_pForm->GetDocument();
			if (pBaseDoc && pBaseDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
				m_pDoc = (CAbstractFormDoc*)pBaseDoc;
		}
		m_pLinks = m_pForm->GetControlLinks();
	}
	m_pDescription->AttachTo(pWnd->m_hWnd);
}

//-----------------------------------------------------------------------------
void CJsonContext::Assign(CJsonContextObj* pOther)
{
	__super::Assign(pOther);
	m_pLinks = ((CJsonContext*)pOther)->m_pLinks;
	m_pDoc = ((CJsonContext*)pOther)->m_pDoc;
	m_pForm = ((CJsonContext*)pOther)->m_pForm;
}

//-----------------------------------------------------------------------------
UINT CJsonContext::GetRowFormViewId(CWndBodyDescription* pWndDesc)
{
	CString sRowView = pWndDesc->m_strRowView;
	if (sRowView.IsEmpty())
		return 0;
	CString sResourceName, sContext;
	CJsonResource::SplitNamespace(sRowView, sResourceName, sContext);
	if (sContext.IsEmpty())
		sContext = m_JsonResource.m_strContext;
	return AfxGetTBResourcesMap()->GetTbResourceID(sResourceName, TbResources, 1, sContext);
}
//-----------------------------------------------------------------------------
void CJsonContext::BuildDataControlLinks()
{
	//valorizzo eventuali variabili calcolate come espressione; lo faccio ora, perché ho il documento
	//completo delle sue variabili
	m_pDescription->EvaluateExpressions(this);
	if (!m_pView)
	{
		if (m_pParentContext)
			m_pView = ((CJsonContext*)m_pParentContext)->GetOwnerView();
		if (!m_pView)
			m_pView = GetParentView(m_pWnd);
	}
	DBTObject* pDBT = NULL;
	//ricorsione sui figli
	for (int i = 0; i < m_pDescription->m_Children.GetCount(); i++)
	{
		CWndObjDescription* pChild = m_pDescription->m_Children[i];
		((CJsonFormEngine*)CJsonFormEngineObj::GetInstance())->AddLink(pChild, m_pWnd, this, m_pView, pDBT);
	}

}
//-----------------------------------------------------------------------------
BOOL CJsonContext::TranslateAliasDefaults(CString& sAlias, CWndObjDescription::WndObjType controlType)
{
	CString sBareText;
	if (sAlias.IsEmpty() || (sAlias.GetAt(0) != ALIAS_IDENTIFIER && !CJsonFormEngineObj::IsExpression(sAlias, sBareText)))
		return FALSE;

	switch (controlType)
	{
	case CWndObjDescription::Label:		sAlias = _T("StringStatic");
		break;
	case CWndObjDescription::Button:	sAlias = _T("Button");
		break;
	case CWndObjDescription::Group:		sAlias = _T("EnumButton");
		break;
	case CWndObjDescription::Radio:		sAlias = _T("RadioButton");
		break;
	case CWndObjDescription::Check:		sAlias = _T("CheckBox");
		break;
	case CWndObjDescription::Combo:		sAlias = _T("StringComboDropDown");
		break;
	case CWndObjDescription::Edit:		sAlias = _T("StringEdit");
		break;
	case CWndObjDescription::BodyEdit:	sAlias = _T("BodyEdit");
		break;
	case CWndObjDescription::List:		sAlias = _T("StringListBox");
		break;
	case CWndObjDescription::TreeAdv:	sAlias = _T("TreeView");
		break;
	default:
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CJsonContext::AttachItemSource(CItemSourceDescription* pItemSourceDescription, CParsedCtrl* pParsedCtrl)
{
	CString sItemSource = pItemSourceDescription->m_strItemSourceName;
	CString sItemSourceNS = pItemSourceDescription->m_strItemSourceNamespace;
	CString sItemSourceParameter = pItemSourceDescription->m_strItemSourceParameter;
	bool bItemSourceUseProductLanguage = pItemSourceDescription->m_bItemSourceUseProductLanguage;
	bool bAllowChanges = pItemSourceDescription->m_bAllowChanges;

	CItemSource* pItemSource = m_pDoc->GetItemSource(sItemSource, CTBNamespace(CTBNamespace::ITEMSOURCE, sItemSourceNS), sItemSourceParameter, bItemSourceUseProductLanguage, bAllowChanges);
	if (pItemSource)
	{
		pParsedCtrl->SetItemSource(pItemSource);
	}
}

//-----------------------------------------------------------------------------
void CJsonContext::AttachDataAdapter(CDataAdapterDescription* pDataAdapterDescription, CParsedCtrl* pParsedCtrl)
{
	CString sDataAdapterName = pDataAdapterDescription->m_strDataAdapterName;
	CString sDataAdapterNamespace = pDataAdapterDescription->m_strDataAdapterNamespace;

	CDataAdapter* pDataAdapter = m_pDoc->GetDataAdapter(sDataAdapterName, CTBNamespace(CTBNamespace::DATA_ADAPTER, sDataAdapterNamespace));

	if (pDataAdapter)
	{
		pParsedCtrl->SetDataAdapter(pDataAdapter);
	}
}

//-----------------------------------------------------------------------------
void CJsonContext::AttachValidator(CValidatorDescription* pValidatorDescription, CParsedCtrl* pParsedCtrl)
{
	CString sValidatorName = pValidatorDescription->m_strValidatorName;
	CString sValidatorNamespace = pValidatorDescription->m_strValidatorNamespace;

	CValidator* pValidator = m_pDoc->GetValidator(sValidatorName, CTBNamespace(CTBNamespace::VALIDATOR, sValidatorNamespace));

	if (pValidator)
	{
		pParsedCtrl->AddValidator(pValidator);
	}
}

//-----------------------------------------------------------------------------
void CJsonContext::AttachControlBehaviour(CControlBehaviourDescription* pControlBehaviourDescription, CParsedCtrl* pParsedCtrl)
{
	CString sName = pControlBehaviourDescription->m_strName;
	CString sNamespace = pControlBehaviourDescription->m_strNamespace;

	CControlBehaviour* pControlBehaviour = m_pDoc->GetControlBehaviour(sName, CTBNamespace(CTBNamespace::CONTROL_BEHAVIOUR, sNamespace));
	if (pControlBehaviour)
	{
		pParsedCtrl->SetControlBehaviour(pControlBehaviour);
		if (pControlBehaviourDescription->m_bItemSource)
		{
			IItemSource* pIS = dynamic_cast<IItemSource*>(pControlBehaviour);
			if (pIS)
			{
				pParsedCtrl->SetItemSource(pIS);
			}
			else
			{
				ASSERT_TRACE1(FALSE, "Control behaviour %s is declared to be an item source but does not implement IItemSource interface\r\n", (LPCTSTR)sName);
			}
		}
		if (pControlBehaviourDescription->m_bValidator)
		{
			IValidator* pV = dynamic_cast<IValidator*>(pControlBehaviour);
			if (pV)
			{
				pParsedCtrl->AddValidator(pV);
			}
			else
			{
				ASSERT_TRACE1(FALSE, "Control behaviour %s is declared to be a validator but does not implement IValidator interface\r\n", (LPCTSTR)sName);
			}

		}

		if (pControlBehaviourDescription->m_bDataAdapter)
		{
			IDataAdapter* pDA = dynamic_cast<IDataAdapter*>(pControlBehaviour);
			if (pDA)
			{
				pParsedCtrl->SetDataAdapter(pDA);
			}
			else
			{
				ASSERT_TRACE1(FALSE, "Control behaviour %s is declared to be a data adapter but does not implement IDataAdapter interface\r\n", (LPCTSTR)sName);
			}
		}
	}
}


//-----------------------------------------------------------------------------
CRuntimeClass* CJsonContext::GetControlClass(CString sControlClass, CWndObjDescription::WndObjType controlType, DataObj* pDataObj, DWORD& dwNeededStyle, DWORD& dwNeededExStyle)
{
	//se il primo translate non è andato a buon fine, guardo che non sia un alias "customizzato" che comincia con @ o {{
	// e nel caso gli associo una control class veritiera di default 
	if (!m_pDoc->TranslateAlias(sControlClass))
		TranslateAliasDefaults(sControlClass, controlType);

	IID clsId;
	if (SUCCEEDED(CLSIDFromString(sControlClass, &clsId)))
		return NULL;//ActiveX control
	CRuntimeClass* pClass = NULL;
	//if control class in undefined, retrieve default from data type
	CRegisteredParsedCtrl* pCtrl = NULL;
	if (sControlClass.IsEmpty())
	{
		if (pDataObj)
		{
			const CParsedCtrlFamily* pFamily = AfxGetParsedControlsRegistry()->GetDefaultFamilyInfo(pDataObj->GetDataType());
			if (pFamily)
			{
				pCtrl = pFamily->GetRegisteredControl(pDataObj->GetDataType());
			}
		}
	}
	else
	{
		pCtrl = AfxGetParsedControlsRegistry()->GetRegisteredControl(sControlClass);
	}

	if (pCtrl)
	{
		pClass = pCtrl->GetClass();
		dwNeededStyle = pCtrl->GetNeededStyle();
		dwNeededExStyle = pCtrl->GetNeededExStyle();
	}
	else
	{
		ASSERT(sControlClass.IsEmpty());
	}
	return pClass;
}

//-----------------------------------------------------------------------------
bool CJsonContext::CanCreateControl(CWndObjDescription* pWndDesc, UINT nID)
{
	if (m_pDoc && m_pDoc->IsInStaticDesignMode())
		//se sto editando, tutti i controlli devono essere creati
		return true;
	if (!CheckActivationExpression(pWndDesc->m_strActivation))
		return false;
	if (m_pDoc)
	{
		if (!m_pDoc->CanCreateControl(nID) || !m_pDoc->m_pClientDocs->CanCreateControl(nID))
			return false;
		POSITION pos = m_pDoc->GetFirstViewPosition();
		while (pos)
		{
			CView* pView = m_pDoc->GetNextView(pos);
			if (pView && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)) && !((CAbstractFormView*)pView)->CanCreateControl(nID))
				return false;
		}
	}
	return true;
}
//-----------------------------------------------------------------------------
bool CJsonContext::Evaluate(CJsonExpressions& expressions, CWndObjDescription* pDescri)
{
	if (m_pDoc && m_pDoc->IsInStaticDesignMode())
		//se sto editando, tutti i controlli devono essere creati
		return false;


	//se l'oggetto non è attivato, non ha senso valutarne le espressioni, potrebbe fra l'altro dare origine ad errori fasulli
	if (!CheckActivationExpression(pDescri->m_strActivation))
		return false;

	POSITION pos = expressions.m_BoolExpressions.GetStartPosition();
	while (pos)
	{
		bool* pKey = NULL;
		CString sExpr;
		expressions.m_BoolExpressions.GetNextAssoc(pos, pKey, sExpr);
		EvaluateExpression<bool, DataBool>(sExpr, pDescri, *pKey);
	}

	pos = expressions.m_IntExpressions.GetStartPosition();
	while (pos)
	{
		int* pKey = NULL;
		CString sExpr;
		expressions.m_IntExpressions.GetNextAssoc(pos, pKey, sExpr);
		EvaluateExpression<int, DataLng>(sExpr, pDescri, *pKey);
	}
	pos = expressions.m_DoubleExpressions.GetStartPosition();
	while (pos)
	{
		double* pKey = NULL;
		CString sExpr;
		expressions.m_DoubleExpressions.GetNextAssoc(pos, pKey, sExpr);
		EvaluateExpression<double, DataDbl>(sExpr, pDescri, *pKey);
	}
	pos = expressions.m_Bool3Expressions.GetStartPosition();
	while (pos)
	{
		Bool3* pKey = NULL;
		CString sExpr;
		expressions.m_Bool3Expressions.GetNextAssoc(pos, pKey, sExpr);
		EvaluateExpression<Bool3, DataBool>(sExpr, pDescri, *pKey);
	}
	pos = expressions.m_StringExpressions.GetStartPosition();
	while (pos)
	{
		CString* pKey = NULL;
		CString sExpr;
		expressions.m_StringExpressions.GetNextAssoc(pos, pKey, sExpr);
		EvaluateExpression<CString, DataStr>(sExpr, pDescri, *pKey);
	}
	return true;
}

//-----------------------------------------------------------------------------
class ExpressionDynamicSymTable : public SymTable
{
	CWndObjDescription* m_pDescription;
public:
	ExpressionDynamicSymTable(CWndObjDescription* pDescription)
		:
		m_pDescription(pDescription)
	{
		SetOwns(TRUE);
	}
	virtual BOOL DispatchFunctionCall(CFunctionDescription* pFunc)
	{
		if (pFunc->GetName() == GET_PARENT_NAME_FUNCTION)
		{
			CWndObjDescription* pDesc = m_pDescription->GetParent();
			if (!pDesc)
				return false;
			pFunc->SetReturnValue(DataStr(pDesc->m_strName));
			return TRUE;
		}
		return FALSE;
	}
	BOOL ResolveCallMethod(CString sFuncName, CFunctionDescription& aMethod, CString& sHandleName) const
	{
		if (sFuncName == GET_PARENT_NAME_FUNCTION)
		{
			aMethod.SetName(sFuncName);
			aMethod.SetNamespace(CTBNamespace(_T("Function.Framework.TbGes.TbGes.") + sFuncName));
			return TRUE;
		}

		return __super::ResolveCallMethod(sFuncName, aMethod, sHandleName);
	}
	virtual SymField* GetField(LPCTSTR pszName) const
	{
		SymField* pField = __super::GetField(pszName);
		if (!pField)
		{
			DBTObject* pDBT = NULL;
			SqlRecord* pRecord = NULL;
			DataObj* pDataObj = NULL;
			CString sBindingName;
			if (m_pDocument)
			{
				ASSERT_KINDOF(CAbstractFormDoc, m_pDocument);
				((CAbstractFormDoc*)m_pDocument)->GetBindingInfo(pszName, _T(""), pDBT, pRecord, pDataObj, sBindingName, FALSE);
			}
			if (pDataObj != NULL)
			{
				pField = new SymField(pszName, pDataObj->GetDataType(), 0, pDataObj);
				((SymTable*)this)->Add(pField);
			}
		}
		return pField;
	}

};
//-----------------------------------------------------------------------------
template <class T, class TDataObj>
bool CJsonContext::EvaluateExpression(const CString& sBareText, CWndObjDescription* pDescri, T& iOut)
{
	return EvaluateExpression<T, TDataObj>(m_pDoc, sBareText, pDescri, iOut);
}
//-----------------------------------------------------------------------------
template <class T, class TDataObj>
bool CJsonContext::EvaluateExpression(CAbstractFormDoc* pDocument, const CString& sBareText, CWndObjDescription* pDescri, T& iOut)
{	CString sAdjustedExpression = AdjustExpression(sBareText);
	Parser parser(sAdjustedExpression);
	ExpressionDynamicSymTable table(pDescri);
	table.SetDocument(pDocument);
	Expression expr(&table);
	TDataObj di;
	if (!expr.Parse(parser, di.GetDataType(), TRUE))
	{
		TRACE1("Invalid expression: %s\r\n", (LPCTSTR)sBareText);
		return false; //un'espressione sintatticamente sbagliata non mi fa creare il campo

	}

	if (!expr.Eval(di))
	{
		TRACE1("Invalid expression: %s\r\n", (LPCTSTR)sBareText);
		return false; //un'espressione sintatticamente sbagliata non mi fa creare il campo
	}

	iOut = (T)di;
	return true;
}

//-----------------------------------------------------------------------------
CAbstractFormView* CJsonContext::GetOwnerView()
{
	if (m_pView)
		return m_pView;
	if (m_pParentContext)
		return ((CJsonContext*)m_pParentContext)->GetOwnerView();
	return NULL;
}
//-----------------------------------------------------------------------------
void CJsonContext::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	if (!m_pDoc)
		return;
	m_pDoc->OnParsedControlCreated(pCtrl);
	m_pDoc->GetHotFilterManager()->OnParsedControlCreated(pCtrl);
	m_pDoc->m_pClientDocs->OnParsedControlCreated(pCtrl);
	CAbstractFormView* pView = GetOwnerView();
	if (pView)
		pView->OnParsedControlCreated(pCtrl);
}
//-----------------------------------------------------------------------------
void CJsonContext::OnColumnInfoCreated(ColumnInfo* pColInfo)
{
	if (!m_pDoc)
		return;
	m_pDoc->OnColumnInfoCreated(pColInfo);
	m_pDoc->m_pClientDocs->OnColumnInfoCreated(pColInfo);
	CAbstractFormView* pView = GetOwnerView();
	if (pView)
		pView->OnColumnInfoCreated(pColInfo);
}

//-----------------------------------------------------------------------------
void CJsonContext::OnPropertyCreated(CTBProperty* pProperty)
{
	if (!m_pDoc)
		return;

	m_pDoc->OnPropertyCreated(pProperty);
	m_pDoc->m_pClientDocs->OnPropertyCreated(pProperty);
	CAbstractFormView* pView = GetOwnerView();
	if (pView)
		pView->OnPropertyCreated(pProperty);
}

//-----------------------------------------------------------------------------
BOOL CJsonContext::OnGetToolTipProperties(CBETooltipProperties* pTooltip)
{
	if (!m_pDoc)
		return FALSE;

	if (m_pDoc->OnGetToolTipProperties(pTooltip))
		return TRUE;
	if (m_pDoc->m_pClientDocs->OnGetToolTipProperties(pTooltip))
		return TRUE;
	CAbstractFormView* pView = GetOwnerView();
	if (pView)
		pView->OnGetToolTipProperties(pTooltip);
	return FALSE;
}

//---------------------------------------------------------------------------
bool CJsonContext::CheckActivation(CString sActivation)
{
	sActivation = sActivation.Trim();
	if (sActivation.IsEmpty())
		return true;

	DBTObject* pDBT = NULL;
	SqlRecord* pRecord = NULL;
	DataObj* pDataObj = NULL;
	CString sBindingName;
	//prima controllo se si trata di una variabile
	if (m_pDoc)
		m_pDoc->GetBindingInfo(sActivation, _T(""), pDBT, pRecord, pDataObj, sBindingName, FALSE);
	if (pDataObj && pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)))
		return TRUE == (BOOL)*((DataBool*)pDataObj);

	CString activationApplicationName;
	CString activationModuleName;
	//se non si trata di variabile, allora e' una stringa di attivazione
	// <nome_applicazione>.<nome_modulo>
	int idx = sActivation.Find('.');
	if (idx >= 0)
	{
		activationApplicationName = sActivation.Mid(0, idx);
		activationModuleName = sActivation.Mid(idx + 1);

		return TRUE == AfxIsActivated(activationApplicationName, activationModuleName);
	}
	TRACE1("Invalid activation tag: %s\r\n", (LPCTSTR)sActivation);
	ASSERT(FALSE);
	return false;
}

//---------------------------------------------------------------------------
CString CJsonContext::AdjustExpression(const CString& sRawExpression)
{
	//aggiusto le espressioni a beneficio del parser:
	//! deve diventare NOT
	//& deve diventare &&
	//| deve diventare ||
	CString sAdjustedExpression;
	LPTSTR szTarget = sAdjustedExpression.GetBuffer(sRawExpression.GetLength() + 100);
	LPCTSTR sz = (LPCTSTR)sRawExpression;
	int i = 0;
	while (*sz)
	{
		switch (*sz)
		{
		case '&':
		case '|':
		{
			szTarget[i++] = *sz;
			szTarget[i++] = *sz;
			if (*(sz + 1) == *sz)
				sz++;
			break;
		}
		case '!':
		{
			szTarget[i++] = ' ';
			szTarget[i++] = 'N';
			szTarget[i++] = 'O';
			szTarget[i++] = 'T';
			szTarget[i++] = ' ';
			break;
		}
		default:
			szTarget[i++] = *sz;
			break;
		}
		sz++;
	}
	szTarget[i++] = 0;
	sAdjustedExpression.ReleaseBuffer();
	return sAdjustedExpression;
}
//---------------------------------------------------------------------------
void CJsonContext::GetActivationExpressions(CStringArray& arIds, CArray<bool>& arActivated)
{
	GetActivationExpressions(m_pDescription, arIds, arActivated);
}
//---------------------------------------------------------------------------
void CJsonContext::GetActivationExpressions(CWndObjDescription* pDesc, CStringArray& arIds, CArray<bool>& arActivated)
{
	if (!pDesc->m_strActivation.IsEmpty())
	{
		bool found = false;
		for (int i = 0; i < arIds.GetCount(); i++)
			if (arIds[i] == pDesc->m_strActivation)
			{
				found = true;
				break;
			}
		if (!found)
		{
			arIds.Add(pDesc->m_strActivation);
			arActivated.Add(CheckActivationExpression(pDesc->m_strActivation));
		}
	}

	if (pDesc->m_Type == CWndObjDescription::BodyEdit)
	{
		CWndBodyDescription* pBodyDesc = (CWndBodyDescription*)pDesc;
		CWndObjDescription* pRowViewDesc = pBodyDesc->GetRowViewDescription(this);
		if (pRowViewDesc)
		{
			GetActivationExpressions(pRowViewDesc, arIds, arActivated);
		}
	}
	for (int i = 0; i < pDesc->m_Children.GetCount(); i++)
	{
		GetActivationExpressions(pDesc->m_Children.GetAt(i), arIds, arActivated);
	}
}

//---------------------------------------------------------------------------
bool CJsonContext::CheckActivationExpression(const CString& activationExpression)
{
	if (activationExpression.IsEmpty())
		return true;
	CString sAdjustedExpression = AdjustExpression(activationExpression);
	Parser parser(sAdjustedExpression);

	ActivationDynamicSymTable table(this);
	table.SetDocument(m_pDoc);
	Expression expr(&table);
	DataBool b;
	if (expr.Parse(parser, DataType::Bool, TRUE) && expr.Eval(b))
	{
		return (bool)b;
	}
	ASSERT(FALSE);
	TRACE1("Invalid activation expression: %s\r\n", (LPCTSTR)activationExpression);
	return false;
}

//-----------------------------------------------------------------------------
void CJsonContext::EnableBodyEditButtons(CBodyEdit* pBodyEdit)
{
	if (!m_pDoc)
		return;

	m_pDoc->EnableBodyEditButtons(pBodyEdit);
	m_pDoc->m_pClientDocs->EnableBodyEditButtons(pBodyEdit);
	CView* pView = m_pDoc->GetFirstView();
	if (pView && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		((CAbstractFormView*)pView)->EnableBodyEditButtons(pBodyEdit);
}


//-----------------------------------------------------------------------------
CRuntimeClass* CHotFilterJsonContext::GetControlClass(CString sControlClass, CWndObjDescription::WndObjType controlType, DataObj* pDataObj, DWORD& dwNeededStyle, DWORD& dwNeededExStyle)
{
	if (!m_sCommnControlClass.IsEmpty())
		sControlClass = m_sCommnControlClass;
	return __super::GetControlClass(sControlClass, controlType, pDataObj, dwNeededStyle, dwNeededExStyle);
}

//-----------------------------------------------------------------------------
CJsonFormEngine* CJsonFormEngine::GetInstance()
{
	return (CJsonFormEngine*)CJsonFormEngineObj::GetInstance();
}

//-----------------------------------------------------------------------------
BOOL CJsonFormEngine::IsValid(CLocalizableDialog* pDialog)
{
	CRuntimeClass *pClass = pDialog->GetRuntimeClass();
	return pClass != RUNTIME_CLASS(CLocalizableDialog) && pClass != RUNTIME_CLASS(CParsedDialog);
}
//-----------------------------------------------------------------------------
int CJsonFormEngine::GetLeftMargin(CWndObjDescription* pTileDesc, bool col1, bool bInStaticArea)
{
	if (pTileDesc == NULL || (pTileDesc->m_Type != CWndObjDescription::Tile && pTileDesc->m_Type != CWndObjDescription::HotFilter))
	{
		//ASSERT(FALSE);
		return 0;
	}
	if (col1)
	{
		if (((CWndTileDescription*)pTileDesc)->m_Size == TileDialogSize::TILE_MINI)
		{
			return ((CWndTileDescription*)pTileDesc)->m_bHasStaticArea
				? pTileDesc->m_Width / 3
				: LEFT_COL_1_MINI;
		}
		else
		{
			return ((CWndTileDescription*)pTileDesc)->m_bHasStaticArea
				? (bInStaticArea ? LEFT_COL_1_STD - STATIC_AREA_WIDTH : LEFT_COL_1_STD)
				: LEFT_COL_1_MINI;
		}

	}
	else
	{
		int left = ((CWndTileDescription*)pTileDesc)->m_nCol2Margin;
		if (left == NULL_COORD)
			left = ((CWndTileDescription*)pTileDesc)->m_Size == TileDialogSize::TILE_WIDE
			? LEFT_COL_2
			: pTileDesc->m_Width / 2;

		return bInStaticArea ? left - STATIC_AREA_WIDTH : left;
	}
}



//-----------------------------------------------------------------------------
CJsonFormEngine::CJsonFormEngine()
{
	CJsonFormEngineObj::g_pJsonFormEngine = this;
}
//-----------------------------------------------------------------------------
CJsonFormEngine::~CJsonFormEngine()
{
	ClearDescriptions();
}

//-----------------------------------------------------------------------------
void CJsonFormEngine::ClearDescriptions()
{
	TB_OBJECT_LOCK(&m_Descriptions);

	POSITION pos = m_Descriptions.GetStartPosition();
	while (pos)
	{
		CString sFile;
		CWndObjDescription *pDescription;
		m_Descriptions.GetNextAssoc(pos, sFile, pDescription);
		delete pDescription;
	}
}

//-----------------------------------------------------------------------------
void CJsonFormEngine::InitCachePath()
{
	if (m_sCachePath.IsEmpty())
	{
		TCHAR path[MAX_PATH];
		m_sCachePath = PathCombine(path, AfxGetPathFinder()->GetAppDataPath(TRUE), szJsonForms);
	}

}
//-----------------------------------------------------------------------------
CJsonContextObj* CJsonFormEngine::CreateContext(const CJsonResource& sJsonResource, bool bCacheDescriptions, bool bIsJsonEditor)
{
	CJsonContextObj* pContext = CJsonContext::Create();
	pContext->m_bIsJsonDesigner = bIsJsonEditor;
	pContext->m_JsonResource = sJsonResource;
	pContext->m_strCurrentResourceContext = sJsonResource.m_strContext;
	if (pContext->m_strCurrentResourceContext.IsEmpty()) {
		ASSERT(FALSE);
		return nullptr;
	}
#ifdef DEBUG
	bCacheDescriptions = false;
#endif
	//sources contiene la lista dei file da caricare, il primo è il principale, gli altri, eventuali, quelli aggiunti da client doc
	CWndObjDescription *pDescription = pContext->m_pDescription;
	{
		TB_OBJECT_LOCK_FOR_READ(&m_Descriptions);
		if (bCacheDescriptions && m_Descriptions.Lookup(pContext->m_JsonResource.m_strName, pDescription))
		{
			pContext->m_pDescription = pDescription->DeepClone();
			return pContext;
		}
	}
	TB_OBJECT_LOCK(&m_Descriptions);
	if (bCacheDescriptions && m_Descriptions.Lookup(pContext->m_JsonResource.m_strName, pDescription))
	{
		pContext->m_pDescription = pDescription->DeepClone();
		return pContext;
	}
	bool bCacheOnFileSystem = false;
	TCHAR path[MAX_PATH];
	if (bCacheOnFileSystem)
	{
		InitCachePath();
		if (!PathFileExists(m_sCachePath))
			CreateDirectory(m_sCachePath);
		PathCombine(path, m_sCachePath, sJsonResource.m_strName + szTBJsonFileExt);
		if (PathFileExists(path))
		{
			CArray<CWndObjDescription*>ar;
			ParseDescription(ar, pContext, path, NULL, NULL, CWndObjDescription::Undefined);

			if (ar.GetSize())
			{
				pDescription = ar[0];
				ASSERT(ar.GetSize() == 1);
			}
		}
	}
	if (!pDescription)
	{
		CArray<CJsonResource> sources;
		sources.Add(sJsonResource);
		GetDeltaJsonFormInfos(sJsonResource.m_strName, sources);
		pDescription = ParseDescriptions(pContext, sources);
		if (bCacheOnFileSystem)
		{
			CLineFile file;
			if (file.Open(path, CFile::modeCreate | CFile::modeWrite | CFile::typeText))
			{
				CJsonSerializer ser;
				pDescription->ActivateDefines(false);
				pDescription->SerializeJson(ser);
				pDescription->ActivateDefines(true);
				file.WriteString(ser.GetJson());
			}
			else
			{
				ASSERT(FALSE);
				TRACE1("Cannot open file: %s\r\n", path);
			}
		}
	}
	pContext->m_pDescription = bCacheDescriptions
		? pDescription->DeepClone()
		: pDescription;
	if (bCacheDescriptions)
		m_Descriptions[pContext->m_JsonResource.m_strName] = pDescription;
	return pContext;
}
//-----------------------------------------------------------------------------
void CJsonFormEngine::MergeContext(const CJsonResource& sJsonResource, CJsonContextObj* pContext)
{
	CArray<CJsonResource> sources;
	sources.Add(sJsonResource);

	GetDeltaJsonFormInfos(sJsonResource.m_strName, sources);

	pContext->m_pDescription = ParseDescriptions(pContext, sources);
}
//-----------------------------------------------------------------------------
BOOL CJsonContext::CreateSplitter(CSplitterDescription* pSplitterDesc, CSplittedForm* pSplitterForm, CBaseDocument* pDoc)
{
	int nRows = pSplitterDesc->m_nRows;
	int nCols = pSplitterDesc->m_nCols;
	int nChildren = pSplitterDesc->m_Children.GetCount();

	if (nRows <= 0 || nCols <= 0)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	CTaskBuilderSplitterWnd* pSplitter = pSplitterForm->CreateSplitter(RUNTIME_CLASS(CTaskBuilderSplitterWnd), nRows, nCols);
	if (!pSplitter)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	//il numero dei figli deve essere uguale al numero delle "celle" definite nella creazione dello splitter
	if (nChildren != pSplitterDesc->m_nRows * pSplitterDesc->m_nCols)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	//le view figlie vengono aggiunte scorrendo riga per riga  
	for (int iRows = 0; iRows < nRows; iRows++)
	{
		for (int iCols = 0; iCols < nCols; iCols++)
		{
			CWndObjDescription* pView = pSplitterDesc->m_Children[iRows + iCols];
			DWORD nId = CJsonFormEngineObj::GetID(pView);
			CCreateContext context;
			CRuntimeClass* pClass = pDoc->GetControlClass(nId);
			if (!pClass)
				pClass = RUNTIME_CLASS(CJsonFormView);
			context.m_pNewViewClass = pClass;
			context.m_pCurrentDoc = pDoc;
			CSingleExtDocTemplate docTemplate(nId, NULL, NULL, NULL);
			CJsonContext* pViewContext = (CJsonContext*)CJsonContext::Create();
			pViewContext->Assign(this);
			pViewContext->m_pDescription = pView;
			pViewContext->m_bOwnDescription = false;
			docTemplate.m_pJsonContext = pViewContext;
			docTemplate.m_nViewID = nId;
			context.m_pNewDocTemplate = &docTemplate;
			context.m_pLastView = NULL;

			pSplitter->AddWindow(pClass, &context, iRows, iCols);
		}
	}

	pSplitter->SetSplitRatio(pSplitterDesc->m_fSplitRatio);

	if (pSplitterDesc->m_SplitterMode == SplitterMode::S_VERTICAL)
		pSplitter->SplitVertically();
	else
		pSplitter->SplitHorizontally();
	return TRUE;
}


//-----------------------------------------------------------------------------
CJsonContextObj* CJsonFormEngine::CreateContext(bool bIsJsonEditor)
{
	CJsonContext* pContext = CJsonContext::Create();
	pContext->m_bIsJsonDesigner = bIsJsonEditor;
	return pContext;
}
//-----------------------------------------------------------------------------
void CJsonFormEngine::GetDeltaJsonFormInfos(const CString& sJsonId, CArray<CJsonResource>& sources)
{
	CServerFormDescription* pDescri = AfxGetApplicationContext()->GetObject<CServerFormDescriArray>(&CApplicationContext::GetClientFormsTable)->Get(sJsonId);
	if (!pDescri)
		return;
	for (int i = 0; i < pDescri->m_arClientForms.GetCount(); i++)
	{
		CClientFormDescription *pClient = pDescri->m_arClientForms.GetAt(i);
		if (!AfxIsActivated(pClient->m_Module))
			continue;
		CJsonResource sJsonResource(pClient->m_sName);

		if (sJsonResource.GetFile().IsEmpty())
		{
			ASSERT(FALSE);
			continue;
		}
		sJsonResource.m_bExclude = pClient->m_bExclude;
		sJsonResource.m_bFromClientForm = true;
		bool existing = false;
		for (int j = 0; j < sources.GetCount(); j++)
		{
			CJsonResource res = sources.GetAt(j);
			if (res.m_strName == sJsonResource.m_strName)
			{
				existing = true;
				if (pClient->m_bExclude)
					res.m_bExclude = true;
				break;
			}
		}
		if (!existing)
			sources.Add(sJsonResource);
	}
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonTileDialog, CTileDialog)
//-----------------------------------------------------------------------------
CJsonTileDialog::CJsonTileDialog(UINT nIDD)
	: CTileDialog(_T(""), nIDD)
{
	InitializeFromContext();//solo in questo costruttore la chiamo, perché solo qui ho un costesto valido, perché ho l'IDD; quando uso l'altro costruttore, la devo chiamare nella AssignContext
}

//-----------------------------------------------------------------------------
void CJsonTileDialog::InitializeFromContext()
{
	CJsonContext* pContext = (CJsonContext*)GetJsonContext();
	CWndObjDescription* pDesc = (pContext ? pContext->m_pDescription : NULL);
	if (!pDesc || !pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
	{
		TRACE("JSON description not found or not valid, be sure it is a tile object\r\n");
		ASSERT(FALSE);
		return;
	}
	CWndTileDescription* pTileDesc = (CWndTileDescription*)pDesc;

	SetFormName(CJsonFormEngine::GetObjectName(pTileDesc));
	if (pTileDesc->m_bIsCollapsible != B_UNDEFINED)
		SetCollapsible(pTileDesc->m_bIsCollapsible == B_TRUE);
	if (pTileDesc->m_bIsPinnable != B_UNDEFINED)
		SetPinnable(pTileDesc->m_bIsPinnable == B_TRUE);
	SetCollapsed(pTileDesc->m_bIsCollapsed);
	//non posso chiamarla nel costruttore
	//SetPinned(pTileDesc->m_bIsPinned);
	m_bWrapTileParts = pTileDesc->m_bWrapTileParts;
	CString sTitle = AfxLoadJsonString(pTileDesc->m_strText, pTileDesc);
	SetTitle(sTitle);

	SetResetValuesAfterUnpin(pTileDesc->m_bResetValuesAfterUnpin);

	if (pTileDesc->m_Style != TDS_NONE)
		SetTileStyle(AfxGetTileDialogStyle(pTileDesc->m_Style));
	if (pTileDesc->m_bHasTitle != B_UNDEFINED)
		SetHasTitle(pTileDesc->m_bHasTitle == B_TRUE);
	if (pTileDesc->m_nFlex != -1)
		SetFlex(pTileDesc->m_nFlex);
	if (pTileDesc->m_nMinWidth != NULL_COORD)
		SetMinWidth(pTileDesc->m_nMinWidth);
	if (pTileDesc->m_nMinHeight != NULL_COORD)
		SetMinHeight(pTileDesc->m_nMinHeight);
	if (pTileDesc->m_nMaxWidth != NULL_COORD)
		SetMaxWidth(pTileDesc->m_nMaxWidth);
}
//-----------------------------------------------------------------------------
void CJsonTileDialog::AssignContext(CJsonContextObj* pContext)
{
#pragma warning( push )
#pragma warning( disable: 4996)
	m_pJsonContext = pContext;
	((CJsonContext*)m_pJsonContext)->m_pForm = this;
	((CJsonContext*)m_pJsonContext)->m_pLinks = m_pControlLinks;
#pragma warning( pop )
	/*
		if (pContext->m_pDescription->GetID() != pContext->m_JsonResource.m_strName)
		{
			AfxGetTBResourcesMap()->AddResource(
				TbResources,
				pContext->m_pDescription->GetID(),
				pContext->m_JsonResource.m_strContext,
				m_nIDHelp);
		}*/
	InitializeFromContext();
}

//-----------------------------------------------------------------------------
BOOL CJsonTileDialog::OnCmdMsg(UINT nID, int nCode, void* pExtra,
	AFX_CMDHANDLERINFO* pHandlerInfo)
{
	// first pump through pane
	if (__super::OnCmdMsg(nID, nCode, pExtra, pHandlerInfo))
		return TRUE;

	// then pump through document
	if (m_pOwner && m_pOwner->IsKindOf(RUNTIME_CLASS(CCmdTarget)))
	{
		return ((CCmdTarget*)m_pOwner)->OnCmdMsg(nID, nCode, pExtra, pHandlerInfo);
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CJsonTileDialog::UseSplitters()
{
	CJsonContext* pContext = (CJsonContext*)GetJsonContext();
	CWndObjDescription* pDesc = (pContext ? pContext->m_pDescription : NULL);
	if (!pDesc || !pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
	{
		TRACE("JSON description not found or not valid, be sure it is a tile object\r\n");
		ASSERT(FALSE);
		return FALSE;
	}

	//Se la tile ha uno splitter, e' l'unico figlio
	if (pDesc->m_Children.GetCount() > 0 && pDesc->m_Children[0]->m_Type == CWndObjDescription::Splitter)
	{
		return TRUE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
void CJsonTileDialog::OnCreateSplitters(CCreateContext* pContext)
{
	CJsonContext* pJsonContext = (CJsonContext*)GetJsonContext();
	CWndObjDescription* pDesc = (pContext ? pJsonContext->m_pDescription : NULL);
	if (!pDesc || !pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
	{
		TRACE("JSON description not found or not valid, be sure it is a tile object\r\n");
		ASSERT(FALSE);
	}

	//Se la tile ha uno splitter, e' l'unico figlio
	if (pDesc->m_Children.GetCount() > 0 && pDesc->m_Children[0]->m_Type == CWndObjDescription::Splitter)
	{
		CSplitterDescription* pSplitterDesc = (CSplitterDescription*)(pDesc->m_Children[0]);
		pJsonContext->CreateSplitter(pSplitterDesc, this, (CBaseDocument*)pContext->m_pCurrentDoc);
	}
}

//-----------------------------------------------------------------------------
BOOL CJsonTileDialog::OnInitDialog()
{
	CWndObjDescription* pDesc = (GetJsonContext() ? GetJsonContext()->m_pDescription : NULL);
	if (!pDesc || !pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
	{
		TRACE("JSON description not found or not valid, be sure it is a tile object\r\n");
	}
	else if (((CWndTileDescription*)pDesc)->m_bHasStaticArea)
	{
		if (((CWndTileDescription*)pDesc)->m_Size != TILE_MINI)
		{
			CGroupBoxDescription desc1(NULL);
			desc1.SetID(_T("IDC_STATIC_AREA"));
			desc1.m_X = 0;
			desc1.m_Y = 0;
			desc1.m_Width = STATIC_AREA_WIDTH;
			desc1.m_Height = pDesc->m_Height;
			CJsonFormEngineObj::GetInstance()->ProcessWndDescription(&desc1, this, GetJsonContext());

			if (((CWndTileDescription*)pDesc)->m_Size == TILE_WIDE)
			{
				CGroupBoxDescription desc2(NULL);
				desc2.SetID(_T("IDC_STATIC_AREA_2"));
				desc2.m_X = LEFT_SECOND_STATIC_AREA;
				desc2.m_Y = 0;
				desc2.m_Width = STATIC_AREA_WIDTH;
				desc2.m_Height = pDesc->m_Height;
				CJsonFormEngineObj::GetInstance()->ProcessWndDescription(&desc2, this, GetJsonContext());
			}
		}

	}
	return __super::OnInitDialog();
}

//-----------------------------------------------------------------------------
bool CJsonTileDialog::IsInitiallyPinned()
{
	CWndObjDescription* pDesc = (GetJsonContext() ? GetJsonContext()->m_pDescription : NULL);
	if (!pDesc || !pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
	{
		TRACE("JSON description not found or not valid, be sure it is a tile object\r\n");
		return true;
	}
	return ((CWndTileDescription*)pDesc)->m_bIsPinned;
}

//-----------------------------------------------------------------------------
TileDialogSize CJsonTileDialog::GetSize()
{
	CWndObjDescription* pDesc = (GetJsonContext() ? GetJsonContext()->m_pDescription : NULL);
	if (!pDesc || !pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
	{
		TRACE("JSON description not found or not valid, be sure it is a tile object\r\n");
		return TileDialogSize::TILE_STANDARD;
	}
	return ((CWndTileDescription*)pDesc)->m_Size;
}

//-----------------------------------------------------------------------------
int CJsonTileDialog::GetFlex()
{
	CWndObjDescription* pDesc = (GetJsonContext() ? GetJsonContext()->m_pDescription : NULL);
	if (pDesc && pDesc->IsKindOf(RUNTIME_CLASS(CWndTileDescription)))
	{
		return ((CWndTileDescription*)pDesc)->m_nFlex;
	}
	return -1;
}

//---------------------------------------------------------------------------------
template <class T> TBJsonTileGroupWrapper<T>::~TBJsonTileGroupWrapper()
{
	if (m_bOwnContext)
		delete m_pContext;
}
//-----------------------------------------------------------------------------
HotFilterObj* CJsonContext::CreateHotFilter(UINT nHFID, CWndObjDescription* pDesc)
{
	CHotFilterDescription* pHFDescri = (CHotFilterDescription*)pDesc;
	CRuntimeClass* pClass = m_pDoc->GetControlClass(nHFID);
	if (!pClass)
	{
		CJsonResource res = pDesc->GetResource();
		if (res.m_strName == _T("IDD_TD_HOTFILTER_LIST_CHECKBOX"))
			pClass = RUNTIME_CLASS(HotFilterList);
		else if (res.m_strName == _T("IDD_TD_HOTFILTER_RANGE"))
			pClass = RUNTIME_CLASS(HotFilterRange);
		else if (res.m_strName == _T("IDD_TD_HOTFILTER_RANGE_WITH_SELECTION"))
			pClass = RUNTIME_CLASS(HotFilterRange);
		else if (res.m_strName == _T("IDD_TD_HOTFILTER_RANGE_DATE"))
			pClass = RUNTIME_CLASS(HotFilterDateRange);
		else if (res.m_strName == _T("IDD_TD_HOTFILTER_RANGE_DATE_WITH_SELECTION"))
			pClass = RUNTIME_CLASS(HotFilterDateRange);
		else if (res.m_strName == _T("IDD_TD_HOTFILTER_RANGE_INT"))
			pClass = RUNTIME_CLASS(HotFilterIntRange);
		else if (res.m_strName == _T("IDD_TD_HOTFILTER_RANGE_INT_WITH_SELECTION"))
			pClass = RUNTIME_CLASS(HotFilterIntRange);
		else if (res.m_dwId)
		{
			pClass = m_pDoc->GetControlClass(res.m_dwId);
		}
		else
		{
			DWORD nID = AfxGetTBResourcesMap()->GetTbResourceID(res.m_strName, TbResourceType::TbResources);
			pClass = m_pDoc->GetControlClass(nID);
		}
	}
	if (!pClass)
	{
		ASSERT(FALSE);
		TRACE1("Cannot create hot filter %s, no class available\r\n", (LPCTSTR)pDesc->GetID());
		return NULL;
	}

	HotFilterObj* pHF = m_pDoc->GetHotFilterManager()->Add(pHFDescri->m_strName, pClass, nHFID);
	if (pHF)
	{
		pHF->SetManageUI(false);//le tile non le deve creare l'hotfilter manager, perché sono create dal motore json

		if (pHFDescri->m_pBindings && pHFDescri->m_pBindings->m_pHotLink)
			pHF->AttachNsHotlink(pHFDescri->m_pBindings->m_pHotLink);

		m_pDoc->OnAfterHotLinkCreated(pHF);
	}
	return pHF;
}
//-----------------------------------------------------------------------------
template <class T > void TBJsonTileGroupWrapper<T>::Customize(CWndObjDescriptionContainer& children, CLayoutContainer *pContainer, CTilePanel* pContainerPanel)
{
	//ricorsione sui figli
	for (int i = 0; i < children.GetCount(); i++)
	{
		CWndObjDescription* pChild = children[i];

		if (pChild->m_Type == CWndObjDescription::Tile
			|| pChild->m_Type == CWndObjDescription::HotFilter)
		{
			UINT nTileID = CJsonFormEngineObj::GetID(pChild);
			if (!m_pContext->CanCreateControl(pChild, nTileID))
				continue;
			CObject* pTileOwner = NULL;
			if (pChild->m_Type == CWndObjDescription::HotFilter)
				pTileOwner = m_pContext->CreateHotFilter(nTileID, pChild);
			CBaseDocument* pDoc = GetDocument();
			CRuntimeClass* pClass = pChild->m_Type == CWndObjDescription::HotFilter
				? NULL
				: pDoc ? pDoc->GetControlClass(nTileID) : NULL;
			if (!pClass)
				pClass = RUNTIME_CLASS(CJsonTileDialog);
			ASSERT(pClass->IsDerivedFrom(RUNTIME_CLASS(CJsonTileDialog)));

			CJsonTileDialog* pTile = (CJsonTileDialog*)pClass->CreateObject();
			CJsonContext* pNewContext = pChild->m_Type == CWndObjDescription::HotFilter
				? new CHotFilterJsonContext(pChild->m_strControlClass)//per utilizzare la control class comune a tutti i controlli
				: (CJsonContext*)CJsonContext::Create();
			pNewContext->Assign(m_pContext);
			pNewContext->m_pDescription = pChild;
			pNewContext->m_bOwnDescription = false;

			if (((CWndTileDescription*)pChild)->m_Size == TileDialogSize::TILE_MINI || ((CWndTileDescription*)pChild)->m_Size == TileDialogSize::TILE_MICRO)
				((CWndTileDescription*)pChild)->m_nMinWidth = -2 /*ORIGINAL*/;

			pTile->AssignContext(pNewContext);
			BOOL bInitiallyPinned = pTile->IsInitiallyPinned();
			CString sTitle = AfxLoadJsonString(pChild->m_strText, pChild);

			if (pContainerPanel)
				pContainerPanel->AddTile
				(
					pContainerPanel->GetActiveTab(),
					pContainer,
					pTile,
					nTileID,
					sTitle,
					((CWndTileDescription*)pChild)->m_Size,
					((CWndTileDescription*)pChild)->m_nFlex,
					!bInitiallyPinned,
					pTileOwner
				);
			else
			{
				CBaseTileDialog* pResTile = AddTile
				(
					pContainer,
					pTile,
					nTileID,
					sTitle,
					((CWndTileDescription*)pChild)->m_Size,
					AUTO,
					pTileOwner
				);

				if (pResTile)
					pResTile->ForceSetPinned(bInitiallyPinned);
			}
			if (((CWndTileDescription*)pChild)->m_bIsCollapsed)
				pTile->SetCollapsed(((CWndTileDescription*)pChild)->m_bIsCollapsed);
			if (((CWndTileDescription*)pChild)->m_bIsCollapsible != B_UNDEFINED)
				pTile->SetCollapsible(((CWndTileDescription*)pChild)->m_bIsCollapsible == B_TRUE);

		}
		else if (pChild->m_Type == CWndObjDescription::TilePanel)
		{
			UINT nTileID = CJsonFormEngineObj::GetID(pChild);
			if (!m_pContext->CanCreateControl(pChild, nTileID))
				continue;
			CWndLayoutContainerDescription* pCntDesc = (CWndLayoutContainerDescription*)pChild;
			CString sTitle = AfxLoadJsonString(pChild->m_strText, pChild);
			CTilePanel* pPanel = AddPanel
			(
				pContainer,
				pChild->m_strName,
				sTitle,
				pCntDesc->m_LayoutType,
				pCntDesc->m_LayoutAlign,
				pCntDesc->m_nFlex,
				nTileID
			);
			if (pCntDesc->m_bIsCollapsible != B_UNDEFINED)
				pPanel->SetCollapsible(pCntDesc->m_bIsCollapsible == B_TRUE);

			pPanel->SetCollapsed(pCntDesc->m_bIsCollapsed);

			if (pCntDesc->m_bShowAsTile)
				pPanel->ShowAsTile();
			if (pCntDesc->m_Style != TDS_NONE)
				pPanel->SetTileStyle(AfxGetTileDialogStyle(pCntDesc->m_Style));

			//pPanel->SetTileDialogStyle(AfxGetTileDialogStyle(?));

			pPanel->EnsureTabExistance();
			Customize(pChild->m_Children, pPanel->GetActiveTab()->GetLayoutContainer(), pPanel);
		}
		else if (pChild->m_Type == CWndObjDescription::StatusTilePanel)
		{
			UINT nTileID = CJsonFormEngineObj::GetID(pChild);
			if (!m_pContext->CanCreateControl(pChild, nTileID))
				continue;
			CWndLayoutStatusContainerDescription* pCntDesc = (CWndLayoutStatusContainerDescription*)pChild;
			CString sTitle = AfxLoadJsonString(pChild->m_strText, pChild);
			CTilePanel* pPanel = AddStatusPanel();

			pPanel->SetLayoutType(pCntDesc->m_LayoutType);
			pPanel->SetLayoutAlign(pCntDesc->m_LayoutAlign);

			if (pCntDesc->m_bIsCollapsible != B_UNDEFINED)
				pPanel->SetCollapsible(pCntDesc->m_bIsCollapsible == B_TRUE);

			pPanel->SetCollapsed(pCntDesc->m_bIsCollapsed);

			if (pCntDesc->m_bShowAsTile)
				pPanel->ShowAsTile();

			pPanel->EnsureTabExistance();
			Customize(pChild->m_Children, pPanel->GetActiveTab()->GetLayoutContainer(), pPanel);
		}
		else if (pChild->m_Type == CWndObjDescription::StatusTile)
		{
			UINT nTileID = CJsonFormEngineObj::GetID(pChild);
			if (!m_pContext->CanCreateControl(pChild, nTileID))
				continue;
			CRuntimeClass* pClass = GetDocument()->GetControlClass(nTileID);
			if (!pClass)
				pClass = RUNTIME_CLASS(CJsonTileDialog);
			ASSERT(pClass->IsDerivedFrom(RUNTIME_CLASS(CJsonTileDialog)));

			CLinearStatusTile* pStatusTile = new CLinearStatusTile();
			CJsonContext* pNewContext = (CJsonContext*)CJsonContext::Create();
			pNewContext->Assign(m_pContext);
			pNewContext->m_pDescription = pChild;
			pNewContext->m_bOwnDescription = false;
			pStatusTile->AssignContext(pNewContext);

			if (pContainerPanel)
			{
				pContainerPanel->AddStatusTile(pStatusTile, nTileID);
			}
			else
			{
				ASSERT(FALSE); //status tile must be in a TilePanel
			}
		}
		else if (pChild->m_Type == CWndObjDescription::LayoutContainer)
		{
			if (!m_pContext->CanCreateControl(pChild, CJsonFormEngineObj::GetID(pChild)))
				continue;

			ASSERT_KINDOF(CWndLayoutContainerDescription, pChild);

			CWndLayoutContainerDescription* pCntDesc = (CWndLayoutContainerDescription*)pChild;
			CLayoutContainer* pCnt =

				pContainerPanel
				? pContainerPanel->
				AddContainer(
					pContainer,
					pCntDesc->m_LayoutType,
					pCntDesc->m_nFlex,
					pCntDesc->m_LayoutAlign)
				:
				AddContainer(
					pContainer,
					pCntDesc->m_LayoutType,
					pCntDesc->m_nFlex,
					pCntDesc->m_LayoutAlign);
			Customize(pChild->m_Children, pCnt, pContainerPanel);
			if (pCntDesc->m_bIsCollapsible != B_UNDEFINED)
				pCnt->SetGroupCollapsible(pCntDesc->m_bIsCollapsible == B_TRUE);
		}
		else
		{
			ASSERT(FALSE);
			continue;
		}

	}
}
//-----------------------------------------------------------------------------
template <class T> void TBJsonTileGroupWrapper<T>::Customize()
{
	//posso avere un item, e allora il contesto me lo da lui, oppure mi viene assegnato da fuori
	CJsonContext* pContext = m_pDlgInfoItem ? (CJsonContext*)m_pDlgInfoItem->GetJsonContext() : NULL;
	if (pContext)
	{
		SetDescription(pContext, pContext->m_pDescription);
		m_bOwnContext = false; //il contesto viene distrutto dalla m_pDlgInfoItem
	}
	else
	{
		m_bOwnContext = true;
	}
	if (!m_pContext)
	{
		ASSERT(FALSE);
		return;
	}
	CWndLayoutContainerDescription* pCntDesc = (CWndLayoutContainerDescription*)m_pWndDesc;
	SetLayoutType(pCntDesc->m_LayoutType);
	SetLayoutAlign(pCntDesc->m_LayoutAlign);
	SetFlex(pCntDesc->m_nFlex);
	if (pCntDesc->m_Style != TDS_NONE)
		SetTileDialogStyle(AfxGetTileDialogStyle(pCntDesc->m_Style));
	if (pCntDesc->m_bIsCollapsible != B_UNDEFINED)
		SetGroupCollapsible(pCntDesc->m_bIsCollapsible == B_TRUE);
	Customize(m_pWndDesc->m_Children, GetLayoutContainer(), NULL);
}

//-----------------------------------------------------------------------------
template <class T> BOOL TBJsonTileGroupWrapper<T>::OnPrepareAuxData()
{
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocument();
	if (pDoc)
		pDoc->OnPrepareAuxData(this);
	return TRUE;
}

//--------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonTileGroup, CTileGroup)

//--------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonPinnedTilesTileGroup, CPinnedTilesTileGroup)

//--------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonHeaderStrip, CHeaderStrip)

//-----------------------------------------------------------------------------
CJsonHeaderStrip::~CJsonHeaderStrip()
{
	SAFE_DELETE(m_pContext);
}
//-----------------------------------------------------------------------------
void CJsonHeaderStrip::Customize()
{
	if (!m_pContext)
	{
		ASSERT(FALSE);
		return;
	}

	__super::Customize();

	Customize(m_pWndDesc->m_Children, GetLayoutContainer(), NULL);
	SetCaption(_T(""));
}

//-----------------------------------------------------------------------------
void CJsonHeaderStrip::Customize(CWndObjDescriptionContainer& children, CLayoutContainer *pContainer, CTilePanel* pContainerPanel)
{
	//ricorsione sui figli
	for (int i = 0; i < children.GetCount(); i++)
	{
		CWndObjDescription* pChild = children[i];

		if (pChild->m_Type == CWndObjDescription::StatusTilePanel)
		{
			UINT nTileID = CJsonFormEngineObj::GetID(pChild);
			if (!m_pContext->CanCreateControl(pChild, nTileID))
				continue;
			CWndLayoutStatusContainerDescription* pCntDesc = (CWndLayoutStatusContainerDescription*)pChild;
			CString sTitle = AfxLoadJsonString(pChild->m_strText, pChild);
			CTilePanel* pPanel = AddStatusPanel();

			pPanel->SetLayoutType(pCntDesc->m_LayoutType);
			pPanel->SetLayoutAlign(pCntDesc->m_LayoutAlign);

			if (pCntDesc->m_bIsCollapsible != B_UNDEFINED)
				pPanel->SetCollapsible(pCntDesc->m_bIsCollapsible == B_TRUE);

			pPanel->SetCollapsed(pCntDesc->m_bIsCollapsed);

			if (pCntDesc->m_bShowAsTile)
				pPanel->ShowAsTile();
			pPanel->EnsureTabExistance();
			Customize(pChild->m_Children, pPanel->GetActiveTab()->GetLayoutContainer(), pPanel);
		}
		else if (pChild->m_Type == CWndObjDescription::StatusTile)
		{
			UINT nTileID = CJsonFormEngineObj::GetID(pChild);
			if (!m_pContext->CanCreateControl(pChild, nTileID))
				continue;
			CRuntimeClass* pClass = GetDocument()->GetControlClass(nTileID);
			if (!pClass)
				pClass = RUNTIME_CLASS(CJsonTileDialog);
			ASSERT(pClass->IsDerivedFrom(RUNTIME_CLASS(CJsonTileDialog)));

			CLinearStatusTile* pStatusTile = new CLinearStatusTile();
			CJsonContext* pNewContext = (CJsonContext*)CJsonContext::Create();
			pNewContext->Assign(m_pContext);
			pNewContext->m_pDescription = pChild;
			pNewContext->m_bOwnDescription = false;
			pStatusTile->AssignContext(pNewContext);

			if (pContainerPanel)
			{
				pContainerPanel->AddStatusTile(pStatusTile, nTileID);
			}
			else
			{
				ASSERT(FALSE); //status tile must be in a TilePanel
			}
		}
		else
		{
			ASSERT(FALSE);
			continue;
		}
	}
}

//-----------------------------------------------------------------------------
template <class T> BOOL TJsonTabManager<T>::CreateEx(DWORD dwExStyle, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	if (__super::CreateEx(dwExStyle, dwStyle, rect, pParentWnd, nID))
	{
		CRect r = m_pWndDesc->GetRect();
		::MapDialogRect(pParentWnd->m_hWnd, r);
		MoveWindow(r);
		return TRUE;
	}
	ASSERT(FALSE);
	return FALSE;
}


//-----------------------------------------------------------------------------
template <class T> void TJsonTabManager<T>::Customize()
{
	//ricorsione sui figli
	for (int i = 0; i < m_pWndDesc->m_Children.GetCount(); i++)
	{
		CWndObjDescription* pChild = m_pWndDesc->m_Children[i];
		UINT nIDD = CJsonFormEngineObj::GetID(pChild);
		if (!m_pContext->CanCreateControl(pChild, nIDD))
			continue;

		if (pChild->m_Type != CWndObjDescription::Tab && pChild->m_Type != CWndObjDescription::Panel && pChild->m_Type != CWndObjDescription::View)
		{
			ASSERT(FALSE);
			continue;
		}

		DlgInfoItem* pItem = AddDialog(nIDD);
		pItem->GetInfoOSL()->m_Namespace.AutoCompleteNamespace(CTBNamespace::TABDLG, CJsonFormEngine::GetObjectName(pChild), GetNamespace().ToString());
		delete pItem->m_pJsonContext;
		pItem->m_pJsonContext = CJsonContext::Create();
		pItem->m_pJsonContext->Assign(m_pContext);
		pItem->m_pJsonContext->m_pDescription = pChild;
		pItem->m_pJsonContext->m_bOwnDescription = false;
		pItem->m_strTitle = AfxLoadJsonString(pChild->m_strText, pChild);
	}
}
//-----------------------------------------------------------------------------
template <class T> BOOL TJsonTabDialog<T>::Create(CBaseTabManager* pParentWnd)
{
	if (!__super::Create(pParentWnd))
		return FALSE;
	CWndObjDescription* pWndDesc = m_pDlgInfo->GetJsonContext()->m_pDescription;
	pWndDesc->AttachTo(m_hWnd);
	m_nID = CJsonFormEngineObj::GetID(pWndDesc);
	SetDlgCtrlID(m_nID);

	return TRUE;
}

IMPLEMENT_DYNCREATE(CJsonTabManager, CTabManager);
//-----------------------------------------------------------------------------
DlgInfoItem* CJsonTabManager::AddDialog(UINT nIDD)
{
	return CTabManager::AddDialog(RUNTIME_CLASS(CJsonTabDialog), nIDD);
}

IMPLEMENT_DYNCREATE(CJsonTabDialog, CTabDialog);
IMPLEMENT_DYNCREATE(CJsonWizardTabManager, CTabWizard);
//-----------------------------------------------------------------------------
DlgInfoItem* CJsonWizardTabManager::AddDialog(UINT nIDD)
{
	return CTabWizard::AddDialog(RUNTIME_CLASS(CJsonWizardTabDialog), nIDD);
}
IMPLEMENT_DYNCREATE(CJsonWizardTabDialog, CWizardTabDialog);

//-----------------------------------------------------------------------------
BOOL CJsonTileManager::CreateEx(DWORD dwExStyle, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	if (__super::CreateEx(dwExStyle, dwStyle, rect, pParentWnd, nID))
	{
		CRect r = m_pWndDesc->GetRect();
		::MapDialogRect(pParentWnd->m_hWnd, r);
		MoveWindow(r);
		return TRUE;
	}
	ASSERT(FALSE);
	return FALSE;
}


//-----------------------------------------------------------------------------
void CJsonTileManager::Customize()
{
	//ricorsione sui figli
	for (int i = 0; i < m_pWndDesc->m_Children.GetCount(); i++)
	{
		CWndObjDescription* pChild = m_pWndDesc->m_Children[i];
		UINT nGroupID = CJsonFormEngineObj::GetID(pChild);
		if (!m_pContext->CanCreateControl(pChild, nGroupID))
			continue;

		if (pChild->m_Type != CWndObjDescription::TileGroup)
		{
			ASSERT(FALSE);
			continue;
		}
		ASSERT_KINDOF(CWndLayoutContainerDescription, pChild);
		CRuntimeClass* pClass = GetDocument()->GetControlClass(nGroupID);
		if (!pClass)
			pClass = RUNTIME_CLASS(CJsonTileGroup);
		ASSERT(pClass->IsDerivedFrom(RUNTIME_CLASS(CJsonTileGroup)));
		CString sTitle = AfxLoadJsonString(pChild->m_strText, pChild);
		CString sToolTip = AfxLoadJsonString(pChild->m_strHint, pChild);
		CString sIcon = ((CWndLayoutContainerDescription*)pChild)->m_strIcon;

		TileGroupInfoItem* pItem = AddTileGroup(pClass, CJsonFormEngine::GetObjectName(pChild), sTitle, sIcon, sToolTip, nGroupID);
		delete pItem->m_pJsonContext;
		pItem->m_pJsonContext = CJsonContext::Create();
		pItem->m_pJsonContext->Assign(m_pContext);
		pItem->m_pJsonContext->m_pDescription = pChild;
		pItem->m_pJsonContext->m_bOwnDescription = false;
		//se la descrizione non è condivisa, dopo aver creato la finestra la associo alla sua descrizione
		//così posso aggiornare quest'ultima, ad esempio in fase di editing
		//if (m_pContext->!m_bSingletonDescription)
		//	pChild->AttachTo(pItem->m_pBaseTabDlg->GetChildTileGroup()->m_hWnd); verrà fatto nella init dialog quando la dialog viene creata
	}
}
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonTileManager, CTileManager)

//-----------------------------------------------------------------------------
template <class T> void TBJsonBodyEditWrapper<T>::SetAllBodyEditStyles()
{
	CWndBodyDescription* pBodyDesc = (CWndBodyDescription*)m_pWndDesc;
	SET_BE_PROP(pBodyDesc->m_bAllowCallDialog, BE_STYLE_ALLOW_CALLDIALOG);
	SET_BE_PROP(pBodyDesc->m_bAllowColumnLock, BE_STYLE_ALLOW_COLUMN_LOCK);
	SET_BE_PROP(pBodyDesc->m_bAllowColumnLockInteractive, BE_STYLE_ALLOW_COLUMN_LOCK_INTERACTIVE);
	SET_BE_PROP(pBodyDesc->m_bAllowCopy, BE_STYLE_ALLOW_COPY);
	SET_BE_PROP(pBodyDesc->m_bAllowCustomize, BE_STYLE_ALLOW_CUSTOMIZE);
	//sostituiti con chiamate a EnableDeleteRow, EnableInsertRow, EnableAddRow
	SET_BE_PROP(pBodyDesc->m_bAllowInsert, BE_STYLE_ALLOW_INSERT);
	SET_BE_PROP(pBodyDesc->m_bAllowDelete, BE_STYLE_ALLOW_DELETE);
	SET_BE_PROP(pBodyDesc->m_bAllowDrag, BE_STYLE_ALLOW_DRAG);
	SET_BE_PROP(pBodyDesc->m_bAllowDrop, BE_STYLE_ALLOW_DROP);
	SET_BE_PROP(pBodyDesc->m_bAllowPaste, BE_STYLE_ALLOW_PASTE);
	SET_BE_PROP(pBodyDesc->m_bAllowDragReadOnlyDoc, BE_STYLE_ALLOW_DRAG_READONLY_DOC);
	SET_BE_PROP(pBodyDesc->m_bAllowMultipleSel, BE_STYLE_ALLOW_MULTIPLE_SEL);
	SET_BE_PROP(pBodyDesc->m_bAllowOrdering, BE_STYLE_ALLOW_SORT);
	SET_BE_PROP(pBodyDesc->m_bAllowOrderingOnBrowse, BE_STYLE_APPLY_SORT_ON_BROWSE);
	SET_BE_PROP(pBodyDesc->m_bAllowOrderingOnEdit, BE_STYLE_ALLOW_SORT_ON_EDIT);
	SET_BE_PROP(pBodyDesc->m_bAllowRemoveColumnInteractive, BE_STYLE_ALLOW_REMOVE_COLUMN_INTERACTIVE);
	SET_BE_PROP(pBodyDesc->m_bAllowSearch, BE_STYLE_ALLOW_SEARCH);
	SET_BE_PROP(pBodyDesc->m_bChangeColor, BE_STYLE_CHANGE_COLOR);
	SET_BE_PROP(pBodyDesc->m_bEnlargeAllStringColumns, BE_STYLE_ENLARGE_ALLSTRINGCOLUMNS);
	SET_BE_PROP(pBodyDesc->m_bEnlargeCustom, BE_STYLE_ENLARGE_CUSTOM);
	SET_BE_PROP(pBodyDesc->m_bEnlargeLastColumn, BE_STYLE_ENLARGE_LASTCOLUMN);
	SET_BE_PROP(pBodyDesc->m_bEnlargeLastStringColumn, BE_STYLE_ENLARGE_LASTSTRINGCOLUMN);
	SET_BE_PROP(pBodyDesc->m_bShowBorders, BE_STYLE_SHOW_BORDERS);
	SET_BE_PROP(pBodyDesc->m_bShowColumnHeaders, BE_STYLE_SHOW_COLUMN_HEADERS);
	SET_BE_PROP(pBodyDesc->m_bShowFooterToolbar, BE_STYLE_SHOW_FOOTER_TOOLBAR);
	SET_BE_PROP(pBodyDesc->m_bShowHeaderToolbar, BE_STYLE_SHOW_HEADER_TOOLBAR);
	SET_BE_PROP(pBodyDesc->m_bShowHorizLines, BE_STYLE_SHOW_HORIZ_LINES);
	SET_BE_PROP(pBodyDesc->m_bShowHorizScrollbar, BE_STYLE_SHOW_HORIZ_SCROLLBAR);
	SET_BE_PROP(pBodyDesc->m_bShowVertLines, BE_STYLE_SHOW_VERT_LINES);
	SET_BE_PROP(pBodyDesc->m_bShowVertScrollbar, BE_STYLE_SHOW_VERT_SCROLLBAR);
	SET_BE_PROP(pBodyDesc->m_bShowDataTip, BE_STYLE_SHOW_DATATIP);
	SET_BE_PROP(pBodyDesc->m_bShowStatusBar, BE_STYLE_SHOW_STATUSBAR);
	SET_BE_PROP(pBodyDesc->m_bShowAlternateColor, BE_STYLE_SHOW_ALTERNATE_COLOR);
}

//-----------------------------------------------------------------------------
template <class T> CRuntimeClass* TBJsonBodyEditWrapper<T>::GetRowViewClass()
{
	UINT idc = GetRowFormViewId();
	//prima vedo se è registrata nel documento
	CRuntimeClass* pRowViewClass = GetDocument() ? GetDocument()->GetControlClass(idc) : NULL;
	if (pRowViewClass && !pRowViewClass->IsDerivedFrom(RUNTIME_CLASS(CJsonRowView)))
	{
		ASSERT(FALSE);
		pRowViewClass = NULL;
	}
	if (pRowViewClass)
		return pRowViewClass;
	//poi la chiedo al bodyedit
	return idc == 0 ? NULL : RUNTIME_CLASS(CJsonRowView);
}

//-----------------------------------------------------------------------------
template <class T> UINT TBJsonBodyEditWrapper<T>::GetRowFormViewId()
{
	return m_pContext->GetRowFormViewId((CWndBodyDescription*)m_pWndDesc);
}
//-----------------------------------------------------------------------------
template <class T> void TBJsonBodyEditWrapper<T>::FindHotLink()
{
	if (m_pDBT)
		m_pDBT->PrepareDynamicColumns(TRUE);
}

//-----------------------------------------------------------------------------
template <class T> void TBJsonBodyEditWrapper<T>::OnBeforeCustomize()
{
	//customizzazione toolbar del bodyEdit
	for (int i = 0; i < m_pWndDesc->m_Children.GetCount(); i++)
	{
		CWndObjDescription* pChild = m_pWndDesc->m_Children[i];

		if (pChild->m_Type == CWndObjDescription::Toolbar)
		{
			for (int j = 0; j < pChild->m_Children.GetCount(); j++)
			{
				CWndObjDescription* pDesc1 = pChild->m_Children.GetAt(j);
				switch (pDesc1->m_Type)
				{
				case CWndObjDescription::ToolbarButton:
				{
					if (!pDesc1->IsKindOf(RUNTIME_CLASS(CToolbarBtnDescription)))
					{
						ASSERT(FALSE);
						continue;
					}
					UINT nId = CJsonFormEngineObj::GetID(pDesc1);
					if (!((CJsonContext*)m_pContext)->CanCreateControl(pDesc1, nId))
						continue;

					CToolbarBtnDescription* pToolBarBtnDesc = (CToolbarBtnDescription*)pDesc1;
					CString sText = AfxLoadJsonString(pToolBarBtnDesc->m_strText, pToolBarBtnDesc);
					CString sHint = AfxLoadJsonString(pToolBarBtnDesc->m_strHint, pToolBarBtnDesc);
					CString strImageNS = pToolBarBtnDesc->m_sIcon;
					if (strImageNS.Left(6) != _T("Image."))
						strImageNS = _T("Image.") + strImageNS;

					CBEButton* pButton = ((CToolbarDescription*)pChild)->m_bBottom
						? AddFooterButton(pToolBarBtnDesc->m_strName, nId, strImageNS, sHint)
						: AddAuxToolBarButton(pToolBarBtnDesc->m_strName, nId, strImageNS, sHint, sText);

					if (!pButton)
						continue;

					pButton->m_bDefaultClick = pToolBarBtnDesc->m_bDefault;

					if (pToolBarBtnDesc->m_bIsDropdown)
					{
						for (int k = 0; k < pToolBarBtnDesc->m_Children.GetCount(); k++)
						{
							CWndObjDescription* pDescMenuItem = pToolBarBtnDesc->m_Children.GetAt(k);
							if (!pDescMenuItem->IsKindOf(RUNTIME_CLASS(CMenuItemDescription)))
							{
								ASSERT(FALSE);
								continue;
							}
							UINT nMenuItemId = CJsonFormEngineObj::GetID(pDescMenuItem);
							if (!((CJsonContext*)m_pContext)->CanCreateControl(pDescMenuItem, nMenuItemId))
							{
								continue;
							}
							if (((CMenuItemDescription*)pDescMenuItem)->m_bIsSeparator)
								pButton->AddMenuSeparator();
							else
								pButton->AddMenuItem(pDescMenuItem->m_strName, AfxLoadJsonString(pDescMenuItem->m_strText, pToolBarBtnDesc), nMenuItemId, 1, 0, ((CMenuItemDescription*)pDescMenuItem)->m_strIcon);
						}
					}
					break;
				}
				case CWndObjDescription::HRef:
				{ //se ricevo un href dentro una tile, non dveo fare nulla
					break;
				}
				default:
				{
					DataObj* pDataObj = NULL;
					HotKeyLink* pHotLink = NULL;
					DWORD nButtonId = BTN_DEFAULT;
					if (pDesc1->m_pBindings)
					{
						SqlRecord* pRec = NULL;
						DBTObject* pDBT = NULL;
						CString sBindingName;
						m_pContext->GetBindingInfo(pDesc1->GetID(), pDesc1->m_strName, pDesc1->m_pBindings, pDBT, pRec, pDataObj, sBindingName);
						if (!pDataObj && !GetDocument()->IsInStaticDesignMode())
						{
							TRACE2("Invalid data source: %s; id: %s\r\n", (LPCTSTR)pDesc1->m_pBindings->m_strDataSource, (LPCTSTR)pDesc1->GetID());
							ASSERT(FALSE);
							continue;
						}
						HotLinkInfo* pInfo = pDesc1->m_pBindings->m_pHotLink;

						CString sHotLink;
						if (pInfo)
							sHotLink = pInfo->m_strName;
						if (pInfo && !sHotLink.IsEmpty() && !GetDocument()->IsInStaticDesignMode())
						{
							pHotLink = m_pContext->m_pDoc->GetHotLink(sHotLink, CTBNamespace(CTBNamespace::HOTLINK, pInfo->m_strNamespace));
#ifdef DEBUG
							if (!m_pContext->m_pDoc->IsInStaticDesignMode())
								ASSERT(pHotLink);
#endif
							if (pHotLink)
								pHotLink->Parameterize(pInfo, pDesc1->m_pBindings->m_nButtonId);
						}

						nButtonId = pDesc1->m_pBindings->m_nButtonId;

					}
					DWORD dwNeededStyle = (WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS), dwNeededExStyle = 0;
					CRuntimeClass* pClass = m_pContext->GetControlClass(pDesc1->m_strControlClass, pDesc1->m_Type, pDataObj, dwNeededStyle, dwNeededExStyle);
					if (!pClass) {
						ASSERT(FALSE);
						continue;
					}
					CBEBtnCtrl* pCtrl = m_HeaderToolBar.AddControl(pClass,
						pDesc1->m_strName,
						CJsonFormEngineObj::GetID(pDesc1),
						AfxLoadJsonString(pDesc1->m_strHint, pDesc1),
						AfxLoadJsonString(pDesc1->m_strControlCaption, pDesc1),
						pDesc1->m_Width,
						dwNeededStyle,
						-1,
						pDataObj
					);
					if (pCtrl && pHotLink)
						pCtrl->AttachHotKeyLink(pHotLink, nButtonId);
				}
				}
			}
		}
	}
}

//-----------------------------------------------------------------------------
template <class T> void TBJsonBodyEditWrapper<T>::Customize()
{
	EnableInsertRow(((CWndBodyDescription*)m_pWndDesc)->m_bInsertRowEnabled);
	EnableAddRow(((CWndBodyDescription*)m_pWndDesc)->m_bAddRowEnabled);
	EnableDeleteRow(((CWndBodyDescription*)m_pWndDesc)->m_bDeleteRowEnabled);
	SetMultipleLinesPerRow(((CWndBodyDescription*)m_pWndDesc)->m_nLinesPerRow);
	CJsonContext* pRowViewContext = NULL;
	UINT nID = GetRowFormViewId();
	if (nID != 0)
	{
		CJsonResource res = AfxGetTBResourcesMap()->DecodeID(TbResources, nID);
		//è stata mappata come risorsa dinamica: si tratta di risorsa in file tbjson
		if (!res.IsEmpty())
		{
			pRowViewContext = (CJsonContext*)CJsonFormEngineObj::GetInstance()->CreateContext(res);
			if (pRowViewContext)
			{
				pRowViewContext->m_pDoc = m_pContext->m_pDoc;
				pRowViewContext->m_pDescription->EvaluateExpressions(pRowViewContext);
				//in presenza di text della rowview, viene usato quello, altrimenti si scala su quello della descrizione del bodyEdit
				if (pRowViewContext->m_pDescription->m_strText.IsEmpty())
					m_strRowFormViewTitle = AfxLoadJsonString(m_pWndDesc->m_strText, m_pWndDesc);
				else
					m_strRowFormViewTitle = AfxLoadJsonString(pRowViewContext->m_pDescription->m_strText, pRowViewContext->m_pDescription);
			}
		}
	}

	double numTitlesRows = ((CWndBodyDescription*)m_pWndDesc)->m_dTitleRows;
	bool bCountRows = numTitlesRows == DEFAULT_TITLE_ROWS;//se non specificato nel json, lo calcolo caso mai non ci stesse
	//ricorsione sui figli
	for (int i = 0; i < m_pWndDesc->m_Children.GetCount(); i++)
	{
		CWndObjDescription* pChild = m_pWndDesc->m_Children[i];

		if (pChild->m_Type == CWndObjDescription::ColTitle)
		{
			CWndBodyColumnDescription* pColDesc = (CWndBodyColumnDescription*)pChild;
			if (pRowViewContext)
			{
				FillMissingColumnProps(pRowViewContext->m_pDescription, pColDesc);
			}
			if (!m_pContext->CanCreateControl(pColDesc, CJsonFormEngineObj::GetID(pChild)))
				continue;

			CString sTitle = AfxLoadJsonString(pColDesc->m_strText, pColDesc);

			if (bCountRows)//se non specificato nel json, lo calcolo caso mai non ci stesse
			{
				int numLines = CountLines(sTitle);
				numTitlesRows = numLines > numTitlesRows ? numLines : numTitlesRows;
			}

			DataObj* pDataObj = NULL;
			HotKeyLink* pHotLink = NULL;
			DWORD nButtonId = BTN_DEFAULT;
			CString sFieldName, sBindingName;
			bool fieldFromHKL = false;

			if (pColDesc->m_pBindings)// && m_pWndDesc->m_pBindings)
			{
				if (m_pDBT) {
					pDataObj = m_pDBT->GetBindingData(m_pWndDesc->m_pBindings->m_strDataSource, pColDesc->m_pBindings->m_strDataSource, sFieldName, sBindingName, fieldFromHKL);
					if (!pDataObj && !GetDocument()->IsInStaticDesignMode())
					{
						TRACE2("Invalid data source: %s; id: %s\r\n", (LPCTSTR)pColDesc->m_pBindings->m_strDataSource, (LPCTSTR) pColDesc->GetID());
						ASSERT(FALSE);
						continue;
					}
				}
				HotLinkInfo* pInfo = NULL;
				if (pColDesc->m_pBindings)
					pInfo = pChild->m_pBindings->m_pHotLink;

				CString sHotLink;
				if (pInfo)
					sHotLink = pInfo->m_strName;
				if (pInfo && !sHotLink.IsEmpty() && !GetDocument()->IsInStaticDesignMode())
				{
					pHotLink = m_pContext->m_pDoc->GetHotLink(sHotLink, CTBNamespace(CTBNamespace::HOTLINK, pInfo->m_strNamespace));
#ifdef DEBUG
					if (!m_pContext->m_pDoc->IsInStaticDesignMode())
						ASSERT(pHotLink);
#endif
					if (pHotLink)
					{
						pHotLink->Parameterize(pInfo, pChild->m_pBindings->m_nButtonId);
						if (m_pDBT) {
							m_pDBT->AddHotLinkKeyField(pHotLink, sFieldName);
						}
					}
				}

				nButtonId = pColDesc->m_pBindings->m_nButtonId;

			}
			DWORD dwNeededStyle = 0, dwNeededExStyle = 0;
			CRuntimeClass* pClass = m_pContext->GetControlClass(pColDesc->m_strControlClass, pChild->m_Type, pDataObj, dwNeededStyle, dwNeededExStyle);
			if (!pClass)
			{
#ifdef DEBUG
				if (!GetDocument()->IsInStaticDesignMode())
				{
					TRACE2("Invalid controlClass: '%s' for column: '%s\r\n", (LPCTSTR)pColDesc->m_strControlClass, (LPCTSTR)pColDesc->GetID());
					ASSERT(FALSE);
				}
#endif // DEBUG
				continue;
			}

			if (pColDesc->m_bSort)
				dwNeededStyle |= CBS_SORT;
			DWORD computedStyle = dwNeededStyle | dwNeededExStyle & ~WS_EX_CLIENTEDGE;//client edge non serve per il bodyedit, inoltre non ho capito perché ma interferisce col textlimit (non va a capo quando dovrebbe)
			CString sColumnName = pColDesc->m_strName;
			if (!fieldFromHKL && sColumnName.IsEmpty())
				sColumnName = sFieldName;
			if (sColumnName.IsEmpty())
				sColumnName = pColDesc->GetID();
			ColumnInfo* pColumn = AddColumn(sColumnName, sTitle, computedStyle, CJsonFormEngine::GetID(pColDesc), pDataObj, pClass, pHotLink, nButtonId);
			if (pColDesc->m_nChars != -1 && pColDesc->m_nChars != 0)
				pColumn->SetCtrlSize(pColDesc->m_nChars, pColDesc->m_nRows, pColDesc->m_nTextLimit);
			if (pColDesc->m_Width != NULL_COORD)
				pColumn->SetScreenWidth(pColDesc->m_Width, FALSE);

			CParsedCtrl* pParsedCtrl = pColumn->GetParsedCtrl();
			AssignProps(pParsedCtrl, pDataObj, pColDesc);

			pParsedCtrl->m_pOwnerWndDescription = pColDesc;
			//pColumn->CreateCtrl(this);
			if (pColDesc->m_pStateData)
			{
				CString sBindingName;
				DBTObject* pDBT = NULL;
				SqlRecord* pRec = NULL;
				CString sFieldName;
				bool fieldFromHKL = false;
				if (m_pDBT) {
					pDataObj = m_pDBT->GetBindingData(m_pWndDesc->m_pBindings->m_strDataSource, pColDesc->m_pStateData->m_pBindings->m_strDataSource, sFieldName, sBindingName, fieldFromHKL);
					if (!pDataObj)
					{
						bool isVirtual = false;
						GetDocument()->GetDataSource(_T(""), pColDesc->m_pStateData->m_pBindings->m_strDataSource, pDBT, pRec, pDataObj, isVirtual);
					}
					if (pDataObj)
					{
						if (pDataObj->IsKindOf(RUNTIME_CLASS(DataInt)) || pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)))
						{
							pColumn->AttachStateData(pDataObj, pColDesc->m_pStateData->m_bInvertDefaultStates == B_TRUE);
						}
						else
						{
							ASSERT_TRACE2(FALSE, "Invalid data source type for state data; data source: %s; id:%s\r\n", (LPCTSTR)pColDesc->m_pStateData->m_pBindings->m_strDataSource, (LPCTSTR)pColDesc->GetID());
						}

					}
					else
					{
						ASSERT_TRACE2(FALSE, "Data source not found: %s; id:%s\r\n", (LPCTSTR)pColDesc->m_pStateData->m_pBindings->m_strDataSource, (LPCTSTR)pColDesc->GetID());
					}
				}
			}
			if (pColDesc->m_pItemSourceDescri)
				m_pContext->AttachItemSource(pColDesc->m_pItemSourceDescri, pParsedCtrl);

			for (int i = 0; i < pColDesc->m_arValidators.GetSize(); i++)
			{
				CValidatorDescription* pValidator = pColDesc->m_arValidators[i];
				if (pValidator)
					m_pContext->AttachValidator(pValidator, pParsedCtrl);
			}
			pParsedCtrl->ReadStaticPropertiesFromJson();
			if (pColDesc->m_pControlBehaviourDescription)
				m_pContext->AttachControlBehaviour(pColDesc->m_pControlBehaviourDescription, pParsedCtrl);

			if (pColDesc->m_pDataAdapterDescri)
				m_pContext->AttachDataAdapter(pColDesc->m_pDataAdapterDescri, pParsedCtrl);

			WORD wColumnStatus = STATUS_NORMAL;
			if (pColDesc->m_bStatusHidden)
				wColumnStatus |= (STATUS_HIDDEN);
			if (pColDesc->m_bStatusGrayed)
				wColumnStatus |= (STATUS_GRAYED);
			if (pColDesc->m_bStatusNoChange_Grayed)
				wColumnStatus |= (STATUS_NOCHANGE_GRAYED);
			if (pColDesc->m_bStatusNoChange_Hidden)
				wColumnStatus |= (STATUS_NOCHANGE_HIDDEN);
			if (pColDesc->m_bStatusLocked)
				wColumnStatus |= (STATUS_LOCKED);
			if (pColDesc->m_bStatusSortedAsc)
				wColumnStatus |= (STATUS_SORTED_ASC);
			if (pColDesc->m_bStatusSortedDes)
				wColumnStatus |= (STATUS_SORTED_DESC);

			pColumn->SetStatus(wColumnStatus);
			if (pColDesc->m_bHasFooterDescr && pDataObj)
				pColumn->SetHasFooter(pColDesc->m_bHasFooterDescr);
			if (pColDesc->m_bAllowEnlarge)
				pColumn->SetAllowEnlarge();
			m_pContext->OnColumnInfoCreated(pColumn);
		}
	}
	if (numTitlesRows > 1)
	{
		SetUITitlesRows(numTitlesRows);
	}
	if (((CWndBodyDescription*)m_pWndDesc)->m_nMaxRecords != MAX_BODY_RECORDS)
		SetMaxRecords(((CWndBodyDescription*)m_pWndDesc)->m_nMaxRecords);
	delete pRowViewContext;
}

//-----------------------------------------------------------------------------
template <class T> BOOL TBJsonBodyEditWrapper<T>::OnCreateClient()
{
	BOOL b = __super::OnCreateClient();
	//se la descrizione non è condivisa, dopo aver creato la finestra la associo alla sua descrizione
	//così posso aggiornare quest'ultima, ad esempio in fase di editing
	int idx = 0;
	//ricorsione sui figli
	for (int i = 0; i < m_pWndDesc->m_Children.GetCount(); i++)
	{
		ColumnInfo* pInfo = NULL;
		CWndObjDescription* pDesc = m_pWndDesc->m_Children[i];
		int idc = CJsonFormEngine::GetID(pDesc);
		while (idx < m_AllColumnsInfo.GetCount() && (pInfo = m_AllColumnsInfo[idx])->GetCtrlID() != idc)
			idx++;

		if (pInfo)
			pDesc->AttachTo(pInfo->GetParsedCtrl()->GetCtrlCWnd()->m_hWnd);
		if (idx >= m_AllColumnsInfo.GetCount())
			break;
		idx++;
	}
	return b;
}
//-----------------------------------------------------------------------------
template <class T> BOOL TBJsonBodyEditWrapper<T>::OnGetToolTipProperties(CBETooltipProperties* pTp)
{
	if (m_pContext->OnGetToolTipProperties(pTp))
		return TRUE;
	return __super::OnGetToolTipProperties(pTp);
}
//-----------------------------------------------------------------------------
template <class T> BOOL TBJsonBodyEditWrapper<T>::CanDoDeleteRow()
{
	CAbstractFormDoc* pDoc = GetDocument();
	return pDoc ? pDoc->CanDoDeleteRow(this) : __super::CanDoDeleteRow();
}
//-----------------------------------------------------------------------------
template <class T> void TBJsonBodyEditWrapper<T>::EnableButtons()
{
	m_pContext->EnableBodyEditButtons(this);
	__super::EnableButtons();
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonBodyEdit, CBodyEdit)
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonTreeBodyEdit, CTreeBodyEdit)

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonPropertyGrid, CTBPropertyGrid)
void CJsonPropertyGrid::Customize(CWndObjDescriptionContainer& children, CTBProperty* pGroup)
{
	CTBProperty* pProp = NULL;
	for (int i = 0; i < children.GetCount(); i++)
	{
		CWndPropertyGridItemDescription* pWndDesc = (CWndPropertyGridItemDescription*)children[i];
		if (pWndDesc->m_Type != CWndObjDescription::PropertyGridItem)
		{
			ASSERT(FALSE);
			continue;
		}
		UINT nID = CJsonFormEngineObj::GetID(pWndDesc);
		if (!m_pContext->CanCreateControl(pWndDesc, nID))
			continue;

		CString sText = AfxLoadJsonString(pWndDesc->m_strText, pWndDesc);
		CString sHint = AfxLoadJsonString(pWndDesc->m_strHint, pWndDesc);

		SqlRecord* pRec = NULL;
		DataObj* pDataObj = NULL;
		DBTObject* pDBT = NULL;
		CString sBindingName;
		m_pContext->GetBindingInfo(pWndDesc->GetID(), pWndDesc->m_strName, pWndDesc->m_pBindings, pDBT, pRec, pDataObj, sBindingName);

		DWORD dwNeededStyle = 0, dwNeededExStyle = 0;
		CRuntimeClass* pClass = m_pContext->GetControlClass(pWndDesc->m_strControlClass, pWndDesc->m_Type, pDataObj, dwNeededStyle, dwNeededExStyle);
		CString sHotLink;
		int nButtonID = BTN_DEFAULT;
		if (pWndDesc->m_pBindings)
		{
			if (pWndDesc->m_pBindings->m_pHotLink)
				sHotLink = pWndDesc->m_pBindings->m_pHotLink->m_strName;
			nButtonID = pWndDesc->m_pBindings->m_nButtonId;
		}
		HotKeyLink* pHotLink = NULL;
		if (pWndDesc->m_pBindings && pWndDesc->m_pBindings->m_pHotLink && !sHotLink.IsEmpty() && m_pContext->m_pDoc)
		{
			pHotLink = m_pContext->m_pDoc->GetHotLink(sHotLink, CTBNamespace(CTBNamespace::HOTLINK, pWndDesc->m_pBindings->m_pHotLink->m_strNamespace));
#ifdef DEBUG
			if (!m_pContext->m_pDoc->IsInStaticDesignMode())
				ASSERT(pHotLink);
#endif
			if (pHotLink)
				pHotLink->Parameterize(pWndDesc->m_pBindings->m_pHotLink, pWndDesc->m_pBindings->m_nButtonId);
		}

		DWORD wStyle = 0;
		if (pWndDesc->m_bSort)
			wStyle |= CBS_SORT;
		if (pWndDesc->m_bVScroll)
			wStyle |= WS_VSCROLL;
		if (pWndDesc->m_bPassword)
			wStyle |= ES_PASSWORD;
		if (pWndDesc->m_bAutoHScroll)
			wStyle |= ES_AUTOHSCROLL;
		if (pWndDesc->m_bMultiline)
			wStyle |= ES_MULTILINE;

		if (pGroup)
			pProp = AddSubItem(pGroup, sBindingName, sText, sHint, pDataObj, nID, dwNeededStyle | wStyle, pClass, pHotLink, nButtonID, pRec, pWndDesc->m_nRows);
		else
			pProp = AddProperty(sBindingName, sText, sHint, pDataObj, nID, dwNeededStyle | wStyle, pClass, pHotLink, nButtonID, pRec, pWndDesc->m_nRows);

		Customize(pWndDesc->m_Children, pProp);

		if (pWndDesc->m_bCollapsed)
			pProp->Expand(false);
		CParsedCtrl* pCtrl = pProp->GetControl();
		if (pCtrl)
		{
			pCtrl->m_pOwnerWndDescription = pWndDesc;
			pCtrl->ReadStaticPropertiesFromJson();
			pCtrl->ReadDynamicPropertiesFromJson();

			AssignProps(pCtrl, pDataObj, pWndDesc);
			if (pWndDesc->m_pItemSourceDescri)
				m_pContext->AttachItemSource(pWndDesc->m_pItemSourceDescri, pCtrl);

			for (int i = 0; i < pWndDesc->m_arValidators.GetSize(); i++)
			{
				CValidatorDescription* pValidator = pWndDesc->m_arValidators[i];
				if (pValidator)
					m_pContext->AttachValidator(pValidator, pCtrl);
			}

			if (pWndDesc->m_pControlBehaviourDescription)
				m_pContext->AttachControlBehaviour(pWndDesc->m_pControlBehaviourDescription, pCtrl);

			/*	if (pWndDesc->m_pContextMenuDescri)
					m_pContext->AttachContextMenu(pColDesc->m_pContextMenuDescri, pParsedCtrl);

				if (pWndDesc->m_pDataAdapterDescri)
					m_pContext->AttachDataAdapter(pColDesc->m_pDataAdapterDescri, pParsedCtrl);*/
		}
		m_pContext->OnPropertyCreated(pProp);
	}
}
//---------------------------------------------------------------------------------------
void CJsonPropertyGrid::OnCustomize()
{
	Customize(m_pWndDesc->m_Children, NULL);
}
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CJsonRowView, CRowFormView)

//-----------------------------------------------------------------------------
CJsonRowView::CJsonRowView(UINT nResourceId /*= 0*/)
	:
	CRowFormView(_NS_VIEW(""), nResourceId)
{
}
//-----------------------------------------------------------------------------
void CJsonRowView::BuildDataControlLinks()
{
}
//-----------------------------------------------------------------------------
void CJsonFormEngine::ClearCache()
{
	ClearDescriptions();
	InitCachePath();
	if (PathFileExists(m_sCachePath))
		RemoveFolderTree(m_sCachePath);
}

//-----------------------------------------------------------------------------
BOOL CJsonFormEngine::ProcessWndDescription(CWndObjDescription* pWndDesc, CWnd* pParentWnd, CJsonContextObj* pContextObj)
{
	CJsonContext* pContext = (CJsonContext*)pContextObj;
	if (!pContext->CanCreateControl(pWndDesc, CJsonFormEngineObj::GetID(pWndDesc)))
		return FALSE;
	//preprocessing
	switch (pWndDesc->m_Type)
	{
	case CWndObjDescription::BodyEdit:
	case CWndObjDescription::TreeBodyEdit:
	case CWndObjDescription::PropertyGrid:
	{
		CString sAnchor = CalculateLURect(pWndDesc, pContext);
		//se il bodyedit è il primo controllo, lo allineo in posizione 0,0
		if (pWndDesc->m_CalculatedLURect.top == SPACING && pWndDesc->m_CalculatedLURect.left == LEFT_COL_1_MINI)
			pWndDesc->m_CalculatedLURect.OffsetRect(-LEFT_COL_1_MINI, -SPACING);
		UpdateAnchorInfo(sAnchor, pWndDesc, pContext);
		return TRUE;
	}
	case CWndObjDescription::TileManager:
	case CWndObjDescription::TileGroup:
	case CWndObjDescription::TilePanel:
	case CWndObjDescription::Tabber:
	case CWndObjDescription::Tab:
	case CWndObjDescription::Tile:
	case CWndObjDescription::LayoutContainer:
	case CWndObjDescription::HeaderStrip:
	case CWndObjDescription::HotFilter:
	case CWndObjDescription::Splitter:
	case CWndObjDescription::Toolbar:
	case CWndObjDescription::TabbedToolbar:
	case CWndObjDescription::Frame:
	case CWndObjDescription::View:
	{
		return TRUE;		//oggetti creati dopo che il documento è disponibile, nella BuildDataControlLinks
	}
	default:
		//prima creo la finestra
		return __super::ProcessWndDescription(pWndDesc, pParentWnd, pContextObj);
	}
}

//-----------------------------------------------------------------------------
BOOL CJsonFormEngine::AddLink(CWndObjDescription* pWndDesc, CWnd* pParentWnd, CJsonContextObj* pContextObj, CAbstractFormView* pParentView, DBTObject*&pDBT)
{
	CJsonContext* pContext = (CJsonContext*)pContextObj;
	if (!pContext->CanCreateControl(pWndDesc, CJsonFormEngineObj::GetID(pWndDesc)))
		return FALSE;
	CParsedForm* pForm = GetParsedForm(pParentWnd);

	ASSERT(((CJsonContext*)pContextObj)->m_pForm == pForm);
	ASSERT(((CJsonContext*)pContextObj)->m_pLinks == (pForm ? pForm->GetControlLinks() : NULL));

	CParsedCtrl* pParsedCtrl = NULL;
	//post processing
	switch (pWndDesc->m_Type)
	{

	case CWndObjDescription::PropertyGrid:
	{
		if (!pContext->m_pForm)
			return TRUE;
		UINT nID = CJsonFormEngineObj::GetID(pWndDesc);
		CRuntimeClass* pClass = pContext->m_pDoc->GetControlClass(nID);
		if (!pClass)
			pClass = RUNTIME_CLASS(CJsonPropertyGrid);
		ASSERT(pClass->IsDerivedFrom(RUNTIME_CLASS(CJsonPropertyGrid)));

		CRect r = pWndDesc->m_CalculatedLURect;
		::MapDialogRect(pParentWnd->m_hWnd, r);
		CJsonPropertyGrid* pGrid = (CJsonPropertyGrid*)pClass->CreateObject();
		pGrid->SetDescription(pContext, pWndDesc);
		::AddLinkPropertyGrid(pGrid, pContext->m_pForm, pParentWnd, pContext->m_pLinks, nID, CJsonFormEngine::GetObjectName(pWndDesc), r);
		if (!pGrid)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		pWndDesc->AttachTo(pGrid->m_hWnd);
		return TRUE;
	}
	case CWndObjDescription::BodyEdit:
	case CWndObjDescription::TreeBodyEdit:
	{
		if (!pContext->m_pForm)
			return TRUE;

		SqlRecord* pRec = NULL;
		DataObj* pDataObj = NULL;
		DBTObject* pDBT = NULL;
		CString sBindingName;
		pContext->GetBindingInfo(pWndDesc->GetID(), pWndDesc->m_strName, pWndDesc->m_pBindings, pDBT, pRec, pDataObj, sBindingName);

		//codice simile alla AddLinkAndCreateBodyEdit, ma con alcune modifiche a metà istruzioni
		if (pDBT && !pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		{
			ASSERT(FALSE);
			return FALSE;
		}
		int idc = CJsonFormEngine::GetID(pWndDesc);

		CHECK_UNIQUE_NAME(pContext->m_pLinks, sBindingName);

		CRuntimeClass* pClass = pContext->m_pDoc ? pContext->m_pDoc->GetControlClass(idc) : NULL;
		if (pClass && (!pClass->IsDerivedFrom(RUNTIME_CLASS(CJsonBodyEdit)) || !pClass->IsDerivedFrom(RUNTIME_CLASS(CBodyEdit))))
		{
			ASSERT(FALSE);
			pClass = NULL;
		}

		if (!pClass)
		{
			pClass = (pWndDesc->m_Type == CWndObjDescription::BodyEdit) ? RUNTIME_CLASS(CJsonBodyEdit) : RUNTIME_CLASS(CJsonTreeBodyEdit);
		}

		CBodyEdit* pBodyEdit = (CBodyEdit*)pClass->CreateObject();
		pBodyEdit->SetName(sBindingName);
		pBodyEdit->GetNamespace().SetChildNamespace(CTBNamespace::GRID, sBindingName, pContext->m_pForm->GetNamespace());
		pBodyEdit->SetParentForm(pContext->m_pForm);
		pBodyEdit->SetBodyEditID(idc);

		CTBJsonData* pJsonData = dynamic_cast<CTBJsonData*>(pBodyEdit);
		if (pJsonData)
		{
			pJsonData->SetDescription(pContext, pWndDesc);//devo assegnare la descrizione prima della Attach, che chiamerà la Customize
		}

		CString sTitle = AfxLoadJsonString(pWndDesc->m_strText, pWndDesc);

		IJsonBodyEditWrapper* pJsonBodyEditWrapper = dynamic_cast<IJsonBodyEditWrapper*>(pBodyEdit);
		if (pJsonBodyEditWrapper)
		{
			pJsonBodyEditWrapper->SetAllBodyEditStyles();
			CRuntimeClass* pRowViewClass = pJsonBodyEditWrapper->GetRowViewClass();
			pBodyEdit->Attach((DBTSlaveBuffered*)pDBT, pRowViewClass, sTitle);
		}
		CString sAnchor;

		CRect pxRect = pWndDesc->m_CalculatedLURect;
		::MapDialogRect(pParentWnd->m_hWnd, pxRect);
		DWORD style = WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | BS_OWNERDRAW;
		if (pWndDesc->m_bTabStop)
			style |= WS_TABSTOP;
		BOOL bCreated = pBodyEdit->Create
		(
			style,
			pxRect,
			pParentWnd,
			idc
		);

		pContext->m_pLinks->Add(pBodyEdit);
		pWndDesc->AttachTo(pBodyEdit->m_hWnd);


		return TRUE;
	}


	case CWndObjDescription::TileManager:
	{

		CJsonTileManager* pTiler = NULL;
		int idc = CJsonFormEngine::GetID(pWndDesc);
		CRuntimeClass* pClass = pContext->m_pDoc ? pContext->m_pDoc->GetControlClass(idc) : NULL;
		if (pClass == NULL)
			pClass = RUNTIME_CLASS(CJsonTileManager);
		ASSERT(pClass->IsDerivedFrom(RUNTIME_CLASS(CJsonTileManager)));
		ASSERT_KINDOF(CTabberDescription, pWndDesc);
		if (pParentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			pTiler = (CJsonTileManager*)((CAbstractFormView*)pParentWnd)->AddTileManager(idc, pClass, GetObjectName(pWndDesc), FALSE);
		}
		else if (pParentWnd->IsKindOf(RUNTIME_CLASS(CJsonTileDialog)))
		{
			pTiler = (CJsonTileManager*)((CJsonTileDialog*)pParentWnd)->AddTileManager(idc, pClass, GetObjectName(pWndDesc), FALSE);
		}
		else
		{
			CParsedForm* pDialog = GetParsedForm(pParentWnd);
			if (pDialog)
			{

				pTiler = (CJsonTileManager*)pDialog->AddBaseTileManager(idc, pClass, GetObjectName(pWndDesc), FALSE);
			}
		}
		if (!pTiler)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		pTiler->SetDescription(pContext, pWndDesc);
		if (((CTabberDescription*)pWndDesc)->m_bIsVertical == Bool3::B_TRUE)
			pTiler->SetShowMode(CTabSelector::VERTICAL_TILE);
		else if (((CTabberDescription*)pWndDesc)->m_bIsVertical == Bool3::B_FALSE)
			pTiler->SetShowMode(CTabSelector::NORMAL);

		pTiler->OnInitialUpdate(idc, pParentWnd);
		int nDirStretch = 0;
		if (pWndDesc->m_bHFill)
			nDirStretch |= 1;
		if (pWndDesc->m_bVFill)
			nDirStretch |= 2;
		pTiler->SetAutoSizeCtrl(nDirStretch);

		pTiler->SetDescription(NULL, NULL);
		pWndDesc->AttachTo(pTiler->m_hWnd);
		return TRUE;
	}
	case CWndObjDescription::TileGroup:
	{
		ASSERT_KINDOF(CWndLayoutContainerDescription, pWndDesc);
		UINT nGroupID = CJsonFormEngineObj::GetID(pWndDesc);

		CRuntimeClass* pClass = pContext->m_pDoc->GetControlClass(nGroupID);

		bool bManageUnpinned = ((CWndLayoutContainerDescription*)pWndDesc)->m_bManageUnpinned;

		if (bManageUnpinned)
		{
			if (!pClass)
				pClass = RUNTIME_CLASS(CJsonPinnedTilesTileGroup);
			ASSERT(pClass->IsDerivedFrom(RUNTIME_CLASS(CJsonPinnedTilesTileGroup)));
		}
		else
		{
			if (!pClass)
				pClass = RUNTIME_CLASS(CJsonTileGroup);
			ASSERT(pClass->IsDerivedFrom(RUNTIME_CLASS(CJsonTileGroup)));
		}

		CJsonTileGroup*				pGroup = NULL;
		CJsonPinnedTilesTileGroup*	pGroupPinned = NULL;

		if (bManageUnpinned)
		{
			if (pParentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
				pGroupPinned = (CJsonPinnedTilesTileGroup*)((CAbstractFormView*)pParentWnd)->AddTileGroup(nGroupID, pClass, CJsonFormEngine::GetObjectName(pWndDesc), FALSE);
			else if (pParentWnd->IsKindOf(RUNTIME_CLASS(CParsedDialogWithTiles)))
				pGroupPinned = (CJsonPinnedTilesTileGroup*)((CParsedDialogWithTiles*)pParentWnd)->AddTileGroup(nGroupID, pClass, CJsonFormEngine::GetObjectName(pWndDesc), FALSE);
			else if (pParentWnd->IsKindOf(RUNTIME_CLASS(CTabDialog)))
				pGroupPinned = (CJsonPinnedTilesTileGroup*)((CTabDialog*)pParentWnd)->AddTileGroup(nGroupID, pClass, CJsonFormEngine::GetObjectName(pWndDesc), FALSE);
			else
			{
				CParsedForm* pDialog = GetParsedForm(pParentWnd);
				if (pDialog)
				{
					pGroupPinned = (CJsonPinnedTilesTileGroup*)pDialog->AddBaseTileGroup(nGroupID, pClass, GetObjectName(pWndDesc), FALSE);
				}
			}
			if (!pGroupPinned)
			{
				ASSERT(FALSE);
				return FALSE;
			}
		}
		else
		{
			if (pParentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
				pGroup = (CJsonTileGroup*)((CAbstractFormView*)pParentWnd)->AddTileGroup(nGroupID, pClass, CJsonFormEngine::GetObjectName(pWndDesc), FALSE);
			else if (pParentWnd->IsKindOf(RUNTIME_CLASS(CParsedDialogWithTiles)))
				pGroup = (CJsonTileGroup*)((CParsedDialogWithTiles*)pParentWnd)->AddTileGroup(nGroupID, pClass, CJsonFormEngine::GetObjectName(pWndDesc), FALSE);
			else if (pParentWnd->IsKindOf(RUNTIME_CLASS(CTabDialog)))
				pGroup = (CJsonTileGroup*)((CTabDialog*)pParentWnd)->AddTileGroup(nGroupID, pClass, CJsonFormEngine::GetObjectName(pWndDesc), FALSE);
			else
			{
				CParsedForm* pDialog = GetParsedForm(pParentWnd);
				if (pDialog)
				{
					pGroup = (CJsonTileGroup*)pDialog->AddBaseTileGroup(nGroupID, pClass, GetObjectName(pWndDesc), FALSE);
				}
			}
			if (!pGroup)
			{
				ASSERT(FALSE);
				return FALSE;
			}
		}
		CJsonContext* pNewContext = CJsonContext::Create();
		pNewContext->Assign(pContext);

		if (bManageUnpinned)
		{
			pGroupPinned->SetOwnsPane(((CWndLayoutContainerDescription*)pWndDesc)->m_bOwnsPane);
			pGroupPinned->SetDescription(pNewContext, pWndDesc);
		}
		else
			pGroup->SetDescription(pNewContext, pWndDesc);

		CRect r = pWndDesc->GetRect();
		::MapDialogRect(pParentWnd->m_hWnd, r);

		if (bManageUnpinned)
		{
			pGroupPinned->OnInitialUpdate(nGroupID, pParentWnd, r);
			pWndDesc->AttachTo(pGroupPinned->m_hWnd);
		}
		else
		{
			pGroup->OnInitialUpdate(nGroupID, pParentWnd, r);
			pWndDesc->AttachTo(pGroup->m_hWnd);
		}

		return TRUE;
	}
	case CWndObjDescription::Tabber:
	{
		CBaseTabManager* pTabber = NULL;
		int idc = CJsonFormEngine::GetID(pWndDesc);
		//sono un tab wizard wizard se lo dichiaro o se lo dichara la parent view
		bool bWizard = ((CTabberDescription*)pWndDesc)->m_bWizard;
		if (!bWizard)
		{
			CWndObjDescription* pRoot = pWndDesc->GetRoot();
			if (pRoot && pRoot->m_Type == CWndObjDescription::Frame)
				bWizard = ((CWndFrameDescription*)pRoot)->m_bWizard;
		}
		CRuntimeClass *pClass = bWizard
			? RUNTIME_CLASS(CJsonWizardTabManager)
			: RUNTIME_CLASS(CJsonTabManager);
		if (pParentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			pTabber = ((CAbstractFormView*)pParentWnd)->AddTabManager(idc, pClass, GetObjectName(pWndDesc), FALSE);
		}
		else
		{
			CParsedForm* pDialog = GetParsedForm(pParentWnd);
			if (pDialog)
			{
				pTabber = pDialog->AddBaseTabManager(idc, pClass, GetObjectName(pWndDesc), FALSE);
			}
		}
		if (!pTabber)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		CTBJsonData* pJsonData = dynamic_cast<CTBJsonData*>(pTabber);
		pJsonData->SetDescription(pContext, pWndDesc);

		pTabber->OnInitialUpdate(idc, pParentWnd);
		int nDirStretch = 0;
		if (pWndDesc->m_bHFill)
			nDirStretch |= 1;
		if (pWndDesc->m_bVFill)
			nDirStretch |= 2;
		pTabber->SetAutoSizeCtrl(nDirStretch);

		pJsonData->SetDescription(NULL, NULL);
		pWndDesc->AttachTo(pTabber->m_hWnd);
		return TRUE;
	}

	case CWndObjDescription::Tile:
	case CWndObjDescription::TilePanel:
	case CWndObjDescription::Tab:
		return TRUE;
	case CWndObjDescription::HeaderStrip:
	{
		CJsonHeaderStrip* pHeader = NULL;
		int idc = CJsonFormEngine::GetID(pWndDesc);


		if (pParentWnd->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			CRect r = pWndDesc->GetRect();
			::MapDialogRect(pParentWnd->m_hWnd, r);
			CString sTitle = AfxLoadJsonString(pWndDesc->m_strText, pWndDesc);
			pHeader = (CJsonHeaderStrip*)((CAbstractFormView*)pParentWnd)->AddHeaderStrip(idc, sTitle, FALSE, r, RUNTIME_CLASS(CJsonHeaderStrip));
		}
		if (!pHeader)
		{
			ASSERT(FALSE);
			return FALSE;
		}
		if (pWndDesc->m_pBindings)
		{
			int curPos = 0;
			CString strToken = pWndDesc->m_pBindings->m_strDataSource.Tokenize(_T(","), curPos);
			while (strToken != "")
			{
				SqlRecord* pRec = NULL;
				DataObj* pDataObj = NULL;
				DBTObject* pDBT = NULL;
				CString sBindingName;
				if (pContext->m_pDoc)
					pContext->m_pDoc->GetBindingInfo(strToken, _T(""), pDBT, pRec, pDataObj, sBindingName, TRUE);
				if (pDataObj)
					pHeader->Add(pDataObj);
				strToken = pWndDesc->m_pBindings->m_strDataSource.Tokenize(_T(","), curPos);

			}

		}

		CJsonContext* pNewContext = CJsonContext::Create();
		pNewContext->Assign(pContext);

		pHeader->SetDescription(pNewContext, pWndDesc);


		CRect r = pWndDesc->GetRect();
		::MapDialogRect(pParentWnd->m_hWnd, r);

		pHeader->OnInitialUpdate(idc, pParentWnd, r);


		pWndDesc->AttachTo(pHeader->m_hWnd);
		break;
	}
	default:
	{
		if (!pContext->m_pForm)
			return TRUE;
		SqlRecord* pRec = NULL;
		DataObj* pDataObj = NULL;
		bool fieldFromHKL = false;
		CString sBindingName = pWndDesc->m_strName;
		//se sono in rowview chiedo i dati al solo dbt slavebuffered
		if (pParentView && pParentView->IsKindOf(RUNTIME_CLASS(CRowFormView)) && pDBT && pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)))
		{
			if (pWndDesc->m_pBindings)
			{
				pRec = pDBT->GetRecord();
				CString sFieldName;
				pDataObj = ((DBTSlaveBuffered*)pDBT)->GetBindingData(_T(""), pWndDesc->m_pBindings->m_strDataSource, sFieldName, sBindingName, fieldFromHKL);

			}
			else if (sBindingName.IsEmpty())
			{
				sBindingName = pWndDesc->GetID();
			}

		}
		//se non sono in rowview, oppure il dbt non ha il dato, seguo il giro normale
		if (!pDataObj)
		{
			pContext->GetBindingInfo(pWndDesc->GetID(), pWndDesc->m_strName, pWndDesc->m_pBindings, pDBT, pRec, pDataObj, sBindingName);
		}
		DWORD dwNeededStyle = 0, dwNeededExStyle = 0;
		CRuntimeClass* pClass = pContext->GetControlClass(pWndDesc->m_strControlClass, pWndDesc->m_Type, pDataObj, dwNeededStyle, dwNeededExStyle);
		if (!pClass)
			return TRUE;
		HotKeyLink* pHotLink = NULL;
		CString sHotLink;
		int nButtonID = BTN_DEFAULT;

		if (pWndDesc->m_pBindings)
		{
			if (pWndDesc->m_pBindings->m_pHotLink)
				sHotLink = pWndDesc->m_pBindings->m_pHotLink->m_strName;
			nButtonID = pWndDesc->m_pBindings->m_nButtonId;
		}

		int idc = CJsonFormEngine::GetID(pWndDesc);
		if (pWndDesc->m_pBindings && pWndDesc->m_pBindings->m_pHotLink && !sHotLink.IsEmpty() && pContext->m_pDoc)
		{
			pHotLink = pContext->m_pDoc->GetHotLink(sHotLink, CTBNamespace(CTBNamespace::HOTLINK, pWndDesc->m_pBindings->m_pHotLink->m_strNamespace));

#ifdef DEBUG
			if (pContext->m_pDoc->GetDesignMode() != CBaseDocument::DM_STATIC)
				ASSERT(pHotLink);
#endif

			if (pHotLink)
			{
				pHotLink->Parameterize(pWndDesc->m_pBindings->m_pHotLink, pWndDesc->m_pBindings->m_nButtonId);
				if (pDBT && pDBT->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered))) {
					CString sFieldName = pRec->GetColumnName(pDataObj);
					((DBTSlaveBuffered*)pDBT)->AddHotLinkKeyField(pHotLink, sFieldName);
				}
			}

		}
		//prima provo col metodo virtuale della parsed form
		pParsedCtrl = pContext->m_pForm->AddLink(idc, sBindingName, pRec, pDataObj, pClass, pHotLink, nButtonID);
		if (!pParsedCtrl)
		{
			//nel caso le classi derivate non lo avessero implementato, uso quello globale, potrei perdere dei comportamenti
			pParsedCtrl = ::AddLink(sBindingName, pParentWnd, pContext->m_pLinks, idc, pRec, pDataObj, pClass, pHotLink, nButtonID);
			if (pParsedCtrl)
				pContext->m_pForm->SetChildControlNamespace(sBindingName, pParsedCtrl);

		}
		if (!pParsedCtrl)
		{
			ASSERT(FALSE);
			return FALSE;
		}

		pParsedCtrl->m_pOwnerWndDescription = pWndDesc;
		if (!pWndDesc->m_bVisible && pDataObj)
			pDataObj->SetHide();
		bool bFontSet = false;
		CParsedStatic* pParsedStatic = dynamic_cast<CParsedStatic*>(pParsedCtrl);
		if (pParsedStatic)
			pParsedStatic->m_bRightAnchor = pWndDesc->m_bRightAnchor;

		if (pWndDesc->m_pNumbererDescription)
		{
			CString sService = pWndDesc->m_pNumbererDescription->m_sServiceNs;
			if (!pWndDesc->m_pNumbererDescription->m_sServiceName.IsEmpty())
				sService += szNsInstanceSeparator + pWndDesc->m_pNumbererDescription->m_sServiceName;
			pParsedCtrl->AttachNumberer(sService, FALSE, pWndDesc->m_pNumbererDescription->m_bUseFormatMask);
		}

		if (pWndDesc->IsKindOf(RUNTIME_CLASS(CParsedLabelDescription)))
		{
			CParsedLabelDescription* pLabelDesc = (CParsedLabelDescription*)pWndDesc;

			CLabelStatic* pLabelStatic = dynamic_cast<CLabelStatic*>(pParsedCtrl);
			if (pLabelStatic)
			{
				bFontSet = true;
				if (pWndDesc->m_pFontDescription)
				{
					pLabelStatic->SetOwnFont(
						pWndDesc->m_pFontDescription->m_bIsBold,
						pWndDesc->m_pFontDescription->m_bIsItalic,
						pWndDesc->m_pFontDescription->m_bIsUnderline,
						(int)pWndDesc->m_pFontDescription->m_nFontSize,
						pWndDesc->m_pFontDescription->m_strFaceName);
				}
				if (pLabelDesc->m_LinePos != CLabelStatic::LP_NONE)
				{
					if (!pWndDesc->m_pFontDescription)
						pLabelStatic->SetOwnFont(AfxGetThemeManager()->GetStaticWithLineFont(), FALSE);

					COLORREF titleColor = pLabelDesc->m_crTextColor == EMPTY_COLOR
						? AfxGetThemeManager()->GetStaticWithLineLineForeColor()
						: pLabelDesc->m_crTextColor;

					pLabelStatic->SetTextColor(titleColor);

					pLabelStatic->ShowTextWithLine(titleColor, 1, pLabelDesc->m_LinePos);
				}
			}

		}
		if (!bFontSet && pWndDesc->m_pFontDescription)//la label static ha la propria logica di gestione del font
		{
			CCustomFont *pCustomFont = dynamic_cast<CCustomFont*>(pParsedCtrl);
			if (pCustomFont)
			{
				pCustomFont->SetOwnFont(pWndDesc->m_pFontDescription->m_bIsBold,
					pWndDesc->m_pFontDescription->m_bIsItalic,
					pWndDesc->m_pFontDescription->m_bIsUnderline,
					(int)pWndDesc->m_pFontDescription->m_nFontSize,
					pWndDesc->m_pFontDescription->m_strFaceName);
			}
		}
		if (pWndDesc->IsKindOf(RUNTIME_CLASS(CComboDescription)) && ((CComboDescription*)pWndDesc)->m_pItemSourceDescri)
			pContext->AttachItemSource(((CComboDescription*)pWndDesc)->m_pItemSourceDescri, pParsedCtrl);
		else if (pWndDesc->IsKindOf(RUNTIME_CLASS(CListDescription)) && ((CListDescription*)pWndDesc)->m_pItemSourceDescri)
			pContext->AttachItemSource(((CListDescription*)pWndDesc)->m_pItemSourceDescri, pParsedCtrl);

		pParsedCtrl->ReadDynamicPropertiesFromJson();
		pParsedCtrl->ReadStaticPropertiesFromJson();
		if (pWndDesc->m_pControlBehaviourDescription)
			pContext->AttachControlBehaviour(pWndDesc->m_pControlBehaviourDescription, pParsedCtrl);

		CTextObjDescription* pTextDescription = dynamic_cast<CTextObjDescription*>(pWndDesc);
		if (pTextDescription)
		{
			if (pTextDescription->m_arValidators.GetSize() > 0)
			{
				for (int i = 0; i < pTextDescription->m_arValidators.GetSize(); i++)
				{
					CValidatorDescription* pValidator = pTextDescription->m_arValidators[i];
					if (pValidator)
						pContext->AttachValidator(pValidator, pParsedCtrl);
				}
			}

			if (pTextDescription->m_pDataAdapter)
			{
				pContext->AttachDataAdapter(pTextDescription->m_pDataAdapter, pParsedCtrl);
			}
		}

		AssignProps(pParsedCtrl, pDataObj, pWndDesc);
		if (!pWndDesc->m_strText.IsEmpty())
		{
			CString sText = AfxLoadJsonString(pWndDesc->m_strText, pWndDesc);
			pParsedCtrl->SetValue(sText);
		}
		if (!pWndDesc->m_strControlCaption.IsEmpty())
		{
			CString sCaption = AfxLoadJsonString(pWndDesc->m_strControlCaption, pWndDesc);
			int nCaptionW = pWndDesc->m_CaptionWidth;
			if (nCaptionW == NULL_COORD)
				nCaptionW = pWndDesc->GetParent()->m_CaptionWidth;
			if (nCaptionW != NULL_COORD)
			{
				CRect rCaption = CRect(0, 0, nCaptionW, 0);
				::MapDialogRect(pParentWnd->m_hWnd, rCaption);
				nCaptionW = rCaption.Width();
			}
			pParsedCtrl->SetCtrlCaption(sCaption,
				pWndDesc->m_CaptionHorizontalAlign,
				pWndDesc->m_CaptionVerticalAlign,
				CParsedCtrl::Left,
				nCaptionW,
				FALSE);
			CControlLabel* pLabel = pParsedCtrl->GetControlLabel();
			if (pWndDesc->m_pCaptionFontDescription)
			{
				CFont* pFont = pLabel->GetFont();
				pLabel->SetFont(pContext->CreateFontFromDesc(pFont, pWndDesc->m_pCaptionFontDescription));
			}
			if (!pWndDesc->m_bVisible)
				pLabel->ShowWindow(SW_HIDE);
			else
				pParsedCtrl->SetCtrlLabelDefaultPosition(pWndDesc->m_CaptionHorizontalAlign, pWndDesc->m_CaptionVerticalAlign, CParsedCtrl::Left, nCaptionW);
		}


		if (pWndDesc->m_pStateData)
		{
			pContext->GetBindingInfo(pWndDesc->GetID(), pWndDesc->m_strName, pWndDesc->m_pStateData->m_pBindings, pDBT, pRec, pDataObj, sBindingName);
			if (pDataObj)
			{
				pParsedCtrl->AttachStateData(pDataObj, pWndDesc->m_pStateData->m_bInvertDefaultStates == B_TRUE);
				pParsedCtrl->UpdateStateButtons();
				if (pWndDesc->m_pNumbererDescription)
				{
					CStateCtrlObj* pStateButton = pParsedCtrl->GetStateCtrl(pDataObj);
					if (pStateButton)
					{
						pStateButton->EnableStateInEditMode(pWndDesc->m_pStateData->m_bEnableStateInEdit);
						pStateButton->EnableCtrlInEditMode(pWndDesc->m_pStateData->m_bEnableStateCtrlInEdit);
					}
				}
			}

		}

		pContext->OnParsedControlCreated(pParsedCtrl);

		pWndDesc->AttachTo(pParsedCtrl->GetCtrlCWnd()->m_hWnd);
	}
	}

	return TRUE;
}


//-----------------------------------------------------------------------------
void CJsonFormEngine::BuildWebControlLinks(CParsedForm* pParsedForm, CJsonContext* pContext, CWndObjDescription* pWndDesc)
{
	//ricorsione sui figli
	for (int i = 0; i < pWndDesc->m_Children.GetCount(); i++)
	{
		CWndObjDescription* pChild = pWndDesc->m_Children[i];

		UINT nIDC = CJsonFormEngineObj::GetID(pChild);
		if (!pContext->CanCreateControl(pChild, nIDC))
			continue;

		if (pChild->m_Type == CWndObjDescription::HotFilter)
		{
			pContext->CreateHotFilter(nIDC, pChild);
			continue;
		}
		SqlRecord* pRec = NULL;
		DataObj* pDataObj = NULL;
		DBTObject* pDBT = NULL;
		CString sBindingName;
		if (pChild->m_Type == CWndObjDescription::ColTitle)
		{
			CWndObjDescription* pRowViewDesc = ((CWndBodyDescription*)pWndDesc)->GetRowViewDescription(pContext);
			if (pRowViewDesc)
				FillMissingColumnProps(pRowViewDesc, (CWndBodyColumnDescription*)pChild);

			if (pChild->m_pBindings)
			{
				CString sDataSource = pChild->m_pBindings->m_strDataSource;
				int idx = sDataSource.Find(_T('.'));
				if (idx == -1)
				{
					BindingInfo* pParentBinding = pChild->GetParent()->m_pBindings;
					if (pParentBinding)
						sDataSource = pParentBinding->m_strDataSource + _T('.') + sDataSource;
				}
				
				if (pContext->m_pDoc)
					pContext->m_pDoc->GetBindingInfo(sDataSource, pChild->GetID(), pDBT, pRec, pDataObj, sBindingName, TRUE);
			}
		}
		else
		{
			pContext->GetBindingInfo(pChild->GetID(), pChild->m_strName, pChild->m_pBindings, pDBT, pRec, pDataObj, sBindingName);
		}
		//solo i parsed controls
		if (!pChild->m_strControlClass.IsEmpty())
		{
			HotKeyLink* pHotLink = NULL;
			HotLinkInfo* pInfo = NULL;
			if (pChild->m_pBindings)
				pInfo = pChild->m_pBindings->m_pHotLink;

			int idc = CJsonFormEngine::GetID(pChild);

		

			if (pInfo && !pInfo->m_strName.IsEmpty() && pContext->m_pDoc)
			{
				pHotLink = pContext->m_pDoc->GetHotLink(pInfo->m_strName, CTBNamespace(CTBNamespace::HOTLINK, pInfo->m_strNamespace));

#ifdef DEBUG
				if (pContext->m_pDoc->GetDesignMode() != CBaseDocument::DM_STATIC)
					ASSERT(pHotLink);
#endif

				if (pHotLink)
					pHotLink->Parameterize(pInfo, pChild->m_pBindings->m_nButtonId);

			}
			DWORD dwNeededStyle = 0, dwNeededExStyle = 0;
			CRuntimeClass* pClass = pContext->GetControlClass(pChild->m_strControlClass, pChild->m_Type, pDataObj, dwNeededStyle, dwNeededExStyle);
			if (!pClass)
			{
				ASSERT(FALSE);
				continue;
			}
			//se non ha data obj, come fara il gestionalista a fare EnableWindow() ecc.?
			CAbstractFormDoc* pDoc = pContext->m_pDoc;
			CParsedCtrl* pControl = pDoc ? pDoc->DispatchOnCreateParsedCtrl(nIDC, pClass) : NULL;
			CObject* pObject = pClass->CreateObject();
			if (pControl == NULL)
				pControl = GetParsedCtrl(pObject);
			if (!pControl)
				continue;

			pChild->m_pAssociatedControl = pControl;
			pControl->m_pOwnerWndDescription = pChild;
			//pControl->Attach(nBtnID);
			pControl->AttachDocument(pDoc);
			if (pDataObj)
			{
				if (!pChild->m_bVisible)
					pDataObj->SetHide();
				pControl->Attach(pDataObj);
			}
			if (pHotLink)
			{
				pHotLink->AttachDocument(pContext->m_pDoc);
				pControl->AttachHotKeyLink(pHotLink);
			}

			// pRecord can be NULL for DataObj not conneted to any SqlRecord
			// use default len and precision
			//
			if (pRec && pDataObj)
			{
				long nLength = 0;
				TRY
				{
					nLength = pRec->GetColumnLength(pDataObj); //potrebbe essere anche un parametro
				}
					CATCH(SqlException, e)
				{
					delete pControl;
					ASSERT(FALSE);
					THROW_LAST();
				}
				END_CATCH

					pControl->SetCtrlMaxLen(nLength, FALSE);
				pControl->AttachRecord(pRec);
			}

			if (pHotLink)
			{
				pHotLink->DoOnCreatedOwnerCtrl();
			}

			if (pChild->m_pNumbererDescription)
			{
				CString sService = pChild->m_pNumbererDescription->m_sServiceNs;
				if (!pChild->m_pNumbererDescription->m_sServiceName.IsEmpty())
					sService += szNsInstanceSeparator + pChild->m_pNumbererDescription->m_sServiceName;
				pControl->AttachNumberer(sService, FALSE, pChild->m_pNumbererDescription->m_bUseFormatMask);
			}

			if (pChild->m_pStateData)
			{
				if (pContext->m_pDoc)
					pContext->GetBindingInfo(pChild->GetID(), pChild->m_strName, pChild->m_pStateData->m_pBindings, pDBT, pRec, pDataObj, sBindingName);
				if (pDataObj)
				{
					pControl->AttachStateData(pDataObj, pChild->m_pStateData->m_bInvertDefaultStates == B_TRUE);
					//pControl->UpdateStateButtons();
					if (pChild->m_pNumbererDescription)
					{
						CStateCtrlObj* pStateButton = pControl->GetStateCtrl(pDataObj);
						if (pStateButton)
						{
							pStateButton->EnableStateInEditMode(pChild->m_pStateData->m_bEnableStateInEdit);
							pStateButton->EnableCtrlInEditMode(pChild->m_pStateData->m_bEnableStateCtrlInEdit);
						}
					}
				}
			}

			if (pChild->m_pControlBehaviourDescription)
				pContext->AttachControlBehaviour(pChild->m_pControlBehaviourDescription, pControl);

			pParsedForm->m_pControlLinks->Add(pObject);

		}


		BuildWebControlLinks(pParsedForm, pContext, pChild);
	}
}
//-----------------------------------------------------------------------------
void CJsonFormEngine::BuildWebControlLinks(CParsedForm* pParsedForm, CJsonContextObj* pContextObj)
{
	CJsonContext* pContext = (CJsonContext*)pContextObj;

	CWndObjDescription* pWndDesc = pContext->m_pDescription;
	BuildWebControlLinks(pParsedForm, pContext, pWndDesc);
}
