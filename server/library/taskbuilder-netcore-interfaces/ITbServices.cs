using System;
using System.Xml;

namespace TaskBuilderNetCore.Interfaces
{
    public interface ITbServices
    {
        void CloseTB(string authenticationToken);
  //      ITbLoaderClient CreateTB(PathFinder pathFinder, string authenticationToken, string companyName, ITbLoaderClient tbInterface, DateTime applicationDate, bool useRemoteServer = false);
        string[] GetData(string authenticationToken, XmlDocument paramsDoc, DateTime applicationDate, bool useApproximation);
        string GetDocumentSchema(string authenticationToken, string documentNamespace, string profileName, string forUser);
        string GetReportSchema(string authenticationToken, string reportNamespace, string forUser);
        string GetTbLoaderInstantiatedListXML(string authToken);
        //WCFBinding GetWCFBinding();
        string getXMLEnum(string authenticationToken, int enumPos, string language);
        XmlDocument GetXMLHotLink(string authenticationToken, string docNamespace, string nsUri, string fieldXPath);
        void Init();
        bool IsAlive();
        bool IsTbLoaderInstantiated(string authToken);
        void KillProcess(int processID, string authTok);
        void KillThread(int threadID, int processID, string authTok);
        void ReleaseTB(string token);
        string RunFunction(string authenticationToken, string request, string nameSpace, string functionName, out string errorMsg);
        bool SetData(string authenticationToken, string data, DateTime applicationDate, int action, out XmlDocument resDoc, bool useApproximation);
        bool SetData(string authenticationToken, string data, DateTime applicationDate, int action, out string result, bool useApproximation);
        int SetData(string authenticationToken, string data, DateTime applicationDate, int action, bool useApproximation, out string result);
        bool StopProcess(int processID, string authTok);
        bool StopThread(int threadID, int processID, string authTok);
        XmlDocument XmlGetParameters(string authenticationToken, XmlDocument paramsDoc, DateTime applicationDate, bool useApproximation);
    }
}