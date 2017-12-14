using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Interfaces;
using Newtonsoft.Json;
using Microarea.TaskBuilderNet.Core.EasyStudioServer;
using System.Threading;
using System.Globalization;

namespace Microarea.TaskBuilderNet.Core.MenuManagerLoader
{
	//---------------------------------------------------------------------------
	/// <summary>
	/// Classe usata per gestire la distruzione di un oggetto mandato col metodo SendMessageCallback
	/// </summary>
	class GarbageBag
	{
		private GCHandle handle;
		private IntPtr data;

		public MenuFunctionsDllImports.SendMessageDelegate GarbageFunction = new MenuFunctionsDllImports.SendMessageDelegate(Collect);

		public GarbageBag(IntPtr data)
		{
			this.data = data;
		}

		//-----------------------------------------------------------------------------
		public static void Collect(IntPtr hWnd, uint uMsg, IntPtr dwData, IntPtr lResult)
		{
			GCHandle handle = (GCHandle)dwData;
			GarbageBag bag = (GarbageBag)handle.Target;
			bag.Dispose();
		}

		internal IntPtr GetHandle()
		{
			if (!handle.IsAllocated)
				handle = GCHandle.Alloc(this);
			return (IntPtr)handle;
		}

		internal void Dispose()
		{
			handle.Free();
			Marshal.FreeHGlobal(data);
		}
	}
	//=======================================================================================================
	public class NewMenuFunctions
	{
		//--------------------------------------------------------------------------------
		public static void OpenRecentLink(IntPtr hwnd, string val)
		{
			SendMessageMarshal(val, hwnd, MenuFunctionsDllImports.UM_MAGO_LINKER);
		}

		//---------------------------------------------------------------------------------
		internal static string GetCustomUserPreferencesFile(IPathFinder pathFinder)
		{
			string path = pathFinder.GetCustomUserApplicationDataPath();
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			return Path.Combine(path, "preferences.bin");
		}

		//---------------------------------------------------------------------------------
		internal static string GetCustomUserMostUsedFile(IPathFinder pathFinder)
		{
			string path = pathFinder.GetCustomUserApplicationDataPath();
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			return Path.Combine(path, "mostUsed.bin");
		}

		//---------------------------------------------------------------------------
		public static void DoSSOLogOff
			(string cryptedtoken)
		{
			GenericForms.LoginFacilities lf = new GenericForms.LoginFacilities();
			lf.SSOLogOff(cryptedtoken);

		}

		//---------------------------------------------------------------------------
		public static string DoSSOLoginWeb
			(string cryptedtoken, string username, string password, string company, bool winNT, bool overwriteLogin,
			bool relogin, IntPtr menuHandle, IDatabaseCkecker dbChecker, out string jsonMessage, out bool alreadyLogged,
			out bool changePassword, out bool changeAutologinInfo, out int result, string saveAutologinInfo)
		{
			GenericForms.LoginFacilities lf = new GenericForms.LoginFacilities();
			string token = lf.SSOLogin(cryptedtoken, username, password, company, false, winNT, overwriteLogin, relogin, IntPtr.Zero, out alreadyLogged, out changePassword, out changeAutologinInfo, saveAutologinInfo, out result);



			if (string.IsNullOrEmpty(token))
			{
				SendMessageMarshal("logging", menuHandle, (uint)ExternalAPI.UM_LOGIN_INCOMPLETED);
			}
			else
			{
				if (dbChecker.Check(token))
					SendMessageMarshal(token, menuHandle, (relogin) ? (uint)ExternalAPI.UM_RELOGIN_COMPLETED : (uint)ExternalAPI.UM_LOGIN_COMPLETED);
				else
				{
					SendMessageMarshal("logging", menuHandle, (uint)ExternalAPI.UM_LOGIN_INCOMPLETED);
					lf.Diagnostic.Set(dbChecker.Diagnostic);
					lf.Logoff();
					token = "";
				}
			}
			jsonMessage = lf.Diagnostic.ToJson(false);
			return token;

			/*if (!authtoken.IsNullOrEmpty() && !dbChecker.Check(authtoken))
            {
                lf.Diagnostic.Set(dbChecker.Diagnostic);
                lf.Logoff();
                authtoken = "";
            }
            jsonMessage = lf.Diagnostic.ToJson(false);

            return authtoken;*/
		}

