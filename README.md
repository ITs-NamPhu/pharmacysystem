# 🏥 Pharmacy Management System

## 📖 Description

**Pharmacy Management System** is a web-based application designed to manage pharmacy operations.

The system provides role-based access control and supports multi-branch pharmacy management.

Building AI Agent assists staff can query business data through a chat interface, and a forecasting agent can recommend purchase quantities by correlating sales history with weather, air quality, search trends.

The project consists of:

- **Backend** — ASP.NET Core Web API (`.NET 9`)
- **Frontend** — React 18 single-page application
- **Database** — Microsoft SQL Server 2022
- **AI Service** — Python (FastAPI + LangChain / LangGraph / Langfuse)
- **Deployment** — Docker Compose, Github Action

---

## ✨ Features

### 🔐 Access Control
- **Role-based access control** across 5 roles (`admin`, `manage_supply`, `manage_branch`, `user_sale`, `user_warehouse`) and 100 granular permissions, enforced by a dynamic authorization policy provider.
- **Multi-branch data scoping** — every request is scoped to a branch through the `X-Branch-Id` header, so one account can work across several branches with a different role in each.
- **JWT authentication** — short-lived access token (15 min) plus rotating refresh tokens (7 days, stored as SHA-256 hashes), BCrypt password hashing, and automatic account lockout after 5 failed attempts.
- **Separation of duties** — a user can never approve a document they created themselves.

### 💊 Inventory & Pharmacy Operations
- **FEFO batch tracking** — every stock movement is tracked at batch level with manufacture and expiry dates; batches are allocated first-expired-first-out, or manual option.
- **Goods receipt workflow** — draft → pending → approved → completed (or rejected) with per-batch stock creation on completion.
- **Sales & payments** — invoices, payment receipts, customer wallet (cash and wallet-based), and credit/debt tracking.
- **Monthly debt closing** — per-customer debt summaries with opening balance, increases, payments and a locked closing balance.
- **Stock take pipeline** — count sheets with system-vs-actual variance, then stock adjustment and destroy receipts to write off expired or damaged goods.
- **Master data** — medicines with categories, manufacturers, units and unit conversion; customers, suppliers.

### 📊 Reporting & Operations
- **Dashboard** — is designed difference for 5 role and system have 21 endpoints.
- **Automated background jobs** that use **Hangfire** — monthly debt closing with customer notification emails, and a daily reconciliation job that flags invoices whose paid amount does not match the receipts applied to them.
- **Email notifications** — invoice and receipt confirmations rendered from HTML templates.

### 🛡 Reliability
- **Idempotency middleware** — mutating requests carrying an `Idempotency-Key` header are de-duplicated via Redis, replaying the original response on retry instead of double-applying a write.
- **Rate limiting** — tiered policies partitioned per user (falling back to per-IP for anonymous calls): a strict per-IP token bucket on login (10/min) and refresh (60/min), a sliding-window limiter on dashboard reads (120/min), a tighter token bucket for expensive reads and AI search (40 burst, 120/min), and baseline read/write buckets for everything else. Limits are configurable via the `RateLimiting` section in `appsettings.json`.


### 🤖 AI Layer
- **Chat agent** — answers business questions in natural language (Vietnamese) by calling 9 purpose-built tools: medicine & inventory search, inventory alerts, FEFO allocation, sales search, customer lookup, branch list, supplier search, goods receipt history and analytics reports.
- **Forecast agent** — a supply-chain assistant with 6 tools that recommends what to purchase and how much, fusing sales history with product stock levels, weather, air quality, Google Trends interest and Vietnamese public holidays.
- **Supervisor routing** — a LangGraph supervisor classifies each question and routes it to the general chat agent or the forecast agent, with automatic model selection between Groq and Gemini based on query complexity.
- **Streaming responses** — answers are streamed to the browser over Server-Sent Events, with tool call progress surfaced in the chat UI.
- **Persistent chat history** — conversations and messages are stored in SQL Server with cursor-based pagination, so history survives restarts.
- **Observability & evaluation** — every agent run is traced in Langfuse, and a 37-case golden-set evaluation harness scores tool-call accuracy, hallucinated numbers and latency.

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Frontend | React 18 (Create React App) |
| Frontend State | Redux (persisted) + Zustand |
| Frontend UI | Bootstrap 5 / React-Bootstrap, Recharts, SCSS |
| Backend | ASP.NET Core Web API (.NET 9) |
| ORM | Entity Framework Core 9 |
| Database | Microsoft SQL Server 2022 |
| Authentication | JWT Bearer (access + rotating refresh token) |
| Password Hashing | BCrypt |
| Validation | FluentValidation + custom business-rule validators |
| Caching & Idempotency | Redis 7 (StackExchange) |
| Background Jobs | Hangfire |
| AI Service | Python 3.11 / FastAPI |
| AI Framework | LangChain 1.x / LangGraph 1.x |
| LLMs | Groq (`openai/gpt-oss-20b`), Google Gemini |
| AI Observability | Langfuse 4 |
| Containerization | Docker + Docker Compose |
| Version Control | Git / GitHub Actions |

