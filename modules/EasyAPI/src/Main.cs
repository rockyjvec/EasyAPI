Example state;

void Main(string argument)
{
    if(state == null)
    {
        state = new Example(GridTerminalSystem, Me, Echo, ElapsedTime);
    }

    // Set the minimum time between ticks here to prevent lag.
    // To utilise onSingleTap and onDoubleTap, set the minimum time to the same
    // time period of the timer running this script (e.g. 1 * EasyAPI.Seconds).
    state.Tick(100 * EasyAPI.Milliseconds, argument);
}
