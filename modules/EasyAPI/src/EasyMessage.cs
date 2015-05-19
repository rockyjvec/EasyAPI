public struct EasyMessage
{
    public EasyBlock From;
    public String Subject;
    public String Message;
    public long Timestamp;

    // unserialize
    public EasyMessage(String serialized)
    {
        var parts = serialized.Split(':');
        if(parts.Length < 4)
        {
            throw new Exception("Error unserializing message.");
        }
        int numberInGrid = Convert.ToInt32(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[0])));
        var blocks = new List<IMyTerminalBlock>();
        EasyAPI.grid.GetBlocksOfType<IMyProgrammableBlock>(blocks, delegate(IMyTerminalBlock block) {
           return (block as IMyProgrammableBlock).NumberInGrid == numberInGrid;
        });
        if(blocks.Count == 0)
        {
            throw new Exception("Message sender no longer exits!");
        }
        this.From = new EasyBlock((IMyTerminalBlock)blocks[0]);
        this.Subject = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[1]));
        this.Message = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[2]));
        this.Timestamp = Convert.ToInt64(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[3])));
    }

    public EasyMessage(EasyBlock From, String Subject, String Message)
    {
        this.From = From;
        this.Subject = Subject;
        this.Message = Message;
        this.Timestamp = DateTime.Now.Ticks;
    }

    public String Serialize()
    {
        String text = "";

        text += System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("" + From.Block.NumberInGrid));
        text += ":" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Subject));
        text += ":" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Message));
        text += ":" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("" + Timestamp));

        return text;
    }
}
