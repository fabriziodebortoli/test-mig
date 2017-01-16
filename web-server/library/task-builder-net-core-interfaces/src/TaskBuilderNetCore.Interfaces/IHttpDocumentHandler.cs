using System.Net.Http;
namespace TaskBuilderNetCore.Interfaces
{
    public interface IHttpDocumentHandler
    {
        bool ProcessRequest(HttpContent context);

		//void RunDocument(System.Web.SessionState.HttpSessionState Session, String sessionGuid, string authToken, string objectNamespace);
	}
}
