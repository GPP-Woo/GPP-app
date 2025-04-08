<template>
  <simple-spinner v-if="loading" />

  <alert-inline v-else-if="error || !data.organisaties.length || !data.informatiecategorieen.length"
    >Er is iets misgegaan bij het ophalen van de waardelijsten...</alert-inline
  >

  <fieldset v-else>
    <legend>Waardelijsten</legend>

    <option-group :title="WAARDELIJSTEN.ORGANISATIE" :options="data.organisaties" v-model="model" />

    <option-group
      :title="WAARDELIJSTEN.INFORMATIECATEGORIE"
      :options="data.informatiecategorieen"
      v-model="model"
    />

    <option-group
      v-if="data.onderwerpen.length"
      :title="WAARDELIJSTEN.ONDERWERP"
      :options="data.onderwerpen"
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

const lijsten = ["organisaties", "informatiecategorieen", "onderwerpen"] as const;

const loading = ref(false);
const error = ref(false);

const data = reactive(
  lijsten.reduce(
    (acc, key) => ({ ...acc, [key]: [] }),
    {} as Record<(typeof lijsten)[number], OptionProps[]>
  )
);

const getLijsten = async () => {
  loading.value = true;

  try {
    const results = await Promise.allSettled(
      lijsten.map((lijst) =>
        fetchAllPages<OptionProps | Onderwerp>(`/api/v1/${lijst}`).then((data) => {
          const mappedData = data.map(
            (option) =>
              ("officieleTitel" in option && {
                uuid: option.uuid,
                naam: option.officieleTitel
              }) ||
              option
          ) as OptionProps[];

          return mappedData;
        })
      )
    );

    results.forEach((result, index) =>
      result.status === "fulfilled" ? (data[lijsten[index]] = result.value) : (error.value = true)
    );
  } catch {
    error.value = true;
  } finally {
    loading.value = false;
  }
};

// After loading remove uuids from model that are not present/active anymore in ODRC
watch(loading, () => {
  const uuids = lijsten.flatMap((key) => data[key].map((item) => item.uuid));

  model.value = model.value.filter((uuid: string) => uuids.includes(uuid));
});

onMounted(() => getLijsten());
</script>
