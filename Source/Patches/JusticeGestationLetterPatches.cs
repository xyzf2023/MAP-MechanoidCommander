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
            if (pawn == null || pawn.def?.defName != JusticeDefName)
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
