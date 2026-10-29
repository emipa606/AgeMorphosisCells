# GitHub Copilot Instructions for Age Morphosis Cells (Continued)

## Mod Overview and Purpose

Age Morphosis Cells (Continued) is a RimWorld mod that adds a new building called the Age Morphosis Cell. This building functions similarly to a cryptosleep casket but accelerates aging for pawns inside it. The aging rate can be adjusted through the mod settings menu, and the building requires power to operate. This mod is an updated version of TheSleeplessSorclock's original mod.

## Key Features and Systems

### Age Morphosis Cell
- A new building that accelerates aging for pawns inside it.
- Configurable aging rate via mod settings.
- Requires power to operate.

### Job Definitions
- New job definitions for pawns entering and carrying pawns to the Age Morphosis Cell.

### Research Projects
- New research project to unlock the Age Morphosis Cell.

## Coding Patterns and Conventions

- **C# Files**:
  - Use PascalCase for class names and method names.
  - Follow RimWorld's naming conventions for XML files and tags.
  - Use meaningful variable names that describe their purpose.

- **XML Files**:
  - Ensure that all XML files are properly formatted and adhere to RimWorld's XML schema.
  - Use attributes like `defName`, `label`, and `description` appropriately.

## XML Integration

### XML File Structure
- **Jobs_AMCell.xml**: Contains job definitions for interacting with the Age Morphosis Cell.
- **AMCell_Research.xml**: Defines the research project required to unlock the Age Morphosis Cell.
- **AMCell.xml**: Contains the definition for the Age Morphosis Cell building.

### XML Tags and Attributes
- Use `defName` for unique identifiers.
- Use `label` and `description` for user-facing text.
- Ensure that all required attributes are present and correctly formatted.

## Harmony Patching

- **Harmony Patching**: The mod uses Harmony patches to modify existing RimWorld code behavior. Ensure that patches are applied correctly to avoid conflicts or crashes.

### Example Harmony Patch

csharp
using HarmonyLib;
using RimWorld;
using Verse;

namespace AgeMorphosisCells
{
    [HarmonyPatch(typeof(Pawn_AgeTracker), "AdvanceTicks")]
    public static class Pawn_AgeTracker_AdvanceTicks_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_AgeTracker __instance)
        {
            if (__instance.pawn.GetComp<CompAgeMorphosis>() != null)
            {
                __instance.age += 0.1f; // Example: Increase age by 10% each tick
            }
        }
    }
}


## Suggestions for Copilot

1. **C# Coding**:
   - When generating new C# code, ensure that it adheres to the coding patterns and conventions mentioned above.
   - Use Copilot to quickly scaffold new classes, methods, and job drivers.
   - Use Copilot to generate XML definitions for new jobs, research projects, and buildings.

2. **XML Integration**:
   - Use Copilot to generate and format XML files according to RimWorld's schema.
   - Use Copilot to generate XML tags and attributes for job definitions, research projects, and building definitions.

3. **Harmony Patching**:
   - Use Copilot to generate Harmony patches for modifying existing RimWorld code behavior.
   - Ensure that generated patches are correctly applied to avoid conflicts or crashes.

4. **Error Reporting**:
   - Use Copilot to generate error reports that include relevant log information and steps to reproduce the error.
   - Use Copilot to suggest solutions for common errors and issues.

## Known Issues

- None for now, but if you encounter any issues, please report them to the GitHub repository or the Discord channel.
- Ensure that errors are reported with relevant log information and steps to reproduce the error.

## Conclusion

Age Morphosis Cells (Continued) is a powerful mod that adds a unique feature to RimWorld. By following the coding patterns and conventions, and utilizing Copilot for generating and managing code, you can contribute to the mod's development and enhance its functionality.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

