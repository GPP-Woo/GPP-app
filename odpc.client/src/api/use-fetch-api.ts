import { createFetch } from "@vueuse/core";
import { handleFetchError } from "./api-error";

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
      // console.log(ctx);
      return ctx;
    },
    onFetchError(ctx) {
      handleFetchError(ctx.response?.status);

      return ctx;
    }
  }
});
