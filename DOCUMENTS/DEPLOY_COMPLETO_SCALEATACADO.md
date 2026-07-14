# DEPLOY COMPLETO — ScaleAtacado
## Guia passo a passo: Servidor Windows + PrintAgent em máquina separada

---

## Arquitetura final

```
┌─────────────────────────────────────────────────┐
│  MÁQUINA SERVIDOR  (ex: IP 192.168.1.100)       │
│                                                  │
│  Windows 10/11 + Docker Desktop                 │
│  └─ docker compose                              │
│      ├─ postgres-db  (banco — interno)          │
│      ├─ api          (ASP.NET — interno :5000)  │
│      └─ web/nginx    (PORTA 80 → rede local)    │
│                                                  │
│  Firewall: PORTA 80 aberta                      │
└──────────────────────┬──────────────────────────┘
                       │  rede local (porta 80)
        ┌──────────────┼──────────────┐
        ▼              ▼              ▼
  [Computador      [Computador    [Máquina
   operador 1]      operador 2]    PrintAgent]
   Browser           Browser        IP: 192.168.1.50
   http://           http://        └─ PrintAgent.exe
   192.168.1.100     192.168.1.100     SignalR ws://
                                       API http://
                                       (TUDO porta 80)
```

> **Importante:** Todo tráfego — browser, API, SignalR/WebSocket do PrintAgent —
> passa pela **porta 80**. Apenas essa porta precisa ser aberta.

---

---

# PARTE 1 — MÁQUINA SERVIDOR

---

## Aplicações a instalar na máquina servidor

| Aplicação | Para que serve | Obrigatório |
|---|---|---|
| Docker Desktop for Windows | Roda os containers (banco, API, Blazor) | Sim |
| Git for Windows | Baixar o código-fonte do projeto | Recomendado |
| Notepad++ ou VS Code | Editar arquivos de configuração | Opcional |

---

## PASSO 1 — Verificar os requisitos do Windows

Abra o **PowerShell** e execute:

```powershell
# Verificar versão do Windows
winver
```

**Requisitos mínimos:**
- Windows 10 versão 2004 (build 19041) ou superior
- Windows 11 (qualquer versão)
- 8 GB RAM (16 GB recomendado)
- 20 GB de espaço livre em disco

**Verificar se a virtualização está habilitada:**
```powershell
Get-ComputerInfo -Property "HyperVRequirementVirtualizationFirmwareEnabled"
```
- Se retornar `True` → OK, pode continuar
- Se retornar `False` → é necessário entrar na BIOS/UEFI e habilitar Intel VT-x (Intel) ou AMD-V (AMD)

---

## PASSO 2 — Instalar o Docker Desktop

### 2.1 — Baixar o instalador

Acesse no browser da máquina servidor:
```
https://www.docker.com/products/docker-desktop/
```
Clique em **"Download for Windows"**. O arquivo se chama `Docker Desktop Installer.exe` (~600 MB).

### 2.2 — Instalar

1. Execute o arquivo `Docker Desktop Installer.exe` como **Administrador**
   (botão direito → "Executar como administrador")

2. Na tela de configuração:
   - ✅ **"Use WSL 2 instead of Hyper-V"** — deixar marcado
   - ✅ **"Add shortcut to desktop"** — opcional

3. Clique **"Ok"** e aguarde a instalação (~5 minutos)

4. Ao final, clique **"Close and restart"** e aguarde o Windows reiniciar

### 2.3 — Primeira execução do Docker Desktop

Após reiniciar:
1. O Docker Desktop abre automaticamente (ícone da baleia na barra de tarefas)
2. Aceite os termos de uso clicando em **"Accept"**
3. Na tela de boas-vindas, pode fechar ou pular (Skip)
4. Aguarde o ícone da baleia ficar **estático** (não pulsando) — indica que o Docker está pronto

### 2.4 — Configurar inicialização automática com o Windows

No Docker Desktop, clique no ícone **⚙️ Settings** (engrenagem, canto superior direito):

- Aba **General:**
  - ✅ **"Start Docker Desktop when you sign in to your computer"** — marcar

- Clique **"Apply & restart"**

