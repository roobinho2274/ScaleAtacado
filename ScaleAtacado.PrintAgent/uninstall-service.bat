@echo off
set SERVICE_NAME=ScalePrintAgent

echo ============================================
echo  ScaleAtacado PrintAgent - Desinstalacao
echo ============================================
echo.

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERRO: Execute como Administrador.
    pause
    exit /b 1
)

echo Parando servico...
sc stop "%SERVICE_NAME%" >nul 2>&1
timeout /t 3 /nobreak >nul

echo Removendo servico...
sc delete "%SERVICE_NAME%"

if %errorLevel% equ 0 (
    echo Servico removido com sucesso.
) else (
    echo AVISO: Servico nao encontrado ou ja removido.
)

echo.
pause
