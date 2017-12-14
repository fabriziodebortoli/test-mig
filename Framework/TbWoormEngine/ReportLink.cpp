
#include "stdafx.h"

#include <TBGeneric\EnumsTable.h>
#include <TBGeneric\CMapi.h>

#include <TBGenlib\Baseapp.h>
#include <TBGenlib\Parsedt.h>
#include <TBGenlib\Parslbx.h>
#include <TBGenlib\AddressEdit.h>
#include <TBGenlib\hlinkobj.h>

#include <TBParser\parser.h>

#include "edtcmm.h"

#include "RepTable.h"

#include "ReportLink.h"

#include "ActionsRepEngin.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//							WoormLinkFilter
///////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
WoormLinkFilter::WoormLinkFilter()
	:
	m_nFilterAlias				(0),
	m_nValueFilterAlias			(0),
	m_nOpFilter					(T_NULL_TOKEN),
	m_FilterType				(DataType::Null),
	m_pFilterValue				(NULL)
{
}

//------------------------------------------------------------------------------
WoormLinkFilter::~WoormLinkFilter()
{
	if (m_pFilterValue)
		delete m_pFilterValue;
}

//------------------------------------------------------------------------------
void WoormLinkFilter::Clear()
{
	m_FilterType				= DataType::Null;
	m_nOpFilter					= T_NULL_TOKEN;	
	m_nFilterAlias				= 0;
	m_nValueFilterAlias			= 0;
	m_pFilterValue				= NULL;
}

/////////////////////////////////////////////////////////////////////////////
//							WoormLink
///////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(WoormLink, CObject);

//------------------------------------------------------------------------------
WoormLink::WoormLink(WoormTable* pViewSymbolTable)
	:
	m_LinkType					(ConnectionEmpty),
	m_SubType					(File),
	m_bLinkTargetByField		(FALSE),

	m_nAlias					(1),
	m_nCurrentRow				(0),
	m_bWrongName				(FALSE), 
	m_bSyntaxWithExpr			(FALSE),

	m_pViewSymbolTable			(pViewSymbolTable),
	m_pLocalSymbolTable				(NULL),
	m_pDocumentContextSymbolTable	(NULL),

	m_pEnableLinkWhenExpr		(NULL),
	m_pBeforeLink				(NULL),
	m_pAfterLink				(NULL),

	m_nCounterForGenerateLocalID(SpecialReportField::REPORT_LOWER_SPECIAL_ID - 1)
{
	m_pLocalSymbolTable = (WoormTable*) (m_pViewSymbolTable->CreateLocalScope());
	m_pDocumentContextSymbolTable = (WoormTable*) (m_pLocalSymbolTable->CreateLocalScope());

	// OwnerId field: memorizza l'ID (puntatore) dell'eventuale documento chiamante
	{
		WoormField* pField = new WoormField(SpecialReportField::NAME.LINKED_DOC, WoormField::FIELD_INPUT, DataType::Long, SpecialReportField::ID.LINKED_DOC);
			pField->SetHidden(TRUE);	
		m_pLocalSymbolTable->Add(pField);

		WoormField* pRV = new WoormField(SpecialReportField::NAME.FUNCTION_RETURN_VALUE, WoormField::FIELD_INPUT, DataType::Variant, SpecialReportField::ID.FUNCTION_RETURN_VALUE);
			pRV->SetHidden(TRUE);	
		m_pLocalSymbolTable->Add(pRV);
	}

	m_pBeforeLink = new Block(NULL, m_pLocalSymbolTable, NULL, FALSE);
	m_pAfterLink = new Block(NULL, m_pLocalSymbolTable, NULL, FALSE);
}

//------------------------------------------------------------------------------
WoormLink::~WoormLink()
{
	SAFE_DELETE(m_pEnableLinkWhenExpr);

	if (m_pDocumentContextSymbolTable)
		m_pDocumentContextSymbolTable->DeleteMeAsLocalScope();
	if (m_pLocalSymbolTable)
		m_pLocalSymbolTable->DeleteMeAsLocalScope();
	SAFE_DELETE (m_pBeforeLink);
	SAFE_DELETE (m_pAfterLink);
}

//------------------------------------------------------------------------------
void WoormLink::Clear()
{
	m_nCurrentRow				= 0;

	m_LinkType					= ConnectionEmpty;
	m_SubType					= File;

	m_nAlias					= 1;

	m_Filter1.Clear();
	m_Filter2.Clear();

	m_pLocalSymbolTable->RemoveAll();
}

