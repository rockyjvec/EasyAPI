/************************************************************************************
EasyAPI - Documentation: http://steamcommunity.com/sharedfiles/filedetails/?id=381043
*************************************************************************************/

public class Example : EasyAPI
{
    public Example(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime) : base(grid, me, echo, elapsedTime)
    {
        // Start your code here
    }
}


/*********************************************/ 
/*** Advanced users only beyond this point ***/ 
/*********************************************/ 

Example state;

public Program()
{
    state = new Example(GridTerminalSystem, Me, Echo, Runtime.TimeSinceLastRun);
	
	Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateType)
{
    if(state == null)
    {
        state = new Example(GridTerminalSystem, Me, Echo, Runtime.TimeSinceLastRun);
    }

    // Set the minimum time between ticks here to prevent lag.
    // To utilize onSingleTap and onDoubleTap, set the minimum time to the same
    // time period of the timer running this script (e.g. 1 * EasyAPI.Seconds).
    state.Tick(100 * EasyAPI.Milliseconds, argument);
}
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


public class EasyBlocks
{
    public List<EasyBlock> Blocks;

    // Constructor with IMyTerminalBlock list
    public EasyBlocks(List<IMyTerminalBlock> TBlocks)
    {
        this.Blocks = new List<EasyBlock>();

        for(int i = 0; i < TBlocks.Count; i++)
        {
            EasyBlock Block = new EasyBlock(TBlocks[i]);
            this.Blocks.Add(Block);
        }
    }

    // Constructor with EasyBlock list
    public EasyBlocks(List<EasyBlock> Blocks)
    {
        this.Blocks = Blocks;
    }

    public EasyBlocks()
    {
        this.Blocks = new List<EasyBlock>();
    }

    // Get number of blocks in list
    public int Count()
    {
        return this.Blocks.Count;
    }

    // Get a specific block from the list
    public EasyBlock GetBlock(int i)
    {
        return this.Blocks[i];
    }

    /*********************/
    /*** Block Filters ***/
    /*********************/

    /*** Interface Filters ***/

    public EasyBlocks WithInterface<T>() where T: class
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        for(int i = 0; i < this.Blocks.Count; i++)
        {
            T block = this.Blocks[i].Block as T;

            if(block != null)
            {
                FilteredList.Add(this.Blocks[i]);
            }
        }

