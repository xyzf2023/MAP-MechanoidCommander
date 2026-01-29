using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    [HarmonyPatch(typeof(MainTabWindow_Work), "get_Pawns")]
    public static class Patch_MainTabWindow_Work_Pawns
    {
        private const string JusticeDefName = "MAP_Mech_Justice";

        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<Pawn> __result)
        {
            if (__result == null)
            {
                return;
            }

            MechCommanderModSettings? settings = MechCommanderModSettingsWindow.Settings;
            if (settings == null || !settings.addJusticeToWorkTab)
            {
                return;
            }

            Map? map = Find.CurrentMap;
            if (map == null)
            {
                return;
            }

            List<Pawn> pawns = __result.ToList();
            HashSet<Pawn> existing = new HashSet<Pawn>(pawns);

            List<Pawn> justicePawns = map.mapPawns.AllPawnsSpawned
                .Where(p => p.def != null
                    && p.def.defName == JusticeDefName
                    && p.Faction == Faction.OfPlayer
                    && !p.Dead
                    && !p.DevelopmentalStage.Baby())
                .ToList();

            for (int i = 0; i < justicePawns.Count; i++)
            {
                Pawn pawn = justicePawns[i];
                if (pawn.workSettings == null)
                {
                    pawn.workSettings = new Pawn_WorkSettings(pawn);
                    pawn.workSettings.EnableAndInitialize();
                }
                else if (!pawn.workSettings.Initialized)
                {
                    pawn.workSettings.EnableAndInitialize();
                }
                if (existing.Add(pawn))
                {
                    pawns.Add(pawn);
                }
            }

            __result = pawns;
        }
    }
}
