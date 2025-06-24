<template>
  <div v-if="!isReadonly" class="form-group form-group-button">
    <label :for="`item-${itemsId}`">{{ itemNameSingular }} toevoegen</label>

    <input
      :id="`item-${itemsId}`"
      type="text"
      v-model.trim="item"
      :aria-invalid="!item"
      @keydown.enter.prevent="addItem"
    />

    <button type="button" :disabled="!item" :aria-disabled="!item" @click="addItem">
      Toevoegen
    </button>
  </div>

  <details ref="detailsRef" aria-live="polite">
    <summary>Toegevoegde {{ itemNamePlural }}</summary>

    <p v-if="!items.length">Er zijn (nog) geen {{ itemNamePlural }} toegevoegd.</p>

    <ul v-else class="reset">
      <li v-for="(linkedItem, index) in items" :key="`${itemsId}-${index}`">
        <button
          type="button"
          :title="`${linkedItem} verwijderen`"
          :aria-label="`${linkedItem} verwijderen`"
          :class="['button secondary', { 'icon-after xmark': !isReadonly }]"
          :disabled="isReadonly"
          :aria-disabled="isReadonly"
          @click="() => removeItem(index)"
        >
          {{ linkedItem }}
        </button>
      </li>
    </ul>
  </details>
</template>

<script setup lang="ts">
import { ref, useId, useModel } from "vue";
import toast from "@/stores/toast";

const props = defineProps<{
  modelValue: string[];
  itemNameSingular: string;
  itemNamePlural: string;
  isReadonly?: boolean;
}>();

const items = useModel(props, "modelValue");
const item = ref("");

const detailsRef = ref<HTMLDetailsElement>();

const itemsId = useId();

const addItem = () => {
  if (items.value.includes(item.value)) {
    toast.add({
      text: "Dit item is al toegevoegd.",
      type: "error"
    });

    return;
  }

  if (detailsRef.value) detailsRef.value.open = true;

  items.value = [...items.value, item.value.trim()];
  item.value = "";
};

const removeItem = (index: number) => (items.value = items.value.filter((_, i) => i !== index));
</script>

<style lang="scss" scoped>
ul {
  display: flex;
  flex-wrap: wrap;
  column-gap: var(--spacing-small);
}

label::first-letter {
  text-transform: uppercase;
}
</style>
