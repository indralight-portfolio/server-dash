@echo off
setlocal enabledelayedexpansion

set args=
for /r %%f in (*.csv) do (
  set f=%%~nxf
	rem echo !f!
	set args=!args! !f!
)

rem echo "!args!"
.TagChecker.exe !args!

timeout /t 5
exit /b 0
