import { ref } from "vue";
import { asyncComputed } from "@vueuse/core";
import type { PagedResult } from "@/api";

const fetchPage = <T>(url: string, signal?: AbortSignal | undefined) =>
  fetch(url, { headers: { "is-api": "true" }, signal })
    .then((r) => (r.ok ? r : Promise.reject(r)))
    .then((r) => r.json() as Promise<PagedResult<T>>);

export const fetchAllPages = async <T>(
  url: string,
  signal?: AbortSignal | undefined
): Promise<T[]> => {
  const { results, next } = await fetchPage<T>(url, signal);
  if (next) {
    const { pathname, search } = new URL(next);

    return [...results, ...(await fetchAllPages<T>(pathname + search, signal))];
  }

  return results;
};

export const useAllPages = <T>(url: string) => {
  const isFetching = ref(true);
  const error = ref(false);

  const data = asyncComputed(
    async (onCancel) => {
      const abortController = new AbortController();

      onCancel(() => abortController.abort());

      return await fetchAllPages<T>(url, abortController.signal).catch((e: Response) => {
        console.log(e.status);

        error.value = true;

        return [] as T[];
      });
    },
    [],
    isFetching
  );

  return {
    data,
    isFetching,
    error
  };
};
