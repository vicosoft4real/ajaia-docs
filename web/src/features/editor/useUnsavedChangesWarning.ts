import { useEffect } from "react";
export function useUnsavedChangesWarning(hasUnsavedChanges: boolean) { useEffect(() => { if (!hasUnsavedChanges) return; const warn = (event: Event) => { event.preventDefault(); (event as BeforeUnloadEvent).returnValue = ""; }; window.addEventListener("beforeunload", warn); return () => window.removeEventListener("beforeunload", warn); }, [hasUnsavedChanges]); }
