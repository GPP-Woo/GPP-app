namespace ODPC.Features.Formats
{
    public static class FormatsMock
    {
        public static readonly Dictionary<string, Format> Formats = new Format[]
        {
            new() { Name = "7z", MimeType = "application/x-7z-compressed", Extension = ".7z", Identifier = "a81129a3-ec70-40f3-8eb6-fc94e97ef865" },
            new() { Name = "7z Win v1", MimeType = "application/x-compressed", Extension = ".7z", Identifier = "a81129a3-ec70-40f3-8eb6-fc94e97ef865" },
            new() { Name = "7z Win v2", MimeType = "application/octet-stream", Extension = ".7z", Identifier = "a81129a3-ec70-40f3-8eb6-fc94e97ef865" },
            new() { Name = "CSV", MimeType = "text/csv", Identifier = "6bdd2631-b5d5-472a-b336-9cc643822f0a" },
            new() { Name = "Excel XLS", MimeType = "application/vnd.ms-excel", Identifier = "822928f7-73f0-45d3-b1a0-4f3a3dbc361a" },
            new() { Name = "Excel XLSX", MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Identifier = "fd16ded8-f013-4ca7-ba21-ee3ca36a6054" },
            new() { Name = "HTML", MimeType = "text/html", Identifier = "bf16a8af-f40b-4e36-96e3-0bdd5d6f5316" },
            new() { Name = "ODF", MimeType = "application/vnd.oasis.opendocument.formula", Identifier = "26a3df9c-290c-40a4-98cb-387842816698" },
            new() { Name = "ODP", MimeType = "application/vnd.oasis.opendocument.presentation", Identifier = "8b31aa20-d284-4540-aca4-222e1394c1d5" },
            new() { Name = "ODS", MimeType = "application/vnd.oasis.opendocument.spreadsheet", Identifier = "837abc18-616f-4e82-945b-fe78ab939860" },
            new() { Name = "ODT", MimeType = "application/vnd.oasis.opendocument.text", Identifier = "53b7d662-4413-4901-a3a4-6c129fcb93c1" },
            new() { Name = "PDF", MimeType = "application/pdf", Identifier = "a8836b30-8b25-4af6-9b35-e68e4f644c59" },
            new() { Name = "TXT", MimeType = "text/plain", Identifier = "549c6a31-6274-4b51-8528-fa9de141681e" },
            new() { Name = "PPSX", MimeType = "application/vnd.openxmlformats-officedocument.presentationml.slideshow", Identifier = "ff7cf4ad-372f-4996-b6af-5bf8c5b178d8" },
            new() { Name = "PPT", MimeType = "application/vnd.ms-powerpoint", Identifier = "580e3592-c5b2-40d0-ab7a-0c626c8e171a" },
            new() { Name = "PPTX", MimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation", Identifier = "fc188a40-f0bf-415f-a339-fd7520b531f8" },
            new() { Name = "PPS", MimeType = "application/vnd.ms-powerpoint.slideshow.macroEnabled.12", Identifier = "65298a3a-346d-4614-9fee-028f532ed8bc" },
            new() { Name = "RTF", MimeType = "application/rtf", Identifier = "63026476-5d40-424e-a113-b02ed7fba760" },
            new() { Name = "DOC", MimeType = "application/msword", Identifier = "26ccc5e3-acf2-4251-9618-46321e2b9d36" },
            new() { Name = "DOCX", MimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Identifier = "ae0ea877-3207-4a97-b5df-bf552bc9b895" },
            new() { Name = "ZIP", MimeType = "application/zip", Extension = ".zip", Identifier = "f879f55e-a9c2-4779-96b2-288d6359d86b" },
            new() { Name = "ZIP Win v1", MimeType = "application/zip-compressed", Extension = ".zip", Identifier = "f879f55e-a9c2-4779-96b2-288d6359d86b" },
            new() { Name = "ZIP Win v2", MimeType = "application/x-zip-compressed", Extension = ".zip", Identifier = "f879f55e-a9c2-4779-96b2-288d6359d86b" },
            new() { Name = "ZIP Win v3", MimeType = "application/octet-stream", Extension = ".zip", Identifier = "f879f55e-a9c2-4779-96b2-288d6359d86b" }
        }.ToDictionary(x => x.Name);
    }
}
