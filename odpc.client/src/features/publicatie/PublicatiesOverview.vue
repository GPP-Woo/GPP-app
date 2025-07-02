<template>
  <h1>Overzicht publicaties</h1>

  <menu class="reset">
    <li>
      <router-link :to="{ name: 'publicatie' }" class="button icon-after note"
        >Nieuwe publicatie</router-link
      >
    </li>
  </menu>

  <form>
    <fieldset :disabled="isLoading" class="search">
      <legend>Zoek op</legend>

      <div class="form-group">
        <label for="zoeken">Titel</label>

        <input type="text" id="zoeken" v-model="searchString" @keydown.enter.prevent="onSearch" />
      </div>

      <date-range-picker v-model:from-date="fromDate" v-model:until-date="untilDateInclusive" />

      <div class="form-group-button">
        <button
          type="button"
          class="icon-after loupe"
          aria-label="Zoek"
          @click="onSearch"
          :disabled="isLoading"
        >
          Zoek
        </button>
      </div>
    </fieldset>

    <fieldset :disabled="isLoading" class="filter">
      <legend>Filter op</legend>

      <template v-for="{ label, searchParam, listKey } in filterConfig" :key="listKey">
        <div class="form-group" v-if="mijnWaardelijsten[listKey]?.length">
          <label for="sorteer">{{ label }}</label>

          <select :name="listKey" :id="listKey" v-model="queryParams[searchParam]">
            <option v-if="queryParams[searchParam]" value="">Verwijder filter</option>

            <option v-for="{ uuid, naam } in mijnWaardelijsten[listKey]" :key="uuid" :value="uuid">
              {{ naam }}
            </option>
          </select>
        </div>
      </template>
    </fieldset>
  </form>

  <simple-spinner v-if="isLoading"></simple-spinner>

  <alert-inline v-else-if="hasError">Er is iets misgegaan, probeer het nogmaals.</alert-inline>

  <template v-else-if="pageCount">
    <section>
      <div class="form-group form-group-inline">
        <label for="sorteer">Sorteer op</label>

        <select name="sorteer" id="sorteer" v-model="queryParams.sorteer">
          <option v-for="(value, key) in sortingOptions" :key="key" :value="key">
            {{ value }}
          </option>
        </select>
      </div>

      <div class="page-nav">
        <p>
          <strong>{{ pagedResult?.count || 0 }}</strong>
          {{ pagedResult?.count === 1 ? "resultaat" : "resultaten" }}
        </p>

        <menu class="reset">
          <li>
            <button
              type="button"
              aria-label="Vorige pagina"
              :disabled="!pagedResult?.previous"
              @click="onPrev"
            >
              &laquo;
            </button>
          </li>

          <li>pagina {{ queryParams.page }} van {{ pageCount }}</li>

          <li>
            <button
              type="button"
              aria-label="Volgende pagina"
              :disabled="!pagedResult?.next"
              @click="onNext"
            >
              &raquo;
            </button>
          </li>
        </menu>
      </div>
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
import { computed, ref, watch, type UnwrapRef } from "vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import AlertInline from "@/components/AlertInline.vue";
import DateRangePicker from "@/components/DateRangePicker.vue";
import { usePagedSearch } from "@/composables/use-paged-search";
import { useMijnWaardelijsten } from "./composables/use-mijn-waardelijsten";
import { PublicatieStatus, type Publicatie } from "./types";

const addDays = (dateString: string, days: number) => {
  if (!dateString) return dateString;

  const date = new Date(dateString);
  const nextDateUtc = new Date(
    Date.UTC(date.getFullYear(), date.getMonth(), date.getDate() + days)
  );

  return nextDateUtc.toISOString().substring(0, 10);
};

const searchString = ref("");
const fromDate = ref("");
const untilDateInclusive = ref("");

// we zoeken met een datum in een datum-tijd veld, daarom corrigeren we de datum hier
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

const searchParamsConfig = {
  page: "1",
  sorteer: "-registratiedatum",
  search: "", // searchString
  registratiedatumVanaf: "", // fromDate
  registratiedatumTot: "", // untilDateExclusive
  publishers: "",
  informatieCategorieen: "",
  onderwerpen: ""
};

const filterConfig: {
  label: string;
  searchParam: keyof Pick<
    typeof searchParamsConfig,
    "publishers" | "informatieCategorieen" | "onderwerpen"
  >;
  listKey: keyof UnwrapRef<typeof mijnWaardelijsten>;
}[] = [
  { label: "Organisatie", searchParam: "publishers", listKey: "organisaties" },
  {
    label: "Informatiecategorie",
    searchParam: "informatieCategorieen",
    listKey: "informatiecategorieen"
  },
  { label: "Onderwerp", searchParam: "onderwerpen", listKey: "onderwerpen" }
];

const sortingOptions = {
  officiele_titel: "Title (a-z)",
  "-officiele_titel": "Title (z-a)",
  verkorte_titel: "Verkorte title (a-z)",
  "-verkorte_titel": "Verkorte title (z-a)",
  registratiedatum: "Registratiedatum (oud-nieuw)",
  "-registratiedatum": "Registratiedatum (nieuw-oud)"
};

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

const {
  queryParams,
  pagedResult,
  pageCount,
  isFetching: loadingPageResult,
  error: pagedResultError,
  onNext,
  onPrev
} = usePagedSearch<Publicatie, typeof searchParamsConfig>("publicaties", searchParamsConfig);

// sync linked refs from queryParams / urlSearchParams once on init
watch(queryParams, syncFromQuery, { once: true });

// sync linked refs to queryParams onSearch
const onSearch = syncToQuery;
</script>

<style lang="scss" scoped>
// reset margins, use gaps
p,
button,
input,
select,
.form-group {
  margin-block: 0;
}

menu {
  display: flex;
  align-items: center;
  gap: var(--spacing-default);
  margin-block-end: var(--spacing-default);
}

fieldset {
  gap: var(--spacing-default);

  &.search {
    display: flex;
    flex-wrap: wrap;
    align-items: flex-end;
  }

  .form-group {
    flex-grow: 1;
  }

  &.filter {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(var(--section-width-small), 1fr));
  }
}

section {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(var(--section-width), 1fr));
  gap: var(--spacing-default);
  margin-block-end: var(--spacing-default);

  .form-group-inline {
    flex-direction: row;
    align-items: center;

    label {
      margin-block: 0;
      margin-inline-end: var(--spacing-default);
    }
  }
}

.page-nav {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  column-gap: var(--spacing-large);

  menu {
    margin-block-end: 0;
  }

  button {
    padding-block: var(--spacing-extrasmall);
  }
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
