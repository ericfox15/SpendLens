# Budget Helper

A full-stack personal budgeting app built to mirror the architecture of a production eMAR (electronic medication administration record) system. Every layer, pattern, and naming convention is a direct parallel to the real production codebase — learning this app means learning the patterns you work with daily.

## Architecture

```
Budget Helper
├── Budget.Api                  ASP.NET Core 8 Web API
│   ├── Controllers             HTTP entry points (Account, Bill, Category, Pass, Auth, Report, Import)
│   ├── Attributes              Global filters (Permission, Audit)
│   └── Jobs                    Background jobs (FluentScheduler, BackgroundService)
│
├── Budget.Pass                 Pass generate + submit vertical slice
│   ├── Generate                Bill → Payment expansion (schedule math)
│   └── Submit                  Batch payment submission + offline sync
│
├── Budget.Shared               CRUD vertical slices (Account, Bill, Category, User, Setting)
│   └── */Repository, Service   Interface + implementation pairs
│
├── Budget.Application.Contracts  Enums, Options (no logic, no DB deps)
├── Budget.Database             DbUp migration runner (SQL scripts)
├── Budget.Test.Unit            xUnit + Moq + FluentAssertions (13 tests)
└── Budget.Frontend             Angular 22, NgModule, NGXS, Dexie
```

## Key patterns this app teaches

| Pattern | Where |
|---------|-------|
| Controller → Service → Repository | Every vertical slice |
| F*/S*/E* naming convention | All models and enums |
| Dapper inline SQL + stored procs | All repositories |
| DbUp versioned migrations | Budget.Database |
| JWT + bit-flag permissions | ControllerAuthentication, PermissionGlobalAttribute |
| Global action filters | AuditRequestGlobalAttribute, PermissionGlobalAttribute |
| IOptions<T> with fail-fast validation | RepositoryOptions, JwtOptions |
| DI via extension methods | AddSharedRepository(), AddPassService(), etc. |
| NGXS state + temp negative IDs | pass.state.ts, pass.actions.ts |
| **Configurable action-route pipeline** | action-route.model.ts, payment.service.ts, RepositoryConfiguration |
| Offline-first with Dexie + IsOfflineSync | budget.database.ts, submit-queue.service.ts |
| BackgroundService + IServiceScopeFactory | PaymentDraftCleanupJob |
| FluentScheduler | JobScheduler |

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Docker (for SQL Server) or a local SQL Server instance

### 1. Start SQL Server
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" \
  -p 1433:1433 --name budget-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Run Database Migrations
```bash
cd Budget.Database
dotnet run
```

### 3. Start the API
```bash
cd Budget.Api
dotnet run
```

### 4. Start the Frontend (dev)
```bash
cd Budget.Frontend
npm install
ng serve
```

Navigate to `http://localhost:4200`. Login with the seeded admin credentials (see `0001_BUDGET_Init.sql`).

## Understanding the codebase

Every source file has a companion `.md` explanation file in the same directory:

```
FAccount.cs          → FAccount.cs.md
pass.state.ts        → pass.state.ts.md
ServicePassGenerate  → ServicePassGenerate.cs.md
```

Read the `.md` file first to understand what a file does and why it's designed the way it is, then read the source code.

## Running Tests

```bash
cd Budget.Test.Unit
dotnet test
# Expected: 13 passed
```
