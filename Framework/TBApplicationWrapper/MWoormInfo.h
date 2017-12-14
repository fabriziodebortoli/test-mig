#pragma once

using namespace Microarea::TaskBuilderNet::Interfaces::Model;
using namespace Microarea::TaskBuilderNet::Core::EasyBuilder;
using namespace Microarea::TaskBuilderNet::Core::Localization;

class CWoormInfo;
class CWoormDoc;
class CMapiMessage;
class CDiagnostic;
namespace Microarea { namespace Framework	{ namespace TBApplicationWrapper 
{
	//===========================================================================
	public ref class MEMailMessage
	{
		CMapiMessage*	m_pMail;
		CDiagnostic*	m_pDiagnostic;
		bool			deliveryNotification;
		bool			readNotification;
		bool			hasCodeBehind;
		System::Collections::Generic::IList<System::String^>^ tos;
		System::Collections::Generic::IList<System::String^>^ ccs;
		System::Collections::Generic::IList<System::String^>^ bccs;
		System::Collections::Generic::IList<System::String^>^ attachments;
		System::Collections::Generic::IList<System::String^>^ attachmentsTitles;

	internal:
		MEMailMessage(CMapiMessage*, CDiagnostic* pDiagnostic);
		
	public:
		/// <summary>
		///	Constracts a new mail message
		/// </summary>
		MEMailMessage();

		/// <summary>
		///	Distructor of a mail message
		/// </summary>
		~MEMailMessage();

		/// <summary>
		///	Finalize a mail message
		/// </summary>
		!MEMailMessage();

		/// <summary>
		///	Email Subject
		/// </summary>
		property System::String^ Subject { System::String^ get(); void set(System::String^ value); }
		/// <summary>
		///	Email Body
		/// </summary>
		property System::String^ Body { System::String^ get(); void set(System::String^ value); }
		/// <summary>
		///	Email From
		/// </summary>
		property System::String^ From { System::String^ get(); void set(System::String^ value); }
		/// <summary>
		///	Email Identity
		/// </summary>
		property System::String^ Identity { System::String^ get(); void set(System::String^ value); }
		/// <summary>
		///	Email From Name
		/// </summary>
		property System::String^ FromName { System::String^ get(); void set(System::String^ value); }
		/// <summary>
		///	Email From Name
		/// </summary>
		property System::String^ ReplyTo { System::String^ get(); void set(System::String^ value); }

		/// <summary>
		///	The list of email receivers (To)
		/// </summary>
		property System::Collections::Generic::IList<System::String^>^ Tos { System::Collections::Generic::IList<System::String^>^ get(); }
		/// <summary>
		///	The list of email c/c (Cc)
		/// </summary>
		property System::Collections::Generic::IList<System::String^>^ Ccs { System::Collections::Generic::IList<System::String^>^ get(); }
		/// <summary>
		///	The list of email hidden c/c (Bcc)
		/// </summary>
		property System::Collections::Generic::IList<System::String^>^ Bccs { System::Collections::Generic::IList<System::String^>^ get(); }
		/// <summary>
		///	The list of email attachments file names
		/// </summary>
		property System::Collections::Generic::IList<System::String^>^ Attachments { System::Collections::Generic::IList<System::String^>^ get(); }
		/// <summary>
		///	The list of email attachments titles
		/// </summary>
		property System::Collections::Generic::IList<System::String^>^ AttachmentsTitles { System::Collections::Generic::IList<System::String^>^ get(); }
		/// <summary>
		///	The list of email attachments titles
		/// </summary>
		property System::String^ MapiProfile { System::String^ get(); void set (System::String^); }
		/// <summary>
		///	Get and sets read notification
		/// </summary>
		property bool ReadNotification { bool get(); void set (bool value); }
		/// <summary>
		///	Get and sets delivery notification
		/// </summary>
		property bool DeliveryNotification { bool get(); void set (bool value); }

	public:
		bool Send();
	};

	ref class MDataObj;

