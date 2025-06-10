<template>
  <fieldset>
    <legend>Gebruikersgroep gegevens</legend>

    <div class="form-group">
      <label for="titel">Naam *</label>

      <input
        id="titel"
        type="text"
        v-model.trim="naam"
        required
        aria-required="true"
        aria-describedby="titelError"
        :aria-invalid="!naam"
      />

      <span id="titelError" class="error">Naam is een verplicht veld</span>
    </div>

    <div class="form-group">
      <label for="omschrijving">Omschrijving</label>

      <textarea id="omschrijving" v-model="omschrijving" rows="4"></textarea>
    </div>

    <add-remove-items
      v-model="gekoppeldeGebruikers"
      item-name-singular="gebruiker"
      item-name-plural="gebruikers"
    />
  </fieldset>
</template>

<script setup lang="ts">
import { computed, useModel } from "vue";
import AddRemoveItems from "@/components/AddRemoveItems.vue";
import type { Gebruikersgroep } from "../types";

const props = defineProps<{ modelValue: Gebruikersgroep }>();

const model = useModel(props, "modelValue");

const useModelProperty = <K extends keyof Gebruikersgroep>(key: K) =>
  computed({
    get: () => model.value[key],
    set: (v) => {
      model.value = { ...props.modelValue, [key]: v };
    }
  });

const naam = useModelProperty("naam");

const omschrijving = useModelProperty("omschrijving");

const gekoppeldeGebruikers = useModelProperty("gekoppeldeGebruikers");
</script>

<style lang="scss" scoped>
ul {
  display: flex;
  flex-wrap: wrap;
  column-gap: var(--spacing-small);
}
</style>
