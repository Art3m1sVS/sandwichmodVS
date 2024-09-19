using Vintagestory.API.Common;

namespace sandwich;

public class ItemSlotCuttingBoard : ItemSlot
{
    public override int MaxSlotStackSize => 1;

    public ItemSlotCuttingBoard(InventoryBase inventory) : base(inventory)
    {
        this.inventory = inventory;
    }

    public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
    {
        return sourceSlot.IsCuttingBoardable() && base.CanTakeFrom(sourceSlot, priority);
    }

    public override bool CanHold(ItemSlot fromSlot)
    {
        return fromSlot.IsCuttingBoardable() && base.CanHold(fromSlot);
    }

    public static bool IsStorable(CollectibleObject obj)
    {
        return obj?.Attributes?.KeyExists(attributeCodeCuttingBoard) == true && obj.Attributes[attributeCodeCuttingBoard].AsBool();
    }
}
