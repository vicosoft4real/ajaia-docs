# Ajaia Docs Design Specification

**Status:** Approved in conversation on 2026-08-15; written-spec review required before implementation planning

**Candidate:** Victor Sileola Ogundowo (`vicosoft4real@gmail.com`)

**Timebox:** 4–6 hours of focused implementation

**Repository:** Standalone `ajaia-docs` monorepo

## 1. Product Goal

Build a reviewer-friendly collaborative document editor that demonstrates a coherent product slice across document creation, rich-text editing, import, persistence, sharing, authorization, testing, and deployment. The product must let a reviewer demonstrate the complete owner-to-collaborator journey in under two minutes without registering an account or configuring external services.

The product is intentionally not a Google Docs clone. It prioritizes reliable persistence, clear access boundaries, an excellent single-document editing experience, and an evaluation-ready delivery package.

## 2. Success Criteria

A reviewer can:

1. Choose a seeded demo identity.
2. Create a blank document or import a UTF-8 `.txt` or `.md` file.
3. Rename the document and apply bold, italic, underline, headings, bulleted lists, and numbered lists.
4. Observe clear saving and saved states, refresh, and recover the preserved document and formatting.
5. Share the document with another seeded user.
6. Switch identity and find the document under **Shared with me**.
7. Open and edit the shared document while owner-only actions remain unavailable and server-protected.
8. Follow one README command path locally and one live URL in the submission package.

## 3. Deliberate Scope

### Included

- Three seeded users with one-click assessment login
- Server-issued encrypted HttpOnly sessions
- Owned and shared document library views
- Blank-document creation
- Inline rename for owners
- Lexical rich-text editor
- Bold, italic, underline, H1/H2, bulleted lists, numbered lists, undo, and redo
- Debounced, serialized autosave with visible state
- UTF-8 `.txt` and `.md` imports up to 1 MB
- Owner-to-user sharing and revocation
- PostgreSQL persistence
- Optimistic concurrency for multi-session safety
- Responsive and accessible desktop/mobile UI
- Automated unit, integration, frontend, and browser coverage proportionate to risk
- Docker-based single-URL deployment
- Evaluation documentation and Chrome screenshots

### Excluded

- Live cursors, operational transforms, CRDTs, or real-time co-editing
- Presence indicators that imply real-time collaboration
- Comments, suggestions, or document version history
- `.docx` import, PDF export, attachments, and embedded uploads
- Read-only/editor role variants; a granted collaborator receives edit access
- Registration, passwords, password recovery, email delivery, and social login
- Folders, search, pagination, and document templates

Optimistic concurrency is the honest substitute for real-time collaboration in this timebox. It prevents silent overwrite without presenting the product as a live multi-user editor.

## 4. System Architecture

The repository is a single deployable monorepo:

```text
ajaia-docs/
├── src/
│   ├── AjaiaDocs.Core/
│   ├── AjaiaDocs.Application/
│   ├── AjaiaDocs.Infrastructure/
│   └── AjaiaDocs.Api/
├── web/
├── tests/
│   ├── AjaiaDocs.UnitTests/
│   └── AjaiaDocs.IntegrationTests/
├── e2e/
├── docs/
└── Dockerfile
```

Production uses one origin: ASP.NET Core serves the API and the compiled React SPA. This removes cross-origin cookie and deployment complexity, gives reviewers one URL, and keeps the timebox focused on product behavior.

### Backend boundaries

- **Core** owns entities, domain rules, value decisions, and error contracts. It has no persistence or web dependencies.
- **Application** owns feature slices, commands, queries, validators, DTOs, and repository/session abstractions.
- **Infrastructure** owns Dapper repositories, PostgreSQL connections, versioned SQL migrations, session implementation support, and demo-data seeding.
- **API** owns thin Minimal API modules, transport validation, authorization context extraction, antiforgery enforcement, structured problem responses, health checks, and SPA hosting.

Expected failures use explicit result values and stable error codes. Exceptions are reserved for unexpected infrastructure faults and are converted to sanitized problem details at the API boundary.

