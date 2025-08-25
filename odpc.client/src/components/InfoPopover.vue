<template>
  <slot name="trigger" :trigger-props="triggerProps">
    <button type="button" v-bind="triggerProps">?</button>
  </slot>

  <div ref="tooltipRef" popover role="tooltip" :id="tooltipId" class="info-popover" @click.prevent>
    <slot></slot>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, useId } from "vue";
import { onKeyStroke } from "@vueuse/core";

const tooltipId = useId();

const tooltipRef = ref<HTMLElement>();

const triggerProps = computed(() => ({
  popovertarget: tooltipId,
  "aria-describedby": tooltipId
}));

onKeyStroke("Tab", () => tooltipRef.value?.hidePopover());
</script>

<style lang="scss" scoped>
.info-popover {
  position-area: block-start inline-end;
  position-try: block-start, inline-end, block-end, inline-start;
  inline-size: max-content;
  max-inline-size: min(90vw, 32rem);
  padding: 1rem;
  margin: 0.5rem;
  border: 1px solid var(--border);
  box-shadow:
    0px 4px 6px rgba(0, 0, 0, 0.1),
    0px 6px 12px rgba(0, 0, 0, 0.06);
}
</style>
