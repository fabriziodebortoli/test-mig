using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

using Microsoft.Win32;

namespace Microarea.Library.SystemServices
{
	/// <summary>
	/// Summary description for AspnetReg.
	/// </summary>
	public class AspnetReg
	{
		/// <exception cref="FileNotFoundException">Thrown when the constructor is unable to locate aspnet_regiis.exe</exception>
		public AspnetReg(string aspnetRegExe, int siteID, string webFolderName)
		{
			if (string.IsNullOrEmpty(aspnetRegExe))
				aspnetRegExe = GetAspNetRegPath();
			if (!File.Exists(aspnetRegExe))
				throw new FileNotFoundException("Unable to locate file.", aspNetRegFileName);
			this.siteID = siteID;
			this.webFolderName = webFolderName;
			pl = new ProcessLauncher(aspnetRegExe);
			this.logicalPath = string.Format(CultureInfo.InvariantCulture,
				"W3SVC/{0}/ROOT/{1}", this.siteID, this.webFolderName);
		}

		//---------------------------------------------------------------------
		private const string aspNetRegFileName = "aspnet_regiis.exe";
		private readonly ProcessLauncher pl;
		private readonly int siteID;
		private readonly string webFolderName;
		private readonly string logicalPath;

		//---------------------------------------------------------------------
		public string Output { get { return pl.Output; } }
		public string Error { get { return pl.Error; } }

		//---------------------------------------------------------------------
		private string GetAspNetRegPath()
		{
			Assembly ass = Assembly.GetAssembly(typeof(object));
			string loc = ass.Location;
			string frameworkPath = Path.GetDirectoryName(loc);
			return Path.Combine(frameworkPath, aspNetRegFileName);
		}

		//---------------------------------------------------------------------
		public bool UnRegister()
		{
			return pl.Execute(string.Concat("-k ", this.logicalPath));
		}

		//---------------------------------------------------------------------
		public bool Register()
		{
			return pl.Execute(string.Concat("-s ", this.logicalPath));
		}

		//---------------------------------------------------------------------
		public static string GetAspNetExeFile(string frameworkVersion)
		{
			string frameworkContainer = null;
			// Framework container folder location can be found on the registry
			// on HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\InstallRoot
			string subKeyString = @"SOFTWARE\Microsoft\.NETFramework"; // here in variable for debug purposes
			using (RegistryKey rk = Registry.LocalMachine)
			{
				using (RegistryKey subKey = rk.OpenSubKey(subKeyString))
				{
					Debug.Assert(subKey != null); // actually, if this code runs, the key should always be present!
					if (subKey != null) // ...but you can never know where this code will be ported ;-)
						frameworkContainer = (string)subKey.GetValue("InstallRoot", null);
				}
			}

			if (frameworkContainer == null)
			{
				Debug.Fail("Cannot locate .NET framework container folder, but it should exists!");
				return null;
			}

			// the specific version is contained in a subfolder named "v"+frameworkVersion
			string specificFolderName = frameworkVersion;
			if (!specificFolderName.StartsWith("v"))
				specificFolderName = string.Concat('v', specificFolderName);

			string folderFullPath = Path.Combine(frameworkContainer, specificFolderName);
			if (!Directory.Exists(folderFullPath))
				return null;

			string exeFullPath = Path.Combine(folderFullPath, aspNetRegFileName);

			// now we look for the executable file existence (you can never know)
			if (!File.Exists(exeFullPath))
				return null;

			return exeFullPath;
		}

		//---------------------------------------------------------------------
	}
}
