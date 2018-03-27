@echo off

set port=4200
IF [%~1]==[port] IF NOT [%~2]==[] set port=%~2

cd ..\..\..\server\web-server
start dotnet run
cd ..\..\client\apps\rs

node --max_old_space_size=9120 "node_modules\@angular\cli\bin\ng" serve --preserve-symlinks --port=%port%