### 2.5 — Verificar a instalação

Abra o **PowerShell** e execute:
```powershell
docker --version
docker compose version
docker run hello-world
```

O `hello-world` deve imprimir uma mensagem de sucesso. Se sim, o Docker está funcionando corretamente.

---

## PASSO 3 — Instalar o Git for Windows

O Git é a forma mais limpa de manter o código do projeto atualizado.

### 3.1 — Baixar

```
https://git-scm.com/download/win
```
Clique em **"64-bit Git for Windows Setup"**.

### 3.2 — Instalar

Execute o instalador com as opções padrão (pode clicar "Next" em tudo).
As configurações padrão são adequadas para uso no servidor.

### 3.3 — Verificar

```powershell
git --version
```
Deve mostrar a versão instalada.

---

## PASSO 4 — Preparar a pasta do projeto no servidor

### 4.1 — Criar a pasta de deploy

```powershell
New-Item -ItemType Directory -Force "C:\ScaleAtacado"
```

### 4.2 — Copiar os arquivos do projeto

**Opção A — Via Git (recomendado se tiver repositório):**
```powershell
cd C:\
git clone https://github.com/sua-org/scale-atacado.git ScaleAtacado
```

**Opção B — Copiar de outra máquina via rede ou pendrive:**

Na máquina de desenvolvimento, copie a pasta inteira do projeto para
um pendrive ou compartilhamento de rede, depois no servidor:

```powershell
# Exemplo copiando de pendrive (E:) para C:\ScaleAtacado
robocopy "E:\ScaleAtacado" "C:\ScaleAtacado" /E `
    /XD bin obj .git .vs node_modules `
    /XF "appsettings.Development.json" "*.user"
```

**Opção C — Copiar da máquina de desenvolvimento pela rede:**
```powershell
# Na máquina de desenvolvimento, abra o PowerShell e execute:
# (substitua IP_DO_SERVIDOR pelo IP real)
robocopy "C:\Users\robso\Documents\ACR\Project\ScaleAtacado" `
    "\\IP_DO_SERVIDOR\c$\ScaleAtacado" /E `
    /XD bin obj .git .vs node_modules `
    /XF "appsettings.Development.json" "*.user"
```

### 4.3 — Confirmar que os arquivos estão corretos

```powershell
ls C:\ScaleAtacado
```

Deve listar, entre outros:
- `docker-compose.yml`
- `ScaleAtacado.Api\`
- `ScaleAtacado.Blazor\`
- `ScaleAtacado.Infrastructure\`

---

## PASSO 5 — Criar o arquivo `.env` com os segredos

O `docker-compose.yml` lê as variáveis de ambiente de um arquivo `.env`.
Este arquivo contém senhas e chaves — **nunca suba para o Git**.

### 5.1 — Descobrir o IP desta máquina

```powershell
ipconfig
```

Procure pelo adaptador de rede local (Ethernet ou Wi-Fi) e anote o
**Endereço IPv4** (exemplo: `192.168.1.100`).

### 5.2 — Criar o arquivo

```powershell
notepad "C:\ScaleAtacado\.env"
```

O Bloco de Notas perguntará se deseja criar o arquivo — clique **Sim**.

Cole o conteúdo abaixo substituindo pelos valores reais:

```env
# ─── Banco de dados ───────────────────────────────
POSTGRES_PASSWORD=SenhaFortePostgres2024!

# ─── JWT — mínimo 32 caracteres ───────────────────
# Dica: use uma frase longa e aleatória
JWT_KEY=ChaveJWTSuperSecretaComPeloMenosTrintaEDoisCaracteres!!

# ─── PrintAgent ───────────────────────────────────
# Chave compartilhada entre API e o PrintAgent
# Pode ser qualquer texto longo e aleatório
AGENT_KEY=ChaveSecretaDoAgenteDePressao2024!

