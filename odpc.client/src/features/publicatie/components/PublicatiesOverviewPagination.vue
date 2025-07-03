<template>
  <div class="pagination">
    <p aria-live="polite">
      <strong>{{ pagedResult?.count || 0 }}</strong>
      {{ pagedResult?.count === 1 ? "resultaat" : "resultaten" }}
    </p>

    <menu class="reset">
      <li>
        <button
          type="button"
          aria-label="Vorige pagina"
          :disabled="!pagedResult?.previous"
          @click="$emit(`onPrev`)"
        >
          &laquo;
        </button>
      </li>

      <li>pagina {{ page }} van {{ pageCount }}</li>

      <li>
        <button
          type="button"
          aria-label="Volgende pagina"
          :disabled="!pagedResult?.next"
          @click="$emit(`onNext`)"
        >
          &raquo;
        </button>
      </li>
    </menu>
  </div>
</template>

<script setup lang="ts">
import type { PagedResult } from "@/api";
import type { Publicatie } from "../types";
import type { DeepReadonly } from "vue";

defineProps<{
  pagedResult: DeepReadonly<PagedResult<Publicatie>> | null;
  pageCount: number;
  page: string;
}>();
</script>

<style lang="scss" scoped>
.pagination {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  column-gap: var(--spacing-large);

  menu {
    display: flex;
    align-items: center;
    gap: var(--spacing-default);
  }

  button {
    padding-block: var(--spacing-extrasmall);
  }
}
</style>
