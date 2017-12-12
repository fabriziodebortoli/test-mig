using System;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.OracleDataAccess
{
	/// <summary>
	/// Summary description for OracleUserInfo.
	/// </summary>
	// ========================================================================
	public class OracleUserInfo
	{
		//---------------------------------------------------------------------
		private string connectionString  	= string.Empty;
		private string oracleService	 	= string.Empty;
		private string oracleUserId		 	= string.Empty;
		private string oracleUserPwd	 	= string.Empty;
		private string oracleCompanyName 	= string.Empty;
		private bool   oracleUserIsWinNT 	= false;
		
		//----------------------------------------------------------------------
		public string ConnectionString  { get { return connectionString;	}}
		public string OracleService		{ get { return oracleService;		} set { oracleService		= value; }}
		public string OracleCompanyName	{ get { return oracleCompanyName;	} set { oracleCompanyName	= value; }}
		public string OracleUserId		{ get { return oracleUserId;		} set { oracleUserId		= value; }}
		public string OracleUserPwd		{ get { return oracleUserPwd;		} set { oracleUserPwd		= value; }}
		public bool   OracleUserIsWinNT { get { return oracleUserIsWinNT;	} set { oracleUserIsWinNT	= value; }}

		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public OracleUserInfo()
		{
		}

		#region BuildStringConnection - Costruisce la stringa di connessione
		/// <summary>
		/// BuildStringConnection
		/// </summary>
		//---------------------------------------------------------------------
		public void BuildStringConnection()
		{
			connectionString = TBDatabaseType.GetConnectionString
				(
					oracleService, 
					oracleUserId, 
					oracleUserPwd, 
					string.Empty, 
					NameSolverDatabaseStrings.OraOLEDBProvider, 
					oracleUserIsWinNT, 
					false,
					0
				);
		}
		#endregion
	}

	//============================================================================
	public class OracleUser : IComparable
	{
		//---------------------------------------------------------------------
		private string	oracleUserId		= string.Empty;
		private bool	oracleUserOSAuthent	= false;

		//----------------------------------------------------------------------
		public string	OracleUserId		{ get { return oracleUserId; } set { oracleUserId = value; }}
		public bool		OracleUserOSAuthent	{ get { return oracleUserOSAuthent;	} set { oracleUserOSAuthent	= value; }}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="oracleUserId"></param>
		/// <param name="oracleUserOSAuthent"></param>
		//----------------------------------------------------------------------
		public OracleUser(string oracleUserId, bool oracleUserOSAuthent)
		{
			this.oracleUserId = oracleUserId;
			this.oracleUserOSAuthent = oracleUserOSAuthent;
		}

		#region IComparable Members
		//----------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			if (obj is OracleUser)
			{
				OracleUser tempUser = (OracleUser)obj;
				return oracleUserId.CompareTo(tempUser.oracleUserId);
			}
			throw new ArgumentException(OracleDataAccessStrings.ArgumentOracleUserExc);
		}
		#endregion
	}
}