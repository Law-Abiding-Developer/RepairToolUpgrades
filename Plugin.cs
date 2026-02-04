using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using PluginInfo = RepairToolUpgrades.PluginInfo;
using RepairToolUpgrades.RepairToolModules;
using LawAbidingTroller.RepairToolUpgrades.RepairToolEfficiencyModules;
using Nautilus.Assets;

namespace LawAbidingTroller.RepairToolUpgrades
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.snmodding.nautilus")]
    [BepInDependency("com.lawabidingmodder.upgradeslib")]
    public class Plugin : BaseUnityPlugin
    {
        public new static ManualLogSource Logger { get; private set; }

        public static GameInput.Button OpenUpgradesButton = EnumHandler.AddEntry<GameInput.Button>("OpenRTUpgrades")
            .CreateInput("Open Repair Tool Upgrades")
            .WithKeyboardBinding(GameInputHandler.Paths.Keyboard.B)
            .WithCategory("Tools Upgrades");
        private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

        public static TechCategory Repairtoolupgrades = EnumHandler.AddEntry<TechCategory>("RepairToolUpgrades").WithPdaInfo("Repair Tool Upgrades").RegisterToTechGroup(UpgradesLIB.Plugin.toolupgrademodules);
        
        private void Awake()
        {
            // set project-scoped logger instance
            Logger = base.Logger;
            
            Logger.LogInfo($"Awake method is running. Config Options loaded. Dependencies exist. Loading {PluginInfo.PLUGIN_NAME}...");
            
            // create the storage
            StartCoroutine(UpgradesLIB.Plugin.CreateUpgradesContainer(TechType.Welder, "WelderUpgradeStorage", "WelderUpgradeStorageChild", 2, 1));

            // register harmony patches, if there are any
            Harmony.CreateAndPatchAll(Assembly, "RepairToolUpgrades");
            
            // create the tabs and move the welder to the tab
            CraftTreeHandler.AddTabNode(UpgradesLIB.Items.Equipment.Handheldprefab.HandheldfabTreeType, "RepairToolTab", "Repair Tool", SpriteManager.Get(TechType.Welder), "Tools");
            
            // initialize prefabs
            InitializePrefabs();
            
            Logger.LogInfo("Plugin fully loaded successfully!");
        }
        
        private void InitializePrefabs()
        {
            RepairToolSpeedModuleMk1.Register();
            RepairToolSpeedModuleMk2.Register();
            RepairToolSpeedModuleMk3.Register();
            RepairToolEfficiencyModules.RepairToolEfficiencyModules.RegisterAll();
        }
    }
}