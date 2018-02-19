
#pragma once

#include <TbGenlib\OslInfo.h>
#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\TBWebFriendlyMenu.h>
#include <TbGenlib\ParsCbx.h>
#include "BaseDoc.h"
#include "beginh.dex"

class CMasterToolCbx;
#define TOOLBAR_DRAW_SMALL_BUTTON_WIDTH 16
#define TOOLBAR_DRAW_LARGE_BUTTON_WIDTH 24

#define	INFINITY_SEP_WIDTH				1
/////////////////////////////////////////////////////////////////////////////
// Macro per Woorm & Radar 
/////////////////////////////////////////////////////////////////////////////
//Woorm & Radar
//

#define FIND_BAR_DROPDOWN_MENU_ON_UPDATE_COMMAND_UI(OnMethodToCall, OnMethodToNext, OnMethodToPrev, OnMethodToFindEdit)\
	ON_UPDATE_COMMAND_UI(ID_DROPDOWN_MENU_CONDITION_EQUAL, OnMethodToCall)\
	ON_UPDATE_COMMAND_UI(ID_DROPDOWN_MENU_CONDITION_NOTEQUAL, OnMethodToCall)\
	ON_UPDATE_COMMAND_UI(ID_DROPDOWN_MENU_CONDITION_LESS, OnMethodToCall)\
	ON_UPDATE_COMMAND_UI(ID_DROPDOWN_MENU_CONDITION_LESSEQUAL, OnMethodToCall)\
	ON_UPDATE_COMMAND_UI(ID_DROPDOWN_MENU_CONDITION_GREATER, OnMethodToCall)\
	ON_UPDATE_COMMAND_UI(ID_DROPDOWN_MENU_CONDITION_GREATEREQUAL, OnMethodToCall)\
	ON_UPDATE_COMMAND_UI(ID_TOOLBAR_FIND_NEXT, OnMethodToNext)\
	ON_UPDATE_COMMAND_UI(ID_TOOLBAR_FIND_PREV, OnMethodToPrev)\
	ON_UPDATE_COMMAND_UI(ID_TOOLBAR_FIND_EDITBOX, OnMethodToFindEdit)

#define FIND_BAR_DROPDOWN_MENU_ON_UPDATE_COMMAND()\
	ON_COMMAND(ID_DROPDOWN_MENU_CONDITION_EQUAL,		OnFindConditionEqual)\
	ON_COMMAND(ID_DROPDOWN_MENU_CONDITION_NOTEQUAL,		OnFindConditionNotEqual)\
	ON_COMMAND(ID_DROPDOWN_MENU_CONDITION_LESS,			OnFindConditionLess)\
	ON_COMMAND(ID_DROPDOWN_MENU_CONDITION_LESSEQUAL,	OnFindConditionLessEqual)\
	ON_COMMAND(ID_DROPDOWN_MENU_CONDITION_GREATER,		OnFindConditionGreater)\
	ON_COMMAND(ID_DROPDOWN_MENU_CONDITION_GREATEREQUAL, OnFindConditionGreaterEqual)\
	ON_COMMAND(ID_DROPDOWN_MENU_CONDITION_BEGINSWITH,	OnFindConditionGreaterBeginsWith)\
	ON_COMMAND(ID_DROPDOWN_MENU_CONDITION_ENDSWITH,		OnFindConditionGreaterEndsWith)\
	ON_COMMAND(ID_DROPDOWN_MENU_CONDITION_CONTAINS,		OnFindConditionGreaterContains)\
	ON_COMMAND(ID_DROPDOWN_MENU_CONDITION_DOESCONTAINS, OnFindConditionGreaterDoesNotContain)\

#define FIND_BAR_METHOD()\
	public:\
		ECompareType GetFindCondition() { return m_iFindCondition; }\
		void		 SetFindCondition (ECompareType nCond) { m_iFindCondition = nCond;}\
	private:\
		ECompareType m_iFindCondition;\
	protected:\
		\
		afx_msg void OnFindConditionEqual (){ SetFindCondition(ECompareType::CMP_EQUAL);}\
		afx_msg void OnFindConditionNotEqual(){ SetFindCondition(ECompareType::CMP_NOT_EQUAL);}\
		afx_msg void OnFindConditionLess(){ SetFindCondition(ECompareType::CMP_LESSER_THEN); }\
		afx_msg void OnFindConditionLessEqual(){ SetFindCondition(ECompareType::CMP_LESSER_OR_EQUAL);}\
		afx_msg void OnFindConditionGreater(){ SetFindCondition(ECompareType::CMP_GREATER_THEN);}\
		afx_msg void OnFindConditionGreaterEqual(){ SetFindCondition(ECompareType::CMP_GREATER_OR_EQUAL);}\
		afx_msg void OnFindConditionGreaterBeginsWith(){ SetFindCondition(ECompareType::CMP_BEGIN_WITH);}\
		afx_msg void OnFindConditionGreaterEndsWith() { SetFindCondition(ECompareType::CMP_END_WITH);}\
		afx_msg void OnFindConditionGreaterContains(){ SetFindCondition(ECompareType::CMP_CONTAINS);}\
		afx_msg void OnFindConditionGreaterDoesNotContain(){ SetFindCondition(ECompareType::CMP_NOT_CONTAINS);}

#define FIND_ADD_TOOLBAR(pToolBar)\
	pToolBar->AddEditToRight(ID_TOOLBAR_FIND_EDITBOX, STANDARD_IMAGE_LIBRARY_NS, _T("RadarFindEdit"), 150, ES_AUTOHSCROLL, _TB("Search :"));\
	pToolBar->AddButtonToRight(ID_TOOLBAR_FIND_PREV, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconPrev, IconSize::TOOLBAR), _TB("Prev"));\
	pToolBar->AddButtonToRight(ID_TOOLBAR_FIND_NEXT, STANDARD_IMAGE_LIBRARY_NS, TBIcon(szIconNext, IconSize::TOOLBAR), _TB("Next"));\
	pToolBar->SetDropdown(ID_TOOLBAR_FIND_NEXT);

#define DROPDOWN_MENU_POPOLATE(pToolBar, pDataObj, iFind) \
	{\
		CTBToolBarMenu menu;\
		menu.CreateMenu();\
		WORD wType = pDataObj->GetDataType().m_wType;\
		if (wType != DATA_ENUM_TYPE || wType != DATA_BOOL_TYPE)\
			{\
			switch (wType)\
			{\
				case DATA_STR_TYPE	:\
				case DATA_TXT_TYPE	:\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_CONTAINS			? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_CONTAINS ,	 CompareTypeStrings::CONTAINS() );\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_NOT_CONTAINS		? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_DOESCONTAINS , CompareTypeStrings::DOESCONTAINS() );\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_EQUAL			? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_EQUAL ,			 CompareTypeStrings::EQUAL() );\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_NOT_EQUAL		? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_NOTEQUAL ,		 CompareTypeStrings::NOTEQUAL() );\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_BEGIN_WITH		? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_BEGINSWITH ,	 CompareTypeStrings::BEGINSWITH() );\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_END_WITH			? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_ENDSWITH ,	 CompareTypeStrings::ENDSWITH() );\
					break;\
			\
				case DATA_INT_TYPE	:\
				case DATA_LNG_TYPE	:\
				case DATA_DBL_TYPE	:\
				case DATA_DATE_TYPE	:\
				case DATA_MON_TYPE	:\
				case DATA_PERC_TYPE	:\
				case DATA_QTA_TYPE	:\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_EQUAL			? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_EQUAL ,			 CompareTypeStrings::EQUAL());\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_NOT_EQUAL		? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_NOTEQUAL ,		 CompareTypeStrings::NOTEQUAL());\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_LESSER_THEN		? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_LESS ,		 CompareTypeStrings::LESS());\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_LESSER_OR_EQUAL	? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_LESSEQUAL ,	 CompareTypeStrings::LESSEQUAL());\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_GREATER_THEN		? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_GREATER ,		 CompareTypeStrings::GREATER());\
					menu.AppendMenu(MF_STRING | (iFind == ECompareType::CMP_GREATER_OR_EQUAL	? MF_CHECKED : MF_UNCHECKED ), ID_DROPDOWN_MENU_CONDITION_GREATEREQUAL , CompareTypeStrings::GREATEREQUAL());\
				break;\
			}\
			}\
		pToolBar->UpdateDropdownMenu(nIdCommand, &menu);\
	}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

#define	TOOLBARMENU_ICON_SIZE	25		// Max 32 not over!