		//---------------------------------------------------------------------------------
		internal static string GetCustomUserHiddenTilesFile(IPathFinder pathFinder)
		{
			string path = pathFinder.GetCustomUserApplicationDataPath();
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			return Path.Combine(path, "hiddenTiles.bin");
		}

		//---------------------------------------------------------------------------------
		internal static string GetCustomUserHistoryFile(IPathFinder pathFinder)
		{
			string path = pathFinder.GetCustomUserApplicationDataPath();
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			return Path.Combine(path, "history.bin");
		}

		//---------------------------------------------------------------------------------
		internal static string GetAngularJSSafeJson(XmlDocument documMenu)
		{
			string allJson = JsonConvert.SerializeXmlNode(documMenu, Newtonsoft.Json.Formatting.None);
			return Jsonizer(allJson);
		}

		//---------------------------------------------------------------------------------
		internal static string GetAngularJSSafeJson(string allJson)
		{
			return Jsonizer(allJson);
		}

		//---------------------------------------------------------------------------------
		private static string Jsonizer(string orginal)
		{
			orginal = orginal.Replace("@", "");
			//orginal = orginal.Replace("\\", "\\\\");
			orginal = orginal.Replace("\"false\"", "false");
			orginal = orginal.Replace("\"true\"", "true");
			return orginal;
		}

		//---------------------------------------------------------------------------
		internal static XmlDocument GetCustomUserAppDataXmlDocument(string file)
		{
			XmlDocument doc = new XmlDocument();
			if (File.Exists(file))
			{
				try
				{
					doc.Load(file);
					return doc;
				}
				catch { }
			}

			XmlElement root = doc.CreateElement("Root");
			doc.AppendChild(root);
			return doc;
		}

		//---------------------------------------------------------------------------
		internal static string GetCustomUserFavoriteFile(IPathFinder pathFinder)
		{
			string path = pathFinder.GetCustomUserApplicationDataPath();
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			return Path.Combine(path, "favorites.bin");
		}



		//---------------------------------------------------------------------------
		private static void SendMessageMarshal(string val, IntPtr hwnd, uint message)
		{
			IntPtr localHGlobal = IntPtr.Zero;
			Process p = Process.GetCurrentProcess();

			byte[] bytes = Encoding.UTF8.GetBytes(val);
			localHGlobal = Marshal.AllocHGlobal(bytes.Length);
			for (int i = 0; i < bytes.Length; i++)
				Marshal.WriteByte(localHGlobal, i, bytes[i]);
			IntPtr pRemoteBuffer = IntPtr.Zero;
			try
			{
				pRemoteBuffer = MenuFunctionsDllImports.VirtualAllocEx(p.Handle, IntPtr.Zero, bytes.Length, MenuFunctionsDllImports.MEM_COMMIT | MenuFunctionsDllImports.MEM_RESERVE, MenuFunctionsDllImports.PAGE_READWRITE);
				int tot = 0;
				if (!MenuFunctionsDllImports.WriteProcessMemory(p.Handle, pRemoteBuffer, localHGlobal, bytes.Length, ref tot))
				{
					Marshal.FreeHGlobal(localHGlobal);
					return;
				}
			}
			catch
			{
				Marshal.FreeHGlobal(localHGlobal);
				return;
			}
			//devo creare un oggetto all'interno del quale faccio un Pin per evitare che il garbage collector me lo pulisca
			//la callback farà poi unpin nel metodo dispose, e rilascerà anche le risorse allocate in hglobal
			GarbageBag bag = new GarbageBag(localHGlobal);

			if (!MenuFunctionsDllImports.SendMessageCallback(hwnd, message, (IntPtr)bytes.Length, pRemoteBuffer, bag.GarbageFunction, bag.GetHandle()))
				bag.Dispose();//se fallisce la chiamata, faccio subito unpin e rilascio le risorse allocate

		}

