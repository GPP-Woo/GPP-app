<template>
  <alert-inline v-if="!error">
    Sluit dit scherm niet en blijf op deze pagina tot de upload is afgerond.
  </alert-inline>

  <alert-inline v-else>
    Niet alle documenten zijn opgeslagen. Controleer de publicatie en voeg ontbrekende documenten
    opnieuw toe.
  </alert-inline>

  <section class="upload-progress">
    <template v-if="!error">
      <p role="status">{{ completed }} van {{ documenten.length }} documenten geüpload</p>

      <progress :value="total" max="100" aria-label="Totale voortgang"></progress>
    </template>

    <p v-else role="status">{{ failed }} van {{ documenten.length }} documenten mislukt</p>

    <ul class="reset">
      <li
        v-for="(doc, index) in documenten"
        :key="`${index}-${doc.bestandsnaam}`"
        :class="doc._client?.error ? UploadStatus.failed : doc._client?.upload?.status"
      >
        <span class="name" :title="doc.bestandsnaam">{{ doc.bestandsnaam }}</span>

        <span class="status">
          <template v-if="doc._client?.error">{{ doc._client.error }}</template>
          <template v-else-if="doc._client?.upload?.status === UploadStatus.uploading"
            >{{ doc._client.upload.progress }}%</template
          >
          <template v-else-if="doc._client?.upload">{{
            statusLabels[doc._client.upload.status]
          }}</template>
        </span>
      </li>
    </ul>
  </section>

  <button v-if="error" type="button" class="button" @click="emit('back')">
    Terug naar publicatie
  </button>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { UploadStatus, type PublicatieDocument } from "../types";
import AlertInline from "@/components/AlertInline.vue";

const props = defineProps<{
  documenten: PublicatieDocument[];
  total: number;
  error: boolean;
}>();

const emit = defineEmits<{ back: [] }>();

const statusLabels: Record<UploadStatus, string> = {
  pending: "Wachten",
  uploading: "Bezig",
  done: "Klaar",
  failed: "Mislukt"
};

const completed = computed(
  () => props.documenten.filter((doc) => doc._client?.upload?.status === UploadStatus.done).length
);

const failed = computed(() => props.documenten.filter((doc) => doc._client?.error).length);
</script>

<style lang="scss" scoped>
.upload-progress {
  display: flex;
  flex-direction: column;
  row-gap: var(--spacing-default);
  padding: var(--spacing-default);
  margin-bottom: var(--spacing-default);
  border: 1px solid var(--border);
  border-radius: var(--standard-border-radius);
}

progress {
  inline-size: 100%;
}

[role="status"] {
  font-style: italic;
  text-align: end;
  margin-block: 0;
}

ul {
  li {
    display: grid;
    grid-template-columns: minmax(0, 4fr) 1fr;
    column-gap: var(--spacing-default);
    padding: var(--spacing-small);
    border-block-start: 1px solid var(--border);

    &.uploading {
      font-style: italic;
    }

    &.done {
      font-weight: 600;
    }

    &.failed {
      color: var(--danger);
    }
  }

  .name {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .status {
    padding-inline-start: var(--spacing-default);
    border-inline-start: 1px solid var(--border);
  }
}
</style>
