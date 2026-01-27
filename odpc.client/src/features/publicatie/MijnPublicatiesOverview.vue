<template>
  <h1>Mijn publicaties</h1>

  <menu class="reset">
    <li>
      <router-link :to="{ name: 'publicatie' }" class="button icon-after note"
        >Nieuwe publicatie</router-link
      >
    </li>
  </menu>

  <form @submit.prevent="search">
    <publicaties-overview-search
      v-model:search-string="searchString"
      v-model:from-date="fromDate"
      v-model:until-date-inclusive="untilDateInclusive"
      :disabled="isLoading"
    />

    <publicaties-overview-filter
      v-model:query-params="queryParams"
      :informatiecategorieen="mijnWaardelijsten.informatiecategorieen"
      :onderwerpen="mijnWaardelijsten.onderwerpen"
      :disabled="isLoading"
    />
  </form>

  <simple-spinner v-if="isLoading"></simple-spinner>

  <alert-inline v-else-if="hasError">Er is iets misgegaan, probeer het nogmaals.</alert-inline>

  <template v-else-if="pageCount">
    <publicaties-overview-result
      :paged-result="pagedResult"
      :page-count="pageCount"
      :query-params="queryParams"
      @onPrev="onPrev"
      @onNext="onNext"
    />
  </template>

  <alert-inline v-else>Geen publicaties gevonden.</alert-inline>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import AlertInline from "@/components/AlertInline.vue";
import { usePublicatiesOverview } from "./composables/use-publicaties-overview";
import PublicatiesOverviewSearch from "./components/PublicatiesOverviewSearch.vue";
import PublicatiesOverviewFilter from "./components/PublicatiesOverviewFilter.vue";
import PublicatiesOverviewResult from "./components/PublicatiesOverviewResult.vue";

const {
  searchString,
  fromDate,
  untilDateInclusive,
  mijnWaardelijsten,
  queryParams,
  pagedResult,
  pageCount,
  isLoading,
  hasError,
  search,
  initPagedSearch,
  onNext,
  onPrev
} = usePublicatiesOverview();

onMounted(() => initPagedSearch());
</script>

<style lang="scss" scoped>
// clear margins, use gaps
:deep(*:not(fieldset, label)) {
  margin-block: 0;
}

menu {
  display: flex;
  margin-block-end: var(--spacing-default);
}
</style>
