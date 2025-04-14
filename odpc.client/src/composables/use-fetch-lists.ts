import { ref, computed, onMounted } from "vue";
import { promiseAll } from "@/utils";
import { fetchAllPages } from "./use-all-pages";

export type ListItem = {
  uuid: string;
  naam: string;
};

const defaultFetcher = async (url: string) =>
  fetchAllPages<{ uuid: string; naam: string } | { uuid: string; officieleTitel: string }>(
    url
  ).then((r) =>
    r.map(({ uuid, ...rest }) => ({
      uuid,
      naam: "naam" in rest ? rest.naam : rest.officieleTitel
    }))
  );

export const useFetchLists = <K extends string>(
  urls: Record<K, string>,
  fetcher = defaultFetcher
) => {
  const loading = ref(false);
  const error = ref(false);

  const lists = ref(
    Object.keys(urls).reduce(
      (acc, key) => ({ ...acc, [key as K]: [] }),
      {} as Record<K, ListItem[]>
    )
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
    (Object.values(lists.value) as ListItem[][]).flat().map((item) => item.uuid)
  );

  onMounted(() => fetchLists());

  return {
    lists,
    uuids,
    loading,
    error
  };
};
