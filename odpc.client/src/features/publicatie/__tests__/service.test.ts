import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { ref } from "vue";
import { sendDocument, uploadFile, ApiError } from "../service";
import { LoggedOutError } from "@/api";
import toast, { messages } from "@/stores/toast";

// service.ts fetches the mime types on import
vi.mock("@/api/use-fetch-api", () => ({
  useFetchApi: () => ({ json: () => ({ data: ref([]) }) })
}));

const fetchMock = vi.fn();

const jsonResponse = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), {
    status,
    headers: { "content-type": "application/json" }
  });

describe("sendDocument", () => {
  beforeEach(() => {
    fetchMock.mockReset();
    vi.stubGlobal("fetch", fetchMock);
    [...messages].forEach((m) => toast.remove(m));
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("sends the body as json and returns the response", async () => {
    fetchMock.mockResolvedValue(jsonResponse({ uuid: "abc" }, 201));

    const result = await sendDocument<{ uuid: string }>("POST", "/api/documenten", { naam: "x" });

    expect(result).toEqual({ uuid: "abc" });
    expect(fetchMock).toHaveBeenCalledWith("/api/documenten", {
      method: "POST",
      headers: { "content-type": "application/json", "is-api": "true" },
      body: JSON.stringify({ naam: "x" })
    });
  });

  it("expects no response body on DELETE", async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 204 }));

    expect(await sendDocument("DELETE", "/api/documenten/1")).toBeUndefined();
  });

  it("throws an ApiError without status on a network error", async () => {
    const original = new TypeError("Failed to fetch");
    fetchMock.mockRejectedValue(original);

    await expect(sendDocument("POST", "/api/documenten", {})).rejects.toMatchObject({
      kind: "metadata",
      status: undefined,
      original
    });
  });

  it("throws an ApiError with status on an error response", async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 500 }));

    const error = await sendDocument("PUT", "/api/documenten/1", {}).catch((e) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect(error.status).toBe(500);
  });

  it("throws LoggedOutError and shows a message on a 401", async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 401 }));

    await expect(sendDocument("POST", "/api/documenten", {})).rejects.toBeInstanceOf(
      LoggedOutError
    );
    expect(messages).toHaveLength(1);
  });
});

type Outcome = { status: number } | "error";

// Minimal XMLHttpRequest: each send() consumes the next scripted outcome
class StubXhr {
  static instances: StubXhr[] = [];
  static outcomes: Outcome[] = [];

  method = "";
  url = "";
  headers: Record<string, string> = {};
  body?: FormData;
  status = 0;
  upload: { onprogress?: (e: ProgressEvent) => void } = {};
  onload?: () => void;
  onerror?: (e: unknown) => void;
  onabort?: (e: unknown) => void;

  constructor() {
    StubXhr.instances.push(this);
  }

  open(method: string, url: string) {
    this.method = method;
    this.url = url;
  }

  setRequestHeader(name: string, value: string) {
    this.headers[name] = value;
  }

  send(body: FormData) {
    this.body = body;

    const outcome = StubXhr.outcomes.shift() ?? { status: 200 };

    if (outcome === "error") {
      this.onerror?.(new Event("error"));
      return;
    }

    this.upload.onprogress?.({ lengthComputable: true, loaded: 50, total: 100 } as ProgressEvent);
    this.status = outcome.status;
    this.onload?.();
  }
}

describe("uploadFile", () => {
  const file = new File(["abcdefghij"], "test.txt");

  // deliberately out of order: volgnummer decides the sequence
  const delen = [
    { url: "https://host/api/v2/delen/2?x=1", volgnummer: 2, omvang: 6 },
    { url: "https://host/api/v2/delen/1", volgnummer: 1, omvang: 4 }
  ];

  const blobText = (xhr: StubXhr) =>
    new Promise<string>((resolve) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.readAsText(xhr.body!.get("inhoud") as Blob);
    });

  beforeEach(() => {
    StubXhr.instances = [];
    StubXhr.outcomes = [];
    vi.stubGlobal("XMLHttpRequest", StubXhr);
    [...messages].forEach((m) => toast.remove(m));
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("uploads the parts in order, each with its slice of the file", async () => {
    await uploadFile(file, delen);

    expect(StubXhr.instances.map((x) => x.method)).toEqual(["PUT", "PUT"]);
    expect(StubXhr.instances.map((x) => x.url)).toEqual(["/api/v2/delen/1", "/api/v2/delen/2?x=1"]);
    expect(StubXhr.instances[0].headers).toEqual({ "is-api": "true" });
    expect(await blobText(StubXhr.instances[0])).toBe("abcd");
    expect(await blobText(StubXhr.instances[1])).toBe("efghij");
  });

  it("reports progress as a percentage of the total size", async () => {
    const onProgress = vi.fn();

    await uploadFile(file, delen, onProgress);

    // 0, mid-part (2 of 4 bytes), end of part 1, mid-part (4 + 3 of 10 bytes), done
    expect(onProgress.mock.calls.map(([p]) => p)).toEqual([0, 20, 40, 70, 100]);
  });

  it("stops at the first part that fails", async () => {
    StubXhr.outcomes = [{ status: 500 }];

    const error = await uploadFile(file, delen).catch((e) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({ kind: "upload", status: 500 });
    expect(StubXhr.instances).toHaveLength(1);
  });

  it("throws an ApiError without status on a network error", async () => {
    StubXhr.outcomes = ["error"];

    await expect(uploadFile(file, delen)).rejects.toMatchObject({
      kind: "upload",
      status: undefined
    });
  });

  it("throws LoggedOutError and shows a message on a 401", async () => {
    StubXhr.outcomes = [{ status: 401 }];

    await expect(uploadFile(file, delen)).rejects.toBeInstanceOf(LoggedOutError);
    expect(messages).toHaveLength(1);
  });
});
