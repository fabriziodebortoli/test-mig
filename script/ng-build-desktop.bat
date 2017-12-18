cd ..\client\web-form
REM node --max_old_space_size=9120 "node_modules\@angular\cli\bin\ng" build --env=desktop --target=production --build-optimizer --aot --no-sourcemaps --preserve-symlinks --output-path="..\..\..\TaskBuilder\WebFramework\M4Client"
node --max_old_space_size=9120 "node_modules\@angular\cli\bin\ng" build --prod --build-optimizer --preserve-symlinks --output-path="..\..\..\TaskBuilder\WebFramework\M4Client"
cd ..\..
..\..\..\Apps\ClickOnceDeployer\ClickOnceDeployer.exe
