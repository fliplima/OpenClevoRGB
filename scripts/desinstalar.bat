@echo off
title Desinstalador - OpenClevoRGB

echo ====================================================================
echo                 DESINSTALADOR - OPENCLEVO RGB
echo ====================================================================
echo.

net session >nul 2>&1
if %errorLevel% NEQ 0 (
    echo [ERRO] Este script precisa ser executado como ADMINISTRADOR.
    echo Clique com o botao direito em "desinstalar.bat" e selecione "Executar como Administrador".
    echo.
    pause
    exit /b 1
)

echo 1. Fechando processos em execucao...
taskkill /F /IM OpenClevoRGB.exe >nul 2>&1
taskkill /F /IM AvellRGBMaster.exe >nul 2>&1

echo 2. Removendo tarefa agendada do Windows...
schtasks /delete /tn "OpenClevoRGB" /f >nul 2>&1
schtasks /delete /tn "AvellRGBKeyboardMaster" /f >nul 2>&1

echo 3. Removendo atalhos da Area de Trabalho e Menu Iniciar...
del /f /q "%USERPROFILE%\Desktop\OpenClevoRGB.lnk" >nul 2>&1
del /f /q "%PUBLIC%\Desktop\OpenClevoRGB.lnk" >nul 2>&1
del /f /q "%USERPROFILE%\Desktop\Avell RGB Keyboard Master.lnk" >nul 2>&1
del /f /q "%PUBLIC%\Desktop\Avell RGB Keyboard Master.lnk" >nul 2>&1
rmdir /s /q "%ProgramData%\Microsoft\Windows\Start Menu\Programs\OpenClevoRGB" >nul 2>&1
rmdir /s /q "%ProgramData%\Microsoft\Windows\Start Menu\Programs\Avell RGB Keyboard Master" >nul 2>&1

echo 4. Removendo registro de Desinstalacao no Windows...
reg delete "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\OpenClevoRGB" /f >nul 2>&1
reg delete "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\AvellRGBKeyboardMaster" /f >nul 2>&1

set "INSTALL_DIR=%ProgramFiles%\OpenClevoRGB"
if exist "%INSTALL_DIR%" (
    echo 5. Removendo pasta de instalacao...
    rmdir /s /q "%INSTALL_DIR%" >nul 2>&1
)

set "OLD_DIR=%ProgramFiles%\Avell RGB Keyboard Master"
if exist "%OLD_DIR%" (
    rmdir /s /q "%OLD_DIR%" >nul 2>&1
)

echo.
echo ====================================================================
echo   Desinstalacao concluida com sucesso!
echo ====================================================================
echo.
pause

