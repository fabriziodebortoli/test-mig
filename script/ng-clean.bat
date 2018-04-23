@echo off

echo \client\libs\core cleaning...
cd ..\client\libs\core\src
call rimraf node_modules

echo \client\libs\menu cleaning...
cd ..\..\..\libs\menu\src
call rimraf node_modules

echo \client\libs\reporting-studio cleaning...
cd ..\..\..\libs\reporting-studio\src
call rimraf node_modules

REM echo \client\libs\icons cleaning...
REM cd ..\..\..\libs\icons\src
REM call rimraf node_modules

echo \client\libs\erp cleaning...
cd ..\..\..\libs\erp\src
call rimraf node_modules

REM echo \client\libs\bpm cleaning...
REM cd ..\..\..\libs\bpm\src
REM call rimraf node_modules

REM echo \client\libs\esp cleaning...
REM cd ..\..\..\libs\esp\src
REM call rimraf node_modules

REM echo \client\libs\sfm cleaning...
REM cd ..\..\..\libs\sfm\src
REM call rimraf node_modules

echo \client\web-form cleaning...
cd ..\..\..\web-form
call rimraf node_modules

echo.
echo npm cache verify
call npm cache verify
echo.
echo Clean completed