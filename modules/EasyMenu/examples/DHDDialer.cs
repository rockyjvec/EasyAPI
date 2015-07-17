/************************************************************************************ 
DHD - Dialing menu for Stargate Mod
*************************************************************************************/ 

const string lcdName = "DHD Lcd"; // LCD to display the menu on
const string addressBookName = "DHD Lcd"; // defaults to private text of LCD, you could use a text-panel though, if you prefer
const string dhdType = "DHD"; // defaults to any type with DHD in the name.
const int lcdLines = 11; // defaults to 11 for 1.5 font size.  Should be set to the number of lines visible on the LCD

public class Example : EasyAPI 
{
    // Cache all the different blocks
    
    EasyLCD lcd;
    EasyMenu menu;    
    EasyBlock dhd;
    EasyBlock addressBook;
    
    public Example(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime) : base(grid, me, echo, elapsedTime) 
    {
        this.lcd = new EasyLCD(Blocks.Named(lcdName).FindOrFail("Missing LCD.  Check lcdName: " + lcdName));
        this.dhd = Blocks.OfTypeLike(dhdType).FindOrFail("Missing DHD. Check dhdType: " + dhdType).GetBlock(0);
        this.addressBook = Blocks.Named(addressBookName).FindOrFail("Missing DHD Address Book.  Check addressBookName: " + addressBookName).GetBlock(0);
        
        // Create menu
        this.menu = new EasyMenu("DHD Controller", new [] {
            new EasyMenuItem("Dial Gate", delegate(EasyMenuItem dialGateItem) {
                dialGateItem.children.Clear();

                string[] addresses = ((IMyTextPanel)(this.addressBook.Block)).GetPrivateText().Split('\n');
                
                for(int n = 0; n < addresses.Length; n++)
                {
                    string address = addresses[n];
                    
                    if(address != "")
                    {
                        dialGateItem.children.Add(new EasyMenuItem(address, dialGate, gateStatus));
                    }
                }
                
                dialGateItem.children.Sort();
                
                return true;
            }),
            new EasyMenuItem("Jump", jump, jumpStatus),
        });
        
        // Handle Menu Commands
        On("Up", delegate() {
            this.menu.Up();
            refresh();           
        });

        On("Down", delegate() {
            this.menu.Down();
            refresh();           
        });

        On("Choose", delegate() {
            this.menu.Choose();
            refresh();           
        });
        
        On("Back", delegate() {
            this.menu.Back();
            refresh();           
        });

        refresh();           
    } 
    
    public bool dialGate(EasyMenuItem item)
    {
        this.dhd.SetName(item.Text);
        
        return false;
    }
    
    public string gateStatus(EasyMenuItem item)
    {
        if(dhd.Name() == item.Text)
        {
            return item.Text + " (Dialed)";
        }
        
        return item.Text;
    }

    public bool jump(EasyMenuItem item)
    {
        dhd.ApplyAction("Open");
        
        return false;
    }
    
    public string jumpStatus(EasyMenuItem item)
    {
        if(dhd.Name() != "")
        {
            return item.Text + " to \"" + dhd.Name() + "\"";
        }
        
        return item.Text;
    }

    public void refresh()
    {
        lcd.clear();
        lcd.update();
        lcd.SetText(menu.Draw(40, lcdLines));
    }    
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

    protected IMyGridTerminalSystem GridTerminalSystem;
    protected Action<string> Echo;
    protected TimeSpan ElapsedTime;
    static public IMyGridTerminalSystem grid;

    /*** Events ***/
    private Dictionary<string,List<Action>> ArgumentActions;
    private List<EasyInterval> Schedule;
    private List<EasyInterval> Intervals;
    private List<IEasyEvent> Events;

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
    public EasyAPI(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime)
    {
        this.clock = this.start = DateTime.Now.Ticks;
        this.delta = 0;

        this.GridTerminalSystem = EasyAPI.grid = grid;
        this.Echo = echo;
        this.ElapsedTime = elapsedTime;
        this.ArgumentActions = new Dictionary<string,List<Action>>();
        this.Events = new List<IEasyEvent>();
        this.Schedule = new List<EasyInterval>();
        this.Intervals = new List<EasyInterval>();

        // Get the Programmable Block that is running this script (thanks to LordDevious and LukeStrike)
        this.Self = new EasyBlock(me);

        this.Reset();
    }

    private void handleEvents()
    {
        for(int n = 0; n < Events.Count; n++)
        {
            if(!Events[n].handle())
            {
                Events.Remove(Events[n]);
            }
        }
    }

    public void AddEvent(IEasyEvent e)
    {
        Events.Add(e);
    }

    public void AddEvent(EasyBlock block, Func<EasyBlock, bool> evnt, Func<EasyBlock, bool> action)
    {
        this.AddEvent(new EasyEvent<EasyBlock>(block, evnt, action));
    }

