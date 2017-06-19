using Microarea.AdminServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Controllers.Helpers
{
	//================================================================================
	public class HttpHelper
    {
		HttpClient client;

		//--------------------------------------------------------------------------------
		public HttpHelper()
		{
			client = new HttpClient();
		}

		//--------------------------------------------------------------------------------
		public async Task<OperationResult> PostDataAsync(string url, List<KeyValuePair<string, string>> bodyEntries)
		{
			OperationResult operationResult = new OperationResult();

			if (String.IsNullOrEmpty(url))
			{
				operationResult.Message = "Empty url is not allowed";
				return operationResult;
			}

			try
			{
				var formContent = new FormUrlEncodedContent(bodyEntries);
				HttpResponseMessage responseMessage = await client.PostAsync(url, formContent);
				var responseData = responseMessage.Content.ReadAsStringAsync();
				operationResult.ObjectResult = responseData;
			}
			catch (Exception ex)
			{
				operationResult.Result = false;
				operationResult.Message = "An error occurred while executin PostDataAsync, " + ex.Message;
			}

			return operationResult;
		}
	}
}



