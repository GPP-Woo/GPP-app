import { type MaybeRefOrGetter, toRef, computed } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import { useAppData } from "@/composables/use-app-data";
import type { MijnGebruikersgroep } from "../types";

const API_URL = `/api`;

export const useMijnGebruikersgroepen = (uuid: MaybeRefOrGetter<string | undefined>) => {
  const gebruikersgroepUuid = toRef(uuid);

  const { lijsten } = useAppData();

  const { data, isFetching, error } = useFetchApi(() => `${API_URL}/mijn-gebruikersgroepen`).json<
    MijnGebruikersgroep[]
  >();

  const gekoppeldeWaardelijstenUuids = computed(
    () =>
      data.value?.find((groep) => groep.uuid === gebruikersgroepUuid.value)?.gekoppeldeWaardelijsten
  );

  const gekoppeldeWaardelijsten = computed(() => ({
    organisaties: lijsten.value?.organisaties.filter((item) =>
      gekoppeldeWaardelijstenUuids.value?.includes(item.uuid)
    ),
    informatiecategorieen: lijsten.value?.informatiecategorieen.filter((item) =>
      gekoppeldeWaardelijstenUuids.value?.includes(item.uuid)
    ),
    onderwerpen: lijsten.value?.onderwerpen.filter((item) =>
      gekoppeldeWaardelijstenUuids.value?.includes(item.uuid)
    )
  }));

  return {
    data,
    isFetching,
    error,
    gekoppeldeWaardelijsten,
    gekoppeldeWaardelijstenUuids
  };
};
