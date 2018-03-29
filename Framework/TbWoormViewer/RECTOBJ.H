
#pragma once

#include <TbGeneric\tools.h>
#include <TbGeneric\DlgBars.h>

#include "singleob.h"
#include "listdlg.h"
#include "textobj.h"
#include "viewpars.h"
#include "RectObj.hjson" //JSON AUTOMATIC UPDATE

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class ViewParser;
class ViewUnparser;
class CWoormDocMng;
class CTBPicture;
class CBarCode;
class Expression;
class TableColumn;
class CLayout;
class CRichEditControl50W;

//==============================================================================
class TB_EXPORT BaseRect : public SingleItemObj
{
	friend class CWoormDocMng;
	friend class TextRect;
	friend class FieldRect;
	friend class TableColumn;
	friend class CRS_ObjectPropertyView;
	friend class CRSHiddenProp;
	friend class CRSDialogProp;
	friend class CRSMulColorWithExprProp;
	friend class CRSMulIntProp;
	friend class CRSMulBoolProp;
	friend class CRSMulHiddenProp;
	friend class RepeaterObjects;
	friend class CRSTreeCtrl;
	friend class RepeaterObjects;


	DECLARE_DYNAMIC (BaseRect)
public:
	enum DrawMode { HIDDEN, HIDDENEDIT, HIDDENEDITEXPR, VISIBLE };
	
protected:
	Borders			m_Borders;

	int				m_nHRatio = 0;
	int				m_nVRatio = 0;

	BOOL			m_bHidden = FALSE;
	Expression*		m_pHideExpr = NULL;

	Expression*		m_pTextColorExpr = NULL;
	Expression*		m_pBkgColorExpr = NULL;

	Expression*		m_pTooltipExpr = NULL;
	Expression*		m_pBordersExpr = NULL;

	int				m_nDropShadowHeight = 0;
	COLORREF		m_crDropShadowColor;

	//Print attributes
	BOOL			m_bAnchorPageLeft = FALSE;
	BOOL			m_bAnchorPageRight = FALSE;

	BOOL			m_bTemplate = FALSE;
	BOOL			m_bTemplateOverridden = FALSE;
	BaseRect*		m_pDefault = NULL;
public:
	CString			m_sStyleClass;

	WORD			m_AnchorLeftColumnID = 0;   // = 0, anchor to no column 
	WORD			m_AnchorRightColumnID = 0; 

public:
	// costruttori
	BaseRect	(CPoint, CWoormDocMng*);
	BaseRect	(const BaseRect& source);
	virtual ~BaseRect();

public:
			BOOL	SetHideExpression (LPCTSTR szExpr);
	Borders	GetBorders() { return m_Borders; }

	INT		GetRatio()		{	return (m_nHRatio + m_nVRatio) / 2; }
	BOOL	IsRoundedBox()	{	return ((m_nHRatio + m_nVRatio) > 0); }

	virtual BOOL	InMe (CPoint ptWhere);	

	virtual DrawMode GetDrawMode 	(CDC* pDC);

	virtual	BOOL	IsAnchored		()	const { return m_AnchorLeftColumnID != 0; }
	virtual	BOOL	IsAlwaysHidden	()	const { return m_bHidden && !m_pHideExpr; }

	virtual BOOL	CanDeleteField	(LPCTSTR pszFieldName, CString&) const;

	//implementation of baseobj's virtual method to update layout object ->virtual to can be reimplemented
	virtual void		Redraw				();
	//implementation of baseobj's virtual method to update layout object
	virtual COLORREF*	GetBkgColor			()						{ return NULL; }
	virtual Expression** GetBkgColorExpr	()						{ return &m_pBkgColorExpr; }
	virtual COLORREF*	GetLabelForeColor	()						{ return NULL; }
	virtual COLORREF*	GetValueForeColor	()						{ return NULL; }
	virtual int*		GetShadowSize		()						{ return &m_nDropShadowHeight; }
	virtual COLORREF*	GetShadowColor		()						{ return &m_crDropShadowColor; }
	virtual int*		GetBorderSize		()						{ return & m_BorderPen.m_nWidth; }
	virtual COLORREF*	GetBorderColor		()						{ return & m_BorderPen.m_rgbColor; }
	//virtual Borders*	GetBorders			()						{ return & m_Borders; }
	virtual FontIdx		GetCaptionFontIdx	()						{ return NULL; }
	virtual void		SetCaptionFontIdx	(FontIdx fontIdx)		{/*do notthing*/ }
	virtual FontIdx		GetValueFontIdx		()						{ return NULL; }
	virtual void		SetValueFontIdx		(FontIdx fontIdx)		{/*do notthing*/ }
	virtual CString		GetWrmStyleClass	()						{ return m_sStyleClass; }
	virtual void		SetWrmStyleClass	(CString styleClass)	{ m_sStyleClass = styleClass; }
	virtual void		SetWrmStyleClass	(CObject* templateObj)	{ SetStyle(dynamic_cast<BaseRect*>(templateObj)); }

protected:
	virtual	BOOL	ParseProps	(ViewParser& lex);
	virtual	BOOL	ParseBlock	(ViewParser& lex);

