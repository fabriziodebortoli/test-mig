using System;
using System.Web;
using System.Net;
using RESTGate.Helpers;
using RESTGate.OrganizerCore;

namespace RESTGate.Services.WebHelpers
{
    //================================================================================
    public class WebFrontLine
    {
        //--------------------------------------------------------------------------------
        public static bool ServicesFirstCall(HttpContext ctx, string token)
        {
            if (String.IsNullOrEmpty(token))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                ctx.Response.ContentType = "text/json";
                ctx.Response.Write(JSONHelper.ToJSON(new { result = "Token is missing" }));
                return false;
            }

            bool isValidToken = LivingTokens.Instance.AddToken(token);

            if (!isValidToken)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                ctx.Response.ContentType = "text/json";
                ctx.Response.Write(JSONHelper.ToJSON(new { result = "Invalid token" }));
                return false;
            }

            return true;
        }
    }
}