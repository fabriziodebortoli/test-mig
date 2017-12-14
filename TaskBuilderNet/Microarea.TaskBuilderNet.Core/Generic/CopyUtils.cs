using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace BuildPublisher
{
	//======================================================================================
	public sealed class FileRoutines
	{
		public static event EventHandler<CopyFileProgressEventArgs> CopyFileProgress;
		public static event EventHandler<CopyFileProgressEventArgs> CopyFileCompleted;
		public static event EventHandler<CopyFileFailedEventArgs> CopyFileFailed;
		//-------------------------------------------------------------------------------------
		private static void OnCopyFileCompleted(CopyFileProgressEventArgs e)
		{
			if (CopyFileCompleted != null)
				CopyFileCompleted(null, e);
		}

		//-------------------------------------------------------------------------------------
		private static void OnCopyFileFailed(CopyFileFailedEventArgs e)
		{
			if (CopyFileFailed != null)
				CopyFileFailed(null, e);
		}

		//-------------------------------------------------------------------------------------
		public static void CopyFile(string source, string destination, CopyFileOptions options, object state = null)
		{
			try
			{
				if (string.IsNullOrEmpty(source))
					throw new ArgumentNullException("source");
				if (string.IsNullOrEmpty(destination))
					throw new ArgumentNullException("destination");
				if ((options & ~CopyFileOptions.All) != 0)
					throw new ArgumentOutOfRangeException("options");

				if (!File.Exists(source))
					throw new FileNotFoundException("source file does not exist");

				FileInfo sourceInfo = new FileInfo(source);
				FileInfo destinationInfo = new FileInfo(destination);

				new FileIOPermission(FileIOPermissionAccess.Read, sourceInfo.FullName).Demand();
				new FileIOPermission(FileIOPermissionAccess.Write, destinationInfo.FullName).Demand();

				CopyProgressRoutine cpr = new CopyProgressRoutine(new CopyProgressData(sourceInfo, destinationInfo, CopyFileCallBack, state).CallbackHandler);

				bool cancel = false;
				DirectoryInfo destDirInfo = new DirectoryInfo(Path.GetDirectoryName(destinationInfo.FullName));
				if (!destDirInfo.Exists)
					destDirInfo.Create();

				//per riempire il listviewitem subito e non solo sulla copia
				CopyFileCallBack(sourceInfo, destinationInfo, state, sourceInfo.Length, 0);

				if (!CopyFileEx(sourceInfo.FullName, destinationInfo.FullName, cpr, IntPtr.Zero, ref cancel, (int)options))
					throw new IOException(new Win32Exception().Message);

				OnCopyFileCompleted(new CopyFileProgressEventArgs(sourceInfo, destinationInfo, state, (int)sourceInfo.Length, (int)sourceInfo.Length));
			}
			catch (Exception exc)
			{
				OnCopyFileFailed(new CopyFileFailedEventArgs(state.ToString(), exc));
			}
		}

		//-------------------------------------------------------------------------------------
		private static CopyFileCallbackAction CopyFileCallBack(FileInfo source, FileInfo destination, object state, long totalFileSize, long totalBytesTransferred)
		{
			if (CopyFileProgress != null)
			{
				CopyFileProgressEventArgs args = new CopyFileProgressEventArgs(source, destination, state, totalFileSize, totalBytesTransferred);
				CopyFileProgress(null, args);
			}

			return CopyFileCallbackAction.Continue;
		}

		[SuppressUnmanagedCodeSecurity]
		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		//-------------------------------------------------------------------------------------
		private static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref bool pbCancel, int dwCopyFlags);
	}

	//======================================================================================
	class CopyProgressData
	{
		private FileInfo _source = null;
		private FileInfo _destination = null;
		private CopyFileCallback _callback = null;
		private object _state = null;

		//-------------------------------------------------------------------------------------
		public CopyProgressData(FileInfo source, FileInfo destination, CopyFileCallback callback, object state)
		{
			_source = source;
			_destination = destination;
			_callback = callback;
			_state = state;
		}

		//-------------------------------------------------------------------------------------
		public int CallbackHandler
			(
			long totalFileSize, long totalBytesTransferred,
			long streamSize, long streamBytesTransferred,
			int streamNumber, int callbackReason,
			IntPtr sourceFile, IntPtr destinationFile, IntPtr data
			)
		{
			return (int)_callback(_source, _destination, _state, totalFileSize, totalBytesTransferred);
		}
	}

	//======================================================================================
	public class CopyFileFailedEventArgs : EventArgs
	{
		private string state;
		private Exception exception;

		public Exception Exception { get { return exception; } set { exception = value; } }
		public string State { get { return state; } set { state = value; } }
		
		//-------------------------------------------------------------------------------------
		public CopyFileFailedEventArgs(string state, Exception exception)
		{
			this.state = state;
			this.exception = exception;
		}
	}
	

	//======================================================================================
	public class CopyFileProgressEventArgs : EventArgs
	{
		FileInfo source;
		FileInfo destination;
		object state;
		long totalFileSize;
		long totalBytesTransferred;

		public FileInfo Source { get { return source; } set { source = value; } }
		public FileInfo Destination { get { return destination; } set { destination = value; } }
		public object State { get { return state; } set { state = value; } }
		public long TotalFileSize { get { return totalFileSize; } set { totalFileSize = value; } }
		public long TotalBytesTransferred { get { return totalBytesTransferred; } set { totalBytesTransferred = value; } }

		//-------------------------------------------------------------------------------------
		public CopyFileProgressEventArgs(FileInfo source, FileInfo destination, object state, long totalFileSize, long totalBytesTransferred)
		{
			this.source = source;
			this.destination = destination;
			this.state = state;
			this.totalFileSize = totalFileSize;
			this.totalBytesTransferred = totalBytesTransferred;
		}
	}

	//-------------------------------------------------------------------------------------
	delegate int CopyProgressRoutine(long totalFileSize, long TotalBytesTransferred, long streamSize, long streamBytesTransferred, int streamNumber, int callbackReason, IntPtr sourceFile, IntPtr destinationFile, IntPtr data);

	//-------------------------------------------------------------------------------------
	public delegate CopyFileCallbackAction CopyFileCallback(FileInfo source, FileInfo destination, object state, long totalFileSize, long totalBytesTransferred);

	[Flags]
	//-------------------------------------------------------------------------------------
	public enum CopyFileOptions { None = 0x0, FailIfDestinationExists = 0x1, Restartable = 0x2, AllowDecryptedDestination = 0x8, All = FailIfDestinationExists | Restartable | AllowDecryptedDestination }

	//-------------------------------------------------------------------------------------
	public enum CopyFileCallbackAction { Continue = 0, Cancel = 1, Stop = 2, Quiet = 3 }
}
