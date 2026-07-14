# ROTEIRO DE DEPLOY — ScaleAtacado

---

## PARTE 1 — API + Blazor no Linux (Docker Compose)

### Visão geral da arquitetura em produção

```
Internet (porta 80)
        │
        ▼
┌─── Docker Compose ──────────────────────────────────────┐
│                                                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │  web  (nginx — porta 80)                        │  │
│  │  • /             → serve Blazor WASM (estático) │  │
│  │  • /api/         → proxy http://api:5000/api/   │  │
│  │  • /hubs/        → proxy WebSocket http://api   │  │
│  └──────────────────────────────────────────────────┘  │
│                          ▲                              │
│  ┌───────────────┐  ┌────┴───────────────────────────┐ │
│  │  postgres-db  │  │  api (ASP.NET Core — 5000)     │ │
│  │  (interno)    │◄─│  migrations automáticas        │ │
│  └───────────────┘  └────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

A API **não tem porta exposta** para fora — só o nginx (porta 80) é acessível externamente.
O Blazor faz requisições ao próprio nginx (`/api/...`) que as encaminha para a API internamente.
Não há CORS entre browser e API no fluxo normal.

---

### Passo 1 — Instalar Docker no servidor Linux

```bash
# Ubuntu 22.04 LTS (ou Debian 12)
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
newgrp docker

# Verificar
docker --version        # Docker version 26.x ou superior
docker compose version  # Docker Compose version v2.x
```

---

### Passo 2 — Enviar o projeto para o servidor

**Opção A — via Git (recomendado):**
```bash
# No servidor Linux
sudo mkdir -p /opt/scale-atacado
sudo chown $USER:$USER /opt/scale-atacado

git clone https://github.com/sua-org/scale-atacado.git /opt/scale-atacado
cd /opt/scale-atacado
```

**Opção B — via rsync do Windows (sem Git no servidor):**
```powershell
# No Windows PowerShell, dentro da pasta raiz do projeto
rsync -avz `
  --exclude='.git' --exclude='bin/' --exclude='obj/' `
  --exclude='*.user' --exclude='appsettings.Development.json' `
  . usuario@IP_DO_SERVIDOR:/opt/scale-atacado/
```
> Se não tiver rsync no Windows, use o **WinSCP** ou **MobaXterm** para copiar a pasta.

---

### Passo 3 — Criar o arquivo `.env`

O `docker-compose.yml` lê segredos de um arquivo `.env` na mesma pasta.
**Este arquivo nunca vai para o Git.**

```bash
cd /opt/scale-atacado
nano .env
```

```env
# Senha do PostgreSQL
POSTGRES_PASSWORD=SenhaFortePostgres2024!

# Chave JWT — mínimo 32 caracteres aleatórios
# Gerar: openssl rand -base64 48
JWT_KEY=MinhaChaveJWTSuperSecretaComPeloMenosTrintaEDoisChars!!

# Chave secreta do PrintAgent (deve ser idêntica ao appsettings do agente)
AGENT_KEY=ChaveSecretaDoAgenteDePressao2024!

# URL de onde o sistema será acessado (para CORS — sem barra no final)
# Exemplos: http://192.168.1.10  ou  http://seudominio.com
CORS_ORIGINS=http://IP_OU_DOMINIO_DO_SERVIDOR
```

```bash
# Proteger o arquivo (só o dono pode ler)
chmod 600 .env
```

> **Gerar JWT_KEY de forma segura:**
> ```bash
> openssl rand -base64 48
> ```

---

### Passo 4 — Fechar porta do banco em produção

O `docker-compose.yml` expõe a porta 5432 do banco (para uso em desenvolvimento).
Em produção, ela não deve ficar aberta. Edite o arquivo:

```bash
nano docker-compose.yml
```

No bloco `postgres-db`, comente as linhas de `ports`:
```yaml
postgres-db:
  image: postgres:15-alpine
  # ports:         # ← comentar em produção
  #   - "5432:5432"
```

---

### Passo 5 — Subir os containers

```bash
cd /opt/scale-atacado
docker compose up -d --build
```

Na **primeira execução**, o Docker faz build das imagens (download das imagens base + compilação do projeto).
Isso leva entre 3 e 8 minutos dependendo da conexão do servidor.

**Acompanhar o progresso:**
```bash
docker compose logs -f
# Ctrl+C para sair dos logs sem derrubar os containers
```

Quando a API subir com sucesso, você verá nos logs:
```
scale_atacado_api  | info: Applying pending migrations...
scale_atacado_api  | Now listening on: http://[::]:5000
```

