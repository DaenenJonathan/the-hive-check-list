# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**TheHiveCheckLists** — a real-time checklist management system for brand managers and warehouse preparers. Brand managers import checklists from Excel (.xlsx), preparers update item statuses, and a full audit trail tracks all changes. Multilingual: French, Dutch, English.

## Tech Stack

- **Backend**: ASP.NET Core (.NET 8+), Clean Architecture, Entity Framework Core, MediatR (CQRS), FluentValidation, SignalR (real-time), JWT auth
- **Frontend**: Angular (latest), ngx-translate (i18n), Angular Material or PrimeNG
- **Database**: PostgreSQL (preferred) or SQL Server
- **Excel parsing**: ClosedXML or EPPlus (isolated in Infrastructure)
- **Testing**: xUnit, FluentAssertions, NSubstitute, Testcontainers

## Build & Run Commands

### Backend
```bash
# From repo root
dotnet restore
dotnet build

# Run the API
dotnet run --project src/WebApi

# Run all tests
dotnet test

# Run a single test project
dotnet test src/Application.Tests/Application.Tests.csproj

# Run a single test by name
dotnet test --filter "FullyQualifiedName~CreateChecklistCommandHandlerTests"

# Add EF migration
dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/WebApi

# Apply migrations
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

### Frontend
```bash
# From /frontend/
npm install
ng serve            # dev server at http://localhost:4200
ng build --prod     # production build
ng test             # unit tests
ng lint
```

## Backend Architecture

### Layer Dependency Rule
```
WebApi → Application → Domain
Infrastructure → Application → Domain
Domain depends on nothing.
```

### Domain (`src/Domain/`)
Pure business logic only — no EF, no ASP.NET, no DTOs.

Key entities: `Action`, `Checklist`, `ChecklistItem`, `AuditLog`, `User`

`ChecklistItem` statuses: `ToPrepare`, `Prepared`, `Missing`, `PartiallyPrepared`, `Loaded`, `Cancelled`, `Replaced`

Roles: `Admin`, `Manager`, `WarehouseUser`, `Viewer`

### Application (`src/Application/`)
CQRS via MediatR. Every feature folder contains Commands/, Queries/, DTOs/.

Key interfaces defined here (implemented in Infrastructure):
- `IApplicationDbContext` — EF access
- `IExcelChecklistParser` — Excel parsing
- `ICurrentUserService` — authenticated user context
- `IAuditService` — write audit logs

Handlers must be short; validation via FluentValidation validators (`CreateChecklistCommandValidator`, etc.).

Use `Result<T>` / `Result` return types — never throw exceptions for normal business cases (not found, validation failure, access denied).

### Infrastructure (`src/Infrastructure/`)
Implements Application interfaces. Contains EF Core `ApplicationDbContext`, configurations, migrations, `ExcelChecklistParser`, JWT/Identity services.

### WebApi (`src/WebApi/`)
Thin controllers — call MediatR, return results. Global exception middleware maps to consistent error responses:
```json
{ "status": 400, "message": "Validation error", "errors": [] }
```
SignalR hubs for real-time checklist updates. Swagger enabled.

## Excel Import Flow

```
Upload .xlsx → Parse (IExcelChecklistParser) → Validate → Preview DTO
→ User confirms → ImportChecklistFromExcelCommand → Create/Update Checklist → AuditLog
```

Parsing is entirely in Infrastructure. Controllers only receive the file and forward to the command.

## Frontend Architecture (`frontend/src/app/`)

```
core/        — AuthService, CurrentUserService, AuthInterceptor, ApiErrorInterceptor, RoleGuard (import once only)
shared/      — Reusable components (status badge, confirm button, table, loader), pipes, utils
features/    — One folder per domain: actions/, checklists/, checklist-items/, imports/, users/
layout/      — Shell, nav, sidebar
```

Each feature folder: `pages/`, `components/`, `services/`, `models/`, `routes.ts`

API calls stay inside the feature's service. Components only display data and delegate to services.

## Naming Conventions

| Backend | Frontend |
|---|---|
| `CreateChecklistCommand` | `checklist-list.component.ts` |
| `CreateChecklistCommandHandler` | `checklist-detail.component.ts` |
| `CreateChecklistCommandValidator` | `checklist-import.component.ts` |
| `GetChecklistByIdQuery` | `checklist.service.ts` |
| `ChecklistDto`, `ChecklistItemDto` | `checklist.model.ts` |

## Hard Rules

- No business logic in controllers or Angular components
- Domain never depends on Infrastructure
- Never expose Domain entities directly in API responses — always use DTOs
- Security enforced server-side; frontend guards are UX only
- All significant mutations must write an `AuditLog` entry (who, what entity, old value, new value, when)
- No `any` in TypeScript

## Routes

```
/actions          /actions/:id
/checklists/:id
/imports
/users
```
