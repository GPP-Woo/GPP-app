<template>
  <footer>
    <p>GPP-Woo</p>

    <dl v-if="versionInfo">
      <div v-if="versionInfo.semanticVersion">
        <dd>Versie:</dd>
        <dt>{{ versionInfo.semanticVersion }}</dt>
      </div>

      <div v-if="versionInfo.gitSha">
        <dd>Commit:</dd>
        <dt>{{ versionInfo.gitSha.substring(0, 7) }}</dt>
      </div>
    </dl>
  </footer>
</template>

<script lang="ts" setup>
import { useFetchApi } from "@/api";

const { data: versionInfo } = useFetchApi(() => "/api/environment/version").json<{
  semanticVersion?: string;
  gitSha?: string;
}>();
</script>

<style lang="scss" scoped>
p {
  margin-block: 0 1rem;
}

dl {
  display: flex;
  justify-content: center;
  margin-block: 0;
  column-gap: 1rem;
  font-size: 0.75rem;

  div {
    display: flex;
  }

  dd {
    margin-inline-start: 0;
    margin-inline-end: 1ch;
  }
}
</style>
