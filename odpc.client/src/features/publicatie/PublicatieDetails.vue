<template>
  <simple-spinner v-show="loading"></simple-spinner>

  <form v-if="!loading" @submit.prevent="submit" v-form-invalid-handler>
    <alert-inline v-if="mijnGebruikersgroepenError || !mijnGebruikersgroepen?.length"
      >Er is iets misgegaan bij het ophalen van de gegevens. Neem contact op met de
      beheerder.</alert-inline
    >

    <section v-else>
      <alert-inline v-if="publicatieError"
        >Er is iets misgegaan bij het ophalen van de publicatie...</alert-inline
      >

      <publicatie-form
        v-else
        v-model="publicatie"
        :forbidden="forbidden"
        :readonly="readonly"
        :mijn-gebruikersgroepen="mijnGebruikersgroepen"
        :gekoppelde-waardelijsten="gekoppeldeWaardelijsten"
      />

      <alert-inline v-if="documentenError"
        >Er is iets misgegaan bij het ophalen van de documenten...</alert-inline
      >

      <documenten-form
        v-else-if="publicatie.gebruikersgroep || readonly"
        v-model:files="files"
        v-model:documenten="documenten"
        :readonly="readonly"
      />
    </section>

    <div class="form-submit">
      <span class="required-message">Velden met (*) zijn verplicht</span>

      <menu class="reset">
        <li>
          <button type="button" title="Opslaan" class="button secondary" @click="navigate">
            Annuleren
          </button>
        </li>

        <li>
          <button type="submit" title="Opslaan" :disabled="readonly || error">Opslaan</button>
        </li>
      </menu>
    </div>

    <prompt-modal
      :dialog="dialog"
      :cancel-text="currentDialogConfig?.cancelText"
      :confirm-text="currentDialogConfig?.confirmText"
    >
      <section v-html="currentDialogConfig?.message"></section>
    </prompt-modal>
  </form>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useRouter } from "vue-router";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import AlertInline from "@/components/AlertInline.vue";
import PromptModal from "@/components/PromptModal.vue";
import { useMultipleConfirmDialogs } from "@/composables/use-multiple-confirm-dialogs";
import toast from "@/stores/toast";
import PublicatieForm from "./components/PublicatieForm.vue";
import DocumentenForm from "./components/DocumentenForm.vue";
import { usePublicatie } from "./composables/use-publicatie";
import { useDocumenten } from "./composables/use-documenten";
import { useMijnGebruikersgroepen } from "./composables/use-mijn-gebruikersgroepen";
import { PublicatieStatus } from "./types";
import { Dialogs } from "./dialogs";
import { previousRoute } from "@/router/guards";

const router = useRouter();

const props = defineProps<{ uuid?: string }>();

const { dialog, currentDialogConfig, showDialog } = useMultipleConfirmDialogs();

const loading = computed(
  () =>
    loadingPublicatie.value ||
    loadingDocumenten.value ||
    loadingMijnGebruikersgroepen.value ||
    loadingDocument.value ||
    uploadingFile.value
);

const error = computed(
  () =>
    !!publicatieError.value ||
    !!documentenError.value ||
    !!documentError.value ||
    !!mijnGebruikersgroepenError.value
);

const readonly = computed(
  () => initialStatus.value === PublicatieStatus.ingetrokken || forbidden.value
);

// Publicatie
const {
  publicatie,
  isFetching: loadingPublicatie,
  error: publicatieError,
  submitPublicatie
} = usePublicatie(props.uuid);

// Store initial publicatie status in seperate ref to manage UI-state
const initialStatus = ref<keyof typeof PublicatieStatus>(PublicatieStatus.gepubliceerd);

watch(loadingPublicatie, () => (initialStatus.value = publicatie.value.publicatiestatus));

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
} = useMijnGebruikersgroepen(() => publicatie.value.gebruikersgroep);

const clearPublicatieWaardelijsten = () =>
  (publicatie.value = {
    ...publicatie.value,
    ...{
      publisher: "",
      informatieCategorieen: [],
      onderwerpen: []
    }
  });

watch(loading, () => {
  if (error.value) return;

  // Preset gebruikersgroep of a new publicatie when only one mijnGebruikersgroep
  if (!publicatie.value.uuid && mijnGebruikersgroepen.value?.length === 1) {
    publicatie.value.gebruikersgroep = mijnGebruikersgroepen.value[0].uuid;
  }

  // When publicatie is created outside the app user has to select gebruikersgroep
  // Clear waardelijsten of publicatie to prevent mismatch on waardelijsten
  if (publicatie.value.uuid && !publicatie.value.gebruikersgroep) {
    clearPublicatieWaardelijsten();
  }
});

// Clear waardelijsten of publicatie on change gebruikersgroep
watch(
  () => publicatie.value.gebruikersgroep,
  (_, oldValue) => oldValue && clearPublicatieWaardelijsten()
);

const forbidden = computed(() => {
  if (!publicatie.value.gebruikersgroep) return false;

  return (
    // Gebruiker not assigned to gebruikersgroep of the publicatie
    !mijnGebruikersgroepen.value?.some(
      (groep) => groep.uuid === publicatie.value.gebruikersgroep
    ) ||
    // Gebruikersgroep is not assigned to any organisatie
    !gekoppeldeWaardelijsten.value.organisaties?.length ||
    // Gebruikersgroep is not assigned to any informatiecategorie
    !gekoppeldeWaardelijsten.value.informatiecategorieen?.length ||
    // Gebruikersgroep is not assigned to publisher organisatie
    (publicatie.value.publisher &&
      !gekoppeldeWaardelijstenUuids.value?.includes(publicatie.value.publisher)) ||
    // Gebruikersgroep is not assigned to every informatiecategorie of publicatie
    !publicatie.value.informatieCategorieen.every((uuid: string) =>
      gekoppeldeWaardelijstenUuids.value?.includes(uuid)
    ) ||
    // Gebruikersgroep is not assigned to every onderwerp of publicatie
    !publicatie.value.onderwerpen.every((uuid: string) =>
      gekoppeldeWaardelijstenUuids.value?.includes(uuid)
    )
  );
});

const navigate = () => {
  if (previousRoute.value?.name === "publicaties") {
    router.push({ name: previousRoute.value.name, query: previousRoute.value?.query });
  } else {
    router.push({ name: "publicaties" });
  }
};

const submit = async () => {
  if (documenten.value.length === 0) {
    const { isCanceled } = await showDialog(Dialogs.noDocumenten);

    if (isCanceled) return;
  } else if (publicatie.value.publicatiestatus === PublicatieStatus.ingetrokken) {
    const { isCanceled } = await showDialog(Dialogs.retractPublicatie);

    if (isCanceled) {
      // Reset publicatie status in model to 'gepubliceerd' when user doesn't want to retract
      publicatie.value.publicatiestatus = PublicatieStatus.gepubliceerd;

      return;
    }
  }

  try {
    await submitPublicatie();

    // As soon as a publicatie gets status 'ingetrokken' in ODRC, the associated documents will
    // be automatically set to 'ingetrokken' as well and can no longer be updated from ODPC
    if (publicatie.value.publicatiestatus !== PublicatieStatus.ingetrokken)
      await submitDocumenten();
  } catch {
    return;
  }

  toast.add({ text: "De publicatie is succesvol opgeslagen." });

  navigate();
};
</script>

<style lang="scss" scoped>
section {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(var(--section-width), 1fr));
  grid-gap: var(--spacing-default);
}
</style>
