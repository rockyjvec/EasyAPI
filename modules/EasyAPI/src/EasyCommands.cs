/*** Commands ***/

// Base EasyCommands class
public class EasyCommands
{
    /*** Private Properties ***/
    
    private EasyAPI api; // The selected menu item (child)
    private EasyBlocks blocks;
    public Dictionary<string,string> functions = new Dictionary<string,string>();
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
    
    private void skipWhitespace()
    {
        while(pos < text.Length && char.IsWhiteSpace(text, pos))
        {
            pos++;
        }
    }
    
    private void skipLineComment()
    {
        pos += 2;
        while(pos < text.Length)
        {
            if(text[pos] == "\n"[0])
            {
                pos++;
                break;
            }
            pos++;
        }        
    }
    
    private void skipBlockComment()
    {
        pos += 2;
        while(pos < text.Length && pos + 1 < text.Length)
        {
            if(text[pos] == '*' && text[pos+1] == '/')
            {
                pos += 2;
                break;
            }
            pos++;
        }
    }
    
    private void skipNonCode()
    {
        while(pos < text.Length)
        {
            if(char.IsWhiteSpace(text, pos)) skipWhitespace();
            else if(pos + 1 < text.Length && text[pos] == '/')
            {
                if(text[pos+1] == '/') skipLineComment();
                else if(text[pos+1] == '*') skipBlockComment();
                else break;
            }
            else break;
        }
    }
    
    private string getIdentifier()
    {
        string identifier = "";
        while(pos < text.Length && char.IsLetterOrDigit(text[pos]))
        {
            identifier += text[pos];
            pos++;
        }
        return identifier;        
    }
    
    private string getParm()
    {
        string param = "";
        skipNonCode();
        if(pos < text.Length && text[pos] == '(')
        {
            pos++;
            skipNonCode();
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
                    else if(text[pos] == '\\' && pos + 1 < text.Length)
                    {
                        if(text[pos + 1] == '"')
                        {
                            param += '"';                                 
                            pos++;
                        }
                        else if(text[pos + 1] == 'n')
                        {
                            param += "\n";
                            pos++;
                        }
                        else
                        {
                            param += text[pos];
                        }
                    }
                    else
                    {
                        param += text[pos];
                    }
                    pos++;
                }
            }
            skipNonCode();
            
            if(pos < text.Length && text[pos]== ')')
            {
                pos++;
            }
        }
        
        return param;
    }
    
    private string getFunction()
    {
        // Todo this is REALLY basic and it should do a lot more parsing instead of just looking for the end curly brace.  Functions will break if a curly brace appears anywhere inside them
        string code = "";
        while(pos < text.Length)
        {
            if(text[pos] == '}')
            {
              pos++;
              break;
            }
            code += text[pos];
            pos++;
        }
        return code;
    }
    
    private void require(char c)
    {
        if(pos < text.Length && text[pos] == c)
        {
            pos++;
            return;
        }
        failure("Required: " + c);
    }
    
    private void doCommand()
    {
        while(pos < text.Length && text[pos] != ';')
        {
            skipNonCode();
            string command = getIdentifier();
            
            if(command == "")
            {
                return;
            }
            
            string parm = "";
            
            switch(command)
            {
                case "function":
                    skipNonCode();
                    string identifier = getIdentifier();
                    skipNonCode();
                    require('(');
                    skipNonCode();
                    require(')');
                    skipNonCode();
                    require('{');
                    skipNonCode();
                    string function = getFunction();
                    if(functions.ContainsKey(identifier))
                    {
                        failure("Function " + identifier + " already defined!");
                    }
                    functions.Add(identifier, function);
                    return;
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
                case "WriteCustomData":
                case "WritePrivateText":
                    parm = getParm();
                    blocks.WriteCustomData(parm);
                    break;
                case "AppendPublicText":
                    parm = getParm();
                    blocks.AppendPublicText(parm);
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
                    blocks.DebugDumpProperties();
                    break;
                case "Run":
                    parm = getParm();
                    blocks.Run(api, parm);
                    break;
                case "RunPB":
                    parm = getParm();
                    blocks.RunPB(parm);
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
                default:
                    failure("Invalid command: '" + command + "'");
                    break;
            }

            skipNonCode();
            
            if(pos < text.Length && text[pos] == '.') pos++;
        }
    }
}
