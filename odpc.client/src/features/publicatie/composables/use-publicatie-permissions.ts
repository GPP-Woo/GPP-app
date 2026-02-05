import { computed, type DeepReadonly, type Ref } from "vue";
import { useAppData } from "@/composables/use-app-data";
import { PublicatieStatus, type MijnGebruikersgroep, type Publicatie } from "../types";

export const usePublicatiePermissions = (
  publicatie: Ref<Publicatie>,
  mijnGebruikersgroepen: DeepReadonly<Ref<MijnGebruikersgroep[] | null>>
) => {
  const { user, lijsten } = useAppData();

  const isOwner = computed(
    () => !publicatie.value.eigenaar || publicatie.value.eigenaar.identifier === user.value?.id
  );

  const isReadonly = computed(
    () =>
      !isOwner.value ||
      unauthorized.value ||
      publicatie.value.publicatiestatus === PublicatieStatus.ingetrokken
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

  const canClaim = computed(
    () =>
      !isOwner.value &&
      !unauthorized.value &&
      publicatie.value.publicatiestatus !== PublicatieStatus.ingetrokken
  );

  const userHasAccessToGroep = computed(() =>
    mijnGebruikersgroepen.value?.some(
      (groep) => groep.uuid === publicatie.value.eigenaarGroep?.identifier
    )
  );

  const groepWaardelijstenUuids = computed(
    () =>
      mijnGebruikersgroepen.value?.find(
        (groep) => groep.uuid === publicatie.value.eigenaarGroep?.identifier
      )?.gekoppeldeWaardelijsten
  );

  const groepWaardelijsten = computed(() => ({
    organisaties: lijsten.value?.organisaties.filter((item) =>
      groepWaardelijstenUuids.value?.includes(item.uuid)
    ),
    informatiecategorieen: lijsten.value?.informatiecategorieen.filter((item) =>
      groepWaardelijstenUuids.value?.includes(item.uuid)
    ),
    onderwerpen: lijsten.value?.onderwerpen.filter((item) =>
      groepWaardelijstenUuids.value?.includes(item.uuid)
    )
  }));

  const groepHasWaardelijsten = computed(
    () =>
      groepWaardelijsten.value.organisaties?.length &&
      groepWaardelijsten.value.informatiecategorieen?.length
  );

  const publicatieWaardelijstenMatch = computed(
    () =>
      // Gebruikersgroep is assigned to publisher organisatie (or publisher not set yet)
      (!publicatie.value.publisher ||
        groepWaardelijstenUuids.value?.includes(publicatie.value.publisher)) &&
      // Gebruikersgroep is assigned to every informatiecategorie of publicatie
      publicatie.value.informatieCategorieen.every((uuid: string) =>
        groepWaardelijstenUuids.value?.includes(uuid)
      ) &&
      // Gebruikersgroep is assigned to every onderwerp of publicatie
      publicatie.value.onderwerpen.every((uuid: string) =>
        groepWaardelijstenUuids.value?.includes(uuid)
      )
  );

  const unauthorized = computed(() => {
    if (!publicatie.value.eigenaarGroep) return false;

    return (
      !userHasAccessToGroep.value ||
      !groepHasWaardelijsten.value ||
      !publicatieWaardelijstenMatch.value
    );
  });

  return {
    isReadonly,
    canDraft,
    canDelete,
    canRetract,
    canClaim,
    unauthorized,
    groepWaardelijsten
  };
};
