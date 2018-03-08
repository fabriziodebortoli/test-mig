
#include "stdafx.h"

#include <TBGenlib\baseapp.h>

#include "datafunc.h"
#include "repfield.h"
#include "reptable.h"

#include "qryengin.h"
#include "repengin.h"

#include "report.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//---------------------------------------------------------------------------
DataFunction::DataFunction(SymTable* pSymTable)
    :
    Expression		(pSymTable),
    m_pFuncItem		(NULL),
    m_nOccurrence	(0),
	m_FunctionType	(T_NOTOKEN)
{
}

//---------------------------------------------------------------------------
BOOL DataFunction::Eval()
{
    if (IsAnExpression())
    {
        if (!Expression::Eval(GetAccData()))
			return GetErrId() == NULL_OPR;		//@@TODO NON BLOCCA L'ESECUZIONE !!!

		SetAccData(GetAccData());
		return TRUE;
    }

	if (!GetData().IsValid() || !GetAccData().IsValid())
	{
		ErrHandler(Expression::NULL_OPR);
		return TRUE;		//@@TODO NON BLOCCA L'ESECUZIONE !!!
	}

	if (!IsUpdated(*m_pFuncItem))
		return TRUE;

	m_nOccurrence++;

	switch (m_FunctionType)
	{
		case T_CCAT :
		{
			if	(
					GetAccData().IsKindOf(RUNTIME_CLASS(DataStr)) &&
					GetData().IsKindOf(RUNTIME_CLASS(DataStr))
				)
			{
				SetAccData  
					(
						DataStr (GetAccData().Str() + CString(" ") + GetData().Str())
					);

				return TRUE;
			}
			return FALSE;
		}

		case T_CSUM :
		{
			if (GetAccData().IsKindOf(RUNTIME_CLASS(DataStr)))
			{
				SetAccData(DataStr (GetAccData().Str() + GetData().Str()));
				return TRUE;
			}
			else if (GetAccData().IsKindOf(RUNTIME_CLASS(DataInt)))
			{
				SetAccData(DataInt((int) ((DataInt&) GetAccData()) +
								   (int) ((DataInt&) GetData())));
				return TRUE;
			}					
			else if (GetAccData().IsKindOf(RUNTIME_CLASS(DataLng)))
			{
				long nVal = 0;
				if (GetData().IsKindOf(RUNTIME_CLASS(DataInt)))
					nVal = (int) ((DataInt&) GetData());
				else if (GetData().IsKindOf(RUNTIME_CLASS(DataLng)))
					nVal = (long) ((DataLng&) GetData());
				else return FALSE;

				SetAccData(DataLng((long) ((DataLng&) GetAccData()) + nVal));
				return TRUE;
			}					
			else if (GetAccData().IsKindOf(RUNTIME_CLASS(DataDbl)))
			{
				double nVal = 0.0;
				if (GetData().IsKindOf(RUNTIME_CLASS(DataInt)))
					nVal = (int) ((DataInt&) GetData());
				else if (GetData().IsKindOf(RUNTIME_CLASS(DataLng)))
					nVal = (long) ((DataLng&) GetData());
				else if (GetData().IsKindOf(RUNTIME_CLASS(DataDbl)))
					nVal = (double) ((DataDbl&) GetData());
				else return FALSE;
					
				SetAccData(DataDbl((double) ((DataDbl&) GetAccData()) + nVal));
				return TRUE;
			}

			return FALSE;
		}

		case T_CMIN :
			if (GetAccData().IsGreaterThan(GetData()))
				SetAccData(GetData());
			return TRUE;

		case T_CMAX :
			if (GetAccData().IsLessThan(GetData()))
				SetAccData(GetData());
			return TRUE;

		case T_CLAST:
			SetAccData(GetData());
			return TRUE;

		case T_CFIRST:
			if (this->m_nOccurrence == 1)
				SetAccData(GetData());
			return TRUE;

		case T_CCOUNT:
		{
			if (GetAccData().IsKindOf(RUNTIME_CLASS(DataLng)))
				SetAccData(DataLng(m_nOccurrence));
			else if (GetAccData().IsKindOf(RUNTIME_CLASS(DataInt)))
				SetAccData(DataInt(m_nOccurrence));
			else if (GetAccData().IsKindOf(RUNTIME_CLASS(DataDbl)))
				SetAccData(DataDbl(m_nOccurrence));
			else return FALSE;

			return TRUE;
		}

		case T_CAVG:
		{
			if (GetAccData().IsKindOf(RUNTIME_CLASS(DataInt)))
			{
				short m = (short)(DataInt&)GetAccData();
				short v = (short)(DataInt&)GetData();

				SetAccData(DataInt(short(m + (v - m) / double(m_nOccurrence))));
				return TRUE;
			}

			if (GetAccData().IsKindOf(RUNTIME_CLASS(DataLng)))
			{
				long m = (long)(DataLng&)GetAccData();

				long v = 0;
				if (GetData().IsKindOf(RUNTIME_CLASS(DataInt)))
					v = (int)((DataInt&)GetData());
				else if (GetData().IsKindOf(RUNTIME_CLASS(DataLng)))
					v = (long)((DataLng&)GetData());
				else return FALSE;

				SetAccData(DataLng(long(m + (v - m) / double(m_nOccurrence))));
				return TRUE;
			}

			if (GetAccData().IsKindOf(RUNTIME_CLASS(DataDbl)))
			{
				double m = (double)(DataDbl&)GetAccData();

				double v = 0.0;
				if (GetData().IsKindOf(RUNTIME_CLASS(DataInt)))
					v = (int)((DataInt&)GetData());
				else if (GetData().IsKindOf(RUNTIME_CLASS(DataLng)))
					v = (long)((DataLng&)GetData());
				else if (GetData().IsKindOf(RUNTIME_CLASS(DataDbl)))
					v = (double)((DataDbl&)GetData());
				else return FALSE;

				SetAccData(DataDbl(m + (v - m) / double(m_nOccurrence)));
				return TRUE;
			}

			return FALSE;
		}
	} // switch

    return FALSE;
}

