# Requirements

Eseguire con attenzione l'installazione di tutti questi prerequisiti, verificando di avere le corrette versioni installate.  
Installare i prerequisiti **con privilegi di Amministratore**.


### [Git](http://git-scm.com)
Installare la [**GitHub app** per Windows](http://windows.github.com).

Dopo l'installazione, effettuare la login con le proprie credenziali di GitHub aziendali. Se non sono state ancora assegnate, chiederle.

Per saperne di più: [GitHub's Guide to Installing Git](https://help.github.com/articles/set-up-git).

### [Node.js](http://nodejs.org), (version `>=6.11`)
  Installare la 'LTS' (Long Term Support) a **64bit**
  
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

### [rimraf](https://www.npmjs.com/package/rimraf)
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
![Prerequisiti](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/Prerequisiti.png)

### Microarea Extension

Scaricare le **MicroareaExtension** per **VS2017** che si trovano online come da screenshots.
![Tools -> Extension and Updates](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/microarea-extension-1.png)
![Search 'microarea'](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/microarea-extension-2.jpg)

In seguito, aprire una qualsiasi solution Mago o TB, e dal menu Microarea Tools, scegliere la voce **Create IIS virtual directory**

### (Solo per utenti Windows con IIS)[IIS URL Rewrite 2.1](https://www.iis.net/downloads/microsoft/url-rewrite)

[Install extension](http://www.microsoft.com/web/handlers/webpi.ashx?command=getinstallerredirect&appid=urlrewrite2)

Scaricare ed installare il modulo aggiuntivo per IIS per URL Rewrite necessario al corretto funzionamento del sistema di routing Angular con IIS

### (Opzionale) [Visual Studio Code](http://code.visualstudio.com/) 
Source code editor.
