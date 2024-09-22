using ACulinaryArtillery;
using Newtonsoft.Json.Linq;
using sandwich;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

[assembly: ModInfo(name: "Sammiches", modID: "sandwich")]


namespace sandwich;

public class SandwichModSystem : ModSystem
{
    public Dictionary<string, WhenOnSandwichProperties> SandwichPatches { get; set; } = new();
    public Dictionary<string, CuttingBoardProperties> CuttingBoardPatches { get; set; } = new();
    public Dictionary<string, bool> CuttingBoardStorablePatches { get; set; } = new();

    public override void Start(ICoreAPI api)
    {
        api.RegisterItemClass("sandwich.ItemSandwich", typeof(ItemSandwich));
        api.RegisterBlockClass("sandwich.BlockCuttingBoard", typeof(BlockCuttingBoard));
        api.RegisterBlockEntityClass("sandwich.CuttingBoard", typeof(BlockEntityCuttingBoard));
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        if (!api.World.Config.HasAttribute(worldConfigSandwichLayersLimit))
        {
            api.World.Config.SetInt(worldConfigSandwichLayersLimit, defaultSandwichLayersLimit);
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        foreach (IAsset asset in api.Assets.GetMany("config/sandwich/sandwich_ingredients/"))
        {
            try
            {
                SandwichPatches.AddRange(asset.ToObject<Dictionary<string, WhenOnSandwichProperties>>());
            }
            catch (Exception e)
            {
                api.Logger.Error($"[Sandwich] Failed loading sandwich ingredients from file {asset.Location}:");
                api.Logger.Error(e);
            }
        }

        foreach (IAsset asset in api.Assets.GetMany("config/sandwich/cuttingboard_properties/"))
        {
            try
            {
                CuttingBoardPatches.AddRange(asset.ToObject<Dictionary<string, CuttingBoardProperties>>());
            }
            catch (Exception e)
            {
                api.Logger.Error($"[Sandwich] Failed loading cutting board patches from file {asset.Location}:");
                api.Logger.Error(e);
            }
        }

        foreach (IAsset asset in api.Assets.GetMany("config/sandwich/cuttingboard_storable/"))
        {
            try
            {
                CuttingBoardStorablePatches.AddRange(asset.ToObject<Dictionary<string, bool>>());
            }
            catch (Exception e)
            {
                api.Logger.Error($"[Sandwich] Failed loading 'storable on cutting board' patches from file {asset.Location}:");
                api.Logger.Error(e);
            }
        }
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        foreach (CollectibleObject obj in api.World.Collectibles)
        {
            if (obj == null || obj.Code == null)
            {
                continue;
            }

            foreach ((string code, WhenOnSandwichProperties props) in SandwichPatches)
            {
                if (obj.WildCardMatch(code) && !WhenOnSandwichProperties.HasAtribute(obj) && obj.HasNutrition())
                {
                    obj.EnsureAttributesNotNull();
                    WhenOnSandwichProperties.SetAtribute(obj, props);
                    break;
                }
            }

            foreach ((string code, CuttingBoardProperties props) in CuttingBoardPatches)
            {
                if (obj.WildCardMatch(code) && !CuttingBoardProperties.HasAtribute(obj))
                {
                    foreach ((string key, string value) in obj.Variant)
                    {
                        props.ConvertTo.FillPlaceHolder(key, value);
                    }

                    obj.EnsureAttributesNotNull();
                    CuttingBoardProperties.SetAtribute(obj, props);
                    break;
                }
            }

            foreach ((string code, bool storable) in CuttingBoardStorablePatches)
            {
                if (obj.WildCardMatch(code) && (obj.Attributes == null || !obj.Attributes.KeyExists(attributeCodeCuttingBoard)))
                {
                    obj.EnsureAttributesNotNull();
                    obj.Attributes.Token[attributeCodeCuttingBoard] = JToken.FromObject(storable);
                    break;
                }
            }

            if (WhenOnSandwichProperties.HasAtribute(obj) || obj?.Attributes?[attributeCodeCuttingBoard]?.AsBool() == true)
            {
                if (obj.CreativeInventoryTabs != null && obj.CreativeInventoryTabs.Any() && !obj.CreativeInventoryTabs.Contains(ModId))
                {
                    obj.CreativeInventoryTabs = obj.CreativeInventoryTabs.Append(ModId);
                }
            }
        }
    }

    public override void Dispose()
    {
        SandwichPatches?.Clear();
        CuttingBoardPatches?.Clear();
        CuttingBoardStorablePatches?.Clear();
    }
}