        return new EasyBlocks(FilteredList);
    }

    /*** Type Filters ***/

    public EasyBlocks OfType(String Type)
    {
        return TypeFilter("==", Type);
    }

    public EasyBlocks NotOfType(String Type)
    {
        return TypeFilter("!=", Type);
    }

    public EasyBlocks OfTypeLike(String Type)
    {
        return TypeFilter("~", Type);
    }

    public EasyBlocks NotOfTypeLike(String Type)
    {
        return TypeFilter("!~", Type);
    }

    public EasyBlocks OfTypeRegex(String Pattern)
    {
        return TypeFilter("R", Pattern);
    }

    public EasyBlocks NotOfTypeRegex(String Pattern)
    {
        return TypeFilter("!R", Pattern);
    }

    protected EasyBlocks TypeFilter(String op, String Type)
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        for(int i = 0; i < this.Blocks.Count; i++)
        {
            if(EasyCompare(op, this.Blocks[i].Type(), Type))
            {
                FilteredList.Add(this.Blocks[i]);
            }
        }

        return new EasyBlocks(FilteredList);
    }

    /*** Name Filters ***/

    public EasyBlocks Named(String Name)
    {
        return NameFilter("==", Name);
    }

    public EasyBlocks NotNamed(String Name)
    {
        return NameFilter("!=", Name);
    }

    public EasyBlocks NamedLike(String Name)
    {
        return NameFilter("~", Name);
    }

    public EasyBlocks NotNamedLike(String Name)
    {
        return NameFilter("!~", Name);
    }

    public EasyBlocks NamedRegex(String Pattern)
    {
        return NameFilter("R", Pattern);
    }

    public EasyBlocks NotNamedRegex(String Pattern)
    {
        return NameFilter("!R", Pattern);
    }

    protected EasyBlocks NameFilter(String op, String Name)
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        for(int i = 0; i < this.Blocks.Count; i++)
        {
            if(EasyCompare(op, this.Blocks[i].Name(), Name))
            {
                FilteredList.Add(this.Blocks[i]);
            }
        }

        return new EasyBlocks(FilteredList);
    }

    /*** Group Filters ***/

    public EasyBlocks InGroupsNamed(String Group)
    {
        return GroupFilter("==", Group);
    }

    public EasyBlocks InGroupsNotNamed(String Group)
    {
        return GroupFilter("!=", Group);
    }

    public EasyBlocks InGroupsNamedLike(String Group)
    {
        return GroupFilter("~", Group);
    }

    public EasyBlocks InGroupsNotNamedLike(String Group)
    {
        return GroupFilter("!~", Group);
    }

    public EasyBlocks InGroupsNamedRegex(String Pattern)
    {
        return GroupFilter("R", Pattern);
    }

    public EasyBlocks InGroupsNotNamedRegex(String Pattern)
    {
        return GroupFilter("!R", Pattern);
    }

    public EasyBlocks GroupFilter(String op, String Group)
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
        EasyAPI.grid.GetBlockGroups(groups);
        List<IMyBlockGroup> matchedGroups = new List<IMyBlockGroup>();
        List<IMyTerminalBlock> groupBlocks = new List<IMyTerminalBlock>();

        for(int n = 0; n < groups.Count; n++)
        {
            if(EasyCompare(op, groups[n].Name, Group))
            {
                matchedGroups.Add(groups[n]);
            }
        }

        for(int n = 0; n < matchedGroups.Count; n++)
        {
            for(int i = 0; i < this.Blocks.Count; i++)
            {
                IMyTerminalBlock block = this.Blocks[i].Block;
                matchedGroups[n].GetBlocks(groupBlocks);
                for(int j = 0; j < groupBlocks.Count; j++)
                {
                    if(block == groupBlocks[j])
                    {
                        FilteredList.Add(this.Blocks[i]);
                    }
                }
                groupBlocks.Clear();
            }
        }

        return new EasyBlocks(FilteredList);
    }

    /*** Sensor Filters ***/

    public EasyBlocks SensorsActive(bool isActive = true)
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        for(int i = 0; i < this.Blocks.Count; i++)
        {
            if(this.Blocks[i].Type() == "Sensor" && ((IMySensorBlock)this.Blocks[i].Block).IsActive == isActive)
            {
                FilteredList.Add(this.Blocks[i]);
            }
        }

        return new EasyBlocks(FilteredList);
    }

    public EasyBlocks RoomPressure(String op, Single percent)
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        for(int i = 0; i < this.Blocks.Count; i++)
        {
            if(this.Blocks[i].RoomPressure(op, percent))
            {
                FilteredList.Add(this.Blocks[i]);
            }
        }

        return new EasyBlocks(FilteredList);
    }


    /*** Advanced Filters ***/

    public EasyBlocks FilterBy(Func<EasyBlock, bool> action)
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        for(int i = 0; i < this.Blocks.Count; i++)
        {
            if(action(this.Blocks[i]))
            {
                FilteredList.Add(this.Blocks[i]);
            }
        }

        return new EasyBlocks(FilteredList);
    }


    /*** Other ***/

    public EasyBlocks First()
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        if(this.Blocks.Count > 0)
        {
            FilteredList.Add(Blocks[0]);
        }

        return new EasyBlocks(FilteredList);
    }

    public EasyBlocks Add(EasyBlock Block)
    {
        this.Blocks.Add(Block);

        return this;
    }

    public EasyBlocks Plus(EasyBlocks Blocks)
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        FilteredList.AddRange(this.Blocks);
        for(int i = 0; i < Blocks.Count(); i++)
        {
            if(!FilteredList.Contains(Blocks.GetBlock(i)))
            {
                FilteredList.Add(Blocks.GetBlock(i));
            }
        }

        return new EasyBlocks(FilteredList);
    }

    public EasyBlocks Minus(EasyBlocks Blocks)
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        FilteredList.AddRange(this.Blocks);
        for(int i = 0; i < Blocks.Count(); i++)
        {
            FilteredList.Remove(Blocks.GetBlock(i));
        }

        return new EasyBlocks(FilteredList);
    }

    public static EasyBlocks operator +(EasyBlocks a, EasyBlocks b)
    {
        return a.Plus(b);
    }

    public static EasyBlocks operator -(EasyBlocks a, EasyBlocks b)
    {
        return a.Minus(b);
    }

    public EasyBlocks Plus(EasyBlock Block)
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        FilteredList.AddRange(this.Blocks);

        if(!FilteredList.Contains(Block))
        {
            FilteredList.Add(Block);
        }

        return new EasyBlocks(FilteredList);
    }

    public EasyBlocks Minus(EasyBlock Block)
    {
        List<EasyBlock> FilteredList = new List<EasyBlock>();

        FilteredList.AddRange(this.Blocks);
        FilteredList.Remove(Block);

        return new EasyBlocks(FilteredList);
    }

    public static EasyBlocks operator +(EasyBlocks a, EasyBlock b)
    {
        return a.Plus(b);
    }

    public static EasyBlocks operator -(EasyBlocks a, EasyBlock b)
    {
        return a.Minus(b);
    }
    
    /*** Operations ***/

    public EasyBlocks FindOrFail(string message)
    {
        if(this.Count() == 0) throw new Exception(message);

        return this;
    }

    public EasyBlocks Run(EasyAPI api, string type = "public")
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].Run(api, type);
        }
        
        return this;
    }
    
    public EasyBlocks SendMessage(EasyMessage message)
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].SendMessage(message);
        }

        return this;
    }


    public EasyBlocks ApplyAction(String Name)
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].ApplyAction(Name);
        }

        return this;
    }

    public EasyBlocks SetProperty<T>(String PropertyId, T value, int bleh = 0)
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].SetProperty<T>(PropertyId, value);
        }

        return this;
    }

    public EasyBlocks SetFloatValue(String PropertyId, float value, int bleh = 0)
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].SetFloatValue(PropertyId, value);
        }

        return this;
    }

    public T GetProperty<T>(String PropertyId, int bleh = 0)
    {
        return this.Blocks[0].GetProperty<T>(PropertyId);
    }

    public EasyBlocks On()
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].On();
        }

        return this;
    }

    public EasyBlocks Off()
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].Off();
        }

        return this;
    }

    public EasyBlocks Toggle()
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].Toggle();
        }

        return this;
    }
    
    public EasyBlocks RunPB(string argument = "")
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].RunPB(argument);
        }

        return this;
    }

    public EasyBlocks WritePublicText(string text)
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].WritePublicText(text);
        }

        return this;
    }

    public EasyBlocks WriteCustomData(string text)
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].WriteCustomData(text);
        }

        return this;
    }

    public EasyBlocks WritePublicTitle(string text)
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].WritePublicTitle(text);
        }

        return this;
    }

    public EasyBlocks WritePrivateTitle(string text)
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].WritePublicTitle(text);
        }

        return this;
    }

    public EasyBlocks AppendPublicText(string text)
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].AppendPublicText(text);
        }

        return this;
    }

    public EasyInventory Items()
    {
        return new EasyInventory(this.Blocks);
    }

    public string DebugDump(bool throwIt = true)
    {
        String output = "\n";

        for(int i = 0; i < this.Blocks.Count; i++)
        {
            output += this.Blocks[i].Type() + ": " + this.Blocks[i].Name() + "\n";
        }

        if(throwIt)
            throw new Exception(output);
        else
            return output;
    }

    public string DebugDumpActions(bool throwIt = true)
    {
        String output = "\n";

        for(int i = 0; i < this.Blocks.Count; i++)
        {
            output += "[ " + this.Blocks[i].Type() + ": " + this.Blocks[i].Name() + " ]\n";
            output += "*** ACTIONS ***\n";
            List<ITerminalAction> actions = this.Blocks[i].GetActions();

            for(int j = 0; j < actions.Count; j++)
            {
                output += actions[j].Id + ":" + actions[j].Name + "\n";
            }

            output += "\n\n";
        }

        if(throwIt)
            throw new Exception(output);
        else
            return output;
    }

    public string DebugDumpProperties(bool throwIt = true)
    {
        String output = "\n";

        for(int i = 0; i < this.Blocks.Count; i++)
        {
            output += "[ " + this.Blocks[i].Type() + ": " + this.Blocks[i].Name() + " ]\n";
            output += "*** PROPERTIES ***\n";
            List<ITerminalProperty> properties = this.Blocks[i].GetProperties();

            for(int j = 0; j < properties.Count; j++)
            {
                output += properties[j].TypeName + ": " + properties[j].Id + "\n";
            }

            output += "\n\n";
        }

        if(throwIt)
            throw new Exception(output);
        else
            return output;
    }
    
    /*** Events ***/
    
    public EasyBlocks AddEvent(Func<EasyBlock, bool> evnt, Func<EasyBlock, bool> action, bool onChange = false)
    {
        for(int i = 0; i < this.Blocks.Count; i++)
        {
            this.Blocks[i].AddEvent(evnt, action, onChange);
        }

        return this;        
    }

    private bool EasyCompare(String op, String a, String b)
    {
        switch(op)
        {
            case "==":
                return (a == b);
            case "!=":
                return (a != b);
            case "~":
                return a.Contains(b);
            case "!~":
                return !a.Contains(b);
            case "R":
                System.Text.RegularExpressions.Match m = (new System.Text.RegularExpressions.Regex(b)).Match(a);
                while(m.Success)
                {
                    return true;
                }
                return false;
            case "!R":
                return !EasyCompare("R", a, b);
        }
        return false;
    }
    
}
public struct EasyBlock
{
    public IMyTerminalBlock Block;
    private IMySlimBlock slim;

