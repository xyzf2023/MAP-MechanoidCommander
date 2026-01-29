using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    [HarmonyPatch(typeof(SkillRecord), "get_TotallyDisabled")]
    public static class Patch_SkillRecord_TotallyDisabled
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        [HarmonyPostfix]
        public static void Postfix(SkillRecord __instance, ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            if (__instance?.def != SkillDefOf.Social)
            {
                return;
            }

            Pawn? pawn = AccessTools.Field(typeof(SkillRecord), "pawn")?.GetValue(__instance) as Pawn;
            if (pawn?.def?.defName == JusticeDefName)
            {
                __result = false;
            }
        }
    }
}
