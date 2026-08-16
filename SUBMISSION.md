# Ajaia Docs reviewer submission

## Submission status

This is the evidence-led submission package for Ajaia Docs. It deliberately separates verified backend evidence from parallel work that must be observed before final submission.

| Item | Status |
| --- | --- |
| Source and approved design/implementation plan | Present in repository |
| Backend release gate | 75 unit + 59 integration = 134 tests |
| Reviewer documentation | Present: README, architecture, AI workflow, and walkthrough script |
| Frontend release gate | 22 files / 47 tests passed; typecheck, lint, and production build passed |
| Docker runtime | Docker image rebuilt; Compose PostgreSQL and API services running; `/health` returned `{"status":"healthy"}` |
| Clean Chrome product journey | Installed-Google-Chrome `ajaia-docs.spec.ts` passed 2/2: create/format/share/collaborator and Markdown import; no browser errors |
| Desktop/mobile Chrome screenshots | Chrome visual rerun passed 1/1 in installed Google Chrome; all four screenshots inspected and committed in `6527635` |
| Cross-user cache/logout blockers | Reviewer cache fixed in `07ad85f`; logout sequencing fixed in `a9a2d57`; AppShell 4-test suite and typecheck passed |
| Render service | Live at [https://ajaia-docs-z2ua.onrender.com](https://ajaia-docs-z2ua.onrender.com) from merged `main` commit `6639ae0328b5d530334b528191108a807a5edef4`; `/health` returned `{"status":"healthy"}`, `/` returned 200 `text/html`, and production startup logs are live |
| Deployed Chrome journey | Pending: the browser-extension capability required to run it against Render is unavailable; no deployed-browser success is claimed |
| Human walkthrough recording | Complete: [Loom walkthrough](https://www.loom.com/share/e1c4f6a6b75e489da9b89e825a09267f) |
| Archive and Google Drive upload | Provisional archive inspected; rebuild required after final Loom URL commit. Candidate-owned Drive upload remains pending |

## Source and included artifacts

The repository currently includes:

- .NET solution source, PostgreSQL migrations, backend unit/integration tests, and API routes
- React/Vite frontend source, frontend tests, and Chrome E2E tests
- Dockerfile, Docker Compose configuration, Render Blueprint, and CI workflow
- `README.md`, `ARCHITECTURE.md`, `AI_WORKFLOW.md`, this document, and `WALKTHROUGH_SCRIPT.md`
- Chrome E2E/visual test sources and inspected responsive evidence: `docs/screenshots/desktop-library.png`, `desktop-editor.png`, `mobile-library.png`, and `mobile-editor.png`

`ajaia-docs-submission.zip` is a provisional inspected archive built from `a5980bf`. It includes source, tests, Docker/Render files, the four screenshots, and the pre-Loom documentation. Rebuild it after this video-link documentation commit, then inspect the rebuilt archive before the candidate uploads it to Drive.

## Live application

Public URL: [https://ajaia-docs-z2ua.onrender.com](https://ajaia-docs-z2ua.onrender.com)

Observed live checks: `/health` returned `{"status":"healthy"}`; `/` returned HTTP `200` with `text/html`; production startup logs are live. The Render service is running merged `main` commit `6639ae0328b5d530334b528191108a807a5edef4`.

Render disclosure: a free web service may cold-start after 15 idle minutes. The PostgreSQL 16 free database is currently available through **2026-09-15**. A web-service restart can invalidate a demo login cookie without a persistent ASP.NET Core Data Protection key ring; select the same seeded identity again. PostgreSQL, not the service filesystem, stores documents.

## Evaluator-ready submission note

Ajaia Docs is live at [https://ajaia-docs-z2ua.onrender.com](https://ajaia-docs-z2ua.onrender.com). Start with **Amina Okafor**, create or import a document, format and save it, share it with **Chidi Okeke**, switch identity, and confirm that Chidi can edit content but cannot rename, share, revoke, or delete. The service is live on merged `main` commit `6639ae0328b5d530334b528191108a807a5edef4`; its root route returned `200 text/html` and `/health` returned `{"status":"healthy"}`.

The evidence package records 134 backend tests (75 unit, 59 integration), 22 frontend files / 47 tests plus typecheck, lint, and production build, a local Docker health check, a clean installed-Google-Chrome journey passing 2/2 without browser errors, and a visual pass of 1/1 with four inspected desktop/mobile screenshots. Critical/high JavaScript advisories were patched. The production dependency audit still reports three moderate React Router v6 advisories; the app uses hard-coded internal navigation and does not route untrusted user-supplied URLs, but the residual advisories remain disclosed.

The locally observed Chrome evidence must not be mistaken for a Render-browser run: the deployed Chrome journey is still pending because the needed browser-extension capability is unavailable. The walkthrough is recorded at [Loom](https://www.loom.com/share/e1c4f6a6b75e489da9b89e825a09267f). The remaining candidate-owned handoffs are rebuilding/inspecting the final archive and uploading the archive, screenshots, and video-link file to Google Drive.

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

The observed backend release gate is **75 unit tests + 59 integration tests = 134**.

The observed frontend evidence is **22 files / 47 tests**, plus passed typecheck, lint, and production build. The observed Chrome product evidence is **2/2 passed** in installed Google Chrome with no browser errors; the visual evidence is **1/1 passed**, with four inspected screenshots committed in `6527635`.

## Remaining human-owned handoffs

1. Rebuild and inspect the final archive after this Loom URL commit.
2. Upload the final archive, screenshots, and video-link file to one Google Drive folder.

The tracking issue stays open until the implementation PR merges. It must not be closed manually before merge.

## Next 2–4 hours

1. Run the Chrome journey against the Render URL when the required browser-extension capability becomes available; record only its observed result.
2. Rebuild/inspect the final archive after this Loom URL commit and complete the candidate-owned Google Drive upload with the screenshots and video-link file.
