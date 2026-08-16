import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { User } from "../../types/api";

export type SessionState = { user: User | null; antiforgeryToken: string | null };
const initialState: SessionState = { user: null, antiforgeryToken: null };

const sessionSlice = createSlice({
  name: "session",
  initialState,
  reducers: {
    setCurrentUser: (state, action: PayloadAction<User>) => { state.user = action.payload; },
    setAntiforgeryToken: (state, action: PayloadAction<string>) => { state.antiforgeryToken = action.payload; },
    clearSession: () => initialState,
  },
});

export const { clearSession, setAntiforgeryToken, setCurrentUser } = sessionSlice.actions;
export default sessionSlice.reducer;
