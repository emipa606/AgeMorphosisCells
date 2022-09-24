using Verse;

namespace AMCells;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class AMCellsSettings : ModSettings
{
    public bool DoMultiply = true;
    public int Multiplyer = 1;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref DoMultiply, "DoMultiply", true);
        Scribe_Values.Look(ref Multiplyer, "Multiplyer", 1);
    }
}