### Frontend boundaries

- React 18, TypeScript, and Vite provide the SPA foundation.
- RTK Query owns server state, cache invalidation, and authenticated API requests.
- Lexical owns editor behavior and editor-state serialization.
- Tailwind and accessible Radix-style primitives provide the design system.
- Route-level pages compose focused feature components; API and editor concerns remain isolated.

## 5. Domain and Persistence Model

### User

Seeded users are immutable assessment identities:

- `Id` UUID
- `DisplayName`
- `Email`
- `AvatarColor`

The migration uses stable UUIDs so seeded identities and browser tests remain deterministic.

### Document

- `Id` UUID
- `OwnerId` UUID
- `Title` varchar(120)
- `ContentFormat` enum-like varchar: `lexical`, `markdown`, or `plainText`
- `Content` text; interpreted according to `ContentFormat` (serialized JSON for `lexical`, raw UTF-8 text for imports)
- `PlainText` text for library previews and future search
- `Version` positive integer
- `CreatedAt` UTC timestamp
- `UpdatedAt` UTC timestamp

Deletion is a hard delete in this assessment scope. Foreign keys cascade to shares, and the confirmation flow prevents accidental deletion. Recoverable trash is future work.

Normal saves use `lexical`. Imports begin as `markdown` or `plainText`, remain fully persisted before the editor opens, and are normalized to Lexical state on the first successful editor save.

### DocumentShare

- `DocumentId` UUID
- `UserId` UUID
- `SharedByUserId` UUID
- `CreatedAt` UTC timestamp
- unique key on `(DocumentId, UserId)`

The database forbids sharing a document with its owner and prevents duplicate grants. Repository reads always apply an owner-or-share access predicate.

## 6. Authorization and Session Model

The login screen displays the three seeded identities. Choosing one posts the identity ID to the demo-session endpoint, which verifies that it is a seeded user before issuing an encrypted ASP.NET Core authentication cookie.

Session properties:

- HttpOnly
- Secure in production
- SameSite `Strict`
- eight-hour expiry with sliding renewal
- no user identity accepted from document request bodies
- antiforgery token/header required for state-changing requests

The UI labels login as **Demo access for reviewers** so the mechanism is not mistaken for production authentication.

Authorization rules:

| Capability | Owner | Collaborator |
|---|---:|---:|
| List/open document | Yes | Yes |
| Edit content | Yes | Yes |
| Rename | Yes | No |
| Share/revoke | Yes | No |
| Delete | Yes | No |

An inaccessible document ID returns `404` to avoid exposing its existence. An authenticated collaborator attempting a known owner-only operation receives `403` with a stable `owner_required` code.

## 7. API Contract

The API surface is intentionally small:

```text
GET    /api/session
POST   /api/session
DELETE /api/session
GET    /api/session/antiforgery

GET    /api/users/share-candidates?documentId={id}

GET    /api/documents?scope=all|owned|shared
POST   /api/documents
POST   /api/documents/import
GET    /api/documents/{id}
PUT    /api/documents/{id}/content
PUT    /api/documents/{id}/title
DELETE /api/documents/{id}

GET    /api/documents/{id}/shares
POST   /api/documents/{id}/shares
DELETE /api/documents/{id}/shares/{userId}

GET    /health
```

Document content updates contain:

- `contentFormat: "lexical"`
- serialized editor `content`
- extracted `plainText`
- `expectedVersion`

The repository executes an atomic update constrained by document ID, access, and expected version. Zero updated rows are resolved as not found, forbidden, or conflict without leaking access details. A successful update increments and returns `version` and `updatedAt`.

Owner title updates also include `expectedVersion` and increment the same document version. The frontend serializes title and content mutations through one document-save coordinator, preventing two local requests from racing. Share grants and revocations do not alter the document version because they update a separate access relation.

Errors use `application/problem+json` with a stable `code`, human-readable `detail`, and field-level validation errors where relevant.

## 8. File Import

`POST /api/documents/import` accepts one multipart file. The server:

