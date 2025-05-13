<template>
  <div class="form-group">
    <label for="fromDate">Datum van</label>

    <input type="date" id="fromDate" v-model="fromDate" />
  </div>

  <div class="form-group">
    <label for="untilDate">Datum tot en met</label>

    <input type="date" id="untilDate" v-model="untilDate" />
  </div>
</template>

<script setup lang="ts">
import { useModel } from "vue";

const props = defineProps<{ fromDate: string; untilDate: string }>();

const fromDate = useModel(props, "fromDate");
const untilDate = useModel(props, "untilDate");
</script>

<style lang="scss" scoped>
input,
.form-group {
  margin-block-end: 0;
}
</style>
