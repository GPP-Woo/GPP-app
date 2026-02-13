<template>
	<dialog ref="dialogRef" class="metadata-preview-modal" @close="onClose">
		<form method="dialog">
			<header>
				<h2>Metadata suggesties van AI</h2>
				<p class="description">
					Selecteer welke suggesties je wilt toepassen. Velden met bestaande waarden worden
					gemarkeerd.
				</p>
			</header>

			<div class="options">
				<label class="checkbox-label">
					<input v-model="skipFilledFields" type="checkbox" />
					<span>Velden met bestaande waarden overslaan</span>
				</label>
			</div>

			<div class="suggestions-container">
				<!-- Publication-level fields -->
				<section v-if="publicationSuggestions.length > 0" class="suggestion-section">
					<h3>
						Publicatie velden
						<span class="source-badge">van: {{ mainDocumentName }}</span>
					</h3>

					<ul class="suggestion-list">
						<li
							v-for="suggestion in publicationSuggestions"
							:key="suggestion.field"
							class="suggestion-item"
							:class="{ 'has-value': suggestion.currentValue }"
						>
							<label class="checkbox-label">
								<input
									v-model="suggestion.selected"
									type="checkbox"
									:disabled="skipFilledFields && Boolean(suggestion.currentValue)"
								/>
								<span class="field-name">{{ suggestion.label }}</span>
							</label>

							<div class="field-values">
								<div v-if="suggestion.currentValue" class="current-value">
									<span class="value-label">Huidig:</span>
									<span class="value-text">{{ formatValue(suggestion.currentValue) }}</span>
								</div>
								<div class="suggested-value">
									<span class="value-label">Suggestie:</span>
									<span class="value-text">{{ formatValue(suggestion.suggestedValue) }}</span>
								</div>
							</div>
						</li>
					</ul>
				</section>

				<!-- Document-level fields for each document -->
				<section
					v-for="docSuggestion in documentSuggestions"
					:key="docSuggestion.documentUuid"
					class="suggestion-section"
				>
					<h3>
						{{ docSuggestion.documentName }}
					</h3>

					<ul class="suggestion-list">
						<li
							v-for="suggestion in docSuggestion.fields"
							:key="suggestion.field"
							class="suggestion-item"
							:class="{ 'has-value': suggestion.currentValue }"
						>
							<label class="checkbox-label">
								<input
									v-model="suggestion.selected"
									type="checkbox"
									:disabled="skipFilledFields && Boolean(suggestion.currentValue)"
								/>
								<span class="field-name">{{ suggestion.label }}</span>
							</label>

							<div class="field-values">
								<div v-if="suggestion.currentValue" class="current-value">
									<span class="value-label">Huidig:</span>
									<span class="value-text">{{ formatValue(suggestion.currentValue) }}</span>
								</div>
								<div class="suggested-value">
									<span class="value-label">Suggestie:</span>
									<span class="value-text">{{ formatValue(suggestion.suggestedValue) }}</span>
								</div>
							</div>
						</li>
					</ul>
				</section>

				<div v-if="isLoading" class="loading-state">
					<span class="spinner"></span>
					<span>Metadata genereren...</span>
				</div>

				<div v-if="error" class="error-state">
					<span>{{ error }}</span>
				</div>
			</div>

			<menu class="reset actions">
				<li>
					<button type="submit" value="cancel" class="button secondary">Annuleren</button>
				</li>
				<li>
					<button
						type="submit"
						value="confirm"
						class="button"
						:disabled="!hasSelectedSuggestions"
					>
						Toepassen ({{ selectedCount }})
					</button>
				</li>
			</menu>
		</form>
	</dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import { whenever } from "@vueuse/core";

export type FieldSuggestion = {
	field: string;
	label: string;
	currentValue: unknown;
	suggestedValue: unknown;
	selected: boolean;
};

export type DocumentSuggestion = {
	documentUuid: string;
	documentName: string;
	fields: FieldSuggestion[];
};

export type MetadataPreviewData = {
	publicationSuggestions: FieldSuggestion[];
	documentSuggestions: DocumentSuggestion[];
	mainDocumentName: string;
};

const props = defineProps<{
	isRevealed: boolean;
	isLoading: boolean;
	error: string | null;
	data: MetadataPreviewData | null;
}>();

const emit = defineEmits<{
	close: [];
	confirm: [data: MetadataPreviewData];
}>();

const dialogRef = ref<HTMLDialogElement>();
const skipFilledFields = ref(true);

const publicationSuggestions = computed(() => props.data?.publicationSuggestions ?? []);
const documentSuggestions = computed(() => props.data?.documentSuggestions ?? []);
const mainDocumentName = computed(() => props.data?.mainDocumentName ?? "");

const selectedCount = computed(() => {
	let count = publicationSuggestions.value.filter((s) => s.selected).length;
	for (const doc of documentSuggestions.value) {
		count += doc.fields.filter((s) => s.selected).length;
	}
	return count;
});

