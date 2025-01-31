using EFT;
using EFT.UI;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using LeaveItThere;
using LeaveItThere.Helpers;
using LeaveItThere.Components;
using System.Reflection;
using System.IO;
using Comfort.Common;
using Helpers.CursorHelper;
using Helpers.RouteHelper;
using Helpers.CraftPackHelper;
using Components.Workbenches;

[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("Jehree.LeaveItThere", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin("Tyrian.MikhailReznichenko", "MikhailReznichenko", "1.0.0")]

public class MikhailReznichenko : BaseUnityPlugin
{
    public static ManualLogSource LogSource;
    public static List<string> GetItemIds;
    public static List<string> GetWorkbenchIds;
    public static List<string> GetExcludedItemIds;
    public static CraftItemDataPack GetCraftPack;
    public const string itemidsToClient = "/tyrian/mikhailreznichenko/itemids_to_client";
    public const string workbenchidsToClient = "/tyrian/mikhailreznichenko/workbenchids_to_client";
    public const string craftsToClient = "/tyrian/mikhailreznichenko/crafts_to_client";
    public static string dllPath = Assembly.GetExecutingAssembly().Location;
    public static string directoryPath = Path.GetDirectoryName(dllPath);
    public static string bundlePath = Path.Combine(directoryPath, "crafthudcanvas.bundle");
    public static Player Player => Singleton<GameWorld>.Instance?.MainPlayer;
    public static bool HudDisabled = true;
    public static bool IsFikaInstalled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.fika.core");

    public void Awake()
    {
        LogSource = BepInEx.Logging.Logger.CreateLogSource(" Mikhail Reznichenko ");
        LogSource.LogWarning("Jehree is smelly :3");
    }

    public void Start()
    {
        GetItemIds = RouteHelper.ServerRoute<List<string>>(itemidsToClient) ?? [];
        GetWorkbenchIds = RouteHelper.ServerRoute<List<string>>(workbenchidsToClient) ?? [];
        GetCraftPack = new CraftItemDataPack{WorkbenchCrafts = RouteHelper.ServerRoute<Dictionary<string, List<CraftableItem>>>(craftsToClient)};
        LITStaticEvents.OnFakeItemInitialized += OnFakeItemInitialized;

        if (GetCraftPack == null || GetCraftPack.WorkbenchCrafts == null || GetCraftPack.WorkbenchCrafts.Count == 0)
        {
            LogSource.LogError("craftPack failed to load!");
            return;
        }
        LogSource.LogWarning($"craftPack loaded with {GetCraftPack.WorkbenchCrafts.Count} workbenches.");
    }

    public void Update()
    {
        if (HudDisabled) return;
        CursorHelper.SetCursor(ECursorType.Idle);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnFakeItemInitialized(FakeItem fakeItem)
    {
        if (GetItemIds.Contains(fakeItem.LootItem.Item.TemplateId) == false) return;
        fakeItem.Flags.IsPhysicalRegardlessOfSize = true;
        fakeItem.Flags.RemoveRootCollider = true;
        if (GetWorkbenchIds.Contains(fakeItem.LootItem.Item.TemplateId) == false) return;
        _ = fakeItem.gameObject.AddComponent<Workbenches>();
    }
    public static void OnHudStart()
    {
        HudDisabled = false;

        if (Player is null) return;
        if (!Player.IsYourPlayer) return;

        Player.MovementContext.ToggleBlockInputPlayerRotation(true);

        CursorHelper.SetCursor(ECursorType.Idle);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Singleton<GameWorld>.Instance.MainPlayer.GetComponent<GamePlayerOwner>().ClearInteractionState();

        if (GamePlayerOwner.MyPlayer is not null)
        {
            GamePlayerOwner.IgnoreInputWithKeepResetLook = true;
            GamePlayerOwner.IgnoreInputInNPCDialog = true;
        }

        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdown);
    }

    public static void OnHudEnd()
    {
        HudDisabled = true;

        if (Player is null) return;
        if (!Player.IsYourPlayer) return;

        Player.MovementContext.ToggleBlockInputPlayerRotation(false);

        CursorHelper.SetCursor(ECursorType.Invisible);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (GamePlayerOwner.MyPlayer is not null)
        {
            GamePlayerOwner.IgnoreInputWithKeepResetLook = false;
            GamePlayerOwner.IgnoreInputInNPCDialog = false;
        }

        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdown);
    }
}