#define STANDARD_IMAGE_LIBRARY_NS _T("Framework.TbGes.TbGes")
#define TOOLBAR_IMAGE_LIBRARY_ADD_NS  ((CString)_T("Framework.TbGes.TbGes."))

typedef CArray<HACCEL, HACCEL> AcceleratorArray;

#define TOOLBAR_NAMETOOLS		_T("Tools")
#define	PURE_ALWAYS_DROPDOWN	2
#define	MIXED_ALWAYS_DROPDOWN	1

/////////////////////////////////////////////////////////////////////////////
// TaskBuilderToolbarImageSet
//
// Questa classe permette di definire le immagini associate ad un singolo bottone di toolbar
// Per ogni bottone vanno specificate immagini per lo stato normal, disabled e hot (mouse over),
// sia small che large. Il namespace della library che li contiene permetterà di caricarla on-demand
// Se viene usato il costruttore con un sola immagine ci pensera la TBToolbar a generare a runtime le altre
// "versioni" (disabilitata, ecc.)
class TB_EXPORT TaskBuilderToolbarImageSet
{
public:
	TaskBuilderToolbarImageSet()
	{
		m_nIDBmpNormalLarge			= 0;
		m_nIDBmpNormalSmall			= 0;
		m_nIDBmpDisabledLarge		= 0;
		m_nIDBmpDisabledSmall		= 0;
		m_nIDBmpHotLarge			= 0;
		m_nIDBmpHotSmall			= 0;
		m_nIDSingleImageNormal		= 0;
		m_nIDSingleImageHot			= 0;
		m_bUseEdgeEnhancement		= TRUE;
		m_bPNG						= FALSE;
	}

	TaskBuilderToolbarImageSet
		(
			const	CString	strLibraryNamespace,
					UINT	nIDBmpNormalLarge,
					UINT	nIDBmpNormalSmall,
					UINT	nIDBmpDisabledLarge,
					UINT	nIDBmpDisabledSmall,
					UINT	nIDBmpHotLarge,
					UINT	nIDBmpHotSmall,
					BOOL	bUseEdgeEnhancement = TRUE,
					BOOL	bPNG				= FALSE
		)
	{
		m_strLibraryNamespace	= strLibraryNamespace;
		m_nIDBmpNormalLarge		= nIDBmpNormalLarge;
		m_nIDBmpNormalSmall		= nIDBmpNormalSmall;
		m_nIDBmpDisabledLarge	= nIDBmpDisabledLarge;
		m_nIDBmpDisabledSmall	= nIDBmpDisabledSmall;
		m_nIDBmpHotLarge		= nIDBmpHotLarge;
		m_nIDBmpHotSmall		= nIDBmpHotSmall;
		m_bUseEdgeEnhancement	= bUseEdgeEnhancement;

		m_nIDSingleImageNormal = 0;
		m_nIDSingleImageHot    = 0;
		m_bPNG					  = bPNG;
	}

	TaskBuilderToolbarImageSet
		(
			const	CString	strLibraryNamespace,
					UINT	nIDSingleImageNormal,
					UINT	nIDSingleImageHot = 0,
					BOOL	bUseEdgeEnhancement = TRUE,
					BOOL	bPNG				= FALSE
		)
	{
		m_strLibraryNamespace	= strLibraryNamespace;

		m_nIDSingleImageNormal	= nIDSingleImageNormal;
		m_nIDSingleImageHot		= nIDSingleImageHot;

		m_bUseEdgeEnhancement	= bUseEdgeEnhancement;
		m_bPNG					= bPNG;

		m_nIDBmpNormalLarge		= 0;
		m_nIDBmpNormalSmall		= 0;
		m_nIDBmpDisabledLarge	= 0;
		m_nIDBmpDisabledSmall	= 0;
		m_nIDBmpHotLarge		= 0;
		m_nIDBmpHotSmall		= 0;
	}

	TaskBuilderToolbarImageSet
		(
			const	CString	strLibraryNamespace,
					CString	sBmpNormalLargeName,
					CString	sBmpNormalSmallName,
					CString	sBmpDisabledLargeName,
					CString	sBmpDisabledSmallName,
					CString	sBmpHotLargeName,
					CString	sBmpHotSmallName,
					BOOL	bUseEdgeEnhancement = TRUE,
					BOOL	bPNG				= FALSE
		)
	{
		m_strLibraryNamespace	= strLibraryNamespace;
		m_sBmpNormalLargeName	= sBmpNormalLargeName;
		m_sBmpNormalSmallName	= sBmpNormalSmallName;
		m_sBmpDisabledLargeName	= sBmpDisabledLargeName;
		m_sBmpDisabledSmallName	= sBmpDisabledSmallName;
		m_sBmpHotLargeName		= sBmpHotLargeName;
		m_sBmpHotSmallName		= sBmpHotSmallName;
		m_bUseEdgeEnhancement	= bUseEdgeEnhancement;
		m_bPNG					= bPNG;

		m_sSingleImageNormal.Empty();
		m_sSingleImageHot.Empty();
		
	}

	TaskBuilderToolbarImageSet
		(
			const	CString	strLibraryNamespace,
			const	CString	sSingleImageNormal,
			const	CString	sSingleImageHot = _T(""),
					BOOL	bUseEdgeEnhancement = TRUE,
					BOOL	bPNG				= FALSE
		)
	{
		m_strLibraryNamespace	= strLibraryNamespace;
		m_sSingleImageNormal	= sSingleImageNormal;
		m_sSingleImageHot		= sSingleImageHot;
		m_bUseEdgeEnhancement	= bUseEdgeEnhancement;
		m_bPNG					= bPNG;

		m_sBmpNormalLargeName.Empty();
		m_sBmpNormalSmallName.Empty();
		m_sBmpDisabledLargeName.Empty();
		m_sBmpDisabledSmallName.Empty();
		m_sBmpHotLargeName.Empty();
		m_sBmpHotSmallName.Empty();
	}

public:
	TaskBuilderToolbarImageSet& operator = (const TaskBuilderToolbarImageSet& aImgSet)
	{
		m_strLibraryNamespace	= aImgSet.m_strLibraryNamespace;

		m_nIDBmpNormalLarge		= aImgSet.m_nIDBmpNormalLarge;
		m_nIDBmpNormalSmall		= aImgSet.m_nIDBmpNormalSmall;
		m_nIDBmpDisabledLarge	= aImgSet.m_nIDBmpDisabledLarge;
		m_nIDBmpDisabledSmall	= aImgSet.m_nIDBmpDisabledSmall;
		m_nIDBmpHotLarge		= aImgSet.m_nIDBmpHotLarge;
		m_nIDBmpHotSmall		= aImgSet.m_nIDBmpHotSmall;

		m_nIDSingleImageNormal		= aImgSet.m_nIDSingleImageNormal;
		m_nIDSingleImageHot			= aImgSet.m_nIDSingleImageHot;

		m_sBmpNormalLargeName		= aImgSet.m_sBmpNormalLargeName;
		m_sBmpNormalSmallName		= aImgSet.m_sBmpNormalSmallName;
		m_sBmpDisabledLargeName		= aImgSet.m_sBmpDisabledLargeName;
		m_sBmpDisabledSmallName		= aImgSet.m_sBmpDisabledSmallName;
		m_sBmpHotLargeName			= aImgSet.m_sBmpHotLargeName;
		m_sBmpHotSmallName			= aImgSet.m_sBmpHotSmallName;

		m_sSingleImageNormal		= aImgSet.m_sSingleImageNormal;
		m_sSingleImageHot			= aImgSet.m_sSingleImageHot;
		m_bPNG						= aImgSet.m_bPNG;

		m_bUseEdgeEnhancement	= aImgSet.m_bUseEdgeEnhancement;

		return *this;
	}

public:
	CString	m_strLibraryNamespace;
	UINT	m_nIDBmpNormalLarge;
	UINT	m_nIDBmpNormalSmall;
	UINT	m_nIDBmpDisabledLarge;
	UINT	m_nIDBmpDisabledSmall;
	UINT	m_nIDBmpHotLarge;
	UINT	m_nIDBmpHotSmall;

	CString	m_sBmpNormalLargeName;
	CString	m_sBmpNormalSmallName;
	CString	m_sBmpDisabledLargeName;
	CString	m_sBmpDisabledSmallName;
	CString	m_sBmpHotLargeName;
	CString	m_sBmpHotSmallName;

	UINT	m_nIDSingleImageNormal;
	UINT	m_nIDSingleImageHot;
	CString	m_sSingleImageNormal;
	CString	m_sSingleImageHot;

	BOOL	m_bUseEdgeEnhancement;
	BOOL	m_bPNG;
};

class CTBToolBar;

