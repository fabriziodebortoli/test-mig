using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Permissions;

namespace Microarea.Library.LogManager.Loggables
{
	/// <summary>
	/// ErrorEventArgs.
	/// </summary>
	//=========================================================================
	public class ErrorEventArgs :
		LoggableEventArgs,
		IApplicationDescriptor,
		IProcessDescriptor,
		IMachineDescriptor,
		IDebugDescriptor
	{
		private ErrorGravity gravity = ErrorGravity.None;
		MethodBase callingMethod;

		//---------------------------------------------------------------------
		public ErrorGravity Gravity
		{
			get
			{
				return this.gravity;
			}
		}

		#region Constructors

		//---------------------------------------------------------------------
		public ErrorEventArgs()
			: this(-1, ErrorGravity.Low, string.Empty, 2)
		{}

		//---------------------------------------------------------------------
		public ErrorEventArgs(ErrorGravity gravity)
			: this(-1, gravity, string.Empty, 2)
		{}

		//---------------------------------------------------------------------
		public ErrorEventArgs(string message)
			: this(-1, ErrorGravity.Low, message, 2)
		{}

		//---------------------------------------------------------------------
		public ErrorEventArgs(int eventCode)
			: this(eventCode, ErrorGravity.Low, string.Empty, 2)
		{}

		//---------------------------------------------------------------------
		public ErrorEventArgs(ErrorGravity gravity, string message)
			: this(-1, gravity, message, 2)
		{}

		//---------------------------------------------------------------------
		public ErrorEventArgs(int eventCode, string message)
			: this(eventCode, ErrorGravity.Low, message, 2)
		{}

		//---------------------------------------------------------------------
		public ErrorEventArgs(int eventCode, ErrorGravity gravity)
			: this(eventCode, gravity, string.Empty, 2)
		{}

		//---------------------------------------------------------------------
		public ErrorEventArgs(int eventCode, ErrorGravity gravity, string message)
			: this(eventCode, gravity, message, 2)
		{}

		//---------------------------------------------------------------------
		public ErrorEventArgs(
			int eventCode,
			ErrorGravity gravity,
			string message,
			int stackTraceDepth
			)
			: base(eventCode, EventTypes.Error, message)
		{
			this.gravity = gravity;
			StackTrace st = new StackTrace();

			callingMethod = st.GetFrame(stackTraceDepth).GetMethod();

			//TODO Matteo: come recupero i valori dei parametri?
		}

		//---------------------------------------------------------------------
		protected ErrorEventArgs(ErrorEventArgs ee)
			: base(ee as LoggableEventArgs)
		{
			if (ee != null)
			{
				this.gravity = ee.gravity;
				this.callingMethod = MethodBase.GetMethodFromHandle(ee.callingMethod.MethodHandle);
			}
		}

		#endregion

		#region IApplicationDescriptor Members

		//---------------------------------------------------------------------
		public string ApplicationName
		{
			[SecurityPermission(SecurityAction.LinkDemand)]
			get
			{
				return Process.GetCurrentProcess().MainModule.ModuleName;
			}
		}

		//---------------------------------------------------------------------
		public string ApplicationPath
		{
			[SecurityPermission(SecurityAction.LinkDemand)]
			get
			{
				return Process.GetCurrentProcess().MainModule.FileName;
			}
		}

		#endregion

		#region IProcessDescriptor Members

		//---------------------------------------------------------------------
		public int ProcessId
		{
			[SecurityPermission(SecurityAction.LinkDemand)]
			get
			{
				return Process.GetCurrentProcess().Id;
			}
		}

		//---------------------------------------------------------------------
		public string ProcessName
		{
			[SecurityPermission(SecurityAction.LinkDemand)]
			get
			{
				return Process.GetCurrentProcess().ProcessName;
			}
		}

		//---------------------------------------------------------------------
		public string AccountName
		{
			get
			{
				CultureInfo aCultureInfo = CultureInfo.InvariantCulture;
				string domainName = Environment.UserDomainName;
				return String.Format(
					aCultureInfo,
					"{0}{1}",
					(domainName == null || domainName.Length == 0) ?
						String.Empty
						:
						String.Format(aCultureInfo, "{0}\\", domainName),
					Environment.UserName
					);
			}
		}

		#endregion

		#region IDebugDescriptor Members
		
		//---------------------------------------------------------------------
		public string Method
		{
			get
			{
				return this.callingMethod.Name;
			}
		}
		
		//---------------------------------------------------------------------
		public System.Collections.IDictionary Parameters
		{
			get
			{
				System.Collections.Hashtable parameters = new System.Collections.Hashtable();

				ParameterInfo[] parameterInfos = callingMethod.GetParameters();
				if (parameterInfos != null && parameterInfos.Length > 0)
					foreach (ParameterInfo parameterInfo in parameterInfos)
						parameters.Add(parameterInfo.Name, null);

				return parameters;
			}
		}

		#endregion

		#region IMachineDescriptor Members

		//---------------------------------------------------------------------
		public string OSVersion
		{
			get
			{
				try
				{
					OperatingSystem aOpSys = null;
					if ((aOpSys = Environment.OSVersion) != null)
					{
						return aOpSys.ToString();
					}
					return string.Empty;
				}
				catch (InvalidOperationException)
				{
					return string.Empty;
				}
			}
		}

		//---------------------------------------------------------------------
		public string ClrVersion
		{
			get
			{
				Version aVersion = null;
				if ((aVersion = Environment.Version) != null)
				{
					return aVersion.ToString();
				}
				return string.Empty;
			}
		}

		//---------------------------------------------------------------------
		public string MachineName
		{
			get
			{
				try
				{
					return Environment.MachineName;
				}
				catch (InvalidOperationException)
				{
					return string.Empty;
				}
			}
		}

		#endregion

		//---------------------------------------------------------------------
		public override object Clone()
		{
			return new ErrorEventArgs(this);
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public override string Serialize(IMessageFormatter formatter)
		{
			if (formatter == null)
				throw new ArgumentNullException("formatter", "Unable to serialize if 'formatter' is null");

			// Load in the formatter the base.Serialize result
			base.Serialize(formatter);

			formatter.OpenRoot("Error");
			formatter.AddElementWithValue(
				"Gravity",
				gravity
				);

			formatter.OpenElement("Process");
			formatter.AddElementWithValue("name", ProcessName);
			formatter.AddElementWithValue("id", ProcessId);
			formatter.AddElementWithValue("account", AccountName);
			formatter.CloseElement();

			formatter.OpenElement("Application");
			formatter.AddElementWithValue("name", ApplicationName);
			formatter.AddElementWithValue("path", ApplicationPath);
			formatter.CloseElement();

			formatter.OpenElement("Debug");
			formatter.AddElementWithValue("class", callingMethod.ReflectedType.FullName);
			formatter.AddElementWithValue("method", Method);

			System.Collections.ICollection parameters = Parameters;
			if (parameters != null && parameters.Count > 0)
			{
				formatter.OpenElement("parameters");
				foreach (System.Collections.DictionaryEntry entry in parameters)
					formatter.AddElementWithValue(entry.Key.ToString(), string.Empty);

				formatter.CloseElement();
			}
			else
				formatter.AddElementWithValue("parameters", "empty");

			formatter.CloseElement();

			formatter.OpenElement("Machine");
			formatter.AddElementWithValue("name", MachineName);
			formatter.AddElementWithValue("OS", OSVersion);
			formatter.AddElementWithValue("CLR", ClrVersion);
			formatter.CloseElement();

			formatter.CloseRoot();

			return formatter.ToString();
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			ErrorEventArgs e = obj as ErrorEventArgs;

			if (e == null)
				return false;

			return base.Equals(e) &&
				gravity == e.gravity &&
				callingMethod == e.callingMethod;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return
				base.GetHashCode() +
				gravity.GetHashCode() +
				(callingMethod != null ? callingMethod.GetHashCode() : 0)
				;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return base.ToString();
		}
	}

	//=========================================================================
	public enum ErrorGravity
	{
		None,
		Low,
		Medium,
		High,
		Fatal
	}
}
