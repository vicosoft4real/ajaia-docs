import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import { useLayoutEffect, useRef } from "react";
import { initializeEditorContent, type ContentFormat } from "./editorInitialization";

export type { ContentFormat } from "./editorInitialization";
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