/////////////////////////////////////////////////////////////////////////////
// 
class TB_EXPORT CInfoOSLButton: public CInfoOSL
{
public:
	UINT		m_nID;
	CString		m_strName;


	CInfoOSLButton (UINT id, CString strName) 
		:
		CInfoOSL	(OSLType_Function), 
		m_nID		(id),
		m_strName	(strName)
	{}

	CInfoOSLButton () 
		:
		CInfoOSL (OSLType_Function), 
		m_nID		(0) 
	{}
};

/////////////////////////////////////////////////////////////////////////////
// CTaskBuilderToolBar
/*
	Estensione di CToolbar per toolbar con bottoni con immagini true color sia large (24x24 pxls) che
	small (16x16 pxls). Gestisce l'aggiunta dinamica sia di bottoni singoli che di intere toolbar. Per ogni bottone può  
	essere impostato lo stile dropdown, sia con tendina separata che per l'intero bottone (stile BTN_WHOLEDROPDOWN)
	Sono supportati bottoni con più immagini alternative (es.: sospendi / riprendi)

	La toolbar va creata con il metodo CreateEmpty.

	I bottoni si possono aggiungere sia singolarmente (metodi AddButton) che in gruppo (metodi AddButtons). I bottoni 
	vengono sempre aggiunti in fondo alla toolbar. I bitmap possono essere specificati sia esplicitamente che incapsulati 
	in un TaskBuilderToolbarImageSet (vedi sopra).

	È possibile specificare che gli ID dei bitmap vengano rintracciati in una particolare dll, indicandone il namespace. Se 
	il namespace è vuoto, vengono ricercati nella corrente dll (o meglio, visto che si usa la AfxFindResourceHandle, parte
	il "walking" sulle risorse)

	Se il numero di immagini è superiore a quello dei comandi, si assume che le immagini extra siano alternative per 
	uno o più bottoni (es.: sospendi / riprendi). Per commutare dall'immagine standard a quella alternativa si usa il 
	metodo SetAlternativeImage: va indicata la posizione relativa nella lista delle	immagini di quella a cui passare. 
	Nel caso più semplice c'è un bottone con una alternativa, quindi questa ha posizione relativa +1.

	Ogni bottone può essere dichiarato "dropdown", con il meotodo SetDropdown. Per gestire il menu e le azioni relative, 
	va intercettato il messaggio di notifica TBN_DROPDOWN sull'ID AFX_IDW_TOOLBAR (ID standard assegnato da MFC alle toolbar)

	Per tutte le altre azioni (es.: nascondere un bottone, impostare altri stili), si rimanda alle caratteristiche standard
	di CToolBar
*/
class CAbstractFormFrame;
class CToolBarButtons;


// 
// bcgsoft ToolBar
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CImageAssociation : public CObject
{
private:
	UINT m_nCommandID;
	UINT m_nImageID;
	UINT m_nIndex;		// number of immage in LockedImages
	INT  m_nIndexAlternative;

	UINT m_nOldImageID;
	CString m_nOldImageNS;

	BOOL m_bCustomImage;
	BOOL m_bCustomText;

	CString		m_sImageNameSpace;
	CString		m_sButtonNameSpace;
	CString		m_strText;
	CString		m_strTextAlternative;

public:
	CImageAssociation(UINT nCommandID, UINT nImageIDn, UINT nIndex = 0);
	CImageAssociation(UINT nCommandID, CString sImageNameSpace, UINT nIndex = 0);

	const UINT GetCommandID() const; 
	const UINT GetImageID() const; 
	const UINT GetIndex() const;
	
	void SetImageNameSpace(CString sImageNameSpace) {m_sImageNameSpace = sImageNameSpace; }
	CString GetImageNameSpace() {return m_sImageNameSpace; }

	void SetButtonNameSpace(CString sButtonNameSpace) {m_sButtonNameSpace = sButtonNameSpace; }
	CString GetButtonNameSpace() {return m_sButtonNameSpace; }

	void SetCustom(BOOL bCustom = TRUE)			{m_bCustomImage = bCustom;}
	void SetCustomText(BOOL bCustom = TRUE)		{m_bCustomText = bCustom;}
	
	BOOL IsCustom()	{return m_bCustomImage;}
	BOOL IsCustomText()	{return m_bCustomText;}

	void SetOldIdImage(UINT nID)				{m_nOldImageID = nID;}
	UINT GetOldIdImage()						{return m_nOldImageID;}

	void	SetOldNsImage(CString nsImageOld)	{ m_nOldImageNS = nsImageOld; }
	CString GetOldNsImage()						{ return m_nOldImageNS; }

	void SetAlternativeIndexImage(INT nImage)	{m_nIndexAlternative = nImage;}
	INT GetAlternativeIndexImage()				{return m_nIndexAlternative;}

	void SetText(CString mText, CString m_strTextAlternative = _T(""));
	CString GetText()			 { return m_strText;}
	CString GetTextAlternative() { return m_strTextAlternative;}
};

/////////////////////////////////////////////////////////////////////////////
class CTBToolbarEditBoxButton : public CBCGPToolbarEditBoxButton
{
	DECLARE_SERIAL(CTBToolbarEditBoxButton)
	
public:
	CTBToolbarEditBoxButton();
	CTBToolbarEditBoxButton(const CString& sPrompt);
	~CTBToolbarEditBoxButton();

	CString GetLabel();
	void SetLabel(const CString& text);

	virtual void CopyFrom (const CBCGPToolbarButton& src);

	virtual void OnMove ();

	virtual void OnDraw (CDC* pDC, const CRect& rect, CBCGPToolBarImages* pImages,
						BOOL bHorz = TRUE, BOOL bCustomizeMode = FALSE,
						BOOL bHighlight = FALSE,
						BOOL bDrawBorder = TRUE,
						BOOL bGrayDisabledButtons = TRUE);

	virtual HBRUSH OnCtlColor(CDC* pDC, UINT nCtlColor);

	void SetHeight(int iHeight);
	int  GetHeight();

	void SetFont(CFont* pFont, BOOL bRedraw = TRUE);
	void SetIcon(HICON hIcon);

private:
	HICON	m_hIcon;
	CString m_stLabel;
	int		m_iHeight;
};

/////////////////////////////////////////////////////////////////////////////
class CTBToolbarComboBoxButton : public CBCGPToolbarComboBoxButton
{
	DECLARE_SERIAL(CTBToolbarComboBoxButton)

public:
	CTBToolbarComboBoxButton ();
	CTBToolbarComboBoxButton (UINT uiID, DWORD dwStyle = CBS_DROPDOWNLIST, int iWidth = 0, const CString& sPrompt = _T(""));

	~CTBToolbarComboBoxButton();

	CString GetLabel();
	void SetLabel(const CString& text);

	virtual void OnMove();

	virtual void OnDraw(CDC* pDC, const CRect& rect, CBCGPToolBarImages* pImages,
		BOOL bHorz = TRUE, BOOL bCustomizeMode = FALSE,
		BOOL bHighlight = FALSE,
		BOOL bDrawBorder = TRUE,
		BOOL bGrayDisabledButtons = TRUE);

	virtual void CopyFrom(const CBCGPToolbarButton& src);

private:
	CString m_stLabel;
	bool  m_bMove;
	CRect m_TmpRectCombo;
	CRect m_TmpRerectButton;
	CRect m_TmpRrect;
	
};

/////////////////////////////////////////////////////////////////////////////
class CTBToolbarButton : public CBCGPToolbarButton
{
	DECLARE_SERIAL(CTBToolbarButton)
	friend class CTBToolBar;
private:
	BOOL	m_bGhost;
	BOOL    m_bClone;
	BOOL	m_bWantText;		// allow buttons with empty text among others with text
	BOOL	m_bSecondaryFillColor; 
	BOOL	m_bDefaultButton;
	CRect	m_rectButton;
	BOOL	m_bAutoHide;

public:
	CTBToolbarButton();
	virtual ~CTBToolbarButton();

	virtual void OnDraw (CDC* pDC, const CRect& rect, CBCGPToolBarImages* pImages,
						BOOL bHorz = TRUE, BOOL bCustomizeMode = FALSE,
						BOOL bHighlight = FALSE,
						BOOL bDrawBorder = TRUE,
						BOOL bGrayDisabledButtons = TRUE);

	virtual void CopyFrom (const CBCGPToolbarButton& src);
	virtual SIZE OnCalculateSize(CDC* pDC, const CSize& sizeDefault, BOOL bHorz);

