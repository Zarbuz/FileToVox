using SchematicReader;
using SchematicToVox.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Extensions
{
    public static class Extensions
    {
        private static readonly Dictionary<Tuple<int, int>, Color32> _colors = new Dictionary<Tuple<int, int>, Color32>()
        {
            { new Tuple<int, int>(1, 0), new Color32(125, 125, 125, 255) }, // Stone
            { new Tuple<int, int>(1, 1), new Color32(153, 113, 98, 255) },  // Granite
            { new Tuple<int, int>(1, 2), new Color32(159, 114, 98, 255) },  // Polished Granite
            { new Tuple<int, int>(1, 3), new Color32(179, 179, 182, 255) }, // Diorite
            { new Tuple<int, int>(1, 4), new Color32(183, 183, 185, 255) }, // Polished Diorite
            { new Tuple<int, int>(1, 5), new Color32(130, 131, 131, 255) }, // Andesite
            { new Tuple<int, int>(1, 6), new Color32(133, 133, 134, 255) }, // Polished Andesite
            { new Tuple<int, int>(2, 0), new Color32(118, 179, 76, 255) },  // Grass
            { new Tuple<int, int>(3, 0), new Color32(134, 96, 76, 255) },   // Dirt
            { new Tuple<int, int>(3, 1), new Color32(134, 96, 76, 255) },   // Coarse Dirt
            { new Tuple<int, int>(3, 2), new Color32(134, 96, 76, 255) },   // Podzol
            { new Tuple<int, int>(4, 0), new Color32(122, 122, 122, 255) }, // Cobblestone
            { new Tuple<int, int>(5, 0), new Color32(156, 127, 78, 255) },  // Wooden Plank (Oak)
            { new Tuple<int, int>(5, 1), new Color32(103, 77, 46, 255) },   // Wooden Plank (Spruce)
            { new Tuple<int, int>(5, 2), new Color32(195, 179, 123, 255) }, // Wooden Plank (Birch)
            { new Tuple<int, int>(5, 3), new Color32(154, 110, 77, 255) },  // Wooden Plank (Jungle)
            { new Tuple<int, int>(5, 4), new Color32(169, 91, 51, 255) },   // Wooden Plank (Acacia)
            { new Tuple<int, int>(5, 5), new Color32(61, 39, 18, 255) },    // Wooden Plank (Dark Oak)
            { new Tuple<int, int>(6, 0), new Color32(71, 102, 37, 255) },   // Sapling (Oak)
            { new Tuple<int, int>(6, 1), new Color32(51, 58, 33, 255) },    // Sapling (Spruce)
            { new Tuple<int, int>(6, 2), new Color32(118, 150, 84, 255) },  // Sapling (Birch)
            { new Tuple<int, int>(6, 3), new Color32(48, 86, 18, 255) },    // Sapling (Jungle)
            { new Tuple<int, int>(6, 4), new Color32(114, 115, 20, 255) },  // Sapling (Acacia)
            { new Tuple<int, int>(6, 5), new Color32(56, 86, 28, 255) },    // Sapling (Dark Oak)
            { new Tuple<int, int>(7, 0), new Color32(83, 83, 83, 255) },    // Bedrock
            { new Tuple<int, int>(8, 0), new Color32(112, 175, 220, 255) }, // Water
            { new Tuple<int, int>(9, 0), new Color32(112, 175, 220, 255) }, // Water (No Spread)
            { new Tuple<int, int>(10, 0), new Color32(207, 91, 19, 255) },  // Lava
            { new Tuple<int, int>(11, 0), new Color32(212, 90, 18, 255) },  // Lava (No Spread)
            { new Tuple<int, int>(12, 0), new Color32(219, 211, 160, 255) },// Sand
            { new Tuple<int, int>(12, 1), new Color32(167, 87, 32, 255) },  // Red Sand
            { new Tuple<int, int>(13, 0), new Color32(126, 124, 122, 255) },// Gravel
            { new Tuple<int, int>(14, 0), new Color32(143, 139, 124, 255) },// Gold Ore
            { new Tuple<int, int>(15, 0), new Color32(135, 130, 126, 255) },// Iron Ore


        };

        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }


        public static Color32 GetBlockColor(this Block block)
        {
            Color32 color = new Color32(0, 0, 0, 0);
            return color;
        }
    }
}
