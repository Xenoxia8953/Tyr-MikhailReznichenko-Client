using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using LeaveItThere.Helpers;
using LeaveItThere.Components;
using Newtonsoft.Json;
using UnityEngine;
using SPT.Common.Http;
using System;

namespace Tyr_MikhailReznichenko_Client
{
    [BepInDependency("Jehree.LeaveItThere", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Tyrian.MikhailReznichenko", "MikhailReznichenko", "1.0.0")]

    public class LayerSetter : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static List<string> GetItemIds;
        public const string configToClient = "/tyrian/mikhailreznichenko/config_to_client";

        public void Awake()
        {
            LogSource = BepInEx.Logging.Logger.CreateLogSource(" Mikhail Reznichenko ");
            LogSource.LogWarning("Jehree is smelly :3");
        }

        public void Start()
        {
            LogSource.LogWarning("Attempting to Initialise server route.");
            GetItemIds = ServerRouteHelper.ServerRoute<List<string>>(configToClient);
            LeaveItThereStaticEvents.OnFakeItemInitialized += OnFakeItemInitialized;
        }

        public void OnFakeItemInitialized(FakeItem fakeItem)
        {
            if (GetItemIds.Contains(fakeItem.LootItem.Item.TemplateId) == false) return;
            fakeItem.AddonFlags.IsPhysicalRegardlessOfSize = true;

            foreach (Transform child in fakeItem.transform)
            {
                Transform colliderTransform = child.Find("collider");
                Transform ballisticTransform = child.Find("ballistic");

                if (colliderTransform != null)
                {
                    colliderTransform.gameObject.layer = 18;
                }

                if (ballisticTransform != null)
                {
                    ballisticTransform.gameObject.layer = 12;
                }
            }
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
            if (data is string)
            {
                Dictionary<string, string> dataDict = new Dictionary<string, string>();
                dataDict.Add("data", (string)data);
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

