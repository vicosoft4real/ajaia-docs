import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { DeleteDocumentDialog } from "./DeleteDocumentDialog";

const api = vi.hoisted(() => ({ antiforgery: vi.fn(), remove: vi.fn() }));
vi.mock("../../store/api/ajaiaApi", () => ({
  useLazyGetAntiforgeryQuery: () => [api.antiforgery],
  useDeleteDocumentMutation: () => [api.remove, { isLoading: false, isError: false }],
}));

describe("DeleteDocumentDialog", () => {
  beforeEach(() => {
    api.antiforgery.mockReturnValue({ unwrap: () => Promise.reject(new Error("unavailable")) });
    api.remove.mockReturnValue({ unwrap: () => Promise.resolve() });
  });

  it("offers retry when antiforgery retrieval fails", async () => {
    render(<DeleteDocumentDialog documentId="document-1" title="Review brief" open onOpenChange={vi.fn()} />);
    await userEvent.click(screen.getByRole("button", { name: "Delete document" }));
    expect(await screen.findByRole("alert")).toHaveTextContent("The document could not be deleted. Try again.");
    expect(screen.getByRole("button", { name: "Try delete again" })).toBeVisible();
  });
});
