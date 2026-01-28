import { computed, ref, watch } from "vue";
import { usePagedSearch } from "@/composables/use-paged-search";
import { useMijnWaardelijsten } from "./use-mijn-waardelijsten";
import type { Publicatie } from "../types";

const addDays = (dateString: string, days: number) => {
  if (!dateString) return dateString;

  const date = new Date(dateString);
  const nextDateUtc = new Date(
    Date.UTC(date.getFullYear(), date.getMonth(), date.getDate() + days)
  );

  return nextDateUtc.toISOString().substring(0, 10);
};

const QueryParamsConfig = {
  page: "1",
  sorteer: "-registratiedatum",
  search: "", // searchString
  registratiedatumVanaf: "", // fromDate
  registratiedatumTot: "", // untilDateExclusive
  informatieCategorieen: "",
  onderwerpen: "",
  publicatiestatus: "",
  eigenaarGroep: ""
};

// eigenaarGroep?: Ref<string>
export const usePublicatiesOverview = () => {
  const searchString = ref(""); // search
  const fromDate = ref(""); // registratiedatumVanaf
  const untilDateInclusive = ref("");

  // we zoeken met een datum in een datum-tijd veld, daarom corrigeren we de datum hier
  // registratiedatumTot
  const untilDateExclusive = computed({
    get: () => addDays(untilDateInclusive.value, 1),
    set: (v) => (untilDateInclusive.value = addDays(v, -1))
  });

  const {
    mijnGebruikersgroepen,
    mijnWaardelijsten,
    isFetching: loadingMijnWaardelijsten,
    error: mijnWaardelijstenError
  } = useMijnWaardelijsten();

  const {
    queryParams,
    pagedResult,
    pageCount,
    isFetching: loadingPageResult,
    error: pagedResultError,
    initPagedSearch,
    onNext,
    onPrev
  } = usePagedSearch<Publicatie, typeof QueryParamsConfig>("publicaties", QueryParamsConfig);

  const isLoading = computed(() => loadingMijnWaardelijsten.value || loadingPageResult.value);
  const hasError = computed(() => !!mijnWaardelijstenError.value || !!pagedResultError.value);

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

  // sync linked refs from queryParams / urlSearchParams
  watch(queryParams, syncFromQuery, { deep: true });

  return {
    // search/filter
    searchString,
    fromDate,
    untilDateInclusive,

    // data
    mijnGebruikersgroepen,
    mijnWaardelijsten,
    queryParams,
    pagedResult,
    pageCount,

    // loading/error
    isLoading,
    hasError,

    // actions
    search: syncToQuery,
    initPagedSearch,
    onNext,
    onPrev
  };
};
