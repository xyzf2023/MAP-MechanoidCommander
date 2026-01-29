using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    [DefOf]
    public static class MechCommander_JobDefOf
    {
        public static JobDef MAP_UseChipForBandwidth = null!;
        public static JobDef MAP_UseBandwidthLink = null!;

        static MechCommander_JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MechCommander_JobDefOf));
        }
    }
}
