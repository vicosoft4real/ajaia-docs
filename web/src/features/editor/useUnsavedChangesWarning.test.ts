import { renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { useUnsavedChangesWarning } from "./useUnsavedChangesWarning";

describe("useUnsavedChangesWarning", () => {
  it("warns only while changes remain", () => {
    const { rerender } = renderHook(({ dirty }) => useUnsavedChangesWarning(dirty), { initialProps: { dirty: false } });
    const clean = new Event("beforeunload", { cancelable: true }); window.dispatchEvent(clean); expect(clean.defaultPrevented).toBe(false);
    rerender({ dirty: true }); const dirty = new Event("beforeunload", { cancelable: true }); const prevent = vi.spyOn(dirty, "preventDefault"); window.dispatchEvent(dirty); expect(prevent).toHaveBeenCalled();
  });
});
