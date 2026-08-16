export type SaveState="saved"|"saving"|"changes-not-saved"|"conflict";
const labels:Record<SaveState,string>={saved:"Saved",saving:"Saving…","changes-not-saved":"Changes not saved",conflict:"Resolve conflict"};
export function SaveStatus({state}: {state:SaveState}) {return <span role="status" aria-live="polite" className="inline-flex items-center gap-2 font-[Manrope] text-sm font-bold text-slate-600"><span aria-hidden className={`h-2 w-2 rounded-full ${state==="saved"?"bg-emerald-500":state==="saving"?"animate-pulse bg-blue-500":"bg-amber-500"}`}/>{labels[state]}</span>}
