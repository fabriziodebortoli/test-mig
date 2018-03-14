
# Nuovo TaskBuilder Database Layer

Per la versione WEB è stato effettuato un cambio piuttosto sostanziale sia nel motore che nelle logiche di accesso al database, infatti il cambio tra OleDB (utilzzato nella versione 2.X) ed Ado.Net ha si lasciato invariato le interfaccie programmative ma ha cambiato radicalmente la modalità di connessione e di messa a disposizione dei dati estratti.

**Connessione**: si passa da uno stato full connected , l'applicazione è sempre connessa al database, ad uno stato disconnected dove  l'applicazione si connette al database solo se necessario.

**Dati estratti**: essendo ADO.Net nato proprio per una gestione disconnessa, i cursori dinamici e scrollabili non sono più supportati. Questo vuol dire che i dati o sono estratti lato server ed accedibili mediante un cursore forward-only read-only oppure sono estratti lato client dove il rowset estratto è statico ovvero non viene più modificato dalle altre connessioni (come nel caso di cursori keyset o dynamic).

Di seguito vedremo come questo cambio di rotta ha cambiato le logiche di gestione dei dati pur non modificando le le interfaccie programmative (SqlConnection, SqlSession, SqlTable...).


# Gestione connessione
La gestione della connessione si basa su due classi:

**SqlConnection**: contiene le informazioni necessarie per eseguire la connessione quali ad esempio la stringa di connessione. Esistono una o più SqlConnection in base al numero di connessioni necessarie all’applicazione (es: connessione principale al database aziendale, connessioni secondarie al database aziendali per gestione security o auditing, connessione a database secondari (es. DMS) o database esterni). Essendo la stringa di connessione di tipo ADO.NET essa non deve contenere il provider OLEDB.

**SqlSession**: è l’oggetto che effettua la vera e propria connessione basandosi sulle informazioni presenti sulla SqlConnection di cui fa parte.  Ogni SqlConnection può gestire una o più SqlSession. 
La singola SqlSession può essere aperta e chiusa quando si ha necessità di connettersi al database.
Una sessione nasce chiusa e solo quando è necessario viene aperta.

Il metodo AfxGetDefaultSqlConnection() restituisce:
-	un clone della connessione principale se la chiamata è stata eseguita all'interno del thread di documento
-	la connessione principale altrimenti

questo vuol dire che se la AfxGetDefaultSqlConnection() è di thread di documento di conseguenza lo è anche la AfxGetDefaultSqlSession(). 

La connessione viene aperta
In **modo esplicito** durante:
* 	la fase di startup dell’applicativo
*	le varie fasi del documento: browsing, editing, saving, deleting
*	la OnBatchExecute di una procedura batch
*	il processo di estrazione dati di WOORM

In **modo implicito** quando un SqlTable ha necessità di effettuare una query al di fuori dei casi elencati in precedenza.

Il SqlTable viene disconesso in automatico dalla piattaforma al termine della query/fetch quando :
-	non estrae alcuna riga
-	si aspetta di estrarre solo una riga (vedi TableReader, TableUpdater…)
-	ha terminato il ciclo di MoveNext e il rowset è stato completamente consumato (vedi EOF)\
-	e’ stato aperto un cursore scrollabile. In questo caso la piattaforma opera su un DataTable per cui non c’è necessità di rimanere connesso
Se il SqlTable non rientra in questi casi sarà compito del programmatore disconnettere il comando utilizzando il metodo Disconnect di SqlRowSet.


# Gestione cursori

In ADO.NET esistono due modalità di estrazione dati:
-	Mediante un cursore forward-only read-only lato server  implementato dal SqlDataReader. 
-	Mediante un cursore lato client completamente disconnesso implementato dal SqlDataSet

Questo vuol dire che se il SqlTable viene aperto in modalità Forward-Only (il default) viene utilizzato un SqlDataReader mentre nel caso di un apertura con bScrollable = TRUE, la piattaforma utilizza un DataTable (una versione semplificata ed ottimizzata del SqlDataSet).

```virtual	void Open
					(
						BOOL bUpdatable = FALSE,   // é un rowset su cui verranno effettuate operazioni di insert/update/delete
						BOOL bScrollable = FALSE,   // se TRUE é un rowset con un cursore di tipo scrollable
						BOOL bSensitivity = TRUE  // non usato ma lasciato per compatibilità con il passato
					);
 ```
 Nel caso di un SqlTable aperto in scrittura (bUpdatable = TRUE) la piattaforma effettua la query di update/insert/delete attraverso il comando di ExecuteNonQuery.
 
 Per utilizzare al meglio la nuova piattaforma si consiglia di:
  - se il SqlTable è forward-only consumare tutti i dati estratti mediante il MoveNext 
  - usare un cursore scrollabile solo se strettamente necessario e solo se si devono estrarre un numero limitato di record : l'utilizzo di un DataTable vuol dire occupazione di memoria lato client, ovvero i dati sono tutti disponibili nella memoria dove gira l'applicativo e non sul server del db come nel caso di cursore lato server. 
  
  
# Modifiche alla classe SqlTable

- numeri record estratti: con un cursore di tipo forward-only il numero di record estratti si ha solo alla fine delle operazione di fetch mentre per un cursore lato client (senza paginazione) il numero di record si ha subito dopo la query.
Per conoscere il numero di record estratti è necessario ora utilizzare la funzione di GetRowSetCount, che ne caso di cursore forward-only può effettuare una query "SELECT COUNT(*) FROM ( strSQl ) AS CT, dove strSQL è la query originale eliminata della parte di Order By. Il risultato viene memorizzato nel data member m_lRecordCounts della classe base MSqlCommand utilizzato per le eventuali richieste succesive.  Il metodo risulta molto oneroso. Utilizzarlo se si ha l'effettiva necessità.

- parametri: i parametri sono scritti nella query non più con il ? ma con il nome del parametro :@paramName. Per mantenere la compatibilità e non modificare ovunque la sintassi delle query, la classe SqlRowSet espone ed utilizza il metodo SubstituteQuestionMarks. 

  
