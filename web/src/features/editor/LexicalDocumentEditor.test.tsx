import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { renderEditor } from "./test/renderEditor";

describe("LexicalDocumentEditor", () => {
  it("imports plain text without reporting an initial change", async () => {
    const editor = renderEditor("First paragraph\n\nSecond paragraph", "plainText");
    expect(screen.getByRole("textbox", { name: "Document content" })).toHaveTextContent("First paragraph");
    expect(editor.changes()).toHaveLength(0);
    await editor.user.type(screen.getByRole("textbox", { name: "Document content" }), " edited");
    expect(editor.latestChange().plainText).toContain("edited");
  });

  it("imports markdown headings and lists", () => {
    renderEditor("# Plan\n\n- Draft\n- Review", "markdown");
    expect(screen.getByRole("heading", { level: 1, name: "Plan" })).toBeInTheDocument();
    expect(screen.getByRole("list")).toBeInTheDocument();
  });
});
