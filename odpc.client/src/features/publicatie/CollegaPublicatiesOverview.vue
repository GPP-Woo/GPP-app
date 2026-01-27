<template>
  <h1>Publicaties van collega's</h1>

  <publicaties-overview-mijn-gebruikersgroepen
    v-model:query-params="queryParams"
    :mijn-gebruikersgroepen="mijnGebruikersgroepen"
    :is-loading="isLoading"
  />

  <form v-if="queryParams.eigenaarGroep" @submit.prevent="search">
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
import { watch } from "vue";
import { useRoute } from "vue-router";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import AlertInline from "@/components/AlertInline.vue";
import { usePublicatiesOverview } from "./composables/use-publicaties-overview";
import PublicatiesOverviewMijnGebruikersgroepen from "./components/PublicatiesOverviewMijnGebruikersgroepen.vue";
import PublicatiesOverviewSearch from "./components/PublicatiesOverviewSearch.vue";
import PublicatiesOverviewFilter from "./components/PublicatiesOverviewFilter.vue";
import PublicatiesOverviewResult from "./components/PublicatiesOverviewResult.vue";

const route = useRoute();

const {
  searchString,
  fromDate,
  untilDateInclusive,
  mijnGebruikersgroepen,
  mijnWaardelijsten,
  queryParams,
  pagedResult,
  pageCount,
  isLoading,
  hasError,
  search,
  onNext,
  onPrev
} = usePublicatiesOverview();

// in eigenaarGroepMode preset eigenaarGroep when:
// a. only one mijn-gebruikersgroep
// b. valid eigenaarGroep from query
watch(mijnGebruikersgroepen, (groepen) => {
  if (!groepen?.length) return;

  const { eigenaarGroep } = route.query;

  if (groepen.length === 1) {
    queryParams.value.eigenaarGroep = groepen[0].uuid;
  } else if (eigenaarGroep && groepen.some((groep) => groep.uuid === eigenaarGroep)) {
    queryParams.value.eigenaarGroep = eigenaarGroep as string;
  }
});
</script>

<style lang="scss" scoped>
// clear margins, use gaps
:deep(*:not(fieldset, label)) {
  margin-block: 0;
}
</style>
