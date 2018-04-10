@echo off

tasklist /FI "IMAGENAME eq TbJsonCacheServer.exe" 2>NUL | find /I /N "TbJsonCacheServer.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo.
    echo TbJson.bat - Killing process TbJsonCacheServer.exe
    taskkill /f /im TbJsonCacheServer.exe
    echo.
)

set VisualStudioPath=C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\
set BuildSorgenti="%VisualStudioPath%devenv.com"

echo.
echo TbJson.bat - Building TbJson.sln
echo.
%BuildSorgenti% /SafeMode "..\Tools\TbJson\TbJson.sln" /build Release

cd ../../
rem echo.
rem echo TbJson.bat - Removing applications folder of web-form project
rem rd /s /q .\Taskbuilder\client\web-form\src\app\applications
rem echo.

echo TbJson.bat - TS Generation...
.\Taskbuilder\Framework\TbUtility\TbJson\TbJson.exe /ts .
cd .\Taskbuilder\script
echo.
