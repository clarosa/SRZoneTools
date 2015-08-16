@ECHO OFF
SETLOCAL
SET testdir=%CD%
SET exedir=%CD%\..\bin\Debug
IF "%~1"=="" GOTO Usage
SET srcdir=%~1
SET ext=czn_pc
IF NOT "%~2"=="" SET ext=%~2

IF NOT EXIST "%exedir%\SRZoneTool.exe" GOTO :NoExe

FOR /R "%srcdir%" %%F IN (*.%ext%) DO CALL :test %%F
PAUSE
GOTO :EOF

:test
ECHO "%*"
COPY "%*" "%testdir%\test1.%ext%"
IF ERRORLEVEL 1 GOTO ErrorInCall
"%exedir%\SRZoneTool" "%testdir%\test1.%ext%" -o "%testdir%\test2.xml" -v >"%testdir%\test1.txt"
IF ERRORLEVEL 1 GOTO ErrorInCall
"%exedir%\SRZoneTool" "%testdir%\test2.xml" -o "%testdir%\test3.%ext%"
IF ERRORLEVEL 1 GOTO ErrorInCall
FC /B "%testdir%\test1.%ext%" "%testdir%\test3.%ext%" | more
FC /B "%testdir%\test1.%ext%" "%testdir%\test3.%ext%" >nul
IF ERRORLEVEL 1 GOTO ErrorInCall
GOTO :EOF

:ErrorInCall
ECHO Failed with file "%*"
ECHO The command processor will exit.
PAUSE
EXIT

:Usage
ECHO Usage: testall directory [extension]
PAUSE
GOTO :EOF

:NoExe
ECHO Missing file "%exedir%\SRZoneTool.exe"
PAUSE
GOTO :EOF
