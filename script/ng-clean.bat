@echo off

echo \client\core cleaning...
cd ..\client\core\src
call rimraf node_modules

echo \client\libs\menu cleaning...
cd ..\..\libs\menu\src
call rimraf node_modules

echo \client\reporting-studio cleaning...
cd ..\..\..\reporting-studio\src
call rimraf node_modules

REM echo \client\icons cleaning...
REM cd ..\..\icons\src
REM call rimraf node_modules

echo \client\erp cleaning...
cd ..\..\erp\src
call rimraf node_modules

REM echo \client\bpm cleaning...
REM cd ..\..\bpm\src
REM call rimraf node_modules

REM echo \client\esp cleaning...
REM cd ..\..\esp\src
REM call rimraf node_modules

REM echo \client\sfm cleaning...
REM cd ..\..\sfm\src
REM call rimraf node_modules

echo \client\web-form cleaning...
cd ..\..\web-form
call rimraf node_modules

echo.
echo npm cache verify
call npm cache verify
echo.
echo Clean completed