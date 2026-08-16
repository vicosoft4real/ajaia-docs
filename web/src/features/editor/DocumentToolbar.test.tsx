import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { renderEditor } from "./test/renderEditor";

describe("DocumentToolbar", () => {
  it("exposes accessible formatting controls with focus styling and active state", async () => {
    const { user } = renderEditor("", "plainText");
    const bold = screen.getByRole("button", { name: "Bold" });
    expect(bold).toHaveClass("focus-visible:ring-2");
    await user.click(bold);
    expect(bold).toHaveAttribute("aria-pressed", "true");
    await user.type(screen.getByRole("textbox", { name: "Document content" }), "Strong");
    expect(screen.getByText("Strong").tagName).toBe("STRONG");
  });

  it("preserves heading, underline, and numbered list structure after reload", async () => {
    const first = renderEditor("", "plainText");
    await first.user.click(screen.getByRole("button", { name: "Heading 1" }));
    await first.user.click(screen.getByRole("button", { name: "Underline" }));
    await first.user.click(screen.getByRole("button", { name: "Numbered list" }));
    await first.user.type(screen.getByRole("textbox", { name: "Document content" }), "Release plan");
    const serialized = first.latestChange().content;
    first.unmount();
    renderEditor(serialized, "lexical");
    expect(screen.getByRole("heading", { level: 1, name: "Release plan" })).toBeInTheDocument();
    expect(screen.getByRole("list").tagName).toBe("OL");
    expect(screen.getByText("Release plan")).toHaveStyle("text-decoration: underline");
  });
});
