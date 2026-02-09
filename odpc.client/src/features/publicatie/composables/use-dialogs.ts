import { useConfirmDialog } from "@vueuse/core";

export const useDialogs = () => ({
  draftDialog: useConfirmDialog(),
  deleteDialog: useConfirmDialog(),
  retractDialog: useConfirmDialog(),
  claimDialog: useConfirmDialog(),
  noDocumentsDialog: useConfirmDialog()
});
