import { computed, type Ref } from "vue";
import { PublicatieStatus, type Publicatie } from "../types";
import type { useMijnGebruikersgroepen } from "./use-mijn-gebruikersgroepen";

type UseMijnGebruikersgroepenReturn = ReturnType<typeof useMijnGebruikersgroepen>;

export const usePublicatiePermissions = (
  publicatie: Ref<Publicatie>,
  mijnGebruikersgroepen: UseMijnGebruikersgroepenReturn["data"],
  gekoppeldeWaardelijsten: UseMijnGebruikersgroepenReturn["gekoppeldeWaardelijsten"],
  gekoppeldeWaardelijstenUuids: UseMijnGebruikersgroepenReturn["gekoppeldeWaardelijstenUuids"]
) => {
  const isReadonly = computed(
    () => publicatie.value.publicatiestatus === PublicatieStatus.ingetrokken || unauthorized.value
  );

  const canDraft = computed(
    () => !publicatie.value.uuid || publicatie.value.publicatiestatus === PublicatieStatus.concept
  );

  const canDelete = computed(
    () => publicatie.value.uuid && publicatie.value.publicatiestatus === PublicatieStatus.concept
  );

  const canRetract = computed(
    () => publicatie.value.publicatiestatus === PublicatieStatus.gepubliceerd
  );
  
  const userHasAccessToGroup = computed(() =>
    mijnGebruikersgroepen.value?.some(
      (groep) => groep.uuid === publicatie.value.eigenaarGroep?.identifier
    )
  );

  const groupHasWaardelijsten = computed(
    () =>
      gekoppeldeWaardelijsten.value.organisaties?.length &&
      gekoppeldeWaardelijsten.value.informatiecategorieen?.length
  );

  const publicatieWaardelijstenMatch = computed(
    () =>
      // Gebruikersgroep is assigned to publisher organisatie (or publisher not set yet)
      (gekoppeldeWaardelijstenUuids.value?.includes(publicatie.value.publisher) ||
        !publicatie.value.publisher) &&
      // Gebruikersgroep is assigned to every informatiecategorie of publicatie
      publicatie.value.informatieCategorieen.every((uuid: string) =>
        gekoppeldeWaardelijstenUuids.value?.includes(uuid)
      ) &&
      // Gebruikersgroep is assigned to every onderwerp of publicatie
      publicatie.value.onderwerpen.every((uuid: string) =>
        gekoppeldeWaardelijstenUuids.value?.includes(uuid)
      )
  );

  const unauthorized = computed(() => {
    if (!publicatie.value.eigenaarGroep) return false;

    return (
      !userHasAccessToGroup.value ||
      !groupHasWaardelijsten.value ||
      !publicatieWaardelijstenMatch.value
    );
  });

  return {
    isReadonly,
    canDraft,
    canDelete,
    canRetract,
    unauthorized
  };
};
