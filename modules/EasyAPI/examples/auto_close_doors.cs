public class Example : EasyAPI  
{
    EasyBlocks doors;

    public Example(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime) : base(grid, me, echo, elapsedTime)  
    {  
        doors = Blocks.OfType("Sliding Door");

        // split work over time so it doesn't throw a complexity error (tested with 384 doors)
        In(1 * EasyAPI.Seconds, delegate() {
              doors.AddEvent(  
                  delegate(EasyBlock block) {
                      return block.Open();  
                  },  
                  doorOpened  
              );  
        });
    }  
  
    public bool doorOpened(EasyBlock door)  
    {
        In(2 * EasyAPI.Seconds, delegate() {
            door.ApplyAction("Open_Off");
        });
 
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
        door.AddEvent(  
            delegate(EasyBlock block) {  
                return block.Open();  
            },  
            doorOpened  
        );  
  
        return false;  
    }  
}  
