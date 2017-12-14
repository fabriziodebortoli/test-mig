using System;
using System.Runtime.InteropServices;
//using ComType = System.Runtime.InteropServices.ComTypes;

namespace Microarea.Library.SystemServices.Processes
{
    // holds the process info.
    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessEntry32
    {
        public uint Size;
        public uint Usage;
        public uint ID;
        public IntPtr DefaultHeapID;
        public uint ModuleID;
        public uint Threads;
        public uint ParentProcessID;
        public int PriorityClassBase;
        public uint Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string ExeFilename;
    };

}
