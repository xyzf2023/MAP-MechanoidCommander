using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MAP_MechCommander
{
    public class FloatMenuOptionProvider_UseChipForBandwidth : FloatMenuOptionProvider
    {
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
            if (clickedThing == null)
            {
                yield break;
            }

            if (!ChipBandwidthUtility.TryGetBandwidthPerChip(clickedThing.def, out int bandwidthPerChip))
            {
                yield break;
            }

            Pawn pawn = context.FirstSelectedPawn;
            if (pawn == null || !pawn.RaceProps.IsMechanoid)
            {
                yield break;
            }

            if (!BandwidthComponentUtility.TryGetBandwidthComponent(pawn, out IBandwidthComponent bandwidthComp))
            {
                yield break;
            }

            if (bandwidthComp.GetRemainingTotalBandwidth() <= 0)
            {
                yield break;
            }

            if (!pawn.CanReach(clickedThing, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                yield return new FloatMenuOption(GetUseOneLabel(clickedThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }

            if (!pawn.CanReserve(clickedThing))
            {
                yield return new FloatMenuOption(GetUseOneLabel(clickedThing) + ": " + "Reserved".Translate().CapitalizeFirst(), null);
                yield break;
            }

            if (clickedThing.stackCount <= 1)
            {
                yield return BuildUseOption(pawn, clickedThing, 1, GetUseOneLabel(clickedThing), bandwidthComp.MaxTotalBandwidth);
                yield break;
            }

            yield return BuildUseOption(pawn, clickedThing, 1, GetUseOneLabel(clickedThing), bandwidthComp.MaxTotalBandwidth);
            bool willWaste = bandwidthComp.GetRemainingTotalBandwidth() < bandwidthPerChip * clickedThing.stackCount;
            yield return BuildUseOption(pawn, clickedThing, clickedThing.stackCount, GetUseAllLabel(clickedThing), bandwidthComp.MaxTotalBandwidth, willWaste);
        }

        private static FloatMenuOption BuildUseOption(Pawn pawn, Thing chip, int count, string label, int maxBandwidth, bool confirmWaste = false)
        {
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, delegate
            {
                if (confirmWaste)
                {
                    TaggedString text = "MechCommander.Menu.IncreaseBandwidth.WasteConfirm"
                        .Translate(pawn.LabelShortCap, chip.LabelNoCount, maxBandwidth);
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate
                    {
                        IssueJob(pawn, chip, count);
                    }));
                }
                else
                {
                    IssueJob(pawn, chip, count);
                }
            }), pawn, chip, "ReservedBy", null);
        }

        private static void IssueJob(Pawn pawn, Thing chip, int count)
        {
            chip.SetForbidden(false, false);
            Job job = JobMaker.MakeJob(MechCommander_JobDefOf.MAP_UseChipForBandwidth, chip);
            job.count = count;
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        private static string GetUseOneLabel(Thing chip)
        {
            return "MechCommander.Menu.IncreaseBandwidth.UseOne".Translate(chip.LabelNoCount, chip);
        }

        private static string GetUseAllLabel(Thing chip)
        {
            return "MechCommander.Menu.IncreaseBandwidth.UseAll".Translate(chip.LabelNoCount, chip);
        }
    }
}
