import type { ProblemDetails } from "../../types/api";

export function shareErrorMessage(error: unknown): string {
  const problem = (error as { data?: ProblemDetails })?.data;
  switch (problem?.code) {
    case "duplicate_share": return "This reviewer already has access.";
    case "self_share":
    case "owner_cannot_be_collaborator": return "The document owner already has access.";
    case "user_not_found": return "That reviewer could not be found.";
    case "share_not_found": return "That access was already removed.";
    default: return problem?.detail || "Sharing could not be updated. Try again.";
  }
}

export function initials(displayName: string): string {
  return displayName.split(/\s+/).filter(Boolean).slice(0, 2).map((part) => part[0]).join("").toUpperCase();
}
