#pragma once

#include <BCGCBPro\BCGPPropList.h>
#include "parsobj.H"
#include "PARSCBX.H"
#include "beginh.dex"

class DataObj;
class CBaseDocument;
class CTBPropertyGrid;
class HotKeyLink;
class CCheckBitmap;

//==================================================================================
class TB_EXPORT CTBProperty : public CBCGPProp, public IDisposingSourceImpl
{
	DECLARE_DYNAMIC(CTBProperty)

	friend class CTBPropertyGrid;

public:
	CTBProperty	(
					CString sName,
					CString sPropertyLeftText,
					CString sPropertyBottomText,
					int		nRowsNumber = 1
				);
	CTBProperty (
					CString				sName,
					CString				sPropertyLeftText,
					CString				sPropertyBottomText,
					const _variant_t&	value,
					int					nRowsNumber = 1
				);
	virtual ~CTBProperty();

private:
	CString				m_sObjectName;
	CParsedCtrl*		m_pParsedCtrl;
	BOOL				m_bHotLinkInline;
	BOOL				m_bViewHKLCode;
	BOOL				m_bViewHKLDescription;
	CCheckBitmap*		m_pCheckBitmap;
	CBitmap				m_CollapsedImage;
	CBitmap				m_ExpandedImage;
	CBitmap				m_CollapsedImageSubGroups;
	CBitmap				m_ExpandedImageSubGroups;
	BOOL				m_bForcedAlwaysEnabled;

	// property custom colors
	COLORREF			m_clrGroupBackground;	
	COLORREF			m_clrGroupText;		
	COLORREF			m_oldclrGroupBackground;
	COLORREF			m_oldclrGroupText;
	int					m_nRowsNumber;

private:
	void				DestroyAndCreateFB			(CFont* fromFont, BOOL bUnderline);
	CTBProperty*		GetNearlySubProperty		(CTBProperty* pCurrProp, CTBProperty*& pPrevProp, BOOL bPrev);
	CTBProperty*		GetNextEditableProperty		(CList<CBCGPProp*, CBCGPProp*>* pLstProp, POSITION& pos);

	CTBProperty*		AddSubProperty				(CTBProperty* pChildProperty);
	void				SetGroup					(BOOL isGroup);
	void				SetOwnerList				(CBCGPPropList* pWndList) { ASSERT_VALID(pWndList); __super::SetOwnerList(pWndList); }

	void				InitCollapseExpandImages	();

public:
	void				SetHotLinkInline			(BOOL bViewHKLCode = TRUE, BOOL bViewHKLDescription = TRUE);
	void				SetDataReadOnly				(BOOL bRO);
	void				UpdateStatus				();
	void				SetAlwaysEnabled			(BOOL bEnable) { m_bForcedAlwaysEnabled = bEnable; }
	void				SetControl					(CParsedCtrl* pCtrl);
	CParsedCtrl*		GetControl					() { return m_pParsedCtrl; }
	void				DoValueChanged				();
	void				DoValuesChanged				();
	BOOL				RemoveSubItem				(CTBProperty*& pProp, BOOL bDelete = TRUE);
	void				RemoveAllSubItems			();
	void				SetDataModified				(BOOL bMod);
		

	COLORREF			GetGroupBackground();
	COLORREF			GetrGroupText();
	void				SetGroupBackground(COLORREF clr);
	void				SetrGroupText(COLORREF clr);
	CWndObjDescription* GetControlDescription(CWndObjDescriptionContainer* pContainer, int index);

	CString				GetName()			 { return m_sObjectName; }			//nome programmativo dell'oggetto(token di namespace)
	void				SetName(const CString& sName) { m_sObjectName = sName; }
	CString				GetText()			 { return m_strName; }				//nome della proprietà
	void				SetText(const CString& sText) { m_strName = sText; }
	CString				GetDescription()	 { return m_strDescr; }				//descrizione (hint in basso)
	void				SetDescription(const CString& sDescription) { m_strDescr = sDescription; }
	CWnd*				GetWndInPlace() { return m_pWndInPlace; }

public:
	virtual BOOL		OnEdit						(LPPOINT lptClick);
	virtual BOOL		OnEndEdit					();
	
	virtual CWnd*		CreateInPlaceEdit			(CRect rectEdit, BOOL& bDefaultFormat);
	virtual void		OnDestroyWindow				();
	virtual CString		FormatProperty				();
	virtual CString		OnFormatHotLinkDescription	();
	virtual void		OnDrawValue					(CDC* pDC, CRect rect);
	virtual BOOL		OnUpdateValue				();
	virtual void		OnDrawExpandBox				(CDC* pDC, CRect rectExpand);
	virtual void		OnBeforeDrawProperty		();
	virtual void		OnAfterDrawProperty			();

};

