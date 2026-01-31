using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    /// <summary>
    /// 放行带机控标记的机械体（正义/隐者）使用原版修理机械体 WorkGiver。
    /// </summary>
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

    /// <summary>
    /// 禁止 WorkGiver_RepairMech 给机械体分配「修理自己」的工作（目标与执行者为同一 pawn 时无工作）。
    /// </summary>
    [HarmonyPatch(typeof(WorkGiver_RepairMech), "HasJobOnThing")]
    public static class Patch_WorkGiver_RepairMech_HasJobOnThing
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Thing t, ref bool __result)
        {
            if (!__result || pawn == null || t == null)
            {
                return;
            }

            // 目标与执行者为同一 pawn 时，不允许修理自己
            if (t is Pawn targetPawn && pawn == targetPawn)
            {
                __result = false;
            }
        }
    }
}
