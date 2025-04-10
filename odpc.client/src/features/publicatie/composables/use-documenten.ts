import { ref, type Ref, computed, watch } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import { useAllPages } from "@/composables/use-all-pages";
import toast from "@/stores/toast";
import { uploadFile } from "../service";
import type { PublicatieDocument } from "../types";

export const useDocumenten = (pubUUID: Ref<string | undefined>) => {
  // Documenten
  const files = ref<File[]>([]);
  const documenten = ref<PublicatieDocument[]>([]);

  const {
    data: documentenData,
    loading: loadingDocumenten,
    error: documentenError
  } = useAllPages<PublicatieDocument>(
    computed(() => (pubUUID.value ? `/api/v1/documenten/?publicatie=${pubUUID.value}` : null))
  );

  watch(documentenData, (value) => (documenten.value = value ?? []));

  const submitDocumenten = async () => {
    if (!pubUUID.value || !documenten.value) return;

    try {
      for (const [index, doc] of documenten.value.entries()) {
        if (!doc.uuid) {
          docUUID.value = undefined;

          await postDocument({ ...doc, publicatie: pubUUID.value }).execute();

          if (!documentError.value) await uploadDocument(index);
        } else {
          docUUID.value = doc.uuid;

          await putDocument(doc).execute();
        }

        if (documentError.value) {
          toast.add({
            text: "De metadata bij het document kon niet worden opgeslagen, probeer het nogmaals...",
            type: "error"
          });

          throw new Error();
        }
      }
    } catch {
      throw new Error();
    }
  };

  // Document
  const docUUID = ref<string>();
  const uploadingFile = ref(false);

  const {
    post: postDocument,
    put: putDocument,
    data: documentData,
    isFetching: loadingDocument,
    error: documentError
  } = useFetchApi(() => `/api/v1/documenten${docUUID.value ? "/" + docUUID.value : ""}`, {
    immediate: false
  }).json<PublicatieDocument>();

  const uploadDocument = async (index: number) => {
    if (files.value?.[index] && documentData.value?.bestandsdelen?.length) {
      uploadingFile.value = true;

      try {
        await uploadFile(files.value[index], documentData.value.bestandsdelen);
      } catch {
        toast.add({
          text: "Het document kon niet worden geupload, probeer het nogmaals...",
          type: "error"
        });

        throw new Error();
      } finally {
        uploadingFile.value = false;
      }
    } else {
      throw new Error();
    }
  };

  return {
    files,
    documenten,
    loadingDocumenten,
    documentenError,
    loadingDocument,
    documentError,
    uploadingFile,
    submitDocumenten
  };
};
