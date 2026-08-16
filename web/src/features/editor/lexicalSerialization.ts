import { $getRoot, type EditorState } from "lexical";

export const serializeEditorState = (state: EditorState) => JSON.stringify(state.toJSON());
export const getPlainText = (state: EditorState) => state.read(() => $getRoot().getTextContent());
export const serializeLexicalState = serializeEditorState;
export const getEditorPlainText = getPlainText;
