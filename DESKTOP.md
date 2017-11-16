# Sincronizzazione e Build ambiente di sviluppo Desktop

Questo documento descrive come preparare l'ambiente di sviluppo Desktop con il menu nuovo

* [Prerequisite Software](#prerequisite-software)
* [Microarea Extension](#microarea-extension)
* [Script](#script)

## Prerequisite Software

Before you can build and test, you must install and configure the following products on your development machine:

* [Git](http://git-scm.com) and/or the **GitHub app** (for [Mac](http://mac.github.com) or
  [Windows](http://windows.github.com)); [GitHub's Guide to Installing
  Git](https://help.github.com/articles/set-up-git) is a good source of information.

* [Node.js](http://nodejs.org), (version `>=6.11`) which is used to run a development web server,
  run tests, and generate distributable files. We also use Node's Package Manager, `npm`
  which comes with Node. Depending on your system, you can install Node either from
  source or as a pre-packaged bundle.

* [Typescript](https://www.typescriptlang.org), (version `>= 2.4`) a superset of JavaScript that compiles to clean JavaScript output.

```shell
# global installation typescript
$ npm i -g "typescript@latest"
```

* [Angular CLI](https://cli.angular.io/), (version `>= 1.4`) a command line interface for Angular
```shell
# global installation Angular CLI
$ npm i -g "@angular/cli"
```

* [rimraf] - Shortcut to *rm -rf*
```shell
# global installation rimraf
$ npm i -g rimraf
```

* .NET Core

  * Download and install **.NET Core 2.0 SDK** - [link](https://www.microsoft.com/net/download/core)

* Verificare di avere installato il seguente componente nella sezione "aspnet web development"
![Prequisiti](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/Prerequisiti.png)


* [Visual Studio Code](http://code.visualstudio.com/) source code editor

* [Visual Studio Typescript Extension](https://www.microsoft.com/en-us/download/details.aspx?id=48593) - Only for VS 2015 users


## Microarea Extension

Scaricare le **MicroareaExtension** per **vs2017** che si trovano online come da screenshots.
![Tools -> Extension and Updates](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/microarea-extension-1.png)
![Search 'microarea'](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/microarea-extension-2.jpg)

In seguito, dal menu Microarea Tools, scegliere la voce **Create IIS virtual directory**

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

### Troubleshooting
* messaggio di errore: "Not in a development path: it should contains the 'standard' subfolder". Se si sta lanciando il batch file da windows explorer, il sistema non ha una current directory valida; aprire un prompt e dare un "cd" all'interno della directory di sviluppo, o lanciare il batch da un prompt di comandi.
