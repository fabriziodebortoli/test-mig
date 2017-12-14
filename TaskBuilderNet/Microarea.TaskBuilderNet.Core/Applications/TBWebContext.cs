using System;
using System.Collections.Generic;
using System.Web;

namespace Microarea.TaskBuilderNet.Core.Applications
{
	public class TBWebContext : IApplicationBag
	{
		[ThreadStatic]
		public string AuthenticationToken = "";
		public static TBWebContext Current = new TBWebContext();
		private Dictionary<string, object> applicationObjects = new Dictionary<string, object>();
		private Dictionary<string, Dictionary<string, object>> sessions = new Dictionary<string, Dictionary<string, object>>();
		
		private Dictionary<string, object> SessionObjects 
		{
			get
			{
				lock (this)
				{
					Dictionary<string, object> so = null;
					if (!sessions.TryGetValue(AuthenticationToken, out so))
					{
						so = new Dictionary<string, object>();
						sessions[AuthenticationToken] = so;
					}
					return so;
				}
			}
		}
			
		public object this[string name]
		{
			get
			{
				if (HttpContext.Current == null)
				{
					lock (applicationObjects)
					{
						object o = null;
						applicationObjects.TryGetValue(name, out o);
						return o;
					}
				}
				else
				{
					return HttpContext.Current.Application[name];
				}
			}
			set
			{
				if (HttpContext.Current == null)
				{
					lock (applicationObjects)
						applicationObjects[name] = value;
				}
				else
				{
					HttpContext.Current.Application[name] = value;
				}
			}
		}

		public object FromSession(string name)
		{
			if (HttpContext.Current == null)
			{
				lock (this)
				{
					object o = null;
					SessionObjects.TryGetValue(name, out o);
					return o;
				}
			}
			else
			{
				return HttpContext.Current.Session == null ? null : HttpContext.Current.Session[name];
			}
		}
		public void ToSession(string name, object value)
		{
			if (HttpContext.Current == null)
			{
				lock (this)
					SessionObjects[name] = value;
			}
			else
			{
				HttpContext.Current.Session[name] = value;
			}
		}

		public string SessionID
		{
			get
			{
				if (HttpContext.Current == null)
				{
					return AuthenticationToken;
				}
				else
				{
					return HttpContext.Current.Session.SessionID;
				}

			}
		}
	}
}
