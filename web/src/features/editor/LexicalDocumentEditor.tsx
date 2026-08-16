import { LexicalComposer } from "@lexical/react/LexicalComposer";
import { ContentEditable } from "@lexical/react/LexicalContentEditable";
import { HistoryPlugin } from "@lexical/react/LexicalHistoryPlugin";
import { ListPlugin } from "@lexical/react/LexicalListPlugin";
import { OnChangePlugin } from "@lexical/react/LexicalOnChangePlugin";
import { RichTextPlugin } from "@lexical/react/LexicalRichTextPlugin";
import { ListItemNode, ListNode } from "@lexical/list";
import { HeadingNode, QuoteNode } from "@lexical/rich-text";
import { DocumentToolbar } from "./DocumentToolbar";
import { initializeEditorContent, type ContentFormat } from "./editorInitialization";
import { getPlainText, serializeEditorState } from "./lexicalSerialization";
import "./editor.css";

export type EditorChange={content:string;plainText:string};
export type LexicalDocumentEditorProps={initialContent:string;contentFormat:ContentFormat;onChange:(change:EditorChange)=>void};
export function LexicalDocumentEditor({initialContent,contentFormat,onChange}:LexicalDocumentEditorProps){return <LexicalComposer initialConfig={{namespace:"ajaia-document",nodes:[HeadingNode,QuoteNode,ListNode,ListItemNode],editorState:(editor)=>initializeEditorContent(editor,initialContent,contentFormat),onError(error){throw error},theme:{text:{underline:"editor-underline"},heading:{h1:"font-[Literata] text-4xl font-semibold",h2:"font-[Literata] text-2xl font-semibold"},list:{ol:"list-decimal pl-7",ul:"list-disc pl-7"}}}}><section className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm"><DocumentToolbar/><div className="relative"><RichTextPlugin contentEditable={<ContentEditable aria-label="Document content" className="min-h-80 px-8 py-7 font-[Literata] text-lg leading-8 text-slate-900 outline-none focus-visible:ring-2 focus-visible:ring-inset"/>} placeholder={<div className="pointer-events-none absolute left-8 top-7 font-[Literata] text-lg text-slate-400">Start writing…</div>} ErrorBoundary={({children})=><>{children}</>}/><HistoryPlugin/><ListPlugin/><OnChangePlugin ignoreSelectionChange onChange={state=>onChange({content:serializeEditorState(state),plainText:getPlainText(state)})}/></div></section></LexicalComposer>}
