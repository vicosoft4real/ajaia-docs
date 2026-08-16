import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { HttpResponse, http } from "msw";
import { Route } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { DemoLoginPage } from "../../features/auth/DemoLoginPage";
import { demoUsers } from "../../features/auth/demoUsers";
import { RequireSession } from "../../features/auth/RequireSession";
import { sessionUser } from "../../mocks/fixtures";
import { server } from "../../mocks/server";
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

  it("shows the newly selected reviewer after switching users in the same store", async () => {
    const user = userEvent.setup();
    let activeUser = null as (typeof demoUsers)[number] | null;
    server.use(
      http.get("/api/session", () => activeUser
        ? HttpResponse.json(activeUser)
        : HttpResponse.json({ code: "unauthenticated", detail: "Session required." }, { status: 401 })),
      http.post("/api/session", async ({ request }) => {
        const { userId } = await request.json() as { userId: string };
        activeUser = demoUsers.find((candidate) => candidate.id === userId) ?? null;
        return HttpResponse.json(activeUser);
      }),
      http.delete("/api/session", () => {
        activeUser = null;
        return new HttpResponse(null, { status: 204 });
      }),
    );
    renderWithApp(<DemoLoginPage />, {
      initialEntry: "/login",
      routePath: "/login",
      extraRoutes: <Route path="/documents" element={<RequireSession />} />,
    });

    await user.click(screen.getByRole("button", { name: /continue as amina okafor/i }));
    expect(within(await screen.findByRole("banner")).getByText("Amina Okafor")).toBeVisible();
    await user.click(screen.getByRole("button", { name: /switch user/i }));
    await user.click(await screen.findByRole("button", { name: /continue as chidi okeke/i }));

    expect(within(await screen.findByRole("banner")).getByText("Chidi Okeke")).toBeVisible();
  });
});
