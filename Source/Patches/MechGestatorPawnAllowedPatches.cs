using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MAP_MechCommander
{
    [HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
    public static class Patch_WorkGiver_DoBill_JobOnThing
    {
        [HarmonyPrefix]
        public static bool Prefix(WorkGiver_DoBill __instance, Pawn pawn, Thing thing, bool forced, ref Job? __result)
        {
            if (pawn == null || thing == null)
            {
                return true;
            }

            if (!(thing is Building_MechGestator gestator))
            {
                return true;
            }

            if (gestator.billStack == null)
            {
                return true;
            }

            foreach (Bill_Production bill in gestator.billStack.Bills)
            {
                if (bill.recipe != null && MechGestatorRecipeUtility.IsJusticeDisabledForGestation(bill.recipe, pawn))
                {
                    __result = null;
                    return false;
                }
            }

            return true;
        }
    }
}
