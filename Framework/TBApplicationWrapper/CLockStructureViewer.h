#pragma once

class CLockStructure;

namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper
{
	/// <summary>
	/// Internal use.
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class CLockStructureViewer : public System::Windows::Forms::Form
	{
		System::Collections::Generic::List<System::Drawing::Point>^ fromPointList;
		System::Collections::Generic::List<System::Drawing::Point>^ toPointList;
		System::Collections::Generic::List<System::String^>^ tooltipList;
		COLORREF lineRGB;

	public:
		CLockStructureViewer(void)
		{
			InitializeComponent();
			fromPointList = gcnew System::Collections::Generic::List<System::Drawing::Point>();
			toPointList = gcnew System::Collections::Generic::List<System::Drawing::Point>();
			tooltipList = gcnew System::Collections::Generic::List<System::String^>();
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~CLockStructureViewer()
		{
			if (components)
			{
				delete components;
			}
		}

	protected: 
		
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::Button^  btnClose;
	private: System::Windows::Forms::Panel^  lockStructurePanel;
	private: System::Windows::Forms::ToolTip^  toolTip;
	private: System::ComponentModel::IContainer^  components;



	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>


#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->components = (gcnew System::ComponentModel::Container());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->btnClose = (gcnew System::Windows::Forms::Button());
			this->lockStructurePanel = (gcnew System::Windows::Forms::Panel());
			this->toolTip = (gcnew System::Windows::Forms::ToolTip(this->components));
			this->SuspendLayout();
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(29, 10);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(61, 13);
			this->label1->TabIndex = 1;
			this->label1->Text = L"Lock graph";
			// 
			// btnClose
			// 
			this->btnClose->Anchor = System::Windows::Forms::AnchorStyles::Bottom;
			this->btnClose->AutoSize = true;
			this->btnClose->DialogResult = System::Windows::Forms::DialogResult::OK;
			this->btnClose->Location = System::Drawing::Point(251, 363);
			this->btnClose->Name = L"btnClose";
			this->btnClose->Size = System::Drawing::Size(75, 23);
			this->btnClose->TabIndex = 2;
			this->btnClose->Text = L"Close";
			this->btnClose->UseVisualStyleBackColor = true;
			// 
			// lockStructurePanel
			// 
			this->lockStructurePanel->Anchor = static_cast<System::Windows::Forms::AnchorStyles>((((System::Windows::Forms::AnchorStyles::Top | System::Windows::Forms::AnchorStyles::Bottom) 
				| System::Windows::Forms::AnchorStyles::Left) 
				| System::Windows::Forms::AnchorStyles::Right));
			this->lockStructurePanel->AutoScroll = true;
			this->lockStructurePanel->BackColor = System::Drawing::Color::MediumSpringGreen;
			this->lockStructurePanel->BorderStyle = System::Windows::Forms::BorderStyle::Fixed3D;
			this->lockStructurePanel->Location = System::Drawing::Point(32, 26);
			this->lockStructurePanel->Name = L"lockStructurePanel";
			this->lockStructurePanel->Size = System::Drawing::Size(512, 314);
			this->lockStructurePanel->TabIndex = 3;
			this->lockStructurePanel->MouseMove += gcnew System::Windows::Forms::MouseEventHandler(this, &CLockStructureViewer::lockStructurePanel_MouseMove);
			this->lockStructurePanel->Paint += gcnew System::Windows::Forms::PaintEventHandler(this, &CLockStructureViewer::lockStructurePanel_Paint);
			// 
			// CLockStructureViewer
			// 
			this->AcceptButton = this->btnClose;
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(577, 390);
			this->Controls->Add(this->lockStructurePanel);
			this->Controls->Add(this->btnClose);
			this->Controls->Add(this->label1);
			this->Name = L"CLockStructureViewer";
			this->Text = L"Lock Structure Viewer";
			this->Resize += gcnew System::EventHandler(this, &CLockStructureViewer::CLockStructureViewer_Resize);
			this->Load += gcnew System::EventHandler(this, &CLockStructureViewer::CLockStructureViewer_Load);
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

	private: 
		void CLockStructureViewer_Load(Object^  sender, System::EventArgs^  e);
		int AddNode(const CLockStructure* pStructure, CString strObject, System::Collections::Generic::Dictionary<System::String^, int>^ ht);

	private:
		void lockStructurePanel_Paint(System::Object^  sender, System::Windows::Forms::PaintEventArgs^  e) ;
	private: 
		void lockStructurePanel_MouseMove(System::Object^  sender, System::Windows::Forms::MouseEventArgs^  e);
		void ShowTooltip(System::Drawing::Point p);
		COLORREF GetPixel(System::Drawing::Point p);

		System::Void CLockStructureViewer_Resize(System::Object^  sender, System::EventArgs^  e) 
		{
			btnClose->Left = Width / 2 - btnClose->Width / 2;
		}

public:
		static property bool CanShowLockStructure
		{
			bool get();
		}
		
};
}}}
