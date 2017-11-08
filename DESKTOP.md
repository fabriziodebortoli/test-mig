# Sincronizzazione e Build ambiente di sviluppo Desktop

Questo documento descrive come preparare l'ambiente di sviluppo Desktop con il menu nuovo

* [Prerequisite Software](#prerequisite-software)
* [Microarea Extension](#microarea-extension)
* [Script](#Script)

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
Scaricare il file [M4-Clone.bat](https://github.com/Microarea/Taskbuilder/blob/master/docs/script/M4-Clone.bat) (tasto destro mouse -> Salva con nome) che ha lo scopo di clonare, installare e buildare tutti i componenti provenienti da git.
Questo script come prima cosa cancella eventuali cadaveri di cartelle legate alla parte web, quindi qualsiasi modifica alla parte web non pushata su git, ***con questo script andrà irrimediabilmente persa***.


### Sincronizzazione e Build
Scaricare il file [M4-Get-Build.bat](https://github.com/Microarea/Taskbuilder/blob/master/docs/script/M4-Get-Build.bat) (tasto destro mouse -> Salva con nome)  effettua una sincroniccata pulita, più le relative build della parte angular  e netcore.


