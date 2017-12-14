rem %1: %MicroareaUtility%
rem %2: $(TARGETPATH)

echo off
if A%1 == A goto skip
if A%2 == A goto skip

:start
	if not exist %1 goto skip
	if not exist %2 goto skip
	%1\FileEncrypter.exe %2 ..\..\..\..\WebFramework\LoginManager\bin
	goto end
	
:skip
	echo skipping file encrypter...

:end
	echo %1
	echo %2
	echo on