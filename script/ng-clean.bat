@echo off

echo \client\core cleaning...
cd ..\client\core
call rimraf node_modules
echo \client\core\src cleaning...
cd src
call rimraf node_modules

echo \client\reporting-studio cleaning...
cd ..\..\reporting-studio
call rimraf node_modules
echo \client\reporting-studio\src cleaning...
cd src
call rimraf node_modules

echo \client\icons cleaning...
cd ..\..\icons
call rimraf node_modules
echo \client\icons\src cleaning...
cd src
call rimraf node_modules

echo \client\erp cleaning...
cd ..\..\erp
call rimraf node_modules
echo \client\erp\src cleaning...
cd src
call rimraf node_modules

echo \client\bpm cleaning...
cd ..\..\bpm
call rimraf node_modules
echo \client\bpm\src cleaning...
cd src
call rimraf node_modules

echo \client\esp cleaning...
cd ..\..\esp
call rimraf node_modules
echo \client\esp\src cleaning...
cd src
call rimraf node_modules

echo \client\sfm cleaning...
cd ..\..\sfm
call rimraf node_modules
echo \client\sfm\src cleaning...
cd src
call rimraf node_modules

echo \client\web-form cleaning...
cd ..\..\web-form
call rimraf node_modules

echo.
echo npm cache verify
call npm cache verify
echo.
echo Clean completed