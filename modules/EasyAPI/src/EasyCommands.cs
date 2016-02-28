/*** Commands ***/

// Base EasyCommands class
public class EasyCommands
{
    /*** Private Properties ***/
    
    private EasyAPI api; // The selected menu item (child)
    private EasyBlocks blocks;

    private int pos = 0;
    private string text = "";
    
    /*** Constructors ***/
    
    public EasyCommands(EasyAPI api)
    {
        this.api = api;
    }
    
    /*** Private Methods ***/
    
    public void handle(string text)
    {
        this.pos = 0;
        this.text = text;

        while(pos < text.Length)
        {
            doCommand();
            pos++;
        }
    }
    
    private void failure(string message)
    {
        throw new Exception("EasyCommand Error: " + message);
    }
    
    private bool isWhitespace(char c)
    {
        switch((int)c)
        {
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 32:
                return true;
        }
        return false;
    }
    
    private bool isAlphanum(char c)
    {
        if((48 <= c && c <= 57) || (97 <= c && c <= 122) || (65 <= c && c <= 90))
            return true;
        else
            return false;
    }
    
    private void skipWhitespace()
    {
        while(pos < text.Length && isWhitespace(text[pos]))
        {
            pos++;
        }
    }
    
    private string getIdentifier()
    {
        string identifier = "";
        while(pos < text.Length && isAlphanum(text[pos]))
        {
            identifier += text[pos];
            pos++;
        }
        return identifier;        
    }
    
    private string getParm()
    {
        string param = "";
        skipWhitespace();
        if(pos < text.Length && text[pos] == '(')
        {
            pos++;
            skipWhitespace();
            if(pos < text.Length && text[pos] == '"')
            {
                pos++;
                while(pos < text.Length)
                {
                    if(text[pos] == '"')
                    {
                        pos++;
                        break;
                    }
                    else if(text[pos] == '\\' && pos + 1 < text.Length && text[pos + 1] == '"') 
                    {
                        param += '"';     
                        pos++;
                    }
                    else
                    {
                        param += text[pos];
                    }
                    pos++;
                }
            }
            skipWhitespace();
            
            if(pos < text.Length && text[pos]== ')')
            {
                pos++;
            }
        }
        
        return param;
    }
    
    private void doCommand()
    {
        while(pos < text.Length && text[pos] != ';')
        {
            skipWhitespace();
            string command = getIdentifier();
            string parm = "";
            EasyBlocks blks;
            EasyCommands cmd;
            
            switch(command)
            {
                case "Echo":
                    parm = getParm();
                    api.Echo(parm);
                    break;
                case "Blocks":
                    api.Refresh();
                    this.blocks = api.Blocks;
                    break;
                /*** Actions ***/
                case "ApplyAction":
                    parm = getParm();
                    blocks.ApplyAction(parm);
                    break;
                case "WritePublicText":
                    parm = getParm();
                    blocks.WritePublicText(parm);
                    break;
                case "WritePrivateText":
                    parm = getParm();
                    blocks.WritePrivateText(parm);
                    break;
                case "AppendPublicText":
                    parm = getParm();
                    blocks.AppendPublicText(parm);
                    break;
                case "AppendPrivateText":
                    parm = getParm();
                    blocks.AppendPrivateText(parm);
                    break;
                case "On":
                    parm = getParm();
                    blocks.On();
                    break;
                case "Off":
                    parm = getParm();
                    blocks.Off();
                    break;
                case "Toggle":
                    parm = getParm();
                    blocks.Toggle();
                    break;
                case "DebugDump":
                    parm = getParm();
                    blocks.DebugDump();
                    break;
                case "DebugDumpActions":
                    parm = getParm();
                    blocks.DebugDumpActions();
                    break;
                case "DebugDumpProperties":
                    parm = getParm();
                    blocks.DebugDumpActions();
                    break;
                case "Run":
                    parm = getParm();
                    blocks.Run(api, parm);
                    break;

                /*** Filters ***/
                case "Named":
                    parm = getParm();
                    blocks = blocks.Named(parm);
                    break;
                case "NamedLike":
                    parm = getParm();
                    blocks = blocks.NamedLike(parm);
                    break;
                case "NotNamed":
                    parm = getParm();
                    blocks = blocks.NotNamed(parm);
                    break;
                case "NotNamedLike":
                    parm = getParm();
                    blocks = blocks.NotNamedLike(parm);
                    break;
                case "InGroupsNamed":
                    parm = getParm();
                    blocks = blocks.InGroupsNamed(parm);
                    break;
                case "InGroupsNamedLike":
                    parm = getParm();
                    blocks = blocks.InGroupsNamedLike(parm);
                    break;
                case "InGroupsNotNamed":
                    parm = getParm();
                    blocks = blocks.InGroupsNotNamed(parm);
                    break;
                case "InGroupsNotNamedLike":
                    parm = getParm();
                    blocks = blocks.InGroupsNotNamedLike(parm);
                    break;
                case "OfType":
                    parm = getParm();
                    blocks = blocks.OfType(parm);
                    break;
                case "OfTypeLike":
                    parm = getParm();
                    blocks = blocks.OfTypeLike(parm);
                    break;
                case "NotOfType":
                    parm = getParm();
                    blocks = blocks.NotOfType(parm);
                    break;
                case "NotOfTypeLike":
                    parm = getParm();
                    blocks = blocks.OfTypeLike(parm);
                    break;
                case "":
                    break;
                default:
                    failure("Invalid command: '" + command + "'");
                    break;
            }

            skipWhitespace();
            
            if(pos < text.Length && text[pos] == '.') pos++;
        }
    }
}
