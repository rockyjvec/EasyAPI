/**************************************************/ 
/*** EasyAPI class. Extend for easier scripting ***/ 
/**************************************************/ 
public abstract class EasyAPI 
{ 
    private long start = 0; // Time at start of program 
    private long clock = 0; // Current time in ticks 
    private long delta = 0; // Time since last call to Tick in ticks 
 
    public EasyBlock Self; // Reference to the Programmable Block that is running this script 
 
    public IMyGridTerminalSystem GridTerminalSystem;
    public Action<string> Echo;
    public TimeSpan ElapsedTime;
    
    static public IMyGridTerminalSystem grid; 
 
    /*** Events ***/ 
    private Dictionary<string,List<Action>> ArgumentActions; 
    private Dictionary<string,List<Action<int, string[]>>> CommandActions; 
    private List<EasyInterval> Schedule; 
    private List<EasyInterval> Intervals; 
    private EasyCommands commands;
 
    /*** Overridable lifecycle methods ***/ 
    public virtual void onRunThrottled(float intervalTranspiredPercentage) {} 
    public virtual void onTickStart() {} 
    public virtual void onTickComplete() {} 
    public virtual bool onSingleTap() { return false; } 
    public virtual bool onDoubleTap() { return false; } 
    private int InterTickRunCount = 0; 
 
    /*** Cache ***/ 
    public EasyBlocks Blocks; 
 
    /*** Constants ***/ 
    public const long Microseconds = 10; // Ticks (100ns) 
    public const long Milliseconds = 1000 * Microseconds; 
    public const long Seconds =   1000 * Milliseconds; 
    public const long Minutes = 60 * Seconds; 
    public const long Hours = 60 * Minutes; 
    public const long Days = 24 * Hours; 
    public const long Years = 365 * Days; 
 
    /*** Constructor ***/ 
    public EasyAPI(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime, string commandArgument = "EasyCommand") 
    { 
        this.clock = this.start = DateTime.Now.Ticks; 
        this.delta = 0; 
 
        this.GridTerminalSystem = EasyAPI.grid = grid; 
        this.Echo = echo; 
        this.ElapsedTime = elapsedTime; 
        this.ArgumentActions = new Dictionary<string,List<Action>>(); 
        this.CommandActions = new Dictionary<string,List<Action<int, string[]>>>(); 
        this.Schedule = new List<EasyInterval>(); 
        this.Intervals = new List<EasyInterval>(); 
        this.commands = new EasyCommands(this);
 
        // Get the Programmable Block that is running this script (thanks to LordDevious and LukeStrike) 
        this.Self = new EasyBlock(me); 
 
        this.Reset(); 
    } 
 
    public void AddEvent(EasyEvent e) 
    { 
        EasyEvent.add(e); 
    } 
 
    public void AddEvent(EasyBlock block, Func<EasyBlock, bool> evnt, Func<EasyBlock, bool> action, bool onChange = false) 
    {
        this.AddEvent(new EasyEvent(block, evnt, action, onChange)); 
    } 
 
    public void AddEvents(EasyBlocks blocks, Func<EasyBlock, bool> evnt, Func<EasyBlock, bool> action, bool onChange = false)
    { 
        for(int i = 0; i < blocks.Count(); i++) 
        { 
            this.AddEvent(new EasyEvent(blocks.GetBlock(i), evnt, action, onChange)); 
        } 
    } 
 
    // Get messages sent to this block 
    public List<EasyMessage> GetMessages() 
    { 
        var mymessages = new List<EasyMessage>(); 
 
        var parts = this.Self.Name().Split('\0'); 
 
        if(parts.Length > 1) 
        { 
            for(int n = 1; n < parts.Length; n++) 
            { 
                EasyMessage m = new EasyMessage(parts[n]); 
                mymessages.Add(m); 
            } 
 
            // Delete the messages once they are received 
            this.Self.SetName(parts[0]); 
        } 
        return mymessages; 
    } 
 
    // Clear messages sent to this block 
    public void ClearMessages() 
    { 
        var parts = this.Self.Name().Split('\0'); 
 
        if(parts.Length > 1) 
        { 
            // Delete the messages 
            this.Self.SetName(parts[0]); 
        } 
    } 
 
    public EasyMessage ComposeMessage(String Subject, String Message) 
    { 
        return new EasyMessage(this.Self, Subject, Message); 
    } 
 
