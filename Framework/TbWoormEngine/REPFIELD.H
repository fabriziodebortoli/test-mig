
#pragma once

#include <TbGeneric\DataObj.h>
#include <TbGeneric\RdeProtocol.h>
#include <TbGeneric\Pictures.h>
#include <TbParser\Parser.h>
#include <TbParser\SymTable.h>

#include "MultiLayout.h"
#include "disptbl.h"
#include "datafunc.h"

//includere alla fine degli include del .H
#include "beginh.dex"
//============================================================================

#define RULE_ENGINE     0					// data level for RuleItem
#define QUERY_ENGINE 	(RULE_ENGINE+1)		// data level for QueryItem
#define REPORT_ENGINE	(QUERY_ENGINE+1)

//============================================================================
class EventActions;
class GroupFunction;
class WoormField;
class WoormTable;
class RepEngine;
class CWoormDoc;
class RuleObj;
class SqlTableInfoArray;
class RecordArray;
class WoormTable;
class FieldWidthFactors;
class WoormTable;

//============================================================================
class TB_EXPORT WoormField : public SymField
{
	friend WoormTable;
	friend class CRS_ObjectPropertyView;
	friend class CRSReportTreeView;
	DECLARE_DYNAMIC(WoormField)
public:
	enum RepFieldType
		{
			FIELD_NORMAL, FIELD_INPUT, 
			FIELD_COLUMN, FIELD_SUBTOTAL, FIELD_COLTOTAL
		};
	enum SourceFieldType
	{
		NONE, DB_FIELD, FUNCTION_FIELD, EXPRESSION_FIELD, INPUT_FIELD, ASK_FIELD, HIDDEN_FIELD, 
		TOTAL_FIELD //aggiunto per icona toolbar property list 
	};

protected:
	RepFieldType	m_FieldType = RepFieldType::FIELD_NORMAL;

	BOOL		m_bHidden = TRUE;;
	BOOL		m_bAsk = FALSE;
		BOOL		m_bReInit = FALSE;
		BOOL		m_bStatic = FALSE;
		BOOL		m_bLowerLimit = FALSE;
		BOOL		m_bUpperLimit = FALSE;

	BOOL		m_bMultiline = FALSE;
	int			m_nLen = 0;                 // they need for eventual store on
	int         m_nNumDec = 0;              // temporary file

	Expression*		m_pInitExpression = NULL;

	BOOL			m_bNativeColumnExpr = FALSE;
	CString			m_strPhysicalName;

	CString			m_sXmlAliasName;
	BOOL			m_bNoXml = FALSE;

	CString			m_sThreadContextVar;
	BOOL			m_bOwnThreadContextVar = FALSE;

	BOOL			m_bIsExprField = FALSE;
	BOOL			m_bIsTableField = FALSE;
	BOOL			m_bIsSpecialField = FALSE;
	CString			m_strDisplayTable;

	//RuleItem
	BOOL			m_bRuleDataUpdated = FALSE;

	BOOL			m_bReadOnly = FALSE;

	RuleObj*		m_pOwnerRule = NULL;          // only used to resolve order by expression
	//QueryItem
	DataObj*		m_pQryData = NULL;
	BOOL			m_bQryDataUpdated = FALSE;
	GroupFunction*	m_pGroupFunction = NULL;		// property of GroupBy

	// WoormField
	BOOL			m_bDisplayed = FALSE;
	RDEcmd			m_AccumKind = 0;
	BOOL			m_bRepDataUpdated = FALSE;
	DataObj*		m_pRepData = NULL;
	WoormField*		m_pOwnRepField = NULL;		// WoormField Owner (only for SubTotal/ColTotal fields)
	EventFunction*	m_pEventFunction = NULL;
	CStringArray	m_Substrings;
	DisplayTableEntry*	m_pDisplayTable = NULL;	// Display Table Owner

public:													
	WoormField
		(
			const CString& strName, 
			RepFieldType ft = FIELD_NORMAL,
			const DataType& aDataType = DataType::Null,
			WORD nAlias = SpecialReportField::NO_INTERNAL_ID, 
			DataObj* pValue = NULL, 
			BOOL bCloneValue = TRUE
		);
	WoormField
	(
		WoormField*, 
		WORD nAlias = SpecialReportField::NO_INTERNAL_ID
	);

