using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    public class CompCaravanOwner : ThingComp
    {
        public static bool PawnCanBeCaravanOwner(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            if (pawn.GetComp<CompCaravanOwner>() != null)
            {
                return true;
            }

            return BandwidthComponentUtility.HasBandwidthComponent(pawn);
        }
    }
}
