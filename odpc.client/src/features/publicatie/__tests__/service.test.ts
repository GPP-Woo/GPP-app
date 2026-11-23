import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { ref } from "vue";
import { sendDocument, ApiError } from "../service";
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
