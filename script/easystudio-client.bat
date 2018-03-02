@echo off

REM ===========================================================================

ng-clean.bat

cd ..\client/apps/easystudio


npm install --no-save
ng serve

dotnet run
