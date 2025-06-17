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

      <div
        v-if="!readonly && doc.publicatiestatus === PublicatieStatus.gepubliceerd"
        class="form-group"
      >
        <label
          ><input
            type="checkbox"
            v-model="doc.pendingRetract"
            :aria-describedby="`pendingRetract-${detailsId}`"
          />
          Document intrekken</label
        >

        <span v-show="doc.pendingRetract" :id="`pendingRetract-${detailsId}`" class="alert"
          >Let op: deze actie kan niet ongedaan worden gemaakt.</span
        >
      </div>
    </template>

    <div class="form-group">
      <label for="creatiedatum">Datum document *</label>

      <input
        id="creatiedatum"
        type="date"
        v-model="doc.creatiedatum"
        required
        aria-required="true"
        :aria-describedby="`creatiedatumError-${detailsId}`"
        :aria-invalid="!doc.creatiedatum"
        :max="today"
      />

      <span :id="`creatiedatumError-${detailsId}`" class="error">Vul een geldige datum in.</span>
    </div>

    <div class="form-group">
      <label for="titel">Titel *</label>

      <input
        id="titel"
        type="text"
        v-model.trim="doc.officieleTitel"
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

    <add-remove-items
      v-model="kenmerken"
      item-name-singular="kenmerk"
      item-name-plural="kenmerken"
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
import { useId, useModel } from "vue";
import AddRemoveItems from "@/components/AddRemoveItems.vue";
import { useKenmerken } from "../composables/use-kenmerken";
import { PublicatieStatus, type PublicatieDocument } from "../types";

const props = defineProps<{ doc: PublicatieDocument; readonly?: boolean }>();

const doc = useModel(props, "doc");

const kenmerken = useKenmerken(doc);

const detailsId = useId();

const today = new Date().toISOString().split("T")[0];
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
