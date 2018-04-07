
#include "stdafx.h"

#include <ctype.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ContextFunctions.h>

#include <TbGenlib\generic.h>
#include <TBGenlib\baseapp.h>
#include <TbGenlib\FunProto.h>
#include <TbGenlib\expr.h>

#include <TbGes\TBHtmlParser.h>

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\TBNamespaces.h>

#include <TbParser\Parser.h>
#include <TbParser\SymTable.h>

#include "MultiLayout.h"
#include "repengin.h"
#include "reptable.h"
#include "repfield.h"
#include "rpsymtbl.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//============================================================================
//          WoormField Class implementation
//============================================================================
IMPLEMENT_DYNAMIC(WoormField, SymField)

//----------------------------------------------------------------------------
WoormField::WoormField
	(
		const CString& strName, 
		RepFieldType ft,
		const DataType& dataType,
		WORD nAlias, 
		DataObj* pValue, 
		BOOL bCloneValue
	)
    :
    SymField (strName, dataType, nAlias, pValue, bCloneValue),
	
	m_FieldType		(ft),

	m_bHidden		(FALSE),
	m_bReInit		(FALSE),
	m_bStatic		(FALSE),
	m_bAsk			(FALSE),
	m_bMultiline	(FALSE),
	m_bLowerLimit	(FALSE),
	m_bUpperLimit	(FALSE),
	m_nLen				(0),
    m_nNumDec			(0),

	m_pInitExpression	(NULL),

	m_strPhysicalName	(strName),

	m_bIsExprField		(FALSE),
	m_bIsTableField		(FALSE),
	m_bIsSpecialField	(FALSE),

	m_bNativeColumnExpr (FALSE),

	m_bNoXml			(FALSE),
	m_bOwnThreadContextVar (FALSE),

    m_bRuleDataUpdated	(FALSE),
    m_bReadOnly			(FALSE),
	m_bQryDataUpdated	(FALSE),
	m_pQryData			(NULL),
	m_bRepDataUpdated	(FALSE),
	m_pRepData			(NULL),
    m_pOwnerRule		(NULL),
	m_pGroupFunction	(NULL),
	m_bDisplayed		(FALSE),
	m_AccumKind			(0),
	m_pDisplayTable		(NULL),
	m_pOwnRepField		(NULL),
	m_pEventFunction	(NULL)
{
	if (pValue == NULL)
		AllocData();

	SetFieldType(ft);

	if (dataType != DataType::Null && !IsReadOnly())
	{
		m_pQryData = DataObj::DataObjCreate(dataType);
		m_pRepData = DataObj::DataObjCreate(dataType);
	}
}

//----------------------------------------------------------------------------
WoormField::WoormField
				(
					WoormField* pClone,
					WORD nAlias/* = SpecialReportField::NO_INTERNAL_ID*/
				)
	: SymField(*pClone)
{
	SetId(nAlias);
	m_strName = m_strName + ::cwsprintf(L"_%d", nAlias);

	m_FieldType		= pClone->m_FieldType;

	m_bHidden		= pClone->m_bHidden;

	//TODO non  clona le info dei campi di askdialog (perchè probabilmente non è stata clonata)
	//m_bAsk		= FALSE;// pClone->m_bAsk;
	//m_bReInit		= FALSE;// pClone->m_bReInit;
	//m_bStatic		= FALSE;// pClone->m_bStatic;
	//m_bLowerLimit	= FALSE;// pClone->m_bLowerLimit;
	//m_bUpperLimit	= FALSE;// pClone->m_bUpperLimit;

	m_bMultiline	= pClone->m_bMultiline;
	m_nLen			= pClone->m_nLen;                 // they need for eventual store on
	m_nNumDec		= pClone->m_nNumDec;              // temporary file

	m_pInitExpression = pClone->m_pInitExpression
		? new Expression(*pClone->m_pInitExpression)
		: NULL;

	m_pEventFunction = pClone->m_pEventFunction
		? new EventFunction(*pClone->m_pEventFunction)
		: NULL;
	if (pClone->m_pEventFunction && 
		!pClone->m_pEventFunction->GetPublicName().IsEmpty() &&
		m_strName.Find(pClone->m_pEventFunction->GetPublicName()) == 0
		)
		m_pEventFunction->SetPublicName(m_strName);

	//m_bNativeColumnExpr		= pClone->m_bNativeColumnExpr;
	//m_strPhysicalName		= pClone->m_strPhysicalName;

	m_sXmlAliasName			= pClone->m_sXmlAliasName.IsEmpty() ? L"" : pClone->m_sXmlAliasName + ::cwsprintf(L"_%d", nAlias);
	m_bNoXml				= pClone->m_bNoXml;

	m_sThreadContextVar		= pClone->m_sThreadContextVar;
	m_bOwnThreadContextVar	= pClone->m_bOwnThreadContextVar;

	m_bIsExprField			= pClone->m_bIsExprField;
	//m_bIsTableField			= pClone->m_bIsTableField;
	m_bIsSpecialField		= pClone->m_bIsSpecialField;
	m_strDisplayTable		= pClone->m_strDisplayTable;

	//TODO non  clona le info di Engine
	//RuleItem
	//m_bRuleDataUpdated = FALSE;
	//m_bReadOnly = FALSE;
	//m_pOwnerRule = NULL;          // only used to resolve order by expression
 //  //QueryItem
	//m_pQryData = NULL;
	//m_bQryDataUpdated = FALSE;
	//m_pGroupFunction = NULL;		// property of GroupBy
	//// WoormField
	//m_bDisplayed = FALSE;
	//m_AccumKind = 0;
	//m_bRepDataUpdated = FALSE;
	//m_pRepData = NULL;
	//m_pOwnRepField = NULL;		// WoormField Owner (only for SubTotal/ColTotal fields)
	//m_Substrings;
	//m_pDisplayTable = NULL;	// Display Table Owner

}

//----------------------------------------------------------------------------
WoormField::~WoormField()
{
	SAFE_DELETE (m_pInitExpression);
	SAFE_DELETE (m_pEventFunction);

	if (!IsReadOnly())	// se e` read only esiste una sola copia di dato posseduta dal WoormField
	{
		SAFE_DELETE (m_pRepData);
		SAFE_DELETE (m_pQryData);
	}
}

//----------------------------------------------------------------------------
WoormTable*  WoormField::GetWoormTable()	const
 { return dynamic_cast<WoormTable*>(GetSymTable()); }

//----------------------------------------------------------------------------
void WoormField::SetDataType (const DataType& dataType, BOOL bArray/* = FALSE*/)
{
	__super::SetDataType (dataType, bArray);

	if (!IsReadOnly())	// se e` read only esiste una sola copia di dato posseduta dal WoormField
	{
		SAFE_DELETE (m_pRepData);
		SAFE_DELETE (m_pQryData);
		
		if (dataType != DataType::Null)
		{
			m_pQryData = DataObj::DataObjCreate(dataType);
			m_pRepData = DataObj::DataObjCreate(dataType);
		}
	}
	else
	{
		m_pRepData = m_pQryData = __super::GetData();
	}
}

