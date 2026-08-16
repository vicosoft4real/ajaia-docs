import { createBrowserRouter, Navigate } from "react-router-dom";
import { DemoLoginPage } from "../features/auth/DemoLoginPage";
import { RequireSession } from "../features/auth/RequireSession";
import { CreateDocumentPage, DocumentLibraryPage } from "../features/documents/DocumentLibraryPage";
import { DocumentEditorPage } from "../features/editor/DocumentEditorPage";

export const router = createBrowserRouter([
  { path: "/login", element: <DemoLoginPage /> },
  { element: <RequireSession />, children: [
    { path: "/documents", element: <DocumentLibraryPage /> },
    { path: "/documents/new", element: <CreateDocumentPage /> },
    { path: "/documents/:documentId", element: <DocumentEditorPage /> },
  ] },
  { path: "*", element: <Navigate replace to="/documents" /> },
]);