---

## 🏗️ System Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                     React SPA  ·  :3000                          │
│        Bootstrap 5 · Redux + Zustand · Axios · SSE chat          │
└───────────────┬──────────────────────────────┬───────────────────┘
                │ REST (JWT + X-Branch-Id)     │ SSE stream
                │ Idempotency-Key              │
                ▼                              ▼
┌───────────────────────────────┐   ┌───────────────────────────────┐
│   .NET 9 Web API  ·  :8080    │   │   AI Agent  ·  :8000          │
│  Controllers → Services →     │◄──┤   FastAPI · LangGraph         │
│  Repositories → EF Core       │   │   Supervisor + 2 agents       │
│  FluentValidation · RBAC      │   │   15 tools · Groq / Gemini    │
└──────┬─────────────┬──────────┘   └───────────────┬───────────────┘
       │             │                              │
       ▼             ▼                              ▼
┌─────────────┐  ┌───────────┐               ┌─────────────┐
│ SQL Server  │  │   Redis   │               │  Langfuse   │
│  ·  :1433   │  │  ·  :6379 │               │   ·  :3001  │
│ 43 tables   │  │ cache +   │               │ tracing +   │
│ FEFO stock  │  │ idempotency│              │ evaluation  │
└─────────────┘  └───────────┘               └─────────────┘
        ▲
        │  EF Core migrations applied on startup
┌───────┴─────────────────────────────────────────────────────────┐
│                     Hangfire  ·  SQL Server storage            │
│      monthly debt closing  ·  daily reconciliation              │
└─────────────────────────────────────────────────────────────────┘
```

The AI service never touches the database. It authenticates the user by forwarding their token to the .NET API, which re-validates it and applies the same permission and branch-scope rules as the rest of the system.

---

## 📁 Project Structure

```
.
├── docker-compose.yml            # 12 services: app stack + Langfuse stack
├── .github/workflows/ci.yaml     # CI/CD pipeline
│
├── backend/                      # ASP.NET Core 9 Web API
│   ├── backend.sln
│   └── PharmacyManagement/
│       ├── Program.cs            # DI, pipeline, migrations
│       ├── Controllers/          # 24 controllers, ~120 endpoints
│       ├── Models/               # 46 entities + DbContext + sample_data.sql
│       ├── DTOs/                 # request/response contracts
│       ├── Services/             # business logic, batch selection, notifications
│       ├── Repositories/         # data access
│       ├── Mappers/              # entity → response DTO
│       ├── Validators/           # FluentValidation + permission handlers
│       ├── Middlewares/          # exception handling, idempotency
│       ├── Migrations/           # EF Core migrations
│       ├── Templates/            # HTML email templates
│       └── Dockerfile
│
├── frontend/pharmacymanage/      # React 18 SPA (CRA)
│   ├── src/
│   │   ├── components/           # UI by feature (Manage*, DashBoard, ChatWidget)
│   │   ├── config/               # role → route permission map
│   │   ├── redux/                # user slice, persisted to localStorage
│   │   ├── stores/               # Zustand (chat, filters)
│   │   ├── services/             # API client
│   │   ├── utils/                # Axios instance, refresh queue, idempotency
│   │   ├── styles/               # SCSS
│   │   └── layout.js             # routes
│   └── Dockerfile
│
└── ai-agent/                     # Python AI service
    ├── main.py                   # FastAPI app, lifespan
    ├── app/
    │   ├── api/                  # chat (SSE) + health endpoints
    │   ├── agents/               # supervisor, router, chat & forecast agents, prompts
    │   ├── tools/                # 9 general tools + 6 forecast tools
    │   ├── services/             # LLM factory, backend client, external APIs, Langfuse
    │   ├── models/               # Pydantic models
    │   └── config/               # pydantic-settings
    ├── tests/                    # pytest suite (109 tests)
    ├── eval/                     # golden-set evaluation harness
    ├── .env.example
    └── Dockerfile
