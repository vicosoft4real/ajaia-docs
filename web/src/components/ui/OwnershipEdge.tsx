type OwnershipEdgeProps = { color: string; label: string };

export function OwnershipEdge({ color, label }: OwnershipEdgeProps) {
  return <div className="flex items-center gap-2 text-xs font-bold text-ink/70"><span aria-hidden="true" className="h-5 w-1 rounded-full" style={{ backgroundColor: color }} /><span>{label}</span></div>;
}