	~WoormField();

public:
	WoormTable* GetWoormTable()	const;

	WoormField::RepFieldType	GetFieldType () const { return m_FieldType; }
	CString						GetSourceDescription();
	WoormField::SourceFieldType	GetSourceEnum();
	void						SetFieldType (WoormField::RepFieldType ft);

	virtual void	SetDataPtr	(DataObj* pData, BOOL bOwnData = TRUE);

	BOOL			IsNormal			() const { return m_FieldType == FIELD_NORMAL; }
	virtual BOOL	IsInput				() const { return m_FieldType == FIELD_INPUT; }
	virtual BOOL	IsColumn			() const { return m_FieldType == FIELD_COLUMN; }
	virtual BOOL	IsColTotal			() const { return m_FieldType == FIELD_COLTOTAL; }
	virtual BOOL	IsSubTotal			() const { return m_FieldType == FIELD_SUBTOTAL; }

	virtual int		GetLen			() const { return m_nLen; }
	int				GetNumDec		() const { return m_nNumDec; }
	void			SetPrecision	(int l, int d)		{ m_nLen = l; m_nNumDec = d; }
	void			SetLen			(int nLen)			{ m_nLen = nLen;			}
	void			SetNumDec		(int nPrecision)	{ m_nNumDec	= nPrecision;	}

	virtual BOOL	IsHidden			() const { return m_bHidden; }
	virtual BOOL	IsReInit			() const { return m_bReInit; }
	virtual BOOL	IsStatic			() const { return m_bStatic; }
	virtual BOOL	IsAsk				() const { return m_bAsk; }
	BOOL			IsMultiline			() const { return m_bMultiline; }

	void			SetHidden			(BOOL b)		{ m_bHidden=b; }
	void			SetReInit			(BOOL b)		{ m_bReInit = b; if (b) m_bStatic = FALSE; }
	void			SetStatic			(BOOL b)		{ m_bStatic = b; if (b) m_bReInit = FALSE; }
	void			SetAsk				(BOOL b)		{ m_bAsk = b; 
															if (!b) { m_bStatic = FALSE; m_bReInit = FALSE; 
																	  m_bLowerLimit = FALSE; m_bUpperLimit = FALSE;	} 
														}
	void			SetMultiline		(BOOL b)		{ m_bMultiline = b; }
	
	//----
	virtual BOOL	IsLowerLimit		() const { return m_bLowerLimit; }
	virtual BOOL	IsUpperLimit		() const { return m_bUpperLimit; }
	void			SetLowerLimit		(BOOL b) { m_bLowerLimit = b; if (b) m_bUpperLimit = FALSE; }
	void			SetUpperLimit		(BOOL b) { m_bUpperLimit = b; if (b) m_bLowerLimit = FALSE; }
	
	//----
	Expression*&	GetInitExpression	()							{ return m_pInitExpression; }
	void			SetInitExpression	(Expression* pExpr)			{ SAFE_DELETE(m_pInitExpression); m_pInitExpression = pExpr; }
	BOOL			SetInitExpression	(LPCTSTR szExpr);

	BOOL			IsPredefinedField	() const					{ return GetId() >= SpecialReportField::REPORT_LOWER_SPECIAL_ID; }
	
	LPCTSTR			GetPhysicalName		() const					{ return m_strPhysicalName; }
	void			SetPhysicalName		 (LPCTSTR pszName)			{ m_strPhysicalName = pszName; }                                               
	
	BOOL			IsNativeColumnExpr	() const					{ return m_bNativeColumnExpr; }
	void			SetNativeColumnExpr (BOOL bNativeColumnExpr)	{ m_bNativeColumnExpr = bNativeColumnExpr; }

