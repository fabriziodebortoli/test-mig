using System;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.TaskBuilderNet.ParametersManager.MAActivation;


namespace Microarea.TaskBuilderNet.ParametersManager
{
	//=========================================================================
	public abstract class ProtocolManager
	{
		//--------------------------------------------------------------------------- 
		private ProtocolManager(){}

		/// <summary>
		/// Chiama il server microarea passando le informazioni della corrente installazione
		/// </summary>
		/// <param name="parameters">I dati di installazione</param>
		/// <returns>la stringa di attivazione o stringa vuota (codice da definire)</returns>
		//--------------------------------------------------------------------------- 
		public static Parameters Ping(Parameters parameters, SqlConnection sysDBConnection, string pcode)
		{
			SessionManager sm = SessionManagerFactory.CreateSessionManager(parameters.Country, sysDBConnection);
            sm.pcode = pcode;
            return sm.Ping(parameters, sysDBConnection);
		}

		/// <summary>
		/// Chiama il server microarea passando le informazioni della corrente installazione
		/// </summary>
		/// <param name="parameters">I dati di installazione</param>
		/// <returns>messaggi</returns>
		//--------------------------------------------------------------------------- 
		public static IList GetMessages(Parameters parameters, string pcode)
		{
			SessionManager sm = SessionManagerFactory.CreateSessionManager(parameters.Country);
            sm.pcode = pcode;
			return sm.GetMessages(parameters);
		}

		/// <summary>
		/// Chiama il server microarea passando le informazioni della corrente installazione
		/// </summary>
		/// <param name="parameters">I dati di installazione</param>
		/// <returns>messaggi</returns>
		//--------------------------------------------------------------------------- 
		public static bool SendAccessMail(Parameters parameters, string pcode)
		{
			SessionManager sm = SessionManagerFactory.CreateSessionManager(parameters.Country);
            sm.pcode = pcode;
            return sm.SendAccessMail(parameters);
		}

        /// <summary>
        /// Chiama il server microarea passando le informazioni della corrente installazione
        /// </summary>
        /// <param name="parameters">I dati di installazione</param>
        /// <returns>messaggi</returns>
        //--------------------------------------------------------------------------- 
        public static bool StoreMLUChoice(Parameters parameters, SqlConnection sysDBConnection, string pcode)
        {
            Sync sm = SessionManagerFactory.CreateSessionManager(parameters.Country, sysDBConnection) as Sync;
            if (sm != null)
            {
                sm.pcode = pcode;
                sm.StoreMLUChoice(parameters, sysDBConnection);
        }
			return true;
			
		}

	/*	//--------------------------------------------------------------------------- 
		public static bool IsPending(SqlConnection sysDBConnection)
		{
			return SessionManagerFactory.IsPending(sysDBConnection);
		}

		//--------------------------------------------------------------------------- 
		public static void ReloadGlobalSettings()
		{
			SessionManagerFactory.ReloadGlobalSettings();
		}*/
	}

	//=========================================================================
	internal class SessionManagerFactory
	{
		/*private enum CommunicationState { sync, asyncPending, asyncNotPending }
		private static GlobalSettings globalSettings = null;*/
		//private static IPathFinder pathFinder = null;

		//--------------------------------------------------------------------------- 
		static SessionManagerFactory()
		{
			

			/*string globalFile = pathFinder.GetGlobalSettingsFile();
			globalSettings = GlobalSettings.GetGlobalSettings(globalFile);*/
		}

		//--------------------------------------------------------------------------- 
		/*public static bool ReloadGlobalSettings()
		{
			string globalFile = pathFinder.GetGlobalSettingsFile();
			globalSettings = GlobalSettings.GetGlobalSettings(globalFile);
			return globalSettings != null;
		}*/

