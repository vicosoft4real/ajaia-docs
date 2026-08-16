# Ajaia Docs walkthrough script (3–5 minutes)

Use the live deployment after it exists; it is not deployed at this update. Until then, record against the local Docker stack after the final Chrome release gate passes.

## 0:00–0:25 — Set the frame

Show the login page.

> “Ajaia Docs is a focused collaborative editor for reviewer evaluation. It is intentionally not a real-time Google Docs clone: it demonstrates reliable persistence, clear owner/collaborator permissions, and conflict-safe editing in a single deployable product.”

Point out **Demo access for reviewers** and select **Amina Okafor**.

## 0:25–1:05 — Create and import

In the library, briefly show the owned/shared distinction. Choose **New document**, create a document named **Launch brief**, and open it.

Then show the import entry point and select a small UTF-8 `.md` or `.txt` file. Explain:

> “Imports accept only UTF-8 `.txt` and `.md` files, up to 1 MiB. The server validates extension, size, and decoding; MIME type alone is not trusted.”

If time is tight, do the import as a brief secondary demonstration and return to **Launch brief** for the editing journey.

## 1:05–1:45 — Format, save, and refresh

Type a short release plan. Use the toolbar to apply bold, italic, underline, a heading, a bulleted list, and a numbered list. Pause long enough for the visible save state to show **Saved**.

> “The editor serializes title and content saves through one coordinator. It debounces changes, sends the last acknowledged version, and keeps only one save in flight.”

Refresh the page and show the saved title/content/formatting returning.

The integrated frontend gate verified this editor surface through 47 tests, typecheck, lint, and production build. The installed-Google-Chrome visual rerun passed 1/1 and the responsive evidence was inspected; demonstrate the same visible save-state labels and formatting sequence in the recording.

## 1:45–2:35 — Share and switch identity

Open **Share**, select **Chidi Okeke**, and grant access. Switch identity to Chidi. In **Shared with me**, open **Launch brief** and add one short line of content.

Call out the controls that are absent or unavailable for a collaborator:

> “A collaborator can read and edit content, but cannot rename, share, revoke access, or delete. That is enforced by the API as well as the interface.”

If the UI exposes a suitable error or test view, note that owner-only attempts receive `owner_required`; inaccessible document IDs are concealed as `404`.

## 2:35–3:15 — Explain safe collaboration

> “This is not live co-editing. Each write includes an expected document version. PostgreSQL updates only when that version still matches; a stale write returns a conflict and the client preserves local edits instead of silently overwriting another session.”

Show the saved state again after Chidi’s edit and refresh once to demonstrate persisted content.

## 3:15–4:00 — Architecture and security

Show [ARCHITECTURE.md](ARCHITECTURE.md), a simple architecture diagram, or the README architecture link.

> “React, RTK Query, and Lexical run in the browser. ASP.NET Core serves the compiled SPA and API from the same origin. The API calls focused application handlers over Dapper/PostgreSQL. The session is an encrypted HttpOnly cookie, state changes require antiforgery, and server queries apply the owner-or-share rule.”

Mention the three seeded identities and the Render caveat: free services can cold-start and a free database can expire; a restart may require selecting the demo identity again.

## 4:00–4:35 — Scope and AI verification

> “We deliberately excluded CRDTs, presence, comments, version history, `.docx`/PDF workflows, and broad workspace features so the delivered slice could be tested and honestly demonstrated.”

> “Codex, Superpowers, frontend-design, and parallel agents accelerated implementation and documentation. AI suggestions for real-time collaboration and extra platform layers were narrowed or rejected. The package records 125 verified backend tests, 47 frontend tests plus typecheck/lint/build, a locally running Docker stack, and a 1/1 installed-Google-Chrome visual pass with inspected responsive screenshots. Deployment evidence is added only after its checks complete.”

Close on the submission materials: README for setup, architecture and AI workflow notes, screenshots, and the human-owned video link.

## Recording checklist

- Use the observed final URL and wait through a cold start if necessary.
- Keep the walkthrough to 3–5 minutes; do not imply live cursors or automatic merge.
- Show an actual save then refresh, and an actual identity switch after sharing.
- Do not expose secrets; the seeded `.example.test` identities are intentional demo data.
- After upload, provide the unlisted URL so `WALKTHROUGH_VIDEO_URL.txt` can contain that URL as its only line.
