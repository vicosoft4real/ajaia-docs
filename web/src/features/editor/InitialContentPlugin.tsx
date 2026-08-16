import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import { $convertFromMarkdownString, TRANSFORMERS } from "@lexical/markdown";
import { $createParagraphNode, $createTextNode, $getRoot, type LexicalEditor } from "lexical";
import { useLayoutEffect, useRef } from "react";

export type ContentFormat = "lexical" | "markdown" | "plainText";
export function initializeEditorContent(editor: LexicalEditor, content: string, format: ContentFormat) {
  if (format === "lexical" && content) { editor.setEditorState(editor.parseEditorState(content)); return; }
  const root = $getRoot(); root.clear();
  if (format === "markdown") $convertFromMarkdownString(content, TRANSFORMERS);
  else for (const block of content.split(/\n{2,}/)) root.append($createParagraphNode().append($createTextNode(block)));
}
export function InitialContentPlugin({ content, format }: { content: string; format: ContentFormat }) {
  const [editor] = useLexicalComposerContext();
  const initialized = useRef(false);
  useLayoutEffect(() => {
    if (initialized.current) return;
    initialized.current = true;
    editor.update(() => initializeEditorContent(editor, content, format), { tag: "initial-content" });
  }, [content, editor, format]);
  return null;
}