---

### Passo 6 — Verificar os containers

```bash
docker compose ps
```

Esperado:
```
NAME                 STATUS          PORTS
scale_atacado_db     Up (healthy)    5432/tcp
scale_atacado_api    Up              5000/tcp
scale_atacado_web    Up              0.0.0.0:80->80/tcp
```

**Testar se a aplicação responde:**
```bash
curl http://localhost
# Deve retornar HTML do Blazor

curl -s http://localhost/api/auth/setup | head -c 200
# Deve retornar JSON (mesmo que seja "já configurado")
```

---

### Passo 7 — Configuração inicial do sistema (primeira vez)

Execute **uma única vez** para criar a empresa e o primeiro administrador:

```bash
curl -X POST http://IP_DO_SERVIDOR/api/auth/setup \
  -H "Content-Type: application/json" \
  -d '{
    "adminName": "Seu Nome Completo",
    "adminEmail": "admin@suaempresa.com",
    "adminPassword": "SenhaAdmin123",
    "companyName": "Empresa Exemplo Ltda",
    "companyCNPJ": "00.000.000/0001-00",
    "companyAddress": "Rua Exemplo, 123 — Cidade/UF",
    "companyPhone": "(11) 99999-9999"
  }'
```

Resposta esperada: JSON com o token JWT do admin recém-criado.
Após isso, acesse `http://IP_DO_SERVIDOR` no browser e faça login.

> **Recomeçar do zero (apenas em ambiente de teste):**
> ```bash
> docker exec -it scale_atacado_db psql -U admin -d scale_atacado -c 'DELETE FROM "Companies";'
> ```

---

### Passo 8 — Configurar firewall (Ubuntu com UFW)

```bash
sudo ufw allow 22/tcp    # SSH
sudo ufw allow 80/tcp    # HTTP
sudo ufw allow 443/tcp   # HTTPS (para quando adicionar SSL)
sudo ufw enable
sudo ufw status
```

---

### Passo 9 (opcional) — HTTPS com domínio próprio via Caddy

