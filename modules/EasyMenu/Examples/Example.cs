public class Example : EasyAPI 
{
    // Cache all the different blocks
    
    EasyLCD lcd;
    EasyMenu menu;    

    EasyBlocks timer;
    EasyBlocks screen;
    EasyBlocks speaker;

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
        Single slider = screen.GetProperty<Single>("ChangeIntervalSlider"); 

        if(slider < 5) 
        {
            menu.Up();
            screen.SetProperty<Single>("ChangeIntervalSlider", (Single)5);
        }

        if(slider > 5)
        {
            menu.Down();
            screen.SetProperty<Single>("ChangeIntervalSlider", (Single)5);
        }        

        Single delay = timer.GetProperty<Single>("TriggerDelay");

        if(delay > 5)  
        { 
            menu.Choose();
            timer.SetProperty<Single>("TriggerDelay", (Single)5);
        } 
        if(delay < 5) 
        { 
            menu.Back();
            timer.SetProperty<Single>("TriggerDelay", (Single)5);
        } 
        
        lcd.clear();
        lcd.update();
        lcd.SetText(menu.Draw());
    }
    
    public Example(IMyGridTerminalSystem grid) : base(grid) 
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
        this.timer = Blocks.Named("MenuTimer").FindOrFail("MenuTimer not found!");       
        this.screen = Blocks.Named("MenuLCD").FindOrFail("MenuLCD not found!");
        this.speaker = Blocks.Named("MenuSpeaker").FindOrFail("MenuSpeaker not found!");
        
        this.lcd = new EasyLCD(this.screen);
        
        Every(100 * Milliseconds, doUpdates);
    } 
}
