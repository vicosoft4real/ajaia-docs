import { Navigate } from "react-router-dom";
import { AppShell } from "../../components/layout/AppShell";
import { useGetSessionQuery } from "../../store/api/ajaiaApi";

export function RequireSession() {
  const { data, error, isLoading, refetch } = useGetSessionQuery();
  if (isLoading) return <div aria-label="Checking reviewer session" className="session-skeleton" role="status"><span /><span /><span /></div>;
  if (error && "status" in error && error.status === 401) return <Navigate replace to="/login" />;
  if (!data) return <main className="session-error"><h1>Session check failed</h1><p>The document workspace could not verify this reviewer.</p><button onClick={() => void refetch()} type="button">Try again</button></main>;
  return <AppShell user={data} />;
}
