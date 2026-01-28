import { readonly } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import type { MijnGebruikersgroep } from "../types";

const API_URL = `/api`;

export const useMijnGebruikersgroepen = () => {
  const { data, isFetching, error } = useFetchApi(() => `${API_URL}/mijn-gebruikersgroepen`).json<
    MijnGebruikersgroep[]
  >();

  return {
    data: readonly(data),
    isFetching: readonly(isFetching),
    error: readonly(error)
  };
};
