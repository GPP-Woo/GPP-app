<template>
  <header>
    <h1>{{ title }}</h1>

    <nav v-if="user?.isLoggedIn">
      <ul>
        <li>
          <router-link :to="{ name: 'mijn-publicaties' }">Mijn publicaties</router-link>
        </li>

        <li>
          <router-link :to="{ name: 'collega-publicaties' }"
            >Publicaties van collega's</router-link
          >
        </li>

        <li v-if="user?.isAdmin">
          <router-link :to="{ name: 'gebruikersgroepen' }">Gebruikersgroepen</router-link>
        </li>

        <li>
          <a href="/api/logoff" title="Uitloggen" class="icon-after exit">Uitloggen</a>
        </li>
      </ul>
    </nav>
  </header>
</template>

<script lang="ts" setup>
import { useAppData } from "@/composables/use-app-data";

const title = import.meta.env.VITE_APP_TITLE;

const { user } = useAppData();
</script>

<style lang="scss" scoped></style>
