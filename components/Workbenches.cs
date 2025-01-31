using LeaveItThere.Common;
using LeaveItThere.Components;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;
using Comfort.Common;
using System.Linq;
using Helpers.CraftPackHelper;
using EFT.Interactive;
using LeaveItThere.Helpers;
using LeaveItThere.Fika;
using LeaveItThere;
namespace Components.Workbenches
{
    public class Workbenches : MonoBehaviour
    {
        public static FakeItem FakeItem { get; private set; }
        public static AssetBundle uiBundle = null;
        public static StashGridClass[] container = null;

        public void Awake()
        {
            FakeItem = this.GetComponent<FakeItem>();

            if (FakeItem.LootItem.Item.Owner is TraderControllerClass traderOwner)
            {
                MikhailReznichenko.LogSource.LogWarning($"Item is owned by a trader: {traderOwner.GetType().FullName}");

                if (traderOwner.MainStorage == null || traderOwner.MainStorage.Length == 0)
                {
                    MikhailReznichenko.LogSource.LogError($"TraderController {traderOwner.GetType().FullName} has no storage grids!");
                    return;
                }

                container = traderOwner.MainStorage;
            }
            FakeItem.Actions.Add(new CraftAmmoInteraction(FakeItem));
        }
        public class CraftAmmoInteraction(FakeItem fakeItem) : CustomInteraction(fakeItem)
        {
            public override string Name => "Craft Ammo";

            public override void OnInteract()
            {
                Workbenches workbenchComponent = FakeItem.gameObject.GetComponent<Workbenches>();
                workbenchComponent?.CreateCraftUI(FakeItem.LootItem.Item.TemplateId);
            }
        }

        private void CreateCraftUI(string workbenchType)
        {
            if (MikhailReznichenko.HudDisabled == false) return;
            if (uiBundle == null)
            {
                uiBundle = AssetBundle.LoadFromFile(MikhailReznichenko.bundlePath);
                if (uiBundle == null)
                {
                    MikhailReznichenko.LogSource.LogWarning("Failed to load AssetBundle!");
                    return;
                }
            }

            GameObject prefab = uiBundle.LoadAsset<GameObject>("CraftHudCanvas");
            if (prefab == null)
            {
                MikhailReznichenko.LogSource.LogWarning("Failed to load CraftHudCanvas prefab!");
                return;
            }

            GameObject uiInstance = Instantiate(prefab);
            uiInstance.transform.SetParent(this.transform, false);

            Dropdown craftDropdown = uiInstance.transform.Find("Image/Text/CraftDropdown").GetComponent<Dropdown>();
            Text requiredItemsDescription = uiInstance.transform.Find("Image/Text/Text/Image/RequiredItemsDescription").GetComponent<Text>();
            Button craftButton = uiInstance.transform.Find("Image/Text/CraftButton").GetComponent<Button>();
            Button closeButton = uiInstance.transform.Find("Image/Text/CloseButton").GetComponent<Button>();

            CraftItemDataPack craftPack = MikhailReznichenko.GetCraftPack;
            if (craftPack == null || !craftPack.WorkbenchCrafts.ContainsKey(workbenchType) || craftPack.WorkbenchCrafts.Count == 0)
            {
                MikhailReznichenko.LogSource.LogWarning("Craft data not available for this workbench type!");
                return;
            }

            var craftList = craftPack.WorkbenchCrafts[workbenchType];
            craftDropdown.options.Clear();
            foreach (var craft in craftList)
            {
                craftDropdown.options.Add(new Dropdown.OptionData(craft.CraftName));
            }

            craftDropdown.onValueChanged.AddListener(index => UpdateCraftingInfo(index, requiredItemsDescription, craftList));
            if (craftDropdown.options.Count > 0)
            {
                UpdateCraftingInfo(0, requiredItemsDescription, craftList);
            }

            craftButton.onClick.AddListener(() =>
            {
                OnCraftButtonClicked(craftDropdown.value, craftList);
                if (uiBundle != null)
                {
                    uiBundle.Unload(false);
                    uiBundle = null;
                }
                Destroy(uiInstance);
                MikhailReznichenko.OnHudEnd();
            });
            closeButton.onClick.AddListener(() =>
            {
                if (uiBundle != null)
                {
                    uiBundle.Unload(false);
                    uiBundle = null;
                }
                Destroy(uiInstance);
                MikhailReznichenko.OnHudEnd();
            });

            MikhailReznichenko.OnHudStart();
        }

        private void UpdateCraftingInfo(int index, Text descriptionText, List<CraftableItem> craftList)
        {
            if (index < 0 || index >= craftList.Count)
            {
                MikhailReznichenko.LogSource.LogWarning("Invalid craft index selected!");
                return;
            }

            CraftableItem selectedCraft = craftList[index];
            descriptionText.text = $"Required: {selectedCraft.CraftRequiredName}";
        }