//----------------------------------------------------------------------------
CString	WoormField::GetSourceDescription()
{
	CString s;
	if (IsTableRuleField())
	{
		switch(GetFieldType())
		{ 
			case WoormField::RepFieldType::FIELD_COLUMN:
				s = _TB("DB column");
				break;
			case WoormField::RepFieldType::FIELD_COLTOTAL:
				s = _TB("DB total");
				break;
			case WoormField::RepFieldType::FIELD_SUBTOTAL:
				s = _TB("DB subtotal");
				break;
			default:
				s = _TB("DB field");
		}
	}
	else if (IsAsk())
		return GetFieldType() == WoormField::RepFieldType::FIELD_COLUMN ? _TB("Request column") : _TB("Request field");
	else if (IsInput())
		return GetFieldType() == WoormField::RepFieldType::FIELD_COLUMN ? _TB("Input column") : _TB("Input field");
	else if (IsExprRuleField())
	{
		switch (GetFieldType())
		{
		case WoormField::RepFieldType::FIELD_COLUMN:
			s = _TB("Expression column");
			break;
		case WoormField::RepFieldType::FIELD_COLTOTAL:
			s = _TB("Expression total");
			break;
		case WoormField::RepFieldType::FIELD_SUBTOTAL:
			s = _TB("Expression subtotal");
			break;
		default:
			s = _TB("Expression field");
		}
	}
	else
	{
		switch (GetFieldType())
		{
		case WoormField::RepFieldType::FIELD_COLUMN:
			s = _TB("Function column");
			break;
		case WoormField::RepFieldType::FIELD_COLTOTAL:
			s = _TB("Function total");
			break;
		case WoormField::RepFieldType::FIELD_SUBTOTAL:
			s = _TB("Function subtotal");
			break;
		default:
			s = _TB("Function field");
		}
	}
	return s;
}

//----------------------------------------------------------------------------
WoormField::SourceFieldType	WoormField::GetSourceEnum()
{
	if (m_bIsTableField)
		return WoormField::SourceFieldType::DB_FIELD;
	else if (IsAsk())
		return WoormField::SourceFieldType::ASK_FIELD;
	else if (IsInput())
		return WoormField::SourceFieldType::INPUT_FIELD;
	else if (IsExprRuleField())
		return WoormField::SourceFieldType::EXPRESSION_FIELD;
	else if (IsColTotal())
		return WoormField::SourceFieldType::TOTAL_FIELD;
	else
		return WoormField::SourceFieldType::FUNCTION_FIELD;
}

//----------------------------------------------------------------------------
void WoormField::SetFieldType(RepFieldType ft)
{
	m_FieldType = ft;

	if (m_FieldType != FIELD_INPUT)
	{
		SetAsk(FALSE);
		SetReInit(FALSE);
		SetStatic(FALSE);
	}
	else
		SetReadOnly();
}

 //----------------------------------------------------------------------------
void WoormField::SetDataPtr (DataObj* pData, BOOL bOwnData/* = TRUE*/)
{
	__super::SetDataPtr (pData, bOwnData);

	if (m_FieldType == FIELD_INPUT)
		m_pRepData = m_pQryData = __super::GetData();
}

 

//----------------------------------------------------------------------------
void WoormField::UpdateQryData ()
{
	if (IsARuleItem() && IsRuleUpdated())
    {
		ASSERT(m_pQryData);
		// validation is in DataObj
		m_pQryData->Assign(*GetRuleData());
		m_bQryDataUpdated = TRUE;
	}
}

//----------------------------------------------------------------------------
DataObj* WoormField::GetData (int nDataLevel /*= -1*/) const
{
	//ASSERT(m_pRepData);
	if (nDataLevel >= 0)
	{
		switch(nDataLevel)
		{
			case REPORT_ENGINE:
				return m_pRepData;
			case QUERY_ENGINE:
				return m_pQryData;
			case RULE_ENGINE:
				return __super::GetData();
		}
	}
	if (GetSymTable())
	{
		switch(GetSymTable()->GetDataLevel())
		{
			case REPORT_ENGINE:
				return m_pRepData;
			case QUERY_ENGINE:
				return m_pQryData;
			case RULE_ENGINE:
				return __super::GetData();
		}
	}
	return __super::GetData();
}

//----------------------------------------------------------------------------
Expression::MessageID WoormField::Init ()
{
	if (!m_sThreadContextVar.IsEmpty() && !m_bOwnThreadContextVar)
		return Expression::EMPTY_MESSAGE;

	ASSERT_VALID(m_pRepData);
	ASSERT_VALID(m_pQryData);
	ASSERT_VALID(GetRuleData());

	if (m_pInitExpression)
	{
		if (!m_pInitExpression->Eval(*m_pRepData))
			return m_pInitExpression->GetErrId();
		
		InitAllDataLevel();
	}
	else
	{
		if (this->GetId() != SpecialReportField::ID.OWNER)
		{
			ClearAllDataLevel();
		}
	}

	return Expression::EMPTY_MESSAGE;
}

//----------------------------------------------------------------------------
Expression::MessageID  WoormField::Init (DataObj* pRepData)
{
	if (!m_sThreadContextVar.IsEmpty() && !m_bOwnThreadContextVar)
		return Expression::EMPTY_MESSAGE;

	ASSERT(m_pRepData && pRepData);
	if (DataType::IsCompatible(pRepData->GetDataType(), GetDataType()))
		m_pRepData->Assign(*pRepData);
	else if (pRepData->IsKindOf(RUNTIME_CLASS(DataStr)))
		m_pRepData->Assign(((DataStr*)pRepData)->Str());
	else
	{
		ASSERT_TRACE1(FALSE, "WoormField::Init (DataObj* pRepData): datatype incompatible for field %s\n", GetName());
		return Expression::CHECKTYPE;
	}

	InitAllDataLevel();

	return Expression::EMPTY_MESSAGE;
}

//----------------------------------------------------------------------------
void WoormField::SetReadOnly()
{
	if (m_bReadOnly)
		return;
	ASSERT(!m_pRepData || m_pRepData != SymField::GetData());

	m_bReadOnly = TRUE;

	// nel caso sia un campo di imput si tiene un puntatore allo stesso
	// DataObj che verra` cancellato dal distruttore della classe SymField
	SAFE_DELETE (m_pRepData)
	SAFE_DELETE (m_pQryData)
 
	m_pRepData = m_pQryData = SymField::GetData();
}

//----------------------------------------------------------------------------
void WoormField::InitAllDataLevel()
{
	ASSERT_VALID(m_pRepData);
	ASSERT_VALID(m_pQryData);
	ASSERT_VALID(GetRuleData());

	// the caller has stored a new value in the RepData
	ReportUpdated();

	GetQryData()->Assign(*m_pRepData);
	QueryUpdated();

	GetRuleData()->Assign(*m_pRepData);
	RuleUpdated();
}

//----------------------------------------------------------------------------
void WoormField::ClearAllDataLevel()
{
	ASSERT_VALID(m_pRepData);
	ASSERT_VALID(m_pQryData);
	ASSERT_VALID(GetRuleData());

	m_pRepData->Clear();
	ReportUpdated();

	GetQryData()->Clear();
	QueryUpdated();

	GetRuleData()->Clear();
	RuleUpdated();
}

