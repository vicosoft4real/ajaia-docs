import { ArrowRight, FileCheck2 } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { OwnershipEdge } from "../../components/ui/OwnershipEdge";
import { useLazyGetAntiforgeryQuery, useStartSessionMutation } from "../../store/api/ajaiaApi";
import { demoUsers } from "./demoUsers";

export function DemoLoginPage() {
  const navigate = useNavigate();
  const [activeUserId, setActiveUserId] = useState<string | null>(null);
  const [error, setError] = useState("");
  const [getAntiforgery] = useLazyGetAntiforgeryQuery();
  const [startSession] = useStartSessionMutation();

  const continueAs = async (userId: string) => {
    setActiveUserId(userId); setError("");
    try {
      await getAntiforgery().unwrap();
      await startSession({ userId }).unwrap();
      navigate("/documents", { replace: true });
    } catch {
      setError("Demo access could not start. Try selecting the reviewer again.");
      setActiveUserId(null);
    }
  };

  return <main className="login-stage">
    <section className="proof-panel" aria-labelledby="login-thesis">
      <div className="brand-mark"><FileCheck2 aria-hidden="true" size={20} /> Ajaia Docs</div>
      <div><p className="proof-label">Review copy · shared workspace</p><h1 id="login-thesis">A clear place to shape the next draft.</h1><p>Read closely, leave the work stronger, and keep ownership visible from first note to final copy.</p></div>
      <OwnershipEdge color="#25A77A" label="Collaboration stays attributed" />
    </section>
    <section className="login-panel" aria-labelledby="demo-access-title">
      <div><p className="eyebrow">Workspace sign in</p><h2 id="demo-access-title">Demo access for reviewers</h2><p className="supporting-copy">Choose a seeded identity to enter the document library.</p></div>
      <div className="reviewer-list">{demoUsers.map((user) => <Card className="reviewer-card" key={user.id}>
        <span className="avatar" style={{ backgroundColor: user.avatarColor }} aria-hidden="true">{user.displayName.split(" ").map((part) => part[0]).join("")}</span>
        <span className="reviewer-details"><strong>{user.displayName}</strong><span>{user.email}</span></span>
        <Button aria-label={`Continue as ${user.displayName}`} disabled={activeUserId !== null} onClick={() => void continueAs(user.id)} type="button">{activeUserId === user.id ? "Opening…" : "Continue"}<ArrowRight aria-hidden="true" size={16} /></Button>
      </Card>)}</div>
      {error && <p className="error-message" role="alert">{error}</p>}
    </section>
  </main>;
}
