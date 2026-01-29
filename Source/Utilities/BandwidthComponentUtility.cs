using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    public interface IBandwidthComponent
    {
        int ExtraBandwidth { get; }
        int MaxTotalBandwidth { get; }
        void AddBandwidth(int amount);
        int GetRemainingTotalBandwidth();
    }

    public static class BandwidthComponentUtility
    {
        public static bool HasBandwidthComponent(Pawn pawn)
        {
            return TryGetBandwidthComponent(pawn, out _);
        }

        public static bool TryGetBandwidthComponent(Pawn pawn, out IBandwidthComponent component)
        {
            component = null!;
            if (pawn == null)
            {
                return false;
            }

            CompMechanitorBandwidth mechComp = pawn.GetComp<CompMechanitorBandwidth>();
            if (mechComp != null)
            {
                component = mechComp;
                return true;
            }

            HediffComp_MechBandwidth? hediffComp = GetBandwidthHediffComp(pawn);
            if (hediffComp != null)
            {
                component = hediffComp;
                return true;
            }

            return false;
        }

        public static HediffComp_MechBandwidth? GetBandwidthHediffComp(Pawn pawn)
        {
            List<Hediff>? hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs == null)
            {
                return null;
            }

            for (int i = 0; i < hediffs.Count; i++)
            {
                HediffComp_MechBandwidth comp = hediffs[i].TryGetComp<HediffComp_MechBandwidth>();
                if (comp != null)
                {
                    return comp;
                }
            }

            return null;
        }

        public static void EnsureMechanitorTracker(Pawn pawn)
        {
            if (pawn == null || !ModsConfig.BiotechActive)
            {
                return;
            }

            if (pawn.mechanitor == null)
            {
                pawn.mechanitor = new Pawn_MechanitorTracker(pawn);
            }

            if (pawn.relations == null)
            {
                pawn.relations = new Pawn_RelationsTracker(pawn);
            }

            pawn.mechanitor.Notify_PawnSpawned(true);
        }
    }
}
