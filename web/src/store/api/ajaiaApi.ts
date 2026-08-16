import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { clearSession, setAntiforgeryToken, setCurrentUser, type SessionState } from "../../features/auth/sessionSlice";
import type { AntiforgeryResponse, DocumentDetail, DocumentSummary, User } from "../../types/api";

type ApiState = { session: SessionState };

export const ajaiaApi = createApi({
  reducerPath: "ajaiaApi",
  tagTypes: ["Documents", "Document"],
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
    getDocuments: builder.query<DocumentSummary[], "all" | "owned" | "shared">({
      query: (scope) => ({ url: "/documents", params: { scope } }),
      providesTags: (result) => result
        ? [{ type: "Documents" as const, id: "LIST" }, ...result.map(({ id }) => ({ type: "Document" as const, id }))]
        : [{ type: "Documents", id: "LIST" }],
    }),
    createDocument: builder.mutation<DocumentDetail, { title?: string }>({
      query: (body) => ({ url: "/documents", method: "POST", body }),
      invalidatesTags: [{ type: "Documents", id: "LIST" }],
    }),
    importDocument: builder.mutation<DocumentDetail, File>({
      query: (file) => { const body = new FormData(); body.append("file", file); return { url: "/documents/import", method: "POST", body }; },
      invalidatesTags: [{ type: "Documents", id: "LIST" }],
    }),
    deleteDocument: builder.mutation<void, string>({
      query: (id) => ({ url: `/documents/${id}`, method: "DELETE" }),
      invalidatesTags: (_result, _error, id) => [{ type: "Documents", id: "LIST" }, { type: "Document", id }],
    }),
  }),
});

export const { useCreateDocumentMutation, useDeleteDocumentMutation, useEndSessionMutation, useGetAntiforgeryQuery, useGetDocumentsQuery, useGetSessionQuery, useImportDocumentMutation, useLazyGetAntiforgeryQuery, useStartSessionMutation } = ajaiaApi;
