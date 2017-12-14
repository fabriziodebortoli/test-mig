#pragma once

#include <afxtempl.h>

#include <TBGenlib\TBToolbar.h>

#include "beginh.dex"

// Contiene le informazioni relative ad un insieme di bottoni (al limite anche uno solo)
// da aggiungere alla toolbar del documento
//==============================================================================
class TB_EXPORT CButtonInfo : public CObject
{
	DECLARE_DYNAMIC(CButtonInfo);

public:
	CButtonInfo
		(
			UINT nCommandID,
			const CString& sName,
			const CString& sText,
			const CString& sImageNameSpace,
			const CString& sToolBarName,
			const CString& sToolTip,
			BOOL	bDropdown
		);
	~CButtonInfo();

public:
	UINT m_nCommandID;
	CString m_sName;
	CString m_sText;
	CString m_sToolbarName;
	CString m_sImageNameSpace;
	CString m_sToolTip;
	BOOL	m_bDropdown;
};

// Contiene le informazioni relative hai comboBox
// da aggiungere alla toolbar del documento
//==============================================================================
class TB_EXPORT CComboInfo : public CObject
{
	DECLARE_DYNAMIC(CComboInfo);

public:
	CComboInfo
		(
			UINT nID,
			const CString& aLibNamespace,
			const CString& sName,
			int nWidth,
			DWORD dwStyle,
			const CString& sToolBarName
		);
	~CComboInfo();

public:
	UINT m_nID;
	CString m_libNamespace;
	CString m_sName;
	int m_nWidth;
	DWORD m_dwStyle;
	CString m_sToolBarName;
};

// Contiene le informazioni relative hai editBox
// da aggiungere alla toolbar del documento
//==============================================================================
class TB_EXPORT CEditInfo : public CObject
{
	DECLARE_DYNAMIC(CEditInfo);

public:
	CEditInfo
		(
			UINT nID,
			const CString& aLibNamespace,
			const CString& sName,
			int nWidth,
			DWORD dwStyle,
			const CString& sToolBarName
		);
	~CEditInfo();

public:
	UINT m_nID;
	CString m_libNamespace;
	CString m_sName;
	int m_nWidth;
	DWORD m_dwStyle;
	CString m_sToolBarName;
};

// Contiene le informazioni relative alle label
// da aggiungere alla toolbar del documento
//==============================================================================
class TB_EXPORT CLabelInfo : public CObject
{
	DECLARE_DYNAMIC(CLabelInfo);

public:
	CLabelInfo
		(
			UINT nID,
			CString sText,
			CString sToolBarName
		);
	~CLabelInfo();

public:
	UINT m_nID;
	CString m_sText;
	CString m_sToolBarName;
};

class TB_EXPORT CDropdownMenuItemInfo : public CObject
{
	DECLARE_DYNAMIC(CDropdownMenuItemInfo);

public:
	CDropdownMenuItemInfo(UINT nCommandID, UINT_PTR nIDNewItem, CString sNewItem, CString sToolBarName);
	~CDropdownMenuItemInfo();

public:
	UINT m_nCommandID;
	UINT_PTR m_nIDNewItem;
	CString m_sNewItem;
	CString m_sToolBarName;
};
// Contiene le informazioni relative separetor
//==============================================================================
class TB_EXPORT CSeparatorInfo : public CObject
{
	DECLARE_DYNAMIC(CSeparatorInfo);

public:
	CSeparatorInfo	(CString sToolBarName);
	~CSeparatorInfo();
	CString m_sToolBarName;
};

// array og element to insert in Toolbar
typedef	CTypedPtrArray	<Array,	CObject*>	CObjectInfoArray;

// Questa classe fa da "contenitore" dei vari bottoni da aggiungere al documento, e gestisce
// gli acceleratori associati
// I bottoni possono essere inseriti qui anche "a più riprese", in gruppo o uno alla volta
// Alla fine saranno aggiunti tutti al documento quando verrà chiamata la CreateNewButtons
// Il metodo PreTranslateMsg verrà chiamato per gestire eventuali acceleratori

////////////////////////////////////////////////////////////////////////////////
//				class CToolBarButtons definition
////////////////////////////////////////////////////////////////////////////////
//
//==============================================================================
class TB_EXPORT CToolBarButtons
{
private:
	CObjectInfoArray		m_ButtonsInfo;		// insieme di bottoni da aggiungere

public:
	CToolBarButtons();
	~CToolBarButtons();

public:
	void AddButton
		(
			UINT nCommandID,
			const CString& sName,
			const CString& sText,
			const CString& sImageNameSpace,
			const CString& sToolBarName,
			const CString& sToolTip,
			BOOL	bDropdown
		);
	
	void AddComboBox
	(
		UINT nID, 
		const CString& aLibNamespace, 
		const CString& sName, 
		int nWidth, 
		DWORD dwStyle, 
		const CString& sToolBarName
	);

	void AddEdit
	(
		UINT	nCommandID,
		const CString& aLibNamespace, 
		const CString& sName, 
		int nWidth, 
		DWORD dwStyle, 
		const CString& sToolBarName
	);

	void AddLabel
		(
			UINT nID, 
			const CString& szText, 
			const CString& sToolBarName
		);

	void AddSeparator	(const CString& sToolBarName);
	void AddDropdownMenuItem(UINT nCommandID, UINT_PTR nIDNewItem, const CString& sNewItem, const CString& sToolBarName);
	// aggiunge i bottoni fin qui "collezionati" alla toolbar
	BOOL CreateNewButtons(CTBTabbedToolbar* pTabbedBar);
	
	const CObjectInfoArray* GetButtonsInfo() { return &m_ButtonsInfo; }
}; 

#include "endh.dex"
