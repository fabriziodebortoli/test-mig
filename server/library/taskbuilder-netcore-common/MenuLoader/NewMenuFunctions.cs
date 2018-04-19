using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using Newtonsoft.Json;
using TaskBuilderNetCore.Interfaces;
using static Microarea.Common.Generic.InstallationInfo;
using System.Runtime.InteropServices;
namespace Microarea.Common.MenuLoader
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
		////--------------------------------------------------------------------------------
		//public static void OpenRecentLink(IntPtr hwnd, string val)
		//{
		//	SendMessageMarshal(val, hwnd, MenuFunctionsDllImports.UM_MAGO_LINKER);
		//}

		//---------------------------------------------------------------------------------
		internal static string GetCustomUserPreferencesFile(PathFinder pathFinder)
		{
			string path = pathFinder.GetCustomUserApplicationDataPath();
			if (!pathFinder.ExistPath(path))
                pathFinder.CreateFolder(path, true);

			return Path.Combine(path, "preferences.bin");
		}

		//---------------------------------------------------------------------------------
		internal static string GetCustomUserMostUsedFile(PathFinder pathFinder)
		{
			string path = pathFinder.GetCustomUserApplicationDataPath();
			if (!pathFinder.ExistPath(path))
                pathFinder.CreateFolder(path, true);

			return Path.Combine(path, "mostUsed.bin");
		}

        //---------------------------------------------------------------------------
        public static void DoSSOLogOff
            (string cryptedtoken)
        {
            GenericForms.LoginFacilities lf = new GenericForms.LoginFacilities();
            lf.SSOLogOff(cryptedtoken);

        }

        ////---------------------------------------------------------------------------
        //public static string DoSSOLoginWeb
        //    (string cryptedtoken, string username, string password, string company, bool winNT, bool overwriteLogin,
        //    bool relogin, IntPtr menuHandle, IDatabaseCkecker dbChecker, out string jsonMessage, out bool alreadyLogged,
        //    out bool changePassword, out bool changeAutologinInfo, out int result, string saveAutologinInfo)
        //{
        //    GenericForms.LoginFacilities lf = new GenericForms.LoginFacilities();
        //    string token = lf.SSOLogin(cryptedtoken, username, password, company, false, winNT, overwriteLogin, relogin, IntPtr.Zero, out alreadyLogged, out changePassword, out changeAutologinInfo, saveAutologinInfo, out  result);

        //    if (string.IsNullOrEmpty(token))
        //    {
        //        SendMessageMarshal("logging", menuHandle, (uint)ExternalAPI.UM_LOGIN_INCOMPLETED);
        //    }
        //    else
        //    {
        //        if (dbChecker.Check(token))
        //            SendMessageMarshal(token, menuHandle, (relogin) ? (uint)ExternalAPI.UM_RELOGIN_COMPLETED : (uint)ExternalAPI.UM_LOGIN_COMPLETED);
        //        else
        //        {
        //            SendMessageMarshal("logging", menuHandle, (uint)ExternalAPI.UM_LOGIN_INCOMPLETED);
        //            lf.Diagnostic.Set(dbChecker.Diagnostic);
        //            lf.Logoff(token);
        //            token = "";
        //        }
        //    }
        //    jsonMessage = lf.Diagnostic.ToJson(false);
        //    return token;

        //    /*if (!authtoken.IsNullOrEmpty() && !dbChecker.Check(authtoken))
        //    {
        //        lf.Diagnostic.Set(dbChecker.Diagnostic);
        //        lf.Logoff();
        //        authtoken = "";
        //    }
        //    jsonMessage = lf.Diagnostic.ToJson(false);

        //    return authtoken;*/
        //} //TODOLUCA

        //---------------------------------------------------------------------------------
        internal static string GetCustomUserHiddenTilesFile(PathFinder pathFinder)
		{
			string path = pathFinder.GetCustomUserApplicationDataPath();
			if (!pathFinder.ExistPath(path))
                pathFinder.CreateFolder(path, true);

			return Path.Combine(path, "hiddenTiles.bin");
		}

		//---------------------------------------------------------------------------------
		internal static string GetCustomUserHistoryFile(PathFinder pathFinder)
		{
			string path = pathFinder.GetCustomUserApplicationDataPath();
			if (!pathFinder.ExistPath(path))
                pathFinder.CreateFolder(path, true);

			return Path.Combine(path, "history.bin");
		}

		//---------------------------------------------------------------------------------
		internal static string GetAngularJSSafeJson(XmlDocument documMenu)
		{
			string allJson = Newtonsoft.Json.JsonConvert.SerializeXmlNode(documMenu, Newtonsoft.Json.Formatting.None);
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
		internal static XmlDocument GetCustomUserAppDataXmlDocument(string file, PathFinder pathFinder)
		{
			XmlDocument doc = new XmlDocument();

			if (pathFinder.ExistFile(file))
			{
				try
				{
                    doc = pathFinder.LoadXmlDocument(doc, file);
                    return doc;
				}
				catch { }
			}

			XmlElement root = doc.CreateElement("Root");
			doc.AppendChild(root);
			return doc;		
		}

		//---------------------------------------------------------------------------
		internal static string GetCustomUserFavoriteFile(PathFinder pathFinder)
		{
			string path = pathFinder.GetCustomUserApplicationDataPath();
			if (!pathFinder.ExistPath(path))
                pathFinder.CreateFolder(path, true);

			return Path.Combine(path, "favorites.bin");
		}



		////---------------------------------------------------------------------------
		//private static void SendMessageMarshal(string val, IntPtr hwnd, uint message)
		//{
		//	IntPtr localHGlobal = IntPtr.Zero;
		//	Process p = Process.GetCurrentProcess();

		//	byte[] bytes = Encoding.UTF8.GetBytes(val);
		//	localHGlobal = Marshal.AllocHGlobal(bytes.Length);
		//	for (int i = 0; i < bytes.Length; i++)
		//		Marshal.WriteByte(localHGlobal, i, bytes[i]);
		//	IntPtr pRemoteBuffer = IntPtr.Zero;
		//	try
		//	{
		//		IntPtr pointer = p.SafeHandle.DangerousGetHandle();
		//		pRemoteBuffer = MenuFunctionsDllImports.VirtualAllocEx(pointer, IntPtr.Zero, bytes.Length, MenuFunctionsDllImports.MEM_COMMIT | MenuFunctionsDllImports.MEM_RESERVE, MenuFunctionsDllImports.PAGE_READWRITE);
		//		int tot = 0;
		//		if (!MenuFunctionsDllImports.WriteProcessMemory(pointer, pRemoteBuffer, localHGlobal, bytes.Length, ref tot))
		//		{
		//			Marshal.FreeHGlobal(localHGlobal);
		//			return;
		//		}
		//	}
		//	catch
		//	{
		//		Marshal.FreeHGlobal(localHGlobal);
		//		return;
		//	}
		//	//devo creare un oggetto all'interno del quale faccio un Pin per evitare che il garbage collector me lo pulisca
		//	//la callback farà poi unpin nel metodo dispose, e rilascerà anche le risorse allocate in hglobal
		//	GarbageBag bag = new GarbageBag(localHGlobal);

		//	if (!MenuFunctionsDllImports.SendMessageCallback(hwnd, message, (IntPtr)bytes.Length, pRemoteBuffer, bag.GarbageFunction, bag.GetHandle()))
		//		bag.Dispose();//se fallisce la chiamata, faccio subito unpin e rilascio le risorse allocate

		//}

		//TODOLUCA
		////---------------------------------------------------------------------------
		//public static string GetMessageMarshal(IntPtr hwnd, Message m)
		//{
		//	IntPtr localHGlobal = IntPtr.Zero;
		//	string result = String.Empty;

		//	try
		//	{
		//		int numBytes = m.WParam.ToInt32();
		//		localHGlobal = Marshal.AllocHGlobal(m.WParam);
		//		int tot = 0;
		//		bool b = LocalizerConnector.ReadProcessMemory(hwnd, m.LParam, localHGlobal, numBytes, ref tot);

		//		byte[] bytes = new byte[numBytes];
		//		for (int i = 0; i < numBytes; i++)
		//			bytes[i] = Marshal.ReadByte(localHGlobal, i);
		//		result = Encoding.UTF8.GetString(bytes);
		//	}
		//	catch { }
		//	finally
		//	{
		//		if (localHGlobal != IntPtr.Zero)
		//			Marshal.FreeHGlobal(localHGlobal);
		//	}

		//	return result;
		//}

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
		public static void SetRememberMe(string authenticationToken, string checkedVal)
		{
			GenericForms.LoginFacilities.SetRememberMe(authenticationToken, checkedVal);
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


  //      //---------------------------------------------------------------------------
  //      public static string DoLogin
  //          (string username, string password, string company, bool rememberMe, bool winNT, 
  //          bool overwriteLogin, bool ccd, bool relogin, IntPtr menuHandle, IDatabaseCkecker dbChecker, 
  //          out string jsonMessage, out bool alreadyLogged, out bool changePassword, out bool changeAutologinInfo, string saveAutologinInfo, out string culture, out string uiCulture)
		//{
		//	//tolti i thread: la SendMessageMarshal adesso usa la SendMessageCallback, che è asincrona
		//	SendMessageMarshal("logging", menuHandle, (uint)ExternalAPI.UM_LOGGING);

		//	culture = uiCulture = string.Empty;
		//	GenericForms.LoginFacilities lf = new GenericForms.LoginFacilities();
		//	if (ccd)
		//		Functions.ClearCachedData(username);
		//	alreadyLogged = false;
		//	string token = lf.Login
  //              (username, password, company, rememberMe, winNT, overwriteLogin, relogin, menuHandle, out alreadyLogged, out changePassword, out changeAutologinInfo, saveAutologinInfo, out culture, out uiCulture);

		//	if (string.IsNullOrEmpty(token))
		//	{
		//		SendMessageMarshal("logging", menuHandle, (uint)ExternalAPI.UM_LOGIN_INCOMPLETED);
		//	}
		//	else
		//	{
		//		if (dbChecker.Check(token))
		//			SendMessageMarshal(token, menuHandle, (relogin) ? (uint)ExternalAPI.UM_RELOGIN_COMPLETED : (uint)ExternalAPI.UM_LOGIN_COMPLETED);
		//		else
		//		{
		//			SendMessageMarshal("logging", menuHandle, (uint)ExternalAPI.UM_LOGIN_INCOMPLETED);
		//			lf.Diagnostic.Set(dbChecker.Diagnostic);
		//			lf.Logoff(token);
		//			token = "";
		//		}
		//	}
		//	jsonMessage = lf.Diagnostic.ToJson(false);
		//	return token;
		//}

        //---------------------------------------------------------------------------
  //      public static string DoLoginWeb
  //          (string username, string password, string company, bool winNT, bool overwriteLogin, 
  //          bool relogin, IDatabaseCkecker dbChecker, out string jsonMessage, out bool alreadyLogged, 
  //          out bool changePassword, out bool changeAutologinInfo, string saveAutologinInfo, out string culture, out string uiCulture)
		//{
		//	GenericForms.LoginFacilities lf = new GenericForms.LoginFacilities();
		//	culture = uiCulture = string.Empty;
		//	string token = lf.Login
  //              (username, password, company, false, winNT, overwriteLogin, relogin, IntPtr.Zero, out alreadyLogged, out changePassword, out  changeAutologinInfo, saveAutologinInfo, out culture, out uiCulture);
		//	if (!token.IsNullOrEmpty() && !dbChecker.Check(token))
		//	{
		//		lf.Diagnostic.Set(dbChecker.Diagnostic);
		//		lf.Logoff(token);
		//		token = "";
		//	}
		//	jsonMessage = lf.Diagnostic.ToJson(false);

		//	return token;
		//}      
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
