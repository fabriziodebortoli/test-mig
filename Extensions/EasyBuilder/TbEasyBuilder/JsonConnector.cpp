#include "stdafx.h"
#include "JsonDesigner.h"
#include "JsonConnector.h"

JsonConnector::JsonConnector(CEasyStudioDesignerView* pDesignerView, System::String^ initialJsonFile)
	: m_pDesignerView(pDesignerView), initialJsonFile(initialJsonFile)
{
}

//-----------------------------------------------------------------------------
System::String^ JsonConnector::Json::get()
{
	return gcnew System::String (m_pDesignerView->GetSourceCode());
}
//-----------------------------------------------------------------------------
void JsonConnector::Json::set(System::String^ value)
{
	UpdateFromSourceCode(value);
	m_pDesignerView->UpdateSourceCode();
}
//-----------------------------------------------------------------------------
void JsonConnector::UpdateFromSourceCode(System::String^ value)
{
	m_pDesignerView->UpdateFromSourceCode(value);
}
//-----------------------------------------------------------------------------
bool JsonConnector::SaveJson()
{
	return m_pDesignerView->m_pDialog ? m_pDesignerView->m_pDialog->SaveFile() : false;
}
//-----------------------------------------------------------------------------
bool JsonConnector::OpenJson(System::String^ file, bool isDocOutline)
{
	if (!m_pDesignerView->OpenDialog(file, isDocOutline))
		return false;
	m_pDesignerView->UpdateSourceCode();
	return true;
}
//-----------------------------------------------------------------------------
bool JsonConnector::CloseJson(System::String^ file, bool isDocOutline)
{
	if (!m_pDesignerView->CloseDialog(file, isDocOutline))
		return false;
	m_pDesignerView->UpdateSourceCode(CString(""));
	return true;
}
//-----------------------------------------------------------------------------
bool JsonConnector::UpdateWindow(System::IntPtr hwnd)
{
	m_pDesignerView->UpdateSourceCode();
	return true;
}
//-----------------------------------------------------------------------------
bool JsonConnector::UpdateWindow(System::String^ code)
{
	m_pDesignerView->UpdateSourceCode(CString(code));
	return true;
}
//-----------------------------------------------------------------------------
bool JsonConnector::UpdateTabOrder(System::IntPtr hwnd)
{
	if (!m_pDesignerView->m_pDialog->UpdateTabOrder((HWND)hwnd.ToInt64()))
		return false;
	m_pDesignerView->UpdateSourceCode();
	OnCodeChanged(this, System::EventArgs::Empty);
	return true;
}
//-----------------------------------------------------------------------------
void JsonConnector::AddWindow(System::IntPtr hwnd, System::IntPtr hwndParent)
{
	if (!m_pDesignerView->m_pDialog->AddDescription((HWND)hwnd.ToInt64(), (HWND)hwndParent.ToInt64()))
		return;
	m_pDesignerView->UpdateSourceCode();
}
//-----------------------------------------------------------------------------
void JsonConnector::DeleteWindow(System::IntPtr hwnd)
{
	if (!m_pDesignerView->m_pDialog->DeleteDescription((HWND)hwnd.ToInt64()))
		return;
	m_pDesignerView->UpdateSourceCode();
	OnCodeChanged(this, System::EventArgs::Empty);
}

//-----------------------------------------------------------------------------
void JsonConnector::OnCodeChanged(System::Object^ sender, System::EventArgs^ args)
{
	m_pDesignerView->UpdateFromSourceCode(m_pDesignerView->GetCodeControlCode());
}

//-----------------------------------------------------------------------------
void JsonConnector::AttachCodeEditor(Microarea::EasyBuilder::UI::JsonCodeControl^ c)
{
	if (m_pDesignerView->m_CodeControl)
		m_pDesignerView->m_CodeControl->CodeChanged -= gcnew System::EventHandler(this, &JsonConnector::OnCodeChanged);
	m_pDesignerView->m_CodeControl = c;
	if (c)
		c->CodeChanged += gcnew System::EventHandler(this, &JsonConnector::OnCodeChanged);
}



//-----------------------------------------------------------------------------
void JsonConnector::OnFormEditorDirtyChanged(Object^ sender, DirtyChangedEventArgs^ args)
{
	
	if (!m_pDesignerView)
		return;
	m_pDesignerView->SetDirty(args->Dirty);

}

//-----------------------------------------------------------------------------
System::String^ JsonConnector::Undo()
{
	return m_pDesignerView->Undo();
}

//-----------------------------------------------------------------------------
System::String^ JsonConnector::Redo()
{
	return m_pDesignerView->Redo();
}