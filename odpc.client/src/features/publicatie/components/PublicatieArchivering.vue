<template>
  <details>
    <summary>Archivering</summary>

    <p v-if="noValuesAvailable">Er zijn geen gegevens over de bewaartermijn bekend.</p>

    <dl v-else>
      <dt>Bron bewaartermijn:</dt>
      <dd>{{ bronBewaartermijn }}</dd>

      <dt>Categorie in bron:</dt>
      <dd>{{ selectiecategorie }}</dd>

      <dt>Archief-actie:</dt>
      <dd>{{ archiefnominatie ? Archiefnominatie[archiefnominatie] : "" }}</dd>

      <dt>Archief-actie gepland op:</dt>
      <dd>{{ formatDate(archiefactiedatum) }}</dd>

      <dt>Toelichting:</dt>
      <dd>{{ toelichtingBewaartermijn }}</dd>
    </dl>
  </details>
</template>

<script setup lang="ts">
import { formatDate } from "@/helpers";
import { Archiefnominatie, type Publicatie } from "../types";
import { computed } from "vue";

const props = defineProps<Publicatie>();

const noValuesAvailable = computed(
  () =>
    !props.bronBewaartermijn &&
    !props.selectiecategorie &&
    !props.archiefnominatie &&
    !props.archiefactiedatum &&
    !props.toelichtingBewaartermijn
);
</script>

<style lang="scss" scoped>
dl {
  display: grid;
  grid-template-columns: max-content 1fr;
  gap: var(--spacing-default);
  margin: 0;

  dt {
    grid-column: 1;
    font-weight: 600;
    color: var(--text);
  }

  dd {
    grid-column: 2;
    font-style: italic;
    margin: 0;
  }

  dd:empty,
  dt:has(+ dd:empty) {
    display: none;
  }
}
</style>
