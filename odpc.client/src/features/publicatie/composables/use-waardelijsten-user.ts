import { computed } from "vue";
import { useFetchApi } from "@/api";
import type { OptionProps } from "@/components/option-group/types";
import type { Onderwerp } from "../types";

export const useWaardelijstenUser = () => {
  const {
    data: mijnOrganisaties,
    isFetching: loadingMijnOrganisaties,
    error: mijnOrganisatiesError
  } = useFetchApi(() => `/api/v1/mijn-organisaties`).json<OptionProps[]>();

  const {
    data: mijnInformatiecategorieen,
    isFetching: loadingMijnInformatiecategorieen,
    error: mijnInformatiecategorieenError
  } = useFetchApi(() => `/api/v1/mijn-informatiecategorieen`).json<OptionProps[]>();

  const {
    data: mijnOnderwerpenData,
    isFetching: loadingMijnOnderwerpen,
    error: mijnOnderwerpenError
  } = useFetchApi(() => `/api/v1/mijn-onderwerpen`).json<Onderwerp[]>();

  // map Onderwerp to OptionProps
  const mijnOnderwerpen = computed<OptionProps[] | null>(
    () => mijnOnderwerpenData.value?.map((o) => ({ uuid: o.uuid, naam: o.officieleTitel })) || null
  );

  const loadingWaardelijstenUser = computed(
    () =>
      loadingMijnOrganisaties.value ||
      loadingMijnInformatiecategorieen.value ||
      loadingMijnOnderwerpen.value
  );
  const waardelijstenUserError = computed(
    () =>
      mijnOrganisatiesError.value ||
      mijnInformatiecategorieenError.value ||
      mijnOnderwerpenError.value
  );

  const mijnWaardelijstenUuids = computed(() => [
    ...(mijnOrganisaties.value?.map((item) => item.uuid) || []),
    ...(mijnInformatiecategorieen.value?.map((item) => item.uuid) || []),
    ...(mijnOnderwerpen.value?.map((item) => item.uuid) || [])
  ]);

  return {
    mijnOrganisaties,
    mijnInformatiecategorieen,
    mijnOnderwerpen,
    mijnWaardelijstenUuids,
    loadingWaardelijstenUser,
    waardelijstenUserError
  };
};
