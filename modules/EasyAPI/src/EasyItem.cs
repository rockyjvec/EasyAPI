// This represents a single stack of items in the inventory
public struct EasyItem
{
    private EasyBlock Block;
    public int InventoryIndex;
    private VRage.ModAPI.IMyInventory Inventory;
    public int ItemIndex;
    private VRage.ModAPI.IMyInventoryItem Item;

    public EasyItem(EasyBlock Block, int InventoryIndex, VRage.ModAPI.IMyInventory Inventory, int ItemIndex, VRage.ModAPI.IMyInventoryItem Item)
    {
        this.Block = Block;
        this.InventoryIndex = InventoryIndex;
        this.Inventory = Inventory;
        this.ItemIndex = ItemIndex;
        this.Item = Item;
    }

    public String Type(int dummy = 0)
    {
        return this.Item.Content.SubtypeName;
    }

    public VRage.MyFixedPoint Amount()
    {
        return this.Item.Amount;
    }

    public void MoveTo(EasyBlocks Blocks, int Inventory = 0, int dummy = 0)
    {
        // Right now it moves them to all of them.  Todo: determine if the move was successful an exit for if it was.
        // In the future you will be able to sort EasyBlocks and use this to prioritize where the items get moved.
        for(int i = 0; i < Blocks.Count(); i++)
        {
            this.Inventory.TransferItemTo(Blocks.GetBlock(i).Block.GetInventory(Inventory), ItemIndex);
        }
    }
}
