<template>
  <div class="header">
    <h1>{{ !uuid ? `Nieuwe publicatie` : `Publicatie` }}</h1>

    <menu v-if="publicatie.publicatiestatus === PublicatieStatus.gepubliceerd" class="reset">
      <li>
        <a
          :href="publicatie.urlPublicatieExtern"
          class="button secondary icon-after external"
          target="_blank"
        >
          Bekijk online

          <span class="visually-hidden">(externe link)</span>
        </a>
      </li>
    </menu>
  </div>

  <simple-spinner v-show="isLoading"></simple-spinner>

  <div v-if="isGenerating" class="generating-overlay" aria-live="assertive">
    <div class="generating-overlay-content">
      <simple-spinner></simple-spinner>
      <p>Metadata wordt gegenereerd...</p>
    </div>
  </div>

  <form v-if="!isLoading" @submit.prevent="submit" v-form-invalid-handler>
    <alert-inline v-if="mijnGebruikersgroepenError || !mijnGebruikersgroepen?.length"
      >Er is iets misgegaan bij het ophalen van de gegevens. Neem contact op met de
      beheerder.</alert-inline
    >

    <section v-else>
      <alert-inline v-if="publicatieError"
        >Er is iets misgegaan bij het ophalen van de publicatie of de publicatie is niet (meer)
        beschikbaar...</alert-inline
      >

      <publicatie-form
        v-else
        v-model="publicatie"
        :unauthorized="unauthorized"
        :is-readonly="isReadonly"
        :is-draft-mode="isDraftMode"
        :mijn-gebruikersgroepen="mijnGebruikersgroepen"
        :gekoppelde-waardelijsten="gekoppeldeWaardelijsten"
      />

      <alert-inline v-if="documentenError"
        >Er is iets misgegaan bij het ophalen van de documenten bij deze publicatie...</alert-inline
      >

      <documenten-form
        v-else-if="publicatie.eigenaarGroep || isReadonly"
        v-model:files="files"
        v-model:documenten="documenten"
        :is-readonly="isReadonly"
      />
    </section>

    <!-- Document selection for metadata generation -->
    <div
      v-if="isAvailable && existingDocuments.length > 1 && !isReadonly && !hasError"
      class="document-selector"
    >
      <label for="generate-document-select">Document voor AI metadata generatie</label>
      <select id="generate-document-select" v-model="selectedDocumentUuid">
        <option value="">-- Selecteer een document --</option>
        <option v-for="doc in existingDocuments" :key="doc.uuid" :value="doc.uuid">
          {{ doc.bestandsnaam }}
        </option>
      </select>
    </div>

    <div class="form-submit">
      <menu class="reset">
        <li class="cancel">
          <button type="button" title="Opslaan" class="button secondary" @click="navigate">
            Annuleren
          </button>
        </li>

        <template v-if="publicatie.eigenaarGroep && !isReadonly && !hasError">
          <!-- main actions -->
          <li v-if="canDraft">
            <button
              type="submit"
              title="Opslaan als concept"
              class="button secondary"
              value="draft"
              @click="setValidationMode"
            >
              Opslaan als concept
            </button>
          </li>

          <!-- metadata generation -->
          <li v-if="isAvailable && existingDocuments.length">
            <button
              type="button"
              title="Genereer metadata met AI"
              class="button secondary"
              :disabled="!selectedDocumentUuid || isGenerating"
              @click="handleGenerateMetadata"
            >
              {{ isGenerating ? "Bezig..." : "Genereer metadata" }}
            </button>
          </li>

          <li>
            <button type="submit" title="Publiceren" value="publish" @click="setValidationMode">
              Publiceren
            </button>
          </li>

          <!-- delete / retract actions -->
          <li v-if="canDelete">
            <button
              type="button"
              title="Publicatie verwijderen"
              class="button danger"
              @click="remove"
            >
              Publicatie verwijderen
            </button>
          </li>

          <li v-if="canRetract">
            <button
              type="submit"
              title="Publicatie intrekken"
              class="button danger"
              value="retract"
              @click="setValidationMode"
            >
              Publicatie intrekken
            </button>
          </li>
        </template>
      </menu>

      <p class="required-message">Velden met (*) zijn verplicht</p>
    </div>

    <prompt-modal
      :dialog="draftDialog"
      cancel-text="Nee, keer terug"
      confirm-text="Ja, sla op als concept"
    >
      <draft-dialog-content />
    </prompt-modal>

    <prompt-modal
      :dialog="deleteDialog"
      cancel-text="Nee, keer terug"
      confirm-text="Ja, verwijderen"
    >
      <delete-dialog-content />
    </prompt-modal>

    <prompt-modal
      :dialog="retractDialog"
      cancel-text="Nee, gepubliceerd laten"
      confirm-text="Ja, intrekken"
    >
      <retract-dialog-content />
    </prompt-modal>

    <prompt-modal
      :dialog="noDocumentsDialog"
      cancel-text="Nee, documenten toevoegen"
      confirm-text="Ja, publiceren"
    >
      <no-documents-dialog-content />
    </prompt-modal>
  </form>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useRouter } from "vue-router";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import AlertInline from "@/components/AlertInline.vue";
