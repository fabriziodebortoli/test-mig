
namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// Codifica i messaggi di errore inviati indietro al client nel caso qualcosa
	/// non vada a buon fine durante la chiamata al WebService per la generazione
	/// dell'ActivationKey oppure durante la chiamata al WebService per la
	/// generazione dei Serial Number.
	/// </summary>
	//=========================================================================
	public class ErrorMessage
	{
		#region Messaggi di errore relativi al ping

		/// <summary>
		/// In base alle informazioni inviate col ping l'attivazione deve essere bloccata.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	PingToBeBlocked					=	"1010";

		/// <summary>
		/// Il web service invocato non e` abilitato a rilasciare l'attivazione.
		/// </summary>
		/// <remarks>
		/// Capita, per esempio, se un Mago.Net cinese chiede l'attivazione al
		/// web service italiano o viceversa.
		/// </remarks>
		//---------------------------------------------------------------------
		public	const	string	PingUnableToSatisfyRequest		=	"1015";

		/// <summary>
		/// Non é stato decodificato correttamente il MODE della richiesta o non é stato decriptato
		/// correttamente l'envelope dei parametri.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	PingRequestNotUnderstood		=	"1020";

		/// <summary>
		/// Il web service ha sollevato un'eccezione durante il processamento delle infromazioni per cui
		/// non é stato in grado di svolgere il suo compito.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	PingErrorVerifyingData			=	"1030";

		/// <summary>
		/// I parametri in andata sono diversi da quelli di ritorno.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	PingNotEqualsParameters			=	"1040";

		/// <summary>
		/// Lettura della dll ParametersManager fallita
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	PongNotReadDll					=	"1050";

		/// <summary>
		/// Errore nel decript di ParametersManager.dll
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	PongNotDecriptDll				=	"1060";

		/// <summary>
		/// Errore nell'uso di ParametersManager.dll via Reflection
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	PongReflectionError				=	"1070";

		/// <summary>
		/// Errore nell'uso di ParametersManager.dll via Reflection
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ParamsDllNotFound				=	"1080";

		/// <summary>
		/// ArgsException
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ArgumentsNull					=	"1090";

		/// <summary>
		/// Exception varie
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ParamManagerException			=	"1091";

		/// <summary>
		/// Country is null, impossible to decide the URL of the WebService
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	NullCountry						=	"1092";

		/// <summary>
		/// Eccezione reperendo i messaggi dal server
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	GetMessagesReflectionError		=	"1093";

		#endregion

		#region Messaggi di errore relativi alla Registrazione Asincrona
		/// <summary>
		/// Errore diurante il pack dei paramentri
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ErrorPackSynch			=	"700";
		//---------------------------------------------------------------------
		public	const	string	ErrorPackAsynch			=	"701";
		//---------------------------------------------------------------------
		public	const	string	ErrorWriteID			=	"710";
		//---------------------------------------------------------------------
		public	const	string	ErrorSerialize			=	"720";
		//---------------------------------------------------------------------
		public	const	string	ErrorEmptyID			=	"730";
		//---------------------------------------------------------------------
		public	const	string	ErrorNullRequest		=	"740";
		//---------------------------------------------------------------------
		public	const	string	ErrorNullResponse		=	"741";
		//---------------------------------------------------------------------
		public	const	string	ErrorUnpackReq			=	"750";
		//---------------------------------------------------------------------
		public	const	string	ErrorUnpackRes			=	"751";
		//---------------------------------------------------------------------
		public	const	string	RequestNotSavedCorrectly =  "760";
		//---------------------------------------------------------------------
		public	const	string	ErrorInvalidRequest		=  "770";
		



		#endregion

		#region Messaggi di errore relativi alla generazione dell'ActivationKey

		/// <summary>
		/// Errore nel Ínt.parse dell'errore
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ErrorNotValid				=	"70";//Assolutamente numero parsabile a int

		/// <summary>
		/// Richiesta andata a buon fine.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Ok						=	"80";

		/// <summary>
		/// Richiesta Asincrona andata a buon fine.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	AsyncOk						=	"85";

		/// <summary>
		/// Errore nella formattazione dell'xml della risposta del WebService.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ResponseXmlNotValid		=	"90";

		//---------------------------------------------------------------------
		public	const	string	ResponseEmpty		=	"95";

		/// <summary>
		/// Errore sconosciuto.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Unknown_Error			=	"100";

		/// <summary>
		/// I serial number del produttore specificato corrispondono a prodotti effettivamente venduti
		/// oppure compare almeno un serial number che risulta giá attivato. Oppure e' stato digitato male.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Producer_SerialNumber_Not_Verified			=	"105";

		/// <summary>
		/// I serial number non corrispondono a prodotti effettivamente venduti oppure compare almeno un
		/// serial number che risulta giá attivato. Oppure e' stato digitato male.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Not_Verified			=	"110";

		/// <summary>
		/// I serial number inviati risultano corretti, tuttavia tra quelli inviati ne mancano alcuni che
		/// erano gia' attivati. Effettuare la richiesta di attivazione sottoponendo tutti i serial number
		/// gia' attivati oltre ai nuovi inviati.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Missing_SerialNumber		=	"115";

		/// <summary>
		/// Errore nel ricreare il file XML di configurazione a partire dalla
		/// stringa in cui era stato serializzato per la trasmissione via SOAP al Web Service.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Loading_XML_Data	=	"120";

		/// <summary>
		/// Errore nella stringa che identifica la versione del prodotto da attivare.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Invalid_Version_Format	=	"125";

		/// <summary>
		/// Errore durante il processo di firma digitale dei dati. 
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Signing_Data		=	"130";
		
		/// <summary>
		/// Errore durante l'accesso al database per la verifica dei serial number.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Verifing_Data		=	"140";

		/// <summary>
		/// Errore durante l'accesso al database per la memorizzazione di una nuova attivazione.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Updating_DataBase	=	"150";

		/// <summary>
		/// Il database e` occupato da altre operazioni, ritentare piu` tardi.
		/// </summary>
		/// <remarks>
		/// Accade quando si arriva al momento di lock-are tabelle e queste sono
		/// gia` lock-ate da operatori i nterni per cui l'attivaizone non puo`
		/// essere rilasciata.
		/// </remarks>
		//---------------------------------------------------------------------
		public	const	string	Resource_Busy			=	"155";

		/// <summary>
		/// Errore durante l'inizializzazione del modulo che consente la registrazione di logs
		/// ogni volta che si verificano errori.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Initializing_Web_Service	=	"160";

		/// <summary>
		/// L'attributo "Country" specificato nel Serial Number non corrisponde a quello specificato 
		/// in UserInfo/Country oppure non é presente UserInfo/Country nel file inviato al Web Service
		/// per la generazione dell'ActivationKey.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Country_Not_Valid				=	"170";

		/// <summary>
		/// Trovata mescolanza di moduli pro e prolite(in sostanza MSD e NDB)
		/// </summary>
		//---------------------------------------------------------------------
		public const string ProAndProLite = "175";

		/// <summary>
		/// Errore negli elementi figli del tag "UserInfo" nel file inviato per la generazione
		/// dell'ActivationKey.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_In_UserInfo_Element		=	"180";

		/// <summary>
		/// Errore negli elementi figli del tag "LicensedFiles" nel file inviato per la generazione
		/// dell'ActivationKey.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_In_LicensedFiles_Element		=	"190";

		/// <summary>
		/// Nel wce inviato mancano i licensed.config.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	No_Licensed_Found		=	"195";

		/// <summary>
		/// Errore in uno dei Serial Number (lunghezza maggiore o minore del consentito,
		/// errore di formato ecc.).
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_In_SerialNumber_Format		=	"200";

		/// <summary>
		/// Non sono stati trovati Serial Number per uno o piú "SalesModule" nel file di configurazione.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	No_SerialNumber_Found				=	"210";

		/// <summary>
		/// Errore durante il processo del nodo XML che memorizza il valore della chiave di attivazione.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_In_ActivationKey_Element		=	"220";

		/// <summary>
		/// La chiave di attivazione inviata col file XML di configurazione non corrisponde ai Serial Number
		/// presenti nel file stesso.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ActivationKey_Not_Valid				=	"230";

		/// <summary>
		/// E'stata richiesta una nuova attivazione in seguito ad un cambio di configurazione mentre in
		/// realtá non vengono sottoposti serial number di nuovi moduli rispetto alla vecchia configurazione.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Configuration_Not_Changed			=	"240";

		/// <summary>
		/// L'utente che ha richiesto l'attivazione non é stato riconosciuto dal sistema o si sono verificati
		/// errori durante l'accesso al database per l'estrazione delle informazioni riguardanti l'utente che
		/// ha richiesto l'attivazione.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	User_Not_Recognized					=	"250";

		/// <summary>
		/// La partita iva sottoposta non corrisponde a quella presente nei nostri database.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Incorrect_VatNumber					=	"255";

		/// <summary>
		/// L'indirizzo email inserito per i dati utente è in realtà un indirizzo
		/// email del suo rivenditore..
		/// </summary>
		//---------------------------------------------------------------------
		public const string UserEmailAddressNotAllowed = "256";

		/// <summary>
		/// Errore durante il processo di generazione della stringa contenente la chiave di attivazione.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Generating_ActivationKey		=	"260";

		/// <summary>
		/// I database per i moduli inviati non corrispondono tra loro.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Invalid_Database_Value				=	"270";

		/// <summary>
		/// Le edition per i moduli inviati non corrispondono tra loro.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Invalid_Edition_Value				=	"280";

		/// <summary>
		/// I sistemi operativi per i moduli inviati non corrispondono tra loro.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Invalid_OpSys_Value					=	"290";

		/// <summary>
		/// La richiesta inviata dall'utente non contiene le credenziali adatte per accedere al servizio.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Invalid_Request						=	"300";

		/// <summary>
		/// Nel file wce inviato non e' specificato il serial number per le CAL.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	No_CAL_Found						=	"310";

		/// <summary>
		/// Nel file wce inviato non e' specificato il serial number per il server dell'installazione.
		/// </summary>
		/// <remarks>
		/// Allo stato attuale (17/01/2005, ore 14:25) i moduli che possono fare da server sono:
		/// Server, Advanced Package, Trade Package, 
		/// </remarks>
		//---------------------------------------------------------------------
		public	const	string	No_ServerModule_Found				=	"315";

		/// <summary>
		/// Nel file wce inviato sono presenti sia la security light che la security full
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Found_OSLS_And_OSLL				=	"316";

		/// <summary>
		/// Nel file wce inviato è presente la security light ma nell'attivazione che si va modificando c'era la security full..
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Found_OSLL_But_OSLS_Was_Previously_Activated		=	"317";

		/// <summary>
		/// Nel file wce inviato non e' specificato l'attributo 'Producer' per uno o piu' serial number.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	No_Producer_Found						=	"320";

		/// <summary>
		/// Nel Licensed.config di Mago.Net è presente il sales module
		/// per Magic Document Platform ma i suoi numeri (server ed eventuali CAL)
		/// non hanno appiccicata la producer key che è obbligatoria per questo
		/// sales module.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ProducerKey_Not_Found					=	"325";

		/// <summary>
		/// Nel Licensed.config di Mago.Net è presente il sales module
		/// per Magic Document Platform in cui è specificata una producer key
		/// di un centro di sviluppo che non ha la licenze per usare MDP.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ProducerKey_Without_License				=	"326";

		/// <summary>
		/// Nel file wce inviato non e' specificato il nome del prodotto come attributo del tag <Product>.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	ProductName_Not_Found					=	"330";

		/// <summary>
		/// Nel file wce inviato e' specificato almeno due volte lo stesso prodotto
		/// (letto dal nome del prodotto come attributo del tag <Product>).
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Duplicated_Product						=	"335";

		/// <summary>
		/// L'utente che ha richiesto l'attivazione e' disabilitato.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	User_Disabled							=	"340";

		/// <summary>
		/// Attivazione congelata.
		/// </summary>
		/// <remarks>
		/// Un'attivazione è congelata se e solo se l'attivazione è attiva ma l'MLU è scaduto.
		/// </remarks>
		//---------------------------------------------------------------------
		public	const	string	ActivationFrozen						=	"342";

		/// <summary>
		/// L'utente ha richiesto un cambio di configurazione per
		/// un'attivazione disattiva.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Activation_Disabled							=	"345";

		/// <summary>
		/// Non e' possibile contattare un web service di terze parti per la verifica
		/// dei serial number di un verticalista.
		/// </summary>
		/// <remarks>
		/// Ritorna come innerText del nodo il nome del verticalista il cui
		/// web service che non risponde.
		/// </remarks>
		//---------------------------------------------------------------------
		public	const	string	Error_Contacting_Web_Service			=	"350";

		/// <summary>
		/// L'ordine di migrazione non e' stato ancora generato.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Migration_Order_Not_Yet_Generated		=	"360";

		/// <summary>
		/// E' stato trovato l'attributo 'hasserial' per un produttore che
		/// non e' Microarea.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	HasSerialAttribute_Not_Allowed			=	"370";

		/// <summary>
		/// L'ora rilevata sul client e' troppo disallineata dall'orario corrente.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Time_Error								=	"380";

		/// <summary>
		/// L'utente ha inviato poche C.A.L. rispetto a quanto e' permesso
		/// per l'edition della copia di Mago.Net che intende attivare.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Too_Little_CAL_Specified					=	"385";

		/// <summary>
		/// L'utente ha inviato troppe C.A.L. rispetto a quanto e' permesso
		/// per l'edition della copia di Mago.Net che intende attivare.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Too_Many_CAL_Specified					=	"390";

		/// <summary>
		/// L'utente ha inviato poche C.A.L. ORACLE rispetto a quanto e' permesso
		/// per l'edition della copia di Mago.Net che intende attivare.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Too_Little_Oracle_CAL_Specified			=	"391";

		/// <summary>
		/// La verifica delle licenze per embedding, verticali o standalone
		/// ha dato esito negativo.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Invalid_License							=	"395";
	
		/// <summary>
		/// Il prodotto per cui e` stata chiesta l'attivazione e` disattivo.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Product_Disabled						=	"396";

		/// <summary>
		/// I prodotti installati insieme sono incompatibili.
		/// </summary>
		/// <remarks>
		/// Esempi di prodotti incompatibili sono:
		/// <list type="">
		/// <item>Mago.Net e un embedded qualunque</item>
		/// <item>Mago.Net e uno stand alone qualunque</item>
		/// <item>Un embedded e uno stand alone.</item>
		/// </list>
		/// Una coppia formata da uno qualunque dei prodotti sopra riportati e
		/// un vertcale e` ammessa.
		/// </remarks>
		//---------------------------------------------------------------------
		public	const	string	Incompatible_Products					=	"397";

		#endregion

		#region Messaggi di errore relativi alla generazione del Serial Number

		/// <summary>
		/// Errore generico durante la generazione del Serial Number.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Generating_SerialNumber		=	"400";

		/// <summary>
		/// Errore nei parametri per la generazione del codice prodotto.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_In_ProductCode_Format			=	"410";

		/// <summary>
		/// Errore nella traduzione da Codice Prodotto a Serial Number.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Translating_From_ProductCode_To_SerialNumber	=	"420";

		/// <summary>
		/// Errore nella traduzione da Serial Number a Codice Prodotto.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Translating_From_SerialNumber_To_ProductCode	=	"430";

		/// <summary>
		/// Errore nella generazione del numero progressivo che fa parte del
		/// Serial Number.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Generating_ProgressiveNumber					=	"440";

		/// <summary>
		/// Errore nella generazione del Product Code.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Generating_ProductCode						=	"450";

		/// <summary>
		/// Eccezione ritornando i dati utente per conferma.
		/// </summary>
		//---------------------------------------------------------------------
		public const string Error_Confirmating_User_Data							=	"460";

		/// <summary>
		/// I serial number inviati non corrispondono ad alcun utente su PAI
		/// per cui non posso ritornare indietro alcuna nformazione.
		/// </summary>
		//---------------------------------------------------------------------
		public const string Cannot_Confirm_User_Data								=	"470";

		#endregion 

		#region Messaggi di errore relativi alla gestione dei contatori

		/// <summary>
		/// Errore nella lettura del counter.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_Read									=	"500{0}";

		/// <summary>
		/// Errore nella scrittura del counter.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_Write								=	"501{0}";

		/// <summary>
		/// Errore nella rimozione del counter.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_Remove								=	"502{0}";

		/// <summary>
		/// Non esiste la directory che contiene il counter.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_FolderNotFound						=	"503{0}";

		/// <summary>
		/// Errore nella creazione la directory che contiene il counter.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_FolderNotCreated						=	"504{0}";

		/// <summary>
		/// Non esiste il file che contiene il counter.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_FileNotFound							=	"26_505{0}";

		/// <summary>
		/// Non è stato possibile creare il file che contiene il counter.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_FileNotCreated						=	"26_506{0}";

		/// <summary>
		/// Non è stato possibile interpretare la stringa letta.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_BadStringRead						=	"507{0}";

		/// <summary>
		/// Non è stato possibile decrittare la stringa letta sul counter.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_DecryptingString						=	"508{0}";
		
		/// <summary>
		/// Non è stato possibile decrittare la stringa letta sul counter.
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_EncryptingString						=	"509{0}";

		/// <summary>
		/// Il flag demo è inconsistente nei contatori standard
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_StandardDemoInconsistency			=	"550";

		/// <summary>
		/// La chiave di attivazione è inconsistente tra i vari contatori
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string	Error_Counters_ActivationKeysInconsistency			=	"551";

		//---------------------------------------------------------------------
		public	const	string	Error_Counters_DifferentInstallation				=	"552";

	#endregion

		#region ClientError - errori lato client prima della chiamata per Attivazione(ver.2)
	
		/// <summary>
		///È stata rilevata una eccezione nel webupdater prima della chiamata
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string		ExceptionBeforeCall				=	"2222";
		/// <summary>
		/// Impossibile recuperare LoginManager.asmx
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string		LoginManagerNotExisting			=	"2223";
		/// <summary>
		/// La chiamata è fallita per una WebException
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string		WebExceptionPinging				=	"2224";
		/// <summary>
		/// La chiamata è fallita per una SoapException
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string		SoapExceptionPinging			=	"2225";
		/// <summary>
		/// La chiamata è fallita per una InvalidOperationException-System.Web.Services
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string		InvalidOperationExceptionPinging=	"2226";
		/// <summary>
		/// La chiamata è fallita per una Exception generica
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string		ExceptionPinging				=	"2227";
		/// <summary>
		/// La chiamata  al WS è fallita per una Exception generica istanziando la classse proxy
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string		ExceptionInstantiatingProxy		=	"2228";
		/// <summary>
		/// La chiamata  al WS è fallita per una Exception generica istanziando la classse proxy
		/// </summary>
		//---------------------------------------------------------------------
		public	const	string		ExceptionDuringCommunication		=	"2229";
		
		#endregion
	}
}
