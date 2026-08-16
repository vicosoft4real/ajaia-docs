# Ajaia Docs reviewer submission

## Submission status

This is the evidence-led submission package for Ajaia Docs. It deliberately separates verified backend evidence from parallel work that must be observed before final submission.

| Item | Status |
| --- | --- |
| Source and approved design/implementation plan | Present in repository |
| Backend baseline | Verified at `95d85d0`: 66 unit + 59 integration = 125 tests |
| Reviewer documentation | Present: README, architecture, AI workflow, and walkthrough script |
| Frontend checks and final combined test totals | `TODO_FINALIZE: insert observed commands, results, and final counts` |
| Desktop/mobile Chrome screenshots | `TODO_FINALIZE: add observed committed paths after visual validation` |
| Render public URL and deployed E2E evidence | `TODO_FINALIZE: AJAIA_DEPLOY_URL and observed Chrome result` |
| Human walkthrough recording URL | Human-owned; do not create `WALKTHROUGH_VIDEO_URL.txt` until the candidate supplies the final unlisted URL |
| Archive and Google Drive upload | Human-owned after final merge; `TODO_FINALIZE: archive checksum/location and Drive confirmation` |

## Source and included artifacts

The final repository is intended to include:

- .NET solution source, PostgreSQL migrations, backend unit/integration tests, and API routes
- React/Vite frontend source, frontend tests, and Chrome E2E tests
- Dockerfile, Docker Compose configuration, Render Blueprint, and CI workflow
- `README.md`, `ARCHITECTURE.md`, `AI_WORKFLOW.md`, this document, and `WALKTHROUGH_SCRIPT.md`
- `docs/screenshots/` desktop and mobile evidence after Chrome capture

`TODO_FINALIZE: verify these files against the final merged commit and the submission archive; do not treat this intended inventory as a completed archive verification.`

## Live application

Public URL: `TODO_FINALIZE: AJAIA_DEPLOY_URL`

Expected health endpoint: `TODO_FINALIZE: observed ${AJAIA_DEPLOY_URL}/health response`

Render disclosure: a free web service may cold-start after 15 idle minutes, and a free PostgreSQL database may expire after 30 days. A web-service restart can invalidate a demo login cookie on a free instance without a persistent key ring; select the same seeded identity again. PostgreSQL, not the service filesystem, stores documents.

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

The UI-specific claims—formatting behavior, autosave display, visual accessibility, and Chrome journey—remain `TODO_FINALIZE` until their final automated and visual evidence is captured.

## Verified tests

The observed backend baseline is **66 unit tests + 59 integration tests = 125**. Those counts are intentionally scoped to the backend commit `95d85d0`.

`TODO_FINALIZE: append the final release-gate transcript and exact frontend/Chrome counts only after commands have been run against the final merged source.`

## Incomplete handoffs that must remain human-owned

1. Record the walkthrough using [WALKTHROUGH_SCRIPT.md](WALKTHROUGH_SCRIPT.md) and provide the unlisted Loom or YouTube URL.
2. Add that URL as the sole line in `WALKTHROUGH_VIDEO_URL.txt`, commit it, and rebuild the archive after merge.
3. Upload the final archive, screenshots, and video-link file to one Google Drive folder.

The tracking issue stays open until the implementation PR merges. It must not be closed manually before merge.

## Next 2–4 hours

1. Finish the frontend, then run frontend unit tests, typecheck, lint, and production build.
2. Build the full Docker stack, check `/health`, run the Chrome journey in Google Chrome, and inspect desktop/mobile screenshots for overflow, keyboard focus, and console errors.
3. Complete a requirements review and a code-quality/security/accessibility review; add regression tests for accepted findings and rerun the full gate.
4. Deploy the Render Blueprint, capture the exact public URL, and repeat the Chrome journey against it.
5. Complete the human recording/upload handoff, merge only after passing checks, then create and inspect the final archive.
