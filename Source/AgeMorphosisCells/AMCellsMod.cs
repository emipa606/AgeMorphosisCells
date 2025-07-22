using Mlie;
using UnityEngine;
using Verse;

namespace AMCells;

[StaticConstructorOnStartup]
internal class AMCellsMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static AMCellsMod Instance;

    private static string currentVersion;

    /// <summary>
    ///     The private settings
    /// </summary>
    private AMCellsSettings settings;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public AMCellsMod(ModContentPack content) : base(content)
    {
        Instance = this;
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal AMCellsSettings Settings
    {
        get
        {
            settings ??= GetSettings<AMCellsSettings>();

            return settings;
        }
    }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Age Morphosis Cells";
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(rect);
        listingStandard.CheckboxLabeled("settings_doMultiply_title".Translate(), ref Settings.DoMultiply,
            "settings_doMultiply_desc".Translate());
        listingStandard.Label("settings_multiplyer_title_new".Translate(Settings.Multiplyer), -1f,
            "settings_multiplyer_desc".Translate());
        listingStandard.IntAdjuster(ref Settings.Multiplyer, 1);
        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("settings_modversion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
        Settings.Write();
    }
}