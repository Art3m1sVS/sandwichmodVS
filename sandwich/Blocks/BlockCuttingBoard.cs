using System.Linq;
using sandwich;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace sandwich;

public class BlockCuttingBoard : Block
{
    private WorldInteraction[] interactions;

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        if (api.Side == EnumAppSide.Client)
        {
            interactions = (api as ICoreClientAPI)?.GetOrCreateToolrackInteractions("CuttingBoardBlockInteractions", EnumStorageOption.CuttingBoardable);
        }
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
    {
        return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
    }

    public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) => true;

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        return world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityCuttingBoard blockEntity
            ? blockEntity.OnInteract(byPlayer, blockSel)
            : base.OnBlockInteractStart(world, byPlayer, blockSel);
    }
}
