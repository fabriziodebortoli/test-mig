set port=4200
IF [%~1]==[port] IF NOT [%~2]==[] set port=%~2

start ..\..\..\TaskBuilder\TaskBuilderNet\Microarea.TaskBuilderNet.TBLoaderService\bin\Debug\TbLoaderService.exe
cd ..\..\server\web-server
start dotnet run
cd ..\..\client\web-form

node --max_old_space_size=9120 "node_modules\@angular\cli\bin\ng" serve --preserve-symlinks --port=%port%