    public EasyBlock(IMyTerminalBlock Block)
    {
        this.Block = Block;
        this.slim = null;
    }

    public IMySlimBlock Slim()
    {
        if(this.slim == null)
        {
            this.slim = this.Block.CubeGrid.GetCubeBlock(this.Block.Position);
        }

        return this.slim;
    }

    public String Type()
    {
        return this.Block.DefinitionDisplayNameText;
    }

    public Single Damage()
    {
        return this.CurrentDamage() / this.MaxIntegrity() * (Single)100.0;
    }

    public Single CurrentDamage()
    {
        return this.Slim().CurrentDamage;
    }

    public Single MaxIntegrity()
    {
        return this.Slim().MaxIntegrity;
    }

    public bool Open()
    {
        IMyDoor door = Block as IMyDoor;

        if(door != null)
        {
            return door.Status == DoorStatus.Open;
        }

        return false;
    }

    public String Name()
    {
        return this.Block.CustomName;
    }

    public void SendMessage(EasyMessage message)
    {
        // only programmable blocks can receive messages
        if(Type() == "Programmable block")
        {
            SetName(Name() + "\0" + message.Serialize());
        }
    }

    public List<String> NameParameters(char start = '[', char end = ']')
    {
        List<String> matches;

        this.NameRegex(@"\" + start + @"(.*?)\" + end, out matches);

        return matches;
    }

    public bool RoomPressure(String op, Single percent)
    {
        String roomPressure = DetailedInfo()["Room pressure"];

        Single pressure = 0;

        if(roomPressure != "Not pressurized")
        {
            pressure = Convert.ToSingle(roomPressure.TrimEnd('%'));
        }

        switch(op)
        {
            case "<":
                return pressure < percent;
            case "<=":
                return pressure <= percent;
            case ">=":
                return pressure >= percent;
            case ">":
                return pressure > percent;
            case "==":
                return pressure == percent;
            case "!=":
                return pressure != percent;
        }

        return false;
    }

    public Dictionary<String, String> DetailedInfo()
    {
        Dictionary<String, String> properties = new Dictionary<String, String>();

        var statements = this.Block.DetailedInfo.Split('\n');

        for(int n = 0; n < statements.Length; n++)
        {
            var pair = statements[n].Split(':');

            properties.Add(pair[0], pair[1].Substring(1));
        }

        return properties;
    }


    public bool NameRegex(String Pattern, out List<String> Matches)
    {
        System.Text.RegularExpressions.Match m = (new System.Text.RegularExpressions.Regex(Pattern)).Match(this.Block.CustomName);

        Matches = new List<String>();

        bool success = false;
        while(m.Success)
        {
            if(m.Groups.Count > 1)
            {
                Matches.Add(m.Groups[1].Value);
            }
            success = true;

            m = m.NextMatch();
        }

        return success;
    }

    public bool NameRegex(String Pattern)
    {
        List<String> matches;

        return this.NameRegex(Pattern, out matches);
    }

    public ITerminalAction GetAction(String Name)
    {
        return this.Block.GetActionWithName(Name);
    }

    public EasyBlock ApplyAction(String Name)
    {
        ITerminalAction Action = this.GetAction(Name);

        if(Action != null)
        {
            Action.Apply(this.Block);
        }

        return this;
    }
    
    public T GetProperty<T>(String PropertyId)
    {
        return Sandbox.ModAPI.Interfaces.TerminalPropertyExtensions.GetValue<T>(this.Block, PropertyId);
    }

    public EasyBlock SetProperty<T>(String PropertyId, T value)
    {
        try
        {
            var prop = this.GetProperty<T>(PropertyId);
            Sandbox.ModAPI.Interfaces.TerminalPropertyExtensions.SetValue<T>(this.Block, PropertyId, value);
        }
        catch(Exception e)
        {

        }

        return this;
    }

    public EasyBlock SetFloatValue(String PropertyId, float value)
    {
        try
        {
            Sandbox.ModAPI.Interfaces.TerminalPropertyExtensions.SetValueFloat(this.Block, PropertyId, value);
        }
        catch(Exception e)
        {

        }

        return this;
    }

    public EasyBlock On()
    {
        this.ApplyAction("OnOff_On");

        return this;
    }

    public EasyBlock Off()
    {
        this.ApplyAction("OnOff_Off");

        return this;
    }

    public EasyBlock Toggle()
    {
        if(this.Block.IsWorking)
        {
            this.Off();
        }
        else
        {
            this.On();
        }

        return this;
    }
    
    public EasyBlock Run(EasyAPI api, string type = "public")
    {
        var cmd = new EasyCommands(api);
        switch(type)
        {
            case "private":
                cmd.handle(this.GetCustomData());
                break;  
            default:
                cmd.handle(this.GetPublicText());                    
                break;
        }
        
        return this;
    }

    public EasyBlock RunPB(string argument = "")
    {
        IMyProgrammableBlock pb = Block as IMyProgrammableBlock;

        if(pb != null)
        {
            pb.TryRun(argument);
        }

        return this;        
    }
    
    public string GetPublicText()
    {
        string ret = "";
        
        IMyTextPanel textPanel = Block as IMyTextPanel;

        if(textPanel != null)
        {
            ret = textPanel.GetPublicText();
        }
        
        return ret;
    }
    
    public string GetCustomData()
    {
        string ret = "";
        
        IMyTextPanel textPanel = Block as IMyTextPanel;

        if(textPanel != null)
        {
            ret = textPanel.CustomData;
        }
        
        return ret;
    }
    
    public EasyBlock WritePublicTitle(string text)
    {
        IMyTextPanel textPanel = Block as IMyTextPanel;

        if(textPanel != null)
        {
            textPanel.WritePublicTitle(text);
        }

        return this;
    }

    public EasyBlock WritePublicText(string text)
    {
        IMyTextPanel textPanel = Block as IMyTextPanel;

        if(textPanel != null)
        {
            textPanel.WritePublicText(text, false);
        }

        return this;
    }

    public EasyBlock WriteCustomData(string text)
    {
        IMyTextPanel textPanel = Block as IMyTextPanel;

        if(textPanel != null)
        {
            textPanel.CustomData = text;
        }

        return this;
    }

    public EasyBlock AppendPublicText(string text)
    {
        IMyTextPanel textPanel = Block as IMyTextPanel;

        if(textPanel != null)
        {
            textPanel.WritePublicText(text, true);
        }

        return this;
    }

    public EasyBlock SetName(String Name)
    {
        this.Block.CustomName = Name;

        return this;
    }

    public List<ITerminalAction> GetActions()
    {
        List<ITerminalAction> actions = new List<ITerminalAction>();
        this.Block.GetActions(actions);
        return actions;
    }

    public List<ITerminalProperty> GetProperties()
    {
        List<ITerminalProperty> properties = new List<ITerminalProperty>();
        this.Block.GetProperties(properties);
        return properties;
    }
    
    /*** Events ***/
    
    public EasyBlock AddEvent(Func<EasyBlock, bool> evnt, Func<EasyBlock, bool> action, bool onChange = false)
    {
        EasyEvent.add(new EasyEvent(this, evnt, action, onChange));
        
        return this;
    }
    
    public EasyInventory Items(Nullable<int> fix_duplicate_name_bug = null)
    {
        List<EasyBlock> Blocks = new List<EasyBlock>();
        Blocks.Add(this);

        return new EasyInventory(Blocks);
    }

    public static bool operator ==(EasyBlock a, EasyBlock b)
    {
        return a.Block == b.Block;
    }

    public override bool Equals(object o)
    {
        return (EasyBlock)o == this;
    }
    
    public override int GetHashCode()
    {
        return Block.GetHashCode();
    }
    
    public static bool operator !=(EasyBlock a, EasyBlock b)
    {
        return a.Block != b.Block;
    }
    
    
}
// Stores all items in matched block inventories for later filtering
public class EasyInventory
{
    public List<EasyItem> Items;

