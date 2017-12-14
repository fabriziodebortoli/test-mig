#pragma once

#include <TbGenlib\OslInfo.h>
#include "beginh.dex"

class CTBNamespace;
class CInfoOSL;
//======================================================================
class TB_EXPORT CTaskBuilderCaptionBar : public CBCGPCaptionBar
{
private:
	CArray<CInfoOSL*> 	m_OSLInfos;

public:
	enum AlignStyle
	{
		ALIGN_LEFT,
		ALIGN_RIGHT,
		ALIGN_CENTER
	};

	CTaskBuilderCaptionBar();
	~CTaskBuilderCaptionBar();
	
public:
	BOOL Create(UINT nID, CWnd* pParentWnd);

	COLORREF	GetBkgColor		() const;
	COLORREF	GetTextColor	() const;
	COLORREF	GetBorderColor	() const;

	void		SetBkgColor		(COLORREF crBkg);
	void		SetTextColor	(COLORREF crText);
	void		SetBorderColor	(COLORREF crText);
	void		SetImage		(UINT nID);
	void		SetButton		(const CString sTitle, UINT nID, BOOL bDropDown = 0, AlignStyle align = ALIGN_RIGHT);
	void		SetText			(const CString& strText, AlignStyle align = ALIGN_RIGHT);

	virtual BOOL OnClickCloseButton();

	CArray<CInfoOSL*>& GetOSLInfos();
	CInfoOSL* GetOSLInfoByName(const CString& sName);

	BOOL CanExecuteLink		();
	BOOL CanClickButton		();
	BOOL CanClickImage		();
	BOOL CanExecuteCommand	(UINT nID, UINT nCode);
		
protected:
	afx_msg	LRESULT	OnGetControlDescription(WPARAM wParam, LPARAM lParam);

	DECLARE_MESSAGE_MAP()

private:
	void AttachOSLInfos (CInfoOSL* pParentInfo);
};

#include "endh.dex"