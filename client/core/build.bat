@echo off

@echo.
@echo Library compilation (npm run build)
call npm run build >> ".npm-run-build.log"
for /f "delims==" %%a in (.npm-run-build.log) do set lastline=%%a
if NOT "%lastline%" == "Compilation finished succesfully" (
    @echo Build error!!!
	exit
) else (
    @echo Compilation finished succesfully
    del ".npm-run-build.log"
)

@echo.
set "corefolder=%cd%"
set "repofolder=c:/Microarea/@taskbuilder/ng-core"
cd %repofolder
git checkout .
git pull


