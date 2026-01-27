import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { useGenerateMetadata } from "../use-generate-metadata";
import type { Publicatie, PublicatieDocument } from "../../types";

// Mock useAppData — simulates the waardelijsten as they exist in the GPP-app
vi.mock("@/composables/use-app-data", () => ({
  useAppData: () => ({
    lijsten: {
      value: {
        organisaties: [
          { uuid: "org-uuid-1", naam: "Gemeente Amsterdam" },
          { uuid: "org-uuid-2", naam: "Gemeente Utrecht" }
        ],
        informatiecategorieen: [
          { uuid: "cat-uuid-1", naam: "Adviezen" },
          { uuid: "cat-uuid-2", naam: "Convenanten" },
          { uuid: "cat-uuid-3", naam: "Jaarplannen en jaarverslagen" }
        ],
        onderwerpen: [
          { uuid: "ond-uuid-1", naam: "Verkeer" },
          { uuid: "ond-uuid-2", naam: "Onderwijs" }
        ]
      }
    }
  })
}));

const mockToastAdd = vi.fn();
vi.mock("@/stores/toast", () => ({
  default: { add: (...args: unknown[]) => mockToastAdd(...args) }
}));

const createPublicatie = (overrides?: Partial<Publicatie>): Publicatie => ({
  publisher: "",
  verantwoordelijke: "",
  officieleTitel: "",
  verkorteTitel: "",
  omschrijving: "",
  eigenaarGroep: null,
  publicatiestatus: "concept",
  informatieCategorieen: [],
  onderwerpen: [],
  kenmerken: [],
  ...overrides
});

const createDocument = (overrides?: Partial<PublicatieDocument>): PublicatieDocument => ({
  uuid: "550e8400-e29b-41d4-a716-446655440000",
  publicatie: "660e8400-e29b-41d4-a716-446655440000",
  officieleTitel: "Advies bestemmingsplan centrum",
  publicatiestatus: "concept",
  creatiedatum: "2024-01-01",
  bestandsnaam: "advies-bestemmingsplan-centrum.pdf",
  bestandsformaat: "application/pdf",
  bestandsomvang: 245760,
  kenmerken: [],
  ...overrides
});

/**
 * Realistic woo-hoo MetadataGenerationResponse matching the actual API schema.
 * Based on the DiWoo metadata model and integration test fixtures from the woo-hoo repo.
 * All nullable fields are included to match the real JSON structure.
 */
