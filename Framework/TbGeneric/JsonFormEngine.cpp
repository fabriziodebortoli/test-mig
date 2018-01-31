#include "stdafx.h"
#include <TbNameSolver\JsonSerializer.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include "Linefile.h"
#include "DLLMod.h"
#include "JsonFormEngine.h" 
#include "TBThemeManager.h"
#include "WndObjDescription.h"
#include "GeneralFunctions.h"

static const TCHAR szTBJsonFileExtFilter[] = _T("*.tbjson");

class CPusher
{
	CJsonContextObj* m_pContext;
	int pushedY1;
	int pushedY2;

public:
	CPusher(CJsonContextObj* pContext) : m_pContext(pContext) {
		pushedY1 = pContext->m_LatestY1;
		pContext->m_LatestY1 = 0;
		pushedY2 = pContext->m_LatestY2;
		pContext->m_LatestY2 = 0;
	}
	~CPusher() {
		m_pContext->m_LatestY1 = pushedY1;
		m_pContext->m_LatestY2 = pushedY2;
	}
};

//===========================================================
// DialogTemplate
//===========================================================

//-----------------------------------------------------------------------------
LPCDLGTEMPLATE DialogTemplate::Template(int & size)
{
	size = v.size();
	return (LPCDLGTEMPLATE)&v[0];
}

//-----------------------------------------------------------------------------
void DialogTemplate::AlignToDword()
{
	if (v.size() % 4) Write(NULL, 4 - (v.size() % 4));
}

//-----------------------------------------------------------------------------
void DialogTemplate::Write(LPCVOID pvWrite, DWORD cbWrite)
{
	v.insert(v.end(), cbWrite, 0);
	if (pvWrite) CopyMemory(&v[v.size() - cbWrite], pvWrite, cbWrite);
}

//-----------------------------------------------------------------------------
template<typename T> void DialogTemplate::Write(T t)
{
	Write(&t, sizeof(T));
}
//-----------------------------------------------------------------------------
void DialogTemplate::WriteString(LPCWSTR psz)
{
	Write(psz, (lstrlenW(psz) + 1) * sizeof(WCHAR));
}

//===========================================================
CJsonFormEngineObj* CJsonFormEngineObj::g_pJsonFormEngine = NULL;

//-----------------------------------------------------------------------------
CJsonContextObj::~CJsonContextObj()
{
	if (m_bOwnDescription)
		delete m_pDescription;

	for (int i = 0; i <= m_arFonts.GetUpperBound(); i++)
	{
		CFont* pFont = m_arFonts.GetAt(i);
		pFont->DeleteObject();
	}
}


//-----------------------------------------------------------------------------
CJsonContextObj* CJsonContextObj::GetRootContext()
{
	return m_pParentContext ? m_pParentContext->GetRootContext() : this;
}
//-----------------------------------------------------------------------------
void CJsonContextObj::Assign(CJsonContextObj* pOther)
{
	m_pParentContext = pOther;
	m_pFont = pOther->m_pFont;
	m_pWnd = pOther->m_pWnd;
	m_pDescription = pOther->m_pDescription;
	m_JsonResource = pOther->m_JsonResource;
	m_bOwnDescription = false;
}

//-----------------------------------------------------------------------------
CFont* CJsonContextObj::CreateFontFromDesc(CFont* pTemplateFont, CFontDescription* pFontDesc)
{
	LOGFONT lf;
	pTemplateFont->GetLogFont(&lf);
	lf.lfItalic = pFontDesc->m_bIsItalic;
	lf.lfUnderline = pFontDesc->m_bIsUnderline;
	if (pFontDesc->m_nFontSize)
		lf.lfHeight = GetDisplayFontHeight((int)pFontDesc->m_nFontSize);
	if (pFontDesc->m_bIsBold)
		lf.lfWeight = FW_BOLD;
	if (!pFontDesc->m_strFaceName.IsEmpty())
		_tcscpy_s(lf.lfFaceName, pFontDesc->m_strFaceName);
	CFont* pFont = new CFont();
	pFont->CreateFontIndirect(&lf);
	m_arFonts.Add(pFont);
	return pFont;
}

