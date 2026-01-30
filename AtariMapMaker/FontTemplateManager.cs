using System;
using System.Collections.Generic;
using System.Linq;

namespace AtariMapMaker
{
    public static class FontTemplateManager
    {
        public class FontTemplate
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public Func<int, byte> PatternFunction { get; set; }
        }

        private static readonly List<FontTemplate> templates = new List<FontTemplate>
        {
            new FontTemplate
            {
                Name = "All Font0",
                Description = "All lines use Font 0",
                PatternFunction = (line) => 0
            },
            new FontTemplate
            {
                Name = "Alternate 0-1",
                Description = "Alternating between Font 0 and Font 1",
                PatternFunction = (line) => (byte)(line % 2)
            },
            new FontTemplate
            {
                Name = "Alternate 0-1-2-3",
                Description = "Alternating between Fonts 0, 1, 2, 3",
                PatternFunction = (line) => (byte)(line % 4)
            },
            new FontTemplate
            {
                Name = "Alternate 0-1-2",
                Description = "Alternating between Fonts 0, 1, 2",
                PatternFunction = (line) => (byte)(line % 3)
            },
            new FontTemplate
            {
                Name = "Top Half Font0, Bottom Half Font1",
                Description = "First half uses Font 0, second half uses Font 1",
                PatternFunction = (line) => (byte)(line < 10 ? 0 : 1)  // Assuming 20 line screen
            }
        };

        public static List<FontTemplate> GetTemplates()
        {
            return templates;
        }

        public static FontTemplate GetTemplate(string name)
        {
            return templates.FirstOrDefault(t => t.Name == name);
        }

        public static void ApplyTemplate(AtariMap map, string templateName, int? screenX = null, int? screenY = null)
        {
            if (map == null) return;

            var template = GetTemplate(templateName);
            if (template == null) return;

            // Always per-screen mode: apply to all screens if no specific screen provided, otherwise just that screen
            if (screenX.HasValue && screenY.HasValue)
            {
                // Apply to specific screen
                for (int i = 0; i < map.ScreenSize.Height; i++)
                {
                    map.SetFontForLine(screenX.Value, screenY.Value, i, template.PatternFunction(i));
                }
            }
            else
            {
                // Apply to all screens
                for (int sy = 0; sy < map.MapSize.Height; sy++)
                {
                    for (int sx = 0; sx < map.MapSize.Width; sx++)
                    {
                        for (int i = 0; i < map.ScreenSize.Height; i++)
                        {
                            map.SetFontForLine(sx, sy, i, template.PatternFunction(i));
                        }
                    }
                }
            }

            map.FontTemplatePattern = templateName;
        }

        public static bool IsTemplateLocked(AtariMap map)
        {
            return map != null && map.FontTemplateLocked;
        }

        public static void SetTemplateLocked(AtariMap map, bool locked)
        {
            if (map != null)
                map.FontTemplateLocked = locked;
        }
    }
}
