@echo off
setlocal EnableDelayedExpansion
title Instalacao Transparente - OpenClevoRGB

echo ====================================================================
echo         OPENCLEVO RGB - INSTALADOR TRANSPARENTE (OPEN SOURCE)
echo ====================================================================
echo.
echo Este script realiza a instalacao aberta do utilitario para que voce
echo possa inspecionar cada comando executado no seu sistema.
echo.

:: 1. Verificacao de privilegios de Administrador
net session >nul 2>&1
if %errorLevel% NEQ 0 (
    echo [ERRO] Este script precisa ser executado como ADMINISTRADOR.
    echo.
    echo Por favor, clique com o botao direito em "instalar.bat"
    echo e selecione "Executar como Administrador".
    echo.
    pause
    exit /b 1
)

cd /d "%~dp0\.."

:: 2. Definicao de diretorios de origem
set "EXE_SRC=%cd%\bin\OpenClevoRGB.exe"
set "DLL_SRC=%cd%\resources\clevomof.dll"

if not exist "!EXE_SRC!" (
    echo [AVISO] O executavel bin\OpenClevoRGB.exe nao foi encontrado.
    echo Compilando agora atraves do script scripts\build.bat...
    call scripts\build.bat
    if not exist "!EXE_SRC!" (
        echo [ERRO] Falha ao compilar bin\OpenClevoRGB.exe.
        pause
        exit /b 1
    )
)

if not exist "!DLL_SRC!" (
    echo [ERRO] Arquivo de driver resources\clevomof.dll nao encontrado!
    pause
    exit /b 1
)

:: 3. Definicao do diretorio de destino
set "INSTALL_DIR=%ProgramFiles%\OpenClevoRGB"

echo.
echo [1/6] Pasta de instalacao: "!INSTALL_DIR!"
echo.

:: 4. Finaliza processos antigos
echo [2/6] Fechando instancias antigas se houver...
taskkill /F /IM OpenClevoRGB.exe >nul 2>&1
taskkill /F /IM AvellRGBMaster.exe >nul 2>&1

:: 5. Cria pasta e copia arquivos
echo [3/6] Copiando arquivos do programa...
if not exist "!INSTALL_DIR!" mkdir "!INSTALL_DIR!"
copy /Y "!EXE_SRC!" "!INSTALL_DIR!\OpenClevoRGB.exe" >nul
copy /Y "!DLL_SRC!" "!INSTALL_DIR!\clevomof.dll" >nul
if exist "%cd%\bin\Desinstalador.exe" (
    copy /Y "%cd%\bin\Desinstalador.exe" "!INSTALL_DIR!\Desinstalador.exe" >nul
)

:: 6. Registra metadados de driver ACPI Clevo
echo [4/6] Registrando metadados ACPI (clevomof.dll) no sistema...
if exist "%windir%\SysWOW64" (
    copy /Y "!DLL_SRC!" "%windir%\SysWOW64\clevomof.dll" >nul
)
copy /Y "!DLL_SRC!" "%windir%\System32\clevomof.dll" >nul
reg add "HKLM\SYSTEM\CurrentControlSet\Services\WmiAcpi" /v "MofImagePath" /t REG_SZ /d "syswow64\clevomof.dll" /f >nul

:: 7. Cria atalhos na Area de Trabalho e no Menu Iniciar
echo [5/6] Criando atalhos...
set "TARGET_EXE=!INSTALL_DIR!\OpenClevoRGB.exe"
set "UNINSTALL_EXE=!INSTALL_DIR!\Desinstalador.exe"

