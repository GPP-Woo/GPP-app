<template>
  <div class="form-group">
    <label :for="id">{{ label }}</label>

    <input
      :id="id"
      type="date"
      v-model="dateTimeComputed"
      :aria-describedby="`${id}-error`"
      :max="maxDate"
      :disabled="disabled"
      :aria-disabled="disabled"
    />

    <span :id="`${id}-error`" class="error">Vul een geldige datum in.</span>
  </div>
</template>

<script setup lang="ts">
import { computed, useModel } from "vue";
import { getTimezoneOffsetString } from "@/helpers/date";

const DEFAULT_TIME = "12:00:00";

const props = defineProps<{
  modelValue?: string | null;
  id: string;
  label: string;
  maxDate: string;
  disabled?: boolean;
}>();

const model = useModel(props, "modelValue");

const dateTimeComputed = computed({
  get: () => model.value?.split("T")[0],
  set: (date) => (model.value = date ? `${date}T${DEFAULT_TIME}${getTimezoneOffsetString()}` : null)
});
</script>
