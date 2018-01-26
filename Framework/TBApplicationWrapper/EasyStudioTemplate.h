#pragma once
#include "MDocument.h"

#include "MDataObj.h"
#include "MSqlRecord.h"
#include "MFormatters.h"
#include "MDocument.h"
#include "MParsedControls.h"
#include "MPanel.h"
#include "MHotLink.h"
#include "beginh.dex"

using namespace Microarea::TaskBuilderNet::Interfaces::View;
class CWndObjDescription;
class CTemplateDescription;
namespace Microarea {
	namespace Framework {
		namespace TBApplicationWrapper
		{
			//================================================================================
			public ref class EasyStudioTemplateWndCreateInfo
			{
				IWindowWrapperContainer^	parent;
				System::Type^				controlType;
				System::String^				controlClass;
				System::String^				caption;
				Point						screenLocation;

			public:
				property IWindowWrapperContainer^	Parent			{ IWindowWrapperContainer^ get(); void set(IWindowWrapperContainer^ value); }
				property System::Type^				ControlType		{ System::Type^ get(); void set(System::Type^ value); }
				property System::String^			ControlClass	{ System::String^ get(); void set(System::String^ value); }
				property System::String^			Caption			{ System::String^ get(); void set(System::String^ value); }
				property Point 						ScreenLocation	{ Point get(); void set(Point value); }

			public:
				EasyStudioTemplateWndCreateInfo();
			};

			//================================================================================
			public ref class EasyStudioTemplateWndCreateEventArgs
			{
				EasyStudioTemplateWndCreateInfo^ info;
				IWindowWrapper^ wrapper;

			public:
				EasyStudioTemplateWndCreateEventArgs(EasyStudioTemplateWndCreateInfo^ info);

			public:
				property EasyStudioTemplateWndCreateInfo^	Info { EasyStudioTemplateWndCreateInfo^ get(); }
				property IWindowWrapper^	Wrapper { IWindowWrapper^ get(); void set(IWindowWrapper^ value);  }
			};

			//=============================================================================
			public ref class EasyStudioTemplate
			{
				CTemplateDescription*	m_pDescription;
				System::String^			name;
				System::String^			fileName;

			public:
				EasyStudioTemplate(IWindowWrapperContainer^ container, System::String^ name);
				EasyStudioTemplate();
				~EasyStudioTemplate();
				!EasyStudioTemplate();

			public:
				property System::String^	Name			{ System::String^ get(); }
				property System::String^	FileName		{ System::String^ get(); }
				property bool				IsFromFile		{ bool get(); }
				property bool				IsValid			{ bool get(); }
				property System::Type^		ViewModelRootType { System::Type^ get(); }

				bool			LoadFrom		(IWindowWrapperContainer^ container);
				bool			LoadFrom		(System::String^ fileName);
				bool			Save			(bool inCustom);
				bool			Rename			(System::String^ oldName, System::String^ newName);
				IWindowWrapper^ ApplyViewModel	(IWindowWrapperContainer^ container, System::Drawing::Point screenLocation);
				bool			Delete			();

				/// <summary>
				/// Event raised when the window has to be created
				/// </summary>
				virtual event System::EventHandler<EasyStudioTemplateWndCreateEventArgs^>^ NeedCreateWindow;

			public:
				static CWndObjDescription*	CreateWndDescriptionFrom(IWindowWrapperContainer^ container);
				static bool Save(IJsonObject* pDescri, CString sfileName);

			private:
				void			CreateDescription	(IWindowWrapperContainer^ container);
				IWindowWrapper^ ApplyViewModel		(CWndObjDescription* pDescription, IWindowWrapperContainer^ container, System::Drawing::Point screenLocation);

				static CWndObjDescription*	CreateFromType(System::Type^ type);
				static System::Type^		GetTypeFromWndType(int nWndType);
				static void					AddJsonDescriptionFor(IWindowWrapperContainer^ container, CWndObjDescription* pParentDescription);
				static void					ApplyAttributesTo(IWindowWrapper^ container, CWndObjDescription* pDescription);
			};
		}
	}
}

#include "endh.dex"