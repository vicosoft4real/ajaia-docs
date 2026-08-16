import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { documentListItem } from "../../mocks/fixtures";
import { DocumentCard } from "./DocumentCard";

describe("DocumentCard", () => {
  it("labels ownership in text and opens the document", () => {
    const onOpen = vi.fn();
    render(<DocumentCard document={documentListItem} onOpen={onOpen} />);
    expect(screen.getByText("Owned")).toBeVisible();
    fireEvent.click(screen.getByRole("button", { name: /Open Editorial review brief/ }));
    expect(onOpen).toHaveBeenCalledWith(documentListItem.id);
  });

  it("offers deletion only for owned documents", () => {
    const onDelete = vi.fn();
    const { rerender } = render(<DocumentCard document={documentListItem} onOpen={vi.fn()} onDelete={onDelete} />);
    fireEvent.click(screen.getByRole("button", { name: `Delete ${documentListItem.title}` }));
    expect(onDelete).toHaveBeenCalledWith(documentListItem);
    rerender(<DocumentCard document={{ ...documentListItem, isOwner: false }} onOpen={vi.fn()} onDelete={onDelete} />);
    expect(screen.queryByRole("button", { name: `Delete ${documentListItem.title}` })).not.toBeInTheDocument();
  });
});
