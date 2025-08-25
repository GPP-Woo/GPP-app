<template>
  <h1>Overzicht publicaties</h1>

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
    <section>
      <publicaties-overview-sort v-model:query-params="queryParams" />

      <publicaties-overview-pagination
        :paged-result="pagedResult"
        :page-count="pageCount"
        :page="queryParams.page"
        @onPrev="onPrev"
        @onNext="onNext"
      />
    </section>

    <ul class="reset card-link-list" aria-live="polite">
      <li
        v-for="{
          uuid,
          officieleTitel,
          verkorteTitel,
          registratiedatum,
          publicatiestatus
        } in pagedResult?.results"
        :key="uuid"
      >
        <router-link
          :to="{ name: 'publicatie', params: { uuid } }"
          :title="officieleTitel"
          class="card-link icon-after pen"
          :class="{ draft: publicatiestatus === PublicatieStatus.concept }"
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
                Intl.DateTimeFormat("default", { dateStyle: "long" }).format(
                  Date.parse(registratiedatum)
                )
              }}
            </dd>
          </dl>
        </router-link>
      </li>
    </ul>
  </template>

  <alert-inline v-else>Geen publicaties gevonden.</alert-inline>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import AlertInline from "@/components/AlertInline.vue";
import { usePagedSearch } from "@/composables/use-paged-search";
import { useMijnWaardelijsten } from "./composables/use-mijn-waardelijsten";
import { PublicatieStatus, type Publicatie } from "./types";
import PublicatiesOverviewSearch from "./components/PublicatiesOverviewSearch.vue";
import PublicatiesOverviewFilter from "./components/PublicatiesOverviewFilter.vue";
import PublicatiesOverviewSort from "./components/PublicatiesOverviewSort.vue";
import PublicatiesOverviewPagination from "./components/PublicatiesOverviewPagination.vue";

const addDays = (dateString: string, days: number) => {
  if (!dateString) return dateString;

  const date = new Date(dateString);
  const nextDateUtc = new Date(
    Date.UTC(date.getFullYear(), date.getMonth(), date.getDate() + days)
  );

  return nextDateUtc.toISOString().substring(0, 10);
};

const searchString = ref(""); // search
const fromDate = ref(""); // registratiedatumVanaf
const untilDateInclusive = ref("");

// we zoeken met een datum in een datum-tijd veld, daarom corrigeren we de datum hier
// registratiedatumTot
const untilDateExclusive = computed({
  get: () => addDays(untilDateInclusive.value, 1),
  set: (v) => (untilDateInclusive.value = addDays(v, -1))
});

const isLoading = computed(() => loadingMijnWaardelijsten.value || loadingPageResult.value);
const hasError = computed(() => !!mijnWaardelijstenError.value || !!pagedResultError.value);

const {
  mijnWaardelijsten,
  isFetching: loadingMijnWaardelijsten,
  error: mijnWaardelijstenError
} = useMijnWaardelijsten();

const syncFromQuery = () => {
  const { search, registratiedatumVanaf, registratiedatumTot } = queryParams.value;

  [searchString.value, fromDate.value, untilDateExclusive.value] = [
    search,
    registratiedatumVanaf,
    registratiedatumTot
  ];
};

const syncToQuery = () => {
  Object.assign(queryParams.value, {
    search: searchString.value,
    registratiedatumVanaf: fromDate.value,
    registratiedatumTot: untilDateExclusive.value
  });
};

const QueryParamsConfig = {
  page: "1",
  sorteer: "-registratiedatum",
  search: "", // searchString
  registratiedatumVanaf: "", // fromDate
  registratiedatumTot: "", // untilDateExclusive
  informatieCategorieen: "",
  onderwerpen: "",
  publicatiestatus: ""
};

const {
  queryParams,
  pagedResult,
  pageCount,
  isFetching: loadingPageResult,
  error: pagedResultError,
  onNext,
  onPrev
} = usePagedSearch<Publicatie, typeof QueryParamsConfig>("publicaties", QueryParamsConfig);

// sync linked refs from queryParams / urlSearchParams
watch(queryParams, syncFromQuery, { deep: true });

// sync linked refs to queryParams onSearch
const search = syncToQuery;
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
