using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    public class CompMechanitorMarker : ThingComp
    {
        public CompProperties_MechanitorMarker Props => (CompProperties_MechanitorMarker)props;

        public static bool PawnHasMarker(Pawn pawn)
        {
            return pawn != null && pawn.GetComp<CompMechanitorMarker>() != null;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Pawn? pawn = parent as Pawn;
            if (pawn?.mechanitor != null)
            {
                pawn.mechanitor.Notify_BandwidthChanged();
                AccessTools.Method(typeof(Pawn_MechanitorTracker), "Notify_ControlGroupAmountMayChanged")
                    ?.Invoke(pawn.mechanitor, null);
            }
        }
    }
}