    /*** Execute one tick of the program (interval is the minimum time between ticks) ***/ 
    public void Tick(long interval = 0, string argument = "") 
    { 
        /*** Handle Arguments ***/ 
 
        if(argument != "")
        {           
            if(this.ArgumentActions.ContainsKey(argument)) 
            { 
                for(int n = 0; n < this.ArgumentActions[argument].Count; n++) 
                { 
                    this.ArgumentActions[argument][n](); 
                } 
            } 
            else if(this.CommandActions.Count > 0) 
            {
                int argc = 0;
                var matches = System.Text.RegularExpressions.Regex.Matches(argument, @"(?<match>[^\s""]+)|""(?<match>[^""]*)""");
                
                string[] argv = new string[matches.Count];
                for(int n = 0; n < matches.Count; n++)
                {
                    argv[n] = matches[n].Groups["match"].Value;
                }
                
                argc = argv.Length;
                
                if(argc > 0 && this.CommandActions.ContainsKey(argv[0]))
                {
                    for(int n = 0; n < this.CommandActions[argv[0]].Count; n++) 
                    { 
                        this.CommandActions[argv[0]][n](argc, argv); 
                    }                 
                }
            }
            else if(argument.Length > 12 && argument.Substring(0, 12) == "EasyCommand ")
            {
                this.commands.handle(argument.Substring(12));
            }
        }

        long now = DateTime.Now.Ticks; 
        if(this.clock > this.start && now - this.clock < interval) { 
            InterTickRunCount++; 
            float transpiredPercentage = ((float)((double)(now - this.clock) / interval)); 
            onRunThrottled(transpiredPercentage); 
            return; // Don't run until the minimum time between ticks 
        } 
        if(InterTickRunCount == 1) { 
            if(onSingleTap()) { 
                return; // Override has postponed this Tick to next Run 
            } 
        } else if(InterTickRunCount > 1) { 
            if(onDoubleTap()) { 
                return; // Override has postponed this Tick to next Run 
            } 
        } 
        InterTickRunCount = 0; 
        onTickStart(); 
 
        long lastClock = this.clock; 
        this.clock = now; 
        this.delta = this.clock - lastClock; 
 
        /*** Handle Events ***/ 
        EasyEvent.handle(); 
 
        /*** Handle Intervals ***/ 
        for(int n = 0; n < this.Intervals.Count; n++) 
        { 
            if(this.clock >= this.Intervals[n].time) 
            { 
                long time = this.clock + this.Intervals[n].interval - (this.clock - this.Intervals[n].time); 
 
                (this.Intervals[n].action)(); 
                this.Intervals[n] = new EasyInterval(time, this.Intervals[n].interval, this.Intervals[n].action); // reset time interval 
            } 
        } 
 
        /*** Handle Schedule ***/ 
        for(int n = 0; n < this.Schedule.Count; n++) 
        { 
            if(this.clock >= this.Schedule[n].time) 
            { 
                (this.Schedule[n].action)(); 
                Schedule.Remove(this.Schedule[n]); 
            } 
        } 
 
        onTickComplete(); 
    } 
 
    public long GetDelta() {return this.delta;} 
 
    public long GetClock() {return clock;} 
 
    public void On(string argument, Action callback) 
    { 
        if(!this.ArgumentActions.ContainsKey(argument)) 
        { 
            this.ArgumentActions.Add(argument, new List<Action>()); 
        } 
 
        this.ArgumentActions[argument].Add(callback); 
    } 
 
    public void OnCommand(string argument, Action<int, string[]> callback) 
    { 
        if(!this.CommandActions.ContainsKey(argument)) 
        { 
            this.CommandActions.Add(argument, new List<Action<int, string[]>>()); 
        } 
 
        this.CommandActions[argument].Add(callback);
    } 
 
    /*** Call a function at the specified time ***/ 
    public void At(long time, Action callback) 
    { 
        long t = this.start + time; 
        Schedule.Add(new EasyInterval(t, 0, callback)); 
    } 
 
    /*** Call a function every interval of time ***/ 
    public void Every(long time, Action callback) 
    { 
        Intervals.Add(new EasyInterval(this.clock + time, time, callback)); 
    } 
 
    /*** Call a function in "time" seconds ***/ 
    public void In(long time, Action callback) 
    { 
        this.At(this.clock - this.start + time, callback); 
    } 
 
    /*** Resets the clock and refreshes the blocks.  ***/ 
    public void Reset() 
    { 
        this.start = this.clock; 
        this.ClearMessages(); // clear messages 
        this.Refresh(); 
    } 
 
    /*** Refreshes blocks.  If you add or remove blocks, call this. ***/ 
    public void Refresh() 
    { 
        List<IMyTerminalBlock> kBlocks = new List<IMyTerminalBlock>(); 
        GridTerminalSystem.GetBlocks(kBlocks); 
        Blocks = new EasyBlocks(kBlocks); 
    } 
} 