//----------------------------------------------------------------------------
void WoormField::SetData  (DataObj& aValue)
{
	if (!m_sThreadContextVar.IsEmpty() && !m_bOwnThreadContextVar)
		return;

	ASSERT_VALID(m_pRepData);

	if (DataType::IsCompatible(aValue.GetDataType(), m_pRepData->GetDataType()))
		m_pRepData->Assign(aValue);
	else if (aValue.IsKindOf(RUNTIME_CLASS(DataStr)))
		m_pRepData->Assign(((DataStr*)&aValue)->Str());
	else
	{
		ASSERT_TRACE1(FALSE, "WoormField::SetData (DataObj* pRepData): datatype incompatible for field %s\n", GetName());
	}

	ReportUpdated();
}

//----------------------------------------------------------------------------
void WoormField::UpdateRepData ()
{
	if (IsARuleItem() && IsQueryUpdated())
	{
		ASSERT(m_pRepData);
		GetRepData()->Assign(*GetQryData());
		QryDataProcessed();
		m_bRepDataUpdated = TRUE;
	}
}

//----------------------------------------------------------------------------
BOOL WoormField::SplitString(const CString& strData, CWoormDoc *pWoormDocument)
{
	m_Substrings.RemoveAll();

	int nStrLen = strData.GetLength();
	if (nStrLen <= 0)
		return FALSE;

	int nMaxLen = pWoormDocument 
		? pWoormDocument->CalculateFieldWidth(GetId(), strData) 
		: GetLen();
	
	ASSERT(nMaxLen >= 0);
	if (nMaxLen <= 0)
		nMaxLen = GetLen();
	ASSERT(nMaxLen > 0);
	if (nMaxLen <= 0)
		nMaxLen = 1;

	int nStartChar = 0;
	int nRealChars;

	while (nStartChar < nStrLen)
	{
		for	(
				nRealChars = 0;
				nRealChars < nMaxLen && (nStartChar + nRealChars) < nStrLen;
				nRealChars++
			)
			if (strData[nStartChar + nRealChars] == LF_CHAR)
			{
				// se si esce invece per naturale terminazione del loop
				// nRealChars e` gia` incrementato
				nRealChars++;
				break;
			}

		// mi posiziono sul carattere successivo a quello teoricamente ultimo
		// (male che vada e` il '\0' di terminazione)
		int nIdx = nStartChar + nRealChars;
		ASSERT(nIdx > 0);
		if (nIdx < nStrLen && strData[nIdx - 1] != LF_CHAR)
		{
			if (!_istspace(strData[nIdx]))
			{
				// se mi trovo ancora su un carattere vado a sinistra cercando il primo
				// separatore
				while (nIdx > nStartChar && !_istspace(strData[nIdx - 1]))
					nIdx--; 

				// se la parola e` troppo lunga si utilizzano
				// tutti gli nRealChars calcolati precedentemente
				if (nIdx == nStartChar)
					nIdx = nStartChar + nRealChars;
			}
			else
			{
				// se mi trovo su un blank vado a destra cercando il primo
				// carattere buono
				while (nIdx < nStrLen && _istspace(strData[nIdx]))
					nIdx++;
			}
		}

		// numero di caratteri totali della sotto-stringa
		nRealChars = nIdx - nStartChar;
		nIdx--;

		// strip blank, CR e LF
		while (nIdx >= nStartChar && _istspace(strData[nIdx]))
			nIdx--;

		if (nIdx >= nStartChar)
			m_Substrings.Add(strData.Mid(nStartChar, nIdx - nStartChar + 1));
		else
			m_Substrings.Add(_T(""));

		nStartChar += nRealChars;
	}

	if (m_Substrings.GetSize() != 0)
		return TRUE;

	return FALSE;
}

//----------------------------------------------------------------------------
#define CHAR_EURO 0x20AC

BOOL WoormField::SplitStringOptimized(CWoormDoc *pWoormDocument)
{
	m_Substrings.RemoveAll();

	if (pWoormDocument == NULL)
		return SplitString(((DataStr*)GetRepData())->GetString(), NULL);

	if (!pWoormDocument->UseOptimizedLineBreak())
		return SplitString(((DataStr*)GetRepData())->GetString(), pWoormDocument);

	FieldWidthFactors fieldWidthFactors;
	if (!pWoormDocument->GetFieldWidthFactors(GetId(), fieldWidthFactors, IsSubTotal()))
	{
		ASSERT(FALSE);
		return SplitString(((DataStr*)GetRepData())->GetString(), pWoormDocument);
	}
	//---------------------------------

	fieldWidthFactors.m_nWidth = max(1, int(fieldWidthFactors.m_nWidth * pWoormDocument->GetColumnWidthPercentage()));

	//----
	CString strData;	//dopo conterr� la formattazione
	if (fieldWidthFactors.m_pFormatStyle)
	{
		fieldWidthFactors.m_pFormatStyle->FormatDataObj (*GetRepData(), strData);
	}
	else strData = ((DataStr*)GetRepData())->GetString();

	if (fieldWidthFactors.m_bMiniHtml)
	{
		return SplitStringWithMiniHtml(pWoormDocument, fieldWidthFactors, strData);
	}

	//--------------------------------

	int nStrLen = strData.GetLength();
	if (nStrLen <= 0)
		return FALSE;

	int nAvgLen = pWoormDocument->CalculateFieldWidth(GetId(), strData);
	ASSERT(nAvgLen >= 0);
	if (nAvgLen <= 0)
		nAvgLen = GetLen();
	ASSERT(nAvgLen > 0);
	if (nAvgLen <= 0)
		nAvgLen = 1;

	//----
	CString sData(strData); sData.Remove('\r');
	TCHAR* nextToken;
	TCHAR* pszData = sData.GetBuffer(sData.GetLength());
	TCHAR *pLine = _tcstok_s(pszData, _T("\n"), &nextToken);
	CString sLine, sHeadLine;
	int nLenHeadLine;
	while (pLine != NULL)
	{
		sLine = pLine;
		//----
		while (!sLine.IsEmpty())
		{
			nLenHeadLine = min(nAvgLen, sLine.GetLength());
			sHeadLine = sLine.Left(nLenHeadLine);

			//try to extend length: head line is all lowercase and it is shorter than actual column width
			while (nLenHeadLine < sLine.GetLength())
			{
				int cx = fieldWidthFactors.m_pFontStyle->GetStringWidth2 (fieldWidthFactors.m_pDC, sHeadLine);
				if (cx >= fieldWidthFactors.m_nWidth)
					break;

				nLenHeadLine++;
				sHeadLine = sLine.Left(nLenHeadLine);
			}
			//reduce length to fit actual column width: head line is all uppercase and it is longer than actual column width
			while (nLenHeadLine > 1)
			{
				int cx = fieldWidthFactors.m_pFontStyle->GetStringWidth2 (fieldWidthFactors.m_pDC, sHeadLine);
				if (cx < fieldWidthFactors.m_nWidth)
					break;

				nLenHeadLine--;
				sHeadLine = sLine.Left(nLenHeadLine);
			}
			if (nLenHeadLine == sLine.GetLength())
			{
				m_Substrings.Add(sHeadLine);
				break;
			}
			ASSERT(sLine != sHeadLine);

			//length fit actual column width: now it searches a point to left to break line
			int nLenBreak = nLenHeadLine;
			int nLenSpecialChar = -1;
			//TRACE((LPCTSTR)::cwsprintf(L"\nbefore split\n" + sHeadLine +"\n"));
			for (
					;
					nLenBreak > 0  &&
					(
						//TODO leggere da setting altri caratteri su cui NON brekkare
						(
							_istalnum(sLine[nLenBreak - 1])
							|| 
							sLine[nLenBreak - 1] == '/'
						)
						||
						(
							nLenBreak > 1 
							&&
							_istdigit(sLine[nLenBreak - 2])
							&&
							wcschr(L".,-:/", sLine[nLenBreak - 1])
							&&
							_istdigit(sLine[nLenBreak])
						)
						||
						(
							nLenBreak < (sLine.GetLength() - 1)
							&&
							_istdigit(sLine[nLenBreak - 1])
							&&
							wcschr(L".,-:/", sLine[nLenBreak])
							&&
							_istdigit(sLine[nLenBreak + 1])
							)
						||
						(
							_istdigit(sLine[nLenBreak])
							&&
							wcschr(L"€£$", sLine[nLenBreak - 1])
						)
						||
						(
							_istdigit(sLine[nLenBreak - 1])
							&&
							wcschr(L"€&$", sLine[nLenBreak])
						)
					);
					nLenBreak--
				)
			{
				//It is better brutal break on special char than breaking on the last char
				if (nLenSpecialChar < 0 && wcschr(L".,-:/€&$", sLine[nLenBreak]))
					nLenSpecialChar = min (nLenBreak + 1, nLenHeadLine);
			}

			//if there is not a proper break point it get all head line: it makes a brutal break
			if (nLenBreak == 0) 
				nLenBreak = nLenSpecialChar > -1 ? nLenSpecialChar : nLenHeadLine;

			sHeadLine = sLine.Left(nLenBreak);
			//TRACE((LPCTSTR)::cwsprintf(L"\nafter split\n" + sHeadLine + "\n"));

		//	sHeadLine.Trim();
			if (!sHeadLine.IsEmpty())
				m_Substrings.Add(sHeadLine);

			//---- loop on tail line
			sLine = sLine.Mid(nLenBreak);
		}
		//----
		pLine = _tcstok_s(NULL, _T("\n"), &nextToken);
	}
	//----
	sData.ReleaseBuffer();
	if (m_Substrings.GetSize() != 0)
		return TRUE;

	return FALSE;
}

