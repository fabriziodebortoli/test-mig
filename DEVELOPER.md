# Building and Running TaskBuilder Web

This document describes how to set up your development environment to build and test Angular.
It also explains the basic mechanics of using `git`, `node`, and `npm`.

* [Prerequisite Software](#prerequisite-software)
* [Getting the Sources](#getting-the-sources)
* [VS Code Recommended Extensions](#vs-code-recommended-extensions)
	
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
npm install -g typescript@next
```

* [Visual Studio Typescript Extension](https://www.microsoft.com/en-us/download/details.aspx?id=48593)

* [Angular CLI](https://cli.angular.io/), (version `>= 1.0.0-beta.31`) a command line interface for Angular
```shell
# global installation Angular CLI
npm install -g @angular-cli
```

* [.NET Core](https://www.microsoft.com/net/download/core#/current), (version `>= 1.1`) Download and install **SDK - Installer** and the **Visual Studio 2015 Tools (Preview 2)**

* [Visual Studio Code](http://code.visualstudio.com/) source code editor


## Getting the Sources

Clone the repository into the ~/development/standard with alias **Web**:

```shell
# Move to the ~/development/standard
# for example:
cd c:/development/standard

# Clone GitHub repository:
clone https://github.com/Microarea/Taskbuilder.git Web
```

## VS Code Recommended Extensions

Install **VS Code** recommended extensions

```shell
# Move to the ~/development/standard/Web
cd c:/development/standard/Web

# Open VS Code
code .
```

Follow the instructions: https://code.visualstudio.com/docs/editor/extension-gallery#_workspace-recommended-extensions)