# ─── URL de acesso ────────────────────────────────
# IP desta máquina servidor na rede local
# (descoberto com ipconfig acima)
CORS_ORIGINS=http://192.168.1.100
```

Salve com **Ctrl+S** e feche o Bloco de Notas.

> **Atenção:** Se o IP do servidor mudar (máquina com IP dinâmico), será
> necessário atualizar o CORS_ORIGINS e reiniciar os containers.
> Recomendado configurar IP fixo no roteador para a máquina servidor.

---

## PASSO 6 — Ajustar o `docker-compose.yml`

Em produção, a porta do banco de dados (5432) não deve ficar acessível
pela rede. Abra o arquivo para editar:

```powershell
notepad "C:\ScaleAtacado\docker-compose.yml"
```

Encontre o trecho abaixo e **comente as duas linhas de ports** adicionando `#`:

```yaml
  postgres-db:
    image: postgres:15-alpine
    # ports:               ← adicione # nesta linha
    #   - "5432:5432"      ← adicione # nesta linha
```

Salve e feche.

---

## PASSO 7 — Construir e subir os containers

Abra o **PowerShell** (não precisa ser Administrador):

```powershell
cd C:\ScaleAtacado
docker compose up -d --build
```

### O que acontece:

| Etapa | O que o Docker faz | Tempo aprox. |
|---|---|---|
| Download das imagens | Baixa .NET 8, nginx, PostgreSQL | 3–8 min (depende da internet) |
| Build da API | Compila o projeto C# dentro do container | 2–4 min |
| Build do Blazor | Compila e gera os arquivos estáticos | 2–3 min |
| Subir containers | Inicia banco → API → nginx | 30–60 seg |

### Acompanhar os logs em tempo real:

```powershell
docker compose logs -f
```

Pressione **Ctrl+C** para sair dos logs sem derrubar os containers.

Quando estiver tudo pronto, você verá nos logs da API:
```
scale_atacado_api  | info: Microsoft.Hosting.Lifetime[14]
scale_atacado_api  | Now listening on: http://[::]:5000
```

### Verificar o status dos containers:

```powershell
docker compose ps
```

Resultado esperado:
```
NAME                 STATUS           PORTS
scale_atacado_db     Up (healthy)     5432/tcp
scale_atacado_api    Up               5000/tcp
scale_atacado_web    Up               0.0.0.0:80->80/tcp
```

Os três containers devem estar como `Up`. O banco deve estar `(healthy)`.

### Testar localmente no servidor:

```powershell
# Abre o browser com a aplicação
Start-Process "http://localhost"
```

Deve abrir a tela de login do ScaleAtacado.

---

## PASSO 8 — Abrir a porta 80 no Firewall do Windows

Para que outros computadores da rede (operadores e PrintAgent)
consigam acessar a aplicação, é preciso liberar a porta 80.

Abra o **PowerShell como Administrador**
(Win + X → "Terminal (Admin)" ou "Windows PowerShell (Admin)"):

```powershell
New-NetFirewallRule `
    -DisplayName "ScaleAtacado - HTTP" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort 80 `
    -Action Allow `
    -Profile Domain,Private
```

### Verificar se a regra foi criada:

```powershell
Get-NetFirewallRule -DisplayName "ScaleAtacado - HTTP" | Format-Table Name, Enabled, Action
```

Deve mostrar `Enabled: True` e `Action: Allow`.

---

## PASSO 9 — Configuração inicial do sistema (primeira vez)

Execute no PowerShell para criar a empresa e o primeiro usuário administrador:

```powershell
$body = @{
    adminName      = "Administrador"
    adminEmail     = "admin@suaempresa.com"
    adminPassword  = "SenhaAdmin123"
    companyName    = "Nome da Empresa Ltda"
    companyCNPJ    = "00.000.000/0001-00"
    companyAddress = "Rua Exemplo, 123 — Cidade/UF"
    companyPhone   = "(11) 99999-9999"
} | ConvertTo-Json -Depth 2

$resp = Invoke-RestMethod `
    -Uri "http://localhost/api/auth/setup" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body

$resp | ConvertTo-Json
```

Resposta esperada: JSON com `"success": true` e o token do admin.

Após isso, acesse `http://localhost` no browser e faça login com
o e-mail e senha informados acima.

---

## PASSO 10 — Configurar IP fixo para o servidor (recomendado)