	virtual	BOOL	ParseProp	(ViewParser& lex, BOOL bBlock = TRUE);
	virtual	BOOL	OnParseProp	(ViewParser& lex, BOOL bBlock = TRUE) { return FALSE;}

	virtual BOOL	ParseHidden(ViewParser& lex, Token* arStopTokens, int nStopToken);
	virtual void	UnparseHidden (ViewUnparser& ofile);
	virtual void	UnparsePrintInfo (ViewUnparser& ofile);

	BOOL	ParseDynamicColor (ViewParser& lex, COLORREF& rgbColor, Token tk, Expression*& pExpr);
	BOOL	ParseTextColor (ViewParser& lex, COLORREF& rgbColor);
	BOOL	ParseBkgColor (ViewParser& lex, COLORREF& rgbColor);
	
	void	UnparseDynamicColor (ViewUnparser& ofile, Token tk, Expression*& pExpr);

	BOOL	ParseDropShadow (ViewParser& lex);
	BOOL	ParseTooltip (ViewParser& lex);
	void	UnparseDropShadow (ViewUnparser& ofile);
	void	UnparseTooltip (ViewUnparser& ofile);

	void	DrawRound	(CDC&, COLORREF rgbBkgColor, BOOL bPreview, BOOL bOpaque = TRUE);
	void	DrawBorders	(CDC&, Borders*);
	void	DrawDropShadow (CDC&);

	CString MapFile(const CString &strFileName);

	virtual void		Resize			(ActionType, CPoint, CPoint); //reimplementato per impedire resize ai campi 
																	  //ancorati a colonne
			void		DrawIcons			(CDC& DC, CRect inside);
			void		DrawEditingBorder	(CDC& DC); 
			
			void		PostDraw			(CDC& DC, BOOL bPreview, const CRect& inside);
public:
			BOOL		PreDraw				(CDC* pDC);

			void	ShowPopup		(CPoint, CBCGPScrollView*);
	virtual	BOOL	OnShowPopup		(CMenu&) { return TRUE; }

	virtual void	ToggleBorder	(UINT nID);
	virtual BOOL	IsBorderOn		(UINT nID);
	virtual void	ToggleBorder	(UINT nID, BOOL bOn);
	virtual void	SnapToGrid		();

	virtual	void	MoveBaseRect	(int x1, int y1, int x2, int y2, BOOL bRepaint = TRUE, BOOL bIgnoreBorder = FALSE);
	virtual	void	MoveBaseRect	(int xOffset, int yOffset, BOOL bIgnoreBorder = FALSE);
			
			//i metodi seguenti sono parametrici sull'array degli objects perche sono coinvolti nel processo di clonazione
			BOOL	SetAnchorColumn			(CLayout* pObjects, TableColumn*& pLeftCol, TableColumn*& pRightCol);
			void	AnchorFieldToColumn		(CLayout* pObjects, WORD wLeftColID, WORD wRightColID);
			void	FreeFieldFromColumn		(CLayout* pObjects);
			void	FreeFieldFromRepeater	(CLayout* pObjects);
	virtual BOOL	DeleteEditorEntry		();

			void	SetHidden		(BOOL bH = TRUE) { m_bHidden = bH; }

	virtual CString GetTooltip		(int nPage = -1, CPoint pt = CPoint(0,0) ); 

			void	OnCopyAttributes ();
			virtual void	CopyAttributes	(CObject* pObjSource);
			
			virtual void	UnparseProp (ViewUnparser& ofile);
			virtual void	UnparseAuxProp (ViewUnparser& ofile) {}