//-----------------------------------------------------------------------------
LPCDLGTEMPLATE CJsonContextObj::CreateTemplate(bool popup)
{
	if (!m_pDescription)
	{
		ASSERT(FALSE);
		return NULL;
	}
	DWORD nStyle = m_pDescription->m_nStyle | DS_SETFONT;//uso obbligatoriamente un font diverso da quello di sistema per trasformare le coordinate
	DWORD nExStyle = m_pDescription->m_nExStyle;
	m_pDescription->ApplyStyleFromProperties(nStyle, nExStyle);
	//aggiusto lo stile nel caso di finestra modale, che deve obbligatoriamente essere popup (in certi casi negli rc era sbagliato
	//ma MFC tollerava)
	if (popup)
	{
		nStyle |= WS_POPUP;
		nStyle &= ~WS_CHILD;
	}
	return CreateTemplate(m_pDescription, nStyle, nExStyle);
}
//-----------------------------------------------------------------------------
LPCDLGTEMPLATE CJsonContextObj::CreateTemplate(CWndObjDescription* pDescription, DWORD nStyle, DWORD nExStyle)
{
	return CreateTemplate(pDescription->GetRect(), AfxLoadJsonString(pDescription->m_strText, pDescription), nStyle, nExStyle);
}
//-----------------------------------------------------------------------------
LPCDLGTEMPLATE CJsonContextObj::CreateTemplate(const CRect& rect, const CString& sTitle, DWORD nStyle, DWORD nExStyle)
{
	DialogTemplate tmp;
	// Write out the extended dialog template header
	tmp.Write<WORD>(1); // dialog version
	tmp.Write<WORD>(0xFFFF); // extended dialog template
	tmp.Write<DWORD>(0); // help ID
	tmp.Write<DWORD>(nExStyle); // extended style
	tmp.Write<DWORD>(nStyle);

	tmp.Write<WORD>(0); // number of controls
	tmp.Write<WORD>((WORD)(rect.left == NULL_COORD ? 0 : rect.left)); // X
	tmp.Write<WORD>((WORD)(rect.top == NULL_COORD ? 0 : rect.top)); // Y
	if (IsScale())
	{
		tmp.Write<WORD>(rect.Width() == NULL_COORD ? 55000 : rect.Width()); // width
		tmp.Write<WORD>(rect.Height() == NULL_COORD ? 40000 : rect.Height()); // height
	}
	else
	{
		tmp.Write<WORD>(rect.Width() == NULL_COORD ? 654 : rect.Width()); // width
		tmp.Write<WORD>(rect.Height() == NULL_COORD ? 200 : rect.Height()); // height
	}
	tmp.WriteString(L""); // no menu
	tmp.WriteString(L""); // default dialog class
	tmp.WriteString(sTitle); // title
	CString sFaceName;
	WORD point = 0;
	/*#ifdef DEBUG
		if (!pDescription->m_sFontName.IsEmpty())
		{
			point = pDescription->m_nFontSize;
			sFaceName = pDescription->m_sFontName;
		}
		else
	#endif*/
	{
		LOGFONT lf;
		AfxGetFormFont()->GetLogFont(&lf);
		point = GetDisplayFontPointSize(lf.lfHeight);
		sFaceName = lf.lfFaceName;
	}

	tmp.Write<WORD>(point); // point
	tmp.Write<WORD>((WORD)0); // weight
	tmp.Write<BYTE>(0); // Italic
	tmp.Write<BYTE>(0); // CharSet
	tmp.WriteString(sFaceName);
	int size;
	LPCDLGTEMPLATE t = tmp.Template(size);

	void* p = new BYTE[size];
	memcpy_s(p, size, (void*)t, size);
	return (LPCDLGTEMPLATE)p;
}


//-----------------------------------------------------------------------------
CJsonFormEngineObj::~CJsonFormEngineObj()
{

}

//-----------------------------------------------------------------------------
CJsonFormEngineObj* CJsonFormEngineObj::GetInstance()
{
	return g_pJsonFormEngine;
}

//-----------------------------------------------------------------------------
DWORD CJsonFormEngineObj::GetID(CWndObjDescription* pWndDesc)
{
	TbResourceType type = TbResourceType::TbControls;
	CWndPanelDescription* pParentDesc = dynamic_cast<CWndPanelDescription*>(pWndDesc->GetParent());
	CString sId = pParentDesc && pParentDesc->m_bUserControl
		? pParentDesc->GetID() + _T('_') + pWndDesc->GetID()
		: pWndDesc->GetID();
	switch (pWndDesc->m_Type)
	{
	case CWndObjDescription::View:
	case CWndObjDescription::Tab:
	case CWndObjDescription::Tile:
	case CWndObjDescription::StatusTile:
	case CWndObjDescription::HotFilter:
	case CWndObjDescription::DockingPane:
	case CWndObjDescription::Panel:
		type = TbResourceType::TbResources;
		break;
	default:
		type = TbResourceType::TbControls;
		break;
	}
	return AfxGetTBResourcesMap()->GetTbResourceID(sId, type);

	//DWORD CJsonFormEngineObj::GetIDD(LPCTSTR lpszId)
	//	return AfxGetTBResourcesMap()->GetTbResourceID(lpszId, TbResourceType::TbResources);
	//DWORD CJsonFormEngineObj::GetID(LPCTSTR lpszId)
	//	return AfxGetTBResourcesMap()->GetTbResourceID(lpszId, TbResourceType::TbCommands);
}

//-----------------------------------------------------------------------------
CString CJsonFormEngineObj::GetObjectName(CWndObjDescription* pWndDesc)
{
	CString sName = pWndDesc->m_strName;
	if (sName.IsEmpty())
		sName = pWndDesc->GetID();
	return sName;
}


