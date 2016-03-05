EasyMenu menu;

void Main(string arg) 
{ 
    if(menu == null)
    {
        menu = new EasyMenu("Menu Title", new [] {  
            new EasyMenuItem("Run PB 1", delegate(EasyMenuItem item) {  
                IMyProgrammableBlock PB = GridTerminalSystem.GetBlockWithName("EAP") as IMyProgrammableBlock;   

                PB.TryRun("Argument 1");  
                return false;
            }),  
                new EasyMenuItem("Run PB 2", delegate(EasyMenuItem item) {  
                IMyProgrammableBlock PB = GridTerminalSystem.GetBlockWithName("EAP") as IMyProgrammableBlock;   

                PB.TryRun("Argument 2");  
                return false;
            })  
        });
    }
    
    if(arg == "Up") menu.Up(); 
    if(arg == "Down") menu.Down(); 
    if(arg == "Choose") menu.Choose(); 
    if(arg == "Back") menu.Back(); 

    var lcd = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel; 
  
    lcd.WritePublicText(menu.Draw(), false); 
}