//----------------------------------------------------------------------------
BOOL WoormField::SplitStringWithMiniHtml(CWoormDoc*, FieldWidthFactors& fieldWidthFactors, CString strValue)
{
	if (!fieldWidthFactors.m_pDC || fieldWidthFactors.m_nWidth <= 0)
		return FALSE;

	CFont font;
	CFont* old_pFont = NULL;
	if (fieldWidthFactors.m_pFontStyle)
	{
		if (fieldWidthFactors.m_pFontStyle->CreateFont(font))
			old_pFont = fieldWidthFactors.m_pDC->SelectObject(&font);
	}

	TBHtmlParser htmlParser;
	htmlParser.HTMLDcStringSplitter(fieldWidthFactors.m_pDC, CRect(0, 0, fieldWidthFactors.m_nWidth, 20), strValue, &m_Substrings);

	if (old_pFont)
		fieldWidthFactors.m_pDC->SelectObject(old_pFont);

	return m_Substrings.GetSize() > 0;
}

//----------------------------------------------------------------------------
BOOL WoormField::SplitImageToMultiline(CWoormDoc *pWoormDocument, int h, int w, CTBPicture::ImageFitMode efit)
{
	m_Substrings.RemoveAll();

	CString sFileName = ((DataStr*)GetRepData())->GetString();

	if (!sFileName.IsEmpty())
	{
		CTBPicture bitmap;
		if (bitmap.ReadFile(sFileName, TRUE))
		{
			//int w = bitmap.GetWidth();
			int hbmp = bitmap.GetHeight();
			int top = 0;
			int bottom = 0;
			while (hbmp > h)
			{
				bottom = top + h;
				m_Substrings.Add(cwsprintf(L"%s:%d,%d", (LPCTSTR)sFileName, top, bottom));
				top = bottom;
				hbmp -= h;
			}
			if (hbmp > 0)
			{
				bottom = top + hbmp;
				m_Substrings.Add(cwsprintf(L"%s:%d,%d", (LPCTSTR)sFileName, top, bottom));
			}
		}
	}
	return m_Substrings.GetSize() > 0;
}

//----------------------------------------------------------------------------
BOOL WoormField::Display (RepEngine& aRepEngine)
{
	if (GetDataType() == DataType::Array) 
		return WriteArray(aRepEngine);

	if (IsHidden()) 
		return TRUE;

	if (m_Substrings.GetSize() > 0 && (GetDisplayTableEngine() && GetDisplayTableEngine()->m_pDT && GetDisplayTableEngine()->m_pDT->m_nMultiLineFieldsCurrLine == 0))
	{
		// ho fatto prima una display di un campo multiline che non � stata consumata da un WriteLine
		// -> ripulisco variabili
		m_Substrings.RemoveAll();
		GetDisplayTableEngine()->m_pDT->m_nMultiLineFieldsNum--;
	}

	if (
		!aRepEngine.GetCallerDoc()->ExistsColumnID (GetId(), aRepEngine.GetCallerDoc()->m_dsCurrentLayoutEngine)
		&&
		!aRepEngine.GetCallerDoc()->ExistsID (GetId(), aRepEngine.GetCallerDoc()->m_dsCurrentLayoutEngine)
		)
		return TRUE;

	// per i campi liberi ed i totali di colonna di tipo non si esegue la rasterizzazione
	if (!GetDisplayTable() || IsColTotal())
		return Write(aRepEngine);

	if (m_Substrings.GetSize() > 0 || GetDisplayTableEngine()->m_pDT->m_nMultiLineFieldsCurrLine > 0)
	{
		// tentativo di visualizzare una colonna che si sta rasterizzando gia` o mentre si e`
		// in fase di rasterizzazione avanzata (cioe` dalla seconda riga in poi)
//@@TODO		aRepEngine.ResetRTError();
//@@TODO		return aRepEngine.SetRTError(_TB("Unable to carry out the Display action for the field while displaying multi-line fields."), 0, GetPublicName());
		TRACE(EngineScheduler::FormatMessage(EngineScheduler::REPORT_DISPLAY_WHILE_RASTER_FIELDS) + GetName() + '\n');
		ASSERT(FALSE);
		return TRUE;
	}

	// Si "rasterizza" la stringa corrente se la colonna e` multiline (GetNumDec() > 0)
	if (!IsRepDataNull() && GetRepData()->IsKindOf(RUNTIME_CLASS(DataStr)) && GetNumDec() > 0)
	{
		BOOL bShowedAsImage = aRepEngine.GetCallerDoc()->ColumnShowedAsImage(GetId(), aRepEngine.GetCallerDoc()->m_dsCurrentLayoutEngine) ;
		CObject* pColPtr = bShowedAsImage ? aRepEngine.GetCallerDoc()->GetColumnPtr(GetId(), aRepEngine.GetCallerDoc()->m_dsCurrentLayoutEngine) : NULL;

		if (::SendMessage(aRepEngine.GetOwnRepEngine()->m_hComunicationWnd, UM_GET_SPLITTED_STRING, (WPARAM)pColPtr, (LPARAM) this) == 1)
			GetDisplayTableEngine()->m_pDT->m_nMultiLineFieldsNum++; // si incrementa il numero di colonne rasterizzate
	}

	return Write(aRepEngine);
}

