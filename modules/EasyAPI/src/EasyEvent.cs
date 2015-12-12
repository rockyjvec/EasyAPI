public class EasyEvent : IEasyEvent 
{ 
    Func<EasyBlock,bool> op; // The comparison function 
 
    private EasyBlock obj; // Object to pass through to the callback when the event is triggered 
 
    private Func<EasyBlock,bool> callback; // What to call when the event occurs 
 
    public EasyEvent(EasyBlock obj, Func<EasyBlock,bool> op, Func<EasyBlock,bool> callback) 
    { 
        this.op = op; 
        this.callback = callback; 
        this.obj = obj; 
    } 
 
    public override bool handle() 
    { 
        if(op((EasyBlock)obj)) 
        { 
            return callback((EasyBlock)obj); 
        } 
 
        return true; 
    } 
} 