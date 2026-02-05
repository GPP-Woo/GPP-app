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

    <dl v-if="model.uuid">
      <dt>ID</dt>
      <dd>{{ model.uuid }}</dd>

      <dt>Publicatie-eigenaar</dt>
      <dd>{{ model.eigenaar?.weergaveNaam }}</dd>
    </dl>

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
import { computed, onMounted, ref, useModel, watch, type DeepReadonly } from "vue";
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
  mijnGebruikersgroepen: DeepReadonly<MijnGebruikersgroep[]>;
  groepWaardelijsten: {
    organisaties?: OptionProps[];
    informatiecategorieen?: OptionProps[];
    onderwerpen?: OptionProps[];
  };
}>();

const model = useModel(props, "modelValue");

const kenmerken = useKenmerken(model);

const { lijsten } = useAppData();

const expandOptionGroup = ref(false);

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

// When groepWaardelijsten don't match the publicatie eigenaarGroep (unauthorized)
// or when publicatie has status 'ingetrokken', the form is displayed as readonly/disabled
// In readonly mode waardelijsten are constructed based on all/existing waardelijsten because there is (unauthorized) -
// or there may be (ingetrokken) a mismatch in waardelijsten between the publicatie and groepWaardelijsten
const waardelijsten = computed(() =>
  props.isReadonly
    ? {
        organisaties: lijsten.value?.organisaties.filter((item) =>
          model.value.publisher?.includes(item.uuid)
        ),
        informatiecategorieen: lijsten.value?.informatiecategorieen.filter((item) =>
          model.value.informatieCategorieen.includes(item.uuid)
        ),
        onderwerpen: lijsten.value?.onderwerpen.filter((item) =>
          model.value.onderwerpen.includes(item.uuid)
        )
      }
    : props.groepWaardelijsten
);

// Externally created publicaties will not have a eigenaarGroep untill updated from ODPC
const isPublicatieWithoutEigenaarGroep = computed(
  () => !!model.value.uuid && !model.value.eigenaarGroep
);

const presetSingleGebruikersgroep = () => {
  if (props.mijnGebruikersgroepen?.length !== 1) return;

  const { uuid, naam } = props.mijnGebruikersgroepen[0];

  model.value.eigenaarGroep = { identifier: uuid, weergaveNaam: naam };
};

const presetSingleWaardelijstOption = () => {
  const { organisaties, informatiecategorieen } = props.groepWaardelijsten;

  if (organisaties?.length === 1) model.value.publisher = organisaties[0].uuid;

  if (informatiecategorieen?.length === 1)
    model.value.informatieCategorieen.push(informatiecategorieen[0].uuid);
};

watch(
  eigenaarGroepIdentifier,
  (_, oldGroep) => {
    // Clear waardelijsten of publicatie when mismatch waardelijsten <> gebruikersgroep (=unauthorized) on
    // a) switch from one to another eigenaarGroep or
    // b) initial select eigenaarGroep when isPublicatieWithoutEigenaarGroep
    if (props.unauthorized && (!!oldGroep || isPublicatieWithoutEigenaarGroep.value)) {
      model.value.publisher = "";
      model.value.informatieCategorieen = [];
      model.value.onderwerpen = [];

      // Expand/show the cleared option groups to user
      expandOptionGroup.value = true;
    }

    // Preset publisher and informatiecategorie of a new publicatie when only one option available
    if (!model.value.uuid) presetSingleWaardelijstOption();
  },
  { flush: "post" }
);

// Preset eigenaarGroep of a new - or externally created publicatie when only one mijnGebruikersgroep
onMounted(() => {
  if (!model.value.uuid || isPublicatieWithoutEigenaarGroep.value) {
    presetSingleGebruikersgroep();
  }
});
</script>

<style lang="scss" scoped>
dl {
  display: flex;
  flex-direction: column;
  margin-block: 0;

  dt {
    color: inherit;
    font-weight: var(--font-bold);
    margin-block-end: var(--spacing-small);
  }

  dd {
    margin-inline: 0;
    margin-block-end: calc(var(--spacing-small) + var(--spacing-default));
  }
}
</style>
