<template>
  <fieldset>
    <legend>Zoek op</legend>

    <div class="form-group">
      <label for="zoeken">Titel</label>

      <input type="text" id="zoeken" v-model="searchString" />
    </div>

    <date-range-picker v-model:from-date="fromDate" v-model:until-date="untilDateInclusive" />

    <div class="form-group-submit">
      <button type="submit" class="icon-after loupe" aria-label="Zoek">Zoek</button>
    </div>
  </fieldset>
</template>

<script setup lang="ts">
import DateRangePicker from "@/components/DateRangePicker.vue";

const searchString = defineModel<string>("searchString", { required: true });
const fromDate = defineModel<string>("fromDate", { required: true });
const untilDateInclusive = defineModel<string>("untilDateInclusive", { required: true });
</script>

<style lang="scss" scoped>
fieldset {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  gap: var(--spacing-default);

  .form-group {
    flex-grow: 1;
  }
}
</style>
