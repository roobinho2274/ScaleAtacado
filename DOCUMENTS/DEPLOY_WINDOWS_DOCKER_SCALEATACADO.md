# DEPLOY ScaleAtacado — Docker no Windows
## Do zero até acessível pela rede local

---

## Visão geral da arquitetura

```
Máquina Windows (ex: 192.168.1.100)
│
├── Docker Desktop (usa WSL2 internamente — transparente)
│   │
│   └── docker compose
│       ├── postgres-db  (interno, sem porta exposta)
│       ├── api          (interno :5000)
│       └── web/nginx    (porta 80 → acessível na rede)
│
└── ScaleAtacado.PrintAgent.exe  ← roda nativamente no Windows
    └── conecta em http://localhost  (mesma máquina!)
```

> Os containers são **Linux containers** rodando dentro do WSL2 que o Docker Desktop
> gerencia automaticamente. Os Dockerfiles não precisam de nenhuma alteração.

---

## Pré-requisitos da máquina Windows

- Windows 10 versão 2004+ (build 19041) ou Windows 11
- 8 GB RAM mínimo (recomendado 16 GB)
- Virtualização habilitada na BIOS (Intel VT-x / AMD-V)
- Espaço em disco: ~10 GB para imagens Docker + dados

---

## PASSO 1 — Instalar o Docker Desktop

1. Baixe em: **https://www.docker.com/products/docker-desktop/**
   (botão "Download for Windows")

2. Execute o instalador. Na tela de opções:
   - ✅ **"Use WSL 2 instead of Hyper-V"** — manter marcado (padrão)
   - ✅ **"Add shortcut to desktop"** — opcional

3. Reinicie o computador quando solicitado.

4. Após reiniciar, o Docker Desktop abre automaticamente.
   Aguarde o ícone da baleia na bandeja do sistema ficar **estático** (não animado).

5. Verifique no **PowerShell**:
   ```powershell
   docker --version
   docker compose version
   ```
   Deve mostrar versões sem erro.

### Configurar o Docker Desktop para iniciar com o Windows

No Docker Desktop: **Settings (⚙️) → General**
- ✅ **"Start Docker Desktop when you sign in to your computer"** — marcar

---

## PASSO 2 — Preparar a pasta de deploy

Crie uma pasta dedicada para o deploy (separada da pasta de desenvolvimento):

```powershell
New-Item -ItemType Directory -Force "C:\ScaleAtacado"
```

### Copiar os arquivos do projeto

**Opção A — Copiar da pasta de desenvolvimento (mesma máquina):**
```powershell
# Copiar tudo exceto bin/, obj/ e segredos
robocopy "C:\Users\robso\Documents\ACR\Project\ScaleAtacado" "C:\ScaleAtacado" /E /XD bin obj .git .vs node_modules /XF "appsettings.Development.json" "*.user"
```

**Opção B — Via Git (se o projeto estiver em um repositório):**
```powershell
cd C:\
git clone https://github.com/sua-org/scale-atacado.git ScaleAtacado
```

---

## PASSO 3 — Criar o arquivo `.env`

O `docker-compose.yml` lê as variáveis secretas de um arquivo `.env`.
Abra o Bloco de Notas ou VS Code para criar o arquivo:

```powershell
notepad "C:\ScaleAtacado\.env"
```

Cole o conteúdo abaixo, **substituindo os valores pelos reais**:

```env
# Senha do PostgreSQL
POSTGRES_PASSWORD=SenhaFortePostgres2024!

# Chave JWT — mínimo 32 caracteres aleatórios
JWT_KEY=MinhaChaveJWTSuperSecretaComPeloMenosTrintaEDoisChars!!

# Chave do PrintAgent (deve ser igual ao appsettings do agente)
AGENT_KEY=ChaveSecretaDoAgenteDePressao2024!

# URL de acesso pela rede (IP desta máquina Windows)
# Descobrir o IP: ipconfig | findstr "IPv4"
CORS_ORIGINS=http://192.168.1.100
```

Salve e feche.

> **Descobrir o IP desta máquina:**
> ```powershell
> ipconfig | findstr "IPv4"
> ```
> Use o IP da placa de rede (ex: `192.168.1.100`), não o `127.0.0.1`.

---

## PASSO 4 — Fechar a porta do banco em produção

Em produção, a porta 5432 do PostgreSQL **não deve ficar exposta** na rede.

Abra o arquivo:
```powershell
notepad "C:\ScaleAtacado\docker-compose.yml"
```

Encontre o bloco `postgres-db` e comente as linhas de `ports`:
```yaml
postgres-db:
  image: postgres:15-alpine
  # ports:               ← comente estas duas linhas
  #   - "5432:5432"
```