//==================================================================================
class TB_EXPORT CTBPropertyGrid : public CBCGPPropList, public CGridControlObj, public IDisposingSourceImpl
{
	DECLARE_DYNCREATE(CTBPropertyGrid)

	friend class CTBProperty;
	friend class CRSEditorParametersView;
	friend class CRSEditorParametersViewDebug;
private:
	BOOL					m_bValidatingCell;
	CTBProperty*			m_pBadProperty;
	BOOL					m_bGridFocused;
	BOOL					m_bGridActive;
	BOOL					m_bSetFocusOnly;
	CString					m_SearchBoxPrompt;
	CTBProperty*			m_pDefaultRootProperty;
	BOOL					m_bDefaultRootProperty;
	TBThemeManager*			m_pTBThemeManager;
	BOOL					m_bDestroyingCompoents;
protected:
	CFont*					m_pHyperlinkFont;
	COLORREF				m_clrHyperLinkForeColor;
	BOOL					m_bLButtonDownHit;
	
public:
	CTBPropertyGrid(const CString sName = _T(""));
	virtual ~CTBPropertyGrid(void);

public:
	void				RemoveAllComponents();

	CTBProperty*		FindItemByID		(UINT nIDC, BOOL bSearchSubItems = TRUE);
	CParsedCtrl*		GetActiveParsedCtrl	();
	void				SetMarkModifiedProperties(BOOL bMarkModifiedProperties = TRUE);
	void				SetHasDefaultRootProperty(BOOL bHasRootProperty = TRUE);

