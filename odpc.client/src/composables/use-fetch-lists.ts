import { ref, computed, onMounted } from "vue";
import { handleFetchError } from "@/api";
import { promiseAll } from "@/utils";

type FetcherFn<T> = (url: string) => Promise<T[]>;

const defaultFetcher = async <T>(url: string) =>
  fetch(url, { headers: { "is-api": "true" } })
    .then((r) => (r.ok ? r : (handleFetchError(r.status), Promise.reject(r))))
    .then((r) => r.json() as Promise<T[]>);

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

    const promises = Object.fromEntries(
      (Object.entries(urls) as [K, string][]).map(([key, url]) => [key, fetcher(url)])
    );

    try {
      const results = await promiseAll(promises);

      lists.value = results;
    } catch {
      error.value = true;
    } finally {
      loading.value = false;
    }
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
