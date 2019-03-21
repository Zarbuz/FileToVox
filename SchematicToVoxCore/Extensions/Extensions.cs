using System;
using System.Collections.Generic;
using System.Drawing;

namespace SchematicToVoxCore.Extensions
{
    public static class Extensions
    {
        private static readonly Dictionary<Tuple<int, int>, Color> _colors = new Dictionary<Tuple<int, int>, Color>()

        {
            { new Tuple<int, int>(1, 0), Color.FromArgb(125, 125, 125, 255) }, //Stone
            { new Tuple<int, int>(1, 1), Color.FromArgb(153, 113, 98, 255) }, //Granite
            { new Tuple<int, int>(1, 2), Color.FromArgb(159, 114, 98, 255) }, //Polished Granite
            { new Tuple<int, int>(1, 3), Color.FromArgb(179, 179, 182, 255) }, //Diorite
            { new Tuple<int, int>(1, 4), Color.FromArgb(183, 183, 185, 255) }, //Polished Diorite
            { new Tuple<int, int>(1, 5), Color.FromArgb(130, 131, 131, 255) }, //Andesite
            { new Tuple<int, int>(1, 6), Color.FromArgb(133, 133, 134, 255) }, //Polished Andesite
            { new Tuple<int, int>(2, 0), Color.FromArgb(118, 179, 76, 255) }, //Grass
            { new Tuple<int, int>(3, 0), Color.FromArgb(134, 96, 67, 255) }, //Dirt
            { new Tuple<int, int>(3, 1), Color.FromArgb(134, 96, 67, 255) }, //Coarse Dirt
            { new Tuple<int, int>(3, 2), Color.FromArgb(134, 96, 67, 255) }, //Podzol
            { new Tuple<int, int>(4, 0), Color.FromArgb(122, 122, 122, 255) }, //Cobblestone
            { new Tuple<int, int>(5, 0), Color.FromArgb(156, 127, 78, 255) }, //Wooden Plank (Oak)
            { new Tuple<int, int>(5, 1), Color.FromArgb(103, 77, 46, 255) }, //Wooden Plank (Spruce)
            { new Tuple<int, int>(5, 2), Color.FromArgb(195, 179, 123, 255) }, //Wooden Plank (Birch)
            { new Tuple<int, int>(5, 3), Color.FromArgb(154, 110, 77, 255) }, //Wooden Plank (Jungle)
            { new Tuple<int, int>(5, 4), Color.FromArgb(169, 91, 51, 255) }, //Wooden Plank (Acacia)
            { new Tuple<int, int>(5, 5), Color.FromArgb(61, 39, 18, 255) }, //Wooden Plank (Dark Oak)
            { new Tuple<int, int>(6, 0), Color.FromArgb(71, 102, 37, 255) }, //Sapling (Oak)
            { new Tuple<int, int>(6, 1), Color.FromArgb(51, 58, 33, 255) }, //Sapling (Spruce)
            { new Tuple<int, int>(6, 2), Color.FromArgb(118, 150, 84, 255) }, //Sapling (Birch)
            { new Tuple<int, int>(6, 3), Color.FromArgb(48, 86, 18, 255) }, //Sapling (Jungle)
            { new Tuple<int, int>(6, 4), Color.FromArgb(114, 115, 20, 255) }, //Sapling (Acacia)
            { new Tuple<int, int>(6, 5), Color.FromArgb(56, 86, 28, 255) }, //Sapling (Dark Oak)
            { new Tuple<int, int>(7, 0), Color.FromArgb(83, 83, 83, 255) }, //Bedrock
            { new Tuple<int, int>(8, 0), Color.FromArgb(112, 175, 220, 255) }, //Water
            { new Tuple<int, int>(9, 0), Color.FromArgb(112, 175, 220, 255) }, //Water (No Spread)
            { new Tuple<int, int>(10, 0), Color.FromArgb(207, 91, 19, 255) }, //Lava
            { new Tuple<int, int>(11, 0), Color.FromArgb(212, 90, 18, 255) }, //Lava (No Spread)
            { new Tuple<int, int>(12, 0), Color.FromArgb(219, 211, 160, 255) }, //Sand
            { new Tuple<int, int>(12, 1), Color.FromArgb(167, 87, 32, 255) }, //Red Sand
            { new Tuple<int, int>(13, 0), Color.FromArgb(126, 124, 122, 255) }, //Gravel
            { new Tuple<int, int>(14, 0), Color.FromArgb(143, 139, 124, 255) }, //Gold Ore
            { new Tuple<int, int>(15, 0), Color.FromArgb(135, 130, 126, 255) }, //Iron Ore
            { new Tuple<int, int>(16, 0), Color.FromArgb(115, 115, 115, 255) }, //Coal Ore
            { new Tuple<int, int>(17, 0), Color.FromArgb(102, 81, 49, 255) }, //Wood (Oak)
            { new Tuple<int, int>(17, 1), Color.FromArgb(45, 28, 12, 255) }, //Wood (Spruce)
            { new Tuple<int, int>(17, 2), Color.FromArgb(206, 206, 201, 255) }, //Wood (Birch)
            { new Tuple<int, int>(17, 3), Color.FromArgb(87, 67, 26, 255) }, //Wood (Jungle)
            { new Tuple<int, int>(17, 4), Color.FromArgb(88, 69, 39, 255) }, //Wood (Oak 4)
            { new Tuple<int, int>(17, 5), Color.FromArgb(36, 20, 5, 255) }, //Wood (Oak 5)
            { new Tuple<int, int>(18, 0), Color.FromArgb(69, 178, 49, 255) }, //Leaves (Oak)
            { new Tuple<int, int>(18, 1), Color.FromArgb(116, 116, 116, 255) }, //Leaves (Spruce)
            { new Tuple<int, int>(18, 2), Color.FromArgb(135, 135, 135, 255) }, //Leaves (Birch)
            { new Tuple<int, int>(18, 3), Color.FromArgb(45, 125, 16, 255) }, //Leaves (Jungle)
            { new Tuple<int, int>(19, 0), Color.FromArgb(194, 195, 84, 255) }, //Sponge
            { new Tuple<int, int>(19, 1), Color.FromArgb(153, 148, 53, 255) }, //Wet Sponge
            { new Tuple<int, int>(20, 0), Color.FromArgb(218, 240, 244, 255) }, //Glass
            { new Tuple<int, int>(21, 0), Color.FromArgb(102, 112, 134, 255) }, //Lapis Lazuli Ore
            { new Tuple<int, int>(22, 0), Color.FromArgb(38, 67, 137, 255) }, //Lapis Lazuli Block
            { new Tuple<int, int>(23, 0), Color.FromArgb(131, 131, 131, 255) }, //Dispenser
            { new Tuple<int, int>(24, 0), Color.FromArgb(220, 211, 159, 255) }, //Sandstone
            { new Tuple<int, int>(24, 1), Color.FromArgb(220, 211, 159, 255) }, //Sandstone (Chiseled)
            { new Tuple<int, int>(24, 2), Color.FromArgb(220, 211, 159, 255) }, //Sandstone (Smooth)
            { new Tuple<int, int>(25, 0), Color.FromArgb(100, 67, 50, 255) }, //Note Block
            { new Tuple<int, int>(26, 0), Color.FromArgb(180, 52, 54, 255) }, //Bed (Block)
            { new Tuple<int, int>(27, 0), Color.FromArgb(246, 199, 35, 255) }, //Rail (Powered)
            { new Tuple<int, int>(28, 0), Color.FromArgb(124, 124, 124, 255) }, //Rail (Detector)
            { new Tuple<int, int>(29, 0), Color.FromArgb(142, 191, 119, 255) }, //Sticky Piston
            { new Tuple<int, int>(30, 0), Color.FromArgb(220, 220, 220, 255) }, //Cobweb
            { new Tuple<int, int>(31, 0), Color.FromArgb(149, 101, 41, 255) }, //Tall Grass (Dead Shrub)
            { new Tuple<int, int>(31, 1), Color.FromArgb(2, 145, 36, 255) }, //Tall Grass
            { new Tuple<int, int>(31, 2), Color.FromArgb(91, 125, 56, 255) }, //Tall Grass (Fern)
            { new Tuple<int, int>(32, 0), Color.FromArgb(123, 79, 25, 255) }, //Dead Shrub
            { new Tuple<int, int>(33, 0), Color.FromArgb(165, 139, 83, 255) }, //Piston
            { new Tuple<int, int>(34, 0), Color.FromArgb(189, 150, 94, 255) }, //Piston (Head)
            { new Tuple<int, int>(35, 0), Color.FromArgb(221, 221, 221, 255) }, //Wool
            { new Tuple<int, int>(35, 1), Color.FromArgb(219, 125, 62, 255) }, //Orange Wool
            { new Tuple<int, int>(35, 2), Color.FromArgb(179, 80, 188, 255) }, //Magenta Wool
            { new Tuple<int, int>(35, 3), Color.FromArgb(106, 138, 201, 255) }, //Light Blue Wool
            { new Tuple<int, int>(35, 4), Color.FromArgb(177, 166, 39, 255) }, //Yellow Wool
            { new Tuple<int, int>(35, 5), Color.FromArgb(65, 174, 56, 255) }, //Lime Wool
            { new Tuple<int, int>(35, 6), Color.FromArgb(208, 132, 153, 255) }, //Pink Wool
            { new Tuple<int, int>(35, 7), Color.FromArgb(64, 64, 64, 255) }, //Gray Wool
            { new Tuple<int, int>(35, 8), Color.FromArgb(154, 161, 161, 255) }, //Light Gray Wool
            { new Tuple<int, int>(35, 9), Color.FromArgb(46, 110, 137, 255) }, //Cyan Wool
            { new Tuple<int, int>(35, 10), Color.FromArgb(126, 61, 181, 255) }, //Purple Wool
            { new Tuple<int, int>(35, 11), Color.FromArgb(46, 56, 141, 255) }, //Blue Wool
            { new Tuple<int, int>(35, 12), Color.FromArgb(79, 50, 31, 255) }, //Brown Wool
            { new Tuple<int, int>(35, 13), Color.FromArgb(53, 70, 27, 255) }, //Green Wool
            { new Tuple<int, int>(35, 14), Color.FromArgb(150, 52, 48, 255) }, //Red Wool
            { new Tuple<int, int>(35, 15), Color.FromArgb(25, 22, 22, 255) }, //Black Wool
            { new Tuple<int, int>(36, 0), Color.FromArgb(229, 229, 229, 255) }, //Piston (Moving)
            { new Tuple<int, int>(37, 0), Color.FromArgb(255, 255, 0, 255) }, //Dandelion
            { new Tuple<int, int>(38, 0), Color.FromArgb(218, 0, 13, 255) }, //Poppy
            { new Tuple<int, int>(38, 1), Color.FromArgb(37, 152, 138, 255) }, //Blue Orchid
            { new Tuple<int, int>(38, 2), Color.FromArgb(177, 141, 211, 255) }, //Allium
            { new Tuple<int, int>(38, 3), Color.FromArgb(255, 255, 167, 255) }, //Azure Bluet
            { new Tuple<int, int>(38, 4), Color.FromArgb(208, 57, 22, 255) }, //Red Tulip
            { new Tuple<int, int>(38, 5), Color.FromArgb(95, 134, 32, 255) }, //Orange Tulip
            { new Tuple<int, int>(38, 6), Color.FromArgb(94, 153, 65, 255) }, //White Tulip
            { new Tuple<int, int>(38, 7), Color.FromArgb(101, 150, 73, 255) }, //Pink Tulip
            { new Tuple<int, int>(38, 8), Color.FromArgb(176, 197, 139, 255) }, //Oxeye Daisy
            { new Tuple<int, int>(39, 0), Color.FromArgb(120, 87, 65, 255) }, //Brown Mushroom
            { new Tuple<int, int>(40, 0), Color.FromArgb(225, 15, 13, 255) }, //Red Mushroom
            { new Tuple<int, int>(41, 0), Color.FromArgb(249, 236, 78, 255) }, //Block of Gold
            { new Tuple<int, int>(42, 0), Color.FromArgb(219, 219, 219, 255) }, //Block of Iron
            { new Tuple<int, int>(43, 0), Color.FromArgb(161, 161, 161, 255) }, //Stone Slab (Double)
            { new Tuple<int, int>(43, 1), Color.FromArgb(223, 216, 164, 255) }, //Sandstone Slab (Double)
            { new Tuple<int, int>(43, 2), Color.FromArgb(146, 121, 68, 255) }, //Wooden Slab (Double)
            { new Tuple<int, int>(43, 3), Color.FromArgb(152, 152, 152, 255) }, //Cobblestone Slab (Double)
            { new Tuple<int, int>(43, 4), Color.FromArgb(225, 104, 73, 255) }, //Brick Slab (Double)
            { new Tuple<int, int>(43, 5), Color.FromArgb(120, 120, 120, 255) }, //Stone Brick Slab (Double)
            { new Tuple<int, int>(43, 6), Color.FromArgb(55, 24, 29, 255) }, //Nether Brick Slab (Double)
            { new Tuple<int, int>(43, 7), Color.FromArgb(234, 230, 224, 255) }, //Quartz Slab (Double)
            { new Tuple<int, int>(43, 8), Color.FromArgb(168, 168, 168, 255) }, //Smooth Stone Slab (Double)
            { new Tuple<int, int>(43, 9), Color.FromArgb(223, 216, 163, 255) }, //Smooth Sandstone Slab (Double)
            { new Tuple<int, int>(44, 0), Color.FromArgb(166, 166, 166, 255) }, //Stone Slab
            { new Tuple<int, int>(44, 1), Color.FromArgb(220, 212, 162, 255) }, //Sandstone Slab
            { new Tuple<int, int>(44, 2), Color.FromArgb(201, 162, 101, 255) }, //Wooden Slab
            { new Tuple<int, int>(44, 3), Color.FromArgb(109, 109, 109, 255) }, //Cobblestone Slab
            { new Tuple<int, int>(44, 4), Color.FromArgb(147, 80, 65, 255) }, //Brick Slab
            { new Tuple<int, int>(44, 5), Color.FromArgb(128, 128, 128, 255) }, //Stone Brick Slab
            { new Tuple<int, int>(44, 6), Color.FromArgb(70, 34, 41, 255) }, //Nether Brick Slab
            { new Tuple<int, int>(44, 7), Color.FromArgb(237, 235, 228, 255) }, //Quartz Slab
            { new Tuple<int, int>(45, 0), Color.FromArgb(188, 48, 6, 255) }, //Brick
            { new Tuple<int, int>(46, 0), Color.FromArgb(175, 38, 0, 255) }, //TNT
            { new Tuple<int, int>(47, 0), Color.FromArgb(107, 88, 57, 255) }, //Bookshelf
            { new Tuple<int, int>(48, 0), Color.FromArgb(101, 135, 101, 255) }, //Moss Stone
            { new Tuple<int, int>(49, 0), Color.FromArgb(20, 18, 29, 255) }, //Obsidian
            { new Tuple<int, int>(50, 0), Color.FromArgb(255, 255, 0, 255) }, //Torch
            { new Tuple<int, int>(51, 0), Color.FromArgb(222, 95, 0, 255) }, //Fire
            { new Tuple<int, int>(52, 0), Color.FromArgb(26, 39, 49, 255) }, //Mob Spawner
            { new Tuple<int, int>(53, 0), Color.FromArgb(166, 135, 78, 255) }, //Wooden Stairs (Oak)
            { new Tuple<int, int>(54, 0), Color.FromArgb(164, 116, 42, 255) }, //Chest
            { new Tuple<int, int>(55, 0), Color.FromArgb(255, 0, 0, 255) }, //Redstone Wire
            { new Tuple<int, int>(56, 0), Color.FromArgb(129, 140, 143, 255) }, //Diamond Ore
            { new Tuple<int, int>(57, 0), Color.FromArgb(97, 219, 213, 255) }, //Block of Diamond
            { new Tuple<int, int>(58, 0), Color.FromArgb(136, 80, 46, 255) }, //Workbench
            { new Tuple<int, int>(59, 0), Color.FromArgb(60, 89, 23, 255) }, //Wheat (Crop)
            { new Tuple<int, int>(60, 0), Color.FromArgb(130, 86, 51, 255) }, //Farmland
            { new Tuple<int, int>(61, 0), Color.FromArgb(103, 103, 103, 255) }, //Furnace
            { new Tuple<int, int>(62, 0), Color.FromArgb(255, 185, 0, 255) }, //Furnace (Smelting)
            { new Tuple<int, int>(63, 0), Color.FromArgb(184, 154, 91, 255) }, //Sign (Block)
            { new Tuple<int, int>(64, 0), Color.FromArgb(148, 115, 56, 255) }, //Wood Door (Block)
            { new Tuple<int, int>(65, 0), Color.FromArgb(121, 95, 52, 255) }, //Ladder
            { new Tuple<int, int>(66, 0), Color.FromArgb(182, 144, 81, 255) }, //Rail
            { new Tuple<int, int>(67, 0), Color.FromArgb(106, 106, 106, 255) }, //Cobblestone Stairs
            { new Tuple<int, int>(68, 0), Color.FromArgb(184, 154, 91, 255) }, //Sign (Wall Block)
            { new Tuple<int, int>(69, 0), Color.FromArgb(106, 89, 64, 255) }, //Lever
            { new Tuple<int, int>(70, 0), Color.FromArgb(122, 122, 122, 255) }, //Stone Pressure Plate
            { new Tuple<int, int>(71, 0), Color.FromArgb(194, 194, 194, 255) }, //Iron Door (Block)
            { new Tuple<int, int>(72, 0), Color.FromArgb(201, 160, 101, 255) }, //Wooden Pressure Plate
            { new Tuple<int, int>(73, 0), Color.FromArgb(132, 107, 107, 255) }, //Redstone Ore
            { new Tuple<int, int>(74, 0), Color.FromArgb(221, 45, 45, 255) }, //Redstone Ore (Glowing)
            { new Tuple<int, int>(75, 0), Color.FromArgb(102, 0, 0, 255) }, //Redstone Torch (Off)
            { new Tuple<int, int>(76, 0), Color.FromArgb(255, 97, 0, 255) }, //Redstone Torch
            { new Tuple<int, int>(77, 0), Color.FromArgb(132, 132, 132, 255) }, //Button (Stone)
            { new Tuple<int, int>(78, 0), Color.FromArgb(223, 241, 241, 255) }, //Snow
            { new Tuple<int, int>(79, 0), Color.FromArgb(125, 173, 255, 255) }, //Ice
            { new Tuple<int, int>(80, 0), Color.FromArgb(239, 251, 251, 255) }, //Snow Block
            { new Tuple<int, int>(81, 0), Color.FromArgb(15, 131, 29, 255) }, //Cactus
            { new Tuple<int, int>(82, 0), Color.FromArgb(158, 164, 176, 255) }, //Clay Block
            { new Tuple<int, int>(83, 0), Color.FromArgb(148, 192, 101, 255) }, //Sugar Cane (Block)
            { new Tuple<int, int>(84, 0), Color.FromArgb(133, 89, 59, 255) }, //Jukebox
            { new Tuple<int, int>(85, 0), Color.FromArgb(141, 116, 66, 255) }, //Fence (Oak)
            { new Tuple<int, int>(86, 0), Color.FromArgb(227, 140, 27, 255) }, //Pumpkin
            { new Tuple<int, int>(87, 0), Color.FromArgb(111, 54, 52, 255) }, //Netherrack
            { new Tuple<int, int>(88, 0), Color.FromArgb(84, 64, 51, 255) }, //Soul Sand
            { new Tuple<int, int>(89, 0), Color.FromArgb(143, 118, 69, 255) }, //Glowstone
            { new Tuple<int, int>(90, 0), Color.FromArgb(87, 10, 191, 255) }, //Portal
            { new Tuple<int, int>(91, 0), Color.FromArgb(241, 152, 33, 255) }, //Jack-O-Lantern
            { new Tuple<int, int>(92, 0), Color.FromArgb(236, 255, 255, 255) }, //Cake (Block)
            { new Tuple<int, int>(93, 0), Color.FromArgb(178, 178, 178, 255) }, //Redstone Repeater (Block Off)
            { new Tuple<int, int>(94, 0), Color.FromArgb(178, 178, 178, 255) }, //Redstone Repeater (Block On)
            { new Tuple<int, int>(95, 0), Color.FromArgb(255, 255, 255, 255) }, //Stained Glass (White)
            { new Tuple<int, int>(95, 1), Color.FromArgb(216, 127, 51, 255) }, //Stained Glass (Orange)
            { new Tuple<int, int>(95, 2), Color.FromArgb(178, 76, 216, 255) }, //Stained Glass (Magenta)
            { new Tuple<int, int>(95, 3), Color.FromArgb(102, 153, 216, 255) }, //Stained Glass (Light Blue)
            { new Tuple<int, int>(95, 4), Color.FromArgb(229, 229, 51, 255) }, //Stained Glass (Yellow)
            { new Tuple<int, int>(95, 5), Color.FromArgb(127, 204, 25, 255) }, //Stained Glass (Lime)
            { new Tuple<int, int>(95, 6), Color.FromArgb(242, 127, 165, 255) }, //Stained Glass (Pink)
            { new Tuple<int, int>(95, 7), Color.FromArgb(76, 76, 76, 255) }, //Stained Glass (Gray)
            { new Tuple<int, int>(95, 8), Color.FromArgb(117, 117, 117, 255) }, //Stained Glass (Light Grey)
            { new Tuple<int, int>(95, 9), Color.FromArgb(76, 127, 153, 255) }, //Stained Glass (Cyan)
            { new Tuple<int, int>(95, 10), Color.FromArgb(127, 63, 178, 255) }, //Stained Glass (Purple)
            { new Tuple<int, int>(95, 11), Color.FromArgb(51, 76, 178, 255) }, //Stained Glass (Blue)
            { new Tuple<int, int>(95, 12), Color.FromArgb(102, 76, 51, 255) }, //Stained Glass (Brown)
            { new Tuple<int, int>(95, 13), Color.FromArgb(102, 127, 51, 255) }, //Stained Glass (Green)
            { new Tuple<int, int>(95, 14), Color.FromArgb(153, 51, 51, 255) }, //Stained Glass (Red)
            { new Tuple<int, int>(95, 15), Color.FromArgb(25, 25, 25, 255) }, //Stained Glass (Black)
            { new Tuple<int, int>(96, 0), Color.FromArgb(126, 93, 45, 255) }, //Trapdoor
            { new Tuple<int, int>(97, 0), Color.FromArgb(124, 124, 124, 255) }, //Monster Egg (Stone)
            { new Tuple<int, int>(97, 1), Color.FromArgb(141, 141, 141, 255) }, //Monster Egg (Cobblestone)
            { new Tuple<int, int>(97, 2), Color.FromArgb(122, 122, 122, 255) }, //Monster Egg (Stone Brick)
            { new Tuple<int, int>(97, 3), Color.FromArgb(105, 120, 81, 255) }, //Monster Egg (Mossy Stone Brick)
            { new Tuple<int, int>(97, 4), Color.FromArgb(104, 104, 104, 255) }, //Monster Egg (Cracked Stone)
            { new Tuple<int, int>(97, 5), Color.FromArgb(118, 118, 118, 255) }, //Monster Egg (Chiseled Stone)
            { new Tuple<int, int>(98, 0), Color.FromArgb(122, 122, 122, 255) }, //Stone Bricks
            { new Tuple<int, int>(98, 1), Color.FromArgb(122, 122, 122, 255) }, //Mossy Stone Bricks
            { new Tuple<int, int>(98, 2), Color.FromArgb(122, 122, 122, 255) }, //Cracked Stone Bricks
            { new Tuple<int, int>(98, 3), Color.FromArgb(122, 122, 122, 255) }, //Chiseled Stone Brick
            { new Tuple<int, int>(99, 0), Color.FromArgb(207, 175, 124, 255) }, //Brown Mushroom (Block)
            { new Tuple<int, int>(100, 0), Color.FromArgb(202, 170, 120, 255) }, //Red Mushroom (Block)
            { new Tuple<int, int>(101, 0), Color.FromArgb(109, 108, 106, 255) }, //Iron Bars
            { new Tuple<int, int>(102, 0), Color.FromArgb(211, 239, 244, 255) }, //Glass Pane
            { new Tuple<int, int>(103, 0), Color.FromArgb(196, 189, 40, 255) }, //Melon (Block)
            { new Tuple<int, int>(104, 0), Color.FromArgb(146, 221, 105, 255) }, //Pumpkin Vine
            { new Tuple<int, int>(105, 0), Color.FromArgb(115, 174, 83, 255) }, //Melon Vine
            { new Tuple<int, int>(106, 0), Color.FromArgb(32, 81, 12, 255) }, //Vines
            { new Tuple<int, int>(107, 0), Color.FromArgb(165, 135, 82, 255) }, //Fence Gate (Oak)
            { new Tuple<int, int>(108, 0), Color.FromArgb(148, 64, 42, 255) }, //Brick Stairs
            { new Tuple<int, int>(109, 0), Color.FromArgb(122, 122, 122, 255) }, //Stone Brick Stairs
            { new Tuple<int, int>(110, 0), Color.FromArgb(138, 113, 117, 255) }, //Mycelium
            { new Tuple<int, int>(111, 0), Color.FromArgb(118, 118, 118, 255) }, //Lily Pad
            { new Tuple<int, int>(112, 0), Color.FromArgb(44, 22, 26, 255) }, //Nether Brick
            { new Tuple<int, int>(113, 0), Color.FromArgb(44, 22, 26, 255) }, //Nether Brick Fence
            { new Tuple<int, int>(114, 0), Color.FromArgb(44, 22, 26, 255) }, //Nether Brick Stairs
            { new Tuple<int, int>(115, 0), Color.FromArgb(166, 40, 45, 255) }, //Nether Wart
            { new Tuple<int, int>(116, 0), Color.FromArgb(84, 196, 177, 255) }, //Enchantment Table
            { new Tuple<int, int>(117, 0), Color.FromArgb(124, 103, 81, 255) }, //Brewing Stand (Block)
            { new Tuple<int, int>(118, 0), Color.FromArgb(73, 73, 73, 255) }, //Cauldron (Block)
            { new Tuple<int, int>(119, 0), Color.FromArgb(52, 52, 52, 255) }, //End Portal
            { new Tuple<int, int>(120, 0), Color.FromArgb(52, 137, 209, 255) }, //End Portal Frame
            { new Tuple<int, int>(121, 0), Color.FromArgb(221, 223, 165, 255) }, //End Stone
            { new Tuple<int, int>(122, 0), Color.FromArgb(12, 9, 15, 255) }, //Dragon Egg
            { new Tuple<int, int>(123, 0), Color.FromArgb(151, 99, 49, 255) }, //Redstone Lamp
            { new Tuple<int, int>(124, 0), Color.FromArgb(227, 160, 66, 255) }, //Redstone Lamp (On)
            { new Tuple<int, int>(125, 0), Color.FromArgb(161, 132, 77, 255) }, //Oak-Wood Slab (Double)
            { new Tuple<int, int>(125, 1), Color.FromArgb(125, 90, 54, 255) }, //Spruce-Wood Slab (Double)
            { new Tuple<int, int>(125, 2), Color.FromArgb(215, 204, 141, 255) }, //Birch-Wood Slab (Double)
            { new Tuple<int, int>(125, 3), Color.FromArgb(183, 133, 96, 255) }, //Jungle-Wood Slab (Double)
            { new Tuple<int, int>(125, 4), Color.FromArgb(169, 88, 48, 255) }, //Acacia Wood Slab (Double)
            { new Tuple<int, int>(125, 5), Color.FromArgb(67, 42, 21, 255) }, //Dark Oak Wood Slab (Double)
            { new Tuple<int, int>(126, 0), Color.FromArgb(158, 133, 73, 255) }, //Oak-Wood Slab
            { new Tuple<int, int>(126, 1), Color.FromArgb(100, 79, 46, 255) }, //Spruce-Wood Slab
            { new Tuple<int, int>(126, 2), Color.FromArgb(235, 225, 155, 255) }, //Birch-Wood Slab
            { new Tuple<int, int>(126, 3), Color.FromArgb(139, 97, 60, 255) }, //Jungle-Wood Slab
            { new Tuple<int, int>(126, 4), Color.FromArgb(171, 92, 51, 255) }, //Acacia Wood Slab
            { new Tuple<int, int>(126, 5), Color.FromArgb(66, 42, 18, 255) }, //Dark Oak Wood Slab
            { new Tuple<int, int>(127, 0), Color.FromArgb(221, 113, 31, 255) }, //Cocoa Plant
            { new Tuple<int, int>(128, 0), Color.FromArgb(231, 226, 168, 255) }, //Sandstone Stairs
            { new Tuple<int, int>(129, 0), Color.FromArgb(109, 128, 116, 255) }, //Emerald Ore
            { new Tuple<int, int>(130, 0), Color.FromArgb(42, 58, 60, 255) }, //Ender Chest
            { new Tuple<int, int>(131, 0), Color.FromArgb(124, 124, 124, 255) }, //Tripwire Hook
            { new Tuple<int, int>(132, 0), Color.FromArgb(90, 90, 90, 255) }, //Tripwire
            { new Tuple<int, int>(133, 0), Color.FromArgb(81, 217, 117, 255) }, //Block of Emerald
            { new Tuple<int, int>(134, 0), Color.FromArgb(129, 94, 52, 255) }, //Wooden Stairs (Spruce)
            { new Tuple<int, int>(135, 0), Color.FromArgb(206, 192, 132, 255) }, //Wooden Stairs (Birch)
            { new Tuple<int, int>(136, 0), Color.FromArgb(136, 95, 69, 255) }, //Wooden Stairs (Jungle)
            { new Tuple<int, int>(137, 0), Color.FromArgb(142, 139, 134, 255) }, //Command Block
            { new Tuple<int, int>(138, 0), Color.FromArgb(116, 221, 215, 255) }, //Beacon
            { new Tuple<int, int>(139, 0), Color.FromArgb(89, 89, 89, 255) }, //Cobblestone Wall
            { new Tuple<int, int>(139, 1), Color.FromArgb(42, 94, 42, 255) }, //Mossy Cobblestone Wall
            { new Tuple<int, int>(140, 0), Color.FromArgb(118, 65, 51, 255) }, //Flower Pot (Block)
            { new Tuple<int, int>(141, 0), Color.FromArgb(10, 140, 0, 255) }, //Carrot (Crop)
            { new Tuple<int, int>(142, 0), Color.FromArgb(4, 164, 23, 255) }, //Potatoes (Crop)
            { new Tuple<int, int>(143, 0), Color.FromArgb(179, 146, 89, 255) }, //Button (Wood)
            { new Tuple<int, int>(144, 0), Color.FromArgb(176, 176, 176, 255) }, //Head Block (Skeleton)
            { new Tuple<int, int>(144, 1), Color.FromArgb(79, 85, 85, 255) }, //Head Block (Wither)
            { new Tuple<int, int>(144, 2), Color.FromArgb(98, 146, 75, 255) }, //Head Block (Zombie)
            { new Tuple<int, int>(144, 3), Color.FromArgb(204, 151, 126, 255) }, //Head Block (Steve)
            { new Tuple<int, int>(144, 4), Color.FromArgb(82, 175, 67, 255) }, //Head Block (Creeper)
            { new Tuple<int, int>(145, 0), Color.FromArgb(71, 67, 67, 255) }, //Anvil
            { new Tuple<int, int>(145, 1), Color.FromArgb(71, 67, 67, 255) }, //Anvil (Slightly Damaged)
            { new Tuple<int, int>(145, 2), Color.FromArgb(71, 67, 67, 255) }, //Anvil (Very Damaged)
            { new Tuple<int, int>(146, 0), Color.FromArgb(158, 107, 29, 255) }, //Trapped Chest
            { new Tuple<int, int>(147, 0), Color.FromArgb(254, 253, 112, 255) }, //Weighted Pressure Plate (Light)
            { new Tuple<int, int>(148, 0), Color.FromArgb(229, 229, 229, 255) }, //Weighted Pressure Plate (Heavy)
            { new Tuple<int, int>(149, 0), Color.FromArgb(75, 74, 76, 255) }, //Redstone Comparator (Off)
            { new Tuple<int, int>(150, 0), Color.FromArgb(191, 198, 189, 255) }, //Redstone Comparator (On)
            { new Tuple<int, int>(151, 0), Color.FromArgb(251, 237, 221, 255) }, //Daylight Sensor
            { new Tuple<int, int>(152, 0), Color.FromArgb(171, 27, 9, 255) }, //Block of Redstone
            { new Tuple<int, int>(153, 0), Color.FromArgb(125, 84, 79, 255) }, //Nether Quartz Ore
            { new Tuple<int, int>(154, 0), Color.FromArgb(113, 113, 113, 255) }, //Hopper
            { new Tuple<int, int>(155, 0), Color.FromArgb(234, 230, 223, 255) }, //Quartz Block
            { new Tuple<int, int>(155, 1), Color.FromArgb(224, 219, 210, 255) }, //Chiseled Quartz Block
            { new Tuple<int, int>(155, 2), Color.FromArgb(234, 231, 225, 255) }, //Pillar Quartz Block
            { new Tuple<int, int>(156, 0), Color.FromArgb(235, 232, 227, 255) }, //Quartz Stairs
            { new Tuple<int, int>(157, 0), Color.FromArgb(155, 129, 65, 255) }, //Rail (Activator)
            { new Tuple<int, int>(158, 0), Color.FromArgb(116, 116, 116, 255) }, //Dropper
            { new Tuple<int, int>(159, 0), Color.FromArgb(209, 178, 161, 255) }, //Stained Clay (White)
            { new Tuple<int, int>(159, 1), Color.FromArgb(161, 83, 37, 255) }, //Stained Clay (Orange)
            { new Tuple<int, int>(159, 2), Color.FromArgb(149, 88, 108, 255) }, //Stained Clay (Magenta)
            { new Tuple<int, int>(159, 3), Color.FromArgb(113, 108, 137, 255) }, //Stained Clay (Light Blue)
            { new Tuple<int, int>(159, 4), Color.FromArgb(186, 133, 35, 255) }, //Stained Clay (Yellow)
            { new Tuple<int, int>(159, 5), Color.FromArgb(103, 117, 52, 255) }, //Stained Clay (Lime)
            { new Tuple<int, int>(159, 6), Color.FromArgb(161, 78, 78, 255) }, //Stained Clay (Pink)
            { new Tuple<int, int>(159, 7), Color.FromArgb(57, 42, 35, 255) }, //Stained Clay (Gray)
            { new Tuple<int, int>(159, 8), Color.FromArgb(135, 104, 95, 255) }, //Stained Clay (Light Gray)
            { new Tuple<int, int>(159, 9), Color.FromArgb(86, 91, 91, 255) }, //Stained Clay (Cyan)
            { new Tuple<int, int>(159, 10), Color.FromArgb(118, 70, 86, 255) }, //Stained Clay (Purple)
            { new Tuple<int, int>(159, 11), Color.FromArgb(74, 59, 91, 255) }, //Stained Clay (Blue)
            { new Tuple<int, int>(159, 12), Color.FromArgb(77, 51, 35, 255) }, //Stained Clay (Brown)
            { new Tuple<int, int>(159, 13), Color.FromArgb(76, 83, 42, 255) }, //Stained Clay (Green)
            { new Tuple<int, int>(159, 14), Color.FromArgb(143, 61, 46, 255) }, //Stained Clay (Red)
            { new Tuple<int, int>(159, 15), Color.FromArgb(37, 22, 16, 255) }, //Stained Clay (Black)
            { new Tuple<int, int>(160, 0), Color.FromArgb(246, 246, 246, 255) }, //Stained Glass Pane (White)
            { new Tuple<int, int>(160, 1), Color.FromArgb(208, 122, 48, 255) }, //Stained Glass Pane (Orange)
            { new Tuple<int, int>(160, 2), Color.FromArgb(171, 73, 208, 255) }, //Stained Glass Pane (Magenta)
            { new Tuple<int, int>(160, 3), Color.FromArgb(97, 147, 208, 255) }, //Stained Glass Pane (Light Blue)
            { new Tuple<int, int>(160, 4), Color.FromArgb(221, 221, 48, 255) }, //Stained Glass Pane (Yellow)
            { new Tuple<int, int>(160, 5), Color.FromArgb(122, 196, 24, 255) }, //Stained Glass Pane (Lime)
            { new Tuple<int, int>(160, 6), Color.FromArgb(233, 122, 159, 255) }, //Stained Glass Pane (Pink)
            { new Tuple<int, int>(160, 7), Color.FromArgb(73, 73, 73, 255) }, //Stained Glass Pane (Gray)
            { new Tuple<int, int>(160, 8), Color.FromArgb(145, 145, 145, 255) }, //Stained Glass Pane (Light Gray)
            { new Tuple<int, int>(160, 9), Color.FromArgb(73, 122, 147, 255) }, //Stained Glass Pane (Cyan)
            { new Tuple<int, int>(160, 10), Color.FromArgb(122, 61, 171, 255) }, //Stained Glass Pane (Purple)
            { new Tuple<int, int>(160, 11), Color.FromArgb(48, 73, 171, 255) }, //Stained Glass Pane (Blue)
            { new Tuple<int, int>(160, 12), Color.FromArgb(97, 73, 48, 255) }, //Stained Glass Pane (Brown)
            { new Tuple<int, int>(160, 13), Color.FromArgb(97, 122, 48, 255) }, //Stained Glass Pane (Green)
            { new Tuple<int, int>(160, 14), Color.FromArgb(147, 48, 48, 255) }, //Stained Glass Pane (Red)
            { new Tuple<int, int>(160, 15), Color.FromArgb(24, 24, 24, 255) }, //Stained Glass Pane (Black)
            { new Tuple<int, int>(161, 0), Color.FromArgb(135, 135, 135, 255) }, //Leaves (Acacia)
            { new Tuple<int, int>(161, 1), Color.FromArgb(55, 104, 33, 255) }, //Leaves (Dark Oak)
            { new Tuple<int, int>(162, 0), Color.FromArgb(176, 90, 57, 255) }, //Wood (Acacia Oak)
            { new Tuple<int, int>(162, 1), Color.FromArgb(93, 74, 49, 255) }, //Wood (Dark Oak)
            { new Tuple<int, int>(163, 0), Color.FromArgb(172, 92, 50, 255) }, //Wooden Stairs (Acacia)
            { new Tuple<int, int>(164, 0), Color.FromArgb(71, 44, 21, 255) }, //Wooden Stairs (Dark Oak)
            { new Tuple<int, int>(165, 0), Color.FromArgb(120, 200, 101, 255) }, //Slime Block
            { new Tuple<int, int>(166, 0), Color.FromArgb(223, 52, 53, 255) }, //Barrier
            { new Tuple<int, int>(167, 0), Color.FromArgb(199, 199, 199, 255) }, //Iron Trapdoor
            { new Tuple<int, int>(168, 0), Color.FromArgb(114, 175, 165, 255) }, //Prismarine
            { new Tuple<int, int>(168, 1), Color.FromArgb(92, 158, 143, 255) }, //Prismarine Bricks
            { new Tuple<int, int>(168, 2), Color.FromArgb(72, 106, 94, 255) }, //Dark Prismarine
            { new Tuple<int, int>(169, 0), Color.FromArgb(172, 199, 190, 255) }, //Sea Lantern
            { new Tuple<int, int>(170, 0), Color.FromArgb(220, 211, 159, 255) }, //Hay Bale
            { new Tuple<int, int>(171, 0), Color.FromArgb(202, 202, 202, 255) }, //Carpet (White)
            { new Tuple<int, int>(171, 1), Color.FromArgb(221, 133, 75, 255) }, //Carpet (Orange)
            { new Tuple<int, int>(171, 2), Color.FromArgb(177, 67, 186, 255) }, //Carpet (Magenta)
            { new Tuple<int, int>(171, 3), Color.FromArgb(75, 113, 189, 255) }, //Carpet (Light Blue)
            { new Tuple<int, int>(171, 4), Color.FromArgb(197, 183, 44, 255) }, //Carpet (Yellow)
            { new Tuple<int, int>(171, 5), Color.FromArgb(60, 161, 51, 255) }, //Carpet (Lime)
            { new Tuple<int, int>(171, 6), Color.FromArgb(206, 142, 168, 255) }, //Carpet (Pink)
            { new Tuple<int, int>(171, 7), Color.FromArgb(70, 70, 70, 255) }, //Carpet (Grey)
            { new Tuple<int, int>(171, 8), Color.FromArgb(162, 162, 162, 255) }, //Carpet (Light Gray)
            { new Tuple<int, int>(171, 9), Color.FromArgb(48, 116, 145, 255) }, //Carpet (Cyan)
            { new Tuple<int, int>(171, 10), Color.FromArgb(148, 81, 202, 255) }, //Carpet (Purple)
            { new Tuple<int, int>(171, 11), Color.FromArgb(54, 69, 171, 255) }, //Carpet (Blue)
            { new Tuple<int, int>(171, 12), Color.FromArgb(82, 52, 32, 255) }, //Carpet (Brown)
            { new Tuple<int, int>(171, 13), Color.FromArgb(62, 85, 33, 255) }, //Carpet (Green)
            { new Tuple<int, int>(171, 14), Color.FromArgb(187, 61, 57, 255) }, //Carpet (Red)
            { new Tuple<int, int>(171, 15), Color.FromArgb(35, 31, 31, 255) }, //Carpet (Black)
            { new Tuple<int, int>(172, 0), Color.FromArgb(150, 92, 66, 255) }, //Hardened Clay
            { new Tuple<int, int>(173, 0), Color.FromArgb(18, 18, 18, 255) }, //Block of Coal
            { new Tuple<int, int>(174, 0), Color.FromArgb(162, 191, 244, 255) }, //Packed Ice
            { new Tuple<int, int>(175, 0), Color.FromArgb(207, 116, 20, 255) }, //Sunflower
            { new Tuple<int, int>(175, 1), Color.FromArgb(168, 112, 178, 255) }, //Lilac
            { new Tuple<int, int>(175, 2), Color.FromArgb(102, 158, 88, 255) }, //Double Tallgrass
            { new Tuple<int, int>(175, 3), Color.FromArgb(84, 129, 72, 255) }, //Large Fern
            { new Tuple<int, int>(175, 4), Color.FromArgb(215, 2, 8, 255) }, //Rose Bush
            { new Tuple<int, int>(175, 5), Color.FromArgb(192, 150, 207, 255) }, //Peony
            { new Tuple<int, int>(176, 0), Color.FromArgb(240, 240, 240, 255) }, //Standing Banner (Block)
            { new Tuple<int, int>(177, 0), Color.FromArgb(240, 240, 240, 255) }, //Wall Banner (Block)
            { new Tuple<int, int>(178, 0), Color.FromArgb(240, 240, 240, 255) }, //Inverted Daylight Sensor
            { new Tuple<int, int>(179, 0), Color.FromArgb(172, 86, 29, 255) }, //Red Sandstone
            { new Tuple<int, int>(179, 1), Color.FromArgb(172, 86, 29, 255) }, //Red Sandstone (Chiseled)
            { new Tuple<int, int>(179, 2), Color.FromArgb(172, 86, 29, 255) }, //Red Sandstone (Smooth)
            { new Tuple<int, int>(180, 0), Color.FromArgb(174, 87, 29, 255) }, //Red Sandstone Stairs
            { new Tuple<int, int>(181, 0), Color.FromArgb(174, 87, 29, 255) }, //Red Sandstone Slab (Double)
            { new Tuple<int, int>(182, 0), Color.FromArgb(174, 87, 29, 255) }, //Red Sandstone Slab
            { new Tuple<int, int>(183, 0), Color.FromArgb(80, 60, 36, 255) }, //Fence Gate (Spruce)
            { new Tuple<int, int>(184, 0), Color.FromArgb(221, 205, 141, 255) }, //Fence Gate (Birch)
            { new Tuple<int, int>(185, 0), Color.FromArgb(175, 122, 77, 255) }, //Fence Gate (Jungle)
            { new Tuple<int, int>(186, 0), Color.FromArgb(52, 32, 14, 255) }, //Fence Gate (Dark Oak)
            { new Tuple<int, int>(187, 0), Color.FromArgb(207, 107, 54, 255) }, //Fence Gate (Acacia)
            { new Tuple<int, int>(188, 0), Color.FromArgb(126, 93, 53, 255) }, //Fence (Spruce)
            { new Tuple<int, int>(189, 0), Color.FromArgb(199, 184, 123, 255) }, //Fence (Birch)
            { new Tuple<int, int>(190, 0), Color.FromArgb(187, 134, 95, 255) }, //Fence (Jungle)
            { new Tuple<int, int>(191, 0), Color.FromArgb(63, 46, 30, 255) }, //Fence (Dark Oak)
            { new Tuple<int, int>(192, 0), Color.FromArgb(197, 107, 58, 255) }, //Fence (Acacia)
            { new Tuple<int, int>(193, 0), Color.FromArgb(110, 83, 48, 255) }, //Wooden Door Block (Spruce)
            { new Tuple<int, int>(194, 0), Color.FromArgb(247, 243, 224, 255) }, //Wooden Door Block (Birch)
            { new Tuple<int, int>(195, 0), Color.FromArgb(169, 119, 80, 255) }, //Wooden Door Block (Jungle)
            { new Tuple<int, int>(196, 0), Color.FromArgb(170, 85, 41, 255) }, //Wooden Door Block (Acacia)
            { new Tuple<int, int>(197, 0), Color.FromArgb(78, 55, 33, 255) }, //Wooden Door Block (Dark Oak)
            { new Tuple<int, int>(198, 0), Color.FromArgb(220, 197, 205, 255) }, //End rod
            { new Tuple<int, int>(199, 0), Color.FromArgb(96, 59, 96, 255) }, //Chorus Plant
            { new Tuple<int, int>(200, 0), Color.FromArgb(133, 103, 133, 255) }, //Chorus Flower
            { new Tuple<int, int>(201, 0), Color.FromArgb(166, 121, 166, 255) }, //Purpur Block
            { new Tuple<int, int>(202, 0), Color.FromArgb(170, 126, 170, 255) }, //Purpur Pillar
            { new Tuple<int, int>(203, 0), Color.FromArgb(168, 121, 168, 255) }, //Purpur Stairs
            { new Tuple<int, int>(204, 0), Color.FromArgb(168, 121, 168, 255) }, //Purpur Slab (Double)
            { new Tuple<int, int>(205, 0), Color.FromArgb(168, 121, 168, 255) }, //Purpur Slab
            { new Tuple<int, int>(206, 0), Color.FromArgb(225, 230, 170, 255) }, //End Stone Bricks
            { new Tuple<int, int>(207, 0), Color.FromArgb(179, 134, 0, 255) }, //Beetroot Block
            { new Tuple<int, int>(208, 0), Color.FromArgb(152, 125, 69, 255) }, //Grass Path
            { new Tuple<int, int>(209, 0), Color.FromArgb(240, 240, 240, 255) }, //End Gateway
            { new Tuple<int, int>(210, 0), Color.FromArgb(155, 137, 39, 255) }, //Repeating Command Block
            { new Tuple<int, int>(211, 0), Color.FromArgb(118, 178, 151, 255) }, //Chain Command Block
            { new Tuple<int, int>(212, 0), Color.FromArgb(118, 162, 252, 255) }, //Frosted Ice
            { new Tuple<int, int>(213, 0), Color.FromArgb(202, 78, 6, 255) }, //Magma Block
            { new Tuple<int, int>(214, 0), Color.FromArgb(129, 0, 8, 255) }, //Nether Wart Block
            { new Tuple<int, int>(215, 0), Color.FromArgb(86, 0, 4, 255) }, //Red Nether Brick
            { new Tuple<int, int>(216, 0), Color.FromArgb(143, 147, 131, 255) }, //Bone Block
            { new Tuple<int, int>(217, 0), Color.FromArgb(0, 0, 0, 0) }, //Void Block
            { new Tuple<int, int>(218, 0), Color.FromArgb(43, 43, 43, 255) }, //Observer
            { new Tuple<int, int>(219, 0), Color.FromArgb(223, 223, 220, 255) }, //White Shulker Box
            { new Tuple<int, int>(220, 0), Color.FromArgb(208, 118, 59, 255) }, //Orange Shulker Box
            { new Tuple<int, int>(221, 0), Color.FromArgb(186, 100, 194, 255) }, //Magenta Shulker Box
            { new Tuple<int, int>(222, 0), Color.FromArgb(103, 143, 204, 255) }, //Light Blue Shulker Box
            { new Tuple<int, int>(223, 0), Color.FromArgb(193, 183, 61, 255) }, //Yellow Shulker Box
            { new Tuple<int, int>(224, 0), Color.FromArgb(73, 185, 61, 255) }, //Lime Shulker Box
            { new Tuple<int, int>(225, 0), Color.FromArgb(208, 140, 161, 255) }, //Pink Shulker Box
            { new Tuple<int, int>(226, 0), Color.FromArgb(84, 82, 82, 255) }, //Gray Shulker Box
            { new Tuple<int, int>(227, 0), Color.FromArgb(165, 162, 162, 255) }, //Light Gray Shulker Box
            { new Tuple<int, int>(228, 0), Color.FromArgb(69, 137, 165, 255) }, //Cyan Shulker Box
            { new Tuple<int, int>(229, 0), Color.FromArgb(151, 105, 151, 255) }, //Purple Shulker Box
            { new Tuple<int, int>(230, 0), Color.FromArgb(102, 114, 202, 255) }, //Blue Shulker Box
            { new Tuple<int, int>(231, 0), Color.FromArgb(142, 113, 94, 255) }, //Brown Shulker Box
            { new Tuple<int, int>(232, 0), Color.FromArgb(112, 131, 85, 255) }, //Green Shulker Box
            { new Tuple<int, int>(233, 0), Color.FromArgb(195, 89, 86, 255) }, //Red Shulker Box
            { new Tuple<int, int>(234, 0), Color.FromArgb(58, 55, 55, 255) }, //Black Shulker Box
            { new Tuple<int, int>(235, 0), Color.FromArgb(249, 255, 254, 255) }, //White Glazed Terracota
            { new Tuple<int, int>(236, 0), Color.FromArgb(225, 97, 0, 255) }, //Orange Glazed Terracota
            { new Tuple<int, int>(237, 0), Color.FromArgb(241, 165, 191, 255) }, //Magenta Glazed Terracota
            { new Tuple<int, int>(238, 0), Color.FromArgb(77, 185, 221, 255) }, //Light Blue Glazed Terracota
            { new Tuple<int, int>(239, 0), Color.FromArgb(238, 170, 13, 255) }, //Yellow Glazed Terracota
            { new Tuple<int, int>(240, 0), Color.FromArgb(133, 207, 33, 255) }, //Lime Glazed Terracota
            { new Tuple<int, int>(241, 0), Color.FromArgb(244, 181, 203, 255) }, //Pink Glazed Terracota
            { new Tuple<int, int>(242, 0), Color.FromArgb(96, 114, 119, 255) }, //Gray Glazed Terracota
            { new Tuple<int, int>(243, 0), Color.FromArgb(204, 208, 210, 255) }, //Light Gray Glazed Terracota
            { new Tuple<int, int>(244, 0), Color.FromArgb(23, 168, 168, 255) }, //Cyan Glazed Terracota
            { new Tuple<int, int>(245, 0), Color.FromArgb(100, 31, 156, 255) }, //Purple Glazed Terracota
            { new Tuple<int, int>(246, 0), Color.FromArgb(44, 46, 143, 255) }, //Blue Glazed Terracota
            { new Tuple<int, int>(247, 0), Color.FromArgb(171, 123, 80, 255) }, //Brown Glazed Terracota
            { new Tuple<int, int>(248, 0), Color.FromArgb(117, 160, 37, 255) }, //Green Glazed Terracota
            { new Tuple<int, int>(249, 0), Color.FromArgb(209, 86, 80, 255) }, //Red Glazed Terracota
            { new Tuple<int, int>(250, 0), Color.FromArgb(62, 14, 14, 255) }, //Black Glazed Terracota
            { new Tuple<int, int>(251, 0), Color.FromArgb(207, 213, 214, 255) }, //White Concrete
            { new Tuple<int, int>(251, 1), Color.FromArgb(224, 97, 0, 255) }, //Orange Concrete
            { new Tuple<int, int>(251, 2), Color.FromArgb(169, 48, 159, 255) }, //Magenta Concrete
            { new Tuple<int, int>(251, 3), Color.FromArgb(35, 137, 199, 255) }, //Light Blue Concrete
            { new Tuple<int, int>(251, 4), Color.FromArgb(239, 174, 21, 255) }, //Yellow Concrete
            { new Tuple<int, int>(251, 5), Color.FromArgb(95, 170, 25, 255) }, //Lime Concrete
            { new Tuple<int, int>(251, 6), Color.FromArgb(213, 100, 142, 255) }, //Pink Concrete
            { new Tuple<int, int>(251, 7), Color.FromArgb(54, 57, 61, 255) }, //Gray Concrete
            { new Tuple<int, int>(251, 8), Color.FromArgb(125, 125, 115, 255) }, //Light Gray Concrete
            { new Tuple<int, int>(251, 9), Color.FromArgb(21, 119, 136, 255) }, //Cyan Concrete
            { new Tuple<int, int>(251, 10), Color.FromArgb(99, 31, 155, 255) }, //Purple Concrete
            { new Tuple<int, int>(251, 11), Color.FromArgb(45, 47, 144, 255) }, //Blue Concrete
            { new Tuple<int, int>(251, 12), Color.FromArgb(97, 60, 32, 255) }, //Brown Concrete
            { new Tuple<int, int>(251, 13), Color.FromArgb(73, 91, 36, 255) }, //Green Concrete
            { new Tuple<int, int>(251, 14), Color.FromArgb(143, 33, 33, 255) }, //Red Concrete
            { new Tuple<int, int>(251, 15), Color.FromArgb(7, 9, 14, 255) }, //Black Concrete
            { new Tuple<int, int>(252, 0), Color.FromArgb(230, 232, 233, 255) }, //White Concrete Powder
            { new Tuple<int, int>(252, 1), Color.FromArgb(234, 136, 34, 255) }, //Orange Concrete Powder
            { new Tuple<int, int>(252, 2), Color.FromArgb(195, 81, 186, 255) }, //Magenta Concrete Powder
            { new Tuple<int, int>(252, 3), Color.FromArgb(90, 197, 221, 255) }, //Light Blue Concrete Powder
            { new Tuple<int, int>(252, 4), Color.FromArgb(237, 198, 48, 255) }, //Yellow Concrete Powder
            { new Tuple<int, int>(252, 5), Color.FromArgb(128, 191, 41, 255) }, //Lime Concrete Powder
            { new Tuple<int, int>(252, 6), Color.FromArgb(239, 174, 197, 255) }, //Pink Concrete Powder
            { new Tuple<int, int>(252, 7), Color.FromArgb(76, 80, 85, 255) }, //Gray Concrete Powder
            { new Tuple<int, int>(252, 8), Color.FromArgb(154, 154, 147, 255) }, //Light Gray Concrete Powder
            { new Tuple<int, int>(252, 9), Color.FromArgb(35, 148, 154, 255) }, //Cyan Concrete Powder
            { new Tuple<int, int>(252, 10), Color.FromArgb(120, 48, 169, 255) }, //Purple Concrete Powder
            { new Tuple<int, int>(252, 11), Color.FromArgb(72, 74, 171, 255) }, //Blue Concrete Powder
            { new Tuple<int, int>(252, 12), Color.FromArgb(121, 81, 51, 255) }, //Brown Concrete Powder
            { new Tuple<int, int>(252, 13), Color.FromArgb(103, 124, 55, 255) }, //Green Concrete Powder
            { new Tuple<int, int>(252, 14), Color.FromArgb(164, 51, 50, 255) }, //Red Concrete Powder
            { new Tuple<int, int>(252, 15), Color.FromArgb(22, 24, 28, 255) }, //Black Concrete Powder

            { new Tuple<int, int>(255, 0), Color.FromArgb(50, 33, 36, 255) }, //Structure Block

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

        public static Color UIntToColor(this uint color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            return Color.FromArgb(r, g, b, a);
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
