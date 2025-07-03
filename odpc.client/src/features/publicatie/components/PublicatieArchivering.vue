<template>
  <details>
    <summary>Archivering</summary>

    <dl>
      <dt>Bron bewaartermijn:</dt>
      <dd>{{ bronBewaartermijn || "-" }}</dd>

      <dt>Categorie in bron:</dt>
      <dd>{{ selectiecategorie || "-" }}</dd>

      <dt>Archief-actie:</dt>
      <dd>{{ archiefnominatie ? Archiefnominatie[archiefnominatie] : "-" }}</dd>

      <dt>Archief-actie gepland op:</dt>
      <dd>{{ formatDate(archiefactiedatum) ?? "-" }}</dd>

      <dt>Toelichting:</dt>
      <dd>{{ toelichtingBewaartermijn || "-" }}</dd>
    </dl>
  </details>
</template>

<script setup lang="ts">
import { formatDate } from "@/helpers";
import { Archiefnominatie, type Publicatie } from "../types";

defineProps<Publicatie>();
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
}
</style>
