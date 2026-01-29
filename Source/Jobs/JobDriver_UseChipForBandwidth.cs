using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MAP_MechCommander
{
    public class JobDriver_UseChipForBandwidth : JobDriver
    {
        private const int UseDurationTicks = 180;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, job.count, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => pawn == null || !BandwidthComponentUtility.HasBandwidthComponent(pawn));

            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return GotoAdjacentToTarget(TargetIndex.A);

            Toil wait = Toils_General.Wait(UseDurationTicks);
            wait.WithProgressBarToilDelay(TargetIndex.A);
            wait.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            wait.tickAction = delegate
            {
                Thing thing = job.targetA.Thing;
                if (thing != null)
                {
                    pawn.rotationTracker.FaceTarget(thing);
                }
            };
            yield return wait;

            Toil apply = new Toil();
            apply.initAction = delegate
            {
                Thing chip = job.targetA.Thing;
                if (chip == null || chip.Destroyed)
                {
                    return;
                }

                if (!ChipBandwidthUtility.TryGetBandwidthPerChip(chip.def, out int bandwidthPerChip))
                {
                    return;
                }

                int count = job.count > 0 ? job.count : 1;
                int useCount = count;
                if (chip.stackCount < useCount)
                {
                    useCount = chip.stackCount;
                }

                if (useCount <= 0)
                {
                    return;
                }

                if (!BandwidthComponentUtility.TryGetBandwidthComponent(pawn, out IBandwidthComponent bandwidthComp))
                {
                    return;
                }

                int remaining = bandwidthComp.GetRemainingTotalBandwidth();
                if (remaining <= 0)
                {
                    return;
                }

                int addAmount = bandwidthPerChip * useCount;
                if (addAmount > remaining)
                {
                    addAmount = remaining;
                }

                if (addAmount > 0)
                {
                    bandwidthComp.AddBandwidth(addAmount);
                    chip.SplitOff(useCount).Destroy(DestroyMode.Vanish);
                }
            };
            apply.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return apply;
        }

        private Toil GotoAdjacentToTarget(TargetIndex targetIndex)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Thing thing = job.GetTarget(targetIndex).Thing;
                if (thing == null || pawn.Map == null)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                    return;
                }

                IntVec3 cell = FindBestAdjacentCell(thing.Position, pawn);
                pawn.pather.StartPath(new LocalTargetInfo(cell), PathEndMode.OnCell);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            toil.FailOnDespawnedNullOrForbidden(targetIndex);
            return toil;
        }

        private static IntVec3 FindBestAdjacentCell(IntVec3 targetCell, Pawn pawn)
        {
            Map map = pawn.Map;
            IntVec3 bestCell = targetCell;
            int bestDistance = int.MaxValue;
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(new TargetInfo(targetCell, map)))
            {
                if (!cell.InBounds(map) || !cell.Standable(map))
                {
                    continue;
                }

                int dist = cell.DistanceToSquared(pawn.Position);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestCell = cell;
                }
            }

            return bestCell;
        }
    }
}