//---------------------------------------------------------------------------
void DataFunction::ResetOccurrence()
{
	m_nOccurrence = 0;
}

//---------------------------------------------------------------------------
BOOL DataFunction::ParseFunction(Parser& parser, DataObj* pAccData)
{
    if (!parser.LookForAFunc())
		return parser.SetError(EngineScheduler::FormatMessage(EngineScheduler::QUERY_UNKNOWN_FUNCTION));;

    m_FunctionType = parser.SkipToken();

    CString strItemName;
    if (parser.ParseItem(strItemName))
    {
        m_pFuncItem = (WoormField*) m_pSymTable->GetField(strItemName);
        if (m_pFuncItem == NULL)
            return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strItemName);

        if (m_pFuncItem->IsReadOnly())
            return parser.SetError(_TB("Input field are not allowed in this context"), strItemName);
        
		DataType fromType = m_pFuncItem->GetDataType();
		DataType toType = pAccData ? pAccData->GetDataType() : DataType::Null;

		switch (m_FunctionType)
		{
			case T_CCAT :
			{
				if (fromType == DATA_STR_TYPE)
				{
					if (toType == DATA_NULL_TYPE)	return TRUE;
					if (toType == DATA_STR_TYPE)	return TRUE;
				}
				break;
			}
			case T_CCOUNT:
			{
				switch (toType)
				{
					case DATA_NULL_TYPE:
					case DATA_INT_TYPE:
					case DATA_LNG_TYPE:
					case DATA_DBL_TYPE:
					case DATA_MON_TYPE:
					case DATA_QTA_TYPE:
					case DATA_PERC_TYPE:
						return TRUE;
				} 					
				break;
			}

			case T_CMIN :
			case T_CMAX :
				if (fromType == DATA_ENUM_TYPE)
				{
					if (toType == DATA_NULL_TYPE)	return TRUE;
					if (toType == DATA_ENUM_TYPE)	return TRUE;
					break;
				}

			case T_CLAST:
			case T_CFIRST:
					if (fromType == DATA_DATE_TYPE)
					{
						if (toType == DATA_NULL_TYPE)	return TRUE;
						if (toType == DATA_DATE_TYPE)	return TRUE;
						break;
					}
				// prosegue nel successivo
			case T_CAVG:
			case T_CSUM :
				{
					switch (fromType.m_wType)
					{
					case DATA_STR_TYPE:
						if (toType == DATA_NULL_TYPE)	return TRUE;
						if (toType == DATA_STR_TYPE)	return TRUE;
						break;

					case DATA_INT_TYPE:
						if (toType == DATA_NULL_TYPE)	return TRUE;
						if (toType == DATA_INT_TYPE)	return TRUE;
						if (toType == DATA_LNG_TYPE)	return TRUE;
						if (toType == DATA_DBL_TYPE)	return TRUE;
						if (toType == DATA_MON_TYPE)	return TRUE;
						if (toType == DATA_QTA_TYPE)	return TRUE;
						if (toType == DATA_PERC_TYPE)	return TRUE;
						break;

					case DATA_LNG_TYPE:
						if (toType == DATA_NULL_TYPE)	return TRUE;
						if (toType == DATA_LNG_TYPE)	return TRUE;
						if (toType == DATA_DBL_TYPE)	return TRUE;
						if (toType == DATA_MON_TYPE)	return TRUE;
						if (toType == DATA_QTA_TYPE)	return TRUE;
						if (toType == DATA_PERC_TYPE)	return TRUE;
						break;

					case DATA_DBL_TYPE:
						if (toType == DATA_NULL_TYPE)	return TRUE;
						if (toType == DATA_DBL_TYPE)	return TRUE;
						if (toType == DATA_MON_TYPE)	return TRUE;
						if (toType == DATA_QTA_TYPE)	return TRUE;
						if (toType == DATA_PERC_TYPE)	return TRUE;
						break;

					case DATA_MON_TYPE:
						if (toType == DATA_NULL_TYPE)	return TRUE;
						if (toType == DATA_DBL_TYPE)	return TRUE;
						if (toType == DATA_MON_TYPE)	return TRUE;
						break;

					case DATA_QTA_TYPE:
						if (toType == DATA_NULL_TYPE)	return TRUE;
						if (toType == DATA_DBL_TYPE)	return TRUE;
						if (toType == DATA_QTA_TYPE)	return TRUE;
						break;

					case DATA_PERC_TYPE:
						if (toType == DATA_NULL_TYPE)	return TRUE;
						if (toType == DATA_DBL_TYPE)	return TRUE;
						if (toType == DATA_PERC_TYPE)	return TRUE;
						break;
				} // switch fromType.m_wType
			}

            default  : return parser.SetError(EngineScheduler::FormatMessage(EngineScheduler::QUERY_UNKNOWN_FUNCTION));
		}// switch
            
        return parser.SetError(EngineScheduler::FormatMessage(EngineScheduler::QUERY_INCOMPATIBLE_FUNCTION_OPERAND), strItemName);
     }
    return FALSE;
}

