#pragma once

#include "PARSOBJ.H"
#include "beginh.dex"

//========================================================================================================
class TB_EXPORT CTBGaugeManager
{
public:
	enum LINEAR_GAUGE_GRADIENT_TYPE
	{
		LINEAR_GAUGE_GRADIENT_TYPE_FIRST = 0,
		LINEAR_GAUGE_NO_GRADIENT = LINEAR_GAUGE_GRADIENT_TYPE_FIRST,
		LINEAR_GAUGE_GRADIENT_HORIZONTAL = 1,
		LINEAR_GAUGE_GRADIENT_VERTICAL = 2,
		LINEAR_GAUGE_GRADIENT_DIAGONAL_LEFT = 3,
		LINEAR_GAUGE_GRADIENT_DIAGONAL_RIGHT = 4,
		LINEAR_GAUGE_GRADIENT_CENTER_HORIZONTAL = 5,
		LINEAR_GAUGE_GRADIENT_CENTER_VERTICAL = 6,
		LINEAR_GAUGE_GRADIENT_RADIAL_TOP = 7,
		LINEAR_GAUGE_GRADIENT_RADIAL_CENTER = 8,
		LINEAR_GAUGE_GRADIENT_RADIAL_BOTTOM = 9,
		LINEAR_GAUGE_GRADIENT_RADIAL_LEFT = 10,
		LINEAR_GAUGE_GRADIENT_RADIAL_RIGHT = 11,
		LINEAR_GAUGE_GRADIENT_RADIAL_TOP_LEFT = 12,
		LINEAR_GAUGE_GRADIENT_RADIAL_TOP_RIGHT = 13,
		LINEAR_GAUGE_GRADIENT_RADIAL_BOTTOM_LEFT = 14,
		LINEAR_GAUGE_GRADIENT_RADIAL_BOTTOM_RIGHT = 15,
		LINEAR_GAUGE_GRADIENT_BEVEL = 16,
		LINEAR_GAUGE_GRADIENT_PIPE_VERTICAL = 17,
		LINEAR_GAUGE_GRADIENT_PIPE_HORIZONTAL = 18,
		LINEAR_GAUGE_GRADIENT_TYPE_LAST = LINEAR_GAUGE_GRADIENT_PIPE_HORIZONTAL
	};

