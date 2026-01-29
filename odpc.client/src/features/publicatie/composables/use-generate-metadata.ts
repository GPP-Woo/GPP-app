import { ref } from "vue";
import { useAppData } from "@/composables/use-app-data";
import toast from "@/stores/toast";
import { config } from "@/config";
import type { Publicatie, PublicatieDocument } from "../types";

type WooHooResponse = {
  success: boolean;
  request_id: string;
  suggestion: {
    metadata: {
      titelcollectie: {
        officieleTitel: string;
        verkorteTitels?: string[];
        alternatieveTitels?: string[];
      };
      omschrijvingen?: string[];
      classificatiecollectie?: {
        informatiecategorieen?: { categorie: string; resource: string; label: string }[];
        themas?: { resource: string; label: string }[];
        trefwoorden?: string[];
      };
      creatiedatum?: string;
      geldigheid?: {
        begindatum?: string;
        einddatum?: string;
      };
      documenthandelingen?: {
        soortHandeling: { handeling: string; resource: string; label: string };
        atTime?: string;
      }[];
    };
  } | null;
  error: string | null;
};

const toDateString = (value?: string) => {
  if (!value) return null;
  // Handle both "2024-01-15" and "2024-01-15T00:00:00" formats
  return value.split("T")[0];
};

const toDateTimeString = (value?: string) => {
  if (!value) return null;
  // If already has time part, return as-is; otherwise append midnight
  return value.includes("T") ? value : `${value}T00:00:00Z`;
};

export const useGenerateMetadata = () => {
  const isGenerating = ref(false);
  const isAvailable = ref(false);
  const { lijsten } = useAppData();

  const checkAvailability = async () => {
    try {
      const response = await fetch(`${config.odpcApiUrl}/api/v1/metadata/health`);
      isAvailable.value = response.ok;
    } catch {
      isAvailable.value = false;
    }
  };

  const findMatchingUuids = (
    labels: string[],
    lijst: readonly { readonly uuid: string; readonly naam: string }[] | undefined
  ): string[] => {
    if (!lijst) return [];
    return labels
      .filter((label): label is string => !!label) // Filter out undefined/null labels
      .map((label) => lijst.find((item) => item.naam.toLowerCase() === label.toLowerCase())?.uuid)
      .filter((uuid): uuid is string => !!uuid);
  };

  const generateMetadata = async (
    documentUuid: string,
    publicatie: Publicatie,
    documenten: PublicatieDocument[]
  ): Promise<{
    publicatie: Partial<Publicatie>;
    document?: Partial<PublicatieDocument>;
  } | null> => {
    isGenerating.value = true;

    try {
      const response = await fetch(
        `${config.odpcApiUrl}/api/v1/metadata/generate/${encodeURIComponent(documentUuid)}`,
        { method: "POST" }
      );

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `HTTP ${response.status}`);
      }

      const data: WooHooResponse = await response.json();

      if (!data.success || !data.suggestion) {
        throw new Error(data.error || "Metadata generatie mislukt.");
      }

      const meta = data.suggestion.metadata;

      // Map publication-level fields
      const publicatieUpdate: Partial<Publicatie> = {};

      if (meta.titelcollectie?.officieleTitel) {
        publicatieUpdate.officieleTitel = meta.titelcollectie.officieleTitel;
      }

      if (meta.titelcollectie?.verkorteTitels?.length) {
        publicatieUpdate.verkorteTitel = meta.titelcollectie.verkorteTitels[0];
      }

      if (meta.omschrijvingen?.length) {
        publicatieUpdate.omschrijving = meta.omschrijvingen[0];
      }

      if (meta.geldigheid?.begindatum) {
        publicatieUpdate.datumBeginGeldigheid = toDateString(meta.geldigheid.begindatum);
      }

      if (meta.geldigheid?.einddatum) {
        publicatieUpdate.datumEindeGeldigheid = toDateString(meta.geldigheid.einddatum);
      }

      // Match informatiecategorieen by label
      if (meta.classificatiecollectie?.informatiecategorieen?.length) {
        const labels = meta.classificatiecollectie.informatiecategorieen.map((c) => c.label);
        const matched = findMatchingUuids(labels, lijsten.value?.informatiecategorieen);
        if (matched.length) {
          publicatieUpdate.informatieCategorieen = matched;
        }
      }

      // Match themas/onderwerpen by label
      if (meta.classificatiecollectie?.themas?.length) {
        const labels = meta.classificatiecollectie.themas.map((t) => t.label);
        const matched = findMatchingUuids(labels, lijsten.value?.onderwerpen);
        if (matched.length) {
          publicatieUpdate.onderwerpen = matched;
        }
      }

      // Map trefwoorden to kenmerken
      if (meta.classificatiecollectie?.trefwoorden?.length) {
        publicatieUpdate.kenmerken = meta.classificatiecollectie.trefwoorden.map((kw) => ({
          kenmerk: kw,
          bron: "woo-hoo"
        }));
      }

      // Map document-level fields
      let documentUpdate: Partial<PublicatieDocument> | undefined;

      const targetDoc = documenten.find((d) => d.uuid === documentUuid);
      if (targetDoc) {
        documentUpdate = {};

        if (meta.titelcollectie?.officieleTitel) {
          documentUpdate.officieleTitel = meta.titelcollectie.officieleTitel;
        }

        if (meta.omschrijvingen?.length) {
          documentUpdate.omschrijving = meta.omschrijvingen[0];
        }

        if (meta.creatiedatum) {
          documentUpdate.creatiedatum = toDateString(meta.creatiedatum) ?? targetDoc.creatiedatum;
        }

        // Extract dates from documenthandelingen
        if (meta.documenthandelingen?.length) {
          const ondertekening = meta.documenthandelingen.find(
            (h) => h.soortHandeling.handeling === "ONDERTEKENING"
          );
          if (ondertekening?.atTime) {
            documentUpdate.datumOndertekend = toDateTimeString(ondertekening.atTime);
          }

          const ontvangst = meta.documenthandelingen.find(
            (h) => h.soortHandeling.handeling === "ONTVANGST"
          );
          if (ontvangst?.atTime) {
            documentUpdate.ontvangstdatum = toDateTimeString(ontvangst.atTime);
          }
        }
      }

      return { publicatie: publicatieUpdate, document: documentUpdate };
    } catch (err) {
      toast.add({
        text:
          err instanceof Error
            ? `Metadata generatie mislukt: ${err.message}`
            : "Er is iets misgegaan bij het genereren van metadata.",
        type: "error"
      });
      return null;
    } finally {
      isGenerating.value = false;
    }
  };

  return {
    isGenerating,
    isAvailable,
    checkAvailability,
    generateMetadata
  };
};
