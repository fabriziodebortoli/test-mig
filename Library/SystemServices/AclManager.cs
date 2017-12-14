using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;

namespace Microarea.Library.SystemServices
{
	/// <summary>
	/// Summary description for AclManager.
	/// </summary>
	public class AclManager
	{
		public static int	Inherit		= (int)(InheritFlags.ContainerInheritAce | InheritFlags.ObjectInheritAce);
		
		//---------------------------------------------------------------------------
		static AclManager()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Può generare eccezioni</remarks>
		/// <param name="user"></param>
		//---------------------------------------------------------------------------
		public void SetAcl(string path, string user, uint rightsMAsk, int inheritMask)
		{
			SecurityDescriptor sd = new SecurityDescriptor(path); //va bene anche per files
			sd.InsertAllowedAce(rightsMAsk, inheritMask, user);
			sd.CommitChanges();
		}
		
		//---------------------------------------------------------------------------
		public class Rights
		{
			public static uint Read		= (uint)(AccessRights.FileReadData	| AccessRights.FileReadAttributes	| AccessRights.FileReadEA	| AccessRights.GenericRead);
			public static uint Write	= (uint)(AccessRights.FileWriteData	| AccessRights.FileWriteAttributes	| AccessRights.FileWriteEA	| AccessRights.GenericWrite);
			public static uint FullControl	= (uint)AccessRights.GenericAll;
			public static uint ReadWrite	= (uint) (Read | Write);
		}
	}
}
