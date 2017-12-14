using System;
using System.Collections;
using System.Xml;

namespace Microarea.TaskBuilderNet.Licence.SalesModulesReader
{
	/// <summary>
	/// Elenco dei codici restituiti dal Web Service; le relative descrizioni
	/// sono elencate nella classe Strings.
	/// </summary>
	/// <remarks>
	/// Mantenere un rapporto di 1 a 1 tra codici e descrizioni; inoltre per
	/// ogni nuovo codice inserire l'apposito caso di switch nel metodo GetDescriptionOf.
	/// </remarks>
	//=========================================================================
	public class MessagesCode
	{
		#region Public methods
		/// <summary>
		/// Restituisce la descrizione dell'errore definito tramite un codice;
		/// in coda al messaggio è inoltre riportato il codice stesso.
		/// </summary>
		/// <param name="code">Codice di errore.</param>
		/// <returns>La descrizione dell'errore.</returns>
		//---------------------------------------------------------------------
		public static string GetDescriptionOf(string code)
		{
			string description = string.Empty;

			switch (code)
			{
				case Ok:
					description = SalesModulesReaderStrings.Ok;
					break;
				case NotConsistentXml:
					description = SalesModulesReaderStrings.NotConsistentXml;
					break;
				case WrongExtension:
					description = SalesModulesReaderStrings.WrongExtension;
					break;
				case WrongSyntax:
					description = SalesModulesReaderStrings.WrongSyntax;
					break;
				case AuthenticationFailed:
					description = SalesModulesReaderStrings.AuthenticationFailed;
					break;
				case NotAuthorizedCdS:
					description = SalesModulesReaderStrings.NotAuthorizedCdS;
					break;
				case DatabaseConnectionFailed:
					description = SalesModulesReaderStrings.DatabaseConnectionFailed;
					break;
				case NotAuthorizedContents:
					description = SalesModulesReaderStrings.NotAuthorizedContents;
					break;
				case CryptingFailed:
					description = SalesModulesReaderStrings.CryptingFailed;
					break;
				case ResponseXmlNotValid:
					description = SalesModulesReaderStrings.ResponseXmlNotValid;
					break;
				case GenericServerError:
					description = SalesModulesReaderStrings.GenericServerError;
					break;
				case TimeError:
					description = SalesModulesReaderStrings.TimeError;
					break;
				case NoSyncServerUrl:
					description = SalesModulesReaderStrings.NoSyncServerUrl;
					break;
				case SyncServerException:
					description = SalesModulesReaderStrings.SyncServerException;
					break;
				case InvalidRequest:
					description = SalesModulesReaderStrings.InvalidRequest;
					break;
				case InitializationError:
					description = SalesModulesReaderStrings.InitializationError;
					break;
				case LockedLogin:
					description = SalesModulesReaderStrings.LockedLogin;
					break;
				case PasswordExpired:
					description = SalesModulesReaderStrings.PasswordExpired;
					break;
				case PasswordToChange:
					description = SalesModulesReaderStrings.PasswordToChange;
					break;
				case OnlyEnglishSite:
					description = SalesModulesReaderStrings.OnlyEnglishSite;
					break;
				case DisabledLogin:
					description = SalesModulesReaderStrings.DisabledLogin;
					break;
				case TooMuchLoginErrors:
					description = SalesModulesReaderStrings.TooMuchLoginErrors;
					break;
				case ErrorContactingDb:
					description = SalesModulesReaderStrings.ErrorContactingDb;
					break;
				case InvalidHasSerial:
					description = SalesModulesReaderStrings.InvalidHasSerial;
					break;
				case IncompleteContent:
					description = SalesModulesReaderStrings.IncompleteContent;
					break;
				case ProductNotRegisteredAndNoLicence:
					description = SalesModulesReaderStrings.ProductNotRegisteredAndNoLicence;
					break;
				case ProductNotRegisteredAndNoStandAlone:
					description = SalesModulesReaderStrings.ProductNotRegisteredAndNoStandAlone;
					break;
				case ShortNamesMissing:
					description = SalesModulesReaderStrings.ShortNamesMissing;
					break;
				case ShortNameInvalid:
					description = SalesModulesReaderStrings.ShortNameInvalid;
					break;
				case IncludesInvalid:
					description = SalesModulesReaderStrings.IncludesInvalid;
					break;
				case ShortNamePrivate:
					description = SalesModulesReaderStrings.ShortNamePrivate;
					break;
				case ProductNotActive:
					description = SalesModulesReaderStrings.ProductNotActive;
					break;
				default:
					description = SalesModulesReaderStrings.GenericError;
					break;
			}

			return description + " (" + SalesModulesReaderStrings.Code + ": " + code + ").";
		}
		#endregion

