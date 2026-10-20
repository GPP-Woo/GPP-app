import { ref, computed, watch, type MaybeRefOrGetter, toRef } from "vue";
import { useAllPages } from "@/composables/use-all-pages";
import { LoggedOutError } from "@/api/error";
import { ApiError, sendDocument, uploadFile } from "../service";
import {
  PublicatieStatus,
  UploadStatus,
  type Bestandsdeel,
  type ClientState,
  type PublicatieDocument
} from "../types";

export class DocumentenSubmitError extends Error {
  constructor(message = "Not all documenten could be submitted") {
    super(message);
  }
}

export const useDocumenten = (uuid: MaybeRefOrGetter<string | undefined>) => {
  const pubUuid = toRef(uuid);

  // Documenten
  const documenten = ref<PublicatieDocument[]>([]);

  const {
    data,
    loading: loadingDocumenten,
    error: documentenError,
    refetch: refetchDocumenten
  } = useAllPages<PublicatieDocument>(() =>
    pubUuid.value ? `/api/v2/documenten/?publicatie=${pubUuid.value}` : null
  );

  watch(data, (value) => (documenten.value = value ?? []));

  const submittingDocumenten = ref(false);

  const uploadingDocumenten = computed(() =>
    documenten.value.filter((doc) => doc._client?.upload || doc._client?.error)
  );

  const documentenSubmitError = computed(() => documenten.value.some((doc) => doc._client?.error));

  const uploadTotalProgress = computed(() => {
    const uploads = documenten.value.filter((doc) => doc._client?.upload);
    const totalSize = uploads.reduce((sum, doc) => sum + doc.bestandsomvang, 0);
    const uploaded = uploads.reduce(
      (sum, doc) => sum + (doc.bestandsomvang * (doc._client?.upload?.progress ?? 0)) / 100,
      0
    );

    return totalSize ? Math.floor((uploaded / totalSize) * 100) : 0;
  });

  const submitDocumenten = async () => {
    if (!pubUuid.value || !documenten.value) return;

    documenten.value.forEach((doc) => {
      if (!doc.uuid && doc._client)
        doc._client.upload = { progress: 0, status: UploadStatus.pending };
    });

    submittingDocumenten.value = true;

    try {
      for (const doc of documenten.value) {
        try {
          await submitDocument(doc);
        } catch (err) {
          if (err instanceof LoggedOutError) throw err;
        }
      }
    } finally {
      submittingDocumenten.value = false;
    }

    if (documentenSubmitError.value) throw new DocumentenSubmitError();
  };

  const submitDocument = async (doc: PublicatieDocument) => {
    const { _client, ...meta } = doc;

    try {
      if (!doc.uuid) {
        const created = await sendDocument<PublicatieDocument>("POST", "/api/v2/documenten", {
          ...meta,
          publicatie: pubUuid.value
        });

        await uploadDocument(_client, created.bestandsdelen);
      } else if (doc.pendingAction === "delete") {
        await sendDocument("DELETE", `/api/v2/documenten/${doc.uuid}`);
      } else {
        await sendDocument("PUT", `/api/v2/documenten/${doc.uuid}`, {
          ...meta,
          publicatiestatus:
            doc.pendingAction === "retract" ? PublicatieStatus.ingetrokken : doc.publicatiestatus
        });
      }
    } catch (err) {
      console.error(err);

      if (err instanceof LoggedOutError) throw err;

      if (_client?.upload) _client.upload.status = UploadStatus.failed;

      const client = (doc._client ??= {});

      if (err instanceof ApiError && err.kind === "upload") {
        client.error = "Het uploaden is mislukt";
      } else if (err instanceof ApiError) {
        client.error =
          doc.pendingAction === "delete"
            ? "Het verwijderen is mislukt"
            : "De metadata kon niet worden opgeslagen";
      } else {
        client.error = "Onverwachte fout";
      }

      throw err;
    }
  };

  const uploadDocument = async (client?: ClientState, bestandsdelen?: Bestandsdeel[] | null) => {
    const { file, upload } = client ?? {};

    if (!file || !upload) {
      throw new Error("uploadDocument: document has no file to upload");
    }

    if (!bestandsdelen?.length) {
      throw new ApiError("upload", undefined, "no bestandsdelen in response");
    }

    upload.status = UploadStatus.uploading;

    await uploadFile(file, bestandsdelen, (percentage) => (upload.progress = percentage));

    upload.progress = 100;
    upload.status = UploadStatus.done;
  };

  return {
    documenten,
    loadingDocumenten,
    documentenError,
    submittingDocumenten,
    uploadingDocumenten,
    documentenSubmitError,
    uploadTotalProgress,
    submitDocumenten,
    refetchDocumenten
  };
};