	void SetGhost		(BOOL bGhost = TRUE);
	void SetClone		(BOOL bClone = TRUE);
	void SetWantText	(BOOL bWantText = TRUE);
	BOOL IsGhost		() { return m_bGhost;}
	BOOL IsClone        () { return m_bClone; }
	BOOL WantText		() { return m_bWantText;}

	void SecondaryFillColorButton(BOOL bEnable = TRUE) { m_bSecondaryFillColor = bEnable; }
	BOOL IsSecondaryFillColorButton() { return m_bSecondaryFillColor; }

	void SetDefaultButton(BOOL bEnable = TRUE) { m_bDefaultButton = bEnable; }
	BOOL isDefaultButton() { return m_bDefaultButton; }
	CRect getRectButton() { return m_rectButton; }

	BOOL GetAutoHide() { return m_bAutoHide; }
	void SetAutoHide(BOOL bAutoHide) { m_bAutoHide = bAutoHide; }
};

/////////////////////////////////////////////////////////////////////////////
class CTBToolbarLabel : public CBCGPToolbarButton  
{
	DECLARE_SERIAL(CTBToolbarLabel)
public:
	CTBToolbarLabel(UINT uiID = 0, LPCTSTR lpszText = NULL);
	virtual ~CTBToolbarLabel();

	void SetWidth(int width);
	
	virtual void OnDraw (CDC* pDC, const CRect& rect, CBCGPToolBarImages* pImages, 
						 BOOL bHorz = TRUE, BOOL bCustomizeMode = FALSE,						BOOL bHighlight = FALSE,						BOOL bDrawBorder = TRUE, 
						 BOOL bGrayDisabledButtons = TRUE);   	

	virtual BOOL IsEditable () const	
	{	
		return FALSE; 
	}

	virtual SIZE OnCalculateSize(CDC*pDC,const CSize&sizeDefault,BOOL bHorz);
	virtual void CopyFrom(const CBCGPToolbarButton& src);

private:
	BOOL		m_bCustomWidth;
	BOOL		m_bIsRightSpace;
	UINT		m_iWidth;
	UINT		m_nAlign;
	COLORREF	m_textColor;
	BOOL		m_bTitle;

public:
	void SetRightSpace();
	void SetTextAlign(UINT nAlign = TA_BOTTOM);
	UINT GetTextAlign() { return m_nAlign; }
	BOOL IsRightSpace() {return m_bIsRightSpace;} 
	void SetColorText(COLORREF col) { m_textColor  = col;}
	void SetTitle();
	BOOL IsTiitle() { return m_bTitle;}
	COLORREF GetColorText() { return m_textColor; }
};

/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CIconList
{
private:
	HICON		hIco;
	CString		mText;
	UINT		iID;

public:
	CIconList();
	CIconList(HICON hico, UINT nID, CString mText = _T(""));
	
	virtual ~CIconList();

	HICON		GetIcon() {return hIco;};
	UINT		GetId()   {return iID;};
	CString		GetText() {return mText;};
};

/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTBToolBarMenu : public CTBWebFriendlyMenu
{
public: 
	CTBToolBarMenu();
	~CTBToolBarMenu();

	BOOL AppendMenu(UINT nFlags, UINT_PTR nIDNewItem = NULL, LPCTSTR lpszNewItem = NULL, UINT nIDImgUnchecked = 0, UINT nIDImgChecked = 0, BOOL bPng = TRUE);
	BOOL InsertMenu(UINT nPosition, UINT nFlags, UINT_PTR nIDNewItem = NULL, LPCTSTR lpszNewItem = NULL, UINT nIDImgUnchecked = 0, UINT nIDImgChecked = 0, BOOL bPng = TRUE);

	BOOL AppendMenu(UINT nFlags, UINT_PTR nIDNewItem , LPCTSTR lpszNewItem, CBitmap* pBmpUnchecked, CBitmap* pBmpChecked = NULL);
	BOOL InsertMenu(UINT nPosition, UINT nFlags, UINT_PTR nIDNewItem, LPCTSTR lpszNewItem, CBitmap* pBmpUnchecked, CBitmap* pBmpChecked = NULL);

	BOOL AppendMenu(UINT nFlags, UINT_PTR nIDNewItem, LPCTSTR lpszNewItem, const CString& sImageNSUnchecked, const CString& sImageNSChecked = NULL);
	BOOL InsertMenu(UINT nPosition, UINT nFlags, UINT_PTR nIDNewItem, LPCTSTR lpszNewItem, const CString& sImageNSUnchecked, const CString& sImageNSChecked = NULL);
	
	void SetMenuItemBitmaps(UINT nID, CBitmap* pBmpUnchecked, CBitmap* pBmpChecked);
	void SetMenuItemBitmaps(UINT nID, const CString& aLibNamespace, UINT nIDImgUnchecked, UINT nIDImgChecked, BOOL bPng = TRUE);
	void SetMenuItemHICON(UINT nID, HICON hIconUnchecked, HICON hIconChecked);

	HICON	GetIconUnChecked(UINT nID);
	HICON	GetIconChecked(UINT nID);

	void SetDC(CDC* pDC);

	void AppendFromMenu(CMenu* pSorg);

	void RemoveAll();
	BOOL ExistID(int nIDToFound);

private:
	void IconsListClean();

private:
	CList<CIconList, CIconList&> m_iconsListUnChecked;
	CList<CIconList, CIconList&> m_iconsListChecked;
	CDC* m_pDC;
};

/////////////////////////////////////////////////////////////////////////////
class CTBToolbarMenuButton : public CBCGPToolbarMenuButton
{
	DECLARE_SERIAL(CTBToolbarMenuButton)
public:
	CTBToolbarMenuButton();
	virtual ~CTBToolbarMenuButton();
	virtual CBCGPPopupMenu* CreatePopupMenu ();
	virtual void CopyFrom (const CBCGPToolbarButton& src);
	virtual SIZE OnCalculateSize(CDC* pDC, const CSize& sizeDefault, BOOL bHorz);

	void	TextBelow(BOOL bBelow = TRUE);
	CWnd*   GetMessageWnd() { return m_pWndMessage;}
	void	EnableAlwaysDropDown(BOOL bAlwaysDropDown = TRUE);
	int		GetAlwaysDropDown() const;
	void	SetAlwaysDropDown(int nAlwaysDropDown);
	
	// TODO: this was protected,
	// but I get link errors with OpenMenu.
	virtual BOOL OpenPopupMenu(CWnd* pWnd = NULL);
	void CreateMenu(CMenu* pMenu);
	void OnPopulatedMenuButton();
	HMENU GetMenu();

	BOOL GetAutoHide() { return m_bAutoHide; }
	void SetAutoHide(BOOL bAutoHide) { m_bAutoHide = bAutoHide; }

	void SetMissingClick(BOOL bMissing);
	BOOL HasMissingClick() const;

private:
	BOOL			m_bAutoHide;

protected:
	BOOL			m_bTBelow;
	int				m_nAlwaysDropDown;
	BOOL			m_MissingClick;

protected:
	virtual void OnDraw (CDC* pDC, const CRect& rect, CBCGPToolBarImages* pImages,
						BOOL bHorz = TRUE, BOOL bCustomizeMode = FALSE,
						BOOL bHighlight = FALSE,
						BOOL bDrawBorder = TRUE,
						BOOL bGrayDisabledButtons = TRUE);
};

class CTBSupportTabbedToolbar;
class CTBSupportToolBar;

/////////////////////////////////////////////////////////////////////////////
class CTBTabWndToolbar : public CBCGPTabWnd
{
public:
	CTBTabWndToolbar();
	virtual ~CTBTabWndToolbar();

	virtual void OnDraw(CDC* pDC);
	virtual int GetTabsHeight() const;
	