    public EasyInventory(List<EasyBlock> Blocks)
    {
        this.Items = new List<EasyItem>();

        // Get contents of all inventories in list and add them to EasyItems list.
        for(int i = 0; i < Blocks.Count; i++)
        {
            EasyBlock Block = Blocks[i];

            for(int j = 0; j < Block.Block.InventoryCount; j++)
            {
                IMyInventory Inventory = Block.Block.GetInventory(j);

                List<IMyInventoryItem> Items = Inventory.GetItems();

                for(int k = 0; k < Items.Count; k++)
                {
                    this.Items.Add(new EasyItem(Block, j, Inventory, k, Items[k]));
                }
            }
        }
    }

    public EasyInventory(List<EasyItem> Items)
    {
        this.Items = Items;
    }

    public EasyInventory OfType(String SubTypeId)
    {
        List<EasyItem> FilteredItems = new List<EasyItem>();

        for(int i = 0; i < this.Items.Count; i++)
        {
            if(this.Items[i].Type() == SubTypeId)
            {
                FilteredItems.Add(this.Items[i]);
            }
        }

        return new EasyInventory(FilteredItems);
    }

    public EasyInventory InInventory(int Index)
    {
        List<EasyItem> FilteredItems = new List<EasyItem>();

        for(int i = 0; i < this.Items.Count; i++)
        {
            if(this.Items[i].InventoryIndex == Index)
            {
                FilteredItems.Add(this.Items[i]);
            }
        }

        return new EasyInventory(FilteredItems);
    }