//------------------------------------------------------------------------------
BOOL WoormLink::ParseItem(Parser& lex, BOOL blk, WoormTable* pSymTable)
{
	BOOL bOk = TRUE;
	CString strName;
	int nAlias;
	DataType dtType;
	DataObj* pValue=NULL;
	Expression* pItemExpr;

	do
	{
		dtType = DataType::Null;
		nAlias = 0;

		if 
			(
				m_LinkType != ConnectionRadar &&
				!lex.LookAhead(T_END) &&
				!lex.LookAhead(T_SELECT)
			)
		{
			bOk = lex.ParseDataType(dtType);
			if (!bOk)
				return FALSE;
		}

		switch (lex.LookAhead())
		{
			case T_ID: 
				{
					bOk = lex.ParseID (strName);

					if (lex.LookAhead(T_ALIAS))
					{
						bOk = WoormLink::ParseAlias(lex, nAlias);
						if (!bOk)
							return FALSE;
						//lookup sym table
						if (m_LinkType == ConnectionRadar)
						{
							//gestione speciale old style
							WoormField* pF = new WoormField(strName, WoormField::FIELD_INPUT, dtType, nAlias);
							pSymTable->Add(pF);
						}
						else
						{
							SymField* pF = m_pViewSymbolTable->GetFieldByID(nAlias);
							if (!pF)
								return FALSE;
							CString sValName = pF->GetName();
							if (!AddLinkParam(strName, dtType, sValName))
							    return FALSE;
						}
					} 
					else if (lex.Matched(T_ASSIGN))
					{
						pItemExpr = new Expression(m_pViewSymbolTable);
						pItemExpr->SetStopTokens(T_SEP);
						if (!pItemExpr->Parse(lex, dtType, TRUE))
						{
							delete pItemExpr;
							return FALSE;
						}
						//Add(new ConnectionParam(strName, dtType, pItemExpr));
						WoormField* pF = new WoormField(strName, WoormField::FIELD_INPUT, dtType, m_nCounterForGenerateLocalID--);
						pF->SetInitExpression(pItemExpr);
						pSymTable->Add(pF);

						lex.SkipToken();
					}
					else
					{
						pValue = NULL;
						bOk = ParseConstValue (lex, dtType, pValue);
						if (bOk)
						{
							CString strConstValue = UnparseConstValue(pValue);
	                        if (!AddLinkParam(strName, dtType, strConstValue))
								return false;
						}
						else
						{
							if (pValue) delete pValue;
						}
						pValue = NULL;
					}

					break;
				}

			case T_EOF : lex.SetError (_TB("Unexpected End of file")); bOk = FALSE; 
				break;

			case T_END :
				if (blk) 
					return bOk;
				lex.SetError(_TB("Unexpected END"));
				return FALSE;

			case T_SELECT :
				if (!blk) 
					return bOk;
				lex.SetError(_TB("END expected"));
				return FALSE;

			default :
				if (blk)
				{
					lex.SetError(_TB("Missing END in Options section"));
					bOk = FALSE;
				}
		}
	}
	while (bOk && blk);
	return bOk;
}

//------------------------------------------------------------------------------
BOOL WoormLink::ParseItems (Parser& lex, WoormTable* pSymTable)
{
	BOOL bOk = TRUE;
	BOOL bHaveBegin = lex.Matched(T_BEGIN);

	do 
	{ 
		bOk = ParseItem(lex, bHaveBegin, pSymTable) && !lex.Bad() && !lex.Eof(); 
	}
	while (bOk && !lex.LookAhead(T_END));

	if (bHaveBegin && !lex.Match(T_END)) 
		return FALSE;
	
	return bOk;
}

