using System;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.WebServices.LoginManager
{
	/// <summary>
	/// InitReason.
	/// </summary>
	//=========================================================================
	public enum InitReason
	{
		None,
		ApplicationStart,
		WebServiceCall,
		OnCacheItemRemoved,
		ProductRegistration
	}

	//=========================================================================
	public class InitEventArgs
	{
		private InitReason	initReason;
		private string		details;
		private string		loginManagerVersion;

		//---------------------------------------------------------------------
		public InitEventArgs(InitReason initReason, string loginManagerVersion, string details)
		{
			this.initReason = initReason;
			this.details	= details;
			this.loginManagerVersion = loginManagerVersion;
		}

		// N.B.:
		// le stringhe qui sono volutamente NON TRADOTTE per evitare che
		// in assistenza arrivino messaggi di errore in lingue a noi sconosciute
		// e quindi che siano messaggi inutilizzabili
		//---------------------------------------------------------------------
		public override string ToString()
		{
			StringBuilder strBuilder = new StringBuilder();

			switch (initReason)
			{
				case InitReason.ApplicationStart:
				{
					strBuilder.Append("Application start");
					if (details.Length > 0)
						strBuilder.Append(String.Concat(": ", details));
					break;
				}
				case InitReason.WebServiceCall:
				{
					strBuilder.Append("LoginManager had been initialized by a web service call");
					if (details.Length > 0)
						strBuilder.Append(String.Concat(": ", details));
					break;
				}
				case InitReason.OnCacheItemRemoved:
				{
                    if (!InstallationData.ServerConnectionInfo.EnableLMVerboseLog) return null;
                    //ATTENZIONE, questo lo loggo solo se verboso impostato a true.
					if (details.Length > 0 )
						strBuilder.Append(
							String.Format("LoginManager is up and running",
								//"Following file was modified: {0}", non palesiamo più, mettiamo messaggio criptico
								this.details
								)
							);

					break;
				}
				case InitReason.ProductRegistration:
				{
					strBuilder.Append("LoginManager had been initialized after a product registration");
					if (details.Length > 0)
						strBuilder.Append(String.Concat(": ", details));
					break;
				}
				default:
				{
					strBuilder.Append("Unable to know why LoginManager was restarted");
					if (details.Length > 0)
						strBuilder.Append(String.Concat(": ", details));
					break;
				}
			}

			if (!String.IsNullOrEmpty(loginManagerVersion))
			{
				strBuilder.Append(
					String.Format(
						System.Globalization.CultureInfo.InvariantCulture,
						"{0}{0}LoginManager ",
						Environment.NewLine
						)
					).Append(loginManagerVersion);
			}

			string operatingSystem = string.Empty;
			try
			{
				operatingSystem = Environment.OSVersion.ToString();
			}
			catch (InvalidOperationException exc)
			{
				System.Diagnostics.Debug.WriteLine(exc.ToString());
				operatingSystem = "Operating system not available";
			}

			strBuilder.Append(Environment.NewLine).Append(operatingSystem);

			strBuilder.Append(
				Environment.NewLine).Append(
				".NET Framework (CLR) ").Append(
				Environment.Version).Append(
				Environment.NewLine
				);

			return strBuilder.ToString();
		}
	}
}