			void	UpdateDocument (BOOL bUpdateProperty = FALSE);	//TODO

			virtual void	SetStyle		(BaseRect*);
			virtual void	RemoveStyle		();
			virtual void	ClearStyle		();

					BOOL	IsTemplate				() const { return m_bTemplate; }
					BOOL	IsTemplateOverridden	() const { return m_bTemplateOverridden; }
					void	MarkTemplateOverridden	();
			virtual	void	RenameAlias				(int offset);

	BOOL	IsNotDefaultRatio() const;
	BOOL	IsNotDefaultBorderPen() const;
	BOOL	IsNotDefaultBorders() const;
	BOOL	IsNotDefaultTrasparent() const;
	BOOL	IsNotDefaultDropShadow() const;

	virtual BOOL HasDynamicExpr() const;

protected:
	//{{AFX_MSG(BaseRect)
	afx_msg	void	SetBorders		();
	afx_msg	void	SetRatios		();
	afx_msg	void	SetTransparent	();
	
	afx_msg	void	OnSetHidden			();	
	afx_msg	void	OnSetTooltip		();
	afx_msg	void	OnSetDropShadow		();

	afx_msg	void	ToggleAnchorLeft	();
	afx_msg	void	ToggleAnchorRight	();
	afx_msg	void	SetAnchorColumn		();

	afx_msg	void	ToggleTemplate		();
	afx_msg	void	OnSetStyle			();
	afx_msg	void	OnSetStyleName		();
	afx_msg	void	OnSetStyleNameForCopyObject		();
	afx_msg	void	OnSetStyleNameForInheritObject		();
	afx_msg	void	OnClearCustomStyle	();

	afx_msg	void	OnBringToFront		(UINT nID);
	afx_msg	void	OnSendToBack		();

	afx_msg void	OnLayoutCopyObject	();
	afx_msg void	OnLayoutMoveObject	();
	//}}AFX_MSG
public:
	static void SetTemplateFont		(CWoormDocMng* pDoc, FontIdx& f, FontIdx def, LPCTSTR fnt);
	static void RemoveTemplateFont	(CWoormDocMng* pDoc, FontIdx& f, FontIdx def, LPCTSTR fnt);
	static void ClearTemplateFont	(CWoormDocMng* pDoc, FontIdx& f, FontIdx* def, LPCTSTR fnt);
	static void SubstituteTemplateFont	(CWoormDocMng* pDoc, FontIdx& f, FontIdx a, FontIdx b);

	DECLARE_MESSAGE_MAP()
};

//==============================================================================
class TB_EXPORT SqrRect : public BaseRect
{
	friend class CRS_ObjectPropertyView;
	friend class CRSMulColorWithExprProp;

	DECLARE_DYNAMIC (SqrRect)

protected:
	COLORREF m_rgbBkgColor;

protected:
	virtual	BOOL	OnParseProp	(ViewParser& lex, BOOL bBlock = TRUE);

public:
	// costruttori
	SqrRect	(CPoint, CWoormDocMng*);
	SqrRect	(const SqrRect& source);
	virtual BaseObj* Clone() const { return new SqrRect(*this); }

public:
	virtual	void	Draw	(CDC&, BOOL);
		COLORREF	DoDraw  (CDC&, BOOL, CRect& inside);

	// definition file manager function
	virtual	BOOL	Parse	(ViewParser&);
	virtual	void	Unparse	(ViewUnparser&);

	CString GetDescription (BOOL = TRUE) const { return cwsprintf(_T("(%d, %d, %d, %d)"), m_BaseRect.top,m_BaseRect.left,m_BaseRect.bottom,m_BaseRect.right); }

	BOOL	IsNotDefaultBkgColor() const;
	void	SetBkgColor(COLORREF cr) { m_rgbBkgColor = cr; }

	virtual void	SetStyle		(BaseRect*);
	virtual void	RemoveStyle	();
	virtual void	ClearStyle			();

	//implementation of baseobj's virtual method to update layout object
	virtual COLORREF*	GetBkgColor	() { return &m_rgbBkgColor; }
	virtual void		Redraw		();

	COLORREF	GetCurrBkgColor() { return m_rgbBkgColor; }

protected:
	virtual void	UnparseAuxProp	(ViewUnparser&);

	afx_msg	void	SetColors	();

	DECLARE_MESSAGE_MAP()
};

