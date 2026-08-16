import { $convertFromMarkdownString, TRANSFORMERS } from "@lexical/markdown";
import { $createParagraphNode, $createTextNode, $getRoot, type LexicalEditor } from "lexical";

export type ContentFormat = "lexical" | "markdown" | "plainText";
export function initializeEditorContent(editor: LexicalEditor, content: string, format: ContentFormat) {
  if (format === "lexical" && content) { editor.setEditorState(editor.parseEditorState(content)); return; }
  const root = $getRoot(); root.clear();
  if (format === "markdown") $convertFromMarkdownString(content, TRANSFORMERS);
  else for (const block of content.split(/\n{2,}/)) root.append($createParagraphNode().append($createTextNode(block)));
}
