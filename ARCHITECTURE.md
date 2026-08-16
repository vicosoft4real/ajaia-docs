# Ajaia Docs architecture

## Design in brief

Ajaia Docs is a single-deployable .NET and React monorepo. In production, ASP.NET Core serves both its API and the compiled SPA from one origin. That keeps cookie authentication same-origin and gives reviewers one URL, while PostgreSQL holds all persistent application data.

```text
Browser (React / RTK Query / Lexical)
              │ same-origin HTTPS + HttpOnly cookie
              ▼
ASP.NET Core API ──► Application handlers ──► Core rules
       │                    │
       │                    ▼
       └────────────► Infrastructure (Dapper + PostgreSQL)
```

## Layer direction

Dependencies point inward:

```text
API → Application → Core
API → Infrastructure → Application → Core
```

- **Core** has document and sharing entities, access decisions, and stable result/error values. It has no web or database dependency.
- **Application** owns feature commands, queries, validation, DTOs, and repository/session abstractions.
- **Infrastructure** owns Dapper repositories, PostgreSQL connections, embedded/versioned SQL migrations, and demo-user seeding.
- **API** owns thin Carter route modules, cookie authentication, antiforgery, transport validation, problem responses, health checks, and SPA hosting.
- **Web** is the React 18/TypeScript SPA. RTK Query owns server state; Lexical owns editor state and serialization; feature-level UI composes library, editor, import, sharing, and authentication experiences.

Expected failures travel as result values with stable codes such as `not_found`, `owner_required`, and `conflict`. Unexpected errors are logged server-side and mapped to sanitized `application/problem+json` responses at the API boundary.

## Persistence model

PostgreSQL contains three application tables plus migration bookkeeping:

| Table | Responsibility |
| --- | --- |
| `app_users` | Immutable seeded reviewer identities, including display name, email, and avatar color. |
| `documents` | Owner, title, content format/content/plain text, positive version, and UTC timestamps. |
| `document_shares` | A collaborator grant, its owner/granter, and creation time; `(document_id, user_id)` is unique. |
| `schema_migrations` | Applied embedded SQL migration versions. |

The schema constrains titles to 1–120 trimmed characters, permits only `lexical`, `markdown`, or `plainText` storage formats, and uses foreign keys so deleting a document cascades to shares. A PostgreSQL trigger rejects attempts to share a document with its owner. Migrations are applied in filename order under an advisory lock so concurrent startup does not apply a migration twice.

## Authentication and access flow

1. A reviewer chooses one of the three seeded users on the demo login screen.
2. `POST /api/session` confirms that the requested ID is seeded, then issues an encrypted ASP.NET Core authentication cookie.
3. The browser receives an antiforgery token from `/api/session/antiforgery` and sends it in `X-XSRF-TOKEN` for state-changing requests.
4. Route handlers derive the actor exclusively from the cookie—not from request-body owner or actor IDs.
5. Repository reads impose an owner-or-share predicate. A user without access receives `404` so an inaccessible document’s existence is not disclosed.

The production cookie is HttpOnly, `SameSite=Strict`, secure, and uses eight-hour sliding expiry. Every state-changing API request requires antiforgery validation.

| Capability | Owner | Granted collaborator |
| --- | ---: | ---: |
| List/open | Yes | Yes |
| Edit content | Yes | Yes |
| Rename | Yes | No |
| Share/revoke | Yes | No |
| Delete | Yes | No |

The API is authoritative: hiding an owner-only control in the UI is not the security boundary.

## Write and concurrency flow

Title and content write operations carry an `expectedVersion`. Their SQL update predicate includes the document ID, the derived access rule, and that version; a successful write increments and returns the version.

```sql
UPDATE documents AS d
SET content = @Content,
    content_format = @ContentFormat,
    plain_text = @PlainText,
    version = d.version + 1,
    updated_at = @UpdatedAt
WHERE d.id = @DocumentId
  AND d.version = @ExpectedVersion
  AND (d.owner_id = @ActorId OR EXISTS (... matching share ...))
RETURNING d.*;
```

The editor save coordinator debounces changes, permits one request at a time, coalesces edits made during a request, and uses the last server-acknowledged version for both title and content. A stale update returns `409 conflict`; the client preserves local state, stops automatic retries, and offers a reload of the saved version. There is no silent last-writer-wins behavior.

## One-origin deployment

The planned multi-stage Docker image builds the Vite SPA, publishes the API, and copies `web/dist` into the API’s static assets. The container listens on the platform `PORT`; the Render Blueprint provides a Docker web service, a PostgreSQL database connection, production environment configuration, and `/health` as its health endpoint.

The Dockerfile, Docker Compose configuration, and Render Blueprint are present in the repository. The Docker image has built, and the Compose PostgreSQL and API services are running locally. The Render service has not yet been deployed or health-checked publicly.

## Deliberate scope cuts

This timeboxed product intentionally excludes live cursors, operational transforms, CRDTs, presence, comments, suggestions, history, `.docx` import, PDF export, attachments, role variants, registration/password/email/social login, folders, search, pagination, and templates. A share is edit access; there is no separate read-only role.

These omissions keep the evaluated slice honest: reliable persistence, clear authorization, import validation, formatting, sharing, and conflict detection are stronger evidence than a broad set of partially implemented collaboration features.

## Next work

1. Deploy the Render Blueprint, verify `/health`, and rerun the Chrome journey against the public origin.
2. Complete the human-owned video recording and Drive upload.
3. After the timebox, consider version history, comments, richer imports/exports, search/folders, and a genuinely real-time collaboration protocol only with the corresponding conflict-resolution and operational safeguards.
