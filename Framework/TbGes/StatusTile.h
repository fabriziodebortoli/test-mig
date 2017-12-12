
#pragma once

//NOW INCLUDED IN COMMON PCH: #include <TbGenlib\PARSCBX.H>
//NOW INCLUDED IN COMMON PCH: #include <TbGenlib\PARSEDT.H>


#include "TileDialog.h"

#include "beginh.dex"

class CTBLinearGaugeCtrl;
class CTBCircularGaugeCtrl;
class CTilePanel;
class CJsonTileDialog;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////
////					class CStatusStaticLabel definition
////////////////////////////////////////////////////////////////////////////////////////////////////////////
////==========================================================================================================
//class TB_EXPORT CStatusStaticLabel : public CLabelStatic
//{
//	DECLARE_DYNCREATE(CStatusStaticLabel)
//
//protected:
//	//afx_msg BOOL	OnEraseBkgnd(CDC* pDC);
//	//afx_msg 
//
//	DECLARE_MESSAGE_MAP();
//};

///////////////////////////////////////////////////////////////////////////////////////////////////////////
//					class CBaseStatusTile definition
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//==========================================================================================================
class TB_EXPORT CBaseStatusTile : public CJsonTileDialog
{
	DECLARE_DYNAMIC(CBaseStatusTile)

public:
	CBaseStatusTile();

private:	//private members
	COLORREF				m_DefaultLeftBorderTileColor;
	COLORREF				m_LeftBorderTileColor;
	DataStr*				m_pDescription;
	CFont*					m_pFont;
	BOOL					m_bHasLeftBorder;

protected:
	bool					m_bClickable = false;
	COLORREF				m_GaugeBorderFrameColor;
	COLORREF				m_DescriptionTextColor;
	TBThemeManager*			m_pThemeManager;
	int						m_YMinLeftBorder;
	int						m_YMaxLeftBorder;

public:
	DataQty*				m_pDblValue;
	
public:	//public methods
			void			SetHasLeftBorder			()							{ m_bHasLeftBorder = TRUE; }
			//todo silvano: rimuovere parametri opzionali quando finito il porting di ERP
			void			SetDescription				(DataStr* pDescription, BOOL bStyleBold = FALSE, BOOL bStyleItalic = FALSE)
			{ 
				m_pDescription			= pDescription; 
			
			}

			void			SetDescription				(DataStr* pDescription, BOOL bStyleBold, BOOL bStyleItalic, COLORREF cColor)
			{ 
				SetDescription(pDescription, bStyleBold, bStyleItalic);
				m_DescriptionTextColor	= cColor;
			}

			void			SetLeftBorderColor(COLORREF* pClrBorderColor) { ASSERT(FALSE); /*Silvano, eliminare metodo*/ }

protected:
			DataStr*		GetDescription				() { return m_pDescription; }
			CFont*			GetFont						() { return m_pFont; }

			void			RedrawStatusTileObj			();

public:
			void			SetFont						(CFont* pFont) { m_pFont = pFont; }

protected:	//private virtual methods
	virtual BOOL			OnInitDialog				();
	virtual	void			OnUpdateControls(BOOL bParentIsVisible = TRUE);
protected:	//protected virtual methods
	virtual void			OnMouseClick				() { }
	virtual void			CustomizeGauge				() { }
	virtual void			RedrawDescription			() { }

public:
	virtual void			SetTileClickable(bool bTileClickable) {
		m_bClickable = bTileClickable;
	}

protected:
	//{{AFX_MSG(CBaseStatusTile)
	afx_msg void			OnPaint						();
	afx_msg	void			OnLButtonDown				(UINT nFlags, CPoint point);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

///////////////////////////////////////////////////////////////////////////////////////////////////////////
//					class CLinearStatusTile definition
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//==========================================================================================================
class TB_EXPORT CLinearStatusTile : public CBaseStatusTile
{
	DECLARE_DYNAMIC(CLinearStatusTile)

public:
	CLinearStatusTile(BOOL bClickable = FALSE);

	CLinearStatusTile(CString ns, BOOL bClickable = FALSE);

private:	//private members
	DataStr*			m_pTileImage;
	DataStr*			m_pAuxLeftDescription;
	DataStr*			m_pAuxRightDescription;
		
	//manage styles
	BOOL				m_bAuxLeftBold;
	BOOL				m_bAuxLeftItalic;
	BOOL				m_bAuxRightBold;
	BOOL				m_bAuxRightItalic;
	DataQty				m_nDblValue;

protected:	//protected members
	CTBLinearGaugeCtrl*	m_pLinearGauge;
	CLabelStatic*		m_pDescriptionLabel;

public:		//public methods
	//void SetTileClickable				(BOOL bSet) { m_bClickable = bSet; }
	void SetImageNS						(DataStr* pNSImg) { m_pTileImage = pNSImg; }
	void SetAuxLeftDescription			(DataStr* pAuxLeftDescription, BOOL bStyleBold, BOOL bStyleItalic) 
	{ 
		
		m_pAuxLeftDescription	= pAuxLeftDescription; 
		m_bAuxLeftBold			= bStyleBold;
		m_bAuxLeftItalic		= bStyleItalic;

	}

	void SetAuxRightDescription			(DataStr* pAuxRightDescription, BOOL bStyleBold, BOOL bStyleItalic) 
	{
		
		m_pAuxRightDescription	= pAuxRightDescription; 
		m_bAuxRightBold			= bStyleBold;
		m_bAuxRightItalic		= bStyleItalic;
	}

private:	//private methods
	void SetAuxDescriptionsPos			(CWnd* pWndLeft, CWnd* pWndRight, int xImage, int yGauge);
	void SetGaugePos					(CWnd* pWnd, int& yGauge);
	void SetDescriptionPos				(CWnd* pWnd, int x, int offsetX, int offsetY);
	void SetImagePos					(CWnd* pWnd, int& xImage, int& yImage, int& wImage, int& hImage);
	void SetDescriptionStyle			();
	

private:	//private virtual methods
	virtual void BuildDataControlLinks	();
	virtual void RedrawDescription		();

protected:

	virtual BOOL	OnInitDialog();
	virtual void	CustomizeGauge();

	//{{AFX_MSG(CLinearStatusTile)
	afx_msg void			OnSize		(UINT nType, int cx, int cy);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

//=============================================================================
//			Class CWndStatusTileDescription
//=============================================================================
class TB_EXPORT CWndStatusTileDescription : public CWndTileDescription
{
	DECLARE_DYNCREATE(CWndStatusTileDescription);
public:
	bool		m_bIsClickable = false;
	bool		m_bHasLeftBorder = false;
	
	int			m_BorderLeftColor = 0;

private:
	CWndStatusTileDescription()
	{
		m_Type = CWndObjDescription::StatusTile;
	}

public:
	CWndStatusTileDescription(CWndObjDescription* pParent)
		: CWndTileDescription(pParent)
	{
		m_Type = CWndObjDescription::StatusTile;
	}

	virtual void Assign(CWndObjDescription* pDesc);
	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};


#include "endh.dex"
