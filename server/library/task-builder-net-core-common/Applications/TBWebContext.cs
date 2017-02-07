using System;
using System.Collections.Generic;

namespace Microarea.Common.Applications
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
				if (Current == null)
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
					return Current.applicationObjects[name];
				}
			}
			set
			{
				if (Current == null)
				{
					lock (applicationObjects)
						applicationObjects[name] = value;
				}
				else
				{
					Current.applicationObjects[name] = value;
				}
			}
		}

		public object FromSession(string name)
		{
			if (Current == null)
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
				return Current.SessionObjects == null ? null : Current.SessionObjects[name];
			}
		}
		public void ToSession(string name, object value)
		{
			if (Current == null)
			{
				lock (this)
					SessionObjects[name] = value;
			}
			else
			{
				Current.SessionObjects[name] = value;
			}
		}

		public string SessionID
		{
			get
			{
				if (Current == null)
				{
					return AuthenticationToken;
				}
				else
				{
					return Current.SessionID;
				}

			}
		}
	}
}
