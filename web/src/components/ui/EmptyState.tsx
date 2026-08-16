import type { ReactNode } from "react";

export function EmptyState({ title, description, action }: { title: string; description: string; action?: ReactNode }) {
  return <section className="col-span-full rounded-2xl border border-dashed border-border bg-surface px-6 py-16 text-center" aria-live="polite">
    <div className="mx-auto max-w-md"><h2 className="font-editorial text-2xl font-semibold">{title}</h2><p className="mt-3 text-sm leading-6 text-ink/65">{description}</p>{action && <div className="mt-6">{action}</div>}</div>
  </section>;
}
