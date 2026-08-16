import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { renderEditor } from "./test/renderEditor";

describe("DocumentToolbar", () => {
  it("exposes accessible formatting controls with focus styling and active state", async () => {
    const { user } = renderEditor("Strong", "plainText");
    const bold = screen.getByRole("button", { name: "Bold" });
    const content = screen.getByRole("textbox", { name: "Document content" });
    expect(bold).toHaveClass("focus-visible:ring-2");
    await user.click(content);
    await user.keyboard("{Control>}a{/Control}");
    await user.click(bold);
    expect(bold).toHaveAttribute("aria-pressed", "true");
    expect(screen.getByText("Strong").tagName).toBe("STRONG");
  });

  it("preserves headings, underline, and normalized numbered lists after reload", async () => {
    const heading = renderEditor("Release plan", "plainText");
    const headingContent = screen.getByRole("textbox", { name: "Document content" });
    await heading.user.click(headingContent);
    await heading.user.click(screen.getByRole("button", { name: "Heading 1" }));
    const headingJson = heading.latestChange().content;
    heading.unmount();
    const headingReload = renderEditor(headingJson, "lexical");
    expect(screen.getByRole("heading", { level: 1, name: "Release plan" })).toBeInTheDocument();
    headingReload.unmount();

    const list = renderEditor("Release plan", "plainText");
    const listContent = screen.getByRole("textbox", { name: "Document content" });
    await list.user.click(listContent);
    await list.user.keyboard("{Control>}a{/Control}");
    await list.user.click(screen.getByRole("button", { name: "Underline" }));
    await list.user.click(screen.getByRole("button", { name: "Numbered list" }));
    const listJson = list.latestChange().content;
    list.unmount();
    renderEditor(listJson, "lexical");
    expect(screen.getByRole("list").tagName).toBe("OL");
    expect(screen.getByText("Release plan")).toHaveStyle("text-decoration: underline");
  });
});
