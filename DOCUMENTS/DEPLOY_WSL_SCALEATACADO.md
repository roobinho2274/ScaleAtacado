# DEPLOY ScaleAtacado via WSL2 no Windows
## Do zero até acessível pela rede local

---

## Visão geral

```
Máquina Windows (ex: 192.168.1.100)
│
├── WSL2 Ubuntu 22.04
│   └── Docker Compose
│       ├── postgres-db  (interno)
│       ├── api          (interno :5000)
│       └── web/nginx    (:80)  ← aplicação aqui
│
└── Porta 80 acessível por qualquer PC da rede local
    ex: http://192.168.1.100
```

---

## PARTE 1 — Instalar o WSL2

### Pré-requisitos da máquina Windows

- Windows 10 versão 2004+ (build 19041) ou Windows 11
- Virtualização habilitada na BIOS (Intel VT-x / AMD-V)
- 4 GB RAM mínimo, recomendado 8 GB

### 1.1 — Habilitar o WSL2

Abra o **PowerShell como Administrador** (`Win + X → Terminal (Admin)`) e execute:

```powershell
wsl --install
```

Esse único comando:
- Habilita os recursos "Plataforma de Máquina Virtual" e "Subsistema Windows para Linux"
- Define o WSL2 como versão padrão
- Baixa e instala o Ubuntu automaticamente

**Reinicie o computador quando solicitado.**

> Se tiver WSL1 de antes, atualize para WSL2:
> ```powershell
> wsl --set-default-version 2
> ```

### 1.2 — Instalar o Ubuntu 22.04

Após reiniciar, abra o **PowerShell como Administrador** e instale o Ubuntu 22.04 especificamente:

```powershell
wsl --install -d Ubuntu-22.04
```

Uma janela de terminal abrirá pedindo para criar um usuário Linux:
- **Digite um nome de usuário** (ex: `ubuntu`) — pode ser simples, sem espaços
- **Digite uma senha** — anote, será pedida ao usar `sudo`
- Repita a senha

### 1.3 — Verificar a instalação

```powershell
wsl --list --verbose
```

Resultado esperado:
```
  NAME            STATE           VERSION
* Ubuntu-22.04    Running         2
```

A coluna VERSION deve mostrar `2`. Se mostrar `1`, converta:
```powershell
wsl --set-version Ubuntu-22.04 2
```

---

## PARTE 2 — Configurar rede para acesso pela rede local

Por padrão, o WSL2 usa **NAT** (rede isolada). Outros computadores da rede não conseguem acessar diretamente. Há duas formas de resolver, dependendo do seu Windows:

---

### Opção A — Windows 11 22H2 ou superior (RECOMENDADO — mais simples)

A partir do Windows 11 build 22621.2359, existe o modo **"Mirrored Networking"** que compartilha o IP do Windows com o WSL2.

**1. Verificar se seu Windows suporta:**
```powershell
winver
```
Se a versão for **22H2 build 22621** ou superior, use esta opção.

**2. Criar ou editar o arquivo `.wslconfig`:**

Abra o Bloco de Notas e crie o arquivo em `C:\Users\SEU_USUARIO\.wslconfig`:
```powershell
notepad "$env:USERPROFILE\.wslconfig"
```

Cole o conteúdo:
```ini
[wsl2]
networkingMode=mirrored
autoProxy=false
```

Salve e feche.

**3. Reiniciar o WSL2:**
```powershell
wsl --shutdown
wsl -d Ubuntu-22.04
```

**Resultado:** O WSL2 agora usa o **mesmo IP** que a máquina Windows. Tudo que rodar na porta 80 do WSL2 estará automaticamente acessível por `http://IP_DO_WINDOWS` de qualquer máquina da rede.

---

### Opção B — Windows 10 ou Windows 11 versões mais antigas (port forwarding)

O WSL2 tem um IP interno diferente do Windows (ex: `172.x.x.x`). É preciso encaminhar a porta 80 do Windows para o WSL2.

**1. Criar o script de port forwarding:**

Crie o arquivo `C:\ScaleAtacado\wsl-portforward.ps1`:

```powershell
# Descobrir o IP atual do WSL2
$wslIP = (wsl hostname -I).Trim().Split()[0]
Write-Host "IP do WSL2: $wslIP"

# Remover regras antigas
netsh interface portproxy delete v4tov4 listenport=80 listenaddress=0.0.0.0 2>$null

# Adicionar novo encaminhamento
netsh interface portproxy add v4tov4 `
    listenport=80 listenaddress=0.0.0.0 `
    connectport=80 connectaddress=$wslIP

# Verificar
netsh interface portproxy show v4tov4
Write-Host "Port forwarding 0.0.0.0:80 -> ${wslIP}:80 configurado."
```

**2. Executar o script como Administrador:**
```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
C:\ScaleAtacado\wsl-portforward.ps1
```

