using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RepairToolUpgrades.RepairToolModules;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace LawAbidingTroller.RepairToolUpgrades;

[HarmonyPatch(typeof(Welder))]
public class WelderPatches
{
    private static Dictionary<Welder,float> timers = new();
    [HarmonyPatch(nameof(Welder.Update))] [HarmonyPostfix]
    public static void Update_Postfix(Welder __instance)
    {
        if (__instance == null) {Plugin.Logger.LogError($"__instance is null in {nameof(Update_Postfix)}!");return;}
        if (!timers.ContainsKey(__instance)) timers[__instance] = 0; 
        timers[__instance] += Time.deltaTime;
        var tempstorage = __instance.GetComponent<StorageContainer>();
        if (tempstorage == null) {Plugin.Logger.LogError($"tempstorage is null in {nameof(Update_Postfix)}!");return;}
        float highestspeed = 0;
        foreach (var item in tempstorage.container.GetItemTypes())
        {
            if (!UpgradeData.UpgradeDataDict.TryGetValue(item, out var tempdata)) { Plugin.Logger.LogError($"Cannot get TechType '{item}' from dictionary '{nameof(UpgradeData.UpgradeDataDict)}'");continue;}
            highestspeed = Mathf.Max(highestspeed, tempdata.Speedmultiplier);
        }

        float timetoweld = 0.047f;
        if (highestspeed != 0)
        {
            timetoweld /= highestspeed;
        }
        if (__instance.usedThisFrame && timers[__instance] >= timetoweld) {__instance.Weld(); timers[__instance] = 0.0f; }
        if (tempstorage.container == null) {Plugin.Logger.LogError("tempstorage.container is null!");return;}
        tempstorage.container._label = "REPAIR TOOL";
        var allowedtech = new[]
        {
            RepairToolSpeedModuleMk1.Mk1Weldspeedprefabinfo.TechType,
            RepairToolSpeedModuleMk2.Mk2Weldspeedprefabinfo.TechType,
            RepairToolSpeedModuleMk3.Mk3Weldspeedprefabinfo.TechType,
            RepairToolEfficiencyModules.RepairToolEfficiencyModules.PrefabInfos[0].TechType,
            RepairToolEfficiencyModules.RepairToolEfficiencyModules.PrefabInfos[1].TechType,
            RepairToolEfficiencyModules.RepairToolEfficiencyModules.PrefabInfos[2].TechType
        };
        tempstorage.container.SetAllowedTechTypes(allowedtech);
        if (Input.GetKeyDown(Config.OpenUpgradesContainerkeybind))
        {
            if (tempstorage.open) { ErrorMessage.AddWarning("Close 'REPAIR TOOL' to open it" ); return; }
            tempstorage.Open();
        }
        __instance.StopWeldingFX();
    }
    [HarmonyPatch(nameof(Welder.Weld))]
    [HarmonyPrefix]
    public static void Weld_Prefix(Welder __instance)
    {
        __instance.healthPerWeld = 1f;
        __instance.weldEnergyCost = 0.01f;
        var tempstorage = __instance.GetComponent<StorageContainer>();
        if (tempstorage == null) {Plugin.Logger.LogError("Failed to get storage container component for Repair tool!");return;}
        UpgradeData tempdata;
        float highestefficiency = 0;
        foreach (var item in tempstorage.container.GetItemTypes())
        {
            if (!UpgradeData.UpgradeDataDict.TryGetValue(item, out tempdata)) { Plugin.Logger.LogError($"Cannot get TechType '{item}' from dictionary '{nameof(UpgradeData.UpgradeDataDict)}'");continue;}

            if (tempdata.Speedmultiplier == 0)
            {
                highestefficiency = Mathf.Max(highestefficiency, tempdata.Efficiency);
            }
        }
        if (highestefficiency != 0)
        {
            __instance.weldEnergyCost /= highestefficiency;
        }
    }

    [HarmonyPatch(nameof(Welder.OnDisable))]
    [HarmonyPostfix]
    public static void OnDisable_Postfix(Welder __instance)
    {
        timers.Remove(__instance);
    }
}

[HarmonyPatch(typeof(CyclopsExternalDamageManager))]
public class CyclopsExternalDamageManagerPatches
{
    [HarmonyPatch(nameof(CyclopsExternalDamageManager.RepairPoint))]
    [HarmonyPrefix]
    public static bool RepairPoint_Prefix(CyclopsExternalDamageManager __instance, CyclopsDamagePoint point)
    {
        
        __instance.unusedDamagePoints.Add(point);
        float healthBack = __instance.subLiveMixin.maxHealth / __instance.damagePoints.Length;
        PatchData.damagePoints.Add(point, new PatchData(healthBack, __instance));
        if (__instance.damagePoints.Length - __instance.unusedDamagePoints.Count == 0)
        {
            __instance.subLiveMixin.AddHealth(__instance.subLiveMixin.maxHealth);
        }
        else
        {
            __instance.subLiveMixin.AddHealth(1f);
        }
        __instance.ToggleLeakPointsBasedOnDamage();
        return false;
    }
}

[HarmonyPatch(typeof(CyclopsDamagePoint))]
public class CyclopsDamagePointPatches
{
    [HarmonyPatch(nameof(CyclopsDamagePoint.OnRepair))]
    [HarmonyPostfix]
    public static void OnRepair_Postfix(CyclopsDamagePoint __instance)
    {
        PatchData.damagePoints.TryGetValue(__instance, out var point);
        if (point == null) return;
        point.SubLiveMixin.subLiveMixin.AddHealth(point.HealthBack);
        PatchData.damagePoints.Remove(__instance);
    }
}

public class UpgradeData
{
    public static Dictionary<TechType, UpgradeData> UpgradeDataDict = new Dictionary<TechType, UpgradeData>();
    public float Speedmultiplier;
    public float Efficiency;


    public UpgradeData(float speedmultiplier = 0, float efficiency = 0)
    {
        this.Speedmultiplier = speedmultiplier;
        this.Efficiency = efficiency;
    }
}

public class PatchData
{
    public static Dictionary<CyclopsDamagePoint, PatchData> damagePoints = new Dictionary<CyclopsDamagePoint, PatchData>();
    public float HealthBack;
    public CyclopsExternalDamageManager SubLiveMixin;

    public PatchData(float healthBack, CyclopsExternalDamageManager subLiveMixin)
    {
        HealthBack = healthBack;
        SubLiveMixin = subLiveMixin;
    }
}