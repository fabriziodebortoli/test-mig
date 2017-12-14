using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.Core.StringLoader
{
	//================================================================================
	public class LocalizerConnector
	{
		//================================================================================
		public struct PointInfo
		{
			public Point Pt;

			//--------------------------------------------------------------------------------
			public void Parse(byte[] bytes)
			{
				MemoryStream ms = new MemoryStream(bytes);
				DictionaryBinaryParser parser = new DictionaryBinaryParser(ms);
				Pt.X = parser.ParseInt();
				Pt.Y = parser.ParseInt();
			}
			//--------------------------------------------------------------------------------
			public byte[] Unparse()
			{
				MemoryStream ms = new MemoryStream();
				DictionaryBinaryParser parser = new DictionaryBinaryParser(ms);
				parser.UnparseInt(Pt.X);
				parser.UnparseInt(Pt.Y);
				return ms.ToArray();
			}
		}
		//================================================================================
		public class FormInfo
		{
			public string Culture;
			public string Text;
			public string Namespace;
			public uint ID;
			public List<string> DictionaryPaths = new List<string>();
			//--------------------------------------------------------------------------------
			public void Parse(byte[] bytes)
			{
				MemoryStream ms = new MemoryStream(bytes);
				DictionaryBinaryParser parser = new DictionaryBinaryParser(ms);
				Culture = parser.ParseString();
				Text = parser.ParseString();
				Namespace = parser.ParseString();
				//compatibilità pregressa
				if (!parser.EOF)
					ID = parser.ParseUInt();
				//compatibilità pregressa
				if (!parser.EOF)
				{
					string s =parser.ParseString();
					foreach (string t in s.Split(';'))
					{
						string t1 = t.Trim();
						if (t1.Length == 0)
							continue;
						DictionaryPaths.Add(t1);
					}
				}
			}
			//--------------------------------------------------------------------------------
			public byte[] Unparse()
			{
				MemoryStream ms = new MemoryStream();
				DictionaryBinaryParser parser = new DictionaryBinaryParser(ms);
				parser.UnparseString(Culture);
				parser.UnparseString(Text);
				parser.UnparseString(Namespace);
				parser.UnparseUInt(ID);
				StringBuilder sb = new StringBuilder();
				foreach (string s in DictionaryPaths)
				{
					sb.Append(s + ";");
				}
				parser.UnparseString(sb.ToString());
				return ms.ToArray();
			}
		}
		//--------------------------------------------------------------------------------
		public const int MEM_COMMIT				= 0x1000;
		public const int MEM_RESERVE			= 0x2000;
		public const int MEM_RESET				= 0x80000;
		public const int MEM_PHYSICAL			= 0x400000;
		public const int MEM_TOP_DOWN			= 0x100000; 
		public const int MEM_DECOMMIT			= 0x4000;
		public const int MEM_RELEASE			= 0x8000;
		public const int PAGE_EXECUTE			= 0x10;
		public const int PAGE_EXECUTE_READ		= 0x20;
		public const int PAGE_EXECUTE_READWRITE	= 0x40;
		public const int PAGE_EXECUTE_WRITECOPY	= 0x80;
		public const int PAGE_NOACCESS			= 0x01;
		public const int PAGE_READONLY			= 0x02;
		public const int PAGE_READWRITE			= 0x04;
		public const int PAGE_WRITECOPY			= 0x08;
		
		public const int remoteBufferSize = 1024;

		//--------------------------------------------------------------------------------
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam);
		
		//--------------------------------------------------------------------------------
		[DllImport("kernel32.dll")]
		public static extern IntPtr VirtualAllocEx(
			IntPtr hProcess,
			IntPtr lpAddress,
			int dwSize,
			int flAllocationType,
			int flProtect
			);

		//--------------------------------------------------------------------------------
		[DllImport("kernel32.dll")]
		public static extern bool VirtualFreeEx(
			IntPtr hProcess,
			IntPtr lpAddress,
			int dwSize,
			int dwFreeType
			);

		//--------------------------------------------------------------------------------
		[DllImport("kernel32.dll")]
		public static extern bool ReadProcessMemory( 
			IntPtr hProcess, 
			IntPtr lpBaseAddress, 
			IntPtr lpBuffer, 
			int nSize, 
			ref int lpNumberOfBytesRead 
			);
		//--------------------------------------------------------------------------------
		[DllImport("kernel32.dll")]
		public static extern bool WriteProcessMemory(
			IntPtr hProcess,
			IntPtr lpBaseAddress,
			IntPtr lpBuffer,
			int nSize,
			ref int lpNumberOfBytesWritten
			);
		//--------------------------------------------------------------------------------
		[DllImport("user32.dll", EntryPoint="RegisterWindowMessage", CharSet=CharSet.Auto)]
		public static extern uint RegisterWindowMessage(string lpString);
 

		public static uint GetNamespaceMessage;
		
		//--------------------------------------------------------------------------------
		static LocalizerConnector()
		{
			try
			{
				GetNamespaceMessage = RegisterWindowMessage(typeof(LocalizerConnector).FullName + "GetNamespaceMessage");
			}
			catch(Exception ex)
			{
				GetNamespaceMessage = 0;
				Debug.Fail(ex.Message);
			}
		}

		//--------------------------------------------------------------------------------
		public static bool WndProc(ref Message m) 
		{
			IntPtr result = IntPtr.Zero;
			try
			{
				return WndProc(m.Msg, m.LParam, m.WParam, out result);
			}
			finally 
			{
				m.Result = result;
			}
		}
		//--------------------------------------------------------------------------------
		public static bool WndProc(int msg, IntPtr lParam, IntPtr wParam, out IntPtr result) 
		{
			result = IntPtr.Zero;
			if (msg != GetNamespaceMessage)
				return false;
			
			Control ctrl = Form.FromChildHandle(lParam);
			if (ctrl == null)
				return false;
			Control container = ctrl;
			while (container != null 
				&& !(container is UserControl)
				&& !(container is Form))
                container = container.Parent;

			if (container == null)
				return false;
				
			IntPtr buff = wParam;
			FormInfo fi = new FormInfo();
			fi.Culture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
			fi.Text = ctrl.Text;
			fi.Namespace = container.GetType().FullName;

			byte[] bytes = fi.Unparse();
			int offs = 0;
			foreach (byte b in bytes)
				Marshal.WriteByte(buff, offs++, b);
			result = (IntPtr) bytes.Length;
			return true;
		}

		//--------------------------------------------------------------------------------
		public static FormInfo GetWindowInfosFromPoint(Process proc, IntPtr windowHandle, Point screenPoint)
		{
			IntPtr controlHandle = windowHandle;
			//prima cerco una finestra che mi risponda
			while (IntPtr.Zero != windowHandle && - 1 != SendMessage(windowHandle, ExternalAPI.UM_GET_LOCALIZER_INFO, IntPtr.Zero, IntPtr.Zero).ToInt32())
				windowHandle = ExternalAPI.GetParent(windowHandle);
			if (windowHandle == IntPtr.Zero)
				return null;//non ho trovato finestre che gestiscono il mesaggio
			IntPtr pRemoteBuffer = IntPtr.Zero;
			try
			{
				FormInfo info = new FormInfo();
				pRemoteBuffer = VirtualAllocEx(proc.Handle, IntPtr.Zero, remoteBufferSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
				PointInfo pi = new PointInfo();
				pi.Pt = screenPoint;
				byte[] bytes = pi.Unparse();
				int tot = 0;
				IntPtr localHGlobal = Marshal.AllocHGlobal(bytes.Length);
				try
				{
					for (int i = 0; i < bytes.Length; i++)
						 Marshal.WriteByte(localHGlobal, i, bytes[i]);
					if (!WriteProcessMemory(proc.Handle, pRemoteBuffer, localHGlobal, bytes.Length, ref tot))
						return null;
				}
				finally
				{
					Marshal.FreeHGlobal(localHGlobal);
				}
				int numBytes = SendMessage(windowHandle, ExternalAPI.UM_GET_LOCALIZER_INFO, pRemoteBuffer, controlHandle).ToInt32();
				if (numBytes > 0)
				{
					localHGlobal = Marshal.AllocHGlobal(numBytes);
					bool b = ReadProcessMemory(proc.Handle, pRemoteBuffer, localHGlobal, numBytes, ref tot);

					bytes = new byte[numBytes];
					for (int i = 0; i < numBytes; i++)
						bytes[i] = Marshal.ReadByte(localHGlobal, i);

					Marshal.FreeHGlobal(localHGlobal);
					info.Parse(bytes);
					return info;
				}
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.Message);
			}
			finally
			{
				if (pRemoteBuffer != IntPtr.Zero)
				{
					bool res = VirtualFreeEx(proc.Handle, pRemoteBuffer, 0, MEM_RELEASE);
				}
			}
			return null;
		}
		
		//--------------------------------------------------------------------------------
		public static bool GetFormInfo(Process proc, IntPtr windowHandle, out FormInfo info)
		{
			IntPtr pRemoteBuffer = IntPtr.Zero;
			info = new FormInfo();
			try
			{
				pRemoteBuffer = VirtualAllocEx(proc.Handle, IntPtr.Zero, remoteBufferSize, MEM_COMMIT|MEM_RESERVE, PAGE_READWRITE);
				int numBytes = SendMessage(proc.MainWindowHandle, GetNamespaceMessage, pRemoteBuffer, windowHandle).ToInt32();
				if (numBytes > 0)
				{
					IntPtr localHGlobal = Marshal.AllocHGlobal(numBytes);
					int tot = 0;
					bool b = ReadProcessMemory(proc.Handle, pRemoteBuffer, localHGlobal, numBytes, ref tot); 
			
					byte[] bytes = new byte[numBytes];
					for (int i = 0; i < numBytes; i++)
						bytes[i] = Marshal.ReadByte(localHGlobal, i);
					
					Marshal.FreeHGlobal(localHGlobal);
					info.Parse(bytes);
					return true;
				}
			}
			catch(Exception ex)
			{
				Debug.Fail(ex.Message);
				return false;
			}
			finally
			{
				if (pRemoteBuffer != IntPtr.Zero)
				{
					bool res = VirtualFreeEx(proc.Handle, pRemoteBuffer, remoteBufferSize, MEM_RELEASE);
				}
			}

			return false;
		}
	}
}
