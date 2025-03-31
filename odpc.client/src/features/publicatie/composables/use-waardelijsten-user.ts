import { computed } from "vue";
import { useFetchApi } from "@/api";
import type { OptionProps } from "@/components/option-group/types";

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
    data: mijnOnderwerpen,
    isFetching: loadingMijnOnderwerpen,
    error: mijnOnderwerpenError
  } = useFetchApi(() => `/api/v1/mijn-onderwerpen`).json<OptionProps[]>();

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
