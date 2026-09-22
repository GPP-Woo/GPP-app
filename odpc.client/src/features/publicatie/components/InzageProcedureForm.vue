<template>
  <section>
    <button
      v-if="canLink && !model"
      type="button"
      class="button secondary icon-after pen"
      @click="linkInzageProcedure"
    >
      Inzage-procedure koppelen
    </button>

    <template v-if="model">
      <div v-if="model.uuid && !isReadonly" class="form-group">
        <label
          ><input type="checkbox" v-model="pendingDelete" /> Inzage-procedure verwijderen</label
        >

        <span v-show="model.pendingAction" class="alert"
          >Let op: deze actie kan niet ongedaan worden gemaakt.</span
        >
      </div>

      <date-input
        v-model="model.datumBeginInzagetermijn"
        id="datumBeginInzagetermijn"
        label="Begindatum inzagetermijn"
        :min-date="ISOToday"
        :max-date="model.datumEindeInzagetermijn || undefined"
        required
      />

      <date-input
        v-model="model.datumEindeInzagetermijn"
        id="datumEindeInzagetermijn"
        label="Einddatum inzagetermijn"
        :min-date="minDatumEindeInzagetermijn"
        required
      />

      <div class="form-group">
        <label :for="model.beschikbaarRechtsmiddel || `zienswijze`"
          >Beschikbaar rechtsmiddel *</label
        >

        <label
          ><input
            id="zienswijze"
            type="radio"
            name="beschikbaarRechtsmiddel"
            v-model="model.beschikbaarRechtsmiddel"
            :value="BeschikbaarRechtsmiddel.zienswijze"
            required
            :aria-invalid="!model.beschikbaarRechtsmiddel"
            aria-describedby="beschikbaarRechtsmiddelError"
          />
          Zienswijze</label
        >

        <label
          ><input
            id="bezwaar"
            type="radio"
            name="beschikbaarRechtsmiddel"
            v-model="model.beschikbaarRechtsmiddel"
            :value="BeschikbaarRechtsmiddel.bezwaar"
            required
            :aria-invalid="!model.beschikbaarRechtsmiddel"
            aria-describedby="beschikbaarRechtsmiddelError"
          />
          Bezwaar</label
        >

        <span id="beschikbaarRechtsmiddelError" class="error"
          >Beschikbaar rechtsmiddel is een verplicht veld</span
        >
      </div>

      <div v-if="model.urlReactieformulier" class="form-group">
        <label for="urlReactieformulier">URL reactieformulier</label>

        <input id="urlReactieformulier" type="url" :value="model.urlReactieformulier" disabled />
      </div>

      <div class="form-group">
        <label for="toelichting">Toelichting *</label>

        <textarea
          id="toelichting"
          v-model.trim="model.toelichting"
          rows="4"
          required
          :aria-invalid="!model.toelichting"
          aria-describedby="toelichtingError"
        ></textarea>

        <span id="toelichtingError" class="error">Toelichting is een verplicht veld</span>
      </div>

      <div class="form-group">
        <label for="urlBekendmaking">URL bekendmaking</label>

        <input id="urlBekendmaking" type="text" v-model.trim="model.urlBekendmaking" />
      </div>

      <div class="form-group">
        <label
          ><input type="checkbox" v-model="model.automatischIntrekken" /> Automatisch intrekken na
          afloop inzagetermijn</label
        >
      </div>

      <button
        v-if="!model.uuid && !isReadonly"
        type="button"
        class="button secondary icon-after trash"
        @click="removeInzageProcedure"
      >
        Inzage-procedure verwijderen
      </button>
    </template>

    <prompt-modal :dialog="dialog" confirm-message="Ja, verwijderen" cancel-message="Nee, behouden">
      <p>Weet u zeker dat u deze inzage-procedure wilt verwijderen?</p>
    </prompt-modal>
  </section>
</template>

<script setup lang="ts">
import { computed, useModel } from "vue";
import { useConfirmDialog } from "@vueuse/core";
import DateInput from "@/components/DateInput.vue";
import PromptModal from "@/components/PromptModal.vue";
import { ISOToday, ISOTomorrow } from "@/helpers/date";
import { BeschikbaarRechtsmiddel, type InzageProcedure } from "../types";

const props = defineProps<{
  modelValue: InzageProcedure | null;
  canLink: boolean;
  isReadonly: boolean;
}>();

const model = useModel(props, "modelValue");

const dialog = useConfirmDialog();

const linkInzageProcedure = () =>
  (model.value = {
    publicatie: "",
    toelichting: "",
    beschikbaarRechtsmiddel: "",
    datumBeginInzagetermijn: ISOToday ?? "",
    datumEindeInzagetermijn: "",
    automatischIntrekken: false
  });

const pendingDelete = computed({
  get: () => model.value?.pendingAction === "delete",
  set: (checked) => {
    if (model.value) model.value.pendingAction = checked ? "delete" : null;
  }
});

const removeInzageProcedure = async () => {
  if (!(await dialog.reveal()).isCanceled) model.value = null;
};

const minDatumEindeInzagetermijn = computed(() => {
  const isoTomorrow = ISOTomorrow ?? "";
  const startDate = model.value?.datumBeginInzagetermijn;

  return startDate && startDate > isoTomorrow ? startDate : isoTomorrow;
});
</script>