		#region Error codes
		public static string Detail = SalesModulesReaderStrings.Detail;
		/// <summary>
		/// Richiesta andata a buon fine
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Ok						=	"0";
		/// <summary>
		/// Contenuto non XML
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	NotConsistentXml		=	"10";
		/// <summary>
		/// Estensione del file sbagliata
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	WrongExtension			=	"20";
		/// <summary>
		/// Sintassi del file non corretta
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	WrongSyntax				=	"30";
		/// <summary>
		/// Credenziali di autenticazione sbagliate
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	AuthenticationFailed	=	"40";
		/// <summary>
		/// L'utente non è un Centro di Sviluppo autorizzato 
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	NotAuthorizedCdS		=	"50";
		/// <summary>
		/// Fallita la connessione al database per le verifiche
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	DatabaseConnectionFailed=	"60";
		/// <summary>
		/// Il file ha dei contenuti non permessi, es: moduli di mago o tb non permessi (embedded o standAlone)
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	NotAuthorizedContents	=	"70";
		/// <summary>
		/// Crypt fallito
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	CryptingFailed			=	"80";
		/// <summary>
		/// Ì messaggi non sono interpretabili (xml non corretto)
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ResponseXmlNotValid		=	"90";
		/// <summary>
		/// Generico errore accaduto lato server
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	GenericServerError		=	"100";
		/// <summary>
		/// Errore tempo non sincronizzato
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	TimeError				=	"110";
		/// <summary>
		/// Manca l'indirizzo del WS
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	NoSyncServerUrl			=	"120";
		/// <summary>
		/// Eccezione durante la chiamata al ws per il time
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	SyncServerException		=	"130";
		/// <summary>
		/// La chiamata al webService non è criptata secondo le regole, non può essere accettata
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	InvalidRequest			=	"140";
		/// <summary>
		/// Non si riesce a loggare sull'eventlog
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	InitializationError		=	"150";
		/// <summary>
		/// Login bloccata
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	LockedLogin				=	"160";
		/// <summary>
		/// Login scaduta
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	PasswordExpired			=	"170";
		/// <summary>
		/// Password da cambiare
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	PasswordToChange		=	"180";
		/// <summary>
		/// Login valida solo per sito inglese
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	OnlyEnglishSite			=	"190";
		/// <summary>
		/// Login disabilitata
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	DisabledLogin			=	"200";
		/// <summary>
		/// Troppi errori in fase di login
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	TooMuchLoginErrors		=	"210";
		/// <summary>
		/// Eccezioni durante le query al db
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ErrorContactingDb		=	"220";
		/// <summary>
		/// Attributo hasserial invalido
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	InvalidHasSerial		=	"230";
		/// <summary>
		/// Il Sales Modules non può contenere esclusivamente moduli funzionali di altri produttori
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	IncompleteContent		=	"240";
		/// <summary>
		/// Il prodotto non è censito e non è uno standAlone
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ProductNotRegisteredAndNoLicence	=	"250";
		/// <summary>
		/// Il prodotto non è censito e non è uno standAlone
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ProductNotRegisteredAndNoStandAlone	=	"260";
		/// <summary>
		/// Il modulo non esprime il tag shortNames, obbligatorio per tutti
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ShortNamesMissing		=	"270";
		/// <summary>
		/// Il modulo non esprime il tag shortNames, obbligatorio per tutti
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ShortNameInvalid		=	"271";
		/// <summary>
		/// Il modulo non esprime il tag shortNames, obbligatorio per tutti
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ShortNamePrivate		=	"272";
		/// <summary>
		/// Il modulo esprime per il tag includes, un valore non valido
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	IncludesInvalid		=	"280";
		/// <summary>
		/// Il modulo esprime per il tag includes, un valore non valido
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ProductNotActive	= "396";
		/// <summary>
		/// È stato inviato un csm non valido
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	CsmNotValid	= "290";
		#endregion
	}

