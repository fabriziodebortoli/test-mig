# Sincronizzazione e Build ambiente di sviluppo Desktop

Questo documento descrive come preparare l'ambiente di sviluppo Desktop con il menu nuovo

* [Prerequisiti](#prerequisiti)
* [Script](#script)
* [Troubleshooting](#troubleshooting)

## Prerequisiti

Eseguire con attenzione l'installazione di tutti questi prerequisiti, verificando di avere le corrette versioni installate.

In condizioni normali l'installazione richiede 30' - 45'.

### [Git](http://git-scm.com)
Installare la [**GitHub app** per Windows](http://windows.github.com).

Per saperne di più: [GitHub's Guide to Installing Git](https://help.github.com/articles/set-up-git).

### [Node.js](http://nodejs.org), (version `>=6.11`)
  Installare la 'LTS' (Long Term Support). 
  
  Node.js is used to run a development web server,
  run tests, and generate distributable files. We also use Node's Package Manager, `npm`
  which comes with Node. Depending on your system, you can install Node either from
  source or as a pre-packaged bundle.
  
  Se lo si ha già, verificare la versione da riga di comando: `node -v`

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

Scaricare le **MicroareaExtension** per **vs2017** che si trovano online come da screenshots.
![Tools -> Extension and Updates](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/microarea-extension-1.png)
![Search 'microarea'](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/microarea-extension-2.jpg)

In seguito, aprire una qualsiasi solution Mago o TB, e dal menu Microarea Tools, scegliere la voce **Create IIS virtual directory**

### (Opzionale) [Visual Studio Code](http://code.visualstudio.com/) 
Source code editor.

## Script 

Scaricare uno degli script seguenti ed eseguire come Amministratore, soprattutto se in ambiente Windows 10.
Entrambi gli script, alla partenza, vi chiederanno di specificare il path di installazione della vostra cartella di sviluppo (ad esempio “c:\development”)

### Clone, Install e Build
Il file **M4-Clone.bat** ha lo scopo di clonare, installare e buildare tutti i componenti provenienti da git.

Si può scaricare da qui: [download](https://github.com/Microarea/Taskbuilder/blob/master/docs/script/M4-Clone.bat?raw=true) (tasto destro del mouse -> Salva con nome)

Oppure nel repository  in ```<InstallationPath>/Standard/web/docs/script/M4-Client.bat```

Questo script come prima cosa cancella eventuali cadaveri di cartelle legate alla parte web, quindi qualsiasi modifica alla parte web non pushata su git, ***con questo script andrà irrimediabilmente persa***.


### Sincronizzazione e Build
Il file **M4-Get-Build.bat** effettua una sincronizzata pulita, più le relative build della parte angular  e netcore.

Si può scaricare da qui: [download](https://github.com/Microarea/Taskbuilder/blob/master/docs/script/M4-Get-Build.bat?raw=true) (tasto destro del mouse -> Salva con nome)

Oppure nel repository in ```<InstallationPath>/Standard/web/docs/script/M4-Get-Build.bat```



### Build Completa

Il file **M4-Build-Complete.bat** effettua invece tutte  le operazioni necessarie a sincronizzare l’intero ambiente di lavoro:

 1. Pulizia completa della cartella \apps
 2. Get latest (compreso di tentativo di automerge)  di taskbuilder, erp 
 3. Build in debug di taskbuilder e erp
 4. Sincronizzazione da git e compilata dei sorgenti web. (effettua tutte le operazioni presenti nel “M4-Get-Build.bat”

Si può scaricare da qui: [download](https://github.com/Microarea/Taskbuilder/blob/master/docs/script/M4-Build-Complete.bat?raw=true) (tasto destro del mouse -> Salva con nome)

Oppure, nel repository in ```<InstallationPath>/Standard/web/docs/script/M4-Build-Complete.bat```

N.B.: nello script è presente, anche se commentato con un “rem” , tutta la parte relativa ad MDC:  se normalmente sincronizzate anche questa applicazione, dovete semplicemente togliere i relativi “rem” dalle righe pertinenti ad mdc

E' utile lanciare questo script tutte le sere, magari anche con una schedulazione di windows, in modo da avere l’intero ambiente pronto all’uso la mattina dopo (ovviamente a meno di errori di compilazione dovuti a checkin infausti)

## Troubleshooting
* messaggio di errore: "Not in a development path: it should contains the 'standard' subfolder". Se si sta lanciando il batch file da windows explorer, il sistema non ha una current directory valida; aprire un prompt e dare un "cd" all'interno della directory di sviluppo, o lanciare il batch da un prompt di comandi.

* nell'installazione delle dipendenze con NPM (`npm i`) compaiono moltissimi messaggi contenenti "ENOENT". Cancellare i files `package-lock.json` della sottocartella `standard\web\client\web-form`, e la cartella `C:\Users\[nome utente]\AppData\Roaming\npm-cache`
