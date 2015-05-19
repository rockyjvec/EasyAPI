public class Example : EasyAPI
{
    public bool damaged1Percent(EasyBlock b)
    {
        // Get parameters from the name (separated by '[' and ']')
        var parameters = b.NameParameters('[', ']'); 

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

        return false; // Remove the event once it is triggered
    }

    public bool notDamaged1Percent(EasyBlock b)
    {
        // Get parameters from the name (separated by '[' and ']')
        var parameters = b.NameParameters('[', ']'); 

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

        return false; // Remove the event once it is triggered.
    }

    public Example(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime) : base(grid, me, echo, elapsedTime) 
    {
        EasyBlocks monitoredBlocks = Blocks; // The event will be added to all these blocks
        
        AddEvents(
            monitoredBlocks,
            delegate(EasyBlock block) { // When this function returns true, the event is triggered
                return block.Damage() > 1; // when the block is damage more than 1%
            },
            damaged1Percent // this is called when the event is triggered
        );
    }
}
