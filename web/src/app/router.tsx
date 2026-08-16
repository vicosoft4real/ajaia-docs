import { createBrowserRouter, Navigate } from "react-router-dom";
import { DemoLoginPage } from "../features/auth/DemoLoginPage";
import { RequireSession } from "../features/auth/RequireSession";

export const router = createBrowserRouter([
  { path: "/login", element: <DemoLoginPage /> },
  { element: <RequireSession />, children: [
    { path: "/documents", element: <section><p className="eyebrow">Document library</p><h1>Your documents</h1></section> },
    { path: "/documents/new", element: <h1>New document</h1> },
    { path: "/documents/:documentId", element: <h1>Document workspace</h1> },
  ] },
  { path: "*", element: <Navigate replace to="/documents" /> },
]);
