using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;

namespace sandwich
{
    public class ItemCuttingKnife : Item
    {
        ProPickWorkSpace ppws;
        SkillItem[] toolModes;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            ICoreClientAPI capi = api as ICoreClientAPI;

            toolModes = ObjectCacheUtil.GetOrCreate(api, "cuttingKnifeToolModes", () =>
            {
                SkillItem[] modes;
                    modes = new SkillItem[2];
                    modes[0] = new SkillItem() { Code = new AssetLocation("genericSlice"), Name = Lang.Get("Slice into sandwich bread slice") };
                    modes[1] = new SkillItem() { Code = new AssetLocation("burgerSlice"), Name = Lang.Get("Slice into burger buns") };
                    //modes[2] = new SkillItem() { Code = new AssetLocation("hotdogSlice"), Name = Lang.Get("Slice into hot dog buns") };


                if (capi != null)
                {
                    modes[0].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("sandwich:textures/item/icons/slice.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                    modes[0].TexturePremultipliedAlpha = false;

                    modes[1].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("sandwich:textures/item/icons/burger.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                    modes[1].TexturePremultipliedAlpha = false;

                    //modes[2].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("sandwich:textures/item/icons/hotdog.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                    //modes[2].TexturePremultipliedAlpha = false;
                }
                return modes;
            });

            if (api.Side == EnumAppSide.Server)
            {
                ppws = ObjectCacheUtil.GetOrCreate(api, "cuttingknifeworkspace", () =>
                {
                    ProPickWorkSpace ppws = new ProPickWorkSpace();
                    ppws.OnLoaded(api);
                    return ppws;
                });
            }
        }


        public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
        {
            return toolModes;
        }

        public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
        {
            return Math.Min(toolModes.Length - 1, slot.Itemstack.Attributes.GetInt("toolMode"));
        }

        public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel, int toolMode)
        {
            slot.Itemstack.Attributes.SetInt("toolMode", toolMode);
        }

        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            base.OnHeldIdle(slot, byEntity);
        }



        public override void OnUnloaded(ICoreAPI api)
        {
            base.OnUnloaded(api);
            if (api is ICoreServerAPI sapi)
            {
                sapi.ObjectCache.Remove("cuttingknifeworkspace");   // Unnecessary on registered items beyond the first, but does no harm
            }

            for (int i = 0; toolModes != null && i < toolModes.Length; i++)
            {
                toolModes[i]?.Dispose();
            }
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return new WorldInteraction[] {
                new WorldInteraction()
                {
                    ActionLangCode = "Change tool mode",
                    HotKeyCodes = new string[] { "toolmodeselect" },
                    MouseButton = EnumMouseButton.None
                }
            };

        }
    }
}