1. Confirms the authenticated user.
2. Enforces a 1 MB request/file limit.
3. Accepts only `.txt` and `.md` filenames.
4. Decodes strict UTF-8 and rejects invalid byte sequences.
5. Derives a trimmed title from the filename, falling back to `Untitled document`.
6. Persists the source content, content format, and a safe plain-text preview.
7. Returns the created document and editor route.

MIME type is advisory; extension, size, and decoding are enforced server-side. The UI states the supported types and limit beside the import control.

## 9. Editing and Save Semantics

The editor provides a sticky toolbar with bold, italic, underline, H1, H2, bulleted list, numbered list, undo, and redo. Keyboard shortcuts and active formatting states remain visible.

The save coordinator:

1. Marks the document dirty on editor changes.
2. Debounces for 700 ms.
3. Allows only one save request at a time.
4. Coalesces changes made during an in-flight save into the next request.
5. Sends the last acknowledged version.
6. Updates the local version only from a successful response.

Visible states are `Saved`, `Saving…`, `Changes not saved`, and `Resolve conflict`. Navigation with unsaved changes receives a browser warning.

A `409` preserves the local editor state, stops automatic retries, and offers **Reload saved version**. The product does not silently choose a winner or claim real-time merge behavior.

## 10. Product and Visual Design

The page's single job is to help a small team find a document, write, and hand it to a collaborator with minimal ceremony.

### Tokens

- Midnight ink: `#17233C`
- Cool paper: `#F7F9FC`
- Action cobalt: `#365CF5`
- Shared mint: `#25A77A`
- Warning amber: `#C77A15`
- Mist border: `#DCE3EF`
- Manrope for interface copy and controls
- Literata for the editable document surface

Semantic CSS variables expose these values; components do not scatter raw colors.

### Layout

```text
Library                              Editor
┌────────────┬──────────────────┐    ┌────────── title · Saved · Share ───────┐
│ User       │ New / Import     │    │ B  I  U  H1  H2  •  1.  Undo  Redo     │
│ switcher   │                  │    ├──────────────────────────────────────────┤
│            │ Recent documents │    │              writing page                │
│ Owned      │                  │    │                                          │
│ Shared     │ Owned / Shared   │    │                                          │
└────────────┴──────────────────┘    └──────────────────────────────────────────┘
```

The signature element is a narrow ownership edge repeated on cards and the editor: cobalt means owned and mint means shared. Text labels remain present so status never depends on color alone.

The editor is the dominant surface. Decoration stays restrained. Motion is limited to save-status transitions and respects `prefers-reduced-motion`.

### Responsive and accessible behavior

- Desktop uses the library rail and centered writing page.
- Mobile moves navigation into a drawer and wraps the formatting toolbar.
- Every action is reachable by keyboard and has a visible focus treatment.
- Dialogs trap focus, expose accessible names/descriptions, and restore focus on close.
- Empty, loading, error, and unauthorized states provide a clear next action.
- Contrast meets WCAG AA for normal text and essential controls.

## 11. Frontend Routes and Components

```text
/login
/documents
/documents/new
/documents/:documentId
```

Primary feature units:

- `DemoLoginPage`: identity selection and assessment-auth explanation
- `DocumentLibraryPage`: owned/shared filters, new/import actions, empty states
- `DocumentCard`: ownership edge, owner identity, preview, update time
- `DocumentEditorPage`: document loading, access-aware controls, save coordinator
- `DocumentToolbar`: formatting commands and active states
- `ShareDocumentDialog`: eligible-user selection, current grants, revoke actions
- `ImportDocumentDialog`: file constraints, selection, progress, error guidance
- `SaveStatus`: accessible save/error/conflict announcements

Owner-only controls are omitted or disabled with an explanation for collaborators, but the API remains the authority.

## 12. Validation and Failure Experience

