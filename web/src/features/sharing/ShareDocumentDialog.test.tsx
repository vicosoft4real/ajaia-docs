import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { describe, expect, it, vi } from "vitest";
import { documentShare, shareCandidate } from "../../mocks/fixtures";
import { server } from "../../mocks/server";
import { renderWithApp } from "../../test/renderWithApp";
import { ShareDocumentDialog } from "./ShareDocumentDialog";

describe("ShareDocumentDialog", () => {
  it("grants and revokes a seeded collaborator with explicit feedback", async () => {
    let shares = [] as typeof documentShare[];
    server.use(
      http.get("/api/users/share-candidates", () => HttpResponse.json([shareCandidate])),
      http.get("/api/documents/:id/shares", () => HttpResponse.json(shares)),
      http.post("/api/documents/:id/shares", () => { shares = [documentShare]; return HttpResponse.json(documentShare, { status: 201 }); }),
      http.delete("/api/documents/:id/shares/:userId", () => { shares = []; return new HttpResponse(null, { status: 204 }); }),
    );
    const user = userEvent.setup();
    renderWithApp(<ShareDocumentDialog documentId={documentShare.documentId} open onOpenChange={vi.fn()} />);
    await user.click(await screen.findByRole("button", { name: /share with chidi/i }));
    expect(await screen.findByText("Chidi Okeke has access")).toBeVisible();
    await user.click(await screen.findByRole("button", { name: /remove chidi okeke/i }));
    expect(await screen.findByText("Access removed")).toBeVisible();
  });

  it("closes with Escape and reports duplicate access", async () => {
    const changed = vi.fn();
    server.use(http.get("/api/users/share-candidates", () => HttpResponse.json([shareCandidate])), http.get("/api/documents/:id/shares", () => HttpResponse.json([])), http.post("/api/documents/:id/shares", () => HttpResponse.json({ code: "duplicate_share", detail: "Conflict" }, { status: 409 })));
    const user = userEvent.setup(); renderWithApp(<ShareDocumentDialog documentId={documentShare.documentId} open onOpenChange={changed} />);
    await user.click(await screen.findByRole("button", { name: /share with chidi/i }));
    expect(await screen.findByRole("alert")).toHaveTextContent("already has access");
    await user.keyboard("{Escape}"); expect(changed).toHaveBeenCalledWith(false);
  });
});
