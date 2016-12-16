using System;
using System.Net;
using Microsoft.ApplicationInsights.DataContracts;

namespace TaskBuilderNetCore.Interfaces
{
    public interface IHttpDocumentHandler
    {
       // bool ProcessRequest(HttpContext context);

		void RunDocument(SessionState Session, String sessionGuid, string authToken, string objectNamespace);
	}
}
