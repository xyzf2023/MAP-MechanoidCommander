using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    /// <summary>
    /// 放行正义/隐者零优先级时仍显示铸造相关 WorkGiver 右键选项（Transpiler）。
    /// </summary>
    [HarmonyPatch(typeof(FloatMenuOptionProvider_WorkGivers), "GetWorkGiverOption")]
    public static class Patch_FloatMenuOptionProvider_WorkGivers_GetWorkGiverOption
    {
        private const string JusticeDefName = "MAP_Mech_Justice";
        private const string RepairMechDefName = "RepairMech";

        private static bool AllowZeroPriority(Pawn pawn, WorkTypeDef workType)
        {
            if (pawn?.def?.defName != JusticeDefName || workType == null)
            {
                return false;
            }

            return workType == WorkTypeDefOf.Crafting || workType == WorkTypeDefOf.Smithing;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            MethodInfo getPriority = AccessTools.Method(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.GetPriority));
            MethodInfo allowZeroPriority = AccessTools.Method(typeof(Patch_FloatMenuOptionProvider_WorkGivers_GetWorkGiverOption), nameof(AllowZeroPriority));

            for (int i = 0; i < codes.Count - 2; i++)
            {
                CodeInstruction first = codes[i];
                if (first.opcode == OpCodes.Callvirt && (MethodInfo)first.operand == getPriority
                    && codes[i + 1].opcode == OpCodes.Ldc_I4_0
                    && codes[i + 2].opcode == OpCodes.Ceq)
                {
                    CodeInstruction loadWorkType = codes[i - 1];
                    if (!IsLoadLocal(loadWorkType.opcode))
                    {
                        break;
                    }

                    codes.InsertRange(i + 3, new[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(loadWorkType.opcode, loadWorkType.operand),
                        new CodeInstruction(OpCodes.Call, allowZeroPriority),
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Ceq),
                        new CodeInstruction(OpCodes.And)
                    });
                    break;
                }
            }

            return codes;
        }

        private static bool IsLoadLocal(OpCode opcode)
        {
            return opcode == OpCodes.Ldloc_0
                || opcode == OpCodes.Ldloc_1
                || opcode == OpCodes.Ldloc_2
                || opcode == OpCodes.Ldloc_3
                || opcode == OpCodes.Ldloc
                || opcode == OpCodes.Ldloc_S;
        }
    }

    /// <summary>
    /// 拦截铸造相关右键：当 WorkGiver 为修理机械体且目标与选中 pawn 为同一人时，不添加修理选项（禁止自我修理）。
    /// 原版签名：GetWorkGiverOption(Pawn pawn, WorkGiverDef workGiver, LocalTargetInfo target, FloatMenuContext context)
    /// </summary>
    [HarmonyPatch(typeof(FloatMenuOptionProvider_WorkGivers), "GetWorkGiverOption")]
    public static class Patch_FloatMenuOptionProvider_WorkGivers_BlockRepairSelf
    {
        private const string RepairMechDefName = "RepairMech";

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, WorkGiverDef workGiver, LocalTargetInfo target, ref FloatMenuOption? __result)
        {
            if (__result == null || workGiver == null || pawn == null)
            {
                return;
            }

            if (workGiver.defName != RepairMechDefName)
            {
                return;
            }

            // 目标与执行者为同一 pawn 时，不显示修理选项
            if (target.HasThing && target.Thing is Pawn targetPawn && targetPawn == pawn)
            {
                __result = null;
            }
        }
    }
}
