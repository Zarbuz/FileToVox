using System;
using System.Collections.Generic;
using System.Drawing;

namespace SchematicToVoxCore.Extensions
{
    public static class Extensions
    {
        private static readonly Dictionary<Tuple<int, int>, Color> _colors = new Dictionary<Tuple<int, int>, Color>()

        {
            { new Tuple<int, int>(1, 0), Color.FromArgb(255, 125, 125, 125) }, //Stone
            { new Tuple<int, int>(1, 1), Color.FromArgb(255, 153, 113, 98) }, //Granite
            { new Tuple<int, int>(1, 2), Color.FromArgb(255, 159, 114, 98) }, //Polished Granite
            { new Tuple<int, int>(1, 3), Color.FromArgb(255, 179, 179, 182) }, //Diorite
            { new Tuple<int, int>(1, 4), Color.FromArgb(255, 183, 183, 185) }, //Polished Diorite
            { new Tuple<int, int>(1, 5), Color.FromArgb(255, 130, 131, 131) }, //Andesite
            { new Tuple<int, int>(1, 6), Color.FromArgb(255, 133, 133, 134) }, //Polished Andesite
            { new Tuple<int, int>(2, 0), Color.FromArgb(255, 118, 179, 76) }, //Grass
            { new Tuple<int, int>(3, 0), Color.FromArgb(255, 134, 96, 67) }, //Dirt
            { new Tuple<int, int>(3, 1), Color.FromArgb(255, 134, 96, 67) }, //Coarse Dirt
            { new Tuple<int, int>(3, 2), Color.FromArgb(255, 134, 96, 67) }, //Podzol
            { new Tuple<int, int>(4, 0), Color.FromArgb(255, 122, 122, 122) }, //Cobblestone
            { new Tuple<int, int>(5, 0), Color.FromArgb(255, 156, 127, 78) }, //Wooden Plank (Oak)
            { new Tuple<int, int>(5, 1), Color.FromArgb(255, 103, 77, 46) }, //Wooden Plank (Spruce)
            { new Tuple<int, int>(5, 2), Color.FromArgb(255, 195, 179, 123) }, //Wooden Plank (Birch)
            { new Tuple<int, int>(5, 3), Color.FromArgb(255, 154, 110, 77) }, //Wooden Plank (Jungle)
            { new Tuple<int, int>(5, 4), Color.FromArgb(255, 169, 91, 51) }, //Wooden Plank (Acacia)
            { new Tuple<int, int>(5, 5), Color.FromArgb(255, 61, 39, 18) }, //Wooden Plank (Dark Oak)
            { new Tuple<int, int>(6, 0), Color.FromArgb(255, 71, 102, 37) }, //Sapling (Oak)
            { new Tuple<int, int>(6, 1), Color.FromArgb(255, 51, 58, 33) }, //Sapling (Spruce)
            { new Tuple<int, int>(6, 2), Color.FromArgb(255, 118, 150, 84) }, //Sapling (Birch)
            { new Tuple<int, int>(6, 3), Color.FromArgb(255, 48, 86, 18) }, //Sapling (Jungle)
            { new Tuple<int, int>(6, 4), Color.FromArgb(255, 114, 115, 20) }, //Sapling (Acacia)
            { new Tuple<int, int>(6, 5), Color.FromArgb(255, 56, 86, 28) }, //Sapling (Dark Oak)
            { new Tuple<int, int>(7, 0), Color.FromArgb(255, 83, 83, 83) }, //Bedrock
            { new Tuple<int, int>(8, 0), Color.FromArgb(255, 112, 175, 220) }, //Water
            { new Tuple<int, int>(9, 0), Color.FromArgb(255, 112, 175, 220) }, //Water (No Spread)
            { new Tuple<int, int>(10, 0), Color.FromArgb(255, 207, 91, 19) }, //Lava
            { new Tuple<int, int>(11, 0), Color.FromArgb(255, 212, 90, 18) }, //Lava (No Spread)
            { new Tuple<int, int>(12, 0), Color.FromArgb(255, 219, 211, 160) }, //Sand
            { new Tuple<int, int>(12, 1), Color.FromArgb(255, 167, 87, 32) }, //Red Sand
            { new Tuple<int, int>(13, 0), Color.FromArgb(255, 126, 124, 122) }, //Gravel
            { new Tuple<int, int>(14, 0), Color.FromArgb(255, 143, 139, 124) }, //Gold Ore
            { new Tuple<int, int>(15, 0), Color.FromArgb(255, 135, 130, 126) }, //Iron Ore
            { new Tuple<int, int>(16, 0), Color.FromArgb(255, 115, 115, 115) }, //Coal Ore
            { new Tuple<int, int>(17, 0), Color.FromArgb(255, 102, 81, 49) }, //Wood (Oak)
            { new Tuple<int, int>(17, 1), Color.FromArgb(255, 45, 28, 12) }, //Wood (Spruce)
            { new Tuple<int, int>(17, 2), Color.FromArgb(255, 206, 206, 201) }, //Wood (Birch)
            { new Tuple<int, int>(17, 3), Color.FromArgb(255, 87, 67, 26) }, //Wood (Jungle)
            { new Tuple<int, int>(17, 4), Color.FromArgb(255, 88, 69, 39) }, //Wood (Oak 4)
            { new Tuple<int, int>(17, 5), Color.FromArgb(255, 36, 20, 5) }, //Wood (Oak 5)
            { new Tuple<int, int>(18, 0), Color.FromArgb(255, 69, 178, 49) }, //Leaves (Oak)
            { new Tuple<int, int>(18, 1), Color.FromArgb(255, 116, 116, 116) }, //Leaves (Spruce)
            { new Tuple<int, int>(18, 2), Color.FromArgb(255, 135, 135, 135) }, //Leaves (Birch)
            { new Tuple<int, int>(18, 3), Color.FromArgb(255, 45, 125, 16) }, //Leaves (Jungle)
            { new Tuple<int, int>(19, 0), Color.FromArgb(255, 194, 195, 84) }, //Sponge
            { new Tuple<int, int>(19, 1), Color.FromArgb(255, 153, 148, 53) }, //Wet Sponge
            { new Tuple<int, int>(20, 0), Color.FromArgb(255, 218, 240, 244) }, //Glass
            { new Tuple<int, int>(21, 0), Color.FromArgb(255, 102, 112, 134) }, //Lapis Lazuli Ore
            { new Tuple<int, int>(22, 0), Color.FromArgb(255, 38, 67, 137) }, //Lapis Lazuli Block
            { new Tuple<int, int>(23, 0), Color.FromArgb(255, 131, 131, 131) }, //Dispenser
            { new Tuple<int, int>(24, 0), Color.FromArgb(255, 220, 211, 159) }, //Sandstone
            { new Tuple<int, int>(24, 1), Color.FromArgb(255, 220, 211, 159) }, //Sandstone (Chiseled)
            { new Tuple<int, int>(24, 2), Color.FromArgb(255, 220, 211, 159) }, //Sandstone (Smooth)
            { new Tuple<int, int>(25, 0), Color.FromArgb(255, 100, 67, 50) }, //Note Block
            { new Tuple<int, int>(26, 0), Color.FromArgb(255, 180, 52, 54) }, //Bed (Block)
            { new Tuple<int, int>(27, 0), Color.FromArgb(255, 246, 199, 35) }, //Rail (Powered)
            { new Tuple<int, int>(28, 0), Color.FromArgb(255, 124, 124, 124) }, //Rail (Detector)
            { new Tuple<int, int>(29, 0), Color.FromArgb(255, 142, 191, 119) }, //Sticky Piston
            { new Tuple<int, int>(30, 0), Color.FromArgb(255, 220, 220, 220) }, //Cobweb
            { new Tuple<int, int>(31, 0), Color.FromArgb(255, 149, 101, 41) }, //Tall Grass (Dead Shrub)
            { new Tuple<int, int>(31, 1), Color.FromArgb(255, 2, 145, 36) }, //Tall Grass
            { new Tuple<int, int>(31, 2), Color.FromArgb(255, 91, 125, 56) }, //Tall Grass (Fern)
            { new Tuple<int, int>(32, 0), Color.FromArgb(255, 123, 79, 25) }, //Dead Shrub
            { new Tuple<int, int>(33, 0), Color.FromArgb(255, 165, 139, 83) }, //Piston
            { new Tuple<int, int>(34, 0), Color.FromArgb(255, 189, 150, 94) }, //Piston (Head)
            { new Tuple<int, int>(35, 0), Color.FromArgb(255, 221, 221, 221) }, //Wool
            { new Tuple<int, int>(35, 1), Color.FromArgb(255, 219, 125, 62) }, //Orange Wool
            { new Tuple<int, int>(35, 2), Color.FromArgb(255, 179, 80, 188) }, //Magenta Wool
            { new Tuple<int, int>(35, 3), Color.FromArgb(255, 106, 138, 201) }, //Light Blue Wool
            { new Tuple<int, int>(35, 4), Color.FromArgb(255, 177, 166, 39) }, //Yellow Wool
            { new Tuple<int, int>(35, 5), Color.FromArgb(255, 65, 174, 56) }, //Lime Wool
            { new Tuple<int, int>(35, 6), Color.FromArgb(255, 208, 132, 153) }, //Pink Wool
            { new Tuple<int, int>(35, 7), Color.FromArgb(255, 64, 64, 64) }, //Gray Wool
            { new Tuple<int, int>(35, 8), Color.FromArgb(255, 154, 161, 161) }, //Light Gray Wool
            { new Tuple<int, int>(35, 9), Color.FromArgb(255, 46, 110, 137) }, //Cyan Wool
            { new Tuple<int, int>(35, 10), Color.FromArgb(255, 126, 61, 181) }, //Purple Wool
            { new Tuple<int, int>(35, 11), Color.FromArgb(255, 46, 56, 141) }, //Blue Wool
            { new Tuple<int, int>(35, 12), Color.FromArgb(255, 79, 50, 31) }, //Brown Wool
            { new Tuple<int, int>(35, 13), Color.FromArgb(255, 53, 70, 27) }, //Green Wool
            { new Tuple<int, int>(35, 14), Color.FromArgb(255, 150, 52, 48) }, //Red Wool
            { new Tuple<int, int>(35, 15), Color.FromArgb(255, 25, 22, 22) }, //Black Wool
            { new Tuple<int, int>(36, 0), Color.FromArgb(255, 229, 229, 229) }, //Piston (Moving)
            { new Tuple<int, int>(37, 0), Color.FromArgb(255, 255, 255, 0) }, //Dandelion
            { new Tuple<int, int>(38, 0), Color.FromArgb(255, 218, 0, 13) }, //Poppy
            { new Tuple<int, int>(38, 1), Color.FromArgb(255, 37, 152, 138) }, //Blue Orchid
            { new Tuple<int, int>(38, 2), Color.FromArgb(255, 177, 141, 211) }, //Allium
            { new Tuple<int, int>(38, 3), Color.FromArgb(255, 255, 255, 167) }, //Azure Bluet
            { new Tuple<int, int>(38, 4), Color.FromArgb(255, 208, 57, 22) }, //Red Tulip
            { new Tuple<int, int>(38, 5), Color.FromArgb(255, 95, 134, 32) }, //Orange Tulip
            { new Tuple<int, int>(38, 6), Color.FromArgb(255, 94, 153, 65) }, //White Tulip
            { new Tuple<int, int>(38, 7), Color.FromArgb(255, 101, 150, 73) }, //Pink Tulip
            { new Tuple<int, int>(38, 8), Color.FromArgb(255, 176, 197, 139) }, //Oxeye Daisy
            { new Tuple<int, int>(39, 0), Color.FromArgb(255, 120, 87, 65) }, //Brown Mushroom
            { new Tuple<int, int>(40, 0), Color.FromArgb(255, 225, 15, 13) }, //Red Mushroom
            { new Tuple<int, int>(41, 0), Color.FromArgb(255, 249, 236, 78) }, //Block of Gold
            { new Tuple<int, int>(42, 0), Color.FromArgb(255, 219, 219, 219) }, //Block of Iron
            { new Tuple<int, int>(43, 0), Color.FromArgb(255, 161, 161, 161) }, //Stone Slab (Double)
            { new Tuple<int, int>(43, 1), Color.FromArgb(255, 223, 216, 164) }, //Sandstone Slab (Double)
            { new Tuple<int, int>(43, 2), Color.FromArgb(255, 146, 121, 68) }, //Wooden Slab (Double)
            { new Tuple<int, int>(43, 3), Color.FromArgb(255, 152, 152, 152) }, //Cobblestone Slab (Double)
            { new Tuple<int, int>(43, 4), Color.FromArgb(255, 225, 104, 73) }, //Brick Slab (Double)
            { new Tuple<int, int>(43, 5), Color.FromArgb(255, 120, 120, 120) }, //Stone Brick Slab (Double)
            { new Tuple<int, int>(43, 6), Color.FromArgb(255, 55, 24, 29) }, //Nether Brick Slab (Double)
            { new Tuple<int, int>(43, 7), Color.FromArgb(255, 234, 230, 224) }, //Quartz Slab (Double)
            { new Tuple<int, int>(43, 8), Color.FromArgb(255, 168, 168, 168) }, //Smooth Stone Slab (Double)
            { new Tuple<int, int>(43, 9), Color.FromArgb(255, 223, 216, 163) }, //Smooth Sandstone Slab (Double)
            { new Tuple<int, int>(44, 0), Color.FromArgb(255, 166, 166, 166) }, //Stone Slab
            { new Tuple<int, int>(44, 1), Color.FromArgb(255, 220, 212, 162) }, //Sandstone Slab
            { new Tuple<int, int>(44, 2), Color.FromArgb(255, 201, 162, 101) }, //Wooden Slab
            { new Tuple<int, int>(44, 3), Color.FromArgb(255, 109, 109, 109) }, //Cobblestone Slab
            { new Tuple<int, int>(44, 4), Color.FromArgb(255, 147, 80, 65) }, //Brick Slab
            { new Tuple<int, int>(44, 5), Color.FromArgb(255, 128, 128, 128) }, //Stone Brick Slab
            { new Tuple<int, int>(44, 6), Color.FromArgb(255, 70, 34, 41) }, //Nether Brick Slab
            { new Tuple<int, int>(44, 7), Color.FromArgb(255, 237, 235, 228) }, //Quartz Slab
            { new Tuple<int, int>(45, 0), Color.FromArgb(255, 188, 48, 6) }, //Brick
            { new Tuple<int, int>(46, 0), Color.FromArgb(255, 175, 38, 0) }, //TNT
            { new Tuple<int, int>(47, 0), Color.FromArgb(255, 107, 88, 57) }, //Bookshelf
            { new Tuple<int, int>(48, 0), Color.FromArgb(255, 101, 135, 101) }, //Moss Stone
            { new Tuple<int, int>(49, 0), Color.FromArgb(255, 20, 18, 29) }, //Obsidian
            { new Tuple<int, int>(50, 0), Color.FromArgb(255, 255, 255, 0) }, //Torch
            { new Tuple<int, int>(51, 0), Color.FromArgb(255, 222, 95, 0) }, //Fire
            { new Tuple<int, int>(52, 0), Color.FromArgb(255, 26, 39, 49) }, //Mob Spawner
            { new Tuple<int, int>(53, 0), Color.FromArgb(255, 166, 135, 78) }, //Wooden Stairs (Oak)
            { new Tuple<int, int>(54, 0), Color.FromArgb(255, 164, 116, 42) }, //Chest
            { new Tuple<int, int>(55, 0), Color.FromArgb(255, 255, 0, 0) }, //Redstone Wire
            { new Tuple<int, int>(56, 0), Color.FromArgb(255, 129, 140, 143) }, //Diamond Ore
            { new Tuple<int, int>(57, 0), Color.FromArgb(255, 97, 219, 213) }, //Block of Diamond
            { new Tuple<int, int>(58, 0), Color.FromArgb(255, 136, 80, 46) }, //Workbench
            { new Tuple<int, int>(59, 0), Color.FromArgb(255, 60, 89, 23) }, //Wheat (Crop)
            { new Tuple<int, int>(60, 0), Color.FromArgb(255, 130, 86, 51) }, //Farmland
            { new Tuple<int, int>(61, 0), Color.FromArgb(255, 103, 103, 103) }, //Furnace
            { new Tuple<int, int>(62, 0), Color.FromArgb(255, 255, 185, 0) }, //Furnace (Smelting)
            { new Tuple<int, int>(63, 0), Color.FromArgb(255, 184, 154, 91) }, //Sign (Block)
            { new Tuple<int, int>(64, 0), Color.FromArgb(255, 148, 115, 56) }, //Wood Door (Block)
            { new Tuple<int, int>(65, 0), Color.FromArgb(255, 121, 95, 52) }, //Ladder
            { new Tuple<int, int>(66, 0), Color.FromArgb(255, 182, 144, 81) }, //Rail
            { new Tuple<int, int>(67, 0), Color.FromArgb(255, 106, 106, 106) }, //Cobblestone Stairs
            { new Tuple<int, int>(68, 0), Color.FromArgb(255, 184, 154, 91) }, //Sign (Wall Block)
            { new Tuple<int, int>(69, 0), Color.FromArgb(255, 106, 89, 64) }, //Lever
            { new Tuple<int, int>(70, 0), Color.FromArgb(255, 122, 122, 122) }, //Stone Pressure Plate
            { new Tuple<int, int>(71, 0), Color.FromArgb(255, 194, 194, 194) }, //Iron Door (Block)
            { new Tuple<int, int>(72, 0), Color.FromArgb(255, 201, 160, 101) }, //Wooden Pressure Plate
            { new Tuple<int, int>(73, 0), Color.FromArgb(255, 132, 107, 107) }, //Redstone Ore
            { new Tuple<int, int>(74, 0), Color.FromArgb(255, 221, 45, 45) }, //Redstone Ore (Glowing)
            { new Tuple<int, int>(75, 0), Color.FromArgb(255, 102, 0, 0) }, //Redstone Torch (Off)
            { new Tuple<int, int>(76, 0), Color.FromArgb(255, 255, 97, 0) }, //Redstone Torch
            { new Tuple<int, int>(77, 0), Color.FromArgb(255, 132, 132, 132) }, //Button (Stone)
            { new Tuple<int, int>(78, 0), Color.FromArgb(255, 223, 241, 241) }, //Snow
            { new Tuple<int, int>(79, 0), Color.FromArgb(255, 125, 173, 255) }, //Ice
            { new Tuple<int, int>(80, 0), Color.FromArgb(255, 239, 251, 251) }, //Snow Block
            { new Tuple<int, int>(81, 0), Color.FromArgb(255, 15, 131, 29) }, //Cactus
            { new Tuple<int, int>(82, 0), Color.FromArgb(255, 158, 164, 176) }, //Clay Block
            { new Tuple<int, int>(83, 0), Color.FromArgb(255, 148, 192, 101) }, //Sugar Cane (Block)
            { new Tuple<int, int>(84, 0), Color.FromArgb(255, 133, 89, 59) }, //Jukebox
            { new Tuple<int, int>(85, 0), Color.FromArgb(255, 141, 116, 66) }, //Fence (Oak)
            { new Tuple<int, int>(86, 0), Color.FromArgb(255, 227, 140, 27) }, //Pumpkin
            { new Tuple<int, int>(87, 0), Color.FromArgb(255, 111, 54, 52) }, //Netherrack
            { new Tuple<int, int>(88, 0), Color.FromArgb(255, 84, 64, 51) }, //Soul Sand
            { new Tuple<int, int>(89, 0), Color.FromArgb(255, 143, 118, 69) }, //Glowstone
            { new Tuple<int, int>(90, 0), Color.FromArgb(255, 87, 10, 191) }, //Portal
            { new Tuple<int, int>(91, 0), Color.FromArgb(255, 241, 152, 33) }, //Jack-O-Lantern
            { new Tuple<int, int>(92, 0), Color.FromArgb(255, 236, 255, 255) }, //Cake (Block)
            { new Tuple<int, int>(93, 0), Color.FromArgb(255, 178, 178, 178) }, //Redstone Repeater (Block Off)
            { new Tuple<int, int>(94, 0), Color.FromArgb(255, 178, 178, 178) }, //Redstone Repeater (Block On)
            { new Tuple<int, int>(95, 0), Color.FromArgb(255, 255, 255, 255) }, //Stained Glass (White)
            { new Tuple<int, int>(95, 1), Color.FromArgb(255, 216, 127, 51) }, //Stained Glass (Orange)
            { new Tuple<int, int>(95, 2), Color.FromArgb(255, 178, 76, 216) }, //Stained Glass (Magenta)
            { new Tuple<int, int>(95, 3), Color.FromArgb(255, 102, 153, 216) }, //Stained Glass (Light Blue)
            { new Tuple<int, int>(95, 4), Color.FromArgb(255, 229, 229, 51) }, //Stained Glass (Yellow)
            { new Tuple<int, int>(95, 5), Color.FromArgb(255, 127, 204, 25) }, //Stained Glass (Lime)
            { new Tuple<int, int>(95, 6), Color.FromArgb(255, 242, 127, 165) }, //Stained Glass (Pink)
            { new Tuple<int, int>(95, 7), Color.FromArgb(255, 76, 76, 76) }, //Stained Glass (Gray)
            { new Tuple<int, int>(95, 8), Color.FromArgb(255, 117, 117, 117) }, //Stained Glass (Light Grey)
            { new Tuple<int, int>(95, 9), Color.FromArgb(255, 76, 127, 153) }, //Stained Glass (Cyan)
            { new Tuple<int, int>(95, 10), Color.FromArgb(255, 127, 63, 178) }, //Stained Glass (Purple)
            { new Tuple<int, int>(95, 11), Color.FromArgb(255, 51, 76, 178) }, //Stained Glass (Blue)
            { new Tuple<int, int>(95, 12), Color.FromArgb(255, 102, 76, 51) }, //Stained Glass (Brown)
            { new Tuple<int, int>(95, 13), Color.FromArgb(255, 102, 127, 51) }, //Stained Glass (Green)
            { new Tuple<int, int>(95, 14), Color.FromArgb(255, 153, 51, 51) }, //Stained Glass (Red)
            { new Tuple<int, int>(95, 15), Color.FromArgb(255, 25, 25, 25) }, //Stained Glass (Black)
            { new Tuple<int, int>(96, 0), Color.FromArgb(255, 126, 93, 45) }, //Trapdoor
            { new Tuple<int, int>(97, 0), Color.FromArgb(255, 124, 124, 124) }, //Monster Egg (Stone)
            { new Tuple<int, int>(97, 1), Color.FromArgb(255, 141, 141, 141) }, //Monster Egg (Cobblestone)
            { new Tuple<int, int>(97, 2), Color.FromArgb(255, 122, 122, 122) }, //Monster Egg (Stone Brick)
            { new Tuple<int, int>(97, 3), Color.FromArgb(255, 105, 120, 81) }, //Monster Egg (Mossy Stone Brick)
            { new Tuple<int, int>(97, 4), Color.FromArgb(255, 104, 104, 104) }, //Monster Egg (Cracked Stone)
            { new Tuple<int, int>(97, 5), Color.FromArgb(255, 118, 118, 118) }, //Monster Egg (Chiseled Stone)
            { new Tuple<int, int>(98, 0), Color.FromArgb(255, 122, 122, 122) }, //Stone Bricks
            { new Tuple<int, int>(98, 1), Color.FromArgb(255, 122, 122, 122) }, //Mossy Stone Bricks
            { new Tuple<int, int>(98, 2), Color.FromArgb(255, 122, 122, 122) }, //Cracked Stone Bricks
            { new Tuple<int, int>(98, 3), Color.FromArgb(255, 122, 122, 122) }, //Chiseled Stone Brick
            { new Tuple<int, int>(99, 0), Color.FromArgb(255, 207, 175, 124) }, //Brown Mushroom (Block)
            { new Tuple<int, int>(100, 0), Color.FromArgb(255, 202, 170, 120) }, //Red Mushroom (Block)
            { new Tuple<int, int>(101, 0), Color.FromArgb(255, 109, 108, 106) }, //Iron Bars
            { new Tuple<int, int>(102, 0), Color.FromArgb(255, 211, 239, 244) }, //Glass Pane
            { new Tuple<int, int>(103, 0), Color.FromArgb(255, 196, 189, 40) }, //Melon (Block)
            { new Tuple<int, int>(104, 0), Color.FromArgb(255, 146, 221, 105) }, //Pumpkin Vine
            { new Tuple<int, int>(105, 0), Color.FromArgb(255, 115, 174, 83) }, //Melon Vine
            { new Tuple<int, int>(106, 0), Color.FromArgb(255, 32, 81, 12) }, //Vines
            { new Tuple<int, int>(107, 0), Color.FromArgb(255, 165, 135, 82) }, //Fence Gate (Oak)
            { new Tuple<int, int>(108, 0), Color.FromArgb(255, 148, 64, 42) }, //Brick Stairs
            { new Tuple<int, int>(109, 0), Color.FromArgb(255, 122, 122, 122) }, //Stone Brick Stairs
            { new Tuple<int, int>(110, 0), Color.FromArgb(255, 138, 113, 117) }, //Mycelium
            { new Tuple<int, int>(111, 0), Color.FromArgb(255, 118, 118, 118) }, //Lily Pad
            { new Tuple<int, int>(112, 0), Color.FromArgb(255, 44, 22, 26) }, //Nether Brick
            { new Tuple<int, int>(113, 0), Color.FromArgb(255, 44, 22, 26) }, //Nether Brick Fence
            { new Tuple<int, int>(114, 0), Color.FromArgb(255, 44, 22, 26) }, //Nether Brick Stairs
            { new Tuple<int, int>(115, 0), Color.FromArgb(255, 166, 40, 45) }, //Nether Wart
            { new Tuple<int, int>(116, 0), Color.FromArgb(255, 84, 196, 177) }, //Enchantment Table
            { new Tuple<int, int>(117, 0), Color.FromArgb(255, 124, 103, 81) }, //Brewing Stand (Block)
            { new Tuple<int, int>(118, 0), Color.FromArgb(255, 73, 73, 73) }, //Cauldron (Block)
            { new Tuple<int, int>(119, 0), Color.FromArgb(255, 52, 52, 52) }, //End Portal
            { new Tuple<int, int>(120, 0), Color.FromArgb(255, 52, 137, 209) }, //End Portal Frame
            { new Tuple<int, int>(121, 0), Color.FromArgb(255, 221, 223, 165) }, //End Stone
            { new Tuple<int, int>(122, 0), Color.FromArgb(255, 12, 9, 15) }, //Dragon Egg
            { new Tuple<int, int>(123, 0), Color.FromArgb(255, 151, 99, 49) }, //Redstone Lamp
            { new Tuple<int, int>(124, 0), Color.FromArgb(255, 227, 160, 66) }, //Redstone Lamp (On)
            { new Tuple<int, int>(125, 0), Color.FromArgb(255, 161, 132, 77) }, //Oak-Wood Slab (Double)
            { new Tuple<int, int>(125, 1), Color.FromArgb(255, 125, 90, 54) }, //Spruce-Wood Slab (Double)
            { new Tuple<int, int>(125, 2), Color.FromArgb(255, 215, 204, 141) }, //Birch-Wood Slab (Double)
            { new Tuple<int, int>(125, 3), Color.FromArgb(255, 183, 133, 96) }, //Jungle-Wood Slab (Double)
            { new Tuple<int, int>(125, 4), Color.FromArgb(255, 169, 88, 48) }, //Acacia Wood Slab (Double)
            { new Tuple<int, int>(125, 5), Color.FromArgb(255, 67, 42, 21) }, //Dark Oak Wood Slab (Double)
            { new Tuple<int, int>(126, 0), Color.FromArgb(255, 158, 133, 73) }, //Oak-Wood Slab
            { new Tuple<int, int>(126, 1), Color.FromArgb(255, 100, 79, 46) }, //Spruce-Wood Slab
            { new Tuple<int, int>(126, 2), Color.FromArgb(255, 235, 225, 155) }, //Birch-Wood Slab
            { new Tuple<int, int>(126, 3), Color.FromArgb(255, 139, 97, 60) }, //Jungle-Wood Slab
            { new Tuple<int, int>(126, 4), Color.FromArgb(255, 171, 92, 51) }, //Acacia Wood Slab
            { new Tuple<int, int>(126, 5), Color.FromArgb(255, 66, 42, 18) }, //Dark Oak Wood Slab
            { new Tuple<int, int>(127, 0), Color.FromArgb(255, 221, 113, 31) }, //Cocoa Plant
            { new Tuple<int, int>(128, 0), Color.FromArgb(255, 231, 226, 168) }, //Sandstone Stairs
            { new Tuple<int, int>(129, 0), Color.FromArgb(255, 109, 128, 116) }, //Emerald Ore
            { new Tuple<int, int>(130, 0), Color.FromArgb(255, 42, 58, 60) }, //Ender Chest
            { new Tuple<int, int>(131, 0), Color.FromArgb(255, 124, 124, 124) }, //Tripwire Hook
            { new Tuple<int, int>(132, 0), Color.FromArgb(255, 90, 90, 90) }, //Tripwire
            { new Tuple<int, int>(133, 0), Color.FromArgb(255, 81, 217, 117) }, //Block of Emerald
            { new Tuple<int, int>(134, 0), Color.FromArgb(255, 129, 94, 52) }, //Wooden Stairs (Spruce)
            { new Tuple<int, int>(135, 0), Color.FromArgb(255, 206, 192, 132) }, //Wooden Stairs (Birch)
            { new Tuple<int, int>(136, 0), Color.FromArgb(255, 136, 95, 69) }, //Wooden Stairs (Jungle)
            { new Tuple<int, int>(137, 0), Color.FromArgb(255, 142, 139, 134) }, //Command Block
            { new Tuple<int, int>(138, 0), Color.FromArgb(255, 116, 221, 215) }, //Beacon
            { new Tuple<int, int>(139, 0), Color.FromArgb(255, 89, 89, 89) }, //Cobblestone Wall
            { new Tuple<int, int>(139, 1), Color.FromArgb(255, 42, 94, 42) }, //Mossy Cobblestone Wall
            { new Tuple<int, int>(140, 0), Color.FromArgb(255, 118, 65, 51) }, //Flower Pot (Block)
            { new Tuple<int, int>(141, 0), Color.FromArgb(255, 10, 140, 0) }, //Carrot (Crop)
            { new Tuple<int, int>(142, 0), Color.FromArgb(255, 4, 164, 23) }, //Potatoes (Crop)
            { new Tuple<int, int>(143, 0), Color.FromArgb(255, 179, 146, 89) }, //Button (Wood)
            { new Tuple<int, int>(144, 0), Color.FromArgb(255, 176, 176, 176) }, //Head Block (Skeleton)
            { new Tuple<int, int>(144, 1), Color.FromArgb(255, 79, 85, 85) }, //Head Block (Wither)
            { new Tuple<int, int>(144, 2), Color.FromArgb(255, 98, 146, 75) }, //Head Block (Zombie)
            { new Tuple<int, int>(144, 3), Color.FromArgb(255, 204, 151, 126) }, //Head Block (Steve)
            { new Tuple<int, int>(144, 4), Color.FromArgb(255, 82, 175, 67) }, //Head Block (Creeper)
            { new Tuple<int, int>(145, 0), Color.FromArgb(255, 71, 67, 67) }, //Anvil
            { new Tuple<int, int>(145, 1), Color.FromArgb(255, 71, 67, 67) }, //Anvil (Slightly Damaged)
            { new Tuple<int, int>(145, 2), Color.FromArgb(255, 71, 67, 67) }, //Anvil (Very Damaged)
            { new Tuple<int, int>(146, 0), Color.FromArgb(255, 158, 107, 29) }, //Trapped Chest
            { new Tuple<int, int>(147, 0), Color.FromArgb(255, 254, 253, 112) }, //Weighted Pressure Plate (Light)
            { new Tuple<int, int>(148, 0), Color.FromArgb(255, 229, 229, 229) }, //Weighted Pressure Plate (Heavy)
            { new Tuple<int, int>(149, 0), Color.FromArgb(255, 75, 74, 76) }, //Redstone Comparator (Off)
            { new Tuple<int, int>(150, 0), Color.FromArgb(255, 191, 198, 189) }, //Redstone Comparator (On)
            { new Tuple<int, int>(151, 0), Color.FromArgb(255, 251, 237, 221) }, //Daylight Sensor
            { new Tuple<int, int>(152, 0), Color.FromArgb(255, 171, 27, 9) }, //Block of Redstone
            { new Tuple<int, int>(153, 0), Color.FromArgb(255, 125, 84, 79) }, //Nether Quartz Ore
            { new Tuple<int, int>(154, 0), Color.FromArgb(255, 113, 113, 113) }, //Hopper
            { new Tuple<int, int>(155, 0), Color.FromArgb(255, 234, 230, 223) }, //Quartz Block
            { new Tuple<int, int>(155, 1), Color.FromArgb(255, 224, 219, 210) }, //Chiseled Quartz Block
            { new Tuple<int, int>(155, 2), Color.FromArgb(255, 234, 231, 225) }, //Pillar Quartz Block
            { new Tuple<int, int>(156, 0), Color.FromArgb(255, 235, 232, 227) }, //Quartz Stairs
            { new Tuple<int, int>(157, 0), Color.FromArgb(255, 155, 129, 65) }, //Rail (Activator)
            { new Tuple<int, int>(158, 0), Color.FromArgb(255, 116, 116, 116) }, //Dropper
            { new Tuple<int, int>(159, 0), Color.FromArgb(255, 209, 178, 161) }, //Stained Clay (White)
            { new Tuple<int, int>(159, 1), Color.FromArgb(255, 161, 83, 37) }, //Stained Clay (Orange)
            { new Tuple<int, int>(159, 2), Color.FromArgb(255, 149, 88, 108) }, //Stained Clay (Magenta)
            { new Tuple<int, int>(159, 3), Color.FromArgb(255, 113, 108, 137) }, //Stained Clay (Light Blue)
            { new Tuple<int, int>(159, 4), Color.FromArgb(255, 186, 133, 35) }, //Stained Clay (Yellow)
            { new Tuple<int, int>(159, 5), Color.FromArgb(255, 103, 117, 52) }, //Stained Clay (Lime)
            { new Tuple<int, int>(159, 6), Color.FromArgb(255, 161, 78, 78) }, //Stained Clay (Pink)
            { new Tuple<int, int>(159, 7), Color.FromArgb(255, 57, 42, 35) }, //Stained Clay (Gray)
            { new Tuple<int, int>(159, 8), Color.FromArgb(255, 135, 104, 95) }, //Stained Clay (Light Gray)
            { new Tuple<int, int>(159, 9), Color.FromArgb(255, 86, 91, 91) }, //Stained Clay (Cyan)
            { new Tuple<int, int>(159, 10), Color.FromArgb(255, 118, 70, 86) }, //Stained Clay (Purple)
            { new Tuple<int, int>(159, 11), Color.FromArgb(255, 74, 59, 91) }, //Stained Clay (Blue)
            { new Tuple<int, int>(159, 12), Color.FromArgb(255, 77, 51, 35) }, //Stained Clay (Brown)
            { new Tuple<int, int>(159, 13), Color.FromArgb(255, 76, 83, 42) }, //Stained Clay (Green)
            { new Tuple<int, int>(159, 14), Color.FromArgb(255, 143, 61, 46) }, //Stained Clay (Red)
            { new Tuple<int, int>(159, 15), Color.FromArgb(255, 37, 22, 16) }, //Stained Clay (Black)
            { new Tuple<int, int>(160, 0), Color.FromArgb(255, 246, 246, 246) }, //Stained Glass Pane (White)
            { new Tuple<int, int>(160, 1), Color.FromArgb(255, 208, 122, 48) }, //Stained Glass Pane (Orange)
            { new Tuple<int, int>(160, 2), Color.FromArgb(255, 171, 73, 208) }, //Stained Glass Pane (Magenta)
            { new Tuple<int, int>(160, 3), Color.FromArgb(255, 97, 147, 208) }, //Stained Glass Pane (Light Blue)
            { new Tuple<int, int>(160, 4), Color.FromArgb(255, 221, 221, 48) }, //Stained Glass Pane (Yellow)
            { new Tuple<int, int>(160, 5), Color.FromArgb(255, 122, 196, 24) }, //Stained Glass Pane (Lime)
            { new Tuple<int, int>(160, 6), Color.FromArgb(255, 233, 122, 159) }, //Stained Glass Pane (Pink)
            { new Tuple<int, int>(160, 7), Color.FromArgb(255, 73, 73, 73) }, //Stained Glass Pane (Gray)
            { new Tuple<int, int>(160, 8), Color.FromArgb(255, 145, 145, 145) }, //Stained Glass Pane (Light Gray)
            { new Tuple<int, int>(160, 9), Color.FromArgb(255, 73, 122, 147) }, //Stained Glass Pane (Cyan)
            { new Tuple<int, int>(160, 10), Color.FromArgb(255, 122, 61, 171) }, //Stained Glass Pane (Purple)
            { new Tuple<int, int>(160, 11), Color.FromArgb(255, 48, 73, 171) }, //Stained Glass Pane (Blue)
            { new Tuple<int, int>(160, 12), Color.FromArgb(255, 97, 73, 48) }, //Stained Glass Pane (Brown)
            { new Tuple<int, int>(160, 13), Color.FromArgb(255, 97, 122, 48) }, //Stained Glass Pane (Green)
            { new Tuple<int, int>(160, 14), Color.FromArgb(255, 147, 48, 48) }, //Stained Glass Pane (Red)
            { new Tuple<int, int>(160, 15), Color.FromArgb(255, 24, 24, 24) }, //Stained Glass Pane (Black)
            { new Tuple<int, int>(161, 0), Color.FromArgb(255, 135, 135, 135) }, //Leaves (Acacia)
            { new Tuple<int, int>(161, 1), Color.FromArgb(255, 55, 104, 33) }, //Leaves (Dark Oak)
            { new Tuple<int, int>(162, 0), Color.FromArgb(255, 176, 90, 57) }, //Wood (Acacia Oak)
            { new Tuple<int, int>(162, 1), Color.FromArgb(255, 93, 74, 49) }, //Wood (Dark Oak)
            { new Tuple<int, int>(163, 0), Color.FromArgb(255, 172, 92, 50) }, //Wooden Stairs (Acacia)
            { new Tuple<int, int>(164, 0), Color.FromArgb(255, 71, 44, 21) }, //Wooden Stairs (Dark Oak)
            { new Tuple<int, int>(165, 0), Color.FromArgb(255, 120, 200, 101) }, //Slime Block
            { new Tuple<int, int>(166, 0), Color.FromArgb(255, 223, 52, 53) }, //Barrier
            { new Tuple<int, int>(167, 0), Color.FromArgb(255, 199, 199, 199) }, //Iron Trapdoor
            { new Tuple<int, int>(168, 0), Color.FromArgb(255, 114, 175, 165) }, //Prismarine
            { new Tuple<int, int>(168, 1), Color.FromArgb(255, 92, 158, 143) }, //Prismarine Bricks
            { new Tuple<int, int>(168, 2), Color.FromArgb(255, 72, 106, 94) }, //Dark Prismarine
            { new Tuple<int, int>(169, 0), Color.FromArgb(255, 172, 199, 190) }, //Sea Lantern
            { new Tuple<int, int>(170, 0), Color.FromArgb(255, 220, 211, 159) }, //Hay Bale
            { new Tuple<int, int>(171, 0), Color.FromArgb(255, 202, 202, 202) }, //Carpet (White)
            { new Tuple<int, int>(171, 1), Color.FromArgb(255, 221, 133, 75) }, //Carpet (Orange)
            { new Tuple<int, int>(171, 2), Color.FromArgb(255, 177, 67, 186) }, //Carpet (Magenta)
            { new Tuple<int, int>(171, 3), Color.FromArgb(255, 75, 113, 189) }, //Carpet (Light Blue)
            { new Tuple<int, int>(171, 4), Color.FromArgb(255, 197, 183, 44) }, //Carpet (Yellow)
            { new Tuple<int, int>(171, 5), Color.FromArgb(255, 60, 161, 51) }, //Carpet (Lime)
            { new Tuple<int, int>(171, 6), Color.FromArgb(255, 206, 142, 168) }, //Carpet (Pink)
            { new Tuple<int, int>(171, 7), Color.FromArgb(255, 70, 70, 70) }, //Carpet (Grey)
            { new Tuple<int, int>(171, 8), Color.FromArgb(255, 162, 162, 162) }, //Carpet (Light Gray)
            { new Tuple<int, int>(171, 9), Color.FromArgb(255, 48, 116, 145) }, //Carpet (Cyan)
            { new Tuple<int, int>(171, 10), Color.FromArgb(255, 148, 81, 202) }, //Carpet (Purple)
            { new Tuple<int, int>(171, 11), Color.FromArgb(255, 54, 69, 171) }, //Carpet (Blue)
            { new Tuple<int, int>(171, 12), Color.FromArgb(255, 82, 52, 32) }, //Carpet (Brown)
            { new Tuple<int, int>(171, 13), Color.FromArgb(255, 62, 85, 33) }, //Carpet (Green)
            { new Tuple<int, int>(171, 14), Color.FromArgb(255, 187, 61, 57) }, //Carpet (Red)
            { new Tuple<int, int>(171, 15), Color.FromArgb(255, 35, 31, 31) }, //Carpet (Black)
            { new Tuple<int, int>(172, 0), Color.FromArgb(255, 150, 92, 66) }, //Hardened Clay
            { new Tuple<int, int>(173, 0), Color.FromArgb(255, 18, 18, 18) }, //Block of Coal
            { new Tuple<int, int>(174, 0), Color.FromArgb(255, 162, 191, 244) }, //Packed Ice
            { new Tuple<int, int>(175, 0), Color.FromArgb(255, 207, 116, 20) }, //Sunflower
            { new Tuple<int, int>(175, 1), Color.FromArgb(255, 168, 112, 178) }, //Lilac
            { new Tuple<int, int>(175, 2), Color.FromArgb(255, 102, 158, 88) }, //Double Tallgrass
            { new Tuple<int, int>(175, 3), Color.FromArgb(255, 84, 129, 72) }, //Large Fern
            { new Tuple<int, int>(175, 4), Color.FromArgb(255, 215, 2, 8) }, //Rose Bush
            { new Tuple<int, int>(175, 5), Color.FromArgb(255, 192, 150, 207) }, //Peony
            { new Tuple<int, int>(176, 0), Color.FromArgb(255, 240, 240, 240) }, //Standing Banner (Block)
            { new Tuple<int, int>(177, 0), Color.FromArgb(255, 240, 240, 240) }, //Wall Banner (Block)
            { new Tuple<int, int>(178, 0), Color.FromArgb(255, 240, 240, 240) }, //Inverted Daylight Sensor
            { new Tuple<int, int>(179, 0), Color.FromArgb(255, 172, 86, 29) }, //Red Sandstone
            { new Tuple<int, int>(179, 1), Color.FromArgb(255, 172, 86, 29) }, //Red Sandstone (Chiseled)
            { new Tuple<int, int>(179, 2), Color.FromArgb(255, 172, 86, 29) }, //Red Sandstone (Smooth)
            { new Tuple<int, int>(180, 0), Color.FromArgb(255, 174, 87, 29) }, //Red Sandstone Stairs
            { new Tuple<int, int>(181, 0), Color.FromArgb(255, 174, 87, 29) }, //Red Sandstone Slab (Double)
            { new Tuple<int, int>(182, 0), Color.FromArgb(255, 174, 87, 29) }, //Red Sandstone Slab
            { new Tuple<int, int>(183, 0), Color.FromArgb(255, 80, 60, 36) }, //Fence Gate (Spruce)
            { new Tuple<int, int>(184, 0), Color.FromArgb(255, 221, 205, 141) }, //Fence Gate (Birch)
            { new Tuple<int, int>(185, 0), Color.FromArgb(255, 175, 122, 77) }, //Fence Gate (Jungle)
            { new Tuple<int, int>(186, 0), Color.FromArgb(255, 52, 32, 14) }, //Fence Gate (Dark Oak)
            { new Tuple<int, int>(187, 0), Color.FromArgb(255, 207, 107, 54) }, //Fence Gate (Acacia)
            { new Tuple<int, int>(188, 0), Color.FromArgb(255, 126, 93, 53) }, //Fence (Spruce)
            { new Tuple<int, int>(189, 0), Color.FromArgb(255, 199, 184, 123) }, //Fence (Birch)
            { new Tuple<int, int>(190, 0), Color.FromArgb(255, 187, 134, 95) }, //Fence (Jungle)
            { new Tuple<int, int>(191, 0), Color.FromArgb(255, 63, 46, 30) }, //Fence (Dark Oak)
            { new Tuple<int, int>(192, 0), Color.FromArgb(255, 197, 107, 58) }, //Fence (Acacia)
            { new Tuple<int, int>(193, 0), Color.FromArgb(255, 110, 83, 48) }, //Wooden Door Block (Spruce)
            { new Tuple<int, int>(194, 0), Color.FromArgb(255, 247, 243, 224) }, //Wooden Door Block (Birch)
            { new Tuple<int, int>(195, 0), Color.FromArgb(255, 169, 119, 80) }, //Wooden Door Block (Jungle)
            { new Tuple<int, int>(196, 0), Color.FromArgb(255, 170, 85, 41) }, //Wooden Door Block (Acacia)
            { new Tuple<int, int>(197, 0), Color.FromArgb(255, 78, 55, 33) }, //Wooden Door Block (Dark Oak)
            { new Tuple<int, int>(198, 0), Color.FromArgb(255, 220, 197, 205) }, //End rod
            { new Tuple<int, int>(199, 0), Color.FromArgb(255, 96, 59, 96) }, //Chorus Plant
            { new Tuple<int, int>(200, 0), Color.FromArgb(255, 133, 103, 133) }, //Chorus Flower
            { new Tuple<int, int>(201, 0), Color.FromArgb(255, 166, 121, 166) }, //Purpur Block
            { new Tuple<int, int>(202, 0), Color.FromArgb(255, 170, 126, 170) }, //Purpur Pillar
            { new Tuple<int, int>(203, 0), Color.FromArgb(255, 168, 121, 168) }, //Purpur Stairs
            { new Tuple<int, int>(204, 0), Color.FromArgb(255, 168, 121, 168) }, //Purpur Slab (Double)
            { new Tuple<int, int>(205, 0), Color.FromArgb(255, 168, 121, 168) }, //Purpur Slab
            { new Tuple<int, int>(206, 0), Color.FromArgb(255, 225, 230, 170) }, //End Stone Bricks
            { new Tuple<int, int>(207, 0), Color.FromArgb(255, 179, 134, 0) }, //Beetroot Block
            { new Tuple<int, int>(208, 0), Color.FromArgb(255, 152, 125, 69) }, //Grass Path
            { new Tuple<int, int>(209, 0), Color.FromArgb(255, 240, 240, 240) }, //End Gateway
            { new Tuple<int, int>(210, 0), Color.FromArgb(255, 155, 137, 39) }, //Repeating Command Block
            { new Tuple<int, int>(211, 0), Color.FromArgb(255, 118, 178, 151) }, //Chain Command Block
            { new Tuple<int, int>(212, 0), Color.FromArgb(255, 118, 162, 252) }, //Frosted Ice
            { new Tuple<int, int>(213, 0), Color.FromArgb(255, 202, 78, 6) }, //Magma Block
            { new Tuple<int, int>(214, 0), Color.FromArgb(255, 129, 0, 8) }, //Nether Wart Block
            { new Tuple<int, int>(215, 0), Color.FromArgb(255, 86, 0, 4) }, //Red Nether Brick
            { new Tuple<int, int>(216, 0), Color.FromArgb(255, 143, 147, 131) }, //Bone Block
            { new Tuple<int, int>(217, 0), Color.FromArgb(0, 0, 0, 0) }, //Void Block
            { new Tuple<int, int>(218, 0), Color.FromArgb(255, 43, 43, 43) }, //Observer
            { new Tuple<int, int>(219, 0), Color.FromArgb(255, 223, 223, 220) }, //White Shulker Box
            { new Tuple<int, int>(220, 0), Color.FromArgb(255, 208, 118, 59) }, //Orange Shulker Box
            { new Tuple<int, int>(221, 0), Color.FromArgb(255, 186, 100, 194) }, //Magenta Shulker Box
            { new Tuple<int, int>(222, 0), Color.FromArgb(255, 103, 143, 204) }, //Light Blue Shulker Box
            { new Tuple<int, int>(223, 0), Color.FromArgb(255, 193, 183, 61) }, //Yellow Shulker Box
            { new Tuple<int, int>(224, 0), Color.FromArgb(255, 73, 185, 61) }, //Lime Shulker Box
            { new Tuple<int, int>(225, 0), Color.FromArgb(255, 208, 140, 161) }, //Pink Shulker Box
            { new Tuple<int, int>(226, 0), Color.FromArgb(255, 84, 82, 82) }, //Gray Shulker Box
            { new Tuple<int, int>(227, 0), Color.FromArgb(255, 165, 162, 162) }, //Light Gray Shulker Box
            { new Tuple<int, int>(228, 0), Color.FromArgb(255, 69, 137, 165) }, //Cyan Shulker Box
            { new Tuple<int, int>(229, 0), Color.FromArgb(255, 151, 105, 151) }, //Purple Shulker Box
            { new Tuple<int, int>(230, 0), Color.FromArgb(255, 102, 114, 202) }, //Blue Shulker Box
            { new Tuple<int, int>(231, 0), Color.FromArgb(255, 142, 113, 94) }, //Brown Shulker Box
            { new Tuple<int, int>(232, 0), Color.FromArgb(255, 112, 131, 85) }, //Green Shulker Box
            { new Tuple<int, int>(233, 0), Color.FromArgb(255, 195, 89, 86) }, //Red Shulker Box
            { new Tuple<int, int>(234, 0), Color.FromArgb(255, 58, 55, 55) }, //Black Shulker Box
            { new Tuple<int, int>(235, 0), Color.FromArgb(255, 249, 255, 254) }, //White Glazed Terracota
            { new Tuple<int, int>(236, 0), Color.FromArgb(255, 225, 97, 0) }, //Orange Glazed Terracota
            { new Tuple<int, int>(237, 0), Color.FromArgb(255, 241, 165, 191) }, //Magenta Glazed Terracota
            { new Tuple<int, int>(238, 0), Color.FromArgb(255, 77, 185, 221) }, //Light Blue Glazed Terracota
            { new Tuple<int, int>(239, 0), Color.FromArgb(255, 238, 170, 13) }, //Yellow Glazed Terracota
            { new Tuple<int, int>(240, 0), Color.FromArgb(255, 133, 207, 33) }, //Lime Glazed Terracota
            { new Tuple<int, int>(241, 0), Color.FromArgb(255, 244, 181, 203) }, //Pink Glazed Terracota
            { new Tuple<int, int>(242, 0), Color.FromArgb(255, 96, 114, 119) }, //Gray Glazed Terracota
            { new Tuple<int, int>(243, 0), Color.FromArgb(255, 204, 208, 210) }, //Light Gray Glazed Terracota
            { new Tuple<int, int>(244, 0), Color.FromArgb(255, 23, 168, 168) }, //Cyan Glazed Terracota
            { new Tuple<int, int>(245, 0), Color.FromArgb(255, 100, 31, 156) }, //Purple Glazed Terracota
            { new Tuple<int, int>(246, 0), Color.FromArgb(255, 44, 46, 143) }, //Blue Glazed Terracota
            { new Tuple<int, int>(247, 0), Color.FromArgb(255, 171, 123, 80) }, //Brown Glazed Terracota
            { new Tuple<int, int>(248, 0), Color.FromArgb(255, 117, 160, 37) }, //Green Glazed Terracota
            { new Tuple<int, int>(249, 0), Color.FromArgb(255, 209, 86, 80) }, //Red Glazed Terracota
            { new Tuple<int, int>(250, 0), Color.FromArgb(255, 62, 14, 14) }, //Black Glazed Terracota
            { new Tuple<int, int>(251, 0), Color.FromArgb(255, 207, 213, 214) }, //White Concrete
            { new Tuple<int, int>(251, 1), Color.FromArgb(255, 224, 97, 0) }, //Orange Concrete
            { new Tuple<int, int>(251, 2), Color.FromArgb(255, 169, 48, 159) }, //Magenta Concrete
            { new Tuple<int, int>(251, 3), Color.FromArgb(255, 35, 137, 199) }, //Light Blue Concrete
            { new Tuple<int, int>(251, 4), Color.FromArgb(255, 239, 174, 21) }, //Yellow Concrete
            { new Tuple<int, int>(251, 5), Color.FromArgb(255, 95, 170, 25) }, //Lime Concrete
            { new Tuple<int, int>(251, 6), Color.FromArgb(255, 213, 100, 142) }, //Pink Concrete
            { new Tuple<int, int>(251, 7), Color.FromArgb(255, 54, 57, 61) }, //Gray Concrete
            { new Tuple<int, int>(251, 8), Color.FromArgb(255, 125, 125, 115) }, //Light Gray Concrete
            { new Tuple<int, int>(251, 9), Color.FromArgb(255, 21, 119, 136) }, //Cyan Concrete
            { new Tuple<int, int>(251, 10), Color.FromArgb(255, 99, 31, 155) }, //Purple Concrete
            { new Tuple<int, int>(251, 11), Color.FromArgb(255, 45, 47, 144) }, //Blue Concrete
            { new Tuple<int, int>(251, 12), Color.FromArgb(255, 97, 60, 32) }, //Brown Concrete
            { new Tuple<int, int>(251, 13), Color.FromArgb(255, 73, 91, 36) }, //Green Concrete
            { new Tuple<int, int>(251, 14), Color.FromArgb(255, 143, 33, 33) }, //Red Concrete
            { new Tuple<int, int>(251, 15), Color.FromArgb(255, 7, 9, 14) }, //Black Concrete
            { new Tuple<int, int>(252, 0), Color.FromArgb(255, 230, 232, 233) }, //White Concrete Powder
            { new Tuple<int, int>(252, 1), Color.FromArgb(255, 234, 136, 34) }, //Orange Concrete Powder
            { new Tuple<int, int>(252, 2), Color.FromArgb(255, 195, 81, 186) }, //Magenta Concrete Powder
            { new Tuple<int, int>(252, 3), Color.FromArgb(255, 90, 197, 221) }, //Light Blue Concrete Powder
            { new Tuple<int, int>(252, 4), Color.FromArgb(255, 237, 198, 48) }, //Yellow Concrete Powder
            { new Tuple<int, int>(252, 5), Color.FromArgb(255, 128, 191, 41) }, //Lime Concrete Powder
            { new Tuple<int, int>(252, 6), Color.FromArgb(255, 239, 174, 197) }, //Pink Concrete Powder
            { new Tuple<int, int>(252, 7), Color.FromArgb(255, 76, 80, 85) }, //Gray Concrete Powder
            { new Tuple<int, int>(252, 8), Color.FromArgb(255, 154, 154, 147) }, //Light Gray Concrete Powder
            { new Tuple<int, int>(252, 9), Color.FromArgb(255, 35, 148, 154) }, //Cyan Concrete Powder
            { new Tuple<int, int>(252, 10), Color.FromArgb(255, 120, 48, 169) }, //Purple Concrete Powder
            { new Tuple<int, int>(252, 11), Color.FromArgb(255, 72, 74, 171) }, //Blue Concrete Powder
            { new Tuple<int, int>(252, 12), Color.FromArgb(255, 121, 81, 51) }, //Brown Concrete Powder
            { new Tuple<int, int>(252, 13), Color.FromArgb(255, 103, 124, 55) }, //Green Concrete Powder
            { new Tuple<int, int>(252, 14), Color.FromArgb(255, 164, 51, 50) }, //Red Concrete Powder
            { new Tuple<int, int>(252, 15), Color.FromArgb(255, 22, 24, 28) }, //Black Concrete Powder

            { new Tuple<int, int>(255, 0), Color.FromArgb(255, 50, 33, 36) }, //Structure Block

        };

        public static int GetColorIntensity(this Color color)
        {
            return color.R + color.G + color.B;
        }

        public static uint ColorToUInt(this Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) |
                          (color.G << 8) | (color.B << 0));
        }

        public static uint ByteArrayToUInt(byte r, byte g, byte b, byte a)
        {
            return (uint) ((a << 24) | (r << 16) | (g << 8) | (b << 0));
        }

        public static Color UIntToColor(this uint color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
            => new HashSet<T>(source, comparer);

        public static Color GetBlockColor(int blockID, int data)
        {
            if (_colors.TryGetValue(new Tuple<int, int>(blockID, data), out Color color))
            {
                return color;
            }
            return _colors[new Tuple<int, int>(blockID, 0)];
        }
    }
}