import PromptModal from "@/components/PromptModal.vue";
import { usePreviousRoute } from "@/composables/use-previous-route";
import toast from "@/stores/toast";
import PublicatieForm from "./components/PublicatieForm.vue";
import DocumentenForm from "./components/DocumentenForm.vue";
import DraftDialogContent from "./components/dialogs/DraftDialogContent.vue";
import DeleteDialogContent from "./components/dialogs/DeleteDialogContent.vue";
import RetractDialogContent from "./components/dialogs/RetractDialogContent.vue";
import NoDocumentsDialogContent from "./components/dialogs/NoDocumentsDialogContent.vue";
import { usePublicatie } from "./composables/use-publicatie";
import { useDocumenten } from "./composables/use-documenten";
import { useMijnGebruikersgroepen } from "./composables/use-mijn-gebruikersgroepen";
import { useDialogs } from "./composables/use-dialogs";
import { useGenerateMetadata } from "./composables/use-generate-metadata";
import { PublicatieStatus } from "./types";

const router = useRouter();

const props = defineProps<{ uuid?: string }>();

const { previousRoute } = usePreviousRoute();

const { deleteDialog, draftDialog, retractDialog, noDocumentsDialog } = useDialogs();
const { isGenerating, isAvailable, checkAvailability, generateMetadata } = useGenerateMetadata();

onMounted(checkAvailability);

const isLoading = computed(
  () =>
    loadingPublicatie.value ||
    loadingDocumenten.value ||
    loadingMijnGebruikersgroepen.value ||
    loadingDocument.value ||
    uploadingFile.value
);

const hasError = computed(
  () =>
    !!publicatieError.value ||
    !!documentenError.value ||
    !!documentError.value ||
    !!mijnGebruikersgroepenError.value
);

const isReadonly = computed(
  () => publicatie.value.publicatiestatus === PublicatieStatus.ingetrokken || unauthorized.value
);

const canDraft = computed(
  () => !publicatie.value.uuid || publicatie.value.publicatiestatus === PublicatieStatus.concept
);

const canDelete = computed(
  () => publicatie.value.uuid && publicatie.value.publicatiestatus === PublicatieStatus.concept
);

const canRetract = computed(
  () => publicatie.value.publicatiestatus === PublicatieStatus.gepubliceerd
);

const userHasAccessToGroup = computed(() =>
  mijnGebruikersgroepen.value?.some(
    (groep) => groep.uuid === publicatie.value.eigenaarGroep?.identifier
  )
);

const groupHasWaardelijsten = computed(
  () =>
    gekoppeldeWaardelijsten.value.organisaties?.length &&
    gekoppeldeWaardelijsten.value.informatiecategorieen?.length
);

