
#include "stdafx.h"

#include <TBXmlCore\XMLSchema.h>
#include <TBXmlCore\XMLTags.h>

#include <TbParser\Parser.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\ParsObj.h>

#include "rpsymtbl.h"
#include "askdata.h"
#include "repengin.h"
#include "events.h"
#include "EdtMng.h"
#include "PrgData.h"
#include "ExportSymbols.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//===========================================================================
// AskFieldData
//===========================================================================
IMPLEMENT_DYNAMIC(AskFieldData, CObject);
//----------------------------------------------------------------------------
AskFieldData::AskFieldData(AskGroupData* pGroup)
	:
	m_pGroup				(pGroup),
	m_pReadOnlyExpr			(NULL),
	m_pCaptionExpr			(NULL),
	m_pWhenExpr				(NULL),
	m_bDynamicHidden		(FALSE),
	m_nInputLimit			(T_NOTOKEN),
	m_nCaptionPos			(T_DEFAULT),
	m_CtrlStyle				(AskFieldData::EDIT),
	m_bLeftBoolAlign		(FALSE),
	m_bLeftTextBool			(FALSE),
	m_bHiddenInput			(FALSE),
	m_bMultiSelectionCombo	(FALSE),
	m_bDescriptionCombo		(FALSE),
	m_bShowHotLinkDescription(FALSE)
{}

//----------------------------------------------------------------------------
AskFieldData::~AskFieldData()
{
	SAFE_DELETE (m_pReadOnlyExpr);
	SAFE_DELETE (m_pWhenExpr);
	SAFE_DELETE (m_pCaptionExpr);
}

//----------------------------------------------------------------------------
BOOL AskFieldData::Parse(Parser& lex, WoormTable& aSymTable)
{
	if (!lex.ParseID(m_strPublicName))		
		return FALSE;

	WoormField* pRepField =  aSymTable.GetField(m_strPublicName);
	if (!pRepField)	
		return lex.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), m_strPublicName);

	if (!pRepField->IsInput())	
		return lex.SetError(_TB("Illegal field"), m_strPublicName);

	pRepField->SetAsk(TRUE);

	ASSERT_VALID(this->m_pGroup);
	CWoormDoc* pWDoc = dynamic_cast<CWoormDoc*>(this->m_pGroup->m_pDocument ? this->m_pGroup->m_pDocument : aSymTable.GetDocument());
	if (pWDoc)
	{
		WoormField* pViewField = pWDoc->m_ViewSymbolTable.GetField(m_strPublicName);
		if (pViewField) 
			pViewField->SetAsk(TRUE);
	}

	if (AfxGetExportSymbols()->IsActivated())
		AfxGetExportSymbols()->ChangeItemTypeOnLastFile(m_strPublicName, EXP_SYMB_VAR_INPUT, EXP_SYMB_VAR_ASK);

	if (lex.Matched(T_HIDDEN))
	{
		m_bHiddenInput = TRUE;
		return lex.ParseSep();
	}

	int nStyle = AskFieldData::CHECK_BOX;
	if (pRepField->GetDataType() == DATA_BOOL_TYPE)
	{
		m_CtrlStyle = AskFieldData::CHECK_BOX;
		if	(lex.Matched(T_STYLE))
		{
			if	(
					!lex.ParseTag(T_ASSIGN)	||
					!lex.ParseInt(nStyle)
				)
				return FALSE;

			m_bLeftBoolAlign =	(nStyle & (AskFieldData::CHECK_BOX | AskFieldData::LEFT_ALIGN_DELTA)) == (AskFieldData::CHECK_BOX | AskFieldData::LEFT_ALIGN_DELTA) ||
								(nStyle & (AskFieldData::RADIO_BUTTON | AskFieldData::LEFT_ALIGN_DELTA)) == (AskFieldData::RADIO_BUTTON | AskFieldData::LEFT_ALIGN_DELTA);

			m_bLeftTextBool =	(nStyle & (AskFieldData::CHECK_BOX | AskFieldData::LEFT_TEXT_DELTA)) == (AskFieldData::CHECK_BOX | AskFieldData::LEFT_TEXT_DELTA) ||
								(nStyle & (AskFieldData::RADIO_BUTTON | AskFieldData::LEFT_TEXT_DELTA)) == (AskFieldData::RADIO_BUTTON | AskFieldData::LEFT_TEXT_DELTA);

			m_CtrlStyle = (AskFieldData::CtrlStyle) (nStyle & ~(AskFieldData::LEFT_ALIGN_DELTA | AskFieldData::LEFT_TEXT_DELTA));

			switch (m_CtrlStyle)
			{
				case AskFieldData::CHECK_BOX:
				case AskFieldData::RADIO_BUTTON:
					// ignora eventuali precedenti settaggi
					m_nCaptionPos = T_DEFAULT;
					break; // tutto ok

				case AskFieldData::EDIT:
					break; // tutto ok

				default						:
					return lex.SetError(Expression::FormatMessage (Expression::SYNTAX_ERROR));
			}
		}
	}
	else
	{
		nStyle = AskFieldData::EDIT;
		m_CtrlStyle = AskFieldData::EDIT;

		if	(lex.Matched(T_STYLE))
		{
			if (!lex.ParseTag(T_ASSIGN) || !lex.ParseInt(nStyle))
				return FALSE;

			m_CtrlStyle = (AskFieldData::CtrlStyle) nStyle;
			if (m_CtrlStyle != AskFieldData::EDIT && m_CtrlStyle != AskFieldData::COMBO_BOX  && m_CtrlStyle != AskFieldData::EDIT_HKL)
			{
				lex.SetError(_TB("Only Edit or Combo styles allowed here."));
				return FALSE;
			}
		}
	}

	CString sTemp;
	switch (lex.LookAhead())
	{
		case T_LEFT	:
		case T_TOP	:
			m_nCaptionPos = lex.SkipToken();
			break;
	}

	if (!lex.ParseTag(T_PROMPT))			return FALSE;
	if (!lex.ParseTag(T_ASSIGN))			return FALSE;

	if (lex.Matched(T_ROUNDOPEN))
	{
		m_pCaptionExpr = new Expression(&aSymTable);
		m_pCaptionExpr->SetStopTokens(T_ROUNDCLOSE);
		if (!m_pCaptionExpr->Parse(lex, DATA_STR_TYPE, TRUE))
		{
			delete m_pCaptionExpr;
			return FALSE;
		}
		lex.Match(T_ROUNDCLOSE);
	}
	else
	{
		if (!lex.ParseString(m_strCaption))	
			return FALSE;
	}

	if (pRepField->GetDataType() != DATA_BOOL_TYPE)
	{
		if (lex.Matched(T_LOWER_LIMIT))
		{
			m_nInputLimit = T_LOWER_LIMIT;
			pRepField->SetLowerLimit(TRUE);
		}
		else
			if (lex.Matched(T_UPPER_LIMIT))
			{
				m_nInputLimit = T_UPPER_LIMIT;
				pRepField->SetUpperLimit(TRUE);
			}

	}

	if (lex.Matched(T_READ_ONLY))
	{
		lex.Matched(T_ASSIGN);

		if (m_pReadOnlyExpr)
			delete m_pReadOnlyExpr;

		m_pReadOnlyExpr = new Expression(&aSymTable);
		m_pReadOnlyExpr->SetStopTokens(T_HOTLINK, T_DYNAMIC, T_WHEN, T_SEP);
		if (!m_pReadOnlyExpr->Parse(lex, DATA_BOOL_TYPE, TRUE))
			return FALSE;
	}
	
	m_bDynamicHidden = lex.Matched(T_DYNAMIC);
	if (m_bDynamicHidden && !lex.Match(T_WHEN))
		return FALSE;

	if (m_bDynamicHidden || lex.Matched(T_WHEN))
	{
		SAFE_DELETE(m_pWhenExpr);

		m_pWhenExpr = new Expression(&aSymTable);
		m_pWhenExpr->SetStopTokens(T_HOTLINK, T_SEP);
		if (!m_pWhenExpr->Parse(lex, DATA_BOOL_TYPE, TRUE))
		{
			SAFE_DELETE(m_pWhenExpr);
			return FALSE;
		}
	}

	if (lex.Matched(T_HOTLINK) && !ParseHotlink(lex, aSymTable,  m_nsHotLink))
		return FALSE;

	// se hotlink già presente ignoro il datasource, se c'è solo il 
	// datasource, provo a trasformarlo in hkl (compatibilità vecchie release)
	if (lex.Matched(T_DATASOURCE))
	{
		CTBNamespace nsDataSouce;
		ParseHotlink(lex, aSymTable,  nsDataSouce);
		if (!m_nsHotLink.IsValid() && nsDataSouce.IsValid())
		{
			m_nsHotLink = nsDataSouce;
			m_nsHotLink.SetType(CTBNamespace::HOTLINK);
		}

		m_CtrlStyle = AskFieldData::COMBO_BOX;
	}

	if(m_nsHotLink.IsValid() && m_CtrlStyle != AskFieldData::COMBO_BOX && m_CtrlStyle == AskFieldData::EDIT_HKL)
		return FALSE;

	return lex.ParseSep();
}

