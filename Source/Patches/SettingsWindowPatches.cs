using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MAP_MechCommander
{
    [HarmonyPatch(typeof(Dialog_ModSettings), MethodType.Constructor, typeof(Mod))]
    public static class Patch_Dialog_ModSettings_Constructor
    {
        [HarmonyPostfix]
        public static void Postfix(Dialog_ModSettings __instance, Mod mod)
        {
            if (mod is MechCommanderModSettingsWindow)
            {
                __instance.doCloseButton = false;
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_ModSettings), "DoWindowContents")]
    public static class Patch_Dialog_ModSettings_DoWindowContents
    {
        [HarmonyPostfix]
        public static void Postfix(Dialog_ModSettings __instance, Rect inRect)
        {
            Mod mod = Traverse.Create(__instance).Field("mod").GetValue<Mod>();
            if (mod is not MechCommanderModSettingsWindow)
            {
                return;
            }

            if (MechCommanderModSettingsWindow.Settings == null)
            {
                return;
            }

            Text.Font = GameFont.Small;
            Rect buttonRect = new Rect(
                inRect.width / 2f - Window.CloseButSize.x / 2f,
                inRect.height - 55f + 15f,
                Window.CloseButSize.x,
                Window.CloseButSize.y);

            if (Widgets.ButtonText(buttonRect, "MechCommander.Settings.Reset".Translate()))
            {
                MechCommanderModSettingsWindow.Settings.hediffBlacklist.Clear();
                MechCommanderModSettingsWindow.Settings.addJusticeToWorkTab = false;
                MechCommanderModSettingsWindow.Settings.Write();
            }
        }
    }
}
