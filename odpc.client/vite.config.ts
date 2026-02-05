import { fileURLToPath, URL } from "node:url";

import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import { env } from "process";

const proxyCalls = ["/api", "/signin-oidc", "/signout-callback-oidc", "/healthz"];

const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
    ? env.ASPNETCORE_URLS.split(";")[0]
    : "http://localhost:62230";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url))
    }
  },
  server: {
    proxy: Object.fromEntries(
      proxyCalls.map((key) => [
        key,
        {
          target,
          secure: false
        }
      ])
    )
  },
  build: {
    assetsInlineLimit: 0
  }
});
