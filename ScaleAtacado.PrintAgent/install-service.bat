@echo off
setlocal

set SERVICE_NAME=ScalePrintAgent
set EXE_PATH=%~dp0ScaleAtacado.PrintAgent.exe
set DISPLAY_NAME=ScaleAtacado - Agente de Impressao

echo ============================================
echo  ScaleAtacado PrintAgent - Instalacao
echo ============================================
echo.

REM Verifica se esta rodando como Administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERRO: Execute este script como Administrador.
    echo Clique com o botao direito e escolha "Executar como administrador".
    pause
    exit /b 1
)

REM Verifica se o executavel existe
if not exist "%EXE_PATH%" (
    echo ERRO: Executavel nao encontrado: %EXE_PATH%
    pause
    exit /b 1
)

REM Para e remove servico anterior se ja existir
sc query "%SERVICE_NAME%" >nul 2>&1
if %errorLevel% equ 0 (
    echo Parando servico existente...
    sc stop "%SERVICE_NAME%" >nul 2>&1
    timeout /t 3 /nobreak >nul
    echo Removendo versao anterior...
    sc delete "%SERVICE_NAME%" >nul 2>&1
    timeout /t 2 /nobreak >nul
)

echo Instalando servico...
sc create "%SERVICE_NAME%" binPath="%EXE_PATH%" start=auto DisplayName="%DISPLAY_NAME%"
if %errorLevel% neq 0 (
    echo ERRO: Falha ao criar o servico.
    pause
    exit /b 1
)

sc description "%SERVICE_NAME%" "Agente de impressao termica do sistema ScaleAtacado. Conecta ao servidor via SignalR e imprime recibos automaticamente na impressora configurada."

echo Iniciando servico...
sc start "%SERVICE_NAME%"
if %errorLevel% neq 0 (
    echo AVISO: Servico criado mas nao foi possivel iniciar automaticamente.
    echo Verifique o arquivo appsettings.json e tente: sc start %SERVICE_NAME%
) else (
    echo.
    echo ============================================
    echo  Servico instalado e iniciado com sucesso!
    echo ============================================
    echo.
    echo Nome do servico : %SERVICE_NAME%
    echo Executavel      : %EXE_PATH%
    echo Inicio          : Automatico (inicia com o Windows)
    echo.
    echo Para verificar logs: eventvwr.msc ^> Logs do Windows ^> Aplicativo
    echo Para parar: sc stop %SERVICE_NAME%
)

echo.
pause
