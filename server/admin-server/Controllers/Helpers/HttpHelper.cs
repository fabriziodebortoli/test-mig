﻿using Microarea.AdminServer.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Controllers.Helpers
{
	//================================================================================
	public interface IHttpHelper
	{
		Task<OperationResult> PostDataAsync(string url, List<KeyValuePair<string, string>> bodyEntries, string authorizationHeader = "");
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
