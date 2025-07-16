import { ref, watch, type MaybeRefOrGetter, toRef } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import { useAllPages } from "@/composables/use-all-pages";
import toast from "@/stores/toast";
import { uploadFile } from "../service";
import { PublicatieStatus, type PublicatieDocument } from "../types";

export const useDocumenten = (uuid: MaybeRefOrGetter<string | undefined>) => {
  const pubUuid = toRef(uuid);

  // Documenten
  const files = ref<File[]>([]);
  const documenten = ref<PublicatieDocument[]>([]);

  const {
    data,
    loading: loadingDocumenten,
    error: documentenError
  } = useAllPages<PublicatieDocument>(() =>
    pubUuid.value ? `/api/v2/documenten/?publicatie=${pubUuid.value}` : null
  );

  watch(data, (value) => (documenten.value = value ?? []));

  // Document
  const docUuid = ref<string>();
  const uploadingFile = ref(false);

  const {
    post: postDocument,
    put: putDocument,
    delete: deleteDocument,
    data: documentData,
    isFetching: loadingDocument,
    error: documentError
  } = useFetchApi(() => `/api/v2/documenten${docUuid.value ? "/" + docUuid.value : ""}`, {
    immediate: false
  }).json<PublicatieDocument>();

  const submitDocumenten = async () => {
    if (!pubUuid.value || !documenten.value) return;

    for (const [index, doc] of documenten.value.entries()) {
      if (!doc.uuid) {
        // Create
        docUuid.value = undefined;

        await postDocument({ ...doc, publicatie: pubUuid.value }).execute();

        if (!documentError.value) await uploadDocument(index);
      } else if (doc.pendingAction === "delete") {
        // Delete
        docUuid.value = doc.uuid;

        await deleteDocument().text().execute();
      } else {
        // Update
        docUuid.value = doc.uuid;

        await putDocument({
          ...doc,
          publicatiestatus:
            doc.pendingAction === "retract" ? PublicatieStatus.ingetrokken : doc.publicatiestatus
        }).execute();
      }

      if (documentError.value) {
        toast.add({
          text:
            doc.pendingAction === "delete"
              ? "Het document kon niet worden verwijderd, probeer het nogmaals..."
              : "De metadata bij het document kon niet worden opgeslagen, probeer het nogmaals...",
          type: "error"
        });

        // Reset for retry
        documentError.value = null;
        doc.pendingAction = null;

        throw new Error(`submitDocumenten`);
      }
    }
  };

  const uploadDocument = async (index: number) => {
    if (files.value?.[index] && documentData.value?.bestandsdelen?.length) {
      uploadingFile.value = true;

      try {
        await uploadFile(files.value[index], documentData.value.bestandsdelen);
      } catch (err) {
        toast.add({
          text: "Het document kon niet worden geupload, probeer het nogmaals...",
          type: "error"
        });

        throw err;
      } finally {
        uploadingFile.value = false;
      }
    } else {
      throw new Error(`uploadDocument`);
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
