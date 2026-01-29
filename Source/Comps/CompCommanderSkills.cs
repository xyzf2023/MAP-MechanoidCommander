using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MAP_MechCommander
{
    public class CompCommanderSkills : ThingComp
    {
        private bool skillsInitialized;
        private int lastSkillSyncTick = -1;
        private int lastOverseerId = -1;

        public CompProperties_CommanderSkills Props => (CompProperties_CommanderSkills)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            EnsureStoryAndSkills(respawningAfterLoad);
            InitializeSyncTick();
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            EnsureStoryAndSkills(false);
            InitializeSyncTick();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref skillsInitialized, "skillsInitialized", false);
            Scribe_Values.Look(ref lastSkillSyncTick, "lastSkillSyncTick", -1);
            Scribe_Values.Look(ref lastOverseerId, "lastOverseerId", -1);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                EnsureStoryAndSkills(true);
                InitializeSyncTick();
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            TrySyncSkillsWithOverseer();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (!DebugSettings.ShowDevGizmos)
            {
                yield break;
            }

            if (parent is not Pawn pawn || !CompMechanitorMarker.PawnHasMarker(pawn))
            {
                yield break;
            }

            yield return new Command_Action
            {
                defaultLabel = "MechCommander.Dev.SyncJusticeSkills".Translate(),
                action = ForceSyncSkillsOnce
            };
        }

        private void EnsureStoryAndSkills(bool respawningAfterLoad)
        {
            if (parent is not Pawn pawn)
            {
                return;
            }

            EnsureWorkSettings(pawn);

            if (pawn.story == null)
            {
                pawn.story = new Pawn_StoryTracker(pawn);
            }

            if (pawn.story.bodyType == null)
            {
                pawn.story.bodyType = Props.bodyType ?? BodyTypeDefOf.Male;
            }

            if (pawn.skills == null)
            {
                InitializeSkills(pawn);
            }
            else if (!skillsInitialized || respawningAfterLoad)
            {
                EnsureSkillsCorrect(pawn);
            }
        }

        private void InitializeSyncTick()
        {
            if (lastSkillSyncTick < 0 && Find.TickManager != null)
            {
                lastSkillSyncTick = Find.TickManager.TicksGame;
            }

            if (lastOverseerId < 0)
            {
                UpdateOverseerId();
                SyncSkillsWithOverseer();
            }
        }

        private void TrySyncSkillsWithOverseer()
        {
            if (lastSkillSyncTick < 0 || Find.TickManager == null)
            {
                return;
            }

            if (UpdateOverseerId())
            {
                lastSkillSyncTick = Find.TickManager.TicksGame;
                SyncSkillsWithOverseer();
                return;
            }

            if (Find.TickManager.TicksGame - lastSkillSyncTick < 60000)
            {
                return;
            }

            lastSkillSyncTick = Find.TickManager.TicksGame;
            SyncSkillsWithOverseer();
        }

        private void InitializeSkills(Pawn pawn)
        {
            if (pawn.skills == null)
            {
                pawn.skills = new Pawn_SkillTracker(pawn);
            }

            ApplySkillLevels(pawn);
            skillsInitialized = true;
        }

        private void EnsureWorkSettings(Pawn pawn)
        {
            if (pawn.workSettings == null)
            {
                pawn.workSettings = new Pawn_WorkSettings(pawn);
                pawn.workSettings.EnableAndInitialize();
            }

            if (pawn.workSettings.EverWork && !pawn.WorkTypeIsDisabled(WorkTypeDefOf.Smithing))
            {
                if (pawn.workSettings.GetPriority(WorkTypeDefOf.Smithing) == 0)
                {
                    pawn.workSettings.SetPriority(WorkTypeDefOf.Smithing, 3);
                }
            }
        }

        private void EnsureSkillsCorrect(Pawn pawn)
        {
            if (pawn.skills == null)
            {
                return;
            }

            ApplySkillLevels(pawn);
            skillsInitialized = true;
        }

        private void ApplySkillLevels(Pawn pawn)
        {
            List<SkillDef> allSkills = DefDatabase<SkillDef>.AllDefsListForReading;
            for (int i = 0; i < allSkills.Count; i++)
            {
                SkillDef skillDef = allSkills[i];
                SkillRecord skill = pawn.skills.GetSkill(skillDef);
                if (skill != null)
                {
                    skill.Level = 0;
                    skill.xpSinceLastLevel = 0f;
                    skill.xpSinceMidnight = 0f;
                    skill.passion = Passion.None;
                }
            }

            if (Props.skillLevels.NullOrEmpty())
            {
                return;
            }

            for (int i = 0; i < Props.skillLevels.Count; i++)
            {
                SkillLevelEntry entry = Props.skillLevels[i];
                if (entry?.skill == null)
                {
                    continue;
                }

                SkillRecord skill = pawn.skills.GetSkill(entry.skill);
                if (skill != null)
                {
                    skill.Level = entry.level;
                    skill.xpSinceLastLevel = 0f;
                    skill.xpSinceMidnight = 0f;
                    skill.passion = entry.passion;
                }
            }
        }

        private void SyncSkillsWithOverseer()
        {
            if (parent is not Pawn pawn)
            {
                return;
            }

            if (pawn.skills == null || Props.skillLevels.NullOrEmpty())
            {
                return;
            }

            for (int i = 0; i < Props.skillLevels.Count; i++)
            {
                SkillLevelEntry entry = Props.skillLevels[i];
                if (entry?.skill == null)
                {
                    continue;
                }

                int desiredLevel = entry.level;
                int mechSkill = pawn.RaceProps.mechFixedSkillLevel;
                if (mechSkill > desiredLevel)
                {
                    desiredLevel = mechSkill;
                }

                SkillRecord commanderSkill = pawn.skills.GetSkill(entry.skill);
                if (commanderSkill == null)
                {
                    continue;
                }

                // 生成阶段 workSettings 可能为空，避免读取 Level 触发 NRE
                commanderSkill.Level = desiredLevel;
                commanderSkill.xpSinceLastLevel = 0f;
                commanderSkill.xpSinceMidnight = 0f;
            }
        }

        private void ForceSyncSkillsOnce()
        {
            if (Find.TickManager != null)
            {
                lastSkillSyncTick = Find.TickManager.TicksGame;
            }

            SyncSkillsWithOverseer();
        }

        private bool UpdateOverseerId()
        {
            if (parent is not Pawn pawn)
            {
                return false;
            }

            Pawn overseer = pawn.GetOverseer();
            int currentId = overseer?.thingIDNumber ?? -1;
            if (currentId < 0)
            {
                return false;
            }
            if (currentId != lastOverseerId)
            {
                lastOverseerId = currentId;
                return true;
            }

            return false;
        }

    }
}
