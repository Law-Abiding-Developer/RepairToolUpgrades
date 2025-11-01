using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using UnityEngine;

namespace LawAbidingTroller.RepairToolUpgrades;

[Menu("Repair Tool Upgrades")]
public class Config : ConfigFile
{
    public static KeyCode OpenUpgradesContainerkeybind = KeyCode.B;
}