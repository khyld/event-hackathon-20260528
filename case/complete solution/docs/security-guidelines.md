# Security guidelines

- Never commit secrets — use `.env` locally.
- Validate all inputs.
- Avoid logging sensitive payloads.
- Keep dependencies updated.

This repo includes a CI build for the frontend and backend. Secret scanning / CodeQL are typically enabled at org/repo settings.
