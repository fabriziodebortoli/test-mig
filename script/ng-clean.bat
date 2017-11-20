cd ..\client\core
del package-lock.json
call rimraf node_modules
cd src
del package-lock.json
call rimraf node_modules

cd ..\..\reporting-studio
del package-lock.json
call rimraf node_modules
cd src
del package-lock.json
call rimraf node_modules

cd ..\..\icons
del package-lock.json
call rimraf node_modules
cd src
del package-lock.json
call rimraf node_modules

cd ..\..\erp
del package-lock.json
call rimraf node_modules
cd src
del package-lock.json
call rimraf node_modules

cd ..\..\bpm
del package-lock.json
call rimraf node_modules
cd src
del package-lock.json
call rimraf node_modules

cd ..\..\web-form
del package-lock.json
call rimraf node_modules

call npm cache verify
