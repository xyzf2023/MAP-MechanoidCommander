using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    // 放行指挥官的武器装备/放下菜单
    [HarmonyPatch(typeof(FloatMenuOptionProvider), "SelectedPawnValid")]
    public static class Patch_FloatMenuOptionProvider_SelectedPawnValid_Weapon
    {
        private static bool IsWeaponProvider(FloatMenuOptionProvider provider)
        {
            return provider is FloatMenuOptionProvider_Equip ||
                   provider is FloatMenuOptionProvider_DropEquipment;
        }

        private static bool CheckOtherRequirements(Traverse traverse, Pawn pawn)
        {
            try
            {
                bool drafted = traverse.Property("Drafted").GetValue<bool>();
                bool undrafted = traverse.Property("Undrafted").GetValue<bool>();
                bool requiresManipulation = traverse.Property("RequiresManipulation").GetValue<bool>();

                bool draftedOk = drafted || !pawn.Drafted;
                bool undraftedOk = undrafted || pawn.Drafted;
                bool manipulationOk = !requiresManipulation ||
                    (pawn.health?.capacities?.CapableOf(PawnCapacityDefOf.Manipulation) ?? false);

                return draftedOk && undraftedOk && manipulationOk;
            }
            catch
            {
                return false;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, FloatMenuContext context, ref bool __result, FloatMenuOptionProvider __instance)
        {
            if (__result || pawn == null || !CompMechanitorMarker.PawnHasMarker(pawn))
            {
                return;
            }

            if (!IsWeaponProvider(__instance))
            {
                return;
            }

            if (!pawn.RaceProps.IsMechanoid)
            {
                return;
            }

            var traverse = Traverse.Create(__instance);
            try
            {
                bool mechanoidCanDo = traverse.Property("MechanoidCanDo").GetValue<bool>();
                if (mechanoidCanDo)
                {
                    return;
                }

                if (CheckOtherRequirements(traverse, pawn))
                {
                    __result = true;
                }
            }
            catch
            {
                // ignore and keep vanilla result
            }
        }
    }

    // 允许指挥官打开装备页
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "CanControlColonist", MethodType.Getter)]
    public static class Patch_ITab_Pawn_Gear_CanControlColonist
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, ITab_Pawn_Gear __instance)
        {
            if (__result)
            {
                return;
            }

            Pawn pawn = Traverse.Create(__instance).Property("SelPawnForGear").GetValue<Pawn>();
            if (pawn != null && CompMechanitorMarker.PawnHasMarker(pawn))
            {
                __result = true;
            }
        }
    }
}
