

# Requirements

Eseguire con attenzione l'installazione di tutti questi prerequisiti, verificando di avere le corrette versioni installate.  
Installare i prerequisiti **con privilegi di Amministratore**.


## Git (v2+) [link](https://git-scm.com/)

Versione **2.16.1** per Windows a 64bit - [Git-2.16.1.3-64-bit.exe](https://github.com/git-for-windows/git/releases/download/v2.16.1.windows.3/Git-2.16.1.3-64-bit.exe)

Versioni per altri Sistemi Operativi - [Downloads](https://git-scm.com/downloads)

## GitHub Desktop [link](https://desktop.github.com/)

Versione per Windows 64bit - [download](https://central.github.com/deployments/desktop/desktop/latest/win32)

Versione per macOS  - [download](https://central.github.com/deployments/desktop/desktop/latest/darwin)

Terminata l'installazione, seguire questa [guida](https://help.github.com/desktop/guides/getting-started-with-github-desktop/authenticating-to-github/#platform-windows) per autenticarsi con le credenziali di GitHub aziendali. Se non sono state ancora assegnate, chiederle.

## Node.js [link](https://nodejs.org/)
  
Versione **8.9.4 LTS** per Windows a 64bit - [download](https://nodejs.org/dist/v8.9.4/node-v8.9.4-x64.msi)

Versione **8.9.4 LTS** per MacOS - [download](https://nodejs.org/dist/v8.9.4/node-v8.9.4.pkg)
 
## NPM Packages

Tramite riga di comando installare i seguenti pacchetti.

Chi usa Windows può aprire una **Powershell** con privilegi da amministratore.

### Typescript [link](https://www.typescriptlang.org)

```shell
npm i -g "typescript@latest"
```

### Angular CLI [link](https://cli.angular.io/)
Angular Command Line Interface

```shell
npm i -g "@angular/cli"
```

### rimraf [link](https://www.npmjs.com/package/rimraf)
A shortcut to *rm -rf*

```shell
npm i -g rimraf
```

### .NET Core SDK [link](https://www.microsoft.com/net/)

Windows - [download](https://www.microsoft.com/net/download/thank-you/dotnet-sdk-2.1.4-windows-x64-installer)

macOS - [download](https://www.microsoft.com/net/download/thank-you/dotnet-sdk-2.1.4-macos-x64-installer)

Linux - [download](https://www.microsoft.com/net/download/thank-you/dotnet-sdk-2.1.4-linux-x64-binaries)

Others - [link](https://www.microsoft.com/net/download/)

### Componenti aggiuntivi di Visual Studio
Aprire l'Installer del Visual Studio 2017 (dalla funzione di ricerca di Windows, cercare "Visual Studio Installer"). Potrebbe richiedere l'aggiornamento dello stesso Installer o di Visual Studio)

Verificare di avere installato il seguente componente nella sezione "aspnet web development"
![Prerequisiti](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/Prerequisiti.png)

### Microarea Extension

Scaricare le **MicroareaExtension** per **VS2017** che si trovano online come da screenshots.
![Tools -> Extension and Updates](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/microarea-extension-1.png)
![Search 'microarea'](https://github.com/Microarea/Taskbuilder/blob/master/docs/img/microarea-extension-2.jpg)

In seguito, aprire una qualsiasi solution Mago o TB, e dal menu Microarea Tools, scegliere la voce **Create IIS virtual directory**

### IIS URL Rewrite 2.1 (Solo per utenti Windows con IIS) [link](https://www.iis.net/downloads/microsoft/url-rewrite)

Estensione IIS URL Rewrite - [download](http://www.microsoft.com/web/handlers/webpi.ashx?command=getinstallerredirect&appid=urlrewrite2)

### Visual Studio Code (Opzionale) [link](http://code.visualstudio.com/) 
Versione per Windows - [download](https://go.microsoft.com/fwlink/?Linkid=852157)

Versioni per altri Sistemi Operativi - [link](https://code.visualstudio.com/Download)