    public VRage.MyFixedPoint Count()
    {
        VRage.MyFixedPoint Total = 0;

        for(int i = 0; i < Items.Count; i++)
        {
            Total += Items[i].Amount();
        }

        return Total;
    }

    public EasyInventory First()
    {
        List<EasyItem> FilteredItems = new List<EasyItem>();

        if(this.Items.Count > 0)
        {
            FilteredItems.Add(this.Items[0]);
        }

        return new EasyInventory(FilteredItems);
    }

    public void MoveTo(EasyBlocks Blocks, int Inventory = 0)
    {
        for(int i = 0; i < Items.Count; i++)
        {
            Items[i].MoveTo(Blocks, Inventory);
        }
    }
}
// This represents a single stack of items in the inventory
public struct EasyItem
{
    private EasyBlock Block;
    public int InventoryIndex;
    private IMyInventory Inventory;
    public int ItemIndex;
    private IMyInventoryItem Item;

    public EasyItem(EasyBlock Block, int InventoryIndex, IMyInventory Inventory, int ItemIndex, IMyInventoryItem Item)
    {
        this.Block = Block;
        this.InventoryIndex = InventoryIndex;
        this.Inventory = Inventory;
        this.ItemIndex = ItemIndex;
        this.Item = Item;
    }

    public String Type(int dummy = 0)
    {
        return this.Item.Content.SubtypeId.ToString();
    }

    public VRage.MyFixedPoint Amount()
    {
        return this.Item.Amount;
    }

    public void MoveTo(EasyBlocks Blocks, int Inventory = 0, int dummy = 0)
    {
        // Right now it moves them to all of them.  Todo: determine if the move was successful an exit for if it was.
        // In the future you will be able to sort EasyBlocks and use this to prioritize where the items get moved.
        for(int i = 0; i < Blocks.Count(); i++)
        {
            this.Inventory.TransferItemTo(Blocks.GetBlock(i).Block.GetInventory(Inventory), ItemIndex);
        }
    }
}
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
public struct EasyMessage
{
    public EasyBlock From;
    public String Subject;
    public String Message;
    public long Timestamp;

    // unserialize
    public EasyMessage(String serialized)
    {
        var parts = serialized.Split(':');
        if(parts.Length < 4)
        {
            throw new Exception("Error unserializing message.");
        }
        int numberInGrid = Convert.ToInt32(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[0])));
        var blocks = new List<IMyTerminalBlock>();
        EasyAPI.grid.GetBlocksOfType<IMyProgrammableBlock>(blocks, delegate(IMyTerminalBlock block) {
           return (block as IMyProgrammableBlock).NumberInGrid == numberInGrid;
        });
        if(blocks.Count == 0)
        {
            throw new Exception("Message sender no longer exits!");
        }
        this.From = new EasyBlock((IMyTerminalBlock)blocks[0]);
        this.Subject = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[1]));
        this.Message = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[2]));
        this.Timestamp = Convert.ToInt64(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[3])));
    }

    public EasyMessage(EasyBlock From, String Subject, String Message)
    {
        this.From = From;
        this.Subject = Subject;
        this.Message = Message;
        this.Timestamp = DateTime.Now.Ticks;
    }

    public String Serialize()
    {
        String text = "";

        text += System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("" + From.Block.NumberInGrid));
        text += ":" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Subject));
        text += ":" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Message));
        text += ":" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("" + Timestamp));

        return text;
    }
}
/*** Commands ***/

// Base EasyCommands class
public class EasyCommands
{
    /*** Private Properties ***/
    
    private EasyAPI api; // The selected menu item (child)
    private EasyBlocks blocks;
    public Dictionary<string,string> functions = new Dictionary<string,string>();
    private int pos = 0;
    private string text = "";
    
    /*** Constructors ***/
    
    public EasyCommands(EasyAPI api)
    {
        this.api = api;
    }
    
    /*** Private Methods ***/
    
