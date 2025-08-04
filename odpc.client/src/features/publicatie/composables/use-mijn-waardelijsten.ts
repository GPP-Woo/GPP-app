import { computed, readonly } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import { useAppData } from "@/composables/use-app-data";
import type { MijnGebruikersgroep } from "../types";

export const useMijnWaardelijsten = () => {
  const { lijsten } = useAppData();

  const { data, isFetching, error } = useFetchApi(() => "/api/mijn-gebruikersgroepen").json<
    MijnGebruikersgroep[]
  >();

  const distinctUuids = computed(() => [
    ...new Set(data.value?.flatMap((item) => item.gekoppeldeWaardelijsten))
  ]);

  const mijnWaardelijsten = computed(() => ({
    organisaties: lijsten.value?.organisaties.filter((item) =>
      distinctUuids.value.includes(item.uuid)
    ),
    informatiecategorieen: lijsten.value?.informatiecategorieen.filter((item) =>
      distinctUuids.value.includes(item.uuid)
    ),
    onderwerpen: lijsten.value?.onderwerpen.filter((item) =>
      distinctUuids.value.includes(item.uuid)
    )
  }));

  return {
    mijnWaardelijsten: readonly(mijnWaardelijsten),
    isFetching: readonly(isFetching),
    error: readonly(error)
  };
};
