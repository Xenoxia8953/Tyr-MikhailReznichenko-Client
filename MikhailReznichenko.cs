using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using LeaveItThere.Helpers;
using LeaveItThere.Components;
using Newtonsoft.Json;
using SPT.Common.Http;
using System.Reflection;
using System.IO;
using Items.AmmoWorkbench;
using EFT;
using Comfort.Common;
using EFT.UI;
using Helpers.CursorHelper;
using UnityEngine;

[BepInDependency("Jehree.LeaveItThere", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin("Tyrian.MikhailReznichenko", "MikhailReznichenko", "1.0.0")]

public class MikhailReznichenko : BaseUnityPlugin
{
    public static ManualLogSource LogSource;
    public static List<string> GetItemIds;
    public static List<string> GetExcludedItemIds;
    public static CraftItemDataPack GetCraftPack;
    public const string itemidsToClient = "/tyrian/mikhailreznichenko/itemids_to_client";
    public const string craftsToClient = "/tyrian/mikhailreznichenko/crafts_to_client";
    public static string dllPath = Assembly.GetExecutingAssembly().Location;
    public static string directoryPath = Path.GetDirectoryName(dllPath);
    public static string bundlePath = Path.Combine(directoryPath, "crafthudcanvas.bundle");
    public static Player Player => Singleton<GameWorld>.Instance?.MainPlayer;
    public static bool HudDisabled = true;

    public void Awake()
    {
        LogSource = BepInEx.Logging.Logger.CreateLogSource(" Mikhail Reznichenko ");
        LogSource.LogWarning("Jehree is smelly :3");
    }

    public void Start()
    {

        GetItemIds = RouteHelper.ServerRoute<List<string>>(itemidsToClient) ?? [];
        GetCraftPack = RouteHelper.ServerRoute<CraftItemDataPack>(craftsToClient, CraftItemDataPack.Request);

        if (GetCraftPack == null || GetCraftPack.CraftItemsByWorkbench == null)
        {
            LogSource.LogError("craftPack failed to load!");
            return;
        }

        LogSource.LogWarning($"craftPack loaded with {GetCraftPack.CraftItemsByWorkbench.Count} workbenches.");
        LeaveItThereStaticEvents.OnFakeItemInitialized += OnFakeItemInitialized;
    }

    public void Update()
    {
        if (HudDisabled) return;
        CursorHelper.SetCursor(ECursorType.Idle);

        if (GamePlayerOwner.MyPlayer is not null)
        {
            GamePlayerOwner.IgnoreInputWithKeepResetLook = true;
            GamePlayerOwner.IgnoreInputInNPCDialog = true;
        }
    }

    public void OnFakeItemInitialized(FakeItem fakeItem)
    {
        if (GetItemIds.Contains(fakeItem.LootItem.Item.TemplateId) == false) return;
        fakeItem.AddonFlags.IsPhysicalRegardlessOfSize = true;
        fakeItem.AddonFlags.RemoveRootCollider = true;
        if (fakeItem.LootItem.Item.TemplateId != "678ffa4b980c2eb729065d08") return;
        _ = fakeItem.gameObject.AddComponent<AmmoWorkbench>();
    }
    public static void OnHudStart()
    {
        HudDisabled = false;

        if (Player is null) return;
        if (!Player.IsYourPlayer) return;

        Player.MovementContext.ToggleBlockInputPlayerRotation(true);

        CursorHelper.SetCursor(ECursorType.Idle);
        Cursor.visible = true;

        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdown);
    }

    public static void OnHudEnd()
    {
        HudDisabled = true;

        if (Player is null) return;
        if (!Player.IsYourPlayer) return;

        Player.MovementContext.ToggleBlockInputPlayerRotation(false);

        CursorHelper.SetCursor(ECursorType.Invisible);
        Cursor.visible = false;

        if (GamePlayerOwner.MyPlayer is not null)
        {
            GamePlayerOwner.IgnoreInputWithKeepResetLook = false;
            GamePlayerOwner.IgnoreInputInNPCDialog = false;
        }

        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdown);
    }
}
public class RouteHelper
{
    public static T ServerRoute<T>(string url, object data = default)
    {
        string json = JsonConvert.SerializeObject(data);
        var req = RequestHandler.PostJson(url, json);

        if (typeof(T) == typeof(CraftItemDataPack))
        {
            List<Dictionary<string, List<CraftableItem>>> craftDataList =
            JsonConvert.DeserializeObject<List<Dictionary<string, List<CraftableItem>>>>(req);
            Dictionary<string, List<CraftableItem>> mergedCrafts = new();

            foreach (var workbenchData in craftDataList)
            {
                foreach (var kvp in workbenchData)
                {
                    if (!mergedCrafts.ContainsKey(kvp.Key))
                    {
                        mergedCrafts[kvp.Key] = new List<CraftableItem>();
                    }

                    mergedCrafts[kvp.Key].AddRange(kvp.Value);
                }
            }
            return (T)(object)new CraftItemDataPack(mergedCrafts);
        }
        return JsonConvert.DeserializeObject<T>(req);
    }
}
public class CraftItemDataPack
{
    public Dictionary<string, List<CraftableItem>> CraftItemsByWorkbench { get; set; }

    [JsonIgnore]
    public static CraftItemDataPack Request => new(new Dictionary<string, List<CraftableItem>>());

    public CraftItemDataPack()
    {
        CraftItemsByWorkbench = new Dictionary<string, List<CraftableItem>>();
    }

    public CraftItemDataPack(Dictionary<string, List<CraftableItem>> craftItemsByWorkbench)
    {
        CraftItemsByWorkbench = craftItemsByWorkbench ?? new Dictionary<string, List<CraftableItem>>();
    }
}
public class CraftableItem
{
    [JsonProperty("craftName")]
    public string CraftName { get; set; }

    [JsonProperty("craftRequiredName")]
    public string CraftRequiredName { get; set; }

    [JsonProperty("craftRequired")]
    public string CraftRequired { get; set; }

    [JsonProperty("craftGiven")]
    public string CraftGiven { get; set; }
}

