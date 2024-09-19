using sandwich;
using Vintagestory.API.Common;

namespace sandwich;

public class BlockCuttingBoard : Block
{
    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        InteractionHelpYOffset = 0.125f;
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        return world.BlockAccessor.GetBlockEntity(blockSel.Position) is not BlockEntityCuttingBoard blockEntity
            ? base.OnBlockInteractStart(world, byPlayer, blockSel)
            : blockEntity.OnInteract(byPlayer) || base.OnBlockInteractStart(world, byPlayer, blockSel);
    }
}