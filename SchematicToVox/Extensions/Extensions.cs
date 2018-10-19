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
            { new Tuple<int, int>(1, 0), new Color32(125, 125, 125, 255) }, //Stone
            { new Tuple<int, int>(1, 1), new Color32(153, 113, 98, 255) }, //Granite
            { new Tuple<int, int>(1, 2), new Color32(159, 114, 98, 255) }, //Polished Granite
            { new Tuple<int, int>(1, 3), new Color32(179, 179, 182, 255) }, //Diorite
            { new Tuple<int, int>(1, 4), new Color32(183, 183, 185, 255) }, //Polished Diorite
            { new Tuple<int, int>(1, 5), new Color32(130, 131, 131, 255) }, //Andesite
            { new Tuple<int, int>(1, 6), new Color32(133, 133, 134, 255) }, //Polished Andesite
            { new Tuple<int, int>(2, 0), new Color32(118, 179, 76, 255) }, //Grass
            { new Tuple<int, int>(3, 0), new Color32(134, 96, 67, 255) }, //Dirt
            { new Tuple<int, int>(3, 1), new Color32(134, 96, 67, 255) }, //Coarse Dirt
            { new Tuple<int, int>(3, 2), new Color32(134, 96, 67, 255) }, //Podzol
            { new Tuple<int, int>(4, 0), new Color32(122, 122, 122, 255) }, //Cobblestone
            { new Tuple<int, int>(5, 0), new Color32(156, 127, 78, 255) }, //Wooden Plank (Oak)
            { new Tuple<int, int>(5, 1), new Color32(103, 77, 46, 255) }, //Wooden Plank (Spruce)
            { new Tuple<int, int>(5, 2), new Color32(195, 179, 123, 255) }, //Wooden Plank (Birch)
            { new Tuple<int, int>(5, 3), new Color32(154, 110, 77, 255) }, //Wooden Plank (Jungle)
            { new Tuple<int, int>(5, 4), new Color32(169, 91, 51, 255) }, //Wooden Plank (Acacia)
            { new Tuple<int, int>(5, 5), new Color32(61, 39, 18, 255) }, //Wooden Plank (Dark Oak)
            { new Tuple<int, int>(6, 0), new Color32(71, 102, 37, 255) }, //Sapling (Oak)
            { new Tuple<int, int>(6, 1), new Color32(51, 58, 33, 255) }, //Sapling (Spruce)
            { new Tuple<int, int>(6, 2), new Color32(118, 150, 84, 255) }, //Sapling (Birch)
            { new Tuple<int, int>(6, 3), new Color32(48, 86, 18, 255) }, //Sapling (Jungle)
            { new Tuple<int, int>(6, 4), new Color32(114, 115, 20, 255) }, //Sapling (Acacia)
            { new Tuple<int, int>(6, 5), new Color32(56, 86, 28, 255) }, //Sapling (Dark Oak)
            { new Tuple<int, int>(7, 0), new Color32(83, 83, 83, 255) }, //Bedrock
            { new Tuple<int, int>(8, 0), new Color32(112, 175, 220, 255) }, //Water
            { new Tuple<int, int>(9, 0), new Color32(112, 175, 220, 255) }, //Water (No Spread)
            { new Tuple<int, int>(10, 0), new Color32(207, 91, 19, 255) }, //Lava
            { new Tuple<int, int>(11, 0), new Color32(212, 90, 18, 255) }, //Lava (No Spread)
            { new Tuple<int, int>(12, 0), new Color32(219, 211, 160, 255) }, //Sand
            { new Tuple<int, int>(12, 1), new Color32(167, 87, 32, 255) }, //Red Sand
            { new Tuple<int, int>(13, 0), new Color32(126, 124, 122, 255) }, //Gravel
            { new Tuple<int, int>(14, 0), new Color32(143, 139, 124, 255) }, //Gold Ore
            { new Tuple<int, int>(15, 0), new Color32(135, 130, 126, 255) }, //Iron Ore
            { new Tuple<int, int>(16, 0), new Color32(115, 115, 115, 255) }, //Coal Ore
            { new Tuple<int, int>(17, 0), new Color32(102, 81, 49, 255) }, //Wood (Oak)
            { new Tuple<int, int>(17, 1), new Color32(45, 28, 12, 255) }, //Wood (Spruce)
            { new Tuple<int, int>(17, 2), new Color32(206, 206, 201, 255) }, //Wood (Birch)
            { new Tuple<int, int>(17, 3), new Color32(87, 67, 26, 255) }, //Wood (Jungle)
            { new Tuple<int, int>(17, 4), new Color32(88, 69, 39, 255) }, //Wood (Oak 4)
            { new Tuple<int, int>(17, 5), new Color32(36, 20, 5, 255) }, //Wood (Oak 5)
            { new Tuple<int, int>(18, 0), new Color32(69, 178, 49, 255) }, //Leaves (Oak)
            { new Tuple<int, int>(18, 1), new Color32(116, 116, 116, 255) }, //Leaves (Spruce)
            { new Tuple<int, int>(18, 2), new Color32(135, 135, 135, 255) }, //Leaves (Birch)
            { new Tuple<int, int>(18, 3), new Color32(45, 125, 16, 255) }, //Leaves (Jungle)
            { new Tuple<int, int>(19, 0), new Color32(194, 195, 84, 255) }, //Sponge
            { new Tuple<int, int>(19, 1), new Color32(153, 148, 53, 255) }, //Wet Sponge
            { new Tuple<int, int>(20, 0), new Color32(218, 240, 244, 255) }, //Glass
            { new Tuple<int, int>(21, 0), new Color32(102, 112, 134, 255) }, //Lapis Lazuli Ore
            { new Tuple<int, int>(22, 0), new Color32(38, 67, 137, 255) }, //Lapis Lazuli Block
            { new Tuple<int, int>(23, 0), new Color32(131, 131, 131, 255) }, //Dispenser
            { new Tuple<int, int>(24, 0), new Color32(220, 211, 159, 255) }, //Sandstone
            { new Tuple<int, int>(24, 1), new Color32(220, 211, 159, 255) }, //Sandstone (Chiseled)
            { new Tuple<int, int>(24, 2), new Color32(220, 211, 159, 255) }, //Sandstone (Smooth)
            { new Tuple<int, int>(25, 0), new Color32(100, 67, 50, 255) }, //Note Block
            { new Tuple<int, int>(26, 0), new Color32(180, 52, 54, 255) }, //Bed (Block)
            { new Tuple<int, int>(27, 0), new Color32(246, 199, 35, 255) }, //Rail (Powered)
            { new Tuple<int, int>(28, 0), new Color32(124, 124, 124, 255) }, //Rail (Detector)
            { new Tuple<int, int>(29, 0), new Color32(142, 191, 119, 255) }, //Sticky Piston
            { new Tuple<int, int>(30, 0), new Color32(220, 220, 220, 255) }, //Cobweb
            { new Tuple<int, int>(31, 0), new Color32(149, 101, 41, 255) }, //Tall Grass (Dead Shrub)
            { new Tuple<int, int>(31, 1), new Color32(2, 145, 36, 255) }, //Tall Grass
            { new Tuple<int, int>(31, 2), new Color32(91, 125, 56, 255) }, //Tall Grass (Fern)
            { new Tuple<int, int>(32, 0), new Color32(123, 79, 25, 255) }, //Dead Shrub
            { new Tuple<int, int>(33, 0), new Color32(165, 139, 83, 255) }, //Piston
            { new Tuple<int, int>(34, 0), new Color32(189, 150, 94, 255) }, //Piston (Head)
            { new Tuple<int, int>(35, 0), new Color32(221, 221, 221, 255) }, //Wool
            { new Tuple<int, int>(35, 1), new Color32(219, 125, 62, 255) }, //Orange Wool
            { new Tuple<int, int>(35, 2), new Color32(179, 80, 188, 255) }, //Magenta Wool
            { new Tuple<int, int>(35, 3), new Color32(106, 138, 201, 255) }, //Light Blue Wool
            { new Tuple<int, int>(35, 4), new Color32(177, 166, 39, 255) }, //Yellow Wool
            { new Tuple<int, int>(35, 5), new Color32(65, 174, 56, 255) }, //Lime Wool
            { new Tuple<int, int>(35, 6), new Color32(208, 132, 153, 255) }, //Pink Wool
            { new Tuple<int, int>(35, 7), new Color32(64, 64, 64, 255) }, //Gray Wool
            { new Tuple<int, int>(35, 8), new Color32(154, 161, 161, 255) }, //Light Gray Wool
            { new Tuple<int, int>(35, 9), new Color32(46, 110, 137, 255) }, //Cyan Wool
            { new Tuple<int, int>(35, 10), new Color32(126, 61, 181, 255) }, //Purple Wool
            { new Tuple<int, int>(35, 11), new Color32(46, 56, 141, 255) }, //Blue Wool
            { new Tuple<int, int>(35, 12), new Color32(79, 50, 31, 255) }, //Brown Wool
            { new Tuple<int, int>(35, 13), new Color32(53, 70, 27, 255) }, //Green Wool
            { new Tuple<int, int>(35, 14), new Color32(150, 52, 48, 255) }, //Red Wool
            { new Tuple<int, int>(35, 15), new Color32(25, 22, 22, 255) }, //Black Wool
            { new Tuple<int, int>(36, 0), new Color32(229, 229, 229, 255) }, //Piston (Moving)
            { new Tuple<int, int>(37, 0), new Color32(255, 255, 0, 255) }, //Dandelion
            { new Tuple<int, int>(38, 0), new Color32(218, 0, 13, 255) }, //Poppy
            { new Tuple<int, int>(38, 1), new Color32(37, 152, 138, 255) }, //Blue Orchid
            { new Tuple<int, int>(38, 2), new Color32(177, 141, 211, 255) }, //Allium
            { new Tuple<int, int>(38, 3), new Color32(255, 255, 167, 255) }, //Azure Bluet
            { new Tuple<int, int>(38, 4), new Color32(208, 57, 22, 255) }, //Red Tulip
            { new Tuple<int, int>(38, 5), new Color32(95, 134, 32, 255) }, //Orange Tulip
            { new Tuple<int, int>(38, 6), new Color32(94, 153, 65, 255) }, //White Tulip
            { new Tuple<int, int>(38, 7), new Color32(101, 150, 73, 255) }, //Pink Tulip
            { new Tuple<int, int>(38, 8), new Color32(176, 197, 139, 255) }, //Oxeye Daisy
            { new Tuple<int, int>(39, 0), new Color32(120, 87, 65, 255) }, //Brown Mushroom
            { new Tuple<int, int>(40, 0), new Color32(225, 15, 13, 255) }, //Red Mushroom
            { new Tuple<int, int>(41, 0), new Color32(249, 236, 78, 255) }, //Block of Gold
            { new Tuple<int, int>(42, 0), new Color32(219, 219, 219, 255) }, //Block of Iron
            { new Tuple<int, int>(43, 0), new Color32(161, 161, 161, 255) }, //Stone Slab (Double)
            { new Tuple<int, int>(43, 1), new Color32(223, 216, 164, 255) }, //Sandstone Slab (Double)
            { new Tuple<int, int>(43, 2), new Color32(146, 121, 68, 255) }, //Wooden Slab (Double)
            { new Tuple<int, int>(43, 3), new Color32(152, 152, 152, 255) }, //Cobblestone Slab (Double)
            { new Tuple<int, int>(43, 4), new Color32(225, 104, 73, 255) }, //Brick Slab (Double)
            { new Tuple<int, int>(43, 5), new Color32(120, 120, 120, 255) }, //Stone Brick Slab (Double)
            { new Tuple<int, int>(43, 6), new Color32(55, 24, 29, 255) }, //Nether Brick Slab (Double)
            { new Tuple<int, int>(43, 7), new Color32(234, 230, 224, 255) }, //Quartz Slab (Double)
            { new Tuple<int, int>(43, 8), new Color32(168, 168, 168, 255) }, //Smooth Stone Slab (Double)
            { new Tuple<int, int>(43, 9), new Color32(223, 216, 163, 255) }, //Smooth Sandstone Slab (Double)
            { new Tuple<int, int>(44, 0), new Color32(166, 166, 166, 255) }, //Stone Slab
            { new Tuple<int, int>(44, 1), new Color32(220, 212, 162, 255) }, //Sandstone Slab
            { new Tuple<int, int>(44, 2), new Color32(201, 162, 101, 255) }, //Wooden Slab
            { new Tuple<int, int>(44, 3), new Color32(109, 109, 109, 255) }, //Cobblestone Slab
            { new Tuple<int, int>(44, 4), new Color32(147, 80, 65, 255) }, //Brick Slab
            { new Tuple<int, int>(44, 5), new Color32(128, 128, 128, 255) }, //Stone Brick Slab
            { new Tuple<int, int>(44, 6), new Color32(70, 34, 41, 255) }, //Nether Brick Slab
            { new Tuple<int, int>(44, 7), new Color32(237, 235, 228, 255) }, //Quartz Slab
            { new Tuple<int, int>(45, 0), new Color32(188, 48, 6, 255) }, //Brick
            { new Tuple<int, int>(46, 0), new Color32(175, 38, 0, 255) }, //TNT
            { new Tuple<int, int>(47, 0), new Color32(107, 88, 57, 255) }, //Bookshelf
            { new Tuple<int, int>(48, 0), new Color32(101, 135, 101, 255) }, //Moss Stone
            { new Tuple<int, int>(49, 0), new Color32(20, 18, 29, 255) }, //Obsidian
            { new Tuple<int, int>(50, 0), new Color32(255, 255, 0, 255) }, //Torch
            { new Tuple<int, int>(51, 0), new Color32(222, 95, 0, 255) }, //Fire
            { new Tuple<int, int>(52, 0), new Color32(26, 39, 49, 255) }, //Mob Spawner
            { new Tuple<int, int>(53, 0), new Color32(166, 135, 78, 255) }, //Wooden Stairs (Oak)
            { new Tuple<int, int>(54, 0), new Color32(164, 116, 42, 255) }, //Chest
            { new Tuple<int, int>(55, 0), new Color32(255, 0, 0, 255) }, //Redstone Wire
            { new Tuple<int, int>(56, 0), new Color32(129, 140, 143, 255) }, //Diamond Ore
            { new Tuple<int, int>(57, 0), new Color32(97, 219, 213, 255) }, //Block of Diamond
            { new Tuple<int, int>(58, 0), new Color32(136, 80, 46, 255) }, //Workbench
            { new Tuple<int, int>(59, 0), new Color32(60, 89, 23, 255) }, //Wheat (Crop)
            { new Tuple<int, int>(60, 0), new Color32(130, 86, 51, 255) }, //Farmland
            { new Tuple<int, int>(61, 0), new Color32(103, 103, 103, 255) }, //Furnace
            { new Tuple<int, int>(62, 0), new Color32(255, 185, 0, 255) }, //Furnace (Smelting)
            { new Tuple<int, int>(63, 0), new Color32(184, 154, 91, 255) }, //Sign (Block)
            { new Tuple<int, int>(64, 0), new Color32(148, 115, 56, 255) }, //Wood Door (Block)
            { new Tuple<int, int>(65, 0), new Color32(121, 95, 52, 255) }, //Ladder
            { new Tuple<int, int>(66, 0), new Color32(182, 144, 81, 255) }, //Rail
            { new Tuple<int, int>(67, 0), new Color32(106, 106, 106, 255) }, //Cobblestone Stairs
            { new Tuple<int, int>(68, 0), new Color32(184, 154, 91, 255) }, //Sign (Wall Block)
            { new Tuple<int, int>(69, 0), new Color32(106, 89, 64, 255) }, //Lever
            { new Tuple<int, int>(70, 0), new Color32(122, 122, 122, 255) }, //Stone Pressure Plate
            { new Tuple<int, int>(71, 0), new Color32(194, 194, 194, 255) }, //Iron Door (Block)
            { new Tuple<int, int>(72, 0), new Color32(201, 160, 101, 255) }, //Wooden Pressure Plate
            { new Tuple<int, int>(73, 0), new Color32(132, 107, 107, 255) }, //Redstone Ore
            { new Tuple<int, int>(74, 0), new Color32(221, 45, 45, 255) }, //Redstone Ore (Glowing)
            { new Tuple<int, int>(75, 0), new Color32(102, 0, 0, 255) }, //Redstone Torch (Off)
            { new Tuple<int, int>(76, 0), new Color32(255, 97, 0, 255) }, //Redstone Torch
            { new Tuple<int, int>(77, 0), new Color32(132, 132, 132, 255) }, //Button (Stone)
            { new Tuple<int, int>(78, 0), new Color32(223, 241, 241, 255) }, //Snow
            { new Tuple<int, int>(79, 0), new Color32(125, 173, 255, 255) }, //Ice
            { new Tuple<int, int>(80, 0), new Color32(239, 251, 251, 255) }, //Snow Block
            { new Tuple<int, int>(81, 0), new Color32(15, 131, 29, 255) }, //Cactus
            { new Tuple<int, int>(82, 0), new Color32(158, 164, 176, 255) }, //Clay Block
            { new Tuple<int, int>(83, 0), new Color32(148, 192, 101, 255) }, //Sugar Cane (Block)
            { new Tuple<int, int>(84, 0), new Color32(133, 89, 59, 255) }, //Jukebox
            { new Tuple<int, int>(85, 0), new Color32(141, 116, 66, 255) }, //Fence (Oak)
            { new Tuple<int, int>(86, 0), new Color32(227, 140, 27, 255) }, //Pumpkin
            { new Tuple<int, int>(87, 0), new Color32(111, 54, 52, 255) }, //Netherrack
            { new Tuple<int, int>(88, 0), new Color32(84, 64, 51, 255) }, //Soul Sand
            { new Tuple<int, int>(89, 0), new Color32(143, 118, 69, 255) }, //Glowstone
            { new Tuple<int, int>(90, 0), new Color32(87, 10, 191, 255) }, //Portal
            { new Tuple<int, int>(91, 0), new Color32(241, 152, 33, 255) }, //Jack-O-Lantern
            { new Tuple<int, int>(92, 0), new Color32(236, 255, 255, 255) }, //Cake (Block)
            { new Tuple<int, int>(93, 0), new Color32(178, 178, 178, 255) }, //Redstone Repeater (Block Off)
            { new Tuple<int, int>(94, 0), new Color32(178, 178, 178, 255) }, //Redstone Repeater (Block On)
            { new Tuple<int, int>(95, 0), new Color32(255, 255, 255, 255) }, //Stained Glass (White)
            { new Tuple<int, int>(95, 1), new Color32(216, 127, 51, 255) }, //Stained Glass (Orange)
            { new Tuple<int, int>(95, 2), new Color32(178, 76, 216, 255) }, //Stained Glass (Magenta)
            { new Tuple<int, int>(95, 3), new Color32(102, 153, 216, 255) }, //Stained Glass (Light Blue)
            { new Tuple<int, int>(95, 4), new Color32(229, 229, 51, 255) }, //Stained Glass (Yellow)
            { new Tuple<int, int>(95, 5), new Color32(127, 204, 25, 255) }, //Stained Glass (Lime)
            { new Tuple<int, int>(95, 6), new Color32(242, 127, 165, 255) }, //Stained Glass (Pink)
            { new Tuple<int, int>(95, 7), new Color32(76, 76, 76, 255) }, //Stained Glass (Gray)
            { new Tuple<int, int>(95, 8), new Color32(117, 117, 117, 255) }, //Stained Glass (Light Grey)
            { new Tuple<int, int>(95, 9), new Color32(76, 127, 153, 255) }, //Stained Glass (Cyan)
            { new Tuple<int, int>(95, 10), new Color32(127, 63, 178, 255) }, //Stained Glass (Purple)
            { new Tuple<int, int>(95, 11), new Color32(51, 76, 178, 255) }, //Stained Glass (Blue)
            { new Tuple<int, int>(95, 12), new Color32(102, 76, 51, 255) }, //Stained Glass (Brown)
            { new Tuple<int, int>(95, 13), new Color32(102, 127, 51, 255) }, //Stained Glass (Green)
            { new Tuple<int, int>(95, 14), new Color32(153, 51, 51, 255) }, //Stained Glass (Red)
            { new Tuple<int, int>(95, 15), new Color32(25, 25, 25, 255) }, //Stained Glass (Black)
            { new Tuple<int, int>(96, 0), new Color32(126, 93, 45, 255) }, //Trapdoor
            { new Tuple<int, int>(97, 0), new Color32(124, 124, 124, 255) }, //Monster Egg (Stone)
            { new Tuple<int, int>(97, 1), new Color32(141, 141, 141, 255) }, //Monster Egg (Cobblestone)
            { new Tuple<int, int>(97, 2), new Color32(122, 122, 122, 255) }, //Monster Egg (Stone Brick)
            { new Tuple<int, int>(97, 3), new Color32(105, 120, 81, 255) }, //Monster Egg (Mossy Stone Brick)
            { new Tuple<int, int>(97, 4), new Color32(104, 104, 104, 255) }, //Monster Egg (Cracked Stone)
            { new Tuple<int, int>(97, 5), new Color32(118, 118, 118, 255) }, //Monster Egg (Chiseled Stone)
            { new Tuple<int, int>(98, 0), new Color32(122, 122, 122, 255) }, //Stone Bricks
            { new Tuple<int, int>(98, 1), new Color32(122, 122, 122, 255) }, //Mossy Stone Bricks
            { new Tuple<int, int>(98, 2), new Color32(122, 122, 122, 255) }, //Cracked Stone Bricks
            { new Tuple<int, int>(98, 3), new Color32(122, 122, 122, 255) }, //Chiseled Stone Brick
            { new Tuple<int, int>(99, 0), new Color32(207, 175, 124, 255) }, //Brown Mushroom (Block)
            { new Tuple<int, int>(100, 0), new Color32(202, 170, 120, 255) }, //Red Mushroom (Block)
            { new Tuple<int, int>(101, 0), new Color32(109, 108, 106, 255) }, //Iron Bars
            { new Tuple<int, int>(102, 0), new Color32(211, 239, 244, 255) }, //Glass Pane
            { new Tuple<int, int>(103, 0), new Color32(196, 189, 40, 255) }, //Melon (Block)
            { new Tuple<int, int>(104, 0), new Color32(146, 221, 105, 255) }, //Pumpkin Vine
            { new Tuple<int, int>(105, 0), new Color32(115, 174, 83, 255) }, //Melon Vine
            { new Tuple<int, int>(106, 0), new Color32(32, 81, 12, 255) }, //Vines
            { new Tuple<int, int>(107, 0), new Color32(165, 135, 82, 255) }, //Fence Gate (Oak)
            { new Tuple<int, int>(108, 0), new Color32(148, 64, 42, 255) }, //Brick Stairs
            { new Tuple<int, int>(109, 0), new Color32(122, 122, 122, 255) }, //Stone Brick Stairs
            { new Tuple<int, int>(110, 0), new Color32(138, 113, 117, 255) }, //Mycelium
            { new Tuple<int, int>(111, 0), new Color32(118, 118, 118, 255) }, //Lily Pad
            { new Tuple<int, int>(112, 0), new Color32(44, 22, 26, 255) }, //Nether Brick
            { new Tuple<int, int>(113, 0), new Color32(44, 22, 26, 255) }, //Nether Brick Fence
            { new Tuple<int, int>(114, 0), new Color32(44, 22, 26, 255) }, //Nether Brick Stairs
            { new Tuple<int, int>(115, 0), new Color32(166, 40, 45, 255) }, //Nether Wart
            { new Tuple<int, int>(116, 0), new Color32(84, 196, 177, 255) }, //Enchantment Table
            { new Tuple<int, int>(117, 0), new Color32(124, 103, 81, 255) }, //Brewing Stand (Block)
            { new Tuple<int, int>(118, 0), new Color32(73, 73, 73, 255) }, //Cauldron (Block)
            { new Tuple<int, int>(119, 0), new Color32(52, 52, 52, 255) }, //End Portal
            { new Tuple<int, int>(120, 0), new Color32(52, 137, 209, 255) }, //End Portal Frame
            { new Tuple<int, int>(121, 0), new Color32(221, 223, 165, 255) }, //End Stone
            { new Tuple<int, int>(122, 0), new Color32(12, 9, 15, 255) }, //Dragon Egg
            { new Tuple<int, int>(123, 0), new Color32(151, 99, 49, 255) }, //Redstone Lamp
            { new Tuple<int, int>(124, 0), new Color32(227, 160, 66, 255) }, //Redstone Lamp (On)
            { new Tuple<int, int>(125, 0), new Color32(161, 132, 77, 255) }, //Oak-Wood Slab (Double)
            { new Tuple<int, int>(125, 1), new Color32(125, 90, 54, 255) }, //Spruce-Wood Slab (Double)
            { new Tuple<int, int>(125, 2), new Color32(215, 204, 141, 255) }, //Birch-Wood Slab (Double)
            { new Tuple<int, int>(125, 3), new Color32(183, 133, 96, 255) }, //Jungle-Wood Slab (Double)
            { new Tuple<int, int>(125, 4), new Color32(169, 88, 48, 255) }, //Acacia Wood Slab (Double)
            { new Tuple<int, int>(125, 5), new Color32(67, 42, 21, 255) }, //Dark Oak Wood Slab (Double)
            { new Tuple<int, int>(126, 0), new Color32(158, 133, 73, 255) }, //Oak-Wood Slab
            { new Tuple<int, int>(126, 1), new Color32(100, 79, 46, 255) }, //Spruce-Wood Slab
            { new Tuple<int, int>(126, 2), new Color32(235, 225, 155, 255) }, //Birch-Wood Slab
            { new Tuple<int, int>(126, 3), new Color32(139, 97, 60, 255) }, //Jungle-Wood Slab
            { new Tuple<int, int>(126, 4), new Color32(171, 92, 51, 255) }, //Acacia Wood Slab
            { new Tuple<int, int>(126, 5), new Color32(66, 42, 18, 255) }, //Dark Oak Wood Slab
            { new Tuple<int, int>(127, 0), new Color32(221, 113, 31, 255) }, //Cocoa Plant
            { new Tuple<int, int>(128, 0), new Color32(231, 226, 168, 255) }, //Sandstone Stairs
            { new Tuple<int, int>(129, 0), new Color32(109, 128, 116, 255) }, //Emerald Ore
            { new Tuple<int, int>(130, 0), new Color32(42, 58, 60, 255) }, //Ender Chest
            { new Tuple<int, int>(131, 0), new Color32(124, 124, 124, 255) }, //Tripwire Hook
            { new Tuple<int, int>(132, 0), new Color32(90, 90, 90, 255) }, //Tripwire
            { new Tuple<int, int>(133, 0), new Color32(81, 217, 117, 255) }, //Block of Emerald
            { new Tuple<int, int>(134, 0), new Color32(129, 94, 52, 255) }, //Wooden Stairs (Spruce)
            { new Tuple<int, int>(135, 0), new Color32(206, 192, 132, 255) }, //Wooden Stairs (Birch)
            { new Tuple<int, int>(136, 0), new Color32(136, 95, 69, 255) }, //Wooden Stairs (Jungle)
            { new Tuple<int, int>(137, 0), new Color32(142, 139, 134, 255) }, //Command Block
            { new Tuple<int, int>(138, 0), new Color32(116, 221, 215, 255) }, //Beacon
            { new Tuple<int, int>(139, 0), new Color32(89, 89, 89, 255) }, //Cobblestone Wall
            { new Tuple<int, int>(139, 1), new Color32(42, 94, 42, 255) }, //Mossy Cobblestone Wall
            { new Tuple<int, int>(140, 0), new Color32(118, 65, 51, 255) }, //Flower Pot (Block)
            { new Tuple<int, int>(141, 0), new Color32(10, 140, 0, 255) }, //Carrot (Crop)
            { new Tuple<int, int>(142, 0), new Color32(4, 164, 23, 255) }, //Potatoes (Crop)
            { new Tuple<int, int>(143, 0), new Color32(179, 146, 89, 255) }, //Button (Wood)
            { new Tuple<int, int>(144, 0), new Color32(176, 176, 176, 255) }, //Head Block (Skeleton)
            { new Tuple<int, int>(144, 1), new Color32(79, 85, 85, 255) }, //Head Block (Wither)
            { new Tuple<int, int>(144, 2), new Color32(98, 146, 75, 255) }, //Head Block (Zombie)
            { new Tuple<int, int>(144, 3), new Color32(204, 151, 126, 255) }, //Head Block (Steve)
            { new Tuple<int, int>(144, 4), new Color32(82, 175, 67, 255) }, //Head Block (Creeper)
            { new Tuple<int, int>(145, 0), new Color32(71, 67, 67, 255) }, //Anvil
            { new Tuple<int, int>(145, 1), new Color32(71, 67, 67, 255) }, //Anvil (Slightly Damaged)
            { new Tuple<int, int>(145, 2), new Color32(71, 67, 67, 255) }, //Anvil (Very Damaged)
            { new Tuple<int, int>(146, 0), new Color32(158, 107, 29, 255) }, //Trapped Chest
            { new Tuple<int, int>(147, 0), new Color32(254, 253, 112, 255) }, //Weighted Pressure Plate (Light)
            { new Tuple<int, int>(148, 0), new Color32(229, 229, 229, 255) }, //Weighted Pressure Plate (Heavy)
            { new Tuple<int, int>(149, 0), new Color32(75, 74, 76, 255) }, //Redstone Comparator (Off)
            { new Tuple<int, int>(150, 0), new Color32(191, 198, 189, 255) }, //Redstone Comparator (On)
            { new Tuple<int, int>(151, 0), new Color32(251, 237, 221, 255) }, //Daylight Sensor
            { new Tuple<int, int>(152, 0), new Color32(171, 27, 9, 255) }, //Block of Redstone
            { new Tuple<int, int>(153, 0), new Color32(125, 84, 79, 255) }, //Nether Quartz Ore
            { new Tuple<int, int>(154, 0), new Color32(113, 113, 113, 255) }, //Hopper
            { new Tuple<int, int>(155, 0), new Color32(234, 230, 223, 255) }, //Quartz Block
            { new Tuple<int, int>(155, 1), new Color32(224, 219, 210, 255) }, //Chiseled Quartz Block
            { new Tuple<int, int>(155, 2), new Color32(234, 231, 225, 255) }, //Pillar Quartz Block
            { new Tuple<int, int>(156, 0), new Color32(235, 232, 227, 255) }, //Quartz Stairs
            { new Tuple<int, int>(157, 0), new Color32(155, 129, 65, 255) }, //Rail (Activator)
            { new Tuple<int, int>(158, 0), new Color32(116, 116, 116, 255) }, //Dropper
            { new Tuple<int, int>(159, 0), new Color32(209, 178, 161, 255) }, //Stained Clay (White)
            { new Tuple<int, int>(159, 1), new Color32(161, 83, 37, 255) }, //Stained Clay (Orange)
            { new Tuple<int, int>(159, 2), new Color32(149, 88, 108, 255) }, //Stained Clay (Magenta)
            { new Tuple<int, int>(159, 3), new Color32(113, 108, 137, 255) }, //Stained Clay (Light Blue)
            { new Tuple<int, int>(159, 4), new Color32(186, 133, 35, 255) }, //Stained Clay (Yellow)
            { new Tuple<int, int>(159, 5), new Color32(103, 117, 52, 255) }, //Stained Clay (Lime)
            { new Tuple<int, int>(159, 6), new Color32(161, 78, 78, 255) }, //Stained Clay (Pink)
            { new Tuple<int, int>(159, 7), new Color32(57, 42, 35, 255) }, //Stained Clay (Gray)
            { new Tuple<int, int>(159, 8), new Color32(135, 104, 95, 255) }, //Stained Clay (Light Gray)
            { new Tuple<int, int>(159, 9), new Color32(86, 91, 91, 255) }, //Stained Clay (Cyan)
            { new Tuple<int, int>(159, 10), new Color32(118, 70, 86, 255) }, //Stained Clay (Purple)
            { new Tuple<int, int>(159, 11), new Color32(74, 59, 91, 255) }, //Stained Clay (Blue)
            { new Tuple<int, int>(159, 12), new Color32(77, 51, 35, 255) }, //Stained Clay (Brown)
            { new Tuple<int, int>(159, 13), new Color32(76, 83, 42, 255) }, //Stained Clay (Green)
            { new Tuple<int, int>(159, 14), new Color32(143, 61, 46, 255) }, //Stained Clay (Red)
            { new Tuple<int, int>(159, 15), new Color32(37, 22, 16, 255) }, //Stained Clay (Black)
            { new Tuple<int, int>(160, 0), new Color32(246, 246, 246, 255) }, //Stained Glass Pane (White)
            { new Tuple<int, int>(160, 1), new Color32(208, 122, 48, 255) }, //Stained Glass Pane (Orange)
            { new Tuple<int, int>(160, 2), new Color32(171, 73, 208, 255) }, //Stained Glass Pane (Magenta)
            { new Tuple<int, int>(160, 3), new Color32(97, 147, 208, 255) }, //Stained Glass Pane (Light Blue)
            { new Tuple<int, int>(160, 4), new Color32(221, 221, 48, 255) }, //Stained Glass Pane (Yellow)
            { new Tuple<int, int>(160, 5), new Color32(122, 196, 24, 255) }, //Stained Glass Pane (Lime)
            { new Tuple<int, int>(160, 6), new Color32(233, 122, 159, 255) }, //Stained Glass Pane (Pink)
            { new Tuple<int, int>(160, 7), new Color32(73, 73, 73, 255) }, //Stained Glass Pane (Gray)
            { new Tuple<int, int>(160, 8), new Color32(145, 145, 145, 255) }, //Stained Glass Pane (Light Gray)
            { new Tuple<int, int>(160, 9), new Color32(73, 122, 147, 255) }, //Stained Glass Pane (Cyan)
            { new Tuple<int, int>(160, 10), new Color32(122, 61, 171, 255) }, //Stained Glass Pane (Purple)
            { new Tuple<int, int>(160, 11), new Color32(48, 73, 171, 255) }, //Stained Glass Pane (Blue)
            { new Tuple<int, int>(160, 12), new Color32(97, 73, 48, 255) }, //Stained Glass Pane (Brown)
            { new Tuple<int, int>(160, 13), new Color32(97, 122, 48, 255) }, //Stained Glass Pane (Green)
            { new Tuple<int, int>(160, 14), new Color32(147, 48, 48, 255) }, //Stained Glass Pane (Red)
            { new Tuple<int, int>(160, 15), new Color32(24, 24, 24, 255) }, //Stained Glass Pane (Black)
            { new Tuple<int, int>(161, 0), new Color32(135, 135, 135, 255) }, //Leaves (Acacia)
            { new Tuple<int, int>(161, 1), new Color32(55, 104, 33, 255) }, //Leaves (Dark Oak)
            { new Tuple<int, int>(162, 0), new Color32(176, 90, 57, 255) }, //Wood (Acacia Oak)
            { new Tuple<int, int>(162, 1), new Color32(93, 74, 49, 255) }, //Wood (Dark Oak)
            { new Tuple<int, int>(163, 0), new Color32(172, 92, 50, 255) }, //Wooden Stairs (Acacia)
            { new Tuple<int, int>(164, 0), new Color32(71, 44, 21, 255) }, //Wooden Stairs (Dark Oak)
            { new Tuple<int, int>(165, 0), new Color32(120, 200, 101, 255) }, //Slime Block
            { new Tuple<int, int>(166, 0), new Color32(223, 52, 53, 255) }, //Barrier
            { new Tuple<int, int>(167, 0), new Color32(199, 199, 199, 255) }, //Iron Trapdoor
            { new Tuple<int, int>(168, 0), new Color32(114, 175, 165, 255) }, //Prismarine
            { new Tuple<int, int>(168, 1), new Color32(92, 158, 143, 255) }, //Prismarine Bricks
            { new Tuple<int, int>(168, 2), new Color32(72, 106, 94, 255) }, //Dark Prismarine
            { new Tuple<int, int>(169, 0), new Color32(172, 199, 190, 255) }, //Sea Lantern
            { new Tuple<int, int>(170, 0), new Color32(220, 211, 159, 255) }, //Hay Bale
            { new Tuple<int, int>(171, 0), new Color32(202, 202, 202, 255) }, //Carpet (White)
            { new Tuple<int, int>(171, 1), new Color32(221, 133, 75, 255) }, //Carpet (Orange)
            { new Tuple<int, int>(171, 2), new Color32(177, 67, 186, 255) }, //Carpet (Magenta)
            { new Tuple<int, int>(171, 3), new Color32(75, 113, 189, 255) }, //Carpet (Light Blue)
            { new Tuple<int, int>(171, 4), new Color32(197, 183, 44, 255) }, //Carpet (Yellow)
            { new Tuple<int, int>(171, 5), new Color32(60, 161, 51, 255) }, //Carpet (Lime)
            { new Tuple<int, int>(171, 6), new Color32(206, 142, 168, 255) }, //Carpet (Pink)
            { new Tuple<int, int>(171, 7), new Color32(70, 70, 70, 255) }, //Carpet (Grey)
            { new Tuple<int, int>(171, 8), new Color32(162, 162, 162, 255) }, //Carpet (Light Gray)
            { new Tuple<int, int>(171, 9), new Color32(48, 116, 145, 255) }, //Carpet (Cyan)
            { new Tuple<int, int>(171, 10), new Color32(148, 81, 202, 255) }, //Carpet (Purple)
            { new Tuple<int, int>(171, 11), new Color32(54, 69, 171, 255) }, //Carpet (Blue)
            { new Tuple<int, int>(171, 12), new Color32(82, 52, 32, 255) }, //Carpet (Brown)
            { new Tuple<int, int>(171, 13), new Color32(62, 85, 33, 255) }, //Carpet (Green)
            { new Tuple<int, int>(171, 14), new Color32(187, 61, 57, 255) }, //Carpet (Red)
            { new Tuple<int, int>(171, 15), new Color32(35, 31, 31, 255) }, //Carpet (Black)
            { new Tuple<int, int>(172, 0), new Color32(150, 92, 66, 255) }, //Hardened Clay
            { new Tuple<int, int>(173, 0), new Color32(18, 18, 18, 255) }, //Block of Coal
            { new Tuple<int, int>(174, 0), new Color32(162, 191, 244, 255) }, //Packed Ice
            { new Tuple<int, int>(175, 0), new Color32(207, 116, 20, 255) }, //Sunflower
            { new Tuple<int, int>(175, 1), new Color32(168, 112, 178, 255) }, //Lilac
            { new Tuple<int, int>(175, 2), new Color32(102, 158, 88, 255) }, //Double Tallgrass
            { new Tuple<int, int>(175, 3), new Color32(84, 129, 72, 255) }, //Large Fern
            { new Tuple<int, int>(175, 4), new Color32(215, 2, 8, 255) }, //Rose Bush
            { new Tuple<int, int>(175, 5), new Color32(192, 150, 207, 255) }, //Peony
            { new Tuple<int, int>(176, 0), new Color32(240, 240, 240, 255) }, //Standing Banner (Block)
            { new Tuple<int, int>(177, 0), new Color32(240, 240, 240, 255) }, //Wall Banner (Block)
            { new Tuple<int, int>(178, 0), new Color32(240, 240, 240, 255) }, //Inverted Daylight Sensor
            { new Tuple<int, int>(179, 0), new Color32(172, 86, 29, 255) }, //Red Sandstone
            { new Tuple<int, int>(179, 1), new Color32(172, 86, 29, 255) }, //Red Sandstone (Chiseled)
            { new Tuple<int, int>(179, 2), new Color32(172, 86, 29, 255) }, //Red Sandstone (Smooth)
            { new Tuple<int, int>(180, 0), new Color32(174, 87, 29, 255) }, //Red Sandstone Stairs
            { new Tuple<int, int>(181, 0), new Color32(174, 87, 29, 255) }, //Red Sandstone Slab (Double)
            { new Tuple<int, int>(182, 0), new Color32(174, 87, 29, 255) }, //Red Sandstone Slab
            { new Tuple<int, int>(183, 0), new Color32(80, 60, 36, 255) }, //Fence Gate (Spruce)
            { new Tuple<int, int>(184, 0), new Color32(221, 205, 141, 255) }, //Fence Gate (Birch)
            { new Tuple<int, int>(185, 0), new Color32(175, 122, 77, 255) }, //Fence Gate (Jungle)
            { new Tuple<int, int>(186, 0), new Color32(52, 32, 14, 255) }, //Fence Gate (Dark Oak)
            { new Tuple<int, int>(187, 0), new Color32(207, 107, 54, 255) }, //Fence Gate (Acacia)
            { new Tuple<int, int>(188, 0), new Color32(126, 93, 53, 255) }, //Fence (Spruce)
            { new Tuple<int, int>(189, 0), new Color32(199, 184, 123, 255) }, //Fence (Birch)
            { new Tuple<int, int>(190, 0), new Color32(187, 134, 95, 255) }, //Fence (Jungle)
            { new Tuple<int, int>(191, 0), new Color32(63, 46, 30, 255) }, //Fence (Dark Oak)
            { new Tuple<int, int>(192, 0), new Color32(197, 107, 58, 255) }, //Fence (Acacia)
            { new Tuple<int, int>(193, 0), new Color32(110, 83, 48, 255) }, //Wooden Door Block (Spruce)
            { new Tuple<int, int>(194, 0), new Color32(247, 243, 224, 255) }, //Wooden Door Block (Birch)
            { new Tuple<int, int>(195, 0), new Color32(169, 119, 80, 255) }, //Wooden Door Block (Jungle)
            { new Tuple<int, int>(196, 0), new Color32(170, 85, 41, 255) }, //Wooden Door Block (Acacia)
            { new Tuple<int, int>(197, 0), new Color32(78, 55, 33, 255) }, //Wooden Door Block (Dark Oak)
            { new Tuple<int, int>(198, 0), new Color32(220, 197, 205, 255) }, //End rod
            { new Tuple<int, int>(199, 0), new Color32(96, 59, 96, 255) }, //Chorus Plant
            { new Tuple<int, int>(200, 0), new Color32(133, 103, 133, 255) }, //Chorus Flower
            { new Tuple<int, int>(201, 0), new Color32(166, 121, 166, 255) }, //Purpur Block
            { new Tuple<int, int>(202, 0), new Color32(170, 126, 170, 255) }, //Purpur Pillar
            { new Tuple<int, int>(203, 0), new Color32(168, 121, 168, 255) }, //Purpur Stairs
            { new Tuple<int, int>(204, 0), new Color32(168, 121, 168, 255) }, //Purpur Slab (Double)
            { new Tuple<int, int>(205, 0), new Color32(168, 121, 168, 255) }, //Purpur Slab
            { new Tuple<int, int>(206, 0), new Color32(225, 230, 170, 255) }, //End Stone Bricks
            { new Tuple<int, int>(207, 0), new Color32(179, 134, 0, 255) }, //Beetroot Block
            { new Tuple<int, int>(208, 0), new Color32(152, 125, 69, 255) }, //Grass Path
            { new Tuple<int, int>(209, 0), new Color32(240, 240, 240, 255) }, //End Gateway
            { new Tuple<int, int>(210, 0), new Color32(155, 137, 39, 255) }, //Repeating Command Block
            { new Tuple<int, int>(211, 0), new Color32(118, 178, 151, 255) }, //Chain Command Block
            { new Tuple<int, int>(212, 0), new Color32(118, 162, 252, 255) }, //Frosted Ice
            { new Tuple<int, int>(213, 0), new Color32(202, 78, 6, 255) }, //Magma Block
            { new Tuple<int, int>(214, 0), new Color32(129, 0, 8, 255) }, //Nether Wart Block
            { new Tuple<int, int>(215, 0), new Color32(86, 0, 4, 255) }, //Red Nether Brick
            { new Tuple<int, int>(216, 0), new Color32(143, 147, 131, 255) }, //Bone Block
            { new Tuple<int, int>(217, 0), new Color32(0, 0, 0, 0) }, //Void Block
            { new Tuple<int, int>(218, 0), new Color32(43, 43, 43, 255) }, //Observer
            { new Tuple<int, int>(219, 0), new Color32(223, 223, 220, 255) }, //White Shulker Box
            { new Tuple<int, int>(220, 0), new Color32(208, 118, 59, 255) }, //Orange Shulker Box
            { new Tuple<int, int>(221, 0), new Color32(186, 100, 194, 255) }, //Magenta Shulker Box
            { new Tuple<int, int>(222, 0), new Color32(103, 143, 204, 255) }, //Light Blue Shulker Box
            { new Tuple<int, int>(223, 0), new Color32(193, 183, 61, 255) }, //Yellow Shulker Box
            { new Tuple<int, int>(224, 0), new Color32(73, 185, 61, 255) }, //Lime Shulker Box
            { new Tuple<int, int>(225, 0), new Color32(208, 140, 161, 255) }, //Pink Shulker Box
            { new Tuple<int, int>(226, 0), new Color32(84, 82, 82, 255) }, //Gray Shulker Box
            { new Tuple<int, int>(227, 0), new Color32(165, 162, 162, 255) }, //Light Gray Shulker Box
            { new Tuple<int, int>(228, 0), new Color32(69, 137, 165, 255) }, //Cyan Shulker Box
            { new Tuple<int, int>(229, 0), new Color32(151, 105, 151, 255) }, //Purple Shulker Box
            { new Tuple<int, int>(230, 0), new Color32(102, 114, 202, 255) }, //Blue Shulker Box
            { new Tuple<int, int>(231, 0), new Color32(142, 113, 94, 255) }, //Brown Shulker Box
            { new Tuple<int, int>(232, 0), new Color32(112, 131, 85, 255) }, //Green Shulker Box
            { new Tuple<int, int>(233, 0), new Color32(195, 89, 86, 255) }, //Red Shulker Box
            { new Tuple<int, int>(234, 0), new Color32(58, 55, 55, 255) }, //Black Shulker Box
            { new Tuple<int, int>(235, 0), new Color32(249, 255, 254, 255) }, //White Glazed Terracota
            { new Tuple<int, int>(236, 0), new Color32(225, 97, 0, 255) }, //Orange Glazed Terracota
            { new Tuple<int, int>(237, 0), new Color32(241, 165, 191, 255) }, //Magenta Glazed Terracota
            { new Tuple<int, int>(238, 0), new Color32(77, 185, 221, 255) }, //Light Blue Glazed Terracota
            { new Tuple<int, int>(239, 0), new Color32(238, 170, 13, 255) }, //Yellow Glazed Terracota
            { new Tuple<int, int>(240, 0), new Color32(133, 207, 33, 255) }, //Lime Glazed Terracota
            { new Tuple<int, int>(241, 0), new Color32(244, 181, 203, 255) }, //Pink Glazed Terracota
            { new Tuple<int, int>(242, 0), new Color32(96, 114, 119, 255) }, //Gray Glazed Terracota
            { new Tuple<int, int>(243, 0), new Color32(204, 208, 210, 255) }, //Light Gray Glazed Terracota
            { new Tuple<int, int>(244, 0), new Color32(23, 168, 168, 255) }, //Cyan Glazed Terracota
            { new Tuple<int, int>(245, 0), new Color32(100, 31, 156, 255) }, //Purple Glazed Terracota
            { new Tuple<int, int>(246, 0), new Color32(44, 46, 143, 255) }, //Blue Glazed Terracota
            { new Tuple<int, int>(247, 0), new Color32(171, 123, 80, 255) }, //Brown Glazed Terracota
            { new Tuple<int, int>(248, 0), new Color32(117, 160, 37, 255) }, //Green Glazed Terracota
            { new Tuple<int, int>(249, 0), new Color32(209, 86, 80, 255) }, //Red Glazed Terracota
            { new Tuple<int, int>(250, 0), new Color32(62, 14, 14, 255) }, //Black Glazed Terracota
            { new Tuple<int, int>(251, 0), new Color32(207, 213, 214, 255) }, //White Concrete
            { new Tuple<int, int>(251, 1), new Color32(224, 97, 0, 255) }, //Orange Concrete
            { new Tuple<int, int>(251, 2), new Color32(169, 48, 159, 255) }, //Magenta Concrete
            { new Tuple<int, int>(251, 3), new Color32(35, 137, 199, 255) }, //Light Blue Concrete
            { new Tuple<int, int>(251, 4), new Color32(239, 174, 21, 255) }, //Yellow Concrete
            { new Tuple<int, int>(251, 5), new Color32(95, 170, 25, 255) }, //Lime Concrete
            { new Tuple<int, int>(251, 6), new Color32(213, 100, 142, 255) }, //Pink Concrete
            { new Tuple<int, int>(251, 7), new Color32(54, 57, 61, 255) }, //Gray Concrete
            { new Tuple<int, int>(251, 8), new Color32(125, 125, 115, 255) }, //Light Gray Concrete
            { new Tuple<int, int>(251, 9), new Color32(21, 119, 136, 255) }, //Cyan Concrete
            { new Tuple<int, int>(251, 10), new Color32(99, 31, 155, 255) }, //Purple Concrete
            { new Tuple<int, int>(251, 11), new Color32(45, 47, 144, 255) }, //Blue Concrete
            { new Tuple<int, int>(251, 12), new Color32(97, 60, 32, 255) }, //Brown Concrete
            { new Tuple<int, int>(251, 13), new Color32(73, 91, 36, 255) }, //Green Concrete
            { new Tuple<int, int>(251, 14), new Color32(143, 33, 33, 255) }, //Red Concrete
            { new Tuple<int, int>(251, 15), new Color32(7, 9, 14, 255) }, //Black Concrete
            { new Tuple<int, int>(252, 0), new Color32(230, 232, 233, 255) }, //White Concrete Powder
            { new Tuple<int, int>(252, 1), new Color32(234, 136, 34, 255) }, //Orange Concrete Powder
            { new Tuple<int, int>(252, 2), new Color32(195, 81, 186, 255) }, //Magenta Concrete Powder
            { new Tuple<int, int>(252, 3), new Color32(90, 197, 221, 255) }, //Light Blue Concrete Powder
            { new Tuple<int, int>(252, 4), new Color32(237, 198, 48, 255) }, //Yellow Concrete Powder
            { new Tuple<int, int>(252, 5), new Color32(128, 191, 41, 255) }, //Lime Concrete Powder
            { new Tuple<int, int>(252, 6), new Color32(239, 174, 197, 255) }, //Pink Concrete Powder
            { new Tuple<int, int>(252, 7), new Color32(76, 80, 85, 255) }, //Gray Concrete Powder
            { new Tuple<int, int>(252, 8), new Color32(154, 154, 147, 255) }, //Light Gray Concrete Powder
            { new Tuple<int, int>(252, 9), new Color32(35, 148, 154, 255) }, //Cyan Concrete Powder
            { new Tuple<int, int>(252, 10), new Color32(120, 48, 169, 255) }, //Purple Concrete Powder
            { new Tuple<int, int>(252, 11), new Color32(72, 74, 171, 255) }, //Blue Concrete Powder
            { new Tuple<int, int>(252, 12), new Color32(121, 81, 51, 255) }, //Brown Concrete Powder
            { new Tuple<int, int>(252, 13), new Color32(103, 124, 55, 255) }, //Green Concrete Powder
            { new Tuple<int, int>(252, 14), new Color32(164, 51, 50, 255) }, //Red Concrete Powder
            { new Tuple<int, int>(252, 15), new Color32(22, 24, 28, 255) }, //Black Concrete Powder

            { new Tuple<int, int>(255, 0), new Color32(50, 33, 36, 255) }, //Structure Block

        };

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
            => new HashSet<T>(source, comparer);


        public static Color32 GetBlockColor(this Block block)
        {
            Color32 color;
            if (_colors.TryGetValue(new Tuple<int, int>(block.BlockID, block.Data), out color))
            {
                return color;
            }
            else
            {
                //Console.WriteLine("Applying default color of the block ID: " + block.BlockID + ":" + block.Data);
                return _colors[new Tuple<int, int>(block.BlockID, 0)];
            }
        }
    }
}
