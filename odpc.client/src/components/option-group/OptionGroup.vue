<template>
  <details
    role="group"
    ref="groupRef"
    :aria-labelledby="`label-${instanceId}`"
    :aria-required="required ? true : undefined"
    @change="setCustomValidity"
    :open="open"
  >
    <summary :id="`label-${instanceId}`">{{ title }} {{ required ? "*" : "" }}</summary>

    <p v-if="required" class="error" :id="`description-${instanceId}`">{{ getMessage(type) }}</p>

    <div v-if="type === `checkbox`" class="input-option check-all">
      <label
        ><input
          type="checkbox"
          @click="toggleAll"
          :checked="allSelected"
          :aria-describedby="`description-${instanceId}`"
          :aria-invalid="!model.length ? true : undefined"
        />
        selecteer alles
      </label>
    </div>

    <div class="input-option" v-for="{ uuid, naam } in options" :key="uuid">
      <label
        ><input
          :type="type"
          :value="uuid"
          v-model="model"
          :aria-describedby="`description-${instanceId}`"
          :aria-invalid="!model.length ? true : undefined"
        />{{ naam }}</label
      >
    </div>
  </details>
</template>

<script setup lang="ts">
import { computed, getCurrentInstance, nextTick, useModel, watch } from "vue";
import { useOptionGroup } from "./use-option-group";
import type { OptionProps } from "./types";

const instanceId = getCurrentInstance()?.uid;

const { groupRef, setCustomValidity, getMessage } = useOptionGroup();

const props = withDefaults(
  defineProps<{
    type?: string;
    title: string;
    options: Readonly<OptionProps[]>;
    modelValue: string | string[];
    required?: boolean;
    open?: boolean;
  }>(),
  {
    type: "checkbox"
  }
);

const model = useModel(props, "modelValue");

const uuids = computed(() => props.options.map((option) => option.uuid));

const allSelected = computed(() => uuids.value.every((uuid) => model.value.includes(uuid)));

const toggleAll = () => {
  model.value =
    Array.isArray(model.value) && allSelected.value
      ? model.value.filter((uuid) => !uuids.value.includes(uuid))
      : [...new Set([...model.value, ...uuids.value])];
};

watch(
  () => props.required,
  () => nextTick(() => setCustomValidity())
);
</script>

<style lang="scss" scoped>
.check-all {
  padding-block-end: var(--spacing-small);
  margin-block-end: var(--spacing-small);
  border-bottom: 1px solid var(--color-grey);
}

details {
  &:has(:user-invalid) {
    border-color: var(--code);

    .error {
      color: var(--code);
    }
  }
}

:disabled {
  .error,
  .check-all {
    display: none;
  }
}
</style>
