import { ref } from "vue";
import { useAppData } from "@/composables/use-app-data";

import type {
	Publicatie,
	PublicatieDocument,
	FieldSuggestion,
	DocumentSuggestion,
	MetadataPreviewData
} from "../types";

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
	return value.split("T")[0];
};

const toDateTimeString = (value?: string) => {
	if (!value) return null;
	return value.includes("T") ? value : `${value}T00:00:00Z`;
};

// Field labels for display
const PUBLICATION_FIELD_LABELS: Record<string, string> = {
	officieleTitel: "Titel",
	verkorteTitel: "Verkorte titel",
	omschrijving: "Omschrijving",
	datumBeginGeldigheid: "Datum in werking",
	datumEindeGeldigheid: "Datum buiten werking",
	informatieCategorieen: "Informatiecategorieën",
	onderwerpen: "Onderwerpen",
	kenmerken: "Trefwoorden"
};

const DOCUMENT_FIELD_LABELS: Record<string, string> = {
	officieleTitel: "Titel",
	omschrijving: "Omschrijving",
	creatiedatum: "Datum document",
	datumOndertekend: "Datum ondertekend",
	ontvangstdatum: "Datum ontvangst"
};

export const useGenerateMetadata = () => {
	const isGenerating = ref(false);
	const isAvailable = ref(false);
	const { lijsten } = useAppData();

	const checkAvailability = async () => {
		try {
			const response = await fetch(`/api/v1/metadata/health`);
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
			.filter((label): label is string => !!label)
			.map((label) => lijst.find((item) => item.naam.toLowerCase() === label.toLowerCase())?.uuid)
			.filter((uuid): uuid is string => !!uuid);
	};

	/**
	 * Generate metadata for a single document (internal helper)
	 */
	const generateForDocument = async (documentUuid: string): Promise<WooHooResponse | null> => {
		try {
			const response = await fetch(
				`/api/v1/metadata/generate/${encodeURIComponent(documentUuid)}`,
				{ method: "POST", headers: { "is-api": "true" } }
			);

			if (!response.ok) {
				const errorText = await response.text();
				throw new Error(errorText || `HTTP ${response.status}`);
			}

			const contentType = response.headers.get("content-type") ?? "";
			if (!contentType.includes("application/json")) {
				throw new Error(`Unexpected response type: ${contentType || "(empty)"}`);
			}

			return await response.json();
		} catch (err) {
			console.error(`Metadata generation failed for ${documentUuid}:`, err);
			return null;
		}
	};

	/**
	 * Create a field suggestion with current and suggested values
	 */
	const DATE_FIELDS = new Set([
		"datumBeginGeldigheid",
		"datumEindeGeldigheid",
		"creatiedatum",
		"datumOndertekend",
		"ontvangstdatum"
	]);

	const LIST_FIELDS = new Set(["informatieCategorieen", "onderwerpen", "kenmerken"]);

	const getFieldType = (field: string): FieldSuggestion["type"] => {
		if (DATE_FIELDS.has(field)) return "date";
		if (LIST_FIELDS.has(field)) return "list";
		return "text";
	};

	const createFieldSuggestion = (
		field: string,
		label: string,
		currentValue: unknown,
		suggestedValue: unknown
	): FieldSuggestion | null => {
		// Only suggest if there's a value to suggest
		if (suggestedValue === null || suggestedValue === undefined || suggestedValue === "") {
			return null;
		}
		// For arrays, only suggest if non-empty
		if (Array.isArray(suggestedValue) && suggestedValue.length === 0) {
			return null;
		}

		return {
			field,
			label,
			currentValue,
			suggestedValue,
			// Pre-select if current value is empty
			selected: !currentValue || (Array.isArray(currentValue) && currentValue.length === 0),
			type: getFieldType(field)
		};
	};

	/**
	 * Extract publication-level suggestions from woo-hoo response
	 */
	const extractPublicationSuggestions = (
		meta: NonNullable<WooHooResponse["suggestion"]>["metadata"],
		publicatie: Publicatie
	): FieldSuggestion[] => {
		const suggestions: FieldSuggestion[] = [];

		// Title
		const titleSuggestion = createFieldSuggestion(
			"officieleTitel",
			PUBLICATION_FIELD_LABELS.officieleTitel,
			publicatie.officieleTitel,
			meta.titelcollectie?.officieleTitel
		);
		if (titleSuggestion) suggestions.push(titleSuggestion);

		// Short title
		const shortTitleSuggestion = createFieldSuggestion(
			"verkorteTitel",
			PUBLICATION_FIELD_LABELS.verkorteTitel,
			publicatie.verkorteTitel,
			meta.titelcollectie?.verkorteTitels?.[0]
		);
		if (shortTitleSuggestion) suggestions.push(shortTitleSuggestion);

		// Description
		const descSuggestion = createFieldSuggestion(
			"omschrijving",
			PUBLICATION_FIELD_LABELS.omschrijving,
			publicatie.omschrijving,
			meta.omschrijvingen?.[0]
		);
		if (descSuggestion) suggestions.push(descSuggestion);

		// Validity dates
		if (meta.geldigheid?.begindatum) {
			const beginSuggestion = createFieldSuggestion(
				"datumBeginGeldigheid",
				PUBLICATION_FIELD_LABELS.datumBeginGeldigheid,
				publicatie.datumBeginGeldigheid,
				toDateString(meta.geldigheid.begindatum)
			);
			if (beginSuggestion) suggestions.push(beginSuggestion);
		}

		if (meta.geldigheid?.einddatum) {
			const endSuggestion = createFieldSuggestion(
				"datumEindeGeldigheid",
				PUBLICATION_FIELD_LABELS.datumEindeGeldigheid,
				publicatie.datumEindeGeldigheid,
				toDateString(meta.geldigheid.einddatum)
			);
			if (endSuggestion) suggestions.push(endSuggestion);
		}

		// Information categories (match by label)
		if (meta.classificatiecollectie?.informatiecategorieen?.length) {
			const labels = meta.classificatiecollectie.informatiecategorieen
				.map((c: { label: string }) => c.label)
				.filter(Boolean);
			const matched = findMatchingUuids(labels, lijsten.value?.informatiecategorieen);
			if (matched.length) {
				const catSuggestion = createFieldSuggestion(
					"informatieCategorieen",
					PUBLICATION_FIELD_LABELS.informatieCategorieen,
					publicatie.informatieCategorieen,
					matched
				);
				if (catSuggestion) suggestions.push(catSuggestion);
			}
		}

		// Topics/themes (match by label)
		if (meta.classificatiecollectie?.themas?.length) {
			const labels = meta.classificatiecollectie.themas.map((t: { label: string }) => t.label).filter(Boolean);
			const matched = findMatchingUuids(labels, lijsten.value?.onderwerpen);
			if (matched.length) {
				const themaSuggestion = createFieldSuggestion(
					"onderwerpen",
					PUBLICATION_FIELD_LABELS.onderwerpen,
					publicatie.onderwerpen,
					matched
				);
				if (themaSuggestion) suggestions.push(themaSuggestion);
			}
		}

		// Keywords as kenmerken
		if (meta.classificatiecollectie?.trefwoorden?.length) {
			const kenmerken = meta.classificatiecollectie.trefwoorden.map((kw: string) => ({
				kenmerk: kw,
				bron: "woo-hoo"
			}));
			const kenmerkSuggestion = createFieldSuggestion(
				"kenmerken",
				PUBLICATION_FIELD_LABELS.kenmerken,
				publicatie.kenmerken,
				kenmerken
			);
			if (kenmerkSuggestion) suggestions.push(kenmerkSuggestion);
		}

		return suggestions;
	};

	/**
	 * Extract document-level suggestions from woo-hoo response
	 */
	const extractDocumentSuggestions = (
		meta: NonNullable<WooHooResponse["suggestion"]>["metadata"],
		document: PublicatieDocument
	): FieldSuggestion[] => {
		const suggestions: FieldSuggestion[] = [];

		// Title
		const titleSuggestion = createFieldSuggestion(
			"officieleTitel",
			DOCUMENT_FIELD_LABELS.officieleTitel,
			document.officieleTitel,
			meta.titelcollectie?.officieleTitel
		);
		if (titleSuggestion) suggestions.push(titleSuggestion);

		// Description
		const descSuggestion = createFieldSuggestion(
			"omschrijving",
			DOCUMENT_FIELD_LABELS.omschrijving,
			document.omschrijving,
			meta.omschrijvingen?.[0]
		);
		if (descSuggestion) suggestions.push(descSuggestion);

		// Creation date
		if (meta.creatiedatum) {
			const dateSuggestion = createFieldSuggestion(
				"creatiedatum",
				DOCUMENT_FIELD_LABELS.creatiedatum,
				document.creatiedatum,
				toDateString(meta.creatiedatum)
			);
			if (dateSuggestion) suggestions.push(dateSuggestion);
		}

		// Document handling dates
		type Handeling = { soortHandeling: { handeling: string }; atTime?: string };
		if (meta.documenthandelingen?.length) {
			const ondertekening = meta.documenthandelingen.find(
				(h: Handeling) => h.soortHandeling.handeling === "ONDERTEKENING"
			);
			if (ondertekening?.atTime) {
				const signedSuggestion = createFieldSuggestion(
					"datumOndertekend",
					DOCUMENT_FIELD_LABELS.datumOndertekend,
					document.datumOndertekend,
					toDateTimeString(ondertekening.atTime)
				);
				if (signedSuggestion) suggestions.push(signedSuggestion);
			}

			const ontvangst = meta.documenthandelingen.find(
				(h: Handeling) => h.soortHandeling.handeling === "ONTVANGST"
			);
			if (ontvangst?.atTime) {
				const receivedSuggestion = createFieldSuggestion(
					"ontvangstdatum",
					DOCUMENT_FIELD_LABELS.ontvangstdatum,
					document.ontvangstdatum,
					toDateTimeString(ontvangst.atTime)
				);
				if (receivedSuggestion) suggestions.push(receivedSuggestion);
			}
		}

		return suggestions;
	};

	/**
	 * Generate metadata preview for all documents
	 * Returns preview data for the modal - doesn't apply anything yet
	 */
	const lastError = ref<string | null>(null);

	const generateMetadataPreview = async (
		publicatie: Publicatie,
		documenten: PublicatieDocument[],
		mainDocumentUuid?: string
	): Promise<MetadataPreviewData | null> => {
		lastError.value = null;
		isGenerating.value = true;

		try {
			// If no main document specified, use the first one with a UUID
			const docsWithUuid = documenten.filter((d) => d.uuid);
			const mainDoc = mainDocumentUuid
				? docsWithUuid.find((d) => d.uuid === mainDocumentUuid)
				: docsWithUuid[0];

			if (!mainDoc?.uuid) {
				throw new Error("Geen document geselecteerd voor metadata generatie");
			}

			const allPublicationSuggestions: FieldSuggestion[] = [];
			const allDocumentSuggestions: DocumentSuggestion[] = [];
			const allKeywords = new Set<string>();

			// Process all documents in parallel
			const results = await Promise.allSettled(
				docsWithUuid.map((doc) => generateForDocument(doc.uuid!))
			);

			for (let i = 0; i < docsWithUuid.length; i++) {
				const result = results[i];
				if (result.status !== "fulfilled") continue;

				const response = result.value;
				if (!response?.success || !response.suggestion) continue;

				const doc = docsWithUuid[i];
				const meta = response.suggestion.metadata;

				// Document-level suggestions
				const docSuggestions = extractDocumentSuggestions(meta, doc);
				if (docSuggestions.length > 0) {
					allDocumentSuggestions.push({
						documentUuid: doc.uuid!,
						documentName: doc.bestandsnaam,
						fields: docSuggestions
					});
				}

				// Publication-level suggestions only from main document
				if (doc.uuid === mainDoc.uuid) {
					const pubSuggestions = extractPublicationSuggestions(meta, publicatie);
					allPublicationSuggestions.push(...pubSuggestions);
				}

				// Aggregate keywords from all documents
				if (meta.classificatiecollectie?.trefwoorden) {
					for (const kw of meta.classificatiecollectie.trefwoorden) {
						allKeywords.add(kw);
					}
				}
			}

			// If we have aggregated keywords from multiple documents, update the kenmerken suggestion
			if (allKeywords.size > 0 && documenten.length > 1) {
				const existingKenmerkIndex = allPublicationSuggestions.findIndex(
					(s) => s.field === "kenmerken"
				);
				const aggregatedKenmerken = Array.from(allKeywords).map((kw) => ({
					kenmerk: kw,
					bron: "woo-hoo"
				}));

				if (existingKenmerkIndex >= 0) {
					allPublicationSuggestions[existingKenmerkIndex].suggestedValue = aggregatedKenmerken;
				} else {
					const kenmerkSuggestion = createFieldSuggestion(
						"kenmerken",
						PUBLICATION_FIELD_LABELS.kenmerken,
						publicatie.kenmerken,
						aggregatedKenmerken
					);
					if (kenmerkSuggestion) allPublicationSuggestions.push(kenmerkSuggestion);
				}
			}

			if (allPublicationSuggestions.length === 0 && allDocumentSuggestions.length === 0) {
				throw new Error("Geen metadata suggesties gevonden in de documenten");
			}

			return {
				publicationSuggestions: allPublicationSuggestions,
				documentSuggestions: allDocumentSuggestions,
				mainDocumentName: mainDoc.bestandsnaam
			};
		} catch (err) {
			const message = err instanceof Error
				? err.message
				: "Er is iets misgegaan bij het genereren van metadata.";
			lastError.value = message;
			return null;
		} finally {
			isGenerating.value = false;
		}
	};

	/**
	 * Apply selected metadata suggestions to publication and documents
	 */
	const applyMetadataSuggestions = (
		previewData: MetadataPreviewData
	): { publicatie: Partial<Publicatie>; documents: Map<string, Partial<PublicatieDocument>> } => {
		const publicatieUpdate: Partial<Publicatie> = {};
		const documentUpdates = new Map<string, Partial<PublicatieDocument>>();

		// Apply selected publication suggestions
		for (const suggestion of previewData.publicationSuggestions) {
			if (suggestion.selected) {
				(publicatieUpdate as Record<string, unknown>)[suggestion.field] = suggestion.suggestedValue;
			}
		}

		// Apply selected document suggestions
		for (const docSuggestion of previewData.documentSuggestions) {
			const docUpdate: Partial<PublicatieDocument> = {};
			let hasUpdates = false;

			for (const suggestion of docSuggestion.fields) {
				if (suggestion.selected) {
					(docUpdate as Record<string, unknown>)[suggestion.field] = suggestion.suggestedValue;
					hasUpdates = true;
				}
			}

			if (hasUpdates) {
				documentUpdates.set(docSuggestion.documentUuid, docUpdate);
			}
		}

		return { publicatie: publicatieUpdate, documents: documentUpdates };
	};

	return {
		isGenerating,
		isAvailable,
		lastError,
		checkAvailability,
		generateMetadataPreview,
		applyMetadataSuggestions
	};
};
