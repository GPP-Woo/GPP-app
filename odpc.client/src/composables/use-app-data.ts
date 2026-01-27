import { readonly, ref } from "vue";
import { promiseAll } from "@/utils";
import { fetchAllPages } from "@/composables/use-all-pages";
import getUser, { type User } from "@/stores/user";

type LijstItem = { uuid: string; omschrijving?: string };
type LijstItemNaam = LijstItem & { naam: string };
type LijstItemTitel = LijstItem & { officieleTitel: string };

const fetcher = (url: string) =>
  fetchAllPages<LijstItemNaam | LijstItemTitel>(url).then((r) =>
    r.map(({ uuid, omschrijving, ...rest }) => ({
      uuid,
      naam: "naam" in rest ? rest.naam : rest.officieleTitel,
      omschrijving
    }))
  );

const fetchLijsten = async () =>
  promiseAll({
    organisaties: fetcher("/api/v2/organisaties"),
    informatiecategorieen: fetcher("/api/v2/informatiecategorieen"),
    onderwerpen: fetcher("/api/v2/onderwerpen")
  });

const lijsten = ref<Awaited<ReturnType<typeof fetchLijsten>> | null>(null);
const user = ref<User | null>(null);

const loading = ref(false);
const error = ref(false);

let loaded = false;

export const useAppData = () => {
  const fetchData = async () => {
    if (loaded) return;

    loading.value = true;

    try {
      lijsten.value = await fetchLijsten();
      user.value = await getUser();
    } catch {
      error.value = true;
    } finally {
      loading.value = false;
      loaded = true;
    }
  };

  const clearData = () => {
    lijsten.value = null;
    user.value = null;
  };

  return {
    lijsten: readonly(lijsten),
    user: readonly(user),
    loading: readonly(loading),
    error: readonly(error),
    fetchData,
    clearData
  };
};
