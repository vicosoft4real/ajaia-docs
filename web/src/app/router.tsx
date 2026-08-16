import { createBrowserRouter, Navigate } from "react-router-dom";
import { DemoLoginPage } from "../features/auth/DemoLoginPage";
import { RequireSession } from "../features/auth/RequireSession";
import { CreateDocumentPage, DocumentLibraryPage } from "../features/documents/DocumentLibraryPage";

export const router = createBrowserRouter([
  { path: "/login", element: <DemoLoginPage /> },
  { element: <RequireSession />, children: [
    { path: "/documents", element: <DocumentLibraryPage /> },
    { path: "/documents/new", element: <CreateDocumentPage /> },
    { path: "/documents/:documentId", element: <h1>Document workspace</h1> },
  ] },
  { path: "*", element: <Navigate replace to="/documents" /> },
]);
