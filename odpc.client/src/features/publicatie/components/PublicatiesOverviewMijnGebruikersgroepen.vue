<template>
  <div v-if="mijnGebruikersgroepen?.length" class="form-group">
    <label for="eigenaarGroep">Gebruikersgroep</label>

    <select id="eigenaarGroep" v-model="queryParams.eigenaarGroep" :disabled="isLoading">
      <option v-if="!queryParams.eigenaarGroep" value="">
        -- Selecteer een gebruikersgroep --
      </option>

      <option v-for="groep in mijnGebruikersgroepen" :key="groep.uuid" :value="groep.uuid">
        {{ groep.naam }}
      </option>
    </select>
  </div>
</template>

<script setup lang="ts">
import type { DeepReadonly } from "vue";
import { type MijnGebruikersgroep } from "../types";

defineProps<{
  mijnGebruikersgroepen?: DeepReadonly<MijnGebruikersgroep[]> | null;
  isLoading: boolean;
}>();

const queryParams = defineModel<{ eigenaarGroep: string }>("queryParams", { required: true });
</script>

<style lang="scss" scoped>
.form-group {
  flex-direction: row;
  align-items: center;

  label {
    margin-block: 0;
    margin-inline-end: var(--spacing-default);
  }

  select {
    min-inline-size: 20rem;
  }
}
</style>