//----------------------------------------------------------------------------
BOOL WoormField::IsTailMultiLineString () const 
{
	return m_Substrings.GetSize() > 0 && GetDisplayTable() &&  
			GetDisplayTableEngine()->m_pDT->m_nMultiLineFieldsCurrLine > 0;	
}

//----------------------------------------------------------------------------
DisplayTableEntryEngine* WoormField::GetDisplayTableEngine() const
{ 
	DisplayTableEntryEngine* p = dynamic_cast<DisplayTableEntryEngine*>(m_pDisplayTable);
	ASSERT_VALID(p);
	return p;
}

//----------------------------------------------------------------------------
BOOL WoormField::WriteArray(RepEngine& aRepEngine)
{
	DataArray* pData = dynamic_cast<DataArray*>(GetRepData());
	ASSERT_VALID(pData);
	if (!pData) return FALSE;

	RDEData tmpRDEData(IRDEManager::ARRAY_DATA, pData->ToString());

	if (aRepEngine.GetOutChannel()->Write(- GetId(), tmpRDEData, FALSE))
		return TRUE;

	EngineScheduler::MessageID nErrExpl = aRepEngine.GetRTErrorCode();
	LPCTSTR pszString = aRepEngine.GetRTErrorFailedName();

	if (pszString == NULL) pszString = GetName();

	aRepEngine.ResetRTError();
	return aRepEngine.SetRTError(EngineScheduler::REPORT_DISP_FIELD, EngineScheduler::FormatMessage(nErrExpl), pszString);
}

//----------------------------------------------------------------------------
BOOL WoormField::Write (RepEngine& aRepEngine, BOOL bWriteSpecial /*= FALSE*/)
{
	if (GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID && !bWriteSpecial)
		return TRUE;

	if (GetDataType() == DataType::Array)
		return WriteArray(aRepEngine);

	// se e` un campo colonnare e sto "rasterizzando" la riga della tabella con 
	// numero di riga raster successive alla prima scrivo solo se ho delle sotto-stringhe
	// ancora da smaltire
	if	(
			!IsColTotal() &&
			GetDisplayTable() && GetDisplayTableEngine()->m_pDT->m_nMultiLineFieldsCurrLine > 0 &&
			m_Substrings.GetSize() == 0
		)
		return TRUE;

	BOOL bIsTailMultiLineString = IsTailMultiLineString();

	DataObj* pData = NULL;
	DataStr dataStr;
	RDEData tmpRDEData;

	// sending a ColTotal it behaves like a sending last nextLine
	// and then the owner table must exist
	//
	if (GetDisplayTable())
		if (IsColTotal())
			GetDisplayTableEngine()->SetOverflow();
		else
			if (!GetDisplayTableEngine()->ExecOverflowActions())
				goto writeError;

	if (GetDisplayTable() && m_Substrings.GetSize() != 0)
	{
		int nMultiLineFieldsCurrLine = GetDisplayTableEngine()->m_pDT->m_nMultiLineFieldsCurrLine;

		// ci si e` protetti gia` prima sull'esaurimento delle sotto-stringhe, ... ma � meglio rifarlo!
		if (nMultiLineFieldsCurrLine <= m_Substrings.GetUpperBound())
		{
			dataStr = m_Substrings.GetAt(nMultiLineFieldsCurrLine);
			pData = &dataStr;
		}
		//else ASSERT(FALSE);
		else ASSERT(nMultiLineFieldsCurrLine == m_Substrings.GetSize());	

		// se e` stata scritta l'ultima sotto-stringa si decrementa il numero
		// di campi multilinea della tabella che si devono ancora scrivere
		if (nMultiLineFieldsCurrLine >= m_Substrings.GetUpperBound())
		{
			GetDisplayTableEngine()->m_pDT->m_nMultiLineFieldsNum--;

			m_Substrings.RemoveAll();
		}
	}
	else
		if (!IsRepDataNull())
			pData = GetRepData();

	if (pData)
	{
		if (IsLowerLimit() && pData->IsLowerValue())
		{
			if (!aRepEngine.GetOutChannel()->Write(GetId(), RDEManager::INPUT_LOWER_DATA, 0))
				goto writeError;

			return TRUE;
		}

		if (IsUpperLimit())
		{
			if (pData->IsUpperValue())
			{
				if (!aRepEngine.GetOutChannel()->Write(GetId(), RDEManager::INPUT_UPPER_DATA, 0))
					goto writeError;

				return TRUE;
			}
			else if (pData->GetDataType() == DATA_STR_TYPE)
			{
				DataStr aDataStr(*(DataStr*)pData);
				aDataStr.TrimUpperLimit();

				tmpRDEData.SetData(m_AccumKind, &aDataStr);

				if (!aRepEngine.GetOutChannel()->Write(GetId(), tmpRDEData))
					goto writeError;

				return TRUE;
			}
		}
	}

	if (pData && pData->GetDataType() == DATA_STR_TYPE)
	{
		ASSERT(((DataStr*)pData)->GetString().GetLength() < 0x7FFF);
	}

	tmpRDEData.SetData(m_AccumKind, pData, bIsTailMultiLineString);

	if (!aRepEngine.GetOutChannel()->Write(GetId(), tmpRDEData))
		goto writeError;

	if (!IsColTotal() && GetDisplayTable())
		GetDisplayTableEngine()->SetDataDisplayed();

	return TRUE;

writeError:
	EngineScheduler::MessageID nErrExpl = aRepEngine.GetRTErrorCode();
	LPCTSTR pszString = aRepEngine.GetRTErrorFailedName();

	if (pszString == NULL) pszString = GetName();

	aRepEngine.ResetRTError();
	return aRepEngine.SetRTError(EngineScheduler::REPORT_DISP_FIELD, EngineScheduler::FormatMessage (nErrExpl), pszString);
}

//----------------------------------------------------------------------------
void WoormField::ReattachDisplayTable (const CString& sLayout)
{
	if (!m_pDisplayTable) 
		return;
	ASSERT_VALID(GetSymTable());
	int idx = GetSymTable()->GetDisplayTables()->Find(m_pDisplayTable->GetId(), sLayout);
	if (idx >= 0)
	{
		m_pDisplayTable = GetSymTable()->GetDisplayTables()->GetAt(idx);
	}
	//else	//succede se il campo � una colonna di una tabella che non appartiene al layout in cui si sta switcciando
	//	ASSERT(FALSE);
}

