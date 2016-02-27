/*** Commands ***/

// Base EasyCommands class
public class EasyCommands
{
    /*** Private Properties ***/
    
    private EasyAPI api; // The selected menu item (child)
    private EasyBlocks blocks;
    
    /*** Constructors ***/
    
    public EasyCommands(EasyAPI api, string command = "EasyCommand")
    {
        this.api = api;
        
        api.OnCommand(command, this.handle);
    }
    
    /*** Private Methods ***/
    
    private bool checkArg(int argument, string[] argv)
    {
        return (argument < argv.Length);
    }
    
    public void handle(int argc, string[] argv)
    {
        if(argc > 1)
        {
            try
            {
                this.blocks = null;
                handleCommand(1, argv);                
            }
            catch(IndexOutOfRangeException e)
            {
                return;
            }
        }
    }
    
    private void handleCommand(int argument, string[] argv)
    {
        while(checkArg(argument, argv))
        {
            switch(argv[argument])
            {
                case "Echo":
                    argument = handleEcho(argument + 1, argv);
                    break;
                case "Blocks":
                    argument = handleBlocks(argument + 1, argv);
                    break;
                default: // By default, use argument as TerminalBlock type command
                    this.blocks = api.Blocks.OfType(argv[argument]);
                    argument = handleBlocks(argument + 1, argv);
                    break;
            }
        }
    }
    
    private int handleEcho(int argument, string[] argv)
    {
        while(checkArg(argument, argv))
        {
            api.Echo(argv[argument]);
            argument++;
        }
        
        return argument;
    }
    
    private int handleBlocks(int argument, string[] argv)
    {
        if(this.blocks == null)
        {
            api.Refresh();
            this.blocks = api.Blocks;
        }
        
        while(checkArg(argument, argv))
        {
            string filter = argv[argument];
            switch(filter[0])
            {
                case '~':
                    blocks = blocks.NamedLike(filter.Substring(1));
                    break;
                case '!':
                    blocks = blocks.NotNamed(filter.Substring(1));
                    break;
                case '*':
                    break;
                default:
                    switch(filter)
                    {
                        /*** Actions ***/
                        case "ApplyAction":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks.ApplyAction(argv[argument]);
                            break;
                        case "WritePublicText":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks.WritePublicText(argv[argument]);
                            break;
                        case "WritePrivateText":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks.WritePrivateText(argv[argument]);
                            break;
                        case "AppendPublicText":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks.AppendPublicText(argv[argument]);
                            break;
                        case "AppendPrivateText":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks.AppendPrivateText(argv[argument]);
                            break;
                        case "On":
                            blocks.On();
                            break;
                        case "Off":
                            blocks.Off();
                            break;
                        case "Toggle":
                            blocks.Toggle();
                            break;
                        case "DebugDump":
                            blocks.DebugDump();
                            break;
                        case "DebugDumpActions":
                            blocks.DebugDumpActions();
                            break;
                        case "DebugDumpProperties":
                            blocks.DebugDumpActions();
                            break;

                        /*** Filters ***/
                        case "Named":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.Named(argv[argument]);
                            break;
                        case "NamedLike":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.NamedLike(argv[argument]);
                            break;
                        case "NotNamed":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.NotNamed(argv[argument]);
                            break;
                        case "NotNamedLike":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.NotNamedLike(argv[argument]);
                            break;
                        case "InGroupsNamed":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.InGroupsNamed(argv[argument]);
                            break;
                        case "InGroupsNamedLike":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.InGroupsNamedLike(argv[argument]);
                            break;
                        case "InGroupsNotNamed":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.InGroupsNotNamed(argv[argument]);
                            break;
                        case "InGroupsNotNamedLike":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.InGroupsNotNamedLike(argv[argument]);
                            break;
                        case "OfType":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.OfType(argv[argument]);
                            break;
                        case "OfTypeLike":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.OfTypeLike(argv[argument]);
                            break;
                        case "NotOfType":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.NotOfType(argv[argument]);
                            break;
                        case "NotOfTypeLike":
                            argument++;
                            if(!checkArg(argument, argv)) break;
                            blocks = blocks.OfTypeLike(argv[argument]);
                            break;
                        default:
                            blocks = blocks.Named(argv[argument]);
                            break;
                    }
                    break;
            }
            
            argument++;
        }
        
        return argument;
    }
}