Para que o IP do servidor não mude (e ninguém precise atualizar configurações),
configure o IP fixo **no roteador** (recomendado) ou no próprio Windows.

### Via roteador (melhor opção):

Acesse o painel do roteador (geralmente `http://192.168.1.1`) e reserve
o IP para o endereço MAC da placa de rede do servidor. Consulte o manual
do roteador para localizar "DHCP Reservation" ou "IP Estático por MAC".

### Via Windows (alternativa):

Abra **Painel de Controle → Redes e Internet → Central de Rede e
Compartilhamento → Alterar as configurações do adaptador**.

Clique com botão direito na conexão de rede → **Propriedades →
Protocolo TCP/IPv4 → Propriedades** → marcar "Usar o seguinte endereço IP"
e preencher com o IP, máscara e gateway da sua rede.

---

## PASSO 11 — Verificar o acesso pela rede

Em **outro computador** da rede, abra o browser e acesse:
```
http://192.168.1.100
```
(substitua pelo IP real do servidor)

Deve aparecer a tela de login do ScaleAtacado.

**Se não abrir, verificar:**
- [ ] `docker compose ps` mostra todos os containers `Up`
- [ ] A regra de firewall foi criada (Passo 8)
- [ ] O IP no CORS_ORIGINS do `.env` está correto
- [ ] O roteador não está bloqueando tráfego entre os PCs da rede
- [ ] Desabilitar temporariamente o Windows Defender/antivírus para testar

---

## PASSO 12 — Confirmar o auto-start dos containers

Com `restart: unless-stopped` no `docker-compose.yml` e o Docker Desktop
configurado para iniciar com o Windows (Passo 2.4), os containers sobem
automaticamente após reinicialização da máquina.

### Testar:

1. Reinicie o servidor
2. Aguarde ~2 minutos após o login
3. Abra o browser e acesse `http://localhost`
4. Se carregar, o auto-start está funcionando

### Se os containers não subirem automaticamente:

Crie uma tarefa no Agendador de Tarefas para garantir:

Salve este script como `C:\ScaleAtacado\iniciar.ps1`:
```powershell
# Aguarda o Docker estar pronto (até 3 minutos)
$tentativas = 0
while ($tentativas -lt 36) {
    try {
        $null = docker info 2>&1
        if ($LASTEXITCODE -eq 0) { break }
    } catch {}
    Start-Sleep -Seconds 5
    $tentativas++
}

# Sobe os containers
Set-Location "C:\ScaleAtacado"
docker compose up -d
```

Registre a tarefa (PowerShell como Administrador):
```powershell
$acao = New-ScheduledTaskAction `
    -Execute "powershell.exe" `
    -Argument "-ExecutionPolicy Bypass -File C:\ScaleAtacado\iniciar.ps1" `
    -WorkingDirectory "C:\ScaleAtacado"

$gatilho = New-ScheduledTaskTrigger -AtStartup

$config = New-ScheduledTaskSettingsSet `
    -StartWhenAvailable `
    -ExecutionTimeLimit (New-TimeSpan -Minutes 10)

Register-ScheduledTask `
    -TaskName "ScaleAtacado AutoStart" `
    -Action $acao `
    -Trigger $gatilho `
    -Settings $config `
    -RunLevel Highest `
    -Force
```

---

## PASSO 13 — Backup automático do banco de dados (recomendado)

Salve como `C:\ScaleAtacado\backup.ps1`:
```powershell
$pasta  = "C:\ScaleAtacado\Backups"
$data   = Get-Date -Format "yyyyMMdd_HHmm"
$arquivo = "$pasta\backup_$data.sql"

New-Item -ItemType Directory -Force $pasta | Out-Null

docker exec scale_atacado_db pg_dump -U admin scale_atacado `
    | Out-File $arquivo -Encoding utf8

# Manter apenas os últimos 30 dias
Get-ChildItem $pasta -Filter "backup_*.sql" |
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } |
    Remove-Item -Force

Write-Host "Backup salvo em: $arquivo"
```

