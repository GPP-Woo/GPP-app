<template>
  <fieldset>
    <legend>Gebruikersgroep gegevens</legend>

    <div class="form-group">
      <label for="titel">Naam *</label>

      <input
        id="titel"
        type="text"
        v-model="naam"
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

    <div class="form-group form-group-button">
      <label for="gebruiker">Gebruiker toevoegen</label>

      <input
        id="gebruiker"
        type="text"
        v-model="gebruiker"
        :aria-invalid="!gebruiker.trim().length"
        @keydown.enter.prevent="addGebruiker"
      />

      <button
        type="button"
        :disabled="!gebruiker.trim().length"
        :aria-disabled="!gebruiker.trim().length"
        @click="addGebruiker"
      >
        Toevoegen
      </button>
    </div>

    <details ref="detailsRef" aria-live="polite">
      <summary>Toegevoegde gebruikers</summary>

      <p v-if="!gekoppeldeGebruikers.length">Er zijn nog geen gebruikers toegevoegd.</p>

      <ul v-else class="reset">
        <li v-for="(gekoppeldeGebruiker, index) in gekoppeldeGebruikers" :key="index">
          <button
            type="button"
            :title="`${gekoppeldeGebruiker} verwijderen`"
            :aria-label="`${gekoppeldeGebruiker} verwijderen`"
            class="button secondary icon-after xmark"
            @click="() => removeGebruiker(index)"
          >
            {{ gekoppeldeGebruiker }}
          </button>
        </li>
      </ul>
    </details>
  </fieldset>
</template>

<script setup lang="ts">
import { computed, ref, useModel } from "vue";
import toast from "@/stores/toast";
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

const detailsRef = ref<HTMLDetailsElement>();

const gebruiker = ref("");

const naam = useModelProperty("naam");

const omschrijving = useModelProperty("omschrijving");

const gekoppeldeGebruikers = useModelProperty("gekoppeldeGebruikers");

const addGebruiker = () => {
  if (!gebruiker.value.trim().length) return;

  if (detailsRef.value) detailsRef.value.open = true;

  if (gekoppeldeGebruikers.value.includes(gebruiker.value)) {
    toast.add({
      text: "Deze gebruiker is al toegevoegd.",
      type: "error"
    });
    return;
  }

  gekoppeldeGebruikers.value = [...gekoppeldeGebruikers.value, gebruiker.value.trim()];

  gebruiker.value = "";
};

const removeGebruiker = (index: number) => {
  gekoppeldeGebruikers.value = gekoppeldeGebruikers.value.filter((_, i) => i !== index);
};
</script>

<style lang="scss" scoped>
ul {
  display: flex;
  flex-wrap: wrap;
  column-gap: var(--spacing-small);
}
</style>
