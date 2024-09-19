global using static sandwich.Constants;
global using Newtonsoft.Json.Linq;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using Vintagestory.API.Client;
global using Vintagestory.API.Common;
global using Vintagestory.API.Config;
global using Vintagestory.API.Datastructures;
global using Vintagestory.API.MathTools;
global using Vintagestory.API.Util;
global using Vintagestory.Client.NoObf;
global using Vintagestory.GameContent;
using sandwich.Items;
using ACulinaryArtillery;
using Vintagestory.API.Server;

[assembly: ModInfo(name: "Sammiches", modID: "sandwich")]


namespace sandwich
{

    public class sandwichModSystem : ModSystem
    {
        public Dictionary<string, WhenOnSandwichProperties> SandwichPatches { get; set; } = new();
        public Dictionary<string, CuttingBoardProperties> CuttingBoardPatches { get; set; } = new();
        public Dictionary<string, bool> CuttingBoardStorablePatches { get; set; } = new();
        public Transformations Transformations { get; } = new();

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass(Mod.Info.ModID + ".breadslice", typeof(ItemExpandedFood));
            api.RegisterItemClass(Mod.Info.ModID + ".peanutbuttersandwich", typeof(ItemPeanutButterHoneySandwich));
            api.RegisterItemClass(Mod.Info.ModID + ".honeybread", typeof(ItemHoneyBread));
            api.RegisterItemClass(Mod.Info.ModID + ".peanutbutterbread", typeof(ItemHoneyBread));
            api.RegisterItemClass(Mod.Info.ModID + ".cheesesandwich", typeof(ItemCheeseSandwich));
            api.RegisterItemClass(Mod.Info.ModID + ".grilledcheesesandwich", typeof(ItemGrilledCheeseSandwich));
            api.RegisterItemClass(Mod.Info.ModID + ".cheeseslice", typeof(ItemExpandedFood));
            api.RegisterItemClass(Mod.Info.ModID + ".cheesebreadslice", typeof(ItemCheeseBreadSlice));
            api.RegisterItemClass(Mod.Info.ModID + ".redmeatsandwich", typeof(ItemRedMeatSandwich));
            api.RegisterItemClass(Mod.Info.ModID + ".redmeatcheesesandwich", typeof(ItemRedMeatCheeseSandwich));
            api.RegisterItemClass(Mod.Info.ModID + ".redmeatbread", typeof(ItemRedMeatBread));
            api.RegisterItemClass(Mod.Info.ModID + ".redmeatslice", typeof(ItemRedMeatSlice));
            api.RegisterItemClass(Mod.Info.ModID + ".ItemSandwich", typeof(ItemSandwich));

            api.RegisterBlockBehaviorClass("sandwich.BbName", typeof(BlockBehaviorName));
            api.RegisterBlockClass(Mod.Info.ModID + ".cuttingboard", typeof(BlockCuttingBoard));
            api.RegisterBlockEntityClass("sandwich.blockentitycuttingboard", typeof(BlockEntityCuttingBoard));

        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            foreach (TransformConfig config in TransformConfigs)
            {
                if (!GuiDialogTransformEditor.extraTransforms.Contains(config))
                {
                    GuiDialogTransformEditor.extraTransforms.Add(config);
                }
            }
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
            Transformations.CuttingBoardTransform = api.LoadAsset<Dictionary<string, ModelTransform>>("sandwich:config/transformations/cuttingboard.json");
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            foreach (CollectibleObject obj in api.World.Collectibles)
            {
                PatchCuttingBoardable(obj);
            }

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
                    if (obj.CreativeInventoryTabs != null && obj.CreativeInventoryTabs.Any() && !obj.CreativeInventoryTabs.Contains(Constants.ModId))
                    {
                        obj.CreativeInventoryTabs = obj.CreativeInventoryTabs.Append(Constants.ModId);
                    }
                }
            }

            api.World.Logger.Event("started '{0}' mod", Mod.Info.Name);
        }

        private void PatchCuttingBoardable(CollectibleObject obj)
        {
            ModelTransform transform = obj.GetTransform(Transformations.CuttingBoardTransform);

            if (WildcardUtil.Match(CuttingBoardableCodes, obj.Code.ToString()) || CuttingBoardableTypes.Contains(obj.GetType()))
            {
                // obj.AddToCreativeInv(tab: ShelvableOne); // for testing

                obj.EnsureAttributesNotNull();
                obj.SetAttribute(CuttingBoardable, true);

                if (transform != null)
                {
                    obj.SetAttribute(CuttingBoardTransform, transform);
                }
            }
        }

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }
        public override void Dispose()
        {
            SandwichPatches?.Clear();
            CuttingBoardPatches?.Clear();
            CuttingBoardStorablePatches?.Clear();
        }
    }
}
