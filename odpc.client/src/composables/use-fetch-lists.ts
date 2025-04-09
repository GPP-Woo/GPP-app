import { ref, computed, onMounted } from "vue";
import { until } from "@vueuse/core";
import { useFetchApi } from "@/api";
import { useAllPages } from "./use-all-pages";

type Fetcher = "fetch-api" | "all-pages";

export const useFetchLists = <K extends string, T extends { uuid: string }>(
  urls: Record<K, string>,
  fetcher: Fetcher = "fetch-api"
) => {
  const loading = ref(false);
  const error = ref(false);

  const lists = ref<Record<K, T[]>>(
    Object.keys(urls).reduce((acc, key) => ({ ...acc, [key as K]: [] }), {} as Record<K, T[]>)
  );

  const fetchLists = async () => {
    loading.value = true;
    error.value = false;

    const promises = (Object.entries(urls) as [K, string][]).map(async ([key, url]) => {
      try {
        const {
          data: dataRef,
          isFetching: loadingRef,
          error: errorRef
        } = fetcher === "all-pages" ? useAllPages<T>(url) : await useFetchApi(url).json<T[]>();

        // await asyncComputed data to be loaded from useAllPages
        await until(loadingRef).toBe(false);

        if (errorRef.value) throw new Error();

        lists.value[key] = dataRef.value ?? [];
      } catch {
        error.value = true;
      }
    });

    await Promise.all(promises);

    loading.value = false;
  };

  const uuids = computed(() =>
    (Object.values(lists.value) as T[][]).flat().map((item) => item.uuid)
  );

  onMounted(() => fetchLists());

  return {
    lists,
    uuids,
    loading,
    error
  };
};
