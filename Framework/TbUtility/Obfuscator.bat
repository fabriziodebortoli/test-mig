@echo off

rem Obfuscator.bat
rem Authors : Ilaria Manzoni, Matteo Canessa, Stefano Iacono.
rem Date : 26 aprile 2006.
rem Purpose : invoking obfuscator.

rem %1 $(TargetFileName)
rem %2 $(ConfigurationName)
rem %3 $(ProjectDir)
rem %4 $(TargetPath)
rem %5 $(OutputDir)

rem Variables check
 	if A%1 == A goto errorUsage
 	if A%2 == A goto errorUsage
 	if A%3 == A goto errorUsage
 	if A%4 == A goto errorUsage
 	if %2 == Debug goto end

	set ObfuscatorFileName=Goliath.exe
	set ObfuscatorName=Goliath
	set ObfuscatorDir="%ProgramFiles%\Microarea\Goliath.Net\"
	set objDirName=obj\
	set targetObjPath=%3%objDirName%%2\%1

	if not A%5 == A set targetObjPath=%3%5\%1

	rem If obfuscator does not exist it prints a warning and exit
	if not exist %ObfuscatorDir%%ObfuscatorFileName% goto warning

	if not exist %targetObjPath% goto warning1
	goto obfuscate

:obfuscate
	echo Invoking Obfuscator
	cd \
	cd %ObfuscatorDir%
	%ObfuscatorFileName% %4 -rI -encrypt
	if not errorlevel 0 goto obfuscatorError	

	echo Copying obfuscated dll to target destination
	copy %4.%ObfuscatorName%\%1 %targetObjPath% /Y
	copy %4.%ObfuscatorName%\%1 %4 /Y

	echo Removing temporary files
	rd %4.%ObfuscatorName% /S/Q

	set ObfuscatorFileName=
	set ObfuscatorName=
	set ObfuscatorDir=
	set objDirName=
	set targetObjPath=

	goto end

:errorUsage
	echo Error in Obfuscator.Bat
	echo Mode d'employ: Obfuscator.bat $(TargetPath) $(TargetFileName) $(ConfigurationName) $(ProjectDir) [$(OutputDir)]
	if A%1 == A echo $(TargetFileName) is not defined
	if A%2 == A echo $(ConfigurationName) is not defined
	if A%3 == A echo $(ProjectDir) is not defined
	if A%4 == A echo $(TargetPath) is not defined
	goto end

:warning
	echo Obfuscator not installed.
	goto end

:warning1
	echo %targetObjPath% does not exist.
	goto end

:obfuscatorError
	echo Obfuscator returned an error.
	goto end

:end
	echo Done.
