using HugsLib;
using HugsLib.Settings;
using Verse;

namespace AMCells
{
    public class SettingAMCells : ModBase
    {
        public static int settingMultiplyer = 1;
        public static bool settingDoMultiply = true;
        private SettingHandle<bool> doMultiply;

        private SettingHandle<int> multiplyer;
        public override string ModIdentifier => "AgeMorphosisCells";

        public override void DefsLoaded()
        {
            multiplyer = Settings.GetHandle("multiplyer", "settings_multiplyer_title".Translate(),
                "settings_multiplyer_desc".Translate(), 1);
            doMultiply = Settings.GetHandle("doMultiply", "settings_doMultiply_title".Translate(),
                "settings_doMultiply_desc".Translate(), true);
            settingMultiplyer = multiplyer.Value;
            settingDoMultiply = doMultiply.Value;
        }

        public override void SettingsChanged()
        {
            base.SettingsChanged();
            if (multiplyer.Value < 0)
            {
                multiplyer.Value = 0;
            }

            settingMultiplyer = multiplyer.Value;
            settingDoMultiply = doMultiply.Value;
        }
    }
}