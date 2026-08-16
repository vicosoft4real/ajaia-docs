import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { clearSession, setAntiforgeryToken, setCurrentUser, type SessionState } from "../../features/auth/sessionSlice";
import type { AntiforgeryResponse, User } from "../../types/api";

type ApiState = { session: SessionState };

export const ajaiaApi = createApi({
  reducerPath: "ajaiaApi",
  baseQuery: fetchBaseQuery({
    baseUrl: "/api",
    credentials: "include",
    timeout: 30000,
    prepareHeaders: (headers, { getState, type }) => {
      const token = (getState() as ApiState).session.antiforgeryToken;
      if (type === "mutation" && token) headers.set("X-XSRF-TOKEN", token);
      return headers;
    },
  }),
  endpoints: (builder) => ({
    getAntiforgery: builder.query<AntiforgeryResponse, void>({
      query: () => "/session/antiforgery",
      async onQueryStarted(_, { dispatch, queryFulfilled }) {
        const { data } = await queryFulfilled;
        dispatch(setAntiforgeryToken(data.token));
      },
    }),
    getSession: builder.query<User, void>({
      query: () => "/session",
      async onQueryStarted(_, { dispatch, queryFulfilled }) {
        try { dispatch(setCurrentUser((await queryFulfilled).data)); } catch { dispatch(clearSession()); }
      },
    }),
    startSession: builder.mutation<User, { userId: string }>({
      query: (body) => ({ url: "/session", method: "POST", body }),
      async onQueryStarted(_, { dispatch, queryFulfilled }) {
        dispatch(setCurrentUser((await queryFulfilled).data));
      },
    }),
    endSession: builder.mutation<void, void>({
      query: () => ({ url: "/session", method: "DELETE" }),
      async onQueryStarted(_, { dispatch, queryFulfilled }) {
        await queryFulfilled;
        dispatch(clearSession());
      },
    }),
  }),
});

export const { useEndSessionMutation, useGetAntiforgeryQuery, useGetSessionQuery, useLazyGetAntiforgeryQuery, useStartSessionMutation } = ajaiaApi;
