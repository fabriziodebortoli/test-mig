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

@echo on

cd %Installation%Standard\
rd /s /q web


git clone https://github.com/Microarea/Taskbuilder.git web

cd %Installation%Standard\web\client\web-form\ 
call npm i >> %Installation%5_npm_install.log

%Installation%Standard\TaskBuilder\Framework\TbUtility\TbJson\tbjson.exe /ts %Installation%Standard\ >> %Installation%6_tbjson.log

cd %Installation%Standard\web\client\web-form\
node --max_old_space_size=5120 "node_modules\@angular\cli\bin\ng" build --env=desktop --no-sourcemaps --preserve-symlinks --output-path="%Installation%Standard\TaskBuilder\WebFramework\M4Client" >> %Installation%7_ng_build.log

cd %Installation%Standard\web\server\web-server
dotnet restore
dotnet publish --framework netcoreapp2.0 --output "%Installation%Standard\TaskBuilder\WebFramework\M4Server" --configuration release >> %Installation%8_dotnet_publish.log

%Installation%Apps\ClickOnceDeployer\ClickOnceDeployer.exe
