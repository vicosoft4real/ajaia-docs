import type { ProblemDetails } from "../../types/api";

export type SaveState = "saved" | "saving" | "changes-not-saved" | "conflict";
export type SaveIntent = { kind: "content"; content: string; plainText: string; contentFormat: "lexical" } | { kind: "title"; title: string };
export type SaveResponse = { version: number; updatedAt: string };
export type DocumentSaveCoordinatorOptions = { initialVersion: number; debounceMs?: number; save: (intent: SaveIntent & { expectedVersion: number }) => Promise<SaveResponse>; onStateChange: (state: SaveState) => void; onVersionChange: (response: SaveResponse) => void; onConflict: (error: ProblemDetails) => void };

export class DocumentSaveCoordinator {
  private version: number; private readonly debounceMs: number; private readonly options: DocumentSaveCoordinatorOptions;
  private pending: Partial<Record<SaveIntent["kind"], SaveIntent>> = {}; private inFlight?: SaveIntent; private timer?: ReturnType<typeof setTimeout>; private state: SaveState = "saved"; private lastKind: SaveIntent["kind"] = "title"; private stopped = false; private disposed = false; private pumpPromise?: Promise<void>;
  constructor(options: DocumentSaveCoordinatorOptions) { this.options = options; this.version = options.initialVersion; this.debounceMs = options.debounceMs ?? 700; }
  setContent(content: string, plainText: string) { this.queue({ kind: "content", content, plainText, contentFormat: "lexical" }); }
  setTitle(title: string) { this.queue({ kind: "title", title }); }
  getState() { return this.state; }
  hasUnsavedChanges() { return Boolean(this.inFlight || this.pending.title || this.pending.content); }
  private queue(intent: SaveIntent) { if (this.disposed || this.stopped) return; this.pending[intent.kind] = intent; this.setState("changes-not-saved"); if (!this.inFlight) this.schedule(); }
  private schedule() { if (this.timer) clearTimeout(this.timer); this.timer = setTimeout(() => { this.timer = undefined; void this.pump(); }, this.debounceMs); }
  private takeNext() { const preferred = this.lastKind === "title" ? "content" : "title"; const kind = this.pending[preferred] ? preferred : preferred === "title" ? "content" : "title"; const intent = this.pending[kind]; if (intent) { delete this.pending[kind]; this.lastKind = kind; } return intent; }
  private pump(): Promise<void> {
    if (this.disposed || this.stopped || this.pumpPromise) return this.pumpPromise ?? Promise.resolve();
    const run = async () => {
      while (!this.disposed && !this.stopped) {
        const intent = this.takeNext(); if (!intent) { this.setState("saved"); return; }
        this.inFlight = intent; this.setState("saving");
        try { const response = await this.options.save({ ...intent, expectedVersion: this.version }); this.version = response.version; this.options.onVersionChange(response); this.inFlight = undefined; }
        catch (error: unknown) { this.inFlight = undefined; if (!this.pending[intent.kind]) this.pending[intent.kind] = intent; const problem = this.problem(error); if (problem?.code === "conflict") { this.stopped = true; this.setState("conflict"); this.options.onConflict(problem); } else this.setState("changes-not-saved"); return; }
      }
    };
    const operation = run().finally(() => { if (this.pumpPromise === operation) this.pumpPromise = undefined; }); this.pumpPromise = operation; return operation;
  }
  private problem(error: unknown): ProblemDetails | undefined { const wrapped = error as { data?: ProblemDetails; code?: string }; return wrapped?.data?.code ? wrapped.data : wrapped?.code ? wrapped as ProblemDetails : undefined; }
  private hasPending() { return Boolean(this.pending.title || this.pending.content); }
  private setState(state: SaveState) { if (this.state !== state) { this.state = state; this.options.onStateChange(state); } }
  async flush() { if (this.timer) { clearTimeout(this.timer); this.timer = undefined; } while (!this.disposed && !this.stopped && (this.inFlight || this.hasPending())) { await (this.pumpPromise ?? this.pump()); if (this.state === "changes-not-saved") break; } }
  retry() { if (this.disposed || this.stopped || !this.hasPending()) return; if (this.timer) clearTimeout(this.timer); this.timer = undefined; void this.pump(); }
  dispose() { this.disposed = true; if (this.timer) clearTimeout(this.timer); this.timer = undefined; }
}
