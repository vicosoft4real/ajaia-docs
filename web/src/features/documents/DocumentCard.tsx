import { ArrowUpRight, Clock3, FileText, Trash2 } from "lucide-react";
import { Card } from "../../components/ui/Card";
import { OwnershipEdge } from "../../components/ui/OwnershipEdge";
import type { DocumentSummary } from "../../types/api";

export type DocumentCardProps = { document: DocumentSummary; onOpen: (id: string) => void; onDelete?: (document: DocumentSummary) => void };

function relativeDate(value: string) {
  const date = new Date(value);
  return Number.isNaN(date.valueOf()) ? "Recently updated" : new Intl.DateTimeFormat(undefined, { dateStyle: "medium" }).format(date);
}

export function DocumentCard({ document, onOpen, onDelete }: DocumentCardProps) {
  const ownership = document.isOwner ? { color: "#365CF5", label: "Owned" } : { color: "#25A77A", label: "Shared" };
  const preview = document.plainText.trim() || "This document is ready for its first words.";
  return <Card className="group relative flex min-h-64 flex-col overflow-hidden p-6 shadow-[0_16px_45px_rgba(23,35,60,.05)] transition hover:-translate-y-0.5 hover:shadow-[0_20px_55px_rgba(23,35,60,.1)]">
    <span aria-hidden="true" className="absolute inset-y-0 left-0 w-1" style={{ backgroundColor: ownership.color }} />
    <div className="flex items-start justify-between gap-4"><OwnershipEdge {...ownership} /><FileText aria-hidden="true" className="text-ink/25" size={21} /></div>
    <h2 className="mt-7 line-clamp-2 font-editorial text-xl font-semibold leading-snug tracking-tight">{document.title}</h2>
    <p className="mt-3 line-clamp-3 text-sm leading-6 text-ink/60">{preview}</p>
    <div className="mt-auto flex items-end justify-between gap-3 pt-7"><div className="min-w-0"><p className="truncate text-xs font-bold text-ink/70">{document.isOwner ? "By you" : `By ${document.owner.displayName}`}</p><p className="mt-1 flex items-center gap-1.5 text-xs text-ink/50"><Clock3 aria-hidden="true" size={13} />{relativeDate(document.updatedAt)}</p></div>
      <div className="flex items-center gap-1">{document.isOwner && onDelete && <button type="button" onClick={() => onDelete(document)} aria-label={`Delete ${document.title}`} className="grid min-h-11 min-w-11 place-items-center rounded-full text-ink/45 transition hover:bg-danger/10 hover:text-danger"><Trash2 aria-hidden="true" size={17} /></button>}<button type="button" onClick={() => onOpen(document.id)} aria-label={`Open ${document.title}`} className="grid min-h-11 min-w-11 place-items-center rounded-full border border-border bg-paper text-action transition group-hover:border-action group-hover:bg-action group-hover:text-white"><ArrowUpRight aria-hidden="true" size={18} /></button></div></div>
  </Card>;
}
