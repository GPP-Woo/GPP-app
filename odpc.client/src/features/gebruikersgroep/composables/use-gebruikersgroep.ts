import { ref, onMounted, watch } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import toast from "@/stores/toast";
import type { Gebruikersgroep } from "../types";

const API_URL = `/api/v1`;

export const useGebruikersgroep = (uuid?: string) => {
  const gebruikersgroep = ref<Gebruikersgroep>({
    naam: "",
    omschrijving: "",
    gekoppeldeWaardelijsten: [],
    gekoppeldeGebruikers: []
  });

  const {
    get: getGebruikersgroep,
    post: postGebruikersgroep,
    put: putGebruikersgroep,
    delete: deleteGebruikersgroep,
    data: gebruikersgroepData,
    isFetching: loadingGebruikersgroep,
    error: gebruikersgroepError
  } = useFetchApi(() => `${API_URL}/gebruikersgroepen${uuid ? "/" + uuid : ""}`, {
    immediate: false
  }).json<Gebruikersgroep>();

  watch(gebruikersgroepData, (value) => (gebruikersgroep.value = value || gebruikersgroep.value));

  const submitGebruikersgroep = async () => {
    if (uuid) {
      await putGebruikersgroep(gebruikersgroep).execute();
    } else {
      await postGebruikersgroep(gebruikersgroep).execute();
    }

    if (gebruikersgroepError.value) {
      toast.add({
        text: "De gebruikersgroep kon niet worden opgeslagen, probeer het nogmaals...",
        type: "error"
      });

      gebruikersgroepError.value = null;

      throw new Error();
    }
  };

  const removeGebruikersgroep = async () => {
    await deleteGebruikersgroep().text().execute();

    if (gebruikersgroepError.value) {
      toast.add({
        text: "De gebruikersgroep kon niet worden verwijderd, probeer het nogmaals...",
        type: "error"
      });

      gebruikersgroepError.value = null;

      throw new Error();
    }
  };

  onMounted(async () => uuid && (await getGebruikersgroep().execute()));

  return {
    gebruikersgroep,
    loadingGebruikersgroep,
    gebruikersgroepError,
    submitGebruikersgroep,
    removeGebruikersgroep
  };
};
