export const PublicatieStatus = Object.freeze({
  concept: "concept",
  gepubliceerd: "gepubliceerd",
  ingetrokken: "ingetrokken"
});

type PublicatieStatus = keyof typeof PublicatieStatus;

type PendingDocumentAction = "delete" | "retract" | null;

export const PendingDocumentActions: Record<PublicatieStatus, PendingDocumentAction> =
  Object.freeze({
    concept: "delete",
    gepubliceerd: "retract",
    ingetrokken: null
  });

export type Publicatie = {
  uuid?: string;
  publisher: string;
  verantwoordelijke: string;
  officieleTitel: string;
  verkorteTitel: string;
  omschrijving: string;
  eigenaar?: Eigenaar;
  publicatiestatus: PublicatieStatus;
  registratiedatum?: string;
  datumBeginGeldigheid?: string | null;
  datumEindeGeldigheid?: string | null;
  informatieCategorieen: string[];
  onderwerpen: string[];
  gebruikersgroep: string;
  kenmerken: Kenmerk[];
  urlPublicatieExtern?: string;
};

export type PublicatieDocument = {
  uuid?: string;
  identifier?: string;
  publicatie: string;
  officieleTitel: string;
  verkorteTitel?: string;
  omschrijving?: string;
  publicatiestatus: PublicatieStatus;
  pendingAction?: PendingDocumentAction;
  creatiedatum: string;
  ontvangstdatum?: string | null;
  datumOndertekend?: string | null;
  bestandsnaam: string;
  bestandsformaat: string;
  bestandsomvang: number;
  bestandsdelen?: Bestandsdeel[] | null;
  kenmerken: Kenmerk[];
};

export type Onderwerp = {
  uuid: string;
  publicaties: string[];
  officieleTitel: string;
  omschrijving: string;
  publicatiestatus: PublicatieStatus;
  promoot: boolean;
  registratiedatum: string;
};

type Eigenaar = {
  identifier: string;
  weergaveNaam: string;
};

export type Bestandsdeel = {
  url: string;
  volgnummer: number;
  omvang: number;
};

export type MimeType = {
  identifier: string;
  name: string;
  mimeType: string;
  extension?: string;
};

export type MijnGebruikersgroep = {
  uuid: string;
  naam: string;
  gekoppeldeWaardelijsten: string[];
};

export type Kenmerk = {
  kenmerk: string;
  bron: string;
};

export type WaardelijstItem = {
  uuid: string;
  naam: string;
};
