import { render, screen, waitFor, within } from "@testing-library/react";
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
import { Provider } from "react-redux";
import { MemoryRouter, Routes } from "react-router-dom";
import { setupStore } from "../../app/store";
import { ajaiaApi } from "../../store/api/ajaiaApi";
import { documentListItem } from "../../mocks/fixtures";

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

  it("clears user-scoped API data before switching reviewers", async () => {
    let sessionRequests = 0;
    server.use(
      http.get("/api/documents", () => HttpResponse.json([documentListItem])),
      http.get("/api/session", () => { sessionRequests += 1; return HttpResponse.json(sessionUser); }),
    );
    const store = setupStore();
    const sessionSubscription = store.dispatch(ajaiaApi.endpoints.getSession.initiate());
    await sessionSubscription;
    await store.dispatch(ajaiaApi.endpoints.getDocuments.initiate("all"));
    expect(Object.keys(store.getState().ajaiaApi.queries)).not.toHaveLength(0);
    render(<Provider store={store}><MemoryRouter initialEntries={["/documents"]}><Routes><Route path="/documents" element={<AppShell user={sessionUser} />} /><Route path="/login" element={<h1>Choose a reviewer</h1>} /></Routes></MemoryRouter></Provider>);

    await userEvent.click(screen.getByRole("button", { name: /switch user/i }));

    expect(await screen.findByRole("heading", { name: /choose a reviewer/i })).toBeVisible();
    await waitFor(() => expect(Object.keys(store.getState().ajaiaApi.queries)).toHaveLength(0));
    expect(sessionRequests).toBe(1);
    sessionSubscription.unsubscribe();
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
