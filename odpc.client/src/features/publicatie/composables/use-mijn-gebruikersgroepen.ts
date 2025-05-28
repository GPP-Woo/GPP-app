import { type MaybeRefOrGetter, toRef, computed } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import { injectLijsten } from "@/stores/lijsten";
import type { MijnGebruikersgroep } from "../types";

const API_URL = `/api/v1`;

export const useMijnGebruikersgroepen = (uuid: MaybeRefOrGetter<string | undefined>) => {
  const gebruikersgroepUuid = toRef(uuid);

  const lijsten = injectLijsten();

  const { data, isFetching, error } = useFetchApi(() => `${API_URL}/mijn-gebruikersgroepen`).json<
    MijnGebruikersgroep[]
  >();

  const gekoppeldeWaardelijstenUuids = computed(
    () =>
      data.value?.find((groep) => groep.uuid === gebruikersgroepUuid.value)?.gekoppeldeWaardelijsten
  );

  const gekoppeldeWaardelijsten = computed(() => ({
    organisaties: lijsten?.organisaties.filter((item) =>
      gekoppeldeWaardelijstenUuids.value?.includes(item.uuid)
    ),
    informatiecategorieen: lijsten?.informatiecategorieen.filter((item) =>
      gekoppeldeWaardelijstenUuids.value?.includes(item.uuid)
    ),
    onderwerpen: lijsten?.onderwerpen.filter((item) =>
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
