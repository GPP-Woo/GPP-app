import { ref, watch, type MaybeRefOrGetter, toRef } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import { useAllPages } from "@/composables/use-all-pages";
import toast from "@/stores/toast";
import type { InzageProcedure } from "../types";

export const useInzageProcedure = (uuid: MaybeRefOrGetter<string | undefined>) => {
  const pubUuid = toRef(uuid);

  const inzageProcedure = ref<InzageProcedure | null>(null);

  const {
    data,
    loading: loadingInzageProcedure,
    error: inzageProcedureError
  } = useAllPages<InzageProcedure>(() =>
    pubUuid.value ? `/api/v2/inzageprocedure?publicatie=${pubUuid.value}` : null
  );

  watch(data, (value) => (inzageProcedure.value = value?.[0] ?? null));

  const {
    post: postInzageProcedure,
    put: putInzageProcedure,
    data: submitData,
    isFetching: submittingInzageProcedure,
    error: submitError
  } = useFetchApi(
    () =>
      `/api/v2/inzageprocedure${inzageProcedure.value?.uuid ? "/" + inzageProcedure.value.uuid : ""}`,
    { immediate: false }
  ).json<InzageProcedure>();

  const submitInzageProcedure = async () => {
    if (!inzageProcedure.value || !pubUuid.value) return;

    if (inzageProcedure.value.uuid) {
      await putInzageProcedure(inzageProcedure).execute();
    } else {
      inzageProcedure.value = { ...inzageProcedure.value, publicatie: pubUuid.value };

      await postInzageProcedure(inzageProcedure).execute();
    }

    if (submitError.value) {
      toast.add({
        text: "De Inzage-procedure kon niet worden opgeslagen, probeer het nogmaals...",
        type: "error"
      });

      submitError.value = null;

      throw new Error(`submitInzageProcedure`);
    }

    inzageProcedure.value = submitData.value;
  };

  return {
    inzageProcedure,
    loadingInzageProcedure,
    inzageProcedureError,
    submittingInzageProcedure,
    submitInzageProcedure
  };
};