Salve e feche.

---

## PASSO 5 — Subir os containers

Abra o **PowerShell** (não precisa ser Administrador):

```powershell
cd C:\ScaleAtacado
docker compose up -d --build
```

### O que acontece na primeira execução:

1. Docker baixa as imagens base (.NET 8, nginx, PostgreSQL) — ~500 MB
2. Compila a API e o Blazor dentro dos containers (~5–10 min)
3. Sobe os 3 containers na ordem correta (banco → API → nginx)
4. A API aplica as migrations do banco automaticamente ao iniciar

### Acompanhar o progresso:
```powershell
docker compose logs -f
# Ctrl+C para sair sem derrubar os containers
```

Quando estiver pronto, você verá nos logs:
```
scale_atacado_api  | Now listening on: http://[::]:5000
```

---

## PASSO 6 — Verificar os containers

```powershell
docker compose ps
```

Resultado esperado:
```
NAME                 STATUS          PORTS
scale_atacado_db     Up (healthy)    5432/tcp
scale_atacado_api    Up              5000/tcp
scale_atacado_web    Up              0.0.0.0:80->80/tcp
```

**Testar localmente:**
```powershell
# Abre no browser padrão
Start-Process "http://localhost"
```
Deve abrir a tela de login do ScaleAtacado.

---

## PASSO 7 — Configuração inicial do sistema (uma vez)

Execute no PowerShell (substitua os dados reais):

```powershell
$body = @{
    adminName     = "Seu Nome Completo"
    adminEmail    = "admin@suaempresa.com"
    adminPassword = "SenhaAdmin123"
    companyName   = "Empresa Exemplo Ltda"
    companyCNPJ   = "00.000.000/0001-00"
    companyAddress = "Rua Exemplo, 123"
    companyPhone  = "(11) 99999-9999"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost/api/auth/setup" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body
```

Resposta esperada: JSON com o token JWT do admin. Após isso, acesse
`http://localhost` no browser e faça login.

> **Recomeçar do zero (apenas em ambiente de teste):**
> ```powershell
> docker exec -it scale_atacado_db psql -U admin -d scale_atacado -c 'DELETE FROM "Companies";'
> ```

---

## PASSO 8 — Liberar a porta 80 no Firewall do Windows

Para que outros computadores da rede local acessem a aplicação,
abra o **PowerShell como Administrador** e execute:

```powershell
New-NetFirewallRule `
    -DisplayName "ScaleAtacado HTTP" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort 80 `
    -Action Allow
```

**Verificar se a regra foi criada:**
```powershell
Get-NetFirewallRule -DisplayName "ScaleAtacado HTTP"
```

**Testar de outro computador da rede:**
Abra o browser em outro PC e acesse `http://192.168.1.100` (IP desta máquina).

---

## PASSO 9 — Auto-start automático dos containers

Com `restart: unless-stopped` já definido no `docker-compose.yml` e o
Docker Desktop configurado para iniciar com o Windows, os containers **já
sobem automaticamente** quando o Docker Desktop inicia após o login.

Porém, em máquinas que ficam ligadas sem usuário logado (modo servidor),
o Docker Desktop pode não iniciar. Nesse caso, crie uma tarefa no
Agendador de Tarefas:

### Via PowerShell (Administrador):

```powershell
$action = New-ScheduledTaskAction `
    -Execute "docker" `
    -Argument "compose -f C:\ScaleAtacado\docker-compose.yml up -d" `
    -WorkingDirectory "C:\ScaleAtacado"

$trigger = New-ScheduledTaskTrigger -AtStartup

$settings = New-ScheduledTaskSettingsSet `
    -StartWhenAvailable `
    -ExecutionTimeLimit (New-TimeSpan -Hours 0)

Register-ScheduledTask `
    -TaskName "ScaleAtacado Docker" `
    -Action $action `
    -Trigger $trigger `
    -Settings $settings `
    -RunLevel Highest `
    -Force
```

> **Nota:** Esta tarefa aguarda o Docker Desktop iniciar. Se o Docker demorar,
> pode falhar na primeira tentativa. A alternativa mais robusta é um script
> que tenta subir e aguarda o Docker estar pronto:

Crie `C:\ScaleAtacado\start.ps1`:
```powershell
# Aguarda Docker estar pronto
$timeout = 120
$elapsed = 0
while ($elapsed -lt $timeout) {
    try {
        docker info 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) { break }
    } catch {}
    Start-Sleep 5
    $elapsed += 5
}

