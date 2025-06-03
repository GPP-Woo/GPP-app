<script setup lang="ts">
import { RouterView } from "vue-router";
import ToastSection from "./components/ToastSection.vue";
import TheHeader from "./components/TheHeader.vue";
import TheFooter from "./components/TheFooter.vue";
import SimpleSpinner from "@/components/SimpleSpinner.vue";
import AlertInline from "@/components/AlertInline.vue";
import { useAppData } from "./composables/use-app-data";

const { loading, error } = useAppData();
</script>

<template>
  <toast-section />

  <Suspense>
    <the-header />
  </Suspense>

  <main>
    <simple-spinner v-if="loading"></simple-spinner>

    <alert-inline v-if="error"
      >Er is iets misgegaan. Neem contact op met de beheerder.</alert-inline
    >

    <router-view v-else />
  </main>

  <the-footer />
</template>
