@echo off
setlocal
cd /d "%~dp0\.."

echo ========================================================
echo   Compilando OpenClevoRGB (Aplicativo)
echo ========================================================

set CSC_PATH="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist %CSC_PATH% (
    set CSC_PATH="C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

if not exist %CSC_PATH% (
    echo [ERRO] O compilador csc.exe nao foi encontrado no sistema.
    pause
    exit /b 1
)

if not exist "bin" mkdir "bin"

echo Compilando bin\OpenClevoRGB.exe com icone e metadados...

%CSC_PATH% /nologo /target:winexe /optimize+ /platform:anycpu ^
    /win32manifest:resources\app.manifest ^
    /win32icon:resources\app.ico ^
    /out:"bin\OpenClevoRGB.exe" ^
    /r:System.dll ^
    /r:System.Drawing.dll ^
    /r:System.Windows.Forms.dll ^
    /r:System.Management.dll ^
    /r:System.Xml.dll ^
    /r:System.Core.dll ^
    src\App\AssemblyInfo.cs ^
    src\App\ClevoController.cs ^
    src\App\AppSettings.cs ^
    src\App\HotkeyManager.cs ^
    src\App\HotkeySettingsForm.cs ^
    src\App\MainForm.cs ^
    src\App\Program.cs

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================
    echo   Sucesso! Executavel criado em: bin\OpenClevoRGB.exe
    echo ========================================================
) else (
    echo.
    echo [ERRO] Falha durante a compilacao.
)
