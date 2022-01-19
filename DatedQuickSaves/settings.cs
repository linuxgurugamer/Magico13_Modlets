using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using KSP.Localization;

namespace DatedQuickSaves
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#DQS_comment-2754813
    // search for "Mod integration into Stock Settings

    public class DQSSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return Localizer.Format("#autoLOC_149458"); } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Dated Quick Save"; } }
        public override string DisplaySection { get { return "Dated Quick Save"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }

        [GameParameters.CustomStringParameterUI("#DQS_QuickSave", toolTip = "", lines = 1)]
        public string QuickSave = "";

        [GameParameters.CustomParameterUI("#DQS_QuickSaveEnable", toolTip = "")]
        public bool QuickSaveEnable = true;

        [GameParameters.CustomParameterUI("#DQS_StockQuickSaveRename", toolTip = "#DQS_StockQuickSaveRename_t")]
        public bool StockQuickSaveRename = true;

        [GameParameters.CustomIntParameterUI("#DQS_MaxQuickSaveCount", toolTip = "#DQS_MaxQuickSaveCount_t", 
            minValue = -1, maxValue = 50)]
        public int MaxQuickSaveCount = 20;

        [GameParameters.CustomIntParameterUI("#DQS_QuickSaveDelay", toolTip = "#DQS_QuickSaveDelay_t", 
            minValue = 500, maxValue = 15000, stepSize =500)]
        public int QuickSaveDelayMS = 1000;

        [GameParameters.CustomStringParameterUI("#DQS_AutoSave", toolTip = "", lines = 1)]
        public string AutoSave = "";

        [GameParameters.CustomParameterUI("#DQS_AutoSaveEnable", toolTip = "")]
        public bool AutoSaveEnable = true;

        [GameParameters.CustomParameterUI("#DQS_AutoSaveOnStart", toolTip = "#DQS_AutoSaveOnStart_t")]
        public bool AutoSaveOnStart = false;

        [GameParameters.CustomIntParameterUI("#DQS_AutoSaveFreq", toolTip = "", minValue = 1, maxValue = 180)]
        public int AutoSaveFreq = 15;

        [GameParameters.CustomIntParameterUI("#DQS_MaxAutoSaveCount", toolTip = "#DQS_MaxAutoSaveCount_t", minValue = -1, maxValue = 50)]
        public int MaxAutoSaveCount = 20;

        [GameParameters.CustomStringParameterUI("#DQS_Note1", toolTip = "", lines = 2)]
        public string note1 = "";

        [GameParameters.CustomParameterUI("#DQS_ReloadExtra", toolTip = "#DQS_ReloadExtra_t")]
        public bool ReloadExtra = false;


        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == nameof(MaxQuickSaveCount)
                || member.Name == nameof(StockQuickSaveRename)
                || member.Name == nameof(QuickSaveDelayMS)
                )
                return QuickSaveEnable;

            if (member.Name == nameof(AutoSaveOnStart)
                || member.Name == nameof(AutoSaveFreq)
                || member.Name == nameof(MaxAutoSaveCount)
                )
                return AutoSaveEnable;

            return true;
        }
    }
}