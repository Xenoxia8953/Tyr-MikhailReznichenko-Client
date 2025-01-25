using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using LeaveItThere.Helpers;
using LeaveItThere.Components;
using Newtonsoft.Json;
using SPT.Common.Http;
using EFT.UI;

namespace Tyr_MikhailReznichenko_Client
{
    [BepInDependency("Jehree.LeaveItThere", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Tyrian.MikhailReznichenko", "MikhailReznichenko", "1.0.0")]

    public class LayerSetter : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static List<string> GetItemIds;
        public static List<string> GetExcludedItemIds;
        public const string itemidsToClient = "/tyrian/mikhailreznichenko/itemids_to_client";

        public void Awake()
        {
            LogSource = BepInEx.Logging.Logger.CreateLogSource(" Mikhail Reznichenko ");
            LogSource.LogWarning("Jehree is smelly :3");
        }

        public void Start()
        {
            GetItemIds = RouteHelper.ServerRoute<List<string>>(itemidsToClient);
            LeaveItThereStaticEvents.OnFakeItemInitialized += OnFakeItemInitialized;
        }
        public void OnFakeItemInitialized(FakeItem fakeItem)
        {
            if (GetItemIds.Contains(fakeItem.LootItem.Item.TemplateId) == false) return;
            fakeItem.AddonFlags.IsPhysicalRegardlessOfSize = true;
            fakeItem.AddonFlags.RemoveRootCollider = true;
        }
    }
    public class RouteHelper
    {
        public static T ServerRoute<T>(string url, object data = default)
        {
            string json = JsonConvert.SerializeObject(data);
            var req = RequestHandler.PostJson(url, json);
            return JsonConvert.DeserializeObject<T>(req);
        }
    }
}

