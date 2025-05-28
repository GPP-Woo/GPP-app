export const PublicatieStatus = Object.freeze({
  concept: "concept",
  gepubliceerd: "gepubliceerd",
  ingetrokken: "ingetrokken"
});

export type Publicatie = {
  uuid?: string;
  publisher: string;
  verantwoordelijke: string;
  officieleTitel: string;
  verkorteTitel: string;
  omschrijving: string;
  eigenaar?: Eigenaar;
  publicatiestatus: keyof typeof PublicatieStatus;
  registratiedatum?: string;
  informatieCategorieen: string[];
  onderwerpen: string[];
  gebruikersgroep?: string;
};

export type PublicatieDocument = {
  uuid?: string;
  identifier?: string;
  publicatie: string;
  officieleTitel: string;
  verkorteTitel?: string;
  omschrijving?: string;
  publicatiestatus: keyof typeof PublicatieStatus;
  creatiedatum: string;
  bestandsnaam: string;
  bestandsformaat: string;
  bestandsomvang: number;
  bestandsdelen?: Bestandsdeel[];
};

export type Onderwerp = {
  uuid: string;
  publicaties: string[];
  officieleTitel: string;
  omschrijving: string;
  publicatiestatus: keyof typeof PublicatieStatus;
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
