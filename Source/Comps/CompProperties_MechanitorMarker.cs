using Verse;

namespace MAP_MechCommander
{
    public class CompProperties_MechanitorMarker : CompProperties
    {
        public int extraMechBandwidth = 0;
        public int extraMechControlGroups = 0;

        public CompProperties_MechanitorMarker()
        {
            compClass = typeof(CompMechanitorMarker);
        }
    }
}
