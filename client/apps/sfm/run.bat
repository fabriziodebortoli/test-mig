set port=4200
IF [%~1]==[port] IF NOT [%~2]==[] set port=%~2

cd ..\..\..\server\web-server
start dotnet run
cd ..\..\client\apps\sfm

ng serve --preserve-symlinks --port=%port%
