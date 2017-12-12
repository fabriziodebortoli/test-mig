using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microarea.Library.Internet
{
	//=========================================================================
	[Flags()]
	public enum AccessMode
	{
		Read = WinInet.GENERIC_READ,
		Write = WinInet.GENERIC_WRITE, 
	}

	//=========================================================================
	public enum TransferMode
	{
		Ascii = WinInet.FTP_TRANSFER_TYPE_ASCII,
		Binary = WinInet.FTP_TRANSFER_TYPE_BINARY,
	}

	/// <summary>
	/// Semplice FTP client che incapsula le chiamate alle API di WinInet.dll
	/// </summary>
	//=========================================================================
	public sealed class FtpClient : IDisposable
	{
		private IntPtr internet;
		private IntPtr connection;
		private IntPtr fileHandle;
		private int context;

		private const int BUFFER_SIZE = 4096;
		private WinInet.WIN32_FIND_DATA findData = new WinInet.WIN32_FIND_DATA();

		private readonly string	host;
		private readonly short	port;
		private readonly string	userName;
		private readonly string	password;

		private bool aborted = false;

		//---------------------------------------------------------------------
		public FtpClient(string host, string userName, string password)
		{
			this.host		= host;
			this.port		= WinInet.INTERNET_DEFAULT_FTP_PORT;
			this.userName	= userName;
			this.password	= password;

			internet = WinInet.InternetOpen
				(
				null,
				WinInet.INTERNET_OPEN_TYPE_DIRECT,
				null,
				null,
				0
				);

			if (internet == IntPtr.Zero)
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		//---------------------------------------------------------------------
		public void Connect()
		{
			if (this.aborted)
				return;

			connection = WinInet.InternetConnect
				(
				this.internet,
				host,
				port,
				userName,
				password,
				WinInet.INTERNET_SERVICE_FTP,
				0,
				ref this.context
				);

			if (connection == IntPtr.Zero)
			{
				WinInet.InternetCloseHandle(this.internet);
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		#region distruttore e housekeeping
		//---------------------------------------------------------------------
		~FtpClient()
		{
			this.CleanUp();
		}
		//---------------------------------------------------------------------
		public void Close()
		{
			((IDisposable)this).Dispose();
		}

		//---------------------------------------------------------------------
		void IDisposable.Dispose()
		{
			this.CleanUp();
			GC.SuppressFinalize(this);
		}

		//---------------------------------------------------------------------
		private void CleanUp()
		{
			if (this.fileHandle != IntPtr.Zero)
				WinInet.InternetCloseHandle(this.fileHandle);

			if (this.connection != IntPtr.Zero)
				WinInet.InternetCloseHandle(this.connection);

			if (this.internet != IntPtr.Zero)
				WinInet.InternetCloseHandle(this.internet);

			// TODO - mettere a null gli eventi
		}
		#endregion

		#region CurrentDirectory
		/// <summary>
		/// Ritorna la current directory remota espressa in modo assoluto,
		/// es. /dir1/dir2
		/// </summary>
		/// <remarks>
		/// il metodo non è esattamente il duale del _get sottostante, perché nel _get
		/// il path può essere sia relativo che assoluto (per questo il nome è leggermente
		/// diverso) e pertanto non ho fatto dei due metodi un'unica property.
		/// il carattere di separazione da usare è lo slash ('/') come da specifiche FTP
		/// </remarks>
		/// <returns>current directory remota espressa in modo assoluto</returns>
		//---------------------------------------------------------------------
		public string GetAbsoluteCurrentDirectory()
		{
			StringBuilder path = new StringBuilder(260);
			int bufferSize = path.Capacity;

			if (!WinInet.FtpGetCurrentDirectory(this.connection, path, ref bufferSize))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			return path.ToString();
		}

		/// <summary>
		/// Imposta la current directory remota
		/// </summary>
		/// <remarks>
		/// il path può essere sia relativo che assoluto.
		/// il carattere di separazione da usare è lo slash ('/') come da specifiche FTP
		/// </remarks>
		/// <param name="path">current directory server FTP remoto (assoluto o relativo)</param>
		//---------------------------------------------------------------------
		public void SetCurrentDirectory(string path)
		{
			if (!WinInet.FtpSetCurrentDirectory(this.connection, path))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		#endregion

		//---------------------------------------------------------------------
		public void Abort()
		{
			this.aborted = true;
		}

		//---------------------------------------------------------------------
		public bool Aborted	{ get { return this.aborted; }}

		//---------------------------------------------------------------------
		private void OpenFile(string fileName, AccessMode access, TransferMode mode)
		{
			this.fileHandle = WinInet.FtpOpenFile(this.connection, fileName, (int) access, (int) mode, out this.context);
			if (this.fileHandle == IntPtr.Zero)
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		//---------------------------------------------------------------------
		private void CloseFile()
		{
			if (this.fileHandle != IntPtr.Zero)
			{
				if (WinInet.InternetCloseHandle(this.fileHandle))
					this.fileHandle = IntPtr.Zero;
				else
					throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		//---------------------------------------------------------------------
		private int WriteFile(string buffer)
		{
			byte[] bytes = new ASCIIEncoding().GetBytes(buffer);
			return this.WriteFile(bytes);
		}

		//---------------------------------------------------------------------
		private int WriteFile(byte[] buffer)
		{
			int byteCount;
			if (!WinInet.InternetWriteFile(this.fileHandle, buffer, buffer.Length, out byteCount))
				throw new Win32Exception(Marshal.GetLastWin32Error());

			return byteCount;
		}

		//---------------------------------------------------------------------
		private bool ReadFile(out string buffer, out int bytesRead)
		{
			// clear the buffer...
			buffer = string.Empty;

			// read from the file
			byte[] readBuffer = new byte[BUFFER_SIZE];
			bool success = WinInet.InternetReadFile(this.fileHandle, readBuffer, readBuffer.Length, out bytesRead);

			// the call failed!
			if (!success)
				throw new Win32Exception(Marshal.GetLastWin32Error());
   
			// we got some data, so convert it for the return...
			if (bytesRead != 0)
				buffer = Encoding.ASCII.GetString(readBuffer, 0, bytesRead);

			return (bytesRead != 0) ? true : false;
		}

		//---------------------------------------------------------------------
		private bool ReadFile(byte[] buffer, out int bytesRead)
		{
			bool success = WinInet.InternetReadFile(this.fileHandle, buffer, buffer.Length, out bytesRead);
			if (!success)
				throw new Win32Exception(Marshal.GetLastWin32Error());
			return (bytesRead != 0) ? true : false;
		}

		//---------------------------------------------------------------------
		public void CreateDirectory(string dirName)
		{
			if (!WinInet.FtpCreateDirectory(this.connection, dirName))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		//---------------------------------------------------------------------
		public void SimpleUploadFile(string strLocalFile, string strRemoteFile)
		{
			if (!WinInet.FtpPutFile(this.connection, strLocalFile, strRemoteFile, 0, 0))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		//---------------------------------------------------------------------
		public void SimpleDownloadFile(string strLocalFile, string strRemoteFile)
		{
			if (!WinInet.FtpGetFile(this.connection, strRemoteFile, strLocalFile, false, 0, 0, 0))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		//---------------------------------------------------------------------
		public bool Download(string sLocal, string sRemote, TransferMode transferMode)
		{
			if (this.aborted)
				return false;

			if (transferMode == TransferMode.Ascii)
				return DownloadAscii(sLocal, sRemote);
			else
				return DownloadBinary(sLocal, sRemote);
		}

		//---------------------------------------------------------------------
		private bool DownloadBinary(string sLocal, string sRemote)
		{
			int size = GetFileSize(sRemote);	// mi serve solo per gli eventi

			byte[] buffer = new byte[BUFFER_SIZE];
			int bytesRead = 0;
			int sum = 0;
			
			sLocal = sLocal.Trim();
			sRemote = sRemote.Trim();
			
			if (sLocal.Length == 0 || sRemote.Length == 0)
				return false;

			OpenFile(sRemote, AccessMode.Read, TransferMode.Binary);

			DateTime utcStartTime = DateTime.UtcNow;

			FileStream fs = new FileStream(sLocal, FileMode.Create);
			BinaryWriter w = new BinaryWriter(fs);

			if (size > 0)
			{
				int prevProgress = 0;	// percentuale moltiplicata per 100 (troncata)
				while (!this.aborted && ReadFile(buffer, out bytesRead))
				{
					w.Write(buffer, 0, bytesRead);
					sum += bytesRead;
					if (sum == size || ProgressDeserveNotification(sum, size, ref prevProgress))
						OnPercentDownload(sRemote, sum, size, utcStartTime);
				}
			}
			
			w.Close();
			fs.Close();
			CloseFile();

			if (this.aborted)
				return false;

			return true;
		}

		//---------------------------------------------------------------------
		private bool DownloadAscii(string sLocal, string sRemote)
		{
			int size = GetFileSize(sRemote);	// mi serve solo per gli eventi

			string data;
			int bytesRead = 0;
			int sum = 0;
			
			sLocal = sLocal.Trim();
			sRemote = sRemote.Trim();
			
			if (sLocal.Length == 0 || sRemote.Length == 0)
				return false;

			OpenFile(sRemote, AccessMode.Read, TransferMode.Ascii);

			DateTime utcStartTime = DateTime.UtcNow;

			FileStream fs = new FileStream(sLocal, FileMode.Create);
			StreamWriter w = new StreamWriter(fs);
			w.BaseStream.Seek(0, SeekOrigin.End);
			
			if (size > 0)
			{
				int prevProgress = 0;	// percentuale moltiplicata per 100 (troncata)
				while (!this.aborted && ReadFile(out data, out bytesRead))
				{
					w.Write(data);
					sum += bytesRead;
					if (sum == size || ProgressDeserveNotification(sum, size, ref prevProgress))
						OnPercentDownload(sRemote, sum, size, utcStartTime);
				}
			}
			
			w.Close();
			fs.Close();
			CloseFile();

			
			if (this.aborted)
				return false;
			return true;
		}

		//---------------------------------------------------------------------
		public int GetFileSize(string fileName)
		{
			IntPtr hFind;

			// FTP non supporta nomi con spazi, come workaround si usa wildcard
			string searchMask = fileName.Replace(' ', '?');

			hFind = WinInet.FtpFindFirstFile(this.connection, searchMask, findData, 0, 0);
			if (hFind == IntPtr.Zero)
			{
				int lastError = Marshal.GetLastWin32Error();
				if (lastError == WinInet.ERROR_NO_MORE_FILES)	// File not found
					throw new FileNotFoundException("File not found on FTP server.", fileName);
				if (lastError != 0)
					throw new Win32Exception(lastError);
			}
			string foundName = findData.fileName;
			if (string.Compare(fileName, foundName, true, CultureInfo.InvariantCulture) != 0)
			{
				// ha trovato un nome diverso che corrisponde alla maschera
				// (abbiamo sostituito i blank con degli '?')
				// occorrerebbe reiterare
				Debug.Fail("il file trovato è errato!");
				throw new FileNotFoundException("File non trovato, ma potrebbe essere presente", fileName);
				// TODO - implementare iterazione della chiamata api
			}

			WinInet.InternetCloseHandle(hFind);
			return findData.nFileSizeLow;
		}

		//---------------------------------------------------------------------
		// TODO - InternetGetConnectedState, UploadAscii, UploadBinary

		#region helper
		//---------------------------------------------------------------------
		private FtpPercentEventArgs GetProgressEvent
			(
			string fileName, 
			int bytesTransferred, 
			int fileSize,
			DateTime startTime
			)
		{
			Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "Downloading file {0} ({1} bytes of {2}", fileName, bytesTransferred, fileSize));
			FtpPercentEventArgs e = new FtpPercentEventArgs
				(
				this.host,
				this.port,
				bytesTransferred,
				fileSize,
				startTime,
				fileName
				);
			return e;
		}

		//---------------------------------------------------------------------
		private bool ProgressDeserveNotification
			(
			int bytesTransferred, 
			int fileSize,
			ref int prevValue
			)
		{
			// deve giudicare se il progresso rispetto al valore precedente è sufficiente
			// a scatenare un evento di progress
			// deve essere molto efficiente, quindi evitare calcoli in float

			//	NO: float percentage = (float)bytesTransferred / fileSize;
			//	SI: int percentage = (int)((100 * (long)bytesTransferred) / fileSize);
			int percentage = FtpPercentEventArgs.GetPercent(bytesTransferred, fileSize);
			
			if (percentage - prevValue >= 1)	// accetto il dettaglio di 1%
			{
				prevValue = percentage;
				Debug.Write(percentage + " ");
				return true;
			}
			return false;
		}
		#endregion

		#region events
		public event FtpEventHandler PercentDownload;

		//---------------------------------------------------------------------
		public void OnPercentDownload
			(
			string fileName, 
			int bytesTransferred, 
			int fileSize,
			DateTime startTime
			)
		{
			FtpPercentEventArgs e = GetProgressEvent
				(
				fileName, 
				bytesTransferred, 
				fileSize,
                startTime
				);

			if (PercentDownload != null)
				PercentDownload(this, e);
		}

		#endregion
	}

	
	//=========================================================================
	public delegate void FtpEventHandler(object sender, FtpEventArgs e);
	
	//=========================================================================
	public class FtpEventArgs : EventArgs
	{
		public readonly string	Host;
		public readonly short	Port;

		//---------------------------------------------------------------------
		public FtpEventArgs(string host, short port)
		{
			this.Host	= host;
			this.Port	= port;
		}
	}
	
	//=========================================================================
	public class FtpPercentEventArgs : FtpEventArgs
	{
		public readonly int BytesTransferred;
		public readonly int FileSize;
		public readonly DateTime UtcStartTime;
		public readonly DateTime UtcEventTime;
		//public readonly double EstimatedSpeed;	// KB/s
		public readonly string FileName;

		//---------------------------------------------------------------------
		public FtpPercentEventArgs
			(
			string host,
			short port,
			int bytesTransferred, 
			int fileSize,
			DateTime utcStartTime,
			string fileName
			)
			: base(host, port)
		{
			this.BytesTransferred	= bytesTransferred;
			this.FileSize			= fileSize;
			this.UtcStartTime		= utcStartTime;
			this.UtcEventTime		= DateTime.UtcNow;
			this.FileName			= fileName;
			
			//double ms = UtcEventTime.Subtract(utcStartTime).TotalMilliseconds;
			//this.EstimatedSpeed	= ((long)bytesTransferred * 1000) / (ms * 1024);
		}
		
		//---------------------------------------------------------------------
		public int Percent
		{
			get { return GetPercent(BytesTransferred, FileSize); }
		}
		
		//---------------------------------------------------------------------
		public static int GetPercent(int bytesTransferred, int fileSize)
		{
			return unchecked((int)((100 * (long)bytesTransferred) / fileSize));
		}
	}

}

