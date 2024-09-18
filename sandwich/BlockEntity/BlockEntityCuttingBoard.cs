using System.ComponentModel;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using static HarmonyLib.Code;
using ACulinaryArtillery;
using static Vintagestory.GameContent.BlockLiquidContainerBase;
using sandwich.Items;
using System.Collections.Generic;
namespace sandwich;

public class BlockEntityCuttingBoard : BlockEntityDisplay
{
    private readonly InventoryGeneric inventory;
    private const int slotCount = 1;

    public override InventoryBase Inventory => inventory;
    public override string InventoryClassName => Block?.Attributes?["inventoryClassName"].AsString();
    public override string AttributeTransformCode => Block?.Attributes?["attributeTransformCode"].AsString();

    public BlockEntityCuttingBoard()
    {
        inventory = new InventoryGeneric(slotCount, "cuttingboard-0", Api, (_, inv) => new ItemSlotCuttingBoard(inv));
    }

    public bool CheckForSandwichCreation(ItemSlot playerSlot, ItemSlot cuttingBoardSlot)
    {
        // Ensure the player is holding an item and the cutting board has an item
        if (playerSlot.Itemstack != null && cuttingBoardSlot.Itemstack != null)
        {
            string heldItemType = playerSlot.Itemstack.Collectible.Code.Path;
            string boardItemType = cuttingBoardSlot.Itemstack.Collectible.Code.Path;
            //ItemSandwich sandwichItem = new();
            //sandwichItem.OnCreatedBySlicing(inventory[0], playerSlot.Itemstack, cuttingBoardSlot.Itemstack);

            string[] heldParts = heldItemType.Contains("-") ? heldItemType.Split('-') : new string[] { heldItemType };
            string[] boardParts = boardItemType.Contains("-") ? boardItemType.Split('-') : new string[] { boardItemType };

            // Extract base types and variants (if applicable)
            string heldItemBaseType = heldParts[0];
            string boardItemBaseType = boardParts[0];
            string boardVariant = boardParts.Length > 1 ? boardParts[1] : "";

            // Loop through all sandwich types in the storage file to find a match
            foreach (var sandwichType in SandwichStorage.SandwichTypes)
            {
                var sandwich = sandwichType.Value;

                Api.World.Logger.Event($"Checking sandwich type: {sandwichType.Key}");
                Api.World.Logger.Event($"Required slices: {string.Join(", ", sandwich.RequiredSlices)}");

                // Check if the held item matches any required slices for this sandwich type
                bool heldMatches = sandwich.RequiredSlices.Any(slice =>
                {
                    var parts = slice.Contains("-") ? slice.Split('-') : new string[] { slice };
                    Api.World.Logger.Event($"Checking slice: {slice}, Held Item Type: {heldItemBaseType}");
                    return parts.Length > 0 && parts[0] == heldItemBaseType;
                });

                // Check if the board item matches any required slices for this sandwich type
                bool boardMatches = sandwich.RequiredSlices.Any(slice =>
                {
                    var parts = slice.Contains("-") ? slice.Split('-') : new string[] { slice };
                    Api.World.Logger.Event($"Checking slice: {slice}, Board Item Type: {boardItemBaseType}");
                    return parts.Length > 0 && parts[0] == boardItemBaseType;
                });

                Api.World.Logger.Event($"Held Matches: {heldMatches}, Board Matches: {boardMatches}");

                if (heldMatches && boardMatches)
                {
                    // Use the board item's variant (if available) for sandwich creation
                    string priorityVariant = !string.IsNullOrEmpty(boardVariant) ? boardVariant : "default";
                    Api.World.Logger.Event($"Priority Variant: {priorityVariant}");

                    // Create the sandwich asset location using the matched sandwich type and variant
                    AssetLocation sandwichAsset = new AssetLocation($"sandwich:{sandwichType.Key}-{priorityVariant}");
                    // Api.World.Logger.Event($"Attempting to create sandwich with asset location: {sandwichAsset}");

                    if (Api.World.GetItem(sandwichAsset) != null)
                    {
                        Api.World.Logger.Event($"Sandwich created successfully: {sandwichAsset}");

                        if (Api.Side == EnumAppSide.Server)
                        {
                            var sandwichStack = new ItemStack(Api.World.GetItem(sandwichAsset), 1);

                            inventory[0].Itemstack = sandwichStack;
                            playerSlot.TakeOut(1);
                            MarkDirty(redrawOnClient: true);
                            updateMeshes();
                        }

                        return true;
                    }
                    else
                    {
                        Api.World.Logger.Event($"Failed to create sandwich stack - sandwich asset not found: {sandwichAsset}");
                    }
                }
            }
        }

        Api.World.Logger.Event("No valid sandwich found for the held and board items.");
        return false;
    }







    internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
    {
        ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

        // If the player's hand is empty, attempt to take an item from the board
        if (slot.Empty)
        {
            return TryTake(byPlayer, blockSel);
        }

        // Check if the item in the player's hand is something that can be placed on the cutting board
        if (slot.IsCuttingBoardable())
        {
            AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;
            if (TryPut(slot, blockSel))
            {
                Api.World.PlaySoundAt(sound ?? DefaultPlaceSound, byPlayer.Entity, byPlayer, randomizePitch: true, 16f);
                updateMeshes();
                Api.World.Logger.Event("Item placed on board: " + slot + " (" + slot.Itemstack + ")");
                return true;
            }
        }

        EnumTool? tool = slot?.Itemstack?.Collectible.Tool;
        var playerStack = slot.Itemstack;
        if (playerStack.Collectible is BlockLiquidContainerBase container)
        {
            BlockLiquidContainerBase liquidContainer = playerStack.Collectible as BlockLiquidContainerBase;
            if (slot.Itemstack.Collectible is not ILiquidSource liquidSource || !liquidSource.AllowHeldLiquidTransfer)
            {
                return false;
            }

            ItemStack contentStackToMove = liquidSource.GetContent(slot.Itemstack);

            WaterTightContainableProps contentProps = liquidSource.GetContentProps(slot.Itemstack);
            if (contentProps == null) {
                return false;
             }
           var itemsPerLitre = container.GetContentProps(slot.Itemstack).ItemsPerLitre;
            int moved = (int)(0.1 * contentProps.ItemsPerLitre);
            Api.World.Logger.Event(contentStackToMove.Collectible.Code.ToString());
            string bowlLiquid = contentStackToMove.Collectible.Code.ToString();

            if (!inventory[0].Empty)
            {
                ItemStack breadStack = inventory[0].Itemstack;
                string breadPath = breadStack?.Collectible?.Code?.Path;

                if (breadPath.StartsWith("breadslice-"))
                {
                    string breadType = breadPath.Split('-')[1];
                    if (bowlLiquid.Contains("peanutliquid-butter")) {
                        AssetLocation breadSliceAsset = new AssetLocation($"sandwich:peanutbutterbread-{breadType}");
                        liquidContainer.CallMethod<int>("splitStackAndPerformAction", byPlayer.Entity, slot, delegate (ItemStack stack)
                        {
                            container.TryTakeContent(stack, moved);
                            return moved;
                        });
                        container.DoLiquidMovedEffects(byPlayer, contentStackToMove, moved, EnumLiquidDirection.Pour);

                        if (Api.Side == EnumAppSide.Server)
                        {

                            var breadSlices = new ItemStack(Api.World.GetItem(breadSliceAsset), 1);

                            // Remove the bread from the board and spawn the slices
                            inventory[0].Itemstack = breadSlices;
                            MarkDirty(redrawOnClient: true);
                            updateMeshes();

                        }
                    } else if (bowlLiquid.Contains("honeyportion"))
                    {
                        AssetLocation breadSliceAsset = new AssetLocation($"sandwich:honeybread-{breadType}");
                        liquidContainer.CallMethod<int>("splitStackAndPerformAction", byPlayer.Entity, slot, delegate (ItemStack stack)
                        {
                            container.TryTakeContent(stack, moved);
                            return moved;
                        });
                        container.DoLiquidMovedEffects(byPlayer, contentStackToMove, moved, EnumLiquidDirection.Pour);

                        if (Api.Side == EnumAppSide.Server)
                        {

                            // Spawn 4 bread slices
                            var breadSlices = new ItemStack(Api.World.GetItem(breadSliceAsset), 1);

                            // Remove the bread from the board and spawn the slices
                            inventory[0].Itemstack = breadSlices;
                            inventory[0].Itemstack.Collectible.Code.Path = breadSliceAsset.Path;
                            MarkDirty(redrawOnClient: true);
                            updateMeshes();

                        }
                    }

                    return true;
                } else
                {
                    Api.World.Logger.Event("No valid bread found on the board.");
                }
            } else
            {
                Api.World.Logger.Event("No item on the cutting board.");
            }

        } else if (tool == EnumTool.Knife || tool == EnumTool.Sword)
        {
            Api.World.Logger.Event("Tool detected: " + tool);
            // Check if there is an item on the board
            if (!inventory[0].Empty)
            {
                ItemStack breadStack = inventory[0].Itemstack;
                string breadPath = breadStack.Collectible.Code.Path;

                // Only proceed if the item on the board is a bread with the form "game:bread-{type}-perfect"
                Api.World.Logger.Event($"{breadPath}");
                if (breadPath.StartsWith("bread-") && breadPath.EndsWith("-perfect"))
                {
                    // Extract the type of bread (between the dashes)
                    string breadType = breadPath.Split('-')[1]; // For example, "cassava" from "game:bread-cassava-perfect"
                    AssetLocation breadSliceAsset = new AssetLocation($"sandwich:breadslice-{breadType}");

                    if (Api.Side == EnumAppSide.Server)
                    {
                        // Spawn 4 bread slices
                        ItemStack breadSlices = new ItemStack(Api.World.GetItem(breadSliceAsset), 4);

                        ItemSandwich sandwichItem = new ItemSandwich();
                        //GridRecipe byRecipe = Api.World.GridRecipes.FirstOrDefault(recipe => recipe.Ingredients.Value.SatisfiesAsIngredient(breadStack));
                        Api.World.SpawnItemEntity(breadSlices, Pos.ToVec3d().Add(0, 0.5, 0));
                        sandwichItem.OnCreatedBySlicing(inventory[0], breadSlices, breadStack, 4);

                        inventory[0].TakeOutWhole();
                        MarkDirty(true);
                        updateMeshes();

                        Api.World.Logger.Event($"Sliced {breadType} bread into slices.");
                    }

                    return true;
                }
                else if (breadPath.StartsWith("cheese-") && breadPath.EndsWith("-1slice"))
                {
                    AssetLocation breadSliceAsset = new AssetLocation($"sandwich:cheeseslice");

                    if (Api.Side == EnumAppSide.Server)
                    {
                        var breadSlices = new ItemStack(Api.World.GetItem(breadSliceAsset), 2);

                        Api.World.SpawnItemEntity(breadSlices, Pos.ToVec3d().Add(0, 0.5, 0));
                        ItemSandwich sandwichItem = new();
                        sandwichItem.OnCreatedBySlicing(inventory[0], breadSlices, breadStack, 2);

                        inventory[0].TakeOutWhole();
                        MarkDirty(true);
                        updateMeshes();
                    }

                    return true;
                }
                else if (breadPath.StartsWith("redmeat-") && breadPath.EndsWith("-cooked"))
                {
                    AssetLocation breadSliceAsset = new AssetLocation($"sandwich:redmeatslice");

                    if (Api.Side == EnumAppSide.Server)
                    {
                        var breadSlices = new ItemStack(Api.World.GetItem(breadSliceAsset), 2);

                        inventory[0].TakeOutWhole();
                        Api.World.SpawnItemEntity(breadSlices, Pos.ToVec3d().Add(0, 0.1, 0));
                        MarkDirty(true);
                        updateMeshes();
                    }

                    return true;
                }
            } else
            {
                Api.World.Logger.Event("No item on the cutting board.");
            }
        }
        else if (playerStack.Collectible.Code.ToString().StartsWith("sandwich:"))
        {
            string playerStackCode = playerStack.Collectible.Code.ToString();
            Api.World.Logger.Event($"Player is holding: {playerStackCode}");

            // Check if there is an item on the board
            if (!inventory[0].Empty)
            {
                ItemStack boardStack = inventory[0].Itemstack;
                string boardBreadPath = boardStack.Collectible.Code.Path;
                Api.World.Logger.Event($"Bread on the board: {boardBreadPath}");

                bool canCreateSandwich = CheckForSandwichCreation(slot, inventory[0]);
                //Api.World.Logger.Event("THIS IS THE SLOT: " + slot?.Itemstack?.Collectible?.Code?.ToString());
                //Api.World.Logger.Event("THIS IS THE INVENTORY: " + inventory[0]?.Itemstack?.Collectible?.Code?.ToString());

                if (canCreateSandwich)
                {
                   // Api.World.Logger.Event("Sandwich creation logic will go here.");
                    CheckForSandwichCreation(slot, inventory[0]);
                } else
                {
                    return false;
                }
            }
            else
            {
                Api.World.Logger.Event("No item on the cutting board.");
            }
        }
        else
        {
            // Api.World.Logger.Event(playerStack.Collectible?.Code?.ToString());
            Api.World.Logger.Event("Player is not holding valid bread.");
        }


        return true;
    } 



