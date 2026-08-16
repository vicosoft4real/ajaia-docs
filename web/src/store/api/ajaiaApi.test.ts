import { configureStore } from "@reduxjs/toolkit";
import { HttpResponse, http } from "msw";
import { describe, expect, it } from "vitest";
import sessionReducer from "../../features/auth/sessionSlice";
import { sessionUser } from "../../mocks/fixtures";
import { server } from "../../mocks/server";
import { ajaiaApi } from "./ajaiaApi";

describe("ajaiaApi session contract", () => {
  it("stores antiforgery and sends it on mutations", async () => {
    let mutationHeader: string | null = null;
    server.use(
      http.post("/api/session", ({ request }) => {
        mutationHeader = request.headers.get("X-XSRF-TOKEN");
        return HttpResponse.json(sessionUser);
      }),
    );
    const store = configureStore({
      reducer: { [ajaiaApi.reducerPath]: ajaiaApi.reducer, session: sessionReducer },
      middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(ajaiaApi.middleware),
    });

    await store.dispatch(ajaiaApi.endpoints.getAntiforgery.initiate()).unwrap();
    await store.dispatch(ajaiaApi.endpoints.startSession.initiate({ userId: sessionUser.id })).unwrap();

    expect(store.getState().session.antiforgeryToken).toBe("test-antiforgery-token");
    expect(mutationHeader).toBe("test-antiforgery-token");
  });
});