		//---------------------------------------------------------------------------
		public static string GetMessageMarshal(IntPtr hwnd, Message m)
		{
			IntPtr localHGlobal = IntPtr.Zero;
			string result = String.Empty;

			try
			{
				int numBytes = m.WParam.ToInt32();
				localHGlobal = Marshal.AllocHGlobal(m.WParam);
				int tot = 0;
				bool b = LocalizerConnector.ReadProcessMemory(hwnd, m.LParam, localHGlobal, numBytes, ref tot);

				byte[] bytes = new byte[numBytes];
				for (int i = 0; i < numBytes; i++)
					bytes[i] = Marshal.ReadByte(localHGlobal, i);
				result = Encoding.UTF8.GetString(bytes);
			}
			catch { }
			finally
			{
				if (localHGlobal != IntPtr.Zero)
					Marshal.FreeHGlobal(localHGlobal);
			}

			return result;
		}

		//---------------------------------------------------------------------------
		public static string ChangePassword(string password)
		{
			GenericForms.LoginFacilities lf = new GenericForms.LoginFacilities();
			return lf.ChangePassword(password);
		}

		//---------------------------------------------------------------------------
		public static void ChangeTheme(string themePath, string user, string company)
		{
			DefaultTheme.ReloadTheme(themePath);

			string themeName = Path.GetFileNameWithoutExtension(themePath);

			NewMenuSaver.SetPreference("PreferredTheme", themeName, user, company);
		}

		//---------------------------------------------------------------------------
		public static string GetUserPreferredTheme(string user, string company)
		{
			return NewMenuLoader.GetPreference("PreferredTheme", user, company);
		}

		//---------------------------------------------------------------------------
		public static void SetRememberMe(string checkedVal)
		{
			GenericForms.LoginFacilities.SetRememberMe(checkedVal);

		}

		//---------------------------------------------------------------------------
		public static string GetRememberMe()
		{
			return GenericForms.LoginFacilities.GetRememberMe().ToString();
		}

		//---------------------------------------------------------------------------
		public static string IsAutoLoginable()
		{
			return GenericForms.LoginFacilities.IsAutoLoginable().ToString();
		}


		//---------------------------------------------------------------------------
		public static string DoLogin
			(string username, string password, string company, bool rememberMe, bool winNT,
			bool overwriteLogin, bool ccd, bool relogin, IntPtr menuHandle, IDatabaseCkecker dbChecker,
			out string jsonMessage, out bool alreadyLogged, out bool changePassword, out bool changeAutologinInfo, string saveAutologinInfo, out string culture, out string uiCulture)
		{
			//tolti i thread: la SendMessageMarshal adesso usa la SendMessageCallback, che è asincrona
			SendMessageMarshal("logging", menuHandle, (uint)ExternalAPI.UM_LOGGING);

			culture = uiCulture = string.Empty;
			GenericForms.LoginFacilities lf = new GenericForms.LoginFacilities();
			if (ccd)
				Functions.ClearCachedData(username);
			alreadyLogged = false;
			string token = lf.Login
				(username, password, company, rememberMe, winNT, overwriteLogin, relogin, menuHandle, out alreadyLogged, out changePassword, out changeAutologinInfo, saveAutologinInfo, out culture, out uiCulture);

			if (string.IsNullOrEmpty(token))
			{
				SendMessageMarshal("logging", menuHandle, (uint)ExternalAPI.UM_LOGIN_INCOMPLETED);
			}
			else
			{
				if (dbChecker.Check(token))
					SendMessageMarshal(token, menuHandle, (relogin) ? (uint)ExternalAPI.UM_RELOGIN_COMPLETED : (uint)ExternalAPI.UM_LOGIN_COMPLETED);
				else
				{
					SendMessageMarshal("logging", menuHandle, (uint)ExternalAPI.UM_LOGIN_INCOMPLETED);
					lf.Diagnostic.Set(dbChecker.Diagnostic);
					lf.Logoff();
					token = "";
				}
			}
			jsonMessage = lf.Diagnostic.ToJson(false);
			return token;
		}

