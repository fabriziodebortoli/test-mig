using System;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// Descrive l'esito del controllo sulla sottoscrizione ISU di un UserID.
	/// </summary>
	//=========================================================================
	public enum SubscribersCheckerResults
	{
		/// <summary>
		/// Errore sconosciuto.
		/// </summary>
		/// <remarks>
		/// Ritornato anche nel caso le credenziali sottoposte per l'accesso
		/// al web service sicuro non siano corrette.
		/// </remarks>
		UnknownError		=	100,

		/// <summary>
		/// Si é verificato un errore durante l'inizializzazione del web service
		/// per cui questo non é in grado di svolgere il proprio compito.
		/// </summary>
		ErrorInitializingWebService	=	200,

		/// <summary>
		/// L'UserID non é stato riconosciuto dal sistema.
		/// </summary>
		/// <remarks>
		/// Ritornato quando il sistema non é stato in grado di risalire
		/// al codice UserID a partire dalla activation key inviata.
		/// </remarks>
		UserNotRecognized	=	300,

		/// <summary>
		/// La sottoscrizione ISU é stata disdettata.
		/// </summary>
		Disabled			=	400,

		/// <summary>
		/// La sottoscrizione ISU é scaduta.
		/// </summary>
		Expired				=	500,

		/// <summary>
		/// La sottoscrizione ISU dell'UserID é regolare.
		/// </summary>
		UserSubscribed		=	600,

		/// <summary>
		/// L'UserID non possiede alcuna sottoscrizione.
		/// </summary>
		UserNotSubscribed	=	700,

		/// <summary>
		/// L'UserID risulta Disabled nei database di Microarea.
		/// </summary>
		UserDisabled		=	750,

		/// <summary>
		/// Le informazioni estratte dal database contengono incongruenze.
		/// </summary>
		/// <remarks>
		/// Esempio di incongruenza potrebbe essere la data in cui é avvenuta la
		/// sottoscrizione posteriore alla data odierna, come se l'UserID avesse
		/// sottoscritto il servizio nel futuro.
		/// </remarks>
		ErrorInDatabase		=	800,

		/// <summary>
		/// Formato non valido per l'activation key inviata.
		/// </summary>
		InvalidActivationKeyFormat	=	900,

		/// <summary>
		/// L'UserID ha richiesto di usufruire del servizio ISU per una configurazione non piú attiva.
		/// </summary>
		/// <remarks>
		/// Una configurazione puó non essere piú attiva per esempio perché disattivata da una nuova
		/// attivazione da parte dell'UserID a seguito di un cambio di configurazione
		/// (aggiunta di sales module).
		/// </remarks>
		ActivationDisabled	=	1000,

		/// <summary>
		/// Si e' verificato un errore durante l'esecuzione della query per l'estrazione dei dati.
		/// </summary>
		ErrorReadingFromDatabase	=	1100,

		/// <summary>
		/// Non e' stata trovata la chiave inviata dall'UserID nei database aziendali.
		/// </summary>
		NoActivationKeyFound		=	1200,

		/// <summary>
		/// Non e' stata trovata alcuna attivazione corrispondente ai dati inviati nei database aziendali.
		/// </summary>
		NoActivationsFound			=	1250,

		/// <summary>
		/// I serial number inviati non hanno corrispettivo nei database aziendali.
		/// </summary>
		ErrorInLicensedConfiguration	=	1300,

		/// <summary>
		/// C'e' una differenza di orario troppo marcata tra client della richiesta e server dove e'
		/// deployato il web service invocato.
		/// </summary>
		TimeError						=	1400
	}

	/// <summary>
	/// Eccezione verificatosi durante il controllo sul database della sottoscrizione ISU.
	/// </summary>
	//=========================================================================
	public class SubscribersCheckerException : Exception
	{
		private		SubscribersCheckerResults	m_type;

		public		SubscribersCheckerResults	Type	{ get{ return m_type;  } }

		//---------------------------------------------------------------------
		public SubscribersCheckerException() : base()
		{}

		//---------------------------------------------------------------------
		public SubscribersCheckerException(string message) : base(message)
		{}

		//---------------------------------------------------------------------
		public SubscribersCheckerException(SubscribersCheckerResults result) : base(result.ToString())
		{
			m_type = result;
		}
	}
}
