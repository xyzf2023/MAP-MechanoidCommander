using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace MAP_MechCommander
{
    public class MechCommanderModSettingsWindow : Mod
    {
        public static MechCommanderModSettings? Settings;
        private Vector2 availableScroll;
        private Vector2 blacklistedScroll;
        private const string AllFilterId = "ALL";
        private const string UnknownFilterId = "UNKNOWN";
        private string leftFilterPackId = AllFilterId;
        private string rightFilterPackId = AllFilterId;

        public MechCommanderModSettingsWindow(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MechCommanderModSettings>();
            Settings.CleanupForCurrentDefs();
        }

        public override string SettingsCategory()
        {
            return "MechCommander.Settings.Category".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (Settings == null)
            {
                return;
            }

            Settings.CleanupForCurrentDefs();

            List<HediffDef> candidates = JusticeHediffSyncUtility.GetCandidateHediffDefsSorted();
            Dictionary<string, string> packLabels = BuildPackLabels(candidates);

            if (leftFilterPackId != AllFilterId && !packLabels.ContainsKey(leftFilterPackId))
            {
                leftFilterPackId = AllFilterId;
            }
            if (rightFilterPackId != AllFilterId && !packLabels.ContainsKey(rightFilterPackId))
            {
                rightFilterPackId = AllFilterId;
            }

            List<HediffDef> available = candidates
                .Where(d => !JusticeHediffSyncUtility.IsBlacklisted(d))
                .Where(d => leftFilterPackId == AllFilterId || GetPackId(d) == leftFilterPackId)
                .ToList();

            List<HediffDef> blacklisted = candidates
                .Where(d => JusticeHediffSyncUtility.IsBlacklisted(d))
                .Where(d => rightFilterPackId == AllFilterId || GetPackId(d) == rightFilterPackId)
                .ToList();

            float headerHeight = 28f;
            Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, headerHeight);
            Widgets.Label(headerRect, "MechCommander.Settings.Header".Translate());

            float toggleHeight = 24f;
            Rect toggleRect = new Rect(inRect.x, headerRect.yMax + 4f, inRect.width, toggleHeight);
            bool addJusticeToWorkTab = Settings.addJusticeToWorkTab;
            Widgets.CheckboxLabeled(toggleRect, "MechCommander.Settings.AddJusticeToWorkTab".Translate(), ref addJusticeToWorkTab);
            TooltipHandler.TipRegion(toggleRect, "MechCommander.Settings.AddJusticeToWorkTabTip".Translate());
            if (addJusticeToWorkTab != Settings.addJusticeToWorkTab)
            {
                Settings.addJusticeToWorkTab = addJusticeToWorkTab;
            }

            Rect contentRect = new Rect(
                inRect.x,
                toggleRect.yMax + 8f,
                inRect.width,
                inRect.height - (toggleRect.yMax - inRect.y) - 12f);
            float columnWidth = (contentRect.width - 10f) / 2f;

            Rect leftRect = new Rect(contentRect.x, contentRect.y, columnWidth, contentRect.height);
            Rect rightRect = new Rect(contentRect.x + columnWidth + 10f, contentRect.y, columnWidth, contentRect.height);

            DrawListPanel(
                leftRect,
                "MechCommander.Settings.AllowedTitle".Translate(),
                available,
                "MechCommander.Settings.ToBlacklist".Translate(),
                def => Settings.SetBlacklisted(def, true),
                ref availableScroll,
                packLabels,
                leftFilterPackId,
                id => leftFilterPackId = id);

            DrawListPanel(
                rightRect,
                "MechCommander.Settings.BlockedTitle".Translate(),
                blacklisted,
                "MechCommander.Settings.FromBlacklist".Translate(),
                def => Settings.SetBlacklisted(def, false),
                ref blacklistedScroll,
                packLabels,
                rightFilterPackId,
                id => rightFilterPackId = id);

            Settings.Write();
            base.DoSettingsWindowContents(inRect);
        }

        private static void DrawListPanel(
            Rect rect,
            string title,
            List<HediffDef> items,
            string buttonLabel,
            System.Action<HediffDef> onClick,
            ref Vector2 scroll,
            Dictionary<string, string> packLabels,
            string currentPackId,
            System.Action<string> onSelectPack)
        {
            float titleHeight = 24f;
            Rect titleRect = new Rect(rect.x, rect.y, rect.width, titleHeight);
            Widgets.Label(titleRect, title);

            float filterHeight = 24f;
            float filterButtonWidth = 300f;
            string currentLabel = currentPackId == AllFilterId
                ? GetAllLabel()
                : (packLabels.TryGetValue(currentPackId, out string label) ? label : GetAllLabel());

            Rect filterButtonRect = new Rect(rect.x, titleRect.yMax + 2f, filterButtonWidth, filterHeight);
            if (Widgets.ButtonText(filterButtonRect, currentLabel))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>
                {
                    new FloatMenuOption(GetAllLabel(), () => onSelectPack(AllFilterId))
                };

                foreach (KeyValuePair<string, string> entry in packLabels.OrderBy(k => k.Value))
                {
                    string id = entry.Key;
                    string name = entry.Value;
                    options.Add(new FloatMenuOption(name, () => onSelectPack(id)));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            float panelPadding = 8f;
            Rect listRect = new Rect(
                rect.x + panelPadding,
                filterButtonRect.yMax + panelPadding,
                rect.width - panelPadding * 2f,
                rect.height - (filterButtonRect.yMax - rect.y) - panelPadding * 2f);

            Color originalColor = GUI.color;
            GUI.color = Color.white;
            Rect borderRect = new Rect(listRect.x - 3f, listRect.y - 2f, listRect.width + 3f, listRect.height + 4f);
            Widgets.DrawBox(borderRect, 1);
            GUI.color = originalColor;

            float lineHeight = Text.LineHeight + 4f;
            float viewHeight = Mathf.Max(items.Count * lineHeight + 4f, listRect.height);
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(listRect, ref scroll, viewRect, true);

            float curY = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                HediffDef def = items[i];
                Rect rowRect = new Rect(0f, curY, viewRect.width, lineHeight);

                float buttonWidth = 28f;
                float buttonHeight = rowRect.height - 6f;
                Rect labelRect = new Rect(rowRect.x, rowRect.y, rowRect.width - buttonWidth - 6f, rowRect.height);
                Rect buttonRect = new Rect(rowRect.x + rowRect.width - buttonWidth, rowRect.y + 3f, buttonWidth, buttonHeight);

                string itemLabel = def.label.NullOrEmpty() ? def.defName : def.label;
                Widgets.Label(labelRect, itemLabel);

                if (Widgets.ButtonText(buttonRect, buttonLabel))
                {
                    onClick(def);
                }

                curY += lineHeight;
            }

            Widgets.EndScrollView();
        }

        private static string GetPackId(HediffDef def)
        {
            return def.modContentPack?.PackageId ?? UnknownFilterId;
        }

        private static string GetAllLabel()
        {
            return "MechCommander.Settings.FilterAll".Translate();
        }

        private static string GetUnknownLabel()
        {
            return "MechCommander.Settings.FilterUnknown".Translate();
        }

        private static Dictionary<string, string> BuildPackLabels(List<HediffDef> defs)
        {
            Dictionary<string, string> labels = new Dictionary<string, string>();
            for (int i = 0; i < defs.Count; i++)
            {
                HediffDef def = defs[i];
                string id = GetPackId(def);
                if (!labels.ContainsKey(id))
                {
                    string label = def.modContentPack?.Name ?? GetUnknownLabel();
                    labels.Add(id, label);
                }
            }
            return labels;
        }
    }
}