//---------------------------------------------------------------------------
BOOL DataFunction::ParseExpression(Parser& parser, WoormField* pDstItem)
{
    if (parser.ErrorFound()) 
		return FALSE;

    if (pDstItem == NULL) 
		return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD));

    if (pDstItem->IsReadOnly())
        return parser.SetError(_TB("Input field are not allowed in aggregate function"), pDstItem->GetName());

    m_FunctionType = T_NOTOKEN;
    m_pFuncItem = pDstItem;

    return Parse(parser, pDstItem->GetDataType(), TRUE);
}

//===========================================================================
//
//              Class EventFunction implementation
//
//===========================================================================

//---------------------------------------------------------------------------
EventFunction::EventFunction (SymTable* pSymTable, WoormField& AccField)
	:
	DataFunction		(pSymTable),
	m_AccumulatorField	(AccField)
{}

//----------------------------------------------------------------------------
EventFunction::EventFunction(SymTable* pSymTable, WoormField& AccField, LPCTSTR pszExpr, const DataType& aDataType)
	:
    DataFunction		(pSymTable),
	m_AccumulatorField	(AccField)
{
	Parser lex(pszExpr);
	Expression::Parse(lex, aDataType, TRUE);
}

//---------------------------------------------------------------------------
DataObj& EventFunction::GetData () const
{ 
	ASSERT(GetFuncItem() && GetFuncItem()->GetRepData());
	return *(GetFuncItem()->GetRepData()); 
}

//---------------------------------------------------------------------------
DataObj& EventFunction::GetAccData	() const 
{ 
	return *(m_AccumulatorField.GetRepData()); 
}

//---------------------------------------------------------------------------
BOOL EventFunction::IsUpdated	(WoormField& anItem)	const
{
	// the Event Functions are applyed after WoormField update
	return ((WoormField&) anItem).HasValidRepData();
}

//---------------------------------------------------------------------------
void EventFunction::SetAccData	(DataObj& aValue)
{ 
	m_AccumulatorField.SetData(aValue); 
}

//---------------------------------------------------------------------------
BOOL EventFunction::Parse(Parser& parser)
{
 	m_FunctionType = T_NOTOKEN;

	if (parser.LookForAFunc())
	{
		BOOL bOk = DataFunction::ParseFunction(parser, m_AccumulatorField.GetRuleData());
		if (bOk)
		{
			if (!GetFuncItem())
			{
				ASSERT(FALSE);
				return FALSE;
			}
			m_strPublicName = GetFuncItem()->GetName();
		}
		return bOk;
	}
			
	m_AccumulatorField.AddMethodList(parser);

	SetStopTokens(T_ALIAS);
	return DataFunction::ParseExpression(parser, &m_AccumulatorField);
}

//----------------------------------------------------------------------------
BOOL EventFunction::CanDeleteField(LPCTSTR pszFieldName)
{
	if (IsAnExpression())
		return !HasMember(pszFieldName);
	else
		return m_strPublicName.CompareNoCase(pszFieldName);
}

