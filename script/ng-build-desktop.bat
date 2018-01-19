cd ..\client\web-form
node --max_old_space_size=9120 "node_modules\@angular\cli\bin\ng" build --env=desktop --preserve-symlinks --output-path="..\..\WebFramework\M4Client"
REM node --max_old_space_size=9120 "node_modules\@angular\cli\bin\ng" build --prod --build-optimizer --preserve-symlinks --output-path="..\..\..\TaskBuilder\WebFramework\M4Client"
