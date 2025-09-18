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
    public class RepairToolSpeedModuleMk3
    {
        public static UpgradeData Mk3Upgradedata = new(7.0f);
        public static CustomPrefab Mk3Weldspeedprefab;
        public static PrefabInfo Mk3Weldspeedprefabinfo;
        public static TechType TechType = TechType.VehiclePowerUpgradeModule;
        public static void Register()
        {
            Mk3Weldspeedprefabinfo = PrefabInfo.WithTechType("RepairToolSpeedUpgradeMk3", "Repair Tool Speed Upgrade Mk 3", "Mk 3 Speed Upgrade for the Repair Tool. Increases repair speed by 7x normal").WithIcon(SpriteManager.Get(TechType.Welder));
            UpgradeData.UpgradeDataDict.Add(Mk3Weldspeedprefabinfo.TechType, Mk3Upgradedata);
            Mk3Weldspeedprefab = new CustomPrefab(Mk3Weldspeedprefabinfo);
            var clone = new CloneTemplate(Mk3Weldspeedprefabinfo, TechType);
            clone.ModifyPrefab += obj =>
            {
                GameObject model = obj.gameObject;
                model.transform.localScale = Vector3.one / (1 + 1 / 2);
            };
            Mk3Weldspeedprefab.SetGameObject(clone);
            Mk3Weldspeedprefab.SetRecipe(new Nautilus.Crafting.RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Aerogel, 1),
                    new Ingredient(TechType.AdvancedWiringKit, 1),
                    new Ingredient(RepairToolSpeedModuleMk2.Mk2Weldspeedprefabinfo.TechType, 1)
                }
            }).WithFabricatorType(UpgradesLIB.Items.Equipment.Handheldprefab.HandheldfabTreeType)
            .WithStepsToFabricatorTab("Tools", "RepairToolTab")
            .WithCraftingTime(5f);
            Mk3Weldspeedprefab.SetUnlock(TechType.Welder);
            Mk3Weldspeedprefab.SetPdaGroupCategory(UpgradesLIB.Plugin.toolupgrademodules, Plugin.Repairtoolupgrades);
            Mk3Weldspeedprefab.Register();
            Plugin.Logger.LogInfo("Prefab RepairToolSpeedUpgradeMk3 successfully initialized!");
        }
    }
}
