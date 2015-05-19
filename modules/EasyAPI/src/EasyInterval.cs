public struct EasyInterval
{
    public long interval;
    public long time;
    public Action action;

    public EasyInterval(long t, long i, Action a)
    {
        this.time = t;
        this.interval = i;
        this.action = a;
    }
}
