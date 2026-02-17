<template>
  <fieldset>
    <legend>Gebruikersgroep gegevens</legend>

    <div v-if="model.uuid" class="form-group">
      <label for="uuid">ID</label>

      <input id="uuid" type="text" v-model="model.uuid" readonly aria-readonly="true" />
    </div>

    <div class="form-group">
      <label for="titel">Naam *</label>

      <input
        id="titel"
        type="text"
        v-model.trim="model.naam"
        required
        aria-required="true"
        aria-describedby="titelError"
        :aria-invalid="!model.naam"
      />

      <span id="titelError" class="error">Naam is een verplicht veld</span>
    </div>

    <div class="form-group">
      <label for="omschrijving">Omschrijving</label>

      <textarea id="omschrijving" v-model="model.omschrijving" rows="4"></textarea>
    </div>

    <add-remove-gebruikers v-model="model.gekoppeldeGebruikers" />
  </fieldset>
</template>

<script setup lang="ts">
import AddRemoveGebruikers from "./AddRemoveGebruikers.vue";
import type { Gebruikersgroep } from "../types";

const model = defineModel<Gebruikersgroep>("modelValue", { required: true });
</script>
