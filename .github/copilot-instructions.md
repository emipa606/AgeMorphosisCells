# GitHub Copilot Instructions for RimWorld Modding Project: Age Morphosis Cells (Continued)

## Mod Overview and Purpose

**Mod Name:** Age Morphosis Cells (Continued)

This mod introduces a new building to RimWorld, the Age Morphosis Cell, which functions similarly to a cryptosleep casket but accelerates the aging of a pawn who enters it. Users can customize the aging rate via the mod settings menu. The building requires power to operate. Originally created by TheSleeplessSorclock, this mod has been updated and maintained by Haecriver.

## Key Features and Systems

- **Age Morphosis Cell:** A new building type that accelerates the aging process of characters in the game.
- **Modular Aging Rate:** Users can adjust how quickly the aging process occurs using a slider in the mod settings.
- **Power Dependency:** The Age Morphosis Cell requires a power connection to function.
- **Integration with Other Mods:** The mod works best with the "Babies and Children" mod, enhancing its impact by providing an option to accelerate growth stages.

## Known Issues

- Currently, no known issues have been reported. Users are encouraged to report any encountered bugs.

## Coding Patterns and Conventions

The codebase follows these patterns and conventions:

1. **Namespace and Class Organization:** Files are structured using C# classes with internal and public access modifiers to encapsulate functionality. 
2. **Inheritance from Base Classes:** Many classes extend RimWorld-specific base classes like `Building`, `IThingHolder`, `IOpenable`, `JobDriver`, etc., to ensure the mod integrates well with the game's existing architecture.
3. **Method Naming:** Methods are intuitively named to reflect their functionality, such as `GetDirectlyHeldThings`, `GetChildHolders`, and `doTickerWork`.

## XML Integration

- RimWorld mods often use XML files for defining static data and integrating with the game's data-driven architecture. While this particular mod focuses heavily on C# for behavior, any XML integration should align with RimWorld's modding framework for consistent data representation.

## Harmony Patching

The mod doesn't explicitly mention Harmony patches; however, if necessary, Harmony can be used to patch RimWorld core behaviors. This involves:

1. **Creating a Harmony Instance:** To apply patches programmatically, using a Harmony instance.
2. **Defining Prefix/Postfix Methods:** Attach methods that run before or after a target method.
3. **Ensure Compatibility:** Write patches that ensure compatibility without altering or breaking existing game functionality.

## Suggestions for Copilot

Here's how GitHub Copilot can assist in this RimWorld modding project:

1. **Code Completion:** Use Copilot to suggest method implementations and fill in class methods quickly based on existing patterns.
2. **Syntax Suggestions:** Leverage Copilot for syntax corrections and optimizations especially when dealing with complex C# constructs.
3. **XML Configuration:** Generate boilerplate code for XML data files to integrate seamlessly with RimWorld’s data-driven mechanisms.
4. **Harmony Patching Examples:** Suggestive snippets for common Harmony patching patterns like Prefixes and Postfixes.
5. **Best Practices Suggestions:** Receive recommendations for adhering to C# best practices, enhancing code readability, and maintainability.

By leveraging Copilot, development efficiency can be increased, focusing more on design and integration rather than detailed manual coding.
