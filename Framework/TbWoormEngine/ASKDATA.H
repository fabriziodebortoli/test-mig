#pragma once

#include <TbGeneric\Array.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class WoormTable;
class Parser;
class Unparser;
class CAskRuleDlg;
class CAskFieldDlg;
class CFunctionDescription;
class CParsedForm;
class InputMng;
class Expression;
class Block;
class ReportEngine;
class CXSDGenerator;

//===========================================================================
class TB_EXPORT AskFieldData : public CObject
{
	friend class CAskRuleDlg;
	friend class CAskFieldDlg;
	friend class AskDialogData;
	friend class AskGroupData;
	friend class CRSTreeCtrl;
	friend class CRSEditView;
	friend class CRS_ObjectPropertyView;
	friend class CRSAskFieldProp;
	friend class AskEntry;
	friend class AskGroup;
	friend class InputMng;
	friend class AskDialogInputMng;
	friend class CRSEditViewDebug;
	friend class CRSEditorSymbolTableViewDebug;

	DECLARE_DYNAMIC(AskFieldData);

public:
	enum CtrlStyle { CHECK_BOX, RADIO_BUTTON, EDIT, LEFT_ALIGN_DELTA = 4, COMBO_BOX = 8, EDIT_HKL = 16, LEFT_TEXT_DELTA = 32};

protected:
	AskGroupData*	m_pGroup;

	CString			m_strPublicName;

	CString			m_strCaption;
	Expression*		m_pCaptionExpr;
	Token			m_nCaptionPos;

	Token			m_nInputLimit;
	CtrlStyle		m_CtrlStyle;
	BOOL			m_bLeftBoolAlign;
	BOOL			m_bLeftTextBool;
	BOOL			m_bMultiSelectionCombo;
	BOOL			m_bDescriptionCombo;

    BOOL			m_bHiddenInput;

	Expression*		m_pReadOnlyExpr;
	Expression*		m_pWhenExpr;
	BOOL			m_bDynamicHidden;

	CTBNamespace	m_nsHotLink;
	Array			m_HotLinkParamsExpr;
	BOOL			m_bShowHotLinkDescription;

public:
	AskFieldData(AskGroupData*	pGroup);
	virtual ~AskFieldData();
	
public:
	CString GetPublicName(){return m_strPublicName;}
	CString GetCaption(){ return m_strCaption; }

	BOOL	CanDelete();

	BOOL	Parse	(Parser&, WoormTable&);
	void	Unparse	(Unparser&, AskGroupData&);

	void	GetSchema		(CXSDGenerator* pSchema, WoormTable&);
	void	GetXmlParameters(CXMLNode*, WoormTable&);
	void	AssignFromXml	(CXMLNode*, WoormTable&);

protected:
	void	UnparseHotlink	(Unparser&);
	BOOL	ParseHotlink	(Parser& lex, WoormTable& aSymTable, CTBNamespace& aNs);	

private:
	BOOL	ParseHotlinkParam	(Parser&, CFunctionDescription*, WoormTable&, BOOL bFromFile);
};

//===========================================================================
class TB_EXPORT AskGroupData : public CObject
{
	friend class CAskRuleDlg;
	friend class CAskGroupDlg;
	friend class AskFieldData;
	friend class AskDialogData;
	friend class CRSTreeCtrl;
	friend class CRSEditView;
	friend class CRSGroupFieldProp;
	friend class CRSBoolProp;
	friend class CRS_ObjectPropertyView;
	friend class CRSReportTreeView;
	friend class AskDialogInputMng;
	friend class AskGroup;
	friend class InputMng;

	DECLARE_DYNAMIC(AskGroupData);

protected:
	CBaseDocument*		m_pDocument;
	WoormTable&			m_SymTable;

	Array				m_AskFields;

	Token				m_nFieldsCaptionPos;

	CString				m_strTitle;
	BOOL				m_bHiddenTitle;
	Expression*			m_pWhenExpr;

public:
	AskGroupData(WoormTable&, CBaseDocument* pDocument);
	virtual ~AskGroupData();

public:
	BOOL			Parse		(Parser&);
	void			Unparse		(Unparser&, AskDialogData&);

	void			GetSchema		(CXSDGenerator* pSchema, int);
	void			GetXmlParameters(CXMLNode*, int);
	void			AssignFromXml	(CXMLNode*, int);

	const CString&	GetTitle		()	const { return m_strTitle; }
	void			SetTitle		(const CString& sTitle)	 { m_strTitle = sTitle; }

	AskFieldData*	GetAskField		(int);
	int				GetAskFieldSize	(){return m_AskFields.GetSize();}
	void			DelAskField		(int idx, BOOL bDeleteField);

	void			DelAllAskField	(BOOL bDeleteFields);

	AskFieldData*	GetAskField		(LPCTSTR);

	BOOL			CanDelete		();
	BOOL			CanDeleteField	(LPCTSTR);
	BOOL			DeleteField		(LPCTSTR);
	void			RenameField		(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName);

	BOOL			IsEmpty			();

	int				AddAskField(AskFieldData*);
public:
	static CString	GetEmptyTitle	();
};

