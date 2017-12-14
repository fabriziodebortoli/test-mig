#pragma once

//includere alla fine degli include del .H


#include <TbGes/tabber.h>
#include <TbGes/ExtdocAbstract.h>
#include <TbGes/ExtDocView.h>
#include <TbGenlib\BaseDoc.h>
#include <TbGenlib\BaseTileManager.h>
#include <TbGenlib\TBCommandInterface.h>

#include "beginh.dex"

//================================================================================
class TB_EXPORT CDesignModeLayoutManipulator : public CDesignModeManipulatorObj, public CTileDesignModeParamsObj
{
private:
	CBaseDocument::DesignMode		m_DesignMode;
	COLORREF						m_GroupColor;
	CBrush*							m_pGroupBrush;

public:
	CDesignModeLayoutManipulator(CBaseDocument::DesignMode aDesignMode);
	~CDesignModeLayoutManipulator();

public:
	CBaseDocument::DesignMode GetDesignMode();

	void OnAfterBuildDataControlLinks(CAbstractFormView* pView);
	void OnAfterBuildDataControlLinks(CTabDialog* pTabDialog);

	// CTileDesignModeParamsObj interface
	virtual BOOL	AreZeroSizesAllowed();
	virtual int		GetTileMaxHeightUnit() const;
	virtual int		GetTileAnchorSize() const;
	virtual BOOL	IsInternalControlMovementEnabled();
	virtual BOOL	FreeResize();
	virtual BOOL	HasToolbar();
	virtual BOOL	HasStatusBar() const;

	virtual CBrush*		GetTileGroupBkgColorBrush();

private:
	BOOL IsSameColor(COLORREF clr1, COLORREF clr2);
	CBrush*	GetBrushOf(COLORREF color);
	void ChangeLayout(const LayoutElementArray* pElements);
};

/// <summary>
/// allows to incapsulate managed/unmanaged parameters into restartDocument
/// </summary>
//================================================================================
class TB_EXPORT CRestartDocumentInvocationInfo : public CManagedDocComponentObj
{
private:
	CString m_strCustomizationName;
	CDesignModeManipulatorObj*	m_pManipulator;

public:
	virtual void CreateNewDocumentOf(CBaseDocument* pDoc);

	CRestartDocumentInvocationInfo(CBaseDocument::DesignMode aDesignMode);
	~CRestartDocumentInvocationInfo();
	virtual CDesignModeManipulatorObj*	GetDesignModeManipulatorObj() { return m_pManipulator; }
	
	CString GetCustomizationName() { return m_strCustomizationName; }
	void SetCustomizationName(CString customizationName) { m_strCustomizationName = customizationName; }
};


#include "endh.dex"