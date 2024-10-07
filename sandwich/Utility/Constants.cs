global using static sandwich.Constants;
using Vintagestory.API.Common;

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
    public const string attributeMW = $"{ModId}:madeWith";
    public const string attributeES = $"{ModId}:expandedSats";

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
}