    public void handle(string text)
    {
        this.pos = 0;
        this.text = text;

        while(pos < text.Length)
        {
            doCommand();
            pos++;
        }
    }
    
    private void failure(string message)
    {
        throw new Exception("EasyCommand Error: " + message);
    }
    
    private void skipWhitespace()
    {
        while(pos < text.Length && char.IsWhiteSpace(text, pos))
        {
            pos++;
        }
    }
    
    private void skipLineComment()
    {
        pos += 2;
        while(pos < text.Length)
        {
            if(text[pos] == "\n"[0])
            {
                pos++;
                break;
            }
            pos++;
        }        
    }
    
    private void skipBlockComment()
    {
        pos += 2;
        while(pos < text.Length && pos + 1 < text.Length)
        {
            if(text[pos] == '*' && text[pos+1] == '/')
            {
                pos += 2;
                break;
            }
            pos++;
        }
    }
    
    private void skipNonCode()
    {
        while(pos < text.Length)
        {
            if(char.IsWhiteSpace(text, pos)) skipWhitespace();
            else if(pos + 1 < text.Length && text[pos] == '/')
            {
                if(text[pos+1] == '/') skipLineComment();
                else if(text[pos+1] == '*') skipBlockComment();
                else break;
            }
            else break;
        }
    }
    
    private string getIdentifier()
    {
        string identifier = "";
        while(pos < text.Length && char.IsLetterOrDigit(text[pos]))
        {
            identifier += text[pos];
            pos++;
        }
        return identifier;        
    }
    
    private string getParm()
    {
        string param = "";
        skipNonCode();
        if(pos < text.Length && text[pos] == '(')
        {
            pos++;
            skipNonCode();
            if(pos < text.Length && text[pos] == '"')
            {
                pos++;
                while(pos < text.Length)
                {
                    if(text[pos] == '"')
                    {
                        pos++;
                        break;
                    }
                    else if(text[pos] == '\\' && pos + 1 < text.Length)
                    {
                        if(text[pos + 1] == '"')
                        {
                            param += '"';                                 
                            pos++;
                        }
                        else if(text[pos + 1] == 'n')
                        {
                            param += "\n";
                            pos++;
                        }
                        else
                        {
                            param += text[pos];
                        }
                    }
                    else
                    {
                        param += text[pos];
                    }
                    pos++;
                }
            }
            skipNonCode();
            
            if(pos < text.Length && text[pos]== ')')
            {
                pos++;
            }
        }
        
        return param;
    }
    
    private string getFunction()
    {
        // Todo this is REALLY basic and it should do a lot more parsing instead of just looking for the end curly brace.  Functions will break if a curly brace appears anywhere inside them
        string code = "";
        while(pos < text.Length)
        {
            if(text[pos] == '}')
            {
              pos++;
              break;
            }
            code += text[pos];
            pos++;
        }
        return code;
    }
    
    private void require(char c)
    {
        if(pos < text.Length && text[pos] == c)
        {
            pos++;
            return;
        }
        failure("Required: " + c);
    }
    
    private void doCommand()
    {
        while(pos < text.Length && text[pos] != ';')
        {
            skipNonCode();
            string command = getIdentifier();
            
            if(command == "")
            {
                return;
            }
            
            string parm = "";
            
            switch(command)
            {
                case "function":
                    skipNonCode();
                    string identifier = getIdentifier();
                    skipNonCode();
                    require('(');
                    skipNonCode();
                    require(')');
                    skipNonCode();
                    require('{');
                    skipNonCode();
                    string function = getFunction();
                    if(functions.ContainsKey(identifier))
                    {
                        failure("Function " + identifier + " already defined!");
                    }
                    functions.Add(identifier, function);
                    return;
                case "Echo":
                    parm = getParm();
                    api.Echo(parm);
                    break;
                case "Blocks":
                    api.Refresh();
                    this.blocks = api.Blocks;
                    break;
                /*** Actions ***/
                case "ApplyAction":
                    parm = getParm();
                    blocks.ApplyAction(parm);
                    break;
                case "WritePublicText":
                    parm = getParm();
                    blocks.WritePublicText(parm);
                    break;
                case "WriteCustomData":
                case "WritePrivateText":
                    parm = getParm();
                    blocks.WriteCustomData(parm);
                    break;
                case "AppendPublicText":
                    parm = getParm();
                    blocks.AppendPublicText(parm);
                    break;
                case "On":
                    parm = getParm();
                    blocks.On();
                    break;
                case "Off":
                    parm = getParm();
                    blocks.Off();
                    break;
                case "Toggle":
                    parm = getParm();
                    blocks.Toggle();
                    break;
                case "DebugDump":
                    parm = getParm();
                    blocks.DebugDump();
                    break;
                case "DebugDumpActions":
                    parm = getParm();
                    blocks.DebugDumpActions();
                    break;
                case "DebugDumpProperties":
                    parm = getParm();
                    blocks.DebugDumpProperties();
                    break;
                case "Run":
                    parm = getParm();
                    blocks.Run(api, parm);
                    break;
                case "RunPB":
                    parm = getParm();
                    blocks.RunPB(parm);
                    break;

                /*** Filters ***/
                case "Named":
                    parm = getParm();
                    blocks = blocks.Named(parm);
                    break;
                case "NamedLike":
                    parm = getParm();
                    blocks = blocks.NamedLike(parm);
                    break;
                case "NotNamed":
                    parm = getParm();
                    blocks = blocks.NotNamed(parm);
                    break;
                case "NotNamedLike":
                    parm = getParm();
                    blocks = blocks.NotNamedLike(parm);
                    break;
                case "InGroupsNamed":
                    parm = getParm();
                    blocks = blocks.InGroupsNamed(parm);
                    break;
                case "InGroupsNamedLike":
                    parm = getParm();
                    blocks = blocks.InGroupsNamedLike(parm);
                    break;
                case "InGroupsNotNamed":
                    parm = getParm();
                    blocks = blocks.InGroupsNotNamed(parm);
                    break;
                case "InGroupsNotNamedLike":
                    parm = getParm();
                    blocks = blocks.InGroupsNotNamedLike(parm);
                    break;
                case "OfType":
                    parm = getParm();
                    blocks = blocks.OfType(parm);
                    break;
                case "OfTypeLike":
                    parm = getParm();
                    blocks = blocks.OfTypeLike(parm);
                    break;
                case "NotOfType":
                    parm = getParm();
                    blocks = blocks.NotOfType(parm);
                    break;
                case "NotOfTypeLike":
                    parm = getParm();
                    blocks = blocks.OfTypeLike(parm);
                    break;
                default:
                    failure("Invalid command: '" + command + "'");
                    break;
            }

            skipNonCode();
            
            if(pos < text.Length && text[pos] == '.') pos++;
        }
    }
}
public class EasyEvent
{ 
    private Func<EasyBlock,bool> op; // The comparison function 
 