Se tiver um domínio apontado para o servidor, use o **Caddy** como proxy reverso com SSL automático (Let's Encrypt).

**9.1 — Mudar a porta do nginx para não conflitar com o Caddy:**

No `docker-compose.yml`, altere a porta do serviço `web`:
```yaml
web:
  ports:
    - "127.0.0.1:8080:80"   # nginx só aceita conexões locais
```

**9.2 — Instalar o Caddy:**
```bash
sudo apt install -y debian-keyring debian-archive-keyring apt-transport-https curl
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | sudo gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' | sudo tee /etc/apt/sources.list.d/caddy-stable.list
sudo apt update && sudo apt install caddy
```

**9.3 — Criar o Caddyfile:**
```bash
sudo nano /etc/caddy/Caddyfile
```

```
seudominio.com {
    reverse_proxy localhost:8080
}
```

```bash
sudo systemctl reload caddy
```

O Caddy emite e renova o certificado SSL automaticamente.
Em 30 segundos, `https://seudominio.com` estará funcionando.

**9.4 — Atualizar o `.env` com a URL HTTPS:**
```env
CORS_ORIGINS=https://seudominio.com
```
```bash
docker compose up -d  # Reinicia a API com a nova env var
```

---

### Comandos de manutenção

```bash
# Atualizar após nova versão do código
cd /opt/scale-atacado && git pull
docker compose up -d --build
# Migrations são aplicadas automaticamente pela API ao reiniciar

# Logs em tempo real
docker compose logs -f api
docker compose logs -f web

# Reiniciar apenas a API
docker compose restart api

# Parar tudo
docker compose down

# Backup do banco
docker exec scale_atacado_db pg_dump -U admin scale_atacado \
  > backup_$(date +%Y%m%d_%H%M).sql

# Restaurar backup
docker exec -i scale_atacado_db psql -U admin scale_atacado < backup_20240101.sql

# Ver uso de CPU/memória dos containers
docker stats
```

---

---

## PARTE 2 — PrintAgent no Windows

O PrintAgent é um aplicativo de **bandeja do sistema (system tray)** que:
- Conecta ao servidor Linux via **SignalR WebSocket**
- Recebe notificações de novos pedidos para imprimir
- Envia para a impressora **EPSON TM-T20X** conectada localmente via GDI

```
Browser (Blazor) → API (Linux) → SignalR → PrintAgent (Windows) → Impressora
```

---

### Pré-requisitos

- Windows 10/11 (64-bit)
- Impressora **EPSON TM-T20X** instalada no Windows (driver EPSON instalado, aparece em "Impressoras e scanners")
- Acesso de rede à porta **80** do servidor Linux
- .NET 8 Desktop Runtime instalado **OU** publicação self-contained (sem dependência)

> **Baixar .NET 8 Runtime (se necessário):**
> dotnet.microsoft.com/download/dotnet/8.0 → ".NET Desktop Runtime 8.x"

---

### Passo 1 — Publicar o PrintAgent (na máquina de desenvolvimento)

```powershell
# Na pasta raiz do projeto, no Windows
dotnet publish .\ScaleAtacado.PrintAgent\ `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -o .\dist\PrintAgent
```

Isso gera `dist\PrintAgent\ScaleAtacado.PrintAgent.exe` (~80 MB) que **não precisa do .NET instalado** na máquina de destino.

---

### Passo 2 — Copiar para a máquina do caixa

Copie **toda a pasta** `dist\PrintAgent\` para a máquina Windows onde a impressora está conectada.

Exemplo de destino:
```
C:\ScaleAtacado\PrintAgent\
    ScaleAtacado.PrintAgent.exe
    appsettings.json
```

---

### Passo 3 — Configurar o `appsettings.json`

Na pasta copiada, edite o `appsettings.json`:

```json
{
  "PrintAgent": {
    "ApiUrl": "http://IP_DO_SERVIDOR",
    "AgentKey": "ChaveSecretaDoAgenteDePressao2024!",
    "PrinterName": "EPSON TM-T20X",
    "FallbackIntervalMinutes": 5
  }
}
```

> **IMPORTANTE:** `AgentKey` deve ser **exatamente igual** ao `AGENT_KEY` do arquivo `.env` no servidor Linux.

> `PrinterName` deve ser exatamente como aparece em **Configurações → Impressoras e scanners** no Windows. Copie o nome exato.

---

### Passo 4 — Executar e testar

Dê duplo-clique em `ScaleAtacado.PrintAgent.exe`.
O programa **não abre janela** — o ícone aparece na bandeja do sistema (canto inferior direito, próximo ao relógio).

- **Botão direito no ícone** → "Configurar..." para abrir a janela de configuração
- Na janela: selecione a impressora na lista, confira a URL e a chave, clique em **"Testar Impressão"**
- O painel de status mostra **verde** (conectado ao Hub) ou **laranja** (modo fallback)
- Clique **"Salvar"** para persistir as configurações

---

### Passo 5 — Iniciar automaticamente com o Windows

**Opção A — Pasta Startup (mais simples):**

1. Pressione `Win + R`, digite `shell:startup`, pressione Enter
2. Crie um atalho para `C:\ScaleAtacado\PrintAgent\ScaleAtacado.PrintAgent.exe` nessa pasta
3. Pronto — o agente iniciará automaticamente quando o usuário fizer login

**Opção B — Agendador de Tarefas (mais robusto, reinicia se travar):**

Abra o **Agendador de Tarefas** (`taskschd.msc`) e crie uma nova tarefa:

| Aba | Campo | Valor |
|---|---|---|
| Geral | Nome | ScaleAtacado PrintAgent |
| Geral | Executar como | Usuário atual (com sessão aberta) |
| Disparadores | Novo disparador | Ao fazer logon — Qualquer usuário |
| Ações | Programa | `C:\ScaleAtacado\PrintAgent\ScaleAtacado.PrintAgent.exe` |
| Condições | Alimentação CA | **Desmarcar** "Iniciar apenas se na alimentação CA" |
| Configurações | Se a tarefa já estiver em execução | Não iniciar nova instância |

Clique OK, informe a senha se pedido.

---

### Fluxo completo após instalação

1. Servidor Linux ativo com `docker compose up -d`
2. PrintAgent iniciado no Windows (conectado ao SignalR)
3. Operador acessa `http://IP_DO_SERVIDOR` no browser → faz login
4. Cria pedido no PDV → clica "Imprimir Recibo" → "Confirmar Impressão"
5. API envia evento SignalR → PrintAgent imprime fisicamente → status atualiza na tela

---

### Resumo dos endereços após deploy

| O quê | Endereço |
|---|---|
| Sistema (browser) | `http://IP_DO_SERVIDOR` |
| API REST | `http://IP_DO_SERVIDOR/api/...` |
| SignalR Hub (PrintAgent) | `http://IP_DO_SERVIDOR/hubs/print` |
| Setup inicial (uma vez) | `POST http://IP_DO_SERVIDOR/api/auth/setup` |
