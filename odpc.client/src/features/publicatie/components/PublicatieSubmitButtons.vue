<template>
  <template v-if="!isReadonly">
    <li v-if="canDelete">
      <button
        type="button"
        title="Publicatie verwijderen"
        class="button danger"
        value="delete"
        @click="$emit(`onRemove`)"
      >
        Publicatie verwijderen
      </button>
    </li>

    <li v-if="canDraft">
      <button
        type="submit"
        title="Opslaan als concept"
        class="button secondary"
        value="draft"
        @click="$emit(`onSetValidationMode`, $event)"
      >
        Opslaan als concept
      </button>
    </li>

    <li v-if="canRetract">
      <button
        type="submit"
        title="Publicatie intrekken"
        class="button danger"
        value="retract"
        @click="$emit(`onSetValidationMode`, $event)"
      >
        Publicatie intrekken
      </button>
    </li>

    <li>
      <button
        type="submit"
        title="Publiceren"
        value="publish"
        @click="$emit(`onSetValidationMode`, $event)"
      >
        Publiceren
      </button>
    </li>
  </template>

  <!-- claim action -->
  <li v-else-if="canClaim">
    <button type="submit" title="Publicatie claimen" value="claim">Publicatie claimen</button>
  </li>
</template>

<script setup lang="ts">
defineProps<{
  isReadonly: boolean;
  canDraft: boolean;
  canDelete: boolean;
  canRetract: boolean;
  canClaim: boolean;
}>();

defineEmits<{
  onSetValidationMode: [e: Event];
  onRemove: [];
}>();
</script>
