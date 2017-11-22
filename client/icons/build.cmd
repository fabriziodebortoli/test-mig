@echo off

set message=%1
set tag=%2
if "%message%" == "" (
    @echo Usage: build "Commit message" "tag"(optional^)
    exit
)

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

set "iconsfolder=%cd%"
set "repofolder=c:/Microarea/@taskbuilder/ng-icons"
cd %repofolder%
@echo Clean library repo
git checkout .

@echo Pull
git pull >> ".pull.log"
for /f "delims==" %%a in (.pull.log) do set lastline=%%a
if NOT "%lastline%" == "Current branch master is up to date." (
    @echo Pull error!!!
	exit
) else (
    @echo Already up-to-date.
    del ".pull.log"
)

@echo Copy dist to library repo
xcopy %iconsfolder%\dist /y /s /e

@echo Git stage and commit
git add .

REM if NOT "%tag%" == "" (
REM     git tag -a %tag% -m "%message%"
REM ) else (
REM     @echo No tag added
REM )
REM git commit -m "%message%"

REM @echo Push
REM git push

@echo Completed!
