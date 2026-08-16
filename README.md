# Ajaia Docs

Ajaia Docs is a reviewer-focused collaborative document editor. It demonstrates one complete, deliberately bounded journey: select a seeded reviewer identity, create or import a document, edit it, save it, share it, and let a collaborator edit the shared document.

It is not a Google Docs clone. The product chooses durable PostgreSQL persistence and optimistic concurrency over live cursors, CRDTs, or a misleading claim of real-time merging.

## Current verification status

The backend release gate at commit `95d85d0` verified **66 unit tests** and **59 integration tests**: **125 backend tests**. The integrated frontend gate verified **47 tests**, typecheck, lint, and production build. The Docker image built and Docker Compose has PostgreSQL and the API running.

The Google Chrome visual rerun passed **1/1** in the installed Google Chrome channel. All four committed screenshots were inspected: desktop library/editor at 1440×1000 and mobile library/editor at 390×844. The Render deployment has not been completed, so no public URL is claimed.

A cross-user cache release blocker—stale reviewer data after switching identities—was found and fixed in `07ad85f`; the focused AppShell gate then passed **4 tests** and typecheck.

## Quick start (Docker)

Prerequisite: Docker Desktop (or another Docker Compose v2-compatible runtime).

```bash
docker compose up --build
```

Open `http://127.0.0.1:8080`, choose a demo identity, and use the document library. Stop the stack with `docker compose down`.

## Developer setup

Prerequisites for the non-container workflow:

- .NET SDK 10.0.0 (the repository pins it in `global.json`)
- Node.js 22
- pnpm 10.33.4
- PostgreSQL 16, or the PostgreSQL service from Docker Compose

```bash
dotnet restore AjaiaDocs.sln
dotnet build AjaiaDocs.sln --configuration Release --no-restore
dotnet test AjaiaDocs.sln --configuration Release --no-build

pnpm install --frozen-lockfile
pnpm --dir web test
pnpm --dir web typecheck
pnpm --dir web lint
pnpm --dir web build
```

For browser verification, start the full stack and run the Chrome project:

```bash
docker compose up -d --build
curl --fail http://127.0.0.1:8080/health
pnpm --dir web test:e2e --project=chrome
```

The backend and frontend checks above have been observed, the Docker image/runtime has been started, and the local Chrome visual evidence passed. The remaining release evidence is the public deployment and its browser run.

## Demo identities

The app seeds these assessment-only identities; no registration, passwords, or external email setup are required:

| Identity | Email |
| --- | --- |
| Amina Okafor | `amina@example.test` |
| Chidi Okeke | `chidi@example.test` |
| Tayo Bello | `tayo@example.test` |

Select an identity from **Demo access for reviewers**. The server only issues a session after confirming the selected seeded user.

## What the product supports

- Owner and shared document library views
- Blank document creation and owner-only rename/delete/share/revoke actions
- UTF-8 `.txt` and `.md` imports up to **1 MiB (1,048,576 bytes)**
- Rich-text editing, formatting, autosave, and refresh persistence (covered by the 47-test frontend gate and inspected Chrome visual evidence)
- Collaborator content edits; collaborators cannot rename, share, revoke, or delete
- PostgreSQL persistence and version-based conflict detection

Imports are checked server-side by filename extension, byte size, and strict UTF-8 decoding. MIME type is advisory. Normal editor payloads are capped at **2 MiB**; document titles are trimmed and limited to 120 characters.

## Deployment and reviewer caveats

Live application URL: not deployed yet.

The approved Render Blueprint uses one Docker web service and PostgreSQL. On Render’s free tier, the web service may cold-start after 15 minutes of inactivity and the database may expire after 30 days. Document data belongs in PostgreSQL, not the web service filesystem.

The session cookie is HttpOnly, `SameSite=Strict`, secure in production, and has an eight-hour sliding expiry. A service restart on a free instance can invalidate demo login cookies because there is no persistent key ring; simply select the same seeded identity again. Persisted document data remains in PostgreSQL.

## More detail

- [Architecture](ARCHITECTURE.md)
- [AI workflow and verification record](AI_WORKFLOW.md)
- [Reviewer submission checklist](SUBMISSION.md)
- [3–5 minute walkthrough script](WALKTHROUGH_SCRIPT.md)

The tracking issue remains open until the implementation PR merges: <https://github.com/vicosoft4real/ajaia-docs/issues/1>.
