<template>
  <publicaties-overview
    :eigenaar-groep-mode="eigenaarGroepMode"
    :key="eigenaarGroepMode ? 'collegas' : 'mijn'"
  />
</template>

<script setup lang="ts">
import PublicatiesOverview from "@/features/publicatie/PublicatiesOverview.vue";

defineProps<{ eigenaarGroepMode?: boolean }>();
</script>