**3. Liberar porta 80 no firewall do Windows:**
```powershell
netsh advfirewall firewall add rule `
    name="WSL2 ScaleAtacado HTTP" `
    dir=in action=allow protocol=TCP localport=80
```

**4. Automatizar na inicialização do Windows:**

Crie uma tarefa no Agendador de Tarefas (`taskschd.msc`):
- Nome: `WSL2 PortForward`
- Executar como: SYSTEM
- Disparador: Ao iniciar o computador
- Ação: `powershell.exe -ExecutionPolicy Bypass -File C:\ScaleAtacado\wsl-portforward.ps1`
- Condições: desmarcar "Alimentação CA"

> **Por que é necessário?** O IP do WSL2 muda a cada reinicialização, então o script precisa rodar toda vez.

---

### Descobrir o IP da máquina Windows (para compartilhar na rede)

No PowerShell do Windows:
```powershell
ipconfig | findstr "IPv4"
```

O IP da sua placa de rede (ex: `192.168.1.100`) é o endereço que outros computadores usarão para acessar a aplicação.

---

## PARTE 3 — Instalar Docker dentro do WSL2

Abra o terminal do Ubuntu (procure "Ubuntu" no menu Iniciar ou execute `wsl` no PowerShell).

### 3.1 — Atualizar o sistema

```bash
sudo apt update && sudo apt upgrade -y
```

### 3.2 — Instalar Docker Engine

```bash
# Instalar Docker direto (script oficial)
curl -fsSL https://get.docker.com | sh

# Adicionar usuário ao grupo docker (evita sudo a cada comando)
sudo usermod -aG docker $USER

# Aplicar o grupo sem precisar fazer logout
newgrp docker
```

### 3.3 — Habilitar o Docker para iniciar automaticamente

O Ubuntu no WSL2 suporta **systemd** (habilitado por padrão no Ubuntu 22.04 com WSL2 moderno).

Verifique se o systemd está ativo:
```bash
systemctl is-system-running
```

Se retornar `running` ou `degraded`, o systemd está ativo. Habilite o Docker:
```bash
sudo systemctl enable docker
sudo systemctl start docker
```

**Se o systemd não estiver ativo** (WSL2 mais antigo), habilite-o:
```bash
# Editar configuração do WSL
sudo nano /etc/wsl.conf
```

Adicione:
```ini
[boot]
systemd=true
```

```bash
# Sair do WSL e reiniciar
exit
```
```powershell
# No PowerShell do Windows
wsl --shutdown
wsl -d Ubuntu-22.04
```

Depois volte e execute o `systemctl enable docker`.

### 3.4 — Verificar

```bash
docker --version
docker compose version
docker run hello-world
```

---

## PARTE 4 — Copiar o projeto para o WSL2

**Importante:** Não rode o Docker Compose a partir do caminho `/mnt/c/...` (Windows). O desempenho é muito ruim. Copie o projeto para dentro do filesystem Linux.

### 4.1 — Copiar os arquivos

No terminal do Ubuntu (WSL2):

```bash
# Criar pasta de destino
mkdir -p ~/scale-atacado

# Copiar do Windows para o Linux (substitua 'robso' pelo seu usuário Windows)
cp -r /mnt/c/Users/robso/Documents/ACR/Project/ScaleAtacado/. ~/scale-atacado/

cd ~/scale-atacado
ls
```

Deve listar os projetos: `ScaleAtacado.Api`, `ScaleAtacado.Blazor`, `docker-compose.yml`, etc.

> **Alternativa com Git:** Se o projeto estiver em um repositório:
> ```bash
> git clone https://github.com/sua-org/scale-atacado.git ~/scale-atacado
> cd ~/scale-atacado
> ```

---

## PARTE 5 — Configurar e subir a aplicação

### 5.1 — Criar o arquivo `.env`

```bash
cd ~/scale-atacado
nano .env
```

```env
# Banco de dados
POSTGRES_PASSWORD=SenhaFortePostgres2024!

# JWT — mínimo 32 caracteres
# Gerar: openssl rand -base64 48
JWT_KEY=MinhaChaveJWTSuperSecretaComPeloMenosTrintaEDoisChars!!

# PrintAgent
AGENT_KEY=ChaveSecretaDoAgenteDePressao2024!

# URL de acesso (IP da máquina Windows na rede local)
CORS_ORIGINS=http://192.168.1.100
```

```bash
chmod 600 .env
```

> Descubra o IP do Windows com `ipconfig` no PowerShell e substitua `192.168.1.100`.

### 5.2 — (Opcional) Fechar porta do banco em produção

```bash
nano docker-compose.yml
```

Comente as linhas de porta do postgres:
```yaml
# ports:
#   - "5432:5432"
```

### 5.3 — Subir os containers

