using System.IO;
using System.Xml.Linq;
using Verse;

namespace AMCells;

[StaticConstructorOnStartup]
public class Main
{
    static Main()
    {
        var hugsLibConfig = Path.Combine(GenFilePaths.SaveDataFolderPath, Path.Combine("HugsLib", "ModSettings.xml"));
        if (!new FileInfo(hugsLibConfig).Exists)
        {
            return;
        }

        var xml = XDocument.Load(hugsLibConfig);

        var modSettings = xml.Root?.Element("AgeMorphosisCells");
        if (modSettings == null)
        {
            return;
        }

        foreach (var modSetting in modSettings.Elements())
        {
            if (modSetting.Name == "multiplyer")
            {
                AMCellsMod.Instance.Settings.Multiplyer = int.Parse(modSetting.Value);
            }

            if (modSetting.Name == "doMultiply")
            {
                AMCellsMod.Instance.Settings.DoMultiply = bool.Parse(modSetting.Value);
            }
        }

        xml.Root.Element("AgeMorphosisCells")?.Remove();
        xml.Save(hugsLibConfig);

        Log.Message("[AgeMorphosisCells]: Imported old HugLib-settings");
    }
}