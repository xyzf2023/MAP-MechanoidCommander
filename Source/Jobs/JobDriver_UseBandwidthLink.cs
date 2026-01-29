using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MAP_MechCommander
{
    public class JobDriver_UseBandwidthLink : JobDriver
    {
        private const int UseDurationTicks = 600;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => pawn == null || !pawn.RaceProps.IsMechanoid);
            this.FailOn(() => pawn == null || BandwidthComponentUtility.HasBandwidthComponent(pawn));
            this.FailOn(() => pawn == null || pawn.IsColonyMechRequiringMechanitor());
            this.FailOn(() => pawn != null && pawn.GetOverseer() != null && pawn.GetOverseer().RaceProps.IsMechanoid);

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
                Thing item = job.targetA.Thing;
                if (item == null || item.Destroyed)
                {
                    return;
                }

                if (pawn?.health == null || BandwidthComponentUtility.HasBandwidthComponent(pawn))
                {
                    return;
                }

                Hediff hediff = HediffMaker.MakeHediff(MechCommander_HediffDefOf.MAP_Mech_BandwidthLink, pawn);
                pawn.health.AddHediff(hediff);
                HediffComp_MechBandwidth? comp = hediff.TryGetComp<HediffComp_MechBandwidth>();
                comp?.AddBandwidth(6);
                BandwidthComponentUtility.EnsureMechanitorTracker(pawn);
                item.Destroy(DestroyMode.Vanish);

                TaggedString label = "MechCommander.Letter.BandwidthLinkInstalled.Label".Translate();
                TaggedString body = "MechCommander.Letter.BandwidthLinkInstalled.Body".Translate(pawn.Named("PAWN"));
                Find.LetterStack.ReceiveLetter(label, body, LetterDefOf.PositiveEvent, pawn);
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
