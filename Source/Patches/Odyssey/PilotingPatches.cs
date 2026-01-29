using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    [HarmonyPatch(typeof(StatWorker), "IsDisabledFor")]
    public static class Patch_StatWorker_IsDisabledFor_Piloting
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        [HarmonyPostfix]
        public static void Postfix(StatWorker __instance, Thing thing, ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            if (!ModsConfig.OdysseyActive)
            {
                return;
            }

            Pawn? pawn = thing as Pawn;
            if (pawn?.def?.defName != JusticeDefName)
            {
                return;
            }

            StatDef? stat = AccessTools.Field(typeof(StatWorker), "stat")?.GetValue(__instance) as StatDef;
            if (stat == StatDefOf.PilotingAbility)
            {
                __result = false;
            }
        }
    }
}
