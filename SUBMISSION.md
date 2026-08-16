# Ajaia Docs reviewer submission

## Submission status

This is the evidence-led submission package for Ajaia Docs. It deliberately separates verified backend evidence from parallel work that must be observed before final submission.

| Item | Status |
| --- | --- |
| Source and approved design/implementation plan | Present in repository |
| Backend baseline | Verified at `95d85d0`: 66 unit + 59 integration = 125 tests |
| Reviewer documentation | Present: README, architecture, AI workflow, and walkthrough script |
| Focused frontend gates | Green for foundation, library, editor, and autosave; final combined frontend/release gate pending |
| Desktop/mobile Chrome screenshots | Pending Chrome run and visual validation |
| Render public URL and deployed E2E evidence | Not deployed yet |
| Human walkthrough recording URL | Candidate-owned; `WALKTHROUGH_VIDEO_URL.txt` contains the pending-recording handoff until a final unlisted URL exists |
| Archive and Google Drive upload | Candidate-owned after final merge; not yet created/uploaded |

## Source and included artifacts

The repository currently includes:

- .NET solution source, PostgreSQL migrations, backend unit/integration tests, and API routes
- React/Vite frontend source, frontend tests, and Chrome E2E tests
- Dockerfile, Docker Compose configuration, Render Blueprint, and CI workflow
- `README.md`, `ARCHITECTURE.md`, `AI_WORKFLOW.md`, this document, and `WALKTHROUGH_SCRIPT.md`
- Chrome E2E/visual test sources; desktop and mobile evidence will be added after the Chrome capture

The final merged archive has not yet been created or inspected.

## Live application

Public URL: not deployed yet.

Expected health endpoint after deployment: `/health`.

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

Focused frontend gates cover the foundation, library, editor, and autosave. Final combined frontend verification, visual accessibility inspection, and the Chrome journey remain pending.

## Verified tests

The observed backend baseline is **66 unit tests + 59 integration tests = 125**. Those counts are intentionally scoped to the backend commit `95d85d0`.

The final combined frontend and Chrome counts will be recorded only after the full release gate runs against the final merged source.

## Incomplete handoffs that must remain human-owned

1. Record the walkthrough using [WALKTHROUGH_SCRIPT.md](WALKTHROUGH_SCRIPT.md) and provide the unlisted Loom or YouTube URL.
2. Replace the pending marker/instructions in `WALKTHROUGH_VIDEO_URL.txt` with that URL as its only line, commit it, and rebuild the archive after merge.
3. Upload the final archive, screenshots, and video-link file to one Google Drive folder.

The tracking issue stays open until the implementation PR merges. It must not be closed manually before merge.

## Next 2–4 hours

1. Finish the frontend, then run frontend unit tests, typecheck, lint, and production build.
2. Build the full Docker stack, check `/health`, run the Chrome journey in Google Chrome, and inspect desktop/mobile screenshots for overflow, keyboard focus, and console errors.
3. Complete a requirements review and a code-quality/security/accessibility review; add regression tests for accepted findings and rerun the full gate.
4. Deploy the Render Blueprint, capture the exact public URL, and repeat the Chrome journey against it.
5. Complete the human recording/upload handoff, merge only after passing checks, then create and inspect the final archive.