//----------------------------------------------------------------------------
BOOL AskFieldData::ParseHotlink (Parser& lex, WoormTable& aSymTable, CTBNamespace& aNs)
{
	CString sTemp;
	CTBNamespace	aModuleNs			= AfxGetPathFinder()->GetNamespaceFromPath(lex.GetFileName());
	AddOnModule*	pAddOnMod			= NULL;
	CTBNamespace::NSObjectType aType	= CTBNamespace::HOTLINK;

	SymField*	pRepField				= aSymTable.GetField(m_strPublicName);
	if (!pRepField)
		return lex.SetError(m_strPublicName + _TB(" Unknown"));

	// parsing delle virgolette e dell = (compatibilità vecchie release)
	if (lex.LookAhead(T_ASSIGN))
		if (!lex.ParseTag(T_ASSIGN))	
			return FALSE;
	if (
			(lex.LookAhead(T_ID) && lex.ParseID(sTemp)) ||
			lex.ParseString(sTemp)
		)
		aNs.AutoCompleteNamespace (aType, sTemp, aModuleNs);
	else
		return FALSE;

	pAddOnMod  = AfxGetAddOnModule(aNs);

	if (!pAddOnMod)
		return lex.SetError(aNs.ToString() + _TB(" Unknown"));

	CFunctionDescription* pFun	= (CFunctionDescription*) pAddOnMod->m_XmlDescription.GetParamObjectInfo(aNs);

	if (!pFun)
		return lex.SetError(aNs.ToString() + _TB(" Unknown"));

	BOOL bOk = ParseHotlinkParam(lex, pFun, aSymTable, TRUE);

	m_bMultiSelectionCombo = lex.Matched(T_MULTI_SELECTIONS);

	if (m_bMultiSelectionCombo)
	{
		if (pRepField->GetDataType() != DataType::String)
			return lex.SetError(aNs.ToString() + _TB("Mutiselections HotLink have to bind to string field"));
	}
	else if (pRepField->GetDataType() != pFun->GetReturnValueDescription().GetDataType())
		return lex.SetError(aNs.ToString() + _TB(" Incompatible"));

	return bOk;
}

// viene usata anche dalla AskFieldDlg
//----------------------------------------------------------------------------
BOOL AskFieldData::ParseHotlinkParam
	(
		Parser&					lex,
		CFunctionDescription*	pFun,
		WoormTable&				aSymTable,
		BOOL					bFromFile
	)
{
	m_HotLinkParamsExpr.RemoveAll();

	if (pFun->GetParameters().GetSize() == 0)
	{
		// e` accettata ()
		if (bFromFile && lex.Matched(T_SQUAREOPEN))
			return lex.ParseTag(T_SQUARECLOSE);

		if (bFromFile && lex.Matched(T_ROUNDOPEN))
			return lex.ParseTag(T_ROUNDCLOSE);

		return TRUE;
	}

	// parse dei parametri di personalizzazione dell'hotlink
	if (bFromFile && !(lex.LookAhead(T_SQUAREOPEN) ? lex.ParseTag(T_SQUAREOPEN) : lex.ParseTag(T_ROUNDOPEN)))
		return FALSE;

	int nLastParam = pFun->GetParameters().GetUpperBound();
	for (int p = 0; p <= nLastParam; p++)
	{
		Expression* pParamExpr = new Expression(&aSymTable);
		pParamExpr->SetStopTokens(T_SQUARECLOSE, T_ROUNDCLOSE, T_COMMA, T_SEP);
		if (!pParamExpr->Parse(lex, pFun->GetParamDescription(p)->GetDataType(), TRUE))
		{
			delete pParamExpr;
			pParamExpr = NULL;

			if (lex.GetError() != Expression::FormatMessage (Expression::EXPREMPTY))
				return FALSE;
			
			lex.ClearError();

			lex.SetError
				(
					Expression::FormatMessage (Expression::EXPREMPTY),
					cwsprintf
					(
						_T("Param.: %s"),
						(LPCTSTR) (pFun->GetParamDescription(p)->GetName())
					)
				);
			return FALSE;
		}

		m_HotLinkParamsExpr.Add(pParamExpr);

		if (p < nLastParam && !lex.Matched(T_COMMA) && !lex.Matched(T_SEP))
			return FALSE;
	}

	return !bFromFile || (lex.LookAhead(T_SQUARECLOSE) ? lex.ParseTag(T_SQUARECLOSE) : lex.ParseTag(T_ROUNDCLOSE));
}

