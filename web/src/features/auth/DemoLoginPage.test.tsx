import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Route } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { renderWithApp } from "../../test/renderWithApp";
import { DemoLoginPage } from "./DemoLoginPage";

describe("DemoLoginPage", () => {
  it("creates a reviewer session and enters the document library", async () => {
    const user = userEvent.setup();
    renderWithApp(<DemoLoginPage />, {
      initialEntry: "/login",
      routePath: "/login",
      extraRoutes: <Route path="/documents" element={<h1>Your documents</h1>} />,
    });

    expect(screen.getByText("Demo access for reviewers")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: /continue as amina okafor/i }));

    expect(await screen.findByRole("heading", { name: /your documents/i })).toBeInTheDocument();
  });

  it("offers the three seeded reviewer identities", () => {
    renderWithApp(<DemoLoginPage />, { routePath: "/login" });

    expect(screen.getByRole("button", { name: /continue as amina okafor/i })).toBeVisible();
    expect(screen.getByRole("button", { name: /continue as chidi okeke/i })).toBeVisible();
    expect(screen.getByRole("button", { name: /continue as tayo bello/i })).toBeVisible();
  });
});
