global using static sandwich.Constants;
using System;
using System.Collections.Generic;
using sandwich.Items;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace sandwich;

public static class Constants
{

    public const string CuttingBoardable = "cuttingboardable";
    public const string CuttingBoardTransform = "cuttingboardTransform";

    public static readonly AssetLocation DefaultPlaceSound = new("sounds/player/build");

    public static readonly List<TransformConfig> TransformConfigs = new()
    {
        new() { AttributeName = CuttingBoardTransform, Title = "On Cutting Board" },
    };

    public static readonly string[] CuttingBoardableCodes = new string[]
{
        "sandwich:breadslice",
        "game:bread-*-perfect",
        "game:cheese-cheddar-1slice",
        "game:redmeat-cooked"
};

    public static readonly Type[] CuttingBoardableTypes = new Type[]
    {
        typeof(ItemBreadSlice)
    };
}