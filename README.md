# Ajaia Docs

Ajaia Docs is a reviewer-focused collaborative document editor. It demonstrates one complete, deliberately bounded journey: select a seeded reviewer identity, create or import a document, edit it, save it, share it, and let a collaborator edit the shared document.

It is not a Google Docs clone. The product chooses durable PostgreSQL persistence and optimistic concurrency over live cursors, CRDTs, or a misleading claim of real-time merging.

## Current verification status

The final backend release gate verified **75 unit tests** and **60 integration tests**: **135 backend tests**. The full frontend suite verified **22 files / 47 tests**, and typecheck, lint, and production build passed. The Docker image rebuilt successfully; Docker Compose has PostgreSQL and the API running, and `/health` returned `{"status":"healthy"}`.

The clean-state installed-Google-Chrome journey passed **2/2** (create/format/share/collaborator flow and Markdown import) with no browser errors. The visual spec also passed **1/1**; all four committed screenshots were inspected: desktop library/editor at 1440×1000 and mobile library/editor at 390×844.

The live service is [https://ajaia-docs-z2ua.onrender.com](https://ajaia-docs-z2ua.onrender.com), deployed from merged `main` commit `28f989dc2bf724b9418e3e7beda102774c6844fd`. Its `/health` endpoint returned exact JSON `{"status":"healthy"}` and the root route returned HTTP `200` with `text/html`. A live authentication probe observed `GET /api/session/antiforgery` returning 200 with a `Secure`, `SameSite=Strict`, `HttpOnly` cookie, `POST /api/session` returning 200, and `GET /api/session` returning 200 as Amina. An installed-Google-Chrome live login reached `/documents`, fully loaded **Work in progress**, produced zero browser errors, and was visually inspected.

The recorded walkthrough is available on [Loom](https://www.loom.com/share/e1c4f6a6b75e489da9b89e825a09267f). The submission package upload is complete: the [Google Drive folder](https://drive.google.com/drive/folders/1iEw1uCn9KWcOyvykQbl_SdSzuhVxEPsd) contains 11 items. Google Drive created the folder private by default; enabling public link sharing remains a manual step because interactive Chrome control is unavailable in this environment.

A cross-user cache release blocker—stale reviewer data after switching identities—was found and fixed in `07ad85f`. A logout-sequencing regression was fixed in `a9a2d57`; the AppShell suite passed **4 tests**, the full frontend suite passed, and typecheck passed.

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

The backend/frontend gates, Docker rebuild and health check, local Chrome journey/visual evidence, live Render health/root/authentication checks, installed-Chrome live login and visual inspection, recorded walkthrough, and 11-item Drive upload above have been observed. The remaining delivery action is to enable public link sharing for the currently private Drive folder manually.

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

Live application URL: [https://ajaia-docs-z2ua.onrender.com](https://ajaia-docs-z2ua.onrender.com)

The Render service is live from merged `main` commit `28f989dc2bf724b9418e3e7beda102774c6844fd`; `/health` returned `{"status":"healthy"}`, `/` returned HTTP `200` with `text/html`, and the live login/session sequence succeeded as Amina. The approved Render Blueprint uses one Docker web service and PostgreSQL. The free web service may cold-start after 15 minutes of inactivity; the PostgreSQL 16 free database is currently available through **2026-09-15**. Document data belongs in PostgreSQL, not the web service filesystem.

The session cookie is HttpOnly, `SameSite=Strict`, secure in production, and has an eight-hour sliding expiry. A restart can invalidate demo login cookies because the free instance has no persistent ASP.NET Core Data Protection key ring; simply select the same seeded identity again. Persisted document data remains in PostgreSQL.

Critical/high JavaScript advisories were patched. `pnpm audit --prod` still reports three moderate React Router v6 advisories; this app uses hard-coded internal navigation paths and does not route untrusted user-supplied URLs. The residual advisories are disclosed rather than represented as resolved.

## More detail

- [Architecture](ARCHITECTURE.md)
- [AI workflow and verification record](AI_WORKFLOW.md)
- [Reviewer submission checklist](SUBMISSION.md)
- [3–5 minute walkthrough script](WALKTHROUGH_SCRIPT.md)
- [Recorded walkthrough (Loom)](https://www.loom.com/share/e1c4f6a6b75e489da9b89e825a09267f)

Design and implementation were tracked in [GitHub issue #1](https://github.com/vicosoft4real/ajaia-docs/issues/1) through the merge.
