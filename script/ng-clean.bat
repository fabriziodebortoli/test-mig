cd ..\client\core
call rimraf node_modules
cd src
call rimraf node_modules

cd ..\..\reporting-studio
call rimraf node_modules
cd src
call rimraf node_modules

cd ..\..\icons
call rimraf node_modules
cd src
call rimraf node_modules

cd ..\..\erp
call rimraf node_modules
cd src
call rimraf node_modules

cd ..\..\bpm
call rimraf node_modules
cd src
call rimraf node_modules

cd ..\..\esp
call rimraf node_modules
cd src
call rimraf node_modules

cd ..\..\web-form
call rimraf node_modules

call npm cache verify