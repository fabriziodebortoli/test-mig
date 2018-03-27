@echo off

echo TbJson.bat - Killing process TbJsonCacheServer.exe
echo.
taskkill /f /im TbJsonCacheServer.exe

set VisualStudioPath=C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\
set BuildSorgenti="%VisualStudioPath%devenv.com"

echo TbJson.bat - Build TbJson.sln
echo.
%BuildSorgenti% /SafeMode "..\Tools\TbJson\TbJson.sln" /rebuild Release

cd ../../
echo TbJson.bat - Remove web-form applications folder
echo.
rd /s /q .\Taskbuilder\client\web-form\src\app\applications

echo TbJson.bat - TS Generation...
echo.
.\Taskbuilder\Framework\TbUtility\TbJson\TbJson.exe /ts .
cd .\Taskbuilder\script
