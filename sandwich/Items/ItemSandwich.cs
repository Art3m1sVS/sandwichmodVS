﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sandwich;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using ACulinaryArtillery;
using EFRecipes;
using static Vintagestory.GameContent.BlockLiquidContainerBase;
using Vintagestory.API.Datastructures;
using System.Xml.Linq;
using System.Collections;
using Vintagestory.API.Common.Entities;
using Cairo;
using System.ComponentModel.Design.Serialization;

namespace sandwich;

public class ItemSandwich : ItemExpandedRawFood, IBakeableCallback, IContainedMeshSource
{
    public static bool TryAdd(ItemSlot slotSandwich, ItemSlot slotHand, IPlayer byPlayer, IWorldAccessor world)
    {
        bool isSandwich = slotSandwich?.Itemstack?.Collectible is ItemSandwich;
        var sandwichPropsInHand = SandwichProperties.FromStack(slotHand?.Itemstack, world);
        bool isSandwichInHand = slotHand?.Itemstack?.Collectible is ItemSandwich && sandwichPropsInHand != null && sandwichPropsInHand.Any;

        if (!isSandwich || isSandwichInHand)
        {
            return false;
        }

        if (slotHand?.Itemstack?.Collectible is ILiquidSource && TryAddLiquid(slotSandwich, slotLiquid: slotHand, byPlayer, world))
        {
            return true;
        }

        if (!WhenOnSandwichProperties.HasAtribute(slotHand?.Itemstack?.Collectible))
        {
            return false;
        }

        SandwichProperties propsInHand = SandwichProperties.FromStack(slotHand.Itemstack, world);
        if (propsInHand != null && propsInHand.Any)
        {
            return false;
        }

        SandwichProperties props = SandwichProperties.FromStack(slotSandwich.Itemstack, world);
        ItemStack stackIngredient = slotHand.Itemstack.Clone();
        stackIngredient.StackSize = 1;
        if (props == null || !props.TryAdd(stackIngredient, world))
        {
            return false;
        }
        props.ToStack(slotSandwich.Itemstack);
        slotHand.TakeOut(1);
        return true;
    }

    public new void OnBaked(ItemStack oldStack, ItemStack newStack)
    {
        string[] ings = (oldStack?.Attributes["madeWith"] as StringArrayAttribute)?.value;
        float[] sats = (oldStack?.Attributes["expandedSats"] as FloatArrayAttribute)?.value;

        if (ings != null) newStack.Attributes["madeWith"] = new StringArrayAttribute(ings);
        if (sats != null) newStack.Attributes["expandedSats"] = new FloatArrayAttribute(sats);

        SandwichProperties properties = SandwichProperties.FromStack(oldStack, api.World);

        if (properties != null)
        {
            ITreeAttribute treeSandwichLayers = oldStack.Attributes.GetOrAddTreeAttribute("sandwichLayers");
            ITreeAttribute newTreeSandwichLayers = new TreeAttribute();

            foreach (var entry in treeSandwichLayers)
            {
                string key = entry.Key;
                ItemStack layerStack = treeSandwichLayers.GetItemstack(key);

                if (layerStack != null)
                {
                    string[] layerIng = (layerStack?.Attributes["madeWith"] as StringArrayAttribute)?.value;
                    float[] layerSats = (layerStack?.Attributes["expandedSats"] as FloatArrayAttribute)?.value;

                    BakingProperties bakeProps = BakingProperties.ReadFrom(layerStack);
                    string resultCode = bakeProps?.ResultCode;

                    if (resultCode != null)
                    {
                        api.World.Logger.Event("Result code: " + resultCode);
                        Item item = api.World.GetItem(new AssetLocation(resultCode));

                        if (BakingStorage.IsBakeable(item.Code.Path))
                        {
                            ItemStack clonedLayerStack = new ItemStack(item);
                            if (layerIng != null) clonedLayerStack.Attributes["madeWith"] = new StringArrayAttribute(layerIng);
                            if (layerSats != null) clonedLayerStack.Attributes["expandedSats"] = new FloatArrayAttribute(layerSats);

                            newTreeSandwichLayers.SetItemstack(key, clonedLayerStack);

                            string itemName = clonedLayerStack.GetName();
                            string itemAttributes = clonedLayerStack.Attributes.ToString();

                            //api.World.Logger.Event("Cloned item name: " + itemName);
                           // api.World.Logger.Event("Cloned item attributes: " + itemAttributes);
                        }
                        else
                        {
                            newTreeSandwichLayers.SetItemstack(key, layerStack);
                            api.World.Logger.Event("Item is not bakeable, keeping original: " + item.Code.Path);
                        }
                    }
                    else
                    {
                        newTreeSandwichLayers.SetItemstack(key, layerStack);
                        api.World.Logger.Event("No result code, keeping original layer stack.");
                    }
                }
            }
            newStack.Attributes["sandwichLayers"] = newTreeSandwichLayers;
        }
    }


