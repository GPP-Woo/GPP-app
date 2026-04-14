<template>
  <details :class="{ nieuw: !doc.uuid }" :open="!doc.uuid">
    <summary v-if="!doc.uuid" @click.prevent tabindex="-1">
      {{ doc.bestandsnaam }}
    </summary>

    <template v-else>
      <summary>
        <template v-if="doc.publicatiestatus === PublicatieStatus.ingetrokken"
          ><s class="summary-text" :aria-describedby="`status-${detailsId}`">{{
            doc.officieleTitel
          }}</s>
          <span :id="`status-${detailsId}`" role="status">ingetrokken</span></template
        >
        <span class="summary-text" v-else>{{ doc.officieleTitel }}</span>

        <span v-if="warnings?.length" class="icon-before alert">mogelijk dubbel</span>

        <a
          :href="`/api/v2/documenten/${doc.uuid}/download`"
          :title="`Download ${doc.bestandsnaam}`"
          class="icon-after download"
          ><span class="visually-hidden">Download {{ doc.bestandsnaam }}</span></a
        >
      </summary>

      <div v-if="!isReadonly" class="form-group">
        <label
          ><input
            type="checkbox"
            v-model="pendingAction"
            :value="PendingDocumentActions[doc.publicatiestatus]"
            :aria-describedby="`pendingAction-${detailsId}`"
          />
          {{
            doc.publicatiestatus === PublicatieStatus.concept
              ? `Document verwijderen`
              : `Document intrekken`
          }}
        </label>

        <span v-show="doc.pendingAction" :id="`pendingAction-${detailsId}`" class="alert"
          >Let op: deze actie kan niet ongedaan worden gemaakt.</span
        >
      </div>
    </template>

    <alert-inline v-for="(warning, index) in warnings" :key="index">
      <span class="icon-before icon-large alert" role="presentation" aria-hidden="true"></span>

      {{ warning }}
    </alert-inline>

    <date-input
      v-model="doc.creatiedatum"
      :id="`creatiedatum-${detailsId}`"
      label="Datum document"
      :max-date="ISOToday"
      :required="true"
      :disabled="isReadonly"
    />

    <div class="form-group">
      <label :for="`titel-${detailsId}`">Titel *</label>

      <input
        :id="`titel-${detailsId}`"
        type="text"
        v-model.trim="doc.officieleTitel"
        required
        aria-required="true"
        :aria-describedby="`titelError-${detailsId}`"
        :aria-invalid="!doc.officieleTitel"
        v-bind="disabledAttrs"
      />

      <span :id="`titelError-${detailsId}`" class="error">Titel is een verplicht veld.</span>
    </div>

    <div class="form-group">
      <label :for="`verkorte_titel-${detailsId}`">Verkorte titel</label>

      <input
        :id="`verkorte_titel-${detailsId}`"
        type="text"
        v-model="doc.verkorteTitel"
        v-bind="disabledAttrs"
      />
    </div>

    <div class="form-group">
      <label :for="`omschrijving-${detailsId}`">Omschrijving</label>

      <textarea
        :id="`omschrijving-${detailsId}`"
        v-model="doc.omschrijving"
        rows="4"
        v-bind="disabledAttrs"
      ></textarea>
    </div>

    <date-input
      v-model="doc.ontvangstdatum"
      :id="`ontvangstdatum-${detailsId}`"
      label="Datum ontvangst"
      :max-date="ISOToday"
      :to-date-time="true"
      :disabled="isReadonly"
    />

    <date-input
      v-model="doc.datumOndertekend"
      :id="`datumOndertekend-${detailsId}`"
      label="Datum ondertekening (intern)"
      :max-date="ISOToday"
      :to-date-time="true"
      :disabled="isReadonly"
    />

    <add-remove-items
      v-model="kenmerken"
      item-name-singular="kenmerk"
      item-name-plural="kenmerken"
      :is-readonly="isReadonly"
      help-text="Bijvoorbeeld het intern of extern documentnummer."
    />

    <button
      v-if="!doc.uuid"
      type="button"
      class="button secondary icon-after trash"
      @click="$emit(`removeDocument`)"
    >
      Verwijderen
    </button>
  </details>
</template>

<script setup lang="ts">
import { computed, useId, useModel } from "vue";
import AddRemoveItems from "@/components/AddRemoveItems.vue";
import AlertInline from "@/components/AlertInline.vue";
import DateInput from "@/components/DateInput.vue";
import { useKenmerken } from "../composables/use-kenmerken";
import { PublicatieStatus, PendingDocumentActions, type PublicatieDocument } from "../types";
import { ISOToday } from "@/helpers";

const props = defineProps<{ doc: PublicatieDocument; isReadonly?: boolean; warnings?: string[] }>();

const doc = useModel(props, "doc");

const kenmerken = useKenmerken(doc);

const detailsId = useId();

const pendingAction = computed({
  get: () => !!doc.value.pendingAction,
  set: (checked) => {
    doc.value.pendingAction = checked ? PendingDocumentActions[doc.value.publicatiestatus] : null;
  }
});

const disabledAttrs = computed(() =>
  props.isReadonly ? { disabled: true, "aria-disabled": true } : {}
);
</script>

<style lang="scss" scoped>
details {
  summary {
    .summary-text {
      flex: 1;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    :not(.summary-text) {
      display: flex;
      flex-shrink: 0;
      font-weight: normal;
      font-style: italic;

      &::before {
        margin-inline-end: 0.5ch;
      }

      &::after {
        min-block-size: 1.3rem;
        min-inline-size: 1.3rem;
      }
    }
  }

  &.nieuw {
    summary {
      pointer-events: none;

      &::before {
        display: none;
      }
    }
  }

  .notice {
    display: flex;
    column-gap: 1ch;
    padding: 1rem;
    border-color: var(--accent);
    background-color: var(--bg);
  }
}
</style>