	BOOL			IsSpecialField		() const					{ return m_bIsSpecialField; }
	void			SetSpecialField		(BOOL bIsSpecialField)		{ m_bIsSpecialField = bIsSpecialField;	}

	BOOL			IsTableRuleField	() const					{ return m_bIsTableField;				}
	void			SetTableRuleField	(BOOL bIsTableField)		{ m_bIsTableField = bIsTableField;		}
	BOOL			IsExprRuleField		() const					{ return m_bIsExprField;				}
	void			SetExprRuleField	(BOOL bIsExprField)			{ m_bIsExprField = bIsExprField;		}

	virtual BOOL	IsRuleField			() const					{ return m_pOwnerRule || IsTableRuleField() || IsExprRuleField(); }

	LPCTSTR			GetDispTable	() const						{ return m_strDisplayTable;			}
	void			SetDispTable	(LPCTSTR pszDispTableName)
																	{
																		m_strDisplayTable = pszDispTableName;
																		SetFieldType(FIELD_COLUMN);
																	}
	void			RemoveDispTable	()
																	{
																		ASSERT(IsColumn());
																		m_strDisplayTable.Empty();
																		SetFieldType(FIELD_NORMAL);
																	}

	CString			GetXmlName		() const						{ if (!m_sXmlAliasName.IsEmpty()) return m_sXmlAliasName; return GetName(); }
	void			SetXmlName		(const CString& s) 				{ m_sXmlAliasName = s; }
	BOOL			GetSkipXml		() const						{ return m_bNoXml; }
	void			SetSkipXml		(BOOL b) 						{ m_bNoXml = b; }
	
	CString			GetThreadContextName	() const				{ return m_sThreadContextVar; }
	void			SetThreadContextName	(const CString& s) 		{ m_sThreadContextVar = s; }
	virtual BOOL	OwnThreadContextVar		() const				{	return !m_sThreadContextVar.IsEmpty() && m_bOwnThreadContextVar;	}
	void			SetOwnThreadContextVar	(BOOL b) 				{ m_bOwnThreadContextVar = b; }

//------------------------------------------------
//RuleItem
public:
	WoormField*		GetOwnRepField() { return m_pOwnRepField; }		// WoormField Owner (only for SubTotal/ColTotal fields)


	DataObj*	GetRuleData			() const { return __super::GetData(); }
	void		SetRuleData			(DataObj* pDataObj)  { SetDataPtr(pDataObj); }

	BOOL		IsARuleItem			() const { return m_pOwnerRule != NULL || IsTableRuleField() || IsExprRuleField(); }

	BOOL		HasValidRuleData	() const { return m_bRuleDataUpdated && !IsRuleDataNull(); }

	void		RuleNullified		() { __super::GetData()->Clear(FALSE); m_bRuleDataUpdated = TRUE; }
	void		RuleUpdated			() { __super::GetData()->SetValid(); m_bRuleDataUpdated = TRUE; }

	void		RuleDataProcessed	() { m_bRuleDataUpdated = FALSE; }
	
	BOOL		IsReadOnly			() const	{ return m_bReadOnly; }
	void		SetReadOnly			();	
                                                            
	void		SetOwnerRule (RuleObj* pOwner)	{ m_pOwnerRule = pOwner; }
	RuleObj*	GetOwnerRule () const			{ return m_pOwnerRule; }
	
protected:
	BOOL		IsRuleDataNull		() const { return !__super::GetData()->IsValid(); }
	BOOL		IsRuleUpdated		() const { return m_bRuleDataUpdated; }

//QueryItem
public:
	DataObj*		GetQryData			() const { return m_pQryData; }

	void			QueryUpdated		() { m_bQryDataUpdated = TRUE;	}
	void			QryDataProcessed	() { m_bQryDataUpdated	= FALSE; }

	void			UpdateQryData		();

