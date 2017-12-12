using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Services;
using System.Web.Services.Protocols;

using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.TbSenderBL.SOS;

namespace Microarea.WebServices.TbSender
{
	/// <summary>
	/// SOSProxy: webservice che fa da interfaccia di comunicazione con il servizio SOS Zucchetti
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	//================================================================================
	public class SOSProxy : System.Web.Services.WebService
	{
		private static SOSEngine engine = new SOSEngine();

		//--------------------------------------------------------------------------------
		[WebMethod]
		public void Init(string url)
		{
			engine.Url = url;
		}

		//--------------------------------------------------------------------------------
		[WebMethod]
		public bool Singolo(string codiceConservatore, string codiceCliente, string password, out Ticket ticket)
		{
			try
			{
				return engine.Singolo(codiceConservatore, codiceCliente, password, out ticket);
			}
			catch (SoapException se)
			{
				throw(se);
			}
			catch (WebException we)
			{
				throw (we);
			}
		}

		//--------------------------------------------------------------------------------
		[WebMethod]
		public bool Sequenza(string codiceConservatore, string codiceCliente, string password, int numeroChunk, int dimensione, out Ticket ticket)
		{
			try
			{
				return engine.Sequenza(codiceConservatore, codiceCliente, password, numeroChunk, dimensione, out ticket);
			}
			catch (SoapException se)
			{
				throw (se);
			}
			catch (WebException we)
			{
				throw (we);
			}
		}

		//--------------------------------------------------------------------------------
		[WebMethod]
		public bool Semplice(string ticketGUID, string nomeFileZip, Byte[] contenutoFileZip, out RispostaSpedizione spedizioneSemplice)
		{
			// Il ticket guid e' stato gia' generato in automatico a monte
			try
			{
				return engine.Semplice(ticketGUID, nomeFileZip, contenutoFileZip, out spedizioneSemplice);
			}
			catch (SoapException se)
			{
				throw (se);
			}
			catch (WebException we)
			{
				throw (we);
			}
		}

		//--------------------------------------------------------------------------------
		[WebMethod]
		public bool Frammentata
			(
			string ticketGUID, 
			string ticketChunkGUID, 
			string nomeFileZip, 
			Byte[] contenutoBlocco,
			out RispostaSpedizione spedizioneFrammentata
			)
		{
			try
			{
				return engine.Frammentata(ticketGUID, ticketChunkGUID, nomeFileZip, contenutoBlocco, out spedizioneFrammentata);
			}
			catch (SoapException se)
			{
				throw (se);
			}
			catch (WebException we)
			{
				throw (we);
			}
		}

		//--------------------------------------------------------------------------------
		[WebMethod]
		public bool StatoSpedizione
			(
			string conservatore,
			string soggetto, 
			string ticketGUID, 
			string nomeFileZip, 
			out StatoSpedizione statospedizione
			)
		{
			// Il ticket guid e' stato gia' generato in automatico a monte
			try
			{
				return engine.StatoSpedizione(conservatore, soggetto, ticketGUID, nomeFileZip, out statospedizione);
			}
			catch (SoapException se)
			{
				throw (se);
			}
			catch (WebException we)
			{
				throw (we);
			}
		}

		//--------------------------------------------------------------------------------
		[WebMethod]
		public bool EsitoSpedizione
			(
			string conservatore, 
			string soggetto, 
			string ticketGUID, 
			string nomeFileZip, 
			string codiceClasseDocumentale, 
			out EsitoSpedizione esitoSpedizione
			)
		{
			// Il ticket guid e' stato gia' generato in automatico a monte
			try
			{
				return engine.EsitoSpedizione(conservatore, soggetto, ticketGUID, nomeFileZip, codiceClasseDocumentale, out esitoSpedizione);
			}
			catch (SoapException se)
			{
				throw(se);
			}
			catch (WebException we)
			{
				throw (we);
			}
		}

		//--------------------------------------------------------------------------------
		[WebMethod]
		public bool ElencoClassiDocumentali(string conservatore, string soggetto, string ticketGUID, out List<ClasseDocumentale> classeDocList)
		{
			// Il ticket guid e' stato gia' generato in automatico a monte
			try
			{
				return engine.ElencoClassiDocumentali(conservatore, soggetto, ticketGUID, out classeDocList);
			}
			catch (SoapException se)
			{
				throw(se);
			}
			catch (WebException we)
			{
				throw (we);
			}
		}

		//--------------------------------------------------------------------------------
		[WebMethod]
		public bool ElencoChiaviClasseDocumentale
			(
			string conservatore, 
			string soggetto, 
			string ticketGUID, 
			string codiceClasseDocumentale, 
			out ClasseDocumentale docClass
			)
		{
			// Il ticket guid e' stato gia' generato in automatico a monte
			try
			{
				return engine.ElencoChiaviClasseDocumentale(conservatore, soggetto, ticketGUID, codiceClasseDocumentale, out docClass);
			}
			catch (SoapException se)
			{
				throw (se);
			}
			catch (WebException we)
			{
				throw (we);
			}
		}
	}
}