//==============================================================================
class TB_EXPORT TextRect : public BaseRect
{
	friend class FieldRect;
	friend class TableColumn;
	friend class CRS_ObjectPropertyView;
	friend class CRSSetFontDlgProp;
	friend class CRSMulColorWithExprProp;
	friend class CRSDialogProp;
	friend class CRSMulAlignmentStyleProp;
	friend class CRSMulAlignmentStyleBitWiseProp;
	friend class CRSTreeCtrl;
	friend class CWoormPDFmake;

	DECLARE_DYNAMIC (TextRect)

protected:
	StaticText	m_StaticText;		// testo da preporre al campo speciale
	BOOL		m_bSpecialField = FALSE;	// indica il tipo di campo speciale
	BOOL		m_bMiniHtml = FALSE;
	CBarCode*	m_pBarCode = NULL;			// pointer to the class that comunicates with DLL barcode (see baseobj)

public:
	// costruttori
	TextRect (CPoint ptCurrPos, CWoormDocMng* pParent);
	TextRect (const TextRect& source);
	virtual BaseObj* Clone() const { return new TextRect(*this); }
	virtual ~TextRect ();

	CString GetBuildText() const;
	CString BuildText	();
protected:
	virtual	BOOL OnParseProp (ViewParser& lex, BOOL bBlock = TRUE);

	static CString BuildText	(CWoormDocMng* pDocument, CString strLocalizedText);
	static CString FormatToken	(CWoormDocMng* pDocument, LPCTSTR pszToken);

	BOOL	DrawBarCode			(CDC&, CRect&, BOOL bPreview);

public:
	virtual	void	Draw			(CDC&, BOOL bPreview);
	virtual	BOOL	OnShowPopup		(CMenu&);

	// definition file manager function
	virtual	BOOL	Parse	(ViewParser&);
	virtual	void	Unparse	(ViewUnparser&);
	virtual void	UnparseAuxProp (ViewUnparser& ofile);

			BOOL	ParseBarCode	(ViewParser& lex);

	BOOL	IsVerticalBarCode	() const { return m_pBarCode ? m_pBarCode->m_bVertical : FALSE; }

	virtual	void	SetText	(const CString&);
	virtual	CString	GetText	() const { return m_StaticText.GetText(); }

			void	DrawHtmlPreview(CDC&, CRect&);
	
			void	SetBarCode (DWORD eBarCodeType, BOOL bVertical = FALSE);

			BOOL	CheckSpecialField(const CString& sText) const;
			BOOL	SetSpecialField();
			void	SetSpecialField	(BOOL bSpecialField) { m_bSpecialField = bSpecialField; }
			BOOL	GetSpecialField	() const { return m_bSpecialField; }
			BOOL	IsMiniHtlm () const { return m_bMiniHtml; }

	virtual	void	VKReturn	();	// virtual key accelerator management

	CString GetDescription (BOOL = TRUE) const { return m_StaticText.GetText (); }

	virtual void	CopyAttributes	(CObject* pObjSource);

	BOOL	IsNotDefaultBkgColor() const;
	BOOL	IsNotDefaultTextColor() const;
	BOOL	IsNotDefaultAlignment() const;
	BOOL	IsNotDefaultFontStyle() const;

	virtual void	SetStyle		(BaseRect*);
	virtual void	RemoveStyle	();
	virtual void	ClearStyle			();

	//implementation of baseobj's virtual method to update layout object
	virtual COLORREF*	GetBkgColor			()					{ return & m_StaticText.m_rgbBkgColor; }
	virtual COLORREF*	GetValueForeColor	()					{ return &m_StaticText.m_rgbTextColor; }
	virtual FontIdx		GetValueFontIdx		()					{ return m_StaticText.GetFontIdx(); }
	virtual void		SetValueFontIdx		(FontIdx fontIdx)	{ m_StaticText.SetFontIdx(fontIdx); }
	AlignType			GetAlign			()					{ return m_StaticText.GetAlign(); }

	virtual BOOL HasDynamicExpr() const;

protected:
	//{{AFX_MSG(TextRect)
	afx_msg	void	SetAlign		();
	afx_msg	void	SetStaticText	();
	afx_msg	void	SetColors		();
	afx_msg	void	SetFontStyle	();
	afx_msg	void	OnToggleVerticalEABarcode ();
	afx_msg	void	ToggleBarCode ();
	afx_msg	void	OnBarCodeAttrs ();
	//}}AFX_MSG
	
