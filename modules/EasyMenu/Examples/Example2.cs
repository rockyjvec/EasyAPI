public class Example : EasyAPI 
{
    // Cache all the different blocks
    
    EasyLCD lcd;
    EasyMenu menu;    

    EasyBlocks speaker;

    public Example(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime) : base(grid, me, echo, elapsedTime) 
    {
        // Create menu
        this.menu = new EasyMenu("Test Menu", new [] {
            new EasyMenuItem("Play Sound", playSound),
            new EasyMenuItem("Door Status", new[] {
                new EasyMenuItem("Door 1", toggleDoor, doorStatus),
                new EasyMenuItem("Door 2", toggleDoor, doorStatus),
                new EasyMenuItem("Door 3", toggleDoor, doorStatus),
                new EasyMenuItem("Door 4", toggleDoor, doorStatus)
            }),
            new EasyMenuItem("Do Nothing")
        });
        
        // Get blocks
        this.speaker = Blocks.Named("MenuSpeaker").FindOrFail("MenuSpeaker not found!");
        
        this.lcd = new EasyLCD(Blocks.Named("MenuLCD").FindOrFail("MenuLCD not found!"));
        
        
        // Handle Commands
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
            doUpdates();           
        });
    } 
    
    public bool toggleDoor(EasyMenuItem item)
    {
        EasyBlock door = Blocks.Named(item.Text).FindOrFail(item.Text + " not found!").GetBlock(0);

        if(door.Open())
            door.ApplyAction("Open_Off");
        else
            door.ApplyAction("Open_On");

        return false; // don't go to a sub-menu if one is available
    }
    
    public string doorStatus(EasyMenuItem item)
    {
        EasyBlock door = Blocks.Named(item.Text).GetBlock(0);
        
        return item.Text + ": " + ((door.Open())?"Open":"Closed");
    }

    public bool playSound(EasyMenuItem item)
    {
        speaker.ApplyAction("PlaySound");
        
        return false;
    }
    
    public void doUpdates()
    {
        lcd.clear();
        lcd.update();
        lcd.SetText(menu.Draw());
    }    
}
