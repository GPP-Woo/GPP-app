import { ref, watch, type MaybeRefOrGetter, toRef } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import { useAllPages } from "@/composables/use-all-pages";
import toast from "@/stores/toast";
import type { InzageProcedure } from "../types";

const HTTP_UNPROCESSABLE_ENTITY = 422;

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
    delete: deleteInzageProcedure,
    data: submitData,
    isFetching: submittingInzageProcedure,
    error: submitError,
    statusCode
  } = useFetchApi(
    () =>
      `/api/v2/inzageprocedure${inzageProcedure.value?.uuid ? "/" + inzageProcedure.value.uuid : ""}`,
    { immediate: false }
  ).json<InzageProcedure>();

  const submitInzageProcedure = async () => {
    if (!inzageProcedure.value || !pubUuid.value) return;

    if (inzageProcedure.value.pendingAction === "delete") {
      // Delete
      if (inzageProcedure.value.uuid) await deleteInzageProcedure().text().execute();
    } else if (inzageProcedure.value.uuid) {
      // Update
      await putInzageProcedure(inzageProcedure).execute();
    } else {
      // Create
      inzageProcedure.value = { ...inzageProcedure.value, publicatie: pubUuid.value };

      await postInzageProcedure(inzageProcedure).execute();
    }

    if (submitError.value) {
      toast.add({
        text:
          statusCode.value === HTTP_UNPROCESSABLE_ENTITY
            ? `De begindatum van de inzage-procedure ligt vóór de publicatiedatum. Pas de begindatum aan en sla de publicatie opnieuw op.`
            : `De Inzage-procedure kon niet worden ${inzageProcedure.value.pendingAction === "delete" ? "verwijderd" : "opgeslagen"}, probeer het nogmaals...`,
        type: "error"
      });

      submitError.value = null;

      throw new Error(`submitInzageProcedure`);
    }

    inzageProcedure.value =
      inzageProcedure.value.pendingAction === "delete" ? null : submitData.value;
  };

  return {
    inzageProcedure,
    loadingInzageProcedure,
    inzageProcedureError,
    submittingInzageProcedure,
    submitInzageProcedure
  };
};
