using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;



namespace Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects
{
	//====================================================================================
	public class TaskSchedulerLogFile
	{
		//--------------------------------------------------------------------------------------------------------
		public const string SchedulerLogFileName = "TBScheduler.log";

		[DllImport( "KERNEL32.DLL", CharSet=CharSet.Ansi)]
		private static extern uint GetLongPathName(string shortPath, StringBuilder longPathBuffer, uint bufferSize);

		private string		name;
		private FileStream	stream = null;
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public TaskSchedulerLogFile()
		{
			string temporaryPath = Path.GetTempPath();
			uint bufferSize = 260;
			StringBuilder longPathBuffer = new StringBuilder((int)bufferSize+1);
			GetLongPathName(temporaryPath, longPathBuffer, bufferSize);
			name = longPathBuffer.ToString() + SchedulerLogFileName;
		}
		
		#region TaskSchedulerLogFile public properties
		//--------------------------------------------------------------------------------------------------------------------------------
		public string Filename { get { return name; } }

		#endregion

		#region TaskSchedulerLogFile public methods

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool Open()
		{
			try 
			{
				// open an existing file, or create a new one
				FileInfo logFileInfo = new FileInfo(name);

				// Open the file just specified. Open it so that no-one else can write to it
				stream = logFileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
			} 
			catch (IOException iOException) 
			{
				Debug.Fail("IOException raised in TaskSchedulerLogFile.Open: " + iOException.Message);
				return false;
			} 
			return true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Close()
		{
			if (stream == null)
				return;
			stream.Close();
			stream = null;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool Write(string lineToWrite)
		{
			if (stream == null || !stream.CanWrite)
			{
				Debug.Assert (false);
				return false;
			}

			StreamWriter writer = new StreamWriter(stream);
			writer.AutoFlush = true;
			writer.WriteLine(lineToWrite);
			writer.Close();
			
			return true;
		}
	}
	#endregion
}