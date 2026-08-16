import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Button } from "./Button";
import { Card } from "./Card";
import { OwnershipEdge } from "./OwnershipEdge";

describe("UI foundations", () => {
  it("keeps native button semantics", () => {
    render(<Button type="button">Review draft</Button>);
    expect(screen.getByRole("button", { name: "Review draft" })).toHaveAttribute("type", "button");
  });

  it("supports an explicitly labelled document panel", () => {
    render(<Card aria-label="Draft details">Content</Card>);
    expect(screen.getByLabelText("Draft details")).toHaveTextContent("Content");
  });

  it("names ownership without relying on color alone", () => {
    render(<OwnershipEdge color="#365CF5" label="Owned by Amina" />);
    expect(screen.getByText("Owned by Amina")).toBeVisible();
  });
});
