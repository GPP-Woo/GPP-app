<template>
  <simple-spinner v-if="loading" />

  <alert-inline v-else-if="error"
    >Er is iets misgegaan bij het ophalen van de waardelijsten...</alert-inline
  >

  <fieldset v-else>
    <legend>Waardelijsten</legend>

    <option-group
      :title="WAARDELIJSTEN.ORGANISATIE"
      :options="lists.organisaties"
      v-model="model"
    />

    <option-group
      :title="WAARDELIJSTEN.INFORMATIECATEGORIE"
      :options="lists.informatiecategorieen"
      v-model="model"
    />

    <option-group
      v-if="lists.onderwerpen.length"
      :title="WAARDELIJSTEN.ONDERWERP"
      :options="lists.onderwerpen"
      v-model="model"
    />
  </fieldset>
</template>

<script setup lang="ts">
import { useModel, watch, computed } from "vue";
import OptionGroup from "@/components/option-group/OptionGroup.vue";
import AlertInline from "@/components/AlertInline.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { WAARDELIJSTEN } from "../types";
import { fetchAllPages } from "@/composables/use-all-pages";
import type { OptionProps } from "@/components/option-group/types";
import { useFetchLists } from "@/composables/use-fetch-lists";

const props = defineProps<{ modelValue: string[] }>();

const model = useModel(props, "modelValue");

// Lijsten
const lijstUrls = {
  organisaties: "/api/v1/organisaties",
  informatiecategorieen: "/api/v1/informatiecategorieen",
  onderwerpen: "/api/v1/onderwerpen"
} as const;

const {
  lists,
  uuids,
  loading,
  error: fetchError
} = useFetchLists<keyof typeof lijstUrls, OptionProps>(lijstUrls, fetchAllPages);

const error = computed(
  () =>
    fetchError.value ||
    !lists.value.organisaties.length ||
    !lists.value.informatiecategorieen.length
);

// After loading remove uuids from model that are not present/active anymore in ODRC
watch(
  loading,
  () =>
    !error.value && (model.value = model.value.filter((uuid: string) => uuids.value.includes(uuid)))
);
</script>
