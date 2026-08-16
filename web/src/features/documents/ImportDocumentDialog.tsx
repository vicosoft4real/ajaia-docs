import { FileUp, RotateCcw } from "lucide-react";
import { useRef, useState, type ChangeEvent } from "react";
import { Button } from "../../components/ui/Button";
import { Dialog, DialogContent, DialogDescription, DialogTitle } from "../../components/ui/Dialog";
import { useImportDocumentMutation, useLazyGetAntiforgeryQuery } from "../../store/api/ajaiaApi";
import type { DocumentDetail, ProblemDetails } from "../../types/api";
import { validateImportFile } from "./documentValidation";

export type ImportDocumentDialogProps = { open: boolean; onOpenChange: (open: boolean) => void; onImported: (document: DocumentDetail) => void };

function errorText(error: unknown) {
  const payload = (error as { data?: ProblemDetails })?.data;
  if (!payload) return "The document could not be imported. Check your connection and try again.";
  const fieldErrors = Object.values(payload.errors ?? {}).flat();
  return [payload.detail, ...fieldErrors].filter(Boolean).join(" ");
}

export function ImportDocumentDialog({ open, onOpenChange, onImported }: ImportDocumentDialogProps) {
  const [file, setFile] = useState<File | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const [getAntiforgery] = useLazyGetAntiforgeryQuery();
  const [importDocument, { isLoading }] = useImportDocumentMutation();
  const choose = (event: ChangeEvent<HTMLInputElement>) => { const next = event.target.files?.[0] ?? null; setFile(next); setMessage(next ? validateImportFile(next) : null); };
  const submit = async () => {
    if (!file) return setMessage("Choose a .txt or .md file to import.");
    const validation = validateImportFile(file); if (validation) return setMessage(validation);
    try { await getAntiforgery().unwrap(); const imported = await importDocument(file).unwrap(); setMessage(null); onImported(imported); onOpenChange(false); }
    catch (error) { setMessage(errorText(error)); }
  };
  const retry = () => { setMessage(null); setFile(null); if (inputRef.current) inputRef.current.value = ""; inputRef.current?.focus(); };
  return <Dialog open={open} onOpenChange={onOpenChange}><DialogContent>
    <div className="mb-5 grid h-12 w-12 place-items-center rounded-xl bg-action/10 text-action"><FileUp aria-hidden="true" /></div>
    <DialogTitle>Import a document</DialogTitle><DialogDescription>Bring plain text or Markdown into your library. The file name becomes the document title.</DialogDescription>
    <div className="mt-6"><label className="text-sm font-bold" htmlFor="document-file">Document file</label><input ref={inputRef} id="document-file" type="file" accept=".txt,.md,text/plain,text/markdown" onChange={choose} className="mt-2 block w-full cursor-pointer rounded-xl border border-border bg-paper p-3 text-sm file:mr-4 file:rounded-lg file:border-0 file:bg-ink file:px-3 file:py-2 file:font-bold file:text-white" /><p className="mt-2 text-xs leading-5 text-ink/55">.txt or .md · UTF-8 text · 1 MB maximum</p>{file && !message && <p className="mt-3 text-sm font-bold text-shared">Ready to import {file.name}</p>}</div>
    {message && <div role="alert" className="mt-5 rounded-xl border border-danger/20 bg-danger/5 p-4 text-sm font-semibold leading-6 text-danger">{message}</div>}
    <div className="mt-7 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">{message && <Button type="button" onClick={retry} className="border border-border bg-white text-ink hover:bg-paper"><RotateCcw aria-hidden="true" size={16} />Try import again</Button>}<Button type="button" disabled={isLoading} onClick={() => void submit()}>{isLoading ? "Importing…" : "Import document"}</Button></div>
  </DialogContent></Dialog>;
}