        private void OnCraftButtonClicked(int selectedIndex, List<CraftableItem> craftList)
        {
            // Validate the selected index.
            if (selectedIndex < 0 || selectedIndex >= craftList.Count)
            {
                MikhailReznichenko.LogSource.LogWarning("Invalid craft selection!");
                return;
            }

            CraftableItem selectedCraft = craftList[selectedIndex];

            // Ensure that the workbench container (an array of grids) exists.
            if (container == null || container.Length == 0)
            {
                MikhailReznichenko.LogSource.LogWarning("Workbench container is null or has no grids!");
                return;
            }

            // Locate the required item in one of the container's grids.
            Item requiredItem = null;
            StashGridClass requiredItemGrid = null;
            foreach (var grid in container)
            {
                requiredItem = grid.Items.FirstOrDefault(item => item.TemplateId == selectedCraft.CraftRequired);
                if (requiredItem != null)
                {
                    requiredItemGrid = grid;
                    break;
                }
            }

            if (requiredItem == null)
            {
                MikhailReznichenko.LogSource.LogWarning(
                    $"Required item {selectedCraft.CraftRequiredName} not found in workbench container!");
                return;
            }

            // Get the CompoundItem that owns these grids.
            // (Assumes each grid's Owner property is set and container is non-empty.)
            CompoundItem compoundContainer = container[0].ParentItem;
            if (compoundContainer == null)
            {
                MikhailReznichenko.LogSource.LogWarning("Compound container not found on grid!");
                return;
            }

            // Remove the required item.
            bool removalSucceeded = false;
            if (MikhailReznichenko.IsFikaInstalled)
            {
                removalSucceeded = LITFikaTools.RemoveItemFromGrid(compoundContainer, requiredItem);
                if (removalSucceeded)
                {
                    MikhailReznichenko.LogSource.LogWarning(
                        $"Removed {selectedCraft.CraftRequiredName} from workbench container using LITFikaTools.");
                }
                else
                {
                    MikhailReznichenko.LogSource.LogWarning(
                        "Failed to remove the required item from the workbench container using LITFikaTools!");
                    return;
                }
            }
            else
            {
                removalSucceeded = ItemHelper.RemoveItemFromContainer(compoundContainer, requiredItem);
                if (removalSucceeded)
                {
                    MikhailReznichenko.LogSource.LogWarning(
                        $"Removed {selectedCraft.CraftRequiredName} from workbench container using ItemHelper.");
                }
                else
                {
                    MikhailReznichenko.LogSource.LogWarning(
                        "Failed to remove the required item from the workbench container using ItemHelper!");
                    return;
                }
            }

            // Create the crafted item.
            string newItemId = MongoID.Generate();
            Item newItem = Singleton<ItemFactoryClass>.Instance.CreateItem(newItemId, selectedCraft.CraftGiven, null);
            if (newItem == null)
            {
                MikhailReznichenko.LogSource.LogWarning("Failed to create crafted item!");
                return;
            }

            // Attempt to add the crafted item to one of the container's grids using SpawnItem.
            bool spawnAttempted = false;
            foreach (var grid in container)
            {
                LocationInGrid freeSpace = grid.FindFreeSpace(newItem);
                if (freeSpace != null)
                {
                    // We pass Vector3.zero (or any dummy position) because we aren’t actually placing the item in the world.
                    if (MikhailReznichenko.IsFikaInstalled)
                    {
                        LITFikaTools.SpawnItem(newItem, Vector3.zero, Quaternion.identity, spawnedLootItem =>
                        {
                            if (spawnedLootItem != null)
                            {
                                var addResult = grid.Add(newItem, freeSpace, false);
                                if (addResult.Succeeded)
                                {
                                    MikhailReznichenko.LogSource.LogWarning($"Successfully added crafted item {newItem.ShortName.Localized()} to workbench container using Fika.");
                                }
                                else
                                {
                                    MikhailReznichenko.LogSource.LogWarning("Failed to add crafted item to workbench container using Fika!");
                                }
                            }
                            else
                            {
                                MikhailReznichenko.LogSource.LogWarning("SpawnItem via Fika returned null LootItem!");
                            }
                        });
                    }
                    else
                    {
                        ItemHelper.SpawnItem(newItem, Vector3.zero, Quaternion.identity, spawnedLootItem =>
                        {
                            if (spawnedLootItem != null)
                            {
                                var addResult = grid.Add(newItem, freeSpace, false);
                                if (addResult.Succeeded)
                                {
                                    MikhailReznichenko.LogSource.LogWarning($"Successfully added crafted item {newItem.ShortName.Localized()} to workbench container.");
                                }
                                else
                                {
                                    MikhailReznichenko.LogSource.LogWarning("Failed to add crafted item to workbench container!");
                                }
                            }
                            else
                            {
                                MikhailReznichenko.LogSource.LogWarning("SpawnItem returned null LootItem!");
                            }
                        });
                    }
                    spawnAttempted = true;
                    break;
                }
            }

            if (!spawnAttempted)
            {
                MikhailReznichenko.LogSource.LogWarning("Failed to add crafted item to workbench container. Possibly full.");
            }
        }
    }
}
