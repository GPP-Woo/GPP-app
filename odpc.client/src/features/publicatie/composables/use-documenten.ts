import { ref, onMounted, type ComputedRef } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import { fetchAllPages } from "@/composables/use-all-pages";
import toast from "@/stores/toast";
import { uploadFile } from "../service";
import type { PublicatieDocument } from "../types";

export const useDocumenten = (pubUUID: ComputedRef<string | undefined>) => {
  // Documenten
  const files = ref<File[]>([]);
  const documenten = ref<PublicatieDocument[]>([]);

  const loadingDocumenten = ref(false);
  const documentenError = ref(false);

  const getDocumenten = () => {
    loadingDocumenten.value = true;

    fetchAllPages<PublicatieDocument>(`/api/v1/documenten/?publicatie=${pubUUID.value}`)
      .then((results) => (documenten.value = results))
      .catch(() => (documentenError.value = true))
      .finally(() => (loadingDocumenten.value = false));
  };

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

  onMounted(() => pubUUID.value && getDocumenten());

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
