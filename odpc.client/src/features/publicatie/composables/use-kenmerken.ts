import { computed, type Ref } from "vue";
import type { Kenmerk } from "../types";

const bron = "GPPWOO";

export const useKenmerken = <T extends { kenmerken?: Kenmerk[] }>(model: Ref<T>) =>
  computed({
    get: () => model.value.kenmerken?.map((k) => k.kenmerk) ?? [],
    set: (kenmerken) => {
      model.value.kenmerken = kenmerken?.map((k) => ({ kenmerk: k, bron })) ?? [];
    }
  });