//----------------------------------------------------------------------------
BOOL WoormField::ParseDisplayAttribute (Parser& parser, WoormTable* pSymTable)
{
	if (parser.Matched(T_HIDDEN)) 
		m_bHidden = TRUE;
	
	if (parser.Matched(T_REINIT)) 
	{
		m_bReInit = TRUE;

		if(parser.LookAhead() != T_INPUT)
			return parser.SetError(_TB("Illegal attribute REINIT for the field"), GetName());
	}
	else if (parser.Matched(T_STATIC)) 
	{
		m_bStatic = TRUE;

		if(parser.LookAhead() != T_INPUT)
			return parser.SetError(_TB("Illegal attribute STATIC for the field"), GetName());
	}

	CString	strTmp;
	Token	nToken = parser.LookAhead();
	switch (nToken)
	{
		case T_INPUT :
		{
			parser.SkipToken();

			SetFieldType(FIELD_INPUT);

			if (m_pEventFunction != NULL)
				return parser.SetError(_TB("Illegal function for the field"), GetName());

			return TRUE;
		}
		case T_COLUMN :
		{
			//NB:Nei report Intrastat/AnnualIntraDispatched2A/Quaterly...pu� essere preceduto da Hidden
			parser.SkipToken();
			m_strDisplayTable.Empty();

			SetFieldType(FIELD_COLUMN);

			BOOL bUnnamed;
			m_pDisplayTable = pSymTable->MatchDisplayTable(parser, bUnnamed);
			if (!m_pDisplayTable)
				return FALSE;

			m_strDisplayTable = m_pDisplayTable->GetTableName();

			m_pDisplayTable->AddColumn(this);

			return TRUE;
		}
		case T_SUBTOTAL :
		case T_COLTOTAL :
		{
			parser.SkipToken();
			// the internal id is used to identify the correct subtotal/coltotal

			if (nToken == T_SUBTOTAL)
			{
				SetFieldType(FIELD_SUBTOTAL);

				m_AccumKind = RDEData::GetSubTotalKind((WORD)GetId());
			}
			else //if (nToken == T_COLTOTAL)
			{
				SetFieldType(FIELD_COLTOTAL);

				m_AccumKind = RDEData::GetColTotalKind((WORD)GetId());
			}

			if (m_pEventFunction == NULL)
				return parser.SetError(_TB("No function is associated to the field Total"), GetName());

			// if it isn't a expression function we can omit the owner column name
			CString strName;
			if (parser.Matched(T_OF))
				parser.ParseID(strName);

			WoormField* pRepField = NULL;
			if (m_pEventFunction->IsAnExpression())
			{
				if (strName.IsEmpty())
					return parser.SetError(_TB("A field name is expected"));

				pRepField = (WoormField*) pSymTable->GetField(strName);
				if (pRepField == NULL)
					return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strName);
			}
			else
			{
				if (!strName.IsEmpty() && strName.CompareNoCase(m_pEventFunction->GetPublicName()) != 0)
					return parser.SetError(_TB("Field Total with different function parameter and column name"), strName);

				pRepField = m_pEventFunction->GetFuncItem();

				if (!strName.IsEmpty() && _tcsicmp(pRepField->GetName(), strName))
					return parser.SetError(_TB("Field Total with different function parameter and column name"), strName);

				strName = pRepField->GetName();
			}
			
			m_pEventFunction->SetPublicName(strName);

			if (!pRepField->IsColumn())
				return parser.SetError(_TB("The associated field is not a column"), strName);

			if (pRepField->IsHidden()) 
				m_bHidden = TRUE;

			if (pSymTable->GetTableType() == WoormTable::RepTable_ENGINE)
				SetId(pRepField->GetId());

			m_pOwnRepField = pRepField;

			m_pDisplayTable	= pRepField->m_pDisplayTable;
			if (m_pDisplayTable)
				m_pDisplayTable->AddColumn(this);

			return TRUE;
		}
	}
	return !parser.ErrorFound();
}

//----------------------------------------------------------------------------
BOOL WoormField::Parse (Parser& parser, WoormTable* aSymTable)
{
	ASSERT_VALID(aSymTable);

	DataType aType, aBaseType;
	if (!parser.ParseDataType(aType, aBaseType))
		return parser.SetError(_TB("Wrong datatype"));

	if (aType == DataType::Array)
	{
		SetFieldType(FIELD_INPUT);
		
		SetDataType(aBaseType, TRUE);
	}	
	else
		SetDataType(aType);

	CString sName;
	if (!parser.ParseID(sName))
		return parser.SetError(_TB("Expected Field Name"));
	if (aSymTable->ExistField(sName))
		return parser.SetError(_TB("Field already defined"), sName);
	
	SetName(sName);

	if (parser.LookAhead(T_SQUAREOPEN))
		if (!parser.ParseSubscr(m_nLen, m_nNumDec))
			return FALSE;

	switch (GetDataType().m_wType)
	{
		case DATA_INT_TYPE:  
		case DATA_LNG_TYPE:  
		case DATA_DBL_TYPE:	
			if (m_nLen > 0 && m_nNumDec >= m_nLen)
				return parser.SetError(EngineScheduler::FormatMessage(EngineScheduler::REPORT_ILLEGAL_PRECISION));
			break;
		case DATA_STR_TYPE:  
		case DATA_TXT_TYPE:	
			if (m_nNumDec > 0)
				m_bMultiline = TRUE;
			break;
	}

	//Aggiungo il  campo alla Symbol Table
	aSymTable->Add(this);						
	//----

	if (parser.Matched(T_ASSIGN))
	{
		m_pEventFunction = new EventFunction(aSymTable, *this);
		if (!m_pEventFunction->Parse(parser)) 
		{
			SAFE_DELETE(m_pEventFunction);
			return FALSE;
		}
	}

	// Match of ALIAS identifier
	WORD wInternalId;
	if (!parser.ParseAlias(wInternalId)) 
		return parser.SetError(EngineScheduler::FormatMessage(EngineScheduler::REPORT_ILLEGAL_ALIAS));

	if (wInternalId <= 0)
		return parser.SetError(EngineScheduler::FormatMessage(EngineScheduler::REPORT_ILLEGAL_ALIAS));
	else
	{
		SymField* pF = aSymTable->GetFieldByID(wInternalId);
		if (pF != NULL && pF != this)
		{
			ASSERT(pF);
			CString strTmp = cwsprintf(_T("%s %d"), pF->GetName(), wInternalId);
			return parser.SetError(_TB("Internal ID identifier already used"), strTmp);
		}
		else
		{
			SetId(wInternalId);
		}
	}

	if (parser.Matched(T_AS))
		parser.ParseID(m_sXmlAliasName);
	m_bNoXml = parser.Matched(T_NO_XML);

	if (parser.Matched(T_CONTEXT))
	{
		parser.ParseString(m_sThreadContextVar);

		ASSERT(__super::GetData());

		m_bOwnThreadContextVar = !CheckContextObject(AfxGetThreadContextBag(), m_sThreadContextVar);
		if (!m_bOwnThreadContextVar && aSymTable->GetDocument() && aSymTable->GetDocument()->GetSymTable())
		{
			//cerco nella symbol table di ViewMode che viene parsata per prima con gli oggetti grafici
			SymField* pF = aSymTable->GetDocument()->GetSymTable()->GetFieldByID(wInternalId);
			if (pF && pF->OwnThreadContextVar())
				m_bOwnThreadContextVar = TRUE;	//� dello stesso report
		}

		DataObj* pThData = AddContextDataObj(AfxGetThreadContextBag(), m_sThreadContextVar, __super::GetData()->GetRuntimeClass(), FALSE);

		if (pThData)
		{
			SetFieldType(FIELD_INPUT);
			SetDataPtr (pThData, FALSE);
		}
		else
		{
			ASSERT_TRACE1(FALSE, "Fallito bind a variabile di contesto di thread %s\n", (LPCTSTR)m_sThreadContextVar);
		}
	}

	// Match of display attribute
	if (!ParseDisplayAttribute(parser, aSymTable))
		return FALSE;

	// Match INIT expression
	if (parser.Matched(T_INIT))
	{
		if (!parser.ParseTag(T_ASSIGN))
			return FALSE;

		AddMethodList(parser);

		m_pInitExpression = new Expression(aSymTable);
		if (!m_pInitExpression->Parse(parser, GetDataType(), TRUE))
			return FALSE;
	}

	return parser.ParseSep() && !parser.ErrorFound();
}