		/// <summary>
		/// Ritorna lo stato corrente della comunicazione in base a tutte le variabili di sessione
		/// </summary>
		/// <param name="sessionGUID"></param>
		/// <returns></returns>
		/*//--------------------------------------------------------------------------- 
		private static CommunicationState GetCommunicationState(out Guid sessionGUID, SqlConnection sysDBConnection, out DateTime reqDate)
		{
			reqDate = DateTime.MinValue;
			sessionGUID = Guid.Empty;

			//se non c'è il file il default è sync
			if (globalSettings == null || globalSettings.CommunicationSettings == null)
				return CommunicationState.sync;

			//se sono in modalità sinc non devo sapere altro
			if (globalSettings.CommunicationSettings.IsSynch)
				return CommunicationState.sync;

			//se sono asincrono devo leggere il guid. Se è valorizzato sono in pendin altrimenti no
			sessionGUID = RequestGUIDMng.ReadCurrentSessionGUID(pathFinder, sysDBConnection, out reqDate);
			if (sessionGUID == Guid.Empty)
				return CommunicationState.asyncNotPending;

			return CommunicationState.asyncPending;
		}*/

		/// <summary>
		/// Verifica lo stato della sessione ed in base a questo crea un oggetto derivato da SessionManager
		/// per gestire un particolare tipo di comunicazione
		/// </summary>
		/// <param name="country"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------------
		internal static SessionManager CreateSessionManager(string country, SqlConnection sysDBConnection)
		{
			//Guid sessionGUID = Guid.Empty;
			//DateTime reqDate = DateTime.MinValue;
			//legge il guid di sessione (se esiste) e il parametro di WUA che specifica sync o async
			//con questi stabilisce lo stato di sessione
			//CommunicationState commState = SessionManagerFactory.GetCommunicationState(out sessionGUID, sysDBConnection, out reqDate);

			//in base allo stato di sessione crea un SessionManager opportuno
			//if (commState == CommunicationState.sync)
			return new Sync(country, BasePathFinder.BasePathFinderInstance);

		/*	if (commState == CommunicationState.asyncPending)
				return new AsyncPending(country, pathFinder, sessionGUID, reqDate);

			//if (commState == CommunicationState.asyncPending)
			return new AsyncNotPending(country, pathFinder);
		 * */
		}

		/// <summary>
		/// Verifica lo stato della sessione ed in base a questo crea un oggetto derivato da SessionManager
		/// per gestire un particolare tipo di comunicazione
		/// </summary>
		/// <param name="country"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------------
		internal static SessionManager CreateSessionManager(string country)
		{
			return new Messages(country, BasePathFinder.BasePathFinderInstance);
		}
/*
		//---------------------------------------------------------------------------
		public static bool IsPending(SqlConnection sysDBConnection)
		{
			Guid sessionGUID = Guid.Empty;
			DateTime reqDate = DateTime.MinValue;
			CommunicationState commState = SessionManagerFactory.GetCommunicationState(out sessionGUID, sysDBConnection, out reqDate);
			return commState == CommunicationState.asyncPending;
		}*/
	}

	/// <summary>
	/// Classe base per la gestione del ping specializzato
	/// </summary>
	//=========================================================================
	public abstract class SessionManager
	{
		protected string country = string.Empty;
		protected IBasePathFinder pathFinder = null;
        internal string pcode;
		//---------------------------------------------------------------------------
		public SessionManager(string country, IBasePathFinder pathFinder)
		{
			this.country = country;
			this.pathFinder = pathFinder;
		}

		//---------------------------------------------------------------------------
		public abstract Parameters Ping (Parameters parameters, SqlConnection sysDBConnection);
		//---------------------------------------------------------------------------
		public abstract IList GetMessages (Parameters parameters);
		//---------------------------------------------------------------------------
		public abstract bool SendAccessMail (Parameters parameters);

		//---------------------------------------------------------------------------
		protected string Pack(Parameters parameters, bool isOn, Guid sessionId)
		{
			string resultString = string.Empty;
			parameters.GetXmlString(out resultString, false);

			IParametersManagerFactory factory = new ParametersManagerFactory();

			return factory.GetParametersManager(parameters.CurrentProtocolVersion).SetParameter(
				isOn,
				resultString,
				sessionId.ToString(),
				country
				);
		}

