#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class ReportData;
class EditorManager;
class ProgramData;
class WoormField;
class WoormTable;
class Parser;
class Unparser;
class RuleDataArray;
class DataType;
class CBaseDocument;
class ReportEngine;
class CXSDGenerator;
class WoormTable;
class DisplayTables;
class TblRuleData;
class CInternalFunctionObjectsParser;
class CHelperSqlCatalog;
class EnumTagArray;
class CHelperExternalReferences;

//===========================================================================
enum RepFieldType;
class DataBool;
//===========================================================================
class TB_EXPORT EditorManager: public CObject
{
private:
	enum	LocalFieldType	{ DATA_FLD, EXP_FLD, VAR_FLD, HID_FLD };

private:
	CBaseDocument*	m_pDocument;
	ProgramData*	m_pPrgData;
	BOOL			m_bModifiedFlag;
	BOOL			m_bCurrRunValid;
	BOOL			m_bProgramLoaded;
	ReportEngine*   m_pReportEngine;

public:
	DataBool&			m_bShowAdvancedForms;

public:
	EditorManager	(ReportEngine*, DataBool& bShowAdvancedForms);
	~EditorManager	();

public:
	// document attaching function
	void	Attach	(CBaseDocument* pDoc);

public:
	// Function Member For Visible Single Field
	BOOL	AddDataField(WORD& nID, CWordArray& ids, TblRuleData* pTblRule = NULL);
	BOOL	AddExpField			(WORD& nID);
	BOOL	AddVarField			(WORD& nID);
	BOOL	AddHiddenField		(WORD& nID);
	BOOL	AddRepeater			(WORD& nID);
	BOOL	AddChart			(WORD& nID);

	BOOL	DeleteField			(WORD nID/*, BOOL bTrasformInHidden*/);
	BOOL	ModifyField			(WORD nID, BOOL bAlsoModifyType = TRUE);

	// Function Member For Visible Table Column Field
	BOOL	AddTableDataField(WORD& nID, CWordArray& ids, BOOL bCreateTable, TblRuleData* pTblRule = NULL);
	BOOL	AddTableExpField	(WORD& nID, CWordArray& ids, BOOL bCreateTable);
	BOOL	AddTableHiddenField	(WORD& nID, CWordArray& ids, BOOL bCreateTable);
	BOOL	AddTableVarField	(WORD& nID, CWordArray& ids, BOOL bCreateTable);

	BOOL	DeleteTable			(WORD nID);

	// Function Member For Total And Subtotal Field
	void	ModColTotal			(WORD idColumn);
	void	GetSubTotal			(WORD idColumn, CWordArray&);
	void	GetColTotal			(WORD idColumn, CWordArray&);
	BOOL	AddAutoColTotal		(WORD idColumn, BOOL bTotalOnNewPage, BOOL bResetOnNewPage);
	BOOL	DelColTotal			(WORD idColumn/*, BOOL bTrasformInHidden*/);
	BOOL	ExistColTotalOf		(WORD idColumn);

    // Function Member To Retrive Field Information
	DataType	GetDataType		(WORD nID);
	CString		GetPublicName	(WORD nID);
	int			GetLen			(WORD nID);
	int			GetDec			(WORD nID);
	CString		GetDescrMenu	(WORD nID);
	BOOL		SetFieldWidth	(WORD nID, int nChars, BOOL bMultiline);

    // Function Member To Retrive Table Information
	CString	GetTablePublicName	(WORD nID);
	void	SetTablePublicName	(WORD nID, LPCTSTR);
	int		GetTableRows		(WORD nID);
	BOOL	SetTableRows		(WORD nID, int);
	BOOL	ExistsTable			(WORD nID);

	// Function Member To Show Any Informations
	//void	ShowVariables		();
	//void	ShowAskRules	();
	//void	ShowEvents		();
	//void	ShowProcedures	();
	//void	ShowQueries		();
	//void	ShowOrderBy		();
	//void	ShowSubTotals	(WORD nTableID = -1);
	//void	ShowRelation	(WORD nID);
	//void	ShowRelations	(CWordArray& ids);

	// Function Member To Generate Id - Identifier
	WORD	GetId			();
	WORD	GetNextId		();
	void	SetLastId		(WORD nId);

	// Function Member To Load & Save & check if is an empty report
	BOOL	Parse			(Parser&);
	BOOL	Parse			(const CString& strRepFileName, CStringArray& arErrors, int& nLine, int& nCol);
	BOOL	Unparse			(Unparser&);
	BOOL	GetSchema		(CXSDGenerator* pSchema);
	BOOL	ExistProgram	();

	BOOL	IsCurrRunValid		();
	void	SetValidRun			(BOOL bValidRun = TRUE);
	BOOL	GetModifiedFlag		();
	void	SetModifiedFlag		(BOOL bModified = TRUE, BOOL setDocToo = TRUE);

	WoormTable*	GetSymTable	() const; 
	RuleDataArray*		GetRuleData	() const;
	ProgramData*	GetPrgData	() const;
	DisplayTables*	GetDispTable();

	CInternalFunctionObjectsParser*		GetInternalFunctions();
	CHelperSqlCatalog*					GetHelperSqlCatalog();
	EnumTagArray*						GetEnumsArray();
	CHelperExternalReferences*			GetHelperExternalReferences();

private:
	void	SetHidden		(WoormTable*, CWordArray&, BOOL);
	BOOL	AddGenericField(WORD&, CWordArray&, LocalFieldType, BOOL bIsSingleField, BOOL bCreateTable = FALSE, TblRuleData* pTblRule = NULL);
	BOOL	CanDeleteFields	(CWordArray& ids, CString& sLog);
	void	DeleteFields	(CWordArray& ids);
	void 	ChangeInHidden	(CWordArray& ids);

	CInternalFunctionObjectsParser* m_pInternalFunctions;
	CHelperSqlCatalog*				m_pHelperSqlCatalog;
	EnumTagArray*					m_pEnumArray;
	CHelperExternalReferences*		m_pHelperExternalReferences;
};

#include "endh.dex"
