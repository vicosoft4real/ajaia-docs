import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { deferred } from "../../test/deferred";
import { DocumentSaveCoordinator, type SaveResponse } from "./saveCoordinator";

describe("DocumentSaveCoordinator", () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  it("serializes writes and advances only from acknowledgements", async () => {
    const first = deferred<SaveResponse>(); const second = deferred<SaveResponse>();
    const save = vi.fn().mockReturnValueOnce(first.promise).mockReturnValueOnce(second.promise);
    const coordinator = new DocumentSaveCoordinator({ initialVersion: 3, debounceMs: 700, save, onStateChange: vi.fn(), onVersionChange: vi.fn(), onConflict: vi.fn() });
    coordinator.setContent("one", "one"); await vi.advanceTimersByTimeAsync(700);
    coordinator.setTitle("Latest title"); expect(save).toHaveBeenCalledTimes(1);
    first.resolve({ version: 4, updatedAt: "a" }); await first.promise; await vi.runAllTimersAsync();
    expect(save).toHaveBeenLastCalledWith(expect.objectContaining({ kind: "title", expectedVersion: 4 }));
    second.resolve({ version: 5, updatedAt: "b" }); await second.promise; await vi.runAllTimersAsync();
    expect(coordinator.getState()).toBe("saved");
  });

  it("coalesces content and retains failed changes for retry", async () => {
    const save = vi.fn().mockRejectedValueOnce(new TypeError("offline")).mockResolvedValueOnce({ version: 2, updatedAt: "b" });
    const coordinator = new DocumentSaveCoordinator({ initialVersion: 1, debounceMs: 700, save, onStateChange: vi.fn(), onVersionChange: vi.fn(), onConflict: vi.fn() });
    coordinator.setContent("old", "old"); coordinator.setContent("latest", "latest");
    await vi.advanceTimersByTimeAsync(700); expect(coordinator.getState()).toBe("changes-not-saved");
    coordinator.retry(); await vi.runAllTimersAsync();
    expect(save).toHaveBeenLastCalledWith(expect.objectContaining({ content: "latest", expectedVersion: 1 }));
    expect(coordinator.hasUnsavedChanges()).toBe(false);
  });

  it("stops after conflicts while preserving local changes", async () => {
    const conflict = { data: { code: "conflict", detail: "stale" } };
    const save = vi.fn().mockRejectedValue(conflict); const onConflict = vi.fn();
    const coordinator = new DocumentSaveCoordinator({ initialVersion: 1, debounceMs: 1, save, onStateChange: vi.fn(), onVersionChange: vi.fn(), onConflict });
    coordinator.setTitle("Local"); await vi.advanceTimersByTimeAsync(1);
    expect(coordinator.getState()).toBe("conflict"); expect(coordinator.hasUnsavedChanges()).toBe(true); expect(onConflict).toHaveBeenCalledWith(conflict.data);
    coordinator.retry(); await vi.runAllTimersAsync(); expect(save).toHaveBeenCalledTimes(1);
  });
});
