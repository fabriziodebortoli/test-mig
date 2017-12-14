using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// ProcessInfo.
	/// </summary>
	//=========================================================================
	public class ProcessInfo
	{
		private Process currentProcess;

		//---------------------------------------------------------------------
		public ProcessInfo()
		{
			try
			{
				currentProcess = Process.GetCurrentProcess();
			}
			catch (Exception exc)
			{
				Debug.WriteLine("ProcessInfo: " + exc.ToString());
				currentProcess = null;
			}
		}

		// N.B.:
		// le stringhe qui sono volutamente NON TRADOTTE per evitare che
		// in assistenza arrivino messaggi di errore in lingue a noi sconosciute
		// e quindi che siano messaggi inutilizzabili
		//---------------------------------------------------------------------
		public override string ToString()
		{
			if (Object.ReferenceEquals(null, currentProcess))
			{
				Debug.WriteLine("ProcessInfo: 'currentProcess' is null");
				return string.Empty;
			}

			StringBuilder aStringBuilder = new StringBuilder();

			string processName = string.Empty;
			try
			{
				processName = currentProcess.ProcessName;
			}
			catch (Exception exc)
			{
				Debug.WriteLine(String.Concat("ProcessInfo.ToString: ", exc.ToString()));
				processName = "Not available because Performances Counters are disabled or corrupted.";
			}
			aStringBuilder.Append(String.Concat("Process name: ", processName));
			aStringBuilder.Append(Environment.NewLine);

			int processId = -1;
			try
			{
				processId = currentProcess.Id;
			}
			catch (Exception exc)
			{
				Debug.WriteLine(String.Concat("ProcessInfo.ToString: ", exc.ToString()));
				processId = -1;
			}
			aStringBuilder.Append(String.Concat("Process id: ", processId));
			aStringBuilder.Append(Environment.NewLine);

			string processStartTime = string.Empty;
			try
			{
				processStartTime = currentProcess.StartTime.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
			}
			catch (Exception exc)
			{
				Debug.WriteLine(String.Concat("ProcessInfo.ToString: ", exc.ToString()));
				processStartTime = string.Empty;
			}
			aStringBuilder.Append(String.Concat("Start time: ", processStartTime));
			aStringBuilder.Append(Environment.NewLine);

			bool hasProcessExited = false;
			try
			{
				hasProcessExited = currentProcess.HasExited;
			}
			catch (Exception exc)
			{
				Debug.WriteLine(String.Concat("ProcessInfo.ToString: ", exc.ToString()));
				hasProcessExited = false;
			}
			if (hasProcessExited)
			{
				string processExitTime = string.Empty;
				try
				{
					processExitTime = currentProcess.ExitTime.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
				}
				catch (Exception exc)
				{
					Debug.WriteLine(String.Concat("ProcessInfo.ToString: ", exc.ToString()));
					processExitTime = string.Empty;
				}
				aStringBuilder.Append(String.Concat("Exit time: ", processExitTime));
				aStringBuilder.Append(Environment.NewLine);

				int processExitCode = -1;
				try
				{
					processExitCode = currentProcess.ExitCode;
				}
				catch (Exception exc)
				{
					Debug.WriteLine(String.Concat("ProcessInfo.ToString: ", exc.ToString()));
					processExitCode = -1;
				}
				aStringBuilder.Append(String.Concat("Exit code: ", processExitCode));
				aStringBuilder.Append(Environment.NewLine);
				aStringBuilder.Append(Environment.NewLine);
			}

			string userName = string.Empty;
			try
			{
				userName = Environment.UserName;
			}
			catch (Exception exc)
			{
				Debug.WriteLine(String.Concat("ProcessInfo.ToString: ", exc.ToString()));
				userName = "Not available because it is impossible to read USERNAME environment variable";
			}
			aStringBuilder.Append(String.Concat("User name: ", userName));
			aStringBuilder.Append(Environment.NewLine);

			bool isMainModuleNull = false;
			try
			{
				isMainModuleNull = Object.ReferenceEquals(null, currentProcess.MainModule);
			}
			catch (Exception exc)
			{
				Debug.WriteLine(String.Concat("ProcessInfo.ToString: ", exc.ToString()));
				isMainModuleNull = true;
			}

            if (!isMainModuleNull)
			{
				aStringBuilder.Append(Environment.NewLine);
				
				string mainModulefileName = string.Empty;
				try
				{
					mainModulefileName = currentProcess.MainModule.FileName;
				}
				catch (Exception exc)
				{
					Debug.WriteLine(String.Concat("ProcessInfo.ToString: ", exc.ToString()));
					mainModulefileName = "";
				}
				aStringBuilder.Append(String.Concat("File name: ", mainModulefileName));
				aStringBuilder.Append(Environment.NewLine);

				string mainModuleModuleName = string.Empty;
				try
				{
					mainModuleModuleName = currentProcess.MainModule.ModuleName;
				}
				catch (Exception exc)
				{
					Debug.WriteLine(String.Concat("ProcessInfo.ToString: ", exc.ToString()));
					mainModulefileName = "";
				}
				aStringBuilder.Append(String.Concat("Module name: ", mainModuleModuleName));
				aStringBuilder.Append(Environment.NewLine);
			}

			aStringBuilder.Append(Environment.NewLine);

			return aStringBuilder.ToString();
		}
	}
}
