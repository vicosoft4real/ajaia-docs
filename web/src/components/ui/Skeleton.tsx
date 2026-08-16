import type { HTMLAttributes } from "react";
import { twMerge } from "tailwind-merge";

export function Skeleton({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div aria-hidden="true" className={twMerge("animate-pulse rounded-lg bg-border/70", className)} {...props} />;
}
