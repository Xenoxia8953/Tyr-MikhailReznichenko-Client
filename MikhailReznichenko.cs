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

namespace Tyr_MikhailReznichenko_Client
{
    [BepInDependency("Jehree.LeaveItThere", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Tyrian.MikhailReznichenko", "MikhailReznichenko", "1.0.0")]

    public class LayerSetter : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static ServerConfig serverConfig;
        public const string configToClient = "/tyrian/mikhailreznichenko/config_to_client";

        public void Awake()
        {
            LogSource = BepInEx.Logging.Logger.CreateLogSource(" Mikhail Reznichenko ");
            LogSource.LogWarning("Logger initialized!");
        }

        public void Start()
        {
            LogSource.LogWarning("Attempting to Initialise server route.");
            try
            {
                serverConfig = ServerRouteHelper.ServerRoute<ServerConfig>(configToClient);
                LogSource.LogWarning($"serverConfig is of type: {serverConfig.GetType()}");
                LogSource.LogWarning("Fetched server configuration.");

                // Check if ItemIds is null or empty
                if (serverConfig.ItemIds == null)
                {
                    LogSource.LogWarning($"serverConfig is of type: {serverConfig.ItemIds.GetType()}");
                    LogSource.LogWarning("ServerConfig.ItemIds is null.");
                    return;
                }

                if (serverConfig.ItemIds.Count == 0)
                {
                    LogSource.LogWarning("ServerConfig.ItemIds is empty.");
                    return;
                }

                // Log the item IDs
                LogSource.LogWarning($"Item IDs: {string.Join(", ", serverConfig.ItemIds)}");
            }
            catch (Exception ex)
            {
                LogSource.LogWarning($"Error attempting server route.");
                LogSource.LogWarning($"Error during Start: {ex.Message}\n{ex.StackTrace}");
            }
            LogSource.LogWarning("Attempting to Initialise subscription to fake item event.");
            LeaveItThereStaticEvents.OnFakeItemInitialized += OnFakeItemInitialized;
        }

        public void OnFakeItemInitialized(FakeItem fakeItem)
        {
            LogSource.LogWarning("Running fake item event.");
            if (serverConfig.ItemIds.Contains(fakeItem.LootItem.Item.TemplateId) == false) return;
            LogSource.LogWarning("Found one of your funky items!");
            fakeItem.AddonFlags.IsPhysicalRegardlessOfSize = true;

            foreach (Transform child in fakeItem.transform)
            {
                Transform colliderTransform = child.Find("collider");
                Transform ballisticTransform = child.Find("ballistic");

                if (colliderTransform != null)
                {
                    LogSource.LogWarning("collider found!");
                    colliderTransform.gameObject.layer = 18;
                }

                if (ballisticTransform != null)
                {
                    LogSource.LogWarning("ballistic found!");
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