//----------------------------------------------------------------------------
void WoormField::SetEventFunction
	(
		SymTable*	aSymTable, 
		LPCTSTR			pszColumnName, 
		Token			aFuncType
	)
{
	SAFE_DELETE (m_pEventFunction);

	if (aFuncType != T_NOTOKEN)
		m_pEventFunction = new EventFunction(aSymTable, *this);

	m_pEventFunction->SetFunctionType(aFuncType);
	m_pEventFunction->SetPublicName(pszColumnName);
}

//----------------------------------------------------------------------------
void WoormField::SetEventFunction
	(
		SymTable*	aSymTable, 
		LPCTSTR			pszColumnName, 
		LPCTSTR			pszExpr,
		const DataType&	aDataType
   )
{
	SAFE_DELETE (m_pEventFunction);

	m_pEventFunction = new EventFunction(aSymTable, *this, pszExpr, aDataType);

	m_pEventFunction->SetPublicName(pszColumnName);
}

//----------------------------------------------------------------------------
void WoormField::SetEmptyEventFunction
(
	SymTable*	aSymTable,
	LPCTSTR			pszFieldName
)
{
	SAFE_DELETE(m_pEventFunction);

	m_pEventFunction = new EventFunction(aSymTable, *this);

	m_pEventFunction->SetPublicName(pszFieldName);
}

//----------------------------------------------------------------------------
BOOL WoormField::CanDeleteField(LPCTSTR pszFieldName)
{
	if (m_pEventFunction)
	{       
		BOOL bTotal = (IsSubTotal() || IsColTotal());
		       
		// if current field isn't a total or else isn't a total of deleting field,
		// we check function expression
		if (!bTotal || _tcsicmp(m_pEventFunction->GetPublicName(), pszFieldName) != 0)
			if (!m_pEventFunction->CanDeleteField(pszFieldName))
				return FALSE;
	}
		
	if (m_pInitExpression &&  !m_pInitExpression->IsEmpty())
		return !m_pInitExpression->HasMember(pszFieldName);

	return TRUE;
}

//----------------------------------------------------------------------------
void WoormField::UnparseDisplayAttribute(Unparser& oFile, BOOL bNoDispTableName)
{
	CString	defaultDispTbl;

	if (m_bHidden)
   	{
        oFile.UnparseBlank	();
		oFile.UnparseTag	(T_HIDDEN, FALSE);
	}

	if (m_bReInit)
   	{
        oFile.UnparseBlank	();
		oFile.UnparseTag	(T_REINIT, FALSE);
	}
	else if (m_bStatic)
   	{
        oFile.UnparseBlank	();
		oFile.UnparseTag	(T_STATIC, FALSE);
	}
	
	if (IsInput())
	{
		oFile.UnparseBlank	();
		oFile.UnparseTag	(T_INPUT, FALSE);

		return;
	}

	if (IsColumn())
    {
		oFile.UnparseBlank	();
		oFile.UnparseTag	(T_COLUMN, FALSE);

		if (!bNoDispTableName)
		{
			if (!m_strDisplayTable.IsEmpty())
			{
				oFile.UnparseBlank	();
				oFile.UnparseTag	(T_OF, 				FALSE);
				oFile.UnparseID		(m_strDisplayTable,	FALSE);
			}
		}
		return;
    }
                                                                                          
	if (IsSubTotal() || IsColTotal())
	{
    	oFile.UnparseBlank();
		if (IsSubTotal())
			oFile.UnparseTag(T_SUBTOTAL,	FALSE);
		else
			oFile.UnparseTag(T_COLTOTAL,	FALSE);
	                                   
        // if it isn't an expression function we omit the owner column pszName
		if (m_pEventFunction && m_pEventFunction->IsAnExpression())
		{
			oFile.UnparseTag(T_OF,							FALSE);
			oFile.UnparseID	(m_pEventFunction->GetPublicName(),	FALSE);
		}
	}
}

//----------------------------------------------------------------------------
void WoormField::UnparseDataType(Unparser& oFile, BOOL bIndent)
{
	DataType dt	= GetDataType();

	// si esegue un "pseudo" incolonnamento tra tipo e nome
	if (GetData())
	{
		if (dt == DataType::Array)
		{
			//TODO manca gestione array di DataType::Record
			DataArray* pArray = (DataArray*)GetData();

			oFile.UnparseDataType(dt, pArray->GetBaseDataType(), FALSE, bIndent);

			if (pArray->GetBaseDataType() == DataType::Record)
			{
				if (pArray->GetSize() > 0)
				{
					DataSqlRecord* pDRec = dynamic_cast<DataSqlRecord*>(pArray->GetAt(0));
					if (pDRec->GetIRecord())
						oFile.Write(pDRec->GetIRecord()->GetTableName(), FALSE);
					else
						ASSERT_TRACE(pDRec->GetIRecord(), "Array di record vuoto: non riesce a deserializzare il table name");
				}
				else
					ASSERT_TRACE(pArray->GetSize() > 0, "Array di record vuoto: non riesce a deserializzare il table name");
			}
		}
		else if (dt == DataType::Record)
		{
			CString sRecName;
			DataObj* pObj = this->GetData();
			ASSERT_VALID(pObj);
			ASSERT_KINDOF(DataSqlRecord, pObj);
			if (pObj)
			{
				DataSqlRecord* pDR = dynamic_cast<DataSqlRecord*>(pObj);
				if (pDR->GetIRecord())
				{
					sRecName = pDR->GetIRecord()->GetTableName();
				}
				else ASSERT(FALSE);
			}

			oFile.UnparseDataType(dt, FALSE, bIndent, sRecName);
		}
		else
		{
			oFile.UnparseDataType(dt, DataType::Null, FALSE, bIndent);

			if (dt == DATA_ENUM_TYPE)
			{
				oFile.Write	
					(
						_T(" /* ") + AfxGetEnumsTable()->GetEnumTagName(dt.m_wTag)  + _T(" */ "),
						FALSE
					);
			}
		}
	}
	else
	{
		ASSERT(FALSE);
		oFile.UnparseDataType(DataType::String, DataType::Null, FALSE, bIndent);
	}
}