const publicatieWaardelijstenMatch = computed(
  () =>
    // Gebruikersgroep is assigned to publisher organisatie (or publisher not set yet)
    (gekoppeldeWaardelijstenUuids.value?.includes(publicatie.value.publisher) ||
      !publicatie.value.publisher) &&
    // Gebruikersgroep is assigned to every informatiecategorie of publicatie
    publicatie.value.informatieCategorieen.every((uuid: string) =>
      gekoppeldeWaardelijstenUuids.value?.includes(uuid)
    ) &&
    // Gebruikersgroep is assigned to every onderwerp of publicatie
    publicatie.value.onderwerpen.every((uuid: string) =>
      gekoppeldeWaardelijstenUuids.value?.includes(uuid)
    )
);

const unauthorized = computed(() => {
  if (!publicatie.value.eigenaarGroep) return false;

  return (
    !userHasAccessToGroup.value ||
    !groupHasWaardelijsten.value ||
    !publicatieWaardelijstenMatch.value
  );
});

// Publicatie
const {
  publicatie,
  isFetching: loadingPublicatie,
  error: publicatieError,
  submitPublicatie,
  deletePublicatie
} = usePublicatie(props.uuid);

// Documenten
const {
  files,
  documenten,
  loadingDocumenten,
  documentenError,
  loadingDocument,
  documentError,
  uploadingFile,
  submitDocumenten
} =
  // Get associated docs by uuid prop when existing pub, so no need to wait for pub fetch.
  // Publicatie.uuid is used when new pub and associated docs: docs submit waits for pub submit/publicatie.uuid.
  useDocumenten(() => props.uuid || publicatie.value?.uuid);

// Mijn gebruikersgroepen
const {
  data: mijnGebruikersgroepen,
  isFetching: loadingMijnGebruikersgroepen,
  error: mijnGebruikersgroepenError,
  gekoppeldeWaardelijsten,
  gekoppeldeWaardelijstenUuids
} = useMijnGebruikersgroepen(() => publicatie.value.eigenaarGroep?.identifier);

const clearPublicatieWaardelijsten = () =>
  (publicatie.value = {
    ...publicatie.value,
    ...{
      publisher: "",
      informatieCategorieen: [],
      onderwerpen: []
    }
  });

// Externally created publicaties will not have a eigenaarGroep untill updated from ODPC
const isPublicatieWithoutEigenaarGroep = ref(false);

watch(isLoading, () => {
  if (hasError.value) return;

  isPublicatieWithoutEigenaarGroep.value =
    !!publicatie.value.uuid && !publicatie.value.eigenaarGroep;

  // Preset eigenaarGroep of a new - or externally created publicatie when only one mijnGebruikersgroep
  if (
    (!publicatie.value.uuid || isPublicatieWithoutEigenaarGroep.value) &&
    mijnGebruikersgroepen.value?.length === 1
  ) {
    const { uuid, naam } = mijnGebruikersgroepen.value[0];

    publicatie.value.eigenaarGroep = { identifier: uuid, weergaveNaam: naam };
  }
});

// Clear waardelijsten of publicatie when mismatch waardelijsten gebruikersgroep (unauthorized) on
// a) switch from one to another gebruikersgroep or
// b) initial select gebruikersgroep when isPublicatieWithoutEigenaarGroep
const shouldClearWaardelijsten = (isSwitchGebruikersgroep: boolean) =>
  unauthorized.value && (isSwitchGebruikersgroep || isPublicatieWithoutEigenaarGroep.value);

watch(
  () => publicatie.value.eigenaarGroep,
  (_, oldValue) => {
    if (shouldClearWaardelijsten(!!oldValue)) clearPublicatieWaardelijsten();
  }
);

// Metadata generation
const existingDocuments = computed(() => documenten.value.filter((doc) => doc.uuid));

const selectedDocumentUuid = ref(
  existingDocuments.value.length ? existingDocuments.value[0].uuid : undefined
);

watch(existingDocuments, (docs) => {
  if (!selectedDocumentUuid.value && docs.length) {
    selectedDocumentUuid.value = docs[0].uuid;
  }
});

