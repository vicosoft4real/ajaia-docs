import { Button } from "../../components/ui/Button";
import { Dialog, DialogContent, DialogDescription, DialogTitle } from "../../components/ui/Dialog";
import { useDeleteDocumentMutation, useLazyGetAntiforgeryQuery } from "../../store/api/ajaiaApi";
import { useState } from "react";

export function DeleteDocumentDialog({ documentId, title, open, onOpenChange, onDeleted }: { documentId: string; title: string; open: boolean; onOpenChange: (open: boolean) => void; onDeleted?: () => void }) {
  const [getAntiforgery] = useLazyGetAntiforgeryQuery(); const [remove, state] = useDeleteDocumentMutation();
  const [requestFailed, setRequestFailed] = useState(false); const failed = requestFailed || state.isError;
  const confirm = async () => { setRequestFailed(false); try { await getAntiforgery().unwrap(); await remove(documentId).unwrap(); onOpenChange(false); onDeleted?.(); } catch { setRequestFailed(true); } };
  return <Dialog open={open} onOpenChange={onOpenChange}><DialogContent><DialogTitle>Delete “{title}”?</DialogTitle><DialogDescription>This removes the document and its shares. This action cannot be undone.</DialogDescription>{failed && <p role="alert" className="mt-5 text-sm font-bold text-danger">The document could not be deleted. Try again.</p>}<div className="mt-7 flex justify-end gap-3"><Button className="border border-border bg-white text-ink" type="button" onClick={() => onOpenChange(false)}>Keep document</Button><Button className="bg-danger" type="button" disabled={state.isLoading} onClick={() => void confirm()}>{state.isLoading ? "Deleting…" : failed ? "Try delete again" : "Delete document"}</Button></div></DialogContent></Dialog>;
}
