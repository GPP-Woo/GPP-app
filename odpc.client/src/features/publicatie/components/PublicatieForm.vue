<template>
  <fieldset :disabled="isReadonly">
    <legend>Publicatie</legend>

    <alert-inline v-if="model.publicatiestatus === PublicatieStatus.ingetrokken"
      >Deze publicatie is ingetrokken.</alert-inline
    >

    <alert-inline v-else-if="unauthorized"
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
        v-model="eigenaarGroepIdentifier"
        required
        aria-required="true"
        aria-describedby="profielError"
        :aria-invalid="!eigenaarGroepIdentifier"
      >
        <option v-if="!eigenaarGroepIdentifier && !isReadonly" :value="null">
          Kies een profiel
        </option>

        <option v-for="{ uuid, naam } in mijnGebruikersgroepen" :key="uuid" :value="uuid">
          {{ naam }}
        </option>
      </select>

      <span id="profielError" class="error">Profiel is een verplicht veld</span>
    </div>

    <template v-if="eigenaarGroepIdentifier || isReadonly">
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

        <date-input
          v-model="model.datumBeginGeldigheid"
          id="datumBeginGeldigheid"
          label="Datum in werking"
          :disabled="isReadonly"
        />

        <date-input
          v-model="model.datumEindeGeldigheid"
          id="datumEindeGeldigheid"
          label="Datum buiten werking"
          :disabled="isReadonly"
        />

        <add-remove-items
          v-model="kenmerken"
          item-name-singular="kenmerk"
          item-name-plural="kenmerken"
          :is-readonly="isReadonly"
        />
      </details>

      <option-group
        v-if="waardelijsten.organisaties?.length"
        type="radio"
        title="Organisatie"
        :key="eigenaarGroepIdentifier ?? undefined"
        :options="waardelijsten.organisaties"
        v-model="model.publisher"
        :required="!isDraftMode"
        :open="expandOptionGroup"
      />

      <option-group
        v-if="waardelijsten.informatiecategorieen?.length"
        type="checkbox"
        title="Informatiecategorieën"
        :key="eigenaarGroepIdentifier ?? undefined"
        :options="waardelijsten.informatiecategorieen"
        v-model="model.informatieCategorieen"
        :required="!isDraftMode"
        :open="expandOptionGroup"
      />

      <option-group
        v-if="waardelijsten.onderwerpen?.length"
        type="checkbox"
        title="Onderwerpen"
        :key="eigenaarGroepIdentifier ?? undefined"
        :options="waardelijsten.onderwerpen"
        v-model="model.onderwerpen"
        :open="expandOptionGroup"
      />

      <publicatie-archivering
        v-if="model.publicatiestatus !== PublicatieStatus.concept"
        v-bind="modelValue"
      />
    </template>
  </fieldset>
</template>

<script setup lang="ts">
import { computed, ref, useModel, watch } from "vue";
import AlertInline from "@/components/AlertInline.vue";
import OptionGroup from "@/components/option-group/OptionGroup.vue";
import AddRemoveItems from "@/components/AddRemoveItems.vue";
import DateInput from "@/components/DateInput.vue";
import { useAppData } from "@/composables/use-app-data";
import { useKenmerken } from "../composables/use-kenmerken";
import { PublicatieStatus, type MijnGebruikersgroep, type Publicatie } from "../types";
import type { OptionProps } from "@/components/option-group/types";
import PublicatieArchivering from "./PublicatieArchivering.vue";

const props = defineProps<{
  modelValue: Publicatie;
  unauthorized: boolean;
  isReadonly: boolean;
  isDraftMode: boolean;
  mijnGebruikersgroepen: MijnGebruikersgroep[];
  gekoppeldeWaardelijsten: {
    organisaties?: OptionProps[];
    informatiecategorieen?: OptionProps[];
    onderwerpen?: OptionProps[];
  };
}>();

const model = useModel(props, "modelValue");

const kenmerken = useKenmerken(model);

const { lijsten } = useAppData();

const eigenaarGroepIdentifier = computed({
  get: () => model.value.eigenaarGroep?.identifier ?? null,
  set: (identifier: string | null) =>
    (model.value.eigenaarGroep = identifier
      ? {
          identifier,
          weergaveNaam: props.mijnGebruikersgroepen.find((g) => g.uuid === identifier)?.naam ?? ""
        }
      : null)
});

// When gekoppeldeWaardelijsten don't match the publicatie (unauthorized)
// or when publicatie has status 'ingetrokken', the form is displayed as readonly/disabled
// In readonly mode waardelijsten are constructed based on all/existing waardelijsten because there is (unauthorized) -
// or there may be (ingestrokken) a mismatch in waardelijsten between the publicatie and gekoppeldeWaardelijsten
const waardelijsten = computed(() =>
  props.isReadonly
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

// Expand option groups after eigenaarGroep changes and waardelijst values are empty
const expandOptionGroup = ref(false);

watch(
  () => eigenaarGroepIdentifier,
  (_, oldGroep) =>
    (expandOptionGroup.value =
      !!oldGroep &&
      model.value.publisher.length === 0 &&
      model.value.informatieCategorieen.length === 0 &&
      model.value.onderwerpen.length === 0)
);
</script>