- Titles are trimmed and must contain 1–120 characters.
- Editor payloads are limited to 2 MB.
- Imports are UTF-8 `.txt` or `.md` files up to 1 MB.
- Empty or whitespace-only imported files are allowed and become editable blank documents with a filename-derived title.
- Sharing with the owner, the current user, an unknown user, or an already-shared user returns a specific validation/conflict response.
- Database and unexpected exception details are logged server-side and never exposed to the browser.
- Failed autosaves do not clear dirty state.
- Offline/network failures offer **Try saving again**.
- Destructive deletion requires confirmation and redirects to the library only after server success.

## 13. Testing Strategy

Implementation follows red-green-refactor. Tests assert observable behavior rather than implementation details.

### Unit tests

- Document title normalization and rejection
- Owner/collaborator capability decisions
- Self-share and duplicate-share rejection
- Successful version increment and stale-version conflict
- Strict import validation
- Save coordinator serialization and change coalescing

### Integration tests

Use a real PostgreSQL Testcontainer and the ASP.NET test host. The key journey creates a document as one seeded user, shares it, opens it as a second user, edits it, and proves that the collaborator cannot rename, share, or delete it.

### Frontend tests

Vitest and Testing Library cover access-dependent controls, import validation, structured error presentation, and save-state transitions using the real save coordinator with the network boundary stubbed.

### Browser and visual verification

Playwright runs the primary journey in Google Chrome. Final visual validation captures desktop and mobile screenshots, checks horizontal overflow, verifies visible focus, and records browser console errors. The repository-level instruction requiring Chrome validation is a release gate.

## 14. Deployment

A multi-stage Dockerfile builds the Vite app, publishes the .NET 10 API, copies the SPA into API static assets, and runs one public service. A Render Blueprint defines:

- one Docker web service
- one PostgreSQL database
- database connection environment variable
- health check path

Versioned migrations and demo seeding run idempotently at startup under a database advisory lock or equivalent single-run protection.

Render's free web service can sleep after 15 idle minutes, and its free PostgreSQL database expires after 30 days. The README and submission note disclose the cold start and expiration. No runtime document data is stored on the web service's ephemeral filesystem. A service restart may invalidate demo login cookies because the free instance does not provide a persistent key ring; reviewers can immediately choose the same seeded identity again, and document data remains intact.

## 15. Delivery Artifacts

The final repository contains:

- `README.md` with local prerequisites and exact run/test commands
- `ARCHITECTURE.md` with boundaries, data flow, tradeoffs, and future work
- `AI_WORKFLOW.md` describing tools used, acceleration, rejected/changed output, and verification
- `SUBMISSION.md` listing every included artifact, live URL, demo identities, working/incomplete scope, and next 2–4 hours
- `WALKTHROUGH_SCRIPT.md` with a 3–5 minute narration and shot sequence
- `WALKTHROUGH_VIDEO_URL.txt` containing the human-recorded final link
- desktop and mobile Chrome screenshots
- source code, migrations, tests, Dockerfile, Compose file, and Render Blueprint

The assistant prepares the script, shot list, screenshots, deployable build, and submission text. Recording the candidate's voice/video and placing the final package in the candidate's Google Drive remain human-owned handoff steps.

## 16. Acceptance Checklist

- [ ] A seeded user can create, rename, edit, save, refresh, and reopen a document.
- [ ] Bold, italic, underline, headings, bullets, and numbers survive refresh.
- [ ] A valid `.txt` or `.md` upload creates an editable document.
- [ ] Unsupported, oversized, and invalid UTF-8 imports receive useful errors.
- [ ] Owned and shared documents are visibly distinct in the library and editor.
- [ ] An owner can grant and revoke another seeded user's access.
- [ ] A collaborator can edit content but cannot rename, share, revoke, or delete.
- [ ] All document access rules are enforced in server queries/commands.
- [ ] Stale updates return `409` and never silently overwrite newer content.
- [ ] Data survives refresh and web-service restart because it resides in PostgreSQL.
- [ ] Automated unit, integration, frontend, and Chrome journey checks pass.
- [ ] Desktop and mobile Chrome visual validation has no blocking defects.
- [ ] Setup, architecture, AI workflow, limitations, credentials, and deployment are documented.
- [ ] The live URL and final walkthrough URL are present in `SUBMISSION.md` before candidate submission.
