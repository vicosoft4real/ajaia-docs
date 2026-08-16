import { FileText, Library, LogOut, Menu, Plus } from "lucide-react";
import { NavLink } from "react-router-dom";
import type { User } from "../../types/api";
import { Button } from "../ui/Button";
import { Dialog, DialogContent, DialogDescription, DialogTitle, DialogTrigger } from "../ui/Dialog";

export function MobileNavDrawer({ user, switching, onSwitchUser }: { user: User; switching: boolean; onSwitchUser: () => void }) {
  return <div className="mobile-nav"><Dialog><DialogTrigger asChild><Button type="button" aria-label="Open navigation" className="border border-border bg-white px-3 text-ink"><Menu aria-hidden="true" size={20} />Menu</Button></DialogTrigger><DialogContent className="left-auto right-0 top-0 flex h-dvh w-[min(22rem,calc(100%-2rem))] max-w-none translate-x-0 translate-y-0 flex-col rounded-none rounded-l-2xl p-6">
    <DialogTitle className="flex items-center gap-2"><FileText aria-hidden="true" size={20} />Ajaia Docs</DialogTitle><DialogDescription>Workspace navigation for {user.displayName}</DialogDescription>
    <nav aria-label="Mobile workspace" className="mt-8 grid gap-2"><NavLink className="mobile-drawer-link" to="/documents"><Library aria-hidden="true" size={19} />Documents</NavLink><NavLink className="mobile-drawer-link" to="/documents/new"><Plus aria-hidden="true" size={19} />New document</NavLink></nav>
    <div className="mt-auto border-t border-border pt-5"><p className="text-sm font-bold">Signed in as {user.displayName}</p><Button className="mt-3 w-full" disabled={switching} onClick={onSwitchUser} type="button"><LogOut aria-hidden="true" size={16} />Switch user</Button></div>
  </DialogContent></Dialog></div>;
}
