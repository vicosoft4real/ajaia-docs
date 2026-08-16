import { screen } from "@testing-library/react";
import { HttpResponse, http } from "msw";
import { describe, expect, it, vi } from "vitest";
import { documentDetail, sessionUser } from "../../mocks/fixtures";
import { server } from "../../mocks/server";
import { renderWithApp } from "../../test/renderWithApp";
import { DocumentEditorPage } from "./DocumentEditorPage";

vi.mock("./LexicalDocumentEditor", () => ({ LexicalDocumentEditor: ({ onChange }: { onChange: (value: { content: string; plainText: string }) => void }) => <button type="button" onClick={() => onChange({ content: '{"root":{"children":[{"text":"changed"}]}}', plainText: "changed" })}>Edit content</button> }));

function renderEditor(detail = documentDetail) {
  server.use(http.get("/api/documents/:id", () => HttpResponse.json(detail)));
  return renderWithApp(<DocumentEditorPage />, { routePath: "/documents/:documentId", initialEntry: `/documents/${detail.id}` });
}

describe("document editor integration", () => {
  it("shows rename, share and delete controls only to the owner", async () => {
    renderEditor(); expect(await screen.findByRole("button", { name: "Share" })).toBeVisible(); expect(screen.getByRole("button", { name: /delete document/i })).toBeVisible(); expect(screen.getByRole("textbox", { name: /document title/i })).not.toHaveAttribute("readonly");
  });
  it("lets a collaborator edit while withholding owner actions", async () => {
    renderEditor({ ...documentDetail, ownerId: "owner", owner: { ...sessionUser, id: "owner", displayName: "Chidi Okeke" }, isOwner: false, canRename: false, canShare: false, canDelete: false });
    expect(await screen.findByText(/collaborator · shared by chidi/i)).toBeVisible(); expect(screen.getByRole("button", { name: /edit content/i })).toBeVisible(); expect(screen.queryByRole("button", { name: "Share" })).not.toBeInTheDocument(); expect(screen.queryByRole("button", { name: /delete document/i })).not.toBeInTheDocument();
  });
  it("surfaces a version conflict without discarding the local draft", async () => {
    server.use(http.put("/api/documents/:id/content", () => HttpResponse.json({ code: "conflict", detail: "Stale version" }, { status: 409 })));
    renderEditor(); const edit = await screen.findByRole("button", { name: /edit content/i }); edit.click();
    expect(await screen.findByRole("alert", {}, { timeout: 2500 })).toHaveTextContent("newer saved version"); expect(edit).toBeVisible();
  });
});
