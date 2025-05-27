import { ref } from "vue";
import { useConfirmDialog } from "@vueuse/core";

export type DialogConfig = {
  message: string;
  cancelText?: string;
  confirmText?: string;
};

export function useMultipleConfirmDialogs() {
  const dialog = useConfirmDialog();

  const currentDialogConfig = ref<DialogConfig | null>(null);

  const showDialog = async (config: DialogConfig) => {
    currentDialogConfig.value = config;

    const { isCanceled } = await dialog.reveal();

    currentDialogConfig.value = null;

    return { isCanceled };
  };

  return {
    dialog,
    currentDialogConfig,
    showDialog
  };
}
