using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TBMobile
{
	public class ErrorEventArgs
	{
		internal ErrorEventArgs(ResponseEventArgs args)
		{
			this.ErrorMessage = args.Message;
			this.Code = args.Code;
			this.ServerResponse = args.ResponseText;
		}
		public string ErrorMessage {
			get;
			set;
		}
		public string ServerResponse
		{
			get;
			set;
		}
		public int Code
		{
			get;
			set;
		}

	}
	public class LoginContext
	{
		private HTTPConnector connector;
		private bool logged = false;
		private string user = "";
		private string company = "";
		public string User { get { return user; }}  
		public string Company { get { return company; }} 

		public event EventHandler LoggedIn;
		public event EventHandler LoggedOff;
		public event EventHandler<ErrorEventArgs> Error;

		public bool Logged
		{
			get { return logged; }
		}
		internal HTTPConnector Connector
		{
			get { return connector; }
		}
		public LoginContext()
		{
			connector = new HTTPConnector();
		}
		private void OnLoginResponse(ResponseEventArgs args)
		{

			JObject o = args.ResponseObject;

			if (args.Success)
			{
				logged = true;
				if (LoggedIn != null)
					LoggedIn(this, EventArgs.Empty);
				return;
			}
			else
			{
				user = company = string.Empty;
			}
			if (Error != null)
			{
				ErrorEventArgs a = new ErrorEventArgs(args);
				Error(this, a);
			}
		}
		private void OnLogoffResponse(ResponseEventArgs args)
		{
				JObject o = args.ResponseObject;
				if (args.Success)
				{
					logged = false;
					if (LoggedOff != null)
						LoggedOff(this, EventArgs.Empty);
					return;
				}
			if (Error != null) {
				ErrorEventArgs a = new ErrorEventArgs(args);
				
				Error (this, a);
			}
		}
		public void Login(string user, string company, string pwd, bool overwrite)
		{
			this.user = user;
			this.company = company;
			connector.Login(user, company, pwd, overwrite, OnLoginResponse);
		}
		public void Logoff()
		{
			connector.Logoff(OnLogoffResponse);
		}

	}
}
