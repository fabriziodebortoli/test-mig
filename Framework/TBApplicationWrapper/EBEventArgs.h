#pragma once

//=============================================================================
public ref class EasyBuilderEventArgs : public System::EventArgs
{
	bool handled;
public:
	property bool Handled 
	{
		bool get() { return handled; }
		void set (bool value) { handled = value; } 
	} 

	static property EasyBuilderEventArgs^ Empty { EasyBuilderEventArgs^ get() { return gcnew EasyBuilderEventArgs();}}
};

//=============================================================================
public ref class WindowMessageEventArgs : EasyBuilderEventArgs
{
private:
	int msg;
	System::IntPtr wParam;
	System::IntPtr lParam;
	
public:
	property int			Msg			{ int get () { return msg;		}	}
	property System::IntPtr	WParam		{ System::IntPtr get () { return wParam;	}	}
	property System::IntPtr	LParam		{ System::IntPtr get () { return lParam;	}	}
	
public:
	WindowMessageEventArgs(int msg, System::IntPtr wParam, System::IntPtr lParam)
	{
		this->msg = msg;	
		this->wParam = wParam;
		this->lParam = lParam;
	}
};

//=============================================================================
public ref class SelectedItemEventArgs : public EasyBuilderEventArgs
{
	System::Object^ selectedItem;

public:
	SelectedItemEventArgs (System::Object^ item)  { selectedItem = item; }
	property System::Object^ SelectedItem  { System::Object^ get() { return selectedItem; } }
};
