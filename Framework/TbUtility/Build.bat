@echo off

rem Build [BUILD-REBUILD] [TB-ERP-PAI-RT-ALL] [DEBUG-RELEASE-ALL-DICONLY] [DIC]

for /f "delims=\ tokens=2*" %%x in ("%CD%") do set _root=%%x

set VSPath=%ProgramFiles%\Microsoft Visual Studio\2017\Professional\Common7\IDE
set LOCALIZERHOME=%ProgramFiles%\Microarea\Microarea TBLocalizer

if exist "%VSPath%\devenv.com" goto Build
set VSPath=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\Common7\IDE
set LOCALIZERHOME=%ProgramFiles(x86)%\Microarea\Microarea TBLocalizer


:Build
if "%1" == "" goto Error
if "%2" == "" goto Error
if "%3" == "" goto Error

if "%2" == "ALL"	goto TaskBuilder
if "%2" == "TB"		goto TaskBuilder
if "%2" == "ERP"	goto Erp
if "%2" == "MDC"	goto MDC
if "%2" == "PAI"	goto PaiNet
if "%2" == "MZP"	goto MZP

goto Error


rem ======================================================================
rem ========== TaskBuilder ===============================================
rem ======================================================================

:TaskBuilder

if "%1" == "BUILD"	goto BuildTb
if "%1" == "REBUILD"	goto RebuildTb
goto Error


:BuildTb

if "%3" == "ALL"	goto BuildTbDebug
if "%3" == "DEBUG"	goto BuildTbDebug
if "%3" == "RELEASE"	goto BuildTbRelease
if "%3" == "DICONLY"	goto GenTBDic
goto Error


:RebuildTb

if "%3" == "ALL"	goto RebuildTbDebug
if "%3" == "DEBUG"	goto RebuildTbDebug
if "%3" == "RELEASE"	goto RebuildTbRelease
if "%3" == "DICONLY"	goto GenTBDic
goto Error


:BuildTbDebug

echo ===== Build TaskBuilder.sln in Debug
del TaskBuilderDeb.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\TaskBuilder\TaskBuilder.sln" /out TaskBuilderDeb.log /build Debug

if "%3" == "DEBUG" goto EndTb


:BuildTbRelease

echo ===== Build TaskBuilder.sln in Release
del TaskBuilderRel.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\TaskBuilder\TaskBuilder.sln" /out TaskBuilderRel.log /build Release

goto EndTb


:RebuildTbDebug

echo ===== Rebuild TaskBuilder.sln in Debug
del TaskBuilderDeb.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\TaskBuilder\TaskBuilder.sln" /out TaskBuilderDeb.log /rebuild Debug

if "%3" == "DEBUG" goto EndTb


:RebuildTbRelease

echo ===== Rebuild TaskBuilder.sln in Release
del TaskBuilderRel.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\TaskBuilder\TaskBuilder.sln" /out TaskBuilderRel.log /rebuild Release

:EndTb

if "%5" == ""	goto NoTBDic
if "%5" == "DIC"	goto GenTBDic
goto Error

:GenTBDic
"%LOCALIZERHOME%\TBLocalizer.exe" "C:\%_root%\Standard\Dictionaries\en\TaskBuilder.tblsln"

:NoTBDic
if "%2" == "TB" goto End

rem ======================================================================
rem ========== ERP =======================================================
rem ======================================================================

:Erp

if "%1" == "BUILD"	goto BuildErp
if "%1" == "REBUILD"	goto RebuildErp
goto Error


:BuildErp

if "%3" == "ALL"	goto BuildErpDebug
if "%3" == "DEBUG"	goto BuildErpDebug
if "%3" == "RELEASE"	goto BuildErpRelease
if "%3" == "DICONLY"	goto GenERPDic
goto Error


:RebuildErp

if "%3" == "ALL"	goto RebuildErpDebug
if "%3" == "DEBUG"	goto RebuildErpDebug
if "%3" == "RELEASE"	goto RebuildErpRelease
if "%3" == "DICONLY"	goto GenERPDic
goto Error


:BuildErpDebug

echo ===== Build Erp in Debug
del ErpDeb.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\ERP\Erp.sln" /out ErpDeb.log /build Debug

if "%3" == "DEBUG" goto EndErp


:BuildErpRelease

echo ===== Build Erp in Release
del ErpRel.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\ERP\Erp.sln" /out ErpRel.log /build Release

goto EndErp


:RebuildErpDebug

echo ===== Rebuild Erp in Debug
del ErpDeb.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\ERP\Erp.sln" /out ErpDeb.log /rebuild Debug

if "%3" == "DEBUG" goto EndErp


:RebuildErpRelease

echo ===== Rebuild Erp in Release
del ErpRel.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\ERP\Erp.sln" /out ErpRel.log /rebuild Release

:EndErp

if "%5" == ""	goto NoERPDic
if "%5" == "DIC"	goto GenERPDic
goto Error

:GenERPDic
"%LOCALIZERHOME%\TBLocalizer.exe" "C:\%_root%\Standard\Dictionaries\en\ERP.tblsln"

:NoERPDic
if "%2" == "ERP" goto End

goto MDC