//----------------------------------------------------------------------------
void EventFunction::DeleteField(LPCTSTR pszFieldName)
{
	if (!IsAnExpression() && m_strPublicName.CompareNoCase(pszFieldName) == 0)
	{
		m_strPublicName.Empty();
		m_FunctionType = T_NOTOKEN;
    }
}

//----------------------------------------------------------------------------
BOOL EventFunction::IsEmpty()
{
 	return (m_FunctionType == T_NOTOKEN && __super::IsEmpty());
}

//----------------------------------------------------------------------------
void EventFunction::Reset(BOOL bResetAll /*= TRUE*/)
{
 	m_FunctionType = T_NOTOKEN;
 	__super::Reset(bResetAll);
}

//----------------------------------------------------------------------------
void EventFunction::UnparsePublicName(Unparser& oFile)
{
	ASSERT(!m_strPublicName.IsEmpty());

	oFile.UnparseOpen	(FALSE);
    oFile.UnparseID		(m_strPublicName,	FALSE);
	oFile.UnparseClose	(FALSE);
}

//----------------------------------------------------------------------------
void EventFunction::UnparseFunction(Unparser& oFile)
{
	oFile.UnparseTag(m_FunctionType, FALSE);
	UnparsePublicName(oFile);
}

//----------------------------------------------------------------------------
void EventFunction::UnparseExpression(Unparser& oFile)
{
	oFile.UnparseExpr(__super::ToString(), FALSE);
}

//----------------------------------------------------------------------------
void EventFunction::Unparse(Unparser& oFile)
{
	if (IsAnExpression())
		UnparseExpression(oFile);
	else
		UnparseFunction(oFile);
}

//-----------------------------------------------------------------------------
CString EventFunction::ToString(BOOL /*bUseAudit*/)
{
	Unparser unparse(TRUE);
	Unparse(unparse);
	unparse.Close();

	CString str = unparse.GetBufferString();
	return str;
}

//===========================================================================
//
//              Class GroupFunction implementation
//
//===========================================================================
DataObj& GroupFunction::GetData () const
{
	return *(GetFuncItem()->GetRuleData());
}

DataObj& GroupFunction::GetAccData () const
{
	return *(GetFuncItem()->GetQryData());
}

BOOL GroupFunction::IsUpdated (WoormField& item) const
{
	return item.HasValidRuleData();
}

//---------------------------------------------------------------------------
void GroupFunction::SetAccData(DataObj& aData)
{
    (GetFuncItem()->GetQryData())->Assign(aData);
    GetFuncItem()->QueryUpdated();
}

//---------------------------------------------------------------------------
BOOL GroupFunction::Parse(Parser& parser)
{
    if (parser.LookAhead(T_ID))
    {
        CString strItemName;

        if (!parser.ParseID(strItemName) || !parser.ParseTag(T_ASSIGN)) 
        	return FALSE;

        WoormField* pAccItem = (WoormField*) m_pSymTable->GetField(strItemName);
        if (pAccItem == NULL)
            return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strItemName);

        // pAccItem will stored in thePFuncItem DataFunction member
        if (!DataFunction::ParseExpression(parser, pAccItem)) 
        	return FALSE;
    }
    else
        if (!DataFunction::ParseFunction(parser, NULL)) 
        	return FALSE;
        	
    //if (!GetFuncItem()->IsARuleItem()/* && this->m_FunctionType != T_CCOUNT*/)
    //    return parser.SetError(_TB("Field has to valorized by rule"), GetFuncItem()->GetName());

    if (GetFuncItem()->GetGroupFunction() != NULL)
        return parser.SetError(_TB("Grouping function already associated to the field"), GetFuncItem()->GetName());

    GetFuncItem()->SetGroupFunction(this);

    return TRUE;
}

//---------------------------------------------------------------------------
BOOL GroupFunction::CheckRuleItems(CString& sErr)
{
	if (!GetFuncItem()->IsARuleItem())
	{
		sErr = cwsprintf(_TB("Field {0-%s} has to valorized by rule"), GetFuncItem()->GetName());
		return FALSE;
	}
	return TRUE;
}

//============================================================================
// FunctionData                                                          	//
//============================================================================
IMPLEMENT_DYNAMIC(FunctionData, Expression)
//----------------------------------------------------------------------------
FunctionData::FunctionData(SymTable* pSymTable, Token aFunctionType)
	:
    Expression			(pSymTable),
	m_FunctionType		(aFunctionType)
{}

