<template>
  <alert-inline v-if="!lijsten?.organisaties.length || !lijsten?.informatiecategorieen.length"
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
      v-if="lijsten?.onderwerpen.length"
      :title="WAARDELIJSTEN.ONDERWERP"
      :options="lijsten.onderwerpen.map(({ omschrijving, ...rest }) => rest)"
      v-model="model"
    />
  </fieldset>
</template>

<script setup lang="ts">
import { useModel, computed, onMounted } from "vue";
import OptionGroup from "@/components/option-group/OptionGroup.vue";
import AlertInline from "@/components/AlertInline.vue";
import { WAARDELIJSTEN } from "../types";
import { useAppData } from "@/composables/use-app-data";

const props = defineProps<{ modelValue: string[] }>();

const model = useModel(props, "modelValue");

const { lijsten } = useAppData();

const uuids = computed(() =>
  lijsten.value
    ? Object.values(lijsten.value)
        .flat()
        .map((item) => item.uuid)
    : []
);

// Remove uuids from model that are not present/active anymore in ODRC
onMounted(() => (model.value = model.value.filter((uuid: string) => uuids.value.includes(uuid))));
</script>
