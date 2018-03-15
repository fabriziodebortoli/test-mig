@echo off

taskkill /f /im TbJsonCacheServer.exe

set VisualStudioPath=C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\
set BuildSorgenti="%VisualStudioPath%devenv.com"

%BuildSorgenti% /SafeMode "..\Tools\TbJson\TbJson.sln" /rebuild Release

cd ../../
rd /s /q .\Taskbuilder\client\web-form\src\app\applications
.\Taskbuilder\Framework\TbUtility\TbJson\TbJson.exe /ts .
cd .\Taskbuilder\script