//-----------------------------------------------------------------------------
BOOL CJsonFormEngineObj::GetJsonFormInfo(const CString& sName, const CTBNamespace& ns, CString& sJsonFile, CTBNamespace& ownerModule)
{
	CString sPath = AfxGetPathFinder()->GetJsonFormPath(ns, CPathFinder::PosType::STANDARD) + SLASH_CHAR + sName + szTBJsonFileExt;
	if (ExistFile(sPath))
	{
		sJsonFile = sPath;
		ownerModule = CTBNamespace(CTBNamespace::MODULE, ns.GetApplicationName() + _T('.') + ns.GetModuleName());
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void GetSize(CWndObjDescription* pWndDesc, int &w, int &h)
{
	if (pWndDesc->m_Width == NULL_COORD)
	{
		switch (pWndDesc->m_Type)
		{
		case CWndObjDescription::Label:
			w = 98;
			break;
		case CWndObjDescription::Combo:
		case CWndObjDescription::List:
			w = 40;
			break;

		case CWndObjDescription::Edit:
			if (pWndDesc->m_strControlClass == "MoneyEdit")
				w = 86;
			else if (pWndDesc->m_strControlClass == "PercentEdit")
				w = 30;
			else if (pWndDesc->m_strControlClass == "DateEdit")
				w = 57;
			else if (pWndDesc->m_strControlClass == "QuantityEdit")
				w = 40;
			else
				w = 40;
			break;
		case CWndObjDescription::Button:
		case CWndObjDescription::Radio:
		case CWndObjDescription::Check:
			w = 109;
			break;
		default:
			w = 40;
			break;
		}
	}
	else
	{
		w = pWndDesc->m_Width;
	}

	if (pWndDesc->m_Height == NULL_COORD)
	{
		switch (pWndDesc->m_Type)
		{
		case CWndObjDescription::Label:
			h = 8;
			break;
		case CWndObjDescription::Button:
		case CWndObjDescription::Radio:
		case CWndObjDescription::Check:
			h = 10;
			break;
		case CWndObjDescription::Combo:
		case CWndObjDescription::List:
			h = 64;
			break;
		default:
			h = 12;
			break;
		}
	}
	else
	{
		h = pWndDesc->m_Height;
	}
}

//-----------------------------------------------------------------------------
CString CJsonFormEngineObj::CalculateLURect(CWndObjDescription* pWndDesc, CJsonContextObj* pContext)
{
	int w = 0, h = 0;
	GetSize(pWndDesc, w, h);
	CWndObjDescription* pWndDescParent = pWndDesc->GetParent();
	int x = pWndDesc->m_X, y = pWndDesc->m_Y;
	CString sAnchor = pWndDesc->m_sAnchor;
	if (x == NULL_COORD && y == NULL_COORD && sAnchor.IsEmpty())
		sAnchor = COL1;

	while (!sAnchor.IsEmpty())
	{
		bool bInStaticArea = pWndDesc->m_Type == CWndObjDescription::Radio ||
			pWndDesc->m_Type == CWndObjDescription::Check ||
			pWndDesc->m_Type == CWndObjDescription::BodyEdit ||
			pWndDesc->m_Type == CWndObjDescription::TreeBodyEdit ||
			pWndDesc->m_Type == CWndObjDescription::PropertyGrid ||
			(pWndDesc->m_Type == CWndObjDescription::Label && pWndDesc->m_strControlClass == _T("LabelStatic"));
		if (sAnchor == COL1)
		{
			//se sono una tile mini mi metto più a sinistra
			x = GetLeftMargin(pWndDescParent, true, bInStaticArea);
			y = pContext->m_LatestY1 + SPACING;
			break;
		}
		else if (sAnchor == COL2)
		{
			//se ho due colonne in una tile standard, devo mettermi 
			x = GetLeftMargin(pWndDescParent, false, bInStaticArea);
			y = pContext->m_LatestY2 + SPACING;
			break;
		}
		else
		{
			CRect brotherRect;
			if (pContext->m_Rects.Lookup(sAnchor, brotherRect))
			{
				x = brotherRect.right + SPACING;
				y = brotherRect.top;
				break;
			}
			else
			{
				//non trovo il controllo da ancorare: probabile che non sia attivato; lo cerco fra i fratelli,
				//e ne eredito il suo ancoraggio
				CWndObjDescription* pAnchor = pWndDescParent->Find(sAnchor);
				ASSERT(pAnchor);
				sAnchor = pAnchor ? pAnchor->m_sAnchor : _T("");
				//niente break, reitera il loop col nuovo anchor
			}
		}
	}
	if (pWndDesc->m_MarginLeft != NULL_COORD)
		x += pWndDesc->m_MarginLeft;
	if (pWndDesc->m_CaptionWidth != NULL_COORD)
		x += pWndDesc->m_CaptionWidth;
	else if (pWndDescParent && pWndDescParent->m_CaptionWidth != NULL_COORD)
		x += pWndDescParent->m_CaptionWidth;

	if (pWndDesc->m_MarginTop != NULL_COORD)
		y += pWndDesc->m_MarginTop;

	if (pWndDesc->m_MarginBottom != NULL_COORD)
		h += pWndDesc->m_MarginBottom;

	CRect luRect(x, y, x + w, y + h);

	//siccome in MFC groupbox e controlli sono fratelli mentre in json sono nidificati,
	//se il parent del controllo è un group devo aggiustare le coordinate in modo che siano relative
	//non alla group, ma al parent della group
	if (pWndDescParent && pWndDescParent->m_Type == CWndObjDescription::Group)
	{
		luRect.OffsetRect(pWndDescParent->m_X, pWndDescParent->m_Y);
	}
	pWndDesc->m_CalculatedLURect = luRect;
	return sAnchor;
}

//-----------------------------------------------------------------------------
void CJsonFormEngineObj::UpdateAnchorInfo(const CString& sAnchor, CWndObjDescription* pWndDesc, CJsonContextObj* pContext)
{
	//per la combo, il rettangolo è quello relativo alla tendina aperta,
	//ma per posizionare il controllo mi serve quello relativo alla tendina chiusa
	if (pWndDesc->m_Type == CWndObjDescription::Combo)
	{
		pWndDesc->m_CalculatedLURect.bottom = pWndDesc->m_CalculatedLURect.top + 12;
		if (pWndDesc->m_MarginBottom != NULL_COORD)
			pWndDesc->m_CalculatedLURect.bottom += pWndDesc->m_MarginBottom;
	}
	if (!sAnchor.IsEmpty())
	{
		CWndObjDescription* pWndDescParent = pWndDesc->GetParent();
		CString sAdjustedAnchor = sAnchor;
		while (true)
		{
			if (sAdjustedAnchor == COL1)
			{
				pContext->m_LatestY1 = max(pContext->m_LatestY1, pWndDesc->m_CalculatedLURect.bottom);
				break;
			}
			else if (sAdjustedAnchor == COL2)
			{
				pContext->m_LatestY2 = max(pContext->m_LatestY2, pWndDesc->m_CalculatedLURect.bottom);
				break;
			}
			else
			{
				CWndObjDescription* pAnchor = pWndDescParent->Find(sAdjustedAnchor);
				if (!pAnchor)
				{
					ASSERT(FALSE);
					break;
				}
				else
				{
					sAdjustedAnchor = pAnchor->m_sAnchor;
					if (sAdjustedAnchor.IsEmpty())
						break;
				}
			}
		}
	}
	pContext->m_Rects[pWndDesc->GetID()] = pWndDesc->m_CalculatedLURect;
}
//-----------------------------------------------------------------------------
BOOL CJsonFormEngineObj::ProcessWndDescription(CWndObjDescription* pWndDesc, CWnd* pParentWnd, CJsonContextObj* pContext)
{
	if (!pWndDesc)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	HWND hWnd = NULL;
	CString sAnchor = CalculateLURect(pWndDesc, pContext);
	CRect pxRect = pWndDesc->m_CalculatedLURect;
	//trasformo le coordinate da dialog unit a pixel
	::MapDialogRect(pContext->m_pWnd->m_hWnd, pxRect);
	//costruisco lo stile; prima quello che proviene dal controllo
	DWORD nStyle = pWndDesc->m_nStyle;
	DWORD nExStyle = pWndDesc->m_nExStyle | WS_EX_NOPARENTNOTIFY; //default
	//poi quello proveneiente dalle sue proprietà esplicite
	pWndDesc->ApplyStyleFromProperties(nStyle, nExStyle);

	//aggiusto il rettangolo per tenere conto del titolo, menu ecc
	//AdjustWindowRectEx(&r, nStyle, FALSE, nExStyle);

	//processo l'elemento specifico
	bool recurseChilds = false;
	switch (pWndDesc->m_Type)
	{
	case CWndObjDescription::View:
	case CWndObjDescription::DockingPane:
	{
		recurseChilds = true;
		break;
	}
	case CWndObjDescription::Panel:
	{
		recurseChilds = true;
		AutoDeletePtr<DLGTEMPLATE> pTemplate = (DLGTEMPLATE*)pContext->CreateTemplate(pWndDesc, nStyle, nExStyle);
		if (!pTemplate)
			return FALSE;
		hWnd = CreateDialogIndirect(NULL, pTemplate, pParentWnd->m_hWnd, NULL);
		break;
	}
	case CWndObjDescription::Group:
	{
		hWnd = CreateWindowEx(nExStyle, _T("BUTTON"), NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
			pParentWnd->m_hWnd, NULL, NULL, NULL);
		//ricorsione sui figli
		for (int i = 0; i < pWndDesc->m_Children.GetCount(); i++)
		{
			CWndObjDescription* pChild = pWndDesc->m_Children[i];
			if (!ProcessWndDescription(pChild, pParentWnd, pContext))
				return FALSE;
		}
		break;
	}
	case CWndObjDescription::Label:
	case CWndObjDescription::Image:
	case CWndObjDescription::TreeAdv:
	{
		hWnd = CreateWindowEx(nExStyle, _T("STATIC"), NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
			pParentWnd->m_hWnd, NULL, NULL, NULL);

		break;
	}
	case CWndObjDescription::Edit:
	{
		hWnd = CreateWindowEx(nExStyle, _T("EDIT"), NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
			pParentWnd->m_hWnd, NULL, NULL, NULL);
		break;
	}
	case CWndObjDescription::Combo:
	{
		hWnd = CreateWindowEx(nExStyle, _T("COMBOBOX"), NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
			pParentWnd->m_hWnd, NULL, NULL, NULL);
		break;
	}
	case CWndObjDescription::List:
	{
		hWnd = CreateWindowEx(nExStyle, _T("LISTBOX"), NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
			pParentWnd->m_hWnd, NULL, NULL, NULL);
		break;
	}
	case CWndObjDescription::Button:
	case CWndObjDescription::Radio:
	case CWndObjDescription::Check:
	{

		hWnd = CreateWindowEx(nExStyle, _T("BUTTON"), NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
			pParentWnd->m_hWnd, NULL, NULL, NULL);
		if (pWndDesc->IsKindOf(RUNTIME_CLASS(CWndCheckRadioDescription)) && ((CWndCheckRadioDescription*)pWndDesc)->m_bChecked)
		{
			::SendMessage(hWnd, BM_SETCHECK, BST_CHECKED, 0);
		}
		break;
	}
	case CWndObjDescription::ProgressBar:
	{
		hWnd = CreateWindowEx(nExStyle, _T("MSCTLS_PROGRESS32"), NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
			pParentWnd->m_hWnd, NULL, NULL, NULL);
		break;
	}
	case CWndObjDescription::Tabber:
	{
		hWnd = CreateWindowEx(nExStyle, _T("SYSTABCONTROL32"), NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
			pParentWnd->m_hWnd, NULL, NULL, NULL);
		break;
	}
	case CWndObjDescription::Tree:
	{
		hWnd = CreateWindowEx(nExStyle, _T("SYSTREEVIEW32"), NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
			pParentWnd->m_hWnd, NULL, NULL, NULL);
		break;
	}
	case CWndObjDescription::ListCtrl:
	{
		hWnd = CreateWindowEx(nExStyle, _T("SYSLISTVIEW32"), NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
			pParentWnd->m_hWnd, NULL, NULL, NULL);
		break;
	}
	case CWndObjDescription::Spin:
	{
		hWnd = CreateWindowEx(nExStyle, _T("MSCTLS_UPDOWN32"), NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
			pParentWnd->m_hWnd, NULL, NULL, NULL);
		break;
	}
	case CWndObjDescription::GenericWndObj:
	{
		if (!pWndDesc->m_strControlClass.IsEmpty())
		{
			IID clsId;
			if (SUCCEEDED(CLSIDFromString(pWndDesc->m_strControlClass, &clsId)))
			{
				CWnd wnd;
				VERIFY(wnd.CreateControl(clsId, NULL, nStyle, pxRect, pParentWnd, GetID(pWndDesc)));
				wnd.Detach();
			}
			else
			{
				hWnd = CreateWindowEx(nExStyle, pWndDesc->m_strControlClass, NULL, nStyle, pxRect.left, pxRect.top, pxRect.Width(), pxRect.Height(),
					pParentWnd->m_hWnd, NULL, NULL, NULL);
			}
			break;
		}
	}
	default:
		ASSERT(FALSE);
		TRACE1("Unrecognized type: %s\n", CWndObjDescription::GetEnumDescription(pWndDesc->m_Type));
		return TRUE;
	}

	//la struttura usata per creare la finestra 
	//può essere eliminata, probabilmente verrà nuovamente subclassata a valle
	//la finestra sottostante però deve rimanere, quindi ne faccio il detach
	if (hWnd)
	{

		CWnd* pWnd = CWnd::FromHandle(hWnd);
		pWnd->SetDlgCtrlID(GetID(pWndDesc));
		BOOL bIcon = FALSE;
		BOOL bBitmap = FALSE;

		BOOL bButton = pWndDesc->m_Type == CWndObjDescription::Button;
		if (bButton)
		{
			bIcon = ((nStyle & BS_ICON) == BS_ICON);
			bBitmap = ((nStyle & BS_BITMAP) == BS_BITMAP);
		}

		BOOL bStatic = pWndDesc->m_Type == CWndObjDescription::Label || pWndDesc->m_Type == CWndObjDescription::Image;
		if (bStatic)
		{
			bIcon = ((nStyle & SS_ICON) == SS_ICON);
			bBitmap = ((nStyle & SS_BITMAP) == SS_BITMAP);
		}
		//se contiene una bitmap, il percorso de file è contenuto nella proprietà text
		if ((bIcon || bBitmap) && !pWndDesc->m_strText.IsEmpty())
		{
			CTBNamespace aNamespace(pWndDesc->GetResource().GetOwnerNamespace());
			aNamespace.SetType(CTBNamespace::IMAGE);
			aNamespace.SetObjectName(pWndDesc->m_strText);

			CString sFile = AfxGetPathFinder()->GetModuleFilesPath(aNamespace, CPathFinder::STANDARD) + SLASH_CHAR + pWndDesc->m_strText;
			HANDLE h = NULL;
			if (bIcon)
			{
				h = LoadImage(NULL, sFile, IMAGE_ICON, 0, 0, LR_DEFAULTSIZE | LR_LOADFROMFILE);
				if (bButton)
					::SendMessage(hWnd, BM_SETIMAGE, IMAGE_ICON, (LPARAM)h);
				else
					::SendMessage(hWnd, STM_SETICON, (WPARAM)h, NULL);
			}
			else
			{
				h = LoadImage(NULL, sFile, IMAGE_BITMAP, 0, 0, LR_CREATEDIBSECTION | LR_DEFAULTSIZE | LR_LOADFROMFILE);
				if (bButton)
					::SendMessage(hWnd, BM_SETIMAGE, IMAGE_BITMAP, (LPARAM)h);
				else
					::SendMessage(hWnd, STM_SETIMAGE, IMAGE_BITMAP, (LPARAM)h);
			}

			UINT msg = bButton ? BM_SETIMAGE : (bIcon ? STM_SETICON : STM_SETIMAGE);
			WPARAM wParam = bBitmap ? IMAGE_BITMAP : IMAGE_ICON;


		}
		else
		{
			if (!pWndDesc->m_strText.IsEmpty())
				pWnd->SetWindowText(AfxLoadJsonString(pWndDesc->m_strText, pWndDesc));
			CFont* pFont = pContext->m_pFont;
			if (!pFont)
				pFont = AfxGetControlFont();
			//ho un font da usare, se sono un parsed control (m_strControlClass non vuota)
			//imposterò il font dopo, nella AddLink
			if (pWndDesc->m_pFontDescription)
			{
				//i parsed control usano la SetOwnFont
				pFont = pWndDesc->m_strControlClass.IsEmpty()
					? pContext->CreateFontFromDesc(pFont, pWndDesc->m_pFontDescription)
					: NULL;
			}

			if (pFont)
				pWnd->SetFont(pFont);

		}

		pWndDesc->AttachTo(hWnd);

		UpdateAnchorInfo(sAnchor, pWndDesc, pContext);

		if (recurseChilds)
		{
			CPusher pusher(pContext);
			//ricorsione sui figli
			for (int i = 0; i < pWndDesc->m_Children.GetCount(); i++)
			{
				CWndObjDescription* pChild = pWndDesc->m_Children[i];
				if (!ProcessWndDescription(pChild, pWnd, pContext))
					return FALSE;
			}

		}
	}

	return TRUE;
}


//-----------------------------------------------------------------------------
void CJsonFormEngineObj::AddAssociation(HWND hwnd, CWndObjDescription* pDesc)
{
	TB_OBJECT_LOCK(&m_Windows);
	m_Windows[hwnd] = pDesc;
}

//-----------------------------------------------------------------------------
bool CJsonFormEngineObj::IsExpression(const CString& sText, CString& sBareText)
{
	int len = sText.GetLength();
	//se trovo una sintassi del tipo: {{x}}, allora devo cercare la variable x
	if (len >= 5 &&
		sText[0] == _T('{') &&
		sText[1] == _T('{') &&
		sText[len - 1] == _T('}') &&
		sText[len - 2] == _T('}'))
	{
		sBareText = sText.Mid(2, len - 4);
		return true;
	}
	return false;
}
//-----------------------------------------------------------------------------
void CJsonFormEngineObj::RemoveAssociation(HWND hwnd, CWndObjDescription* pDesc)
{
	TB_OBJECT_LOCK(&m_Windows);
	CWndObjDescription* pExistingDesc = NULL;
	if (!m_Windows.Lookup(hwnd, pExistingDesc) || pExistingDesc != pDesc)
		return;

	m_Windows.RemoveKey(hwnd);
}
//-----------------------------------------------------------------------------
CWndObjDescription* CJsonFormEngineObj::GetAssociation(HWND hwnd)
{
	TB_OBJECT_LOCK_FOR_READ(&m_Windows);
	CWndObjDescription* pDesc = NULL;
	m_Windows.Lookup(hwnd, pDesc);
	return pDesc;
}


//-----------------------------------------------------------------------------
CWndObjDescription* CJsonFormEngineObj::ParseDescriptions(CJsonContextObj* pContext, CArray<CJsonResource>& sources)
{
	if (sources.GetCount() == 0)
	{
		ASSERT(FALSE);
		return NULL;
	}
	//sources contiene la lista dei file da caricare, il primo è il principale, gli altri, eventuali, quelli aggiunti da client doc
	CWndObjDescription *pDescription = pContext->m_pDescription;

	for (int i = 0; i < sources.GetSize(); i++)
	{
		CJsonResource source = sources[i];
		if (source.m_bExclude)
			continue;
		CArray<CWndObjDescription*>ar;
		ParseDescription(ar, pContext, source, L"", pDescription, CWndObjDescription::Undefined);
		if (!pDescription && ar.GetCount())
		{
			//se pDescription è NULL, sono nella root, e allora troverò solo una descrizione
			//posso trovare più descrizioni solo se parso json forms, che potrebbero andare 
			//a modificare più di un elemento del server form
			ASSERT(ar.GetCount() == 1);
			pDescription = ar[0];
			if (pDescription && pDescription->IsKindOf(RUNTIME_CLASS(CWndFrameDescription)))
			{
				CWndFrameDescription* pFrameDesc = (CWndFrameDescription*)pDescription;
				for (int i = 0; i < pFrameDesc->m_arHrefHierarchy.GetCount(); i++)
				{
					CString sHref = pFrameDesc->m_arHrefHierarchy[i];
					if (sHref == pContext->m_JsonResource.m_strName)
						continue;//già considerato a livello root
					GetDeltaJsonFormInfos(sHref, sources);
				}

			}
		}
	}
	ASSERT(pDescription);
	return pDescription;
}

//-----------------------------------------------------------------------------
void CJsonFormEngineObj::ParseDescription(CArray<CWndObjDescription*>&ar, CJsonContextObj* pContext, CJsonResource source, LPCTSTR sActivation, CWndObjDescription* pDescriptionToMerge, int expectedType)
{
	CString sFile;
	CTBNamespace moduleNamespace;
	source.GetInfo(sFile, moduleNamespace);
	if (sFile.IsEmpty())
	{
		ASSERT(FALSE);
		return;
	}
	ParseDescription(ar, pContext, sFile, sActivation, pDescriptionToMerge, expectedType);
	for (int i = 0; i < ar.GetCount(); i++)
	{
		CWndObjDescription* pDesc = ar[i];
		CJsonResource* pRes = new CJsonResource(source);
		pRes->m_strJsonContext = pDesc->m_strContext;
		pDesc->m_Resources.Add(pRes);
	}
}
//-----------------------------------------------------------------------------
void CJsonFormEngineObj::ParseDescription(CArray<CWndObjDescription*>&ar, CJsonContextObj* pContext, const CString& sFile, LPCTSTR sActivation, CWndObjDescription* pDescriptionToMerge, int expectedType)
{
	CLineFile file;
	if (!file.Open(sFile, CFile::modeRead | CFile::typeText))
	{

		ASSERT(FALSE);
		TRACE1("Cannot open file: %s", sFile);
		return;
	}
	return ParseDescriptionFromText(ar, pContext, file.ReadToEnd(), sActivation, pDescriptionToMerge, expectedType);
}

//-----------------------------------------------------------------------------
void CJsonFormEngineObj::ParseDescriptionFromText(CArray<CWndObjDescription*>&ar, CJsonContextObj* pContext, LPCTSTR lpszText, LPCTSTR sActivation, CWndObjDescription* pDescriptionToMerge, int expectedType)
{
	CJsonFormParser parser;
	parser.m_pRootContext = pContext;
	parser.m_sActivation = sActivation;
	if (!parser.ReadJsonFromString(lpszText))
	{
		CString sError = parser.GetError();
		CString sErrorMex = _T("Error parsing the file.");
		AfxGetDiagnostic()->Add(sError);
		TRACE(sError);
		MessageBox(0, _TB("" + sErrorMex + "\n" + sError), (LPCWSTR)sErrorMex, 0);
		return;
	}

	if (pDescriptionToMerge)//leggo un delta
	{
		bool bOldForAppend = parser.m_bForAppend;
		parser.m_bForAppend = true;
		CString sId;
		if (parser.Has(szJsonId))
			sId = parser.ReadString(szJsonId);
		CArray<CWndObjDescription*>arFound;
		pDescriptionToMerge->FindAll(sId, arFound);
		if (arFound.GetCount())
		{
			for (int i = 0; i < arFound.GetCount(); i++)
			{
				CWndObjDescription* pDescription = arFound[i];
				pDescription->ParseJson(parser);
				parser.m_bForAppend = bOldForAppend;
				if (pDescription)
					ar.Add(pDescription);
				if (arFound.GetCount() > 1)
					parser.Reset();
			}
			return;
		}
		parser.m_bForAppend = bOldForAppend;
	}
	CWndObjDescription* pDesc = CWndObjDescription::ParseJsonObject(parser, NULL, (CWndObjDescription::WndObjType) expectedType);
	if (pDesc)
		ar.Add(pDesc);
}


//-----------------------------------------------------------------------------
BOOL CJsonFormEngineObj::CreateChilds(CJsonContextObj* pContext, CWnd* pWnd)
{
	if (!pContext || !pContext->m_pDescription)
	{
		ASSERT(FALSE);
		return FALSE;
	}
	pContext->Associate(pWnd);
	CPusher pusher(pContext);
	//ricorsione sui figli
	for (int i = 0; i < pContext->m_pDescription->m_Children.GetCount(); i++)
	{
		CWndObjDescription* pChild = pContext->m_pDescription->m_Children[i];
		ProcessWndDescription(pChild, pWnd, pContext);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
TB_EXPORT HACCEL TBLoadAccelerators(UINT nAccelIDR)
{
	AutoDeletePtr<CJsonContextObj> pContext = TBLoadAcceleratorContext(nAccelIDR);
	if (!pContext)
	{
		HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(nAccelIDR), RT_ACCELERATOR);
		return ::LoadAccelerators(hInst, MAKEINTRESOURCE(nAccelIDR));
	}
	int nSize = 0;
	AutoDeletePtr<ACCEL> pAccel = pContext->m_pDescription->m_pAccelerator->ToACCEL(pContext, nSize);
	return CreateAcceleratorTable(pAccel, nSize);

}

//-----------------------------------------------------------------------------
TB_EXPORT CJsonContextObj* TBLoadAcceleratorContext(UINT nAccelIDR)
{
	//se sono nel contesto di WebLook, non carico gli acceleratori, consumano risorse inutilmente
	if (AfxIsRemoteInterface())
		return NULL;
	CJsonResource sResource = AfxGetTBResourcesMap()->DecodeID(TbResources, nAccelIDR);
	//è stata mappata come risorsa dinamica: si tratta di risorsa in file tbjson
	if (sResource.IsEmpty())
	{
		return NULL;
	}
	CJsonContextObj* pJsonContext = CJsonFormEngineObj::GetInstance()->CreateContext(sResource);
	if (!pJsonContext)
		return NULL;

	if (!pJsonContext->m_pDescription || !pJsonContext->m_pDescription->m_pAccelerator)
	{
		delete pJsonContext;
		return NULL;
	}

	return pJsonContext;
}


//-----------------------------------------------------------------------------
bool CJsonFormParser::ResolveValue(LPCTSTR szName, UsedDefines& resolvedDefines, Json::Value& val)
{
	bool resolved = false;
	CString sName, sVal;
	if (BeginReadObject(szName))
	{
		if (Has(szJsonConst))
		{
			sName = ReadString(szJsonConst);
			if (m_pRootContext->m_Defines.Lookup(sName, val))
			{
				resolvedDefines[szName] = sName;
				resolved = true;
			}
		}
		EndReadObject();
	}
	ASSERT_TRACE1(resolved, "Constant not found: %s\n", szName);
	return resolved;
}
//-----------------------------------------------------------------------------
CString CJsonFormParser::ResolveString(LPCTSTR szName, UsedDefines& resolvedDefines)
{
	Json::Value val;
	bool resolved = ResolveValue(szName, resolvedDefines, val);
	return resolved && val.isString() ? val.asString().c_str() : _T("");
}
//-----------------------------------------------------------------------------
int CJsonFormParser::ResolveInt(LPCTSTR szName, UsedDefines& resolvedDefines)
{
	Json::Value val;
	bool resolved = ResolveValue(szName, resolvedDefines, val);
	return resolved && val.isInt() ? val.asInt() : 0;
}
//-----------------------------------------------------------------------------
bool CJsonFormParser::ResolveInt(LPCTSTR szName, CJsonExpressions& map, int& i)
{
	if (TryReadInt(szName, i))
		return true;
	CString s;
	if (!TryReadString(szName, s))
		return false;
	CString sBareText;
	if (!CJsonFormEngineObj::IsExpression(s, sBareText))
		return false;
	map.m_IntExpressions[&i] = sBareText;
	return true;
}
//-----------------------------------------------------------------------------
bool CJsonFormParser::ResolveBool(LPCTSTR szName, UsedDefines& resolvedDefines)
{
	Json::Value val;
	bool resolved = ResolveValue(szName, resolvedDefines, val);
	return resolved && val.isBool() ? val.asBool() : false;
}
//-----------------------------------------------------------------------------
bool CJsonFormParser::ResolveString(LPCTSTR szName, CJsonExpressions& map, CString& s)
{
	if (!TryReadString(szName, s))
		return false;
	CString sBareText;
	if (!CJsonFormEngineObj::IsExpression(s, sBareText))
		return false;
	map.m_StringExpressions[&s] = sBareText;
	return true;
}
//-----------------------------------------------------------------------------
bool CJsonFormParser::ResolveBool(LPCTSTR szName, CJsonExpressions& map, bool& b)
{
	if (TryReadBool(szName, b))
		return true;
	CString s;
	if (!TryReadString(szName, s))
		return false;
	CString sBareText;
	if (!CJsonFormEngineObj::IsExpression(s, sBareText))
		return false;
	map.m_BoolExpressions[&b] = sBareText;
	return true;
}
//-----------------------------------------------------------------------------
bool CJsonFormParser::ResolveBool3(LPCTSTR szName, CJsonExpressions& map, Bool3& b3)
{
	bool b;
	if (TryReadBool(szName, b))
	{
		b3 = b ? B_TRUE : B_FALSE;
		return true;
	}
	CString s;
	if (!TryReadString(szName, s))
		return false;
	CString sBareText;
	if (!CJsonFormEngineObj::IsExpression(s, sBareText))
		return false;
	map.m_Bool3Expressions[&b3] = sBareText;
	return true;
}
//-----------------------------------------------------------------------------
double CJsonFormParser::ResolveDouble(LPCTSTR szName, UsedDefines& resolvedDefines)
{
	Json::Value val;
	bool resolved = ResolveValue(szName, resolvedDefines, val);
	return resolved && val.isDouble() ? val.asDouble() : 0.0;
}
//-----------------------------------------------------------------------------
bool CJsonFormParser::ResolveDouble(LPCTSTR szName, CJsonExpressions& map, double& d)
{
	if (TryReadDouble(szName, d))
		return true;
	CString s;
	if (!TryReadString(szName, s))
		return false;
	CString sBareText;
	if (!CJsonFormEngineObj::IsExpression(s, sBareText))
		return false;
	map.m_DoubleExpressions[&d] = sBareText;
	return true;
}
