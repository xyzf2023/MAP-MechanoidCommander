using RimWorld;
using UnityEngine;
using Verse;

namespace MAP_MechCommander
{
    public class HediffComp_MechBandwidth : HediffComp, IBandwidthComponent
    {
        private const int MaxTotalBandwidthValue = 100;
        private int extraBandwidth;

        public int ExtraBandwidth => extraBandwidth;
        public int MaxTotalBandwidth => MaxTotalBandwidthValue;

        public void AddBandwidth(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            int remaining = GetRemainingTotalBandwidth();
            if (remaining <= 0)
            {
                return;
            }

            if (amount > remaining)
            {
                amount = remaining;
            }

            extraBandwidth += amount;
            NotifyBandwidthChanged();
        }

        public int GetRemainingTotalBandwidth()
        {
            Pawn? pawn = Pawn;
            if (pawn == null)
            {
                return 0;
            }

            float currentTotal = pawn.GetStatValue(StatDefOf.MechBandwidth);
            int remaining = MaxTotalBandwidthValue - Mathf.RoundToInt(currentTotal);
            if (remaining < 0)
            {
                return 0;
            }

            return remaining;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref extraBandwidth, "extraBandwidth", 0);
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            Pawn? pawn = Pawn;
            if (pawn != null)
            {
                BandwidthComponentUtility.EnsureMechanitorTracker(pawn);
            }
        }

        private void NotifyBandwidthChanged()
        {
            Pawn? pawn = Pawn;
            if (pawn?.mechanitor != null)
            {
                pawn.mechanitor.Notify_BandwidthChanged();
            }
        }
    }

    public class HediffCompProperties_MechBandwidth : HediffCompProperties
    {
        public HediffCompProperties_MechBandwidth()
        {
            compClass = typeof(HediffComp_MechBandwidth);
        }
    }
}