    private static bool TryAddLiquid(ItemSlot slotSandwich, ItemSlot slotLiquid, IPlayer byPlayer, IWorldAccessor world)
    {
        BlockLiquidContainerBase liquidContainer = slotLiquid.Itemstack.Collectible as BlockLiquidContainerBase;
        ILiquidSource liquidSource = slotLiquid.Itemstack.Collectible as ILiquidSource;

        if (liquidSource == null && slotLiquid.Itemstack.Collectible.MatterState != EnumMatterState.Liquid)
        {
            return false;
        }

        if (!liquidSource.AllowHeldLiquidTransfer)
        {
            return false;
        }

        ItemStack contentStackToMove = liquidSource.GetContent(slotLiquid.Itemstack);
        WhenOnSandwichProperties whenOnSandwichProps = WhenOnSandwichProperties.GetProps(contentStackToMove?.Collectible);
        if (whenOnSandwichProps == null)
        {
            return false;
        }
        SandwichProperties props = SandwichProperties.FromStack(slotSandwich.Itemstack, world);
        if (!props.CanAdd(contentStackToMove, world))
        {
            return false;
        }
        WaterTightContainableProps contentProps = liquidSource.GetContentProps(slotLiquid.Itemstack);
        int moved = (int)(whenOnSandwichProps.LitresPerLayer * contentProps.ItemsPerLitre);
        if (liquidSource.GetCurrentLitres(slotLiquid.Itemstack) < whenOnSandwichProps.LitresPerLayer || moved <= 0)
        {
            return false;
        }
        if (liquidContainer.GetMethod("splitStackAndPerformAction") != null) // 1.19.8
        {
            liquidContainer.CallMethod<int>("splitStackAndPerformAction", byPlayer.Entity, slotLiquid, delegate (ItemStack stack)
            {
                liquidContainer.TryTakeContent(stack, moved);
                return moved;
            });
        } else if (liquidContainer.GetMethod("SplitStackAndPerformAction") != null) // 1.20
        {
            liquidContainer.CallMethod<int>("SplitStackAndPerformAction", byPlayer.Entity, slotLiquid, delegate (ItemStack stack)
            {
                liquidContainer.TryTakeContent(stack, moved);
                return moved;
            });
        }

        ItemStack stackIngredient = contentStackToMove.Clone();
        stackIngredient.StackSize = moved;
        if (props == null || !props.TryAdd(stackIngredient, world))
        {
            return false;
        }
        props.ToStack(slotSandwich.Itemstack);
        liquidContainer.DoLiquidMovedEffects(byPlayer, contentStackToMove, moved, EnumLiquidDirection.Pour);
        return true;
    }

    public override void OnBeforeRender(ICoreClientAPI capi, ItemStack stack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
    {
        base.OnBeforeRender(capi, stack, target, ref renderinfo);

        string key = GetMeshCacheKey(stack);
        Dictionary<string, MultiTextureMeshRef> InvMeshes = ObjectCacheUtil.GetOrCreate(capi, "sandwich:sandwich-invmeshes", () => new Dictionary<string, MultiTextureMeshRef>());
        if (!InvMeshes.TryGetValue(key, out MultiTextureMeshRef meshref))
        {
            MeshData mesh = GenMesh(stack, capi.ItemTextureAtlas, null);
            meshref = InvMeshes[key] = capi.Render.UploadMultiTextureMesh(mesh);
        }
        renderinfo.ModelRef = meshref;
    }

    public override MeshData GenMesh(ItemStack stack, ITextureAtlasAPI targetAtlas, BlockPos atBlockPos)
    {
        MeshData mesh = new MeshData(4, 3);

        float prevSize = 0;
        float rotation = 0;

        List<ItemStack> stacks = new() { stack };

        SandwichProperties properties = SandwichProperties.FromStack(stack, api.World);
        IEnumerable<ItemStack> ordered = properties?.GetOrdered(api.World);
        if (ordered != null)
        {
            stacks.AddRange(ordered);
        }

        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack ingredientStack = stacks[i];
            bool last = i == stacks.Count - 1 && stacks.Count != 1;
            MeshData ingredientMesh = GenIngredientMesh(api as ICoreClientAPI, ref prevSize, ref rotation, ingredientStack, last);
            mesh.AddMeshData(ingredientMesh);
        }
        return mesh;
    }

