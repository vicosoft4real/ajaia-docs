import { HttpResponse, http } from "msw";
import { demoUsers } from "../features/auth/demoUsers";
import { sessionUser } from "./fixtures";

export const handlers = [
  http.get("/api/session/antiforgery", () => HttpResponse.json({ token: "test-antiforgery-token" })),
  http.get("/api/session", () => HttpResponse.json(sessionUser)),
  http.post("/api/session", async ({ request }) => {
    const body = await request.json() as { userId?: string };
    const user = demoUsers.find((candidate) => candidate.id === body.userId);
    return user ? HttpResponse.json(user) : HttpResponse.json({ code: "not_found", detail: "Reviewer not found." }, { status: 404 });
  }),
  http.delete("/api/session", () => new HttpResponse(null, { status: 204 })),
];