const createWooHooResponse = (metadataOverrides = {}) => ({
  success: true,
  request_id: "req_550e8400-e29b-41d4-a716-446655440000",
  suggestion: {
    metadata: {
      identifiers: ["ZAAK-2024-001234"],
      publisher: {
        resource: "https://identifier.overheid.nl/tooi/id/gemeente/gm0363",
        label: "Gemeente Amsterdam"
      },
      titelcollectie: {
        officieleTitel: "Advies inzake wijziging bestemmingsplan centrum",
        verkorteTitels: ["Advies bestemmingsplan centrum"],
        alternatieveTitels: null
      },
      omschrijvingen: [
        "Advies van de commissie ruimtelijke ordening over de voorgenomen wijziging van het bestemmingsplan voor het centrumgebied, met betrekking tot horecabestemmingen."
      ],
      classificatiecollectie: {
        informatiecategorieen: [
          {
            categorie: "ADVIEZEN",
            resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_5ba23c01",
            label: "Adviezen"
          }
        ],
        documentsoorten: [
          {
            soort: "ADVIES",
            resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_advies",
            label: "advies"
          }
        ],
        themas: [
          {
            resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_verkeer",
            label: "Verkeer"
          }
        ],
        trefwoorden: ["bestemmingsplan", "horeca", "centrum"]
      },
      creatiedatum: "2024-03-15",
      geldigheid: {
        begindatum: "2024-03-15T00:00:00",
        einddatum: "2025-03-15"
      },
      language: {
        taal: "NL",
        resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_nl",
        label: "Nederlands"
      },
      format: {
        resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_pdf",
        label: "PDF"
      },
      documenthandelingen: [
        {
          soortHandeling: {
            handeling: "ONDERTEKENING",
            resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_ondertekening",
            label: "ondertekening"
          },
          atTime: "2024-03-14T10:00:00Z",
          wasAssociatedWith: {
            resource: "https://identifier.overheid.nl/tooi/id/gemeente/gm0363",
            label: "Gemeente Amsterdam"
          }
        },
        {
          soortHandeling: {
            handeling: "ONTVANGST",
            resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_ontvangst",
            label: "ontvangst"
          },
          atTime: "2024-03-13T09:30:00Z",
          wasAssociatedWith: {
            resource: "https://identifier.overheid.nl/tooi/id/gemeente/gm0363",
            label: "Gemeente Amsterdam"
          }
        },
        {
          soortHandeling: {
            handeling: "VASTSTELLING",
            resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_vaststelling",
            label: "vaststelling"
          },
          atTime: "2024-03-15T14:00:00Z",
          wasAssociatedWith: {
            resource: "https://identifier.overheid.nl/tooi/id/gemeente/gm0363",
            label: "Gemeente Amsterdam"
          }
        }
      ],
      verantwoordelijke: null,
      medeverantwoordelijken: null,
      opsteller: null,
      naamOpsteller: null,
      aggregatiekenmerk: null,
      isPartOf: null,
      hasParts: null,
      documentrelaties: null,
      redenVerwijderingVervanging: null,
      ...metadataOverrides
    },
    confidence: {
      overall: 0.85,
      fields: [
        {
          field_name: "officieleTitel",
          confidence: 0.95,
          reasoning: "Title extracted directly from document header"
        },
        {
          field_name: "informatiecategorie",
          confidence: 0.82,
          reasoning: "Classified based on content analysis and keywords"
        },
        {
          field_name: "trefwoorden",
          confidence: 0.78,
          reasoning: "Keywords extracted from document body"
        },
        {
          field_name: "publisher",
          confidence: 0.9,
          reasoning: "Publisher identified from document header"
        },
        {
          field_name: "creatiedatum",
          confidence: 0.88,
          reasoning: "Date extracted from document metadata"
        }
      ]
    },
    model_used: "mistralai/mistral-large-2411",
    processing_time_ms: 2847
  },
  error: null
});

const DOC_UUID = "550e8400-e29b-41d4-a716-446655440000";

