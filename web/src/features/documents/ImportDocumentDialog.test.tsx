import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { describe, expect, it, vi } from "vitest";
import { server } from "../../mocks/server";
import { renderWithApp } from "../../test/renderWithApp";
import { ImportDocumentDialog } from "./ImportDocumentDialog";

describe("ImportDocumentDialog", () => {
  it("renders structured server errors and leaves a next action", async () => {
    server.use(http.get("/api/session/antiforgery", () => HttpResponse.json({ token: "token" })), http.post("/api/documents/import", () => HttpResponse.json({ detail: "The document could not be decoded.", errors: { file: ["Save it as UTF-8 text."] } }, { status: 400 })));
    renderWithApp(<ImportDocumentDialog open onOpenChange={vi.fn()} onImported={vi.fn()} />);
    await userEvent.upload(screen.getByLabelText("Document file"), new File(["notes"], "notes.md", { type: "text/markdown" }));
    await userEvent.click(screen.getByRole("button", { name: "Import document" }));
    expect(await screen.findByText("The document could not be decoded. Save it as UTF-8 text.")).toBeVisible();
    expect(screen.getByRole("button", { name: "Try import again" })).toBeVisible();
  });
});
