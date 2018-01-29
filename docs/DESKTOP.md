# Sincronizzazione e Build ambiente di sviluppo Desktop

Questo documento descrive come preparare e aggiornare l'ambiente di sviluppo per utilizzare il menu web da parte di chi lavora solo in ambiente Desktop.

**ATTENZIONE**: la procedura non vale se sviluppate anche in ambiente web. Con questa procedura vengono cancellati eventuali cadaveri di cartelle legate alla parte web, quindi qualsiasi modifica alla parte web non pushata su git, ***con questo metodo andrà irrimediabilmente persa***. 

* [Prerequisiti](#prerequisiti)
* [Script](#script)
* [Troubleshooting](#troubleshooting)

## Prerequisiti

Prima di proseguire, verificare i [requisiti](https://github.com/Microarea/Taskbuilder/blob/master/docs/REQUIREMENTS.md)

## Script

Il prerequisito per eseguire la build dell'ambiente web è di avere sincronizzato TaskBuilder, ERP ed ogni altra solution C++ (es.: MDC, ecc.).

Eseguire **COME AMMINISTRATORE** lo script `Standard\TaskBuilder\BuildWebEnvironment.bat`: se vengono riportati degli errori, vedere [**Troubleshooting**](#troubleshooting).

La prima volta, se non avete mai fatto accesso a GIT da linea di comando, comparirà una finestra che vi chiede le credenziali, inseritele. Non verranno più richieste successivamente.

Il lancio dello script va ripetuto ogni volta che si sincronizza TaskBuilder; può essere fatto prima o dopo la compilata delle solution di ERP e di TB.

### BONUS!! Uso di Mago web
Facendo quanto sopra potete anche usare il *vero* Mago web.

Aprite un prompt di comandi e spostatevi nel folder `Standard\web\`.  
Date il comando: `git checkout master` (cambiate il branch di lavoro di TB web).

Lanciate lo script `Standard\web\client\web-form\run.bat` (da linea di comando o con doppio click da Windows Explorer). Si aprono due command prompt ed un TBLoader. Quando nel command prompt con titolo "@angular/cli" compare la scritta "webpack compiled successfully" aprite il browser su `localhost:4200`.

Per tornare sul branch della 2.x, date il comando `git checkout dev_2_x` (dopo avere chiuso tutti i command prompt aperti dal run).

## Troubleshooting
* nell'installazione delle dipendenze con NPM (`npm i`) compaiono moltissimi messaggi contenenti "ENOENT". Cancellare i files `package-lock.json` della sottocartella `standard\web\client\web-form`, e la cartella `C:\Users\[nome utente]\AppData\Roaming\npm-cache`

* nell'installazione delle dipendenze con NPM (`npm i`) compare un messaggio che indica la mancanza di un componente Kendo. Modificare il file `C:\Users\[nome utente]\.npmrc` rimuovendone le righe relative alla licenza Kendo.

* in caso di dubbi di situazione delle cartelle "sporca", provare a cancellare completamente la cartella `standard\web` e ripetere l'operazione.

* se la versione di Typescript non è quella corretta anche dopo averlo installato, potreste averne una vecchia versione installata da una precedente versione di Visual Studio, e la sua cartella si trova nel PATH *prima* di quella dove lo installa NPM.  
Modificare il PATH eliminando la cartella che punta al "vecchio" Typescript
