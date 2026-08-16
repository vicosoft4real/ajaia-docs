import type { DocumentDetail, DocumentListItem, DocumentShare, ImportedText, ShareCandidate, User } from "../types/api";

export const sessionUser: User = { id: "00000000-0000-0000-0000-000000000001", displayName: "Amina Okafor", email: "amina@example.test", avatarColor: "#365CF5" };
export const documentListItem: DocumentListItem = { id: "10000000-0000-0000-0000-000000000001", ownerId: sessionUser.id, title: "Editorial review brief", contentFormat: "lexical", plainText: "Review notes", version: 1, updatedAt: "2026-08-16T10:00:00Z", owner: sessionUser, isOwner: true };
export const documentDetail: DocumentDetail = { ...documentListItem, content: '{"root":{"children":[]}}', createdAt: "2026-08-16T09:00:00Z", canEdit: true, canRename: true, canShare: true, canDelete: true };
export const shareCandidate: ShareCandidate = { id: "00000000-0000-0000-0000-000000000002", displayName: "Chidi Okeke", email: "chidi@example.test", avatarColor: "#25A77A" };
export const documentShare: DocumentShare = { documentId: documentDetail.id, userId: shareCandidate.id, displayName: shareCandidate.displayName, email: shareCandidate.email, avatarColor: shareCandidate.avatarColor, createdAt: "2026-08-16T10:15:00Z" };
export const importedText: ImportedText = { title: "Imported notes", format: "plainText", content: "Imported notes", plainText: "Imported notes" };
