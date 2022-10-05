using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace SailingSpeed;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Guilds : BaseUnityPlugin
{
	private const string ModName = "Sailing Speed";
	private const string ModVersion = "1.0.1";
	private const string ModGUID = "org.bepinex.plugins.sailingspeed";

	private enum Toggle
	{
		On = 1,
		Off = 0
	}

	private static readonly ConfigSync configSync = new(ModName) { CurrentVersion = ModVersion };

	private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
	{
		ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

		SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
		syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

		return configEntry;
	}

	private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

	private static ConfigEntry<Toggle> serverConfigLocked = null!;
	private static ConfigEntry<float> sailingBaseSpeed = null!;

	public void Awake()
	{
		serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, new ConfigDescription("Locks the config and enforces the servers configuration."));
		configSync.AddLockingConfigEntry(serverConfigLocked);
		sailingBaseSpeed = config("1 - General", "Sailing base speed (percentage)", 110f, new ConfigDescription("Base speed for ships as a percentage of the vanilla base speed.", new AcceptableValueRange<float>(10f, 300f)));

		Assembly assembly = Assembly.GetExecutingAssembly();
		Harmony harmony = new(ModGUID);
		harmony.PatchAll(assembly);
	}

	[HarmonyPatch(typeof(Ship), nameof(Ship.GetSailForce))]
	private class ChangeShipBaseSpeed
	{
		private static void Postfix(ref Vector3 __result)
		{
			__result *= sailingBaseSpeed.Value / 100f;
		}
	}
}
