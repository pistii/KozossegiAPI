using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;

namespace KozoskodoAPI.Realtime.Helpers
{
    public static class Extension
    {
        public static HttpContext gethttpcontext(this HubCallerContext context) =>
           context?.Features
            .Select(x => x.Value as IHttpContextFeature)
            .FirstOrDefault(x => x != null)?.HttpContext;

        static public T GetQueryParameterValue<T>(this IQueryCollection httpquery, string queryparametername) =>
           httpquery.TryGetValue(queryparametername, out var value) && value.Any()
             ? (T)Convert.ChangeType(value.FirstOrDefault(), typeof(T))
             : default;
    }
}
