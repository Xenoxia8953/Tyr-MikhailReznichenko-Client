using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using LeaveItThere.Helpers;
using LeaveItThere.Components;
using Newtonsoft.Json;
using UnityEngine;
using SPT.Common.Http;
using static Val;
using System;

namespace MikhailReznichenko
{
    [BepInDependency("Jehree.LeaveItThere", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Tyrian.MikhailReznichenko", "MikhailReznichenko", "1.0.0")]

    public class LayerSetter : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static ServerConfig serverConfig;
        public const string configToClient = "/tyrian/mikhail_reznichenko/config_to_client";

        public void Awake()
        {
            LogSource = BepInEx.Logging.Logger.CreateLogSource(" Mikhail Reznichenko ");
            LogSource.LogDebug("Logger initialized!");
        }

        public void Start()
        {
            LogSource.LogDebug("Attempting to Initialise server route.");
            try
            {
                serverConfig = ServerRouteHelper.ServerRoute<ServerConfig>(configToClient);
                LogSource.LogInfo("Fetched server configuration.");

                // Check if ItemIds is null or empty
                if (serverConfig.ItemIds == null)
                {
                    LogSource.LogWarning("ServerConfig.ItemIds is null.");
                    return;
                }

                if (serverConfig.ItemIds.Count == 0)
                {
                    LogSource.LogWarning("ServerConfig.ItemIds is empty.");
                    return;
                }

                // Log the item IDs
                LogSource.LogDebug($"Item IDs: {string.Join(", ", serverConfig.ItemIds)}");
            }
            catch (Exception ex)
            {
                LogSource.LogError($"Error attempting server route.");
                LogSource.LogError($"Error during Start: {ex.Message}\n{ex.StackTrace}");
            }
            LogSource.LogDebug("Attempting to Initialise subscription to fake item event.");
            LeaveItThereStaticEvents.OnFakeItemInitialized += OnFakeItemInitialized;
        }

        public void OnFakeItemInitialized(FakeItem fakeItem)
        {
            LogSource.LogDebug("Running fake item event.");
            if (serverConfig.ItemIds.Contains(fakeItem.LootItem.Item.TemplateId) == false) return;
            LogSource.LogDebug("Found one of your funky items!");
            fakeItem.AddonFlags.IsPhysicalRegardlessOfSize = true;

            foreach (Transform child in fakeItem.transform)
            {
                Transform colliderTransform = child.Find("collider");
                Transform ballisticTransform = child.Find("ballistic");

                if (colliderTransform != null)
                {
                    LogSource.LogDebug("collider found!");
                    colliderTransform.gameObject.layer = 18;
                }

                if (ballisticTransform != null)
                {
                    LogSource.LogDebug("ballistic found!");
                    ballisticTransform.gameObject.layer = 12;
                }
            }
        }

        public struct ServerConfig
        {
            public List<string> ItemIds;
        }
    }

    public class ServerRouteHelper
    {
        public static T ServerRoute<T>(string url, object data = default(object))
        {
            string json = JsonConvert.SerializeObject(data);
            var req = RequestHandler.PostJson(url, json);
            return JsonConvert.DeserializeObject<T>(req);
        }

        public static string ServerRoute(string url, object data = default(object))
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

