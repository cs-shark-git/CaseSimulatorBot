using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseSimulatorBot.Utils
{
    internal class ColorPicker
    {

        public Color GetRarityColor(string rarity) 
        {
            switch (rarity)
            {
                case "Common":
                    return Color.FromRgb(0xB3, 0xB3, 0xB3);
                case "Uncommon":
                    return Color.FromRgb(0x5E, 0x8B, 0xD6);
                case "Rare":
                    return Color.FromRgb(0x8B, 0x55, 0xFB);
                case "Legendary":
                    return Color.FromRgb(0xD1, 0x2D, 0xDF);
                case "Mythical":
                    return Color.FromRgb(0xEA, 0x31, 0x31);
            }
            return Color.White;
        }
    }
}
