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
        //if an instance does not have a timer set for it yet, set a timer
        if (!timers.ContainsKey(__instance)) timers[__instance] = 0; 
        //add time delta time every frame to avoid fps dependence 
        timers[__instance] += Time.deltaTime;
        //Get the storage container reference
        var tempstorage = __instance.GetComponent<StorageContainer>();
        //Null check it
        if (tempstorage == null) {Plugin.Logger.LogError($"tempstorage is null in {nameof(Update_Postfix)}!");return;}
        float highestspeed = 0;
        //Search the storage container for an upgrade with the highest speed multiplier
        foreach (var item in tempstorage.container.GetItemTypes())
        {
            if (!UpgradeData.UpgradeDataDict.TryGetValue(item, out var tempdata)) { Plugin.Logger.LogError($"Cannot get TechType '{item}' from dictionary '{nameof(UpgradeData.UpgradeDataDict)}'");continue;}
            highestspeed = Mathf.Max(highestspeed, tempdata.Speedmultiplier);
        }
        //Base time before calling Weld
        float timetoweld = 0.047f;
        //Check if an upgrade was found
        if (highestspeed != 0)
        {
            //Divide by multiplier if so
            timetoweld /= highestspeed;
        }
        //If the repair tool is used this frame and the time before calling Weld is now less than or equal to 0,
        //call Weld
        if (__instance.usedThisFrame && timers[__instance] >= timetoweld) {__instance.Weld(); timers[__instance] = 0.0f; }
        //Check if the ItemContainer is null
        if (tempstorage.container == null) {Plugin.Logger.LogError("tempstorage.container is null!");return;}
        //Set the label in the PDS (i have no idea why this is in update)
        tempstorage.container._label = "REPAIR TOOL";
        //Set the allowed tech to every upgrade from this mod
        var allowedtech = new[]
        {
            RepairToolSpeedModuleMk1.Mk1Weldspeedprefabinfo.TechType,
            RepairToolSpeedModuleMk2.Mk2Weldspeedprefabinfo.TechType,
            RepairToolSpeedModuleMk3.Mk3Weldspeedprefabinfo.TechType,
            //No I'm not okay, I don't even want to look at this
            RepairToolEfficiencyModules.RepairToolEfficiencyModules.PrefabInfos[0].TechType,
            RepairToolEfficiencyModules.RepairToolEfficiencyModules.PrefabInfos[1].TechType,
            RepairToolEfficiencyModules.RepairToolEfficiencyModules.PrefabInfos[2].TechType
        };
        //Set the ItemsContainer's allowed tech
        tempstorage.container.SetAllowedTechTypes(allowedtech);
        //Check if the keybind was pressed (im lazy to use the new input system
        if (GameInput.GetButtonDown(Plugin.OpenUpgradesButton))
        {
            //Is it already open?
            if (tempstorage.open) { ErrorMessage.AddWarning("Close 'REPAIR TOOL' to open it" ); return; }
            //If not, open it
            tempstorage.Open();
        }
        //Forcefully stop the welding fx to avoid a light show (sound never plays anyway)
        __instance.StopWeldingFX();
    }
    [HarmonyPatch(nameof(Welder.Weld))]
    [HarmonyPrefix]
    public static void Weld_Prefix(Welder __instance)
    {
        Plugin.Logger.LogDebug($"activeWeldTarget: {__instance.activeWeldTarget}, .name: {__instance.activeWeldTarget.name}");
        
        __instance.healthPerWeld = 1f;
        if (__instance.activeWeldTarget.name.Equals("DamagePoint")) __instance.healthPerWeld *= 2;
        __instance.weldEnergyCost = 0.01f;
        var tempstorage = __instance.GetComponent<StorageContainer>();
        if (tempstorage == null) {Plugin.Logger.LogError("Failed to get storage container component for Repair tool!");return;}
        UpgradeData tempdata;
        float highestefficiency = 0;
        if (tempstorage.container == null) return;
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

/*[HarmonyPatch(typeof(CyclopsExternalDamageManager))]
[HarmonyPatch(nameof(CyclopsExternalDamageManager.RepairPoint))]
public class CyclopsExternalDamageManager_RepairPoint_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeMatcher = new CodeMatcher(instructions)
            .MatchForward(true, 
                new CodeMatch(OpCodes.Conv_I4), new CodeMatch(OpCodes.Conv_R4),
                new CodeMatch(OpCodes.Div), new CodeMatch(OpCodes.Stloc_0))
            .Insert(new CodeInstruction(OpCodes.Call, 
                AccessTools.Method(typeof(PatchData),
                    nameof(PatchData.RepairPointHelper), 
                    new []{typeof(CyclopsExternalDamageManager), 
                        typeof(CyclopsDamagePoint)})))
            .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0), 
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(CyclopsExternalDamageManager),
                    nameof(CyclopsExternalDamageManager.subLiveMixin))), new CodeMatch(OpCodes.Ldloc_0))
            .RemoveInstructions(3).Insert(new CodeInstruction(OpCodes.Ldc_I4_1));
        return codeMatcher.InstructionEnumeration();
    }
}*/

/*[HarmonyPatch(typeof(CyclopsDamagePoint))]
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
}*/


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
    
    public static void RepairPointHelper(CyclopsExternalDamageManager __instance, CyclopsDamagePoint point)
    {
        float healthBack = __instance.subLiveMixin.maxHealth / __instance.damagePoints.Length;
        PatchData.damagePoints.Add(point, new PatchData(healthBack, __instance));
    }
}