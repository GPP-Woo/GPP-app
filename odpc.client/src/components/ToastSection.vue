<template>
  <transition-group name="toast" tag="section">
    <output
      v-for="(message, key) in messages"
      :key="key"
      role="status"
      :class="message.type"
      @click="toast.remove(message)"
      ><div v-html="message.text"></div
    ></output>
  </transition-group>
</template>

<script lang="ts" setup>
import toast, { messages } from "@/stores/toast";
</script>

<style scoped lang="scss">
section {
  position: fixed;
  z-index: 2;
  display: grid;
  justify-items: center;
  justify-content: center;
  gap: var(--spacing-default);
  pointer-events: none;
  inset-block-start: 0;
  inset-inline: 0;
  // ...
  padding: 0;
  padding-block-start: 11rem;
  margin: 0;
  border: none;
}

output {
  display: grid;
  grid: auto-flow / auto 1fr auto;
  column-gap: var(--spacing-default);
  width: var(--section-width);
  padding: 0.5rem var(--spacing-default);
  border: 1px solid var(--color-grey);
  pointer-events: auto;

  &::before,
  &::after {
    display: block;
    width: 1.25rem;
    height: 1.25rem;
  }
}

.confirm {
  background-color: var(--color-success);

  &::before {
    content: url("@/assets/icons/check.svg");
  }
}

.error {
  cursor: pointer;
  background-color: var(--color-error);

  &::before {
    content: url("@/assets/icons/triangle-exclamation.svg");
  }

  &::after {
    content: url("@/assets/icons/circle-xmark.svg");
  }
}

.toast-move,
.toast-enter-active,
.toast-leave-active {
  transition: all 0.5s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
}

.toast-leave-active {
  position: absolute;
}
</style>