```bash
cd ~/scale-atacado
docker compose up -d --build
```

Primeira execução: baixa as imagens base e compila os projetos (~5-10 min dependendo da internet).

Acompanhe os logs:
```bash
docker compose logs -f
```

Quando aparecer `Now listening on: http://[::]:5000`, a API está pronta.

### 5.4 — Verificar

```bash
docker compose ps
```

```
NAME                 STATUS          PORTS
scale_atacado_db     Up (healthy)    5432/tcp
scale_atacado_api    Up              5000/tcp
scale_atacado_web    Up              0.0.0.0:80->80/tcp
```

```bash
# Testar localmente dentro do WSL2
curl http://localhost
# Deve retornar HTML do Blazor
```

---

## PARTE 6 — Configuração inicial do sistema (uma única vez)

```bash
curl -X POST http://localhost/api/auth/setup \
  -H "Content-Type: application/json" \
  -d '{
    "adminName": "Administrador",
    "adminEmail": "admin@suaempresa.com",
    "adminPassword": "SenhaAdmin123",
    "companyName": "Nome da Empresa Ltda",
    "companyCNPJ": "00.000.000/0001-00",
    "companyAddress": "Rua Exemplo, 123",
    "companyPhone": "(11) 99999-9999"
  }'
```

---

## PARTE 7 — Testar acesso pela rede local

Em **outro computador** da mesma rede, abra o browser e acesse:

```
http://IP_DA_MAQUINA_WINDOWS
```

Exemplo: `http://192.168.1.100`

Deve aparecer a tela de login do ScaleAtacado.

**Se não abrir, verificar checklist:**
- [ ] `docker compose ps` mostra todos os containers como `Up`
- [ ] `curl http://localhost` funciona dentro do WSL2
- [ ] (Opção A) `.wslconfig` com `networkingMode=mirrored` e WSL2 reiniciado
- [ ] (Opção B) Script de port forwarding foi executado como admin
- [ ] Firewall do Windows permite porta 80 (regra criada no Passo 2)
- [ ] Firewall da rede/roteador não bloqueia porta 80 entre as máquinas

---

## PARTE 8 — Iniciar automaticamente com o Windows

Por padrão, o WSL2 e os containers só sobem quando alguém abre o terminal.
Para um uso "tipo servidor", configure a inicialização automática:

### 8.1 — WSL2 iniciar automaticamente no boot

Crie o arquivo `C:\ScaleAtacado\start-scale.bat`:

```bat
@echo off
wsl -d Ubuntu-22.04 -u root -e bash -c "cd ~/scale-atacado && docker compose up -d"
```

Crie uma tarefa no **Agendador de Tarefas** (`taskschd.msc`):

| Campo | Valor |
|---|---|
| Nome | ScaleAtacado AutoStart |
| Executar como | SYSTEM |
| Executar com privilégios mais altos | Sim (marcar) |
| Disparador | Ao iniciar o computador |
| Ação — Programa | `C:\Windows\System32\cmd.exe` |
| Ação — Argumentos | `/c C:\ScaleAtacado\start-scale.bat` |
| Condições — Alimentação CA | Desmarcar |
| Configurações — Se já estiver em execução | Não iniciar nova instância |

> **Atenção:** Se estiver usando port forwarding (Opção B), adicione o script `wsl-portforward.ps1` como uma segunda ação nessa mesma tarefa, ou crie uma tarefa separada para ele.

### 8.2 — Verificar se subiu após reiniciar

Depois de reiniciar o Windows, aguarde ~30 segundos e teste:
```powershell
# No PowerShell do Windows
curl http://localhost
```

---

## Resumo rápido — o que acessar de onde

| De qual máquina | URL |
|---|---|
| Mesma máquina Windows (host) | `http://localhost` |
| Outro PC na rede local | `http://192.168.1.100` (IP do Windows) |
| PrintAgent | ApiUrl = `http://192.168.1.100` |

---

## Comandos úteis do dia a dia

```bash
# Entrar no WSL2 (pelo PowerShell do Windows)
wsl -d Ubuntu-22.04

# Dentro do WSL2 — gerenciar a aplicação
cd ~/scale-atacado
docker compose ps               # ver status
docker compose logs -f api      # logs da API em tempo real
docker compose restart api      # reiniciar só a API
docker compose down             # parar tudo
docker compose up -d            # subir tudo

# Atualizar após mudança no código
cp -r /mnt/c/Users/robso/Documents/ACR/Project/ScaleAtacado/. ~/scale-atacado/
docker compose up -d --build

# Backup do banco
docker exec scale_atacado_db pg_dump -U admin scale_atacado \
  > ~/backups/backup_$(date +%Y%m%d_%H%M).sql

# Fechar o WSL2 (desliga todos os containers junto)
# No PowerShell do Windows:
wsl --shutdown
```
