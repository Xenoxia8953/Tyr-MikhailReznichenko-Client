using BepInEx.Logging;
using Newtonsoft.Json;
using SPT.Common.Http;

namespace Helpers.RouteHelper;
public class RouteHelper
{
    public static T ServerRoute<T>(string url, T data = default)
    {
        string json = JsonConvert.SerializeObject(data);
        string req = RequestHandler.PostJson(url, json);
        return JsonConvert.DeserializeObject<T>(req);
    }
}