	CTBProperty*		AddProperty	(	
										CString			sName, 
										CString			sPropertyLeftText, 
										CString			sPropertyBottomText, 
										UINT			nIDC, 
										DWORD			dwStyle = 0,
										DataObj*		pDataObj = NULL, 
										CRuntimeClass*	pParsedCtrlClass = NULL,
										HotKeyLink*		pHotKeyLink = NULL,
										UINT			nBtnID = BTN_DEFAULT,
										SqlRecord*		pSqlRecord = NULL,
										int				nRowsNumber = 1
									);
	CTBProperty*		AddProperty	(	
										CString			sName, 
										CString			sPropertyLeftText, 
										CString			sPropertyBottomText, 
										DataObj*		pDataObj = NULL, 
										UINT			nIDC = 0, 
										DWORD			dwStyle = 0,
										CRuntimeClass*	pParsedCtrlClass = NULL,
										HotKeyLink*		pHotKeyLink = NULL,
										UINT			nBtnID = BTN_DEFAULT,
										SqlRecord*		pSqlRecord = NULL,
										int				nRowsNumber = 1
									);
	CTBProperty*		AddSubItem	(
										CTBProperty*	pParentProperty,
										CString			sName,
										CString			sPropertyLeftText,
										CString			sPropertyBottomText,
										UINT			nIDC,
										DWORD			dwStyle = 0,
										DataObj*		pDataObj = NULL,
										CRuntimeClass*	pParsedCtrlClass = NULL,
										HotKeyLink*		pHotKeyLink = NULL,
										UINT			nBtnID = BTN_DEFAULT,
										SqlRecord*		pSqlRecord = NULL,
										int				nRowsNumber = 1
									);
	CTBProperty*		AddSubItem	(
										CTBProperty*	pParentProperty,
										CString			sName,
										CString			sPropertyLeftText,
										CString			sPropertyBottomText,
										DataObj*		pDataObj = NULL,
										UINT			nIDC = 0,
										DWORD			dwStyle = 0,
										CRuntimeClass*	pParsedCtrlClass = NULL,
										HotKeyLink*		pHotKeyLink = NULL,
										UINT			nBtnID = BTN_DEFAULT,
										SqlRecord*		pSqlRecord = NULL,
										int				nRowsNumber = 1
									);
	BOOL				Create				(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	void				InitPropList		();
	BOOL				DeleteProperty		(CTBProperty*& pProp, BOOL bRedraw = TRUE, BOOL bAdjustLayout = TRUE);
	BOOL				RemoveSubItem		(CTBProperty* pParentProperty, CTBProperty*& pSubItem, BOOL bDelete = TRUE);
	void				RemoveAllSubItems	(CTBProperty* pParentProperty);
	COLORREF			SetGroupBackgroundColor(COLORREF clr);
	COLORREF			SetGroupTextColor	(COLORREF clr);

	virtual CString		GetCtrlClass()	{ return _T("TBPropertyGrid"); }
	virtual CString		GetCtrlName()	{ return GetInfoOSL()->m_Namespace.GetObjectName(); }
private:
	void				BeginDestroyingComponents();
	void				EndDestroyingComponents();

	CTBProperty*		CreateProperty		(
												CString			sName,
												CString			sPropertyLeftText,
												CString			sPropertyBottomText,
												DWORD			dwStyle,
												DataObj*		pDataObj = NULL,
												UINT			nIDC = 0,
												CRuntimeClass*	pParsedCtrlClass = NULL,
												HotKeyLink*		pHotKeyLink	= NULL,
												UINT			nBtnID = BTN_DEFAULT,
												SqlRecord*		pSqlRecord = NULL,
												int				nRowsNumber = 1
											);

	BOOL				DoMoveToProp	(BOOL bPrev = TRUE);
	BOOL				UpdateData		(BOOL bSignalError, BOOL bSendMessage = FALSE);
	void				SetFocusOnly	();

public:
	virtual	void			Customize				();
	virtual CParsedCtrl*	GetCurrentParsedCtrl	(UINT nIDC)				{ ASSERT(FALSE); return NULL;}
	virtual CParsedCtrl*	GetCurrentParsedCtrl	(CTBNamespace aNS)		{ ASSERT(FALSE); return NULL;}
	virtual CParsedCtrl*	GetCurrentParsedCtrl	(DataObj* pDataObj)		{ ASSERT(FALSE); return NULL;}
	virtual void			OnUpdateControls		(BOOL bParentIsVisible = TRUE);
	virtual void			SetDataModified			(BOOL bMod);
	
	virtual BOOL	DoMovingKey				(UINT nChar);
	virtual void	EnableControlLinks		(BOOL bEnable = TRUE, BOOL bMustSetOSLReadOnly = FALSE);

protected:
	virtual	void	OnAbortForm			();
	virtual	BOOL	OnCheckForm			(BOOL bEmitError);
	virtual	BOOL	OnKeyHit			(UINT nIDC, UINT nKey, UINT nHitState);
	virtual int		DoToolHitTest		(CPoint, TOOLINFO*)					{ ASSERT(FALSE); return 0; }
	virtual BOOL	DoToolTipNotify		(CTooltipProperties&)				{ ASSERT(FALSE); return TRUE; }

	virtual BOOL	SubclassDlgItem		(UINT nID, CWnd* pParent);
	virtual	BOOL	OnCommand			(WPARAM wParam, LPARAM lParam);
	virtual BOOL	PreTranslateMessage	(MSG* pMsg);
	virtual BOOL	EditItem			(CBCGPProp* pProp, LPPOINT lptClick = NULL);
	virtual BOOL	ValidateItemData	(CBCGPProp* /*pProp*/);
	virtual BOOL	OnDrawProperty(CDC* pDC, CBCGPProp* pProp) const;
	virtual void	OnCustomize			()	{}

protected:
	afx_msg	void 	OnVScroll			(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg void	OnKillFocus			(CWnd* pOldWnd);
	afx_msg void	OnSetFocus			(CWnd* pOldWnd);
	afx_msg	void	OnContextMenu		(CWnd* pWnd, CPoint ptMousePos);
	afx_msg void	OnLButtonDown		(UINT nFlags, CPoint point);
	afx_msg void	OnRButtonDown		(UINT nFlags, CPoint point);
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);
	afx_msg LRESULT OnBadValue			(WPARAM, LPARAM);
	afx_msg LRESULT	OnLosingFocus		(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT	OnValueChanged		(WPARAM wParam, LPARAM lParam);
	afx_msg void	OnDestroy			();
	afx_msg BOOL	OnEraseBkgnd		(CDC* pDC);
	afx_msg BOOL	OnMouseWheel		(UINT nFlags, short zDelta, CPoint pt);
	afx_msg LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	DECLARE_MESSAGE_MAP()
};

//==============================================================================
class TB_EXPORT CWndPropertyGridItemDescription : public CEditObjDescription
{
	DECLARE_DYNCREATE(CWndPropertyGridItemDescription)
protected:
	CWndPropertyGridItemDescription() {		m_Type = CWndObjDescription::PropertyGridItem; }
	~CWndPropertyGridItemDescription() {	delete m_pItemSourceDescri; }

public:

	bool m_bSort = false;
	bool m_bCollapsed = false;
	CItemSourceDescription* m_pItemSourceDescri = NULL;

	CWndPropertyGridItemDescription(CWndObjDescription* pParent) : CEditObjDescription(pParent) {}
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);

protected:
	virtual void EvaluateExpressions(CJsonContextObj * pJsonContext, bool deep = true);
	virtual void Assign(CWndObjDescription* pDesc);
};


#include "endh.dex"