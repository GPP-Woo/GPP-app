<template>
  <header>
    <h1>{{ title }}</h1>

    <nav v-if="user?.isLoggedIn">
      <ul>
        <li>
          <router-link :to="{ name: 'publicaties' }">Publicaties</router-link>
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
import { ref } from "vue";
import getUser, { type User } from "@/stores/user";

const title = import.meta.env.VITE_APP_TITLE;
const user = ref<User | null>(await getUser());
</script>

<style lang="scss" scoped></style>