//----------------------------------------------------------------------------
void WoormField::Unparse(Unparser& oFile, BOOL bNoDispTableName)
{
	if (IsPredefinedField())
		return;

	UnparseDataType(oFile);

	oFile.UnparseID(GetName(), FALSE);

	if (m_nLen > 0 || m_nNumDec > 0)
		oFile.UnparseSubscr (m_nLen, m_nNumDec, FALSE);

	if (m_pEventFunction && !m_pEventFunction->IsEmpty())
	{
		oFile.UnparseTag(T_ASSIGN, FALSE);
		m_pEventFunction->Unparse(oFile);
	}

	oFile.UnparseAlias	(GetId(), FALSE);

	if (!m_sXmlAliasName.IsEmpty())
	{
		oFile.UnparseTag(T_AS, FALSE);
		oFile.UnparseID(m_sXmlAliasName, FALSE);
	};
	if (m_bNoXml)
		oFile.UnparseTag(T_NO_XML, FALSE);

	if (!m_sThreadContextVar.IsEmpty())
	{
		oFile.UnparseTag(T_CONTEXT, FALSE);
		oFile.UnparseString(m_sThreadContextVar, FALSE);
	};

	UnparseDisplayAttribute	(oFile, bNoDispTableName);

	if (GetInitExpression() && !GetInitExpression()->IsEmpty())
	{
		oFile.UnparseTag(T_INIT, 		FALSE);
		oFile.UnparseTag(T_ASSIGN, 		FALSE);
		oFile.UnparseExpr(GetInitExpression()->ToString(),	FALSE);
	}

	oFile.UnparseSep(TRUE);
}

//----------------------------------------------------------------------------
BOOL WoormField::AddMethodList(Parser& parser)
{
	if (
			(GetDataType() == DataType::Object || GetDataType() == DataType::Long)
			&& 
			parser.LookAhead(T_ID)
		)
	{
		CString strFuncName = parser.GetCurrentStringToken();

		CFunctionDescription aFuncPrototype;
		CTBNamespace ns1(CTBNamespace::FUNCTION, strFuncName);
		BOOL bOk = AfxGetTbCmdManager()->GetFunctionDescription(ns1, aFuncPrototype, FALSE);
		if (bOk)
		{
			CDataObjDescription& ret = aFuncPrototype.GetReturnValueDescription();
			if (ret.GetDataType() == DataType::Object)
			{
				CString sClassName = ret.GetClassType();
				AddMethods(sClassName, AfxGetAddOnAppsTable()->GetMapWebClass());
				return TRUE;
			}
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
BOOL WoormField::AddMethodList (const CRuntimeClass* rcStopBaseClassName)
{
	return AddMethods(rcStopBaseClassName, AfxGetAddOnAppsTable()->GetMapWebClass());
}

//-----------------------------------------------------------------------------
BOOL WoormField::SetInitExpression	(LPCTSTR szExpr)
{
	ASSERT(szExpr);
	ASSERT_VALID(this->GetSymTable());

	Parser lex(szExpr);
	lex.LookAhead(); // initialize lexan inner status
	Expression* pInitExpr = new Expression (GetSymTable());
	if (pInitExpr->Parse(lex, GetDataType(), TRUE))
	{
		SetInitExpression(pInitExpr);
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
CString WoormField::BindRuleItem(const RecordArray& aBindRecord, const SqlTableInfoArray& arSqlTableJoinInfoArray)
{
	DataObj* pDataObj = GetRuleData();
	if (pDataObj == NULL)
	{
		ASSERT(FALSE);
		return Expression::FormatMessage(Expression::UNKNOW);
	}

	SqlRecord* pRec = NULL;
	CString strName = GetPhysicalName();

	if (IsNativeColumnExpr())
	{
		int nLen = GetLen();

		pDataObj->Allocate(nLen);
		if (pDataObj->GetDataType() == DataType::String)
			nLen = 2048;

		pRec = aBindRecord.GetAt(0);

		SqlColumnInfo* pColInfo = new SqlColumnInfo(pRec->GetTableName(), strName, *pDataObj);
		pColInfo->m_bVirtual = FALSE;
		pColInfo->m_bVisible = TRUE;
		pColInfo->m_bNativeColumnExpr = TRUE;
		pColInfo->m_lLength = nLen;
		pColInfo->UpdateDataObjType(pDataObj);
		pColInfo->SetDataObjInfo(pDataObj);

		strName = cwsprintf(_T("NativeColumnExpr_Alias%d"), GetId());

		SqlRecordItem* pRecItem = new SqlRecordItem(pDataObj, strName, pColInfo);
		pRecItem->m_bOwnColumnInfo = TRUE;
		pRecItem->m_lLength = nLen;

		pRec->Add(pRecItem);

		return _T("");
	}

	const SqlColumnInfo* pColumnInfo = NULL;
	int nDot = strName.Find(DOT_CHAR);
	if (nDot == -1)
	{
		for (int i = 0; i < aBindRecord.GetSize(); i++)
		{
			pRec = aBindRecord.GetAt(i);
			pColumnInfo = pRec->GetTableInfo()->GetColumnInfo(strName);
			if (pColumnInfo)
				break;
		}
	}
	else
	{
		CString strAlias = strName.Left(nDot);
		strName = strName.Mid(nDot + 1);
		for (int i = 0; i < arSqlTableJoinInfoArray.m_arstrAliasTableName.GetSize(); i++)
		{
			if (strAlias.CompareNoCase(arSqlTableJoinInfoArray.m_arstrAliasTableName.GetAt(i)))
				continue;

			pRec = i < aBindRecord.GetSize() ? aBindRecord.GetAt(i) : NULL;
			pColumnInfo = pRec->GetTableInfo()->GetColumnInfo(strName);
			break;
		}
		if (pColumnInfo == NULL)
		{
			for (int i = 0; i < aBindRecord.GetSize(); i++)
			{
				pRec = aBindRecord.GetAt(i);
				if (strAlias.CompareNoCase(pRec->GetTableName()))
					continue;

				pColumnInfo = pRec->GetTableInfo()->GetColumnInfo(strName);
				if (pColumnInfo)
					break;
			}
		}
	}

	if (pColumnInfo == NULL || pRec == NULL)
		return Expression::FormatMessage(Expression::UNKNOWN_DB_COL_FIELD);

	//@@DataStr to DataText
	//problema campi database varchar trasformati in NText e mappati nel wrm come string e non longstring
	//dobbiamo renderli compatibili
	//confronto il tipo del DataObj allocato con il tipo letto dalle columninfo
	int len = pColumnInfo->GetColumnLength();
	if
		(
		pDataObj->GetDataType() == DataType::String
		&&
		(
		pColumnInfo->GetDataObjType() == DataType::Text
		||
		(pColumnInfo->GetDataObjType() == DataType::String && len > 4000)
		||
		(pColumnInfo->GetDataObjType() == DataType::Null && len > 4000)
		)
		)
	{
		//delete pDataObj;
		pDataObj = DataObj::DataObjCreate(DataType::Text);
		SetRuleData(pDataObj);
	}

	// Prealloca la stringa per avere lo spazio opportuno da utilizzare nella BindCol
	pDataObj->Allocate(len);

	CWordArray dataTypes;
	VERIFY(pColumnInfo->GetDataObjTypes(dataTypes));

	int j;
	for (j = 0; j <= dataTypes.GetUpperBound(); j++)
		if (pDataObj->GetDataType().m_wType == dataTypes[j])
			break;
	if (j > dataTypes.GetUpperBound())
		return _TB("Incompatible fields");

	const_cast<SqlColumnInfo*>(pColumnInfo)->UpdateDataObjType(pDataObj);
	pColumnInfo->SetDataObjInfo(pDataObj);

	pRec->Add(new SqlRecordItem(pDataObj, strName, pColumnInfo));

	return _T("");
}

///////////////////////////////////////////////////////////////////////////////