	DECLARE_MESSAGE_MAP()
};

//==============================================================================
class TB_EXPORT FileRect : public TextRect
{
	friend class CRS_ObjectPropertyView;
	friend class CRSSearchFileName;
	friend class CRSSearchTbDialogProp;
	friend class CRSFileNameProp;
	friend class CRSTreeCtrl;

	DECLARE_DYNAMIC (FileRect)

protected:
	CString					m_strFileName;						// contain .txt strFileName
	CRichEditControl50W*	m_pRichEditCtrl;
	BOOL					m_bIsRtf;

public:
	FileRect (CPoint ptCurrPos, const CString& strName, CWoormDocMng* pParent);
	FileRect (const FileRect& source);
	~FileRect ();

	virtual BaseObj* Clone() const { return new FileRect(*this); }

	virtual	void	Draw			(CDC&, BOOL);

	CString GetDescription (BOOL = TRUE) const;

	// definition file manager function
	virtual	BOOL	Parse	(ViewParser&);
	virtual	void	Unparse	(ViewUnparser&);

	void Reload (BOOL bInvalidate);
protected:
	void	ReadTextFromFile	(const CString &strFileName);
	void	ReplaceTag			(CString& sBody);
	// option function member
	virtual	BOOL	OnShowPopup		(CMenu&);
	
	// virtual key accelerator management
	virtual	void	VKReturn		() { /* do nothing */ }

protected:
	afx_msg	void	LoadFromFile	();

	DECLARE_MESSAGE_MAP()
};

//==============================================================================

class TB_EXPORT GraphRect : public SqrRect
{
	friend class CRSSearchImgName;
	friend class CRSSearchTbDialogProp;
	friend class CRS_ObjectPropertyView;
	friend class CRSFileNameProp;
	friend class CRSTreeCtrl;

	DECLARE_DYNAMIC (GraphRect)

protected:
	CString		m_sImage;		// contain ns or path of image
	AlignType	m_nAlign;		// utilizzato quando l'immagine deve essere visualizzata mantenendo le proporzioni o la dimensione originale 

	CTBPicture	m_Bitmap;		// contain the device independent m_Bitmap

	//immagini ritagliate non piu' esposto
	BOOL		m_bIsCutted;	// visualizzazione parziale
	CRect		m_rectCutted;	// porzione di bitmap da visualizzare

public:
	// costruttori
	GraphRect (CPoint ptCurrPos, const CString& strName, CWoormDocMng* pParent);
	GraphRect (const GraphRect& source);
	virtual BaseObj* Clone() const { return new GraphRect(*this); }

	CString GetDescription (BOOL = TRUE) const;

	CTBPicture*	GetBitmap() { return &m_Bitmap; }

protected:
	void	ConstrainBaseRect	();
	void	Invalidate			(BOOL = FALSE);

public:
	// drawing and menu manager virtual function
	virtual	void	Draw			(CDC&, BOOL);
	virtual	BOOL	OnShowPopup		(CMenu&);

	// definition file manager function
	virtual	BOOL	Parse	(ViewParser&);
	virtual	void	Unparse	(ViewUnparser&);

	CRect CalculateBitmapWithFitMode(CRect& rectCuttedHereToFit, BOOL& bHaveToCutted);
	BOOL	IsNotDefaultAlignment() const;

	void	Reload();
protected:
	virtual void	UnparseAuxProp(ViewUnparser& ofile);		//OnUnparseAuxProp
	virtual BOOL	OnParseProp(ViewParser& lex, BOOL bBlock);	//OnParseAuxProp


protected:
	//{{AFX_MSG(GraphRect)
	afx_msg	void	SetBorders		();
	afx_msg	void	LoadFromFile	();
	afx_msg	void	SaveToFile		();
	
	afx_msg	void	RefreshStandardSize		();
	afx_msg	void	RefreshCurrentSize		();

