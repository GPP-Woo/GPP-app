<template>
  <section>
    <publicaties-overview-sort v-model:query-params="queryParams" />

    <publicaties-overview-pagination
      :paged-result="pagedResult"
      :page-count="pageCount"
      :page="queryParams.page"
      @onPrev="$emit(`onPrev`)"
      @onNext="$emit(`onNext`)"
    />
  </section>

  <ul class="reset card-link-list" aria-live="polite">
    <li
      v-for="{
        uuid,
        officieleTitel,
        verkorteTitel,
        registratiedatum,
        publicatiestatus,
        eigenaar
      } in pagedResult?.results"
      :key="uuid"
    >
      <router-link
        :to="{ name: 'publicatie', params: { uuid } }"
        :title="officieleTitel"
        class="card-link"
        :class="{
          draft: publicatiestatus === PublicatieStatus.concept,
          'icon-after pen': eigenaar?.identifier === user?.id
        }"
      >
        <h2 :aria-describedby="`status-${uuid}`">
          <s v-if="publicatiestatus === PublicatieStatus.ingetrokken">{{ officieleTitel }}</s>

          <template v-else>
            {{ officieleTitel }}
          </template>
        </h2>

        <h3 v-if="verkorteTitel">{{ verkorteTitel }}</h3>

        <span
          :id="`status-${uuid}`"
          role="status"
          class="alert"
          :class="{ danger: publicatiestatus === PublicatieStatus.concept }"
        >
          <template v-if="publicatiestatus === PublicatieStatus.concept">
            Waarschuwing: deze publicatie is nog in concept!
          </template>

          <template v-if="publicatiestatus === PublicatieStatus.gepubliceerd">
            Deze publicatie is gepubliceerd.
          </template>

          <template v-if="publicatiestatus === PublicatieStatus.ingetrokken">
            Deze publicatie is ingetrokken.
          </template>
        </span>

        <dl>
          <dt>Registratiedatum:</dt>
          <dd>
            {{
              registratiedatum &&
              Intl.DateTimeFormat("NL-nl", { dateStyle: "long" }).format(
                Date.parse(registratiedatum)
              )
            }}
          </dd>
        </dl>
      </router-link>
    </li>
  </ul>
</template>

<script setup lang="ts">
import { type DeepReadonly } from "vue";
import type { PagedResult } from "@/api";
import { useAppData } from "@/composables/use-app-data";
import { PublicatieStatus, type Publicatie } from "../types";
import PublicatiesOverviewSort from "./PublicatiesOverviewSort.vue";
import PublicatiesOverviewPagination from "./PublicatiesOverviewPagination.vue";

defineProps<{ pagedResult: DeepReadonly<PagedResult<Publicatie>> | null; pageCount: number }>();

defineEmits<{ onPrev: []; onNext: [] }>();

const queryParams = defineModel<{ page: string; sorteer: string }>("queryParams", {
  required: true
});

const { user } = useAppData();
</script>

<style lang="scss" scoped>
@use "@/assets/mixins";
@include mixins.reset-block-margins;

section {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(var(--section-width), 1fr));
  gap: var(--spacing-default);
  margin-block-end: var(--spacing-default);
}

.card-link-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-default);
}

dl {
  display: flex;
  margin-block: var(--spacing-small) 0;

  dd {
    color: var(--text-light);
    margin-inline-start: var(--spacing-extrasmall);
  }
}
</style>
