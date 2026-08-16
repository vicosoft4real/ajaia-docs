import { render } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { LexicalDocumentEditor, type EditorChange, type LexicalDocumentEditorProps } from "../LexicalDocumentEditor";

export function renderEditor(initialContent: string, contentFormat: LexicalDocumentEditorProps["contentFormat"]) {
  if (!Range.prototype.getBoundingClientRect) {
    Range.prototype.getBoundingClientRect = () => new DOMRect();
  }
  const received: EditorChange[] = [];
  const view = render(<LexicalDocumentEditor initialContent={initialContent} contentFormat={contentFormat} onChange={(change) => received.push(change)} />);
  return { ...view, user: userEvent.setup(), changes: () => received, latestChange: () => { const value = received.at(-1); if (!value) throw new Error("No editor change received"); return value; } };
}
