import { ArrowLeft, RefreshCw, Share2, Trash2 } from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { Button } from "../../components/ui/Button";
import { EmptyState } from "../../components/ui/EmptyState";
import { Skeleton } from "../../components/ui/Skeleton";
import { DeleteDocumentDialog } from "../documents/DeleteDocumentDialog";
import { useGetDocumentQuery, useLazyGetAntiforgeryQuery, useUpdateDocumentContentMutation, useUpdateDocumentTitleMutation } from "../../store/api/ajaiaApi";
import type { DocumentDetail, ProblemDetails } from "../../types/api";
import { LexicalDocumentEditor } from "./LexicalDocumentEditor";
import { SaveStatus } from "./SaveStatus";
import { DocumentSaveCoordinator, type SaveIntent, type SaveState } from "./saveCoordinator";
import { useUnsavedChangesWarning } from "./useUnsavedChangesWarning";

export function DocumentEditorPage() {
  const { documentId = "" } = useParams(); const navigate = useNavigate();
  const query = useGetDocumentQuery(documentId, { skip: !documentId });
  if (query.isLoading) return <EditorLoading />;
  if (!query.data || query.isError) return <EmptyState title="The document did not load" description="Check your connection or return to your library." action={<Button type="button" onClick={() => void query.refetch()}><RefreshCw size={16} />Try again</Button>} />;
  return <LoadedEditor key={`${query.data.id}:${query.data.version}`} initialDocument={query.data} reload={async () => (await query.refetch()).data} onDeleted={() => navigate("/documents", { replace: true })} />;
}

function LoadedEditor({ initialDocument, reload, onDeleted }: { initialDocument: DocumentDetail; reload: () => Promise<DocumentDetail | undefined>; onDeleted: () => void }) {
  const [document, setDocument] = useState(initialDocument); const [title, setTitle] = useState(initialDocument.title); const [saveState, setSaveState] = useState<SaveState>("saved"); const [dirty, setDirty] = useState(false); const [conflict, setConflict] = useState<ProblemDetails>(); const [editorKey, setEditorKey] = useState(0); const [deleteOpen, setDeleteOpen] = useState(false);
  const [getAntiforgery] = useLazyGetAntiforgeryQuery(); const [updateContent] = useUpdateDocumentContentMutation(); const [updateTitle] = useUpdateDocumentTitleMutation();
  const savedTitle = useRef(initialDocument.title); const initialContent = useRef({ content: initialDocument.content, plainText: initialDocument.plainText }); const coordinatorRef = useRef<DocumentSaveCoordinator>();
  const save = useMemo(() => async (intent: SaveIntent & { expectedVersion: number }) => {
    await getAntiforgery().unwrap();
    if (intent.kind === "title") return updateTitle({ id: document.id, title: intent.title, expectedVersion: intent.expectedVersion }).unwrap();
    return updateContent({ id: document.id, contentFormat: intent.contentFormat, content: intent.content, plainText: intent.plainText, expectedVersion: intent.expectedVersion }).unwrap();
  }, [document.id, getAntiforgery, updateContent, updateTitle]);
  useEffect(() => {
    const coordinator = new DocumentSaveCoordinator({ initialVersion: document.version, save, onStateChange: (state) => { setSaveState(state); setDirty(coordinator.hasUnsavedChanges()); }, onVersionChange: (response) => { setDocument((current) => ({ ...current, ...response })); setDirty(coordinator.hasUnsavedChanges()); }, onConflict: setConflict });
    coordinatorRef.current = coordinator; return () => { void coordinator.flush(); coordinator.dispose(); coordinatorRef.current = undefined; };
  }, [document.id, save]);
  useUnsavedChangesWarning(dirty);
  const commitTitle = () => { const next = title.trim() || "Untitled document"; setTitle(next); if (next !== savedTitle.current) { savedTitle.current = next; coordinatorRef.current?.setTitle(next); setDirty(true); } };
  const reloadSaved = async () => { const fresh = await reload(); if (!fresh) return; coordinatorRef.current?.dispose(); setDocument(fresh); setTitle(fresh.title); savedTitle.current = fresh.title; initialContent.current = { content: fresh.content, plainText: fresh.plainText }; setConflict(undefined); setSaveState("saved"); setDirty(false); setEditorKey((key) => key + 1); };
  return <section aria-labelledby="document-title" className="mx-auto max-w-5xl">
    <div className="mb-6 flex flex-wrap items-center justify-between gap-4"><Link className="inline-flex items-center gap-2 text-sm font-bold text-ink/60 hover:text-ink" to="/documents"><ArrowLeft size={16} />Library</Link><div className="flex items-center gap-3"><SaveStatus state={saveState} />{saveState === "changes-not-saved" && <Button type="button" className="min-h-9 px-3 py-1.5" onClick={() => coordinatorRef.current?.retry()}>Retry save</Button>}</div></div>
    {conflict && <div role="alert" className="mb-6 flex flex-col gap-4 rounded-xl border border-amber-300 bg-amber-50 p-5 sm:flex-row sm:items-center sm:justify-between"><div><p className="font-bold text-ink">A newer saved version is available</p><p className="mt-1 text-sm text-ink/65">Your local draft is still here. Reload the saved version before continuing.</p></div><Button type="button" onClick={() => void reloadSaved()}><RefreshCw size={16} />Reload saved version</Button></div>}
    <header className="mb-7 border-b border-border pb-6"><div className="flex flex-col gap-5 sm:flex-row sm:items-start sm:justify-between"><div className="min-w-0 flex-1"><p className="eyebrow">{document.isOwner ? "Your manuscript" : `Shared by ${document.owner.displayName}`}</p><input id="document-title" aria-label="Document title" readOnly={!document.canRename || saveState === "conflict"} value={title} onChange={(event) => setTitle(event.target.value)} onBlur={commitTitle} onKeyDown={(event) => { if (event.key === "Enter") event.currentTarget.blur(); }} className="mt-2 w-full border-0 bg-transparent p-0 font-[Literata] text-4xl font-semibold tracking-tight text-ink outline-none read-only:cursor-default focus:ring-0 sm:text-5xl" /></div>{document.isOwner && <div className="flex gap-2"><Button type="button" className="border border-border bg-white text-ink"><Share2 size={16} />Share</Button><Button type="button" aria-label="Delete document" className="border border-border bg-white text-danger" onClick={() => setDeleteOpen(true)}><Trash2 size={16} /></Button></div>}</div></header>
    <LexicalDocumentEditor key={editorKey} initialContent={document.content} contentFormat={document.contentFormat} onChange={({ content, plainText }) => { if (content === initialContent.current.content && plainText === initialContent.current.plainText) return; coordinatorRef.current?.setContent(content, plainText); setDirty(true); }} />
    {deleteOpen && <DeleteDocumentDialog documentId={document.id} title={title} open onOpenChange={setDeleteOpen} onDeleted={onDeleted} />}
  </section>;
}

function EditorLoading() { return <div role="status" aria-label="Loading document" className="mx-auto max-w-5xl"><Skeleton className="h-12 w-2/3" /><Skeleton className="mt-8 h-96 w-full" /></div>; }
