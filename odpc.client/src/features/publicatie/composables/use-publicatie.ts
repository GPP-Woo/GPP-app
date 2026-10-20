import { ref, computed, onMounted, watch } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import toast from "@/stores/toast";
import { PublicatieStatus, type Publicatie } from "../types";

const API_URL = `/api/v2`;

export const usePublicatie = (uuid?: string) => {
  const publicatie = ref<Publicatie>({
    publisher: null,
    verantwoordelijke: null,
    officieleTitel: "",
    verkorteTitel: "",
    omschrijving: "",
    publicatiestatus: PublicatieStatus.concept,
    informatieCategorieen: [],
    onderwerpen: [],
    kenmerken: [],
    eigenaarGroep: null
  });

  const pubUuid = computed(() => uuid ?? publicatie.value.uuid);

  const {
    data,
    isFetching,
    error,
    get,
    post,
    put,
    delete: deleteMethod
  } = useFetchApi(() => `${API_URL}/publicaties${pubUuid.value ? "/" + pubUuid.value : ""}`, {
    immediate: false
  }).json<Publicatie>();

  watch(data, (value) => (publicatie.value = value ?? publicatie.value));

  const submitPublicatie = async () => {
    // Fill required verantwoordelijke with publisher value and add to publicatie
    publicatie.value = {
      ...publicatie.value,
      ...{
        verantwoordelijke: publicatie.value.publisher
      }
    };

    if (pubUuid.value) {
      await put(publicatie).execute();
    } else {
      await post(publicatie).execute();
    }

    if (error.value) {
      toast.add({
        text: "De publicatie kon niet worden opgeslagen. Probeer het nogmaals of neem contact op met de beheerder.",
        type: "error"
      });

      error.value = null;

      throw new Error(`submitPublicatie`);
    }
  };

  const deletePublicatie = async () => {
    await deleteMethod().text().execute();

    if (error.value) {
      toast.add({
        text: "De publicatie kon niet worden verwijderd. Probeer het nogmaals of neem contact op met de beheerder.",
        type: "error"
      });

      error.value = null;

      throw new Error(`removePublicatie`);
    }
  };

  onMounted(() => uuid && get().execute());

  const refetchPublicatie = () => get().execute();

  return {
    publicatie,
    isFetching,
    error,
    submitPublicatie,
    deletePublicatie,
    refetchPublicatie,
    pubUuid
  };
};
