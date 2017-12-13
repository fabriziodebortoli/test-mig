# Building and Running TaskBuilder Web

This document describes how to set up your development environment to build and test TaskBuilder Web.
It also explains the basic mechanics of using `git`, `node`, and `npm`.

* [Prerequisite Software](#prerequisite-software)
* [Getting the Sources](#getting-the-sources)
* [Install packages](#install-packages)
* [Running](#running)
* [Extra](#extra)
	
## Prerequisite Software
Before you can build and test TaskBuilder Web, you must install and configure the
following products on your development machine:

* [Git](http://git-scm.com) and/or the **GitHub app** (for [Mac](http://mac.github.com) or
  [Windows](http://windows.github.com)); [GitHub's Guide to Installing
  Git](https://help.github.com/articles/set-up-git) is a good source of information.

* [Node.js](http://nodejs.org), (version **LTS**) which is used to run a development web server,
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

## Getting the Sources

Clone the repository into the <installation path>/standard with alias **web**:

```shell
# Move to the <installation path>/standard
# for example:
$ cd /development/standard

# Repo clone
$ git clone https://github.com/Microarea/Taskbuilder.git web
```


## Install packages

Before building projects, needs to install packages for the Angular client and .NET Core servers.

Angular projects need `npm install` inside project:
```shell
# Inside the <installation path>/standard/web/client/web_form
$ npm install
```

## Running

### TbLoaderService c++

Angular web application:
```shell
# Inside the <installation path>/standard/web/client/web_form
$ ./run.bat
```

*run.bat* script is a shortcut to execute *ng serve* reserving 5GB of memory to NodeJS, 
```node --max_old_space_size=5120 "node_modules\@angular\cli\bin\ng" serve```

and .NET Core server

```<installation_path>/standard/web/server/web-server/dotnet run```

and TbLoaderService

```<installation_path>/standard/taskbuilder/TaskBuilderNet/Microarea.TaskBuilderNet.TBLoaderService/bin/Debug/TbLoaderService.exe - In Windows 10, run as administrator```

## Extra

### Using Visual Studio 2015

Developers can also use the VS solution 
*&lt;installation path&gt;/standard/web/server/web-server.sln*

**Set web-server as StartUp Project** and **run WebServer** instead of IIS Express.


 or **ReBuild ERP solution**
 
### VS Code Recommended Extensions

Install **VS Code** recommended extensions

```shell
# Move to the <installation path>/standard/web
$ cd /development/standard/web

# Open VS Code
$ code .
```

Follow the instructions: https://code.visualstudio.com/docs/editor/extension-gallery#_workspace-recommended-extensions)


### Generate ERP Modules

Building ERP solution, the **TbJson** process generate TypeScript and HTML files into the folder:

`<installation path>/standard/web/client/web-form/src/app/applications` 

To re-generate modules, execute the *tbjson* script:

```shell
Move to the `<installation path>/standard/web/script/`
cd c:/development/standard/web/script # example

$ ./tbjson.bat # Windows
$ ./tbjson.sh # Unix
```
