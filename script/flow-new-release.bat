@echo off

cd ..\..\..
set installationpath=%cd%

set /p release="Enter Release branch name: release/"

git checkout develop
git branch release/%release%
git push -u origin release/%release%

cd %installationpath%/Standard/Applications/ERP
git checkout develop
git branch release/%release%
git push -u origin release/%release%


cd %installationpath%/Standard/Applications/MDC
git checkout develop
git branch release/%release%
git push -u origin release/%release%


cd %installationpath%/Standard/Applications/Retail
git checkout develop
git branch release/%release%
git push -u origin release/%release%


cd %installationpath%/Standard/Applications/TFB
git checkout develop
git branch release/%release%
git push -u origin release/%release%


cd %installationpath%/Standard/Applications/TBS
git checkout develop
git branch release/%release%
git push -u origin release/%release%


cd %installationpath%/Standard/Applications/WMSMobile
git checkout develop
git branch release/%release%
git push -u origin release/%release%


cd %installationpath%/Standard/Dictionaries
git checkout develop
git branch release/%release%
git push -u origin release/%release%

