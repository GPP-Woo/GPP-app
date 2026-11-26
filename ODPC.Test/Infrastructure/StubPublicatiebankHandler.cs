using System.Net;
using System.Net.Http.Json;
using ODPC.Apis.Odrc;

namespace ODPC.Test.Infrastructure
{
    public record RecordedRequest(HttpMethod Method, string PathAndQuery, string? Body);

    /// <summary>Stand-in for the publicatiebank: answers requests with a canned response and records what was sent.</summary>
    public class StubPublicatiebankHandler : HttpMessageHandler
    {
        private readonly List<RecordedRequest> _requests = [];
        private readonly Dictionary<(HttpMethod, string), Func<HttpResponseMessage>> _responses = [];

        public IReadOnlyList<RecordedRequest> Requests => _requests;

        public StubPublicatiebankHandler Respond(HttpMethod method, string path, HttpStatusCode status, object? body = null)
        {
            _responses[(method, path)] = () => body == null
                ? new HttpResponseMessage(status)
                : new HttpResponseMessage(status) { Content = JsonContent.Create(body) };

            return this;
        }

        public IOdrcClientFactory ClientFactory => new StubOdrcClientFactory(this);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            _requests.Add(new RecordedRequest(request.Method, request.RequestUri!.PathAndQuery, body));

            return _responses.TryGetValue((request.Method, request.RequestUri.AbsolutePath), out var response)
                ? response()
                : new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        private sealed class StubOdrcClientFactory(StubPublicatiebankHandler handler) : IOdrcClientFactory
        {
            public HttpClient Create(string? handeling) =>
                new(handler, disposeHandler: false) { BaseAddress = new Uri("http://publicatiebank.test") };
        }
    }
}
