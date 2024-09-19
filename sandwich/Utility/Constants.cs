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
    public const string ModId = "sandwich";

    public const string attributeCodeCuttingBoard = $"{ModId}:canPutOnCuttingBoard";
    public const string attributeCuttingBoardProperties = $"{ModId}:cuttingBoardProperties";
    public const string attributeOffsets = "offsets";
    public const string attributeOnCuttingBoardTransform = $"{ModId}:onCuttingBoardTransform";
    public const string attributeWhenOnSandwich = $"{ModId}:whenOnSandwich";
    public const string attributeSandwichLayers = "sandwichLayers";

    public const string langEmpty = "Empty";
    public const string langContents0x1 = "Contents: {0}x {1}";
    public const string langContents = "Contents: ";
    public const string langNutritionfacts = "nutrition-facts-line-satiety";
    public const string langSandwichContents = $"{ModId}:contents-sandwich";
    public const string langSandwich = $"{ModId}:item-sandwich";
    public const string langWhenEaten = $"{ModId}:when-eaten";

    public const string worldConfigSandwichLayersLimit = $"{ModId}:sandwichLayersLimit";
    public const int defaultSandwichLayersLimit = 5;

    public const string cuttingBoardInvClassName = $"{ModId}:cuttingBoard";

    public static readonly AssetLocation soundBuild = AssetLocation.Create("sounds/player/build");

    public const string CuttingBoardable = "cuttingboardable";
    public const string CuttingBoardTransform = "cuttingboardTransform";

    public static readonly AssetLocation DefaultPlaceSound = new("sounds/player/build");

    public static readonly List<TransformConfig> TransformConfigs = new()
    {
        new() { AttributeName = CuttingBoardTransform, Title = "On Cutting Board" },
    };

    public static readonly string[] CuttingBoardableCodes = new string[]
{
        "sandwich:breadslice-*",
        "game:bread-*-perfect",
        "game:cheese-cheddar-1slice",
        "game:redmeat-cooked"
};

    public static readonly Type[] CuttingBoardableTypes = new Type[]
    {
        typeof(ItemBreadSlice)
    };
}