//----------------------------------------------------------------------------
FunctionData::FunctionData(SymTable* pSymTable, LPCTSTR pszExpr, const DataType& aDataType)
	:
    Expression			(pSymTable),
	m_FunctionType		(T_NOTOKEN)
{
	Parser lex(pszExpr);
	Expression::Parse(lex, aDataType, TRUE);
}

//----------------------------------------------------------------------------
BOOL FunctionData::CanDeleteField(LPCTSTR pszFieldName)
{
	if (IsAnExpression())
		return !HasMember(pszFieldName);
	else
		return m_strPublicName.CompareNoCase(pszFieldName);
}

//----------------------------------------------------------------------------
void FunctionData::DeleteField(LPCTSTR pszFieldName)
{
	if (!IsAnExpression() && m_strPublicName.CompareNoCase(pszFieldName) == 0)
	{
		m_strPublicName.Empty();
		m_FunctionType = T_NOTOKEN;
    }
}

//----------------------------------------------------------------------------
BOOL FunctionData::IsEmpty()
{
 	return (m_FunctionType == T_NOTOKEN && __super::IsEmpty());
}

//----------------------------------------------------------------------------
void FunctionData::Reset()
{
 	m_FunctionType = T_NOTOKEN;
 	__super::Reset();
}

//----------------------------------------------------------------------------
BOOL FunctionData::ParseFunction(Parser& lex, const DataType& toType)
{
	m_FunctionType = lex.SkipToken();
	
	if	(lex.ParseItem (m_strPublicName))
	{
    	WoormField* pFuncItem =  (WoormField*) GetSymTable()->GetField(m_strPublicName);
		if (pFuncItem == NULL)
		{
			lex.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), m_strPublicName);
			m_strPublicName.Empty();
            return FALSE;
		}

		if (pFuncItem->IsInput())
			return lex.SetError(_TB("Input field are not allowed in aggregate functions"), m_strPublicName);

		DataType fromType = pFuncItem->GetDataType();

		switch (m_FunctionType)
		{
			case T_CCAT :
			{
				if (fromType == DATA_STR_TYPE)
				{
					if (toType == DATA_NULL_TYPE)	return TRUE;
					if (toType == DATA_STR_TYPE)	return TRUE;
				}
				break;
			}

			case T_CCOUNT:
			{
				switch (toType)
				{
					case DATA_NULL_TYPE:
					case DATA_INT_TYPE:
					case DATA_LNG_TYPE:
					case DATA_DBL_TYPE:
					case DATA_MON_TYPE:
					case DATA_QTA_TYPE:
					case DATA_PERC_TYPE:
						return TRUE;
				} 					
				break;
			}

			case T_CFIRST:
			case T_CLAST:
				if (fromType == DATA_ENUM_TYPE)
				{
					if (toType == DATA_NULL_TYPE)	return TRUE;
					if (toType == DATA_ENUM_TYPE)	return TRUE;
					break;
				}

			case T_CMIN :
			case T_CMAX :
				if (fromType == DATA_DATE_TYPE)
				{
					if (toType == DATA_NULL_TYPE)	return TRUE;
					if (toType == DATA_DATE_TYPE)	return TRUE;
					break;
				}
				// prosegue nel successivo
			case T_CSUM :
			case T_CAVG:
				{
					switch (fromType.m_wType)
					{
						case DATA_STR_TYPE:
							if (toType == DATA_NULL_TYPE)	return TRUE;
							if (toType == DATA_STR_TYPE)	return TRUE;
							break;

						case DATA_INT_TYPE:
							if (toType == DATA_NULL_TYPE)	return TRUE;
							if (toType == DATA_INT_TYPE)	return TRUE;
							if (toType == DATA_LNG_TYPE)	return TRUE;
							if (toType == DATA_DBL_TYPE)	return TRUE;
							if (toType == DATA_MON_TYPE)	return TRUE;
							if (toType == DATA_QTA_TYPE)	return TRUE;
							if (toType == DATA_PERC_TYPE)	return TRUE;
							break;

						case DATA_LNG_TYPE:
							if (toType == DATA_NULL_TYPE)	return TRUE;
							if (toType == DATA_LNG_TYPE)	return TRUE;
							if (toType == DATA_DBL_TYPE)	return TRUE;
							if (toType == DATA_MON_TYPE)	return TRUE;
							if (toType == DATA_QTA_TYPE)	return TRUE;
							if (toType == DATA_PERC_TYPE)	return TRUE;
							break;

						case DATA_DBL_TYPE:
							if (toType == DATA_NULL_TYPE)	return TRUE;
							if (toType == DATA_DBL_TYPE)	return TRUE;
							if (toType == DATA_MON_TYPE)	return TRUE;
							if (toType == DATA_QTA_TYPE)	return TRUE;
							if (toType == DATA_PERC_TYPE)	return TRUE;
							break;

						case DATA_MON_TYPE:
							if (toType == DATA_NULL_TYPE)	return TRUE;
							if (toType == DATA_DBL_TYPE)	return TRUE;
							if (toType == DATA_MON_TYPE)	return TRUE;
							break;

						case DATA_QTA_TYPE:
							if (toType == DATA_NULL_TYPE)	return TRUE;
							if (toType == DATA_DBL_TYPE)	return TRUE;
							if (toType == DATA_QTA_TYPE)	return TRUE;
							break;

						case DATA_PERC_TYPE:
							if (toType == DATA_NULL_TYPE)	return TRUE;
							if (toType == DATA_DBL_TYPE)	return TRUE;
							if (toType == DATA_PERC_TYPE)	return TRUE;
							break;
					} // internal switch	
					break;				
				}
		} // switch

		return lex.SetError(_TB("Incompatible function operand"), m_strPublicName);
	}
	return FALSE;
}