	enum SUB_GAUGE_POS
	{
		BCGP_SUB_GAUGE_POS_FIRST = 0,
		BCGP_SUB_GAUGE_NONE = BCGP_SUB_GAUGE_POS_FIRST,
		BCGP_SUB_GAUGE_TOP = 1,
		BCGP_SUB_GAUGE_BOTTOM = 2,
		BCGP_SUB_GAUGE_LEFT = 3,
		BCGP_SUB_GAUGE_RIGHT = 4,
		BCGP_SUB_GAUGE_POS_LAST = BCGP_SUB_GAUGE_RIGHT
	};

private:
	CBCGPLinearGaugeColors		linearColors;
	CBCGPCircularGaugeColors	circularColors;
	CBCGPTextFormat				m_TextFormat;
	CBCGPTextGaugeImpl*			m_pCustomLabelGauge;
	double						m_dOpacity;


protected:
	CBCGPGaugeImpl*		m_pGaugeImpl;
	BOOL				m_bNoPointer;
	TBThemeManager*     m_pTBThemeManager;
	CString				m_sName;
	DataDbl				m_dMin;
	DataDbl				m_dMax;
	BOOL				m_bFullCircularColor;
	//todo - generalizzare per n-pointers - First area from min to m_dMin to m_nValue; secon area from m_nValue to m_dMax
	COLORREF*			m_pFirstAreaClr;	
	COLORREF*			m_pSecondAreaClr;

protected:
	void				SetValue		(double nValue, int nScale = 0, UINT uiAnimationTime = 100 /* Ms, 0 - no animation */, BOOL bRedraw = FALSE);
	void				AttachGauge		(CBCGPGaugeImpl* pGaugeImpl) { m_pGaugeImpl = pGaugeImpl; }
	void				SetGaugeColors	();
	
public:
	CTBGaugeManager() : m_pGaugeImpl(NULL), m_bNoPointer(FALSE), m_dMin(0.0), m_dMax(100.0), m_pFirstAreaClr(NULL), m_pSecondAreaClr(NULL)
	{ 
		m_pTBThemeManager = AfxGetThemeManager(); 
		m_bFullCircularColor = FALSE; 
		m_pCustomLabelGauge = NULL; 
		m_dOpacity = .5;
	}
	~CTBGaugeManager() {}

public:
	void				SetFont(CFont* pFont);
	void				SetOpacity(double dOpacity) { m_dOpacity = dOpacity; }
	void				SetFrameSize(int nFrameSize);
	int					AddPointer(int nScale = 0, BOOL bRedraw = FALSE);
	void				RemovePointer(int i, BOOL bRedraw = FALSE);
	void				RemoveAllPointers(BOOL bRedraw = FALSE);
	void				AddColoredRange(double nStartValue, double nFinishValue, COLORREF colorStart, COLORREF colorEnd, LINEAR_GAUGE_GRADIENT_TYPE eGradientType, double nWidth = 10.);
	void				AddColoredRange(double nStartValue, double nFinishValue, COLORREF color = -1, double nWidth = 10.);
	void				ModifyRange(int index, double nStartValue, double nFinishValue, COLORREF color = -1, BOOL bRedraw = FALSE);
	void				AddFullColoredRange(double nStartValue, double nFinishValue, COLORREF colorStart, COLORREF colorEnd, LINEAR_GAUGE_GRADIENT_TYPE eGradientType);
	void				AddFullColoredRange(double nStartValue, double nFinishValue, COLORREF color);
	void				RemoveAllColoredRanges();
	void				SetGaugeRange(double nStartValue, double nEndValue);
	void				SetStep(double nStep, int nScale = 0);
	void				SetMajorTickMarkStep(double nStep, int nScale = 0);
	void				SetDirty(BOOL bSet = TRUE, BOOL bRedraw = FALSE);
	void				SetMajorTickMarkSize(double nSize, int nScale = 0);
	void				SetMinorTickMarkSize(double nSize, int nScale = 0);
	void				SetTextLabelFormat(const CString& aLabelFormat);
	void				SetBkgColor(const COLORREF& aBkgColor, double aOpacity = 1.0);
	void				SetBkgGradientColor(const COLORREF& aBkgColor, const COLORREF& aBkgGradientColor, LINEAR_GAUGE_GRADIENT_TYPE eGradientType);
	void				SetFrameBkgColor(const COLORREF& aBkgColor, double aOpacity = 1.0);
	void				SetFrameBkgGradientColor(const COLORREF& aBkgColor, const COLORREF& aBkgGradientColor, LINEAR_GAUGE_GRADIENT_TYPE eGradientType);
	void				SetFrameOutlineColor(const COLORREF& aColor, double aOpacity = 1.0);
	void				SetPointerBkgColor(const COLORREF& aBkgColor);
	void				SetPointerBkgGradient(const COLORREF& aBkgColor, const COLORREF& aBkgGradientColor, LINEAR_GAUGE_GRADIENT_TYPE eGradientType);
	void				SetPointerOutlineColor(const COLORREF& aColor);
	void				SetTickMarkBkgColor(const COLORREF& aBkgColor, double aOpacity = 1.0);
	void				SetTickMarkBkgGradientColor(const COLORREF& aBkgColor, const COLORREF& aBkgGradientColor, LINEAR_GAUGE_GRADIENT_TYPE eGradientType);
	void				SetTickMarkOutlineColor(const COLORREF& aColor);
	void				SetForeColor(const COLORREF& aColor);
	//manage circular Gauge
	void				EnableShapeByTicksArea(BOOL bEnable = TRUE);
	void				SetTicksAreaAngles(double nStartAngle, double mFinishAngle, int nScale = 0);
	void				SetCapSize(double nSize);
	void				SetZoom(double nInflateW, double nInflateH, double nWidth, double nHeight);
	void				SetPointerSize(double nWidth, double nLength, BOOL bExtraLength = FALSE, int nIdxPointer = 0);
	void				AddCustomLabel(const CString& aLabelText, const COLORREF& aColorText, SUB_GAUGE_POS eSubGaugePos, BOOL bStyleUnderline = FALSE, BOOL bStyleBold = FALSE, BOOL bStyleItalic = FALSE);
	void				SetCustomLabelStyle(const CString& aLabelText, const COLORREF& aTextColor, BOOL bStyleUnderline = FALSE, BOOL bStyleBold = FALSE, BOOL bStyleItalic = FALSE);
	void				AddCustomImage(const CString& nsImage, SUB_GAUGE_POS eSubGaugePos);
	//void				RemoveSubGauges();
};


