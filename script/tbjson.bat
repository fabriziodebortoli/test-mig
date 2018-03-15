@echo off

set VisualStudioPath=C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\
set BuildSorgenti="%VisualStudioPath%devenv.com"

%BuildSorgenti% /SafeMode "..\Tools\TbJson\TbJson.sln" /rebuild Release

cd ../
rd /s /q .\client\web-form\src\app\applications
.\Framework\TbUtility\TbJson\TbJson.exe /ts .
cd .\script