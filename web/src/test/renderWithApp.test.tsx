import { screen } from "@testing-library/react";
import { Route } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { renderWithApp } from "./renderWithApp";

describe("renderWithApp", () => {
  it("supplies a real store and memory router", () => {
    renderWithApp(<h1>Document detail</h1>, {
      initialEntry: "/documents/draft-1",
      routePath: "/documents/:documentId",
      extraRoutes: <Route path="/unused" element={<div>Unused</div>} />,
    });

    expect(screen.getByRole("heading", { name: /document detail/i })).toBeVisible();
  });
});
