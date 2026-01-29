using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    [HarmonyPatch(typeof(ITab_Pawn_Character), "get_IsVisible")]
    public static class Patch_ITab_Pawn_Character_IsVisible
    {
        [HarmonyPostfix]
        public static void Postfix(ITab_Pawn_Character __instance, ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            Pawn pawn = Traverse.Create(__instance).Property("SelPawn").GetValue<Pawn>();
            if (pawn == null)
            {
                Thing selThing = Traverse.Create(__instance).Property("SelThing").GetValue<Thing>();
                if (selThing is Corpse corpse)
                {
                    pawn = corpse.InnerPawn;
                }
            }

            if (pawn == null || !CompMechanitorMarker.PawnHasMarker(pawn))
            {
                return;
            }

            if (pawn.Faction != Faction.OfPlayer)
            {
                __result = false;
            }
        }
    }
}
