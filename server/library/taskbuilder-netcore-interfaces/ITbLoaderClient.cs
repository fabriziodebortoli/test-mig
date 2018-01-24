using System;

namespace TaskBuilderNetCore.Interfaces
{
    public interface ITbLoaderClient
    {
        string AuthenticationToken { get; set; }
        bool Available { get; }
        IAsyncResult BeginRunBatchInUnattendedMode(string documentNamespace, string xmlParams, AsyncCallback callback, object asyncState);
        bool BeginRunDocument(string command);
        IAsyncResult BeginRunFunction(string command, string arguments, AsyncCallback callback, object asyncState);
        IAsyncResult BeginRunXMLExportInUnattendedMode(string documentNamespace, string xmlParams, AsyncCallback callback, object asyncState);
        IAsyncResult BeginRunXMLImportInUnattendedMode(string documentNamespace, bool downloadEnvelopes, bool validateData, string xmlParams, AsyncCallback callback, object asyncState);
        bool CanChangeLogin(bool lockTbLoader);
        bool CanCloseLogin();
        bool CanCloseTB();
  //      int ChangeLogin(string oldAuthenticationToken, string newAuthenticationToken, PathFinder pathFinder, bool unlock);
        //int ChangeLogin(string oldAuthenticationToken, string newAuthenticationToken, bool unlock);
        void ClearCache();
        bool CloseDocument(int handle);
        void CloseLogin();
        void CloseTB();
        bool Connected { get; }
        void DestroyTB();
        int DisconnectCompany();
        bool EnableSoapFunctionExecutionControl(bool enable);
        bool EndRunBatchInUnattendedMode(IAsyncResult asyncResult, out int documentHandle, out string[] messages);
        bool EndRunFunction(IAsyncResult asyncResult);
        bool EndRunXMLExportInUnattendedMode(IAsyncResult asyncResult, out int documentHandle, out string[] messages);
        bool EndRunXMLImportInUnattendedMode(IAsyncResult asyncResult, out int documentHandle, out string[] messages);
        bool ExistDocument(int handle);
        DateTime GetApplicationDate();
        int GetApplicationDay();
        int GetApplicationMonth();
        int GetApplicationYear();
        bool GetCurrentUser(out string currentUser, out string currentCompany);
        bool GetData(string paramXML, bool useApproximation, string loginName, out System.Collections.Specialized.StringCollection result);
        bool GetDocumentParameters(string command, ref string xmlParameters, string code);
        string GetDocumentSchema(string documentNamespace, string profileName, string forUser);
        int[] GetDocumentThreads();
        string GetHotlinkQuery(string hotLinkNamespace, string arguments, int action);
        int GetLogins();
        int GetNrOpenDocuments();
        int GetProcessID();
        bool GetReportParameters(string command, ref string xmlParameters, string code);
        string GetReportSchema(string reportNamespace, string forUser);
        bool GetXMLExportParameters(string command, ref string xmlParameters, ref string[] messages, string code);
        string GetXMLHotLink(string documentNamespace, string nsUri, string fieldXPath, string loginName);
        bool GetXMLImportParameters(string command, ref string xmlParameters, ref string[] messages, string code);
        bool GetXMLParameters(string txXTechTempPath, bool useApproximation, string loginName, out string result);
        bool Import(int documentHandle, string envelopeFolder, ref string resultDescription);
        bool InitTbLogin();
        bool IsConnectionActive();
        bool IsLoginValid();
        bool IsTBLocked();
        bool LockTB(string authenticationToken);
        int ReconnectCompany();
        bool RunBatchInUnattendedMode(string documentNamespace, string xmlParams, ref int documentHandle, ref string[] messages);
        bool RunDocument(string command, string arguments, out int handle);
        bool RunDocument(string command, string arguments);
        bool RunDocument(string command);
        bool RunFunction(string command, string arguments);
        bool RunFunction(string command);
        bool RunReport(string command);
        bool RunReport(string command, string arguments);
        bool RunReport(string command, string arguments, out int handle);
        bool RunTextEditor(string command);
        bool RunXMLExportInUnattendedMode(string documentNamespace, string xmlParams, ref int documentHandle, ref string[] messages);
        bool RunXMLImportInUnattendedMode(string documentNamespace, bool downloadEnvelopes, bool validateData, string xmlParams, ref int documentHandle, ref string[] messages);
        void SetApplicationDate(DateTime date);
		void SetApplicationDateToSystemDate();
        bool SetData(string dataXML, int saveAction, string loginName, out string result);
        bool SetDocumentInForeground(int handle);
        void SetUserInteractionMode(int mode);
		void ShowAboutFramework();
        int TbPort { get; set; }
        System.Diagnostics.Process TBProcess { get; set; }
        IntPtr TbProcessHandle { get; }
        int TbProcessId { get; set; }
        string TbServer { get; }
        bool UnLockTB(string authenticationToken);
        void UseRemoteInterface(bool set);
        void WaitForDisconnection();
		void SetMenuHandle(IntPtr menuHandle);
		object Call(IFunctionPrototype iFunctionPrototype, object[] objs);
		string GetExtraFiltering(string[] tables, string where);
	}
}
