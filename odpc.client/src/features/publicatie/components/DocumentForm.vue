<template>
  <fieldset v-if="documenten" aria-live="polite" :disabled="disabled">
    <legend>Documenten</legend>

    <template v-if="!documenten.some((doc) => !doc.uuid) && !disabled">
      <file-upload @filesSelected="filesSelected" />

      <h2 v-if="documenten.length">Toegevoegde documenten</h2>
    </template>

    <details
      v-for="(doc, index) in documenten"
      :key="index"
      :class="{
        nieuw: !doc.uuid
      }"
      :open="!doc.uuid"
    >
      <template v-if="doc.uuid">
        <summary>
          <template v-if="doc.publicatiestatus === PublicatieStatus.ingetrokken"
            ><s :aria-describedby="`status-${index}`">{{ doc.bestandsnaam }}</s>
            <span :id="`status-${index}`" role="status">(ingetrokken)</span></template
          >
          <template v-else>{{ doc.bestandsnaam }}</template>

          <span>
            (<a
              :href="`/api/v1/documenten/${doc.uuid}/download`"
              :title="`Download ${doc.bestandsnaam}`"
              >download</a
            >)
          </span>
        </summary>

        <div v-if="!disabled" class="form-group form-group-radio">
          <label>
            <input
              type="radio"
              v-model="doc.publicatiestatus"
              :value="PublicatieStatus.gepubliceerd"
            />
            Gepubliceerd
          </label>

          <label
            ><input
              type="radio"
              v-model="doc.publicatiestatus"
              :value="PublicatieStatus.ingetrokken"
            />
            Ingetrokken</label
          >
        </div>
      </template>

      <summary v-else @click.prevent tabindex="-1">
        Nieuw: <span>{{ doc.bestandsnaam }}</span>
      </summary>

      <div class="form-group">
        <label for="titel">Titel *</label>

        <input
          id="titel"
          type="text"
          v-model="doc.officieleTitel"
          required
          aria-required="true"
          :aria-describedby="`titelError-${index}`"
          :aria-invalid="!doc.officieleTitel"
        />

        <span :id="`titelError-${index}`" class="error">Titel is een verplicht veld</span>
      </div>

      <div class="form-group">
        <label for="verkorte_titel">Verkorte titel</label>

        <input id="verkorte_titel" type="text" v-model="doc.verkorteTitel" />
      </div>

      <div class="form-group">
        <label for="omschrijving">Omschrijving</label>

        <textarea id="omschrijving" v-model="doc.omschrijving" rows="4"></textarea>
      </div>

      <button
        v-if="!doc.uuid"
        type="button"
        class="button secondary icon-after trash"
        @click="removeDocument(index)"
      >
        Verwijderen
      </button>
    </details>

    <alert-inline v-if="disabled && !documenten.length"
      >Er zijn geen gekoppelde documenten.</alert-inline
    >

    <prompt-modal :dialog="dialog" confirm-message="Ja, verwijderen" cancel-message="Nee, behouden">
      <p>Weet u zeker dat u dit document wilt verwijderen?</p>
    </prompt-modal>
  </fieldset>
</template>

<script setup lang="ts">
import { watch, useModel } from "vue";
import { useConfirmDialog } from "@vueuse/core";
import toast from "@/stores/toast";
import AlertInline from "@/components/AlertInline.vue";
import PromptModal from "@/components/PromptModal.vue";
import { PublicatieStatus, type PublicatieDocument } from "../types";
import { mimeTypes } from "../service";
import FileUpload from "./FileUpload.vue";

const props = defineProps<{
  files: File[];
  documenten: PublicatieDocument[];
  disabled: boolean;
}>();

const dialog = useConfirmDialog();

const files = useModel(props, "files");
const documenten = useModel(props, "documenten");

const getInitialDocument = (): PublicatieDocument => ({
  identifier: "",
  publicatie: "",
  officieleTitel: "",
  verkorteTitel: "",
  omschrijving: "",
  publicatiestatus: PublicatieStatus.gepubliceerd,
  creatiedatum: new Date().toISOString().split("T")[0],
  bestandsnaam: "",
  bestandsformaat: "",
  bestandsomvang: 0
});

const filesSelected = (event: Event | DragEvent) => {
  const selectedFiles: File[] =
    event instanceof DragEvent
      ? [...(event.dataTransfer?.files || [])]
      : [...((event.target as HTMLInputElement).files || [])];

  const unknownType = selectedFiles.some(
    (file) => !mimeTypes.value?.some((type) => type.mimeType === file.type)
  );

  const emptyFile = selectedFiles.some((file) => !file.size);

  if (!selectedFiles.length || unknownType || emptyFile) {
    if (event instanceof DragEvent) {
      event.dataTransfer?.clearData();
    } else {
      (event.target as HTMLInputElement).value = "";
    }

    toast.add({
      text: "Onbekend formaat of leeg bestand.",
      type: "error"
    });

    return;
  }

  files.value = selectedFiles;
};

watch(files, () => addDocumenten());

const addDocumenten = () => {
  const docs: PublicatieDocument[] = [];

  try {
    Array.from(files.value || []).forEach((file) => {
      const doc = getInitialDocument();

      const bestandsformaat = mimeTypes.value?.find(
        (type) => type.mimeType === file.type
      )?.identifier;

      if (!bestandsformaat) throw new Error();

      doc.officieleTitel = file.name.replace(/\.[^/.]+$/, ""); // file name minus extension as default title
      doc.bestandsnaam = file.name;
      doc.bestandsformaat = bestandsformaat;
      doc.bestandsomvang = file.size;
      docs.push(doc);
    });
  } catch {
    return;
  }

  documenten.value = [...docs, ...documenten.value];
};

const removeDocument = async (index: number) => {
  const { isCanceled } = await dialog.reveal();

  if (!isCanceled) documenten.value.splice(index, 1);
};
</script>

<style lang="scss" scoped>
h2 {
  font-size: var(--font-large);
  margin-block-end: var(--spacing-default);
}

details {
  span {
    font-weight: normal;
    margin-inline-start: var(--spacing-extrasmall);
  }

  &.nieuw {
    summary {
      list-style: none;
      pointer-events: none;

      &::-webkit-details-marker {
        display: none;
      }
    }
  }

  &.ingetrokken {
    background-color: var(--disabled);
  }
}

dl {
  margin-block: 0 var(--spacing-default);

  dt {
    color: var(--text);
    font-weight: var(--font-bold);
  }

  dd {
    margin-inline-start: 0;
    margin-block-end: var(--spacing-small);
  }
}
</style>
