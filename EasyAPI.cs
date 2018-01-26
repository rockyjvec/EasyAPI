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


/*** Ignore minified library code below ***/ 
public abstract class EasyAPI{private long d=0;private long f=0;private long g=0;public EasyBlock Self;public IMyGridTerminalSystem GridTerminalSystem;public Action<string>Echo;public TimeSpan ElapsedTime;static public IMyGridTerminalSystem grid;private Dictionary<string,List<Action>>h;private Dictionary<string,List<Action<int,string[]>>>l;private List<EasyInterval>p;private List<EasyInterval>q;private EasyCommands r;public virtual void onRunThrottled(float intervalTranspiredPercentage){}public virtual void onTickStart(){}public virtual void onTickComplete(){}public virtual bool onSingleTap(){return false;}public virtual bool onDoubleTap(){return false;}private int u=0;public EasyBlocks Blocks;public const long Microseconds=10;public const long Milliseconds=1000*Microseconds;public const long Seconds=1000*Milliseconds;public const long Minutes=60*Seconds;public const long Hours=60*Minutes;public const long Days=24*Hours;public const long Years=365*Days;public EasyAPI(IMyGridTerminalSystem grid,IMyProgrammableBlock me,Action<string>echo,TimeSpan elapsedTime,string commandArgument="EasyCommand"){this.f=this.d=DateTime.Now.Ticks;this.g=0;this.GridTerminalSystem=EasyAPI.grid=grid;this.Echo=echo;this.ElapsedTime=elapsedTime;this.h=new Dictionary<string,List<Action>>();this.l=new Dictionary<string,List<Action<int,string[]>>>();this.p=new List<EasyInterval>();this.q=new List<EasyInterval>();this.r=new EasyCommands(this);this.Self=new EasyBlock(me);this.Reset();}public void AddEvent(EasyEvent e){EasyEvent.add(e);}public void AddEvent(EasyBlock block,Func<EasyBlock,bool>evnt,Func<EasyBlock,bool>action,bool onChange=false){this.AddEvent(new EasyEvent(block,evnt,action,onChange));}public void AddEvents(EasyBlocks blocks,Func<EasyBlock,bool>evnt,Func<EasyBlock,bool>action,bool onChange=false){for(int i=0;i<blocks.Count();i++){this.AddEvent(new EasyEvent(blocks.GetBlock(i),evnt,action,onChange));}}public List<EasyMessage>GetMessages(){var mymessages=new List<EasyMessage>();var parts=this.Self.Name().Split('\0');if(parts.Length>1){for(int n=1;n<parts.Length;n++){EasyMessage m=new EasyMessage(parts[n]);mymessages.Add(m);}this.Self.SetName(parts[0]);}return mymessages;}public void ClearMessages(){var parts=this.Self.Name().Split('\0');if(parts.Length>1){this.Self.SetName(parts[0]);}}public EasyMessage ComposeMessage(String Subject,String Message){return new EasyMessage(this.Self,Subject,Message);}public void Tick(long interval=0,string argument=""){if(argument!=""){if(this.h.ContainsKey(argument)){for(int n=0;n<this.h[argument].Count;n++){this.h[argument][n]();}}else if(this.l.Count>0){int argc=0;var matches=System.Text.RegularExpressions.Regex.Matches(argument,@"(?<match>[^\s""]+)|""(?<match>[^""]*)""");string[]argv=new string[matches.Count];for(int n=0;n<matches.Count;n++){argv[n]=matches[n].Groups["match"].Value;}argc=argv.Length;if(argc>0&&this.l.ContainsKey(argv[0])){for(int n=0;n<this.l[argv[0]].Count;n++){this.l[argv[0]][n](argc,argv);}}}else if(argument.Length>12&&argument.Substring(0,12)=="EasyCommand "){this.r.handle(argument.Substring(12));}}long now=DateTime.Now.Ticks;if(this.f>this.d&&now-this.f<interval){u++;float transpiredPercentage=((float)((double)(now-this.f)/interval));onRunThrottled(transpiredPercentage);return;}if(u==1){if(onSingleTap()){return;}}else if(u>1){if(onDoubleTap()){return;}}u=0;onTickStart();long lastClock=this.f;this.f=now;this.g=this.f-lastClock;EasyEvent.handle();for(int n=0;n<this.q.Count;n++){if(this.f>=this.q[n].time){long time=this.f+this.q[n].interval-(this.f-this.q[n].time);(this.q[n].action)();this.q[n]=new EasyInterval(time,this.q[n].interval,this.q[n].action);}}for(int n=0;n<this.p.Count;n++){if(this.f>=this.p[n].time){(this.p[n].action)();p.Remove(this.p[n]);}}onTickComplete();}public long GetDelta(){return this.g;}public long GetClock(){return f;}public void On(string argument,Action callback){if(!this.h.ContainsKey(argument)){this.h.Add(argument,new List<Action>());}this.h[argument].Add(callback);}public void OnCommand(string argument,Action<int,string[]>callback){if(!this.l.ContainsKey(argument)){this.l.Add(argument,new List<Action<int,string[]>>());}this.l[argument].Add(callback);}public void At(long time,Action callback){long t=this.d+time;p.Add(new EasyInterval(t,0,callback));}public void Every(long time,Action callback){q.Add(new EasyInterval(this.f+time,time,callback));}public void In(long time,Action callback){this.At(this.f-this.d+time,callback);}public void Reset(){this.d=this.f;this.ClearMessages();this.Refresh();}public void Refresh(){List<IMyTerminalBlock>kBlocks=new List<IMyTerminalBlock>();GridTerminalSystem.GetBlocks(kBlocks);Blocks=new EasyBlocks(kBlocks);}}public class EasyBlocks{public List<EasyBlock>Blocks;public EasyBlocks(List<IMyTerminalBlock>TBlocks){this.Blocks=new List<EasyBlock>();for(int i=0;i<TBlocks.Count;i++){EasyBlock Block=new EasyBlock(TBlocks[i]);this.Blocks.Add(Block);}}public EasyBlocks(List<EasyBlock>Blocks){this.Blocks=Blocks;}public EasyBlocks(){this.Blocks=new List<EasyBlock>();}public int Count(){return this.Blocks.Count;}public EasyBlock GetBlock(int i){return this.Blocks[i];}public EasyBlocks WithInterface<T>()where T:class{List<EasyBlock>FilteredList=new List<EasyBlock>();for(int i=0;i<this.Blocks.Count;i++){T block=this.Blocks[i].Block as T;if(block!=null){FilteredList.Add(this.Blocks[i]);}}return new EasyBlocks(FilteredList);}public EasyBlocks OfType(String Type){return d("==",Type);}public EasyBlocks NotOfType(String Type){return d("!=",Type);}public EasyBlocks OfTypeLike(String Type){return d("~",Type);}public EasyBlocks NotOfTypeLike(String Type){return d("!~",Type);}public EasyBlocks OfTypeRegex(String Pattern){return d("R",Pattern);}public EasyBlocks NotOfTypeRegex(String Pattern){return d("!R",Pattern);}protected EasyBlocks d(String op,String Type){List<EasyBlock>FilteredList=new List<EasyBlock>();for(int i=0;i<this.Blocks.Count;i++){if(g(op,this.Blocks[i].Type(),Type)){FilteredList.Add(this.Blocks[i]);}}return new EasyBlocks(FilteredList);}public EasyBlocks Named(String Name){return f("==",Name);}public EasyBlocks NotNamed(String Name){return f("!=",Name);}public EasyBlocks NamedLike(String Name){return f("~",Name);}public EasyBlocks NotNamedLike(String Name){return f("!~",Name);}public EasyBlocks NamedRegex(String Pattern){return f("R",Pattern);}public EasyBlocks NotNamedRegex(String Pattern){return f("!R",Pattern);}protected EasyBlocks f(String op,String Name){List<EasyBlock>FilteredList=new List<EasyBlock>();for(int i=0;i<this.Blocks.Count;i++){if(g(op,this.Blocks[i].Name(),Name)){FilteredList.Add(this.Blocks[i]);}}return new EasyBlocks(FilteredList);}public EasyBlocks InGroupsNamed(String Group){return GroupFilter("==",Group);}public EasyBlocks InGroupsNotNamed(String Group){return GroupFilter("!=",Group);}public EasyBlocks InGroupsNamedLike(String Group){return GroupFilter("~",Group);}public EasyBlocks InGroupsNotNamedLike(String Group){return GroupFilter("!~",Group);}public EasyBlocks InGroupsNamedRegex(String Pattern){return GroupFilter("R",Pattern);}public EasyBlocks InGroupsNotNamedRegex(String Pattern){return GroupFilter("!R",Pattern);}public EasyBlocks GroupFilter(String op,String Group){List<EasyBlock>FilteredList=new List<EasyBlock>();List<IMyBlockGroup>groups=new List<IMyBlockGroup>();EasyAPI.grid.GetBlockGroups(groups);List<IMyBlockGroup>matchedGroups=new List<IMyBlockGroup>();List<IMyTerminalBlock>groupBlocks=new List<IMyTerminalBlock>();for(int n=0;n<groups.Count;n++){if(g(op,groups[n].Name,Group)){matchedGroups.Add(groups[n]);}}for(int n=0;n<matchedGroups.Count;n++){for(int i=0;i<this.Blocks.Count;i++){IMyTerminalBlock block=this.Blocks[i].Block;matchedGroups[n].GetBlocks(groupBlocks);for(int j=0;j<groupBlocks.Count;j++){if(block==groupBlocks[j]){FilteredList.Add(this.Blocks[i]);}}groupBlocks.Clear();}}return new EasyBlocks(FilteredList);}public EasyBlocks SensorsActive(bool isActive=true){List<EasyBlock>FilteredList=new List<EasyBlock>();for(int i=0;i<this.Blocks.Count;i++){if(this.Blocks[i].Type()=="Sensor"&&((IMySensorBlock)this.Blocks[i].Block).IsActive==isActive){FilteredList.Add(this.Blocks[i]);}}return new EasyBlocks(FilteredList);}public EasyBlocks RoomPressure(String op,Single percent){List<EasyBlock>FilteredList=new List<EasyBlock>();for(int i=0;i<this.Blocks.Count;i++){if(this.Blocks[i].RoomPressure(op,percent)){FilteredList.Add(this.Blocks[i]);}}return new EasyBlocks(FilteredList);}public EasyBlocks FilterBy(Func<EasyBlock,bool>action){List<EasyBlock>FilteredList=new List<EasyBlock>();for(int i=0;i<this.Blocks.Count;i++){if(action(this.Blocks[i])){FilteredList.Add(this.Blocks[i]);}}return new EasyBlocks(FilteredList);}public EasyBlocks First(){List<EasyBlock>FilteredList=new List<EasyBlock>();if(this.Blocks.Count>0){FilteredList.Add(Blocks[0]);}return new EasyBlocks(FilteredList);}public EasyBlocks Add(EasyBlock Block){this.Blocks.Add(Block);return this;}public EasyBlocks Plus(EasyBlocks Blocks){List<EasyBlock>FilteredList=new List<EasyBlock>();FilteredList.AddRange(this.Blocks);for(int i=0;i<Blocks.Count();i++){if(!FilteredList.Contains(Blocks.GetBlock(i))){FilteredList.Add(Blocks.GetBlock(i));}}return new EasyBlocks(FilteredList);}public EasyBlocks Minus(EasyBlocks Blocks){List<EasyBlock>FilteredList=new List<EasyBlock>();FilteredList.AddRange(this.Blocks);for(int i=0;i<Blocks.Count();i++){FilteredList.Remove(Blocks.GetBlock(i));}return new EasyBlocks(FilteredList);}public static EasyBlocks operator+(EasyBlocks a,EasyBlocks b){return a.Plus(b);}public static EasyBlocks operator-(EasyBlocks a,EasyBlocks b){return a.Minus(b);}public EasyBlocks Plus(EasyBlock Block){List<EasyBlock>FilteredList=new List<EasyBlock>();FilteredList.AddRange(this.Blocks);if(!FilteredList.Contains(Block)){FilteredList.Add(Block);}return new EasyBlocks(FilteredList);}public EasyBlocks Minus(EasyBlock Block){List<EasyBlock>FilteredList=new List<EasyBlock>();FilteredList.AddRange(this.Blocks);FilteredList.Remove(Block);return new EasyBlocks(FilteredList);}public static EasyBlocks operator+(EasyBlocks a,EasyBlock b){return a.Plus(b);}public static EasyBlocks operator-(EasyBlocks a,EasyBlock b){return a.Minus(b);}public EasyBlocks FindOrFail(string message){if(this.Count()==0)throw new Exception(message);return this;}public EasyBlocks Run(EasyAPI api,string type="public"){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].Run(api,type);}return this;}public EasyBlocks SendMessage(EasyMessage message){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].SendMessage(message);}return this;}public EasyBlocks ApplyAction(String Name){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].ApplyAction(Name);}return this;}public EasyBlocks SetProperty<T>(String PropertyId,T value,int bleh=0){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].SetProperty<T>(PropertyId,value);}return this;}public EasyBlocks SetFloatValue(String PropertyId,float value,int bleh=0){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].SetFloatValue(PropertyId,value);}return this;}public T GetProperty<T>(String PropertyId,int bleh=0){return this.Blocks[0].GetProperty<T>(PropertyId);}public EasyBlocks On(){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].On();}return this;}public EasyBlocks Off(){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].Off();}return this;}public EasyBlocks Toggle(){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].Toggle();}return this;}public EasyBlocks RunPB(string argument=""){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].RunPB(argument);}return this;}public EasyBlocks WritePublicText(string text){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].WritePublicText(text);}return this;}public EasyBlocks WriteCustomData(string text){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].WriteCustomData(text);}return this;}public EasyBlocks WritePublicTitle(string text){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].WritePublicTitle(text);}return this;}public EasyBlocks WritePrivateTitle(string text){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].WritePublicTitle(text);}return this;}public EasyBlocks AppendPublicText(string text){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].AppendPublicText(text);}return this;}public EasyInventory Items(){return new EasyInventory(this.Blocks);}public string DebugDump(bool throwIt=true){String output="\n";for(int i=0;i<this.Blocks.Count;i++){output+=this.Blocks[i].Type()+": "+this.Blocks[i].Name()+"\n";}if(throwIt)throw new Exception(output);else return output;}public string DebugDumpActions(bool throwIt=true){String output="\n";for(int i=0;i<this.Blocks.Count;i++){output+="[ "+this.Blocks[i].Type()+": "+this.Blocks[i].Name()+" ]\n";output+="*** ACTIONS ***\n";List<ITerminalAction>actions=this.Blocks[i].GetActions();for(int j=0;j<actions.Count;j++){output+=actions[j].Id+":"+actions[j].Name+"\n";}output+="\n\n";}if(throwIt)throw new Exception(output);else return output;}public string DebugDumpProperties(bool throwIt=true){String output="\n";for(int i=0;i<this.Blocks.Count;i++){output+="[ "+this.Blocks[i].Type()+": "+this.Blocks[i].Name()+" ]\n";output+="*** PROPERTIES ***\n";List<ITerminalProperty>properties=this.Blocks[i].GetProperties();for(int j=0;j<properties.Count;j++){output+=properties[j].TypeName+": "+properties[j].Id+"\n";}output+="\n\n";}if(throwIt)throw new Exception(output);else return output;}public EasyBlocks AddEvent(Func<EasyBlock,bool>evnt,Func<EasyBlock,bool>action,bool onChange=false){for(int i=0;i<this.Blocks.Count;i++){this.Blocks[i].AddEvent(evnt,action,onChange);}return this;}private bool g(String op,String a,String b){switch(op){case "==":return(a==b);case "!=":return(a!=b);case "~":return a.Contains(b);case "!~":return!a.Contains(b);case "R":System.Text.RegularExpressions.Match m=(new System.Text.RegularExpressions.Regex(b)).Match(a);while(m.Success){return true;}return false;case "!R":return!g("R",a,b);}return false;}}public struct EasyBlock{public IMyTerminalBlock Block;private IMySlimBlock d;public EasyBlock(IMyTerminalBlock Block){this.Block=Block;this.d=null;}public IMySlimBlock Slim(){if(this.d==null){this.d=this.Block.CubeGrid.GetCubeBlock(this.Block.Position);}return this.d;}public String Type(){return this.Block.DefinitionDisplayNameText;}public Single Damage(){return this.CurrentDamage()/this.MaxIntegrity()*(Single)100.0;}public Single CurrentDamage(){return this.Slim().CurrentDamage;}public Single MaxIntegrity(){return this.Slim().MaxIntegrity;}public bool Open(){IMyDoor door=Block as IMyDoor;if(door!=null){return door.Status==DoorStatus.Open;}return false;}public String Name(){return this.Block.CustomName;}public void SendMessage(EasyMessage message){if(Type()=="Programmable block"){SetName(Name()+"\0"+message.Serialize());}}public List<String>NameParameters(char start='[',char end=']'){List<String>matches;this.NameRegex(@"\"+start+@"(.*?)\"+end,out matches);return matches;}public bool RoomPressure(String op,Single percent){String roomPressure=DetailedInfo()["Room pressure"];Single pressure=0;if(roomPressure!="Not pressurized"){pressure=Convert.ToSingle(roomPressure.TrimEnd('%'));}switch(op){case "<":return pressure<percent;case "<=":return pressure<=percent;case ">=":return pressure>=percent;case ">":return pressure>percent;case "==":return pressure==percent;case "!=":return pressure!=percent;}return false;}public Dictionary<String,String>DetailedInfo(){Dictionary<String,String>properties=new Dictionary<String,String>();var statements=this.Block.DetailedInfo.Split('\n');for(int n=0;n<statements.Length;n++){var pair=statements[n].Split(':');properties.Add(pair[0],pair[1].Substring(1));}return properties;}public bool NameRegex(String Pattern,out List<String>Matches){System.Text.RegularExpressions.Match m=(new System.Text.RegularExpressions.Regex(Pattern)).Match(this.Block.CustomName);Matches=new List<String>();bool success=false;while(m.Success){if(m.Groups.Count>1){Matches.Add(m.Groups[1].Value);}success=true;m=m.NextMatch();}return success;}public bool NameRegex(String Pattern){List<String>matches;return this.NameRegex(Pattern,out matches);}public ITerminalAction GetAction(String Name){return this.Block.GetActionWithName(Name);}public EasyBlock ApplyAction(String Name){ITerminalAction Action=this.GetAction(Name);if(Action!=null){Action.Apply(this.Block);}return this;}public T GetProperty<T>(String PropertyId){return Sandbox.ModAPI.Interfaces.TerminalPropertyExtensions.GetValue<T>(this.Block,PropertyId);}public EasyBlock SetProperty<T>(String PropertyId,T value){try{var prop=this.GetProperty<T>(PropertyId);Sandbox.ModAPI.Interfaces.TerminalPropertyExtensions.SetValue<T>(this.Block,PropertyId,value);}catch(Exception e){}return this;}public EasyBlock SetFloatValue(String PropertyId,float value){try{Sandbox.ModAPI.Interfaces.TerminalPropertyExtensions.SetValueFloat(this.Block,PropertyId,value);}catch(Exception e){}return this;}public EasyBlock On(){this.ApplyAction("OnOff_On");return this;}public EasyBlock Off(){this.ApplyAction("OnOff_Off");return this;}public EasyBlock Toggle(){if(this.Block.IsWorking){this.Off();}else{this.On();}return this;}public EasyBlock Run(EasyAPI api,string type="public"){var cmd=new EasyCommands(api);switch(type){case "private":cmd.handle(this.GetCustomData());break;default:cmd.handle(this.GetPublicText());break;}return this;}public EasyBlock RunPB(string argument=""){IMyProgrammableBlock pb=Block as IMyProgrammableBlock;if(pb!=null){pb.TryRun(argument);}return this;}public string GetPublicText(){string ret="";IMyTextPanel textPanel=Block as IMyTextPanel;if(textPanel!=null){ret=textPanel.GetPublicText();}return ret;}public string GetCustomData(){string ret="";IMyTextPanel textPanel=Block as IMyTextPanel;if(textPanel!=null){ret=textPanel.CustomData;}return ret;}public EasyBlock WritePublicTitle(string text){IMyTextPanel textPanel=Block as IMyTextPanel;if(textPanel!=null){textPanel.WritePublicTitle(text);}return this;}public EasyBlock WritePublicText(string text){IMyTextPanel textPanel=Block as IMyTextPanel;if(textPanel!=null){textPanel.WritePublicText(text,false);}return this;}public EasyBlock WriteCustomData(string text){IMyTextPanel textPanel=Block as IMyTextPanel;if(textPanel!=null){textPanel.CustomData=text;}return this;}public EasyBlock AppendPublicText(string text){IMyTextPanel textPanel=Block as IMyTextPanel;if(textPanel!=null){textPanel.WritePublicText(text,true);}return this;}public EasyBlock SetName(String Name){this.Block.CustomName=Name;return this;}public List<ITerminalAction>GetActions(){List<ITerminalAction>actions=new List<ITerminalAction>();this.Block.GetActions(actions);return actions;}public List<ITerminalProperty>GetProperties(){List<ITerminalProperty>properties=new List<ITerminalProperty>();this.Block.GetProperties(properties);return properties;}public EasyBlock AddEvent(Func<EasyBlock,bool>evnt,Func<EasyBlock,bool>action,bool onChange=false){EasyEvent.add(new EasyEvent(this,evnt,action,onChange));return this;}public EasyInventory Items(Nullable<int>fix_duplicate_name_bug=null){List<EasyBlock>Blocks=new List<EasyBlock>();Blocks.Add(this);return new EasyInventory(Blocks);}public static bool operator==(EasyBlock a,EasyBlock b){return a.Block==b.Block;}public override bool Equals(object o){return(EasyBlock)o==this;}public override int GetHashCode(){return Block.GetHashCode();}public static bool operator!=(EasyBlock a,EasyBlock b){return a.Block!=b.Block;}}public class EasyInventory{public List<EasyItem>Items;public EasyInventory(List<EasyBlock>Blocks){this.Items=new List<EasyItem>();for(int i=0;i<Blocks.Count;i++){EasyBlock Block=Blocks[i];for(int j=0;j<Block.Block.InventoryCount;j++){IMyInventory Inventory=Block.Block.GetInventory(j);List<IMyInventoryItem>Items=Inventory.GetItems();for(int k=0;k<Items.Count;k++){this.Items.Add(new EasyItem(Block,j,Inventory,k,Items[k]));}}}}public EasyInventory(List<EasyItem>Items){this.Items=Items;}public EasyInventory OfType(String SubTypeId){List<EasyItem>FilteredItems=new List<EasyItem>();for(int i=0;i<this.Items.Count;i++){if(this.Items[i].Type()==SubTypeId){FilteredItems.Add(this.Items[i]);}}return new EasyInventory(FilteredItems);}public EasyInventory InInventory(int Index){List<EasyItem>FilteredItems=new List<EasyItem>();for(int i=0;i<this.Items.Count;i++){if(this.Items[i].InventoryIndex==Index){FilteredItems.Add(this.Items[i]);}}return new EasyInventory(FilteredItems);}public VRage.MyFixedPoint Count(){VRage.MyFixedPoint Total=0;for(int i=0;i<Items.Count;i++){Total+=Items[i].Amount();}return Total;}public EasyInventory First(){List<EasyItem>FilteredItems=new List<EasyItem>();if(this.Items.Count>0){FilteredItems.Add(this.Items[0]);}return new EasyInventory(FilteredItems);}public void MoveTo(EasyBlocks Blocks,int Inventory=0){for(int i=0;i<Items.Count;i++){Items[i].MoveTo(Blocks,Inventory);}}}public struct EasyItem{private EasyBlock d;public int InventoryIndex;private IMyInventory f;public int ItemIndex;private IMyInventoryItem g;public EasyItem(EasyBlock Block,int InventoryIndex,IMyInventory Inventory,int ItemIndex,IMyInventoryItem Item){this.d=Block;this.InventoryIndex=InventoryIndex;this.f=Inventory;this.ItemIndex=ItemIndex;this.g=Item;}public String Type(int dummy=0){return this.g.Content.SubtypeName;}public VRage.MyFixedPoint Amount(){return this.g.Amount;}public void MoveTo(EasyBlocks Blocks,int Inventory=0,int dummy=0){for(int i=0;i<Blocks.Count();i++){this.f.TransferItemTo(Blocks.GetBlock(i).Block.GetInventory(Inventory),ItemIndex);}}}public struct EasyInterval{public long interval;public long time;public Action action;public EasyInterval(long t,long i,Action a){this.time=t;this.interval=i;this.action=a;}}public struct EasyMessage{public EasyBlock From;public String Subject;public String Message;public long Timestamp;public EasyMessage(String serialized){var parts=serialized.Split(':');if(parts.Length<4){throw new Exception("Error unserializing message.");}int numberInGrid=Convert.ToInt32(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[0])));var blocks=new List<IMyTerminalBlock>();EasyAPI.grid.GetBlocksOfType<IMyProgrammableBlock>(blocks,delegate(IMyTerminalBlock block){return(block as IMyProgrammableBlock).NumberInGrid==numberInGrid;});if(blocks.Count==0){throw new Exception("Message sender no longer exits!");}this.From=new EasyBlock((IMyTerminalBlock)blocks[0]);this.Subject=System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[1]));this.Message=System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[2]));this.Timestamp=Convert.ToInt64(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(parts[3])));}public EasyMessage(EasyBlock From,String Subject,String Message){this.From=From;this.Subject=Subject;this.Message=Message;this.Timestamp=DateTime.Now.Ticks;}public String Serialize(){String text="";text+=System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(""+From.Block.NumberInGrid));text+=":"+System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Subject));text+=":"+System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Message));text+=":"+System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(""+Timestamp));return text;}}public class EasyCommands{private EasyAPI d;private EasyBlocks f;public Dictionary<string,string>functions=new Dictionary<string,string>();private int g=0;private string h="";public EasyCommands(EasyAPI api){this.d=api;}public void handle(string text){this.g=0;this.h=text;while(g<text.Length){B();g++;}}private void l(string message){throw new Exception("EasyCommand Error: "+message);}private void p(){while(g<h.Length&&char.IsWhiteSpace(h,g)){g++;}}private void q(){g+=2;while(g<h.Length){if(h[g]=="\n"[0]){g++;break;}g++;}}private void r(){g+=2;while(g<h.Length&&g+1<h.Length){if(h[g]=='*'&&h[g+1]=='/'){g+=2;break;}g++;}}private void u(){while(g<h.Length){if(char.IsWhiteSpace(h,g))p();else if(g+1<h.Length&&h[g]=='/'){if(h[g+1]=='/')q();else if(h[g+1]=='*')r();else break;}else break;}}private string v(){string identifier="";while(g<h.Length&&char.IsLetterOrDigit(h[g])){identifier+=h[g];g++;}return identifier;}private string w(){string param="";u();if(g<h.Length&&h[g]=='('){g++;u();if(g<h.Length&&h[g]=='"'){g++;while(g<h.Length){if(h[g]=='"'){g++;break;}else if(h[g]=='\\'&&g+1<h.Length){if(h[g+1]=='"'){param+='"';g++;}else if(h[g+1]=='n'){param+="\n";g++;}else{param+=h[g];}}else{param+=h[g];}g++;}}u();if(g<h.Length&&h[g]==')'){g++;}}return param;}private string z(){string code="";while(g<h.Length){if(h[g]=='}'){g++;break;}code+=h[g];g++;}return code;}private void A(char c){if(g<h.Length&&h[g]==c){g++;return;}l("Required: "+c);}private void B(){while(g<h.Length&&h[g]!=';'){u();string command=v();if(command==""){return;}string parm="";switch(command){case "function":u();string identifier=v();u();A('(');u();A(')');u();A('{');u();string function=z();if(functions.ContainsKey(identifier)){l("Function "+identifier+" already defined!");}functions.Add(identifier,function);return;case "Echo":parm=w();d.Echo(parm);break;case "Blocks":d.Refresh();this.f=d.Blocks;break;case "ApplyAction":parm=w();f.ApplyAction(parm);break;case "WritePublicText":parm=w();f.WritePublicText(parm);break;case "WriteCustomData":case "WritePrivateText":parm=w();f.WriteCustomData(parm);break;case "AppendPublicText":parm=w();f.AppendPublicText(parm);break;case "On":parm=w();f.On();break;case "Off":parm=w();f.Off();break;case "Toggle":parm=w();f.Toggle();break;case "DebugDump":parm=w();f.DebugDump();break;case "DebugDumpActions":parm=w();f.DebugDumpActions();break;case "DebugDumpProperties":parm=w();f.DebugDumpProperties();break;case "Run":parm=w();f.Run(d,parm);break;case "RunPB":parm=w();f.RunPB(parm);break;case "Named":parm=w();f=f.Named(parm);break;case "NamedLike":parm=w();f=f.NamedLike(parm);break;case "NotNamed":parm=w();f=f.NotNamed(parm);break;case "NotNamedLike":parm=w();f=f.NotNamedLike(parm);break;case "InGroupsNamed":parm=w();f=f.InGroupsNamed(parm);break;case "InGroupsNamedLike":parm=w();f=f.InGroupsNamedLike(parm);break;case "InGroupsNotNamed":parm=w();f=f.InGroupsNotNamed(parm);break;case "InGroupsNotNamedLike":parm=w();f=f.InGroupsNotNamedLike(parm);break;case "OfType":parm=w();f=f.OfType(parm);break;case "OfTypeLike":parm=w();f=f.OfTypeLike(parm);break;case "NotOfType":parm=w();f=f.NotOfType(parm);break;case "NotOfTypeLike":parm=w();f=f.OfTypeLike(parm);break;default:l("Invalid command: '"+command+"'");break;}u();if(g<h.Length&&h[g]=='.')g++;}}}public class EasyEvent{private Func<EasyBlock,bool>d;private EasyBlock f;private Func<EasyBlock,bool>g;private bool h=false;private bool l=false;public EasyEvent(EasyBlock block,Func<EasyBlock,bool>op,Func<EasyBlock,bool>callback,bool onChange=false){this.d=op;this.g=callback;this.f=block;this.l=onChange;}public bool process(){bool result=(this.d)(this.f);if(result&&(!l||!h)){h=result;return(this.g)(this.f);}h=result;return true;}private static List<EasyEvent>p=new List<EasyEvent>();public static void handle(){for(int i=0;i<p.Count;i++){if(!p[i].process()){p.Remove(p[i]);}}}public static void add(EasyEvent e){p.Add(e);}}public class EasyUtils{public const int LOG_MAX_ECHO_LENGTH_CHARS=8000;public const int LOG_MAX_LCD_LENGTH_CHARS=4200;public static StringBuilder LogBuffer;public static void Log(string logMessage,Action<string>echo=null,IMyProgrammableBlock me=null,string label=null,IMyTextPanel mirrorLcd=null,bool truncateForLcd=true){String output="";if(echo==null){output="\n";output+=logMessage;throw new Exception(output);}if(LogBuffer==null){LogBuffer=new StringBuilder();}if(label!=null){logMessage=label+": "+logMessage;}if(mirrorLcd!=null){string currentlyMirrored=mirrorLcd.GetPublicText();if(truncateForLcd&&LogBuffer.Length+logMessage.Length>LOG_MAX_LCD_LENGTH_CHARS){StringBuilder lcdBuffer=new StringBuilder(LogBuffer.ToString());int charAmountToOffset=fullLineCharsExceeding(lcdBuffer,logMessage.Length,LogBuffer.Length-(LOG_MAX_LCD_LENGTH_CHARS-logMessage.Length));lcdBuffer.Remove(0,LogBuffer.Length-LOG_MAX_LCD_LENGTH_CHARS+charAmountToOffset-2);lcdBuffer.AppendLine();lcdBuffer.Append(logMessage);mirrorLcd.WritePublicText(lcdBuffer.ToString(),false);}else{string potentialNewLine=(currentlyMirrored.Length>0)?"\n":"";mirrorLcd.WritePublicText(potentialNewLine+logMessage,true);}}if(LogBuffer.Length+logMessage.Length*2>LOG_MAX_ECHO_LENGTH_CHARS){int charAmountToRemove=fullLineCharsExceeding(LogBuffer,logMessage.Length);LogBuffer.Remove(0,charAmountToRemove);LogBuffer.Append(output);}if(LogBuffer.Length>0){LogBuffer.AppendLine();}LogBuffer.Append(logMessage);echo(LogBuffer.ToString());}public static int fullLineCharsExceeding(StringBuilder sb,int maxLength,int offset=0){int runningCount=0;for(int i=offset;i<sb.Length;i++){runningCount++;if(sb[i]=='\n'){if(runningCount>maxLength){break;}}}return runningCount;}public static void ClearLogBuffer(){LogBuffer.Clear();}public static double Max(double[]values){double runningMax=values[0];for(int i=1;i<values.Length;i++){runningMax=Math.Max(runningMax,values[i]);}return runningMax;}public static double Min(double[]values){double runningMin=values[0];for(int i=1;i<values.Length;i++){runningMin=Math.Min(runningMin,values[i]);}return runningMin;}}public class EasyLCD{public char[]buffer;IMyTextPanel d;EasyBlock f;public int width;public int height;public int xDisplays=0;public int yDisplays=0;private int g=36;private int h=18;Single l;public EasyLCD(EasyBlocks block,double scale=1.0){this.f=block.GetBlock(0);if(this.f.Type()=="Wide LCD panel")this.g=72;this.d=(IMyTextPanel)(block.GetBlock(0).Block);this.l=block.GetProperty<Single>("FontSize");this.width=(int)((double)this.g/this.l);this.height=(int)((double)this.h/this.l);this.buffer=new char[this.width*this.height];this.clear();this.update();}public void SetText(String text,bool append=false){this.d.WritePublicText(text,append);}public void plot(EasyBlocks blocks,double x,double y,double scale=1.0,char brush='o',bool showBounds=true,char boundingBrush='?'){VRageMath.Vector3D max=new Vector3D(this.d.CubeGrid.Max);VRageMath.Vector3D min=new Vector3D(this.d.CubeGrid.Min);VRageMath.Vector3D size=new Vector3D(max-min);int width=(int)size.GetDim(0);int height=(int)size.GetDim(1);int depth=(int)size.GetDim(2);int minX=(int)min.GetDim(0);int minY=(int)min.GetDim(1);int minZ=(int)min.GetDim(2);int maxX=(int)max.GetDim(0);int maxY=(int)max.GetDim(1);int maxZ=(int)max.GetDim(2);double s=(double)depth+0.01;if(width>depth){s=(double)width+0.01;}if(showBounds){box(x+-(((0-(width/2.0))/s)*scale),y+-(((0-(depth/2.0))/s)*scale),x+-(((maxX-minX-(width/2.0))/s)*scale),y+-(((maxZ-minZ-(depth/2.0))/s)*scale),boundingBrush);}for(int n=0;n<blocks.Count();n++){var block=blocks.GetBlock(n);Vector3D pos=new Vector3D(block.Block.Position);pset(x+-((((double)(pos.GetDim(0)-minX-(width/2.0))/s))*scale),y+-((((double)(pos.GetDim(2)-minZ-(depth/2.0))/s))*scale),brush);}}public void pset(double x,double y,char brush='o'){if(x>=0&&x<1&&y>=0&&y<1){this.buffer[this.q(x,y)]=brush;}}private void p(int x,int y,char brush='0'){if(x>=0&&x<this.width&&y>=0&&y<this.height){this.buffer[x+(y*this.width)]=brush;}}public void text(double x,double y,String text){int xx=(int)(x*(this.width-1));int yy=(int)(y*(this.height-1));for(int xs=0;xs<text.Length;xs++){p(xx+xs,yy,text[xs]);}}public void clear(char brush=' '){for(int n=0;n<this.buffer.Length;n++){this.buffer[n]=brush;}}public void update(){String s="";String space="";for(int y=0;y<this.height;y++){space="";for(int x=0;x<this.width;x++){if(buffer[x+(y*this.width)]==' '){space+="  ";}else{s+=space+buffer[x+(y*this.width)];space="";}}s+="\n";}this.d.WritePublicText(s);}private int q(double x,double y){int xx=(int)(x*(this.width-1));int yy=(int)(y*(this.height-1));return xx+yy*this.width;}public void line(double xx0,double yy0,double xx1,double yy1,char brush='o'){int x0=(int)Math.Floor(xx0*(this.width));int y0=(int)Math.Floor(yy0*(this.height));int x1=(int)Math.Floor(xx1*(this.width));int y1=(int)Math.Floor(yy1*(this.height));bool steep=Math.Abs(y1-y0)>Math.Abs(x1-x0);if(steep){int tmp=x0;x0=y0;y0=tmp;tmp=x1;x1=y1;y1=tmp;}if(x0>x1){int tmp=x0;x0=x1;x1=tmp;tmp=y0;y0=y1;y1=tmp;}int dX=(x1-x0),dY=Math.Abs(y1-y0),err=(dX/2),ystep=(y0<y1?1:-1),y=y0;for(int x=x0;x<=x1;++x){if(steep)p(y,x,brush);else p(x,y,brush);err=err-dY;if(err<0){y+=ystep;err+=dX;}}}public void box(double x0,double y0,double x1,double y1,char brush='o'){line(x0,y0,x1,y0,brush);line(x1,y0,x1,y1,brush);line(x1,y1,x0,y1,brush);line(x0,y1,x0,y0,brush);}}
