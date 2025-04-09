import { createFetch } from "@vueuse/core";

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
      // console.log(ctx);
      return ctx;
    }
  }
});
