using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    [HarmonyPatch(typeof(CompUseEffect_CallBossgroup), "CanBeUsedBy")]
    public static class Patch_CompUseEffect_CallBossgroup_CanBeUsedBy
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref AcceptanceReport __result)
        {
            if (__result.Accepted || p == null)
            {
                return;
            }

            if (CompMechanitorMarker.PawnHasMarker(p))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(CompUsable), "CanBeUsedBy")]
    public static class Patch_CompUsable_CanBeUsedBy_Bossgroup
    {
        [HarmonyPostfix]
        public static void Postfix(CompUsable __instance, Pawn p, ref AcceptanceReport __result)
        {
            if (__result.Accepted || p == null)
            {
                return;
            }

            if (!__result.Reason.NullOrEmpty())
            {
                return;
            }

            if (!CompMechanitorMarker.PawnHasMarker(p))
            {
                return;
            }

            CompUseEffect_CallBossgroup? bossgroupComp = __instance?.parent?.GetComp<CompUseEffect_CallBossgroup>();
            if (bossgroupComp == null)
            {
                return;
            }

            AcceptanceReport report = bossgroupComp.CanBeUsedBy(p);
            if (!report.Accepted && report.Reason.NullOrEmpty())
            {
                report = "RequiresMechanitor".Translate();
            }

            __result = report;
        }
    }
}
