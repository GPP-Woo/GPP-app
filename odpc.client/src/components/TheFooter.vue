<template>
  <footer>
    <p>GPP-Woo</p>

    <dl v-if="versionInfo">
      <div v-if="versionInfo.semanticVersion">
        <dt>Versie:</dt>
        <dd>{{ versionInfo.semanticVersion }}</dd>
      </div>

      <div v-if="versionInfo.gitSha">
        <dt>Commit:</dt>
        <dd>{{ versionInfo.gitSha.substring(0, 7) }}</dd>
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
  margin-block: 0;
}

dl {
  display: flex;
  justify-content: center;
  column-gap: 1rem;
  font-size: 0.75rem;

  div {
    display: flex;
  }

  dt {
    margin-inline: 0 1ch;
  }

  dd {
    margin-inline: 0;
  }
}
</style>
