using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ODPC.Apis.Odrc;
using ODPC.Features.Documenten;
using ODPC.Features.Documenten.InitialiseerDocument;
using ODPC.Features.Documenten.UploadBestandsdeel;

namespace ODPC.Test
{
    [TestClass]
    public class DocumentUploadenTest
    {
        private static readonly Guid s_docUuid = Guid.NewGuid();
        private static readonly Guid s_partUuid = Guid.NewGuid();
        private static readonly string s_partPath = $"/api/v2/documenten/{s_docUuid}/bestandsdelen/{s_partUuid}";

        private static PublicatieDocumentUpsert CreateDocument() => new()
        {
            Publicatie = Guid.NewGuid(),
            OfficieleTitel = "Document",
            Bestandsnaam = "a.pdf",
            Bestandsformaat = "application/pdf",
            Bestandsomvang = 10
        };

        [TestMethod]
        public async Task Initialiseert_document_in_publicatiebank()
        {
            var bank = new StubPublicatiebankHandler().Respond(HttpMethod.Post, "/api/v2/documenten", HttpStatusCode.Created,
                new PublicatieDocument { Uuid = s_docUuid, OfficieleTitel = "Document", Bestandsnaam = "a.pdf", Bestandsformaat = "application/pdf", Bestandsomvang = 10 });

            var result = await new InitialiseerDocumentController(bank.ClientFactory).Post("v2", CreateDocument(), default);

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(s_docUuid, ((PublicatieDocument)ok.Value!).Uuid);
        }

        [TestMethod]
        public async Task Geeft_fout_door_als_initialiseren_door_publicatiebank_faalt()
        {
            var bank = new StubPublicatiebankHandler().Respond(HttpMethod.Post, "/api/v2/documenten", HttpStatusCode.InternalServerError);

            await Assert.ThrowsExactlyAsync<HttpRequestException>(() =>
                new InitialiseerDocumentController(bank.ClientFactory).Post("v2", CreateDocument(), default));
        }

        [TestMethod]
        public async Task Stuurt_bestandsdeel_door_naar_publicatiebank()
        {
            var bank = new StubPublicatiebankHandler().Respond(HttpMethod.Put, s_partPath, HttpStatusCode.NoContent);
            using var client = await CreateUploadClientAsync(bank);

            using var response = await client.PutAsync(s_partPath, new StringContent("inhoud van het bestandsdeel"));

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            var forwarded = bank.Requests.Single();
            Assert.AreEqual(HttpMethod.Put, forwarded.Method);
            Assert.AreEqual(s_partPath, forwarded.PathAndQuery);
            Assert.AreEqual("inhoud van het bestandsdeel", forwarded.Body);
        }

        [TestMethod]
        public async Task Meldt_fout_als_publicatiebank_het_bestandsdeel_weigert()
        {
            var bank = new StubPublicatiebankHandler().Respond(HttpMethod.Put, s_partPath, HttpStatusCode.InternalServerError);
            using var client = await CreateUploadClientAsync(bank);

            var ex = await Assert.ThrowsExactlyAsync<HttpRequestException>(() => client.PutAsync(s_partPath, new StringContent("x")));

            Assert.AreEqual(HttpStatusCode.InternalServerError, ex.StatusCode);
        }

        private static async Task<HttpClient> CreateUploadClientAsync(StubPublicatiebankHandler bank)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddSingleton(bank.ClientFactory);

            var app = builder.Build();
            UploadBestandsdeelEndpoint.Map(app);
            await app.StartAsync();

            return app.GetTestClient();
        }
    }
}
