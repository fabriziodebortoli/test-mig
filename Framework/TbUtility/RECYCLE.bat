

SET appcmd=CALL %WINDIR%\system32\inetsrv\appcmd

%appcmd% list apppool /name:"%1"

IF "%ERRORLEVEL%" EQU "0" (
    %appcmd% recycle apppool /APPPOOL.NAME:"%1"
	
	rem per aspettare un poco che il processo vada giù
	ping 127.0.0.1 -n 1 > nul 
) 
