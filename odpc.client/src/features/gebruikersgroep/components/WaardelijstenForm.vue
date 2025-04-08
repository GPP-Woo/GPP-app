<template>
  <simple-spinner v-if="loading" />

  <alert-inline
    v-else-if="error || !lijsten.organisaties.length || !lijsten.informatiecategorieen.length"
    >Er is iets misgegaan bij het ophalen van de waardelijsten...</alert-inline
  >

  <fieldset v-else>
    <legend>Waardelijsten</legend>

    <option-group
      :title="WAARDELIJSTEN.ORGANISATIE"
      :options="lijsten.organisaties"
      v-model="model"
    />

    <option-group
      :title="WAARDELIJSTEN.INFORMATIECATEGORIE"
      :options="lijsten.informatiecategorieen"
      v-model="model"
    />

    <option-group
      v-if="lijsten.onderwerpen.length"
      :title="WAARDELIJSTEN.ONDERWERP"
      :options="lijsten.onderwerpen"
      v-model="model"
    />
  </fieldset>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref, useModel, watch } from "vue";
import OptionGroup from "@/components/option-group/OptionGroup.vue";
import AlertInline from "@/components/AlertInline.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import { WAARDELIJSTEN } from "../types";
import { fetchAllPages } from "@/composables/use-all-pages";
import type { OptionProps } from "@/components/option-group/types";

type Onderwerp = {
  uuid: string;
  officieleTitel: string;
};

const props = defineProps<{ modelValue: string[] }>();

const model = useModel(props, "modelValue");

const loading = ref(false);
const error = ref(false);

const urls = {
  organisaties: "/api/v1/organisaties",
  informatiecategorieen: "/api/v1/informatiecategorieen",
  onderwerpen: "/api/v1/onderwerpen"
} as const;

const lijsten = reactive<Record<keyof typeof urls, OptionProps[]>>({
  organisaties: [],
  informatiecategorieen: [],
  onderwerpen: []
});

const getLijsten = async () => {
  loading.value = true;

  try {
    const results = await Promise.allSettled(
      Object.entries(urls).map(async ([key, url]) =>
        fetchAllPages<OptionProps | Onderwerp>(url).then((data) => {
          const mappedData = data.map(
            (option) =>
              ("officieleTitel" in option && {
                uuid: option.uuid,
                naam: option.officieleTitel
              }) ||
              option
          ) as OptionProps[];

          return { [key]: mappedData };
        })
      )
    );

    results.forEach((result) =>
      result.status === "fulfilled" ? Object.assign(lijsten, result.value) : (error.value = true)
    );
  } catch {
    error.value = true;
  } finally {
    loading.value = false;
  }
};

// After loading remove uuids from model that are not present/active anymore in ODRC
watch(loading, () => {
  const uuids = Object.values(lijsten).flatMap((items) => items.map((item) => item.uuid) || []);

  model.value = model.value.filter((uuid: string) => uuids.includes(uuid));
});

onMounted(() => getLijsten());
</script>
