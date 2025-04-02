import type { OptionProps } from "@/components/option-group/types";

export type Gebruikersgroep = {
  uuid?: string;
  naam: string;
  omschrijving?: string;
  gekoppeldeWaardelijsten: string[];
  gekoppeldeGebruikers: string[];
};

export const WAARDELIJSTEN = {
  ORGANISATIE: "Organisatie",
  INFORMATIECATEGORIE: "Informatiecategorie",
  ONDERWERP: "Onderwerp"
} as const;

export type WaardelijstItem = OptionProps &
  Partial<{
    identifier: string;
    naamMeervoud: string;
    definitie: string;
    oorsprong: string;
    order: number;
  }>;
