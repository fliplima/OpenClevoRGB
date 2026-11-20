@echo off
setlocal
cd /d "%~dp0\.."

echo ========================================================
echo   Compilando e Executando Testes Automatizados
echo ========================================================

set CSC_PATH="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist %CSC_PATH% (
    set CSC_PATH="C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

if not exist %CSC_PATH% (
    echo [ERRO] O compilador csc.exe nao foi encontrado no sistema.
    exit /b 1
)

set TEST_EXE=%TEMP%\OpenClevoRGB.Tests.exe

%CSC_PATH% /nologo /target:exe /debug+ /platform:anycpu ^
    /out:"%TEST_EXE%" ^
    /r:System.dll ^
    /r:System.Drawing.dll ^
    /r:System.Windows.Forms.dll ^
    /r:System.Management.dll ^
    /r:System.Xml.dll ^
    /r:System.Core.dll ^
    src\App\ClevoController.cs ^
    src\App\AppSettings.cs ^
    src\App\HotkeyManager.cs ^
    src\App\HotkeySettingsForm.cs ^
    src\App\UpdateChecker.cs ^
    src\App\MainForm.cs ^
    tests\OpenClevoRGB.Tests.cs

if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao compilar a suite de testes.
    exit /b 1
)

echo.
"%TEST_EXE%"
set TEST_RESULT=%ERRORLEVEL%

if exist "%TEST_EXE%" del /f /q "%TEST_EXE%" >nul 2>&1
if exist "%TEMP%\OpenClevoRGB.Tests.pdb" del /f /q "%TEMP%\OpenClevoRGB.Tests.pdb" >nul 2>&1

exit /b %TEST_RESULT%
