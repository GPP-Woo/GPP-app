import { ref, computed, onMounted } from "vue";
import { until } from "@vueuse/core";
import { useFetchApi } from "@/api";
import { useAllPages } from "./use-all-pages";

type FetcherFn<T> = (url: string) => Promise<{ data: T[]; error: boolean }>;

export const defaultFetcher = async <T>(url: string) => {
  const { data, error } = await useFetchApi(url).json<T[]>();

  return { data: data.value ?? [], error: !!error.value };
};

export const allPagesFetcher = async <T>(url: string) => {
  const { data, loading, error } = useAllPages<T>(url);

  await until(loading).toBe(false);

  return { data: data.value, error: error.value };
};

export const useFetchLists = <K extends string, T extends { uuid: string }>(
  urls: Record<K, string>,
  fetcher: FetcherFn<T> = defaultFetcher
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
        const { data, error: fetchError } = await fetcher(url);

        if (fetchError) throw fetchError;

        lists.value[key] = data;
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
