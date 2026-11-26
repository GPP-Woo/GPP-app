import { describe, it, expect, vi, beforeEach } from "vitest";
import { nextTick, ref } from "vue";
import { useDocumenten, DocumentenSubmitError } from "../use-documenten";
import { ApiError, sendDocument, uploadFile } from "../../service";
import { PublicatieStatus, UploadStatus, type PublicatieDocument } from "../../types";

const serverDocs = ref<PublicatieDocument[]>([]);

vi.mock("@/composables/use-all-pages", () => ({
  useAllPages: () => ({
    data: serverDocs,
    loading: ref(false),
    error: ref(false),
    refetch: vi.fn()
  })
}));

vi.mock("../../service", () => ({
  ApiError: class extends Error {
    constructor(
      public kind: string,
      public status?: number
    ) {
      super(kind);
    }
  },
  sendDocument: vi.fn(),
  uploadFile: vi.fn()
}));

const bestandsdelen = [{ url: "http://x/delen/1", volgnummer: 1, omvang: 10 }];

const createDoc = (overrides: Partial<PublicatieDocument> = {}): PublicatieDocument => ({
  publicatie: "pub-1",
  officieleTitel: "Document",
  publicatiestatus: PublicatieStatus.gepubliceerd,
  creatiedatum: "2026-01-01",
  bestandsnaam: "a.pdf",
  bestandsformaat: "application/pdf",
  bestandsomvang: 10,
  kenmerken: [],
  ...overrides
});

const setup = async (docs: PublicatieDocument[]) => {
  serverDocs.value = [];
  const result = useDocumenten("pub-1");
  serverDocs.value = docs;
  await nextTick();
  return result;
};

describe("useDocumenten", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.spyOn(console, "error").mockImplementation(() => {});
  });

  it("registers a new document and uploads its file", async () => {
    vi.mocked(sendDocument).mockResolvedValue({ uuid: "doc-1", bestandsdelen });
    vi.mocked(uploadFile).mockImplementation(async (_f, _d, onProgress) => onProgress?.(50));

    const file = new File(["0123456789"], "a.pdf");
    const { submitDocumenten, documenten } = await setup([createDoc({ _client: { file } })]);

    await submitDocumenten();

    expect(sendDocument).toHaveBeenCalledWith(
      "POST",
      "/api/v2/documenten",
      expect.objectContaining({ publicatie: "pub-1", officieleTitel: "Document" })
    );
    expect(vi.mocked(sendDocument).mock.calls[0]![2]).not.toHaveProperty("_client");
    expect(uploadFile).toHaveBeenCalledWith(file, bestandsdelen, expect.any(Function));
    expect(documenten.value[0]!._client!.upload).toEqual({
      progress: 100,
      status: UploadStatus.done
    });
  });

  it("deletes and updates existing documents", async () => {
    vi.mocked(sendDocument).mockResolvedValue(undefined);

    const { submitDocumenten } = await setup([
      createDoc({ uuid: "del", pendingAction: "delete" }),
      createDoc({ uuid: "upd" })
    ]);

    await submitDocumenten();

    expect(sendDocument).toHaveBeenCalledWith("DELETE", "/api/v2/documenten/del");
    expect(sendDocument).toHaveBeenCalledWith(
      "PUT",
      "/api/v2/documenten/upd",
      expect.objectContaining({ publicatiestatus: PublicatieStatus.gepubliceerd })
    );
  });

  it("marks a document as failed when the upload fails and still processes the others", async () => {
    vi.mocked(sendDocument).mockResolvedValue({ uuid: "doc-1", bestandsdelen });
    vi.mocked(uploadFile).mockRejectedValue(new ApiError("upload", 500));

    const { submitDocumenten, documenten, documentenSubmitError, submittingDocumenten } =
      await setup([
        createDoc({ _client: { file: new File(["x"], "a.pdf") } }),
        createDoc({ uuid: "ok" })
      ]);

    await expect(submitDocumenten()).rejects.toBeInstanceOf(DocumentenSubmitError);

    expect(documenten.value[0]!._client).toMatchObject({
      error: "Het uploaden is mislukt",
      upload: { status: UploadStatus.failed }
    });
    expect(sendDocument).toHaveBeenCalledWith("PUT", "/api/v2/documenten/ok", expect.anything());
    expect(documentenSubmitError.value).toBe(true);
    expect(submittingDocumenten.value).toBe(false);
  });

  it("reports a metadata error when saving the document fails", async () => {
    vi.mocked(sendDocument).mockRejectedValue(new ApiError("metadata", 400));

    const { submitDocumenten, documenten } = await setup([createDoc({ uuid: "upd" })]);

    await expect(submitDocumenten()).rejects.toBeInstanceOf(DocumentenSubmitError);
    expect(documenten.value[0]!._client!.error).toBe("De metadata kon niet worden opgeslagen");
  });
});
