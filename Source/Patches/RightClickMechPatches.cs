using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MAP_MechCommander
{
    [HarmonyPatch]
    public static class RightClickMechPatches
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        private static bool IsJustice(Pawn? pawn)
        {
            return pawn?.def?.defName == JusticeDefName;
        }

        [HarmonyPatch(typeof(Pawn), "GetFloatMenuOptions")]
        [HarmonyPostfix]
        public static void Pawn_GetFloatMenuOptions_Postfix(Pawn __instance, Pawn selPawn, ref IEnumerable<FloatMenuOption> __result)
        {
            // 当右键自身时，移除来自 WorkGivers/Mechanitor 的修理选项（禁止自我修理）
            if (__instance == selPawn && selPawn != null && __result != null)
            {
                List<FloatMenuOption> list = __result.ToList();
                string selfLabel = __instance.LabelShort ?? "";
                list.RemoveAll(o =>
                {
                    if (o?.Label == null) return false;
                    string lab = o.Label.ToString();
                    return lab.Contains(selfLabel) && (lab.Contains("修理") || lab.Contains("Repair"));
                });
                __result = list;
            }
        }

        // 放行「正义」对物体的右键菜单（穿梭机等）
        [HarmonyPatch(typeof(FloatMenuOptionProvider), "SelectedPawnValid")]
        [HarmonyPostfix]
        public static void FloatMenuOptionProvider_SelectedPawnValid_Postfix(Pawn pawn, FloatMenuContext context, ref bool __result, FloatMenuOptionProvider __instance)
        {
            if (__result || pawn == null)
            {
                return;
            }

            if (!IsJustice(pawn))
            {
                return;
            }

            if (__instance is not FloatMenuOptionProvider_FromThing
                && __instance is not FloatMenuOptionProvider_WorkGivers
                && __instance is not FloatMenuOptionProvider_Trade)
            {
                return;
            }

            try
            {
                var traverse = Traverse.Create(__instance);
                bool drafted = traverse.Property("Drafted").GetValue<bool>();
                bool undrafted = traverse.Property("Undrafted").GetValue<bool>();
                bool requiresManipulation = traverse.Property("RequiresManipulation").GetValue<bool>();

                bool draftedOk = drafted || !pawn.Drafted;
                bool undraftedOk = undrafted || pawn.Drafted;
                bool manipulationOk = !requiresManipulation || (pawn.health?.capacities?.CapableOf(PawnCapacityDefOf.Manipulation) ?? false);

                if (draftedOk && undraftedOk && manipulationOk)
                {
                    __result = true;
                }
            }
            catch
            {
                // 保持原判定
            }
        }
    }
}
