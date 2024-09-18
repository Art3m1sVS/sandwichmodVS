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

[assembly: ModInfo(name: "Sammiches", modID: "sandwich")]


namespace sandwich
{

    public class sandwichModSystem : ModSystem
    {
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
            api.RegisterItemClass(Mod.Info.ModID, typeof(ItemSandwich));

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

        public override void AssetsLoaded(ICoreAPI api)
        {
            Transformations.CuttingBoardTransform = api.LoadAsset<Dictionary<string, ModelTransform>>("sandwich:config/transformations/cuttingboard.json");
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            foreach (CollectibleObject obj in api.World.Collectibles)
            {
                PatchCuttingBoardable(obj);
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

    }
}
