static public bool EasyCompare(String op, String a, String b)
{
    switch(op)
    {
        case "==":
            return (a == b);
        case "!=":
            return (a != b);
        case "~":
            return a.Contains(b);
        case "!~":
            return !a.Contains(b);
        case "R":
            System.Text.RegularExpressions.Match m = (new System.Text.RegularExpressions.Regex(b)).Match(a);
            while(m.Success)
            {
                return true;
            }
            return false;
        case "!R":
            return !EasyCompare("R", a, b);
    }
    return false;
}
