<template>
  <div
    class="dropzone"
    @dragover.prevent="dragging = true"
    @dragleave.prevent="dragging = false"
    @drop.prevent="onFilesSelected"
  >
    <input
      type="file"
      id="files"
      multiple
      title="Bestanden toevoegen"
      :accept="accept"
      @change="onFilesSelected"
    />

    <label for="files">
      <span v-if="dragging">Plaats bestanden hier</span>

      <span v-else>Klik hier om bestanden te selecteren of sleep bestanden hierheen</span>
    </label>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { mimeTypes } from "../service";

const emit = defineEmits<{ (e: "filesSelected", payload: Event | DragEvent): void }>();

const dragging = ref(false);

const accept = computed(
  () => mimeTypes.value?.map((t) => t.extension || t.mimeType).join(",") ?? ""
);

const onFilesSelected = (event: Event | DragEvent) => {
  emit("filesSelected", event);

  dragging.value = false;
};
</script>

<style lang="scss" scoped>
.dropzone {
  position: relative;
  height: 8rem;
  margin-block-end: var(--spacing-default);
}

input,
label {
  position: absolute;
  z-index: 0;
  top: 0;
  width: 100%;
  height: 100%;
}

label {
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: var(--spacing-default) var(--spacing-extralarge);
  background-color: var(--accent-bg);
  border: 3px dashed var(--border);
  border-radius: var(--radius-default);
  transition: 0.2s ease all;

  &:hover {
    border-style: solid;
    background-color: var(--bg);
  }
}

span {
  text-decoration: underline;
}
</style>
