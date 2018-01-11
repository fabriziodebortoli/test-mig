using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Controllers.Helpers.All
{
	//================================================================================
	public interface IHttpHelper
	{
		Task<OperationResult> PostDataAsync(string url, List<KeyValuePair<string, string>> bodyEntries, string authorizationHeader = "");
        Task<OperationResult> GetDataAsync(string url, string authorizationHeader = "");

    }

    //================================================================================
    public class HttpHelper : IHttpHelper
    {
		HttpClient client;

		//--------------------------------------------------------------------------------
		public HttpHelper()
		{
			client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

        //--------------------------------------------------------------------------------
        public async Task<OperationResult> GetDataAsync(string url,  string authorizationHeader = "")
        {
            OperationResult operationResult = new OperationResult();

            if (String.IsNullOrEmpty(url))
            {
                operationResult.Message = "Empty url is not allowed";
                return operationResult;
            }

            try
            {
                if (!string.IsNullOrEmpty(authorizationHeader))
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);
             
                HttpResponseMessage responseMessage = await client.GetAsync(url);
                var responseData = responseMessage.Content.ReadAsStringAsync();
                operationResult.Content = responseData;
                operationResult.Result = true;
            }
            catch (Exception exc)
            {
                operationResult.Code = (int)AppReturnCodes.GWAMNotResponding;
                operationResult.Result = false;
                operationResult.Message = "An error occurred while executing GetDataAsync, " + exc.Message;
            }

            return operationResult;
        }

        //--------------------------------------------------------------------------------
        public async Task<OperationResult> PostDataAsync(string url, List<KeyValuePair<string, string>> bodyEntries, string authorizationHeader = "")
		{
			OperationResult operationResult = new OperationResult();

			if (String.IsNullOrEmpty(url))
			{
				operationResult.Message = "Empty url is not allowed";
				return operationResult;
			}

            try
            {
                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);
                }
            
                var formContent = new FormUrlEncodedContent(bodyEntries);
				HttpResponseMessage responseMessage = await client.PostAsync(url, formContent);
				var responseData = responseMessage.Content.ReadAsStringAsync();
				operationResult.Content = responseData;
				operationResult.Result = true;
			}
			catch (Exception ex)
			{
				operationResult.Code = (int)AppReturnCodes.GWAMNotResponding;
				operationResult.Result = false;
				operationResult.Message = "An error occurred while executing PostDataAsync, " + ex.Message;
			}

			return operationResult;
		}
	}
}
