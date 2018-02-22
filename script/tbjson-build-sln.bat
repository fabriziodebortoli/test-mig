set Build=Release
set VisualStudioPath=C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\Common7\IDE\
set BuildSorgenti="%VisualStudioPath%devenv.com"
cd ..\tools\tbjson

%BuildSorgenti% /SafeMode "TbJson.sln" /build %Build%