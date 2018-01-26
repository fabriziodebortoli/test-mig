@echo off

REM ===========================================================================
REM Determines if launched by a command prompt or Windows Explorer
REM ===========================================================================
SET FROMCMD=0
IF /I ""%COMSPEC%" " == "%CMDCMDLINE%"   SET FROMCMD=1
IF /I "%COMSPEC%" == "%CMDCMDLINE%" SET FROMCMD=1

REM ===========================================================================
REM Check for admin rights
REM ===========================================================================
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    ECHO.
    ECHO.This batch requires administrator rights, use "Run as Administrator"
    ECHO.
    GOTO _failed_
)

REM ===========================================================================
REM Check if the current folder is within a TB development path: a "standard" 
REM subfolder must appear somewhere
REM ===========================================================================

set DevPath=%cd%

REM Look for "standard" and keep only the beginning of the path
IF /i "%DevPath:standard=%" NEQ "%DevPath%" (
    setlocal enabledelayedexpansion
    set "find=*standard"
    call set delete=%%DevPath:!find!=%%
    call set DevPath=%%DevPath:\standard!delete!=%%
    goto _start_
)

REM Maybe "standard" is just inside the current folder
IF /i EXIST %DevPath%\Standard\* (
    goto _start_
)

set BatchPath=%~dp0
REM For right-click from Windows Explorer, try the path the batch file is in
IF %FROMCMD% EQU 0 (
    IF /i "%BatchPath:standard=%" NEQ "%BatchPath%" (
        setlocal enabledelayedexpansion
        set "find=*standard"
        call set delete=%%BatchPath:!find!=%%
        call set DevPath=%%BatchPath:\standard!delete!=%%
        goto _start_
    )
) 

ECHO.
ECHO.Not in a development path: it should contains the "standard" subfolder
ECHO.
GOTO _failed_

:_start_

REM ===========================================================================
REM Check the needed tools are installed: GIT, rimraf, Npm, Angular CLI, Typescript,
REM Node and .NET Core 
REM ===========================================================================

set FAILED=0
WHERE git > NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
    ECHO.
    ECHO GIT has not been installed
    ECHO.
    SET FAILED=1 
)

WHERE rimraf > NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
    ECHO.
    ECHO rimraf has not been installed
    ECHO.
    SET FAILED=1 
)

WHERE npm > NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
    ECHO.
    ECHO Npm has not been installed
    ECHO.
    SET FAILED=1 
)

WHERE node > NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
    ECHO.
    ECHO Node.js has not been installed
    ECHO.
    SET FAILED=1 
)

WHERE ng > NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
    ECHO.
    ECHO Angular CLI has not been installed
    ECHO.
    SET FAILED=1 
)

WHERE tsc > NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
    ECHO.
    ECHO Typescript has not been installed
    ECHO.
    SET FAILED=1 
)

WHERE dotnet > NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
    ECHO.
    ECHO .NET Core has not been installed
    ECHO.
    SET FAILED=1 
)

IF NOT EXIST %DevPath%\Apps\ClickOnceDeployer\ClickOnceDeployer.exe (
    ECHO.
    ECHO TaskBuilder has not been properly compiled
    ECHO.
    SET FAILED=1 
)

IF %FAILED% NEQ 0 (
    GOTO _failed_
)

REM ===========================================================================
REM Perform the steps to build the web part of the desktop version
REM ===========================================================================

cd %DevPath%\Standard\Taskbuilder\script
ECHO.ng-clean.bat
call ng-clean.bat 

@ECHO ON

%DevPath%\Standard\TaskBuilder\Framework\TbUtility\TbJson\tbjson.exe /resetRoutes %DevPath%\Standard\ 

@cd %DevPath%\Standard\Taskbuilder\client\web-form\ 

call npm i --no-save >> %DevPath%\5_npm_install.log

@cd %DevPath%\Standard\Taskbuilder\client\web-form\
node --max_old_space_size=5120 "node_modules\@angular\cli\bin\ng" build --env=desktop --no-sourcemaps --preserve-symlinks --output-path="%DevPath%\Standard\TaskBuilder\WebFramework\M4Client" >> %DevPath%\7_ng_build.log
node --max_old_space_size=9120 "node_modules\@angular\cli\bin\ng" build --preserve-symlinks --output-path="%DevPath%\Standard\TaskBuilder\WebFramework\M4Web" >> %DevPath%\7_ng_build-web.log

@cd %DevPath%\Standard\Taskbuilder\server\web-server
dotnet restore
iisreset
dotnet publish --framework netcoreapp2.0 --output "%DevPath%\Standard\TaskBuilder\WebFramework\M4Server" --configuration release >> %DevPath%\8_dotnet_publish.log

%DevPath%\Apps\ClickOnceDeployer\ClickOnceDeployer.exe

%DevPath%\Apps\ClickOnceDeployer\ClickOnceDeployer.exe Deploy /root %DevPath%\Apps /clean true /version debug

%DevPath%\Apps\ClickOnceDeployer\ClickOnceDeployer.exe updatedeployment /root %DevPath%\Apps /version debug

robocopy %DevPath%\Standard\Taskbuilder\WebFramework\M4Client\assets\ %DevPath%\Standard\Taskbuilder\WebFramework\M4Web\assets\ config.json
robocopy %DevPath%\Standard\Taskbuilder\WebFramework\client\web-form\ %DevPath%\Standard\Taskbuilder\WebFramework\M4Web\ web.config