# AI workflow and verification record

## Tools and roles

Codex was used as the implementation and coordination assistant. Superpowers provided structured spec/plan and task-driven-development workflow support. `frontend-design` was used to accelerate the UI design direction and implementation guidance. Parallel agents worked on bounded, non-overlapping deliverables so backend, frontend, deployment/evidence, and documentation could progress concurrently.

AI accelerated:

- translating the approved product brief into a four-layer backend, small HTTP surface, data model, and test plan;
- producing and iterating on focused implementation slices, including migrations, authorization, import validation, and optimistic-concurrency behavior;
- shaping the one-origin React/.NET delivery approach and evaluator-oriented documentation;
- identifying verification coverage across unit, PostgreSQL integration, frontend, and Chrome browser levels; and
- preparing this reviewer walkthrough and handoff material.

Human direction remained decisive for the approved scope, final review, deployment account access, recording, and Drive submission.

## Recommendations deliberately changed or rejected

The approved design changed or rejected several broader generated directions to protect the timebox and avoid overstating capability:

| Direction considered | Decision |
| --- | --- |
| Real-time cursors, OT/CRDT merging, and presence | Rejected. Version-checked writes make concurrent edits safe without pretending the editor merges live changes. |
| Cross-origin SPA/API deployment | Rejected. The compiled SPA is served by ASP.NET Core from one origin, simplifying cookies and reviewer setup. |
| A mediator/dispatcher layer for handlers | Rejected. Direct handler injection preserves feature slices without added framework/licensing or custom-dispatcher overhead. |
| Broad document-suite features (comments, history, files, PDF/`.docx`, folders, search) | Deferred. The evaluated slice focuses on import, editing, persistence, sharing, access control, and conflict handling. |
| User-supplied actor/owner IDs in document mutations | Rejected for security. The actor is derived only from the encrypted session cookie. |
| AI-recorded voice/video or AI-owned external submission | Rejected. The candidate retains ownership of recording the walkthrough and uploading the final package. |

## Verification actually observed

The final observed backend test evidence is:

| Suite | Verified count |
| --- | ---: |
| Unit | 75 |
| Integration | 59 |
| Total backend | 134 |

The backend tests cover domain/access decisions, application validation and sharing/import behavior, PostgreSQL migration/persistence behavior, session and antiforgery routes, and owner-to-collaborator HTTP journeys.

The integrated frontend release gate verified **22 files / 47 tests**, `pnpm --dir web typecheck`, `pnpm --dir web lint`, and `pnpm --dir web build`. The Docker image rebuilt successfully, Docker Compose has PostgreSQL and the API running locally, and `/health` returned `{"status":"healthy"}`.

The clean-state installed-Google-Chrome `ajaia-docs.spec.ts` journey passed **2/2**, covering create/format/share/collaborator and Markdown-import paths with no browser errors. The visual spec passed **1/1**. The four committed screenshots were inspected: desktop library/editor at 1440×1000 and mobile library/editor at 390×844. The visual gate covers responsive overflow, visible keyboard focus, and browser diagnostics.

A release blocker found during the final pass could retain reviewer-specific cached data after switching identities. It was fixed in `07ad85f`. A second release fix, `a9a2d57`, avoids a session refetch during logout; the AppShell suite passed **4 tests**, the full frontend suite passed, and typecheck passed. The Render service is live at [https://ajaia-docs-z2ua.onrender.com](https://ajaia-docs-z2ua.onrender.com) from merged `main` commit `6639ae0328b5d530334b528191108a807a5edef4`; `/health` returned `{"status":"healthy"}`, `/` returned 200 `text/html`, and production startup logs are live. A Chrome run against the public origin is not claimed because the required browser-extension capability is unavailable.

## Quality guardrails

The plan requires test-first behavior for custom production code, with generated scaffolding as the only setup exception. It also requires an accepted review finding to receive a failing regression test before a fix, then the affected suite and full release gate to be rerun. API errors are sanitized at the boundary, state changes use antiforgery validation, and access decisions are enforced in server queries—not merely hidden in the UI.

Critical/high JavaScript advisories were patched. The production dependency audit still reports three moderate React Router v6 advisories. The app uses hard-coded internal navigation paths rather than untrusted user-supplied URLs; the residual advisories are documented rather than claimed as resolved.

This document does not claim that AI output is correct by itself. The submission relies on observed local test/browser evidence and observed Render health/root evidence; the deployed Chrome journey and candidate-owned video/Drive handoffs remain pending.
