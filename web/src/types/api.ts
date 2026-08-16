export type User = {
  id: string;
  displayName: string;
  email: string;
  avatarColor: string;
};

export type ProblemDetails = {
  code?: string;
  detail: string;
  errors?: Record<string, string[]>;
};

export type AntiforgeryResponse = { token: string };
export type UserSummary = User;
export type DocumentListItem = {
  id: string; ownerId: string; title: string; contentFormat: string; plainText: string;
  version: number; updatedAt: string; owner: UserSummary; isOwner: boolean;
};
export type DocumentDetail = DocumentListItem & {
  content: string; createdAt: string; canEdit: boolean; canRename: boolean;
  canShare: boolean; canDelete: boolean;
};
export type ShareCandidate = User;
export type DocumentShare = {
  documentId: string; userId: string; displayName: string; email: string;
  avatarColor: string; createdAt: string;
};
export type ImportedText = {
  title: string; format: string; content: string; plainText: string;
};
