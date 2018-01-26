public class Example : EasyAPI  
{
    EasyBlocks doors;

    public Example(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime) : base(grid, me, echo, elapsedTime)  
    {  
        doors = Blocks.OfType("Door");

        // split work over time so it doesn't throw a complexity error (tested with 384 doors)
        In(1 * EasyAPI.Seconds, delegate() { // In one second, create the events.
            doors.AddEvent(  
                delegate(EasyBlock block) { // When a door is opened
                    return block.Open();  
                },  
                delegate(EasyBlock block) { // Do the following
                    In(2 * EasyAPI.Seconds, delegate() {  // In 2 seconds
                        block.ApplyAction("Open_Off"); // close the door
                    });                    
                    
                    return true;
                },
                true // only trigger event when the condition (door open) goes from false to true
            );
        });
    }  
}  
