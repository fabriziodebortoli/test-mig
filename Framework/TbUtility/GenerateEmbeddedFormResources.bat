@echo off
if A%1 == A goto errorUsage
if A%2 == A goto errorUsage

if not exist %TBStd%\TaskBuilder\Tools\DllFormResourceExplorer\bin\Release\EmbeddedFormResourceExplorer.exe goto noTool

echo ================ Generating Embedded Form Resources File ==============
if not exist %1 goto notExist

%TBStd%\TaskBuilder\Tools\DllFormResourceExplorer\bin\Release\EmbeddedFormResourceExplorer.exe dllFullName=%1 xmlFullName=%2 Silent=true
echo %2
goto end

:errorUsage
	echo Errore in CopyLib.Bat
	echo Modo d'uso: GenerateEmbeddedFormResources.bat "Source Dll Full Name" "Destination Xml Full File Name"
	echo .
	echo .
	if A%1 == A goto echo Non e' stato indicato "Source Dll Full Name"
	if A%2 == A goto echo Non e' stato indicato "Destination Xml Full File Name"

:notExist
echo Error: %1 file does not exists!
goto end

:end
echo ========= Embedded Form Resources File Generation Terminated ==========

:noTool