		//---------------------------------------------------------------------------
		protected Parameters UnPack(string dataFile, bool isOn, Guid sessionId)
		{
			IParametersManagerFactory factory = new ParametersManagerFactory();
			IParametersManager parametersManager = factory.GetParametersManager(dataFile);

			string resultData = parametersManager.GetParameter(
				isOn,
				dataFile,
				sessionId.ToString(),
				country
				);

			Parameters p = (Parameters)(Parameters.GetFromXmlString(
				resultData,
				typeof(Parameters)
				)
				);
			if (p != null)
				p.CurrentProtocolVersion = parametersManager.Version;

			return p;
		}

		//--------------------------------------------------------------------------- 
		protected string CheckParameters(Parameters sentParameters, Parameters resultParameters, out bool error)
		{
			error = false;

			if (!sentParameters.Equals(resultParameters, false))
			{
				error = true;
				return ReturnValuesWriter.GetErrorString(ErrorMessage.PingNotEqualsParameters);
			}

			return resultParameters.ActivationKey;
		}

		//---------------------------------------------------------------------------
		internal string PrepareFolderForAttachs()
		{
            // dalla 3.5 il setup installa le licenze senza intestazione in tutte le lingue 
            //poi la registrazione salva i file corretti con intestazione.
            //cancello tutte le cartelle che trovo perchè contengono le licenze in lingua installate dal setup
           
			try
			{
				string folderpath = pathFinder.GetLogManAppDataPath();
				if (folderpath == null || folderpath.Length == 0)
					return string.Empty;
                if (!Directory.Exists(folderpath))//se la cartella non esiste la creo
                    Directory.CreateDirectory(folderpath);
                else //altrimenti se esiste ne cancello solo i pdf vecchi e le cartelle che contengono i pdf non intestati        
                    Functions.DeleteFiles(folderpath, "*.pdf", true);

				return folderpath;
			}
			catch { return string.Empty; }

		}
	}

	//=========================================================================
	public class Messages : Sync
	{
		//---------------------------------------------------------------------------
		public Messages(string country, IBasePathFinder pathFinder)
			: base (country, pathFinder)
		{	}

		//---------------------------------------------------------------------------
		public override Parameters Ping (Parameters parameters, SqlConnection sysDBConnection)
		{
			throw new InvalidOperationException("Cannot ping from me!");
		}
		//gestione della situazione di backup se il sito primario ha dato eccezione
		private bool backup = false;
		//---------------------------------------------------------------------------
		public override IList GetMessages (Parameters parameters)
		{
			Guid sessionId = Guid.NewGuid();
			string globalFile = pathFinder.GetProxiesFilePath();

			string URL = null;
			if (backup)
			{
				URL = GetBackUpUrl();
				backup = false;
			}
			else
				URL = GetUrl();
			if (URL == null || URL.Length == 0)
				throw new UriFormatException("Url cannot be null or empty");

			parameters.ServiceUrl = URL;
			string s = string.Empty;
			try
			{
				s = Pack(parameters, true, sessionId);
			}
			catch(Exception e)
			{
				throw new PackException("Cannot pack", e);
			}

			Registration ak = null;
			try
			{
				ak = new Registration();
				ak.Url = parameters.ServiceUrl;
				ProxySettings.SetRequestCredentials(ak, globalFile);
			}
			catch (Exception exc)
			{
				throw new ProxySettingsException("Cannot set proxy information", exc);
			}

			string sexion = sessionId.ToString();
			string res = string.Empty;

			try
			{
				string c = parameters.Country;
				res = ak.GetMessages(ref s, ref sexion, ref c);					
			}

			//in caso di web exception sul sito primario rieseguo la chiamata sul sito di backUp, 
			//per evitare i disguidi in caso dovessimo non essere in linea
			catch (System.Net.WebException exc)
			{
				if (parameters.ServiceUrl != GetBackUpUrl())
				{
					backup = true;
					return GetMessages(parameters);
				}
				throw new MessagesException("Error accessing the network to retrieve messages", exc);
			}
			catch (System.Web.Services.Protocols.SoapException exc)
			{
				throw new MessagesException("Error during web service call to retrieve messages", exc);
			}
			catch (Exception exc)
			{
				throw new MessagesException("Cannot retrieve messages", exc);
			}

			return UnPack(s, false, sessionId);
		}

