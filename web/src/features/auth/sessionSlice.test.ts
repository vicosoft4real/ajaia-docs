import { describe, expect, it } from "vitest";
import { sessionUser } from "../../mocks/fixtures";
import sessionReducer, {
  clearSession,
  setAntiforgeryToken,
  setCurrentUser,
} from "./sessionSlice";

describe("sessionSlice", () => {
  it("tracks the signed-in reviewer and antiforgery token independently", () => {
    const withToken = sessionReducer(undefined, setAntiforgeryToken("secure-token"));
    const signedIn = sessionReducer(withToken, setCurrentUser(sessionUser));

    expect(signedIn).toEqual({ antiforgeryToken: "secure-token", user: sessionUser });
    expect(sessionReducer(signedIn, clearSession())).toEqual({
      antiforgeryToken: null,
      user: null,
    });
  });
});
