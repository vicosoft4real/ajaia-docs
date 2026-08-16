import { UserMinus, UserPlus } from "lucide-react";
import { useState } from "react";
import { Button } from "../../components/ui/Button";
import { Dialog, DialogContent, DialogDescription, DialogTitle } from "../../components/ui/Dialog";
import { useGetShareCandidatesQuery, useGetSharesQuery, useGrantShareMutation, useLazyGetAntiforgeryQuery, useRevokeShareMutation } from "../../store/api/ajaiaApi";
import { initials, shareErrorMessage } from "./sharePresentation";

export type ShareDocumentDialogProps = { documentId: string; open: boolean; onOpenChange: (open: boolean) => void };

export function ShareDocumentDialog({ documentId, open, onOpenChange }: ShareDocumentDialogProps) {
  const candidates = useGetShareCandidatesQuery(documentId, { skip: !open });
  const shares = useGetSharesQuery(documentId, { skip: !open });
  const [grant, grantState] = useGrantShareMutation(); const [revoke, revokeState] = useRevokeShareMutation(); const [antiforgery] = useLazyGetAntiforgeryQuery();
  const [notice, setNotice] = useState<{ kind: "status" | "error"; text: string }>();
  const mutate = async (action: () => { unwrap: () => Promise<unknown> }, success: string) => {
    setNotice(undefined);
    try { await antiforgery().unwrap(); await action().unwrap(); setNotice({ kind: "status", text: success }); }
    catch (error) { setNotice({ kind: "error", text: shareErrorMessage(error) }); }
  };
  const existing = new Set((shares.data ?? []).map((share) => share.userId));
  return <Dialog open={open} onOpenChange={onOpenChange}><DialogContent className="max-h-[min(42rem,calc(100vh-2rem))] overflow-y-auto" aria-describedby="share-description">
    <DialogTitle>Share this document</DialogTitle><DialogDescription id="share-description">Invite a seeded reviewer to edit. Only you can rename, share, or delete this document.</DialogDescription>
    {notice && <p role={notice.kind === "error" ? "alert" : "status"} className={`mt-4 rounded-lg px-3 py-2 text-sm font-bold ${notice.kind === "error" ? "bg-red-50 text-danger" : "bg-emerald-50 text-shared"}`}>{notice.text}</p>}
    <section className="mt-6" aria-labelledby="available-reviewers"><h3 id="available-reviewers" className="text-xs font-extrabold uppercase tracking-widest text-ink/55">Available reviewers</h3>
      {candidates.isLoading && <p role="status" className="mt-3 text-sm text-ink/60">Loading reviewers…</p>}
      {candidates.isError && <p role="alert" className="mt-3 text-sm font-bold text-danger">Reviewers could not be loaded.</p>}
      <ul className="mt-2 divide-y divide-border">{(candidates.data ?? []).filter((candidate) => !existing.has(candidate.id)).map((candidate) => <li key={candidate.id} className="flex items-center gap-3 py-3"><span className="avatar avatar-small shrink-0" style={{ backgroundColor: candidate.avatarColor }} aria-hidden="true">{initials(candidate.displayName)}</span><span className="min-w-0 flex-1"><strong className="block truncate text-sm">{candidate.displayName}</strong><span className="block truncate text-xs text-ink/55">{candidate.email}</span></span><Button type="button" className="shrink-0 px-3" disabled={grantState.isLoading} aria-label={`Share with ${candidate.displayName}`} onClick={() => void mutate(() => grant({ documentId, userId: candidate.id }), `${candidate.displayName} has access`)}><UserPlus aria-hidden="true" size={16} />Share</Button></li>)}</ul>
    </section>
    <section className="mt-6" aria-labelledby="people-with-access"><h3 id="people-with-access" className="text-xs font-extrabold uppercase tracking-widest text-ink/55">People with access</h3>
      {!shares.isLoading && (shares.data?.length ?? 0) === 0 && <p className="mt-3 text-sm text-ink/60">Only you have access.</p>}
      <ul className="mt-2 divide-y divide-border">{(shares.data ?? []).map((share) => <li key={share.userId} className="flex items-center gap-3 py-3"><span className="avatar avatar-small shrink-0" style={{ backgroundColor: share.avatarColor }} aria-hidden="true">{initials(share.displayName)}</span><span className="min-w-0 flex-1"><strong className="block truncate text-sm">{share.displayName}</strong><span className="block truncate text-xs text-ink/55">Collaborator · {share.email}</span></span><Button type="button" className="shrink-0 border border-border bg-white px-3 text-danger" disabled={revokeState.isLoading} aria-label={`Remove ${share.displayName}`} onClick={() => void mutate(() => revoke({ documentId, userId: share.userId }), "Access removed")}><UserMinus aria-hidden="true" size={16} />Remove</Button></li>)}</ul>
    </section>
  </DialogContent></Dialog>;
}
