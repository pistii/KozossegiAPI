using System.Net.Http.Headers;

namespace KozoskodoAPI.Repo
{
    public class BaseAPIRepository
    {
        protected HttpClient client;
        protected DelegatingHandler? _handler;
        protected string _baseUrl;
        protected string _path;

        public BaseAPIRepository(string baseUrl = null, string path = null, DelegatingHandler? handler = null)
        {
            _baseUrl = baseUrl ?? "http://localhost:5000/";
            _path = path;
            _handler = handler;

            client = handler == null ? new HttpClient() : new HttpClient(handler) { BaseAddress = new Uri(_baseUrl) };
            if (!string.IsNullOrEmpty(_baseUrl))
            {
                client.BaseAddress = new Uri(_baseUrl);
            }
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
