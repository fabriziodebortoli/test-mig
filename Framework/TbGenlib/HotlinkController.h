#pragma once

#include "beginh.dex"

class CParsedCtrl;
class DataObj;


//=============================================================================
class TB_EXPORT CHotLinkControllerDecode : public CObject
{
	friend class CHotLinkController;
public:
	enum DecodePosition { Right, Left, Bottom, Top, Fixed };

private:
	DecodePosition	m_Pos;
	DataObj*		m_pDataObj;
	CParsedCtrl*	m_pControl;

public:
	CHotLinkControllerDecode(DataObj* pDataObj, CHotLinkControllerDecode::DecodePosition pos);

	CParsedCtrl*	GetControl		();

	virtual CRuntimeClass* GetControlClass();

private:
	BOOL CreateControl(CParsedForm* pParent, CBaseDocument* pDocument, const CString& sName, CRect aRect, DWORD dwStyle = 0);
	void RepositionControl	(CRect& rectOwner, CRect& rectOriginalPlaceHolder, CRect rectButton, UINT nFlags);
};

//=============================================================================
class TB_EXPORT CHotLinkController : public CObject
{
	DECLARE_DYNAMIC (CHotLinkController)

private:
	CParsedCtrl*	m_pOwner;
	Array			m_Decodes;
	CRect			m_OriginalPlaceHolder;
	bool			m_bPlaceHolderShrinked;

public:
	CHotLinkController(CParsedCtrl* pOwner);

public:
	CHotLinkControllerDecode* Add
		(
			DataObj* pDataObj, 
			const CString& sName, 
			CHotLinkControllerDecode::DecodePosition pos = CHotLinkControllerDecode::Right, 
			BOOL bUsePlaceHolder =  TRUE,
			int nChars = -1
		);
	void AddDefaultDecode	(const CString& sDecodeNs);
	void ShowButton			(const BOOL bShow = TRUE);
	void OnAfterFindRecord	();
	void OnPrepareAuxData	();
	void DoCellPosChanging	(CRect& rectEdit, UINT nFlags);

private:
	HotKeyLinkObj*	GetHotLink			();
	void			RepositionControls	(CRect& rectEdit, UINT nFlags);
	void			ResizePlaceHolder	(bool bShrink);
	CRect			GetButtonRect		();
};