const handleGenerateMetadata = async () => {
  if (!selectedDocumentUuid.value) return;

  const result = await generateMetadata(
    selectedDocumentUuid.value,
    publicatie.value,
    documenten.value
  );

  if (!result) return;

  // Apply publication-level metadata
  publicatie.value = { ...publicatie.value, ...result.publicatie };

  // Apply document-level metadata
  if (result.document) {
    const docIndex = documenten.value.findIndex((d) => d.uuid === selectedDocumentUuid.value);
    if (docIndex !== -1) {
      documenten.value[docIndex] = { ...documenten.value[docIndex], ...result.document };
    }
  }

  toast.add({ text: "Metadata is succesvol gegenereerd." });
};

const navigate = () => {
  if (previousRoute.value?.name === "publicaties") {
    router.push({ name: previousRoute.value.name, query: previousRoute.value?.query });
  } else {
    router.push({ name: "publicaties" });
  }
};

const handleSuccess = (successMessage?: string) => {
  toast.add({ text: successMessage ?? "De publicatie is succesvol opgeslagen" });

  navigate();
};

const submitHandlers = {
  draft: async () => {
    if ((await draftDialog.reveal()).isCanceled) return;

    try {
      await submitPublicatie();
      await submitDocumenten();
    } catch {
      return;
    }

    handleSuccess("De publicatie is succesvol opgeslagen als concept.");
  },
  delete: async () => {
    if ((await deleteDialog.reveal()).isCanceled) return;

    try {
      await deletePublicatie();
    } catch {
      return;
    }

    handleSuccess("De publicatie is succesvol verwijderd.");
  },
  retract: async () => {
    if ((await retractDialog.reveal()).isCanceled) return;

    publicatie.value.publicatiestatus = PublicatieStatus.ingetrokken;

    try {
      // As soon as a publicatie gets status 'ingetrokken', the associated documents will
      // be automatically set to 'ingetrokken' as well and can no longer be updated from ODPC
      await submitPublicatie();
    } catch {
      return;
    }

    handleSuccess("De publicatie is succesvol ingetrokken.");
  },
  publish: async () => {
    if (documenten.value.length === 0 && (await noDocumentsDialog.reveal()).isCanceled) return;

    publicatie.value.publicatiestatus = PublicatieStatus.gepubliceerd;

    documenten.value.forEach((doc) => {
      if (doc.publicatiestatus === PublicatieStatus.concept)
        doc.publicatiestatus = PublicatieStatus.gepubliceerd;
    });

    try {
      await submitPublicatie();
      await submitDocumenten();
    } catch {
      return;
    }

    handleSuccess("De publicatie is succesvol opgeslagen en gepubliceerd.");
  }
} as const;

const isDraftMode = ref(false);

const setValidationMode = (e: Event) =>
  (isDraftMode.value = (e.currentTarget as HTMLButtonElement)?.value === "draft");

const remove = () => submitHandlers.delete();

const submit = (e: Event) => {
  const submitAction = ((e as SubmitEvent).submitter as HTMLButtonElement)?.value;

  if (!submitAction || !(submitAction in submitHandlers)) {
    toast.add({ text: "Onbekende actie.", type: "error" });
    return;
  }

  submitHandlers[submitAction as keyof typeof submitHandlers]();
};
</script>

<style lang="scss" scoped>
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

section {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(var(--section-width), 1fr));
  grid-gap: var(--spacing-default);
}

menu {
  li:has([value="delete"]) {
    order: 2;
  }

  li:has([value="draft"]) {
    order: 3;
  }

  li:has([value="retract"]) {
    order: 4;
  }

  li:has([value="publish"]) {
    order: 5;
  }
}

.document-selector {
  max-width: 600px;
  margin-block-end: var(--spacing-default);

  label {
    display: block;
    font-weight: var(--font-bold);
    margin-block-end: var(--spacing-small);
  }

  select {
    width: 100%;
  }
}

.generating-overlay {
  position: fixed;
  inset: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: rgba(255, 255, 255, 0.8);
}

.generating-overlay-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-default);

  p {
    font-size: var(--font-large);
    font-weight: var(--font-bold);
  }
}
</style>
