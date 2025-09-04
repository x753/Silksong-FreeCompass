using BepInEx;
using BepInEx.Logging;
using GlobalSettings;
using HarmonyLib;

namespace FreeCompass
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class FreeCompassMod : BaseUnityPlugin
    {
        private const string modGUID = "x753.FreeCompass";
        private const string modName = "FreeCompass";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        internal static ManualLogSource ModLogger;

        public static FreeCompassMod Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            harmony.PatchAll();
            ModLogger = BepInEx.Logging.Logger.CreateLogSource(modName);
            ModLogger.LogInfo($"Plugin {modName} is loaded!");

            InitializeConfigs();
        }

        public static bool RequiresCompassPurchased;
        public static bool RequiresCompassEquipped;
        public static int CompassPrice;

        public void InitializeConfigs()
        {
            RequiresCompassPurchased = Config.Bind("Settings", "Requires Compass Purchased", false, "Do you need to purchase the compass?").Value;
            RequiresCompassEquipped = Config.Bind("Settings", "Requires Compass Equipped", false, "Do you need to equip the compass?").Value;
            CompassPrice = Config.Bind("Settings", "Compass Price", 0, "How much does the compass cost?").Value;
        }

        [HarmonyPatch(typeof(GameMap), "PositionCompassAndCorpse")]
        class GameMap_PositionCompassAndCorpse_Patch
        {
            [HarmonyPostfix]
            public static void GameMap_PositionCompassAndCorpse_Postfix(GameMap __instance)
            {
                if (RequiresCompassEquipped) { return; } // vanilla should handle this

                if (__instance.currentSceneObj != null)
                {
                    if (Gameplay.CompassTool.IsUnlocked || !RequiresCompassPurchased)
                    {
                        __instance.compassIcon.SetActive(true);
                        __instance.displayingCompass = true;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ShopItem), "get_Cost")]
        class ShopItem_Cost_Patch
        {
            [HarmonyPrefix]
            public static bool ShopItem_Cost_Prefix(ShopItem __instance, ref int __result)
            {
                if(__instance.name == "Mapper Compass Tool")
                {
                    __result = CompassPrice;
                    return false;
                }

                return true;
            }
        }
    }
}