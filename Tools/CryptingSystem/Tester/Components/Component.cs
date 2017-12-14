using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;

using Microarea.Library.SMBaseHandler;
using Microarea.TaskBuilderNet.ParametersManager;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Tester.Components
{
	//=======================================================================================================
	public class WSRequestWrapper
	{
		private		Crypter30.Crypter30		crypterProxy;

		//---------------------------------------------------------------------------------------------------
		public WSRequestWrapper()
		{
			crypterProxy = new Crypter30.Crypter30();
		}
		
		/// <summary>
		/// Effettua la richiesta della chiave di attivazione.
		/// </summary>
		/// <param name="xmlConfigFile">
		/// file contenente la configurazione da attivare.
		/// </param>
		/// <param name="proxyUrl">
		/// url del proxy/firewall se presente, stringa vuota altrimenti.
		/// </param>
		/// <param name="proxyPort">
		/// porta su cui risponde il proxy/firewall se presente, stringa vuota altrimenti.
		/// </param>
		/// <exception cref="DeltaTimeException">
		/// Lanciata nel caso la differenza di ora tra client e server sia maggiore del consentito.
		/// </exception>
		/// <exception cref="Exception">
		/// Lanciata nel caso non vada a buon fine l'invocazione del web service per le attivazioni.
		/// </exception>
		//---------------------------------------------------------------------------------------------------
		public bool CallWS(ArrayList filenames, string forcedProducer, out MessagesInfoWriter msgInfos, out string destFileFullName, string login, string password, string product)
		{
			CrypterBag cb		= null;
			string val			= null;
			destFileFullName	= String.Empty;
			msgInfos			= new  MessagesInfoWriter();
			bool cryptOk		= false;				
			string sessionCode	= "h725";
			string sessionID	= Guid.NewGuid().ToString();
			IParametersManagerFactory factory = new ParametersManagerFactory();
			try
			{
				//crypterProxy.Url = @"http://www.microarea.it/crypter30/crypter30.asmx";
				crypterProxy.Url = @"http://localhost/crypter30/crypter30.asmx";
				List<Attachment> files = new List<Attachment>() { };
				foreach (string filename in filenames)
				{
					Attachment att = new Attachment();
					FileStream fs = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read);
					byte[] buffer = new byte[fs.Length];
					fs.Read(buffer, 0, buffer.Length);
					fs.Close();
					att.Content = buffer;
					att.Id = filename;
					files.Add(att);
				}
				
				cb = new CrypterBag(login,password, product, forcedProducer);
				cb.Attachments = files;
				string resultString = string.Empty;
				cb.GetXmlString(out resultString, false);
				
				val = factory.GetParametersManager(CrypterBag.ProtocolVersion).SetParameter(
				false,
				resultString,
				sessionID, 
				sessionCode
				);

				cryptOk = crypterProxy.ProcessModule(ref val, ref sessionID, ref sessionCode);
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine(exc.Message);	
				msgInfos.AddMessage(new MessageInfo(MessagesCode.GenericServerError, exc.Message));
				return false;
			}

			string resultData = factory.GetParametersManager(CrypterBag.ProtocolVersion).GetParameter(
				false, 
				val,
				sessionID,
				sessionCode);

			CrypterBag newcb = (CrypterBag)(CrypterBag.GetFromXmlString(resultData,typeof(CrypterBag)));

			msgInfos.AddMessage(newcb.Messages);

			if (!cryptOk)
				return false;

			foreach (Attachment att in newcb.Attachments)
			{
				destFileFullName	= att.Id;
				try
				{					
					byte[] buffer = att.Content;
					FileStream fs = new FileStream(destFileFullName, FileMode.Create);
					BinaryWriter sw = new BinaryWriter(fs);
					sw.Write(buffer, 0, buffer.Length);
					fs.Close();
					sw.Close();
				}
				catch (Exception exc)
				{
					msgInfos.AddMessage(new MessageInfo(MessagesCode.GenericServerError, exc.Message));
					return false;
				}
			}
			return true;
		}

	}
}
