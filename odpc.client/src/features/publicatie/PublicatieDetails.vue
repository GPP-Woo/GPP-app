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
import { computed, ref, watch } from "vue";
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
import { usePublicatiePermissions } from "./composables/use-publicatie-permissions";
import { useDialogs } from "./composables/use-dialogs";
import { PublicatieStatus } from "./types";

const router = useRouter();

const props = defineProps<{ uuid?: string }>();

const { previousRoute } = usePreviousRoute();

const { deleteDialog, draftDialog, retractDialog, noDocumentsDialog } = useDialogs();

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

// Permissions
const { isReadonly, canDraft, canDelete, canRetract, unauthorized } = usePublicatiePermissions(
  publicatie,
  mijnGebruikersgroepen,
  gekoppeldeWaardelijsten,
  gekoppeldeWaardelijstenUuids
);

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

const clearPublicatieWaardelijsten = () =>
  (publicatie.value = {
    ...publicatie.value,
    ...{
      publisher: "",
      informatieCategorieen: [],
      onderwerpen: []
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
</style>
