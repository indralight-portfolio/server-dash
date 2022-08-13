@echo off
setlocal
set "ps=Add-Type -AssemblyName System.windows.forms|Out-Null;"
set "ps=%ps% $f=New-Object System.Windows.Forms.OpenFileDialog;"
set "ps=%ps% $f.Filter='Zip Files (*.zip)|*.zip|All files (*.*)|*.*';"
set "ps=%ps% $f.showHelp=$true;"
set "ps=%ps% $f.ShowDialog()|Out-Null;"
set "ps=%ps% $f.FileName"

for /f "delims=" %%I in ('powershell "%ps%"') do set "filename=%%I"
if "x%filename:zip=%"=="x%filename%" (
  echo 선택된 파일이 없습니다.
  timeout /t 5
  exit /b 0
)
rem echo "%filename%"

"C:\Program Files\7-Zip\7z.exe" x "%filename%" -aoa -o".\ignore.convert~"
pushd "tool~"
.CsvToTxtTest.bat
popd

timeout /t 5
exit /b 0