//----------------------------------------------------------------------------
void AskFieldData::Unparse(Unparser& oFile, AskGroupData& aContainer)
{
	oFile.UnparseID		(m_strPublicName,	FALSE);

	if (m_bHiddenInput)
	{
		oFile.UnparseTag	(T_HIDDEN, FALSE);
		oFile.UnparseSep	(TRUE);
		return;
	}

	int nStyle = m_CtrlStyle;
	
	if (m_bLeftBoolAlign)
		nStyle |= AskFieldData::LEFT_ALIGN_DELTA;

	if (m_bLeftTextBool)
		nStyle |= AskFieldData::LEFT_TEXT_DELTA;

	SymField* pField = aContainer.m_SymTable.GetField(m_strPublicName);
	if (pField && pField->GetDataType() != DATA_BOOL_TYPE && (nStyle == AskFieldData::EDIT || nStyle == AskFieldData::EDIT_HKL))
		nStyle = 0;		// default

	if (nStyle > 0)
	{
		oFile.UnparseTag	(T_STYLE,		FALSE);
		oFile.UnparseTag	(T_ASSIGN,		FALSE);
		oFile.UnparseInt	(nStyle,	FALSE);
		oFile.UnparseBlank	(FALSE);
	}

	if (m_nCaptionPos != T_DEFAULT)
		oFile.UnparseTag(m_nCaptionPos, FALSE);

	oFile.UnparseTag	(T_PROMPT,			FALSE);
	oFile.UnparseTag	(T_ASSIGN,			FALSE);

	if (m_pCaptionExpr)
	{
		oFile.UnparseTag	(T_ROUNDOPEN,	FALSE);
		oFile.UnparseExpr	(m_pCaptionExpr->ToString(), FALSE);
		oFile.UnparseTag	(T_ROUNDCLOSE,	FALSE);
	}
	else
		oFile.UnparseString	(oFile.IsLocalizableTextInCurrentLanguage() ? oFile.LoadReportString(m_strCaption) : m_strCaption, FALSE);	

	if (m_nInputLimit != T_NOTOKEN)
		oFile.UnparseTag(m_nInputLimit, FALSE);
	
	if (m_pReadOnlyExpr)
	{
		oFile.UnparseTag	(T_READ_ONLY,	FALSE);
		oFile.UnparseExpr	(m_pReadOnlyExpr->ToString(), FALSE);
	}
	if (m_pWhenExpr)
	{
		if (m_bDynamicHidden) 
			oFile.UnparseTag	(T_DYNAMIC,	FALSE);
		oFile.UnparseTag	(T_WHEN,	FALSE);
		oFile.UnparseExpr	(m_pWhenExpr->ToString(), FALSE);
	}

	UnparseHotlink(oFile);

	oFile.UnparseSep	(TRUE);
}

//----------------------------------------------------------------------------
void AskFieldData::GetSchema(CXSDGenerator* pSchema, WoormTable& rst)
{
	SymField* pField = rst.GetField(m_strPublicName);
	ASSERT(pField);
	if (!pField)
		return;

	CString strType = pField->GetData()->GetXMLType(FALSE);

	pSchema->BeginSimpleElement(m_strPublicName, strType, _T("1"), _T("1")); 

		pSchema->InsertAttribute(XML_TYPE_ATTRIBUTE,	SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T(""), ::FromTBTypeToNetType(pField->GetData()->GetDataType()));
		
		if (m_pCaptionExpr)
		{
			pSchema->InsertAttribute(XML_TITLE_ATTRIBUTE,	SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T(""), m_pCaptionExpr->ToString());
		}
		else 
			pSchema->InsertAttribute(XML_TITLE_ATTRIBUTE,	SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T(""), m_strCaption);
		
		pSchema->InsertAttribute(_T("length"),			SCHEMA_XSD_DATATYPE_INT_VALUE,		_T(""), cwsprintf(_T("%d"), pField->GetLen()));

		if (m_pReadOnlyExpr)
		{
			pSchema->InsertAttribute(_T("readonly"),	SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T(""), m_pReadOnlyExpr->ToString());
		}
		
		if (m_nInputLimit == T_UPPER_LIMIT)
			pSchema->InsertAttribute(_T("inputLimit"),	SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T(""), _T("Upper"));
		else if (m_nInputLimit == T_LOWER_LIMIT)
			pSchema->InsertAttribute(_T("inputLimit"),	SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T(""), _T("Lower"));

		if (!m_nsHotLink.IsEmpty() && m_nsHotLink.IsValid())
			pSchema->InsertAttribute(_T("hotlink"), SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), m_nsHotLink.ToUnparsedString());
		CString sCtrlType;
		switch (m_CtrlStyle)
		{
		case CHECK_BOX:
			sCtrlType = _T("Check");
			break;
		case RADIO_BUTTON:
			sCtrlType = _T("Radio");
			break;
		case COMBO_BOX:
			sCtrlType = _T("Combo");
			break;
		default:
			sCtrlType = _T("Text");
		}
		pSchema->InsertAttribute(_T("controlType"),	SCHEMA_XSD_DATATYPE_STRING_VALUE,	_T(""), sCtrlType);

		pSchema->EndSimpleElement();
}

//----------------------------------------------------------------------------
void AskFieldData::GetXmlParameters (CXMLNode* pParentNode, WoormTable& rst)
{
	CXMLNode* pNode = pParentNode->CreateNewChild(m_strPublicName);

	SymField* pField = rst.GetField(m_strPublicName);
	ASSERT(pField);
	if (!pField)
		return;

	CString strValue = pField->GetData()->FormatDataForXML(pField->GetData()->GetDataType() != DATA_DATE_TYPE);
	pNode->SetText(strValue);

	pNode->SetAttribute(XML_TYPE_ATTRIBUTE,		::FromTBTypeToNetType(pField->GetData()->GetDataType()));

	if (m_pCaptionExpr)
	{
		pNode->SetAttribute(XML_TITLE_ATTRIBUTE, m_pCaptionExpr->ToString());
	}
	else
		pNode->SetAttribute(XML_TITLE_ATTRIBUTE,	m_strCaption);
	
	if (m_pReadOnlyExpr)
	{
		pNode->SetAttribute(_T("readonly"), m_pReadOnlyExpr->ToString());
	}
}


//----------------------------------------------------------------------------
void AskFieldData::AssignFromXml(CXMLNode* pParentNode, WoormTable& rst)
{
	CXMLNode* pFieldNode = pParentNode->GetChildByName(m_strPublicName);
	if (!pFieldNode)
		return;

	SymField* pField = rst.GetField(m_strPublicName);
	ASSERT(pField);
	if (!pField)
		return;

	CString strValue;
	pFieldNode->GetText(strValue);
	pField->GetData()->AssignFromXMLString(strValue);
}

//----------------------------------------------------------------------------
void AskFieldData::UnparseHotlink(Unparser& oFile)
{
	if (!m_nsHotLink.IsValid())	return;

	oFile.UnparseTag	(T_HOTLINK, FALSE);

	oFile.UnparseID	(m_nsHotLink.ToUnparsedString(), FALSE);

	// le parentesi diventano obbligatorie
	oFile.UnparseTag (T_ROUNDOPEN, FALSE);

	// unparse dei parametri di personalizzazione dell'hotlink
	CString sExpr;
	if (m_HotLinkParamsExpr.GetSize() > 0)
		for (int p = 0; p <= m_HotLinkParamsExpr.GetUpperBound(); p++)
		{
			if (m_HotLinkParamsExpr[p])
			{
				sExpr = ((Expression*)m_HotLinkParamsExpr[p])->ToString();
				sExpr.Trim();
				sExpr.Replace(_T("\n\t"), _T(""));
				oFile.UnparseExpr(sExpr, FALSE);
			}

			if (p < m_HotLinkParamsExpr.GetUpperBound())
				oFile.UnparseTag(T_COMMA, FALSE);
		}

	oFile.UnparseTag (T_ROUNDCLOSE, FALSE);

	if (m_bMultiSelectionCombo)
		oFile.UnparseTag (T_MULTI_SELECTIONS, FALSE);
}

