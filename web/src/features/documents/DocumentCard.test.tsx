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
});
