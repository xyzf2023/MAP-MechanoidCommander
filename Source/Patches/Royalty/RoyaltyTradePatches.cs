using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    [HarmonyPatch(typeof(FactionUtility), "CanTradeWith")]
    public static class Patch_FactionUtility_CanTradeWith
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        [HarmonyPostfix]
        public static void Postfix(Pawn p, Faction faction, TraderKindDef traderKind, ref AcceptanceReport __result)
        {
            if (__result.Accepted)
            {
                return;
            }

            if (!ModsConfig.RoyaltyActive)
            {
                return;
            }

            if (p?.def?.defName != JusticeDefName)
            {
                return;
            }

            if (traderKind?.permitRequiredForTrading == null || faction == null)
            {
                return;
            }

            Pawn overseer = p.GetOverseer();
            if (overseer?.royalty == null)
            {
                return;
            }

            if (overseer.royalty.HasPermit(traderKind.permitRequiredForTrading, faction))
            {
                __result = AcceptanceReport.WasAccepted;
            }
        }
    }
}
