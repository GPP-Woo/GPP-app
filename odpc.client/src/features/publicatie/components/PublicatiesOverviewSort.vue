<template>
  <div class="form-group" role="group" aria-labelledby="sorteer-label">
    <label id="sorteer-label" for="sorteer" aria-label="Sorteer publicaties">Sorteer op</label>

    <select name="sorteer" id="sorteer" v-model="queryParams.sorteer">
      <option v-for="(value, key) in sortingOptions" :key="key" :value="key">
        {{ value }}
      </option>
    </select>
  </div>
</template>

<script setup lang="ts">
const queryParams = defineModel<{ sorteer: string }>("queryParams", { required: true });

const sortingOptions = {
  officiele_titel: "Title (a-z)",
  "-officiele_titel": "Title (z-a)",
  verkorte_titel: "Verkorte title (a-z)",
  "-verkorte_titel": "Verkorte title (z-a)",
  registratiedatum: "Registratiedatum (oud-nieuw)",
  "-registratiedatum": "Registratiedatum (nieuw-oud)"
};
</script>

<style lang="scss" scoped>
.form-group {
  flex-direction: row;
  align-items: center;

  label {
    margin-block: 0;
    margin-inline-end: var(--spacing-default);
  }
}
</style>