```

---

## 🤖 AI Agent

```
                     👤 User
                        │
                        ▼
              ┌─────────────────────┐
              │   React Chat UI     │
              └──────────┬──────────┘
                         │  POST /api/ai/chat  (SSE)
                         ▼
              ┌─────────────────────┐
              │   AI Service        │
              │   FastAPI  ·  :8000 │
              └──────────┬──────────┘
                         │
                         ▼
              ┌─────────────────────┐
              │  Supervisor         │
              │  (LangGraph router) │
              └──────┬───────┬──────┘
                     │       │
        ┌────────────┘       └────────────┐
        ▼                                 ▼
┌───────────────────┐            ┌──────────────────────┐
│   chat_agent      │            │   forecast_agent     │
│                   │            │                      │
│  • Medicine       │            │  • Sales History     │
│  • Inventory      │            │  • Product Stock     │
│  • FEFO Alloc.    │            │  • Weather           │
│  • Invoice        │            │  • Air Quality       │
│  • Customer       │            │  • Search Trends     │
│  • Supplier       │            │  • Holidays          │
│  • Branch         │            └──────────┬───────────┘
│  • Goods Receipt  │                       │
│  • Analytics      │                       │
└─────────┬─────────┘                       │
          └───────────────┬─────────────────┘
                          ▼
              ┌─────────────────────┐
              │  ASP.NET Core API   │
              │  RBAC · branch scope│
              └──────────┬──────────┘
                         ▼
              ┌─────────────────────┐
              │    SQL Server       │
              └─────────────────────┘
```

The `forecast_agent` applies domain heuristics when reasoning over its inputs — cold and rainy weather raises demand for cold, flu and blood-pressure medicines, high PM2.5 raises demand for masks, saline and inhalers, and public holidays are treated as demand spikes — then combines them with a safety-stock ratio to produce a purchase recommendation.

**LLM routing.** Simple lookups are sent to Groq for speed and cost. Analytical questions (revenue trends, comparisons, rankings) are routed to Gemini when an API key is available. Temperature is fixed at `0.0` for all tool reasoning.

---

## 🛠 Prerequisites

### Option A — Docker

- [Docker](https://docs.docker.com/get-docker/) 24.0 or later
- [Docker Compose](https://docs.docker.com/compose/install/) v2 (`docker compose`)

Nothing else — the .NET SDK, Node and Python toolchains all live inside the images.

### Option B — Run services locally

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) 22.x and npm
- [Python](https://www.python.org/downloads/) 3.11
- [Microsoft SQL Server](https://www.microsoft.com/sql-server) 2019 or later
- [Redis](https://redis.io/download/) 7.x

---

## 🚀 Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/namphu2810/Supplychain_pharmacy.git
   cd Supplychain_pharmacy
   ```

2. **Configure secrets**

   Create a `.env` file in the project root with the SQL Server password:

   ```bash
   MSSQL_SA_PASSWORD=<your-strong-password>
   ```

   Then create the AI service configuration:

   ```bash
   cp ai-agent/.env
   ```

   At minimum, add an LLM API key to `ai-agent/.env`:

   ```bash
   LLM_PROVIDER=groq
   GROQ_API_KEY=<your-groq-key>
   ```

   The same file drives Docker Compose, so this single step covers both installation options.

