# Building and Running TaskBuilder Web

This document describes how to set up your development environment to build and test TaskBuilder Web.
It also explains the basic mechanics of using `git`, `node`, and `npm`.

* [Prerequisite Software](#prerequisite-software)
* [Getting the Sources](#getting-the-sources)
* [Install packages](#install-packages)
* [Building](#building)
* [Running](#running)
* [Extra](#extra)
	
## Prerequisite Software
Before you can build and test TaskBuilder Web, you must install and configure the
following products on your development machine:

* [Git](http://git-scm.com) and/or the **GitHub app** (for [Mac](http://mac.github.com) or
  [Windows](http://windows.github.com)); [GitHub's Guide to Installing
  Git](https://help.github.com/articles/set-up-git) is a good source of information.

* [Node.js](http://nodejs.org), (version `>=6.9.1`) which is used to run a development web server,
  run tests, and generate distributable files. We also use Node's Package Manager, `npm`
  (version `>=3.5.3`), which comes with Node. Depending on your system, you can install Node either from
  source or as a pre-packaged bundle.

* [Typescript](https://www.typescriptlang.org), (version `>= 2.1.6`) a superset of JavaScript that compiles to clean JavaScript output.

```shell
# global installation typescript
$ npm install -g typescript@next
```

* [Visual Studio Typescript Extension](https://www.microsoft.com/en-us/download/details.aspx?id=48593)

* [Angular CLI](https://cli.angular.io/), (version `>= 1.0.0-beta.31`) a command line interface for Angular
```shell
# global installation Angular CLI
$ npm install -g @angular/cli
```

* .NET Core

  * Download and install **.NET Core 1.1.1 runtime (Current)** - [link](https://www.microsoft.com/net/download/core#/runtime)
  * Download and install **.NET Core 1.1.0 SDK Preview 2.1 build 3177** - [link](https://github.com/dotnet/core/blob/master/release-notes/download-archives/1.1-preview2.1-download.md)

* [Visual Studio Code](http://code.visualstudio.com/) source code editor

### Taskbuilder Cli

Taskbuilder Cli is a command line tool for Taskbuilder Web developers. 

```shell
# install globally
npm install -g "@taskbuilder/cli"
```

It's available globally but for building and running operations you have to be inside a Taskbuilder Web project.


## Getting the Sources

Clone the repository into the <installation path>/standard with alias **web**:

```shell
# Move to the <installation path>/standard
# for example:
$ cd /development/standard

# Taskbuilder Cli clone
$ tb clone
```

**tb clone** command is a shortcut to the git clone command: `git clone https://github.com/Microarea/Taskbuilder.git web`.


## Install packages

Before building projects, needs to install packages for the Angular client and .NET Core servers.

Angular projects need `npm install` inside project folder and .NET Core needs `dotnet restore`.

With the Taskbuilder Cli:
```shell
# Inside the <installation path>/standard/web
$ tb install
```
Will ask you your Telerik account credentials for [Kendo UI Library](https://github.com/Microarea/Taskbuilder/blob/master/docs/KENDO.md):

```
Username: eitri
Password: Microarea.2017
Email: eitri@microarea.it
```


## Building

To build TaskBuilder Web, you need to build both the .NET Core platform and the Angular front-end project.

With the Taskbuilder Cli:
```shell
# Inside the <installation path>/standard/web
$ tb build
```

**tb build** is a shortcut for the Angular `ng build` and .NET Core `dotnet build`.

## Running

### TbLoaderService c++

`<installation_path>/standard/taskbuilder/TaskBuilderNet/Microarea.TaskBuilderNet.TBLoaderService/bin/Debug/TbLoaderService.exe`
*In Windows 10, run as administrator*

With the Taskbuilder Cli:
```shell
# Inside a TBWeb project
$ tb run
```

Options
```shell
# Run only .NET Core web-server project
tb run --server # default port 5000
tb run --server 5001 # to specify server port

# Run only Angular web-form project
tb run --client # default port 4200
tb run --client 4201 # to specify server port
```

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