//==================================================================================
class TB_EXPORT CTBLinearGaugeCtrl : public CBCGPLinearGaugeCtrl, public CTBGaugeManager, public CParsedCtrl, public ResizableCtrl, public IDisposingSourceImpl
{
	DECLARE_DYNCREATE(CTBLinearGaugeCtrl)

public:
	CTBLinearGaugeCtrl(const CString sName = _T(""));
	~CTBLinearGaugeCtrl() {}

public:
	BOOL				Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	
public:
	virtual	void		SetValue(const DataObj& aValue);	//TODO = SetValue/GetValue array of DataObj
	virtual	void		GetValue(DataObj& aValue);
	virtual	DataObj*	GetMinValue() { return &m_dMin; }
	virtual	DataObj*	GetMaxValue() { return &m_dMax; }
	virtual	void		SetMinValue(const DataObj& value) { m_dMin = (const DataDbl&)value; }
	virtual	void		SetMaxValue(const DataObj& value) { m_dMax = (const DataDbl&)value; }
	virtual BOOL		CheckDataObjType(const DataObj* pDataObj = NULL);
	virtual BOOL		SubclassEdit(UINT nID, CWnd* pParent, const CString& );
	virtual DataType	GetDataType()	const { return DataType::Double; }


protected:
	BOOL				OnInitCtrl();
protected:
	afx_msg	LRESULT		OnRecalcCtrlSize(WPARAM, LPARAM);
	afx_msg	void		OnLButtonDown(UINT nFlags, CPoint point);

	DECLARE_MESSAGE_MAP()
};


//==================================================================================
class TB_EXPORT CTBCircularGaugeCtrl : public CBCGPCircularGaugeCtrl, public CTBGaugeManager, public CParsedCtrl, public ResizableCtrl, public IDisposingSourceImpl
{
	DECLARE_DYNCREATE(CTBCircularGaugeCtrl)

public:
	CTBCircularGaugeCtrl(const CString sName = _T(""));
	~CTBCircularGaugeCtrl() { /*RemoveSubGauges();*/ }

private:
	COLORREF			m_clrBaseColor;

public:
	BOOL				Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);
	//automatic management for full colored areas
	void				SetColorFirstArea(COLORREF* aColor);
	void				SetColorSecondArea(COLORREF* aColor);

public:
	virtual	void		SetValue(const DataObj& aValue);	
	virtual	void		GetValue(DataObj& aValue);
	virtual	DataObj*	GetMinValue() { return &m_dMin; }
	virtual	DataObj*	GetMaxValue() { return &m_dMax; }
	virtual	void		SetMinValue(const DataObj& value) { m_dMin = (const DataDbl&)value; }
	virtual	void		SetMaxValue(const DataObj& value) { m_dMax = (const DataDbl&)value; }
	virtual BOOL		CheckDataObjType(const DataObj* pDataObj = NULL);
	virtual BOOL		SubclassEdit(UINT nID, CWnd* pParent, const CString&);
	virtual DataType	GetDataType()	const { return DataType::Double; }


protected:
	BOOL				OnInitCtrl();
protected:
	afx_msg	LRESULT		OnRecalcCtrlSize(WPARAM, LPARAM);
	afx_msg	void		OnLButtonDown(UINT nFlags, CPoint point);

	DECLARE_MESSAGE_MAP()
};


#include "endh.dex"