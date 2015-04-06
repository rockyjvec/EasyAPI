public class Example : EasyAPI
{
    public bool damaged1Percent(EasyBlock b)
    {
        var parameters = b.NameParameters('[', ']'); // Get parameters from the name (separated by '[' and ']')

        // Close doors specified in parameters
        for(int i = 0; i < parameters.Count; i++)
        {
            Blocks.Named(parameters[i]).ApplyAction("Open_Off");
        }

        // Add event when the damage is repaired so we can reopen the doors
        AddEvent(
            b, // add it to this block
            delegate(EasyBlock block) {
                return block.Damage() < 1;
            },
            notDamaged1Percent
        );

        return false; // Remove the event once it is triggered. Otherwise it will continue to be called until the Damage goes back to below 1%.
    }

    public bool notDamaged1Percent(EasyBlock b)
    {
        var parameters = b.NameParameters('[', ']'); // Get parameters from the name (separated by '[' and ']')

        // Open doors specified in parameters
        for(int i = 0; i < parameters.Count; i++)
        {
            Blocks.Named(parameters[i]).ApplyAction("Open_On");
        }

        // Add back the original event to close doors on damage.
        AddEvent(
            b, // add it to this block
            delegate(EasyBlock block) {
                return block.Damage() > 1;
            },
            damaged1Percent
        );

        return false; // Remove the event once it is triggered. Otherwise it will continue to be called until the Damage goes back to above 1%.
    }

    public Example(IMyGridTerminalSystem grid) : base(grid)
    {
        EasyBlocks monitoredBlocks = Blocks; // Add this event to each of these blocks (All by default.  Add filters here if you want to only monitor some blocks)
        AddEvents(
            monitoredBlocks,
            delegate(EasyBlock block) { // When this function returns true, the event is triggered
                return block.Damage() > 1; // when the block is damage more than 1%
            },
            damaged1Percent // this is called when the event is triggered
        );
    }
}
