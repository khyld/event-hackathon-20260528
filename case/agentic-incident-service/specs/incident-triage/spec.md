# Feature: Incident listing + triage

## Goal
Users can browse incidents and see a basic recommendation.

## Acceptance criteria
- `GET /api/incidents` returns incidents
- `GET /api/incidents/{id}` returns one incident
- `POST /api/incidents` adds an incident
- `GET /api/incidents/{id}/recommendation` returns a deterministic recommendation

## Notes
- Use synthetic data only.