    private EasyBlock block; // Object to pass through to the callback when the event is triggered 
 
    private Func<EasyBlock,bool> callback; // What to call when the event occurs 
 
    private bool last = false;
    
    private bool onChange = false;
 
    public EasyEvent(EasyBlock block, Func<EasyBlock,bool> op, Func<EasyBlock,bool> callback, bool onChange = false)
    { 
        this.op = op; 
        this.callback = callback; 
        this.block = block; 
        this.onChange = onChange;
    } 
 
    public bool process() 
    {
        bool result = (this.op)(this.block);
        
        if(result && (!onChange || !last))
        {
            last = result;
            return (this.callback)(this.block);
        } 
 
        last = result;
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
public class EasyUtils {
    public const int LOG_MAX_ECHO_LENGTH_CHARS = 8000; // Mirrored value from MyProgrammableBlock.cs
    public const int LOG_MAX_LCD_LENGTH_CHARS = 4200; // Mirrored value from MyTextPanel.cs
    public static StringBuilder LogBuffer;
    public static void Log(string logMessage, Action<string> echo = null, IMyProgrammableBlock me = null, string label = null, IMyTextPanel mirrorLcd = null, bool truncateForLcd = true)
    {
        String output = "";
        if(echo == null) {
            output = "\n";
            output += logMessage;
            throw new Exception(output);
        }
        if(LogBuffer == null) {
            LogBuffer = new StringBuilder();
        }
        if(label != null) {
            logMessage = label+": "+logMessage;
        }
        if(mirrorLcd != null) {
            string currentlyMirrored = mirrorLcd.GetPublicText();
            if(truncateForLcd && LogBuffer.Length + logMessage.Length > LOG_MAX_LCD_LENGTH_CHARS) {
                StringBuilder lcdBuffer = new StringBuilder(LogBuffer.ToString());
                int charAmountToOffset = fullLineCharsExceeding(lcdBuffer, logMessage.Length, LogBuffer.Length - (LOG_MAX_LCD_LENGTH_CHARS - logMessage.Length));
                lcdBuffer.Remove(0, LogBuffer.Length - LOG_MAX_LCD_LENGTH_CHARS + charAmountToOffset - 2);
                lcdBuffer.AppendLine();
                lcdBuffer.Append(logMessage);
                mirrorLcd.WritePublicText(lcdBuffer.ToString(), false);
            } else {
                string potentialNewLine = (currentlyMirrored.Length > 0)? "\n" : "";
                mirrorLcd.WritePublicText(potentialNewLine+logMessage, true);
            }
        }
        if(LogBuffer.Length + logMessage.Length * 2 > LOG_MAX_ECHO_LENGTH_CHARS) {
            int charAmountToRemove = fullLineCharsExceeding(LogBuffer, logMessage.Length);
            LogBuffer.Remove(0, charAmountToRemove);
            LogBuffer.Append(output);
        }
        if(LogBuffer.Length > 0) {
            LogBuffer.AppendLine();
        }
        LogBuffer.Append(logMessage);
        echo(LogBuffer.ToString());
    }
    public static int fullLineCharsExceeding(StringBuilder sb, int maxLength, int offset = 0) {
        int runningCount = 0;
        for(int i=offset; i<sb.Length; i++) {
            runningCount++;
            if(sb[i] == '\n') {
                if(runningCount > maxLength) {
                    break;
                }
            }
        }
        return runningCount;
    }
    public static void ClearLogBuffer() {
        LogBuffer.Clear();
    }

    //because "System.array does not contain a definition for .Max()"
    public static double Max(double[] values) {
        double runningMax = values[0];
        for(int i=1; i<values.Length; i++) {
            runningMax = Math.Max(runningMax, values[i]);
        }
        return runningMax;
    }

    //because "System.array does not contain a definition for .Min()"
    public static double Min(double[] values) {
        double runningMin = values[0];
        for(int i=1; i<values.Length; i++) {
            runningMin = Math.Min(runningMin, values[i]);
        }
        return runningMin;
    }
}
public class EasyLCD
{
    public char[] buffer;
    IMyTextPanel screen;
    EasyBlock block;

    public int width;
    public int height;

    public int xDisplays = 0;
    public int yDisplays = 0;

    private int wPanel = 36;
    private int hPanel = 18;

    Single fontSize;

    public EasyLCD(EasyBlocks block, double scale = 1.0)
    {
        this.block = block.GetBlock(0);
        if(this.block.Type() == "Wide LCD panel") this.wPanel = 72;

        this.screen = (IMyTextPanel)(block.GetBlock(0).Block);
        this.fontSize = block.GetProperty<Single>("FontSize");

        this.width = (int)((double)this.wPanel / this.fontSize);
        this.height = (int)((double)this.hPanel / this.fontSize);
        this.buffer = new char[this.width * this.height];
        this.clear();
        this.update();
    }

    public void SetText(String text, bool append = false)
    {
        this.screen.WritePublicText(text, append);
    }

    public void plot(EasyBlocks blocks, double x, double y, double scale = 1.0, char brush = 'o', bool showBounds = true, char boundingBrush = '?')
    {
        VRageMath.Vector3D max = new Vector3D(this.screen.CubeGrid.Max);
        VRageMath.Vector3D min = new Vector3D(this.screen.CubeGrid.Min);
        VRageMath.Vector3D size = new Vector3D(max - min);

        int width = (int)size.GetDim(0);
        int height = (int)size.GetDim(1);
        int depth = (int)size.GetDim(2);

        int minX = (int)min.GetDim(0);
        int minY = (int)min.GetDim(1);
        int minZ = (int)min.GetDim(2);

        int maxX = (int)max.GetDim(0);
        int maxY = (int)max.GetDim(1);
        int maxZ = (int)max.GetDim(2);

        double s = (double)depth + 0.01;
        if(width > depth)
        {
            s = (double)width + 0.01;
        }

        if(showBounds)
        {
            box(x + -(((0 - (width / 2.0)) / s) * scale),
                y + -(((0 - (depth / 2.0)) / s) * scale),
                x + -(((maxX - minX - (width / 2.0)) / s) * scale),
                y + -(((maxZ - minZ - (depth / 2.0)) / s) * scale), boundingBrush);
        }

        for(int n = 0; n < blocks.Count(); n++)
        {
            var block = blocks.GetBlock(n);

            Vector3D pos = new Vector3D(block.Block.Position);

            pset(x + -((((double)(pos.GetDim(0) - minX - (width / 2.0)) / s)) * scale),
                 y + -((((double)(pos.GetDim(2) - minZ - (depth / 2.0)) / s)) * scale), brush);
        }
    }

    // draw a pixel to the buffer
    public void pset(double x, double y, char brush = 'o')
    {
        if(x >= 0 && x < 1 && y >= 0 && y < 1)
        {
            this.buffer[this.linear(x, y)] = brush;
        }
    }

    private void pset(int x, int y, char brush = '0')
    {
        if(x >= 0 && x < this.width && y >= 0 && y < this.height)
        {
            this.buffer[x + (y * this.width)] = brush;
        }
    }

    public void text(double x, double y, String text)
    {
        int xx = (int)(x * (this.width - 1));
        int yy = (int)(y * (this.height - 1));

        for(int xs = 0; xs < text.Length; xs++)
        {
            pset(xx + xs, yy, text[xs]);
        }
    }

    // clear the buffer
    public void clear(char brush = ' ')
    {
        for(int n = 0; n < this.buffer.Length; n++)
        {
            this.buffer[n] = brush;
        }
    }

    // Transfer buffer contents to the lcd
    public void update()
    {
        String s = "";
        String space = "";

        //this.screen.WritePublicText(clearBuf);

        for(int y = 0; y < this.height; y++)
        {
            space = "";
            for(int x = 0; x < this.width; x++)
            {
                if(buffer[x + (y * this.width)] == ' ')
                {
                    space += "  ";
                }
                else
                {
                    s += space + buffer[x + (y * this.width)];
                    space = "";
                }
            }
            s += "\n";
        }

        this.screen.WritePublicText(s);
    }

    private int linear(double x, double y)
    {
        int xx = (int)(x * (this.width - 1));
        int yy = (int)(y * (this.height - 1));
        return xx + yy * this.width;
    }

    public void line(double xx0, double yy0, double xx1, double yy1, char brush = 'o')
    {
        int x0 = (int)Math.Floor(xx0 * (this.width));
        int y0 = (int)Math.Floor(yy0 * (this.height));
        int x1 = (int)Math.Floor(xx1 * (this.width));
        int y1 = (int)Math.Floor(yy1 * (this.height));

        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            int tmp = x0;
            x0 = y0;
            y0 = tmp;
            tmp = x1;
            x1 = y1;
            y1 = tmp;
        }

        if (x0 > x1)
        {
            int tmp = x0;
            x0 = x1;
            x1 = tmp;
            tmp = y0;
            y0 = y1;
            y1 = tmp;
        }

        int dX = (x1 - x0), dY = Math.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;

        for (int x = x0; x <= x1; ++x)
        {
            if(steep)
                pset(y, x, brush);
            else
                pset(x, y, brush);
            err = err - dY;
            if (err < 0) { y += ystep;  err += dX; }
        }
    }

    public void box(double x0, double y0, double x1, double y1, char brush = 'o')
    {
        line(x0, y0, x1, y0, brush);
        line(x1, y0, x1, y1, brush);
        line(x1, y1, x0, y1, brush);
        line(x0, y1, x0, y0, brush);
    }
}