	afx_msg	void	OnUpdateProportionalUI	(CCmdUI* pCmdUI);
	afx_msg	void	OnUpdateCutUI			(CCmdUI* pCmdUI);
	afx_msg	void	OnResizeProportional	();
	afx_msg	void	OnCutBitmap				();
	afx_msg	void	OnCancelCut				();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//==============================================================================
class TB_EXPORT MetafileRect : public SqrRect
{
	DECLARE_DYNAMIC (MetafileRect)

protected:
	CEnhMetaFile	m_emfMetafile;	 
	CString			m_strFileName;	 

public:
	// costruttori
	MetafileRect	(CPoint ptCurrPos, const CString& strName, CWoormDocMng* pParent);
	MetafileRect	(const MetafileRect& source);
	virtual BaseObj* Clone() const { return new MetafileRect(*this); }

	CString GetDescription (BOOL = TRUE) const;

protected:
	void	Invalidate			(BOOL = FALSE);

public:
	// drawing and menu manager virtual function
	virtual	void	Draw			(CDC&, BOOL);
	virtual	BOOL	OnShowPopup		(CMenu&);

	// definition file manager function
	virtual	BOOL	Parse	(ViewParser&);
	virtual	void	Unparse	(ViewUnparser&);

protected:
	//{{AFX_MSG(GraphRect)
	afx_msg	void	SetBorders		();
	afx_msg	void	LoadFromFile	();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

//==============================================================================
class TB_EXPORT UrlRect : public SqrRect
{
	DECLARE_DYNAMIC (UrlRect)

protected:
	CWebBrowser		m_ctrlWeb;	 
	CString			m_strUrl;

protected:
	void	Invalidate			(BOOL = FALSE);

public:
	// costruttori
	UrlRect	(CPoint ptCurrPos, const CString& strName, CWoormDocMng* pParent);
	UrlRect	(const UrlRect& source);
	virtual BaseObj* Clone() const { return new UrlRect(*this); }

	CString GetDescription (BOOL = TRUE) const { return _T("[Url] ") + m_strUrl; }

public:
	// drawing and menu manager virtual function
	virtual	void	Draw			(CDC&, BOOL);
	virtual	BOOL	OnShowPopup		(CMenu&);

	// definition file manager function
	virtual	BOOL	Parse	(ViewParser&);
	virtual	void	Unparse	(ViewUnparser&);

protected:
	afx_msg	void	SetBorders		();
	afx_msg	void	SetUrl			();

	DECLARE_MESSAGE_MAP()
};

//==============================================================================
enum EFieldShowAs {
	FT_NORMAL, FT_IMAGE, FT_BARCODE, FT_TEXTFILE, FT_URL
};

TB_EXPORT CString EFieldShowAsToString(EFieldShowAs f);
//==============================================================================

class TB_EXPORT FieldRect : public BaseRect
{
	friend class TextRect;
	friend class TableColumn;
	friend class CRS_ObjectPropertyView;
	friend class CRSDialogProp;
	friend class CRSSetFontDlgProp;
	friend class CRSFieldTypeProp;
	friend class CRSBarCodeTypeComboProp;
	friend class CRSBarCodeComboProp;
	friend class CRSBarCodeFieldComboProp;
	friend class CRSImageFitProp;
	friend class CRSMulAlignmentStyleProp;
	friend class CRSAlignBitwiseProp;
	friend class CRSBarCodeShowText;
	friend class CRSBarCodeShowTextProp;

	DECLARE_DYNAMIC (FieldRect)

public:
	enum EmailParameter { 
		EP_None=0, 

		EP_From=1, EP_Subject=2, EP_Body=3, EP_Cc=4, EP_Bcc=5, 
		EP_Attachment=6, EP_RecipientIdentity=7, EP_To=8, EP_AttachmentReportName=9, EP_TemplateFileName=10,
		
		EP_Fax=11, EP_Addressee=12, EP_Address=13, EP_City=14, 
		EP_County=15, EP_Country=16, EP_ZipCode=17, EP_ISOCode=18,
		EP_DeliveryType=19, EP_PrintType=20,

		EP_To_by_Certified=21,

		EP_Last=EP_To_by_Certified };

	static const CString EmailParamToString(EmailParameter e);

public:
	//BEGIN Property
		EFieldShowAs	m_ShowAs = EFieldShowAs::FT_NORMAL;
		BOOL			m_bMiniHtml = FALSE;
		//Caption
		Label			m_Label;
		Expression*		m_pLabelTextColorExpr;
		Expression*		m_pLabelTextExpr;

		//Value
		Value			m_Value;

		FormatIdx		m_nFormatIdx;
		Expression*		m_pFormatExpr;

		EmailParameter	m_eEmailParameter;
		BOOL			m_bAppendMailPart;

