public class Example : EasyAPI 
{
    EasyLCD lcd;

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
        this.screen = Blocks.Named("MenuLCD").FindOrFail("MenuLCD not found!");
        
        this.lcd = new EasyLCD(this.screen);
        
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
            doUpdates();            
        }); 
         
        On("Update", delegate() { 
            doUpdates(); 
        }); 
    }

    public void doUpdates()
    {
        lcd.clear();
        lcd.update();
        lcd.SetText(menu.Draw());
    }    
} 