    private bool TryPut(ItemSlot slot, BlockSelection blockSel)
    {
        int i = blockSel.SelectionBoxIndex;

        if (inventory[i].Empty)
        {
            int amount = slot.TryPutInto(Api.World, inventory[i]);
            updateMeshes();
            MarkDirty(redrawOnClient: true);
            (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            return amount > 0;
        }
        return false;
    }

    private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
    {
        int i = blockSel.SelectionBoxIndex;

        if (!inventory[i].Empty)
        {
            ItemStack stack = inventory[i].TakeOut(1);
            if (byPlayer.InventoryManager.TryGiveItemstack(stack))
            {
                AssetLocation sound = stack.Block?.Sounds?.Place;
                Api.World.PlaySoundAt(sound ?? DefaultPlaceSound, byPlayer.Entity, byPlayer, randomizePitch: true, 16f);
            }
            if (stack.StackSize > 0)
            {
                Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0, 0.5, 0));
            }
            (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);
            MarkDirty(redrawOnClient: true);
            updateMeshes();
            return true;
        }
        return false;
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        base.GetBlockInfo(forPlayer, sb);
        sb.AppendLine();

        if (!inventory[0].Empty)
        {
            ItemStack stack = inventory[0].Itemstack;
            sb.AppendLine(stack.GetName());
        }
    }

    protected override float[][] genTransformationMatrices()
    {
        float[][] tfMatrices = new float[slotCount][];

        const float x = 0.5f;
        const float y = 0.125f;
        const float z = 0.5f;
        tfMatrices[0] = new Matrixf()
            .Translate(0.5f, -0.06f, 0.5f)
            .RotateYDeg(Block.Shape.rotateY)
            .Translate(x - 0.5f, y, z - 0.5f)
            .Translate(-0.5f, 0f, -0.5f)
            .Values;

        return tfMatrices;
    }
}