<template>
  <fieldset v-if="documenten" aria-live="polite">
    <legend>Documenten</legend>

    <file-upload v-if="!isReadonly" @filesSelected="filesSelected" />

    <template v-if="pendingDocuments.length">
      <h2>Nieuwe documenten</h2>

      <document-details-form
        v-for="(doc, index) in pendingDocuments"
        :key="index"
        :doc="doc"
        @removeDocument="removeDocument(index)"
      />
    </template>

    <template v-if="existingDocuments.length">
      <h2>Toegevoegde documenten</h2>

      <document-details-form
        v-for="(doc, index) in existingDocuments"
        :key="index"
        :doc="doc"
        :is-readonly="isReadonly || doc.publicatiestatus === PublicatieStatus.ingetrokken"
      />
    </template>

    <alert-inline v-if="isReadonly && !documenten.length"
      >Er zijn geen gekoppelde documenten.</alert-inline
    >

    <prompt-modal :dialog="dialog" confirm-message="Ja, verwijderen" cancel-message="Nee, behouden">
      <p>Weet u zeker dat u dit document wilt verwijderen?</p>
    </prompt-modal>
  </fieldset>
</template>

<script setup lang="ts">
import { watch, useModel, computed, ref } from "vue";
import { useConfirmDialog } from "@vueuse/core";
import toast from "@/stores/toast";
import AlertInline from "@/components/AlertInline.vue";
import PromptModal from "@/components/PromptModal.vue";
import { PublicatieStatus, type MimeType, type PublicatieDocument } from "../types";
import { mimeTypes } from "../service";
import FileUpload from "./FileUpload.vue";
import DocumentDetailsForm from "./DocumentDetailsForm.vue";

const props = defineProps<{
  files: File[];
  documenten: PublicatieDocument[];
  isReadonly: boolean;
}>();

const dialog = useConfirmDialog();

const files = useModel(props, "files");
const documenten = useModel(props, "documenten");

const selectedFiles = ref<File[]>([]);

const pendingDocuments = computed(() => documenten.value.filter((doc) => !doc.uuid));
const existingDocuments = computed(() => documenten.value.filter((doc) => doc.uuid));

const getInitialDocument = (): PublicatieDocument => ({
  publicatie: "",
  officieleTitel: "",
  verkorteTitel: "",
  omschrijving: "",
  publicatiestatus: PublicatieStatus.concept,
  creatiedatum: new Date().toISOString().split("T")[0],
  bestandsnaam: "",
  bestandsformaat: "",
  bestandsomvang: 0,
  kenmerken: []
});

// for file types zip and 7z mimetypes are sometimes not properly mapped
// and may fallback to 'application/octet-stream', if so then check against extension
const matchKnownTypes = (file: File, type: MimeType) =>
  file.type === "application/octet-stream" && type.extension
    ? file.name.toLowerCase().endsWith(type.extension)
    : file.type === type.mimeType;

const clearInputData = (event: Event | DragEvent) => {
  if (event instanceof DragEvent) {
    event.dataTransfer?.clearData();
  } else {
    (event.target as HTMLInputElement).value = "";
  }
};

const filesSelected = (event: Event | DragEvent) => {
  const inputData: File[] =
    event instanceof DragEvent
      ? [...(event.dataTransfer?.files || [])]
      : [...((event.target as HTMLInputElement).files || [])];

  const unknownType = inputData.some(
    (file) => !mimeTypes.value?.some((type) => matchKnownTypes(file, type))
  );

  const emptyFile = inputData.some((file) => !file.size);

  if (!inputData.length || unknownType || emptyFile) {
    toast.add({
      text: "Onbekend formaat of leeg bestand.",
      type: "error"
    });
  } else {
    selectedFiles.value = inputData;
  }

  clearInputData(event);
};

watch(selectedFiles, (newFiles) => {
  const newDocuments: PublicatieDocument[] = [];

  try {
    Array.from(newFiles || []).forEach((file) => {
      const doc = getInitialDocument();

      const bestandsformaat = mimeTypes.value?.find((type) =>
        matchKnownTypes(file, type)
      )?.identifier;

      if (!bestandsformaat) throw new Error();

      doc.officieleTitel = file.name.replace(/\.[^/.]+$/, ""); // file name minus extension as default title
      doc.bestandsnaam = file.name;
      doc.bestandsformaat = bestandsformaat;
      doc.bestandsomvang = file.size;

      newDocuments.push(doc);
    });
  } catch {
    return;
  }

  // update both files and documenten together to maintain consistency/stay in sync
  files.value = [...files.value, ...newFiles];
  documenten.value = [...pendingDocuments.value, ...newDocuments, ...existingDocuments.value];
});

const removeDocument = async (index: number) => {
  const { isCanceled } = await dialog.reveal();

  if (!isCanceled) {
    files.value = files.value.filter((_, i) => i !== index);
    documenten.value = documenten.value.filter((_, i) => i !== index);
  }
};
</script>

<style lang="scss" scoped>
h2 {
  font-size: var(--font-large);
  margin-block-end: var(--spacing-default);

  legend + & {
    margin-block-start: 0;
  }
}
</style>