	//=========================================================================
	public class MessagesInfo
	{
		public  ArrayList Messages;
		protected XmlDocument workDoc = null;
	}

	//=========================================================================
	public class MessagesInfoWriter : MessagesInfo
	{
		//---------------------------------------------------------------------
		public MessagesInfoWriter (string msg)
		{			
			XmlDocument doc = new XmlDocument();
			Messages = new ArrayList();
			doc.LoadXml(msg);
			XmlNodeList nodes = doc.SelectNodes("//Message");
			foreach (XmlElement el in nodes)
			{
				Messages.Add(new MessageInfo(el.GetAttribute("value"), el.GetAttribute("detail")));
			}
			workDoc = null;	
		}
		//---------------------------------------------------------------------
		public MessagesInfoWriter ()
		{	}

		//---------------------------------------------------------------------
		public string GetXml()
		{			
			InitWorkDoc();
			WriteErrors();
			return workDoc.OuterXml;
		}

		//---------------------------------------------------------------------
		private void InitWorkDoc()
		{
			workDoc = new XmlDocument();
			XmlElement root = workDoc.CreateElement("Messages");
			workDoc.AppendChild(root);
		}

		//---------------------------------------------------------------------
		public void AddMessage(MessageInfo msg)
		{			
			if (msg == null) return;
			if (Messages == null)
				Messages = new ArrayList();
			Messages.Add(msg);
		}

		//---------------------------------------------------------------------
		public void AddMessage(string xmlMsg)
		{			
			if (xmlMsg == null || xmlMsg == String.Empty)
				return;
			XmlDocument doc = new XmlDocument();
			if (Messages == null)
				Messages = new ArrayList();
			doc.LoadXml(xmlMsg);
			XmlNodeList nodes = doc.SelectNodes("//Message");
			foreach (XmlElement el in nodes)
			{
				Messages.Add(new MessageInfo(el.GetAttribute("value"), el.GetAttribute("detail")));
			}
		}

		//---------------------------------------------------------------------
		public void WriteErrors()
		{
			if (Messages == null || Messages.Count < 1)
				return; 
			foreach (MessageInfo ei in Messages)
			{
				if (ei == null) continue; 
				XmlElement n = workDoc.CreateElement("Message");
				n.SetAttribute("value", ei.Code.ToString());
				if (ei.Detail != null && ei.Detail != String.Empty)
					n.SetAttribute("detail", ei.Detail);
				workDoc.DocumentElement.AppendChild(n);
			}
		}
	}

	//=========================================================================
	public class MessagesInfoReader : MessagesInfo
	{
		//---------------------------------------------------------------------
		public MessagesInfoReader(string xmlString)
		{
			if (xmlString != null && xmlString != String.Empty)
				ParseXml(xmlString);
		}

		//---------------------------------------------------------------------
		public MessagesInfoReader(MessagesInfoWriter writer): this(writer.GetXml())
		{
			
		}

		//---------------------------------------------------------------------
		private void ParseXml(string xmlString)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(xmlString);
			}
			catch (Exception)
			{
				XmlElement el =  doc.CreateElement("Message");
				el.SetAttribute("value", MessagesCode.ResponseXmlNotValid);//messaggio di errore per xml non formattato.
				doc.AppendChild(el);
			}
			
			XmlNodeList msgNodes	= doc.SelectNodes("//Message");
			LoadMessages(msgNodes);
			
		}

		//---------------------------------------------------------------------
		private void LoadMessages(XmlNodeList msgNodes)
		{
			Messages = new ArrayList();
			foreach (XmlElement msgNode in msgNodes)
			{
				string code		= msgNode.GetAttribute("value");
				string details  = msgNode.GetAttribute("detail");
				
				if (code != null && code != String.Empty)
				{
					MessageInfo msgInfo = new MessageInfo(code, details);
					Messages.Add(msgInfo);
				}
			}
		}
	}

	//=========================================================================
	public class MessageInfo
	{
		public string Code;
		public string Detail;

		public MessageInfo (string code, string detail)
		{
			Code = code;
			Detail = detail;
		}

		public MessageInfo (string code)
		{
			Code = code;
			Detail = null;
		}
	}
}
