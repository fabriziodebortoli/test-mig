//*******************************************************************************
// COPYRIGHT NOTES
// ---------------
// This is a sample for BCGControlBar Library Professional Edition
// Copyright (C) 1998-2015 BCGSoft Ltd.
// All rights reserved.
//
// This source code can be used, distributed or modified
// only under terms and conditions 
// of the accompanying license agreement.
//*******************************************************************************
//

#pragma once
#include <map>
#include "XmlOutlineParser.h"

#include "beginh.dex"
/////////////////////////////////////////////////////////////////////////////
// CCustomEditCtrl window


static const DWORD g_dwBreakPointType = g_dwBCGPEdit_FirstUserDefinedMarker;
static const DWORD g_dwColorBreakPointType = g_dwBCGPEdit_FirstUserDefinedMarker << 1;
static const DWORD g_dwBookmarkPointType = g_dwBCGPEdit_FirstUserDefinedMarker;
static const DWORD g_dwColorHeaderType = g_dwColorBreakPointType << 1;
static const DWORD g_dwCurrLineType = g_dwColorHeaderType << 1;

class TB_EXPORT IntellisenseWndExtended : public CBCGPIntelliSenseWnd
{
	DECLARE_MESSAGE_MAP()
public:
	IntellisenseWndExtended() :CBCGPIntelliSenseWnd() {};
	~IntellisenseWndExtended();
	CBCGPBaseIntelliSenseLB* GetIntelliList() { return m_pLstBoxData;}
	virtual BOOL DestroyWindow();


};

class TB_EXPORT IntellisenseData : public CBCGPIntelliSenseData
{
public:
	CString	  m_strItemValue;
	CString   m_strAdditionalInfo;

	IntellisenseData() : CBCGPIntelliSenseData() {}
	IntellisenseData(const IntellisenseData& a) { *this = a; }

	IntellisenseData & operator= (const IntellisenseData & other)
	{
		
		m_dwData=other.m_dwData;
		m_nImageListIndex  = other.m_nImageListIndex;
		m_strAdditionalInfo = other.m_strAdditionalInfo;
		m_strItemHelp = other.m_strItemHelp;
		m_strItemName = other.m_strItemName;
		m_strItemValue = other.m_strItemValue;
		return *this;
	}

};


// IntellisenseMap is based on TRIE data structure 
class TB_EXPORT IntellisenseMap {

private:
	static const int alphabetSizeExtended = 93;
	

public:
	class TB_EXPORT IntellisenseNode {
	public:
		// alphabet size + special characters  + numbers
		IntellisenseNode * children[alphabetSizeExtended];
		bool isEndOfWord = false;
		IntellisenseData* data = NULL;
	};

	
	IntellisenseMap();
	IntellisenseNode* createNode();
	IntellisenseNode* insert(CString key);
	IntellisenseNode* search(CString key, IntellisenseNode* fromHere = NULL);
	void matchPrefix(CObList& lstIntelliSenseData, CString prefix);
	void empty();

protected:
	IntellisenseNode * root=NULL;
	void matchAllRec(CObList& lstIntelliSenseData, IntellisenseNode * node, BOOL hasPoint);
	IntellisenseData* createDataCopy(IntellisenseData* data);
};


class TB_EXPORT CCustomEditCtrl : public CBCGPEditCtrl
{
	Array m_arGarbage;	//all'uscita deletera tutti gli oggetti contenuti

// Construction
public:
	CCustomEditCtrl();
	virtual ~CCustomEditCtrl();

// Attributes
public:
	BOOL	m_bCheckColorTags;	// TRUE if check for tags (<....>) in "OnGetCharColor"

// Operations
public:
	void UseXmlParser (BOOL bXmlParser = TRUE)
	{
		((CXmlOutlineParser*)m_pOutlineParser)->EnableXmlParser (bXmlParser);
	}

// Overrides
public:
	// Outlining Support
	virtual CBCGPOutlineParser* CreateOutlineParser () const
	{
		return new CXmlOutlineParser;
	}

// Implementation
public:

	BOOL OnDrop(COleDataObject* pDataObject, DROPEFFECT dropEffect, CPoint point);
	
	BOOL ToggleCurrentLine (int nCurrRow = -1);
	
	virtual BOOL FindText(LPCTSTR lpszFind, BOOL bNext = TRUE, BOOL bCase = TRUE, BOOL bWholeWord = FALSE);
	BOOL IsPasteEnabled(){ return TRUE; }
	// Marker Support
	virtual void OnDrawMarker(CDC* pDC, CRect rectMarker, const CBCGPEditMarker* pMarker);

