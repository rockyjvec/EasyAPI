public class Example : EasyAPI 
{ 
    public Example(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime) : base(grid, me, echo, elapsedTime) 
    { 
        Blocks.OfTypeLike("Door").AddEvent( 
            delegate(EasyBlock block) { 
                return block.Open(); 
            }, 
            doorOpened,
            true
        ).AddEvent( 
            delegate(EasyBlock block) { 
                return !block.Open(); 
            }, 
            doorClosed,
            true
        );
    } 
 
    public bool doorOpened(EasyBlock door) 
    { 
        // Do something when door is opened 

        return true; 
    } 
 
    public bool doorClosed(EasyBlock door) 
    { 
        // Do something when door is closed 
 
        return true;
    } 
} 
