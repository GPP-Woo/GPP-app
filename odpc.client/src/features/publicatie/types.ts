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

export const Archiefnominatie = Object.freeze({
  blijvend_bewaren: "overbrengen",
  vernietigen: "vernietigen"
});

export type Publicatie = {
  uuid?: string;
  publisher: string;
  verantwoordelijke: string;
  officieleTitel: string;
  verkorteTitel: string;
  omschrijving: string;
  eigenaar?: Eigenaar;
  eigenaarGroep: EigenaarGroep | null;
  publicatiestatus: PublicatieStatus;
  registratiedatum?: string;
  datumBeginGeldigheid?: string | null;
  datumEindeGeldigheid?: string | null;
  informatieCategorieen: string[];
  onderwerpen: string[];
  kenmerken: Kenmerk[];
  urlPublicatieExtern?: string;
  bronBewaartermijn?: string;
  selectiecategorie?: string;
  archiefnominatie?: keyof typeof Archiefnominatie | "";
  archiefactiedatum?: string | null;
  toelichtingBewaartermijn?: string;
};

export type PublicatieDocument = {
  uuid?: string;
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

type EigenaarGroep = {
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
