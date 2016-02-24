public class EasyEvent
{ 
    private Func<EasyBlock,bool> op; // The comparison function 
 
    private EasyBlock obj; // Object to pass through to the callback when the event is triggered 
 
    private Func<EasyBlock,bool> callback; // What to call when the event occurs 
 
    public EasyEvent(EasyBlock obj, Func<EasyBlock,bool> op, Func<EasyBlock,bool> callback) 
    { 
        this.op = op; 
        this.callback = callback; 
        this.obj = obj; 
    } 
 
    public bool handle() 
    { 
        if((this.op)((EasyBlock)this.obj)) 
        { 
            return (this.callback)((EasyBlock)this.obj); 
        } 
 
        return true; 
    } 
}
