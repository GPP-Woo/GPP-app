import { ref, onMounted, watch } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import toast from "@/stores/toast";
import type { Publicatie } from "../types";

const API_URL = `/api/v1`;

export const usePublicatie = (uuid?: string) => {
  const publicatie = ref<Publicatie>({
    publisher: "",
    verantwoordelijke: "",
    officieleTitel: "",
    verkorteTitel: "",
    omschrijving: "",
    publicatiestatus: "gepubliceerd",
    informatieCategorieen: []
  });

  const {
    get: getPublicatie,
    post: postPublicatie,
    put: putPublicatie,
    data: publicatieData,
    isFetching: loadingPublicatie,
    error: publicatieError
  } = useFetchApi(() => `${API_URL}/publicaties${uuid ? "/" + uuid : ""}`, {
    immediate: false
  }).json<Publicatie>();

  watch(publicatieData, (value) => (publicatie.value = value || publicatie.value));

  const submitPublicatie = async () => {
    // Fill required verantwoordelijke with publisher value and add to publicatie
    publicatie.value = {
      ...publicatie.value,
      ...{
        verantwoordelijke: publicatie.value.publisher
        // opsteller: publicatie.value.publisher
      }
    };

    if (uuid) {
      await putPublicatie(publicatie).execute();
    } else {
      await postPublicatie(publicatie).execute();
    }

    if (publicatieError.value) {
      toast.add({
        text: "De publicatie kon niet worden opgeslagen. Probeer het nogmaals of neem contact op met uw beheerder.",
        type: "error"
      });

      publicatieError.value = null;

      throw new Error();
    }
  };

  onMounted(async () => uuid && (await getPublicatie().execute()));

  return {
    publicatie,
    loadingPublicatie,
    publicatieError,
    submitPublicatie
  };
};
