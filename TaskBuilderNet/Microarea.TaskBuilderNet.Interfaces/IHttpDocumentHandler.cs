using System;
using System.Web;
namespace Microarea.TaskBuilderNet.Interfaces
{
    public interface IHttpDocumentHandler
    {
        bool ProcessRequest(HttpContext context);

		void RunDocument(System.Web.SessionState.HttpSessionState Session, String sessionGuid, string authToken, string objectNamespace);
	}
}
