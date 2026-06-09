# Repository instructions for GitHub Copilot

## Project shape
- The active reference app lives under `case/agentic-incident-service/`.
- The `src/AppHost` project starts the local stack with .NET Aspire.
- The backend API lives in `src/ApiService` and should stay lightweight, clear, and easy to test.
- The frontend lives in `src/WebApp` and uses React + Vite.
- The `case/complete solution/` folder is a reference/backup copy; do not treat it as the main working implementation unless explicitly asked.

## Development guidance
- Prefer small, readable changes over large rewrites.
- Keep API contracts aligned with the spec in `case/agentic-incident-service/specs/incident-triage/spec.md`.
- Use synthetic incident data only; do not add real customer or sensitive data.
- Preserve the existing Aspire startup flow for local development.

## Local run expectations
- For the main case app, use Aspire when possible:
  - `cd case/agentic-incident-service`
  - `aspire run --project src/AppHost`
- If you change backend behavior, validate the API endpoints and the frontend contract that calls them.

## Testing and validation
- Add or update tests when behavior changes.
- Prefer deterministic, simple tests for API and triage logic.
- If you touch the reference solution under `case/complete solution/`, use its test script when available:
  - `cd case/complete solution`
  - `./scripts/run-tests.sh`

## Security and quality basics
- Never hardcode secrets, tokens, keys, passwords, or internal endpoints.
- Validate inputs and handle errors cleanly.
- Keep comments and docs accurate when behavior changes.

## Definition of done
- The change matches the relevant spec and existing architecture.
- The code builds and the relevant tests pass.
- Any user-facing behavior is reflected in the documentation or README when needed.
- The change is ready for a PR review.
