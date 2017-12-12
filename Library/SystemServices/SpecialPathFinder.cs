using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microarea.Library.SystemServices
{
	/// <summary>
	/// SpecialPathFinder class uses p/invoke to retrive system special folder paths.
	/// </summary>
	/// <remarks>
	/// Although System.Environment.GetFolderPath(Environment.SpecialFolder) provides similar 
	/// functionalities, paths visibility is restricted for user SYSTEM depending on operating
	/// system, installed service pack, installed security hotfixes an so on, so that often returns
	/// string.Empty and cannot be used reliably; also the Environment.SpecialFolder range is
	/// limited to a small amount of system paths.
	/// </remarks>
	public sealed class SpecialPathFinder
	{
		private SpecialPathFinder() {} // cannot instantiate it.

		//	Based on ms-help://MS.MSDNQTR.2006JAN.1033/shellcc/platform/shell/reference/functions/shgetfolderpath.htm
		//
		//	Minimum DLL Version shell32.dll version 5.0 or later 
		//	Custom Implementation No 
		//	Header shlobj.h 
		//	Import library None 
		//	Minimum operating systems Windows 95 with Internet Explorer 5.0, Windows 98 with Internet Explorer 5.0, Windows 98 Second Edition (SE), Windows NT 4.0 with Internet Explorer 5.0, Windows NT 4.0 with Service Pack 4 (SP4) 
		//	Unicode Implemented as ANSI and Unicode versions.  
		[DllImport("shell32.dll")]
		static extern int SHGetFolderPath
			(
			IntPtr hwndOwner, 
			int nFolder, 
			IntPtr hToken,
			uint dwFlags, 
			[Out] StringBuilder pszPath
			);

		public static string GetFolderPath(SpecialPathFinder.SpecialFolder folder)
		{
			StringBuilder sb = new StringBuilder();
			SHGetFolderPath(IntPtr.Zero, (int)folder, IntPtr.Zero, 0x0000, sb); // TODO process return value
			return sb.ToString();
		}

		/// <summary>
		/// Enumeration of Windows CSIDL values
		/// </summary>
		/// <remarks>
		/// based on http://msdn.microsoft.com/library/en-us/shellcc/platform/Shell/reference/enums/csidl.asp
		/// </remarks>
		public enum SpecialFolder : int
		{
			/// <summary>
			/// CSIDL_FLAG_CREATE (0x8000)
			/// Version 5.0. Combine this CSIDL with any of the following CSIDLs
			/// to force the creation of the associated folder. 
			/// </summary>
			FlagCreate					= 0x8000,

			/// <summary>
			/// CSIDL_ADMINTOOLS (0x0030)
			/// Version 5.0. The file system directory that is used to store administrative tools 
			/// for an individual user. The Microsoft Management Console (MMC) will save customized 
			/// consoles to this directory, and it will roam with the user.
			/// </summary>
			AdministrativeTools			= 0x0030,

			/// <summary>
			/// CSIDL_ALTSTARTUP (0x001d)
			/// The file system directory that corresponds to the user's nonlocalized Startup program group.
			/// </summary>
			AltStartup					= 0x001d,

			/// <summary>
			/// CSIDL_APPDATA (0x001a)
			/// Version 4.71. The file system directory that serves as a common repository for
			/// application-specific data. 
			/// A typical path is C:\Documents and Settings\username\Application Data. 
			/// This CSIDL is supported by the redistributable Shfolder.dll for systems that 
			/// do not have the Microsoft Internet Explorer 4.0 integrated Shell installed.
			/// </summary>
			ApplicationData				= 0x001a,

			/// <summary>
			/// CSIDL_BITBUCKET (0x000a)
			/// The virtual folder containing the objects in the user's Recycle Bin.
			/// </summary>
			BitBucket					= 0x000a,

			/// <summary>
			/// CSIDL_CDBURN_AREA (0x003b)
			/// Version 6.0. The file system directory acting as a staging area for files waiting to be written to CD. 
			/// A typical path is C:\Documents and Settings\username\Local Settings\Application Data\Microsoft\CD Burning.
			/// </summary>
			CDBurnArea					= 0x003b,

			/// <summary>
			/// CSIDL_COMMON_ADMINTOOLS (0x002f)
			/// Version 5.0. The file system directory containing administrative tools for all users of the computer.
			/// </summary>
			CommonAdministrativeTools	= 0x002f,

			/// <summary>
			/// CSIDL_COMMON_ALTSTARTUP (0x001e)
			/// The file system directory that corresponds to the nonlocalized Startup program group for all users. 
			/// Valid only for Microsoft Windows NT systems.
			/// </summary>
			CommonAltStartup			= 0x001e,

			/// <summary>
			/// CSIDL_COMMON_APPDATA (0x0023)
			/// Version 5.0. The file system directory containing application data for all users. 
			/// A typical path is C:\Documents and Settings\All Users\Application Data.
			/// </summary>
			CommonApplicationData		= 0x0023,

			/// <summary>
			/// CSIDL_COMMON_DESKTOPDIRECTORY (0x0019)
			/// The file system directory that contains files and folders that appear on the desktop for all users. 
			/// A typical path is C:\Documents and Settings\All Users\Desktop. Valid only for Windows NT systems.
			/// </summary>
			CommonDesktopDirectory		= 0x0019,

			/// <summary>
			/// CSIDL_COMMON_DOCUMENTS (0x002e)
			/// The file system directory that contains documents that are common to all users. 
			/// A typical paths is C:\Documents and Settings\All Users\Documents. 
			/// Valid for Windows NT systems and Microsoft Windows 95 and Windows 98 systems with Shfolder.dll installed.
			/// </summary>
			CommonDocuments				= 0x002e,

			/// <summary>
			/// CSIDL_COMMON_FAVORITES (0x001f)
			/// The file system directory that serves as a common repository for favorite items common to all users. 
			/// Valid only for Windows NT systems.
			/// </summary>
			CommonFavorites				= 0x001f,

			/// <summary>
			/// CSIDL_COMMON_MUSIC (0x0035)
			/// Version 6.0. The file system directory that serves as a repository for music files common to all users. 
			/// A typical path is C:\Documents and Settings\All Users\Documents\My Music.
			/// </summary>
			CommonMusic					= 0x0035,

			/// <summary>
			/// CSIDL_COMMON_PICTURES (0x0036)
			/// Version 6.0. The file system directory that serves as a repository for image files common to all users. 
			/// A typical path is C:\Documents and Settings\All Users\Documents\My Pictures.
			/// </summary>
			CommonPictures				= 0x0036,

			/// <summary>
			/// CSIDL_COMMON_PROGRAMS (0x0017)
			/// The file system directory that contains the directories for the common program groups that appear on the 
			/// Start menu for all users. 
			/// A typical path is C:\Documents and Settings\All Users\Start Menu\Programs. 
			/// Valid only for Windows NT systems.
			/// </summary>
			CommonPrograms				= 0x0017,

			/// <summary>
			/// CSIDL_COMMON_STARTMENU (0x0016)
			/// The file system directory that contains the programs and folders that appear on the Start menu for all users. 
			/// A typical path is C:\Documents and Settings\All Users\Start Menu. Valid only for Windows NT systems.
			/// </summary>
			CommonStartMenu				= 0x0016,

			/// <summary>
			/// CSIDL_COMMON_STARTUP (0x0018)
			/// The file system directory that contains the programs that appear in the Startup folder for all users. 
			/// A typical path is C:\Documents and Settings\All Users\Start Menu\Programs\Startup. 
			/// Valid only for Windows NT systems.
			/// </summary>
			CommonStartup				= 0x0018,

			/// <summary>
			/// CSIDL_COMMON_TEMPLATES (0x002d)
			/// The file system directory that contains the templates that are available to all users. 
			/// A typical path is C:\Documents and Settings\All Users\Templates. Valid only for Windows NT systems.
			/// </summary>
			CommonTemplates				= 0x002d,

			/// <summary>
			/// CSIDL_COMMON_VIDEO (0x0037)
			/// Version 6.0. The file system directory that serves as a repository for video files common to all users. 
			/// A typical path is C:\Documents and Settings\All Users\Documents\My Videos.
			/// </summary>
			CommonVideo					= 0x0037,

			/// <summary>
			/// CSIDL_CONTROLS (0x0003)
			/// The virtual folder containing icons for the Control Panel applications.
			/// </summary>
			Controls					= 0x0003,

			/// <summary>
			/// CSIDL_COOKIES (0x0021)
			/// The file system directory that serves as a common repository for Internet cookies.
			///  A typical path is C:\Documents and Settings\username\Cookies.
			/// </summary>
			Cookies						= 0x0021,

			/// <summary>
			/// CSIDL_DESKTOP (0x0000)
			/// The virtual folder representing the Windows desktop, the root of the namespace.
			/// </summary>
			Desktop						= 0x0000,

			/// <summary>
			/// CSIDL_DESKTOPDIRECTORY (0x0010)
			/// The file system directory used to physically store file objects on the desktop 
			/// (not to be confused with the desktop folder itself). 
			/// A typical path is C:\Documents and Settings\username\Desktop.
			/// </summary>
			DesktopDirectory			= 0x0010,

			/// <summary>
			/// CSIDL_DRIVES (0x0011)
			/// The virtual folder representing My Computer, containing everything on the local computer: 
			/// storage devices, printers, and Control Panel. The folder may also contain mapped network drives.
			/// </summary>
			Drives						= 0x0011,

			/// <summary>
			/// CSIDL_FAVORITES (0x0006)
			/// The file system directory that serves as a common repository for the user's favorite items. 
			/// A typical path is C:\Documents and Settings\username\Favorites.
			/// </summary>
			Favorites					= 0x0006,

			/// <summary>
			/// CSIDL_FONTS (0x0014)
			/// A virtual folder containing fonts. A typical path is C:\Windows\Fonts.
			/// </summary>
			Fonts						= 0x0014,

			/// <summary>
			/// CSIDL_HISTORY (0x0022)
			/// The file system directory that serves as a common repository for Internet history items.
			/// </summary>
			History						= 0x0022,

			/// <summary>
			/// CSIDL_INTERNET (0x0001)
			/// A virtual folder representing the Internet.
			/// </summary>
			Internet					= 0x0001,

			/// <summary>
			/// CSIDL_INTERNET_CACHE (0x0020)
			/// Version 4.72. The file system directory that serves as a common repository for temporary Internet files. 
			/// A typical path is C:\Documents and Settings\username\Local Settings\Temporary Internet Files.
			/// </summary>
			InternetCache				= 0x0020,

			/// <summary>
			/// CSIDL_LOCAL_APPDATA (0x001c)
			/// Version 5.0. The file system directory that serves as a data repository for local (nonroaming) applications. 
			/// A typical path is C:\Documents and Settings\username\Local Settings\Application Data.
			/// </summary>
			LocalApplicationData		= 0x001c,

			/// <summary>
			/// CSIDL_MYDOCUMENTS (0x000c)
			/// Version 6.0. The virtual folder representing the My Documents desktop item.
			/// </summary>
			MyDocuments					= 0x000c,

			/// <summary>
			/// CSIDL_MYMUSIC (0x000d)
			/// The file system directory that serves as a common repository for music files. 
			/// A typical path is C:\Documents and Settings\User\My Documents\My Music.
			/// </summary>
			MyMusic						= 0x000d,

			/// <summary>
			/// CSIDL_MYPICTURES (0x0027)
			/// Version 5.0. The file system directory that serves as a common repository for image files. 
			/// A typical path is C:\Documents and Settings\username\My Documents\My Pictures.
			/// </summary>
			MyPictures					= 0x0027,

			/// <summary>
			/// CSIDL_MYVIDEO (0x000e)
			/// Version 6.0. The file system directory that serves as a common repository for video files. 
			/// A typical path is C:\Documents and Settings\username\My Documents\My Videos.
			/// </summary>
			MyVideo						= 0x000e,

			/// <summary>
			/// CSIDL_NETHOOD (0x0013)
			/// A file system directory containing the link objects that may exist in the My Network Places virtual folder. 
			/// It is not the same as CSIDL_NETWORK, which represents the network namespace root. 
			/// A typical path is C:\Documents and Settings\username\NetHood.
			/// </summary>
			NetHood						= 0x0013,

			/// <summary>
			/// CSIDL_NETWORK (0x0012)
			/// A virtual folder representing Network Neighborhood, the root of the network namespace hierarchy.
			/// </summary>
			Network						= 0x0012,

			/// <summary>
			/// CSIDL_PERSONAL (0x0005)
			/// Version 6.0. The virtual folder representing the My Documents desktop item. 
			/// This is equivalent to CSIDL_MYDOCUMENTS. 
			/// Previous to Version 6.0. The file system directory used to physically store a user's common repository of documents. 
			/// A typical path is C:\Documents and Settings\username\My Documents. 
			/// This should be distinguished from the virtual My Documents folder in the namespace. 
			/// To access that virtual folder, use SHGetFolderLocation, which returns the ITEMIDLIST 
			/// for the virtual location, or refer to the technique described in Managing the File System.
			/// </summary>
			Personal					= 0x0005,

			/// <summary>
			/// CSIDL_PRINTERS (0x0004)
			/// The virtual folder containing installed printers.
			/// </summary>
			Printers					= 0x0004,

			/// <summary>
			/// CSIDL_PRINTHOOD (0x001b)
			/// The file system directory that contains the link objects that can exist in the Printers virtual folder. 
			/// A typical path is C:\Documents and Settings\username\PrintHood.
			/// </summary>
			PrintHood					= 0x001b,

			/// <summary>
			/// CSIDL_PROFILE (0x0028)
			/// Version 5.0. The user's profile folder. A typical path is C:\Documents and Settings\username. 
			/// Applications should not create files or folders at this level; 
			/// they should put their data under the locations referred to by CSIDL_APPDATA or CSIDL_LOCAL_APPDATA.
			/// </summary>
			Profile						= 0x0028,

			/// <summary>
			/// CSIDL_PROFILES (0x003e)
			/// Version 6.0. The file system directory containing user profile folders. 
			/// A typical path is C:\Documents and Settings.
			/// </summary>
			Profiles					= 0x003e,

			/// <summary>
			/// CSIDL_PROGRAM_FILES (0x0026)
			/// Version 5.0. The Program Files folder. A typical path is C:\Program Files.
			/// </summary>
			ProgramFiles				= 0x0026,

			/// <summary>
			/// CSIDL_PROGRAM_FILES_COMMON (0x002b)
			/// Version 5.0. A folder for components that are shared across applications. 
			/// A typical path is C:\Program Files\Common. 
			/// Valid only for Windows NT, Windows 2000, and Windows XP systems. 
			/// Not valid for Windows Millennium Edition (Windows Me).
			/// </summary>
			CommonProgramFiles			= 0x002b,

			/// <summary>
			/// CSIDL_PROGRAMS (0x0002)
			/// The file system directory that contains the user's program groups (which are themselves file system directories). 
			/// A typical path is C:\Documents and Settings\username\Start Menu\Programs. 
			/// </summary>
			Programs					= 0x0002,

			/// <summary>
			/// CSIDL_RECENT (0x0008)
			/// The file system directory that contains shortcuts to the user's most recently used documents. 
			/// A typical path is C:\Documents and Settings\username\My Recent Documents. 
			/// To create a shortcut in this folder, use SHAddToRecentDocs. 
			/// In addition to creating the shortcut, this function updates the Shell's list of recent documents and 
			/// adds the shortcut to the My Recent Documents submenu of the Start menu.
			/// </summary>
			Recent						= 0x0008,

			/// <summary>
			/// CSIDL_SENDTO (0x0009)
			/// The file system directory that contains Send To menu items. 
			/// A typical path is C:\Documents and Settings\username\SendTo.
			/// </summary>
			SendTo						= 0x0009,

			/// <summary>
			/// CSIDL_STARTMENU (0x000b)
			/// The file system directory containing Start menu items. 
			/// A typical path is C:\Documents and Settings\username\Start Menu.
			/// </summary>
			StartMenu					= 0x000b,

			/// <summary>
			/// CSIDL_STARTUP (0x0007)
			/// The file system directory that corresponds to the user's Startup program group. 
			/// The system starts these programs whenever any user logs onto Windows NT or starts Windows 95. 
			/// A typical path is C:\Documents and Settings\username\Start Menu\Programs\Startup.
			/// </summary>
			Startup						= 0x0007,

			/// <summary>
			/// CSIDL_SYSTEM (0x0025)
			/// Version 5.0. The Windows System folder. A typical path is C:\Windows\System32.
			/// </summary>
			System						= 0x0025,

			/// <summary>
			/// CSIDL_TEMPLATES (0x0015)
			/// The file system directory that serves as a common repository for document templates. 
			/// A typical path is C:\Documents and Settings\username\Templates.
			/// </summary>
			Templates					= 0x0015,

			/// <summary>
			/// CSIDL_WINDOWS (0x0024)
			/// Version 5.0. The Windows directory or SYSROOT. 
			/// This corresponds to the %windir% or %SYSTEMROOT% environment variables. 
			/// A typical path is C:\Windows.
			/// </summary>
			Windows						= 0x0024
		}
	}
}
