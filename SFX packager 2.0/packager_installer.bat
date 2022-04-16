::  ===================================================== ::
::  ==================  Puvox Software  ================= ::
::  ===================================================== ::
::  ================== SFX-Packager v2.0 ================ ::
::  https://puvox.software/blog/creating-portable-exe-or-self/
::  ===================================================== ::
::  ===================================================== ::

@echo off

if "%~1"=="" (
	GOTO :ERROR  
) else (
	GOTO :SUCCESS 
)

:ERROR
mshta javascript:alert("You should drag folder onto this file .\n\n\n(Please read the instruction you will see now)");close(); 
start "" "https://puvox.software/blog/creating-portable-exe-or-self/"
exit

:SUCCESS
:: set path to the dragged folder path
set target_path=%~1

:: current dir path of this bat
set compiler_path=%~dp0

:: ======== removed ===========
:: get folder name 
::		for /f "delims=" %%i in ("%target_path%") do SET original_basename=%%~ni

:: open folder
cd "%target_path%\"

:: set path to empty
set exe_full_path=empty
:: ask user for file (instead of searching for .exe ---> FOR /F "delims=" %%i IN ('dir  /s /b *.exe') DO set exe_full_path=%%i
set /P targetName=Which is the main file, that should be executed after extraction (with extension, i.e.  myApp.exe):
FOR /F "delims=" %%i IN ('dir  /s /b %targetName%') DO set exe_full_path=%%i
:: if none found, exit
if /I "%exe_full_path%" EQU "empty" (
	echo "%targetName% was found. Exiting.."
	pause 5
	exit
)

:: open parent folder
cd "%target_path%\..\"

:: if file
for /f "delims=" %%i in ("%exe_full_path%") do SET exe_file_name=%%~nxi

:: write config.txt 
set target_config=config.txt
@echo ;!@Install@!UTF-8!> %target_config%

:: if same drives, then allow user to choose HARD method. Otherwise, only SOFT can be used
set question=""
set /P question= If you want to a confirmation question to come up during installation, type it here (Otherewise, press ENTER for no-question installation):

IF %question%=="" ( 
 GOTO :NO_QUESTION
)
@echo Title="Unpacking">> %target_config%
@echo BeginPrompt="%question%">> %target_config%

:NO_QUESTION
@echo RunProgram="%exe_file_name%">> %target_config%
@echo ;!@InstallEnd@!>> %target_config%

:: create archive
set temp_archive_name=temp_.7z
:: no compression (change 0 to other level for compression)
"%PROGRAMFILES%/7-zip/7z" a -mx1 %temp_archive_name% "%target_path%\*"

:: compile final exe
copy /b "%compiler_path%/7zS.sfx" + config.txt + %temp_archive_name% __portable__%exe_file_name%.exe

:: delete temp_files
del %temp_archive_name%
del config.txt

mshta javascript:alert("Complete! See package aside the source folder.");close(); 