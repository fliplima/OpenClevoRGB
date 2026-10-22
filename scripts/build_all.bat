@echo off
setlocal
cd /d "%~dp0\.."

echo ========================================================
echo   Compilando Tudo: Aplicativo + Instalador
echo ========================================================

call scripts\build.bat
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao compilar aplicativo.
    pause
    exit /b 1
)

echo.
call scripts\build_installer.bat
if %ERRORLEVEL% NEQ 0 (
    echo [ERRO] Falha ao compilar instalador.
    pause
    exit /b 1
)

echo.
echo ========================================================
echo   TODOS OS BINARIOS COMPILADOS COM SUCESSO!
echo   Arquivos disponiveis na pasta: bin\
echo ========================================================

