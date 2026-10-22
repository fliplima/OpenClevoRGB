@echo off
setlocal
cd /d "%~dp0\.."

echo ========================================================
echo   Compilando Instalador e Desinstalador OpenClevoRGB
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

if not exist "bin\OpenClevoRGB.exe" (
    echo Compilando aplicativo principal primeiro...
    call scripts\build.bat
)

if not exist "resources\clevomof.dll" (
    echo [ERRO] Arquivo resources\clevomof.dll nao encontrado!
    pause
    exit /b 1
)

echo 1. Compilando bin\Desinstalador.exe com icone e metadados...

%CSC_PATH% /nologo /target:winexe /optimize+ /platform:anycpu ^
    /win32manifest:resources\app.manifest ^
    /win32icon:resources\app.ico ^
    /out:"bin\Desinstalador.exe" ^
    /r:System.dll ^
    /r:System.Drawing.dll ^
    /r:System.Windows.Forms.dll ^
    /r:System.Core.dll ^
    src\Uninstaller\AssemblyInfo.cs ^
    src\Uninstaller\UninstallerForm.cs ^
    src\Uninstaller\UninstallerProgram.cs

if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao compilar Desinstalador.exe.
    pause
    exit /b 1
)

echo 2. Compilando bin\Instalador_OpenClevoRGB.exe com icone e recursos embutidos...

%CSC_PATH% /nologo /target:winexe /optimize+ /platform:anycpu ^
    /win32manifest:resources\app.manifest ^
    /win32icon:resources\app.ico ^
    /res:bin\OpenClevoRGB.exe ^
    /res:resources\clevomof.dll ^
    /res:bin\Desinstalador.exe ^
    /out:"bin\Instalador_OpenClevoRGB.exe" ^
    /r:System.dll ^
    /r:System.Drawing.dll ^
    /r:System.Windows.Forms.dll ^
    /r:System.Core.dll ^
    src\Installer\AssemblyInfo.cs ^
    src\Installer\InstallerForm.cs ^
    src\Installer\InstallerProgram.cs

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================
    echo   Sucesso! Instalador e Desinstalador criados em bin\
    echo ========================================================
) else (
    echo.
    echo [ERRO] Falha durante a compilacao do instalador.
)
