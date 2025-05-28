import { promiseAll } from "@/utils";
import { fetchAllPages } from "@/composables/use-all-pages";
import { inject } from "vue";

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

export const getLijsten = async () => {
  try {
    return await fetchLijsten();
  } catch (error) {
    console.error(`One or more lists failed to load. ${error}`);
    return null;
  }
};

export const injectLijsten = () =>
  inject<Awaited<ReturnType<typeof fetchLijsten>> | null>("lijsten", null);
