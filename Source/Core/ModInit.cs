using HarmonyLib;
using Verse;

namespace MAP_MechCommander
{
    [StaticConstructorOnStartup]
    public static class ModInit
    {
        static ModInit()
        {
            new Harmony("mech.commander").PatchAll();
        }
    }
}
