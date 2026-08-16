import { FilePlus2, FileUp, RefreshCw } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "../../components/ui/Button";
import { EmptyState } from "../../components/ui/EmptyState";
import { Skeleton } from "../../components/ui/Skeleton";
import { useCreateDocumentMutation, useGetDocumentsQuery, useLazyGetAntiforgeryQuery } from "../../store/api/ajaiaApi";
import { DocumentCard } from "./DocumentCard";
import { ImportDocumentDialog } from "./ImportDocumentDialog";
import { DeleteDocumentDialog } from "./DeleteDocumentDialog";
import type { DocumentSummary } from "../../types/api";

const scopes = [{ value: "all", label: "All" }, { value: "owned", label: "Owned by me" }, { value: "shared", label: "Shared with me" }] as const;
type Scope = typeof scopes[number]["value"];

export function DocumentLibraryPage() {
  const navigate = useNavigate(); const [scope, setScope] = useState<Scope>("all"); const [importOpen, setImportOpen] = useState(false); const [pendingDelete, setPendingDelete] = useState<DocumentSummary | null>(null);
  const { data = [], isLoading, isFetching, isError, refetch } = useGetDocumentsQuery(scope);
  const [getAntiforgery] = useLazyGetAntiforgeryQuery(); const [create, createState] = useCreateDocumentMutation();
  const createDocument = async () => { try { await getAntiforgery().unwrap(); const document = await create({ title: "Untitled document" }).unwrap(); navigate(`/documents/${document.id}`); } catch { /* State below exposes retry. */ } };
  const emptyCopy = scope === "shared" ? ["Nothing shared with you yet", "Documents others share with you will appear here."] : scope === "owned" ? ["No documents of your own", "Create a document to start writing and invite reviewers."] : ["Your library is ready", "Create a document or import existing notes to begin."];
  return <section aria-labelledby="library-title">
    <div className="flex flex-col gap-6 border-b border-border pb-8 lg:flex-row lg:items-end lg:justify-between"><div><p className="eyebrow">Document library</p><h1 id="library-title" className="mb-0 mt-3">Work in progress</h1><p className="mt-4 max-w-xl leading-7 text-ink/60">Draft, review, and share the writing that is still taking shape.</p></div><div className="flex flex-col gap-3 sm:flex-row"><Button type="button" className="border border-border bg-white text-ink" onClick={() => setImportOpen(true)}><FileUp aria-hidden="true" size={17} />Import</Button><Button type="button" disabled={createState.isLoading} onClick={() => void createDocument()}><FilePlus2 aria-hidden="true" size={17} />{createState.isLoading ? "Creating…" : createState.isError ? "Try create again" : "New document"}</Button></div></div>
    <div className="mt-8 flex items-center justify-between gap-4 overflow-x-auto"><div role="tablist" aria-label="Filter documents" className="flex min-w-max gap-1 rounded-xl border border-border bg-white p-1">{scopes.map((item) => <button key={item.value} type="button" role="tab" aria-selected={scope === item.value} onClick={() => setScope(item.value)} className={`min-h-11 rounded-lg px-4 text-sm font-bold transition ${scope === item.value ? "bg-ink text-white" : "text-ink/60 hover:bg-paper hover:text-ink"}`}>{item.label}</button>)}</div>{isFetching && !isLoading && <span className="text-xs font-bold text-ink/50" role="status">Updating…</span>}</div>
    {isLoading ? <div role="status" aria-label="Loading documents" className="mt-7 grid gap-5 sm:grid-cols-2 xl:grid-cols-3">{[0,1,2].map((item) => <div key={item} className="h-64 rounded-xl border border-border bg-white p-6"><Skeleton className="h-5 w-20" /><Skeleton className="mt-8 h-7 w-4/5" /><Skeleton className="mt-4 h-4 w-full" /><Skeleton className="mt-2 h-4 w-2/3" /></div>)}</div> : isError ? <div className="mt-7"><EmptyState title="The library did not load" description="Check your connection, then try loading your documents again." action={<Button type="button" onClick={() => void refetch()}><RefreshCw aria-hidden="true" size={16} />Try again</Button>} /></div> : <div className="mt-7 grid gap-5 sm:grid-cols-2 xl:grid-cols-3">{data.length ? data.map((document) => <DocumentCard key={document.id} document={document} onOpen={(id) => navigate(`/documents/${id}`)} onDelete={setPendingDelete} />) : <EmptyState title={emptyCopy[0]} description={emptyCopy[1]} action={scope !== "shared" ? <Button type="button" onClick={() => void createDocument()}>Create a document</Button> : undefined} />}</div>}
    <ImportDocumentDialog open={importOpen} onOpenChange={setImportOpen} onImported={(document) => navigate(`/documents/${document.id}`)} />
    {pendingDelete && <DeleteDocumentDialog documentId={pendingDelete.id} title={pendingDelete.title} open onOpenChange={(open) => { if (!open) setPendingDelete(null); }} onDeleted={() => void refetch()} />}
  </section>;
}

export function CreateDocumentPage() {
  const navigate = useNavigate(); const [getAntiforgery] = useLazyGetAntiforgeryQuery(); const [create, state] = useCreateDocumentMutation();
  const start = async () => { try { await getAntiforgery().unwrap(); const document = await create({ title: "Untitled document" }).unwrap(); navigate(`/documents/${document.id}`, { replace: true }); } catch { /* retry remains */ } };
  return <EmptyState title="Start a new document" description="Create a blank page, then shape it with your reviewers." action={<Button type="button" disabled={state.isLoading} onClick={() => void start()}>{state.isLoading ? "Creating…" : state.isError ? "Try again" : "Create document"}</Button>} />;
}
