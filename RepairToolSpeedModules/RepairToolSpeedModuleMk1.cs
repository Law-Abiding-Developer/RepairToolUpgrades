using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LawAbidingTroller.RepairToolUpgrades;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using UnityEngine;

namespace RepairToolUpgrades.RepairToolModules
{
    public class RepairToolSpeedModuleMk1
    {
        public static UpgradeData Mk1Upgradedata = new(1.5f);
        public static CustomPrefab Mk1Weldspeedprefab;
        public static PrefabInfo Mk1Weldspeedprefabinfo;
        public static TechType TechType = TechType.VehiclePowerUpgradeModule;
        public static void Register()
        {
            Mk1Weldspeedprefabinfo = PrefabInfo.WithTechType("RepairToolSpeedUpgradeMk1", "Repair Tool Speed Upgrade Mk 1", "Mk 1 Speed Upgrade for the Repair Tool. Increases the repair speed by 2x normal").WithIcon(SpriteManager.Get(TechType.Welder));
            UpgradeData.UpgradeDataDict.Add(Mk1Weldspeedprefabinfo.TechType, Mk1Upgradedata);
            Mk1Weldspeedprefab = new CustomPrefab(Mk1Weldspeedprefabinfo);
            var clone = new CloneTemplate(Mk1Weldspeedprefabinfo, TechType);
            clone.ModifyPrefab += obj =>
            {
                GameObject model = obj.gameObject;
                model.transform.localScale = Vector3.one / (1 + 1 / 2);
            };
            Mk1Weldspeedprefab.SetGameObject(clone);
            Mk1Weldspeedprefab.SetRecipe(new Nautilus.Crafting.RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Battery, 1),
                    new Ingredient(TechType.WiringKit, 1)
                }
            })
            .WithFabricatorType(UpgradesLIB.Items.Equipment.Handheldprefab.HandheldfabTreeType)
            .WithStepsToFabricatorTab("Tools", "RepairToolTab")
            .WithCraftingTime(5f);
            Mk1Weldspeedprefab.SetUnlock(TechType.Welder);
            Mk1Weldspeedprefab.SetPdaGroupCategory(UpgradesLIB.Plugin.toolupgrademodules, Plugin.Repairtoolupgrades);
            Mk1Weldspeedprefab.Register();
            Plugin.Logger.LogInfo("Prefab RepairToolSpeedUpgradeMk1 successfully initialized!");
        }
    }
}