//------------------------------------------------------------------------------
BOOL WoormLink::ParseConstValue (Parser& lex, DataType dtFilterType, DataObj*& pValue)
{
	BOOL bOk = TRUE;
	pValue = DataObj::DataObjCreate(dtFilterType);

	switch (dtFilterType.m_wType)
	{
		case DATA_INT_TYPE:
		{
			int n = 0;
			bOk = lex.ParseSignedInt(n);
			if (!bOk)
				return FALSE;

			((DataInt*)pValue)->Assign(n);
			break;
		}
		case DATA_LNG_TYPE:
		{
			long l = 0;
			bOk = lex.ParseSignedLong(l);
			if (!bOk)
				return FALSE;

			((DataLng*)pValue)->Assign(l);
			break;
		}
		case DATA_BOOL_TYPE:
		{
			BOOL b = FALSE;
			bOk = lex.ParseBool(b);
			if (!bOk)
				return FALSE;

			((DataBool*)pValue)->Assign(b);
			break;
		}
		case DATA_STR_TYPE:
		{
			CString str;
			bOk = lex.ParseString(str);
			str.Remove(_T('%'));
			if (!bOk)
				return FALSE;

			((DataStr*)pValue)->Assign(str);
			break;
		}
		case DATA_DBL_TYPE:
		case DATA_MON_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE:
		{
			double f = 0;
			bOk = lex.ParseSignedDouble(f);
			if (!bOk)
				return FALSE;

			((DataDbl*)pValue)->Assign(f);
			break;
		}
		case DATA_DATE_TYPE:
		{   
			DWORD dw1 = 0; DWORD dw2 = 0; DataType dt;
				
			if (lex.LookAhead() == T_BRACEOPEN)
			{
				dt = lex.ParseComplexData(&dw1, &dw2, TRUE);
				if (dt != dtFilterType)
					return FALSE;

				((DataDate*)pValue)->Assign (dw1, dw2);
			}
			else
			{
				bOk = lex.ParseDateTimeString (FromDataTypeToToken(dtFilterType), &dw1, &dw2);
				if (!bOk)
					return FALSE;

				((DataDate*)pValue)->Assign (dw1, dw2);
			}
			break;
		} 
		case DATA_ENUM_TYPE:
		{   
			if (lex.LookAhead() != T_BRACEOPEN)
				return FALSE;

			DWORD dw1 = 0; DWORD dw2 = 0; DataType dt; 
				
			if (lex.LookAhead() == T_BRACEOPEN)
			{
				dt = lex.ParseComplexData(&dw1, &dw2, TRUE);
				if (dt != dtFilterType)
					return FALSE;
			}
			else
			{
				bOk = lex.ParseDWord(dw1);
				if(!bOk)
					return FALSE;

			}
			if (dtFilterType.m_wType == DATA_ENUM_TYPE)
				((DataEnum*)pValue)->Assign(dw1);
			break;
		} 
		default: 
			return FALSE;
	}
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL WoormLink::ParseFilterClause (Parser& lex, WoormLinkFilter& Filter)
{
	BOOL bOk =
		lex.ParseDataType (Filter.m_FilterType) &&
		WoormLink::ParseAlias(lex, Filter.m_nFilterAlias);

	//--op
	Filter.m_nOpFilter = lex.LookAhead();
	if 
		( ! (
			Filter.m_nOpFilter == T_EQ ||
			Filter.m_nOpFilter == T_NE ||
			Filter.m_nOpFilter == T_LT ||
			Filter.m_nOpFilter == T_LE ||
			Filter.m_nOpFilter == T_GT ||
			Filter.m_nOpFilter == T_GE ||
			Filter.m_nOpFilter == T_LIKE ||
			Filter.m_nOpFilter == T_DIFF 
		) )
	{
		return FALSE;
	}
		
	if (Filter.m_nOpFilter == T_LIKE && Filter.m_FilterType != DataType::String)
	{
		return FALSE;
	}
	if 
		(
			Filter.m_nOpFilter != T_EQ &&
			Filter.m_nOpFilter != T_NE &&
			Filter.m_nOpFilter != T_DIFF &&
			Filter.m_FilterType == DataType::Bool
		)
	{
		return FALSE;
	}
	if (!lex.SkipToken())
	{
		return FALSE;
	}

	if (lex.LookAhead() == T_ALIAS)
	{
		//manca il tipo, verra' considerato quello del primo termine
		bOk = WoormLink::ParseAlias(lex, Filter.m_nValueFilterAlias);
	}
	else
	{ 
		bOk = ParseConstValue (lex, Filter.m_FilterType, Filter.m_pFilterValue);
	}
	
	return bOk;
}

//------------------------------------------------------------------------------
BOOL WoormLink::Parse (Parser& lex)
{
	BOOL bOk = TRUE;
	if (lex.Matched(T_LINKREPORT))
		m_LinkType = WoormLinkType::ConnectionReport;
	else if (lex.Matched(T_LINKFORM))
		m_LinkType = WoormLinkType::ConnectionForm;
	else if (lex.Matched(T_LINKFUNCTION))
		m_LinkType = WoormLinkType::ConnectionFunction;
	else if (lex.Matched(T_LINKURL))
	{
		m_LinkType = WoormLinkType::ConnectionURL;
		m_SubType = WoormLinkSubType::Url;
		if (lex.LookAhead(T_INT))
		{
			int sub = 0;
			lex.ParseInt(sub);
			switch(sub)
			{
			case Url:
				m_SubType = WoormLinkSubType::Url;
				break;
			case File:
				m_SubType = WoormLinkSubType::File;
				break;
			case MailTo:
				m_SubType = WoormLinkSubType::MailTo;
				break;
			case CallTo:
				m_SubType = WoormLinkSubType::CallTo;
				break;
			case GoogleMap:
				m_SubType = WoormLinkSubType::GoogleMap;
				break;

			default:
				ASSERT(FALSE);
				break;
			}
		}
	}
	else if (lex.Matched(T_LINKRADAR))
	{
		m_LinkType = WoormLinkType::ConnectionRadar;
	}
	else
	{
		lex.SetError(_TB("Expected a Link Tag"));
		return FALSE;
	}

	if (m_LinkType == WoormLinkType::ConnectionRadar)
	{
		bOk = lex.ParseID(m_strTarget);	// Table Name 
	}
	else if (lex.LookAhead(T_ALIAS))
	{
        int nsDocumentAlias;
		bOk = ParseAlias(lex, nsDocumentAlias);
		// lookup id --->strName
        SymField* pF = m_pViewSymbolTable->GetFieldByID(nsDocumentAlias);
		if (pF == NULL) 
		{ 
			lex.SetError (_TB("Unresolved Alias on Link"));
			return FALSE;
		}
		m_strTarget = pF->GetName();

		m_bLinkTargetByField = TRUE;
	}
	else if (lex.LookAhead() == T_ID)
	{
		bOk = lex.ParseID(m_strTarget);

		m_bLinkTargetByField = TRUE;
	}
	else
	{
		bOk = lex.ParseString(m_strTarget);	// Namespace/Path literal
	}
	if (!bOk) 
		return FALSE;

	lex.Matched(T_ON); //per compatibilità con le versioni precedenti

	if (lex.LookAhead() == T_ALIAS)
	{
		bOk = ParseAlias(lex, m_nAlias);
		if (!bOk)
			return FALSE;
		if (m_LinkType != WoormLinkType::ConnectionRadar)
		{
			SymField* pF = m_pViewSymbolTable->GetFieldByID(m_nAlias);
			if (pF == NULL) 
			{ 
				lex.SetError (_TB("Unresolved Alias on Link"));
				return FALSE;
			}
			m_strLinkOwner = pF->GetName();
		}
	}
	else if (lex.LookAhead() == T_ID)
	{
		bOk = lex.ParseID(m_strLinkOwner);
	    if (!bOk)
		return FALSE;
		SymField* pF  = m_pViewSymbolTable->GetField(m_strLinkOwner);
		if (pF == NULL)
		{ 
		lex.SetError (_TB("Unresolved Identifier on Link"));
		return FALSE;
		}
		m_nAlias = pF->GetId();
		m_bSyntaxWithExpr = TRUE; 
	}

	if (m_LinkType != WoormLinkType::ConnectionRadar && lex.Matched(T_WHEN))
	{
		if (m_bSyntaxWithExpr)
		{
			if (m_pEnableLinkWhenExpr)
				delete m_pEnableLinkWhenExpr;

			m_pEnableLinkWhenExpr = new Expression(m_pViewSymbolTable);
			m_pEnableLinkWhenExpr->SetStopTokens(T_BEGIN);
			bOk = m_pEnableLinkWhenExpr->Parse(lex, DataType::Bool, TRUE);
		}
		else
		{
			bOk = ParseFilterClause(lex, m_Filter1);
			if (bOk && (lex.LookAhead() == T_AND || lex.LookAhead() == T_OR) )
			{
				m_nOpLogicalFilter = lex.LookAhead();
				bOk = lex.SkipToken () && ParseFilterClause(lex, m_Filter2);
			}
			if (!bOk)
				return FALSE;
			CString sExpr = UnparseFilter();	//converte il formato
			Parser localLex(sExpr);
			m_pEnableLinkWhenExpr = new Expression(m_pViewSymbolTable);
			bOk = m_pEnableLinkWhenExpr->Parse(localLex, DataType::Bool, TRUE);
		}
	}

	if (!bOk)
		return FALSE;

	if (m_LinkType == WoormLinkType::ConnectionForm && !m_bLinkTargetByField)
	{
		CTBNamespace ns(CTBNamespace::DOCUMENT, m_strTarget);
		m_bWrongName = !ns.IsValid();
	}
	else if (m_LinkType == WoormLinkType::ConnectionReport && !m_bLinkTargetByField)
	{
		CTBNamespace ns(CTBNamespace::REPORT, m_strTarget);
		m_bWrongName = !ns.IsValid();
	}
	else if (m_LinkType == WoormLinkType::ConnectionFunction && !m_bLinkTargetByField)
	{
		CTBNamespace ns(CTBNamespace::FUNCTION, m_strTarget);
		m_bWrongName = !ns.IsValid();

		CFunctionDescription aFunctionDescription;
		bOk = AfxGetTbCmdManager()->GetFunctionDescription(ns, aFunctionDescription, FALSE);
		if (!bOk)
			return FALSE;

		WoormField* pField = m_pLocalSymbolTable->GetFieldByID(SpecialReportField::ID.FUNCTION_RETURN_VALUE);
		if (!pField)
			return FALSE;
		pField->SetDataType(aFunctionDescription.GetReturnValueDataType());
	}

	bOk = ParseItems(lex, this->m_pLocalSymbolTable);
	if (!bOk)
		return FALSE;

	if (lex.Matched(T_CONTEXT))
	{
		bOk = ParseItems(lex, m_pDocumentContextSymbolTable);
		if (!bOk)
			return FALSE;
	}

	if (lex.Matched(T_BEFORE))
	{
		bOk = m_pBeforeLink->Parse(lex);
		m_pBeforeLink->SetForceBeginEnd();
		if (!bOk)
			return FALSE;
	}

	if (lex.Matched(T_AFTER))
	{
		bOk = m_pAfterLink->Parse(lex);
		m_pAfterLink->SetForceBeginEnd();
		//posticipo il parsering dell'after block poichè non conosco ancora i web-methods del LinkedDocument
		//lex.EnableAuditString();
		//bOk = lex.SkipBlock(T_BEGIN, T_END);
		//m_sAfterLink = lex.GetAuditString(); m_sAfterLink.Trim();
		//lex.EnableAuditString(FALSE);
		if (!bOk)
			return FALSE;
	}

	return bOk;
}

//------------------------------------------------------------------------------
void WoormLink::Unparse(Unparser& ofile)
{
	switch (m_LinkType)
	{
		case ConnectionReport: 
		{
			ofile.UnparseTag(T_LINKREPORT, FALSE);

			if (this->m_bLinkTargetByField)
				ofile.UnparseID(m_strTarget, FALSE); //m_strName = nome variabile
			else
				ofile.UnparseString(m_strTarget, FALSE); //m_strName = namespace esplicito
			break;
		}
		case ConnectionForm: 
		{
			ofile.UnparseTag(T_LINKFORM, FALSE);

			if (this->m_bLinkTargetByField)
				ofile.UnparseID(m_strTarget, FALSE); //m_strName = nome variabile
			else
				ofile.UnparseString(m_strTarget, FALSE); //m_strName = namespace esplicito
			break;
		}
		case ConnectionFunction: 
		{
			ofile.UnparseTag(T_LINKFUNCTION, FALSE);

			if (this->m_bLinkTargetByField)
				ofile.UnparseID(m_strTarget, FALSE); //m_strName = nome variabile
			else
				ofile.UnparseString(m_strTarget, FALSE); //m_strName = namespace esplicito
			break;
		}

		case ConnectionURL: 
		{
			ofile.UnparseTag(T_LINKURL, FALSE);		

			if (m_SubType == MailTo)
				ofile.UnparseInt(MailTo, FALSE);	
			else if (m_SubType == CallTo)
				ofile.UnparseInt(CallTo, FALSE);	
			else if (m_SubType == GoogleMap)
				ofile.UnparseInt(GoogleMap, FALSE);
			else if (m_SubType == File)
				ofile.UnparseInt(File, FALSE);
			//Default omesso
			//else if (m_SubType == Url)
			//	ofile.UnparseInt(Url, FALSE);

			ofile.UnparseBlank(FALSE);

			if (this->m_bLinkTargetByField)
				ofile.UnparseID(m_strTarget, FALSE); //m_strName = nome variabile
			else
				ofile.UnparseString(m_strTarget, FALSE); //m_strName = namespace esplicito
			break;
		}

		case ConnectionRadar: 
		{
			ofile.UnparseTag(T_LINKRADAR, FALSE);
			ofile.UnparseID(m_strTarget, FALSE);
			break;
		}
	}
	
	ofile.UnparseCrLf();
	ofile.IncTab();

	ofile.UnparseTag(T_ON, FALSE);

	if (m_LinkType == ConnectionRadar)
	{
		ofile.UnparseTag(T_ALIAS, FALSE);
	    ofile.UnparseInt(m_nAlias, FALSE);
	}
	else
    	ofile.UnparseID(m_strLinkOwner, FALSE);

	ofile.DecTab();
	ofile.UnparseCrLf();

	if (m_LinkType != ConnectionRadar)
	{
		if ((m_pEnableLinkWhenExpr) && (!m_pEnableLinkWhenExpr->IsEmpty()))
		{
			ofile.IncTab();
			ofile.UnparseTag(T_WHEN, FALSE);
			ofile.IncTab();

			UnparseFilter(ofile);

			ofile.DecTab();
			ofile.DecTab();
		}
	}

	UnparseItems(ofile, this->m_pLocalSymbolTable);
	ofile.IncTab();

	if (this->m_pDocumentContextSymbolTable && this->m_pDocumentContextSymbolTable->GetCount())
	{
		ofile.UnparseTag(T_CONTEXT, FALSE);
		ofile.IncTab();
		UnparseItems(ofile, this->m_pDocumentContextSymbolTable);
		ofile.DecTab();
	}

	if (m_pBeforeLink && !m_pBeforeLink->IsEmpty())
	{
		ofile.UnparseTag(T_BEFORE, FALSE);
		m_pBeforeLink->Unparse(ofile);
	}
	if (m_pAfterLink && !m_pAfterLink->IsEmpty())
	{
		ofile.UnparseTag(T_AFTER, FALSE);
		m_pAfterLink->Unparse(ofile);
	}

	ofile.DecTab();
	ofile.UnparseCrLf();
}

//----------------------------------------------------------------------------
void WoormLink::UnparseFilter(Unparser& ofile, BOOL bConvertExpression /*= FALSE*/)
{
	if (bConvertExpression)
	{
		UnparseFilterClause(ofile, m_Filter1);
		
		if (m_Filter2.m_nFilterAlias)
		{
			ofile.UnparseBlank();
			ofile.UnparseTag(m_nOpLogicalFilter, FALSE);

			UnparseFilterClause(ofile, m_Filter2);
		}
	}
	else
	{   
		ofile.UnparseExpr(m_pEnableLinkWhenExpr->ToString(), TRUE);
	}
}

//----------------------------------------------------------------------------
//TAPPULLO vedi CString	Block::Unparse()
CString	WoormLink::UnparseFilter()
{
	CString	strFileName = GetTempName();
	Unparser oFile(strFileName);
	UnparseFilter(oFile, TRUE /*bool convertExpression */);
	oFile.Close();

	CLineFile	iFile;
	CString strTmp;
	TCHAR 		buffer[256];
	UINT flags = CFile::modeRead | CFile::shareDenyWrite | CFile::typeText;
	if (iFile.Open(strFileName, flags))
	{
		while (iFile.ReadString(buffer, 255))
			strTmp += buffer;
		iFile.Close();
	}
	DeleteFile (strFileName);

	if (!strTmp.IsEmpty())
		ConvertCString(strTmp, LF_TO_CRLF);

	return strTmp;
}
 
//----------------------------------------------------------------------------
//TAPPULLO2 vedi CString	Block::Unparse()
CString	WoormLink::UnparseConstValue(const DataObj* pValue)
{
	CString	strFileName = GetTempName();
	Unparser oFile(strFileName);
	UnparseConstValue(oFile, pValue);
	oFile.Close();

	CLineFile	iFile;
	CString strTmp;
	TCHAR 		buffer[256];
	UINT flags = CFile::modeRead | CFile::shareDenyWrite | CFile::typeText;
	if (iFile.Open(strFileName, flags))
	{
		while (iFile.ReadString(buffer, 255))
			strTmp += buffer;
		iFile.Close();
	}
	DeleteFile (strFileName);

	if (!strTmp.IsEmpty())
		ConvertCString(strTmp, LF_TO_CRLF);

	return strTmp;
}

//------------------------------------------------------------------------------
CString	WoormLink::EncodeURLString(CString strAddress)
{	
	if (m_LinkType != ConnectionURL)
		return NULL;

	if (strAddress.IsEmpty())
		return NULL;

	if (m_SubType == MailTo)
	{
		strAddress.Replace(CMapiMessage::TAG_CERTIFIED, L"");
		return _T("mailto:") + strAddress;
	}

	if (m_SubType == CallTo)
	{
		CString sV (strAddress); sV.MakeLower();
		if (sV.Find(_T("callto:")) == 0 || sV.Find(_T("skype:")) == 0)
			return strAddress;

		if (strAddress[0] == '+' || _istalpha(strAddress[0]) || strAddress.Find(_T("00")) == 0)
			return _T("callto:") + strAddress;

		CString sISOCode, sTelephonePrefix;
		for (int i = 0; i <= m_pLocalSymbolTable->GetUpperBound(); i++)
		{
			WoormField* pItem = m_pLocalSymbolTable->GetAt(i);
			if (pItem->GetId() == SpecialReportField::ID.LINKED_DOC || pItem->GetId() == SpecialReportField::ID.FUNCTION_RETURN_VALUE)
				continue;

			DataStr p;
			if (!pItem->GetInitExpression() || !pItem->GetInitExpression()->Eval(p)) 
				continue;

			if (pItem->GetName().CompareNoCase(_NS_WRMVAR("TelephonePrefix")) == 0)
				sTelephonePrefix = p.GetString();
			else if (pItem->GetName().CompareNoCase(_NS_WRMVAR("ISOCountryCode")) == 0)
				sISOCode = p.GetString();
		}
		
		if (sTelephonePrefix.IsEmpty() && !sISOCode.IsEmpty())
		{
			DataStr ds(sISOCode);
			sTelephonePrefix = CPhoneEdit::LookupTelephonePrefix(&ds);
		}
		
		return _T("callto:") + sTelephonePrefix + strAddress;
	}

	if (m_SubType == GoogleMap)
	{
		CString sZip, sCountry, sCounty, sCity, sISOCountryCode, sFederalState, sStreetNumber, sLatitude, sLongitude, sAddressType;

		for (int i = 0; i <= m_pLocalSymbolTable->GetUpperBound(); i++)
		{
			WoormField* pItem = m_pLocalSymbolTable->GetAt(i);
			if (pItem->GetId() == SpecialReportField::ID.LINKED_DOC || pItem->GetId() == SpecialReportField::ID.FUNCTION_RETURN_VALUE)
				continue;

			DataStr p;
			if (!pItem->GetInitExpression() || !pItem->GetInitExpression()->Eval(p)) 
				continue;

			if (pItem->GetName().CompareNoCase(_NS_WRMVAR("Country")) == 0)
				sCountry = p.GetString();
			else if (pItem->GetName().CompareNoCase(_NS_WRMVAR("County")) == 0)
				sCounty = p.GetString();
			else if (pItem->GetName().CompareNoCase(_NS_WRMVAR("City")) == 0)
				sCity = p.GetString();
			else if (pItem->GetName().CompareNoCase(_NS_WRMVAR("ZipCode")) == 0)
				sZip = p.GetString();
			else if (pItem->GetName().CompareNoCase(_NS_WRMVAR("Address")) == 0)
				strAddress = p.GetString();
			else if (pItem->GetName().CompareNoCase(_NS_WRMVAR("FederalState")) == 0)
				sFederalState = p.GetString();
			else if (pItem->GetName().CompareNoCase(_NS_WRMVAR("StreetNumber")) == 0)
				sStreetNumber = p.GetString();
			else if (pItem->GetName().CompareNoCase(_NS_WRMVAR("Latitude")) == 0)
				sLatitude = p.GetString();
			else if (pItem->GetName().CompareNoCase(_NS_WRMVAR("Longitude")) == 0)
				sLongitude = p.GetString();
			else if (pItem->GetName().CompareNoCase(_NS_WRMVAR("ISOCountryCode")) == 0)
				sISOCountryCode = p.GetString();
			else if (pItem->GetName().CompareNoCase(_NS_WRMVAR("AddressType")) == 0)
			{
				sAddressType = p.GetString() + ' ';	//sarà concatenato ad Address
				sAddressType.Trim();
			}
		}
		if (!sISOCountryCode.IsEmpty())
		{
			CString sNs;
			if (AfxIsActivated(MAGONET_APP, _NS_ACT("Company")))
				sNs = _NS_HKL("HotKeyLink.Erp.Company.Dbl.ISOCountryCodes");
			else if (AfxIsActivated(OFM_APP, _NS_ACT("Office")))
				sNs = _NS_HKL("HotKeyLink.OFM.Office.Dbl.ISOCountryCodes");
			else
				return _T("");
			CTBNamespace ns(sNs);

			FailedInvokeCode aFailedCode;
			HotKeyLinkObj* pHKL = AfxGetTbCmdManager()->RunHotlink (ns, &aFailedCode);
			if (pHKL)
			{
				DataStr dsISO(sISOCountryCode);
				if (pHKL->DoFindRecord(&dsISO))
				{
					DataObj* pCountry = pHKL->GetField(_NS_FLD("Description"));
					if (pCountry && !pCountry->IsEmpty())
					{
						sCountry =  ((DataStr*)pCountry)->GetString();
					}
				}
				delete pHKL;
			}
		}

		CString sG;
		if (!sLatitude.IsEmpty() && !sLongitude.IsEmpty())
			sG = CGeocoder::GetGoogleWebLink(sLatitude, sLongitude);
		else
			sG = CGeocoder::GetGoogleWebLink(sAddressType + strAddress, sStreetNumber, sCity, sCounty, sCountry, sFederalState, sZip);
		return sG;
	}

	if (m_SubType == Url)
	{
	//parametri
	for (int i = 0; i <= m_pLocalSymbolTable->GetUpperBound(); i++)
	{
		WoormField* pItem = m_pLocalSymbolTable->GetAt(i);
		if (pItem->GetId() == SpecialReportField::ID.LINKED_DOC || pItem->GetId() == SpecialReportField::ID.FUNCTION_RETURN_VALUE)
			continue;

		if (i > 0) 
			strAddress.AppendChar('&');
		else 
			strAddress.AppendChar('?');

		strAddress.Append(pItem->GetName());
		strAddress.AppendChar('=');

		DataObj* pdsExpItem = DataObj::DataObjCreate(DATA_STR_TYPE);
		if (pItem->GetInitExpression() && pItem->GetInitExpression()->Eval(*pdsExpItem))
			strAddress.Append(::HTMLEncode(pdsExpItem->Str()));	

			SAFE_DELETE(pdsExpItem);
		}
		return strAddress;
	}

	if (m_SubType == File)
	{
		//parametri
		for (int i = 0; i <= m_pLocalSymbolTable->GetUpperBound(); i++)
		{
			WoormField* pItem = m_pLocalSymbolTable->GetAt(i);
			if (pItem->GetId() == SpecialReportField::ID.LINKED_DOC || pItem->GetId() == SpecialReportField::ID.FUNCTION_RETURN_VALUE)
				continue;

			strAddress.AppendChar(' ');

			strAddress.AppendChar('"');

			DataObj* pdsExpItem = DataObj::DataObjCreate(DATA_STR_TYPE);
			if (pItem->GetInitExpression() && pItem->GetInitExpression()->Eval(*pdsExpItem))
				strAddress.Append(::HTMLEncode(pdsExpItem->Str()));

			strAddress.AppendChar('"');

		SAFE_DELETE(pdsExpItem);
	}
		return strAddress;
	}


	return strAddress;
}

//------------------------------------------------------------------------------
void WoormLink::UnparseConstValue (Unparser& ofile, const DataObj* pValue)
{
	if (pValue == NULL)
	{
		TRACE("WoormLink link item: missing const value\n");
		ASSERT(FALSE);
		return;
	}

	switch ((pValue->GetDataType()).m_wType)
	{
		case DATA_INT_TYPE:
		{
			ofile.UnparseInt(*(DataInt*)pValue);
			break;
		}
		case DATA_LNG_TYPE:
		{
			ofile.UnparseLong(*(DataLng*)pValue);
			break;
		}
		case DATA_BOOL_TYPE:
		{
			ofile.UnparseBool(*(DataBool*)pValue);
			break;
		}
		case DATA_STR_TYPE:
		{
			ofile.UnparseString(((DataStr*)pValue)->GetString());
			break;
		}
		case DATA_DBL_TYPE:
		case DATA_MON_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE:
		{
			ofile.UnparseDouble(*(DataDbl*)pValue);
			break;
		}
		case DATA_DATE_TYPE:
		{   
			ofile.Write(ofile.UnparseDateTime(*(DataDate*)pValue));
			break;
		} 
		case DATA_ENUM_TYPE:
		{   
			ofile.UnparseEnum(((DataEnum*)pValue)->GetValue());
			break;
		} 
		default: 
			return;
	}
}

//------------------------------------------------------------------------------
void WoormLink::UnparseFilterClause(Unparser& ofile, const WoormLinkFilter& Filter)
{	
	CString ident = m_pViewSymbolTable->GetFieldByID(Filter.m_nFilterAlias)->GetName();
	ofile.UnparseID(ident, FALSE); 

	ofile.UnparseBlank();
	ofile.UnparseTag(Filter.m_nOpFilter, FALSE);
	
	if (Filter.m_pFilterValue)
	{
		UnparseConstValue(ofile, Filter.m_pFilterValue);
	}
	else
	{
		//BUG: storicamente mancava ofile.UnparseTag(T_ALIAS, FALSE);
		//ofile.UnparseInt(Filter.m_nValueFilterAlias);

	    CString ident = m_pViewSymbolTable->GetFieldByID(Filter.m_nValueFilterAlias)->GetName();
		ofile.UnparseID(ident, FALSE);
	}
}

//------------------------------------------------------------------------------
void WoormLink::UnparseItems (Unparser& ofile, WoormTable* pSymTable)
{
	ofile.UnparseBegin();
	for (int i = 0; i <= pSymTable->GetUpperBound(); i++)
	{
		WoormField* pItem = pSymTable->GetAt(i);
		if (pItem->GetId() == SpecialReportField::ID.LINKED_DOC || pItem->GetId() == SpecialReportField::ID.FUNCTION_RETURN_VALUE)
			continue;

		if (m_LinkType != ConnectionRadar)
		{	
			ofile.UnparseDataType (pItem->GetDataType(), DataType::Null, FALSE);
			if (pItem->GetDataType() == DATA_ENUM_TYPE)
			{
				ofile.Write	
					(
						_T(" /* ") + AfxGetEnumsTable()->GetEnumTagName(pItem->GetDataType().m_wTag)  + _T(" */ "),
						FALSE
					);
			}

            ofile.UnparseID	(pItem->GetName(), FALSE);

	        ofile.UnparseTag (T_ASSIGN,	FALSE);
			
			if (pItem->GetInitExpression())
			{
				CString se = pItem->GetInitExpression()->ToString();
				se.Trim(L" \t\n\r");
				ofile.UnparseExpr(se, FALSE);
			}

			ofile.UnparseTag (T_SEP, TRUE);
		}
		else
		{
		   ofile.UnparseID	(pItem->GetName(), FALSE);
		   ofile.UnparseTag(T_ALIAS, FALSE);
		   ofile.UnparseInt(pItem->GetId());
		}
	}
	ofile.UnparseEnd();
}

//-----------------------------------------------------------------------------
BOOL WoormLink::AddLinkParam(CString identName, DataType dtType, CString strExpr/*, int nAlias*/)
{
	WoormField* pF = new WoormField(identName, WoormField::FIELD_INPUT, dtType, m_nCounterForGenerateLocalID--);
	this->m_pLocalSymbolTable->Add(pF);

	pF->SetInitExpression(strExpr);

    return TRUE;
}


//-----------------------------------------------------------------------------
/* static */ BOOL WoormLink::ParseAlias(Parser& lex, int& nAlias)
{
	nAlias = 0;
	BOOL bOk = lex.ParseTag(T_ALIAS) &&	lex.ParseInt(nAlias);
	if (bOk && lex.Matched(T_COMMA))
	{
		bOk = lex.ParseInt(nAlias);
	}
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL WoormLink::CanDeleteField (LPCTSTR szName, CString& sLog) const
{
	WoormField* pDelF = m_pViewSymbolTable ? m_pViewSymbolTable->GetField(szName) : NULL;
	if (m_LinkType == ConnectionRadar && pDelF)
	{
		if (m_nAlias == pDelF->GetId())
			return (sLog = cwsprintf(_TB("Cannot delete Radar key field {0-%s}")), m_strLinkOwner), FALSE;
	}

	sLog = cwsprintf(_T("Link {0-%s} On {1-%s}"), m_strTarget, m_strLinkOwner);

	if (m_strLinkOwner.CompareNoCase(szName) == 0 || m_strTarget.CompareNoCase(szName) == 0)
		return FALSE;

	sLog +=  _T(" - ");;

	if (m_pEnableLinkWhenExpr && m_pEnableLinkWhenExpr->HasMember(szName))
		return (sLog += _TB("Link's filter condition")), FALSE;

	if (m_pBeforeLink && !m_pBeforeLink->CanDeleteField(szName))
		return (sLog += _TB("Link's before block actions")), FALSE;
	if (m_pAfterLink && !m_pAfterLink->CanDeleteField(szName))
		return (sLog += _TB("Link's after block actions")), FALSE;

	for (int i = 0; i <= m_pLocalSymbolTable->GetUpperBound(); i++)
	{
		WoormField* pItem = m_pLocalSymbolTable->GetAt(i);
		if (pItem->GetId() == SpecialReportField::ID.LINKED_DOC || pItem->GetId() == SpecialReportField::ID.FUNCTION_RETURN_VALUE)
			continue;

		if (m_LinkType != ConnectionRadar)
		{	
			if (pItem->GetInitExpression() && pItem->GetInitExpression()->HasMember(szName))
				return (sLog += cwsprintf(_TB("Link's parameter {0-%s}"), pItem->GetName())), FALSE;
		}
		else if (pDelF)
		{
			if (pItem->GetId() == pDelF->GetId())
				return (sLog += cwsprintf(_TB("Cannot delete Radar parameter {0-%s}"), pItem->GetName())), FALSE;
		}
	}
	
	sLog.Empty();
	return TRUE;
}

///////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
BOOL WoormLinkArray::Parse(Parser& lex)
{
	BOOL bHaveLinks = lex.Matched(T_LINKS);
	if (bHaveLinks && !lex.Match(T_BEGIN)) return FALSE;

	if (m_bPlayBack && m_pViewSymbolTable && m_pViewSymbolTable->GetSize() == 0 )
	{
		lex.SkipToToken(T_END, TRUE, TRUE);
		return TRUE;
	}

	lex.RemoveCommentTrace(); //remove previous comments

	BOOL bOk = TRUE;
	while 
		(
			lex.Matched(T_SELECT) || //compatibilita sintassi r5
			lex.LookAhead(T_LINKFORM) ||
			lex.LookAhead(T_LINKREPORT) ||
			lex.LookAhead(T_LINKRADAR) ||
			lex.LookAhead(T_LINKFUNCTION)||
			lex.LookAhead(T_LINKURL)
		)
	{
		WoormLink* pConn = new WoormLink(m_pViewSymbolTable);

		bOk = pConn->Parse(lex);
		
		if (!bOk) 
		{
			delete pConn;
			return FALSE;
		}

		if (pConn->m_LinkType == WoormLink::ConnectionRadar)
		{
			if (m_pConnectionRadar)
			{
				if (m_pConnectionRadar->m_LinkType != WoormLink::ConnectionEmpty)
				{
					TRACE("ConnectionLink per Radar duplicato\n");
					ASSERT(FALSE);
				}
				else
					delete m_pConnectionRadar;
			}
			m_pConnectionRadar = pConn;
		}
		Add(pConn);
	}
	if (bHaveLinks && !lex.Match(T_END)) return FALSE;
	return bOk;
}

//------------------------------------------------------------------------------
void WoormLinkArray::Unparse(Unparser& oFile) const
{
	if (GetSize())
	{
		oFile.UnparseTag(T_LINKS);
		oFile.UnparseBegin();
		for (int i=0; i < GetSize(); i++)
		{
			GetAt(i)->Unparse(oFile);
		}
		oFile.UnparseEnd();
	}
	oFile.UnparseCrLf();
}

//-----------------------------------------------------------------------------
WoormLink* WoormLinkArray::GetFromID (int nID) const
{
	WoormLink* pConn;
	for (int i = 0; i < GetSize(); i++)
	{
		pConn = GetAt(i);
		if (pConn->m_nAlias == nID)
			return pConn;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL WoormLinkArray::CanDeleteField (LPCTSTR szName, CString& sLog) const
{
	for (int i = 0; i < GetSize(); i++)
	{
		WoormLink* pConn = GetAt(i);
		if (!pConn->CanDeleteField(szName, sLog))
			return FALSE;
	}
	return TRUE;
}

//==============================================================================
