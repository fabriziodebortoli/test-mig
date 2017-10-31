using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microarea.Common
{
    public class AutorizationHeaderManager
    {
        public static string GetAuthorizationElement(HttpRequest request, string elementKey)
        {
            string c = string.Empty;
            string authHeader = request.Headers["Authorization"];
            if (authHeader != null)
            {
                JObject jObject = JObject.Parse(authHeader);
                c = jObject.GetValue(elementKey)?.ToString();
            }
            return c;
        }
    }
}
