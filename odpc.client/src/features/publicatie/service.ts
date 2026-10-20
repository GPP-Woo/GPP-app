import { useFetchApi } from "@/api/use-fetch-api";
import { handleFetchError } from "@/api/error";
import type { Bestandsdeel, MimeType } from "./types";

const { data: mimeTypes } = useFetchApi(() => "/api/formats").json<MimeType[]>();

export class ApiError extends Error {
  constructor(
    public readonly kind: "metadata" | "upload",
    public readonly status?: number,
    public readonly original?: unknown
  ) {
    super(`${kind} error${status ? ` (${status})` : ""}`);
  }
}

const sendDocument = async <T = void>(
  method: "POST" | "PUT" | "DELETE",
  url: string,
  body?: unknown
) => {
  let response: Response;

  try {
    response = await fetch(url, {
      method,
      headers: { "content-type": "application/json", "is-api": "true" },
      body: body ? JSON.stringify(body) : undefined
    });
  } catch (err) {
    throw new ApiError("metadata", undefined, err);
  }

  if (!response.ok) {
    handleFetchError(response.status);

    throw new ApiError("metadata", response.status);
  }

  return (method === "DELETE" ? undefined : await response.json()) as T;
};

const putBestandsdeel = (url: string, blob: Blob, onProgress: (loadedBytes: number) => void) =>
  new Promise<void>((resolve, reject) => {
    const body = new FormData();
    body.append("inhoud", blob);

    const xhr = new XMLHttpRequest();

    xhr.open("PUT", url);
    xhr.setRequestHeader("is-api", "true");

    // loaded/total include the multipart boundaries and headers: scale to the blob size
    xhr.upload.onprogress = (e) => {
      if (e.lengthComputable && e.total > 0) onProgress((e.loaded / e.total) * blob.size);
    };

    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        resolve();
        return;
      }

      try {
        handleFetchError(xhr.status);
        reject(new ApiError("upload", xhr.status));
      } catch (err) {
        reject(err);
      }
    };

    xhr.onerror = (e) => reject(new ApiError("upload", undefined, e));
    xhr.onabort = (e) => reject(new ApiError("upload", undefined, e));

    xhr.send(body);
  });

const uploadFile = async (
  file: File,
  bestandsdelen: Bestandsdeel[],
  onProgress?: (percentage: number) => void
) => {
  const sorted = [...bestandsdelen].sort((a, b) => a.volgnummer - b.volgnummer);
  const totalBytes = sorted.reduce((sum, { omvang }) => sum + omvang, 0);

  let blobStart = 0;

  const report = (uploadedBytes: number) =>
    onProgress?.(totalBytes ? Math.min(100, Math.floor((uploadedBytes / totalBytes) * 100)) : 0);

  report(0);

  for (const { url, omvang } of sorted) {
    const { pathname, search } = new URL(url);

    const blob = file.slice(blobStart, blobStart + omvang);

    await putBestandsdeel(pathname + search, blob, (loaded) => report(blobStart + loaded));

    blobStart += omvang;

    report(blobStart);
  }
};

export { mimeTypes, sendDocument, uploadFile };
