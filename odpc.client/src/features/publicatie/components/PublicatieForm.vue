<template>
  <fieldset :disabled="disabled">
    <legend>Publicatie</legend>

    <alert-inline v-if="model.publicatiestatus === PublicatieStatus.ingetrokken"
      >Deze publicatie is ingetrokken.</alert-inline
    >

    <alert-inline v-else-if="forbidden"
      >U heeft niet (meer) de juiste rechten om deze publicatie aan te passen. Neem contact op met
      uw beheerder.</alert-inline
    >

    <div v-if="!disabled" class="form-group">
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
        <option v-if="!model.gebruikersgroep" value="">Kies een profiel</option>

        <option v-for="{ uuid, naam } in mijnGebruikersgroepen" :key="uuid" :value="uuid">
          {{ naam }}
        </option>
      </select>

      <span id="profielError" class="error">Profiel is een verplicht veld</span>
    </div>

    <template v-if="model.gebruikersgroep || disabled">
      <div v-if="model.uuid && !disabled" class="form-group form-group-radio">
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

      <div v-if="model.uuid" class="form-group">
        <label for="uuid">ID</label>

        <input id="uuid" type="text" v-model="model.uuid" readonly aria-readonly="true" />
      </div>

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
        v-if="waardelijsten.organisaties?.length"
        type="radio"
        title="Organisatie"
        :key="model.gebruikersgroep"
        :options="waardelijsten.organisaties"
        v-model="model.publisher"
        :required="true"
      />

      <option-group
        v-if="waardelijsten.informatiecategorieen?.length"
        type="checkbox"
        title="Informatiecategorieën"
        :key="model.gebruikersgroep"
        :options="waardelijsten.informatiecategorieen"
        v-model="model.informatieCategorieen"
        :required="true"
      />

      <option-group
        v-if="waardelijsten.onderwerpen?.length"
        type="checkbox"
        title="Onderwerpen"
        :key="model.gebruikersgroep"
        :options="waardelijsten.onderwerpen"
        v-model="model.onderwerpen"
      />
    </template>
  </fieldset>
</template>

<script setup lang="ts">
import { computed, useModel } from "vue";
import { injectLijsten } from "@/stores/lijsten";
import AlertInline from "@/components/AlertInline.vue";
import OptionGroup from "@/components/option-group/OptionGroup.vue";
import { PublicatieStatus, type MijnGebruikersgroep, type Publicatie } from "../types";
import type { OptionProps } from "@/components/option-group/types";

const props = defineProps<{
  modelValue: Publicatie;
  disabled: boolean;
  forbidden: boolean;
  mijnGebruikersgroepen: MijnGebruikersgroep[];
  gekoppeldeWaardelijsten: {
    organisaties?: OptionProps[];
    informatiecategorieen?: OptionProps[];
    onderwerpen?: OptionProps[];
  };
}>();

const model = useModel(props, "modelValue");

const lijsten = injectLijsten();

const waardelijsten = computed(() =>
  props.disabled
    ? {
        organisaties: lijsten?.organisaties.filter((item) =>
          model.value.publisher.includes(item.uuid)
        ),
        informatiecategorieen: lijsten?.informatiecategorieen.filter((item) =>
          model.value.informatieCategorieen.includes(item.uuid)
        ),
        onderwerpen: lijsten?.onderwerpen.filter((item) =>
          model.value.onderwerpen.includes(item.uuid)
        )
      }
    : props.gekoppeldeWaardelijsten
);
</script>

<style lang="scss" scoped>
input[type="text"]:read-only {
  background-color: var(--disabled);
}
</style>