rem ======================================================================
rem ========== MDC =======================================================
rem ======================================================================

:MDC

if "%1" == "BUILD"	goto BuildMDC
if "%1" == "REBUILD"	goto RebuildMDC
goto Error


:BuildMDC

if "%3" == "ALL"	goto BuildMDCDebug
if "%3" == "DEBUG"	goto BuildMDCDebug
if "%3" == "RELEASE"	goto BuildMDCRelease
if "%3" == "DICONLY"	goto GenMDCDic
goto Error


:RebuildMDC

if "%3" == "ALL"	goto RebuildMDCDebug
if "%3" == "DEBUG"	goto RebuildMDCDebug
if "%3" == "RELEASE"	goto RebuildMDCRelease
if "%3" == "DICONLY"	goto GenMDCDic
goto Error


:BuildMDCDebug

echo ===== Build MDC in Debug
del MDCDeb.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\MDC\MDC.sln" /out MDCDeb.log /build Debug

if "%3" == "DEBUG" goto EndMDC


:BuildMDCRelease

echo ===== Build MDC in Release
del MDCRel.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\MDC\MDC.sln" /out MDCRel.log /build Release

goto EndMDC


:RebuildMDCDebug

echo ===== Rebuild MDC in Debug
del MDCDeb.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\MDC\MDC.sln" /out MDCDeb.log /rebuild Debug

if "%3" == "DEBUG" goto EndMDC


:RebuildMDCRelease

echo ===== Rebuild MDC in Release
del MDCRel.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\MDC\MDC.sln" /out MDCRel.log /rebuild Release

:EndMDC

if "%5" == ""	goto NoMDCDic
if "%5" == "DIC"	goto GenMDCDic
goto Error

:GenMDCDic
"%LOCALIZERHOME%\TBLocalizer.exe" "C:\%_root%\Standard\Dictionaries\en\MDC.tblsln"

:NoMDCDic
if "%2" == "MDC" goto End

rem goto PaiNet
goto End

rem ======================================================================
rem ========== Pai.Net ===================================================
rem ======================================================================

:PaiNet

if "%1" == "BUILD"	goto BuildPaiNet
if "%1" == "REBUILD"	goto RebuildPaiNet
goto Error


:BuildPaiNet

if "%3" == "ALL"	goto BuildPaiNetDebug
if "%3" == "DEBUG"	goto BuildPaiNetDebug
if "%3" == "RELEASE"	goto BuildPaiNetRelease
if "%3" == "DICONLY"	goto GenPAIDic
goto Error


:RebuildPaiNet

if "%3" == "ALL"	goto RebuildPaiNetDebug
if "%3" == "DEBUG"	goto RebuildPaiNetDebug
if "%3" == "RELEASE"	goto RebuildPaiNetRelease
if "%3" == "DICONLY"	goto GenPAIDic
goto Error


:BuildPaiNetDebug

echo ===== Build Pai.Net in Debug
del PaiNetDeb.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\PaiNet\PaiNet.sln" /out PaiNetDeb.log /build Debug

if "%3" == "DEBUG" goto EndPaiNet


:BuildPaiNetRelease

echo ===== Build Pai.Net in Release
del PaiNetRel.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\PaiNet\PaiNet.sln" /out PaiNetRel.log /build Release

goto EndPaiNet


:RebuildPaiNetDebug

echo ===== Rebuild Pai.Net in Debug
del PaiNetDeb.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\PaiNet\PaiNet.sln" /out PaiNetDeb.log /rebuild Debug

if "%3" == "DEBUG" goto EndPaiNet

:RebuildPaiNetRelease

echo ===== Rebuild Pai.Net in Release
del PaiNetRel.log
"%VSPath%\devenv.com" "C:\%_root%\Standard\Applications\PaiNet\PaiNet.sln" /out PaiNetRel.log /rebuild Release

:EndPaiNet

if "%5" == ""	goto NoPAIDic
if "%5" == "DIC"	goto GenPAIDic
goto Error

:GenPAIDic
"%LOCALIZERHOME%\TBLocalizer.exe" "C:\%_root%\Standard\Dictionaries\it-IT\PaiNet.tblsln"

:NoPAIDic

goto End

rem ======================================================================
rem ========== Error =====================================================
rem ======================================================================

:Error

cls
echo.
echo		Build - Preparazione build di TaskBuilder, Erp, MDC, Pai.Net e Dizionari
echo.
echo Utilizzo:
echo	Build [BUILD-REBUILD] [TB-ERP-PAI-ALL] [DEBUG-RELEASE-ALL] [RootDevFolder] [DIC] 
echo.
echo    L'opzione DIC può essere usata per generare i dizionari delle applicazioni indicate
echo.
echo.
echo	Esempio di build di TaskBuilder in debug più esecuzione TBLocalizer
echo.
echo		Build BUILD TB DEBUG "Development" DIC
echo.
echo.
echo	Esempio di Rebuild All in debug e release senza esecuzione TBLocalizer:
echo.
echo		Build REBUILD ALL ALL "Development"
echo.

goto End


rem ======================================================================
rem ========== End =======================================================
rem ======================================================================

:End

echo 
