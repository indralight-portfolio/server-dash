@echo off
setlocal enabledelayedexpansion

set f=%~1%
if "%f%"=="" (
  set /p f="Input File: "
)

set exepath=ExcelConsole.exe

if "%f%"=="" (
  %~dp0%exepath%
) else (
  %~dp0%exepath% "%f%"
)

if not "%~2%"=="nowait" timeout /t 5
exit /b 0