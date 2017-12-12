using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator;

namespace Microarea.Tools.ProvisioningConfigurator.ProvisioningConfiguratorLauncher
{
	//---------------------------------------------------------------------
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/// <returns>nResult=0 ok - nResult=-1 ko</returns>
		[STAThread]
		//---------------------------------------------------------------------
		static int Main()
		{
			int nResult = -1;

			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				// se non ci sono argomenti cmq l'array non e' null
				CommandLineParam[] arguments = CommandLineParam.FromCommandLine();

				// istanzio un ProvisioningData e lo riempio con i parametri passati come argomento
				ProvisioningData pData = new ProvisioningData(arguments);
                // se i parametri sono validi lancio in modalita' silente la procedura di configurazione
                if (arguments.Length > 0 && pData != null && pData.CheckEmptyValues())
                {
                    ProvisioningEngine provisioningEngine = new ProvisioningEngine(pData);
                    if (provisioningEngine.ConfigureProvisioningEnvironment())
                        nResult = 0;
                }
                else
                {
                    // la form deve caricare l'eventuale file se l'exe viene lanciato senza parametri
                    ProvisioningFormLITE provisioningForm = new ProvisioningFormLITE(loadDataFromFile: arguments.Length == 0);
                    if (pData != null)
                        provisioningForm.ProvisioningData = pData;
                    Application.Run(provisioningForm);
                    nResult = 0;//torno sempre ok, perchè se c'erano degli errori li ho visti in interattivo
                }
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				Debug.WriteLine(exc.Message + "\r\n" + exc.StackTrace);
			}

			return nResult;
		}
	}
}
