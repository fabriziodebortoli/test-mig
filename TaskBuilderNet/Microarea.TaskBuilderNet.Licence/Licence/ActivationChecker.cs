using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Licence.Activation;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	//=========================================================================
	public class FacadeBuilder
	{

		/// <summary>
		/// Genera un numero casuale compreso tra 1 e 100 basandosi sui tick di sistema.
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private int GetBuilderDate()
		{
			return (Environment.TickCount % 100) + 1;
		}

		/// <summary>
		/// Verifica che il wce e la chiave di attivazione siano validi e quindi non siano stati modificati dopo l'attivazione
		/// </summary>
		/// <param name="builderName">stringa dell'inner xml del wce</param>
		/// <param name="builderValue">chiave di attivazione da verificare</param>
		/// <param name="builderIndex">versione dell'algoritmo di generazione della chiave di attivazione.</param>
		/// <param name="builderVersion">ritorna un numero pari se l'attivazione e` ok, dispari altrimenti.</param>
		/// <returns>un booleano fittizio.</returns>
		/// <remarks>considera 0 come pari per il valore builderVersion.</remarks>
		//---------------------------------------------------------------------
		public bool Build(string MicroareaPublicKey, string builderName, string builderValue, int builderIndex, out int builderVersion)
		{
			if (builderName == null || builderName == String.Empty || builderValue == null || builderValue == String.Empty)
			{
				Debug.WriteLine("FacadeBuilder.Build: params are empty."); 
				builderVersion = GetBuilderDate() * 2 - 1;
				return false;
			}

			if (builderIndex < 2)
			{
				builderVersion = GetBuilderDate() * 2;
				return true;
			}
			try
			{
				CspParameters cspParam = new CspParameters();
				cspParam.Flags = CspProviderFlags.UseMachineKeyStore;
				//scrive un file in 
				//	C:\Documents and Settings\All Users\Dati applicazioni\Microsoft\Crypto\RSA\MachineKeys
				//se gli do il nome usa sempre lo stesso, altrimenti ne crea uno nuovo ogni volta.
				//Provo a ottenere il nome dell'utente con cui LgM sta girando e lo assegno al keyContainerName.
				//Se per qualche motivo non riuscissi ad ottenere tale nome, lascio la stringa "LoginManager"
				//(Come avveniva prima di questa modifica).
				//Se avessi problemi di accesso per cause sistemistiche provo col nome vuoto
				//Il fatto di attribuire il nome dell'utente serve per gestire quei casi che si sono verificati
				//in cui, per motivi a noi ignoti, LoginManager veniva fatto girare da application pool differenti
				// e questo fatto faceva si che il file nella MachineKeys generato con le credenziali di un certo
				//utente non poteva essere usato dal nuovo utente.
				//In questo modo ogni utente ha il suo file.
				string keyContainerName = "LoginManager";
				try
				{
					keyContainerName = Environment.UserName;
				}
				catch
				{
					keyContainerName = "LoginManager";
				}
				RSACryptoServiceProvider rsa = null;
				try
				{
					cspParam.KeyContainerName = keyContainerName;
					rsa = new RSACryptoServiceProvider(cspParam);
                   
                }
				catch
				{
					cspParam.KeyContainerName = String.Empty;
					rsa = new RSACryptoServiceProvider(cspParam);
                    
                }
				
				rsa.FromXmlString(MicroareaPublicKey);

				ISignParamsPreparer signParametersPreparer =
						new SignParamsPrepFactoryForVer2_Prot4(Parameters.ProtocolVersion).GetSignParamsPreparer(builderIndex);
				signParametersPreparer.PrepareXmlConfigFile(ref builderName);
				string wce = builderName;
				//Carica il file firmato
				byte[] signedData= null;

				switch (builderIndex)
				{
					case 2:
						signedData = Convert.FromBase64String(builderValue);
						break;
					default:
						signedData = Converter.GetByteArrayFromHexadecimalString(builderValue);
						break;
				}
				
				XmlDocument wceDoc = new XmlDocument();
				wceDoc.LoadXml(wce);
				bool ok = false;
				//Verifica la firma.
				string s = signParametersPreparer.PrepareParamsForSigning(wceDoc).InnerText;
				builderVersion = GetBuilderDate() * 2 - (
						(
						ok = rsa.VerifyData(
											// Ottiene la rappresentazione in byte del file XML di configurazione del programma.
											Converter.GetBytes(signParametersPreparer.PrepareParamsForSigning(wceDoc).InnerText),
											// Algoritmo con cui viene fatto l'hash
											"SHA1",
											// ActivationKey.
											signedData
										)
						) ? 0 : 1
					);
					return ok;
			}
			catch (Exception exc)
			{
				Debug.WriteLine("ActivationChecker.IsActivationValid - Errore: " + exc.Message);
				throw new Exception(exc.Message, exc);
			}
		}
	}
}

