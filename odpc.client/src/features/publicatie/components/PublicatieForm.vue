<template>
  <fieldset :disabled="readonly">
    <legend>Publicatie</legend>

    <alert-inline v-if="model.publicatiestatus === PublicatieStatus.ingetrokken"
      >Deze publicatie is ingetrokken.</alert-inline
    >

    <alert-inline v-else-if="forbidden"
      >Onder dit profiel heb je niet (meer) de juiste rechten om deze publicatie aan te passen. Neem
      contact op met de beheerder.</alert-inline
    >

    <alert-inline v-else-if="model.uuid && model.publicatiestatus === PublicatieStatus.concept"
      >Deze publicatie is nog in concept.</alert-inline
    >

    <div class="form-group">
      <label for="gebruikersgroep">Profiel *</label>

      <select
        name="gebruikersgroep"
        id="gebruikersgroep"
        v-model="model.gebruikersgroep"
        required
        aria-required="true"
        aria-describedby="profielError"
        :aria-invalid="!model.gebruikersgroep"
      >
        <option v-if="!model.gebruikersgroep && !readonly" value="">Kies een profiel</option>

        <option v-for="{ uuid, naam } in mijnGebruikersgroepen" :key="uuid" :value="uuid">
          {{ naam }}
        </option>
      </select>

      <span id="profielError" class="error">Profiel is een verplicht veld</span>
    </div>

    <template v-if="model.gebruikersgroep || readonly">
      <div v-if="model.uuid" class="form-group">
        <label for="uuid">ID</label>

        <input id="uuid" type="text" v-model="model.uuid" readonly aria-readonly="true" />
      </div>

      <div class="form-group">
        <label for="titel">Titel *</label>

        <input
          id="titel"
          type="text"
          v-model.trim="model.officieleTitel"
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
        v-if="waardelijsten.organisaties?.length"
        type="radio"
        title="Organisatie"
        :key="model.gebruikersgroep"
        :options="waardelijsten.organisaties"
        v-model="model.publisher"
        :required="true"
        :open="expandOptionGroup"
      />

      <option-group
        v-if="waardelijsten.informatiecategorieen?.length"
        type="checkbox"
        title="Informatiecategorieën"
        :key="model.gebruikersgroep"
        :options="waardelijsten.informatiecategorieen"
        v-model="model.informatieCategorieen"
        :required="true"
        :open="expandOptionGroup"
      />

      <option-group
        v-if="waardelijsten.onderwerpen?.length"
        type="checkbox"
        title="Onderwerpen"
        :key="model.gebruikersgroep"
        :options="waardelijsten.onderwerpen"
        v-model="model.onderwerpen"
        :open="expandOptionGroup"
      />
    </template>
  </fieldset>
</template>

<script setup lang="ts">
import { computed, ref, useModel, watch } from "vue";
import AlertInline from "@/components/AlertInline.vue";
import OptionGroup from "@/components/option-group/OptionGroup.vue";
import { useAppData } from "@/composables/use-app-data";
import { PublicatieStatus, type MijnGebruikersgroep, type Publicatie } from "../types";
import type { OptionProps } from "@/components/option-group/types";

const props = defineProps<{
  modelValue: Publicatie;
  forbidden: boolean;
  readonly: boolean;
  mijnGebruikersgroepen: MijnGebruikersgroep[];
  gekoppeldeWaardelijsten: {
    organisaties?: OptionProps[];
    informatiecategorieen?: OptionProps[];
    onderwerpen?: OptionProps[];
  };
}>();

const model = useModel(props, "modelValue");

const { lijsten } = useAppData();

// When gekoppeldeWaardelijsten don't match the publicatie (forbidden)
// or when publicatie has status 'ingetrokken', the form is displayed as readonly/disabled
// In readonly mode waardelijsten are constructed based on all/existing waardelijsten because there is (forbidden) -
// or there may be (ingestrokken) a mismatch in waardelijsten between the publicatie and gekoppeldeWaardelijsten
const waardelijsten = computed(() =>
  props.readonly
    ? {
        organisaties: lijsten.value?.organisaties.filter((item) =>
          model.value.publisher.includes(item.uuid)
        ),
        informatiecategorieen: lijsten.value?.informatiecategorieen.filter((item) =>
          model.value.informatieCategorieen.includes(item.uuid)
        ),
        onderwerpen: lijsten.value?.onderwerpen.filter((item) =>
          model.value.onderwerpen.includes(item.uuid)
        )
      }
    : props.gekoppeldeWaardelijsten
);

// Expand option groups after gebruikersgroep changes and waardelijst values are empty
const expandOptionGroup = ref(false);

watch(
  () => model.value.gebruikersgroep,
  (_, oldGroep) =>
    (expandOptionGroup.value =
      !!oldGroep &&
      model.value.publisher.length === 0 &&
      model.value.informatieCategorieen.length === 0 &&
      model.value.onderwerpen.length === 0)
);
</script>

<style lang="scss" scoped>
input[type="text"]:read-only {
  background-color: var(--disabled);
}
</style>
