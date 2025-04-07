import { computed, onMounted, reactive, ref } from "vue";
import type { OptionProps } from "@/components/option-group/types";
import type { Onderwerp } from "../types";

export const useWaardelijstenUser = () => {
  const loadingWaardelijstenUser = ref(false);
  const waardelijstenUserError = ref(false);

  const urls = {
    mijnOrganisaties: "/api/v1/mijn-organisaties",
    mijnInformatiecategorieen: "/api/v1/mijn-informatiecategorieen",
    mijnOnderwerpen: "/api/v1/mijn-onderwerpen"
  } as const;

  const mijnWaardelijsten = reactive<Record<keyof typeof urls, OptionProps[]>>({
    mijnOrganisaties: [],
    mijnInformatiecategorieen: [],
    mijnOnderwerpen: []
  });

  const getMijnLijsten = async () => {
    loadingWaardelijstenUser.value = true;

    try {
      const results = await Promise.allSettled(
        Object.entries(urls).map(async ([key, url]) => {
          const response = await fetch(url);

          const data: OptionProps[] | Onderwerp[] = await response.json();

          return {
            [key]: data.map(
              (option) =>
                (("officieleTitel" in option && {
                  uuid: option.uuid,
                  naam: option.officieleTitel
                }) ||
                  option) as OptionProps
            )
          };
        })
      );

      results.forEach((result) =>
        result.status === "fulfilled"
          ? Object.assign(mijnWaardelijsten, result.value)
          : (waardelijstenUserError.value = true)
      );
    } catch {
      waardelijstenUserError.value = true;
    } finally {
      loadingWaardelijstenUser.value = false;
    }
  };

  const mijnWaardelijstenUuids = computed(() =>
    Object.values(mijnWaardelijsten).flatMap((items) => items.map((item) => item.uuid) || [])
  );

  onMounted(() => getMijnLijsten());

  return {
    mijnWaardelijsten,
    mijnWaardelijstenUuids,
    loadingWaardelijstenUser,
    waardelijstenUserError
  };
};
