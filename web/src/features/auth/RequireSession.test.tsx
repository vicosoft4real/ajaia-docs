import { screen } from "@testing-library/react";
import { HttpResponse, delay, http } from "msw";
import { Route } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { sessionUser } from "../../mocks/fixtures";
import { server } from "../../mocks/server";
import { renderWithApp } from "../../test/renderWithApp";
import { RequireSession } from "./RequireSession";

describe("RequireSession", () => {
  it("shows a document-shaped loading skeleton while checking the cookie session", () => {
    server.use(
      http.get("/api/session", async () => {
        await delay("infinite");
        return HttpResponse.json(sessionUser);
      }),
    );

    renderWithApp(<RequireSession />, {
      initialEntry: "/documents",
      routePath: "/documents",
    });

    expect(screen.getByRole("status", { name: /checking reviewer session/i })).toBeVisible();
  });

  it("redirects an unauthenticated reviewer to demo access", async () => {
    server.use(
      http.get("/api/session", () =>
        HttpResponse.json(
          { code: "unauthenticated", detail: "An authenticated session is required." },
          { status: 401 },
        ),
      ),
    );

    renderWithApp(<RequireSession />, {
      initialEntry: "/documents",
      routePath: "/documents",
      extraRoutes: <Route path="/login" element={<h1>Demo access</h1>} />,
    });

    expect(await screen.findByRole("heading", { name: /demo access/i })).toBeVisible();
  });

  it("renders the application shell for a valid session", async () => {
    renderWithApp(<RequireSession />, {
      initialEntry: "/documents",
      routePath: "/documents",
    });

    expect(await screen.findByRole("banner")).toBeVisible();
    expect(screen.getByText(sessionUser.displayName)).toBeVisible();
  });
});
