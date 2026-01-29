using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MAP_MechCommander
{
    public class FloatMenuOptionProvider_UseBandwidthLink : FloatMenuOptionProvider
    {
        private const string BandwidthLinkDefName = "MAP_MechBandwidthLink";

        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;
        protected override bool MechanoidCanDo => true;

        protected override bool AppliesInt(FloatMenuContext context)
        {
            return context.FirstSelectedPawn != null;
        }

        public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
        {
            if (clickedThing?.def == null || clickedThing.def.defName != BandwidthLinkDefName)
            {
                yield break;
            }

            Pawn pawn = context.FirstSelectedPawn;
            if (pawn == null || !pawn.RaceProps.IsMechanoid || pawn.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            if (BandwidthComponentUtility.HasBandwidthComponent(pawn))
            {
                yield break;
            }

            if (pawn.IsColonyMechRequiringMechanitor())
            {
                yield break;
            }

            Pawn overseer = pawn.GetOverseer();
            if (overseer != null && overseer.RaceProps.IsMechanoid)
            {
                yield return new FloatMenuOption(GetUseLabel(clickedThing) + ": " +
                    "MechCommander.Menu.BandwidthLink.BlockedByMechOverseer".Translate().CapitalizeFirst(), null);
                yield break;
            }

            if (!pawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                yield return new FloatMenuOption(GetUseLabel(clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }

            if (!pawn.CanReserve(clickedThing))
            {
                yield return new FloatMenuOption(GetUseLabel(clickedThing) + ": " + "Reserved".Translate().CapitalizeFirst(), null);
                yield break;
            }

            yield return BuildUseOption(pawn, clickedThing, GetUseLabel(clickedThing));
        }

        private static FloatMenuOption BuildUseOption(Pawn pawn, Thing item, string label)
        {
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, delegate
            {
                item.SetForbidden(false, false);
                Job job = JobMaker.MakeJob(MechCommander_JobDefOf.MAP_UseBandwidthLink, item);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }), pawn, item, "ReservedBy", null);
        }

        private static string GetUseLabel(Thing item)
        {
            return "MechCommander.Menu.BandwidthLink.Use".Translate(item.LabelNoCount, item);
        }
    }
}
