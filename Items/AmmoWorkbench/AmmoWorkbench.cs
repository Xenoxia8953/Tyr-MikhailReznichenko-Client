using LeaveItThere.Common;
using LeaveItThere.Components;
using EFT;
using EFT.UI;
using EFT.Interactive;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Helpers.CursorHelper;
using Comfort.Common;

namespace Items.AmmoWorkbench
{
    internal class AmmoWorkbench : MonoBehaviour
    {
        public FakeItem FakeItem { get; private set; }
        private AssetBundle uiBundle = null;

        public void Awake()
        {
            FakeItem = this.GetComponent<FakeItem>();
            FakeItem.Actions.Add(new CustomInteraction(
                "Craft Ammo",
                false,
                () =>
                {
                    AmmoWorkbench workbenchComponent = FakeItem.gameObject.GetComponent<AmmoWorkbench>();
                    workbenchComponent?.CreateCraftUI(FakeItem.LootItem.Item.TemplateId);
                }
            ));
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
            if (craftPack == null || !craftPack.CraftItemsByWorkbench.ContainsKey(workbenchType))
            {
                MikhailReznichenko.LogSource.LogWarning("Craft data not available for this workbench type!");
                return;
            }

            var craftList = craftPack.CraftItemsByWorkbench[workbenchType];
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

            // Assign button actions
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

        private void OnCraftButtonClicked(int index, List<CraftableItem> craftList)
        {
            if (index < 0 || index >= craftList.Count)
            {
                MikhailReznichenko.LogSource.LogWarning("Invalid craft selection!");
                return;
            }

            CraftableItem selectedCraft = craftList[index];
            MikhailReznichenko.LogSource.LogWarning($"Crafting selected item: {selectedCraft.CraftName}");
            MikhailReznichenko.LogSource.LogWarning($"Removing {selectedCraft.CraftRequiredName} and adding {selectedCraft.CraftName}");
        }
    }
}
