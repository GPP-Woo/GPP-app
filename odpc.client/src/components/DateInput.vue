<template>
  <div class="form-group">
    <label :for="id">{{ label }} <template v-if="required">*</template></label>

    <input
      :id="id"
      type="date"
      v-model="dateComputed"
      :max="maxDate"
      :required="required"
      :aria-required="required || undefined"
      :aria-invalid="required && !dateComputed"
      :aria-describedby="`${id}-error`"
      :disabled="disabled"
      :aria-disabled="disabled"
    />

    <span :id="`${id}-error`" class="error">Vul een geldige datum in.</span>
  </div>
</template>

<script setup lang="ts">
import { computed, useModel } from "vue";
import { getTimezoneOffsetString } from "@/helpers";

const DEFAULT_TIME = "12:00:00";

const props = defineProps<{
  modelValue?: string | null;
  id: string;
  label: string;
  maxDate?: string;
  toDateTime?: boolean; // if true, converts date to datetime string with DEFAULT_TIME and timezone offset
  required?: boolean;
  disabled?: boolean;
}>();

const model = useModel(props, "modelValue");

const formatDateTime = (date: string) =>
  props.toDateTime ? `${date}T${DEFAULT_TIME}${getTimezoneOffsetString(date)}` : date;

const dateComputed = computed({
  get: () => model.value?.split("T")[0],
  set: (date) => (model.value = date ? formatDateTime(date) : null)
});
</script>
