#pragma once
class CEasyStudioDesignerView;
using namespace Microarea::EasyBuilder;

ref class JsonConnector : Microarea::EasyBuilder::IJsonEditor
{
private:
	System::String^ initialJsonFile = nullptr;
	CEasyStudioDesignerView* m_pDesignerView = NULL;
public:
	JsonConnector(CEasyStudioDesignerView* pDesignerView, System::String^ initialJsonFile);

	property System::String^ Json { virtual System::String^ get(); virtual void set(System::String^ value); }

	virtual property System::String^ InitialJsonFile { System::String^ get() { return initialJsonFile; } }
	virtual void AttachCodeEditor(Microarea::EasyBuilder::UI::JsonCodeControl^ c);
	virtual void UpdateFromSourceCode(System::String^ value);
	virtual bool SaveJson();
	virtual bool OpenJson(System::String^ file, bool isDocOutline);
	virtual bool CloseJson(System::String^ file, bool isDocOutline);
	virtual bool UpdateWindow(System::IntPtr hwnd);
	virtual bool UpdateWindow(System::String^ code);
	virtual bool UpdateTabOrder(System::IntPtr hwnd);
	virtual void AddWindow(System::IntPtr hwnd, System::IntPtr hwndParent);
	virtual void DeleteWindow(System::IntPtr hwnd);
	virtual System::String^ Undo();
	virtual System::String^ Redo();
	void OnCodeChanged(System::Object^ sender, System::EventArgs^ args);
	virtual void OnFormEditorDirtyChanged(Object^ sender, DirtyChangedEventArgs^ args);
};

