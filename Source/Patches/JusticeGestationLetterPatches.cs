using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    [HarmonyPatch(typeof(Bill_ProductionMech), "CreateProducts")]
    public static class Patch_Bill_ProductionMech_CreateProducts_JusticeLetter
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        [HarmonyPostfix]
        public static void Postfix(ref Thing __result)
        {
            Pawn? pawn = __result as Pawn;
            if (pawn == null)
            {
                return;
            }

            // 培育完成时立即限制工作类型：PawnGenerator.GeneratePawn 会在 PostPostMake 之后再次调用 EnableAndInitialize()，导致 MechTab/WorkTab 显示全工作
            MechWorkSettingsUtility.RestrictToMechEnabledWorkTypes(pawn);

            if (pawn.def?.defName != JusticeDefName)
            {
                return;
            }

            if (pawn.Faction != Faction.OfPlayer)
            {
                return;
            }

            JusticeGestationTracker? tracker = Current.Game?.GetComponent<JusticeGestationTracker>();
            if (tracker == null || tracker.LetterSent)
            {
                return;
            }

            TaggedString label = "MechCommander.Letter.JusticeGestated.Label".Translate();
            TaggedString body = "MechCommander.Letter.JusticeGestated.Body".Translate(pawn.Named("PAWN"));
            Find.LetterStack.ReceiveLetter(label, body, LetterDefOf.NeutralEvent, pawn);

            tracker.MarkSent();
        }
    }
}