	//END property
protected:
	Formatter*		m_pFormatter;
	
	CTBPicture*		m_pBitmap;
		BOOL			m_bIsCutted;
		CRect			m_rectCutted;

	CBarCode*		m_pBarCode;		// pointer to the class that comunicates with DLL barcode (see baseobj)
	
	CWebBrowser		m_ctrlWeb;	 

public:
	CArray<int, int> m_arBrowsedOnPage;
	//CArray<int, int> m_arSelectedOnPage;
	CArray<int, int> m_arWrongOnPage;
	CStringArray	 m_arTooltip;

public:
	// costruttori
	FieldRect	(CPoint, CWoormDocMng*, WORD wID = 0);
	FieldRect	(const FieldRect& source);
	~FieldRect ();
	virtual BaseObj* Clone() const { return new FieldRect(*this); }
	virtual  void	SetInternalID();

public:
	BOOL	GetAppendMailPart		() const { return m_bAppendMailPart; }
	void	ClearDynamicAttributes	();
	BOOL	IsBrowsed				(int page);
	BOOL	IsWrong					(int page);
	CString	GetTagName() const;
	virtual CString GetTooltip		(int nPage); 

	virtual	void	Format			();
	FormatIdx		GetFormatIdx	() const { return m_nFormatIdx; }
	void			SetFormatIdx	(FormatIdx fi)  { m_pFormatter = NULL; m_nFormatIdx = fi; }
	Formatter*		GetFormatter	();
	Formatter*		GetDynamicFormatter	() const;

	// for RDEManager manipulation
	virtual	BOOL	AssignData		(WORD wID, RDEManager*);
	virtual	BOOL	ExecCommand		(WORD wID, RDEManager*);
	virtual	void	DisableData		();
			void	ForceDisableData();
	virtual	BOOL	CanSearched		() const;
	virtual BOOL	CanDeleteField	(LPCTSTR pszFieldName, CString&) const;

			void*		GetRDEData			() const { return m_Value.m_RDEdata.GetData(); }
			RDEData*	GetRDEValue			()  { return &(m_Value.m_RDEdata); }
			CString		GetText				() const { return m_Value.GetText(); }
			//void		SetText				(const CString& strText)  { return m_Value.SetText(strText); }

	// for EditorManager manipulation
	virtual	BOOL	DeleteEditorEntry	();
	virtual	BOOL	CheckTypeConsistence(CString& sError);

	virtual	void	ModifyField		();
	virtual	void	SetFormatStyle	();

	virtual	BOOL	IsTextFile() const { return m_ShowAs == FT_TEXTFILE; }

	virtual	BOOL	IsEmailParameter () const { return m_eEmailParameter != EP_None; }
	virtual	EmailParameter	GetEmailParameter () const { return m_eEmailParameter; }

	DataType GetDataType() const;
	CString GetDescription (BOOL bShowLabel = TRUE) const;
	CString GetCaption() const;
	CString GetFieldName () const;
	
	BOOL	IsNotDefaultBkgColor() const;
	BOOL	IsNotDefaultTextColor() const;
	BOOL	IsNotDefaultAlignment() const;
	BOOL	IsNotDefaultFontStyle() const;

	BOOL	IsNotDefaultLabelTextColor() const;
	BOOL	IsNotDefaultLabelAlignment() const;
	BOOL	IsNotDefaultLabelFontStyle() const;

	virtual void	SetStyle	(BaseRect*);
	virtual void	RemoveStyle	();
	virtual void	ClearStyle	();

	virtual	void	RenameAlias	(int offset);

	//implementation of baseobj's virtual method to update layout object
	virtual COLORREF*		GetBkgColor			()					{ return &m_Value.m_rgbBkgColor; }
	virtual Expression**	GetBkgColorExpr		()					{ return &m_pBkgColorExpr; }
	virtual COLORREF*		GetLabelForeColor	()					{ return &m_Label.m_rgbTextColor; }
	virtual COLORREF*		GetValueForeColor	()					{ return &m_Value.m_rgbTextColor; }
	virtual FontIdx			GetCaptionFontIdx	()					{ return m_Label.GetFontIdx(); }
	virtual void			SetCaptionFontIdx	(FontIdx fontIdx)	{ m_Label.SetFontIdx(fontIdx); }
	virtual FontIdx			GetValueFontIdx		()					{ return m_Value.GetFontIdx(); }
	virtual void			SetValueFontIdx		(FontIdx fontIdx)	{ m_Value.SetFontIdx(fontIdx); }

