import { createFetch } from "@vueuse/core";
import { handleFetchError } from "./error";

export const useFetchApi = createFetch({
  options: {
    beforeFetch({ options }) {
      options.headers = {
        ...options.headers,
        "content-type": "application/json",
        "is-api": "true"
      };

      return { options };
    },
    afterFetch(ctx) {
      return ctx;
    },
    onFetchError(ctx) {
      handleFetchError(ctx.response?.status);

      return ctx;
    }
  }
});
