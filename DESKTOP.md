# Sincronizzazione e Build ambiente di sviluppo Desktop

Questo documento descrive come preparare e aggiornare l'ambiente di sviluppo per utilizzare il menu web da parte di chi lavora solo in ambiente Desktop.

**ATTENZIONE**: la procedura non vale se sviluppate anche in ambiente web. Con questa procedura vengono cancellati eventuali cadaveri di cartelle legate alla parte web, quindi qualsiasi modifica alla parte web non pushata su git, ***con questo metodo andrà irrimediabilmente persa***. 

* [Prerequisiti](#prerequisiti)
* [Script](#script)
* [Troubleshooting](#troubleshooting)

## Prerequisiti

Eseguire con attenzione l'installazione di tutti questi prerequisiti, verificando di avere le corrette versioni installate.  
Installare i prerequisiti **con privilegi di Amministratore**.

In condizioni normali l'installazione richiede 30' - 45'.

### [Git](http://git-scm.com)
Installare la [**GitHub app** per Windows](http://windows.github.com).

Dopo l'installazione, effettuare la login con le proprie credenziali di GitHub aziendali. Se non sono state ancora assegnate, chiederle.

Per saperne di più: [GitHub's Guide to Installing Git](https://help.github.com/articles/set-up-git).

### [Node.js](http://nodejs.org), (version `>=6.11`)
  Installare la 'LTS' (Long Term Support). 
  
  Node.js is used to run a development web server,
  run tests, and generate distributable files. We also use Node's Package Manager, `npm`
  which comes with Node. Depending on your system, you can install Node either from
  source or as a pre-packaged bundle.
  
  Se lo si ha già, verificare la versione da riga di comando: `node -v`  
  Verificare anche la versione di NPM con il comando `npm -v`, deve essere superiore alla 5.0.

### [Typescript](https://www.typescriptlang.org), (version `>= 2.4`) 
A superset of JavaScript that compiles to clean JavaScript output.

Per installarlo, da riga di comando

```shell
# global installation typescript
$ npm i -g "typescript@latest"
```

Se lo si ha già, verificare la versione da riga di comando: `tsc -v`

### [Angular CLI](https://cli.angular.io/), (version `>= 1.4`)
A command line interface for Angular

Per installarlo, da riga di comando

```shell
# global installation Angular CLI
$ npm i -g "@angular/cli"
```

Se lo si ha già, verificare la versione da riga di comando: `ng -v`

### [rimraf]
A shortcut to *rm -rf*

Per installarlo, da riga di comando

```shell
# global installation rimraf
$ npm i -g rimraf
```

### .NET Core

Scaricare ed installare il [**.NET Core 2.0 SDK**](https://www.microsoft.com/net/download/core)

### Componenti aggiuntivi di Visual Studio
Aprire l'Installer del Visual Studio 2017 (dalla funzione di ricerca di Windows, cercare "Visual Studio Installer"). Potrebbe richiedere l'aggiornamento dello stesso Installer o di Visual Studio)

Verificare di avere installato il seguente componente nella sezione "aspnet web development"
![Prequisiti](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/Prerequisiti.png)

### Microarea Extension

Scaricare le **MicroareaExtension** per **VS2017** che si trovano online come da screenshots.
![Tools -> Extension and Updates](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/microarea-extension-1.png)
![Search 'microarea'](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/microarea-extension-2.jpg)

In seguito, aprire una qualsiasi solution Mago o TB, e dal menu Microarea Tools, scegliere la voce **Create IIS virtual directory**

### (Opzionale) [Visual Studio Code](http://code.visualstudio.com/) 
Source code editor.

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
