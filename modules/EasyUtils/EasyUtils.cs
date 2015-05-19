public class EasyUtils {
    public const int LOG_MAX_ECHO_LENGTH_CHARS = 8000; // Mirrored value from MyProgrammableBlock.cs
    public const int LOG_MAX_LCD_LENGTH_CHARS = 4200; // Mirrored value from MyTextPanel.cs
    public static StringBuilder LogBuffer;
    public static void Log(string logMessage, Action<string> echo = null, IMyProgrammableBlock me = null, string label = null, IMyTextPanel mirrorLcd = null, bool truncateForLcd = true)
    {
        String output = "";
        if(echo == null) {
            output = "\n";
            output += logMessage;
            throw new Exception(output);
        }
        if(LogBuffer == null) {
            LogBuffer = new StringBuilder();
        }
        if(label != null) {
            logMessage = label+": "+logMessage;
        }
        if(mirrorLcd != null) {
            string currentlyMirrored = mirrorLcd.GetPublicText();
            if(truncateForLcd && LogBuffer.Length + logMessage.Length > LOG_MAX_LCD_LENGTH_CHARS) {
                StringBuilder lcdBuffer = new StringBuilder(LogBuffer.ToString());
                int charAmountToOffset = fullLineCharsExceeding(lcdBuffer, logMessage.Length, LogBuffer.Length - (LOG_MAX_LCD_LENGTH_CHARS - logMessage.Length));
                lcdBuffer.Remove(0, LogBuffer.Length - LOG_MAX_LCD_LENGTH_CHARS + charAmountToOffset - 2);
                lcdBuffer.AppendLine();
                lcdBuffer.Append(logMessage);
                mirrorLcd.WritePublicText(lcdBuffer.ToString(), false);
            } else {
                string potentialNewLine = (currentlyMirrored.Length > 0)? "\n" : "";
                mirrorLcd.WritePublicText(potentialNewLine+logMessage, true);
            }
        }
        if(LogBuffer.Length + logMessage.Length * 2 > LOG_MAX_ECHO_LENGTH_CHARS) {
            int charAmountToRemove = fullLineCharsExceeding(LogBuffer, logMessage.Length);
            LogBuffer.Remove(0, charAmountToRemove);
            LogBuffer.Append(output);
        }
        if(LogBuffer.Length > 0) {
            LogBuffer.AppendLine();
        }
        LogBuffer.Append(logMessage);
        echo(LogBuffer.ToString());
    }
    public static int fullLineCharsExceeding(StringBuilder sb, int maxLength, int offset = 0) {
        int runningCount = 0;
        for(int i=offset; i<sb.Length; i++) {
            runningCount++;
            if(sb[i] == '\n') {
                if(runningCount > maxLength) {
                    break;
                }
            }
        }
        return runningCount;
    }
    public static void ClearLogBuffer() {
        LogBuffer.Clear();
    }

    //because "System.array does not contain a definition for .Max()"
    public static double Max(double[] values) {
        double runningMax = values[0];
        for(int i=1; i<values.Length; i++) {
            runningMax = Math.Max(runningMax, values[i]);
        }
        return runningMax;
    }

    //because "System.array does not contain a definition for .Min()"
    public static double Min(double[] values) {
        double runningMin = values[0];
        for(int i=1; i<values.Length; i++) {
            runningMin = Math.Min(runningMin, values[i]);
        }
        return runningMin;
    }
}
