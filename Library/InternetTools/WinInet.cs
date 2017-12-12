using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Microarea.Library.Internet
{
	/// <summary>
	/// WinInet API declarations.
	/// </summary>
	internal sealed class WinInet
	{
		public const int INTERNET_OPEN_TYPE_PRECONFIG = 0;
		public const int INTERNET_OPEN_TYPE_DIRECT = 1;
		public const int INTERNET_OPEN_TYPE_PROXY = 3;
		public const short INTERNET_DEFAULT_FTP_PORT = 21;
		public const int INTERNET_SERVICE_FTP = 1;
		public const int FTP_TRANSFER_TYPE_ASCII = 0x01;
		public const int FTP_TRANSFER_TYPE_BINARY = 0x02;
		public const int GENERIC_WRITE = 0x40000000;
		public const int GENERIC_READ = unchecked((int)0x80000000);
		public const int MAX_PATH = 260;
		public const int ERROR_NO_MORE_FILES = 18;

		[DllImport("wininet.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern IntPtr InternetOpen
			(
			string lpszAgent,
			int dwAcessType,
			string lpszProxyName,
			string lpszProxyBypass,
			int dwFlags
			);

		[DllImport("wininet.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern IntPtr InternetConnect
			(
			IntPtr hInternet,
			string lpszServerName,
			short nServerPort,
			string lpszUserName,
			string lpszPassword,
			int dwService,
			int dwFlags,
			ref int dwContext
			);

		[DllImport("wininet.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool FtpGetCurrentDirectory
			(
			IntPtr hConnect,
			StringBuilder lpszCurrentDirectory,
			ref int lpdwCurrentDirectory
			);

		[DllImport("wininet.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool FtpSetCurrentDirectory
			(
			IntPtr hConnect,
			string lpszCurrentDirectory
			);

		[DllImport("wininet.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern IntPtr FtpOpenFile
			(
			IntPtr hConnect,
			string lpszFileName,
			int dwAccess,
			int dwFlags,
			out int dwContext
			);

		[DllImport("wininet.dll", SetLastError=true)]
		public static extern bool InternetWriteFile
			(
			IntPtr hFile,
			[MarshalAs(UnmanagedType.LPArray)] byte[] lpBuffer,
			int dwNumberOfBytesToWrite,
			out int lpdwNumberOfBytesWritten
			);
  
		[DllImport("wininet.dll", SetLastError=true)]
		public static extern bool InternetReadFile
			(
			IntPtr hFile,
			[MarshalAs(UnmanagedType.LPArray)] byte[] lpBuffer,
			int dwNumberOfBytesToRead,
			out int lpdwNumberOfBytesRead
			);

		[DllImport("wininet.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool InternetCloseHandle(IntPtr hInternet);

		[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError=true)]
		public static extern IntPtr FtpFindFirstFile
			(
			IntPtr hInternet, 
			string strPath, 
			[In, Out] WIN32_FIND_DATA dirData, 
			ulong ulFlags, 
			ulong ulContext //returns handle for InternetFindNextFile
			);

		[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError=true)]
		public static extern bool FtpCreateDirectory
			(
			IntPtr hInternet, 
			string strDirName
			);

		[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError=true)]
		public static extern bool FtpPutFile
			(
			IntPtr hInternet, 
			string strLocalFile, 
			string strRemoteFile, 
			ulong ulFlags, 
			ulong ulContext
			);

		[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError=true)]
		public static extern bool FtpGetFile
			(
			IntPtr hInternet, 
			string strRemoteFile, 
			string strLocalFile, 
			bool bolFailIfExist, 
			ulong ulFlags, 
			ulong ulInetFals, 
			ulong ulContext
			);

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		public class WIN32_FIND_DATA 
		{
			public int	fileAttributes = 0;
			// creationTime was embedded FILETIME structure
			public int	creationTime_lowDateTime = 0 ;
			public int	creationTime_highDateTime = 0;
			// lastAccessTime was embedded FILETIME structure
			public int	lastAccessTime_lowDateTime = 0;
			public int	lastAccessTime_highDateTime = 0;
			// lastWriteTime was embedded FILETIME structure
			public int	lastWriteTime_lowDateTime = 0;
			public int	lastWriteTime_highDateTime = 0;
			public int	nFileSizeHigh = 0;
			public int	nFileSizeLow = 0;
			public int	dwReserved0 = 0;
			public int	dwReserved1 = 0;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
			public String	fileName = null;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=14)]
			public String	alternateFileName = null;
		} //end of class FileDate

	}
}

