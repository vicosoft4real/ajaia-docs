import { FileText, Library, LogOut, Plus } from "lucide-react";
import { Link, NavLink, Outlet, useNavigate } from "react-router-dom";
import type { User } from "../../types/api";
import { Button } from "../ui/Button";
import { useEndSessionMutation, useLazyGetAntiforgeryQuery } from "../../store/api/ajaiaApi";

export function AppShell({ user }: { user: User }) {
  const navigate = useNavigate();
  const [getAntiforgery] = useLazyGetAntiforgeryQuery();
  const [endSession, { isLoading }] = useEndSessionMutation();
  const switchUser = async () => { await getAntiforgery().unwrap(); await endSession().unwrap(); navigate("/login", { replace: true }); };
  return <div className="app-frame"><header className="app-header">
    <Link aria-label="Ajaia Docs home" className="app-logo" to="/documents"><FileText aria-hidden="true" size={20} />Ajaia Docs</Link>
    <nav aria-label="Workspace"><NavLink to="/documents"><Library aria-hidden="true" size={17} />Documents</NavLink><NavLink to="/documents/new"><Plus aria-hidden="true" size={17} />New document</NavLink></nav>
    <div className="session-control"><span className="avatar avatar-small" style={{ backgroundColor: user.avatarColor }} aria-hidden="true">{user.displayName.split(" ").map((part) => part[0]).join("")}</span><span className="session-name">{user.displayName}</span><Button className="switch-button" disabled={isLoading} onClick={() => void switchUser()} type="button"><LogOut aria-hidden="true" size={16} />Switch user</Button></div>
  </header><main className="workspace"><Outlet /></main></div>;
}