describe("useGenerateMetadata", () => {
  let fetchSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    fetchSpy = vi.fn();
    vi.stubGlobal("fetch", fetchSpy);
    mockToastAdd.mockClear();
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("sets isGenerating to true during the request and false after", async () => {
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createWooHooResponse())
    });

    const { isGenerating, generateMetadata } = useGenerateMetadata();

    expect(isGenerating.value).toBe(false);

    const promise = generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(isGenerating.value).toBe(true);

    await promise;

    expect(isGenerating.value).toBe(false);
  });

  it("calls the correct backend endpoint with document UUID", async () => {
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createWooHooResponse())
    });

    const { generateMetadata } = useGenerateMetadata();
    await generateMetadata(DOC_UUID, createPublicatie(), []);

    expect(fetchSpy).toHaveBeenCalledWith(`/api/v1/metadata/generate/${DOC_UUID}`, {
      method: "POST",
      headers: { "is-api": "true" }
    });
  });

  it("maps publication-level metadata from a full woo-hoo response", async () => {
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createWooHooResponse())
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result).not.toBeNull();
    expect(result!.publicatie.officieleTitel).toBe(
      "Advies inzake wijziging bestemmingsplan centrum"
    );
    expect(result!.publicatie.verkorteTitel).toBe("Advies bestemmingsplan centrum");
    expect(result!.publicatie.omschrijving).toBe(
      "Advies van de commissie ruimtelijke ordening over de voorgenomen wijziging van het bestemmingsplan voor het centrumgebied, met betrekking tot horecabestemmingen."
    );
    expect(result!.publicatie.datumBeginGeldigheid).toBe("2024-03-15");
    expect(result!.publicatie.datumEindeGeldigheid).toBe("2025-03-15");
  });

  it("matches informatiecategorieen by label to waardelijst UUIDs", async () => {
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createWooHooResponse())
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result!.publicatie.informatieCategorieen).toEqual(["cat-uuid-1"]);
  });

  it("matches themas to onderwerpen waardelijst UUIDs", async () => {
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createWooHooResponse())
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result!.publicatie.onderwerpen).toEqual(["ond-uuid-1"]);
  });

  it("matches labels case-insensitively", async () => {
    const response = createWooHooResponse({
      classificatiecollectie: {
        informatiecategorieen: [
          {
            categorie: "ADVIEZEN",
            resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_5ba23c01",
            label: "adviezen"
          }
        ],
        documentsoorten: null,
        themas: null,
        trefwoorden: null
      }
    });

    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(response)
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result!.publicatie.informatieCategorieen).toEqual(["cat-uuid-1"]);
  });

  it("maps trefwoorden to kenmerken with bron 'woo-hoo'", async () => {
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createWooHooResponse())
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result!.publicatie.kenmerken).toEqual([
      { kenmerk: "bestemmingsplan", bron: "woo-hoo" },
      { kenmerk: "horeca", bron: "woo-hoo" },
      { kenmerk: "centrum", bron: "woo-hoo" }
    ]);
  });

  it("maps document-level metadata including documenthandelingen dates", async () => {
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createWooHooResponse())
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result!.document).toBeDefined();
    expect(result!.document!.officieleTitel).toBe(
      "Advies inzake wijziging bestemmingsplan centrum"
    );
    expect(result!.document!.omschrijving).toBe(
      "Advies van de commissie ruimtelijke ordening over de voorgenomen wijziging van het bestemmingsplan voor het centrumgebied, met betrekking tot horecabestemmingen."
    );
    expect(result!.document!.creatiedatum).toBe("2024-03-15");
    expect(result!.document!.datumOndertekend).toBe("2024-03-14T10:00:00Z");
    expect(result!.document!.ontvangstdatum).toBe("2024-03-13T09:30:00Z");
  });

  it("does not set document update when document UUID is not in documenten list", async () => {
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createWooHooResponse())
    });

    const { generateMetadata } = useGenerateMetadata();
    const doc = createDocument({ uuid: "other-uuid" });
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [doc]);

    expect(result!.document).toBeUndefined();
  });

  it("returns null and shows error toast on HTTP 502 (backend proxy failure)", async () => {
    fetchSpy.mockResolvedValue({
      ok: false,
      status: 502,
      text: () => Promise.resolve("")
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result).toBeNull();
    expect(mockToastAdd).toHaveBeenCalledWith({
      text: "Metadata generatie mislukt: HTTP 502",
      type: "error"
    });
  });

  it("returns null and shows error toast on HTTP 503 (service not configured)", async () => {
    fetchSpy.mockResolvedValue({
      ok: false,
      status: 503,
      text: () => Promise.resolve("Metadata generation service is not configured.")
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result).toBeNull();
    expect(mockToastAdd).toHaveBeenCalledWith({
      text: "Metadata generatie mislukt: Metadata generation service is not configured.",
      type: "error"
    });
  });

  it("returns null and shows error toast when woo-hoo returns success=false", async () => {
    fetchSpy.mockResolvedValue({
      ok: true,
      json: () =>
        Promise.resolve({
          success: false,
          request_id: "req_error",
          suggestion: null,
          error: "OpenRouter API key not configured. Set OPENROUTER_API_KEY environment variable."
        })
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result).toBeNull();
    expect(mockToastAdd).toHaveBeenCalledWith({
      text: "Metadata generatie mislukt: OpenRouter API key not configured. Set OPENROUTER_API_KEY environment variable.",
      type: "error"
    });
  });

  it("returns null and shows error toast on network failure", async () => {
    fetchSpy.mockRejectedValue(new TypeError("Failed to fetch"));

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result).toBeNull();
    expect(mockToastAdd).toHaveBeenCalledWith({
      text: "Metadata generatie mislukt: Failed to fetch",
      type: "error"
    });
  });

  it("resets isGenerating to false even on error", async () => {
    fetchSpy.mockRejectedValue(new Error("network error"));

    const { isGenerating, generateMetadata } = useGenerateMetadata();
    await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(isGenerating.value).toBe(false);
  });

  it("handles a minimal response with only required fields", async () => {
    const minimalResponse = {
      success: true,
      request_id: "req_minimal",
      suggestion: {
        metadata: {
          identifiers: null,
          publisher: null,
          titelcollectie: {
            officieleTitel: "Minimaal document",
            verkorteTitels: null,
            alternatieveTitels: null
          },
          omschrijvingen: null,
          classificatiecollectie: null,
          creatiedatum: null,
          geldigheid: null,
          language: null,
          format: null,
          documenthandelingen: [],
          verantwoordelijke: null,
          medeverantwoordelijken: null,
          opsteller: null,
          naamOpsteller: null,
          aggregatiekenmerk: null,
          isPartOf: null,
          hasParts: null,
          documentrelaties: null,
          redenVerwijderingVervanging: null
        },
        confidence: {
          overall: 0.4,
          fields: [
            {
              field_name: "officieleTitel",
              confidence: 0.6,
              reasoning: "Only a basic title could be extracted"
            }
          ]
        },
        model_used: "mistralai/mistral-large-2411",
        processing_time_ms: 1200
      },
      error: null
    };

    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(minimalResponse)
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result!.publicatie.officieleTitel).toBe("Minimaal document");
    expect(result!.publicatie.verkorteTitel).toBeUndefined();
    expect(result!.publicatie.omschrijving).toBeUndefined();
    expect(result!.publicatie.datumBeginGeldigheid).toBeUndefined();
    expect(result!.publicatie.informatieCategorieen).toBeUndefined();
    expect(result!.publicatie.onderwerpen).toBeUndefined();
    expect(result!.publicatie.kenmerken).toBeUndefined();
  });

  it("handles datetime-to-date conversion for geldigheid fields", async () => {
    const response = createWooHooResponse({
      geldigheid: {
        begindatum: "2024-06-01T12:30:00Z",
        einddatum: "2025-06-01"
      }
    });

    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(response)
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result!.publicatie.datumBeginGeldigheid).toBe("2024-06-01");
    expect(result!.publicatie.datumEindeGeldigheid).toBe("2025-06-01");
  });

  it("skips informatiecategorieen labels that have no matching waardelijst entry", async () => {
    const response = createWooHooResponse({
      classificatiecollectie: {
        informatiecategorieen: [
          {
            categorie: "ADVIEZEN",
            resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_5ba23c01",
            label: "Adviezen"
          },
          {
            categorie: "WOO_VERZOEKEN",
            resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_woo",
            label: "Woo-verzoeken en -besluiten"
          }
        ],
        documentsoorten: null,
        themas: null,
        trefwoorden: null
      }
    });

    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(response)
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    // Only "Adviezen" matches our waardelijst, "Woo-verzoeken en -besluiten" does not
    expect(result!.publicatie.informatieCategorieen).toEqual(["cat-uuid-1"]);
  });

  it("checkAvailability sets isAvailable to true when health endpoint returns ok", async () => {
    fetchSpy.mockResolvedValue({ ok: true });

    const { isAvailable, checkAvailability } = useGenerateMetadata();

    expect(isAvailable.value).toBe(false);

    await checkAvailability();

    expect(isAvailable.value).toBe(true);
    expect(fetchSpy).toHaveBeenCalledWith("/api/v1/metadata/health", {
      headers: { "is-api": "true" }
    });
  });

  it("checkAvailability sets isAvailable to false when health endpoint returns non-ok", async () => {
    fetchSpy.mockResolvedValue({ ok: false, status: 503 });

    const { isAvailable, checkAvailability } = useGenerateMetadata();
    await checkAvailability();

    expect(isAvailable.value).toBe(false);
  });

  it("checkAvailability sets isAvailable to false on network error", async () => {
    fetchSpy.mockRejectedValue(new TypeError("Failed to fetch"));

    const { isAvailable, checkAvailability } = useGenerateMetadata();
    await checkAvailability();

    expect(isAvailable.value).toBe(false);
  });

  it("handles response with multiple informatiecategorieen matching", async () => {
    const response = createWooHooResponse({
      classificatiecollectie: {
        informatiecategorieen: [
          {
            categorie: "ADVIEZEN",
            resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_5ba23c01",
            label: "Adviezen"
          },
          {
            categorie: "CONVENANTEN",
            resource: "https://identifier.overheid.nl/tooi/def/thes/kern/c_conv",
            label: "Convenanten"
          }
        ],
        documentsoorten: null,
        themas: null,
        trefwoorden: null
      }
    });

    fetchSpy.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(response)
    });

    const { generateMetadata } = useGenerateMetadata();
    const result = await generateMetadata(DOC_UUID, createPublicatie(), [createDocument()]);

    expect(result!.publicatie.informatieCategorieen).toEqual(["cat-uuid-1", "cat-uuid-2"]);
  });
});
