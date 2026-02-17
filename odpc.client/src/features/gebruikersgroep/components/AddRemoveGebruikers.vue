<template>
  <div class="form-group form-group-button">
    <label for="gebruiker"
      >Gebruiker toevoegen
      <info-popover>
        <template #trigger="{ triggerProps }">
          <button type="button" class="button secondary popover-trigger" v-bind="triggerProps">
            ?
          </button>
        </template>

        <p class="popover-content">
          Voer hier de gebruikersnaam van de gebruiker in. De naam van de gebruiker wordt
          automatisch opgehaald zodra deze inlogt in het GPP-Woo publicatieportaal.
        </p>
      </info-popover></label
    >

    <input
      id="gebruiker"
      type="text"
      v-model.trim="gebruiker"
      :aria-invalid="!gebruiker"
      @keydown.enter.prevent="addGebruiker"
    />

    <button type="button" :disabled="!gebruiker" :aria-disabled="!gebruiker" @click="addGebruiker">
      Toevoegen
    </button>
  </div>

  <details ref="detailsRef" aria-live="polite">
    <summary>Toegevoegde gebruikers</summary>

    <p v-if="!gebruikers.length">Er zijn (nog) geen gebruikers toegevoegd.</p>

    <table v-else class="table">
      <thead>
        <tr>
          <th>Gebruikersnaam</th>
          <th>Naam</th>
          <th class="actions">
            <span class="visually-hidden">Acties</span>
          </th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="({ gebruikerId, naam, lastLogin }, index) in gebruikers" :key="gebruikerId">
          <td :title="gebruikerId">{{ gebruikerId }}</td>
          <td :title="naam ?? undefined">
            {{ lastLogin && !naam ? "Ophalen naam mislukt" : (naam ?? "-") }}
          </td>
          <td>
            <button
              :title="`${gebruikerId} verwijderen`"
              :aria-label="`${gebruikerId} verwijderen`"
              class="button secondary icon-after trash"
              @click="removeGebruiker(index)"
            ></button>
          </td>
        </tr>
      </tbody>
    </table>
  </details>
</template>

<script setup lang="ts">
import { ref } from "vue";
import toast from "@/stores/toast";
import InfoPopover from "@/components/InfoPopover.vue";
import type { GekoppeldeGebruiker } from "../types";

const gebruikers = defineModel<GekoppeldeGebruiker[]>("modelValue", { required: true });
const gebruiker = ref("");

const detailsRef = ref<HTMLDetailsElement>();

const addGebruiker = () => {
  if (gebruikers.value.some((g) => g.gebruikerId === gebruiker.value)) {
    toast.add({
      text: "Deze gebruiker is al toegevoegd.",
      type: "error"
    });

    return;
  }

  if (detailsRef.value) detailsRef.value.open = true;

  gebruikers.value = [...gebruikers.value, { gebruikerId: gebruiker.value }];
  gebruiker.value = "";
};

const removeGebruiker = (index: number) =>
  (gebruikers.value = gebruikers.value.filter((_, i) => i !== index));
</script>

<style lang="scss" scoped>
table {
  table-layout: fixed;
  inline-size: 100%;

  th,
  td {
    font-size: 0.875rem;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    border-inline: none;

    &.actions {
      inline-size: 3rem;
    }
  }

  button {
    margin-block: 0;
  }
}

.popover-trigger {
  block-size: 1.5rem;
  inline-size: 1.5rem;
  padding: 0;
  margin: 0;
}

.popover-content {
  font-weight: normal;
  margin-block: 0;
  cursor: text;
}
</style>