    private static MeshData GenIngredientMesh(ICoreClientAPI capi, ref float prevSize, ref float rotation, ItemStack stack, bool last = false)
    {
        MeshData mesh = new MeshData(4, 3);

        WhenOnSandwichProperties props = WhenOnSandwichProperties.GetProps(stack?.Collectible);
        if (props == null)
        {
            switch (stack?.Class)
            {
                case EnumItemClass.Block:
                    capi.Tesselator.TesselateBlock(stack.Block, out mesh);
                    if (stack?.Collectible?.Code?.Domain == "expandedfoods" && stack?.Collectible.MatterState is not EnumMatterState.Liquid)
                    {
                        // Scale down the mesh
                        mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.5f, 0.5f, 0.5f);
                        prevSize += 0.0225f;
                    }
                    else
                    {
                        prevSize += 0.0225f;
                    }
                    return mesh;

                case EnumItemClass.Item:
                    capi.Tesselator.TesselateItem(stack.Item, out mesh);
                    if (stack?.Collectible?.Code?.Domain == "expandedfoods" && stack?.Collectible.MatterState is not EnumMatterState.Liquid)
                    {
                        // Scale down the mesh
                        mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.5f, 0.5f, 0.5f);
                        prevSize += 0.0225f;
                    }
                    else
                    {
                        prevSize += 0.0225f;
                    }
                    return mesh;

                default:
                    prevSize += 0.0225f;
                    return mesh;
            }
        }

        CompositeShape rcshape = props.Shape.Clone();
        if (last && props.ShapeLast != null)
        {
            rcshape = props.ShapeLast.Clone();
        }

        rcshape.Base.WithPathAppendixOnce(".json").WithPathPrefixOnce("shapes/");
        Shape shape = capi.Assets.TryGet(rcshape.Base)?.ToObject<Shape>();
        if (shape == null)
        {
            prevSize += props.Size;
            return mesh;
        }

        ShapeTextureSource texSource = new ShapeTextureSource(capi, shape, shape.ToString());

        Dictionary<string, CompositeTexture> textures;
        if (props.Textures == null || !props.Textures.Any())
        {
            textures = stack.GetTextures();
        }
        else
        {
            textures = props.Textures;
        }

        foreach (KeyValuePair<string, CompositeTexture> val in textures)
        {
            CompositeTexture ctex = val.Value.Clone();
            foreach ((string key, string value) in stack.Collectible.Variant)
            {
                ctex.FillPlaceholder($"{{{key}}}", value);
            }

            ctex.Bake(capi.Assets);
            texSource.textures[val.Key] = ctex;
        }

        capi.Tesselator.TesselateShape("Sandwich item", shape, out mesh, texSource);

        if (props.Rotate)
        {
            int seed = stack.Id + (stack.StackSize * 17);  // could include more entropy
            Random rnd = new Random(seed);
            float rotY = (float)(rnd.NextDouble() * 1.1); // FIX ROTATION

            rotation = props.CopyLastRotation ? rotation : rotY;
            mesh = mesh.Translate(-0.5f, -0.5f, -0.5f);
            mesh = mesh.Rotate(Vec3f.Zero, 0, rotation, 0);
            mesh = mesh.Translate(0.5f, 0.5f, 0.5f);
        }

        // Adjust scaling based on the domain for visual mesh size
        if (stack?.Collectible?.Code?.Domain == "expandedfoods" && stack?.Collectible.MatterState is not EnumMatterState.Liquid)
        {
            mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.5f, 0.5f, 0.5f);
            mesh = mesh.Translate(0, prevSize, 0);
            prevSize += props.Size;
        }
        else
        {
            mesh = mesh.Translate(0, prevSize, 0);
            prevSize += props.Size;
        }
        return mesh;
    }




    public new string GetMeshCacheKey(ItemStack stack)
    {
        SandwichProperties props = SandwichProperties.FromStack(stack, (api as ICoreClientAPI).World);
        if (props == null || !props.Any)
        {
            return Code.ToString();
        }
        return Code.ToString() + "-" + props.ToString();
    }

    public override string GetHeldItemName(ItemStack stack)
    {
        SandwichProperties props = SandwichProperties.FromStack(stack, api.World);
        if (props == null || !props.Any)
        {
            return base.GetHeldItemName(stack);
        }
        return Lang.Get(langSandwich);
    }


    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        SandwichProperties props = SandwichProperties.FromStack(inSlot.Itemstack, world);
        if (props == null || !props.Any)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
            return;
        }

        if (withDebugInfo)
        {
            dsc.AppendLine("<font color=\"#bbbbbb\">Id:" + Id + "</font>");
            dsc.AppendLine("<font color=\"#bbbbbb\">Code: " + Code?.ToString() + "</font>");
            ICoreAPI coreAPI = api;
            if (coreAPI != null && coreAPI.Side == EnumAppSide.Client && (api as ICoreClientAPI).Input.KeyboardKeyStateRaw[1])
            {
                dsc.AppendLine("<font color=\"#bbbbbb\">Attributes: " + inSlot.Itemstack.Attributes.ToJsonToken() + "</font>\n");
            }
        }

        props.GetDescription(inSlot, dsc, world, noLimit: true);

        EntityPlayer entityPlayer = (world.Side == EnumAppSide.Client) ? (world as IClientWorldAccessor).Player.Entity : null;
        SandwichNutritionProperties nutritionProps = props.GetNutritionProperties(inSlot, world, entityPlayer);
        FoodNutritionProperties[] addProps = Attributes?["additionalNutritionProperties"]?.AsObject<FoodNutritionProperties[]>();
        FoodNutritionProperties[] addPropsEF = GetPropsFromArray((inSlot.Itemstack.Attributes["expandedSats"] as FloatArrayAttribute)?.value);

        float spoilState = AppendPerishableInfoText(inSlot, dsc, world);
        Dictionary<string, (float TotalSat, float TotalHealth)> categorySummary = new Dictionary<string, (float, float)>();
        float satLossMul = GlobalConstants.FoodSpoilageSatLossMul(spoilState, inSlot.Itemstack, entityPlayer);
        float healthLossMul = GlobalConstants.FoodSpoilageHealthLossMul(spoilState, inSlot.Itemstack, entityPlayer);

        if (nutritionProps != null)
        {
            // Combine all properties into a single loop for processing
            List<FoodNutritionProperties> allProps = new List<FoodNutritionProperties>(nutritionProps.NutritionPropertiesMany);

            if (addPropsEF != null && addPropsEF.Length > 0)
            {
               // api.World.Logger.Event("addprops not null");
                allProps.AddRange(addPropsEF);
            }
            //api.World.Logger.Event("addprops are null");

            foreach (FoodNutritionProperties property in allProps)
            {
                float totalSat = property.Satiety * satLossMul;
                float totalHealth = property.Health * healthLossMul;
                totalSat = (float)Math.Round(totalSat);

                string category = property.FoodCategory.ToString();

                if (categorySummary.ContainsKey(category))
                {
                    categorySummary[category] = (
                        categorySummary[category].TotalSat + totalSat,
                        categorySummary[category].TotalHealth + totalHealth
                    );
                }
                else
                {
                    categorySummary[category] = (totalSat, totalHealth);
                }
            }

            // Append the "When Eaten" section with total satiety and health values
            dsc.AppendLine(Lang.Get(langWhenEaten));
            foreach (KeyValuePair<string, (float TotalSat, float TotalHealth)> entry in categorySummary)
            {
                string category = entry.Key;
                float totalSat = entry.Value.TotalSat;
                float totalHealth = entry.Value.TotalHealth;
                string translatedCategory = Lang.Get("foodcategory-" + category.ToLowerInvariant());

                if (Math.Abs(totalHealth) > 0.001f)
                {
                    dsc.AppendLine("- " + Lang.Get("{0}, {1} sat, {2} hp",
                        translatedCategory,
                        totalSat,
                        totalHealth));
                }
                else
                {
                    dsc.AppendLine("- " + Lang.Get("{0}, {1} sat",
                        translatedCategory,
                        totalSat));
                }
            }
        }

        // Get sandwich layers and show expandedSats for each layer in "When Eaten"
        if (inSlot.Itemstack.Attributes?.HasAttribute("sandwichLayers") == true)
        {
            var layersAttribute = inSlot.Itemstack.Attributes["sandwichLayers"] as TreeAttribute;
            if (layersAttribute != null)
            {
                foreach (var layer in layersAttribute)
                {
                    ItemStack layerStack = layer.Value as ItemStack;
                    if (layerStack != null)
                    {
                        // Get the expandedSats attribute for this layer
                        var expandedSats = layerStack.Attributes?["expandedSats"] as FloatArrayAttribute;
                        if (expandedSats != null)
                        {
                            foreach (float satValue in expandedSats.value)
                            {
                                //dsc.AppendLine("- " + Lang.Get("Layer with {0} expanded sat", satValue));
                            }
                        }
                        else
                        {
                            //dsc.AppendLine("- " + Lang.Get("Layer with no expandedSats"));
                        }
                    }
                }
            }
        }

        // Check for and print out the "madeWith" attribute
        //if (inSlot.Itemstack.Attributes.HasAttribute("madeWith"))
        //{
        //    var madeWithList = inSlot.Itemstack.Attributes["madeWith"] as StringArrayAttribute;
        //    if (madeWithList != null && madeWithList.value.Length > 0)
        //    {
        //        dsc.AppendLine("Made with:");
        //        foreach (string item in madeWithList.value)
        //        {
        //            dsc.AppendLine($"- {item}");
        //        }
        //    }
        //}

        CollectibleBehavior[] collectibleBehaviors = CollectibleBehaviors;
        for (int i = 0; i < collectibleBehaviors.Length; i++)
        {
            collectibleBehaviors[i].GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        }

        if (dsc.Length > 0)
        {
            dsc.Append("\n");
        }
        dsc.AppendLine(Lang.Get("You are marching into the unknown, armed with... nothing. Have a sandwich."));

        float temperature = GetTemperature(world, inSlot.Itemstack);
        if (temperature > 20f)
        {
            dsc.AppendLine(Lang.Get("Temperature: {0}°C", (int)temperature));
        }

        if (Code != null && Code.Domain != "game")
        {
            Mod mod = api.ModLoader.GetMod(Code.Domain);
            dsc.AppendLine(Lang.Get("Mod: {0}", mod?.Info.Name ?? Code.Domain));
        }
    }



    protected override void tryEatBegin(ItemSlot slot, EntityAgent byEntity, ref EnumHandHandling handling, string eatSound = "eat", int eatSoundRepeats = 1)
    {
        SandwichProperties props = SandwichProperties.FromStack(slot?.Itemstack, byEntity.World);
        if (props == null || !props.Any || props.GetNutritionProperties(slot, byEntity.World, byEntity) == null)
        {
            base.tryEatBegin(slot, byEntity, ref handling, eatSound, eatSoundRepeats);
            return;
        }

        byEntity.World.RegisterCallback(delegate
        {
            playEatSound(byEntity, eatSound, eatSoundRepeats);
        }, 500);
        byEntity.AnimManager?.StartAnimation("eat");
        handling = EnumHandHandling.PreventDefault;
    }

    protected override bool tryEatStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, ItemStack spawnParticleStack = null)
    {
        SandwichProperties props = SandwichProperties.FromStack(slot?.Itemstack, byEntity.World);
        if (props == null || !props.Any || props.GetNutritionProperties(slot, byEntity.World, byEntity) == null)
        {
            return base.tryEatStep(secondsUsed, slot, byEntity, spawnParticleStack);
        }

        Vec3d xYZ = byEntity.Pos.AheadCopy(0.40000000596046448).XYZ;
        xYZ.X += byEntity.LocalEyePos.X;
        xYZ.Y += byEntity.LocalEyePos.Y - 0.40000000596046448;
        xYZ.Z += byEntity.LocalEyePos.Z;
        if (secondsUsed > 0.5f && (int)(30f * secondsUsed) % 7 == 1)
        {
            byEntity.World.SpawnCubeParticles(xYZ, spawnParticleStack ?? slot.Itemstack, 0.3f, 4, 0.5f, (byEntity as EntityPlayer)?.Player);
        }

        if (byEntity.World is IClientWorldAccessor)
        {
            ModelTransform modelTransform = new ModelTransform();
            modelTransform.EnsureDefaultValues();
            modelTransform.Origin.Set(0f, 0f, 0f);
            if (secondsUsed > 0.5f)
            {
                modelTransform.Translation.Y = Math.Min(0.02f, GameMath.Sin(20f * secondsUsed) / 10f);
            }

            modelTransform.Translation.X -= Math.Min(1f, secondsUsed * 4f * 1.57f);
            modelTransform.Translation.Y -= Math.Min(0.05f, secondsUsed * 2f);
            modelTransform.Rotation.X += Math.Min(30f, secondsUsed * 350f);
            modelTransform.Rotation.Y += Math.Min(80f, secondsUsed * 350f);
            //byEntity.Controls.UsingHeldItemTransformAfter = modelTransform;
            return secondsUsed <= 1f;
        }

        return true;
    }

    protected override void tryEatStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity)
    {
        SandwichProperties props = SandwichProperties.FromStack(slot?.Itemstack, byEntity.World);
        SandwichNutritionProperties nutritionProperties = props.GetNutritionProperties(slot, byEntity.World, byEntity);

        if (props == null || !props.Any || nutritionProperties == null)
        {
            base.tryEatStop(secondsUsed, slot, byEntity);
            return;
        }

        if (byEntity.World is not IServerWorldAccessor || !(secondsUsed >= 0.95f))
        {
            return;
        }

        float spoilState = UpdateAndGetTransitionState(api.World, slot, EnumTransitionType.Perish)?.TransitionLevel ?? 0f;
        float satLossMul = GlobalConstants.FoodSpoilageSatLossMul(spoilState, slot.Itemstack, byEntity);
        float healthLossMul = GlobalConstants.FoodSpoilageHealthLossMul(spoilState, slot.Itemstack, byEntity);

        bool any = false;
        foreach (FoodNutritionProperties property in nutritionProperties.NutritionPropertiesMany)
        {
            any = true;
            byEntity.ReceiveSaturation(property.Satiety * satLossMul, property.FoodCategory);

            // Health and intoxication adjustments
            float health = property.Health * healthLossMul;
            float intoxication = byEntity.WatchedAttributes.GetFloat("intoxication");
            byEntity.WatchedAttributes.SetFloat("intoxication", Math.Min(1.1f, intoxication + property.Intoxication));

            if (health != 0f)
            {
                byEntity.ReceiveDamage(new DamageSource
                {
                    Source = EnumDamageSource.Internal,
                    Type = (health > 0f) ? EnumDamageType.Heal : EnumDamageType.Poison
                }, Math.Abs(health));
            }
        }

        // Accessing the expandedSats from layers in the sandwich (if they exist)
        if (slot?.Itemstack != null)
        {
            var itemAttributes = slot.Itemstack.Attributes;

            // Check if expandedSats exists for the itemstack
            if (itemAttributes != null && itemAttributes["expandedSats"] != null)
            {
                var expandedSats = itemAttributes["expandedSats"] as FloatArrayAttribute;

                if (expandedSats != null)
                {
                    // Accessing the expandedSats array
                    foreach (float satValue in expandedSats.value)
                    {
                        // Log or process each expandedSat value (saturation level)
                       // byEntity.World.Logger.Event($"Expanded Sat value: {satValue}");

                        // Find the food category by looking for matching nutrition properties
                        FoodNutritionProperties matchingProperty = null;
                        foreach (var property in nutritionProperties.NutritionPropertiesMany)
                        {
                            if (property.Satiety == satValue) // or any other condition that helps match
                            {
                                matchingProperty = property;
                                break;
                            }
                        }

                        if (matchingProperty != null)
                        {
                            // Apply this saturation value to the entity using the actual food category from the matching property
                            byEntity.ReceiveSaturation(satValue * satLossMul, matchingProperty.FoodCategory);
                        }
                        else
                        {
                            // Default to vegetable if no match is found (if needed)
                            byEntity.ReceiveSaturation(satValue * satLossMul, EnumFoodCategory.NoNutrition);
                        }
                    }
                }
                else
                {
                    //byEntity.World.Logger.Notification("expandedSats attribute is not in the expected format.");
                }
            }
            else
            {
                //byEntity.World.Logger.Notification("No 'expandedSats' attribute found in Itemstack.");
            }
        }

        if (any)
        {
            IPlayer player = null;
            if (byEntity is EntityPlayer)
            {
                player = byEntity.World.PlayerByUid(((EntityPlayer)byEntity).PlayerUID);
            }
            slot.TakeOut(1);
            slot.MarkDirty();
            player.InventoryManager.BroadcastHotbarSlot();
        }
    }



    public override bool Satisfies(ItemStack thisStack, ItemStack otherStack)
    {
        if (thisStack.Class == otherStack.Class && thisStack.Id == otherStack.Id)
        {
            if (!otherStack.Attributes.HasAttribute("madeWith") && thisStack.Attributes.HasAttribute("madeWith"))
            {
                return true;
            }
        }
        return base.Satisfies(thisStack, otherStack);
    }
    public void OnCreatedBySlicing(ItemSlot inputSlot, ItemStack outputSlot, ItemStack byRecipe, int slices)
    {
        //base.OnCreatedBySlicing(allInputslots, outputSlot, byRecipe);
        ItemStack output = outputSlot;
        List<string> ingredients = new List<string>();
        float[] sat = new float[6];
        ItemSlot slot = inputSlot;
        if (slot.Itemstack.Collectible is ItemExpandedFood)
        {
            //api.World.Logger.Event("ITEM IS EXPANDEDFOOD");
            string[] addIngs = (slot.Itemstack.Attributes["madeWith"] as StringArrayAttribute)?.value;
            float[] addSat = (slot.Itemstack.Attributes["expandedSats"] as FloatArrayAttribute)?.value;
            if (addSat != null && addSat.Length == 6)
                sat = sat.Zip(addSat, (x, y) => x + (y / slices)).ToArray();
            if (addIngs != null && addIngs.Length > 0)
            {
                foreach (string aL in addIngs)
                {
                    if (ingredients.Contains(aL))
                        continue;
                    ingredients.Add(aL);
                }
            }
        }
        else
        {
            //api.World.Logger.Event("ITEM IS NOT EXPANDEDFOOD");
            GetNutrientsFromIngredient(ref sat, slot.Itemstack.Collectible, 1);
            sat = sat.Select(nutrient => nutrient / slices).ToArray();
            string aL = slot.Itemstack.Collectible.Code.Domain + ":" + slot.Itemstack.Collectible.Code.Path;
            ingredients.Add(aL);
        }
        ingredients.Sort();
        output.Attributes["madeWith"] = new StringArrayAttribute(ingredients.ToArray());
        output.Attributes["expandedSats"] = new FloatArrayAttribute(sat.ToArray());
    }

    public override ItemStack OnTransitionNow(ItemSlot slot, TransitionableProperties transitionProps)
    {
        string[] ings = (slot.Itemstack.Attributes["madeWith"] as StringArrayAttribute)?.value;
        float[] xNutr = (slot.Itemstack.Attributes["expandedSats"] as FloatArrayAttribute)?.value;

        ItemStack org = base.OnTransitionNow(slot, transitionProps);
        SandwichProperties props = SandwichProperties.FromStack(slot.Itemstack, api.World);
        if (props == null || !props.Any || !(org.Collectible is ItemExpandedFood))
            return org;
        if (ings != null)
            org.Attributes["madeWith"] = new StringArrayAttribute(ings);
        if (xNutr != null && xNutr.Length > 0)
            org.Attributes["expandedSats"] = new FloatArrayAttribute(xNutr);

        ItemStack stack = transitionProps.TransitionedStack.ResolvedItemstack.Clone();
        stack.StackSize = GameMath.RoundRandom(api.World.Rand, slot.StackSize * props.GetOrdered(api.World).Sum(x => x.StackSize) * transitionProps.TransitionRatio);
        return stack;
    }

    public override TransitionState UpdateAndGetTransitionState(IWorldAccessor world, ItemSlot inslot, EnumTransitionType type)
    {
        ItemStack[] contents = SandwichProperties.FromStack(inslot.Itemstack, world)?.GetOrdered(world)?.ToArray();
        if (contents != null)
        {
            UnspoilContents(world, contents);
        }
        return base.UpdateAndGetTransitionState(world, inslot, type);
    }

    protected void UnspoilContents(IWorldAccessor world, ItemStack[] cstacks)
    {
        // Dont spoil the pie contents, the pie itself has a spoilage timer. Semi hacky fix reset their spoil timers each update

        for (int i = 0; i < cstacks.Length; i++)
        {
            ItemStack cstack = cstacks[i];
            if (cstack == null) continue;

            if (!(cstack.Attributes["transitionstate"] is ITreeAttribute))
            {
                cstack.Attributes["transitionstate"] = new TreeAttribute();
            }
            ITreeAttribute attr = (ITreeAttribute)cstack.Attributes["transitionstate"];

            if (attr.HasAttribute("createdTotalHours"))
            {
                attr.SetDouble("createdTotalHours", world.Calendar.TotalHours);
                attr.SetDouble("lastUpdatedTotalHours", world.Calendar.TotalHours);
                var transitionedHours = (attr["transitionedHours"] as FloatArrayAttribute)?.value;
                for (int j = 0; transitionedHours != null && j < transitionedHours.Length; j++)
                {
                    transitionedHours[j] = 0;
                }
            }
        }
    }

    public new void GetNutrientsFromIngredient(ref float[] satHolder, CollectibleObject ing, int mult)
    {
        TreeAttribute check = Attributes?["expandedNutritionProps"].ToAttribute() as TreeAttribute;
        List<string> chk = new List<string>();
        if (check != null)
            foreach (var val in check)
                chk.Add(val.Key);

        FoodNutritionProperties ingProps = null;
        if (chk.Count > 0)
            ingProps = Attributes["expandedNutritionProps"][FindMatch(ing.Code.Domain + ":" + ing.Code.Path, chk.ToArray())].AsObject<FoodNutritionProperties>();
        if (ingProps == null)
            ingProps = ing.Attributes?["nutritionPropsWhenInMeal"].AsObject<FoodNutritionProperties>();
        if (ingProps == null)
            ingProps = ing.NutritionProps;
        if (ingProps == null)
            return;

        if (ingProps.Health != 0)
            satHolder[(int)EnumNutritionMatch.Hp] += ingProps.Health * mult;

        switch (ingProps.FoodCategory)
        {
            case EnumFoodCategory.Fruit:
                satHolder[(int)EnumNutritionMatch.Fruit] += ingProps.Satiety * mult;
                break;

            case EnumFoodCategory.Grain:
                satHolder[(int)EnumNutritionMatch.Grain] += ingProps.Satiety * mult;
                break;

            case EnumFoodCategory.Vegetable:
                satHolder[(int)EnumNutritionMatch.Vegetable] += ingProps.Satiety * mult;
                break;

            case EnumFoodCategory.Protein:
                satHolder[(int)EnumNutritionMatch.Protein] += ingProps.Satiety * mult;
                break;

            case EnumFoodCategory.Dairy:
                satHolder[(int)EnumNutritionMatch.Dairy] += ingProps.Satiety * mult;
                break;
        }
    }

    public static new void ListIngredients(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)

    {
        string desc = Lang.Get("sandwich:Made with ");
        string[] ings = (inSlot.Itemstack.Attributes["madeWith"] as StringArrayAttribute)?.value;

        if (ings == null || ings.Length < 1)
        {
            return;
        }


        List<string> readable = new List<string>();
        for (int i = 0; i < ings.Length; i++)
        {
            AssetLocation obj = new AssetLocation(ings[i]);
            Block block = world.GetBlock(obj);
            string ingInfo = Lang.GetIfExists("recipeingredient-" + (block != null ? "block-" : "item-") + obj.Path);
            if (ingInfo != null && !readable.Contains(ingInfo))
                readable.Add(ingInfo);
        }

        ings = readable.ToArray();

        if (ings == null || ings.Length < 1)
        {
            return;
        }


        if (ings.Length < 2)
        {
            desc += ings[0];

            dsc.AppendLine(desc);
            return;
        }

        for (int i = 0; i < ings.Length; i++)
        {
            AssetLocation obj = new AssetLocation(ings[i]);
            Block block = world.GetBlock(obj);

            if (i + 1 == ings.Length)
            {
                desc += Lang.Get("sandwich:and ") + ings[i];
            }
            else
            {
                desc += ings[i] + ", ";
            }
        }

        dsc.AppendLine(desc);
    }
}

//public override ItemStack OnTransitionNow(ItemSlot slot, TransitionableProperties props)
//{
//    string[] ings = (slot.Itemstack.Attributes["madeWith"] as StringArrayAttribute)?.value;
//    float[] xNutr = (slot.Itemstack.Attributes["expandedSats"] as FloatArrayAttribute)?.value;

//    ItemStack org = base.OnTransitionNow(slot, props);
//    if (org == null || !(org.Collectible is ItemExpandedRawFood))
//        return org;
//    if (ings != null)
//        org.Attributes["madeWith"] = new StringArrayAttribute(ings);
//    if (xNutr != null && xNutr.Length > 0)
//        org.Attributes["expandedSats"] = new FloatArrayAttribute(xNutr);
//    return org;
//}