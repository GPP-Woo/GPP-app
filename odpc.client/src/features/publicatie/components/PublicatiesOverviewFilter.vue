<template>
  <fieldset>
    <legend class="visually-hidden">Filter op</legend>

    <div
      v-if="informatiecategorieen?.length"
      class="form-group"
      role="group"
      aria-labelledby="informatiecategorieen-label"
    >
      <label id="informatiecategorieen-label" for="informatiecategorieen"
        >Filter op informatiecategorie</label
      >

      <select
        name="informatiecategorieen"
        id="informatiecategorieen"
        v-model="queryParams.informatieCategorieen"
        aria-label="Filter publicaties op informatiecategorie"
      >
        <option value="">Alle informatiecategorieën</option>

        <option v-for="{ uuid, naam } in informatiecategorieen" :key="uuid" :value="uuid">
          {{ naam }}
        </option>
      </select>
    </div>

    <div
      v-if="onderwerpen?.length"
      class="form-group"
      role="group"
      aria-labelledby="onderwerpen-label"
    >
      <label id="onderwerpen-label" for="onderwerpen">Filter op onderwerp</label>

      <select
        name="onderwerpen"
        id="onderwerpen"
        v-model="queryParams.onderwerpen"
        aria-label="Filter publicaties op onderwerp"
      >
        <option value="">Alle onderwerpen</option>

        <option v-for="{ uuid, naam } in onderwerpen" :key="uuid" :value="uuid">
          {{ naam }}
        </option>
      </select>
    </div>

    <div class="form-group" role="group" aria-labelledby="publicatiestatus-label">
      <label id="publicatiestatus-label" for="publicatiestatus">Filter op publicatiestatus</label>

      <select
        name="publicatiestatus"
        id="publicatiestatus"
        v-model="queryParams.publicatiestatus"
        aria-label="Filter publicaties op publicatiestatus"
      >
        <option value="">Alle publicatiestatussen</option>

        <option v-for="status in Object.values(PublicatieStatus)" :key="status" :value="status">
          {{ status }}
        </option>
      </select>
    </div>
  </fieldset>
</template>

<script setup lang="ts">
import type { DeepReadonly } from "vue";
import { PublicatieStatus, type WaardelijstItem } from "../types";

defineProps<{
  informatiecategorieen?: DeepReadonly<WaardelijstItem[]>;
  onderwerpen?: DeepReadonly<WaardelijstItem[]>;
}>();

const queryParams = defineModel<{
  informatieCategorieen: string;
  onderwerpen: string;
  publicatiestatus: string;
}>("queryParams", { required: true });
</script>

<style lang="scss" scoped>
fieldset {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(var(--section-width-small), 1fr));
  gap: var(--spacing-default);
  margin-block-end: var(--spacing-large);
  padding: 0;
  border: none;
}
</style>