	// Tooltip Support
	virtual BOOL OnGetTipText (CString& strTipString);

	// BreakPoints
	virtual BOOL SetMarker(int nCurrRow, BOOL bCurrent = FALSE);
	BOOL PointOutBreakpointMarker(int nCurrRow);
	virtual BOOL ToggleBreakpoint();
	void RemoveAllBreakpoints();
	BOOL IsEnableBreakpoints() { return m_bEnableBreakpoints; }
	BOOL EnableBreakpoints(BOOL bFl = TRUE);

	virtual BOOL CanUpdateMarker (CBCGPEditMarker* pMarker) const
	{
		/*if (pMarker->m_dwMarkerType & g_dwColorHeaderType)
		{
			return FALSE;
		}*/

		return TRUE;
	}

	virtual BOOL CanRemoveMarker (CBCGPEditMarker* pMarker) const  
	{
		/*if (pMarker->m_dwMarkerType & g_dwColorHeaderType)
		{
			return FALSE;
		}*/

		return TRUE;
	}

	virtual BOOL FillIntelliSenseList(CObList& lstIntelliSenseData,	LPCTSTR lpszIntelliSense = NULL) const;
	void ReleaseIntelliSenseList(CObList& lstIntelliSenseData) const;
	void EnableLineNumbersMargin(BOOL enable) { m_bEnableLineNumbersMargin = enable; }

	virtual BOOL OnIntelliSenseComplete(int nIdx, CBCGPIntelliSenseData* pData, CString& strText);
	CBCGPIntelliSenseWnd*	GetIntellisenseWnd() { return m_pIntelliSenseWnd; }

	BOOL InvokeIntelliSense(CObList& lstIntelliSenseData,CPoint ptTopLeft);
	BOOL InvokeIntelliSense();

protected:
	// IntelliSense Support
	virtual BOOL OnBeforeInvokeIntelliSense (const CString& strBuffer, int& nCurrOffset, CString& strIntelliSence) const;
	virtual BOOL IsIntelliSenceWord(CString strWord) const;
	virtual BOOL IntelliSenseCharUpdate(const CString& strBuffer, int nCurrOffset, TCHAR nChar, CString& strIntelliSense);

public:
	virtual void OnOutlineChanges (BCGP_EDIT_OUTLINE_CHANGES& changes, BOOL bRedraw = TRUE);
	
	void AddToolTipItem(LPCTSTR word, LPCTSTR toolTip);
	void ColorVariables(CWoormDocMng* document, BOOL viewMode);

	BOOL PreTranslateMessage(MSG* pMsg);
	void SetIntellisenseMode(BOOL mode);
	BOOL IsIntellisenseActive() { return m_pIntelliSenseWnd ? TRUE : FALSE;}
	void ForceIntellisense();
	BOOL m_bForceIntellisense = FALSE;

   // intellisense
	IntellisenseMap*	m_mIntelliMap;


	// Generated message map functions
protected:
	static CStringList m_lstFind;
	
	virtual BOOL CheckIntelliMark(const CString& strBuffer, int& nOffset, CString& strWordSuffix) const;
	virtual void OnGetCharColor (TCHAR ch, int nOffset, COLORREF& clrText, COLORREF& clrBk);

	//{{AFX_MSG(CCustomEditCtrl)
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	LRESULT PostInvokeIntelliSense(WPARAM, LPARAM);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	BOOL	m_bEnableBreakpoints;
	CMapStringToString	m_mTipString;

	CBCGPToolBarImages	m_ImageBreak;
	CBCGPToolBarImages	m_ImageCurrent;

	inline static int GetNextPos(const CString& strBuffer, const CString& strSkipChars, int& nPos, BOOL bForward);
	BOOL FillIntelliSenceWord(const CString& strBuffer, int nOffset, CString& strIntelliSence) const;
	
	//ENUMS + text operations
	public:
		void FindAndReplaceEnums();		
		void SetWindowText(CString text);
		void ChangeSelectedText(CString str);
		void AddIntellisenseWord(CString key, CString intelliItem, CString intelliValue, CString additionalInfo, CString help);
		void EmptyIntellisense();
	

	private:
		CString FormatEnum(WORD nTag, WORD nItem);
		void ReplaceText(CString, CString);
};

#include "ENDH.dex"
