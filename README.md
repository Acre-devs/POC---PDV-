# POC — PDV Local com Angular 20 + ASP.NET Core 10

Prova de Conceito (POC) de uma arquitetura para **PDV Local** hospedado no computador servidor da loja, fornecendo a interface web (Angular 20) e API REST (ASP.NET Core 10) diretamente pelo mesmo executável Windows, com persistência local em **SQLite**.

---

## 1. Arquitetura do Sistema

```text
Computador Servidor da Loja (Windows)
  │
  └── PdvLocal.Api.exe (ASP.NET Core 10 / win-x64)
       │
       ├── 1. API REST (Controllers C# 14 / EF Core 10)
       │     └── Banco SQLite Local (data/pdv.db)
       │
       └── 2. Servidor de Arquivos Estáticos / SPA (wwwroot)
             └── Front-end Angular 20 (Compilado)
       │
       ▼
  Escutando em HTTP 0.0.0.0:5000
       │
       ├── Acesso Local: http://localhost:5000
       └── Acesso na Rede: http://<IP_DO_SERVIDOR>:5000
```

---

## 2. Requisitos e Versões Utilizadas

* **C#**: 14
* **.NET SDK**: 10.0.401 (.NET 10 LTS)
* **ASP.NET Core**: 10.0
* **Entity Framework Core**: 10.0 (SQLite)
* **Angular CLI**: 20.3.37
* **Node.js**: v22.20.0 (LTS)
* **Banco de Dados**: SQLite

---

## 3. Estrutura de Pastas

```text
pdv-local-poc/
│
├── backend/
│   ├── PdvLocal.sln
│   ├── PdvLocal.Domain/         # Entidades de Negócio (Produto)
│   ├── PdvLocal.Application/    # Casos de Uso, DTOs e Interfaces
│   ├── PdvLocal.Infrastructure/ # DbContext, SQLite e Serviços
│   └── PdvLocal.Api/            # ASP.NET Core Web API + SPA Host (wwwroot)
│
├── frontend/
│   └── pdv-local/               # Aplicação Angular 20 SPA
│
└── README.md
```

---

## 4. Como Executar em Desenvolvimento

### Opção A: Executar Backend e Frontend Separados (Dev Mode)

1. **Iniciar o Backend (ASP.NET Core)**:
   ```bash
   cd backend/PdvLocal.Api
   dotnet run
   ```
   * O backend estará disponível em `http://localhost:5000`.

2. **Iniciar o Frontend (Angular Dev Server com Proxy)**:
   ```bash
   cd frontend/pdv-local
   npm start
   # Ou via Angular CLI:
   npx ng serve --proxy-config proxy.conf.json
   ```
   * O frontend Angular estará acessível em `http://localhost:4200` e redirecionará requisições `/api/*` automaticamente para `http://localhost:5000`.

---

## 5. Como Compilar o Angular para o ASP.NET Core

Para gerar os arquivos estáticos de produção do Angular dentro da pasta `wwwroot` do backend:

```bash
cd frontend/pdv-local
npx ng build
```

* O arquivo `angular.json` já está preconfigurado para direcionar o build diretamente para:
  `../../backend/PdvLocal.Api/wwwroot`

---

## 6. Como Publicar e Gerar o Executável Windows (`.exe`)

Execute o comando `dotnet publish` no projeto da API especificando o runtime `win-x64` e self-contained:

```bash
cd backend/PdvLocal.Api
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish
```

### Estrutura Gerada na Pasta `./publish`:
* `PdvLocal.Api.exe` (Executável principal)
* `wwwroot/` (Front-end Angular 20 compilado)
* `data/` (Criado automaticamente na primeira execução para conter o `pdv.db`)

---

## 7. Como Executar o `.exe` Publicado

1. Acesse o diretório de publicação `backend/PdvLocal.Api/publish`.
2. Dê um duplo clique ou execute via prompt de comando:
   ```cmd
   PdvLocal.Api.exe
   ```
3. O terminal exibirá:
   ```text
   Now listening on: http://0.0.0.0:5000
   Application started.
   ```
4. Abra o navegador em:
   * **Localmente**: `http://localhost:5000`
   * **Outro computador na mesma rede**: `http://<IP_DO_SERVIDOR>:5000`

---

## 8. Como Liberar o Acesso no Windows Firewall

Para permitir que outros computadores da rede local acessem o PDV no IP do servidor, libere a porta `5000` no Windows Firewall.

### Via PowerShell (Como Administrador):
```powershell
New-NetFirewallRule -Name "PDVLocal_HTTP_5000" `
                    -DisplayName "PDV Local Servidor (Porta 5000)" `
                    -Direction Inbound `
                    -Protocol TCP `
                    -LocalPort 5000 `
                    -Action Allow
```

### Via Painel de Controle:
1. Abra **Configurações Avançadas do Windows Firewall**.
2. Clique em **Regras de Entrada** -> **Nova Regra...**
3. Selecione **Porta** -> **TCP** -> Especificar porta local: `5000`.
4. Selecione **Permitir a conexão** e conclua a criação da regra.

---

## 9. Localização do Banco de Dados SQLite

O banco SQLite é criado automaticamente no primeiro arranque na pasta relativa:
```text
backend/PdvLocal.Api/data/pdv.db
```
Em modo publicado, ele fica localizado no mesmo diretório do executável:
```text
publish/data/pdv.db
```

---

## 10. Como Alterar a Porta HTTP

Caso deseje alterar a porta padrão `5000` para outra (ex: `8080`):

1. No arquivo `PdvLocal.Api/Program.cs`:
   ```csharp
   builder.WebHost.UseUrls("http://0.0.0.0:8080");
   ```
2. No arquivo `PdvLocal.Api/Properties/launchSettings.json`:
   ```json
   "applicationUrl": "http://0.0.0.0:8080"
   ```

---

## 11. Arquitetura Futura (Roadmap ERP)

Este POC valida o funcionamento local. A arquitetura futura expandirá para:

```text
PDV (Estação / Caixa)
  ├── Angular 20 (SPA Offline-first)
  ├── Agente Local C#
  └── SQLite Contingência
       │
       ▼
Servidor Local da Loja
  ├── ASP.NET Core 10 API
  ├── PostgreSQL (Banco central da loja)
  ├── Módulo Fiscal (NFC-e / SAT)
  └── Serviço de Sincronização
       │
       ▼
ERP Cloud
  └── Sincronização Assíncrona & Dashboard Central
```
