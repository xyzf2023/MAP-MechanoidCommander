using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace MAP_MechCommander
{
    // 注入指挥官额外带宽/控制组数值
    [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
    public static class Patch_StatWorker_GetValueUnfinalized
    {
        [HarmonyPostfix]
        public static void Postfix(StatWorker __instance, StatRequest req, ref float __result)
        {
            if (req.Thing == null)
            {
                return;
            }

            Pawn? pawn = req.Thing as Pawn;
            if (pawn == null)
            {
                return;
            }

            StatDef? stat = AccessTools.Field(typeof(StatWorker), "stat")?.GetValue(__instance) as StatDef;
            if (stat == null)
            {
                return;
            }

            if (stat == StatDefOf.MechBandwidth)
            {
                CompMechanitorMarker marker = pawn.GetComp<CompMechanitorMarker>();
                if (marker != null)
                {
                    __result += marker.Props.extraMechBandwidth;
                }

                if (BandwidthComponentUtility.TryGetBandwidthComponent(pawn, out IBandwidthComponent bandwidthComp))
                {
                    __result += bandwidthComp.ExtraBandwidth;
                }
            }
            else if (stat == StatDefOf.PsychicSensitivity)
            {
                if (pawn.health?.hediffSet?.HasHediff(MechCommander_HediffDefOf.MAP_Mech_BandwidthLink) == true
                    || CompMechanitorMarker.PawnHasMarker(pawn))
                {
                    __result = Mathf.Max(__result, 1f);
                }
            }
            else if (stat == StatDefOf.MechControlGroups)
            {
                CompMechanitorMarker marker = pawn.GetComp<CompMechanitorMarker>();
                if (marker != null)
                {
                    __result += marker.Props.extraMechControlGroups;
                }

                if (BandwidthComponentUtility.GetBandwidthHediffComp(pawn) != null)
                {
                    __result += 3f;
                }
            }
        }
    }
}
