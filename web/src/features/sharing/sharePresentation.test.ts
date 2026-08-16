import { describe, expect, it } from "vitest";
import { initials, shareErrorMessage } from "./sharePresentation";

describe("share presentation", () => {
  it("turns stable API codes into actionable copy", () => {
    expect(shareErrorMessage({ data: { code: "duplicate_share", detail: "Conflict" } })).toBe("This reviewer already has access.");
    expect(shareErrorMessage({ data: { code: "self_share", detail: "No" } })).toBe("The document owner already has access.");
    expect(shareErrorMessage({ data: { code: "user_not_found", detail: "No" } })).toBe("That reviewer could not be found.");
  });
  it("creates short accessible avatar initials", () => expect(initials("Chidi Ada Okeke")).toBe("CA"));
});