		//---------------------------------------------------------------------------
		public override bool SendAccessMail(Parameters parameters)
		{
			Guid sessionId = Guid.NewGuid();
			string globalFile = pathFinder.GetProxiesFilePath();

			string URL = null;
			if (backup)
			{
				URL = GetBackUpUrl();
				backup = false;
			}
			else
				URL = GetUrl();

			if (URL == null || URL.Length == 0)
				throw new UriFormatException("Url cannot be null or empty");

			parameters.ServiceUrl = URL;
			string s = string.Empty;
			try
			{
				s = Pack(parameters, true, sessionId);
			}
			catch(Exception e)
			{
				throw new PackException("Cannot pack", e);
			}

			Registration ak = null;
			try
			{
				ak = new Registration();
				ak.Url = parameters.ServiceUrl;
				ProxySettings.SetRequestCredentials(ak, globalFile);
			}
			catch (Exception exc)
			{
				throw new ProxySettingsException("Cannot set proxy information", exc);
			}

			string sexion = sessionId.ToString();
			bool res = false;

			try
			{
				string c = parameters.Country;
				res = ak.SendAccessMail(ref s, ref sexion, ref c);					
			}

			//in caso di web exception sul sito primario rieseguo la chiamata sul sito di backUp, 
			//per evitare i disguidi in caso dovessimo non essere in linea
			catch (System.Net.WebException exc)
			{
				if (parameters.ServiceUrl != GetBackUpUrl())
				{
					backup = true;
					return SendAccessMail (parameters);
				}
				throw new MessagesException("Error accessing the network to retrieve messages", exc);
			}
			catch (System.Web.Services.Protocols.SoapException exc)
			{
				throw new MessagesException("Error during web service call to retrieve messages", exc);
			}
			catch (Exception exc)
			{
				throw new MessagesException("Cannot retrieve messages", exc);
			}

			return res;		
		}