Agende para rodar todo dia às 2h da manhã (PowerShell como Administrador):
```powershell
$acao = New-ScheduledTaskAction `
    -Execute "powershell.exe" `
    -Argument "-ExecutionPolicy Bypass -File C:\ScaleAtacado\backup.ps1"

$gatilho = New-ScheduledTaskTrigger -Daily -At "02:00"

Register-ScheduledTask `
    -TaskName "ScaleAtacado Backup" `
    -Action $acao `
    -Trigger $gatilho `
    -RunLevel Highest `
    -Force
```

---

---

# PARTE 2 — MÁQUINA DO PRINTAGENT (impressora)

---

## Aplicações a instalar na máquina do PrintAgent

| Aplicação | Para que serve | Obrigatório |
|---|---|---|
| Driver EPSON TM-T20X | Comunicação com a impressora | Sim |
| ScaleAtacado.PrintAgent.exe | O agente de impressão | Sim |
| .NET 8 Desktop Runtime | Necessário para rodar o agente | Apenas se não usar self-contained |

---

## PASSO 1 — Instalar o driver da impressora EPSON TM-T20X

Se a impressora já aparece em **Configurações → Bluetooth e dispositivos →
Impressoras e scanners**, este passo pode ser ignorado.

Caso contrário:
1. Conecte a impressora EPSON TM-T20X via USB ou rede
2. Acesse o site da EPSON e baixe o driver para Windows
3. Execute o instalador do driver
4. Confirme que a impressora aparece em "Impressoras e scanners"

> **Anote o nome exato** como aparece na lista de impressoras.
> Exemplo: `EPSON TM-T20X Receipt` — será usado na configuração.

---

## PASSO 2 — Publicar o PrintAgent (feito na máquina de desenvolvimento)

Na máquina onde você tem o código-fonte, abra o **PowerShell**:

```powershell
cd "C:\Users\robso\Documents\ACR\Project\ScaleAtacado"

dotnet publish .\ScaleAtacado.PrintAgent\ `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -o .\dist\PrintAgent
```

Isso gera `dist\PrintAgent\ScaleAtacado.PrintAgent.exe` — um único arquivo
executável de ~80 MB que **não precisa do .NET instalado** na máquina destino.

---

## PASSO 3 — Copiar o PrintAgent para a máquina da impressora

Copie toda a pasta `dist\PrintAgent\` para a máquina onde a impressora
está instalada. Use pendrive, compartilhamento de rede ou qualquer meio.

Destino sugerido na máquina do PrintAgent:
```
C:\ScaleAtacado\PrintAgent\
    ScaleAtacado.PrintAgent.exe
    appsettings.json