	void	SetShowToolBarTab(BOOL bAutoHide = TRUE) { m_bShowToolBarTab = bAutoHide; }
	BOOL	GetShowToolBarTab() { return m_bShowToolBarTab; }

protected:
	BOOL	m_bShowToolBarTab;

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

protected:
	afx_msg void OnPaint();

	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTBTabbedToolbar : public CBCGPTabbedToolbar, public IOSLObjectManager 
{
private:
	BOOL				m_bUseLargeButtons;
	CArray<CTBToolBar*> m_Toolbars;
	BOOL				m_bSuspendLayout;
	UINT				m_iIdSwitch;
	UINT				m_iIdSwitchMenuStart;
	CDocument*			m_pParentDoc;
	BOOL				m_bAutoHideToolBarButton;
	BOOL				m_bShowToolBarTab;
	BOOL				m_bHighlightedColorClickEnable;
	BOOL				m_bSwitchEnableAlwaysDropDown;

public:
	CTBTabbedToolbar();
	virtual ~CTBTabbedToolbar();
	
	virtual BOOL Create	(CWnd* pParentWnd, CStringArray* pNamespaceArray = NULL);

	CDocument*	GetParentDocument() { return	m_pParentDoc; }

	BOOL ShowInDialog(CWnd* pParentWnd = NULL);
	virtual void EnableDocking (CWnd* pParentWnd, DWORD dwAlignment = CBRS_ALIGN_TOP | CBRS_ALIGN_LEFT); 

	BOOL SetButtonInfo(UINT nID, UINT nStyle, UINT nIDImage, LPCTSTR lpszText = NULL, BOOL bPng = TRUE);
	BOOL SetButtonInfo(UINT nID, UINT nStyle, const CString& sImageNameSpace = _T(""),	LPCTSTR lpszText = NULL);
	BOOL SetButtonInfo(int nIndexToolBar, int nIndexButon, UINT nID, UINT nStyle, UINT nIDImage, LPCTSTR lpszText, BOOL bPng = TRUE);

	BOOL HideGhostButtons ( const	UINT* HideIDs = NULL, const	UINT* GhostIDs = NULL, const UINT* ShowIDs = NULL);
	BOOL HideButton (UINT nID, BOOL bHide = TRUE);
	BOOL IsHideButton(UINT nID);
	BOOL DeleteButton(UINT nID);
	
	BOOL ChangeImage(UINT nID, UINT nIDImag, UINT nIDImageAlternative, BOOL bPng = TRUE);
	BOOL ChangeImage(UINT nID, const CString& sImageNameSpace, const CString& sImageAlternativeNameSpace = _T(""), const CString& sCollapsedImageNameSpace = _T(""));
	BOOL SetCollapsedImage(CString stImageNameSpace);

	BOOL GhostButton(UINT nID, BOOL bGhost = TRUE);

	BOOL SetDropdown			(UINT nCommandID, CMenu* mMenu = NULL, LPCTSTR lpszText = NULL);
	BOOL UpdateDropdownMenu		(UINT nCommandID, CMenu* mMenu);
	//BOOL DisableButtonAutoHidden(UINT nID, BOOL bDisable = TRUE);
	BOOL AddDropdownMenuItem	(UINT nCommandID, UINT nFlags, UINT_PTR nIDNewItem = NULL, LPCTSTR lpszNewItem = NULL, UINT nIDImgUnchecked = 0, UINT nIDImgChecked = 0, BOOL bPng = TRUE);
	HMENU CopyDropdownMenu(UINT nCommandID);
	BOOL InsertDropdownMenuItem	(UINT nPos, UINT nCommandID, UINT nFlags, UINT_PTR nIDNewItem = NULL, LPCTSTR lpszNewItem = NULL, UINT nIDImgUnchecked = 0, UINT nIDImgChecked = 0, BOOL bPng = TRUE);

	BOOL RemoveDropdown			(UINT nCommandID);
	HICON GetIconDropdownMenu	(const CBCGPToolbarMenuButton* pMenuButton);

	int GetToolBarsCount() { return m_Toolbars.GetCount(); }

	virtual BOOL		AddTab (CTBToolBar* pBar, BOOL bVisible = TRUE, BOOL bSetActive = FALSE, BOOL bDetachable = TRUE);
	virtual BOOL		AddTabBefore(CTBToolBar* pBar, LPCTSTR lpszBeforeLabelToolBar, BOOL bVisible = TRUE, BOOL bSetActive = FALSE, BOOL bDetachable = TRUE);

	virtual BOOL		OnPopulatedDropDown(UINT nIdCommand);
	virtual void		AdjustLayout();
	void				AdjustLayoutActiveTab();

	CTBToolBar*			FindToolBar(LPCTSTR lpszText);
	CTBToolBar*			FindToolBarOrAdd(CWnd* pParentWnd, LPCTSTR lpszText);
	INT					FindButton(CString sNameSpace);
	CInfoOSLButton*		FindOslInfoButton(CString sNameSpace);
	CInfoOSLButton*		FindOslInfoButton(UINT nID);

	CTBToolBar*			GetToolBar(int nTab);
	CTBToolBar*			GetToolBarActive();

	BOOL				SetActiveTab(LPCTSTR lpszText);
	BOOL				SetActiveTab(UINT	 nTabId);
	BOOL				SetActiveTabByPos(UINT nPos);
	void				MoveNextTab();
	
	BOOL				RemoveTab	(LPCTSTR lpszText);
	BOOL				RenameTab(LPCTSTR lpszToolBarName, CString stNewTabLabel);
	void				ClosePopupMenu();

	// Returns the currently active tab.
	int					GetActiveTab();
	void				AttachOSLInfo (CInfoOSL* pParent);

	BOOL				SuspendLayout();
	void				ResumeLayout(BOOL bForced = FALSE);
	BOOL				IsLayoutSuspended() {return AfxIsRemoteInterface() ? TRUE : m_bSuspendLayout; }

	CTBToolBar*			FindToolBar(UINT nCommandID);

	void SetStartIDCollapsed(UINT nID);
	void AdjustLayoutImmediate();
	void DoUpdateSize();
	
	UINT GetIDSwitch() { return m_iIdSwitch; }
	void SetIDSwitch(UINT iIdSwitch, UINT iIdSwitchMenuStart);
	void SetSwitchEnableAlwaysDropDown(BOOL b);
	
	static	CWndObjDescription* GetControlStructure(CWndObjDescriptionContainer* pContainer, CTBToolBar* pItem,/* CBaseTabDialog* pDialog,*/ CTBTabbedToolbar* pParentTabManager);

	INT		CalcMinToolBar();

	void	SetAutoHideToolBarButton(BOOL bAutoHide = TRUE) { m_bAutoHideToolBarButton = bAutoHide; }
	BOOL	GetAutoHideToolBarButton() { return m_bAutoHideToolBarButton; }

	void	SetShowToolBarTab(BOOL bAutoHide = TRUE);
	BOOL	GetShowToolBarTab() { return m_bShowToolBarTab; }

	void EnableHighlightedColorClick(BOOL bEnable = TRUE) { m_bHighlightedColorClickEnable = bEnable;  };
	BOOL IsEnableHighlightedColorClick() { return m_bHighlightedColorClickEnable;  };

	void AddDocumentTitle(CString strTitle);

	void UpdateTabWnd();
	void UpdateTabWnd(INT nTab);

public:
	INT	 GetMaxWidth()	{ return m_nWidth;}
	void GetRemovedListID(CList<int, int>* pList);

private:
	UINT GetMaxIDCollapsed();
	
protected:
	void AddTabSwitch(CTBToolBar* pBar);
	void CloneButtons(CTBToolBar* pBar);

private:
	void MoveToolbarToolsToLast();
	INT	FindButton(UINT nID);
	INT m_nIDOverlapButtonStart;

protected:
	INT							m_nWidth;
	

private:
	INT					FindToolBarPos(LPCTSTR lpszText);
	
public:
	int CalcMinimumWidth(BOOL bHidden = FALSE);
	virtual int CalcMaxButtonHeight();

	virtual BCGP_DOCK_TYPE GetDockMode() const
	{
		// Disable Drag & Drop TabbedToolBar
		return BCGP_DT_UNDEFINED;
	}

	void SetSuspendUpdateCmdUI(BOOL bSuspend = TRUE);

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

protected:
	virtual void OnFillBackground (CDC* pDC);
	virtual void DoPaint(CDC* pDC);
	
	afx_msg	LRESULT	OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnChangeActiveTab(WPARAM,LPARAM);
	afx_msg LRESULT OnSetActiveTab(WPARAM wParam, LPARAM lParam);
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnSize(UINT nType, int cx, int cy);

	DECLARE_MESSAGE_MAP()
};

class TB_EXPORT CTBToolBarCmdUI : public CCmdUI
{
public:
	virtual void Enable(BOOL bOn);
};

/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTBCollapsedItem
{
public:
	enum ECollapsedItemType {
		ITEM_BUTTON,
		ITEM_DROPDOWN,
		ITEM_LABEL,
		ITEM_SEPARATOR,
		ITEM_EDIT
	};

public:
	CTBCollapsedItem()
	{
		m_nID = -1;
		m_eType = ECollapsedItemType::ITEM_BUTTON;
		m_sTextItem = _T("");
		m_iWidth = 0;
	}

	CTBCollapsedItem(UINT id, ECollapsedItemType eType, const CString& sTextItem, INT iWidth) :
		CTBCollapsedItem()
	{
		m_nID = id;
		m_eType = eType;
		m_sTextItem = sTextItem;
		m_iWidth = iWidth;
	}

	UINT				GetID() { return m_nID; }
	ECollapsedItemType	GetType() { return m_eType; }
	CString				GetText() { return m_sTextItem; }

	void				SetWidth(INT Width) { m_iWidth = Width; }
	INT					GetWidth() { return m_iWidth ; }

private:
	UINT					m_nID;
	ECollapsedItemType		m_eType;
	CString					m_sTextItem;
	INT						m_iWidth;
};

/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTBToolBar : public CBCGPToolBar, public IOSLObjectManager
{
public:
	CTBToolBar();
	virtual ~CTBToolBar();
	virtual CSize CalcSize(BOOL bVertDock);

public:
	// Da chiamare subito dopo il costruttore per creare la toolbar inizialmente vuota
	BOOL CreateEmptyTabbedToolbar(CWnd* pParentWnd, const CString& sName = _T(""), const CString& sText = _T(""));
	BOOL CreateEmpty(CWnd* pParentWnd, CString sName, DWORD dwStyle = (WS_CHILD | WS_VISIBLE | CBRS_TOP | CBRS_GRIPPER | CBRS_SIZE_DYNAMIC), BOOL bCTBTabbedToolbar = FALSE);

	const CTBNamespace& GetNamespace() const;
	void			    SetNamespace(const CTBNamespace&);

	void ChangeImage(UINT nID, UINT nIDImag, BOOL bPnge = TRUE);
	void ChangeImage(UINT nID, UINT nIDImag, UINT nIDImageAlternative, BOOL bPng = TRUE, UINT nIDCollapsedImag = 0);
	void ChangeImage(UINT nID, const CString& sImageNameSpace, const CString& sImageAlternativeNameSpace = _T(""), const CString& sCollapsedImageNameSpace = _T(""));

	void SetButtonInfo(UINT nID, UINT nStyle, UINT nIDImage, LPCTSTR lpszText = NULL, BOOL bPng = TRUE);
	void SetButtonInfo(UINT nID, UINT nStyle, const CString& sImageNameSpace = _T(""), LPCTSTR lpszText = NULL, CString strTooltip = NULL);

	void SetButtonInfo(int nIndex, UINT nID, UINT nStyle, UINT nIDImage, LPCTSTR lpszText = NULL, BOOL bPng = TRUE);
	void SetButtonInfo(int nIndex, UINT nID, UINT nStyle, HICON hIcon);

	INT GetDefaultAction();
	BOOL SetDefaultAction(UINT nCommandID);

	BOOL AddButton(UINT nCommandID, const CString& sButtonNameSpace, UINT nIDB = 0, const CString& szText = _T(""), BOOL bPNG = TRUE, int nPos = -1);

	BOOL AddButton(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace = _T(""), const CString& szText = _T(""), int nPos = -1);
	BOOL AddButton(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, const CString& strToolTipText, int nPos = -1);

	// metodi lasciati per retrocompatibilità con i verticali TB
	BOOL AddButton(UINT nCommandID, const CString& aLibNamespace, const CString& sName, UINT nIDB = 0, const CString& szText = _T(""), BOOL bPNG = TRUE);
	BOOL AddButton(UINT nCommandID, const CString& aLibNamespace, const CString& sName, HICON hIcon = NULL, const CString& szText = _T(""));
	CImageAssociation* AddButton(UINT nCommandID, const CString& sButtonNameSpace, HICON hIcon = NULL, const CString& szText = _T(""), int nPos = -1, UINT nIDB = 0, BOOL bClone = FALSE);


	// Add Clone button
	BOOL AddButtonAllTab(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, const CString& strToolTipText, int nPos = -1);
	BOOL AddButtonAllTab(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace = _T(""), const CString& szText = _T(""), int nPos = -1);

	// Add Clone Button To Right
	BOOL AddButtonToRightAllTab(UINT nCommandID, const CString& sButtonNameSpace, HICON hIcon, const CString& szText, int nPos = -1);
	BOOL AddButtonToRightAllTab(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, const CString& strToolTipText, int nPos = -1);
	BOOL AddButtonToRightAllTab(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, int nPos = -1);

	// Button To Right
	BOOL AddButtonToRight(UINT nCommandID, const CString& sButtonNameSpace, UINT nIDB = 0, const CString& szText = _T(""), BOOL bPNG = TRUE, int nPos = -1);
	BOOL AddButtonToRight(UINT nCommandID, const CString& sButtonNameSpace, HICON hIcon = NULL, const CString& szText = _T(""), int nPos = -1);
	BOOL AddButtonToRight(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace = _T(""), const CString& szText = _T(""), int nPos = -1);
	BOOL AddButtonToRight(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace, const CString& szText, const CString& strToolTipText, int nPos = -1);
	BOOL AddButtonToRight(UINT nCommandID, const CString& aLibNamespace, const CString& sName, UINT nIDB = 0, const CString& szText = _T(""), BOOL bPNG = TRUE, int nPos = -1);
	BOOL AddButtonToRight(UINT nCommandID, const CString& aLibNamespace, const CString& sName, HICON hIcon = NULL, const CString& szText = _T(""), int nPos = -1);

	BOOL AddEditToRight(UINT nID, const CString& aLibNamespace, const CString& sName, int nWidth = 150, DWORD dwStyle = ES_AUTOHSCROLL, const CString& stLabel = _T(""), const CString& sPrompt = _T(""), int nPos = -1);
	BOOL AddEditToRight(UINT nID, const CString& aLibNamespace, const CString& sName, int nWidth = 150, DWORD dwStyle = ES_AUTOHSCROLL, const CString& stLabel = _T(""), int nPos = -1);

	BOOL AddLabelToRight(UINT nID, const CString& szText, int nPos = -1, UINT nAlign = TA_BOTTOM);

	// Insert Button
	BOOL InsertButtonAfter(UINT nCommandID, const CString& sButtonNameSpace, UINT nIDB = 0, const CString& szText = _T(""), BOOL bPNG = TRUE, UINT nIDInsertPos = 0);
	BOOL InsertButtonBefore(UINT nCommandID, const CString& sButtonNameSpace, UINT nIDB = 0, const CString& szText = _T(""), BOOL bPNG = TRUE, UINT nIDInsertPos = 0);
	BOOL InsertButtonAfter(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace = _T(""), const CString& szText = _T(""), int nIDInsertPos = 0);
	BOOL InsertButtonBefore(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace = _T(""), const CString& szText = _T(""), int nIDInsertPos = 0);

	BOOL AddSeparatorBefore(UINT nIDInsertPos);
	BOOL AddSeparatorAfter(UINT nIDInsertPos);

	// Move button
	BOOL MoveButtonBefore(UINT nCommandID, UINT nIDInsertPos = 0);
	BOOL MoveButtonAfter(UINT nCommandID, UINT nIDInsertPos = 0);

	BOOL AddComboBox(UINT nID, const CString& aLibNamespace, const CString& sName,	int nWidth,	DWORD dwStyle = (WS_CHILD | WS_VISIBLE | CBS_NOINTEGRALHEIGHT | CBS_DROPDOWNLIST | WS_VSCROLL | CBS_SORT), const CString& stLabel = _T(""), INT_PTR iInsertAt = -1);
	BOOL AddComboBox(UINT nID, const CString& sButtonNameSpace,						int nWidth,	DWORD dwStyle = (WS_CHILD | WS_VISIBLE | CBS_NOINTEGRALHEIGHT | CBS_DROPDOWNLIST | WS_VSCROLL | CBS_SORT), const CString& stLabel = _T(""), INT_PTR iInsertAt = -1, const CString& sPrompt = _T(""));
	BOOL AddComboBoxEdit(UINT nID, const CString& sButtonNameSpace, int nWidth, const CString& stLabel = _T(""), INT_PTR iInsertAt = -1, const CString& sPrompt = _T(""), BOOL bSORT = TRUE);
	BOOL AddComboBoxEditToRight(UINT nID, const CString& sButtonNameSpace, int nWidth, const CString& stLabel = _T(""), INT_PTR iInsertAt = -1, const CString& sPrompt = _T(""), BOOL bSORT = TRUE);

	BOOL AddEdit(UINT nID, const CString& aLibNamespace, const CString& sName, int nWidth = 150, DWORD dwStyle = ES_AUTOHSCROLL, const CString& stLabel = _T(""), const CString& sPrompt = _T(""), int nPos = -1);

	BOOL AddLabel(UINT nID, const CString& szText, int nPos = -1, UINT nAlign = TA_BOTTOM);

	// Edit 
	CString GetTextContent(UINT nID);
	BOOL SetTextContent(UINT nID, const CString& text);
	void SetTextFocus(UINT nID);
	BOOL SetEditSize(UINT nID, int iWidth, int iHeight);
	BOOL SetFont(UINT nID, CFont* pFont, BOOL bRedraw = TRUE);
	BOOL SetEditIcon(UINT nID, UINT nIDBImage);
	BOOL SetEditIcon(UINT nID, const CString& sImageNameSpace);

	// Combo
	INT  AddComboItem(UINT nID, const CString& item, DWORD_PTR dwData = 0);
	INT  AddComboSortedItem(UINT nID, const CString& item, DWORD_PTR dwData = 0);
	BOOL RemoveComboItem(UINT nID, INT nItem);
	BOOL RemoveComboItem(UINT nID, DWORD_PTR dwData);
	BOOL RemoveAllComboItems(UINT nID);
	LPCTSTR GetComboItemSelText(UINT nID);
	BOOL SetComboItemSel(UINT nID, UINT nCurSel);
	INT  GetComboItemSel(UINT nID);
	INT  FindComboStringExact(UINT nID, const CString& sItem);
	INT  GetComboCount(UINT nID);
	DWORD_PTR GetComboItemData(UINT nID, UINT nItem);

	// Aggiunge un separatore
	void AddSeparator(int nIdx = -1);

	// Indica che il bottone il cui command corrisponde all'ID passato ha associato una dropdown. 
	// Se bWhole = TRUE l'itnero bottone sarà una dropdown (stile BTN_WHOLEDROPDOWN)
	// NOTA: la composizione del menu e le azioni associate vanno fatte intercettando la notifica TBN_DROPDOWN 
	// sull'ID AFX_IDW_TOOLBAR (ID standard assegnato da MFC alle toolbar)

	BOOL SetDropdown(UINT nCommandID, CMenu* mMenu = NULL, LPCTSTR lpszText = NULL);
	BOOL AddDropdown(UINT nCommandID, const CString& sButtonNameSpace, const CString& sImageNameSpace = _T(""), const CString& szText = _T(""), CMenu* mMenu = NULL, int nPos = -1);

	BOOL UpdateDropdownMenu(UINT nCommandID, CMenu* mMenu);
	BOOL AddDropdownMenuItem(UINT nCommandID, UINT nFlags, UINT_PTR nIDNewItem = NULL, LPCTSTR lpszNewItem = NULL, UINT nIDImgUnchecked = 0, UINT nIDImgChecked = 0, BOOL bPng = TRUE);
	BOOL AddDropdownMenuItemSeparator(UINT nCommandID);
	HMENU CopyDropdownMenu(UINT nCommandID);

	BOOL InsertDropdownMenuItem(UINT nPos, UINT nCommandID, UINT nFlags, UINT_PTR nIDNewItem /*= NULL*/, LPCTSTR lpszNewItem /*= NULL*/, UINT nIDImgUnchecked /*= 0*/, UINT nIDImgChecked/* = 0*/, BOOL bPng /*= TRUE*/);
	BOOL RemoveDropdown(UINT nCommandID);
	BOOL RemoveButtonForID(UINT nCommandID);
	BOOL EnableAlwaysDropDown(UINT nCommandID, BOOL bAlwaysDropDown = TRUE);
	void SetAlwaysDropDown(UINT nCommandID, int nAlwaysDropDown);

	void SetGroup(UINT nCommandID);
	void SetText(UINT nCommandID, LPCTSTR lpszText, LPCTSTR lpszTextAlternative = NULL);

	BOOL SetTextToolTip(UINT nCommandID, CString m_strText);
	CString GetTextToolTip(UINT nCommandID);

	void GetObjectGrant(CInfoOSL* pParentOSL);
	void UpdateWindow();
	void EnableDocking(CWnd* pParentWnd, DWORD dwAlignment = CBRS_ALIGN_TOP | CBRS_ALIGN_LEFT);
	void OnTextBelowButton();
	void OnTextBelowButton(UINT nCommandID, const LPCTSTR pStrText = NULL);

	BOOL HideButton(UINT nID, BOOL bHide = TRUE, BOOL bDisableAutoHide = FALSE);
	BOOL IsHideButton(UINT nID);
	BOOL GhostButton(UINT nID, BOOL bGhost = TRUE);
	BOOL IsGhostButton(UINT nID);
	BOOL IsCloneButton(UINT nID);

	void AttachParentOSL(CInfoOSL* pParentOSL) { m_pParentOSL = pParentOSL; }
	void AttachOSLInfo(CInfoOSL* pParent);
	CString	  GetName() { return m_sName; }
	CString	  GetTitle() { return m_sTitle; }
	HICON GetIconDropdownMenu(const CBCGPToolbarMenuButton* pMenuButton);
	void SetDefaultButton(int iIndex);
	INT  CalcMinToolBar();

	void SetAutoHideToolBarButton(BOOL bAutoHide) { m_bAutoHideToolBarButton = bAutoHide; }
	BOOL GetAutoHideToolBarButton() { return m_bAutoHideToolBarButton; }

	void	SetShowToolBarTab(BOOL bAutoHide = TRUE) { m_bShowToolBarTab = bAutoHide; }
	BOOL	GetShowToolBarTab() { return m_bShowToolBarTab; }

private:
	void AddRightSpaceObj();
	void AddLeftSpaceObj();
	BOOL AutoHideMenuButton(int nIndex);
	BOOL ISMenuButtonVoicesEnabled(CTBToolbarMenuButton* pMenuButton);

	CBCGPToolbarButton* FindCollapsableButton(INT nPosFrom);

	HICON GetOverlapIconDropdownMenu(const CBCGPToolbarMenuButton* pMenuButton);
	void ButtonsOverlapRemoveByID(UINT nID);
	BOOL ButtonsOverlapAdd(CBCGPToolbarButton* pButton);
	void ButtonsOverlapMenuSub(UINT nID, CMenu* pMenu, CArray<CTBToolBarMenu*, CTBToolBarMenu*>* ptMenuArray);
	void ButtonsOverlapMenuUpdate();
	BOOL ButtonsInOverlapList(UINT nID);
	BOOL ButtonsOverlapRemove(int iWidth);
	BOOL ButtonsOverlapRemoveItem(UINT nID);
	BOOL AddButtonOverlap(CTBToolBarMenu* pMenu);
	INT	GetButtonWidth(CBCGPToolbarButton* pButton);

	BOOL m_bDefaultActionEnable;
	INT n_IdDefaultAction;

	HICON LoadImageByNameSpace(CString strImageNS, CDC* pDC = NULL, UINT nWidth = 0);
	HICON LoadImageByNameSpace(CString strImageNS, UINT nID);

	BOOL MoveButton(UINT nCommandID, UINT nPos);

	void CloneButtonsAllTab();

public:
	void SetLastDropDown(UINT nIdCommand) {	m_nIDLastDropDown = nIdCommand;}

private:
	BOOL						m_bPostToolBarUpdate;
	UINT						m_nIDLastDropDown;
	BOOL						m_bAutoHideToolBarButton;
	BOOL						m_bShowToolBarTab;
	BOOL						m_bButtonStyleLoopComplite;
	BOOL						m_bRecalcGray;
	BOOL						m_bSuspendLayout;
	BOOL						m_bDialog;
	UINT						m_nDialogUpdateToolBar;
	BOOL						m_bTBelow;
	BOOL						m_bAdjustLayoutHideButton;
	INT							m_iAdjustLayoutHideButton;
	BOOL						m_bButtonsOverlap;
	BOOL						m_bToolbarInfinity;

	CInfoOSL*					m_pParentOSL;
	AcceleratorArray			m_arAccelerators;
	CTBNamespace				m_Namespace;
	CArray<CImageAssociation*>	m_Images;
	CString						m_sName;
	CString						m_sTitle;

	CTBToolbarButton*			m_pDefaultActionButton;

	CMap<UINT, UINT, HICON, HICON>	m_mapDropDownIconsUnChecked;
	CMap<UINT, UINT, HICON, HICON>	m_mapDropDownIconsChecked;
	CMap<UINT, UINT, HICON, HICON>	m_mapCollapsedImage;
	BOOL						    m_bCenterButtons;

	CTBToolbarMenuButton*		m_pMenuButtonCollapsed;

	// Collapsed items collection
	CList<CTBCollapsedItem*, CTBCollapsedItem*>		m_ListCollapsedItems;	// Key is button ID
	CString											m_stIconCollapsed;		// Collapsed ICON

	CMap<UINT, UINT, BOOL, BOOL>	m_mapButtonNotAutoHidden; // Map IDC not automatic hidden button
	CTBTabbedToolbar*			m_pParentTabbedToolbar;
	COLORREF					m_clrBkgColor;
	COLORREF					m_clrForeColor;
	COLORREF					m_cTextColor;
	COLORREF					m_cTextColorHighlighted;
	COLORREF					m_cHighlightedColorClick;
	BOOL						m_bHighlightedColorClickEnable;
	COLORREF					m_cHighlightedColor;
	CBaseDocument::FormMode		m_PrevFormMode;
	CList<int, int>				m_removedListID;

private:
	void	AddCollapsedImage(UINT nID, HICON hIco);
	HICON	GetCollapsedImage(UINT nID);
	void	SendMessageToolBarUpdate();
public:
	INT							m_nIDOverlapButton;

public:
	CArray<CInfoOSLButton*, CInfoOSLButton*> m_arInfoOSL;
	CInfoOSLButton* GetInfoOSLButton(UINT nID);

	BOOL GetButtonNamespace(UINT nBtnID, CTBNamespace& ns);
	void SetParentTabbedToolbar(CTBTabbedToolbar* pParent);
	void AddDocuemntTitle(CString strTitle);
	BOOL SuspendLayout();
	void ResumeLayout(BOOL bForced = FALSE);
	BOOL IsLayoutSuspended() { return AfxIsRemoteInterface() ? TRUE : m_bSuspendLayout; }
	
	BOOL ShowToolBarDown(CWnd* pParentWnd, BOOL bShowText = FALSE);
	BOOL ShowInDialog(CWnd* pParentWnd = NULL, DWORD dwAlignment = CBRS_ALIGN_TOP | CBRS_ALIGN_LEFT);
	
	void EnableButton(UINT nCommandID, BOOL enable = TRUE);
	void PressButton(UINT nCommandID, BOOL setCheckBoxStyle, BOOL check = TRUE);
		void CheckButton(UINT nCommandID, BOOL check);

	virtual BOOL Create	 (CWnd* pParentWnd,
			UINT nToolbarIDB, 
			CStringArray* pNamespaceArray = NULL,
			DWORD dwStyle = (WS_CHILD | WS_VISIBLE | CBRS_TOP | CBRS_GRIPPER | CBRS_SIZE_DYNAMIC),
			DWORD dwCtrlStyle = (TBSTYLE_FLAT | TBSTYLE_TRANSPARENT | TBSTYLE_TOOLTIPS));

	virtual void OnChangeVisualManager	();
	virtual BOOL OnUserToolTip (CBCGPToolbarButton* pButton, CString& strTTText) const;
	virtual void GetMessageString(UINT nID, CString& strMessageString) const;

	BOOL ExButton(UINT nCommandID);
	INT FindButton(UINT nCommandID);
	INT FindButton(CString sNameSpace, BOOL bIsComplete = TRUE);
	CInfoOSLButton* FindOslInfoButton(CString sNameSpace);
	CTBToolbarButton* FindButtonPtr(UINT nCommandID);
	void SetSecondaryToolBarColor(UINT nCommandID, BOOL bActive = TRUE);
	BOOL OnUpdateCmdUIDialog();

protected:
	BOOL m_bSuspendUpdateCmdUI = FALSE;
public:
	void SetSuspendUpdateCmdUI(BOOL bSuspend = TRUE) { m_bSuspendUpdateCmdUI = bSuspend; }
	BOOL IsSuspendedUpdateCmdUI() { return m_bSuspendUpdateCmdUI; }
	virtual void OnUpdateCmdUI(CFrameWnd* pTarget, BOOL bDisableIfNoHndler);

	virtual BOOL OnSetDefaultButtonText (CBCGPToolbarButton* pButton);
	CWndObjDescription* GetControlStructure(CWndObjDescriptionContainer* pContainer);
	void WriteTabName(CWndObjDescription* pDescription);
	void CenterButtons(BOOL bCenter = TRUE);
	virtual void SetButtonStyle(int nIndex, UINT nStyle);
	virtual void SetButtonStyleByIdc(UINT nCommandID, UINT nStyle);
	virtual CSize CalcFixedLayout(BOOL bStretch, BOOL bHorz);
	
	void SetBkgColor(COLORREF color);
	void SetForeColor(COLORREF color);
	void SetTextColor(COLORREF color);
	void SetTextColorHighlighted(COLORREF color);
	void SetHighlightedColor(COLORREF color);
	void SetHighlightedColorClick(COLORREF color);

	COLORREF GetForeColor();
	COLORREF GetTextColor();
	COLORREF GetTextColorHighlighted();
	COLORREF GetHighlightedColor();
	COLORREF GetBkgColor();
	COLORREF GetHighlightedColorClick();
	void EnableHighlightedColorClick(BOOL bEnable = TRUE);
	BOOL IsEnableHighlightedColorClick();

	CList<int, int>* GetRemovedListID() { return &m_removedListID; }

	void SetCollapsedImage(CString stImageNameSpace);

	void	AppendAccelerator	(HACCEL& hAccelTable, UINT nID, BYTE fVirt, WORD key);
	void	RemoveAccelerator	(HACCEL& hAccelTable, UINT nID);

public:
	int CalcMinimumWidth(BOOL bHidden = FALSE);
	int GetRightSpacePos();

private:
	void IconsDropDownClean();
	void DisableButtonAutoHidden(UINT nID, BOOL bDisable = TRUE);

	void SetTBImages();
	void AddOslInfo	(UINT nCommandID, const CString& sParentNamespace, const CString& sName);
	void AddOslInfo	(UINT nCommandID, const CString& sName);
	int GetImageFromImagesListIDImage(UINT nIDImage);
	int GetImageFromImagesListIDImage(CString nImageNS);
	int GetImageFromImagesListIDC(UINT nIDC);
	BOOL AutoHideButton(CBCGPToolbarButton* pButton);
	CImageAssociation* GetImageAssociation(UINT nIDC);
	
	int		SetButtonInfoChangeImage(UINT nIDC, const CString& sImageNameSpaceOLD);
	int		SetButtonInfoChangeImage(UINT nIDC, UINT nIDImageOLD);
	CString SetButtonInfoChangeText(UINT nIDC,  UINT nIDImageOLD);

	BOOL RemoveImageFromImagesList(UINT nIDImage);
	HINSTANCE ImageContainerInit(const	CString& strLibraryNamespace, UINT nIdImg);
	CTBToolbarLabel* m_pObjectRight;
	CTBToolbarLabel* m_pObjectLeft;
	
	HICON	GetIconUnCheckedDropdown(UINT nID);
	HICON	GetIconCheckedDropdown(UINT nID);

	BOOL	IsDummyButton(CBCGPToolbarButton* pBtn);

	BOOL m_bToRight;
	int  m_nLastRight;
	int  m_iWidthObjectLeft;
	int  m_iWidthObjectRight;

	void	AddToolBarAccelText	(CString& strText, CBCGPToolbarButton* pButton) const;

private:
	CRect GetDockBarRect();

protected:
	int m_iToolbarButton_Width;

public:
	int InsertButtonPtr(CBCGPToolbarButton* pButton, INT_PTR iInsertAt = -1);

	virtual int CalcMaxButtonHeight();
	
	virtual void AdjustSizeImmediate(BOOL bRecalcLayout = TRUE);

	virtual void AdjustLayout();
	BOOL RepositionRightButtons();

private:
	void RepositionRightButtonsCalcButtonsSpace(CRect rectDockBar, INT& iWidthLeft, INT& iWidthRight);
	BOOL IsToolbarInfinity();
	BOOL IsToolbarMenuButton(int iButton);

protected:
	int InsertButtonInternal(const CBCGPToolbarButton& button, INT_PTR iInsertAt = -1);
	virtual CSize CalcLayout (DWORD dwMode, int nLength = -1);
	virtual void DoPaint(CDC* pDC);
	virtual void OnFillBackground (CDC* pDC);
	virtual void DrawSeparator (CDC* pDC, const CRect& rect, BOOL bHorz);
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual BOOL DrawButton(CDC* pDC, CBCGPToolbarButton* pButton, CBCGPToolBarImages* pImages, BOOL bHighlighted,	BOOL bDrawDisabledImages);
	virtual void ShowCommandMessageString(UINT uiCmdId);

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

protected:
	//{{AFX_MSG(CTaskBuilderToolBar)
	afx_msg	LRESULT	OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT	OnSuspendLayoutChanged(WPARAM wParam, LPARAM lParam);
	
	afx_msg void OnDestroy();
	afx_msg LRESULT OnFillAutoComple(WPARAM wp, LPARAM lp);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg LRESULT OnToolBarUpdate(WPARAM, LPARAM);
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
