using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using LeaveItThere.Helpers;
using LeaveItThere.Components;
using Newtonsoft.Json;
using SPT.Common.Http;

namespace Tyr_MikhailReznichenko_Client
{
    [BepInDependency("Jehree.LeaveItThere", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Tyrian.MikhailReznichenko", "MikhailReznichenko", "1.0.0")]

    public class LayerSetter : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static List<string> GetItemIds;
        public static List<string> GetExcludedItemIds;
        public const string configToClient = "/tyrian/mikhailreznichenko/config_to_client";

        public void Awake()
        {
            LogSource = BepInEx.Logging.Logger.CreateLogSource(" Mikhail Reznichenko ");
            LogSource.LogWarning("Jehree is smelly :3");
        }

        public void Start()
        {
            GetItemIds = ServerRouteHelper.ServerRoute<List<string>>(configToClient);
            LeaveItThereStaticEvents.OnFakeItemInitialized += OnFakeItemInitialized;
        }
        public void OnFakeItemInitialized(FakeItem fakeItem)
        {
            if (GetItemIds.Contains(fakeItem.LootItem.Item.TemplateId) == false) return;
            fakeItem.AddonFlags.IsPhysicalRegardlessOfSize = true;
            fakeItem.AddonFlags.RemoveRootCollider = true;
        }
    }
    public class ServerRouteHelper
    {
        public static T ServerRoute<T>(string url, object data = default)
        {
            string json = JsonConvert.SerializeObject(data);
            var req = RequestHandler.PostJson(url, json);
            return JsonConvert.DeserializeObject<T>(req);
        }

        public static string ServerRoute(string url, object data = default)
        {
            string json;
            if (data is string v)
            {
                Dictionary<string, string> dataDict = new()
                {
                    { "data", v }
                };
                json = JsonConvert.SerializeObject(dataDict);
            }
            else
            {
                json = JsonConvert.SerializeObject(data);
            }
            return RequestHandler.PutJson(url, json);
        }
    }
}

