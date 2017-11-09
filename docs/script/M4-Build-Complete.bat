@echo off
set /p installationpath="Enter Installation path <c:\development>: "

if [%installationpath%] == [] (
	set installationpath=c:\development
)

if not exist %installationpath% (
	@echo "Invalid installation path (ex: 'c:\development')"
	exit
)

iisreset

rem set Installation=c:\development\
set Installation=%installationpath%\

set Build=Debug
set VisualStudioPath=C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\

set GetSorgenti="%VisualStudioPath%tf.exe"
set BuildSorgenti="%VisualStudioPath%devenv.com"
erase %Installation%*.log
erase %Installation%Apps\*.* /S /Q
erase %Installation%Standard\TaskBuilder\TaskBuilder3.VC.db
erase %Installation%Standard\Applications\ERP\ERP.sdf
rem erase %Installation%Standard\Applications\MDF\MDF.sdf

%BuildSorgenti% "%Installation%Standard\TaskBuilder\TaskBuilder3.sln" /clean %Build%
%BuildSorgenti% "%Installation%Standard\Applications\ERP\ERP.sln" /clean %Build%
rem %BuildSorgenti% "%Installation%Standard\Applications\MDC\MDC.sln" /clean %Build%

%GetSorgenti% get %Installation%Standard\TaskBuilder /recursive > %Installation%GETTaskBuilder.log
%GetSorgenti% get %Installation%Standard\Applications\ERP /recursive > %Installation%GETERP.log
rem %GetSorgenti% get %Installation%Standard\Applications\MDC /recursive > %Installation%GETMDC.log

%GetSorgenti% resolve %Installation%Standard\TaskBuilder /recursive /auto:automerge
%GetSorgenti% resolve %Installation%Standard\Applications\ERP /recursive /auto:automerge
rem %GetSorgenti% resolve %Installation%Standard\Applications\MDC /recursive /auto:automerge

%BuildSorgenti% "%Installation%Standard\TaskBuilder\TaskBuilder3.sln" /out %Installation%BUILDTaskBuilder3.log /build %Build%
%BuildSorgenti% "%Installation%Standard\Applications\ERP\ERP.sln" /out %Installation%BUILDERP.log /build %Build%
rem %BuildSorgenti% "%Installation%Standard\Applications\MDC\MDC.sln" /out %Installation%BUILDMDC.log /build %Build%


cd %Installation%Standard\web

git pull

cd %Installation%Standard\web\client\web-form\ 
call clean.bat

call npm i >> %Installation%1_npm_install.log

cd %Installation%Standard\web\client\web-form\
node --max_old_space_size=5120 "node_modules\@angular\cli\bin\ng" build --env=desktop --no-sourcemaps --preserve-symlinks --output-path="%Installation%Standard\TaskBuilder\WebFramework\M4Client" >> %Installation%2_ng_build.log

cd %Installation%Standard\web\server\web-server
dotnet restore
dotnet publish --framework netcoreapp2.0 --output "%Installation%Standard\TaskBuilder\WebFramework\M4Server" --configuration release >> %Installation%3_dotnet_publish.log

%Installation%Apps\ClickOnceDeployer\ClickOnceDeployer.exe

@echo on
