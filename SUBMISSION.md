# Ajaia Docs reviewer submission

## Submission status

This is the evidence-led submission package for Ajaia Docs. It separates observed product evidence from the one remaining manual sharing action.

| Item | Status |
| --- | --- |
| Source and approved design/implementation plan | Present in repository |
| Backend release gate | 75 unit + 60 integration = 135 tests |
| Reviewer documentation | Present: README, architecture, AI workflow, and walkthrough script |
| Frontend release gate | 22 files / 47 tests passed; typecheck, lint, and production build passed |
| Docker runtime | Docker image rebuilt; Compose PostgreSQL and API services running; `/health` returned `{"status":"healthy"}` |
| Clean Chrome product journey | Installed-Google-Chrome `ajaia-docs.spec.ts` passed 2/2: create/format/share/collaborator and Markdown import; no browser errors |
| Desktop/mobile Chrome screenshots | Chrome visual rerun passed 1/1 in installed Google Chrome; all four screenshots inspected and committed in `6527635` |
| Cross-user cache/logout blockers | Reviewer cache fixed in `07ad85f`; logout sequencing fixed in `a9a2d57`; AppShell 4-test suite and typecheck passed |
| Render service | Live at [https://ajaia-docs-z2ua.onrender.com](https://ajaia-docs-z2ua.onrender.com) from merged `main` commit `28f989dc2bf724b9418e3e7beda102774c6844fd`; health, root, antiforgery, login, and authenticated-session checks returned 200 |
| Live Chrome login | Installed Google Chrome reached `/documents` as Amina, fully loaded **Work in progress**, reported zero browser errors, and was visually inspected |
| Human walkthrough recording | Complete: [Loom walkthrough](https://www.loom.com/share/e1c4f6a6b75e489da9b89e825a09267f) |
| Archive and Google Drive upload | Complete: 11 items are in the [submission folder](https://drive.google.com/drive/folders/1iEw1uCn9KWcOyvykQbl_SdSzuhVxEPsd). The folder was created private by default; public link sharing must be enabled manually |

## Source and included artifacts

The repository currently includes:

- .NET solution source, PostgreSQL migrations, backend unit/integration tests, and API routes
- React/Vite frontend source, frontend tests, and Chrome E2E tests
- Dockerfile, Docker Compose configuration, Render Blueprint, and CI workflow
- `README.md`, `ARCHITECTURE.md`, `AI_WORKFLOW.md`, this document, and `WALKTHROUGH_SCRIPT.md`
- Chrome E2E/visual test sources and inspected responsive evidence: `docs/screenshots/desktop-library.png`, `desktop-editor.png`, `mobile-library.png`, and `mobile-editor.png`

`ajaia-docs-submission.zip` is the final inspected source archive. It includes source, tests, Docker/Render files, the four screenshots, and the completed Loom documentation. It has been uploaded with the other submission artifacts to the 11-item Google Drive folder linked above.

## Live application

Public URL: [https://ajaia-docs-z2ua.onrender.com](https://ajaia-docs-z2ua.onrender.com)

The Render service is running merged `main` commit `28f989dc2bf724b9418e3e7beda102774c6844fd`. Observed live checks include `/health` returning `{"status":"healthy"}` and `/` returning HTTP `200` with `text/html`. `GET /api/session/antiforgery` returned 200 with a `Secure`, `SameSite=Strict`, `HttpOnly` cookie; `POST /api/session` returned 200; and `GET /api/session` returned 200 as Amina. Installed Google Chrome then reached `/documents`, fully loaded **Work in progress**, produced zero browser errors, and was visually inspected.

Render disclosure: a free web service may cold-start after 15 idle minutes. The PostgreSQL 16 free database is currently available through **2026-09-15**. A web-service restart can invalidate a demo login cookie without a persistent ASP.NET Core Data Protection key ring; select the same seeded identity again. PostgreSQL, not the service filesystem, stores documents.

## Evaluator-ready submission note

Ajaia Docs is live at [https://ajaia-docs-z2ua.onrender.com](https://ajaia-docs-z2ua.onrender.com) on merged `main` commit `28f989dc2bf724b9418e3e7beda102774c6844fd`. Start with **Amina Okafor**, create or import a document, format and save it, share it with **Chidi Okeke**, switch identity, and confirm that Chidi can edit content but cannot rename, share, revoke, or delete.

The evidence package records 135 backend tests (75 unit, 60 integration), 22 frontend files / 47 tests plus typecheck, lint, and production build, a local Docker health check, a clean installed-Google-Chrome product journey passing 2/2 without browser errors, and a visual pass of 1/1 with four inspected desktop/mobile screenshots. Live probes returned 200 for health, root, antiforgery, login, and Amina's authenticated session; the cookie was observed as `Secure`, `SameSite=Strict`, and `HttpOnly`. An installed-Google-Chrome live login reached `/documents`, fully loaded **Work in progress**, produced zero browser errors, and was visually inspected.

The walkthrough is recorded at [Loom](https://www.loom.com/share/e1c4f6a6b75e489da9b89e825a09267f), and the [Google Drive submission folder](https://drive.google.com/drive/folders/1iEw1uCn9KWcOyvykQbl_SdSzuhVxEPsd) contains 11 uploaded items. Google Drive created that folder private by default. Public link sharing still must be enabled manually because the Chrome-control extension needed to change the sharing setting is unavailable in this environment; the upload itself is complete.

## Reviewer identities

| Name | Email | Suggested role in demo |
| --- | --- | --- |
| Amina Okafor | `amina@example.test` | Owner: create/import, rename, share |
| Chidi Okeke | `chidi@example.test` | Collaborator: find shared document and edit content |
| Tayo Bello | `tayo@example.test` | Optional second collaborator/recipient |

These are seeded demo identities, not production accounts. No password is required.

## Working behavior supported by the backend baseline

- Seeded-session authentication, HttpOnly cookie configuration, and antiforgery-protected state changes
- PostgreSQL migrations and stable seeded users
- Document create/list/open, owner rename/delete, content update, and expected-version conflict response
- Strict UTF-8 `.txt` and `.md` import up to 1 MiB, with server-side extension/size/decoding enforcement
- Owner grant/revoke sharing and owner-only enforcement
- Collaborator document access and content editing, while owner-only actions return a protected `owner_required` failure
- Concealed inaccessible documents (`404`) and sanitized structured API errors

The integrated frontend gate verified 22 files / 47 tests, typecheck, lint, and production build. The Docker image rebuilt, local Compose PostgreSQL/API services are running, and `/health` returned `{"status":"healthy"}`. The clean-state installed-Google-Chrome journey passed 2/2 without browser errors; the Chrome visual rerun passed 1/1 and all four 1440×1000/390×844 screenshots were inspected. A cross-user cache blocker was fixed in `07ad85f`; the logout sequencing regression was fixed in `a9a2d57`, after which the AppShell 4-test suite and typecheck passed.

## Verified tests

The observed backend release gate is **75 unit tests + 60 integration tests = 135**.

The observed frontend evidence is **22 files / 47 tests**, plus passed typecheck, lint, and production build. The observed Chrome product evidence is **2/2 passed** in installed Google Chrome with no browser errors; the visual evidence is **1/1 passed**, with four inspected screenshots committed in `6527635`.

## Remaining human-owned handoff

1. In Google Drive, enable public link sharing for the already-uploaded 11-item folder and verify the link from a signed-out browser. Interactive Chrome control could not perform this setting because its required extension is unavailable.

The implementation merged at `28f989dc2bf724b9418e3e7beda102774c6844fd`; the tracking issue was kept open through that merge as required by the delivery workflow.

## Final delivery action

1. Make the uploaded Google Drive folder public by link and confirm anonymous access. No archive rebuild, upload, or deployed-login run remains pending.
