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
        :groep-waardelijsten="groepWaardelijsten"
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

    <!-- Metadata Preview Modal -->
    <metadata-preview-modal
      :is-revealed="showMetadataPreview"
      :is-loading="isGenerating"
      :error="metadataError"
      :data="metadataPreviewData"
      @close="handleMetadataPreviewClose"
      @confirm="handleMetadataPreviewConfirm"
    />

    <div class="form-submit">
      <menu class="reset">
        <li class="cancel">
          <button type="button" title="Opslaan" class="button secondary" @click="navigate">
            Annuleren
          </button>
        </li>

        <li v-if="isAvailable && existingDocuments.length && !isReadonly && !hasError">
          <button
            type="button"
            title="Genereer metadata met AI voor alle documenten"
            class="button secondary"
            :disabled="isGenerating"
            @click="handleGenerateMetadata"
          >
            {{ isGenerating ? "Bezig..." : "Genereer metadata" }}
          </button>
        </li>

        <publicatie-submit-buttons
          v-if="publicatie.eigenaarGroep && !hasError"
          :can-draft="canDraft"
          :can-delete="canDelete"
          :can-retract="canRetract"
          :can-claim="canClaim"
          :is-readonly="isReadonly"
          @onSetValidationMode="setValidationMode"
          @onRemove="remove"
        />
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

    <prompt-modal :dialog="claimDialog" cancel-text="Nee, keer terug" confirm-text="Ja, claimen">
      <claim-dialog-content />
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
import { useAppData } from "@/composables/use-app-data";
import toast from "@/stores/toast";
import PublicatieForm from "./components/PublicatieForm.vue";
import DocumentenForm from "./components/DocumentenForm.vue";
import PublicatieSubmitButtons from "./components/PublicatieSubmitButtons.vue";
import DraftDialogContent from "./components/dialogs/DraftDialogContent.vue";
import DeleteDialogContent from "./components/dialogs/DeleteDialogContent.vue";
import RetractDialogContent from "./components/dialogs/RetractDialogContent.vue";
import ClaimDialogContent from "./components/dialogs/ClaimDialogContent.vue";
import NoDocumentsDialogContent from "./components/dialogs/NoDocumentsDialogContent.vue";
import MetadataPreviewModal from "./components/MetadataPreviewModal.vue";
import { usePublicatie } from "./composables/use-publicatie";
import { useDocumenten } from "./composables/use-documenten";
import { useMijnGebruikersgroepen } from "./composables/use-mijn-gebruikersgroepen";
import { usePublicatiePermissions } from "./composables/use-publicatie-permissions";
import { useDialogs } from "./composables/use-dialogs";
import { useGenerateMetadata } from "./composables/use-generate-metadata";
import { PublicatieStatus } from "./types";

const props = defineProps<{ uuid?: string }>();

const router = useRouter();

const { previousRoute } = usePreviousRoute();

const { user } = useAppData();

const { deleteDialog, draftDialog, retractDialog, claimDialog, noDocumentsDialog } = useDialogs();
const {
	isGenerating,
	isAvailable,
	lastError: metadataLastError,
	checkAvailability,
	generateMetadataPreview,
	applyMetadataSuggestions
} = useGenerateMetadata();

// Metadata preview state
const showMetadataPreview = ref(false);
const metadataPreviewData = ref<import("./types").MetadataPreviewData | null>(null);
const metadataError = ref<string | null>(null);

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
  error: mijnGebruikersgroepenError
} = useMijnGebruikersgroepen();

// Permissions
const { isReadonly, canDraft, canDelete, canRetract, canClaim, unauthorized, groepWaardelijsten } =
  usePublicatiePermissions(publicatie, mijnGebruikersgroepen);

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

// Metadata generation
const existingDocuments = computed(() => documenten.value.filter((doc) => doc.uuid));

const handleGenerateMetadata = async () => {
	if (!existingDocuments.value.length) return;

	metadataError.value = null;
	showMetadataPreview.value = true;

	const result = await generateMetadataPreview(publicatie.value, documenten.value);

	if (result) {
		metadataPreviewData.value = result;
	} else {
		metadataError.value = metadataLastError.value || "Kon geen metadata suggesties genereren";
	}
};

const handleMetadataPreviewClose = () => {
	showMetadataPreview.value = false;
	metadataPreviewData.value = null;
	metadataError.value = null;
};

const handleMetadataPreviewConfirm = (
	data: import("./types").MetadataPreviewData
) => {
	const result = applyMetadataSuggestions(data);

	// Apply publication-level metadata
	if (Object.keys(result.publicatie).length > 0) {
		publicatie.value = { ...publicatie.value, ...result.publicatie };
	}

	// Apply document-level metadata
	for (const [docUuid, docUpdate] of result.documents) {
		const docIndex = documenten.value.findIndex((d) => d.uuid === docUuid);
		if (docIndex !== -1) {
			documenten.value[docIndex] = { ...documenten.value[docIndex], ...docUpdate };
		}
	}

	showMetadataPreview.value = false;
	metadataPreviewData.value = null;

	const appliedCount =
		data.publicationSuggestions.filter((s) => s.selected).length +
		data.documentSuggestions.reduce((acc, doc) => acc + doc.fields.filter((s) => s.selected).length, 0);

	toast.add({ text: `${appliedCount} metadata velden toegepast.` });
};

const navigate = () => {
  if (
    previousRoute.value?.name === "mijn-publicaties" ||
    previousRoute.value?.name === "collega-publicaties"
  ) {
    router.push({ name: previousRoute.value.name, query: previousRoute.value?.query });
  } else {
    router.push({ name: "start" });
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
  },
  claim: async () => {
    if (!user.value || (await claimDialog.reveal()).isCanceled) return;

    const currentEigenaar = publicatie.value.eigenaar;

    // set current user as eigenaar
    publicatie.value.eigenaar = {
      identifier: user.value?.id,
      weergaveNaam: user.value?.fullName
    };

    try {
      await submitPublicatie();
    } catch {
      // reset view on error
      publicatie.value.eigenaar = currentEigenaar;
      return;
    }

    toast.add({ text: "De publicatie is succesvol geclaimd." });
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

</style>
