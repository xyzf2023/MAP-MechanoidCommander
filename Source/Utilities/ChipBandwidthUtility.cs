using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    public static class ChipBandwidthUtility
    {
        private const string SignalChipDefName = "SignalChip";
        private const string PowerfocusChipDefName = "PowerfocusChip";
        private const string NanostructuringChipDefName = "NanostructuringChip";

        public static bool TryGetBandwidthPerChip(ThingDef def, out int bandwidth)
        {
            bandwidth = 0;
            if (def == null)
            {
                return false;
            }

            switch (def.defName)
            {
                case SignalChipDefName:
                    bandwidth = 5;
                    return true;
                case PowerfocusChipDefName:
                    bandwidth = 10;
                    return true;
                case NanostructuringChipDefName:
                    bandwidth = 15;
                    return true;
                default:
                    return false;
            }
        }
    }
}
