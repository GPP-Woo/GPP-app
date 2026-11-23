import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { flushPromises } from "@vue/test-utils";
import { fetchAllPages, useAllPages } from "../use-all-pages";
import { LoggedOutError } from "@/api";
import toast, { messages } from "@/stores/toast";

const fetchMock = vi.fn();

const jsonResponse = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), {
    status,
    headers: { "content-type": "application/json" }
  });

describe("use-all-pages", () => {
  beforeEach(() => {
    fetchMock.mockReset();
    vi.stubGlobal("fetch", fetchMock);
    [...messages].forEach((m) => toast.remove(m));
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  describe("fetchAllPages", () => {
    it("returns an array response as is", async () => {
      fetchMock.mockResolvedValue(jsonResponse([1, 2]));

      expect(await fetchAllPages<number>("/api/items")).toEqual([1, 2]);
      expect(fetchMock).toHaveBeenCalledTimes(1);
    });

    it("follows next links and merges the pages", async () => {
      fetchMock
        .mockResolvedValueOnce(
          jsonResponse({ results: [1, 2], next: "https://host/api/items?page=2" })
        )
        .mockResolvedValueOnce(jsonResponse({ results: [3], next: null }));

      expect(await fetchAllPages<number>("/api/items")).toEqual([1, 2, 3]);
      expect(fetchMock.mock.calls[1][0]).toBe("/api/items?page=2");
    });

    it("throws LoggedOutError and shows a message on a 401", async () => {
      fetchMock.mockResolvedValue(new Response(null, { status: 401 }));

      await expect(fetchAllPages("/api/items")).rejects.toBeInstanceOf(LoggedOutError);
      expect(messages).toHaveLength(1);
    });

    it("rejects on other error statuses", async () => {
      fetchMock.mockResolvedValue(new Response(null, { status: 500 }));

      await expect(fetchAllPages("/api/items")).rejects.toMatchObject({ status: 500 });
    });
  });

  describe("useAllPages", () => {
    it("loads data and turns loading off", async () => {
      fetchMock.mockResolvedValue(jsonResponse([1, 2]));

      const { data, loading, error } = useAllPages<number>("/api/items");

      expect(loading.value).toBe(true);

      await flushPromises();

      expect(data.value).toEqual([1, 2]);
      expect(loading.value).toBe(false);
      expect(error.value).toBe(false);
    });

    it("sets error and clears data on failure", async () => {
      fetchMock.mockResolvedValue(new Response(null, { status: 500 }));

      const { data, loading, error } = useAllPages<number>("/api/items");

      await flushPromises();

      expect(error.value).toBe(true);
      expect(data.value).toEqual([]);
      expect(loading.value).toBe(false);
    });

    it("fetches nothing without a url", async () => {
      const { data, loading } = useAllPages<number>(null);

      await flushPromises();

      expect(fetchMock).not.toHaveBeenCalled();
      expect(data.value).toEqual([]);
      expect(loading.value).toBe(false);
    });

    it("refetch fetches again", async () => {
      fetchMock.mockResolvedValueOnce(jsonResponse([1])).mockResolvedValueOnce(jsonResponse([2]));

      const { data, refetch } = useAllPages<number>("/api/items");

      await flushPromises();
      await refetch();

      expect(data.value).toEqual([2]);
    });
  });
});
