import { createEditor, $createParagraphNode, $createTextNode, $getRoot } from "lexical";
import { expect, it } from "vitest";
import { getPlainText, serializeEditorState } from "./lexicalSerialization";

it("serializes Lexical JSON and extracts plain text", () => {
  const editor = createEditor();
  editor.update(() => $getRoot().append($createParagraphNode().append($createTextNode("Ajaia"))), { discrete: true });
  const state = editor.getEditorState();
  expect(JSON.parse(serializeEditorState(state)).root.type).toBe("root");
  expect(getPlainText(state)).toBe("Ajaia");
});