		//---------------------------------------------------------------------------
		protected new IList UnPack(string dataFile,  bool isOn, Guid sessionId)
		{
			if (dataFile == null) return null;

			string resultData = new ParametersManagerFactory().GetParametersManager(dataFile).GetParameter(
				isOn,
				dataFile,
				sessionId.ToString(),
				country
				);

			byte[] buffer = Convert.FromBase64String(resultData);
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream frontEndStream = new MemoryStream(buffer))
				return bf.Deserialize(frontEndStream) as IList;
		}
	}

	/// <summary>
	/// PackException.
	/// </summary>
	//=========================================================================
	[Serializable]
	public class MessagesException : Exception
	{
		//---------------------------------------------------------------------
		public MessagesException(string message, Exception innerException)
			: base (message, innerException)
		{}

		//---------------------------------------------------------------------
		public MessagesException(string message)
			: base (message, null)
		{}

		// Needed for xml serialization.
		//---------------------------------------------------------------------
		protected MessagesException(
			SerializationInfo info,
			StreamingContext context
			)
			: base (info, context)
		{}
	}

	//=========================================================================
	public class Sync : SessionManager
	{
		//---------------------------------------------------------------------------
		public Sync(string country, IBasePathFinder pathFinder)
			: base(country, pathFinder)
		{

		}

        //--------------------------------------------------------------------------- 
        protected string GetUrl()
        {
            if (pcode != null && pcode.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "vs")
                return "";
            return "http://www.microarea.it/registration/registration.asmx";
        }

        //--------------------------------------------------------------------------- 
        protected string GetBackUpUrl()
        {
            if (pcode != null && pcode.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "vs")
                return "";
            return "http://ping.microarea.eu/registration/registration.asmx";
        }

		//---------------------------------------------------------------------------
		public override IList GetMessages (Parameters parameters)
		{
			throw new InvalidOperationException("Cannot get messages from me!");
		}

		//---------------------------------------------------------------------------
		public override bool SendAccessMail (Parameters parameters)
		{
			throw new InvalidOperationException("Cannot send access Mail from me!");
		}

		//---------------------------------------------------------------------------
		public void StoreMLUChoice(Parameters parameters, SqlConnection sysDBConnectio)
		{
			Guid sessionId = Guid.NewGuid();
			string globalFile = pathFinder.GetProxiesFilePath();
			
			string URL = null;
			if (backup)
			{
				URL = GetBackUpUrl();
				backup = false;
			}
			else
				URL = GetUrl();
			if (URL == null || URL.Length == 0)		
				return ;

			parameters.ServiceUrl = URL;
			string s = string.Empty;

			try
			{
				s = Pack(parameters, true, sessionId);
			}
			catch {return;}

			Registration ak = null;
			try
			{
				ak = new Registration();
				ak.Url = parameters.ServiceUrl;
				ProxySettings.SetRequestCredentials(ak, globalFile);
			}
			catch {return;}

			string sexion = sessionId.ToString();
			string res = string.Empty;

			try
			{
				string c = parameters.Country;
				ak.StoreMluChoice(ref s, ref sexion, ref c);
			}
			catch{}
			
		}

		//gestione della situazione di backup se il sito primario ha dato eccezione
		private bool backup = false;
     

        //---------------------------------------------------------------------------
        public override Parameters Ping (Parameters parameters, SqlConnection sysDBConnection)
		{
			Guid sessionId = Guid.NewGuid();
			string globalFile = pathFinder.GetProxiesFilePath();
			
			string URL = null;
			if (backup)
			{
				URL = GetBackUpUrl();
				backup = false;
			}
			else
				URL = GetUrl();
			if (URL == null || URL.Length == 0)
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.NullCountry, String.Empty));

			parameters.ServiceUrl = URL;
			string s = string.Empty;

			try
			{
				s = Pack(parameters, true, sessionId);
			}
			catch (Exception e)
			{
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ErrorPackSynch, e.Message));
			}

			Registration ak = null;
			try
			{
				ak = new Registration();
				ak.Url = parameters.ServiceUrl;
				ProxySettings.SetRequestCredentials(ak, globalFile);
			}
			catch
			{
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ExceptionInstantiatingProxy));
			}

			string sexion = sessionId.ToString();
			string res = string.Empty;

			try
			{
				string c = parameters.Country;
				string w = "W";
				ServicePointManager.Expect100Continue = false;
				res = ak.IsAlive(ref s, ref sexion, ref c, ref w);

				////salvo gli attachments pdf (mlu- eula)
				//if (ak.ResponseSoapContext != null &&
				//    ak.ResponseSoapContext.Attachments != null &&
				//    ak.ResponseSoapContext.Attachments.Count > 0)

				//    SaveAttachs(ak.ResponseSoapContext.Attachments);

			}
			//in caso di web exception sul sito primario rieseguo la chiamata sul sito di backUp, 
			//per evitare i disguidi in caso dovessimo non essere in linea
			catch (System.Net.WebException exc)
			{
				if (parameters.ServiceUrl != GetBackUpUrl())
				{
					backup = true;
					return Ping (parameters, sysDBConnection);
				}
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ExceptionDuringCommunication, "("+exc.Source+"-"+ exc.GetType().ToString()+") "+ exc.Message));
			}
			catch (System.Web.Services.Protocols.SoapException exc)
			{
				if (parameters.ServiceUrl != GetBackUpUrl())
				{
					backup = true;
					return Ping(parameters, sysDBConnection);
				}

				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ExceptionDuringCommunication, "(" + exc.Source + "-" + exc.GetType().ToString() + ") " + exc.Message));
			}
			catch (Exception exc)
			{
				if (parameters.ServiceUrl != GetBackUpUrl())
				{
					backup = true;
					return Ping(parameters, sysDBConnection);
				}

				if (exc.GetType() == typeof(InvalidOperationException) &&
					string.Compare(exc.Source, "System.Web.Services", true) == 0)
				{
					return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ExceptionDuringCommunication, "(" + exc.Source + "-" + exc.GetType().ToString() + ") " + exc.Message));
				}
				else//non rivelo il testo dell'eccezione
				{
					return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ExceptionDuringCommunication, String.Empty));
				}
			}

			Parameters resultParameters = UnPack(s, false, sessionId);
			if (
				resultParameters != null &&
				resultParameters.Attachments != null &&
				resultParameters.Attachments.Count > 0
				)
				SaveAttachs(resultParameters.Attachments);

			bool b = false;// chiamato 'b' per sviare attenzione, in realtà è 'error'
			string temp = CheckParameters(parameters, resultParameters, out b);

			if (b)
				return Parameters.GetParametersForError(temp);

			return resultParameters;
		}

		//---------------------------------------------------------------------------
		private void SaveAttachs(IList attachs)
		{
			try
			{
               // if (attachs == null || attachs.Count == 0) return;

                //cancellare  le cartelle con la  cultura e i pdf intestati eventualmente presenti
				string folderPath = PrepareFolderForAttachs();
				if (folderPath == null || folderPath.Trim().Length == 0)
					return;

				foreach (Attachment att in attachs)
				{
					if (att == null)
						continue;

					string path = Path.Combine(folderPath, att.Id);
					using (BinaryWriter bw = new BinaryWriter(File.Create(path), System.Text.Encoding.Default))
						if (att.Content != null && att.Content.Length > 0)
							bw.Write(att.Content);
				}
			}
			catch
			{
				//NON BLOCCO L'ATTIVAZIONE SE NON RIESCO A SALVARE I PDF
			}

		}
	}
	/*
	/// <summary>
	/// Gestisce lo stato di sessione asincrono non pending
	/// </summary>
	//=========================================================================
	public class AsyncNotPending : SessionManager
	{
		//---------------------------------------------------------------------------
		public AsyncNotPending(string country, IPathFinder pathFinder)
			: base(country, pathFinder)
		{

		}

		//---------------------------------------------------------------------------
		public override IList GetMessages (Parameters parameters)
		{
			throw new InvalidOperationException("Cannot get messages from me!");
		}

		//---------------------------------------------------------------------------
		public override bool SendAccessMail (Parameters parameters)
		{
			throw new InvalidOperationException("Cannot send access Mail from me!");
		}


		/// <summary>
		/// Genera un nuovo guid di sessione
		/// Lo salva nei file di guid (Imposta lo stato di pending implicitamente)
		/// Genera un pacchetto di chiamata web e lo salva su file
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------------
		public override Parameters Ping (Parameters parameters, SqlConnection sysDBConnection)
		{
			//bool ok = true;
			Guid sessionGUID = Guid.Empty;

			//resetto comunque il guid
			if (!RequestGUIDMng.WriteCurrentSessionGUID(sessionGUID, pathFinder, sysDBConnection))
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ErrorWriteID));

			//sto rigenerando una request quindi se c'è una vecchia responce la elimino
			RequestGUIDMng.DeleteResponse(pathFinder);

			//genero il nuovo guid di sessione
			sessionGUID = Guid.NewGuid();

			//scrivo il pacchetto su file
			string s = string.Empty;
			try
			{
				s = Pack(parameters, true, sessionGUID);
			}
			catch (Exception exc)
			{
				Debug.WriteLine("AsynchNotPending.Ping: " + exc.Message);
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ErrorPackAsynch));
			}

			Envelope env = new Envelope(sessionGUID.ToString(), parameters.Country);
			env.Content = s;
			if (!env.SerializeToFile(pathFinder.GetRequestFile()))
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ErrorSerialize));

			//salvo il guid di sessione e quindi lo stato di pending
			if (!RequestGUIDMng.WriteCurrentSessionGUID(sessionGUID, pathFinder, sysDBConnection))
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ErrorWriteID));
			
			return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.AsyncOk));
		}
	}

	//=========================================================================
	public class AsyncPending : SessionManager
	{
		private Guid sessionGUID = Guid.Empty;
		private DateTime reqDate = DateTime.MinValue;

		//---------------------------------------------------------------------------
		public AsyncPending(string country, IPathFinder pathFinder, Guid sessionGUID, DateTime reqDate)
			: base(country, pathFinder)
		{
			this.sessionGUID = sessionGUID;
			this.reqDate = reqDate;
		}

		//---------------------------------------------------------------------------
		private void CleanAsyncFiles(SqlConnection sysDBConnection)
		{
			//elimino i vecchi file vecchi
			RequestGUIDMng.DeleteResponse(pathFinder);
			RequestGUIDMng.DeleteRequest(pathFinder, sysDBConnection);

		}

		//---------------------------------------------------------------------------
		public override IList GetMessages (Parameters parameters)
		{
			throw new InvalidOperationException("Cannot get messages from me!");
		}

		//---------------------------------------------------------------------------
		public override bool SendAccessMail (Parameters parameters)
		{
			throw new InvalidOperationException("Cannot send access Mail from me!");
		}

		//---------------------------------------------------------------------------
		public override Parameters Ping (Parameters parameters, SqlConnection sysDBConnection)
		{
			//se non trovo il guid di sessione errore
			if (sessionGUID == Guid.Empty)
			{
				CleanAsyncFiles(sysDBConnection);
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ErrorEmptyID));
			}

			TimeSpan ts = DateTime.UtcNow - reqDate;
			if (ts.TotalDays > 7)
			{
				CleanAsyncFiles(sysDBConnection);
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ErrorInvalidRequest));
			}
			//Leggo e unpack la request
			IEnvelope eReq = Envelope.ReadFromFile(pathFinder.GetRequestFile());
			if (eReq == null)
			{
				CleanAsyncFiles(sysDBConnection);
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ErrorNullRequest));
			}

			Parameters reqParams = null;
			try
			{
				reqParams = UnPack(eReq.Content, true, sessionGUID);
			}
			catch (Exception exc)
			{
				Debug.WriteLine("AsynchPending.Ping: " + exc.Message);
				CleanAsyncFiles(sysDBConnection);
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ErrorUnpackReq));
			}

			//leggo e unpack la response
			EnvelopeWithAttachs eRes = (EnvelopeWithAttachs)EnvelopeWithAttachs.ReadFromFile(pathFinder.GetResponseFile());
			if (eRes == null)
			{
				CleanAsyncFiles(sysDBConnection);
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ErrorNullResponse));
			}

			Parameters resParams = null;
			try
			{
				resParams = UnPack(eRes.Content, false, sessionGUID);
			}
			catch (Exception exc)
			{
				Debug.WriteLine("AsynchPending.Ping: " + exc.Message);
				CleanAsyncFiles(sysDBConnection);
				return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ErrorUnpackRes));
			}

			if (eReq != null && eRes.Attachments != null && eRes.Attachments.Count > 0)
				SaveAttachs(eRes.Attachments);

			CleanAsyncFiles(sysDBConnection);
			bool error = false;
			string s = CheckParameters(parameters, reqParams, out error);
			if (error) 
				return reqParams;
			CheckParameters(parameters, resParams, out error);
			return resParams;
		}



		//---------------------------------------------------------------------------
		private void SaveAttachs(IList attachs)
		{
			try
			{
				string folderPath = PrepareFolderForAttachs();
				if (folderPath == String.Empty)
					return;

				foreach (EnvelopeAttach attach in attachs)
				{
					byte[] buffer = Convert.FromBase64String(attach.Content);

					string path = Path.Combine(folderPath, attach.Id);
					using (BinaryWriter bw = new BinaryWriter(File.Create(path), System.Text.Encoding.Default))
					{
						bw.Write(buffer);
					}
				}
			}
			catch
			{
				//Non blocchiamo in caso di erroe durante il salvataggio dei pdf					
			}
		}

	}*/
}
