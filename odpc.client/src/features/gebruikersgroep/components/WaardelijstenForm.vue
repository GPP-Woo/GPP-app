<template>
  <simple-spinner v-if="loading" />

  <alert-inline v-else-if="error"
    >Er is iets misgegaan bij het ophalen van de waardelijsten...</alert-inline
  >

  <fieldset v-else>
    <legend>Waardelijsten</legend>

    <option-group :title="WAARDELIJSTEN.ORGANISATIE" :options="organisaties" v-model="model" />

    <option-group
      :title="WAARDELIJSTEN.INFORMATIECATEGORIE"
      :options="informatiecategorieen"
      v-model="model"
    />

    <option-group :title="WAARDELIJSTEN.ONDERWERP" :options="onderwerpen" v-model="model" />
  </fieldset>
</template>

<script setup lang="ts">
import { computed, useModel, watch } from "vue";
import OptionGroup from "@/components/option-group/OptionGroup.vue";
import AlertInline from "@/components/AlertInline.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { WAARDELIJSTEN, type WaardelijstItem } from "../types";
import { useAllPages } from "@/composables/use-all-pages";

const props = defineProps<{ modelValue: string[] }>();

const model = useModel(props, "modelValue");

const {
  data: organisaties,
  loading: organisatiesLoading,
  error: organisatiesError
} = useAllPages<WaardelijstItem>("/api/v1/organisaties");

const {
  data: informatiecategorieen,
  error: informatiecategorieenError,
  loading: informatiecategorieenLoading
} = useAllPages<WaardelijstItem>("/api/v1/informatiecategorieen");

const {
  data,
  error: onderwerpenError,
  loading: onderwerpenLoading
} = useAllPages<{
  uuid: string;
  officieleTitel: string;
}>("/api/v1/onderwerpen");

// map Onderwerp to WaardelijstItem
const onderwerpen = computed<WaardelijstItem[]>(
  () => data.value.map((o) => ({ uuid: o.uuid, naam: o.officieleTitel })) ?? []
);

const error = computed(
  () =>
    organisatiesError.value ||
    !organisaties.value.length ||
    informatiecategorieenError.value ||
    !informatiecategorieen.value.length ||
    onderwerpenError.value ||
    !onderwerpen.value.length
);

const loading = computed(
  () => informatiecategorieenLoading.value || organisatiesLoading.value || onderwerpenLoading.value
);

const listsLoaded = computed(() => !!model.value.length && !error.value);

const uuids = computed(() => [
  ...organisaties.value.map((item) => item.uuid),
  ...informatiecategorieen.value.map((item) => item.uuid),
  ...onderwerpen.value.map((item) => item.uuid)
]);

// Remove uuids from model that are not present/active anymore in ODRC
watch(listsLoaded, (bool) => {
  if (bool) model.value = model.value.filter((uuid: string) => uuids.value.includes(uuid));
});
</script>