3. **Start the stack**

   ```bash
   docker compose up --build
   ```

   The first run takes a few minutes: SQL Server starts, EF Core migrations are applied automatically, the schema is seeded, and the Langfuse container initialises. Watch for `pharmacy-api` to log its listening address, then open the app.

   To start only the application services and skip the Langfuse observability stack:

   ```bash
   docker compose up --build db redis api ai-agent react
   ```

4. **Verify**

   ```bash
   curl http://localhost:8000/health
   ```

   A healthy AI service reports the status of both itself and its backend dependency.

### Running services locally

<details>
<summary>Backend</summary>

```bash
cd backend
dotnet tool restore
dotnet ef database update --project PharmacyManagement
dotnet run --project PharmacyManagement
```

Listens on `https://localhost:7196` / `http://localhost:5217`. Seed data lives in
`backend/PharmacyManagement/Models/sample_data.sql` — import it into `PharmacySystem`
to get roles, permissions, medicines, customers and suppliers.

</details>

<details>
<summary>Frontend</summary>

```bash
cd frontend/pharmacymanage
npm ci
npm start
```

Listens on `http://localhost:3000`. Set `REACT_APP_API_URL` and `REACT_APP_AI_URL` to
the addresses of your locally running API and AI service.

</details>

<details>
<summary>AI service</summary>

```bash
cd ai-agent
python -m venv .venv && source .venv/bin/activate   # Windows: .venv\Scripts\activate
pip install -r requirements-dev.txt
uvicorn main:app --reload --port 8000
```

Point `BACKEND_URL` at the local API (default `http://api:8080` in Docker,
`http://localhost:5217` locally).

</details>

---

## 💡 Usage

### Service endpoints

| Service | URL | Notes |
|---|---|---|
| Web application | http://localhost:3000 | React SPA |
| Backend API | http://localhost:5000 | Maps to container port `8080` |
| AI service | http://localhost:8000 | `/health`, `/api/ai/chat` |
| Langfuse UI | http://localhost:3001 | Agent traces (seeded credentials required) |
| SQL Server | `localhost:1433` | Database `PharmacySystem` |
| Redis | `localhost:6379` | Cache + idempotency |
| Hangfire dashboard | http://localhost:5000/hangfire | Background job management |

### Signing in

Import `sample_data.sql` to populate roles and users, then sign in at
http://localhost:3000. Each of the 5 roles sees a different dashboard and navigation
menu, and switching the active branch in the header re-scopes every subsequent query
to that branch.

### Asking the AI

Open the chat widget and ask in plain Vietnamese, for example:

- *"Doanh thu tuần này của chi nhánh Hà Nội là bao nhiêu?"* — revenue trend
- *"Thuốc nào sắp hết hạn trong kho?"* — expiring batches
- *"Còn bao nhiêu Paracetamol trong tồn kho?"* — batch-level stock
- *"Nên nhập bao nhiêu thuốc cảm cúm cho tuần tới?"* — routes to the forecast agent

The widget streams the answer as it is produced and shows which tools were called.

### API conventions

Responses use a single envelope, and the Axios layer on the frontend unwraps it before
the data reaches components:

```json
{ "ec": 0, "em": "Success", "dt": { } }
```

`ec: 0` indicates success. `ec: -999` signals an expired access token, which triggers
an automatic refresh-and-retry in the frontend. List endpoints take `page` and `count`
and return `items`, `numRecords` and `totalPage`.

Supply an `Idempotency-Key` header on any `POST`, `PUT`, `PATCH` or `DELETE` request
to make it safe to retry. Supply `X-Branch-Id` to scope the request to one branch.

### Tests and evaluation

```bash
# AI service
cd ai-agent
python -m pytest -q                          # 109 tests, fully mocked
RUN_LIVE_TESTS=1 python -m pytest -m live    # hits real external APIs

# Evaluation harness
python -m eval.run_eval --mode mock --provider auto
python -m eval.compare_models --providers groq,gemini,auto --mode mock --judge

# Frontend
cd frontend/pharmacymanage
npm test -- --watchAll=false

# Backend
cd backend
dotnet test backend.sln
```

---

## 📄 License

Released under the [MIT License](https://opensource.org/licenses/MIT).
