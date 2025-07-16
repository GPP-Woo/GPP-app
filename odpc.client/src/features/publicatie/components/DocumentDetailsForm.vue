<template>
  <details :class="{ nieuw: !doc.uuid }" :open="!doc.uuid">
    <summary v-if="!doc.uuid" @click.prevent tabindex="-1">
      {{ doc.bestandsnaam }}
    </summary>

    <template v-else>
      <summary>
        <template v-if="doc.publicatiestatus === PublicatieStatus.ingetrokken"
          ><s :aria-describedby="`status-${detailsId}`">{{ doc.bestandsnaam }}</s>
          <span :id="`status-${detailsId}`" role="status">(ingetrokken)</span></template
        >
        <template v-else>{{ doc.bestandsnaam }}</template>

        <span>
          (<a
            :href="`/api/v2/documenten/${doc.uuid}/download`"
            :title="`Download ${doc.bestandsnaam}`"
            >download</a
          >)
        </span>
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
import DateInput from "@/components/DateInput.vue";
import { useKenmerken } from "../composables/use-kenmerken";
import { PublicatieStatus, PendingDocumentActions, type PublicatieDocument } from "../types";
import { ISOToday } from "@/helpers";

const props = defineProps<{ doc: PublicatieDocument; isReadonly?: boolean }>();

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
  span {
    font-weight: normal;
    margin-inline-start: var(--spacing-extrasmall);
  }

  &.nieuw {
    summary {
      list-style: none;
      pointer-events: none;

      &::-webkit-details-marker {
        display: none;
      }
    }
  }

  &.ingetrokken {
    background-color: var(--disabled);
  }
}
</style>
