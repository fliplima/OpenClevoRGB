@echo off
:: Batch para registrar o metadado ACPI/WMI Clevo (clevomof.dll)
title Registrando Metadados ACPI Clevo - OpenClevoRGB
echo ==========================================================
echo   Instalador de Metadados ACPI/WMI - OpenClevoRGB
echo ==========================================================
echo.

:: Verifica privilegios de Administrador
net session >nul 2>&1
if %errorLevel% NEQ 0 (
    echo [AVISO] Este script precisa ser executado como ADMINISTRADOR.
    echo Clique com o botao direito neste arquivo e selecione "Executar como Administrador".
    echo.
    pause
    exit /b 1
)

cd /d "%~dp0\.."
set DLL_SOURCE=%cd%\resources\clevomof.dll

if not exist "%DLL_SOURCE%" (
    echo [ERRO] O arquivo clevomof.dll nao foi encontrado em: %DLL_SOURCE%
    pause
    exit /b 1
)

echo 1. Copiando clevomof.dll para SysWOW64 e System32...
if exist "%windir%\SysWOW64" (
    copy /Y "%DLL_SOURCE%" "%windir%\SysWOW64\clevomof.dll" >nul
)
copy /Y "%DLL_SOURCE%" "%windir%\System32\clevomof.dll" >nul

echo 2. Configurando chave de servico WmiAcpi no Registro...
reg add "HKLM\SYSTEM\CurrentControlSet\Services\WmiAcpi" /v "MofImagePath" /t REG_SZ /d "syswow64\clevomof.dll" /f >nul

echo.
echo ==========================================================
echo   [SUCESSO] Metadados ACPI/WMI registrados com sucesso!
echo ==========================================================
echo.
echo O servico de hardware ACPI agora reconhece o teclado RGB.
echo.
pause
