import { computed, type ModelRef } from "vue";
import type { Kenmerk } from "../types";

// Bron "GPP-app" is added as reference to all kenmerken created in GPP-app
const BRON = "GPP-app";

export const useKenmerken = <T extends { kenmerken?: Kenmerk[] }>(model: ModelRef<T>) =>
  computed({
    get: () => model.value.kenmerken?.map((k) => k.kenmerk) ?? [],
    set: (kenmerken) => {
      const existingKenmerken =
        model.value.kenmerken?.filter((k) => kenmerken.includes(k.kenmerk)) ?? [];
      const existingKenmerkValues = existingKenmerken.map((k) => k.kenmerk);

      const newKenmerken = kenmerken.filter((kenmerk) => !existingKenmerkValues.includes(kenmerk));

      model.value.kenmerken = [
        ...existingKenmerken,
        ...newKenmerken.map((kenmerk) => ({ kenmerk, bron: BRON }))
      ];
    }
  });