    public void AddEvents(EasyBlocks blocks, Func<EasyBlock, bool> evnt, Func<EasyBlock, bool> action)
    {
        for(int i = 0; i < blocks.Count(); i++)
        {
            this.AddEvent(new EasyEvent<EasyBlock>(blocks.GetBlock(i), evnt, action));
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

        /*** Handle Arguments ***/

        if(this.ArgumentActions.ContainsKey(argument))
        {
            for(int n = 0; n < this.ArgumentActions[argument].Count; n++)
            {
                this.ArgumentActions[argument][n]();
            }
        }

        /*** Handle Events ***/
        handleEvents();

        /*** Handle Intervals ***/
        for(int n = 0; n < this.Intervals.Count; n++)
        {
            if(this.clock >= this.Intervals[n].time)
            {
                long time = this.clock + this.Intervals[n].interval - (this.clock - this.Intervals[n].time);

                this.Intervals[n].action();
                this.Intervals[n] = new EasyInterval(time, this.Intervals[n].interval, this.Intervals[n].action); // reset time interval
            }
        }

        /*** Handle Schedule ***/
        for(int n = 0; n < this.Schedule.Count; n++)
        {
            if(this.clock >= this.Schedule[n].time)
            {
                this.Schedule[n].action();
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
    private List<EasyBlock> Blocks;

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

                for(int j = 0; j < matchedGroups[n].Blocks.Count; j++)
                {
                    if(block == matchedGroups[n].Blocks[j])
                    {
                        FilteredList.Add(this.Blocks[i]);
                    }
                }
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

    /*** Operations ***/

    public EasyBlocks FindOrFail(string message)
    {
        if(this.Count() == 0) throw new Exception(message);

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
            return door.Open;
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

    public EasyBlock SetName(String Name)
    {
        this.Block.SetCustomName(Name);

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

            for(int j = 0; j < ((IMyInventoryOwner)Block.Block).InventoryCount; j++)
            {
                IMyInventory Inventory = ((IMyInventoryOwner)Block.Block).GetInventory(j);

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
        return this.Item.Content.SubtypeName;
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
            this.Inventory.TransferItemTo(((IMyInventoryOwner)Blocks.GetBlock(i).Block).GetInventory(Inventory), ItemIndex);
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
abstract public class IEasyEvent
{
    public abstract bool handle();
}
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
static public bool EasyCompare(String op, String a, String b)
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
 
/*** Utilities ***/ 
 
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


/*** Menus ***/

// Base EasyMenu class
public class EasyMenu
{
    /*** Private Properties ***/
    
    private EasyMenuItem selectedItem; // The selected menu item (child)
    Stack<EasyMenuItem> path;
    
    /*** Constructors ***/
    
    public EasyMenu(string text, EasyMenuItem[] children = null)
    {
        this.path = new Stack<EasyMenuItem>();
        
        this.path.Push(new EasyMenuItem(text, children));
        
        if(this.path.Peek().children.Count > 0)
        {
            this.selectedItem = this.path.Peek().children[0];
        }
    }
    
    /*** Public API ***/
    
    // Move cursor up
    public void Up()
    {
        int location = this.path.Peek().children.IndexOf(this.selectedItem) - 1;
        
        if(location >= 0)
        {
            this.selectedItem = this.path.Peek().children[location];
        }
    }
    
    // Move cursor down
    public void Down()
    {
        int location = this.path.Peek().children.IndexOf(this.selectedItem) + 1;
        
        if(location < this.path.Peek().children.Count)
        {
            this.selectedItem = this.path.Peek().children[location];
        }        
    }
    
    // Choose action
    public void Choose()
    {
        if(this.selectedItem.doAction())
        {
            if(this.selectedItem.children.Count > 0) // go to sub menu if it exists
            {
                this.path.Push(this.selectedItem);
                this.selectedItem = this.path.Peek().children[0];                            
            }
        }
    }
    
    // Back action
    public void Back()
    {
        if(this.path.Count > 1)
        {
            this.path.Pop();
        
            this.selectedItem = this.path.Peek().children[0];            
        }
    }
    
    // Return string of rendered menu
    public string Draw(int textWidth = 40, int textHeight = 7)
    {
        // long clock = DateTime.Now.Ticks; // used to scroll menu text if it is too wide
        string output = "";
        
        var currentItem = this.path.Peek();
        
        if(currentItem == null)
        {
            throw new Exception("Menu: currentItem is null");
        }
            
        output += this.breadcrumbs() + "\n\n";
        
        
        int selected = currentItem.children.IndexOf(this.selectedItem);
        
        int start = 0;
        int length = currentItem.children.Count;
        
        
        if(selected > (textHeight - 2) / 2)
        {
            start = selected - (textHeight - 2) / 2;
        }

        if(length > textHeight - 2)
        {
            length = textHeight - 2;
        }
               
        if(start + length > currentItem.children.Count)
        {
            start = currentItem.children.Count - (textHeight - 2);
            length = textHeight - 2;
        }

        if(start < 0) start = 0;
        
        for(int n = start; (n < (start + length) && n < currentItem.children.Count); n++)
        {
            EasyMenuItem child = currentItem.children[n];
            if(child == this.selectedItem)
            {
                // Todo, if text is too wide, have it scroll horizontally 
                output += "> " + child.GetText() + " <\n";
            }
            else
            {
                output += child.GetText() + "\n";
            }            
        }
        
        return output;
    }
    
    /*** Private Methods ***/
    
    // Returns breadcrumbs for menu title
    private string breadcrumbs()
    {
        if(this.path.Count == 1)
        {
            return this.path.Peek().GetText();
        }
        else
        {
            var item = this.path.Pop();
            
            var s = this.breadcrumbs() + " » " + item.GetText();
            
            this.path.Push(item);
            
            return s;
        }
    }
}

// Holds menu item info
public class EasyMenuItemClass
{
    /*** Private Properties ***/
    
    private Func<EasyMenuItem, bool> chooseAction; // Function to run when item is chosen
    private Func<EasyMenuItem, string> textAction; // Function to update the text for dynamic menu text

    /*** Public Properties ***/
    
    public long uid; // Unique id of this item for comparisons 
    public string Text; // The text that shows up in the menu for this item
    public List<EasyMenuItem> children; // The children items of this item
    
    public static long currentUid = 0; // Used to generate unique ids for each item in a menu
    
    /*** Constructors ***/

    // Standard constructor
    public EasyMenuItemClass(string text, EasyMenuItem[] children = null, Func<EasyMenuItem, bool> chooseAction = null, Func<EasyMenuItem, string> textAction = null)
    {
        // increment unique id
        EasyMenuItemClass.currentUid++;
        
        this.Text = text;
        this.chooseAction = chooseAction;
        this.textAction = textAction;
        this.uid = EasyMenuItemClass.currentUid;

        // Initialize children
        if(children == null)
        {
            this.children = new List<EasyMenuItem>();
        }
        else
        {
            this.children = new List<EasyMenuItem>(children);        
        }

        // If no text action is specified, use the default one
        if(this.textAction == null)
        {
            this.textAction = get_text;    
        }
    }
    
    /*** Methods ***/

    // Default text action which just returns the static text
    private string get_text(EasyMenuItem item)
    {
        return this.Text;
    }
    
    // Get menu text
    public string GetText(EasyMenuItem item)
    {
        return this.textAction(item);
    }
    
    // True means go into sub menu if it exists, false means stay where you are
    public bool doAction(EasyMenuItem item)
    {
        if(this.chooseAction == null)
        {
            return true;
        }
        else
        {
            return this.chooseAction(item);
        }
    }
    
    // Set the static text
    public void SetText(string text)
    {
        this.Text = text;
    }
    
    // Set the dynamic text action
    public void SetTextAction(Func<EasyMenuItem, string> textAction)
    {
        this.textAction = textAction;
    }

}

// This is to work around an issue in the PB where you can't use a user defined class in a List.  It basically just maps the public methods/properties onto the EasyMenuItemClass class
public struct EasyMenuItem : IComparable<EasyMenuItem>
{
    /*** Private Properties ***/
    
    private EasyMenuItemClass item;
    
    /*** Public Properties ***/
    
    public long uid { get {return this.item.uid;} }
    public string Text { get {return this.item.Text;} } // The text that shows up in the menu for this item
    public List<EasyMenuItem> children { get {return this.item.children;} set {this.item.children = value;} } // The children items of this item

    /*** Constructors ***/
    
    public EasyMenuItem(string text, EasyMenuItem[] children = null, Func<EasyMenuItem, bool> chooseAction = null, Func<EasyMenuItem, string> textAction = null)
    {
        this.item = new EasyMenuItemClass(text, children, chooseAction, textAction);
    }
    
    public EasyMenuItem(string text, Func<EasyMenuItem, bool> chooseAction, Func<EasyMenuItem, string> textAction = null) : this(text, null, chooseAction, textAction) {}
    public EasyMenuItem(Func<EasyMenuItem, string> textAction, Func<EasyMenuItem, bool> chooseAction = null) : this("", chooseAction, textAction) {}
    
    /*** Methods ***/

    public string GetText() {return this.item.GetText(this);}
    public bool doAction() {return this.item.doAction(this);}
    public void SetText(string text) {this.item.SetText(text);}
    public void SetTextAction(Func<EasyMenuItem, string> textAction) {this.item.SetTextAction(textAction);}
    
    /*** Operators ***/
    
    public static bool operator ==(EasyMenuItem x, EasyMenuItem y) {return x.uid == y.uid;}    
    public static bool operator !=(EasyMenuItem x, EasyMenuItem y) {return !(x == y);}    
    public int CompareTo(EasyMenuItem otherItem) {return this.GetText().CompareTo(otherItem.GetText());}
}
