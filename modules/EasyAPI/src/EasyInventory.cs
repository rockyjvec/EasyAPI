// Stores all items in matched block inventories for later filtering
public class EasyInventory
{
    public List<EasyItem> Items;

    public EasyInventory(List<EasyBlock> Blocks)
    {
        this.Items = new List<EasyItem>();

        // Get contents of all inventories in list and add them to EasyItems list.
        for(int i = 0; i < Blocks.Count; i++)
        {
            EasyBlock Block = Blocks[i];

            for(int j = 0; j < Block.Block.InventoryCount; j++)
            {
                IMyInventory Inventory = Block.Block.GetInventory(j);

                List<IMyInventoryItem> Items = Inventory.GetItems();

                for(int k = 0; k < Items.Count; k++)
                {
                    this.Items.Add(new EasyItem(Block, j, Inventory, k, Items[k]));
                }
            }
        }
    }

    public EasyInventory(List<EasyItem> Items)
    {
        this.Items = Items;
    }

    public EasyInventory OfType(String SubTypeId)
    {
        List<EasyItem> FilteredItems = new List<EasyItem>();

        for(int i = 0; i < this.Items.Count; i++)
        {
            if(this.Items[i].Type() == SubTypeId)
            {
                FilteredItems.Add(this.Items[i]);
            }
        }

        return new EasyInventory(FilteredItems);
    }

    public EasyInventory InInventory(int Index)
    {
        List<EasyItem> FilteredItems = new List<EasyItem>();

        for(int i = 0; i < this.Items.Count; i++)
        {
            if(this.Items[i].InventoryIndex == Index)
            {
                FilteredItems.Add(this.Items[i]);
            }
        }

        return new EasyInventory(FilteredItems);
    }

    public VRage.MyFixedPoint Count()
    {
        VRage.MyFixedPoint Total = 0;

        for(int i = 0; i < Items.Count; i++)
        {
            Total += Items[i].Amount();
        }

        return Total;
    }

    public EasyInventory First()
    {
        List<EasyItem> FilteredItems = new List<EasyItem>();

        if(this.Items.Count > 0)
        {
            FilteredItems.Add(this.Items[0]);
        }

        return new EasyInventory(FilteredItems);
    }

    public void MoveTo(EasyBlocks Blocks, int Inventory = 0)
    {
        for(int i = 0; i < Items.Count; i++)
        {
            Items[i].MoveTo(Blocks, Inventory);
        }
    }
}
