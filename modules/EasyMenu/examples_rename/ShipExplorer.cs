public class Example : EasyAPI 
{
    EasyLCD lcd;

    EasyBlocks timer;
    EasyBlocks screen;
    
    EasyMenu menu;

    public Example(IMyGridTerminalSystem grid, IMyProgrammableBlock me, Action<string> echo, TimeSpan elapsedTime) : base(grid, me, echo, elapsedTime) 
    { 
        // Create menu
        this.menu = new EasyMenu("Explore", new [] {
            new EasyMenuItem("Actions", delegate(EasyMenuItem actionsItem) {
                
                List<string> types = new List<string>();
                
                for(int n = 0; n < Blocks.Count(); n++)
                {
                    var block = Blocks.GetBlock(n);
                    
                    if(!types.Contains(block.Type()))
                    {
                        types.Add(block.Type());
                    }
                }
                
                types.Sort();
                
                actionsItem.children.Clear();
                
                for(int n = 0; n < types.Count; n++)
                {
                    actionsItem.children.Add(new EasyMenuItem(types[n], delegate(EasyMenuItem typeItem) {
                     
                        typeItem.children.Clear();

                        var blocks = Blocks.OfType(typeItem.Text);
                        for(int o = 0; o < blocks.Count(); o++)
                        {
                            var block = blocks.GetBlock(o);                            
                            typeItem.children.Add(new EasyMenuItem(block.Name(), delegate(EasyMenuItem blockItem) {
                                
                                blockItem.children.Clear();

                                var actions = block.GetActions();
                                for(int p = 0; p < actions.Count; p++)
                                {
                                    var action = actions[p];
                                    
                                    blockItem.children.Add(new EasyMenuItem(action.Name + "", delegate(EasyMenuItem actionItem) {
                                        block.ApplyAction(action.Id);
                                        
                                        return false;
                                    }));
                                }
                                
                                blockItem.children.Sort();
                                return true;
                            }));
                        }
                        
                        typeItem.children.Sort();
                        return true;
                    }));
                }
                
                actionsItem.children.Sort();                
                return true;
            })
        });
        
        // Get blocks
        this.timer = Blocks.Named("MenuTimer").FindOrFail("MenuTimer not found!");       
        this.screen = Blocks.Named("MenuLCD").FindOrFail("MenuLCD not found!");
        
        this.lcd = new EasyLCD(this.screen);
        
        Every(100 * Milliseconds, doUpdates); // Run doUpdates every 100 milliseconds  
    }

    // Runs every 100 milliseconds
    public void doUpdates()
    {
        // Get value of change interval slider from LCD screen
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

        // Get value of trigger delay from timer block
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

        // Clear LCD and display menu
        lcd.clear();
        lcd.update();
        lcd.SetText(menu.Draw(70, 7));
    }    
} 
