cd ..\..\..\server\web-server
start dotnet run
cd ..\..\client\apps\bpm
REM node --max_old_space_size=5120 "node_modules\@angular\cli\bin\ng" serve --preserve-symlinks --prod
node --max_old_space_size=5120 "node_modules\@angular\cli\bin\ng" serve --preserve-symlinks
