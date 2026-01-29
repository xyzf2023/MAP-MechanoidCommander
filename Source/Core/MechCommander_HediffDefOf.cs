using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    [DefOf]
    public static class MechCommander_HediffDefOf
    {
        public static HediffDef MAP_Mech_BandwidthLink = null!;

        static MechCommander_HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MechCommander_HediffDefOf));
        }
    }
}
