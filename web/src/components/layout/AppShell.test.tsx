import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Route } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { sessionUser } from "../../mocks/fixtures";
import { renderWithApp } from "../../test/renderWithApp";
import { AppShell } from "./AppShell";

describe("AppShell", () => {
  it("provides an accessible responsive workspace frame", () => {
    renderWithApp(<AppShell user={sessionUser} />);

    expect(screen.getByRole("link", { name: /ajaia docs home/i })).toBeVisible();
    expect(screen.getByRole("navigation", { name: /workspace/i })).toBeVisible();
    expect(screen.getByText(sessionUser.displayName)).toBeVisible();
    expect(screen.getByRole("button", { name: /switch user/i })).toBeVisible();
  });

  it("ends the cookie session before returning to login", async () => {
    const user = userEvent.setup();
    renderWithApp(<AppShell user={sessionUser} />, {
      initialEntry: "/documents",
      routePath: "/documents",
      extraRoutes: <Route path="/login" element={<h1>Choose a reviewer</h1>} />,
    });

    await user.click(screen.getByRole("button", { name: /switch user/i }));

    expect(await screen.findByRole("heading", { name: /choose a reviewer/i })).toBeVisible();
  });
});
