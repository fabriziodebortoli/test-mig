@echo off
cls


echo Checking requirements...
echo.
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
    echo.NET Core has not been installed
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

echo Requirements OK...

echo.

setlocal EnableDelayedExpansion

echo Checking local/server versions...
set /p serverVersion=<server-version.txt
cd %DevPath%\Standard\Taskbuilder\script

IF NOT EXIST "local-version.txt" (
	
    echo Clean needed
    echo.
    call ng-clean.bat 	
	cd %DevPath%\Standard\Taskbuilder\client\web-form\     

    
    echo NPM INSTALL
    echo.
	call npm i --no-save >> %DevPath%\5_npm_install.log
    echo NPM INSTALL Completed - See log: %DevPath%\5_npm_install.log
    
    echo.
	
	cd %DevPath%\Standard\Taskbuilder\script
	echo >local-version.txt !serverVersion!

) ELSE (

	set /p serverVersion=<server-version.txt
	set /p localVersion=<local-version.txt

	IF !serverVersion! GTR  !localVersion! (
		echo Clean needed
        echo.
        call ng-clean.bat 	
        cd %DevPath%\Standard\Taskbuilder\client\web-form\     

        
        echo NPM INSTALL
        echo.
        call npm i --no-save >> %DevPath%\5_npm_install.log
        echo NPM INSTALL Completed - See log: %DevPath%\5_npm_install.log
        
        echo.

		cd %DevPath%\Standard\Taskbuilder\script
		ECHO >local-version.txt !serverVersion!
	) 
)

echo.

echo TsJson execution...
echo.
call TbJson.bat
echo TsJson completed...


echo.
@cd %DevPath%\Standard\Taskbuilder\client\web-form\

echo Building Angular M4Client... 
echo.
echo NON CHIUDETE QUESTA FINESTRA, OPERAZIONE MOLTO LUNGA IN CORSO...
echo.
node --max_old_space_size=5120 "node_modules\@angular\cli\bin\ng" build --env=desktop --no-sourcemaps --preserve-symlinks --output-path="%DevPath%\Standard\TaskBuilder\WebFramework\M4Client" >> %DevPath%\7_ng_build.log
echo.
echo Build M4Client completed - See log: %DevPath%\7_ng_build.log
echo.

echo ClickOneDeployer - Generazione config.json...
echo.
%DevPath%\Apps\ClickOnceDeployer\ClickOnceDeployer.exe
echo.

set buildWeb=true
IF [%~2]==[-skipM4Web] IF NOT [%~2]==[] set buildWeb=false
if %buildWeb% == true (

    echo Building Angular M4Web...
    echo.
    echo NON CHIUDETE QUESTA FINESTRA, OPERAZIONE MOLTO LUNGA IN CORSO...
    echo.
    node --max_old_space_size=9120 "node_modules\@angular\cli\bin\ng" build --preserve-symlinks --output-path="%DevPath%\Standard\TaskBuilder\WebFramework\M4Web" >> %DevPath%\7_ng_build-web.log
    echo.
    echo Build M4Web completed - See log: %DevPath%\7_ng_build-web.log


    echo Copia in corso di config.json da M4Client a M4Web
    echo.
    robocopy %DevPath%\Standard\Taskbuilder\WebFramework\M4Client\assets\ %DevPath%\Standard\Taskbuilder\WebFramework\M4Web\assets\ config.json
    echo.

    echo Copia in corso di web.config da web-form a M4Web
    echo.
    robocopy %DevPath%\Standard\Taskbuilder\client\web-form\ %DevPath%\Standard\Taskbuilder\WebFramework\M4Web\ web.config
    echo.

)

echo.

@cd %DevPath%\Standard\Taskbuilder\server\web-server
echo.

echo Restoring DotNet project...
echo.
dotnet restore
echo.

echo iisreset
echo.
iisreset
echo.

echo Publishing DotNet project...
echo.
dotnet publish --framework netcoreapp2.0 --output "%DevPath%\Standard\TaskBuilder\WebFramework\M4Server" --configuration release >> %DevPath%\8_dotnet_publish.log
echo.
echo Dotnet project published - See log: %DevPath%\8_dotnet_publish.log

echo.


if "%~1"=="-skipcod" (goto end)

echo ClickOneDeployer - Manifest per macchine build 1...
echo.
%DevPath%\Apps\ClickOnceDeployer\ClickOnceDeployer.exe Deploy /root %DevPath%\Apps /clean true /version debug
echo.
echo ClickOneDeployer - Manifest per macchine build 2...
echo.
%DevPath%\Apps\ClickOnceDeployer\ClickOnceDeployer.exe updatedeployment /root %DevPath%\Apps /version debug
echo.


:end

echo.

echo build-web-deploy.bat terminato

echo.

IF EXIST %DevPath%\Standard\Taskbuilder\WebFramework\M4Client\ (
	IF EXIST %DevPath%\Standard\Taskbuilder\WebFramework\M4Server\  (
		exit /b 0
		)

) ELSE (
	rem se non trova m4client o m4server, qualcosa � andato storto, exit con error code 	exit /b 1
)
