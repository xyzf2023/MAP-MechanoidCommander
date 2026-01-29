using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    [HarmonyPatch(typeof(JobGiver_Work), "PawnCanUseWorkGiver")]
    public static class Patch_JobGiver_Work_PawnCanUseWorkGiver
    {
        private const string RepairMechDefName = "RepairMech";

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, WorkGiver giver, ref bool __result)
        {
            if (__result || pawn == null || giver?.def == null)
            {
                return;
            }

            if (!CompMechanitorMarker.PawnHasMarker(pawn))
            {
                return;
            }

            if (giver.def.defName != RepairMechDefName)
            {
                return;
            }

            bool baseConditions =
                (giver.def.nonColonistsCanDo || pawn.IsColonist || pawn.IsColonyMech || pawn.IsColonySubhuman) &&
                !pawn.WorkTagIsDisabled(giver.def.workTags) &&
                (giver.def.workType == null || !pawn.WorkTypeIsDisabled(giver.def.workType)) &&
                !giver.ShouldSkip(pawn, false) &&
                giver.MissingRequiredCapacity(pawn) == null;

            if (baseConditions)
            {
                __result = true;
            }
        }
    }
}