	virtual void			MouseMove(CDC&, CPoint, CBCGPScrollView*, BOOL bTransMode = FALSE);

	void		EvalExpressions			(Value& _Value, Label& _Label);

protected:
			BOOL	ParseBitmap		(ViewParser& lex);
			BOOL	ParseBarCode	(ViewParser& lex);
			BOOL	ParseLabel		(ViewParser& lex);
			BOOL	ParseLabelBlock	(ViewParser& lex);
			BOOL	ParseLabelProps	(ViewParser& lex);
			BOOL	ParseLabelProp	(ViewParser& lex, BOOL bBlock = TRUE);

	virtual	BOOL	OnParseProp		(ViewParser& lex, BOOL bBlock = TRUE);

			void	DrawNormal		(CDC&, CRect&, CSize& = CSize(0,0));
			void	DrawBitmap		(CDC&, CRect&);
			void	DrawBarCode		(CDC&, CRect&, BOOL bPreview);

			void	DrawTextFile	(CDC&, CRect&);

			void	DrawUrlFile		(CDC&, CRect&);

			//void	DrawMiniHtml	(CDC&, CRect&, CSize&);
public:
			void	SetBarCode		(DWORD eBarCodeType, BOOL bVertical = FALSE);
			CString	LoadTextFile	(const CString& strFileText);

	virtual	void	Draw			(CDC&, BOOL bPreview);

	virtual	BOOL	OnShowPopup		(CMenu&);

	// definition file manager function
	virtual	BOOL	Parse	(ViewParser&);
	virtual	void	Unparse	(ViewUnparser&);
	virtual void	UnparseAuxProp (ViewUnparser& ofile);

	// virtual key accelerator management
	virtual	void	VKReturn		();

			CString	GetLabelText	() const { return m_Label.GetText(); }
			void	SetLabelText	(const CString& sLabel) { return m_Label.SetText(sLabel); }
	
	virtual BOOL	GetSchema	(CXSDGenerator*, WoormTable*);

	virtual void	CopyAttributes	(CObject* pObjSource);

	virtual BOOL	HasDynamicExpr() const;

protected:	
			void	Invalidate		(BOOL bCurrentSize = FALSE);
			void	ConstrainBaseRect ();

protected:
	afx_msg	void	SetFontStyle	();

	afx_msg void	SetFieldLabel		();
	afx_msg void	SetColors			();
	afx_msg	void	SetLabelFontStyle	();
	afx_msg	void	SetAlign			();
	afx_msg	void	SetLabelAlign		();
	afx_msg	void	ToggleBitmap		();
	afx_msg	void	ToggleBarCode		();
	afx_msg	void	ToggleTextFile		();
	afx_msg	void	OnBarCodeAttrs		();

	afx_msg	void	OnUpdateProportionalUI	(CCmdUI* pCmdUI);
	afx_msg	void	OnUpdateCutUI			(CCmdUI* pCmdUI);
	afx_msg	void	OnResizeProportional	();
	afx_msg	void	OnCutBitmap				();
	afx_msg	void	OnCancelCut				();
	afx_msg	void	RefreshStandardSize		();
	//afx_msg	void	ToggleLink				();
	afx_msg	void	ToggleEmailParameter	(UINT nID);	
	afx_msg	void	ToggleAppendRecipients	();	
	afx_msg	void	OnDynamicFormatStyle	();

private:
	BOOL m_bTransMode = FALSE;

	DECLARE_MESSAGE_MAP()
};

//==============================================================================
class TB_EXPORT BarCodeFieldRect : public FieldRect
{
	DECLARE_DYNAMIC (BarCodeFieldRect)
public:
	// costruttori
	BarCodeFieldRect	(CPoint, CWoormDocMng*, WORD wID = 0);
	BarCodeFieldRect	(const BarCodeFieldRect& source);
	~BarCodeFieldRect ();
	virtual BaseObj* Clone() const { return new BarCodeFieldRect(*this); }

	DECLARE_MESSAGE_MAP()
};
//==============================================================================



TB_EXPORT CString FromNs2Path(const CString& sName, CTBNamespace::NSObjectType t1, CTBNamespace::NSObjectType t2);

//==============================================================================
#include "endh.dex"