        //---------------------------------------------------------------------------
        public static void SendCurrentToken(IntPtr menuHandle, string token)
        {
            SendMessageMarshal(token, menuHandle, (uint)ExternalAPI.UM_SEND_CURRENT_TOKEN);
        }

        //---------------------------------------------------------------------------
        public static void InitTBPostLogin(IntPtr menuHandle, string token, string username, bool clearCacheData, string culture, string uiCulture)
        {
            if (clearCacheData)
                Functions.ClearCachedData(username);

            SendMessageMarshal(token, menuHandle, (uint)ExternalAPI.UM_LOGIN_COMPLETED);

            if (!string.IsNullOrEmpty(culture))
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            if (!string.IsNullOrEmpty(uiCulture))
                Thread.CurrentThread.CurrentCulture = new CultureInfo(uiCulture);
        }

        //---------------------------------------------------------------------------
        public static string DoLoginWeb
			(string username, string password, string company, bool winNT, bool overwriteLogin,
			bool relogin, IDatabaseCkecker dbChecker, out string jsonMessage, out bool alreadyLogged,
			out bool changePassword, out bool changeAutologinInfo, string saveAutologinInfo, out string culture, out string uiCulture)
		{
			GenericForms.LoginFacilities lf = new GenericForms.LoginFacilities();
			culture = uiCulture = string.Empty;
			string token = lf.Login
				(username, password, company, false, winNT, overwriteLogin, relogin, IntPtr.Zero, out alreadyLogged, out changePassword, out changeAutologinInfo, saveAutologinInfo, out culture, out uiCulture);
			if (!token.IsNullOrEmpty() && !dbChecker.Check(token))
			{
				lf.Diagnostic.Set(dbChecker.Diagnostic);
				lf.Logoff();
				token = "";
			}
			jsonMessage = lf.Diagnostic.ToJson(false);

			return token;
		}

		//--------------------------------------------------------------------------------
		public static string GetEasyBuilderAppAssembliesPathsAsJson(string nameSpace, string user)
		{
			//string[] customizations = BasePathFinder.BasePathFinderInstance.GetEasyBuilderAppAssembliesPaths(new NameSpace(nameSpace), user, BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications);

			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			JsonWriter jsonWriter = new JsonTextWriter(sw);
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("Customizations");

			jsonWriter.WriteStartArray();

			foreach (IEasyBuilderApp app in BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications)
			{
				Dictionary<string, string> fileMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				BasePathFinder.BasePathFinderInstance.GetEasyBuilderAppAssembliesPaths(fileMap, new NameSpace(nameSpace), user, app);
				List<string> items = fileMap.Values.ToList();

				foreach (var item in items)
				{
					FileInfo fi = new FileInfo(item);
					if (fi == null || !fi.Exists)
						continue;

					DirectoryInfo di = new DirectoryInfo(fi.Directory.FullName);
					if (!di.Exists)
						continue;

					//Fix anomalia 23468: non serve controllare se ci siano i sorgenti per elencare la personalizzazione,
					//tanto se i sorgenti non ci sono EasyStudio non partira`, il controllo e` fatto a valle.
					//string srcFolder = Path.Combine(di.FullName, Path.GetFileNameWithoutExtension(fi.Name) + "_Src");
					//if (!Directory.Exists(srcFolder))
					//    continue;

					jsonWriter.WriteStartObject();
					jsonWriter.WritePropertyName("fileName");
					jsonWriter.WriteValue(item);

					jsonWriter.WritePropertyName("customizationName");
					jsonWriter.WriteValue(Path.GetFileNameWithoutExtension(fi.Name));

					jsonWriter.WritePropertyName("applicationOwner");
					jsonWriter.WriteValue(Path.GetFileNameWithoutExtension(app.ApplicationName));

					jsonWriter.WritePropertyName("moduleOwner");
					jsonWriter.WriteValue(Path.GetFileNameWithoutExtension(app.ModuleName));

					jsonWriter.WriteEndObject();
				}
			}

			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();

			jsonWriter.Close();
			sw.Close();

			return sw.ToString();
		}

