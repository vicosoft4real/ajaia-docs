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

At the documentation baseline (backend commit `95d85d0`), the observed backend test evidence is:

| Suite | Verified count |
| --- | ---: |
| Unit | 66 |
| Integration | 59 |
| Total backend | 125 |

The backend tests cover domain/access decisions, application validation and sharing/import behavior, PostgreSQL migration/persistence behavior, session and antiforgery routes, and owner-to-collaborator HTTP journeys.

The following work is intentionally recorded as pending, not inferred from the plan:

- `TODO_FINALIZE: exact frontend test result, typecheck, lint, and production-build outputs`
- `TODO_FINALIZE: exact Google Chrome E2E command/result, including import and collaboration journey`
- `TODO_FINALIZE: desktop/mobile screenshot inspection, focus/overflow/console-error evidence`
- `TODO_FINALIZE: Docker health check and complete release-gate output`
- `TODO_FINALIZE: deployed Render health check and Chrome run against its observed URL`

## Quality guardrails

The plan requires test-first behavior for custom production code, with generated scaffolding as the only setup exception. It also requires an accepted review finding to receive a failing regression test before a fix, then the affected suite and full release gate to be rerun. API errors are sanitized at the boundary, state changes use antiforgery validation, and access decisions are enforced in server queries—not merely hidden in the UI.

This document does not claim that AI output is correct by itself. The submission must rely on observed test, browser, and deployment evidence once those parallel handoffs are complete.
