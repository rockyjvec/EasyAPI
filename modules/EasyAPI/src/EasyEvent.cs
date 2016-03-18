public class EasyEvent
{ 
    private Func<EasyBlock,bool> op; // The comparison function 
 
    private EasyBlock block; // Object to pass through to the callback when the event is triggered 
 
    private Func<EasyBlock,bool> callback; // What to call when the event occurs 
 
    public EasyEvent(EasyBlock block, Func<EasyBlock,bool> op, Func<EasyBlock,bool> callback) 
    { 
        this.op = op; 
        this.callback = callback; 
        this.block = block; 
    } 
 
    public bool process() 
    { 
        if((this.op)(this.block)) 
        { 
            return (this.callback)(this.block);
        } 
 
        return true; 
    }
    
    /*** static ***/
    
    private static List<EasyEvent> events = new List<EasyEvent>();
    
    public static void handle() 
    {
        for(int i = 0; i < events.Count; i++)
        {             
            if(!events[i].process()) 
            { 
                events.Remove(events[i]); 
            } 
        } 
    } 

    public static void add(EasyEvent e) 
    { 
        events.Add(e); 
    } 
}
