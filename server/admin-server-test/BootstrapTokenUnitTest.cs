using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microarea.AdminServerTest
{
    [TestClass]
    public class BootstrapTokenTests
	{
		/// <summary>
		/// In a real http use, now we shouldsend the bootstrapTokenContainer JSON-ized to the client
		///	(Content-Type: Application/JSON)
		///
		/// The client should get this:
		///			  
		///	{
		///		"ExpirationDate": "2017-10-30T11:13:15.4343372+01:00",
		///		"Result": true,
		///		"Message": "OK",
		///		"ResultCode": 0,
		///		"JwtToken": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJBY2NvdW50TmFtZSI6ImltYW56b25pQG00LmNvbSIsIkxhbmd1YWdlIjoiaXQtSVQiLCJSZWdpb25hbFNldHRpbmdzIjoiaXQtSVQiLCJVc2VyVG9rZW5zIjpbeyJBY2NvdW50TmFtZSI6ImltYW56b25pQG00LmNvbSIsIlRva2VuVHlwZSI6MSwiVG9rZW4iOiJjZmZmNjg3Yy02MDljLTRlYzUtOTVlZi1jM2U0ZWJiYzZjNzciLCJFeHBpcmVkIjpmYWxzZSwiRXhwaXJhdGlvbkRhdGUiOiIyMDE3LTExLTA2VDExOjA4OjEzLjU5OTQ5MyswMTowMCIsIklzVmFsaWQiOnRydWV9LHsiQWNjb3VudE5hbWUiOiJpbWFuem9uaUBtNC5jb20iLCJUb2tlblR5cGUiOjIsIlRva2VuIjoiZDA2N2ZjYzMtZTFkMi00ZDRhLWE3ZDYtZDljMzZlZDlmMzUyIiwiRXhwaXJlZCI6ZmFsc2UsIkV4cGlyYXRpb25EYXRlIjoiMjAxNy0xMS0wNlQxMTowODoxMy44NTYwNjQ2KzAxOjAwIiwiSXNWYWxpZCI6dHJ1ZX1dLCJTdWJzY3JpcHRpb25zIjpbeyJTdWJzY3JpcHRpb25LZXkiOiJNNCIsIkRlc2NyaXB0aW9uIjoic3Vic2NyaXB0aW9uIE00IiwiQWN0aXZhdGlvblRva2VuIjoiIiwiTGFuZ3VhZ2UiOiIiLCJSZWdpb25hbFNldHRpbmdzIjoiIiwiTWluREJTaXplVG9XYXJuIjoyMDQ0NzIzLCJVbmRlck1haW50ZW5hbmNlIjpmYWxzZSwiVGlja3MiOjEwfSx7IlN1YnNjcmlwdGlvbktleSI6Ik00LU1BTlVGIiwiRGVzY3JpcHRpb24iOiJzdWJzY3JpcHRpb24gTWFudWZhY3R1cmluZyIsIkFjdGl2YXRpb25Ub2tlbiI6IiIsIkxhbmd1YWdlIjoiIiwiUmVnaW9uYWxTZXR0aW5ncyI6IiIsIk1pbkRCU2l6ZVRvV2FybiI6MjA0NDcyMywiVW5kZXJNYWludGVuYW5jZSI6ZmFsc2UsIlRpY2tzIjoxMH0seyJTdWJzY3JpcHRpb25LZXkiOiJNNC1FTlQiLCJEZXNjcmlwdGlvbiI6InN1YnNjcmlwdGlvbiBFbnRlcnByaXNlIiwiQWN0aXZhdGlvblRva2VuIjoiIiwiTGFuZ3VhZ2UiOiIiLCJSZWdpb25hbFNldHRpbmdzIjoiIiwiTWluREJTaXplVG9XYXJuIjoyMDQ0NzIzLCJVbmRlck1haW50ZW5hbmNlIjpmYWxzZSwiVGlja3MiOjEwfSx7IlN1YnNjcmlwdGlvbktleSI6IlMtRU5UIiwiRGVzY3JpcHRpb24iOiIiLCJBY3RpdmF0aW9uVG9rZW4iOiIiLCJMYW5ndWFnZSI6IiIsIlJlZ2lvbmFsU2V0dGluZ3MiOiIiLCJNaW5EQlNpemVUb1dhcm4iOjAsIlVuZGVyTWFpbnRlbmFuY2UiOmZhbHNlLCJUaWNrcyI6LTc2NzI3ODEwNH1dLCJJbnN0YW5jZXMiOlt7Ikluc3RhbmNlS2V5IjoiSS1NNCIsIkRlc2NyaXB0aW9uIjoiaXN0YW56YSBjbG91ZCBNNCIsIkRpc2FibGVkIjpmYWxzZSwiRXhpc3RzT25EQiI6ZmFsc2UsIk9yaWdpbiI6IiIsIlRhZ3MiOiIiLCJVbmRlck1haW50ZW5hbmNlIjpmYWxzZSwiUGVuZGluZ0RhdGUiOiIyMDE3LTA5LTE3VDA5OjMyOjQ2LjQ4IiwiVGlja3MiOi03MjgxMDk3MjR9LHsiSW5zdGFuY2VLZXkiOiJJLU00LUVOVCIsIkRlc2NyaXB0aW9uIjoiaXN0YW56YSBNNCBFbnRlcnByaXNlIiwiRGlzYWJsZWQiOmZhbHNlLCJFeGlzdHNPbkRCIjpmYWxzZSwiT3JpZ2luIjoiIiwiVGFncyI6IiIsIlVuZGVyTWFpbnRlbmFuY2UiOmZhbHNlLCJQZW5kaW5nRGF0ZSI6IjIwMTctMDktMjJUMDg6MTc6MDYuNTYzIiwiVGlja3MiOi03MjgxMDk5NzB9XSwiVXJscyI6W3siSW5zdGFuY2VLZXkiOiJJLU00IiwiVVJMVHlwZSI6MCwiVVJMIjoiaHR0cDovL3Rlc3QubTRhcHAuY29tIiwiRXhpc3RzT25EQiI6ZmFsc2V9LHsiSW5zdGFuY2VLZXkiOiJJLU00IiwiVVJMVHlwZSI6MSwiVVJMIjoiaHR0cHM6Ly9hcHAubTRjbG91ZC5uZXRjb3JlLmNvbSIsIkV4aXN0c09uREIiOmZhbHNlfV0sIlJvbGVzIjpbeyJSb2xlTmFtZSI6IkFkbWluIiwiQWNjb3VudE5hbWUiOiJpbWFuem9uaUBtNC5jb20iLCJFbnRpdHlLZXkiOiJNNC1NQU5VRiIsIkxldmVsIjoiU1VCU0NSSVBUSU9OIiwiVGlja3MiOi0zODk1MDc2MjZ9LHsiUm9sZU5hbWUiOiJBZG1pbiIsIkFjY291bnROYW1lIjoiaW1hbnpvbmlAbTQuY29tIiwiRW50aXR5S2V5IjoiTTQiLCJMZXZlbCI6IlNVQlNDUklQVElPTiIsIlRpY2tzIjotMzkwMTY5ODAyfSx7IlJvbGVOYW1lIjoiQWRtaW4iLCJBY2NvdW50TmFtZSI6ImltYW56b25pQG00LmNvbSIsIkVudGl0eUtleSI6IkktTTQtRU5UIiwiTGV2ZWwiOiJJTlNUQU5DRSIsIlRpY2tzIjotMzk1NTY1NDQ5fSx7IlJvbGVOYW1lIjoiQWRtaW4iLCJBY2NvdW50TmFtZSI6ImltYW56b25pQG00LmNvbSIsIkVudGl0eUtleSI6IlMtRU5UIiwiTGV2ZWwiOiJTVUJTQ1JJUFRJT04iLCJUaWNrcyI6LTM3MjEwMDE2M31dLCJBcHBTZWN1cml0eSI6eyJBcHBJZCI6IkktTTQiLCJTZWN1cml0eVZhbHVlIjoianUyM2ZmLUtPUFAtMDkxMS1pbGEifX0=.+oSxXydUxXqAXUivJWXGSLn3i4nlcOIAOxtg0AJyfdA="
		///	}
		/// </summary>
        [TestMethod]
        public void CreateBootstrapToken()
        {
			// this is the container of the bootstrap token
			BootstrapTokenContainer bootstrapTokenContainer = new BootstrapTokenContainer();

			// this is the token, that we fill we custom properties!
			BootstrapToken bootstrapToken = new BootstrapToken();

			// setting just one token property [...]
			bootstrapToken.AccountName = "fricceri@m4.com";

			// injecting the token on its container
			bootstrapTokenContainer.SetToken(true, 0, "Token has been generated", bootstrapToken, "secret-key");

			Assert.IsNotNull(bootstrapTokenContainer.JwtToken);
		}

		[TestMethod]
		public void ValidateBootstrapToken()
		{
			// this is the container of the bootstrap token
			BootstrapTokenContainer bootstrapTokenContainer = new BootstrapTokenContainer();

			// this is the token, that we fill we custom properties!
			BootstrapToken bootstrapToken = new BootstrapToken();

			// setting just one token property [...]
			bootstrapToken.AccountName = "fricceri@m4.com";

			// preparing roles for the token
			IAccountRoles iRole = new AccountRoles();
			iRole.AccountName = "fricceri@m4.com";
			iRole.RoleName = "Admin";
			iRole.Level = "Instance";
			iRole.EntityKey = "*";
			IAccountRoles[] roles = new AccountRoles[1];
			roles[0] = iRole;
			bootstrapToken.Roles = roles;

			// injecting the token on its container
			bootstrapTokenContainer.SetToken(true, 0, "Token has been generated", bootstrapToken, "secret-key");

			// In a real scenario, now the token has been emitted to a client

			// In a real scenario, the token must be presented to the server as a credential,
			// so we need to validate it to be sure that it's not been hijacked

			// in this test, we don't make any change to the token

			OperationResult opRes = SecurityManager.ValidateToken(bootstrapTokenContainer.JwtToken, "secret-key", "Admin", "*", "Instance");

			Assert.IsTrue(opRes.Result);
		}

		[TestMethod]
		public void DetectTamperedBootstrapToken()
		{
			// this is the container of the bootstrap token
			BootstrapTokenContainer bootstrapTokenContainer = new BootstrapTokenContainer();

			// this is the token, that we fill we custom properties!
			BootstrapToken bootstrapToken = new BootstrapToken();

			// setting just one token property [...]
			bootstrapToken.AccountName = "fricceri@m4.com";

			// preparing roles for the token
			IAccountRoles iRole = new AccountRoles();
			iRole.AccountName = "fricceri@m4.com";
			iRole.RoleName = "Admin";
			iRole.Level = "Instance";
			iRole.EntityKey = "*";
			IAccountRoles[] roles = new AccountRoles[1];
			roles[0] = iRole;
			bootstrapToken.Roles = roles;

			// injecting the token on its container
			bootstrapTokenContainer.SetToken(true, 0, "Token has been generated", bootstrapToken, "secret-key");

			// In a real scenario, now the token has been emitted to a client
			// In a real scenario, the token must be presented to the server as a credential,
			// so we need to validate it to be sure that it's not been hijacked
			// In this test, we change the token, like it was tampered. We must reject it.

			// creating a tampered token by adding some dummy text to the original token
			string tamperedToken = bootstrapTokenContainer.JwtToken + "dummy string";

			OperationResult opRes = SecurityManager.ValidateToken(tamperedToken, "secret-key", "Admin", "*", "Instance");
			Assert.IsFalse(opRes.Result);
		}
	}
}
