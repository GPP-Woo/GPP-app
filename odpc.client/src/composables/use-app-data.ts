import { readonly, ref } from "vue";
import { promiseAll } from "@/utils";
import { fetchAllPages } from "@/composables/use-all-pages";

const fetcher = (url: string) =>
  fetchAllPages<{ uuid: string; naam: string } | { uuid: string; officieleTitel: string }>(
    url
  ).then((r) =>
    r.map(({ uuid, ...rest }) => ({
      uuid,
      naam: "naam" in rest ? rest.naam : rest.officieleTitel
    }))
  );

const fetchLijsten = async () =>
  promiseAll({
    organisaties: fetcher("/api/v1/organisaties"),
    informatiecategorieen: fetcher("/api/v1/informatiecategorieen"),
    onderwerpen: fetcher("/api/v1/onderwerpen")
  });

const lijsten = ref<Awaited<ReturnType<typeof fetchLijsten>> | null>(null);

const loading = ref(false);
const error = ref(false);

let loaded = false;

export const useAppData = () => {
  const fetchData = async () => {
    if (loaded) return;

    loading.value = true;

    try {
      lijsten.value = await fetchLijsten();
    } catch {
      error.value = true;
    } finally {
      loading.value = false;
      loaded = true;
    }
  };

  return {
    lijsten: readonly(lijsten),
    loading: readonly(loading),
    error: readonly(error),
    fetchData
  };
};