const hasSelectedSuggestions = computed(() => selectedCount.value > 0);

const formatValue = (value: unknown): string => {
	if (value === null || value === undefined || value === "") {
		return "(leeg)";
	}
	if (Array.isArray(value)) {
		if (value.length === 0) return "(leeg)";
		// Handle kenmerken array
		if (value[0] && typeof value[0] === "object" && "kenmerk" in value[0]) {
			return value.map((k) => k.kenmerk).join(", ");
		}
		return value.join(", ");
	}
	if (typeof value === "string" && value.length > 100) {
		return value.substring(0, 100) + "...";
	}
	return String(value);
};

const onClose = () => {
	if (dialogRef.value?.returnValue === "confirm" && props.data) {
		emit("confirm", props.data);
	} else {
		emit("close");
	}
};

// Toggle selection of filled fields when skipFilledFields changes
watch(skipFilledFields, (skip) => {
	if (!props.data) return;

	for (const suggestion of props.data.publicationSuggestions) {
		if (suggestion.currentValue) {
			// When skip is ON, deselect filled fields; when OFF, select them
			suggestion.selected = !skip;
		}
	}
	for (const doc of props.data.documentSuggestions) {
		for (const suggestion of doc.fields) {
			if (suggestion.currentValue) {
				suggestion.selected = !skip;
			}
		}
	}
});

whenever(
	() => props.isRevealed,
	() => {
		dialogRef.value?.showModal();
	},
	{ immediate: true }
);
</script>

<style lang="scss" scoped>
.metadata-preview-modal {
	min-width: min(90vw, 700px);
	max-width: 90vw;
	max-height: 85vh;
	padding: var(--spacing-large);
	border: 1px solid var(--border);
	border-radius: var(--radius-default);

	&::backdrop {
		background-color: rgb(102 102 102 / 80%);
	}
}

form {
	display: flex;
	flex-direction: column;
	gap: var(--spacing-default);
	max-height: calc(85vh - 2 * var(--spacing-large));
}

header {
	h2 {
		margin: 0 0 var(--spacing-small);
	}

	.description {
		margin: 0;
		color: var(--text-secondary);
		font-size: 0.9em;
	}
}

.options {
	padding: var(--spacing-small) 0;
	border-bottom: 1px solid var(--border);
}

.checkbox-label {
	display: flex;
	gap: var(--spacing-small);
	align-items: flex-start;
	cursor: pointer;

	input[type="checkbox"] {
		margin-top: 0.2em;
		flex-shrink: 0;
	}

	input:disabled + span {
		opacity: 0.5;
	}
}

.suggestions-container {
	flex: 1;
	overflow-y: auto;
	min-height: 200px;
}

.suggestion-section {
	margin-bottom: var(--spacing-large);

	h3 {
		display: flex;
		align-items: center;
		gap: var(--spacing-small);
		margin: 0 0 var(--spacing-small);
		padding: var(--spacing-small);
		background: var(--background-secondary);
		border-radius: var(--radius-small);
		font-size: 1em;

		.source-badge {
			margin-left: auto;
			font-size: 0.8em;
			font-weight: normal;
			color: var(--text-secondary);
		}
	}
}

.suggestion-list {
	margin: 0;
	padding: 0;
	list-style: none;
}

.suggestion-item {
	padding: var(--spacing-small);
	border: 1px solid var(--border);
	border-radius: var(--radius-small);
	margin-bottom: var(--spacing-small);

	&.has-value {
		border-left: 3px solid var(--color-warning, #f59e0b);
		background: var(--background-warning-subtle, #fffbeb);
	}

	.field-name {
		font-weight: 500;
	}
}

.field-values {
	margin-top: var(--spacing-small);
	margin-left: calc(1rem + var(--spacing-small));
	font-size: 0.9em;
}

.current-value,
.suggested-value {
	display: flex;
	gap: var(--spacing-small);
	margin-bottom: 0.25em;
}

.value-label {
	color: var(--text-secondary);
	min-width: 5em;
}

.value-text {
	flex: 1;
	word-break: break-word;
}

.current-value .value-text {
	text-decoration: line-through;
	opacity: 0.7;
}

.suggested-value .value-text {
	font-weight: 500;
}

.loading-state,
.error-state {
	display: flex;
	align-items: center;
	justify-content: center;
	gap: var(--spacing-default);
	padding: var(--spacing-large);
	color: var(--text-secondary);
}

.error-state {
	color: var(--color-error, #dc2626);
}

.spinner {
	width: 1.5em;
	height: 1.5em;
	border: 2px solid var(--border);
	border-top-color: var(--color-primary);
	border-radius: 50%;
	animation: spin 1s linear infinite;
}

@keyframes spin {
	to {
		transform: rotate(360deg);
	}
}

.actions {
	display: flex;
	gap: var(--spacing-default);
	justify-content: flex-end;
	padding-top: var(--spacing-default);
	border-top: 1px solid var(--border);
	margin: 0;
}
</style>
