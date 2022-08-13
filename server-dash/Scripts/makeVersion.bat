pushd Scripts
for /f "usebackq" %%a in ("version.txt") do set Version=%%a
echo Version : %Version%
popd

pushd ..\
for /f "usebackq" %%a in (`git rev-parse --short HEAD`) do set RevServer=%%a
popd
pushd ..\Lib-Dash
for /f "usebackq" %%a in (`git rev-parse --short HEAD`) do set RevDash=%%a
popd
pushd ..\Data-Dash
for /f "usebackq" %%a in (`git rev-parse --short HEAD`) do set RevData=%%a
popd

echo f | xcopy Scripts\BuildVersion.txt BuildVersion.cs /q/y > NUL
set src=BuildVersion.cs
set reg=s/$Version$/%Version%/g;s/$RevServer$/%RevServer%/g;s/$RevDash$/%RevDash%/g;s/$RevData$/%RevData%/g;
call :convert "%src%" "%reg%"

echo {"Version":"%Version%","RevServer":"%RevServer%","RevDash":"%RevDash%","RevData":"%RevData%","DateTime":"%DATE:~0,4%-%DATE:~5,2%-%DATE:~8,2% %TIME:~0,8%"}>meta.json

exit /b 0

:convert
set src=%~1
set reg=%~2
set tmp=%~dpn1.tmp
rem echo %reg%
rem echo %src% %tmp%
cscript //NoLogo Scripts\sed.vbs "%reg%" < %src% > %tmp%
move /y %tmp% %src% > NUL
exit /b 0