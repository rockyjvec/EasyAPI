/************************************************************************************ 
CAL Menu - powered by EasyAPI and EasyMenu
*************************************************************************************/ 
 
static string calBlockName = "CAL"; // Name of CAL programmable block which contains the CAL script (No timer needed)
static string lcdBlockName = "MenuLCD [LCD]"; // Name of the LCD where you want the menu to display (with standard [LCD] tag for CAL) (Display ON)
static string menuTitle = "CAL Menu"; // The menu title which shows up at the top 
 
/*** Modify menu items here - first column is menu title, second is CAL command ***/

static Dictionary<string, string> menuItems = new Dictionary<string, string> 
{ 
    {"Power Summary", "PowerSummary;PowerTime"}, 
    {"Cargo", "CargoAll"}, 
    {"Inventory", "Inventory"}, 
    {"Damaged Blocks", "Damage"}, 
    {"Oxygen", "Oxygen"}, 
}; 
 
















public class Example : EasyAPI  
{ 
    // Cache all the different blocks 
     
    EasyBlock lcd; 
    EasyBlock cal; 
    EasyMenu menu; 
     
    bool inCAL = false; 
    string calCommand = ""; 
 
    public Example(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime) : base(grid, me, echo, elapsedTime)  
    {
        var children = new List<EasyMenuItem>();

        children.Clear();  
                  
        foreach(var title in menuItems.Keys)  
        {  
            children.Add(new EasyMenuItem(title, delegate(EasyMenuItem menuItem) {  
               
                menuItem.children.Clear();  
                menuItem.children.Add(new EasyMenuItem("hidden"));  
                  
                this.inCAL = true;  
                  
                this.calCommand = menuItems[menuItem.GetText()];  
                  
                return true;  
            }));  
        }
        
        // Create menu 
        this.menu = new EasyMenu(menuTitle, children.ToArray());
         
        this.lcd = Blocks.Named(lcdBlockName).FindOrFail("Menu LCD not found!").GetBlock(0); 
        this.cal = Blocks.Named(calBlockName).FindOrFail("CAL Programmable block not found!").GetBlock(0); 
         
        lcd.WritePublicTitle(""); 
         
        // Handle Arguments 
        On("Up", delegate() { 
            this.menu.Up(); 
            doUpdates();            
        }); 
 
        On("Down", delegate() { 
            this.menu.Down(); 
            doUpdates();            
        }); 
 
        On("Choose", delegate() { 
            this.menu.Choose(); 
            doUpdates();            
        }); 
         
        On("Back", delegate() { 
            this.menu.Back(); 
            this.inCAL = false; 
            doUpdates();            
        });
		
        Every(100 * EasyAPI.Milliseconds, delegate () {
          doUpdates();
        });
	}  
     
    public void doUpdates() 
    {        
        if(this.inCAL) 
        { 
            lcd.WritePublicTitle(calCommand); 
        } 
        else 
        {
            var lines = menu.Draw().Split("\n".ToCharArray());
            string cmd = "";
            foreach(var line in lines) 
            { 
                cmd += "Echo " + line + ";";
            } 
            cmd = cmd.Substring(0, cmd.Length - 1);
            lcd.WritePublicTitle(cmd);
        } 
         
        cal.RunPB(); 
    }     
} 