# Sobe os containers
Set-Location "C:\ScaleAtacado"
docker compose up -d
```

E ajuste a tarefa para executar `powershell.exe` com argumento:
`-ExecutionPolicy Bypass -File C:\ScaleAtacado\start.ps1`

---

## PASSO 10 — PrintAgent na mesma máquina (vantagem do Windows)

Como o PrintAgent roda nativamente no Windows e a API está na mesma máquina
em Docker, o PrintAgent conecta via **`http://localhost`** — sem necessidade
de descobrir IPs ou abrir portas adicionais.

Configure o `appsettings.json` do PrintAgent:
```json
{
  "PrintAgent": {
    "ApiUrl": "http://localhost",
    "AgentKey": "ChaveSecretaDoAgenteDePressao2024!",
    "PrinterName": "EPSON TM-T20X",
    "FallbackIntervalMinutes": 5
  }
}
```

O PrintAgent pode rodar:
- **Manualmente:** duplo-clique no `.exe` (ícone na bandeja do sistema)
- **Com o Windows:** adicionar atalho na pasta `shell:startup`
- **Via Agendador de Tarefas:** conforme descrito no guia do PrintAgent

---

## Resumo — Endereços após o deploy

| De onde | Endereço |
|---|---|
| Na própria máquina (browser) | `http://localhost` |
| Outro PC na rede local | `http://192.168.1.100` (IP do Windows) |
| PrintAgent (mesma máquina) | `ApiUrl = "http://localhost"` |
| PrintAgent (outra máquina) | `ApiUrl = "http://192.168.1.100"` |
| Setup inicial | `POST http://localhost/api/auth/setup` |

---

## Comandos de manutenção (PowerShell)

```powershell
# Entrar na pasta do projeto
cd C:\ScaleAtacado

# Status dos containers
docker compose ps

# Logs em tempo real
docker compose logs -f api
docker compose logs -f web

# Reiniciar apenas a API
docker compose restart api

# Parar tudo
docker compose down

# Subir novamente
docker compose up -d

# Atualizar após mudança no código
# (se veio de robocopy, repita o robocopy antes)
docker compose up -d --build

# Backup do banco
$date = Get-Date -Format "yyyyMMdd_HHmm"
docker exec scale_atacado_db pg_dump -U admin scale_atacado `
    | Out-File "C:\ScaleAtacado\Backups\backup_$date.sql" -Encoding utf8

# Restaurar backup
Get-Content "C:\ScaleAtacado\Backups\backup_20240101.sql" | `
    docker exec -i scale_atacado_db psql -U admin scale_atacado

# Ver uso de memória/CPU dos containers
docker stats

# Remover imagens antigas após rebuild (libera espaço)
docker image prune -f
```

---

## Diferenças em relação ao deploy Linux

| Aspecto | Linux | Windows (este guia) |
|---|---|---|
| Container runtime | Docker Engine | Docker Desktop (WSL2 interno) |
| Containers | Linux | Linux (mesmo — via WSL2) |
| Firewall | `ufw allow 80` | `New-NetFirewallRule` |
| Auto-start | `systemctl` | Docker Desktop + Task Scheduler |
| PrintAgent | Outra máquina Windows | Mesma máquina (usa localhost) |
| Backups | `pg_dump > arquivo.sql` | `pg_dump \| Out-File arquivo.sql` |
| HTTPS fácil | Caddy | Caddy como container adicional |

---

## (Opcional) HTTPS com domínio próprio

Adicione o **Caddy** como container no `docker-compose.yml` para SSL automático:

### 1. Mudar a porta do nginx para não conflitar:

No `docker-compose.yml`, altere `web`:
```yaml
web:
  ports:
    - "127.0.0.1:8080:80"   # só acessível localmente pelo Caddy
```

### 2. Adicionar o container Caddy:

```yaml
caddy:
  image: caddy:alpine
  container_name: scale_atacado_caddy
  ports:
    - "80:80"
    - "443:443"
  volumes:
    - ./Caddyfile:/etc/caddy/Caddyfile:ro
    - caddy_data:/data
    - caddy_config:/config
  depends_on:
    - web
  restart: unless-stopped
```

### 3. Criar o arquivo `Caddyfile` na mesma pasta:

```
seudominio.com {
    reverse_proxy web:80
}
```

### 4. Adicionar os volumes ao final do docker-compose.yml:

```yaml
volumes:
  postgres_data:
  caddy_data:
  caddy_config:
```

### 5. Liberar portas 80 e 443 no firewall:

```powershell
New-NetFirewallRule -DisplayName "ScaleAtacado HTTP"  -Direction Inbound -Protocol TCP -LocalPort 80  -Action Allow
New-NetFirewallRule -DisplayName "ScaleAtacado HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow
```

O Caddy emite e renova o certificado Let's Encrypt automaticamente.
Em ~30 segundos após subir, `https://seudominio.com` estará funcionando.
