using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MAP_MechCommander
{
    /// <summary>
    /// 仅当本次分配到的具体账单是「培育正义/隐者」时，禁止正义接单；
    /// 若培育器上还有其他账单（如培育镰刀机），正义仍可执行那些账单。
    /// </summary>
    [HarmonyPatch(typeof(WorkGiver_DoBill), "JobOnThing")]
    public static class Patch_WorkGiver_DoBill_JobOnThing
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref Job? __result)
        {
            if (__result == null || pawn == null)
            {
                return;
            }

            if (__result.bill is not Bill_Production bill || bill.recipe == null)
            {
                return;
            }

            if (MechGestatorRecipeUtility.IsJusticeDisabledForGestation(bill.recipe, pawn))
            {
                __result = null;
                // 若该账单被限制为「仅正义」执行，则改为「任意」，避免其他机械师永远接不到此单。
                // 解绑后 bill.PawnRestriction 变为 null，下次不会再满足 == pawn，故不会反复调用。
                if (bill.PawnRestriction == pawn)
                {
                    bill.SetAnyPawnRestriction();
                }
            }
        }
    }
}
