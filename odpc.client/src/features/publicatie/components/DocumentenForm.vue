<template>
  <fieldset v-if="documenten" aria-live="polite">
    <legend>Documenten</legend>

    <file-upload v-if="!isReadonly" @filesSelected="filesSelected" />

    <template v-if="pendingDocuments.length">
      <h2>Nieuwe documenten</h2>

      <document-details-form
        v-for="doc in pendingDocuments"
        :key="doc.bestandsnaam"
        :doc="doc"
        :warnings="getDocumentWarnings(doc)"
        @removeDocument="removeDocument(doc)"
      />
    </template>

    <template v-if="existingDocuments.length">
      <h2>Toegevoegde documenten</h2>

      <document-details-form
        v-for="doc in existingDocuments"
        :key="doc.uuid"
        :doc="doc"
        :warnings="getDocumentWarnings(doc)"
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
  documenten: PublicatieDocument[];
  isReadonly: boolean;
}>();

const dialog = useConfirmDialog();

const documenten = useModel(props, "documenten");

const selectedFiles = ref<File[]>([]);

const pendingDocuments = computed(() =>
  documenten.value
    .filter((doc) => !doc.uuid)
    // sort pending documents on bestandsnaam (= initial officieleTitel)
    // to prevent reordering of documents while officieleTitel is edited
    .sort((a, b) => a.bestandsnaam.localeCompare(b.bestandsnaam))
);

const existingDocuments = computed(() => documenten.value.filter((doc) => doc.uuid));

const caseInsensitiveEquals = (a: string, b: string) =>
  a.localeCompare(b, undefined, { sensitivity: "base" }) === 0;

const findDocumentWarnings = (doc: PublicatieDocument, otherDocs: PublicatieDocument[]) => {
  if (doc.publicatiestatus === PublicatieStatus.ingetrokken) return [];

  const activeDocs = otherDocs.filter((d) => d.publicatiestatus !== PublicatieStatus.ingetrokken);

  return [
    doc.bestandsnaam &&
      activeDocs.some((d) => caseInsensitiveEquals(d.bestandsnaam, doc.bestandsnaam)) &&
      `Er bestaat al een document met de bestandsnaam '${doc.bestandsnaam}'.
        Controleer of dit document al in de publicatie zit.`,

    doc.officieleTitel &&
      activeDocs.some((d) => caseInsensitiveEquals(d.officieleTitel, doc.officieleTitel)) &&
      `Er bestaat al een document met de titel '${doc.officieleTitel}'.
        Controleer of dit document al in de publicatie zit.`
  ].filter((warning): warning is string => !!warning);
};

const documentWarnings = computed(() => {
  const docWarnings = new Map<PublicatieDocument, string[]>();

  for (const doc of documenten.value) {
    const otherDocs = documenten.value.filter((d) => d !== doc);

    const warnings = findDocumentWarnings(doc, otherDocs);

    if (warnings.length) docWarnings.set(doc, warnings);
  }

  return docWarnings;
});

const getDocumentWarnings = (doc: PublicatieDocument) => documentWarnings.value.get(doc) ?? [];

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

// some file types have an extension configured in the mimeTypes 'table'
// for these types (zip and 7z) checking on mime-type is not reliable
// so they will be matched on extension to map to their 'identifier'
const matchKnownTypes = (file: File, type: MimeType) =>
  type.extension ? file.name.toLowerCase().endsWith(type.extension) : file.type === type.mimeType;

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
      doc._file = file;

      newDocuments.push(doc);
    });
  } catch {
    return;
  }

  const hasDuplicates = newDocuments.some(
    (doc) => findDocumentWarnings(doc, documenten.value).length > 0
  );

  if (hasDuplicates) {
    toast.add({
      text: "Er zijn dubbele documenten gedetecteerd. Controleer de documenten.",
      type: "error"
    });
  }

  documenten.value = [...pendingDocuments.value, ...newDocuments, ...existingDocuments.value];
});

const removeDocument = async (doc: PublicatieDocument) => {
  const { isCanceled } = await dialog.reveal();

  if (!isCanceled) documenten.value = documenten.value.filter((d) => d !== doc);
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
