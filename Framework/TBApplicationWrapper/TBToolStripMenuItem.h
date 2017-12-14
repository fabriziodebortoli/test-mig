#pragma once

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	//-----------------------------------------------------------------------------
	/// <summary>
	/// Internal Use
	/// </summary>
	public ref class TBMenuStrip : public System::Windows::Forms::MenuStrip
	{
		System::IntPtr windowHandle;

	public:
		TBMenuStrip(System::IntPtr windowHandle)
		{
			this->windowHandle = windowHandle;
		}

		/// <summary>
		/// Internal Use
		/// </summary>
		property System::IntPtr WindowHandle
		{
			System::IntPtr get() { return windowHandle; }
		}

		/// <summary>
		/// Internal Use
		/// </summary>
		static TBMenuStrip^ FromMenuHandle(System::IntPtr windowHandle, System::IntPtr menuHandle);
	private:
		static void ParseMenu(CMenu* pMenu, System::Windows::Forms::ToolStripItemCollection ^items, TBMenuStrip^ mainMenu);
	
	};

	//-----------------------------------------------------------------------------
	/// <summary>
	/// Internal Use
	/// </summary>
	public ref class TBToolStripMenuItem : public System::Windows::Forms::ToolStripMenuItem
	{
		int commandID;
		TBMenuStrip^ ownerMenu;
		bool m_bPreviousState;
		bool m_bEnableChanged; 
		bool m_bWindowEnabled;

	public:
		TBToolStripMenuItem(int commandID, TBMenuStrip^ ownerMenu);

		/// <summary>
		/// Internal Use
		/// </summary>
		property int CommandID { int get() { return commandID; } }
		
		/// <summary>
		/// Internal Use
		/// </summary>
		property System::String^ Text { virtual void set(System::String^ value) override ; }

	protected:
		void    AssignItemEnabledState	(Microarea::Framework::TBApplicationWrapper::TBToolStripMenuItem^ menuItem);
		void	RestoreItemEnabledState	(Microarea::Framework::TBApplicationWrapper::TBToolStripMenuItem^ menuItem);
		virtual void OnClick			(System::EventArgs^ e) override;
		virtual void OnDropDownOpened	(System::EventArgs^ e) override;
	};

} } }