//----------------------------------------------------------------------------
BOOL AskFieldData::CanDelete()
{
	CWoormDoc* pWDoc = dynamic_cast<CWoormDoc*>(this->m_pGroup->m_pDocument);
	if (!pWDoc)
		return TRUE;
	EditorManager* pEM = pWDoc->GetEditorManager();
	if (!pEM)
		return TRUE;

	CString sLog;
	BOOL bCan = pEM->GetPrgData()->CanDeleteField(m_strPublicName, sLog);
	if (!bCan)
	{
		CString msg = cwsprintf(_TB("The field {0-%s} cannot be deleted because it is still in use."), m_strPublicName);
		if (!sLog.IsEmpty())
			msg += '\n' + sLog;
		AfxTBMessageBox(msg, MB_OK | MB_ICONEXCLAMATION);
		return FALSE;
	}

	return TRUE;
}

//===========================================================================
// AskGroupData
//===========================================================================
IMPLEMENT_DYNAMIC(AskGroupData, CObject);

//----------------------------------------------------------------------------
AskGroupData::AskGroupData(WoormTable& aSymTable, CBaseDocument* pDocument)
	:
	m_SymTable			(aSymTable),
	m_bHiddenTitle		(FALSE),
	m_nFieldsCaptionPos	(T_DEFAULT),
	m_pDocument			(pDocument),
	m_pWhenExpr			(NULL)
{}

AskGroupData::~AskGroupData() { SAFE_DELETE(m_pWhenExpr); }

//----------------------------------------------------------------------------
AskFieldData* AskGroupData::GetAskField(LPCTSTR pszFieldName)
{
	for (int nIdx = 0; nIdx < m_AskFields.GetSize(); nIdx++)
	{
		AskFieldData* pAskField = (AskFieldData*) m_AskFields[nIdx];

		if (_tcsicmp( pszFieldName, pAskField->m_strPublicName) == 0)
            return pAskField;
	}
	
	return NULL;
}

