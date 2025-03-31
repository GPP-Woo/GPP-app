<template>
  <fieldset :disabled="disabled">
    <legend>Publicatie</legend>

    <template v-if="model.uuid">
      <div v-if="!disabled" class="form-group form-group-radio">
        <label>
          <input
            type="radio"
            v-model="model.publicatiestatus"
            :value="PublicatieStatus.gepubliceerd"
          />
          Gepubliceerd
        </label>

        <label
          ><input
            type="radio"
            v-model="model.publicatiestatus"
            :value="PublicatieStatus.ingetrokken"
          />
          Ingetrokken</label
        >
      </div>

      <alert-inline v-else>Deze publicatie is ingetrokken.</alert-inline>

      <div class="form-group">
        <label for="uuid">ID</label>

        <input id="uuid" type="text" v-model="model.uuid" readonly aria-readonly="true" />
      </div>
    </template>

    <div class="form-group">
      <label for="titel">Titel *</label>

      <input
        id="titel"
        type="text"
        v-model="model.officieleTitel"
        required
        aria-required="true"
        aria-describedby="titelError"
        :aria-invalid="!model.officieleTitel"
      />

      <span id="titelError" class="error">Titel is een verplicht veld</span>
    </div>

    <details>
      <summary>Meer details</summary>

      <div class="form-group">
        <label for="verkorte_titel">Verkorte titel</label>

        <input id="verkorte_titel" type="text" v-model="model.verkorteTitel" />
      </div>

      <div class="form-group">
        <label for="omschrijving">Omschrijving</label>

        <textarea id="omschrijving" v-model="model.omschrijving" rows="4"></textarea>
      </div>
    </details>

    <option-group
      type="radio"
      title="Organisatie"
      :options="mijnOrganisaties"
      v-model="model.publisher"
      :required="true"
    />

    <option-group
      type="checkbox"
      title="Informatiecategorieën"
      :options="mijnInformatiecategorieen"
      v-model="model.informatieCategorieen"
      :required="true"
    />

    <option-group
      type="checkbox"
      title="Onderwerpen"
      :options="mijnOnderwerpen"
      v-model="model.onderwerpen"
    />
  </fieldset>
</template>

<script setup lang="ts">
import { useModel } from "vue";
import AlertInline from "@/components/AlertInline.vue";
import OptionGroup from "@/components/option-group/OptionGroup.vue";
import { PublicatieStatus, type Publicatie } from "../types";
import type { OptionProps } from "@/components/option-group/types";

const props = defineProps<{
  modelValue: Publicatie;
  disabled: boolean;
  mijnOrganisaties: OptionProps[];
  mijnInformatiecategorieen: OptionProps[];
  mijnOnderwerpen: OptionProps[];
}>();

const model = useModel(props, "modelValue");
</script>

<style lang="scss" scoped>
input[type="text"]:read-only {
  background-color: var(--disabled);
}
</style>
