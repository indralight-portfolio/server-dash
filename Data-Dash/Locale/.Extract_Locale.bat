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
  echo ���õ� ������ �����ϴ�.
  timeout /t 5
  exit /b 0
)
rem echo "%filename%"

"C:\Program Files\7-Zip\7z.exe" x "%filename%" -aoa
"C:\Program Files (x86)\GnuWin32\bin\u2d.exe" *.csv

timeout /t 5
exit /b 0
