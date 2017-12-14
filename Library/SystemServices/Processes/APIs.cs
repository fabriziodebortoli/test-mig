using System;
using System.Text;
using System.Runtime.InteropServices;
//using ComType = System.Runtime.InteropServices.ComTypes;
using System.Threading;
//using System.Windows.Forms;

namespace Microarea.Library.SystemServices.Processes
{
    internal class APIs
    {
        // gets a process list pointer
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processID);

        // gets the first process in the process list
        [DllImport("KERNEL32.DLL")]
        public static extern bool Process32First(IntPtr handle, ref ProcessEntry32 processInfo);

        // gets the next process in the process list
        [DllImport("KERNEL32.DLL")]
        public static extern bool Process32Next(IntPtr handle, ref ProcessEntry32 processInfo);

        // closes handles
        [DllImport("KERNEL32.DLL")]
        public static extern bool CloseHandle(IntPtr handle);

        // gets the process handle
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess
			(
            uint desiredAccess, 
            bool inheritHandle,
            uint processId
			);
	
		//---------------------------------------------------------------------
		//BOOL EnumProcessModules(
		//	HANDLE hProcess,
		//	HMODULE* lphModule,
		//	DWORD cb,
		//	LPDWORD lpcbNeeded
		//	);
		[DllImport("psapi.dll", SetLastError = true)]
		internal static extern long EnumProcessModules
			(
			IntPtr hProcess,
			[Out] IntPtr[] lphModule, // 
			long cb,
			[Out] long cbNeeded
			);

		//---------------------------------------------------------------------
		//DWORD GetModuleFileNameEx(
		//	HANDLE hProcess,
		//	HMODULE hModule,
		//	LPTSTR lpFilename,
		//	DWORD nSize
		//	);
		[DllImport("psapi.dll", SetLastError = true)]
		internal static extern long GetModuleFileNameEx
			(
			IntPtr hProcess,
			IntPtr hModule, 
			[Out] StringBuilder fileName,
			long nSize
			);

		//---------------------------------------------------------------------
        public const int PROCESS_ENTRY_32_SIZE = 296;
        public const uint TH32CS_SNAPPROCESS = 0x00000002;
        public const uint PROCESS_ALL_ACCESS = 0x1F0FFF;

        public static readonly IntPtr PROCESS_LIST_ERROR = new IntPtr(-1);
        public static readonly IntPtr PROCESS_HANDLE_ERROR = new IntPtr(-1);
    }
}
