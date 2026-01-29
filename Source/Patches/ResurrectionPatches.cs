using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    // 机械体构造器复活会清除所有Hediff，需要补回带宽协调模块
    [HarmonyPatch(typeof(Bill_ResurrectMech), "CreateProducts")]
    public static class Patch_Bill_ResurrectMech_CreateProducts
    {
        public struct RestoreState
        {
            public bool hadBandwidthLink;
            public int extraBandwidth;
        }

        [HarmonyPrefix]
        public static void Prefix(Bill_ResurrectMech __instance, ref RestoreState __state)
        {
            __state = default;
            Pawn? pawn = __instance?.Gestator?.ResurrectingMechCorpse?.InnerPawn;
            if (pawn == null)
            {
                return;
            }

            HediffComp_MechBandwidth? comp = BandwidthComponentUtility.GetBandwidthHediffComp(pawn);
            if (comp == null)
            {
                return;
            }

            __state.hadBandwidthLink = true;
            __state.extraBandwidth = comp.ExtraBandwidth;
        }

        [HarmonyPostfix]
        public static void Postfix(Thing __result, RestoreState __state)
        {
            if (!__state.hadBandwidthLink)
            {
                return;
            }

            Pawn? pawn = __result as Pawn;
            if (pawn?.health == null)
            {
                return;
            }

            if (pawn.health.hediffSet.HasHediff(MechCommander_HediffDefOf.MAP_Mech_BandwidthLink))
            {
                return;
            }

            Hediff hediff = HediffMaker.MakeHediff(MechCommander_HediffDefOf.MAP_Mech_BandwidthLink, pawn);
            pawn.health.AddHediff(hediff);
            HediffComp_MechBandwidth? comp = hediff.TryGetComp<HediffComp_MechBandwidth>();
            if (comp != null && __state.extraBandwidth > 0)
            {
                comp.AddBandwidth(__state.extraBandwidth);
            }
            BandwidthComponentUtility.EnsureMechanitorTracker(pawn);
        }
    }
}
