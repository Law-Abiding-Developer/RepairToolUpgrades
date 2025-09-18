using System.Collections.Generic;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using UnityEngine;

namespace LawAbidingTroller.RepairToolUpgrades.RepairToolEfficiencyModules;

public class RepairToolEfficiencyModules
{
    public static PrefabInfo[] PrefabInfos = new PrefabInfo[3];
    public static CustomPrefab[] CustomPrefabs = new CustomPrefab[3];
    public static IngredientList[] IngredientLists = { new(new(TechType.Lubricant,1), new(TechType.Battery,1)), new(new(TechType.Silicone,1), new(TechType.WiringKit,1)), new(new(TechType.AdvancedWiringKit,1), new(TechType.Aerogel,1)) };
    public static void RegisterAll()
    {
        var currentmultiplier = 2f;
        for (int i = 0; i < 3; i++)
        {
            var tempmultiplier = currentmultiplier;
            PrefabInfos[i] = PrefabInfo.WithTechType(
                $"RepairToolEfficiencyUpgradeMk{i+1}",
                $"Repair Tool Efficiency Upgrade Mk {i+1}",
                $"Mk {i+1} efficiency upgrade for the Repair Tool. Increases the repair efficiency by {tempmultiplier}x normal speed."
            ).WithIcon(SpriteManager.Get(TechType.Welder));
            CustomPrefabs[i] = new CustomPrefab(PrefabInfos[i]);
            if(i==0)continue;
            tempmultiplier += i + 1;
        }
        for (int i = 0; i < 3; i++)
        {
            var upgradedata = new UpgradeData(0, currentmultiplier);
            if (i > 0)
            {
                IngredientLists[i].Ingredients.Add(new Ingredient(PrefabInfos[i-1].TechType, 1));
            }
            Register(upgradedata, PrefabInfos[i], CustomPrefabs[i], i+1, IngredientLists[i].Ingredients);
            if (i == 0) continue;
            currentmultiplier += i+1;
        }
    }
    public static void Register(UpgradeData upgrade, PrefabInfo prefabInfo, CustomPrefab prefab, int mk, List<Ingredient> recipe)
    {
        UpgradeData.UpgradeDataDict.Add(prefabInfo.TechType, upgrade);
        var clone = new CloneTemplate(prefabInfo, TechType.VehiclePowerUpgradeModule);
        clone.ModifyPrefab += obj =>
        {
            GameObject model = obj.gameObject;
            model.transform.localScale = Vector3.one / (1 + 0.5f);
        };
        prefab.SetGameObject(clone);
        prefab.SetRecipe(new Nautilus.Crafting.RecipeData()
        {
            craftAmount = 1,
            Ingredients = recipe
        }).WithFabricatorType(UpgradesLIB.Items.Equipment.Handheldprefab.HandheldfabTreeType)
        .WithStepsToFabricatorTab("Tools", "RepairToolTab")
        .WithCraftingTime(5f);
        prefab.SetUnlock(TechType.Welder);
        prefab.SetPdaGroupCategory(UpgradesLIB.Plugin.toolupgrademodules, Plugin.Repairtoolupgrades);
        prefab.Register();
        Plugin.Logger.LogInfo($"Prefab RepairToolEfficiencyUpgradeMk{mk} successfully initialized!");
    }
}

public class IngredientList
{
    public List<Ingredient> Ingredients = new();

    public IngredientList(params  Ingredient[] ingredients)
    {
        foreach (var ingredient in ingredients)
        {
            this.Ingredients.Add(ingredient);
        }
    }
}