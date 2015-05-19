public class EasyEvent<C> : IEasyEvent
    where C: struct
{
    Func<C,bool> op; // The comparison function

    private object obj; // Object to pass through to the callback when the event is triggered

    private Func<C,bool> callback; // What to call when the event occurs

    public EasyEvent(C obj, Func<C,bool> op, Func<C,bool> callback)
    {
        this.op = op;
        this.callback = callback;
        this.obj = obj;
    }

    public override bool handle()
    {
        if(op((C)obj))
        {
            return callback((C)obj);
        }

        return true;
    }
}
