<template>
  <simple-spinner v-show="loading"></simple-spinner>

  <form v-if="!loading" @submit.prevent="submit" v-form-invalid-handler>
    <alert-inline v-if="mijnWaardelijstenError"
      >Er is iets misgegaan bij het ophalen van uw gegevens...</alert-inline
    >

    <section v-else-if="!forbidden">
      <alert-inline v-if="publicatieError"
        >Er is iets misgegaan bij het ophalen van de publicatie...</alert-inline
      >

      <publicatie-form
        v-else
        v-model="publicatie"
        :disabled="initialStatus === PublicatieStatus.ingetrokken"
        :mijn-organisaties="mijnWaardelijsten.mijnOrganisaties || []"
        :mijn-informatiecategorieen="mijnWaardelijsten.mijnInformatiecategorieen || []"
      />

      <alert-inline v-if="documentenError"
        >Er is iets misgegaan bij het ophalen van de documenten...</alert-inline
      >

      <document-form
        v-else
        v-model:files="files"
        v-model:documenten="documenten"
        :disabled="initialStatus === PublicatieStatus.ingetrokken"
      />
    </section>

    <alert-inline v-else
      >U bent niet gekoppeld aan een (juiste) gebruikersgroep. Neem contact op met uw
      beheerder.</alert-inline
    >

    <div class="form-submit">
      <span class="required-message">Velden met (*) zijn verplicht</span>

      <menu class="reset">
        <li>
          <button type="button" title="Opslaan" class="button secondary" @click="navigate">
            Annuleren
          </button>
        </li>

        <li>
          <button
            type="submit"
            title="Opslaan"
            :disabled="error || initialStatus === PublicatieStatus.ingetrokken"
          >
            Opslaan
          </button>
        </li>
      </menu>
    </div>

    <prompt-modal
      :dialog="dialog"
      confirm-message="Ja, intrekken"
      cancel-message="Nee, gepubliceerd laten"
    >
      <span>Weet u zeker dat u dit deze publicatie wilt intrekken?</span>
      <span><strong>Let op:</strong> deze actie kan niet ongedaan worden gemaakt.</span>
    </prompt-modal>
  </form>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useRouter } from "vue-router";
import { previousRoute } from "@/router";
import { useConfirmDialog } from "@vueuse/core";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import AlertInline from "@/components/AlertInline.vue";
import PromptModal from "@/components/PromptModal.vue";
import toast from "@/stores/toast";
import PublicatieForm from "./components/PublicatieForm.vue";
import DocumentForm from "./components/DocumentForm.vue";
import { usePublicatie } from "./composables/use-publicatie";
import { useDocumenten } from "./composables/use-documenten";
import { PublicatieStatus } from "./types";
import { useFetchLists } from "@/composables/use-fetch-lists";
import type { OptionProps } from "@/components/option-group/types";

const router = useRouter();

const props = defineProps<{ uuid?: string }>();

const dialog = useConfirmDialog();

const loading = computed(
  () =>
    loadingPublicatie.value ||
    loadingDocumenten.value ||
    loadingMijnWaardelijsten.value ||
    loadingDocument.value ||
    uploadingFile.value
);

const error = computed(
  () =>
    !!publicatieError.value ||
    !!documentenError.value ||
    !!documentError.value ||
    !!mijnWaardelijstenError.value ||
    forbidden.value
);

// Publicatie
const { publicatie, publicatieError, loadingPublicatie, submitPublicatie } = usePublicatie(
  props.uuid
);

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
  useDocumenten(computed(() => props.uuid || publicatie.value?.uuid));

// Mijn waardelijsten
const waardelijstUrls = {
  mijnOrganisaties: "/api/v1/mijn-organisaties",
  mijnInformatiecategorieen: "/api/v1/mijn-informatiecategorieen"
} as const;

const {
  lists: mijnWaardelijsten,
  uuids: mijnWaardelijstenUuids,
  loading: loadingMijnWaardelijsten,
  error: mijnWaardelijstenError
} = useFetchLists<keyof typeof waardelijstUrls, OptionProps>(waardelijstUrls);

const forbidden = computed(
  () =>
    // Not assigned to any organisatie
    !mijnWaardelijsten.value.mijnOrganisaties.length ||
    // Not assigned to any informatiecategorie
    !mijnWaardelijsten.value.mijnInformatiecategorieen.length ||
    // Not assigned to publisher organisatie
    (publicatie.value.publisher &&
      !mijnWaardelijstenUuids.value.includes(publicatie.value.publisher)) ||
    // Not assigned to every informatiecategorie of publicatie
    !publicatie.value.informatieCategorieen.every((uuid: string) =>
      mijnWaardelijstenUuids.value.includes(uuid)
    )
);

const navigate = () => {
  if (previousRoute.value?.name === "publicaties") {
    router.push({ name: previousRoute.value.name, query: previousRoute.value?.query });
  } else {
    router.push({ name: "publicaties" });
  }
};

const submit = async () => {
  if (publicatie.value.publicatiestatus === PublicatieStatus.ingetrokken) {
    const { isCanceled } = await dialog.reveal();

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
