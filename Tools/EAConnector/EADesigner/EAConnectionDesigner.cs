using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PAT.Workflow.Runtime;
using EAConnector;

namespace EADesigner
{
	public partial class EAConnectionDesigner: ConnectionDesigner
	{
		public EAConnectionDesigner()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Metodo chiamato quando carica la maschera di connessione per EasyAttachment
		/// inserisce già le stringhe di default
		/// </summary>
		/// <param name="Configuration"></param>
		public override void LoadData(object Configuration)
		{
			var conf = (ConnectionConfiguration)Configuration;
			ServerTxt.Text = "localhost";
			InstallationNameTxt.Text = "DEVELOPMENT";
		}

		/// <summary>
		///	Prende l'input dell'utente e lo utilizza per provare a connettersi al web service di easyAttachment
		/// </summary>
		/// <param name="Configuration"></param>
		public override void SaveData(object Configuration)
		{
			var conf = (ConnectionConfiguration)Configuration;
			conf.SetServiceUri(ServerTxt.Text, InstallationNameTxt.Text);
		}
	}
}
