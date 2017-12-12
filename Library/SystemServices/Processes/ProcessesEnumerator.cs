using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Microarea.Library.SystemServices.Processes
{
	public class ProcessesEnumerator
	{
		/// <summary>
		/// Returns the array of all the executing processes paths, except those
		/// running as services, and some other special case.
		/// Differently form its managed counter part, this works even if the 
		/// performance counter was disabled.
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string[] GetExecutingProgramsViaAPI()
		{
			ProcessData[] processDataList = GetProcesses();
			return GetExecutingProgramsViaAPI(processDataList);
		}
		public static string[] GetExecutingProgramsViaAPI(ProcessData[] processDataList)
		{
			if (processDataList == null)
				return new string[]{};
			Hashtable ht = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
			foreach (ProcessData procData in processDataList)
			{
				string exePath = procData.FullPath;
				if (exePath != null && exePath.StartsWith("\\??\\"))
					exePath = exePath.Substring(4);
				if (exePath != null && !ht.Contains(exePath))
					ht.Add(exePath, exePath);
			}
			ArrayList list = new ArrayList();
			list.AddRange(ht.Values);
			list.Sort();
			return (string[])list.ToArray(typeof(string));
		}

		/// <summary>
		/// Returns the array of all the executing processes paths.
		/// In case machine performance counter was disabled, throws an exception.
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string[] GetExecutingProgramsFullManaged()
		{
            Hashtable ht = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
			Process[] proc = Process.GetProcesses();	// elenco processi running
			foreach (Process pr in proc)
			{
				if (pr.Id == 0)	// Idle
					continue;
				try
				{
					ProcessModule main = pr.MainModule;
				}
				catch (Exception exc)
				{
					Debug.WriteLine("Error: " + exc.Message);
					continue;
				}

				string exePath = pr.MainModule.FileName;
				if (exePath != null && exePath.StartsWith("\\??\\"))
					exePath = exePath.Substring(4);
				if (!ht.Contains(exePath))
					ht.Add(exePath, exePath);
			}
			ArrayList list = new ArrayList();
			list.AddRange(ht.Values);
			list.Sort();
			return (string[])list.ToArray(typeof(string));
		}

		/// <summary>
		/// Returns an array of processes description.
		/// Differently form its managed counter part, this works even if the 
		/// performance counter was disabled.
		/// </summary>
		/// <remarks>
		/// For processes running as services, the full path sported by array 
		/// elements is null.
		/// </remarks>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static ProcessData[] GetProcesses()
		{
			ArrayList processDataList = new ArrayList();
			
			ProcessEntry32 processInfo = new ProcessEntry32();
			IntPtr processList;
			IntPtr processHandle = APIs.PROCESS_HANDLE_ERROR;
			bool noError;

			// this creates a pointer to the current process list
			processList = APIs.CreateToolhelp32Snapshot(APIs.TH32CS_SNAPPROCESS, 0);

			if (processList == APIs.PROCESS_LIST_ERROR) { return null; }

			// we use Process32First, Process32Next to loop through the processes
			processInfo.Size = APIs.PROCESS_ENTRY_32_SIZE;
			noError = APIs.Process32First(processList, ref processInfo);

			while (noError)
				try
				{
					processHandle = APIs.OpenProcess( APIs.PROCESS_ALL_ACCESS, false, processInfo.ID);

					string filePath = null;
					IntPtr[] hModules = new IntPtr[200];
					long cbNeeded = 0; // out
					// http://support.microsoft.com/kb/187913/en-us
					if (processHandle != IntPtr.Zero)
					{
						long res = APIs.EnumProcessModules(processHandle, hModules, 200, cbNeeded);
						if (res != 0)
						{
							StringBuilder moduleName = new StringBuilder(256);
							long nSize = 500;
							long nSuccess = APIs.GetModuleFileNameEx(processHandle, hModules[0], moduleName, nSize);
							if (nSuccess > 0)
							{
								filePath = moduleName.ToString();
								Debug.WriteLine(filePath);
							}
							else
							{
								//Debug.Fail("NO PATH", Marshal.GetLastWin32Error().ToString());
							}
						}
						else
						{
							//Debug.Fail("NO MODULES", Marshal.GetLastWin32Error().ToString());
						}
					}
					else
					{
						if (processInfo.ID != 0)
							Debug.WriteLine(Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture));
					}
					
					//Debug.Assert(filePath != null);

					// replace known environment vars place holders
					if (filePath != null && filePath.StartsWith("\\SystemRoot\\System32"))
					{
						string systemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);
						if (systemFolder != null && systemFolder.Length != 0)
							filePath = filePath.Replace("\\SystemRoot\\System32", systemFolder);
					}

					//from here is just managing the gui for the process list
					ProcessData processData = new ProcessData
						(
						processInfo.ID,
						processInfo.ExeFilename, 
						filePath
						);
					processDataList.Add(processData);
				}
				finally
				{
					if (processHandle != APIs.PROCESS_HANDLE_ERROR)
						APIs.CloseHandle(processHandle);

					noError = APIs.Process32Next(processList, ref processInfo);
				}

			APIs.CloseHandle(processList);

			return (ProcessData[])processDataList.ToArray(typeof(ProcessData));
		}

		/// <summary>
		/// Tests the result against its managed equivalent.
		/// </summary>
		/// <remarks>
		/// Known differents are that for processes running as services the 
		/// unmanaged version is not able to read the full path, and that a wmi 
		/// process was not even detected.
		/// </remarks>
		/// <param name="processDataList"></param>
		//---------------------------------------------------------------------
		public static void TestIt(ProcessData[] processDataList)
		{
			Hashtable processDataTable = new Hashtable();
			foreach (ProcessData pData in processDataList)
				processDataTable.Add(pData.ID, pData);

			Process[] mProcesses = Process.GetProcesses();

			Debug.Assert(processDataList.Length == processDataTable.Count);
			Debug.Assert(processDataList.Length <= mProcesses.Length);
			//Debug.Assert(processDataList.Count == mProcesses.Length); // might happen
			foreach (Process pr in mProcesses)
			{
				uint id = (uint)pr.Id;
				bool cont = processDataTable.ContainsKey(id);
				//Debug.Assert(cont); // might happen

				if (pr.Id == 0)	// Idle
					continue;

				ProcessData pData = (ProcessData)processDataTable[id];
				//Debug.Assert(pData != null); // might happen

				try
				{
					ProcessModule main = pr.MainModule;
				}
				catch (Exception exc)
				{
					Debug.WriteLine("Error: " + exc.Message);
					continue;
				}

				if (pData == null)
				{
					Debug.Fail(pr.MainModule.FileName + " was not detected as running process.");
					continue;
				}

				if (pData.FullPath == null)
					Debug.WriteLine("Unable to read {0} path (is it a service)?", pr.MainModule.FileName);
				else
				{
					Debug.Assert(pData.FullPath == pr.MainModule.FileName);
				}
			}
		}

		//---------------------------------------------------------------------
	}
}
