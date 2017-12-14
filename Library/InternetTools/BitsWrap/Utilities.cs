using System;
using System.Runtime.InteropServices;

namespace Microarea.Library.Internet.BitsWrap
{
	internal class Utilities
	{
		public const int RPC_C_AUTHN_LEVEL_CONNECT = 2;
		public const int RPC_C_IMP_LEVEL_IMPERSONATE = 3;

		//Declare Auto Function CoInitializeSecurity Lib "ole32.dll" (ByVal secDesc As IntPtr, ByVal cAuthSvc As Integer, ByVal asAuthSvc As IntPtr, ByVal reserved1 As IntPtr, ByVal authnLevel As Integer, ByVal impLevel As Integer, ByVal authList As IntPtr, ByVal capabilities As Integer, ByVal reserved3 As IntPtr) As Integer
		[DllImport("ole32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int CoInitializeSecurity
			(
			IntPtr secDesc, 
			int cAuthSvc, 
			IntPtr asAuthSvc, 
			IntPtr reserved1, 
			int authnLevel, 
			int impLevel, 
			IntPtr authList, 
			int capabilities, 
			IntPtr reserved3
			);

		/*
		//Declare Auto Function IsEqualGUID Lib "ole32.dll" (ByRef rguid1 As BackgroundCopyManager.GUID, ByRef rguid2 As BackgroundCopyManager.GUID) As Boolean
		[DllImport("ole32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool IsEqualGUID
			(
			BackgroundCopyManager.GUID rguid1, 
			BackgroundCopyManager.GUID rguid2
			);
		*/

		public static string GetAccountName(string SID)
		{
			return LookupSID.GetName(SID);
		}

		/*
		public static System.Guid ConvertToGUID(BackgroundCopyManager.GUID bitsGUID)
		{
			System.Guid myGUID = new System.Guid
				(
				bitsGUID.Data1, 
				bitsGUID.Data2, 
				bitsGUID.Data3, 
				bitsGUID.Data4[0], 
				bitsGUID.Data4[1], 
				bitsGUID.Data4[2], 
				bitsGUID.Data4[3], 
				bitsGUID.Data4[4], 
				bitsGUID.Data4[5], 
				bitsGUID.Data4[6], 
				bitsGUID.Data4[7]
				);
			return myGUID;
		}

		public static BackgroundCopyManager.GUID ConvertToBITSGUID(System.Guid v)
		{
			BackgroundCopyManager.GUID myGUID = new BackgroundCopyManager.GUID();
			byte[] inGUID;
			System.UInt32 Data1;
			System.UInt16 Data2;
			System.UInt16 Data3;
			byte[] Data4 = new byte[8];
			inGUID = v.ToByteArray();
			Data1 = System.BitConverter.ToUInt32(inGUID, 0);
			Data2 = System.BitConverter.ToUInt16(inGUID, 4);
			Data3 = System.BitConverter.ToUInt16(inGUID, 6);
			inGUID.Copy(inGUID, 8, Data4, 0, 8);
			myGUID.Data1 = Data1;
			myGUID.Data2 = Data2;
			myGUID.Data3 = Data3;
			myGUID.Data4 = Data4;
			return myGUID;
		}
		*/

	}
}

