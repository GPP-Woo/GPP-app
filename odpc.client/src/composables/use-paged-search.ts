import { ref, watch, computed, onMounted, readonly } from "vue";
import { useRouter } from "vue-router";
import { useUrlSearchParams } from "@vueuse/core";
import { useFetchApi, type PagedResult } from "@/api";

const PAGE_SIZE = 10;

export const usePagedSearch = <T, QueryParams extends { [key: string]: string; page: string }>(
  endpoint: string,
  queryParamsConfig: QueryParams
) => {
  const router = useRouter();

  const urlSearchParams = useUrlSearchParams("history", {
    initialValue: queryParamsConfig,
    removeFalsyValues: true
  });

  const queryParams = ref({ ...queryParamsConfig });
  const queryParamKeys = Object.keys(queryParamsConfig);

  const initQueryParams = () => {
    queryParams.value = Object.fromEntries(
      queryParamKeys.map((key) => [
        key,
        urlSearchParams[key] ? decodeURIComponent(urlSearchParams[key] as string) : ""
      ])
    );
  };

  const onNext = () => {
    queryParams.value.page = String(Number(queryParams.value.page) + 1);
  };

  const onPrev = () => {
    queryParams.value.page = String(Math.max(1, Number(queryParams.value.page) - 1));
  };

  const pageCount = computed(() =>
    data.value?.count ? Math.ceil(data.value.count / PAGE_SIZE) : 0
  );

  const searchParams = computed(
    () =>
      new URLSearchParams(
        queryParamKeys
          .filter((key) => !!queryParams.value[key]?.trim())
          .map((key) => [key, encodeURIComponent(queryParams.value[key])])
      )
  );

  // Watch for search param changes
  watch(searchParams, async (newParams, oldParams) => {
    // Reset to page 1 when filters change (but not page)
    if (newParams.get("page") === oldParams.get("page") && queryParams.value.page !== "1") {
      queryParams.value.page = "1";

      return false;
    }

    // Update history
    router.replace({ query: { ...Object.fromEntries(newParams.entries()) } });

    // Execute new search
    await get().execute();
  });

  const { get, data, isFetching, error } = useFetchApi(
    () => `/api/v2/${endpoint}${searchParams.value.size ? "?" + searchParams.value : ""}`,
    {
      immediate: false
    }
  ).json<PagedResult<T>>();

  // Get initial params from search url
  onMounted(initQueryParams);

  return {
    queryParams,
    pagedResult: readonly(data),
    pageCount: readonly(pageCount),
    isFetching: readonly(isFetching),
    error: readonly(error),
    onNext,
    onPrev
  };
};