//----------------------------------------------------------------------------
BOOL FunctionData::ParseExpression(Parser& lex, const DataType& aDataType)
{
	m_FunctionType = T_NOTOKEN;

	if (lex.Bad())
		return FALSE;
	
	if (aDataType == DATA_NULL_TYPE)
		return lex.SetError(_TB("Missing field's datatype"));

	SetStopTokens(T_ALIAS);
	return Expression::Parse(lex, aDataType, TRUE);
}

//----------------------------------------------------------------------------
BOOL FunctionData::Parse(Parser& lex, const DataType& aDataType)
{
 	m_FunctionType = T_NOTOKEN;
	return 
		lex.LookForAFunc()
			? ParseFunction(lex, aDataType)
			: ParseExpression(lex, aDataType);
}

//----------------------------------------------------------------------------
void FunctionData::UnparsePublicName(Unparser& oFile)
{
	oFile.UnparseOpen	(FALSE);
    oFile.UnparseID		(m_strPublicName,	FALSE);
	oFile.UnparseClose	(FALSE);
}

//----------------------------------------------------------------------------
void FunctionData::UnparseFunction(Unparser& oFile)
{
	oFile.UnparseTag(m_FunctionType, FALSE);
	UnparsePublicName(oFile);
}

//----------------------------------------------------------------------------
void FunctionData::UnparseExpression(Unparser& oFile)
{
	oFile.UnparseExpr(ToString(), FALSE);
}

//----------------------------------------------------------------------------
void FunctionData::Unparse(Unparser& oFile)
{
	if (IsAnExpression())
		UnparseExpression(oFile);
	else
		UnparseFunction(oFile);
}

//---------------------------------------------------------------------------
BOOL FunctionData::CheckRuleItems(CString& sErr)
{
	WoormField* pFuncItem = (WoormField*)GetSymTable()->GetField(m_strPublicName);

	if (pFuncItem && !pFuncItem->IsARuleItem())
	{
		sErr = cwsprintf(_TB("Field {0-%s} has to valorized by rule"), m_strPublicName);
		return FALSE;
	}
	return TRUE;
}

//===========================================================================
//              Class ExpressionWithCheck implementation
//===========================================================================

IMPLEMENT_DYNAMIC(ExpressionWithCheck, Expression)

