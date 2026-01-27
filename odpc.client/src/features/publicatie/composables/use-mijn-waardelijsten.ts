import { computed, readonly } from "vue";
import { useAppData } from "@/composables/use-app-data";
import { useMijnGebruikersgroepen } from "./use-mijn-gebruikersgroepen";

export const useMijnWaardelijsten = () => {
  const { lijsten } = useAppData();

  const { data, isFetching, error } = useMijnGebruikersgroepen();

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
    mijnGebruikersgroepen: data,
    mijnWaardelijsten: readonly(mijnWaardelijsten),
    isFetching: readonly(isFetching),
    error: readonly(error)
  };
};