powershell -NoProfile -Command ^
    "$ws = New-Object -ComObject WScript.Shell; " ^
    "$desk = [Environment]::GetFolderPath('DesktopDirectory'); " ^
    "$s = $ws.CreateShortcut(\"$desk\OpenClevoRGB.lnk\"); " ^
    "$s.TargetPath = '!TARGET_EXE!'; " ^
    "$s.WorkingDirectory = '!INSTALL_DIR!'; " ^
    "$s.Description = 'OpenClevoRGB'; " ^
    "$s.Save(); " ^
    "$programs = [Environment]::GetFolderPath('CommonPrograms'); " ^
    "$menuDir = Join-Path $programs 'OpenClevoRGB'; " ^
    "if (-not (Test-Path $menuDir)) { New-Item -ItemType Directory -Path $menuDir -Force | Out-Null }; " ^
    "$s2 = $ws.CreateShortcut(\"$menuDir\OpenClevoRGB.lnk\"); " ^
    "$s2.TargetPath = '!TARGET_EXE!'; " ^
    "$s2.WorkingDirectory = '!INSTALL_DIR!'; " ^
    "$s2.Description = 'OpenClevoRGB'; " ^
    "$s2.Save(); " ^
    "if (Test-Path '!UNINSTALL_EXE!') { " ^
    "  $s3 = $ws.CreateShortcut(\"$menuDir\Desinstalar OpenClevoRGB.lnk\"); " ^
    "  $s3.TargetPath = '!UNINSTALL_EXE!'; " ^
    "  $s3.WorkingDirectory = '!INSTALL_DIR!'; " ^
    "  $s3.Description = 'Desinstalar OpenClevoRGB'; " ^
    "  $s3.Save(); " ^
    "}"

:: 8. Configura Tarefa Agendada no Windows para iniciar com Administrador no boot
echo [6/6] Configurando inicializacao automatica com o Windows (Task Scheduler)...
schtasks /create /tn "OpenClevoRGB" /tr "\"!TARGET_EXE!\" -silent" /sc onlogon /rl highest /f >nul 2>&1

:: 9. Cria script de desinstalacao limpo e registro no Painel de Controle
set "UNINSTALL_BAT=!INSTALL_DIR!\Desinstalar.bat"
(
    echo @echo off
    echo title Desinstalador - OpenClevoRGB
    echo echo Desinstalando OpenClevoRGB...
    echo taskkill /F /IM OpenClevoRGB.exe ^>nul 2^>^&1
    echo schtasks /delete /tn "OpenClevoRGB" /f ^>nul 2^>^&1
    echo del /f /q "%%USERPROFILE%%\Desktop\OpenClevoRGB.lnk" ^>nul 2^>^&1
    echo del /f /q "%PUBLIC%\Desktop\OpenClevoRGB.lnk" ^>nul 2^>^&1
    echo rmdir /s /q "%ProgramData%\Microsoft\Windows\Start Menu\Programs\OpenClevoRGB" ^>nul 2^>^&1
    echo reg delete "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenClevoRGB" /f ^>nul 2^>^&1
    echo echo Removendo arquivos...
    echo timeout /t 2 /nobreak ^>nul
    echo rmdir /s /q "!INSTALL_DIR!" ^>nul 2^>^&1
    echo echo Desinstalacao concluida com sucesso!
    echo pause
) > "!UNINSTALL_BAT!"

set "FINAL_UNINSTALLER=!UNINSTALL_BAT!"
if exist "!UNINSTALL_EXE!" set "FINAL_UNINSTALLER=!UNINSTALL_EXE!"

reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenClevoRGB" /v "DisplayName" /t REG_SZ /d "OpenClevoRGB" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenClevoRGB" /v "DisplayIcon" /t REG_SZ /d "!TARGET_EXE!" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenClevoRGB" /v "DisplayVersion" /t REG_SZ /d "1.3.0" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenClevoRGB" /v "Publisher" /t REG_SZ /d "OpenClevoRGB / Felipe Lima" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenClevoRGB" /v "InstallLocation" /t REG_SZ /d "!INSTALL_DIR!" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenClevoRGB" /v "UninstallString" /t REG_SZ /d "\"!FINAL_UNINSTALLER!\"" /f >nul

echo.
echo ====================================================================
echo   [SUCESSO] OpenClevoRGB foi instalado com exito!
echo ====================================================================
echo.
echo Arquivos instalados em: !INSTALL_DIR!
echo Atalhos criados na Area de Trabalho e no Menu Iniciar.
echo Inicializacao configurada com privilégios de Administrador no boot.
echo.

set /p RESP="Deseja abrir o aplicativo agora? (S/N): "
if /i "%RESP%"=="S" (
    start "" "!TARGET_EXE!"
)

echo.
pause

