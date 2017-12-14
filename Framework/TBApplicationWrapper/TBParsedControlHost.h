#pragma once

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{

	/// <summary>
	/// Internal Use
	/// </summary>
	public ref class TBParsedControlHost : public System::Windows::Forms::Panel
	{
	public:
		TBParsedControlHost(void)
		{
		
		}
	protected:
		virtual void WndProc(System::Windows::Forms::Message% m) override ;
	};
}
}
}