BOOL ExpressionWithCheck::CheckVariables(Parser& parser, const Stack* pStack)
{
	WoormField*  pQueryItem;
	CString     strName;

	for (int i = 0; i < pStack->GetSize(); i++)
	{
		ExpItem* pExpItem = (ExpItem*)(*pStack)[i];

		if (pExpItem->IsA() == EXP_ITEM_OPE_CLASS)
		{
			if (
				!CheckVariables(parser, &((ExpItemOpe*)pExpItem)->m_frstOpStack) ||
				!CheckVariables(parser, &((ExpItemOpe*)pExpItem)->m_scndOpStack)
				)
				return FALSE;

			continue;
		}

		if (pExpItem->IsA() != EXP_ITEM_VRB_CLASS)
			continue;

		strName = ((ExpItemVrb*)pExpItem)->m_strNameVrb;
		pQueryItem = (WoormField*)m_pSymTable->GetField(strName);
		if (pQueryItem == NULL)
			return parser.SetError(Expression::FormatMessage(Expression::UNKNOWN_FIELD), strName);

		//Impedisce l'utilizzo dell'opertore ternario per modificare il criterio di raggruppamento
		//SPOSTATO dopo
		//if (!pQueryItem->IsARuleItem())
		//{
		//	return parser.SetError(_TB("The field used for grouping mandatory must have database or expression field type."), CString(L"\r\n" + _TB("The field with wrong type is:") + ' ' + strName));
		//}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL ExpressionWithCheck::Parse(Parser& lex, const DataType& dt, BOOL bKeepString/* = FALSE*/)
{
	BOOL bOk = __super::Parse(lex, dt, bKeepString);
	if (!bOk)
		return FALSE;

	if (m_bSkipCheck)
		return TRUE;

	return CheckVariables(lex, GetStack());
}

//===========================================================================
//              Class GroupBy implementation
//===========================================================================
IMPLEMENT_DYNAMIC(GroupBy, ExpressionWithDebugBlocks);

//---------------------------------------------------------------------------
GroupBy::GroupBy (QueryEngine* pEngine)
    :
	ExpressionWithCheck(&pEngine->GetSymTable()),
    m_pOwnerEngine	(pEngine),
	m_pBefore		(new DebugBlock(dynamic_cast<RepEngine*>(pEngine))),
	m_pAfter		(new DebugBlock(dynamic_cast<RepEngine*>(pEngine)))
{
	m_pBefore->m_strOwnerName	= L"Tuple.Grouping.Before";
	m_pAfter->m_strOwnerName	= L"Tuple.Grouping.After";
}

GroupBy::~GroupBy()
{
	SAFE_DELETE(m_pBefore);
	SAFE_DELETE(m_pAfter);
}

//---------------------------------------------------------------------------
BOOL GroupBy::Eval(CString& strResult)
{
	DataStr newVal;
	
	m_pSymTable->SetDataLevel(RULE_ENGINE);
	
	if (!Expression::Eval(newVal))
		return m_pOwnerEngine->SetRTError(EngineScheduler::QUERY_EVAL_GROUP_EXPR, GetErrDescription());
	
	m_pSymTable->SetDataLevel(QUERY_ENGINE);
	
	strResult = newVal.GetString();

	return TRUE;
}

//---------------------------------------------------------------------------
BOOL GroupBy::Init (QueryEngine* pOwnerEngine)
{	
	// attach the owner
	ASSERT(m_pOwnerEngine == pOwnerEngine);
    m_pOwnerEngine = pOwnerEngine;

	BOOL bOk = Eval(m_strOldValue);

	TRACE("Init grouping:\n %s\n", LPCTSTR(m_strOldValue));
    return bOk;
}

//---------------------------------------------------------------------------
BOOL GroupBy::IsChanged ()
{
    CString strNewVal;

    if (Eval(strNewVal) && (m_strOldValue != strNewVal))
    {
        m_strOldValue = strNewVal;

		TRACE("New group:\n %s\n", LPCTSTR(m_strOldValue));
		return TRUE;
    }

	return FALSE;
}

//---------------------------------------------------------------------------
BOOL GroupBy::EvalFun(BOOL bOnlyExpr)
{
	m_pBefore->Exec();	//for RS-DEBUGGER only

    for (int i = 0; i < m_GroupFunctions.GetSize(); i++)
    {
        GroupFunction* aFun = (GroupFunction*) m_GroupFunctions[i];

		if (aFun->IsAnExpression()) 
		{
			if(!aFun->Eval())
				return m_pOwnerEngine->SetRTError(EngineScheduler::QUERY_EVAL_GROUP_FUNC, aFun->GetErrDescription());
			continue;
		}

		if (
				!bOnlyExpr || 
				aFun->GetFunctionType() == Token::T_CCOUNT 
			)
		{
			if (!aFun->Eval())
				return m_pOwnerEngine->SetRTError(EngineScheduler::QUERY_EVAL_GROUP_FUNC, aFun->GetErrDescription());
			continue;
		}

		aFun->IncOccurrence();
	}

	m_pAfter->Exec();	//for RS-DEBUGGER only
    return TRUE;
}

//---------------------------------------------------------------------------
void GroupBy::ResetOccurrence()
{
	for (int i = 0; i < m_GroupFunctions.GetSize(); i++)
	{
		GroupFunction* aFun = (GroupFunction*)m_GroupFunctions[i];
		if (aFun->IsAnExpression())
			continue;
		aFun->ResetOccurrence();
	}
}

//---------------------------------------------------------------------------
BOOL GroupBy::CheckRuleItems(CString& sErr)
{
	for (int i = 0; i < m_GroupFunctions.GetSize(); i++)
	{
		GroupFunction* aFun = (GroupFunction*)m_GroupFunctions[i];
		if (!aFun->CheckRuleItems(sErr))
			return FALSE;
	}
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL GroupBy::Parse (Parser& parser)
{
	m_bForce = parser.Matched(T_FORCE);

    SetStopTokens(T_DO);

	this->m_bSkipCheck = m_bForce;
    if (!__super::Parse(parser, DATA_STR_TYPE, FALSE /* do not keep the string */)) 
    	return FALSE;

    if (!parser.ParseTag(T_DO)) 
    	return FALSE;

    BOOL groupStatem = parser.Matched(T_BEGIN);
                      
    if (!parser.ErrorFound())
	    do
	    {
	        GroupFunction* pGF = new GroupFunction(m_pSymTable);
	        m_GroupFunctions.Add(pGF);
	        if (!pGF->Parse(parser)) 
	        	return FALSE;
	    }
	    while (parser.ParseSep() && groupStatem && !parser.Matched(T_END));

    return !parser.ErrorFound();
}

//-----------------------------------------------------------------------------
DECLARE_AND_INIT_THREAD_VARIABLE(RuleObj*, pCurrRuleObj, NULL);

//-----------------------------------------------------------------------------
BOOL GroupBy::CheckRuleGraph(CObArray& vectRules, const Stack& aStack)
{
	GET_THREAD_VARIABLE(RuleObj*, pCurrRuleObj);

	BOOL bNeedTemp = FALSE;

    for (int i = aStack.GetUpperBound(); i >= 0; i--)
    {
        ExpItem* pExpItem = (ExpItem*) aStack[i];
		ASSERT_VALID(pExpItem);

        if (pExpItem->IsA() == EXP_ITEM_OPE_CLASS)
		{
			if (CheckRuleGraph(vectRules, ((ExpItemOpe*)pExpItem)->m_frstOpStack))
				bNeedTemp = TRUE;

			if (CheckRuleGraph(vectRules, ((ExpItemOpe*)pExpItem)->m_scndOpStack))
				bNeedTemp = TRUE;

			continue;
		}

        if (pExpItem->IsA() != EXP_ITEM_VRB_CLASS)  continue;

        WoormField* pQueryItem = (WoormField*) m_pSymTable->GetField(((ExpItemVrb*) pExpItem)->m_strNameVrb);
		ASSERT_VALID(pQueryItem);
		if (!pQueryItem)
			continue;

        if (pCurrRuleObj != pQueryItem->GetOwnerRule())
        {
			pCurrRuleObj = pQueryItem->GetOwnerRule();
            
            // la regola e` gia` stata trovata?
            for (int j = 0; j < vectRules.GetSize(); j++)
            	if (pCurrRuleObj == vectRules[j])
	            {
					bNeedTemp = TRUE;
					break;
				}
			
           	vectRules.Add(pCurrRuleObj);
        }
    }

	return bNeedTemp;
}

// this routine must be called after rule graph is make
//-----------------------------------------------------------------------------
BOOL GroupBy::IsTemporaryRequired(CObArray& vectRules)
{
    if (m_ExprStack.GetSize() == 0) return FALSE;

    // builds an array of rules referenced by OrderBy items
	GET_THREAD_VARIABLE(RuleObj*, pCurrRuleObj);

	pCurrRuleObj = NULL;
    vectRules.RemoveAll();
    BOOL bNeedTemp = CheckRuleGraph(vectRules, m_ExprStack);

    if (vectRules.GetSize() == 0)
        return FALSE;			   // this is an OrderBy of only constant!!

    if (!bNeedTemp)
    {
    	// Se esiste una sola Rule referenziata dalla espressione e bNeedTemp e` FALSO
    	// allora vectRules[0] e` senz'altro una DataTableRule (vedi sopra)
    	// In tal caso l'espressione corrente puo` essere usata come Sort String della
    	// Query associata
    	//
        return FALSE;
    }                      
    return TRUE;
}

///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(ExpressionWithDebugBlocks, Expression);

ExpressionWithDebugBlocks::ExpressionWithDebugBlocks(QueryEngine* pEngine)
	: 
	Expression(&pEngine->GetSymTable()),
	m_pBefore(new DebugBlock(dynamic_cast<RepEngine*>(pEngine))),
	m_pAfter(new DebugBlock(dynamic_cast<RepEngine*>(pEngine)))
{
	m_pBefore->m_strOwnerName = L"Tuple.Before";
	m_pAfter->m_strOwnerName = L"Tuple.After";
}

ExpressionWithDebugBlocks::~ExpressionWithDebugBlocks()
{
	SAFE_DELETE(m_pBefore);
	SAFE_DELETE(m_pAfter);
}

//-----------------------------------------------------------------------------
BOOL ExpressionWithDebugBlocks::Eval(DataObj& ret)
{
	if (m_pBefore) m_pBefore->Exec();	//for RS-DEBUGGER only

	if (IsEmpty()) 
		return TRUE;

	BOOL bOk = __super::Eval(ret);
	if (!bOk)
		return FALSE;

	if (m_pAfter) m_pAfter->Exec();	//for RS-DEBUGGER only
	return TRUE;
}


