using RimWorld;
using UnityEngine;
using Verse;

namespace MAP_MechCommander
{
    public class CompMechanitorBandwidth : ThingComp, IBandwidthComponent
    {
        public const int MaxTotalBandwidth = 150;
        private int extraBandwidth;

        public int ExtraBandwidth => extraBandwidth;
        int IBandwidthComponent.MaxTotalBandwidth => MaxTotalBandwidth;

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
            Pawn? pawn = parent as Pawn;
            if (pawn == null)
            {
                return 0;
            }

            float currentTotal = pawn.GetStatValue(StatDefOf.MechBandwidth);
            int remaining = MaxTotalBandwidth - Mathf.RoundToInt(currentTotal);
            if (remaining < 0)
            {
                return 0;
            }

            return remaining;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref extraBandwidth, "extraBandwidth", 0);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            NotifyBandwidthChanged();
        }

        private void NotifyBandwidthChanged()
        {
            Pawn? pawn = parent as Pawn;
            if (pawn?.mechanitor != null)
            {
                pawn.mechanitor.Notify_BandwidthChanged();
            }
        }
    }

    public class CompProperties_MechanitorBandwidth : CompProperties
    {
        public CompProperties_MechanitorBandwidth()
        {
            compClass = typeof(CompMechanitorBandwidth);
        }
    }
}