//===========================================================================
class TB_EXPORT AskDialogData : public CObject
{
	friend class AskGroupData;
	friend class CAskRuleDlg;
	friend class CAskDialogDlg;
	friend class CRSTreeCtrl;
	friend class CRSEditView;
	friend class CRS_ObjectPropertyView;
	friend class CRSReportTreeView;
	friend class CRSReportTreeView;
	friend class AskDialogInputMng;
	friend class InputMng;
	//friend class CDynamicDlg;
	friend class CDynamicContainerDlg;
	friend class AskRuleData;
	friend class CAskDialogDataAndGroups;

	DECLARE_DYNAMIC(AskDialogData);

protected:
	WoormTable&			m_SymTable;
	CBaseDocument*		m_pDocument;

	CString				m_strName;
	CString				m_strTitle;

	Array				m_AskGroups;

	Token				m_nFieldsCaptionPos;

	Expression*			m_pWhenExpr;	//Bool
	Expression*			m_pOnExpr;		//Bool
	Expression*			m_pAbortExpr;	//String

	Block*				m_pBeforeBlock;
	Block*				m_pAfterBlock;

	BOOL				m_bIsOnAsk;

	CStringArray		m_arCommentTraceBefore;
	CStringArray		m_arCommentTraceAfter;

public:
	
	AskDialogData(WoormTable&, CBaseDocument* pDocument);
	virtual ~AskDialogData();

public:
	WoormTable*		GetSymTable	() { return &m_SymTable; }

	BOOL			Parse		(Parser&, ReportEngine*, AskRuleData*);
	void			Unparse		(Unparser&);

	void			GetSchema		(CXSDGenerator* pSchema);
	void			GetXmlParameters(CXMLNode*);
	void			AssignFromXml	(CXMLNode*);

	const CString&	GetTitle	()	const { return m_strTitle;	}
	const CString&	GetName		()	const { return m_strName;	}
	BOOL			IsOnAsk		()	const { return m_bIsOnAsk;	}

	void			SetOnAsk(BOOL isOnAsk){ m_bIsOnAsk = isOnAsk; }
	void			SetCaptionPos(Token tokenType){ m_nFieldsCaptionPos = tokenType; }
	void			SetName	(const CString& strName) { m_strName = strName;	}
	void			SetTitle(const CString& strTitle) { m_strTitle = strTitle; }

	AskGroupData*	GetAskGroup		(int);
	Array*			GetAskControls	() { return &m_AskGroups; }
	int				GetAskGroupSize	(){return m_AskGroups.GetSize();}
	void			DelAskGroup		(int, BOOL bDeleteFields);
	void			DelAllAskGroup	(BOOL bDeleteFields);
	int				AddAskGroup		(AskGroupData*);

	AskFieldData*	GetAskField		(LPCTSTR);

	BOOL			CanDelete		();
	BOOL			CanDeleteField	(LPCTSTR);
	BOOL			DeleteField		(LPCTSTR);
	void			RenameField		(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName);

	BOOL			IsEmpty			();
	BOOL			InitAskFields	();

public:
	static CString	GetEmptyTitle();
};

//===========================================================================
class TB_EXPORT AskRuleData : public CObject
{
	DECLARE_DYNAMIC(AskRuleData);

	friend class CAskRuleDlg;
	friend class CAskDialogDlg;
	friend class CRSReportTreeView;
	friend class AskDialogInputMng;

protected:
	CBaseDocument*		m_pDocument;
	WoormTable&			m_SymTable;

	Array				m_AskDialogs;

	CStringArray		m_arCommentTraceBefore;
	CStringArray		m_arCommentTraceInner;
	CStringArray		m_arCommentTraceAfter;

public:
	AskRuleData(WoormTable&);

	void			SetOwnerDocument (CBaseDocument* pDoc)	{m_pDocument = pDoc;}

public:
	BOOL			Parse			(Parser&, ReportEngine*);
	void			Unparse			(Unparser&);

	// può ritornarmi il tipo come complexElement(asComplexType = FALSE) oppure come complexType (asComplexType = TRUE)
	BOOL			GetSchema		(CXSDGenerator* pSchema, BOOL asComplexType = FALSE);
	void			GetXmlParameters(CXMLNode*);
	void			AssignFromXml	(CXMLNode*);

	int				GetCount() const { return m_AskDialogs.GetCount(); }

	int				Add				(AskDialogData*);

	AskDialogData*	GetAskDialog	(int);
	AskDialogData*	GetAskDialog	(AskFieldData* pAskField);
	AskDialogData*	GetAskDialog	(AskGroupData* pAskGroup);
	AskDialogData*	GetAskDialog	(const CString& strName);
    void			DelAskDialog	(int idx, BOOL bDeleteFields);
	BOOL 			ExistDialog		(const CString& strTitle, AskDialogData* = NULL);

	AskFieldData*	GetAskField		(LPCTSTR);
	BOOL			CanDeleteField	(LPCTSTR) const;
	BOOL			DeleteField		(LPCTSTR);
	void			RenameField		(LPCTSTR pszOldFieldName, LPCTSTR pszNewFieldName);
	CString			GetAdviseName (AskDialogData* pCurrAskDialog);
};

#include "endh.dex"
