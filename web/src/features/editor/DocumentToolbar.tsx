import { useLexicalComposerContext } from "@lexical/react/LexicalComposerContext";
import { $createHeadingNode, $isHeadingNode, type HeadingTagType } from "@lexical/rich-text";
import { $setBlocksType } from "@lexical/selection";
import { INSERT_ORDERED_LIST_COMMAND, INSERT_UNORDERED_LIST_COMMAND, REMOVE_LIST_COMMAND, ListNode, $isListNode } from "@lexical/list";
import { $findMatchingParent, mergeRegister } from "@lexical/utils";
import { $getRoot, $getSelection, $isRangeSelection, CAN_REDO_COMMAND, CAN_UNDO_COMMAND, COMMAND_PRIORITY_LOW, FORMAT_TEXT_COMMAND, REDO_COMMAND, SELECTION_CHANGE_COMMAND, UNDO_COMMAND } from "lexical";
import { useCallback, useEffect, useState } from "react";

type Active = { bold: boolean; italic: boolean; underline: boolean; h1: boolean; h2: boolean; bullet: boolean; number: boolean };
const empty: Active = { bold:false, italic:false, underline:false, h1:false, h2:false, bullet:false, number:false };
export function DocumentToolbar() {
  const [editor] = useLexicalComposerContext(); const [active,setActive]=useState(empty); const [undo,setUndo]=useState(false); const [redo,setRedo]=useState(false);
  const refresh=useCallback(()=>editor.getEditorState().read(()=>{const s=$getSelection(); if(!$isRangeSelection(s)) return; const node=s.anchor.getNode(); const top=node.getKey()==="root"?node:$findMatchingParent(node,n=>n.getParent()?.getKey()==="root"); const list=$findMatchingParent(node,$isListNode) as ListNode|null; setActive({bold:s.hasFormat("bold"),italic:s.hasFormat("italic"),underline:s.hasFormat("underline"),h1:$isHeadingNode(top)&&top.getTag()==="h1",h2:$isHeadingNode(top)&&top.getTag()==="h2",bullet:list?.getListType()==="bullet",number:list?.getListType()==="number"});}),[editor]);
  useEffect(()=>mergeRegister(editor.registerUpdateListener(refresh),editor.registerCommand(SELECTION_CHANGE_COMMAND,()=>{refresh();return false},COMMAND_PRIORITY_LOW),editor.registerCommand(CAN_UNDO_COMMAND,v=>{setUndo(v);return false},COMMAND_PRIORITY_LOW),editor.registerCommand(CAN_REDO_COMMAND,v=>{setRedo(v);return false},COMMAND_PRIORITY_LOW)),[editor,refresh]);
  const button=(name:string, pressed:boolean|undefined, action:()=>void, disabled=false)=><button type="button" aria-label={name} aria-pressed={pressed} title={name} disabled={disabled} onMouseDown={event=>event.preventDefault()} onClick={action} className="focus-visible:ring-2 rounded-md px-2.5 py-2 text-sm font-bold hover:bg-slate-100 aria-pressed:bg-blue-50 aria-pressed:text-blue-700 disabled:opacity-40">{name}</button>;
  const ensureSelection=()=>editor.update(()=>{if(!$isRangeSelection($getSelection()))$getRoot().selectEnd()},{discrete:true});
  const format=(kind:"bold"|"italic"|"underline")=>{ensureSelection();editor.dispatchCommand(FORMAT_TEXT_COMMAND,kind)};
  const list=(ordered:boolean)=>{ensureSelection(); const isActive=ordered?active.number:active.bullet; editor.dispatchCommand(isActive?REMOVE_LIST_COMMAND:ordered?INSERT_ORDERED_LIST_COMMAND:INSERT_UNORDERED_LIST_COMMAND,undefined)};
  const heading=(tag:HeadingTagType)=>editor.update(()=>{const s=$getSelection();if($isRangeSelection(s))$setBlocksType(s,()=> $createHeadingNode(tag));});
  return <div role="toolbar" aria-label="Document formatting" className="sticky top-0 z-10 flex flex-wrap gap-1 border-b border-slate-200 bg-white/95 p-2 font-[Manrope] shadow-sm backdrop-blur">{button("Bold",active.bold,()=>format("bold"))}{button("Italic",active.italic,()=>format("italic"))}{button("Underline",active.underline,()=>format("underline"))}{button("Heading 1",active.h1,()=>{ensureSelection();heading("h1")})}{button("Heading 2",active.h2,()=>{ensureSelection();heading("h2")})}{button("Bulleted list",active.bullet,()=>list(false))}{button("Numbered list",active.number,()=>list(true))}{button("Undo",undefined,()=>editor.dispatchCommand(UNDO_COMMAND,undefined),!undo)}{button("Redo",undefined,()=>editor.dispatchCommand(REDO_COMMAND,undefined),!redo)}</div>;
}
