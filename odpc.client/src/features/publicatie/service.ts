import { ref } from "vue";
import { useFetchApi } from "@/api/use-fetch-api";
import type { Bestandsdeel, MimeType } from "./types";

const mimeTypesMap = ref<Map<string, MimeType> | null>(null);

(async () => {
  const { data } = await useFetchApi(() => "/api/v1/formats").json<MimeType[]>();

  mimeTypesMap.value = new Map(data.value?.map((type) => [type.mimeType, type]));
})();

const uploadFile = async (file: File, bestandsdelen: Bestandsdeel[]) => {
  let blobStart = 0;

  bestandsdelen.sort((a, b) => a.volgnummer - b.volgnummer);

  try {
    for (const { url, omvang } of bestandsdelen) {
      const { pathname, search } = new URL(url);

      const body = new FormData();
      const blob = file.slice(blobStart, blobStart + omvang);

      body.append("inhoud", blob);

      const { ok } = await fetch(pathname + search, {
        method: "PUT",
        body,
        headers: { "is-api": "true" }
      });

      if (!ok) {
        throw new Error(`Error uploadDocument: ${url}`);
      }

      blobStart += omvang;
    }
  } catch {
    throw new Error();
  }
};

export { mimeTypesMap, uploadFile };
