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
            :href="`/api/v1/documenten/${doc.uuid}/download`"
            :title="`Download ${doc.bestandsnaam}`"
            >download</a
          >)
        </span>
      </summary>

      <div v-if="!disabled" class="form-group form-group-radio">
        <label>
          <input
            type="radio"
            v-model="doc.publicatiestatus"
            :value="PublicatieStatus.gepubliceerd"
          />
          Gepubliceerd
        </label>

        <label
          ><input
            type="radio"
            v-model="doc.publicatiestatus"
            :value="PublicatieStatus.ingetrokken"
          />
          Ingetrokken</label
        >
      </div>
    </template>

    <div class="form-group">
      <label for="creatiedatum">Creatiedatum *</label>

      <input
        id="creatiedatum"
        type="date"
        v-model="doc.creatiedatum"
        required
        aria-required="true"
        :aria-describedby="`creatiedatumError-${detailsId}`"
        :aria-invalid="!doc.creatiedatum"
      />

      <span :id="`creatiedatumError-${detailsId}`" class="error"
        >Vul een geldige creatiedatum in.</span
      >
    </div>

    <div class="form-group">
      <label for="titel">Titel *</label>

      <input
        id="titel"
        type="text"
        v-model="doc.officieleTitel"
        required
        aria-required="true"
        :aria-describedby="`titelError-${detailsId}`"
        :aria-invalid="!doc.officieleTitel"
      />

      <span :id="`titelError-${detailsId}`" class="error">Titel is een verplicht veld.</span>
    </div>

    <div class="form-group">
      <label for="verkorte_titel">Verkorte titel</label>

      <input id="verkorte_titel" type="text" v-model="doc.verkorteTitel" />
    </div>

    <div class="form-group">
      <label for="omschrijving">Omschrijving</label>

      <textarea id="omschrijving" v-model="doc.omschrijving" rows="4"></textarea>
    </div>

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
import { useId, useModel } from "vue";
import { PublicatieStatus, type PublicatieDocument } from "../types";

const props = defineProps<{ doc: PublicatieDocument; disabled?: boolean }>();

const doc = useModel(props, "doc");

const detailsId = useId();
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

  input[type="date"] {
    max-inline-size: 15ch;
  }
}
</style>
