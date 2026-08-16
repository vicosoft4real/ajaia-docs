import { describe, expect, it } from "vitest";
import { validateImportFile } from "./documentValidation";

describe("validateImportFile", () => {
  it("accepts txt, markdown, and uppercase extensions", () => {
    expect(validateImportFile(new File(["notes"], "notes.txt"))).toBeNull();
    expect(validateImportFile(new File(["notes"], "NOTES.MD"))).toBeNull();
  });

  it.each([
    [new File([], "empty.md"), "Choose a file that contains some text."],
    [new File(["notes"], "notes.pdf"), "Choose a .txt or .md file no larger than 1 MB."],
    [new File([new Uint8Array(1024 * 1024 + 1)], "large.md"), "Choose a .txt or .md file no larger than 1 MB."],
  ])("rejects invalid files", (file, message) => expect(validateImportFile(file)).toBe(message));
});
