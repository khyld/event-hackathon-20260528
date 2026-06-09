# Agentic Incident Service — Golden Solution (Hackathon Reference)

This repository is a **golden/reference implementation** for the hackathon case:
- .NET Aspire AppHost (Aspire 13.x)
- ASP.NET Core Web API backend
- React + Vite frontend
- Unit + Integration tests
- CI builds both .NET and the frontend

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 18+
- Aspire CLI (recommended)

### Run the AppHost
From the repo root:

```bash
cd case/agentic-incident-service
aspire run --project src/AppHost
```

If you prefer not to use Aspire CLI, you can also run:

```bash
cd case/agentic-incident-service
dotnet run --project src/AppHost
```

### Run tests
From the repo root:

```bash
cd case/complete solution
./scripts/run-tests.sh
```

You can also run the tests directly with:

```bash
dotnet test AgenticIncidentService.sln
```

## Run locally

### Prereqs
- .NET 10 SDK
- Node.js (18+)
- Aspire CLI installed (recommended)

### Start everything with Aspire
From repo root:

```bash
aspire run --project src/AppHost
```

Alternatively, run the API and frontend separately:

```bash
# API
 dotnet run --project src/ApiService

# Frontend
 cd src/WebApp
 npm install
 npm run dev
```

## API endpoints
- `GET /health`
- `GET /api/incidents`
- `GET /api/incidents/{id}`
- `POST /api/incidents`
- `GET /api/incidents/{id}/recommendation`

## Hackathon alignment
- Repo instructions: `.github/copilot-instructions.md`
- Agent roles: `.github/agents/*`
- Skills: `.github/skills/*`
- Specs: `/specs/*`

> Tip: Use PRs for every change and request Copilot code review.
