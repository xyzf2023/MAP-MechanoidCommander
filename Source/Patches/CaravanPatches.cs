using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MAP_MechCommander
{
    // 允许指挥官成为远行队Owner
    [HarmonyPatch(typeof(CaravanUtility), "IsOwner")]
    public static class Patch_CaravanUtility_IsOwner
    {
        [HarmonyPostfix]
        public static void Postfix(ref bool __result, Pawn pawn, Faction caravanFaction)
        {
            if (__result)
            {
                return;
            }

            if (pawn == null || caravanFaction == null)
            {
                return;
            }

            if (pawn.Faction != caravanFaction)
            {
                return;
            }

            if (CompCaravanOwner.PawnCanBeCaravanOwner(pawn))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_FormCaravan), "TrySend")]
    public static class Patch_Dialog_FormCaravan_TrySend
    {
        public struct TempState
        {
            public List<Pawn>? StoryAdded;
            public List<Pawn>? SkillsAdded;
        }

        [HarmonyPrefix]
        public static void Prefix(Dialog_FormCaravan __instance, ref TempState __state)
        {
            __state.StoryAdded = new List<Pawn>();
            __state.SkillsAdded = new List<Pawn>();

            List<TransferableOneWay> transferables =
                Traverse.Create(__instance).Field("transferables").GetValue<List<TransferableOneWay>>();
            if (transferables == null)
            {
                return;
            }

            List<Pawn> pawns = TransferableUtility.GetPawnsFromTransferables(transferables);
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn == null || pawn.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                if (!CompCaravanOwner.PawnCanBeCaravanOwner(pawn)
                    && !CompMechanitorMarker.PawnHasMarker(pawn)
                    && !BandwidthComponentUtility.HasBandwidthComponent(pawn))
                {
                    continue;
                }

                if (pawn.story == null)
                {
                    pawn.story = new Pawn_StoryTracker(pawn);
                    __state.StoryAdded.Add(pawn);
                }

                if (pawn.skills == null)
                {
                    pawn.skills = new Pawn_SkillTracker(pawn);
                    __state.SkillsAdded.Add(pawn);
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix(ref TempState __state)
        {
            if (__state.SkillsAdded != null)
            {
                for (int i = 0; i < __state.SkillsAdded.Count; i++)
                {
                    Pawn pawn = __state.SkillsAdded[i];
                    if (pawn != null)
                    {
                        pawn.skills = null;
                    }
                }
            }

            if (__state.StoryAdded != null)
            {
                for (int i = 0; i < __state.StoryAdded.Count; i++)
                {
                    Pawn pawn = __state.StoryAdded[i];
                    if (pawn != null)
                    {
                        pawn.story = null;
                    }
                }
            }
        }
    }
}
