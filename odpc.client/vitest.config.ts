import { fileURLToPath } from "node:url";
import { mergeConfig, defineConfig, configDefaults } from "vitest/config";
import viteConfig from "./vite.config.ts";

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: "jsdom",
      exclude: [...configDefaults.exclude, "e2e/**"],
      root: fileURLToPath(new URL("./", import.meta.url)),
      coverage: {
        provider: "v8",
        reporter: ["text-summary", "json-summary", "lcov"],
        include: ["src/**/*.{ts,vue}"]
      }
    }
  })
);
