using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RoebuckRibbons
{
    public class RibbonExtension : DefModExtension
    {
        public int precedence;
        public string awardType;
        public int awardCount = 1;

        public string iconPath;

        private Texture2D _icon;
        public Texture2D Icon
        {
            get
            {
                if (_icon == null && !string.IsNullOrEmpty(iconPath))
                {
                    _icon = ContentFinder<Texture2D>.Get(iconPath, true);
                }
                return _icon;
            }
        }
    }

	// Custom tab for displaying a pawn's medals in the inspection pane
    public class ITab_Pawn_Medals : ITab
    {
        private const float IconSize = 32f;
        private const float IconSpacing = 0f;

        public override bool IsVisible
        {
            get
            {
                var pawn = base.SelPawn ?? (base.SelThing as Corpse)?.InnerPawn;
                if (pawn == null) return false;

                // Only colonists
                return pawn.IsColonist;
            }
        }

		// Helper property to get the pawn to display info for (handles live pawns and corpses)
        private Pawn PawnToShowInfoAbout
        {
            get
            {
                Pawn pawn = null;
                if (base.SelPawn != null)
                {
                    pawn = base.SelPawn;
                }
                else
                {
                    Corpse corpse = base.SelThing as Corpse;
                    if (corpse != null)
                    {
                        pawn = corpse.InnerPawn;
                    }
                }
                if (pawn == null)
                {
                    Log.Error("Character tab found no selected pawn to display.");
                    return null;
                }

                return pawn;
            }
        }

        public ITab_Pawn_Medals()
        {
            this.size = new Vector2(600f, 300f);
            this.labelKey = "TabMedals";
            this.tutorTag = "Medals";
        }

        protected override void FillTab()
        {
			// Get the selected pawn or corpse's inner pawn
            Pawn pawn = base.SelPawn ?? (base.SelThing as Corpse)?.InnerPawn;
            if (pawn == null) return;

            // Get all ribbon items 
            List<Thing> ribbons = pawn.inventory?.innerContainer
                .Where(t => t.def.HasModExtension<RibbonExtension>())
                .SelectMany(t => Enumerable.Repeat(t, t.stackCount))
                .ToList() ?? new List<Thing>();

            Widgets.DrawWindowBackground(new Rect(0f, 0f, this.size.x, this.size.y));

            if (ribbons.Count == 0) return;

            int totalAwards = 0;
            foreach (var ribbon in ribbons)
            {
                var ext = ribbon.def.GetModExtension<RibbonExtension>();
                int countPerItem = ext?.awardCount ?? 1;
                totalAwards += ribbon.stackCount * countPerItem; // Max stack right now is 1, but we're including stackcount here in case that changes
            }

			// Create GUIStyle for the total awards label
            GUIStyle totalStyle = new GUIStyle();
            totalStyle.alignment = TextAnchor.UpperLeft;
            totalStyle.fontSize = 12;
            totalStyle.normal.textColor = Color.yellow;
            totalStyle.fontStyle = FontStyle.Bold;

            // Draw the label at the top, we're just going to ignore all of these magic numbers
            GUI.Label(new Rect(0f, 5f, this.size.x, 25f), $"    Medals Earned: {totalAwards}", totalStyle);

			// Icon layout settings
            int iconsPerRow = 3;
            float spacing = 1f;
            float scale = 1.25f;

            // Compute total rows
            int totalRows = Mathf.CeilToInt((float)ribbons.Count / iconsPerRow);

            // Precompute the widest row width for centering
            int lastRowCount = ribbons.Count % iconsPerRow;
            int widestRowCount = (lastRowCount == 0) ? iconsPerRow : lastRowCount;

            // We'll compute rack width dynamically per row since icon widths may differ
            // But we can roughly center vertically based on average icon height
            float avgIconHeight = 0f;
            foreach (var ribbon in ribbons)
            {
                var ext = ribbon.def.GetModExtension<RibbonExtension>();
                Texture2D tex = ext?.Icon ?? ribbon.Graphic.MatSingle.mainTexture as Texture2D;
                avgIconHeight += tex.height * scale;
            }
            avgIconHeight /= ribbons.Count;
            float rackHeight = totalRows * avgIconHeight + (totalRows - 1) * spacing;
            float offsetY = (this.size.y - rackHeight) / 2f;

            // Draw ribbons
            for (int i = 0; i < ribbons.Count; i++)
            {
                var ribbon = ribbons[i];
                var ribbonExt = ribbon.def.GetModExtension<RibbonExtension>();

                // Determine texture to draw
                Texture2D texToDraw = ribbonExt?.Icon ?? (ribbon.Graphic.MatSingle.mainTexture as Texture2D);

                // Icon dimensions (scaled)
                float iconWidth = texToDraw.width * scale;
                float iconHeight = texToDraw.height * scale;

                // Row & column
                int rowFromTop = i / iconsPerRow;
                int visualRow = totalRows - 1 - rowFromTop; // bottom-up
                int col = i % iconsPerRow;

                // Number of icons in this row for centering the row
                int itemsInThisRow = Math.Min(iconsPerRow, ribbons.Count - rowFromTop * iconsPerRow);
                float rowWidth = 0f;
                for (int c = 0; c < itemsInThisRow; c++)
                {
                    var r = ribbons[rowFromTop * iconsPerRow + c];
                    var rExt = r.def.GetModExtension<RibbonExtension>();
                    Texture2D rTex = rExt?.Icon ?? r.Graphic.MatSingle.mainTexture as Texture2D;
                    rowWidth += rTex.width * scale;
                    if (c < itemsInThisRow - 1) rowWidth += spacing;
                }

				// Calculate horizontal offset for this column
                float offsetX = (this.size.x - rowWidth) / 2f;
                float startX = offsetX;
                for (int c = 0; c < col; c++)
                {
                    var r = ribbons[rowFromTop * iconsPerRow + c];
                    var rExt = r.def.GetModExtension<RibbonExtension>();
                    Texture2D rTex = rExt?.Icon ?? r.Graphic.MatSingle.mainTexture as Texture2D;
                    startX += rTex.width * scale + spacing;
                }

                float x = startX;
                float y = offsetY + visualRow * (iconHeight + spacing);

                Rect iconRect = new Rect(x, y, iconWidth, iconHeight);

                // Draw the ribbon icon
                Widgets.DrawTextureFitted(iconRect, texToDraw, 1f);

                // Highlight & tooltip
                if (Mouse.IsOver(iconRect))
                {
                    Widgets.DrawHighlight(iconRect);
                    string tooltip = ribbon.def.label.CapitalizeFirst() + "\n\n<i>" + ribbon.def.description + "</i>";
                    TooltipHandler.TipRegion(iconRect, tooltip);
                }
            }
        }
    }
}
