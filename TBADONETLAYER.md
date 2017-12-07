
# Nuovo TaskBuilder Database Layer

Per la versione WEB è stato effettuato un cambio piuttosto sostanziale sia nel motore che nelle logiche di accesso al database, infatti il cambio tra OleDB (utilzzato nella versione 2.X) ed Ado.Net ha si lasciato invariato le interfaccie programmative ma ha cambiato radicalmente la modalità di connessione e di messa a disposizione dei dati estratti.

**Connessione**: si passa da uno stato full connected , l'applicazione è sempre connessa al database, ad uno stato disconnected dove  l'applicazione si connette al database solo se necessario.

**Dati estratti**: essendo ADO.Net nato proprio per una gestione disconnessa, i cursori dinamici e scrollabili non sono più supportati. Questo vuol dire che i dati o sono estratti lato server ed accedibili mediante un cursore forward-only read-only oppure sono estratti lato client dove il rowset estratto è statico ovvero non viene più modificato dalle altre connessioni (come nel caso di cursori keyset o dynamic).

Di seguito vedremo come questo cambio di rotta ha cambiato le logiche di gestione dei dati pur non modificando le le interfaccie programmative (SqlConnection, SqlSession, SqlTable...).


# Gestione connessione
La gestione della connessione si basa su due classi:

**SqlConnection**: contiene le informazioni necessarie per eseguire la connessione quali ad esempio la stringa di connessione. Esistono una o più SqlConnection in base al numero di connessioni necessarie all’applicazione (es: connessione principale al database aziendale, connessioni secondarie al database aziendali per gestione security o auditing, connessione a database secondari (es. DMS) o database esterni)

**SqlSessione**: è l’oggetto che effettua la vera e propria connessione basandosi sulle informazioni presenti sulla SqlConnection di cui fa parte.  Ogni SqlConnection può gestire una o più SqlSession. 
La singola SqlSession può essere aperta e chiusa quando si ha necessità di connettersi al database.
Una sessione nasce chiusa e solo quando è necessario viene aperta.

La connessione viene aperta e chiusa:

In **modo esplicito** durante:
* la fase di starup dell’applicativo
*	le varie fasi del documento: browsing, editing, saving, deleting
*	la OnBatchExecute di una procedura batch
*	il processo di estrazione dati di WOORM

In **modo implicito** quando un SqlTable ha necessità di effettuare una query al di fuori dei casi elencati in precedenza.

Quando un comando richiede la connessione viene incrementato il contatore dei comandi aperti sulla sessione di appartenenza, questo contantore poi viene decrementato quando il comando al termine delle operazioni sul db si disconette. Quando il contatore è a 0 la connessione viene chiusa di ufficio.

Il SqlTable viene disconesso in automatico dalla piattaforma al termine della query/fetch quando :
-	non estrae alcuna riga
-	si aspetta di estrarre solo una riga (vedi TableReader, TableUpdater…)
-	ha terminato il ciclo di MoveNext e il rowset è stato completamente consumato (vedi EOF)\
-	e’ stato aperto un cursore scrollabile. In questo caso la piattaforma opera su un DataTable per cui non c’è necessità di rimanere connesso
Se il SqlTable non rientra in questi casi sarà compito del programmatore disconnettere il comando.


# Gestione cursori

In ADO.NET esistono due modalità di estrazione dati:
-	Mediante un cursore l forward-only read-only server site implementato dal SqlDataReader. 
-	Mediante un cursore lato client completamente disconnesso implementato dal SqlDataSet

Questo vuol dire che se il SqlTable viene aperto in modalità Forward-Only (il default) viene utilizzato un SqlDataReader mentre nel caso di un apertura con bScrollable = TRUE, la piattaforma utilizza un DataTable (una versione semplificata ed ottimizzata del SqlDataSet).


