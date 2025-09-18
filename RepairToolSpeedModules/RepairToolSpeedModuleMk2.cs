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
    public class RepairToolSpeedModuleMk2
    {
        public static UpgradeData Mk2Upgradedata = new(4.0f);
        public static CustomPrefab Mk2Weldspeedprefab;
        public static PrefabInfo Mk2Weldspeedprefabinfo;
        public static TechType TechType = TechType.VehiclePowerUpgradeModule;
        public static void Register()
        {
            Mk2Weldspeedprefabinfo = PrefabInfo.WithTechType("RepairToolSpeedUpgradeMk2", "Repair Tool Speed Upgrade Mk 2", "Mk 2 Speed Upgrade for the Repair Tool. Increases repair speed by 4x normal").WithIcon(SpriteManager.Get(TechType.Welder));
            UpgradeData.UpgradeDataDict.Add(Mk2Weldspeedprefabinfo.TechType, Mk2Upgradedata);
            Mk2Weldspeedprefab = new CustomPrefab(Mk2Weldspeedprefabinfo);
            var clone = new CloneTemplate(Mk2Weldspeedprefabinfo, TechType);
            clone.ModifyPrefab += obj =>
            {
                GameObject model = obj.gameObject;
                model.transform.localScale = Vector3.one / (1 + 1 / 2);
            };
            Mk2Weldspeedprefab.SetGameObject(clone);
            Mk2Weldspeedprefab.SetRecipe(new Nautilus.Crafting.RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient(TechType.Lubricant, 1),
                    new Ingredient(TechType.WiringKit, 1),
                    new Ingredient(RepairToolSpeedModuleMk1.Mk1Weldspeedprefabinfo.TechType, 1)
                }
            }).WithFabricatorType(UpgradesLIB.Items.Equipment.Handheldprefab.HandheldfabTreeType)
            .WithStepsToFabricatorTab("Tools", "RepairToolTab")
            .WithCraftingTime(5f);
            Mk2Weldspeedprefab.SetUnlock(TechType.Welder);
            Mk2Weldspeedprefab.SetPdaGroupCategory(UpgradesLIB.Plugin.toolupgrademodules, Plugin.Repairtoolupgrades);
            Mk2Weldspeedprefab.Register();
            Plugin.Logger.LogInfo("Prefab RepairToolSpeedUpgradeMk3 successfully initialized!");
        }
    }
}
