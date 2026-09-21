<template>
  <fieldset :disabled="isReadonly">
    <legend>Inzage-procedure</legend>

    <div v-if="!model && canLink" class="form-group">
      <label
        ><input type="checkbox" v-model="linkInzageProcedure" /> Inzage-procedure koppelen</label
      >
    </div>

    <template v-if="model">
      <date-input
        v-model="model.datumBeginInzagetermijn"
        id="datumBeginInzagetermijn"
        label="Begindatum inzagetermijn"
        :min-date="ISOToday"
        :max-date="model.datumEindeInzagetermijn || undefined"
        :required="!isDraftMode"
      />

      <date-input
        v-model="model.datumEindeInzagetermijn"
        id="datumEindeInzagetermijn"
        label="Einddatum inzagetermijn"
        :min-date="minDatumEindeInzagetermijn"
        :required="!isDraftMode"
      />

      <fieldset class="rechtsmiddel">
        <legend>Beschikbaar rechtsmiddel <template v-if="!isDraftMode">*</template></legend>

        <label
          ><input
            type="radio"
            name="beschikbaarRechtsmiddel"
            :value="BeschikbaarRechtsmiddel.zienswijze"
            v-model="model.beschikbaarRechtsmiddel"
            :required="!isDraftMode"
          />
          Zienswijze</label
        >

        <label
          ><input
            type="radio"
            name="beschikbaarRechtsmiddel"
            :value="BeschikbaarRechtsmiddel.bezwaar"
            v-model="model.beschikbaarRechtsmiddel"
            :required="!isDraftMode"
          />
          Bezwaar</label
        >
      </fieldset>

      <div v-if="model.urlReactieformulier" class="form-group">
        <label for="urlReactieformulier">URL reactieformulier</label>

        <input
          id="urlReactieformulier"
          type="url"
          :value="model.urlReactieformulier"
          readonly
          aria-readonly="true"
          disabled
        />
      </div>

      <div class="form-group">
        <label for="toelichting">Toelichting <template v-if="!isDraftMode">*</template></label>

        <textarea
          id="toelichting"
          v-model.trim="model.toelichting"
          rows="4"
          :required="!isDraftMode"
          aria-required="true"
          :aria-invalid="!isDraftMode && !model.toelichting"
        ></textarea>

        <span class="error">Toelichting is een verplicht veld</span>
      </div>

      <div class="form-group">
        <label for="urlBekendmaking">URL bekendmaking</label>

        <input id="urlBekendmaking" type="url" v-model.trim="model.urlBekendmaking" />
      </div>

      <div class="form-group">
        <label
          ><input type="checkbox" v-model="model.automatischIntrekken" /> Automatisch intrekken na
          afloop inzagetermijn</label
        >
      </div>
    </template>
  </fieldset>
</template>

<script setup lang="ts">
import { computed, useModel } from "vue";
import DateInput from "@/components/DateInput.vue";
import { ISOToday, ISOTomorrow } from "@/helpers/date";
import { BeschikbaarRechtsmiddel, type InzageProcedure } from "../types";

const props = defineProps<{
  modelValue: InzageProcedure | null;
  canLink: boolean;
  isReadonly: boolean;
  isDraftMode: boolean;
}>();

const model = useModel(props, "modelValue");

const initialInzageProcedure = (): InzageProcedure => ({
  publicatie: "",
  toelichting: "",
  beschikbaarRechtsmiddel: "",
  datumBeginInzagetermijn: ISOToday ?? "",
  datumEindeInzagetermijn: "",
  automatischIntrekken: false
});

const linkInzageProcedure = computed({
  get: () => !!model.value,
  set: (checked) => (model.value = checked ? initialInzageProcedure() : null)
});

const minDatumEindeInzagetermijn = computed(() => {
  const isoTomorrow = ISOTomorrow ?? "";
  const startDate = model.value?.datumBeginInzagetermijn;

  return startDate && startDate > isoTomorrow ? startDate : isoTomorrow;
});
</script>

<style lang="scss" scoped>
.rechtsmiddel {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-small);
}
</style>
