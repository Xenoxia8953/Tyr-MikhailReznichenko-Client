using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using LeaveItThere.Helpers;
using LeaveItThere.Components;
using Newtonsoft.Json;
using UnityEngine;
using SPT.Common.Http;

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
            GetItemIds = ServerRouteHelper.ServerRoute<List<string>>(configToClient);
            LeaveItThereStaticEvents.OnFakeItemInitialized += OnFakeItemInitialized;
            LeaveItThereStaticEvents.OnItemPlacedStateChanged += OnItemPlacedStateChanged;
        }
        public void OnFakeItemInitialized(FakeItem fakeItem)
        {
            if (GetItemIds.Contains(fakeItem.LootItem.Item.TemplateId) == false) return;
            fakeItem.AddonFlags.IsPhysicalRegardlessOfSize = false;
            fakeItem.gameObject.layer = 22;

            BoxCollider boxCollider = fakeItem.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Destroy(boxCollider);
            }

            List<GameObject> children = GetAllChildren(fakeItem.gameObject);
            ProcessAllChildren(children);
        }
        public void OnItemPlacedStateChanged(FakeItem fakeItem, bool isPlaced)
        {
            if (GetItemIds.Contains(fakeItem.LootItem.Item.TemplateId) == false) return;
            fakeItem.AddonFlags.IsPhysicalRegardlessOfSize = false ;
            fakeItem.gameObject.layer = 22;

            BoxCollider boxCollider = fakeItem.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Destroy(boxCollider);
            }

            List<GameObject> children = GetAllChildren(fakeItem.gameObject);
            ProcessAllChildren(children);
        }
        public static List<GameObject> GetAllChildren(GameObject parent)
        {
            List<GameObject> children = [];

            foreach (Transform child in parent.transform)
            {
                children.Add(child.gameObject);
                children.AddRange(GetAllChildren(child.gameObject));
            }

            return children;
        }
        private void ProcessAllChildren(List<GameObject> children)
        {
            foreach (GameObject child in children)
            {
                if (child.name.Contains("LOD0"))
                {
                    child.gameObject.layer = 15;
                }
                if (child.name.Equals("collider"))
                {
                    child.gameObject.layer = 18;
                }

                if (child.name.Equals("ballistic"))
                {
                    child.gameObject.layer = 12;
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

