using Verse;
using RimWorld;
using System;

namespace RoebuckRibbons
{
	// Automatically injects the custom Medals inspector tab into
    // all humanlike pawns and corpses at game startup.
    //
    // This avoids the need for Harmony patches or XML injection.
    // It modifies ThingDefs directly during static initialization.
    [StaticConstructorOnStartup]
    public static class DefInjector
    {
		// Static constructor runs once when the game loads defs.
        // This is where I'll iterate through all ThingDefs and
        // inject our custom inspector tab where appropriate.
        static DefInjector()
        {
            Log.Message("RoebuckRibbons: Injecting Medals tab");
			
			// Iterate through every ThingDef loaded in the game
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                // Humanlike pawns (colonists, raiders, etc.)
                // Corpses (so medals can still be viewed after death)
                if ((def.thingClass == typeof(Pawn) && def.race.Humanlike) || def.thingClass == typeof(Corpse))
                {
					// Ensure the resolved tab list contains our tab instance
                    if (!def.inspectorTabs.Contains(typeof(ITab_Pawn_Medals)))
                        def.inspectorTabs.Add(typeof(ITab_Pawn_Medals));

					// Try to position our tab immediately after
					// the Character tab for natural UI ordering
                    try
                    {
                        var tab = InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Medals));

                        if (!def.inspectorTabsResolved.Contains(tab))
                        {
                            int charTabIndex = def.inspectorTabsResolved.FindIndex(
                                t => t.GetType() == typeof(ITab_Pawn_Character));
							
							// insert after Character tab if it exists
                            if (charTabIndex >= 0)
                                def.inspectorTabsResolved.Insert(charTabIndex + 1, tab);
							// Otherwise, just append
                            else
                                def.inspectorTabsResolved.Add(tab);
                        }
                    }
                    catch (Exception ex)
                    {
						// If something unexpected happens, we'll just log it and move on to prevent hard-crashes
                        Log.Error("Failed adding Medals tab: " + ex);
                    }
                }
            }
        }
    }
}
