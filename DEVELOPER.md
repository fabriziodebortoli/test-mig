# Building and Running TaskBuilder Web

This document describes how to set up your development environment to build and test TaskBuilder Web.
It also explains the basic mechanics of using `git`, `node`, and `npm`.

* [Prerequisite Software](#prerequisite-software)
* [Getting the Sources](#getting-the-sources)
* [Building](#building)
* [Running](#running)
* [Visual Studio 2015](#using-visual-studio-2015)
* [Generate ERP Modules](#generate-erp-modules)
	
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

* [.NET Core](https://www.microsoft.com/net/download/core#/current), (version `>= 1.1`) Download and install **SDK - Installer** and the **Visual Studio 2015 Tools (Preview 2)**

* [Visual Studio Code](http://code.visualstudio.com/) source code editor


## Getting the Sources

Clone the repository into the <installation path>/standard with alias **web**:

```shell
# Move to the <installation path>/standard
# for example:
$ cd /development/standard

# Clone GitHub repository:
$ git clone https://github.com/Microarea/Taskbuilder.git web
```

### 

It could happen that, building ERP solution before cloning, there is already the 'web' folder because of TypeScript/HTML generation by the TbJson process:
```shell
fatal: destination path 'web' already exists and is not an empty directory.
```
you must manually delete the folder 'web':
```shell
$ del web 
```
and re-execute git clone command.

After that, you will need to [execute TbJson process to generate TS/HTML](#generate-erp-modules).

## VS Code Recommended Extensions

Install **VS Code** recommended extensions

```shell
# Move to the <installation path>/standard/web
$ cd /development/standard/web

# Open VS Code
$ code .
```

Follow the instructions: https://code.visualstudio.com/docs/editor/extension-gallery#_workspace-recommended-extensions)


## Building

To build TaskBuilder Web, you need to build both the .NET Core platform and the Angular front-end project.

### Server .NET Core

```shell
# Move to the <installation path>/standard/web/server
$ cd c:/development/standard/web/server # example

# Restore packages
$ ./restore.bat # Windows
$ ./restore.sh # Unix 

# Build web-server project
$ dotnet build ./web-server/project.json 
```

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

### Front-end Angular 

Building `web-form` Angular project, the `dist` folder will be moved into the `web-server\wwwroot` folder of the .NET Core server.

```shell
# Move to the <installation path>/standard/web/client/web-form
$ cd c:/development/standard/web/client/web-form # example

# Restore packages
$ npm install

# Build
$ ng build
```

## Running

### TbLoaderService c++

`<installation_path>/standard/taskbuilder/TaskBuilderNet/Microarea.TaskBuilderNet.TBLoaderService/bin/Debug/TbLoaderService.exe`
*In Windows 10, run as administrator*


### .NET Core server
```shell
Move to the <installation path>/standard/web/server/web-server
$ dotnet run
```

## Using Visual Studio 2015

Developers can also use the VS solution 
*&lt;installation path&gt;/standard/web/server/web-server.sln*

**Set web-server as StartUp Project** and **run WebServer** instead of IIS Express.


 or **ReBuild ERP solution**