//----------------------------------------------------------------------------
BOOL AskGroupData::CanDeleteField(LPCTSTR pszFieldName)
{
	if (this->m_pWhenExpr && m_pWhenExpr->HasMember(pszFieldName))
		return FALSE;

	AskFieldData* pAskFieldData = GetAskField(pszFieldName);

	if (pAskFieldData->m_pCaptionExpr && pAskFieldData->m_pCaptionExpr->HasMember(pszFieldName))
		return FALSE;
	if (pAskFieldData->m_pReadOnlyExpr && pAskFieldData->m_pReadOnlyExpr->HasMember(pszFieldName))
		return FALSE;
	if (pAskFieldData->m_pWhenExpr && pAskFieldData->m_pWhenExpr->HasMember(pszFieldName))
		return FALSE;

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL AskGroupData::IsEmpty()
{
	return (m_AskFields.GetSize() == 0);
}

//----------------------------------------------------------------------------
 int AskGroupData::AddAskField(AskFieldData* pF)
{
	ASSERT_VALID(pF);
	return m_AskFields.Add(pF);
}

//----------------------------------------------------------------------------
AskFieldData* AskGroupData::GetAskField(int nPos)
{
	if (nPos > m_AskFields.GetUpperBound() || nPos < 0)	return NULL;
	return (AskFieldData*) m_AskFields[nPos];
}

//----------------------------------------------------------------------------
void AskGroupData::RenameField(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName)
{
	for (int nIdx = 0; nIdx < m_AskFields.GetSize(); nIdx++)
	{
		AskFieldData* pAskField = (AskFieldData*)m_AskFields[nIdx];

		if (_tcsicmp(pszOldFieldName, pAskField->m_strPublicName) == 0)
		{
			pAskField->m_strPublicName = pszNewFieldName;
		}
	}
}

//----------------------------------------------------------------------------
BOOL AskGroupData::CanDelete()
{
	for (int i = 0; i < m_AskFields.GetSize(); i++)
	{
		AskFieldData* pAskField = (AskFieldData*)m_AskFields[i];

		if (!pAskField->CanDelete())
			return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL AskGroupData::DeleteField(LPCTSTR pszFieldName)
{
	for (int nIdx = 0; nIdx < m_AskFields.GetSize(); nIdx++)
	{
		AskFieldData* pAskField = (AskFieldData*)m_AskFields[nIdx];

		if (_tcsicmp(pszFieldName, pAskField->m_strPublicName) == 0)
		{
			DelAskField(nIdx, TRUE);
			return TRUE;
		}
	}

	return FALSE;
}

//----------------------------------------------------------------------------
void AskGroupData::DelAskField(int nPos, BOOL bDeleteField)
{
	AskFieldData* askFld = GetAskField(nPos);
	
	if (askFld)
	{
		ProgramData* pPrgData = m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)) ?
								((CWoormDoc*)m_pDocument)->GetEditorManager()->GetPrgData()
								: NULL;
		CString sLog;

		CString sName = askFld->m_strPublicName;

		m_AskFields.RemoveAt(nPos);

		if (bDeleteField && pPrgData && pPrgData->CanDeleteField(sName, sLog))
		{
			pPrgData->DeleteField(sName);
		}
		else
		{
			WoormField* pField = m_SymTable.GetField(sName);
			pField->SetAsk(FALSE); pField->SetHidden(TRUE); 

			if (m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
				((CWoormDoc*)m_pDocument)->SyncronizeViewSymbolTable(pField);
		}
	}
}

//----------------------------------------------------------------------------
void AskGroupData::DelAllAskField(BOOL bDeleteFields)
{
	for (int i = m_AskFields.GetUpperBound(); i >= 0; i--)
		DelAskField(i, bDeleteFields);
}

//----------------------------------------------------------------------------
CString	AskGroupData::GetEmptyTitle()
{
	return _TB("Group Without Title");
}

//----------------------------------------------------------------------------
BOOL AskGroupData::Parse(Parser& lex)
{
	m_strTitle.Empty();
	if (lex.LookAhead() == T_STR && !lex.ParseString(m_strTitle))	
		return FALSE;

	m_strTitle.TrimRight();
	if (m_strTitle.IsEmpty())
		m_strTitle = AskGroupData::GetEmptyTitle();
		
	//controlla visibilità solo della GroupBox NON dei control contenuti
	m_bHiddenTitle = lex.Matched(T_HIDDEN); 
	//controlla visibilità dei control contenuti
	if (lex.Matched(T_WHEN))
	{
		if (m_pWhenExpr)
			delete m_pWhenExpr;

		m_pWhenExpr = new Expression(&m_SymTable);
		m_pWhenExpr->SetStopTokens(T_SEP);
		if (!m_pWhenExpr->Parse(lex, DATA_BOOL_TYPE, TRUE))
			return FALSE;
		if (!lex.Match(T_SEP))
			return FALSE;
	}

	switch (lex.LookAhead())
	{
		case T_LEFT	:
		case T_TOP	:
			m_nFieldsCaptionPos = lex.SkipToken();
			if (!lex.ParseTag(T_PROMPT)) return FALSE;
			break;
	}

	while (!lex.LookAhead(T_END) && !lex.Bad())
	{
		AskFieldData* pAskField = new AskFieldData(this);
		if (!pAskField->Parse(lex, m_SymTable))
		{
			delete pAskField;
            return FALSE;
		}
        m_AskFields.Add( pAskField );
	}

	return (lex.ParseEnd());
}

//----------------------------------------------------------------------------
void AskGroupData::Unparse(Unparser& oFile, AskDialogData& aContainer)
{
	if (m_AskFields.GetSize() == 0)
		return;

	oFile.UnparseBegin(FALSE);
	oFile.UnparseBlank();

	CString strDefaultTitle = AskGroupData::GetEmptyTitle();
	if (m_strTitle.Find(strDefaultTitle) == -1)
		oFile.UnparseString(oFile.IsLocalizableTextInCurrentLanguage() ? oFile.LoadReportString(m_strTitle) : m_strTitle, FALSE);
		
	oFile.UnparseBlank();

	if (m_bHiddenTitle)
		oFile.UnparseTag (T_HIDDEN, FALSE);

	if (m_pWhenExpr)
	{
		oFile.UnparseTag (T_WHEN, FALSE);
		oFile.UnparseExpr	(m_pWhenExpr->ToString(), FALSE);
		oFile.UnparseTag (T_SEP, FALSE);
	}
	if (m_nFieldsCaptionPos != T_DEFAULT)
	{
		oFile.UnparseTag(m_nFieldsCaptionPos, FALSE);
		oFile.UnparseTag(T_PROMPT, FALSE);
	}

	oFile.UnparseCrLf();

	for (int i = 0; i < m_AskFields.GetSize(); i++)
	{
		AskFieldData* pAskField = (AskFieldData*) m_AskFields[i];
		pAskField->Unparse(oFile, *this);
	}

	oFile.UnparseEnd(TRUE);
}

//----------------------------------------------------------------------------
void AskGroupData::GetSchema(CXSDGenerator* pSchema, int nGroup)
{
	CString strName;
	strName.Format(_T("Group%d"), nGroup);

	pSchema->BeginComplexElement(strName, _T("1"), _T("1"));
	pSchema->InsertAttribute(XML_TITLE_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), m_bHiddenTitle ? _T("") : m_strTitle);

	for (int i = 0; i < m_AskFields.GetSize(); i++)
	{
		AskFieldData* pAskField = (AskFieldData*) m_AskFields[i];

		pAskField->GetSchema(pSchema, m_SymTable);
	}

	pSchema->EndComplexElement();
}

//----------------------------------------------------------------------------
void AskGroupData::GetXmlParameters (CXMLNode* pParentNode, int nGroup)
{
	CString strName;
	strName.Format(_T("Group%d"), nGroup);

	CXMLNode* pNode = pParentNode->CreateNewChild(strName);
	pNode->SetAttribute(XML_TITLE_ATTRIBUTE, m_strTitle);

	for (int i = 0; i < m_AskFields.GetSize(); i++)
	{
		AskFieldData* pAskField = (AskFieldData*) m_AskFields[i];

		pAskField->GetXmlParameters(pNode, m_SymTable);
	}
}

//----------------------------------------------------------------------------
void AskGroupData::AssignFromXml(CXMLNode* pParentNode, int nGroup)
{
	CString strGroupName;
	strGroupName.Format(_T("Group%d"), nGroup + 1);
	CXMLNode* pGroupNode = pParentNode->GetChildByName(strGroupName);
	if (!pGroupNode)
		return;

	for (int i = 0; i < m_AskFields.GetSize(); i++)
	{
		AskFieldData* pAskField = (AskFieldData*) m_AskFields[i];
		if (pAskField)
			pAskField->AssignFromXml(pGroupNode, m_SymTable);
	}
}

//===========================================================================
// AskDialogData
//===========================================================================
IMPLEMENT_DYNAMIC(AskDialogData, CObject);

//----------------------------------------------------------------------------
AskDialogData::AskDialogData(WoormTable& aSymTable, CBaseDocument* pDocument)
	:
    m_SymTable			(aSymTable),
	m_nFieldsCaptionPos	(T_DEFAULT),
	m_pDocument			(pDocument),
	m_pWhenExpr			(NULL),
	m_pOnExpr			(NULL),
	m_pAbortExpr		(NULL),
	m_pBeforeBlock		(NULL),
	m_pAfterBlock		(NULL),
	m_bIsOnAsk			(FALSE)
{}

//-----------------------------------------------------------------------------
AskDialogData::~AskDialogData()
{
	SAFE_DELETE(m_pWhenExpr);
	SAFE_DELETE(m_pOnExpr);
	SAFE_DELETE(m_pAbortExpr);
	SAFE_DELETE(m_pBeforeBlock);
	SAFE_DELETE(m_pAfterBlock);
}

//----------------------------------------------------------------------------
CString	AskDialogData::GetEmptyTitle()
{
	return _TB("Dialog Without Title");
}
//----------------------------------------------------------------------------
AskFieldData* AskDialogData::GetAskField(LPCTSTR pszFieldName)
{
	for (int i = 0; i < m_AskGroups.GetSize(); i++)
	{
		AskFieldData* pAskField = ((AskGroupData*) m_AskGroups[i])->GetAskField( pszFieldName );

		if (pAskField)
			return pAskField;
	}

    return NULL;
}

//----------------------------------------------------------------------------
BOOL AskDialogData::CanDeleteField(LPCTSTR pszFieldName)
{
	if (this->m_pWhenExpr && m_pWhenExpr->HasMember(pszFieldName))
		return FALSE;
	if (this->m_pOnExpr && m_pOnExpr->HasMember(pszFieldName))
		return FALSE;
	if (this->m_pAbortExpr && m_pAbortExpr->HasMember(pszFieldName))
		return FALSE;

	if (this->m_pBeforeBlock && m_pBeforeBlock->HasMember(pszFieldName))
		return FALSE;
	if (this->m_pAfterBlock && m_pAfterBlock->HasMember(pszFieldName))
		return FALSE;

	for (int i = 0; i < m_AskGroups.GetSize(); i++)
	{
		AskGroupData* pAskGroupData = (AskGroupData*) m_AskGroups[i];
		if (pAskGroupData->GetAskField(pszFieldName))
			return pAskGroupData->CanDeleteField( pszFieldName );
	}

    return TRUE;
}

//----------------------------------------------------------------------------

void AskDialogData::RenameField(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName)
{
	for (int nIdx = 0; nIdx < m_AskGroups.GetSize(); nIdx++)
	{
		AskGroupData* pAskGroup = (AskGroupData*)m_AskGroups[nIdx];

		pAskGroup->RenameField(pszOldFieldName, pszNewFieldName);
	}
}

//----------------------------------------------------------------------------
BOOL AskDialogData::DeleteField(LPCTSTR pszFieldName)
{
	for (int nIdx = 0; nIdx < m_AskGroups.GetSize(); nIdx++)
	{
		AskGroupData* pAskGroup = (AskGroupData*) m_AskGroups[nIdx];

		if (pAskGroup->DeleteField( pszFieldName ))
		{
			if (pAskGroup->IsEmpty())
				m_AskGroups.RemoveAt( nIdx );
			
			return TRUE;
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
BOOL AskDialogData::CanDelete()
{
	for (int i = 0; i < m_AskGroups.GetSize(); i++)
	{
		AskGroupData* pAskGroup = (AskGroupData*)m_AskGroups[i];

		if (!pAskGroup->CanDelete())
			return FALSE;
	}
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL AskDialogData::IsEmpty()
{
	for (int i = 0; i < m_AskGroups.GetSize(); i++)
	{
		AskGroupData* pAskGroupData = (AskGroupData*) m_AskGroups[i];
		if (!pAskGroupData->IsEmpty())
			return FALSE;
	}
    return TRUE;
}

//----------------------------------------------------------------------------
AskGroupData* AskDialogData::GetAskGroup(int nPos)
{
	if (nPos > m_AskGroups.GetUpperBound() || nPos < 0) return NULL;
	return (AskGroupData*) m_AskGroups[nPos];
}

//----------------------------------------------------------------------------
int AskDialogData::AddAskGroup(AskGroupData* pGroup)
{
	return m_AskGroups.Add(pGroup);
}

//----------------------------------------------------------------------------
void AskDialogData::DelAskGroup(int nPos, BOOL bDeleteFields)
{
	AskGroupData* pAskGroup = GetAskGroup(nPos);
	
	if (pAskGroup)
	{
		pAskGroup->DelAllAskField(bDeleteFields);

		m_AskGroups.RemoveAt(nPos);
	}
}

//----------------------------------------------------------------------------
void AskDialogData::DelAllAskGroup(BOOL bDeleteFields)
{
	for (int i = m_AskGroups.GetUpperBound(); i >= 0 ; i--)
		DelAskGroup(i, bDeleteFields);
}

//----------------------------------------------------------------------------
BOOL AskDialogData::Parse(Parser& lex, ReportEngine* pReportEngine, AskRuleData* pAskRuleData)
{
	lex.GetCommentTrace(m_arCommentTraceBefore);

	if (!lex.ParseTag(T_DIALOG))
	{
		lex.SetError(_TB("'Dialog' token expected"));
		return FALSE;
	}

	if (lex.Matched(T_ON))
	{
		if (!lex.ParseTag(T_ASK)) 
		{
			lex.SetError(_TB("'Ask' token expected"));
			return FALSE;
		}

		m_bIsOnAsk = TRUE;
	}
	
	m_strName.Empty();
	if (!lex.ParseID(m_strName))
	{
		lex.SetError(_TB("Dialog window name expected"));
		return FALSE;
	}

	BOOL bExistsDlg = pAskRuleData->ExistDialog(m_strName);
	if (bExistsDlg)
	{
		if (m_bIsOnAsk)
		{
			lex.SetError(_TB("Invokable dialog box name already defined!"), m_strName);
			return FALSE;
		}

		// if the name already exists set an unique dialog name	
		int i = 0;
		CString strTemp = m_strName;
		while (bExistsDlg)
		{
			m_strName.Format(_T("%s%d"), strTemp, ++i);
			bExistsDlg = pAskRuleData->ExistDialog(m_strName);
		}
	}

	m_strTitle.Empty();
	if (lex.LookAhead() == T_STR && !lex.ParseString(m_strTitle))
	{
		lex.SetError(_TB("Dialog window title expected"));
		return FALSE;
	}

	m_strTitle.TrimRight();
	if (m_strTitle.IsEmpty())
		m_strTitle = GetEmptyTitle();

	switch (lex.LookAhead())
	{
		case T_LEFT	:
		case T_TOP	:
			m_nFieldsCaptionPos = lex.SkipToken();
			if (!lex.ParseTag(T_PROMPT))
			{
				lex.SetError(_TB("'Prompt' token expected"));
				return FALSE;
			}
			break;
	}

	BOOL bBeginConsumed = lex.Matched(T_BEGIN);

	//Before block
	if (lex.Matched(T_BEFORE))
	{
		m_pBeforeBlock = new Block(NULL, &m_SymTable, pReportEngine ? pReportEngine->GetEngine() : NULL, FALSE);

		if (!m_pBeforeBlock->Parse(lex))
			return FALSE;

		m_pBeforeBlock->m_strOwnerName = L"Dialog." + m_strName + L".Before";
	}

	// eventuale espressione di WHEN che abilita il run della dialog
	if (lex.Matched(T_WHEN))
	{
		m_pWhenExpr = new Expression(&m_SymTable);
		m_pWhenExpr->SetStopTokens(T_CONTROLS, T_BEGIN);

        if (!m_pWhenExpr->Parse(lex, DATA_BOOL_TYPE, TRUE)) // <---- KeepString = TRUE : nella versione precedente dell'engine era FALSE
            return FALSE;
	}

	BOOL bVersion4 = lex.Matched(T_CONTROLS);
	if (bVersion4)
	{
		if (!lex.Match(T_BEGIN)) //begin Groups list
		{
			lex.SetError(_TB("'Begin' token expected"));
			return FALSE;
		}

		bBeginConsumed = FALSE;
	}

	while ( (bBeginConsumed || lex.LookAhead(T_BEGIN)) && !lex.Bad())
    {
		if (!bBeginConsumed && !lex.ParseBegin ())			
			return FALSE;

		bBeginConsumed = FALSE;

		AskGroupData* pAskGroup = new AskGroupData(m_SymTable, m_pDocument);
		if (!pAskGroup->Parse(lex))
		{
			delete pAskGroup;
			return FALSE;
		}

		m_AskGroups.Add( pAskGroup );
	}

	if (bVersion4)
	{
		if (!lex.Matched(T_END)) //end CONTROLS (groups list)
		{
			lex.SetError(_TB("'End' token expected"));
			return FALSE;
		}

		//After block
		if (lex.Matched(T_AFTER))
		{
			m_pAfterBlock = new Block(NULL, &m_SymTable, pReportEngine ? pReportEngine->GetEngine() : NULL, FALSE);

			if (!m_pAfterBlock->Parse(lex))
				return FALSE;

			m_pAfterBlock->m_strOwnerName = L"Dialog." + m_strName + L".After";
		}

		// eventuale espressione di ON 
		if (lex.Matched(T_ON))
		{
			m_pOnExpr = new Expression(&m_SymTable);
			m_pOnExpr->SetStopTokens(T_ABORT);

			if (!m_pOnExpr->Parse(lex, DATA_BOOL_TYPE, TRUE))
				return FALSE;

			if (!lex.Match(T_ABORT))
				return FALSE;

			m_pAbortExpr = new Expression(&m_SymTable);
			m_pAbortExpr->SetStopTokens(T_END);

			if (!m_pAbortExpr->Parse(lex, DATA_STR_TYPE, TRUE))
				return FALSE;
		}

		if (!lex.Matched(T_END)) //end DIALOG
		{
			lex.SetError(_TB("'End' token expected"));
			return FALSE;
		}
	}
		
	lex.GetCommentTrace(m_arCommentTraceAfter);
    return TRUE;
}

//----------------------------------------------------------------------------
/*
    Dialog [On Ask] <dialog-name> “<dialog-title>” 
    Begin
        Before
            Begin
                <istruzioni>
            End
        When <BooleanExpression>
        Controls
            Begin
                Begin
					<group1>
                End
                Begin
					<group2>
                End
            End
        After
            Begin
				<istruzioni>
            End
       On <BooleanExpression>  Abort <StringExpression>
 	End

*/
void AskDialogData::Unparse(Unparser& oFile)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);
	if (IsEmpty())	
		return;

	oFile.UnparseTag	(T_DIALOG, FALSE);

	if (m_bIsOnAsk)
	{
		oFile.UnparseTag	(T_ON,		FALSE);
		oFile.UnparseTag	(T_ASK,		FALSE);		
	}

	oFile.UnparseID(m_strName, FALSE);

	CString strDefaultTitle = AskDialogData::GetEmptyTitle();
		
	oFile.UnparseBlank();

	if (m_strTitle.Find(strDefaultTitle) == -1)
		oFile.UnparseString(oFile.IsLocalizableTextInCurrentLanguage() ? oFile.LoadReportString(m_strTitle) : m_strTitle, FALSE);
			
	if (m_nFieldsCaptionPos != T_DEFAULT)
	{
		oFile.UnparseTag(m_nFieldsCaptionPos, FALSE);
		oFile.UnparseTag(T_PROMPT, FALSE);
	}
	oFile.UnparseCrLf();

	oFile.UnparseBegin ();

	if (!m_bIsOnAsk)
	{
		if (m_pBeforeBlock)
		{
			oFile.UnparseTag (T_BEFORE, FALSE);
			m_pBeforeBlock->Unparse(oFile);
		}

		// clausola When di abilitazione del run della dialog.
		if (m_pWhenExpr && !m_pWhenExpr->IsEmpty())
		{
			oFile.UnparseTag (T_WHEN, FALSE);
			oFile.UnparseExpr(m_pWhenExpr->ToString(), TRUE);
		}
	}

	oFile.UnparseTag (T_CONTROLS);		
	oFile.UnparseBegin (); oFile.DecTab();

		for (int i=0; i<m_AskGroups.GetSize(); i++)
		{
			AskGroupData* pAskGroup = (AskGroupData*) m_AskGroups[i];
			pAskGroup->Unparse(oFile, *this);
		}

	oFile.IncTab(); oFile.UnparseEnd (); 

	if (!m_bIsOnAsk)
	{
		if (m_pAfterBlock)
		{
			oFile.UnparseTag (T_AFTER, FALSE);
			m_pAfterBlock->Unparse(oFile);
		}

		// clausola Check di abilitazione del run della dialog.
		if (m_pOnExpr && !m_pOnExpr->IsEmpty())
		{		
			oFile.UnparseTag (T_ON, FALSE);
			oFile.UnparseExpr(m_pOnExpr->ToString(), FALSE);
			oFile.UnparseTag (T_ABORT, FALSE);
			if (m_pAbortExpr && !m_pAbortExpr->IsEmpty())
				oFile.UnparseExpr(m_pAbortExpr->ToString(), TRUE);
			else
				oFile.UnparseString(_TB("Request data are invalid"), TRUE);
		}
	}

	oFile.UnparseEnd ();
	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//----------------------------------------------------------------------------
void AskDialogData::GetSchema (CXSDGenerator* pSchema)
{
	pSchema->BeginComplexElement(m_strName, _T("1"), _T("1"));
	pSchema->InsertAttribute(XML_TITLE_ATTRIBUTE, SCHEMA_XSD_DATATYPE_STRING_VALUE, _T(""), m_strTitle);

	for (int i = 0; i < m_AskGroups.GetSize(); i++)
	{
		AskGroupData* pAskGroup = (AskGroupData*) m_AskGroups[i];

		pAskGroup->GetSchema(pSchema, i+1);
	}

	pSchema->EndComplexElement();
}

//----------------------------------------------------------------------------
void AskDialogData::GetXmlParameters (CXMLNode* pParentNode)
{
	CXMLNode* pNode = pParentNode->CreateNewChild(m_strName);
	pNode->SetAttribute(XML_TITLE_ATTRIBUTE, m_strTitle);

	for (int i = 0; i < m_AskGroups.GetSize(); i++)
	{
		AskGroupData* pAskGroup = (AskGroupData*) m_AskGroups[i];

		pAskGroup->GetXmlParameters(pNode, i+1);
	}
}

//----------------------------------------------------------------------------
void AskDialogData::AssignFromXml(CXMLNode* pParentNode)
{
	CXMLNode* pDialogNode = pParentNode->GetChildByName(m_strName);
	if (!pDialogNode)
		return;

	for (int i = 0; i < m_AskGroups.GetSize(); i++)
	{
		AskGroupData* pAskGroup = (AskGroupData*) m_AskGroups[i];
		if (pAskGroup)
			pAskGroup->AssignFromXml(pDialogNode, i);		
	}
}

//----------------------------------------------------------------------------
BOOL AskDialogData::InitAskFields()
{
	AskGroupData*	pTmpAskGroup;
    AskFieldData*	pTmpAskField;

	for (int nIdxGrp = 0; nIdxGrp < m_AskGroups.GetSize(); nIdxGrp++)
	{
		pTmpAskGroup = GetAskGroup(nIdxGrp);
		if (pTmpAskGroup == NULL || pTmpAskGroup->IsEmpty())
			continue;
				
		for (int idxFld = 0; idxFld < pTmpAskGroup->m_AskFields.GetSize(); idxFld++)
		{
			pTmpAskField = pTmpAskGroup->GetAskField(idxFld);

            WoormField* pRepField = m_SymTable.GetField(pTmpAskField->m_strPublicName);
			if (pRepField)
			{
				DataObj* pDataObj = pRepField->GetData();
				if (pRepField->GetInitExpression() && !pRepField->GetInitExpression()->Eval(*pDataObj))
					return FALSE;
			}				
		}
	}

	return TRUE;
}

//===========================================================================
// AskRuleData
//===========================================================================
IMPLEMENT_DYNAMIC(AskRuleData, CObject);

//----------------------------------------------------------------------------
AskRuleData::AskRuleData(WoormTable& aSymTable)
	:
    m_SymTable	(aSymTable),
	m_pDocument (NULL)
{}

//----------------------------------------------------------------------------
AskDialogData* AskRuleData::GetAskDialog(int nPos)
{
	if (nPos > m_AskDialogs.GetUpperBound() || nPos < 0) return NULL;
	return (AskDialogData*) m_AskDialogs[nPos];
}

//----------------------------------------------------------------------------
AskDialogData* AskRuleData::GetAskDialog(AskFieldData* pAskField)
{
	for (int k = 0; k <= m_AskDialogs.GetUpperBound(); k++)
	{
		AskDialogData*	pAskDlg = GetAskDialog(k);
		ASSERT_VALID(pAskDlg);

		for (int i = 0; i < pAskDlg->GetAskGroupSize(); i++)
		{
			AskGroupData* pAskGroup = pAskDlg->GetAskGroup(i);

			ASSERT_VALID(pAskGroup);
			for (int j = 0; j < pAskGroup->GetAskFieldSize(); j++)
			{
				AskFieldData* pCurrentAskField = pAskGroup->GetAskField(j);
				ASSERT_VALID(pAskField);

				if (pCurrentAskField == pAskField)
					return   GetAskDialog(k);
			}
		}
	}
	return NULL;
}

//----------------------------------------------------------------------------
AskDialogData* AskRuleData::GetAskDialog(AskGroupData* pAskGroup)
{
	for (int k = 0; k <= m_AskDialogs.GetUpperBound(); k++)
	{
		AskDialogData*	pAskDlg = GetAskDialog(k);
		ASSERT_VALID(pAskDlg);

		for (int i = 0; i < pAskDlg->GetAskGroupSize(); i++)
		{
			AskGroupData* pCurrentGroup = pAskDlg->GetAskGroup(i);
			if (pCurrentGroup == pAskGroup)
				return  GetAskDialog(k);
		}
	}
	return NULL;
}

//----------------------------------------------------------------------------
AskDialogData* AskRuleData::GetAskDialog(const CString& strName)
{
	for(int i = 0; i < m_AskDialogs.GetSize(); i++)
	{
		AskDialogData* pAskDlg = (AskDialogData*) m_AskDialogs[i];

		if(strName.CompareNoCase(pAskDlg->GetName()) == 0)
			return pAskDlg;
	}

	return NULL;
}

//----------------------------------------------------------------------------
int	 AskRuleData::Add(AskDialogData* pAskDlg)
{
	return m_AskDialogs.Add(pAskDlg);
}

//----------------------------------------------------------------------------
CString AskRuleData::GetAdviseName(AskDialogData* pCurrAskDialog)
{
	CString strAdvisedName = _T("NoName");
	int i = 0;
	while (ExistDialog(strAdvisedName, pCurrAskDialog))
		strAdvisedName.Format(_T("%s%d"), _T("NoName"), ++i);
	return strAdvisedName;
}

//----------------------------------------------------------------------------
void AskRuleData::DelAskDialog(int nPos, BOOL bDeleteFields)
{
	AskDialogData* pAskDlg = GetAskDialog(nPos);
	
	if (pAskDlg)
	{
		pAskDlg->DelAllAskGroup(bDeleteFields);

		m_AskDialogs.RemoveAt(nPos);
	}
}

//----------------------------------------------------------------------------
AskFieldData* AskRuleData::GetAskField(LPCTSTR pszFieldName)
{
	for (int i = 0; i < m_AskDialogs.GetSize(); i++)
	{
		AskFieldData* pAskField = 
			((AskDialogData*) m_AskDialogs[i])->GetAskField( pszFieldName );

		if (pAskField)
			return pAskField;
	}

	return NULL;
}

//----------------------------------------------------------------------------
BOOL AskRuleData::CanDeleteField(LPCTSTR pszFieldName) const
{
	for (int i = 0; i < m_AskDialogs.GetSize(); i++)
	{
		AskDialogData* pAskDlg = (AskDialogData*) m_AskDialogs[i];
		if (pAskDlg->GetAskField(pszFieldName))
			return pAskDlg->CanDeleteField( pszFieldName );
	}

    return TRUE;
}

//----------------------------------------------------------------------------
BOOL AskRuleData::DeleteField(LPCTSTR pszFieldName)
{
	for (int nIdx = 0; nIdx < m_AskDialogs.GetSize(); nIdx++)
	{
		AskDialogData* pAskDlg = (AskDialogData*) m_AskDialogs[nIdx];

		if (pAskDlg->DeleteField( pszFieldName ))
		{
        	if (pAskDlg->IsEmpty())
				m_AskDialogs.RemoveAt( nIdx );
			
			return TRUE;
		}
	}
	
	return FALSE;
}

//----------------------------------------------------------------------------
void AskRuleData::RenameField(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName)
{
	for (int nIdx = 0; nIdx < m_AskDialogs.GetSize(); nIdx++)
	{
		AskDialogData* pAskDlg = (AskDialogData*)m_AskDialogs[nIdx];

		pAskDlg->RenameField(pszOldFieldName, pszNewFieldName);
	}
}

//----------------------------------------------------------------------------
//	check if already exist a dialog strName, but must not be itself !
BOOL AskRuleData::ExistDialog(const CString& strName, AskDialogData* pAskDialog)
{
	for (int i = 0; i < m_AskDialogs.GetSize(); i++)
	{
		AskDialogData* pAskDlg = (AskDialogData*) m_AskDialogs[i];

		if (pAskDialog == NULL || pAskDlg != pAskDialog)
			if (strName.CompareNoCase( pAskDlg->GetName() ) == 0)
            	return TRUE;
	}

    return FALSE;
}

//----------------------------------------------------------------------------
BOOL AskRuleData::Parse(Parser& lex, ReportEngine* pReportEngine)
{
	lex.GetCommentTrace(this->m_arCommentTraceBefore);

	BOOL bVersion4 = lex.Matched(T_DIALOGS);
	if (bVersion4) lex.Match(T_BEGIN);

	while (lex.LookAhead(T_DIALOG))
	{
		AskDialogData* pAskDlg = new AskDialogData(m_SymTable, m_pDocument);
		if (!pAskDlg->Parse(lex, pReportEngine, this))
		{
			delete pAskDlg;
            return FALSE;
		}

		if (AfxGetExportSymbols()->IsActivated())
			AfxGetExportSymbols()->AddItemOnLastFile(pAskDlg->GetName(), EXP_SYMB_DIALOG);

        m_AskDialogs.Add( pAskDlg );
	}

	lex.GetCommentTrace(this->m_arCommentTraceInner);
	if (bVersion4) lex.Match(T_END);

	lex.GetCommentTrace(this->m_arCommentTraceAfter);

	return TRUE;
}

//----------------------------------------------------------------------------
void AskRuleData::Unparse(Unparser& oFile)
{
	oFile.UnparseComment(this->m_arCommentTraceBefore);

	if (m_AskDialogs.GetSize())
	{
		oFile.UnparseTag (T_DIALOGS);
		oFile.UnparseBegin ();

		for (int i = 0; i < m_AskDialogs.GetSize(); i++)
		{
			AskDialogData* pAskDlg = (AskDialogData*) m_AskDialogs[i];
			pAskDlg->Unparse( oFile );
		}

		oFile.UnparseComment(this->m_arCommentTraceInner);

		oFile.UnparseEnd ();
	}

	oFile.UnparseComment(this->m_arCommentTraceAfter);
}

//----------------------------------------------------------------------------
BOOL AskRuleData::GetSchema (CXSDGenerator* pSchema, BOOL asComplexType)
{
	if (m_AskDialogs.GetSize())
	{
		(asComplexType)	? pSchema->BeginComplexType(XML_PARAMETERS_TAG) : pSchema->BeginComplexElement(XML_PARAMETERS_TAG, _T("0"), _T("1"));

		for (int i = 0; i < m_AskDialogs.GetSize(); i++)
		{
			AskDialogData* pAskDlg = (AskDialogData*) m_AskDialogs[i];

			pAskDlg->GetSchema(pSchema);
		}

		(asComplexType)	? pSchema->EndComplexType() : pSchema->EndComplexElement();
	}
	return TRUE;
}

//----------------------------------------------------------------------------
void AskRuleData::GetXmlParameters (CXMLNode* pParentNode)
{
	CXMLNode* pNode = pParentNode->CreateNewChild(XML_PARAMETERS_TAG);

	for (int i = 0; i < m_AskDialogs.GetSize(); i++)
	{
		AskDialogData* pAskDlg = (AskDialogData*) m_AskDialogs[i];

		pAskDlg->GetXmlParameters(pNode);
	}
}

//----------------------------------------------------------------------------
void AskRuleData::AssignFromXml(CXMLNode* pParentNode)
{
	CXMLNode* pNode = pParentNode->GetChildByName(XML_PARAMETERS_TAG);
	if (!pNode)
		return;

	for (int i = 0; i < m_AskDialogs.GetSize(); i++)
	{
		AskDialogData* pAskDlg = (AskDialogData*) m_AskDialogs[i];
		pAskDlg->AssignFromXml(pNode);
	}
}


