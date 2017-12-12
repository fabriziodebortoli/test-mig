using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin
{
	/// <summary>
	/// SysAdminStatus.
	/// Mantiene le variabili di Stato per il SysAdmin quali:
	/// Connessione (stringa e connessione SQL)
	/// Modalità di visualizzazione (Icone, Lista, Dettaglio..)
	/// Informazioni per la connessione al Db di Sistema (autenticazione, utente, password, nome server, nome database)
	/// </summary>
	//=========================================================================
	public class SysAdminStatus
	{
		#region Variabili Private
		//---------------------------------------------------------------------
		private string			serverName				= string.Empty;
		private string			serverIstanceName		= string.Empty;
		private string			user					= string.Empty;
		private string			password				= string.Empty;
		private string			dataSource				= string.Empty;
		private string			connectionString		= string.Empty;
		private string			ownerDbName				= string.Empty;
		private string			ownerDbPassword			= string.Empty;
		private SqlConnection	currentConnection		= null;
		private bool			integratedConnection	= false;
		private View			typeOfView				= View.Details;
		#endregion

		#region Proprietà
		//---------------------------------------------------------------------
		public string		 ServerName               { get { return serverName;           } set { serverName           = value; } }
		public string		 OwnerDbName              { get { return ownerDbName;          } set { ownerDbName          = value; } }
		public string		 OwnerDbPassword          { get { return ownerDbPassword;      } set { ownerDbPassword      = value; } }
		public bool			 IntegratedConnection     { get { return integratedConnection; } set { integratedConnection = value; } }
		public string		 ServerIstanceName        { get { return serverIstanceName;    } set { serverIstanceName    = value; } }
		public string		 User		              { get { return user;                 } set { user                 = value; } }
		public string		 Password		          { get { return password;             } set { password             = value; } }
		public string		 DataSource				  { get { return dataSource;           } set { dataSource           = value; } }
		public string        ConnectionString		  { get { return connectionString;     } set { connectionString     = value; } }
		public SqlConnection CurrentConnection		  { get { return currentConnection;    } set { currentConnection    = value; } }
		public View			 TypeOfView		          { get { return typeOfView;           } set { typeOfView           = value; }	}
		#endregion

		/// <summary>
		/// Clear - Pulisce le variabili di stato
		/// </summary>
		//---------------------------------------------------------------------
		public void Clear()
		{
			ConnectionString		= string.Empty;
			CurrentConnection.Close();
			CurrentConnection.Dispose();
			DataSource				= string.Empty;
			IntegratedConnection	= false;
			Password				= string.Empty;
			User					= string.Empty;
			ServerName				= string.Empty;
			ServerIstanceName		= string.Empty;
			OwnerDbName				= string.Empty;
			OwnerDbPassword			= string.Empty;
			TypeOfView				= View.Details;
		}
	}
}