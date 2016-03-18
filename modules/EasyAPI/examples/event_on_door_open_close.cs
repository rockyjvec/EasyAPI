public class Example : EasyAPI 
{ 
    public Example(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime) : base(grid, me, echo, elapsedTime) 
    { 
        Blocks.OfTypeLike("Door").AddEvent( 
            delegate(EasyBlock block) { 
                return block.Open(); 
            }, 
            doorOpened 
        ); 
    } 
 
    public bool doorOpened(EasyBlock door) 
    { 
        // Do something when door is opened 

        door.AddEvent( 
            delegate(EasyBlock block) { 
                return !block.Open(); 
            }, 
            doorClosed 
        ); 
 
        return false; 
    } 
 
    public bool doorClosed(EasyBlock door) 
    { 
        // Do something when door is closed 
 
        door.AddEvent( 
            delegate(EasyBlock block) { 
                return block.Open(); 
            }, 
            doorOpened 
        ); 
 
        return false; 
    } 
} 
