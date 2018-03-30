@echo off

echo \client\core cleaning...
cd ..\client\core
call rimraf node_modules
echo \client\core\src cleaning...
cd src
call rimraf node_modules

echo \client\libs\menu cleaning...
cd ..\..\libs\menu
call rimraf node_modules
echo \client\libs\menu\src cleaning...
cd src
call rimraf node_modules

echo \client\reporting-studio cleaning...
cd ..\..\..\reporting-studio
call rimraf node_modules
echo \client\reporting-studio\src cleaning...
cd src
call rimraf node_modules

REM echo \client\icons cleaning...
REM cd ..\..\icons
REM call rimraf node_modules
REM echo \client\icons\src cleaning...
REM cd src
REM call rimraf node_modules

echo \client\erp cleaning...
cd ..\..\erp
call rimraf node_modules
echo \client\erp\src cleaning...
cd src
call rimraf node_modules

REM echo \client\bpm cleaning...
REM cd ..\..\bpm
REM call rimraf node_modules
REM echo \client\bpm\src cleaning...
REM cd src
REM call rimraf node_modules

REM echo \client\esp cleaning...
REM cd ..\..\esp
REM call rimraf node_modules
REM echo \client\esp\src cleaning...
REM cd src
REM call rimraf node_modules

REM echo \client\sfm cleaning...
REM cd ..\..\sfm
REM call rimraf node_modules
REM echo \client\sfm\src cleaning...
REM cd src
REM call rimraf node_modules

echo \client\web-form cleaning...
cd ..\..\web-form
call rimraf node_modules

echo.
echo npm cache verify
call npm cache verify
echo.
echo Clean completed