	void			SetGroupFunction	(GroupFunction* pFun)	{ m_pGroupFunction = pFun;	}
	GroupFunction*	GetGroupFunction	() const				{ return m_pGroupFunction;	}

protected:
	BOOL	IsQryDataNull		() const { return !m_pQryData->IsValid(); }
	BOOL	IsQueryUpdated		() const { return m_bQryDataUpdated; }

public:
	WoormTable*		GetSymTable() const { return (WoormTable*) __super::GetSymTable(); }

	virtual void	SetDataType		(const DataType& dataType, BOOL bArray = FALSE);

	virtual DataObj* GetData			(int nDataLevel = -1) const;
	virtual DataObj* GetRepData			() 	const { return m_pRepData; }
	virtual void	 SetData			(DataObj&);

	BOOL			IsQueryChanged		()	const
		{ return	IsQueryUpdated() && !GetQryData()->IsEqual(*m_pRepData); }

	BOOL			HasValidRepData		() const { return m_bRepDataUpdated && !IsRepDataNull(); }
	void			ReportUpdated		()		{ m_bRepDataUpdated = TRUE; }
	void			RepDataProcessed	()		{ m_bRepDataUpdated = FALSE; }
	
	Expression::MessageID	Init		();
	Expression::MessageID 	Init		(DataObj* pRepData);

	void			InitAllDataLevel	();
	void			ClearAllDataLevel	();
	void			UpdateRepData		();

	BOOL			Display				(RepEngine&);
	void			SetDisplayed		()		{ m_bDisplayed = TRUE; }
	BOOL			Write				(RepEngine&, BOOL bWriteSpecial = FALSE);
	BOOL			WriteArray			(RepEngine&);

	DisplayTableEntry*			GetDisplayTable			() const { return m_pDisplayTable; }
	DisplayTableEntryEngine*	GetDisplayTableEngine	() const;// { return dynamic_cast<DisplayTableEntryEngine*>(m_pDisplayTable); }
	BOOL						IsTailMultiLineString	() const;

	void			ReattachDisplayTable		(const CString& sLayout);
	
	BOOL			IsDisplayed			()	const { return m_bDisplayed; }

protected:
	BOOL			IsRepDataNull		()	const { return !m_pRepData->IsValid(); }
	BOOL			SplitString			(const CString&, CWoormDoc*);

//MERGED RepFieldData ----------------------------------------------------------------------------------
public:
	BOOL			SplitStringOptimized	(CWoormDoc*);
	BOOL			SplitStringWithMiniHtml	(CWoormDoc*, FieldWidthFactors& fieldWidthFactors, CString strValue);
	BOOL			SplitImageToMultiline	(CWoormDoc* pWoormDocument, int h, int w, CTBPicture::ImageFitMode efit);

	EventFunction*&	GetEventFunction	() { return m_pEventFunction; }
	BOOL			HasAFunction		() const { return (m_pEventFunction != NULL);	}
	void			SetEventFunction	(SymTable*, LPCTSTR pszColumnName, Token = T_NOTOKEN);
	void			SetEventFunction	(SymTable*, LPCTSTR pszColumnName, LPCTSTR pszExpr, const DataType&);
	void			SetEmptyEventFunction(SymTable*, LPCTSTR pszFieldName);

	BOOL			CanDeleteField		(LPCTSTR);

	BOOL			Parse				(Parser&, WoormTable*);
	void			Unparse				(Unparser&, BOOL bNoDispTableName);
	void			UnparseDataType		(Unparser& oFile, BOOL bIndent = TRUE);

	BOOL			AddMethodList		(Parser& parser);
	BOOL			AddMethodList		(const CRuntimeClass* rcStopBaseClassName);

	CString			BindRuleItem		(const RecordArray& aBindRecord, const SqlTableInfoArray& arSqlTableJoinInfoArray);

	CString			GetRepFieldTypeDescription() const;	
protected:
	BOOL			ParseDisplayAttribute	(Parser&, WoormTable*);
	void 			UnparseDisplayAttribute	(Unparser&, BOOL bNoDispTableName);
};

//===========================================================================
#include "endh.dex"