		//--------------------------------------------------------------------------------
		public static void GetEasyBuilderAppAndModule(out string application, out string module)
		{
			application = "";
			module = "";
			if (BaseCustomizationContext.CustomizationContextInstance != null)
			{
				application = BaseCustomizationContext.CustomizationContextInstance.CurrentApplication;
				module = BaseCustomizationContext.CustomizationContextInstance.CurrentModule;
			}
		}

		//--------------------------------------------------------------------------------
		public static void SetAppAndModule(string app, string mod)
		{
			BaseCustomizationContext.CustomizationContextInstance.ChangeEasyBuilderApp(app, mod);
			BaseCustomizationContext.CustomizationContextInstance.CurrentApplication = app;
			BaseCustomizationContext.CustomizationContextInstance.CurrentModule = mod;
		}

		//--------------------------------------------------------------------------------
		public static void CreateNewContext(string app, string mod, string type)
		{
			IEasyBuilderApp context = null;
			ApplicationType appType;
			if (!Enum.TryParse<ApplicationType>(type, out appType))
				appType = ApplicationType.Customization;
			context = BaseCustomizationContext.CustomizationContextInstance.CreateNew(app, mod, appType);
			BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications.Add(context);
		}


		const string defaultContextApplication = "DefaultContextApplication";
		const string defaultContextModule = "DefaultContextModule";
		//--------------------------------------------------------------------------------
		public static void GetDefaultContextXml(out string app, out string mod, string user, string company)
		{
			app = mod = "";
			app = NewMenuLoader.GetPreference(defaultContextApplication, user, company);
			mod = NewMenuLoader.GetPreference(defaultContextModule, user, company);
			string directPath = Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomApplicationsPath(), app, mod);
			if (!Directory.Exists(directPath))
			{
				app = mod = "";
				//TODOROBY togliere la coppia da preferences.bin
			}
		}

		//---------------------------------------------------------------------------
		public static void SetDefaultContext(string applic, string module, string user, string company)
		{
			NewMenuSaver.SetPreference(defaultContextApplication, applic, user, company);
			NewMenuSaver.SetPreference(defaultContextModule, module, user, company);
		}

		//---------------------------------------------------------------------------
		public static bool IsDeveloperEdition()
		{
			return NewMenuLoader.isDeveloperEdition();
		}
		//---------------------------------------------------------------------------
		public static void RefreshESApps()
		{
			BasePathFinder.BasePathFinderInstance.ApplicationInfos.Clear();
			BasePathFinder.BasePathFinderInstance.RefreshEasyBuilderApps(ApplicationType.Customization);
		}
	}



	#region DllImport
	//=======================================================================================================
	class MenuFunctionsDllImports
	{

		public const int UM_MAGO_LINKER = WM_USER + 951;
		public const int WM_USER = 0x0400;
		public const int MEM_COMMIT = 0x1000;
		public const int MEM_RESERVE = 0x2000;
		public const int PAGE_READWRITE = 0x04;

		//--------------------------------------------------------------------------------
		[DllImport("kernel32.dll")]
		public static extern bool WriteProcessMemory(
			IntPtr hProcess,
			IntPtr lpBaseAddress,
			IntPtr lpBuffer,
			int nSize,
			ref int lpNumberOfBytesWritten
			);

		public delegate void SendMessageDelegate(IntPtr hWnd, uint uMsg, IntPtr dwData, IntPtr lResult);
		//--------------------------------------------------------------------------------
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam);
		//--------------------------------------------------------------------------------
		[DllImport("user32.dll")]
		public static extern bool SendMessageCallback(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam, SendMessageDelegate lpCallBack, IntPtr dwData);
		//--------------------------------------------------------------------------------
		[DllImport("user32.dll")]
		public static extern bool PostMessage(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam);

		//--------------------------------------------------------------------------------
		[DllImport("kernel32.dll")]
		public static extern IntPtr VirtualAllocEx(
			IntPtr hProcess,
			IntPtr lpAddress,
			int dwSize,
			int flAllocationType,
			int flProtect);

	}
	#endregion
}