	//===========================================================================
	public ref class MWoormInfo : IWoormInfo
	{
		CWoormInfo*				m_pInfo;
		CObject*				m_pReportDocs;
		bool					hasCodeBehind;
		IDocumentDataManager^	context;
		MEMailMessage^			mail;
		System::Collections::Generic::IList<System::String^>^ reports;
		System::Collections::Generic::IList<System::String^>^ outputFiles;
	
	public:
		enum class PrintDialogUI	{ Default = 0, ShowBeforeRunning = 1, OnlyOne = 2, NotShown = 3  };
		enum class ReportUI			{ Default = 0, Hidden = 1, Minimized = 2  };

		enum class FileFormats		{	None = 0, Pdf, Word , Excel, Html, Csv, Xml, 
										ExcelNet, OpenOfficeSheet, WordNet, OpenOfficeDoc, 
										Json, OpenXmlExcel, ReportXml };

		enum class PrintStatus		{ Undefined = 0,  Executed = 1, Aborted = 2 };

	public:
		MWoormInfo	();
		MWoormInfo	(System::IntPtr ptrInfo);
		!MWoormInfo();
		virtual ~MWoormInfo(void);
		/// <summary>
		///	Add a couple name/value in the report engine bag; the name has to correspond to a report variable, so as to initialize it
		/// </summary>
		void AddParameter(System::String^ name, MDataObj^ value);
		/// <summary>
		///	Get the value of the parameter, given its name, only if it's an input parameter
		/// </summary>
		virtual Object^ GetInputParamValue(System::String^ name);
		/// <summary>
		///	Get the names of all input parameters
		/// </summary>
		virtual cli::array<System::String^>^ GetInputParamNames();
		/// <summary>
		///	The list of report namespaces (usually one) than are to be runned
		/// </summary>
		property System::Collections::Generic::IList<System::String^>^ Reports { System::Collections::Generic::IList<System::String^>^ get(); }
		/// <summary>
		///	Number of copies to print
		/// </summary>
		property int Copies { void set(int value); }
		/// <summary>
		///	Number of copies to print
		/// </summary>
		property bool UseMultiCopy { bool get(); void set(bool value); }
		/// <summary>
		///	Behaviour related to the print dialog
		/// </summary>
		property PrintDialogUI PrintDialog { PrintDialogUI get(); void set(PrintDialogUI value); }
		/// <summary>
		///	The report is automatically closed after print has been executed
		/// </summary>
		property bool CloseAfterPrint { bool get(); void set(bool value); }
		/// <summary>
		///	The report is automatically sent to the printer without user interface
		/// </summary>
		property bool AutoPrint { bool get(); void set(bool value); }
		/// <summary>
		///	Printer name to which send report
		/// </summary>
		property System::String^ PrinterName { System::String^ get(); void set(System::String^ value); }
		/// <summary>
		/// Defines the UI type related to report
		/// </summary>
		property ReportUI UI { ReportUI get(); void set(ReportUI value); }
		/// <summary>
		///	Generates file in output 
		/// </summary>
		property FileFormats GeneratesFile { FileFormats get(); void set (FileFormats value); }
		/// <summary>
		///	List of automatically generated output files names
		/// </summary>
		property System::Collections::Generic::IList<System::String^>^ OutputFiles { System::Collections::Generic::IList<System::String^>^  get(); }
		/// <summary>
		///	The report generate attachments
		/// </summary>
		property FileFormats AttachmentsFormat { FileFormats get(); void set(FileFormats value); }
		/// <summary>
		///	Attachments are compressed 
		/// </summary>
		property bool AttachmentsCompressed { bool get(); void set(bool value); }
		/// <summary>
		///	Sends an email 
		/// </summary>
		property bool SendMail { bool get(); void set(bool value); }
		/// <summary>
		///	Shows email UI before sending it
		/// </summary>
		property bool ShowMailUI { bool get(); void set(bool value); }
		/// <summary>
		///	Email message structure
		/// </summary>
		property MEMailMessage^ EMail { MEMailMessage^ get(); }
		/// <summary>
		///	Indicates print status once executed
		/// </summary>
		property PrintStatus Status { PrintStatus get(); }
		
		/// <summary>
		///	Indicates print status once executed
		/// </summary>
		property IDocumentDataManager^ Context { IDocumentDataManager^ get(); void set(IDocumentDataManager^ value); }

		/// <summary>
		///	Run added reports
		/// </summary>
		void RunReports	();
		/// <summary>
		///	Close added reports
		/// </summary>
		void CloseReports();

	private:
		FileFormats	ConvertToFileFormats();
		void		ConvertToExportInfo	(FileFormats ff);

	};

}}}