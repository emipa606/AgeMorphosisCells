# GitHub Copilot Instructions for the Age Morphosis Cells Mod

### Mod Overview and Purpose
The Age Morphosis Cells (AMCells) mod introduces advanced biological processing units called Age Morphosis Cells into RimWorld. These cells allow colonists to undergo controlled aging and rejuvenation, offering strategic benefits for managing colonists' age-related traits and health conditions. The mod aims to add a layer of depth to colony management and enhance player engagement with age dynamics.

### Key Features and Systems
- **Age Morphosis Cells Structures:** Implementable buildings that allow colonists to enter and undergo the aging process.
- **Colonist Interaction:** Colonists can carry others to the cells, enter them, and perform age or rejuvenation processes.
- **Configurable Settings:** Modifiable settings are available to customize the behavior of Age Morphosis Cells using in-game menus.
- **Dynamic UI Elements:** Sliders and dialogs facilitate user interaction and settings adjustments.

### Coding Patterns and Conventions
- **Class and Method Naming:** Classes and methods follow a clear, descriptive naming convention. For instance, `Building_AMCell` describes the building entity, while `JobDriver_CarryToAMCell` indicates the specific job for carrying tasks.
- **Internal and Public Access:** Internal classes like `AMCellsMod` and `AMCellsSettings` are limited to the mod's internal operations, while public classes such as `Building_AMCell` interact more openly within the game's ecosystem.
- **Inheritance and Interfaces:** Utilize inheritance and interfaces (e.g., `IThingHolder`, `IOpenable`) for structuring classes that extend game functionality.
- **Event-Driven Design:** Methods such as `DoTickerWork` are strategically used to integrate with RimWorld's timed events.

### XML Integration
While the provided summary doesn't list specific XML files, XML is typically used for defining game data such as item properties, recipes, and other configurations. In this mod, ensure that XML files align with C# logic to maintain consistency in features like object instantiation and in-game settings.

### Harmony Patching
The mod utilizes Harmony for modifying the game's core behavior without altering original code:
- **Static Patching Classes:** `Patches` class includes nested static classes like `FloatMenuMakerCarryAdder` to insert additional menu options when carrying items to AMCells.
- **Method Prefixes and Postfixes:** Use Harmony annotations to add logic before or after specific game methods.
- **Selective Patching:** Patch only necessary methods to maintain game stability and minimize conflicts with other mods.

### Suggestions for Copilot
- **Expanding Features:** Assist in generating new building types or interaction methods.
- **Boilerplate Code:** Generate templates for new Harmony patches or additional sliders.
- **UI Enhancements:** Suggest and auto-complete code for custom dialogs or interactive elements.
- **Debugging Assistance:** Provide debug logging integration snippets to trace mod behavior.
- **Code Refactoring:** Suggest optimizations or refactorings for existing methods and classes.

By leveraging GitHub Copilot, developers can streamline coding tasks, enhance mod features, and maintain high-quality code that adheres to RimWorld modding standards.
