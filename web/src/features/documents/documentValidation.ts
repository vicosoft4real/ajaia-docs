export const MAX_IMPORT_BYTES = 1024 * 1024;
export const IMPORT_FILE_MESSAGE = "Choose a .txt or .md file no larger than 1 MB.";

export function validateImportFile(file: File): string | null {
  if (!/\.(txt|md)$/i.test(file.name) || file.size > MAX_IMPORT_BYTES) return IMPORT_FILE_MESSAGE;
  if (file.size === 0) return "Choose a file that contains some text.";
  return null;
}