```

---

## PASSO 4 — Configurar o `appsettings.json`

Na pasta `C:\ScaleAtacado\PrintAgent\`, abra o arquivo `appsettings.json`
com o Bloco de Notas:

```powershell
notepad "C:\ScaleAtacado\PrintAgent\appsettings.json"
```

Substitua pelo conteúdo abaixo com os valores corretos:

```json
{
  "PrintAgent": {
    "ApiUrl": "http://192.168.1.100",
    "AgentKey": "ChaveSecretaDoAgenteDePressao2024!",
    "PrinterName": "EPSON TM-T20X Receipt",
    "FallbackIntervalMinutes": 5
  }
}
```

**Campos importantes:**
- `ApiUrl` → IP do servidor (máquina com Docker), **sem barra no final**
- `AgentKey` → deve ser **exatamente igual** ao `AGENT_KEY` do arquivo `.env` no servidor
- `PrinterName` → nome exato como aparece em "Impressoras e scanners" do Windows

Salve e feche.

---

## PASSO 5 — Testar o PrintAgent

Dê **duplo-clique** em `ScaleAtacado.PrintAgent.exe`.

O programa **não abre janela** — aparece um ícone na bandeja do sistema
(canto inferior direito da tela, próximo ao relógio).

**Clique com botão direito no ícone → "Configurar..."**

Na janela de configuração:
1. Verifique se a impressora aparece selecionada na lista
2. Confirme que a URL do servidor está correta
3. O painel de status deve mostrar:
   - **Verde:** conectado ao servidor via SignalR ✅
   - **Laranja:** modo fallback (conecta via HTTP a cada 5 min) — funciona, mas verifique a URL

4. Clique em **"Testar Impressão"** — deve imprimir um cupom de teste

---

## PASSO 6 — Configurar o PrintAgent para iniciar com o Windows

### Opção A — Pasta Startup (simples, para máquinas com usuário sempre logado):

1. Pressione **Win + R**, digite `shell:startup`, pressione **Enter**
2. Na pasta que abrir, crie um **atalho** para
   `C:\ScaleAtacado\PrintAgent\ScaleAtacado.PrintAgent.exe`
3. Pronto — o agente iniciará quando o usuário fizer login

### Opção B — Agendador de Tarefas (mais robusto):

Abra o **Agendador de Tarefas** (Win + R → `taskschd.msc` → Enter).

Clique em **"Criar Tarefa..."** (não "Tarefa Básica"):

**Aba Geral:**
- Nome: `ScaleAtacado PrintAgent`
- Marcar: **"Executar somente quando o usuário estiver conectado"**
- Marcar: **"Executar com os privilégios mais altos"**

**Aba Disparadores → Novo:**
- Iniciar a tarefa: **"Ao fazer logon"**
- Configurações: **"Qualquer usuário"**
- Clique OK

**Aba Ações → Nova:**
- Ação: **"Iniciar um programa"**
- Programa/script: `C:\ScaleAtacado\PrintAgent\ScaleAtacado.PrintAgent.exe`
- Clique OK

**Aba Condições:**
- Desmarcar: **"Iniciar a tarefa somente se o computador estiver na alimentação CA"**

**Aba Configurações:**
- Se a tarefa já estiver em execução: **"Não iniciar uma nova instância"**

Clique **OK** e informe a senha do usuário se solicitado.

---

---

# PARTE 3 — VERIFICAÇÃO FINAL

---

## Checklist de funcionamento

### No servidor:
```powershell
# 1. Verificar containers
docker compose -f C:\ScaleAtacado\docker-compose.yml ps

# Esperado: 3 containers Up
# scale_atacado_db   Up (healthy)
# scale_atacado_api  Up
# scale_atacado_web  Up   0.0.0.0:80->80/tcp
```

### Em um computador de operador (browser):
- [ ] Acessar `http://192.168.1.100` → aparecer tela de login
- [ ] Fazer login com admin → entrar no sistema
- [ ] Criar um pedido de teste no PDV

### Na máquina do PrintAgent:
- [ ] Ícone na bandeja do sistema visível
- [ ] Status verde (Hub conectado) na janela de configuração
- [ ] Ao clicar "Imprimir Recibo" em um pedido no browser → impressora imprime

---

## Resumo dos endereços

| De onde | Para quê | Endereço |
|---|---|---|
| Browser (qualquer PC da rede) | Acessar o sistema | `http://192.168.1.100` |
| PrintAgent (appsettings) | ApiUrl | `http://192.168.1.100` |
| Configuração inicial | Setup da empresa | `POST http://192.168.1.100/api/auth/setup` |

---

## Comandos de manutenção do servidor (PowerShell)

```powershell
# Entrar na pasta
cd C:\ScaleAtacado

# Ver status
docker compose ps

# Ver logs em tempo real
docker compose logs -f

# Ver logs só da API
docker compose logs -f api

# Reiniciar só a API (sem derrubar o banco)
docker compose restart api

# Parar tudo
docker compose down

# Subir tudo
docker compose up -d

# Atualizar o código e rebuildar
# (após copiar novos arquivos para C:\ScaleAtacado)
docker compose up -d --build

# Backup manual do banco
$data = Get-Date -Format "yyyyMMdd_HHmm"
docker exec scale_atacado_db pg_dump -U admin scale_atacado `
    | Out-File "C:\ScaleAtacado\Backups\backup_$data.sql" -Encoding utf8

# Restaurar backup
Get-Content "C:\ScaleAtacado\Backups\backup_20240101.sql" `
    | docker exec -i scale_atacado_db psql -U admin scale_atacado

# Liberar espaço — remover imagens antigas
docker image prune -f

# Verificar uso de CPU e memória dos containers
docker stats
```
