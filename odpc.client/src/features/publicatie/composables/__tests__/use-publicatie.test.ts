import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { defineComponent } from "vue";
import { flushPromises, mount } from "@vue/test-utils";
import { usePublicatie } from "../use-publicatie";
import toast, { messages } from "@/stores/toast";
import { PublicatieStatus } from "../../types";

const fetchMock = vi.fn();

const jsonResponse = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), {
    status,
    headers: { "content-type": "application/json" }
  });

// onMounted needs a component instance
const mountComposable = (uuid?: string) => {
  let result!: ReturnType<typeof usePublicatie>;

  mount(
    defineComponent({
      setup() {
        result = usePublicatie(uuid);
        return () => null;
      }
    })
  );

  return result;
};

const lastCall = () => {
  const [request, init] = fetchMock.mock.calls.at(-1)!;
  const url = typeof request === "string" ? request : request.url;
  const method = typeof request === "string" ? init?.method : request.method;
  const body = typeof request === "string" ? init?.body : undefined;

  return { url, method, body: body ? JSON.parse(body) : undefined };
};

describe("usePublicatie", () => {
  beforeEach(() => {
    fetchMock.mockReset();
    vi.stubGlobal("fetch", fetchMock);
    [...messages].forEach((m) => toast.remove(m));
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("starts as an empty concept without fetching when no uuid is given", () => {
    const { publicatie } = mountComposable();

    expect(fetchMock).not.toHaveBeenCalled();
    expect(publicatie.value).toMatchObject({
      officieleTitel: "",
      publicatiestatus: PublicatieStatus.concept,
      eigenaarGroep: null
    });
  });

  it("loads an existing publicatie on mount", async () => {
    fetchMock.mockResolvedValue(jsonResponse({ uuid: "pub-1", officieleTitel: "Bestaand" }));

    const { publicatie } = mountComposable("pub-1");
    await flushPromises();

    expect(lastCall()).toMatchObject({ url: "/api/v2/publicaties/pub-1", method: "GET" });
    expect(publicatie.value.officieleTitel).toBe("Bestaand");
  });

  it("registers a new publicatie and uses the publisher as verantwoordelijke", async () => {
    fetchMock.mockResolvedValue(jsonResponse({ uuid: "new-1", officieleTitel: "Nieuw" }));

    const { publicatie, submitPublicatie, pubUuid } = mountComposable();
    publicatie.value.publisher = "org-1";
    publicatie.value.officieleTitel = "Nieuw";

    await submitPublicatie();

    expect(lastCall()).toMatchObject({
      url: "/api/v2/publicaties",
      method: "POST",
      body: { publisher: "org-1", verantwoordelijke: "org-1", officieleTitel: "Nieuw" }
    });
    expect(pubUuid.value).toBe("new-1");
  });

  it("updates an existing publicatie, e.g. to publish it", async () => {
    fetchMock
      .mockResolvedValueOnce(jsonResponse({ uuid: "pub-1", publicatiestatus: "concept" }))
      .mockResolvedValueOnce(jsonResponse({ uuid: "pub-1", publicatiestatus: "gepubliceerd" }));

    const { publicatie, submitPublicatie } = mountComposable("pub-1");
    await flushPromises();

    publicatie.value.publicatiestatus = PublicatieStatus.gepubliceerd;
    await submitPublicatie();

    expect(lastCall()).toMatchObject({
      url: "/api/v2/publicaties/pub-1",
      method: "PUT",
      body: { publicatiestatus: "gepubliceerd" }
    });
  });

  it("shows an error toast and throws when saving fails", async () => {
    fetchMock.mockResolvedValue(new Response("boom", { status: 500 }));

    const { submitPublicatie, error } = mountComposable();

    await expect(submitPublicatie()).rejects.toThrow("submitPublicatie");

    expect(messages).toHaveLength(1);
    expect(messages[0]).toMatchObject({ type: "error" });
    expect(messages[0]!.text).toContain("kon niet worden opgeslagen");
    expect(error.value).toBeNull();
  });

  it("deletes a publicatie and reports a failed delete", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse({ uuid: "pub-1" })).mockResolvedValueOnce(
      new Response(null, { status: 204 })
    );

    const { deletePublicatie } = mountComposable("pub-1");
    await flushPromises();
    await deletePublicatie();

    expect(lastCall()).toMatchObject({ url: "/api/v2/publicaties/pub-1", method: "DELETE" });

    fetchMock.mockResolvedValueOnce(new Response("boom", { status: 500 }));

    await expect(deletePublicatie()).rejects.toThrow("removePublicatie");
    expect(messages.at(-1)!.text).toContain("kon niet worden verwijderd");
  });
});
