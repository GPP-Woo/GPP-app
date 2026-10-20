import { ref, toValue, type MaybeRefOrGetter } from "vue";
import { handleFetchError, type PagedResult } from "@/api";

const fetchPage = <T>(url: string, signal?: AbortSignal | undefined) =>
  fetch(url, { headers: { "is-api": "true" }, signal })
    .then((r) => (r.ok ? r : (handleFetchError(r.status), Promise.reject(r))))
    .then((r) => r.json() as Promise<PagedResult<T> | T[]>)
    .then((r) => (Array.isArray(r) ? { results: r, next: undefined } : r));

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

export const useAllPages = <T>(url: MaybeRefOrGetter<string | null>) => {
  const data = ref<T[]>([]);
  const loading = ref(true);
  const error = ref(false);

  let abortController: AbortController | undefined;

  const refetch = async () => {
    abortController?.abort();

    const controller = (abortController = new AbortController());
    const currentUrl = toValue(url);

    loading.value = true;
    error.value = false;

    try {
      const result = currentUrl ? await fetchAllPages<T>(currentUrl, controller.signal) : [];

      if (!controller.signal.aborted) data.value = result;
    } catch {
      if (controller.signal.aborted) return;

      error.value = true;
      data.value = [];
    }

    if (!controller.signal.aborted) loading.value = false;
  };

  refetch();

  return {
    data,
    loading,
    error,
    refetch
  };
};
