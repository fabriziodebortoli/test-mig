#pragma once

using namespace System::Windows::Forms;

//===========================================================================
//		this file can use managed objects
//===========================================================================
#include "ManagedEventsHandler.h"
#include "UserControlHandlersMixed.h"

//===========================================================================
//							CUserControlHandler
//===========================================================================
template<class TMControl>
class CUserControlHandler : public CUserControlHandlerMixed
{
private:
	gcroot<TMControl^>					m_Control;
	gcroot<System::Type^>				m_Type;
	gcroot<ManagedEventsHandlerObj^>	m_pEventsHandler;

public:
		//---------------------------------------------------------------------------------------
		CUserControlHandler<TMControl> (ManagedEventsHandlerObj^ eventsHandler)
			:
			m_pEventsHandler (nullptr)
		{ 
			m_Type = TMControl::typeid;
			ASSERT(eventsHandler != nullptr);
			m_pEventsHandler = eventsHandler;
		}

		//---------------------------------------------------------------------------------------
		~CUserControlHandler	()
		{ 
			if (m_pEventsHandler)
				delete m_pEventsHandler;
			//delete m_Control; la dispose viene chiamata quando muore l'oggetto grafico, qui sarebbe un duplicato, provoca una managed debugging assistant
		}
public:
	TMControl^				GetControl() { return m_Control; }
	virtual Control^		GetWinControl() { return m_Control; }
	ManagedEventsHandlerObj^GetEventsHandler() { return m_pEventsHandler; }
	HWND					GetControlHandle() 
	{ 
		return m_Control 
			? (HWND)m_Control->Handle.ToInt64() 
			: NULL; 
	}
	CWnd*					GetWnd			() { return m_pEventsHandler ? m_pEventsHandler->GetControlWnd() : NULL; }
	
	//---------------------------------------------------------------------------------------
	BOOL CreateControl (DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CWnd* pOwnerWnd)
	{
		ASSERT(pParentWnd);

		if (!CreateManagedControl(dwStyle, rect, pParentWnd, nID))
		{
			ASSERT_TRACE(FALSE, cwsprintf(_T("Error creating Managed User Control ID %d"), nID));
			return FALSE;
		}

		// I attach managed handler
		if (m_pEventsHandler)
		{
			m_pEventsHandler->AttachWindow(pOwnerWnd);
			m_pEventsHandler->MapEvents(GetControl());
		}
		return TRUE;
	}
	
	//---------------------------------------------------------------------------------------
	System::Drawing::Font^ ConvertFont (CFont* pFont)
	{
		if (pFont == NULL)
			return nullptr;
		
		IntPtr hFont (pFont->m_hObject);

		return System::Drawing::Font::FromHfont(hFont);
	}

	//---------------------------------------------------------------------------------------
	System::Drawing::Color ConvertColor (const COLORREF col)
	{
		return System::Drawing::ColorTranslator::FromWin32(col);
	}

	//---------------------------------------------------------------------------------------
	virtual void AttachControl (UnmanagedEventsArgs* pArg) override
	{
		if (!pArg || !pArg->IsKindOf(RUNTIME_CLASS(AttachControlEventArg)))
			return;

		if (m_pEventsHandler)
			delete m_pEventsHandler;

		ENSURE(GetControlHandle() != NULL);

		// I attach managed handler
		if (m_pEventsHandler)
			m_pEventsHandler->MapEvents(GetControl()); 
	}

	//---------------------------------------------------------------------------------------
	virtual void AttachWindow (CWnd* pWnd) override
	{
		if (m_pEventsHandler)
			m_pEventsHandler->AttachWindow(pWnd);
	}

private:

	BOOL CreateManagedControl(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
	{
		m_Control = (TMControl^)System::Activator::CreateInstance(m_Type);
		if (!m_Control)
			return FALSE;
		m_Control->Location = Point(rect.left, rect.top);
		m_Control->Size = Size(rect.right - rect.left, rect.bottom - rect.top);
		::SetParent(GetControlHandle(), pParentWnd->m_hWnd);
		DWORD dwCurrentStyle = ::GetWindowLong(GetControlHandle(), GWL_STYLE);
		dwCurrentStyle |= dwStyle;
		::SetWindowLong(GetControlHandle(), GWL_ID, nID);
		::SetWindowLong(GetControlHandle(), GWL_STYLE, dwCurrentStyle);
		return TRUE;
	}
	
};

//===========================================================================
//							Macros to semplify wrappers
//===========================================================================


#define DECLARE_CTRL_HANDLER(cl, pH) CUserControlHandler<cl>* pH = (CUserControlHandler<cl>*) GetHandler();

//===========================================================================
