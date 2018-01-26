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
