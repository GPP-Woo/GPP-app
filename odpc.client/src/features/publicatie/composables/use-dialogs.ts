import { useConfirmDialog } from "@vueuse/core";

export const useDialogs = () => ({
  draftDialog: useConfirmDialog(),
  deleteDialog: useConfirmDialog(),
  retractDialog: useConfirmDialog(),
  noDocumentsDialog: useConfirmDialog()
});
