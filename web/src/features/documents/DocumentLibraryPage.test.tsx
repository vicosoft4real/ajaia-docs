import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { describe, expect, it } from "vitest";
import { documentListItem } from "../../mocks/fixtures";
import { server } from "../../mocks/server";
import { renderWithApp } from "../../test/renderWithApp";
import { DocumentLibraryPage } from "./DocumentLibraryPage";

describe("DocumentLibraryPage", () => {
  it("labels owned and shared cards in color-independent markup", async () => {
    server.use(http.get("/api/documents", ({ request }) => HttpResponse.json(new URL(request.url).searchParams.get("scope") === "shared" ? [{ ...documentListItem, id: "shared", isOwner: false }] : [documentListItem])));
    renderWithApp(<DocumentLibraryPage />);
    expect(await screen.findByText("Owned")).toBeVisible();
    await userEvent.click(screen.getByRole("tab", { name: "Shared with me" }));
    expect(await screen.findByText("Shared")).toBeVisible();
  });
});
